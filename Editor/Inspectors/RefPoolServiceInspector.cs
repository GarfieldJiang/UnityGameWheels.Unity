using System;
using System.Collections.Generic;
using System.Linq;
using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Core.Ioc;
using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    [UnityAppEditor(typeof(RefPoolService))]
    public class RefPoolServiceInspector : BaseServiceInspector
    {
        private enum SortingOrder
        {
            ObjectType,
            CreateCount,
            DropCount,
            AcquireCount,
            ReleaseCount,
        }

        private int m_PoolCount;
        private string m_SearchText = string.Empty;
        private readonly List<WeakReference<IBaseRefPool>> m_PoolWeakRefs = new List<WeakReference<IBaseRefPool>>();
        private readonly List<WeakReference<IBaseRefPool>> m_FilteredPoolWeakRefs = new List<WeakReference<IBaseRefPool>>();
        private readonly HashSet<Type> m_FoldoutPoolObjectTypes = new HashSet<Type>();
        private SortingOrder m_SortingOrder = SortingOrder.ObjectType;
        private bool m_ReverseSortingOrder = false;
        private RefPoolService m_Service;

        private int ReverseSortingOrderComparisonFactor => m_ReverseSortingOrder ? -1 : 1;

        protected internal override void OnShow(object serviceInstance)
        {
            base.OnShow(serviceInstance);
            Refresh((RefPoolService)serviceInstance);
        }

        private void Refresh(RefPoolService serviceInstance)
        {
            m_Service = serviceInstance;
            m_PoolCount = m_Service.PoolCount;
            m_PoolWeakRefs.Clear();
            m_PoolWeakRefs.AddRange(m_Service.ToList().ConvertAll(pool => new WeakReference<IBaseRefPool>(pool)));

            RefreshSearch();
            RefreshSorting();
        }

        private void RefreshSearch()
        {
            m_FilteredPoolWeakRefs.Clear();

            if (!string.IsNullOrWhiteSpace(m_SearchText))
            {
                m_FilteredPoolWeakRefs.AddRange(m_PoolWeakRefs.Where(weakRef =>
                    weakRef.TryGetTarget(out var tempPool) && (tempPool.ObjectType.FullName?.ToLower().Contains(m_SearchText.ToLower()) ?? false)));
            }
        }

        private void RefreshSorting()
        {
            Comparison<WeakReference<IBaseRefPool>> compareFunc = (x, y) => 0;
            switch (m_SortingOrder)
            {
                case SortingOrder.ObjectType:
                    compareFunc = ComparePoolObjectType;
                    break;
                case SortingOrder.DropCount:
                case SortingOrder.CreateCount:
                case SortingOrder.AcquireCount:
                case SortingOrder.ReleaseCount:
                    compareFunc = (x, y) => ComparePoolObjectStatistics(x, y, m_SortingOrder.ToString());
                    break;
            }

            m_PoolWeakRefs.Sort(compareFunc);
            m_FilteredPoolWeakRefs.Sort(compareFunc);
        }

        protected internal override bool DrawContent(object serviceInstance)
        {
            var poolSectionFoldout = CheckPoolSectionFoldout();

            if (!poolSectionFoldout || m_PoolCount <= 0) return false;

            EditorGUILayout.BeginVertical();
            DrawSearchSection();
            DrawSortingOrderSection();
            DrawPoolSection();
            EditorGUILayout.EndVertical();
            return false;
        }

        private bool CheckPoolSectionFoldout()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Core.Utility.Text.Format("Pools ({0})", m_PoolCount), EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh"))
            {
                Refresh(m_Service);
            }

            EditorGUILayout.EndHorizontal();
            return true;
        }

        private void DrawSortingOrderSection()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Sorting Order:", GUILayout.Width(80f));
                var oldSortingOrder = m_SortingOrder;
                m_SortingOrder = (SortingOrder)EditorGUILayout.EnumPopup(m_SortingOrder, GUILayout.MinWidth(80f));
                if (m_SortingOrder != oldSortingOrder)
                {
                    RefreshSorting();
                }

                GUILayout.FlexibleSpace();
                var oldReverse = m_ReverseSortingOrder;
                m_ReverseSortingOrder = EditorGUILayout.ToggleLeft("Reverse", oldReverse, GUILayout.MaxWidth(80f));
                if (oldReverse != m_ReverseSortingOrder)
                {
                    RefreshSorting();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSearchSection()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Search:", GUILayout.Width(80f));
                var newSearchText = EditorGUILayout.DelayedTextField(m_SearchText);
                if (newSearchText != m_SearchText)
                {
                    newSearchText = newSearchText.Trim();
                }

                if (newSearchText != m_SearchText)
                {
                    m_SearchText = newSearchText;
                    RefreshSearch();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPoolSection()
        {
            var weakRefs = string.IsNullOrWhiteSpace(m_SearchText) ? m_PoolWeakRefs : m_FilteredPoolWeakRefs;
            EditorGUI.indentLevel += 1;
            foreach (var weakRef in weakRefs)
            {
                if (!weakRef.TryGetTarget(out var pool))
                {
                    continue;
                }

                DrawOnePool(pool);
            }

            EditorGUI.indentLevel -= 1;
        }

        private void DrawOnePool(IBaseRefPool pool)
        {
            var oldFoldout = m_FoldoutPoolObjectTypes.Contains(pool.ObjectType);
            var newFoldout = EditorGUILayout.Foldout(oldFoldout, pool.ObjectType.FullName);
            if (oldFoldout != newFoldout)
            {
                if (newFoldout)
                {
                    m_FoldoutPoolObjectTypes.Add(pool.ObjectType);
                }
                else
                {
                    m_FoldoutPoolObjectTypes.Remove(pool.ObjectType);
                }
            }

            if (!newFoldout) return;

            var stats = pool.Statistics;
            var text = Core.Utility.Text.Format("Count: {0}, Capacity: {1}\nAcquire Count: {2}\nRelease Count: {3}\nCreate Count: {4}\nDrop Count: {5}",
                pool.Count, pool.Capacity, stats.AcquireCount, stats.ReleaseCount, stats.CreateCount, stats.DropCount);
            EditorGUILayout.LabelField(text, EditorStyles.wordWrappedLabel);
        }

        private int ComparePoolObjectType(WeakReference<IBaseRefPool> x, WeakReference<IBaseRefPool> y)
        {
            x.TryGetTarget(out var xTarget);
            y.TryGetTarget(out var yTarget);
            if (xTarget == null || yTarget == null)
            {
                return 0;
            }

            return ReverseSortingOrderComparisonFactor * xTarget.ObjectType.FullName?.CompareTo(yTarget.ObjectType.FullName) ?? 0;
        }

        private int ComparePoolObjectStatistics(WeakReference<IBaseRefPool> x, WeakReference<IBaseRefPool> y, string propertyName)
        {
            x.TryGetTarget(out var xTarget);
            y.TryGetTarget(out var yTarget);
            if (xTarget == null || yTarget == null)
            {
                return 0;
            }

            var xStats = xTarget.Statistics;
            var yStats = yTarget.Statistics;
            var pi = typeof(RefPoolStatistics).GetProperty(propertyName);
            if (pi == null)
            {
                throw new NullReferenceException($"Oops, property info for '{propertyName}' not found.");
            }

            var xValue = (int)pi.GetValue(xStats);
            var yValue = (int)pi.GetValue(yStats);
            return ReverseSortingOrderComparisonFactor * xValue.CompareTo(yValue);
        }
    }
}