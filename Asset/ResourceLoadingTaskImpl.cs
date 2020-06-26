namespace COL.UnityGameWheels.Unity.Asset
{
    using System;
    using Core;
    using Core.Asset;
    using UnityEngine;

    public class ResourceLoadingTaskImpl : IResourceLoadingTaskImpl
    {
        public string ResourcePath { get; set; }

        public string ResourceParentDir { get; set; }

        public object ResourceObject { get; private set; }

        private AssetBundleCreateRequest m_AssetBundleCreateRequest = null;

        public bool IsDone => m_AssetBundleCreateRequest?.isDone ?? false;

        public float Progress => m_AssetBundleCreateRequest?.progress ?? 0f;

        public string ErrorMessage { get; private set; }

        public void OnReset()
        {
            ResourceObject = null;
            ErrorMessage = null;

            if (m_AssetBundleCreateRequest != null)
            {
                if (!m_AssetBundleCreateRequest.isDone)
                {
                    Debug.LogWarningFormat("Asset bundle create request not done for path '{0}'.", ResourcePath);
                }

                m_AssetBundleCreateRequest.completed -= OnAssetBundleCreateRequestComplete;
                m_AssetBundleCreateRequest = null;
            }

            ResourcePath = null;
            ResourceParentDir = null;
        }

        public void OnStart()
        {
            if (string.IsNullOrEmpty(ResourcePath))
            {
                throw new InvalidOperationException("Asset path is invalid.");
            }

            if (m_AssetBundleCreateRequest != null)
            {
                throw new InvalidOperationException("Already started");
            }

            m_AssetBundleCreateRequest =
                AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(ResourceParentDir, ResourcePath + Core.Asset.Constant.ResourceFileExtension));
            m_AssetBundleCreateRequest.completed += OnAssetBundleCreateRequestComplete;
        }

        private void OnAssetBundleCreateRequestComplete(AsyncOperation asyncOperation)
        {
            ResourceObject = m_AssetBundleCreateRequest.assetBundle;
            if (ResourceObject == null)
            {
                ErrorMessage = Utility.Text.Format("Failed to create asset bundle from path '{0}'.",
                    System.IO.Path.Combine(ResourceParentDir, ResourcePath));
            }
        }

        public void OnUpdate(TimeStruct timeStruct)
        {
            // Empty.
        }
    }
}