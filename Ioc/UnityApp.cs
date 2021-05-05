using System;
using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Core.Ioc;

namespace COL.UnityGameWheels.Unity.Ioc
{
    public class UnityApp : MonoBehaviourEx
    {
        public Container Container { get; private set; }

        public static UnityApp Instance { get; private set; }


        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            if (Instance != null)
            {
                throw new InvalidOperationException($"There is already an instance of '{nameof(UnityApp)}'.");
            }

            Instance = this;
            Container = new Container();
            Container.BindInstance<ITickService>(gameObject.AddComponent<TickService>());
        }

        protected override void OnDestroy()
        {
            if (!Container.IsDisposing && !Container.IsDisposed)
            {
                Container.Dispose();
            }

            Instance = null;
            base.OnDestroy();
        }
    }
}