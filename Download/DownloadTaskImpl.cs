using COL.UnityGameWheels.Core;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace COL.UnityGameWheels.Unity
{
    internal partial class DownloadTaskImpl : IDownloadTaskImpl
    {
        private UnityWebRequestAsyncOperation m_WebRequestAsyncOperation = null;
        private byte[] m_Buffer = null;
        private int m_BufferOffset = 0;

        public bool IsDone
        {
            get
            {
                return m_WebRequestAsyncOperation != null && m_WebRequestAsyncOperation.webRequest.isDone;
            }
        }

        public long RealDownloadedSize
        {
            get
            {
                return m_WebRequestAsyncOperation == null ? 0L : (long)m_WebRequestAsyncOperation.webRequest.downloadedBytes;
            }
        }

        public DownloadErrorCode? ErrorCode { get; private set; }

        public string ErrorMessage { get; private set; }

        public int ChunkSizeToSave { get; set; }

        public DownloadTaskImpl()
        {
            ErrorMessage = string.Empty;
        }

        public void OnDownloadError()
        {
            m_WebRequestAsyncOperation.webRequest.Dispose();
            m_WebRequestAsyncOperation = null;
        }

        public void OnReset()
        {
            ErrorCode = null;
            ErrorMessage = string.Empty;
            m_Buffer = null;
            m_BufferOffset = 0;

            if (m_WebRequestAsyncOperation != null)
            {
                m_WebRequestAsyncOperation.webRequest.Dispose();
                m_WebRequestAsyncOperation = null;
            }
        }

        public void OnStart(string urlStr, long startByteIndex)
        {
            if (ChunkSizeToSave > 0)
            {
                m_Buffer = new byte[Mathf.NextPowerOfTwo(ChunkSizeToSave) * 2];
            }

            var webRequest = new UnityWebRequest(urlStr);
            webRequest.method = UnityWebRequest.kHttpVerbGET;
            webRequest.downloadHandler = new DownloadHandler(this);

            if (startByteIndex > 0)
            {
                webRequest.SetRequestHeader("Range", Core.Utility.Text.Format("bytes={0}-", startByteIndex));
            }
            m_WebRequestAsyncOperation = webRequest.SendWebRequest();
        }

        public void OnStop()
        {
            m_WebRequestAsyncOperation.webRequest.Dispose();
            m_WebRequestAsyncOperation = null;
        }

        public void OnTimeOut()
        {
            m_WebRequestAsyncOperation.webRequest.Dispose();
            m_WebRequestAsyncOperation = null;
        }

        public void WriteDownloadedContent(BinaryWriter bw, long offset, long size)
        {
            if (m_Buffer == null)
            {
                bw.Write(m_WebRequestAsyncOperation.webRequest.downloadHandler.data, (int)offset, (int)size);
                return;
            }

            bw.Write(m_Buffer, 0, (int)size);
            m_BufferOffset = 0;
        }

        public void Update(TimeStruct timeStruct)
        {
            var webRequest = m_WebRequestAsyncOperation.webRequest;
            if (webRequest.isNetworkError)
            {
                ErrorCode = DownloadErrorCode.Network;
                ErrorMessage = m_WebRequestAsyncOperation.webRequest.error;
                return;
            }
        }
    }
}
