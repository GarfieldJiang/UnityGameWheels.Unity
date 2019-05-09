using COL.UnityGameWheels.Core;
using UnityEngine;

namespace COL.UnityGameWheels.Unity
{
    /// <summary>
    /// Default implementation of an event manager.
    /// </summary>
    [DisallowMultipleComponent]
    public class EventManager : MonoBehaviourEx, IEventManager
    {
        public IEventModule Module { get; private set; }

        public IEventArgsReleaser EventArgsReleaser
        {
            get => Module.EventArgsReleaser;
            set => Module.EventArgsReleaser = value;
        }

        public void AddEventListener(int eventId, OnHearEvent onHearEvent)
        {
            Module.AddEventListener(eventId, onHearEvent);
        }

        public void RemoveEventListener(int eventId, OnHearEvent onHearEvent)
        {
            Module.RemoveEventListener(eventId, onHearEvent);
        }

        public void SendEvent(object sender, BaseEventArgs eventArgs)
        {
            Module.SendEvent(sender, eventArgs);
        }

        public void SendEventNow(object sender, BaseEventArgs eventArgs)
        {
            Module.SendEventNow(sender, eventArgs);
        }

        public void Init()
        {
            Module.Init();
        }

        public void ShutDown()
        {
            Module.ShutDown();
        }

        #region MonoBahviour

        protected override void Awake()
        {
            base.Awake();
            Module = new EventModule();
            Module.MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
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

        #endregion MonoBahviour
    }
}