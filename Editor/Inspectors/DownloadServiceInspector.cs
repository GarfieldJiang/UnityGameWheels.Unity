using COL.UnityGameWheels.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using COL.UnityGameWheels.Core.Ioc;
using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    [UnityAppEditor(typeof(DownloadService))]
    public class DownloadServiceInspector : BaseServiceInspector
    {
        private bool m_WaitingTaskSectionFoldout = true;
        private bool m_OngoingTaskFoldout = true;
        private readonly Dictionary<int, bool> m_DownloadTaskFoldouts = new Dictionary<int, bool>();
        private int m_TaskIdToStop = -1;

        protected internal override bool DrawContent(ITickableContainer container, object serviceInstance)
        {
            var downloadService = (DownloadService)serviceInstance;
            DrawWaitingDownloadTasks(downloadService);
            DrawOngoingDownloadTasks(downloadService);
            StopATaskIfNeeded(downloadService);
            return true;
        }

        private void StopATaskIfNeeded(DownloadService downloadService)
        {
            if (m_TaskIdToStop > -1)
            {
                downloadService.StopDownloading(m_TaskIdToStop);
                m_DownloadTaskFoldouts.Remove(m_TaskIdToStop);
            }

            m_TaskIdToStop = -1;
        }

        private void DrawWaitingDownloadTasks(DownloadService downloadService)
        {
            m_WaitingTaskSectionFoldout = EditorGUILayout.Foldout(m_WaitingTaskSectionFoldout, "Waiting tasks");

            if (!m_WaitingTaskSectionFoldout)
            {
                return;
            }

            var waitingTasks = typeof(DownloadService)
                .GetField("m_WaitingDownloadTaskInfoSlots", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(downloadService);
            var getEnumeratorMethod = waitingTasks.GetType().GetMethod("GetEnumerator", BindingFlags.Instance | BindingFlags.Public);

            EditorGUILayout.BeginVertical();
            {
                bool any = false;
                for (var enumerator = getEnumeratorMethod.Invoke(waitingTasks, null) as IEnumerator; enumerator.MoveNext(); /* Empty */)
                {
                    any = true;
                    var current = enumerator.Current;
                    int taskId = (int)current.GetType()
                        .GetProperty("Key", BindingFlags.Instance | BindingFlags.Public)
                        .GetValue(current, null);
                    var value = current.GetType()
                        .GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)
                        .GetValue(current, null);
                    var downloadTaskInfo = ((DownloadTaskInfo?)value.GetType()
                        .GetField("DownloadTaskInfo", BindingFlags.Instance | BindingFlags.Public)
                        .GetValue(value)).Value;
                    DrawWaitingDownloadTask(taskId, downloadTaskInfo);
                }

                if (!any)
                {
                    EditorGUILayout.LabelField("No waiting tasks.");
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawOngoingDownloadTasks(DownloadService downloadService)
        {
            m_OngoingTaskFoldout = EditorGUILayout.Foldout(m_OngoingTaskFoldout, "Ongoing tasks");

            if (!m_OngoingTaskFoldout)
            {
                return;
            }

            var ongoingTasks = typeof(DownloadService)
                .GetField("m_OngoingDownloadTasks", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(downloadService) as Dictionary<int, IDownloadTask>;

            EditorGUILayout.BeginVertical();
            {
                bool any = false;
                for (var enumerator = ongoingTasks.GetEnumerator(); enumerator.MoveNext(); /* Empty */)
                {
                    any = true;
                    var current = enumerator.Current;
                    int taskId = current.Key;
                    var downloadTask = current.Value;
                    DrawOngoingDownloadTask(taskId, downloadTask);
                }

                if (!any)
                {
                    EditorGUILayout.LabelField("No ongoing tasks.");
                }
            }
            EditorGUILayout.EndVertical();
        }

        private bool DrawOneDownloadTaskHead(int taskId, ref DownloadTaskInfo downloadTaskInfo)
        {
            var foldOut = m_DownloadTaskFoldouts.ContainsKey(taskId) && m_DownloadTaskFoldouts[taskId];
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(10f);
                var newFoldOut = EditorGUILayout.Foldout(foldOut,
                    Core.Utility.Text.Format("Task ID: {0} ({1})", taskId, Path.GetFileName(downloadTaskInfo.SavePath)));
                if (newFoldOut != foldOut || !m_DownloadTaskFoldouts.ContainsKey(taskId))
                {
                    m_DownloadTaskFoldouts[taskId] = newFoldOut;
                }

                if (GUILayout.Button("Stop", GUILayout.Width(60f)))
                {
                    m_TaskIdToStop = taskId;
                }
            }
            EditorGUILayout.EndHorizontal();
            return foldOut;
        }

        private void DrawWaitingDownloadTask(int taskId, DownloadTaskInfo downloadTaskInfo)
        {
            if (!DrawOneDownloadTaskHead(taskId, ref downloadTaskInfo))
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(10f);
                DrawDownloadTaskInfo(downloadTaskInfo);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawOngoingDownloadTask(int taskId, IDownloadTask downloadTask)
        {
            if (downloadTask.Info == null)
            {
                return;
            }

            var downloadTaskInfo = downloadTask.Info.Value;
            if (!DrawOneDownloadTaskHead(taskId, ref downloadTaskInfo))
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(10f);
                DrawDownloadTask(downloadTask);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDownloadTask(IDownloadTask downloadTask)
        {
            if (!downloadTask.Info.HasValue)
            {
                return;
            }

            EditorGUI.indentLevel++;
            var downloadTaskInfo = downloadTask.Info.Value;
            EditorGUILayout.BeginVertical();
            {
                var sb = StringBuilderCache.Acquire();
                sb.AppendLine("URL: " + downloadTaskInfo.UrlStr);
                sb.AppendLine("Save Path: " + downloadTaskInfo.SavePath);
                if (downloadTaskInfo.Size > 0L)
                {
                    sb.AppendFormat("Size: {0:N0}\n", downloadTaskInfo.Size);
                }

                if (downloadTaskInfo.Crc32 != null)
                {
                    sb.AppendFormat("CRC 32: {0}\n", downloadTaskInfo.Crc32.Value);
                }

                if (downloadTaskInfo.Size <= 0L)
                {
                    sb.AppendFormat("Download size: {0:N0}\n", downloadTask.DownloadedSize);
                }
                else
                {
                    float progress = (float)downloadTask.DownloadedSize / downloadTaskInfo.Size;
                    sb.AppendFormat("Download size: {0:N0} ({1:P2})\n",
                        downloadTask.DownloadedSize, progress);
                }

                TimeSpan timeUsed = TimeSpan.FromSeconds(downloadTask.TimeUsed);
                sb.AppendFormat("Time used: {0}.{1:D2}:{2:D2}:{3:D2}\n", timeUsed.Days, timeUsed.Hours, timeUsed.Minutes, timeUsed.Seconds);
                EditorGUILayout.LabelField(StringBuilderCache.GetStringAndRelease(sb), EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        private static void DrawDownloadTaskInfo(DownloadTaskInfo downloadTaskInfo)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();
            {
                var sb = StringBuilderCache.Acquire();
                sb.AppendLine("URL: " + downloadTaskInfo.UrlStr);
                sb.AppendLine("Save Path: " + downloadTaskInfo.SavePath);
                if (downloadTaskInfo.Size > 0L)
                {
                    sb.AppendFormat("Size: {0:N0}\n", downloadTaskInfo.Size);
                }

                if (downloadTaskInfo.Crc32 != null)
                {
                    sb.AppendFormat("CRC 32: {0}\n", downloadTaskInfo.Crc32.Value);
                }

                EditorGUILayout.LabelField(StringBuilderCache.GetStringAndRelease(sb), EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
    }
}