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
        void AddLeaves(IEnumerable<string> key);
        void AddNonLeaf(string key, NonLeafOperation operation, IEnumerable<string> dependencies);
        void SetUp();
        void SetLeafValue(string key, int value);
        int GetValue(string key);
        void AddObserver(string key, IRedDotObserver observer);
        bool RemoveObserver(string key, IRedDotObserver observer);
    }
}