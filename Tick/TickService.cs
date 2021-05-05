using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using COL.UnityGameWheels.Core;

namespace COL.UnityGameWheels.Unity
{
    public class TickService : MonoBehaviourEx, ITickService
    {
        private readonly SortedDictionary<int, HashSet<Action<TimeStruct>>> m_OrderToUpdateCallbackCollectionMap =
            new SortedDictionary<int, HashSet<Action<TimeStruct>>>();

        private readonly Dictionary<Action<TimeStruct>, int> m_UpdateCallbackToOrderMap = new Dictionary<Action<TimeStruct>, int>();

        private readonly SortedDictionary<int, HashSet<Action<TimeStruct>>> m_OrderToLateUpdateCallbackCollectionMap =
            new SortedDictionary<int, HashSet<Action<TimeStruct>>>();

        private readonly Dictionary<Action<TimeStruct>, int> m_LateUpdateCallbackToOrderMap = new Dictionary<Action<TimeStruct>, int>();

        private readonly Queue<Action<TimeStruct>> m_TickQueue = new Queue<Action<TimeStruct>>();

        public void AddUpdateCallback(Action<TimeStruct> updateCallback, int order)
        {
            AddCallback(m_UpdateCallbackToOrderMap, m_OrderToUpdateCallbackCollectionMap, updateCallback, order);
        }

        public void AddLateUpdateCallback(Action<TimeStruct> lateUpdateCallback, int order)
        {
            AddCallback(m_LateUpdateCallbackToOrderMap, m_OrderToLateUpdateCallbackCollectionMap, lateUpdateCallback, order);
        }

        public bool RemoveUpdateCallback(Action<TimeStruct> updateCallback)
        {
            return RemoveCallback(m_UpdateCallbackToOrderMap, m_OrderToUpdateCallbackCollectionMap, updateCallback);
        }

        public bool RemoveLateUpdateCallback(Action<TimeStruct> lateUpdateCallback)
        {
            return RemoveCallback(m_LateUpdateCallbackToOrderMap, m_OrderToLateUpdateCallbackCollectionMap, lateUpdateCallback);
        }

        private void Update()
        {
            Tick(m_UpdateCallbackToOrderMap, m_OrderToUpdateCallbackCollectionMap, m_TickQueue);
        }

        private void LateUpdate()
        {
            Tick(m_LateUpdateCallbackToOrderMap, m_OrderToLateUpdateCallbackCollectionMap, m_TickQueue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddCallback(Dictionary<Action<TimeStruct>, int> callbackToOrderMap,
            SortedDictionary<int, HashSet<Action<TimeStruct>>> orderToCallbackCollectionMap, Action<TimeStruct> callback, int order)
        {
            try
            {
                callbackToOrderMap.Add(callback, order);
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException($"Duplicated callback.");
            }

            EnsureCallbackOrderCollection(orderToCallbackCollectionMap, order).Add(callback);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool RemoveCallback(Dictionary<Action<TimeStruct>, int> callbackToOrderMap,
            SortedDictionary<int, HashSet<Action<TimeStruct>>> orderToCallbackCollectionMap, Action<TimeStruct> callback)
        {
            if (!callbackToOrderMap.TryGetValue(callback, out var order))
            {
                return false;
            }

            EnsureCallbackOrderCollection(orderToCallbackCollectionMap, order).Remove(callback);
            return true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static HashSet<Action<TimeStruct>> EnsureCallbackOrderCollection(SortedDictionary<int, HashSet<Action<TimeStruct>>> parentCollection, int order)
        {
            if (!parentCollection.TryGetValue(order, out var ret))
            {
                ret = new HashSet<Action<TimeStruct>>();
                parentCollection.Add(order, ret);
            }

            return ret;
        }

        private static void Tick(Dictionary<Action<TimeStruct>, int> callbackToOrderMap,
            SortedDictionary<int, HashSet<Action<TimeStruct>>> orderToCallbackCollectionMap,
            Queue<Action<TimeStruct>> tickQueue)
        {
            try
            {
                foreach (var kv in orderToCallbackCollectionMap)
                {
                    foreach (var callback in kv.Value)
                    {
                        tickQueue.Enqueue(callback);
                    }
                }

                foreach (var callback in tickQueue)
                {
                    if (callbackToOrderMap.ContainsKey(callback))
                    {
                        callback(Utility.Time.GetTimeStruct());
                    }
                }
            }
            finally
            {
                tickQueue.Clear();
            }
        }
    }
}