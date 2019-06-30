using System;
using System.Collections.Generic;
using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Core.RedDot;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.RedDot
{
    public class RedDotManager : MonoBehaviourEx, IRedDotManager
    {
        public IRedDotModule Module { get; private set; }

        public void Init()
        {
        }

        public void ShutDown()
        {
        }

        public bool IsSetUp => Module.IsSetUp;

        public event Action OnSetUp
        {
            add => Module.OnSetUp += value;
            remove => Module.OnSetUp -= value;
        }

        public void AddLeaf(string key)
        {
            Module.AddLeaf(key);
        }

        public bool HasNode(string key)
        {
            return Module.HasNode(key);
        }

        public bool HasNode(string key, RedDotNodeType nodeType)
        {
            return Module.HasNode(key, nodeType);
        }

        public RedDotNodeType GetNodeType(string key)
        {
            return Module.GetNodeType(key);
        }

        public IEnumerable<string> GetNodeKeys(RedDotNodeType nodeType)
        {
            return Module.GetNodeKeys(nodeType);
        }

        public IEnumerable<string> GetNodeKeys()
        {
            return Module.GetNodeKeys();
        }

        public int NodeCount => Module.NodeCount;

        public int GetNodeCount(RedDotNodeType nodeType)
        {
            return Module.GetNodeCount(nodeType);
        }

        public IEnumerable<string> GetDependencies(string key)
        {
            return Module.GetDependencies(key);
        }

        public int GetDependencyCount(string key)
        {
            return Module.GetDependencyCount(key);
        }

        public IEnumerable<string> GetReverseDependencies(string key)
        {
            return Module.GetReverseDependencies(key);
        }

        public int GetReverseDependencyCount(string key)
        {
            return Module.GetReverseDependencyCount(key);
        }

        public void AddLeaves(IEnumerable<string> key)
        {
            Module.AddLeaves(key);
        }

        public void AddNonLeaf(string key, NonLeafOperation operation, IEnumerable<string> dependencies)
        {
            Module.AddNonLeaf(key, operation, dependencies);
        }

        public void SetUp()
        {
            Module.SetUp();
        }

        public void SetLeafValue(string key, int value)
        {
            Module.SetLeafValue(key, value);
        }

        public int GetValue(string key)
        {
            return Module.GetValue(key);
        }

        public void AddObserver(string key, IRedDotObserver observer)
        {
            Module.AddObserver(key, observer);
        }

        public bool RemoveObserver(string key, IRedDotObserver observer)
        {
            return Module.RemoveObserver(key, observer);
        }

        protected override void Awake()
        {
            base.Awake();
            Module = new RedDotModule();
        }

        protected override void OnDestroy()
        {
            Module = null;
            base.OnDestroy();
        }

        private void Update()
        {
            Module.Update(new TimeStruct(Time.deltaTime, Time.unscaledDeltaTime, Time.time, Time.unscaledTime));
        }
    }
}