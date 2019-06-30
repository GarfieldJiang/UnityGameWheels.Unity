using System;
using System.Collections.Generic;
using COL.UnityGameWheels.Core.RedDot;

namespace COL.UnityGameWheels.Unity.RedDot
{
    public interface IRedDotManager : IManager
    {
        bool IsSetUp { get; }
        event Action OnSetUp;
        void AddLeaf(string key);
        bool HasNode(string key);
        bool HasNode(string key, RedDotNodeType nodeType);
        RedDotNodeType GetNodeType(string key);
        IEnumerable<string> GetNodeKeys(RedDotNodeType nodeType);
        IEnumerable<string> GetNodeKeys();
        int NodeCount { get; }
        int GetNodeCount(RedDotNodeType nodeType);
        IEnumerable<string> GetDependencies(string key);
        int GetDependencyCount(string key);
        IEnumerable<string> GetReverseDependencies(string key);
        int GetReverseDependencyCount(string key);
        void AddLeaves(IEnumerable<string> key);
        void AddNonLeaf(string key, NonLeafOperation operation, IEnumerable<string> dependencies);
        void SetUp();
        void SetLeafValue(string key, int value);
        int GetValue(string key);
        void AddObserver(string key, IRedDotObserver observer);
        bool RemoveObserver(string key, IRedDotObserver observer);
    }
}