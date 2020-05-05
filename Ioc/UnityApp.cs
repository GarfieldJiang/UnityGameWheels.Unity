using System;
using COL.UnityGameWheels.Core.Ioc;

namespace COL.UnityGameWheels.Unity.Ioc
{
    public class UnityApp : MonoBehaviourEx
    {
        public ITickableContainer Container { get; private set; }

        public static UnityApp Instance { get; private set; }


        public event Action OnShutdownComplete
        {
            add => Container.OnShutdownComplete += value;
            remove => Container.OnShutdownComplete -= value;
        }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != null)
            {
                throw new InvalidOperationException($"There is already an instance of '{nameof(UnityApp)}'.");
            }

            Instance = this;
            Container = new TickableContainer();
        }

        protected virtual void Update()
        {
            if (!Container.IsShuttingDown && !Container.IsShut)
            {
                Container.OnUpdate(Utility.Time.GetTimeStruct());
            }
        }

        protected override void OnDestroy()
        {
            if (!Container.IsRequestingShutdown && !Container.IsShuttingDown && !Container.IsShut)
            {
                Container.ShutDown();
            }

            Instance = null;
            base.OnDestroy();
        }
    }
}