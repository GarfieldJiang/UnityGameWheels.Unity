using COL.UnityGameWheels.Core;
using System;
using UnityEngine;

namespace COL.UnityGameWheels.Unity
{
    [DisallowMultipleComponent]
    public class RefPoolManager : MonoBehaviourEx, IRefPoolManager
    {
        public IRefPoolModule Module { get; private set; }

        [SerializeField]
        private int m_DefaultCapacity = 1;

        public int DefaultCapacity
        {
            get { return m_DefaultCapacity; }
        }

        public int PoolCount
        {
            get { return Module.PoolCount; }
        }

        public bool ReadyToUse { get; private set; }

        public bool Contains<TObject>() where TObject : class, new()
        {
            return Module.Contains<TObject>();
        }

        public bool Contains(Type objectType)
        {
            return Module.Contains(objectType);
        }

        public IRefPool<TObject> Add<TObject>(int initCapacity) where TObject : class, new()
        {
            return Module.Add<TObject>(initCapacity);
        }

        public IRefPool<TObject> Add<TObject>() where TObject : class, new()
        {
            return Module.Add<TObject>();
        }

        public IBaseRefPool Add(Type objectType, int initCapacity)
        {
            return Module.Add(objectType, initCapacity);
        }

        public IBaseRefPool Add(Type objectType)
        {
            return Module.Add(objectType);
        }

        public IRefPool<TObject> GetOrAdd<TObject>() where TObject : class, new()
        {
            return Module.GetOrAdd<TObject>();
        }

        public IBaseRefPool GetOrAdd(Type objectType)
        {
            return Module.GetOrAdd(objectType);
        }

        public void ClearAll()
        {
            Module.ClearAll();
        }

        public void Init()
        {
            Module.Init();
            ReadyToUse = true;
        }

        public void ShutDown()
        {
            ReadyToUse = false;
            Module.ShutDown();
            Module = null;
        }

        #region MonoBahviour

        protected override void Awake()
        {
            base.Awake();
            Module = new RefPoolModule {DefaultCapacity = m_DefaultCapacity};
        }

        private void Update()
        {
            Module.Update(new TimeStruct(Time.deltaTime, Time.unscaledDeltaTime, Time.time, Time.unscaledTime));
        }

        #endregion MonoBahviour
    }
}