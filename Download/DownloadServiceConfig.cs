using COL.UnityGameWheels.Core;
using UnityEngine;

namespace COL.UnityGameWheels.Unity
{
    public class DownloadServiceConfig : ScriptableObject, IDownloadServiceConfigReader
    {
        public string TempFileExtension => m_TempFileExtension;
        public int ConcurrentDownloadCountLimit => m_ConcurrentDownloadCountLimit;
        public int ChunkSizeToSave => m_ChunkSizeToSave;
        public float Timeout => m_Timeout;

        [SerializeField]
        private string m_TempFileExtension = ".tmp";

        [SerializeField]
        private int m_ConcurrentDownloadCountLimit = 3;

        [SerializeField]
        private int m_ChunkSizeToSave = 64 * 1024;

        [SerializeField]
        private float m_Timeout = -1f;
    }
}