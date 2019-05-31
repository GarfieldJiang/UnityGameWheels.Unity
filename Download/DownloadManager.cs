using COL.UnityGameWheels.Core;
using UnityEngine;

namespace COL.UnityGameWheels.Unity
{
    /// <summary>
    /// Default implementation of a download manager, using <see cref="UnityEngine.Networking.UnityWebRequest"/>.
    /// </summary>
    public class DownloadManager : MonoBehaviourEx, IDownloadManager
    {
        [SerializeField]
        private string m_TempFileExtension = ".tmp";

        [SerializeField]
        private int m_ConcurrentDownloadCountLimit = 3;

        [SerializeField]
        private int m_ChunkSizeToSave = 64 * 1024;

        [SerializeField]
        private float m_Timeout = -1f;

        public IDownloadModule Module { get; private set; }

        public IRefPoolModule RefPoolModule
        {
            get
            {
                return Module.RefPoolModule;
            }

            set
            {
                Module.RefPoolModule = value;
            }
        }

        public void Init()
        {
            Module.DownloadTaskPool.RefPoolModule = RefPoolModule;
            Module.DownloadTaskPool.Init();
            Module.Init();
        }

        public void ShutDown()
        {
            Module.ShutDown();
        }

        public int StartDownloading(DownloadTaskInfo downloadTaskInfo)
        {
            return Module.StartDownloading(downloadTaskInfo);
        }

        public bool StopDownloading(int taskId)
        {
            return Module.StopDownloading(taskId);
        }

        #region MonoBehaviour

        protected override void Awake()
        {
            base.Awake();
            Module = new DownloadModule();
            Module.TempFileExtension = m_TempFileExtension;
            Module.ChunkSizeToSave = m_ChunkSizeToSave;
            Module.ConcurrentDownloadCountLimit = m_ConcurrentDownloadCountLimit;
            Module.Timeout = m_Timeout;
            Module.DownloadTaskImplFactory = new DownloadTaskImplFactory();
            Module.DownloadTaskPool = new DownloadTaskPool();
            Module.DownloadTaskPool.DownloadModule = Module;
        }

        private void Update()
        {
            Module.Update(Utility.Time.GetTimeStruct());
        }

        protected override void OnDestroy()
        {
            Module = null;
            base.OnDestroy();
        }

        #endregion MonoBehaviour
    }
}
