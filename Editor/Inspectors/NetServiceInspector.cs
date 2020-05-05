using System.Collections.Generic;
using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Core.Ioc;
using COL.UnityGameWheels.Core.Net;
using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    [UnityAppEditor(typeof(NetService))]
    public class NetServiceInspector : BaseServiceInspector
    {
        private readonly List<INetChannel> m_NetChannels = new List<INetChannel>();

        private readonly HashSet<string> m_FoldoutNetChannelNames = new HashSet<string>();

        protected internal override bool DrawContent(ITickableContainer container, object serviceInstance)
        {
            var netService = (NetService)serviceInstance;
            m_NetChannels.Clear();
            netService.GetChannels(m_NetChannels);
            EditorGUILayout.LabelField($"Channels ({m_NetChannels.Count})", EditorStyles.boldLabel);
            foreach (var channel in m_NetChannels)
            {
                DrawOneChannel(channel);
            }

            return false;
        }

        private void DrawOneChannel(INetChannel channel)
        {
            //EditorGUILayout.BeginVertical();
            {
                var oldFoldout = m_FoldoutNetChannelNames.Contains(channel.Name);
                var newFoldout = EditorGUILayout.Foldout(oldFoldout, $"{channel.Name}");
                if (newFoldout != oldFoldout)
                {
                    if (newFoldout)
                    {
                        m_FoldoutNetChannelNames.Add(channel.Name);
                    }
                    else
                    {
                        m_FoldoutNetChannelNames.Remove(channel.Name);
                    }
                }

                if (newFoldout)
                {
                    EditorGUI.indentLevel += 1;
                    var sb = StringBuilderCache.Acquire();
                    sb.AppendLine($"Type: {channel.GetType().FullName}");
                    sb.AppendLine($"Handler type: {channel.Handler?.GetType().FullName ?? "<null>"}");
                    sb.AppendLine($"State: {channel.State}");
                    sb.AppendLine(
                        $"Local End Point: {channel.LocalEndPoint?.Address.ToString() ?? "<null>"}:{channel.LocalEndPoint?.Port.ToString() ?? "<null>"}");
                    sb.AppendLine(
                        $"Remote End Point: {channel.RemoteEndPoint?.Address.ToString() ?? "<null>"}:{channel.RemoteEndPoint?.Port.ToString() ?? "<null>"}");
                    EditorGUILayout.LabelField(StringBuilderCache.GetStringAndRelease(sb), EditorStyles.wordWrappedLabel);
                    EditorGUI.indentLevel -= 1;
                }
            }
            //EditorGUILayout.EndHorizontal();
        }
    }
}