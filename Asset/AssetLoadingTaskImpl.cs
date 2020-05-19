namespace COL.UnityGameWheels.Unity.Asset
{
    using System;
    using Core;
    using Core.Asset;
    using UnityEngine;

    public class AssetLoadingTaskImpl : IAssetLoadingTaskImpl
    {
        public object ResourceObject { get; set; }

        private AssetBundle AssetBundle => (AssetBundle)ResourceObject;

        public string AssetPath { get; set; }

        private object m_AssetObject = null;

        public object AssetObject => m_AssetObject;

        public bool IsDone => m_AssetBundleRequest != null && m_AssetBundleRequest.isDone;

        public float Progress => m_AssetBundleRequest?.progress ?? 0f;

        public string ErrorMessage { get; private set; }

        private AssetBundleRequest m_AssetBundleRequest = null;

        public void OnReset()
        {
            ResourceObject = null;
            m_AssetObject = null;
            ErrorMessage = null;

            if (m_AssetBundleRequest != null)
            {
                if (!m_AssetBundleRequest.isDone)
                {
                    Debug.LogWarningFormat("Asset bundle request not done for asset '{0}'.", AssetPath);
                }

                m_AssetBundleRequest = null;
            }

            AssetPath = string.Empty;
        }

        public void OnStart()
        {
            if (ResourceObject == null || AssetBundle == null)
            {
                throw new InvalidOperationException("AssetBundle (ResourceObject) is invalid.");
            }

            if (string.IsNullOrEmpty(AssetPath))
            {
                throw new InvalidOperationException("Asset path is invalid.");
            }

            if (m_AssetBundleRequest != null)
            {
                throw new InvalidOperationException("Already started");
            }

            var assetName = AssetPath; //System.IO.Path.GetFileNameWithoutExtension(AssetPath);
            m_AssetBundleRequest = AssetBundle.LoadAssetAsync(assetName);
            m_AssetBundleRequest.completed += OnAssetBundleRequestComplete;
        }

        private void OnAssetBundleRequestComplete(AsyncOperation asyncOperation)
        {
            m_AssetObject = m_AssetBundleRequest.asset;
            if (m_AssetObject == null)
            {
                ErrorMessage = Utility.Text.Format("Failed to load asset '{0}' from asset bundle '{1}'.", AssetPath, ResourceObject);
            }
        }

        public void OnUpdate(TimeStruct timeStruct)
        {
            // Empty.
        }
    }
}