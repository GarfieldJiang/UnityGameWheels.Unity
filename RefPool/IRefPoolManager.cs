using COL.UnityGameWheels.Core;
using System;

namespace COL.UnityGameWheels.Unity
{
    public interface IRefPoolManager : IManager
    {
        IRefPoolModule Module { get; }

        int DefaultCapacity { get; }

        int PoolCount { get; }

        bool Contains<TObject>() where TObject : class, new();

        bool Contains(Type objectType);

        IRefPool<TObject> Add<TObject>(int initCapacity) where TObject : class, new();

        IRefPool<TObject> Add<TObject>() where TObject : class, new();

        IBaseRefPool Add(Type objectType, int initCapacity);

        IBaseRefPool Add(Type objectType);

        IRefPool<TObject> GetOrAdd<TObject>() where TObject : class, new();

        IBaseRefPool GetOrAdd(Type objectType);

        void ClearAll();
    }
}