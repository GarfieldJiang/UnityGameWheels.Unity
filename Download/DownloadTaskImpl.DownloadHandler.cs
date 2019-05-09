using UnityEngine;
using UnityEngine.Networking;

namespace COL.UnityGameWheels.Unity
{
    internal partial class DownloadTaskImpl
    {
        private class DownloadHandler : DownloadHandlerScript
        {
            private DownloadTaskImpl m_Owner = null;

            public DownloadHandler(DownloadTaskImpl owner)
            {
                m_Owner = owner;
            }

            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                var ret = base.ReceiveData(data, dataLength);
                if (m_Owner.m_Buffer == null)
                {
                    return ret;
                }

                var needBufferLength = m_Owner.m_BufferOffset + dataLength;
                if (m_Owner.m_Buffer.Length < needBufferLength)
                {
                    var oldBuffer = m_Owner.m_Buffer;
                    m_Owner.m_Buffer = new byte[Mathf.NextPowerOfTwo(needBufferLength)];
                    System.Buffer.BlockCopy(oldBuffer, 0, m_Owner.m_Buffer, 0, m_Owner.m_BufferOffset);
                }

                System.Buffer.BlockCopy(data, 0, m_Owner.m_Buffer, m_Owner.m_BufferOffset, dataLength);
                m_Owner.m_BufferOffset += dataLength;
                return ret;
            }
        }
    }
}