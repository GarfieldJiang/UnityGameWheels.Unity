using UnityEngine.Networking;

namespace COL.UnityGameWheels.Unity.Asset
{
    using Core.Asset;
    using System;
    using System.Collections;
    using System.IO;
    using UnityEngine;

    internal class AssetIndexForInstallerLoader : IAssetIndexForInstallerLoader
    {
        private readonly MonoBehaviourEx m_MonoBehaviour;

        public AssetIndexForInstallerLoader(MonoBehaviourEx behaviour)
        {
            if (behaviour == null)
            {
                throw new ArgumentNullException("behaviour");
            }

            m_MonoBehaviour = behaviour;
        }

        public void Load(string path, LoadAssetIndexForInstallerCallbackSet callbackSet, object context)
        {
            if (m_MonoBehaviour == null || !m_MonoBehaviour.isActiveAndEnabled)
            {
                throw new InvalidOperationException("Behaviour is not ready to use.");
            }

            m_MonoBehaviour.StartCoroutine(LoadCo(path, callbackSet, context));
        }

        private IEnumerator LoadCo(string path, LoadAssetIndexForInstallerCallbackSet callbackSet, object context)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                path = "file://" + path;
            }

            var www = UnityWebRequest.Get(path);
            yield return www.SendWebRequest();
            if (!string.IsNullOrEmpty(www.error))
            {
                if (callbackSet.OnFailure != null)
                {
                    callbackSet.OnFailure(context);
                }
            }
            else
            {
                if (callbackSet.OnSuccess != null)
                {
                    using (var stream = new MemoryStream(www.downloadHandler.data))
                    {
                        callbackSet.OnSuccess(stream, context);
                    }
                }
            }
        }
    }
}