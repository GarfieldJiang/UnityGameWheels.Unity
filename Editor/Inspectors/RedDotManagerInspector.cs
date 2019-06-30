using COL.UnityGameWheels.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using COL.UnityGameWheels.Core.RedDot;
using COL.UnityGameWheels.Unity.RedDot;
using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    [CustomEditor(typeof(RedDotManager))]
    public class RedDotManagerInspector : BaseManagerInspector
    {
        public override bool AvailableWhenPlaying => true;

        public override bool AvailableWhenNotPlaying => false;

        protected override void DrawContent()
        {
            var t = (RedDotManager)target;
            EditorGUI.BeginDisabledGroup(!t.IsSetUp);
            {
                DrawGetSection(t);
                EditorGUILayout.Space();
                DrawSetSection(t);
            }
            EditorGUI.EndDisabledGroup();
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

        private void DrawGetSection(RedDotManager t)
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
                        m_GetKeyExists = !string.IsNullOrEmpty(m_GetKey) && t.HasNode(m_GetKey);
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
                    EditorGUILayout.LabelField("Value:", t.GetValue(m_GetKey).ToString());
                    m_DependencyFoldout = EditorGUILayout.Foldout(m_DependencyFoldout, $"Depends on ({t.GetDependencyCount(m_GetKey)})");
                    if (m_DependencyFoldout)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var dep in t.GetDependencies(m_GetKey))
                        {
                            EditorGUILayout.LabelField(dep);
                        }

                        EditorGUI.indentLevel--;
                    }

                    m_ReverseDependencyFoldout = EditorGUILayout.Foldout(m_ReverseDependencyFoldout,
                        $"Those depending on this ({t.GetReverseDependencyCount(m_GetKey)})");
                    if (m_ReverseDependencyFoldout)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var dep in t.GetReverseDependencies(m_GetKey))
                        {
                            EditorGUILayout.LabelField(dep);
                        }

                        EditorGUI.indentLevel--;
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSetSection(RedDotManager t)
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
                        var setKeyExists = !string.IsNullOrEmpty(m_SetKey) && t.HasNode(m_SetKey);
                        if (!setKeyExists)
                        {
                            m_SetWarningMessage = $"Node not found for key [{m_SetKey}].";
                        }
                        else if (t.GetNodeType(m_SetKey) != RedDotNodeType.Leaf)
                        {
                            m_SetWarningMessage = $"Key [{m_SetKey}] corresponds to a non-leaf node.";
                        }
                        else
                        {
                            m_SetWarningMessage = string.Empty;
                            t.SetLeafValue(m_SetKey, m_SetValue);
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