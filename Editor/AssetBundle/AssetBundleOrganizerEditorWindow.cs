using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizerEditorWindow : EditorWindow
    {
        private const float CursorRectHalfWidth = 5f;
        private const float ColumnMinWidth = 160f; // Need consider minimum width of EditorGUILayout.HelpBox, which cannot be modified.
        private const int ColumnCount = 4;
        private const float DefaultIndentWidth = 16;
        private const float FoldoutWidth = 14;
        private const float ToggleWidth = 12;
        private const float DefaultLabelWidth = 80f;

        private static readonly GUILayoutOption MinWidthOne = GUILayout.MinWidth(1);
        private static readonly GUILayoutOption ExpandHeight = GUILayout.ExpandHeight(true);
        private static readonly GUILayoutOption ExpandWidth = GUILayout.ExpandWidth(true);

        private float[] m_ColumnRatios = null;
        private bool[] m_ResizingFlags = null;
        private float[] m_ColumnMargins = null;
        private float[] m_ColumnWidths = null;
        private Action[] m_DrawColumnDelegates = null;
        private Rect m_TopAreaRect;
        private Rect m_Position;
        private bool m_NeedRepaint;
        private Vector2 m_RootDirsSectionScrollPosition;
        private AssetBundleOrganizer m_AssetBundleOrganizer = null;
        private Dictionary<string, AssetInfoSatelliteData> m_AssetInfoSatelliteDatas = null;
        private HashSet<AssetBundleOrganizer.AssetInfo> m_SelectedAssetInfos = null;
        private RootDirsSection m_RootDirsSection = null;
        private AssetsSection m_AssetsSection = null;
        private AssetBundlesSection m_AssetBundlesSection = null;
        private AssetBundleContentsSection m_AssetBundleContentsSection = null;
        private BottomSection m_BottomSection = null;
        private Dictionary<string, AssetBundleInfoSatelliteData> m_AssetBundleInfoSatelliteDatas = null;
        private AssetBundleOrganizer.AssetBundleInfo m_SelectedAssetBundleInfo = null;
        private bool m_ShouldClose = false;

        private static GUIStyle LabelLikeButton
        {
            get
            {
                var style = new GUIStyle
                {
                    border = new RectOffset(0, 0, 0, 0),
                    imagePosition = ImagePosition.ImageLeft,
                };
                return style;
            }
        }

        private bool IsResizingInternals
        {
            get { return m_ResizingFlags.Any(f => f); }
        }

        public static void Open()
        {
            var window = GetWindow<AssetBundleOrganizerEditorWindow>(true, "Asset Bundle Organizer");
            window.minSize = new Vector2(720f, 480f);
        }

        #region EditorWindow

        private void OnEnable()
        {
            Init();
        }

        private void OnDisable()
        {
            Deinit();
        }

        private void OnGUI()
        {
            if (position != m_Position)
            {
                m_NeedRepaint = true;

                if (position.width < m_Position.width)
                {
                    for (int i = 0; i < m_ColumnRatios.Length; i++)
                    {
                        m_ColumnRatios[i] = 1f / ColumnCount;
                    }
                }

                m_Position = position;
            }

            EditorGUILayout.BeginVertical(ExpandWidth, ExpandHeight);
            {
                var topRect = EditorGUILayout.BeginHorizontal(ExpandWidth, ExpandHeight);
                {
                    if (topRect.width > 0)
                    {
                        m_TopAreaRect = topRect;
                    }

                    if (Event.current.rawType == EventType.MouseUp)
                    {
                        ResetResizingFlags();
                    }

                    var boxStyle = GUI.skin.box;
                    UpdateColumnMargins(boxStyle);
                    UpdateResizing(Event.current.mousePosition);
                    UpdateColumnWidths();

                    var cursorRectCenterX = m_ColumnMargins[0];
                    for (int i = 0; i < ColumnCount; i++)
                    {
                        float widthLoss = .5f * (i == 0 ? m_ColumnMargins[1] :
                                              i == ColumnCount - 1 ? m_ColumnMargins[ColumnCount - 1] :
                                              (m_ColumnMargins[i] + m_ColumnMargins[i + 1]));

                        EditorGUILayout.BeginVertical(boxStyle, ExpandHeight,
                            GUILayout.Width(m_ColumnWidths[i] - widthLoss));
                        {
                            m_DrawColumnDelegates[i]();
                        }
                        EditorGUILayout.EndVertical();

                        if (i < ColumnCount - 1)
                        {
                            cursorRectCenterX += m_ColumnWidths[i];
                            UpdateCursorRect(i,
                                new Rect(cursorRectCenterX - CursorRectHalfWidth, 0, CursorRectHalfWidth * 2, m_TopAreaRect.height));
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical("box", GUILayout.Height(50f), ExpandWidth);
                {
                    m_BottomSection.Draw();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            if (m_NeedRepaint)
            {
                m_NeedRepaint = false;
                Repaint();
            }

            if (m_ShouldClose)
            {
                Close();
            }
        }

        #endregion EditorWindow

        private void Init()
        {
            m_ColumnRatios = new float[ColumnCount];
            ResetColumnRatios();

            m_ResizingFlags = new bool[ColumnCount - 1];
            ResetResizingFlags();

            m_ColumnMargins = new float[ColumnCount + 1];
            m_ColumnWidths = new float[ColumnCount];

            m_RootDirsSection = new RootDirsSection(this);
            m_AssetsSection = new AssetsSection(this);
            m_AssetBundlesSection = new AssetBundlesSection(this);
            m_AssetBundleContentsSection = new AssetBundleContentsSection(this);

            m_BottomSection = new BottomSection(this);

            m_AssetInfoSatelliteDatas = new Dictionary<string, AssetInfoSatelliteData>();
            m_SelectedAssetInfos = new HashSet<AssetBundleOrganizer.AssetInfo>();

            m_AssetBundleInfoSatelliteDatas = new Dictionary<string, AssetBundleInfoSatelliteData>();

            m_DrawColumnDelegates = new Action[ColumnCount];
            m_DrawColumnDelegates[0] = m_RootDirsSection.Draw;
            m_DrawColumnDelegates[1] = m_AssetsSection.Draw;
            m_DrawColumnDelegates[2] = m_AssetBundlesSection.Draw;
            m_DrawColumnDelegates[3] = m_AssetBundleContentsSection.Draw;

            m_AssetBundleOrganizer = new AssetBundleOrganizer();
            RefreshAssetForest();
            RefreshAssetBundleTree();

            m_NeedRepaint = true;
        }

        private void Deinit()
        {
            m_ShouldClose = false;
        }

        private void UpdateColumnWidths()
        {
            float sum = 0;
            for (int i = 0; i < ColumnCount - 1; i++)
            {
                m_ColumnWidths[i] = Mathf.Round(m_TopAreaRect.width * m_ColumnRatios[i]);
                sum += m_ColumnWidths[i];
            }

            m_ColumnWidths[ColumnCount - 1] = m_TopAreaRect.width - sum;
        }

        private void UpdateColumnMargins(GUIStyle style)
        {
            m_ColumnMargins[0] = style.margin.left;
            for (int i = 1; i < m_ColumnMargins.Length - 1; i++)
            {
                m_ColumnMargins[i] = Mathf.Max(style.margin.left, style.margin.right);
            }

            m_ColumnMargins[m_ColumnMargins.Length - 1] = style.margin.right;
        }

        private void ResetColumnRatios()
        {
            var sum = 0f;
            for (int i = 0; i < m_ColumnRatios.Length - 1; i++)
            {
                m_ColumnRatios[i] = 1f / ColumnCount;
                sum += m_ColumnRatios[i];
            }

            m_ColumnRatios[m_ColumnRatios.Length - 1] = 1f - sum;
        }

        private void ResetResizingFlags()
        {
            for (int i = 0; i < m_ResizingFlags.Length; i++)
            {
                m_ResizingFlags[i] = false;
            }
        }

        private void UpdateResizing(Vector2 mousePosition)
        {
            float startX = 0f;
            for (int i = 0; i < ColumnCount - 1; i++)
            {
                if (i > 0)
                {
                    startX += m_TopAreaRect.width * m_ColumnRatios[i - 1];
                }

                if (!m_ResizingFlags[i])
                {
                    continue;
                }

                float ratioSum = m_ColumnRatios[i] + m_ColumnRatios[i + 1];
                float newLeftColumnWidth = Mathf.Clamp(mousePosition.x - startX, ColumnMinWidth,
                    m_TopAreaRect.width * ratioSum - ColumnMinWidth);
                if (m_TopAreaRect.width > 0)
                {
                    m_ColumnRatios[i] = newLeftColumnWidth / m_TopAreaRect.width;
                    m_ColumnRatios[i + 1] = ratioSum - m_ColumnRatios[i];
                    m_NeedRepaint = true;
                }

                break;
            }
        }

        private void UpdateCursorRect(int index, Rect rect)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            if (Event.current.rawType == EventType.MouseDown && rect.Contains(Event.current.mousePosition) && !IsResizingInternals)
            {
                m_ResizingFlags[index] = true;
            }
        }

        private void RefreshAssetBundleTree()
        {
            m_AssetBundleOrganizer.RefreshAssetBundleTree();
            m_AssetBundleInfoSatelliteDatas.Clear();
            m_SelectedAssetBundleInfo = null;
        }

        private void RefreshAssetForest()
        {
            m_AssetBundleOrganizer.RefreshAssetForest();
            m_AssetInfoSatelliteDatas.Clear();
            m_SelectedAssetInfos.Clear();
        }

        private void ClearSelectedAssetInfos()
        {
            foreach (var assetInfo in m_SelectedAssetInfos)
            {
                AssetInfoSatelliteData satelliteData;
                if (m_AssetInfoSatelliteDatas.TryGetValue(assetInfo.Guid, out satelliteData))
                {
                    satelliteData.Selected = false;
                }
            }

            m_SelectedAssetInfos.Clear();
        }
    }
}