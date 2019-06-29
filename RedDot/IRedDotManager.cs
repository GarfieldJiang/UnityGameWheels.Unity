using System.Collections;
using System.Collections.Generic;
using COL.UnityGameWheels.Core.RedDot;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.RedDot
{
    public interface IRedDotManager : IManager
    {
        void AddLeaf(string key);

        void AddLeaves(IEnumerable<string> key);

        void AddNonLeaf(string key, NonLeafOperation operation, IEnumerable<string> dependsOn);

        void SetUp();

        void SetLeafValue(string key, int value);

        int GetValue(string key);

        void AddObserver(string key, IRedDotObserver observer);

        bool RemoveObserver(string key, IRedDotObserver observer);
    }
}