using COL.UnityGameWheels.Core.Ioc;
using COL.UnityGameWheels.Core.RedDot;
using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    [UnityAppEditor(typeof(RedDotService))]
    public class RedDotServiceInspector : BaseServiceInspector
    {
        protected internal override bool DrawContent(object serviceInstance)
        {
            var t = (RedDotService)serviceInstance;
            EditorGUI.BeginDisabledGroup(!t.IsSetUp);
            {
                DrawGetSection(t);
                EditorGUILayout.Space();
                DrawSetSection(t);
            }
            EditorGUI.EndDisabledGroup();
            return false;
        }

        private string m_GetKeyInput = string.Empty;
        private string m_GetKey = string.Empty;
        private string m_SetKeyInput = string.Empty;
        private int m_SetValue = 0;
        private string m_SetKey = string.Empty;
        private string m_SetWarningMessage = string.Empty;
        private bool m_DependencyFoldout = false;
        private bool m_ReverseDependencyFoldout = false;
        private bool m_GetKeyExists;

        private void DrawGetSection(RedDotService redDotService)
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField("Query a key", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                {
                    m_GetKeyInput = EditorGUILayout.TextField(m_GetKeyInput);
                    if (GUILayout.Button("Query", GUILayout.MaxWidth(80)))
                    {
                        m_GetKey = m_GetKeyInput;
                        m_GetKeyExists = !string.IsNullOrEmpty(m_GetKey) && redDotService.HasNode(m_GetKey);
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (!m_GetKeyExists)
                {
                    EditorGUILayout.HelpBox($"Node not found for key [{m_GetKey}].", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField("Key:", m_GetKey);
                    EditorGUILayout.LabelField("Value:", redDotService.GetValue(m_GetKey).ToString());
                    m_DependencyFoldout = EditorGUILayout.Foldout(m_DependencyFoldout, $"Depends on ({redDotService.GetDependencyCount(m_GetKey)})");
                    if (m_DependencyFoldout)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var dep in redDotService.GetDependencies(m_GetKey))
                        {
                            EditorGUILayout.LabelField(dep);
                        }

                        EditorGUI.indentLevel--;
                    }

                    m_ReverseDependencyFoldout = EditorGUILayout.Foldout(m_ReverseDependencyFoldout,
                        $"Those depending on this ({redDotService.GetReverseDependencyCount(m_GetKey)})");
                    if (m_ReverseDependencyFoldout)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var dep in redDotService.GetReverseDependencies(m_GetKey))
                        {
                            EditorGUILayout.LabelField(dep);
                        }

                        EditorGUI.indentLevel--;
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSetSection(RedDotService redDotService)
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField("Set a leaf's value", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical();
                    {
                        m_SetKeyInput = EditorGUILayout.TextField("Key", m_SetKeyInput);
                        m_SetValue = EditorGUILayout.IntField("Value", m_SetValue);
                    }
                    EditorGUILayout.EndVertical();

                    if (GUILayout.Button("Set", GUILayout.MaxWidth(80)))
                    {
                        m_SetKey = m_SetKeyInput;
                        var setKeyExists = !string.IsNullOrEmpty(m_SetKey) && redDotService.HasNode(m_SetKey);
                        if (!setKeyExists)
                        {
                            m_SetWarningMessage = $"Node not found for key [{m_SetKey}].";
                        }
                        else if (redDotService.GetNodeType(m_SetKey) != RedDotNodeType.Leaf)
                        {
                            m_SetWarningMessage = $"Key [{m_SetKey}] corresponds to a non-leaf node.";
                        }
                        else
                        {
                            m_SetWarningMessage = string.Empty;
                            redDotService.SetLeafValue(m_SetKey, m_SetValue);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(m_SetWarningMessage))
                {
                    EditorGUILayout.HelpBox(m_SetWarningMessage, MessageType.Warning);
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}