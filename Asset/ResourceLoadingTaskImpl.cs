namespace COL.UnityGameWheels.Unity.Asset
{
    using System;
    using Core;
    using Core.Asset;
    using UnityEngine;

    public class ResourceLoadingTaskImpl : IResourceLoadingTaskImpl
    {
        public const int MaxTryTimes = 5;

        public string ResourcePath { get; set; }

        public string ResourceParentDir { get; set; }

        public object ResourceObject { get; private set; }

        private AssetBundleCreateRequest m_AssetBundleCreateRequest = null;
        private int m_TryCount = 0;

        public bool IsDone { get; private set; } = false;

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
            IsDone = false;
        }

        private void Restart()
        {
            m_TryCount++;
            m_AssetBundleCreateRequest =
                AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(ResourceParentDir,
                    ResourcePath + Core.Asset.Constant.ResourceFileExtension));
            m_AssetBundleCreateRequest.completed += OnAssetBundleCreateRequestComplete;
        }

        public void OnStart()
        {
            IsDone = false;
            m_TryCount = 0;
            if (string.IsNullOrEmpty(ResourcePath))
            {
                throw new InvalidOperationException("Asset path is invalid.");
            }

            if (m_AssetBundleCreateRequest != null)
            {
                throw new InvalidOperationException("Already started");
            }

            Restart();
        }

        private string CreateErrorMessage()
        {
            return $"Failed to create asset bundle from path '{System.IO.Path.Combine(ResourceParentDir, ResourcePath)}'.";
        }

        private void OnAssetBundleCreateRequestComplete(AsyncOperation asyncOperation)
        {
            ResourceObject = m_AssetBundleCreateRequest.assetBundle;
            if (ResourceObject != null)
            {
                IsDone = true;
                return;
            }

            if (m_TryCount >= MaxTryTimes)
            {
                IsDone = true;
                ErrorMessage = CreateErrorMessage();
                return;
            }

            Log.Warning(CreateErrorMessage());
            Restart();
        }

        public void OnUpdate(TimeStruct timeStruct)
        {
            // Empty.
        }
    }
}