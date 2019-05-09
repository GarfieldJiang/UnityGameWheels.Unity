using COL.UnityGameWheels.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    [CustomEditor(typeof(EventManager))]
    public class EventManagerInspector : BaseManagerInspector
    {
        private enum SortingOrder
        {
            EventId,
            EventTypeFullName,
            EventTypeName,
        }

        private const float FoldoutIndent = 10f;

        private static readonly string SortingOrderKey = typeof(EventManagerInspector).FullName + ".SortingOrder";
        private static readonly string ShowNoListenerEventsKey = typeof(EventManagerInspector).FullName + ".ShowNoListenerEventsKey";

        private bool m_Inited = false;
        private Dictionary<int, Type> m_EventIdToType = null;
        private List<int> m_EventIds = null;
        private int m_FoldoutEventId = 0;
        private SortingOrder m_SortingOrder;
        private bool m_ShowNoListenerEvents;
        private bool m_ShowListeners = false;
        private Dictionary<int, LinkedList<OnHearEvent>> m_Listeners = null;

        public override bool AvailableWhenPlaying
        {
            get
            {
                return true;
            }
        }

        public override bool AvailableWhenNotPlaying
        {
            get
            {
                return false;
            }
        }

        protected override void DrawContent()
        {
            EnsureInit();
            DrawSortingOrderSection();
            DrawShowNoListenerEventsSection();
            DrawListeners();
            Repaint();
        }

        private void DrawListeners()
        {
            m_ShowListeners = EditorGUILayout.Foldout(m_ShowListeners, "Listeners");

            if (!m_ShowListeners)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(FoldoutIndent);
                EditorGUILayout.BeginVertical();
                {
                    int counter = 0;
                    foreach (var eventId in m_EventIds)
                    {
                        if (DrawListenersOfOneEventId(eventId))
                        {
                            counter++;
                        }
                    }

                    if (counter <= 0)
                    {
                        EditorGUILayout.HelpBox("Nothing to show.", MessageType.Info);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawShowNoListenerEventsSection()
        {
            var newShowNoListenerEvents = EditorGUILayout.Toggle("Show No Listener Events", m_ShowNoListenerEvents);
            if (newShowNoListenerEvents != m_ShowNoListenerEvents)
            {
                m_ShowNoListenerEvents = newShowNoListenerEvents;
                EditorPrefs.SetBool(ShowNoListenerEventsKey, newShowNoListenerEvents);
            }
        }

        private void DrawSortingOrderSection()
        {
            var newSortingOrder = (SortingOrder)EditorGUILayout.EnumPopup("Sorting Order", m_SortingOrder);
            if (newSortingOrder != m_SortingOrder)
            {
                m_SortingOrder = newSortingOrder;
                EditorPrefs.SetInt(SortingOrderKey, (int)m_SortingOrder);
                RefreshEventIdsOrder();
            }
        }

        private bool DrawListenersOfOneEventId(int eventId)
        {
            var listenerCount = m_Listeners.ContainsKey(eventId) ? m_Listeners[eventId].Count : 0;

            if (!m_ShowNoListenerEvents && listenerCount <= 0)
            {
                return false;
            }

            var eventType = EventIdToTypeMap.GetEventType(eventId);
            var displayName = m_SortingOrder == SortingOrder.EventTypeFullName ? eventType.FullName : eventType.Name;
            var foldout = EditorGUILayout.Foldout(eventId == m_FoldoutEventId,
                Core.Utility.Text.Format("[{0}] {1} ({2} listener(s))", eventId, displayName, listenerCount));

            if (eventId == m_FoldoutEventId && !foldout)
            {
                m_FoldoutEventId = 0;
            }

            if (foldout)
            {
                m_FoldoutEventId = eventId;
            }

            if (!foldout || listenerCount <= 0)
            {
                return true;
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(FoldoutIndent);
                EditorGUILayout.BeginVertical();
                {
                    foreach (var listener in m_Listeners[eventId])
                    {
                        var targetName = listener.Target == null ? "<null>" : listener.Target.ToString();
                        var methodName = listener.Method.Name;
                        EditorGUILayout.LabelField(Core.Utility.Text.Format("Target: '{0}', Method: '{1}'",
                            targetName, methodName));
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            return true;
        }

        private void EnsureInit()
        {
            if (m_Inited)
            {
                return;
            }

            m_Inited = true;
            m_EventIdToType = typeof(EventIdToTypeMap)
                .GetField("s_EventIdToType", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null) as Dictionary<int, Type>;
            m_EventIds = m_EventIdToType.Keys.ToList();
            m_SortingOrder = (SortingOrder)EditorPrefs.GetInt(SortingOrderKey, (int)SortingOrder.EventId);
            RefreshEventIdsOrder();

            m_Listeners = typeof(EventModule).
                GetField("m_Listeners", BindingFlags.Instance | BindingFlags.NonPublic).
                GetValue(((target as EventManager).Module) as EventModule) as Dictionary<int, LinkedList<OnHearEvent>>;

            m_ShowNoListenerEvents = EditorPrefs.GetBool(ShowNoListenerEventsKey, false);
        }

        private void RefreshEventIdsOrder()
        {
            switch (m_SortingOrder)
            {
                case SortingOrder.EventTypeFullName:
                    m_EventIds.Sort((a, b) => m_EventIdToType[a].FullName.CompareTo(m_EventIdToType[b].FullName));
                    break;
                case SortingOrder.EventTypeName:
                    m_EventIds.Sort((a, b) => m_EventIdToType[a].Name.CompareTo(m_EventIdToType[b].Name));
                    break;
                default:
                    m_EventIds.Sort();
                    break;
            }
        }
    }
}
