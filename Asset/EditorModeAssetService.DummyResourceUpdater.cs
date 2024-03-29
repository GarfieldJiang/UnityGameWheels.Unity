﻿#if UNITY_EDITOR
using COL.UnityGameWheels.Core.Asset;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Unity.Asset
{
    internal partial class EditorModeAssetService
    {
        private class DummyResourceUpdater : IResourceUpdater
        {
            public bool IsReady => true;

            public IEnumerable<int> GetAvailableResourceGroupIds()
            {
                return new int[0];
            }

            public void GetAvailableResourceGroupIds(List<int> groupIds)
            {
                groupIds.Clear();
            }

            public bool ResourceGroupIdIsAvailable(int groupId)
            {
                return true;
            }

            public ResourceGroupStatus GetResourceGroupStatus(int groupId)
            {
                return ResourceGroupStatus.UpToDate;
            }

            public void StopAllUpdatingResourceGroups()
            {
                // Empty.
            }

            public ResourceGroupUpdateSummary GetResourceGroupUpdateSummary(int groupId)
            {
                return new ResourceGroupUpdateSummary();
            }

            public void StartUpdatingResourceGroup(int groupId, ResourceGroupUpdateCallbackSet callbackSet, object context)
            {
                callbackSet.OnAllSuccess?.Invoke(context);
            }

            public bool StopUpdatingResourceGroup(int groupId)
            {
                return false;
            }
        }
    }
}

#endif