﻿using COL.UnityGameWheels.Core.Ioc;

namespace COL.UnityGameWheels.Unity.Editor
{
    using Core.Asset;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    [UnityAppEditor(typeof(AssetService))]
    public class AssetServiceInspector : BaseServiceInspector
    {
        private float m_LastRefreshTime = float.NegativeInfinity;
        private List<AssetCacheQuery> m_AssetCacheQueries = new List<AssetCacheQuery>();
        private List<ResourceCacheQuery> m_ResourceCacheQueries = new List<ResourceCacheQuery>();
        private readonly HashSet<string> m_UnfoldedAssets = new HashSet<string>();
        private readonly HashSet<string> m_DepUnfoldedAssets = new HashSet<string>();
        private readonly HashSet<string> m_UnfoldedResources = new HashSet<string>();
        private string m_SearchResourceText = string.Empty;
        private string m_SearchAssetText = string.Empty;

        private string AssetCacheQueriesFoldoutKey => Core.Utility.Text.Format("{0}.AssetCacheQueriesFoldout", GetType().FullName);

        private string ResourceCacheQueriesFoldoutKey => Core.Utility.Text.Format("{0}.ResourceCacheQueriesFoldout", GetType().FullName);

        protected internal override bool DrawContent(object serviceInstance)
        {
            var assetService = (AssetService)serviceInstance;
            DrawQueries(assetService);
            return true;
        }

        private bool DrawQueries(AssetService assetService)
        {
            bool needRefreshView = false;
            if (Time.unscaledTime - m_LastRefreshTime > 1f)
            {
                RefreshQueries(assetService);
                needRefreshView = true;
            }

            EditorGUILayout.LabelField("Any asset/bundle is being loaded: " + assetService.IsLoadingAnyAsset);
            DrawResources();
            DrawAssets();
            return needRefreshView;
        }

        private void RefreshQueries(AssetService assetService)
        {
            m_LastRefreshTime = Time.unscaledTime;
            m_AssetCacheQueries = assetService.GetAssetCacheQueries().Values.ToList();
            m_AssetCacheQueries.Sort((a, b) => a.Path?.CompareTo(b.Path) ?? 0);
            m_ResourceCacheQueries = assetService.GetResourceCacheQueries().Values.ToList();
            m_ResourceCacheQueries.Sort((a, b) => a.Path?.CompareTo(b.Path) ?? 0);
        }

        private void DrawResources()
        {
            if (!DrawSectionHeader(ResourceCacheQueriesFoldoutKey, "Asset Bundles: ({0})", m_ResourceCacheQueries.Count))
            {
                return;
            }

            EditorGUI.indentLevel += 1;

            EditorGUILayout.BeginVertical("box");
            m_SearchResourceText = EditorGUILayout.DelayedTextField("Search:", m_SearchResourceText);
            EditorGUILayout.EndVertical();

            foreach (var res in string.IsNullOrEmpty(m_SearchResourceText)
                ? m_ResourceCacheQueries
                : m_ResourceCacheQueries.Where(r => r.Path.Contains(m_SearchResourceText)))
            {
                DrawOneResource(res);
            }

            EditorGUI.indentLevel -= 1;
        }

        private void DrawAssets()
        {
            if (!DrawSectionHeader(AssetCacheQueriesFoldoutKey, "Assets: ({0})", m_AssetCacheQueries.Count))
            {
                return;
            }

            EditorGUI.indentLevel += 1;

            EditorGUILayout.BeginVertical("box");
            m_SearchAssetText = EditorGUILayout.DelayedTextField("Search:", m_SearchAssetText);
            EditorGUILayout.EndVertical();

            foreach (var asset in string.IsNullOrEmpty(m_SearchAssetText)
                ? m_AssetCacheQueries
                : m_AssetCacheQueries.Where(a => a.Path.Contains(m_SearchAssetText)))
            {
                DrawOneAsset(asset);
            }

            EditorGUI.indentLevel -= 1;
        }

        private bool DrawSectionHeader(string foldoutKey, string titleFormat, int count)
        {
            var foldout = EditorPrefs.GetBool(foldoutKey, false);
            var newFoldout = EditorGUILayout.Foldout(foldout, Core.Utility.Text.Format(titleFormat, count));
            if (newFoldout != foldout)
            {
                EditorPrefs.SetBool(foldoutKey, newFoldout);
            }

            return newFoldout;
        }

        private void DrawOneResource(ResourceCacheQuery res)
        {
            if (!DrawFoldout(res.Path, res.Path, m_UnfoldedResources))
            {
                return;
            }

            EditorGUI.indentLevel += 1;

            EditorGUILayout.LabelField("Status:", res.Status.ToString());
            if (!string.IsNullOrEmpty(res.ErrorMessage))
            {
                EditorGUILayout.LabelField("Error Message:", res.ErrorMessage);
            }

            if (res.Status == ResourceCacheStatus.Loading)
            {
                EditorGUILayout.LabelField("Loading Progress:", res.LoadingProgress.ToString());
            }

            EditorGUILayout.LabelField("Retain Count:", res.RetainCount.ToString());

            EditorGUI.indentLevel -= 1;
        }

        private void DrawOneAsset(AssetCacheQuery asset)
        {
            if (!DrawFoldout(asset.Path, asset.Path, m_UnfoldedAssets))
            {
                return;
            }

            EditorGUI.indentLevel += 1;
            {
                EditorGUILayout.LabelField("Status:", asset.Status.ToString());
                if (!string.IsNullOrEmpty(asset.ErrorMessage))
                {
                    EditorGUILayout.LabelField("Error Message:", asset.ErrorMessage);
                }

                if (asset.Status == AssetCacheStatus.Loading)
                {
                    EditorGUILayout.LabelField("Loading Progress:", asset.LoadingProgress.ToString());
                }

                EditorGUILayout.LabelField("Retain Count:", asset.RetainCount.ToString());
                EditorGUILayout.LabelField("Asset Bundle:", asset.ResourcePath);

                var deps = asset.GetDependencyAssetPaths();
                if (deps.Count > 0)
                {
                    var depFoldout = DrawFoldout("Dependency Assets", asset.Path, m_DepUnfoldedAssets);
                    if (depFoldout)
                    {
                        EditorGUI.indentLevel += 1;

                        foreach (var dep in deps)
                        {
                            EditorGUILayout.LabelField(dep);
                        }

                        EditorGUI.indentLevel -= 1;
                    }
                }
            }
            EditorGUI.indentLevel -= 1;
        }

        private static bool DrawFoldout(string title, string key, HashSet<string> unfoldedItems)
        {
            var foldout = unfoldedItems.Contains(key);
            var newFoldout = EditorGUILayout.Foldout(foldout, title);

            if (newFoldout != foldout)
            {
                if (newFoldout)
                {
                    unfoldedItems.Add(key);
                }
                else
                {
                    unfoldedItems.Remove(key);
                }
            }

            return newFoldout;
        }
    }
}