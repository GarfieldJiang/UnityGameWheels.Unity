using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;

#endif

namespace COL.UnityGameWheels.Unity.Editor
{
    using Core.Ioc;
    using Ioc;

    public class UnityAppEditorWindow : EditorWindow
    {
        private bool m_ShowContainerStatusSection = false;
        private bool m_ShowBindingDataSection = false;
        private bool m_ShowSingletonSection = false;
        private Type m_LastInspectedServiceType = null;
        private Type m_InspectedServiceType = null;
        private object m_InspectedServiceInstance = null;
        private bool m_LastDrawInspectorSection = false;
        private readonly Dictionary<Type, BaseServiceInspector> m_Inspectors = new Dictionary<Type, BaseServiceInspector>();
        private readonly HashSet<Type> m_TriedToCreateInspectorTypes = new HashSet<Type>();
        private readonly Dictionary<Type, Type> m_TypeToInspectorTypeMap = new Dictionary<Type, Type>();
        private readonly HashSet<Type> m_BindingDataFoldoutFlags = new HashSet<Type>();
        private Vector2 m_ScrollPosition = Vector2.zero;

        public static void Open()
        {
            var window = GetWindow<UnityAppEditorWindow>();
            window.titleContent = new GUIContent("Unity App");
        }

        void OnEnable()
        {
#if UNITY_2019_1_OR_NEWER
            rootVisualElement.Add(new IMGUIContainer(ImGuiDraw));
#endif
            m_TypeToInspectorTypeMap.Clear();
            m_TriedToCreateInspectorTypes.Clear();
            m_Inspectors.Clear();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
                .Where(a =>
                {
                    var assemblyName = a.GetName().Name;
                    return !assemblyName.Contains("UnityEngine") && !assemblyName.Contains("mscorlib") && !assemblyName.Contains("UnityEditor");
                }))
            {
                foreach (var type in assembly.GetTypes().Where(t =>
                    t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseServiceInspector)) && t.GetCustomAttribute<UnityAppEditorAttribute>() != null))
                {
                    m_TypeToInspectorTypeMap[type.GetCustomAttribute<UnityAppEditorAttribute>().TargetType] = type;
                }
            }
        }

        void OnDisable()
        {
            CleanUpInspectorSection();
            m_BindingDataFoldoutFlags.Clear();
        }

        private void CleanUpInspectorSection()
        {
            if (m_InspectedServiceInstance != null)
            {
                if (m_Inspectors.TryGetValue(m_InspectedServiceType, out var serviceInspector))
                {
                    serviceInspector.OnHide();
                }
            }

            m_LastInspectedServiceType = null;
            m_InspectedServiceType = null;
            m_InspectedServiceInstance = null;
        }


#if UNITY_2019_1_OR_NEWER
        private void ImGuiDraw()
#else
        private void OnGUI()
#endif
        {
            var repaint = false;
            var drawInspectorSection = false;
            if (!Application.isPlaying)
            {
                DrawNotPlayingContent();
            }
            else
            {
                repaint = DrawPlayingContent(ref drawInspectorSection);
            }

            if (!drawInspectorSection && m_LastDrawInspectorSection)
            {
                CleanUpInspectorSection();
            }

            if (repaint)
            {
                Repaint();
            }

            m_LastDrawInspectorSection = drawInspectorSection;
        }

        private bool DrawPlayingContent(ref bool drawInspectorSection)
        {
            var ret = false;
            if (UnityApp.Instance == null)
            {
                EditorGUILayout.HelpBox($"There is no running instance of {nameof(UnityApp)}.",
                    MessageType.Warning);
            }
            else
            {
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
                {
                    EditorGUILayout.BeginVertical();
                    {
                        DrawContainerStatusSection(UnityApp.Instance);
                        if (UnityApp.Instance.Container != null)
                        {
                            DrawBindingDataSection(UnityApp.Instance);
                            DrawSingletonSection(UnityApp.Instance);
                            EditorGUILayout.Space();
                            ret = DrawInspectorSection(UnityApp.Instance);
                            drawInspectorSection = true;
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();
            }

            return ret;
        }

        private void DrawBindingDataSection(UnityApp unityApp)
        {
            m_ShowBindingDataSection = EditorGUILayout.Foldout(m_ShowBindingDataSection, "Bindings", EditorStyles.foldoutHeader);
            if (!m_ShowBindingDataSection)
            {
                return;
            }

            EditorGUI.indentLevel++;
            foreach (var kv in unityApp.Container.GetBindingDatas())
            {
                var serviceType = kv.Key;
                var bindingData = kv.Value;
                var oldFoldout = m_BindingDataFoldoutFlags.Contains(serviceType);
                var newFoldout = EditorGUILayout.Foldout(oldFoldout, serviceType.FullName);
                if (newFoldout != oldFoldout)
                {
                    if (newFoldout)
                    {
                        m_BindingDataFoldoutFlags.Add(serviceType);
                    }
                    else
                    {
                        m_BindingDataFoldoutFlags.Remove(serviceType);
                    }
                }

                if (newFoldout)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Interface:", bindingData.InterfaceType?.ToString() ?? "<null>");
                    EditorGUILayout.LabelField("Impl:", bindingData.ImplType?.ToString() ?? "<null>");
                    EditorGUILayout.LabelField("Life Style:", bindingData.LifeStyle.ToString());
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawSingletonSection(UnityApp unityApp)
        {
            m_ShowSingletonSection = EditorGUILayout.Foldout(m_ShowSingletonSection, "Instances", EditorStyles.foldoutHeader);
            if (!m_ShowSingletonSection)
            {
                return;
            }

            EditorGUI.indentLevel++;
            bool clickedAnyInspect = false;
            bool lastInstanceExists = false;
            foreach (var kv in unityApp.Container.GetSingletons())
            {
                var serviceType = kv.Key;
                var serviceInstance = kv.Value;
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(serviceType.FullName);
                    if (GUILayout.Button("Inspect", GUILayout.MaxWidth(80f)))
                    {
                        m_InspectedServiceType = serviceType;
                        m_InspectedServiceInstance = serviceInstance;
                        clickedAnyInspect = true;
                    }
                }

                if (!clickedAnyInspect && serviceInstance == m_InspectedServiceInstance)
                {
                    lastInstanceExists = true;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (!clickedAnyInspect && !lastInstanceExists)
            {
                m_InspectedServiceType = null;
                m_InspectedServiceInstance = null;
            }

            EditorGUI.indentLevel--;
        }

        private bool DrawInspectorSection(UnityApp unityApp)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            var inspectedServiceDisplayName = m_InspectedServiceType == null ? "<nothing>" : m_InspectedServiceType.FullName;
            EditorGUILayout.LabelField($"Inspecting {inspectedServiceDisplayName}", EditorStyles.boldLabel);
            var ret = false;

            if (m_InspectedServiceType != m_LastInspectedServiceType)
            {
                if (m_LastInspectedServiceType != null &&
                    m_Inspectors.TryGetValue(m_LastInspectedServiceType, out var serviceInspector))
                {
                    serviceInspector.OnHide();
                }
            }

            if (m_InspectedServiceType != m_LastInspectedServiceType && m_InspectedServiceType != null)
            {
                var bindingData = unityApp.Container.GetBindingData(m_InspectedServiceType);
                if (!m_Inspectors.TryGetValue(m_InspectedServiceType, out var serviceInspector) &&
                    !m_TriedToCreateInspectorTypes.Contains(bindingData.ImplType))
                {
                    serviceInspector = CreateInspectorOrNull(bindingData);
                    m_TriedToCreateInspectorTypes.Add(bindingData.ImplType);
                    if (serviceInspector != null)
                    {
                        m_Inspectors[m_InspectedServiceType] = serviceInspector;
                    }
                }

                serviceInspector?.OnShow(m_InspectedServiceInstance);
            }

            if (m_InspectedServiceType != null)
            {
                m_Inspectors.TryGetValue(m_InspectedServiceType, out var serviceInspector);
                if (serviceInspector != null)
                {
                    EditorGUILayout.BeginVertical();
                    ret = serviceInspector.DrawContent(m_InspectedServiceInstance);
                    EditorGUILayout.EndVertical();
                }
            }

            m_LastInspectedServiceType = m_InspectedServiceType;
            return ret;
        }

        private BaseServiceInspector CreateInspectorOrNull(IBindingData bindingData)
        {
            var implType = bindingData.ImplType;
            if ((bindingData.LifeStyle != LifeStyles.Singleton && bindingData.LifeStyle != LifeStyles.Null) ||
                !m_TypeToInspectorTypeMap.TryGetValue(implType, out var inspectorType))
            {
                return null;
            }

            return (BaseServiceInspector)Activator.CreateInstance(inspectorType);
        }

        private void DrawContainerStatusSection(UnityApp unityApp)
        {
            m_ShowContainerStatusSection = EditorGUILayout.Foldout(m_ShowContainerStatusSection, "Container Status", EditorStyles.foldoutHeader);
            if (!m_ShowContainerStatusSection)
            {
                return;
            }

            EditorGUI.indentLevel++;

            var container = unityApp.Container;
            if (container == null)
            {
                EditorGUILayout.LabelField("Container doesn't exist.");
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField($"Type: {container.GetType()}");
                EditorGUILayout.LabelField($"Is Disposing: {container.IsDisposing}");
                EditorGUILayout.LabelField($"Is Disposed: {container.IsDisposed}");
                if (!container.IsDisposing && !container.IsDisposed)
                {
                    var rect = EditorGUILayout.BeginHorizontal();
                    // TODO: Unity bug or I use it wrongly? Why do I have to do the indentation by hand?
                    var indentedRect = EditorGUI.IndentedRect(rect);
                    EditorGUILayout.Space(indentedRect.xMin - rect.xMin, false);
                    if (GUILayout.Button("Dispose"))
                    {
                        container.Dispose();
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        private void DrawNotPlayingContent()
        {
            EditorGUILayout.HelpBox("This window is available when Application.isPlaying returns true.",
                MessageType.Warning);
        }
    }
}