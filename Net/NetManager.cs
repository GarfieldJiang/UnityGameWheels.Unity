namespace COL.UnityGameWheels.Unity.Net
{
    using System.Collections.Generic;
    using Core.Net;

    public class NetManager : MonoBehaviourEx, INetManager
    {
        public INetModule Module { get; private set; }

        public INetChannelFactory ChannelFactory
        {
            get
            {
                return Module.ChannelFactory;
            }

            set
            {
                Module.ChannelFactory = value;
            }
        }

        public INetChannel AddChannel(string name, string typeKey, INetChannelHandler handler, int receivePacketHeaderLength)
        {
            return Module.AddChannel(name, typeKey, handler, receivePacketHeaderLength);
        }

        public INetChannel GetChannel(string name)
        {
            return Module.GetChannel(name);
        }

        public IList<INetChannel> GetChannels()
        {
            return Module.GetChannels();
        }

        public void GetChannels(List<INetChannel> channels)
        {
            Module.GetChannels(channels);
        }

        public bool HasChannel(string name)
        {
            return Module.HasChannel(name);
        }

        public void Init()
        {
            Module.Init();
        }

        public void ShutDown()
        {
            Module.ShutDown();
        }

        public bool TryGetChannel(string name, out INetChannel channel)
        {
            return Module.TryGetChannel(name, out channel);
        }

        #region MonoBehaviour

        protected override void Awake()
        {
            base.Awake();
            Module = new NetModule();
        }

        private void Update()
        {
            Module.Update(Utility.Time.GetTimeStruct());
        }

        protected override void OnDestroy()
        {
            Module.ShutDown();
            Module = null;
            base.OnDestroy();
        }

        #endregion MonoBehaviour
    }
}
