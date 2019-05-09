namespace COL.UnityGameWheels.Unity.Net
{
    using Core.Net;
    using System.Collections.Generic;

    public interface INetManager : IManager
    {
        INetChannelFactory ChannelFactory { get; set; }

        IList<INetChannel> GetChannels();

        void GetChannels(List<INetChannel> channels);

        INetChannel GetChannel(string name);

        bool TryGetChannel(string name, out INetChannel channel);

        bool HasChannel(string name);

        INetChannel AddChannel(string name, string typeKey, INetChannelHandler handler, int receivePacketHeaderLength);
    }
}
