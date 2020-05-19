﻿using UnityEngine;
using UnityEngine.Networking;

namespace COL.UnityGameWheels.Unity
{
    internal partial class DownloadTaskImpl
    {
        private class DownloadHandler : DownloadHandlerScript
        {
            private readonly DownloadTaskImpl m_Owner = null;

            public DownloadHandler(DownloadTaskImpl owner) : base(owner.m_InnerBuffer)
            {
                m_Owner = owner;
            }

            protected override bool ReceiveData(byte[] receivedData, int dataLength)
            {
                var ret = base.ReceiveData(receivedData, dataLength);

                var needBufferLength = m_Owner.m_OuterBufferOffset + dataLength;
                if (m_Owner.m_OuterBuffer.Length < needBufferLength)
                {
                    var oldOuterBuffer = m_Owner.m_OuterBuffer;
                    m_Owner.m_OuterBuffer = new byte[Mathf.NextPowerOfTwo(needBufferLength)];
                    System.Buffer.BlockCopy(oldOuterBuffer, 0, m_Owner.m_OuterBuffer, 0, m_Owner.m_OuterBufferOffset);
                }

                System.Buffer.BlockCopy(receivedData, 0, m_Owner.m_OuterBuffer, m_Owner.m_OuterBufferOffset, dataLength);
                m_Owner.m_OuterBufferOffset += dataLength;
                return ret;
            }
        }
    }
}