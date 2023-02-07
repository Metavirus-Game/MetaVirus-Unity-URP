using System;
using System.Collections.Generic;
using GameEngine.Base;
using GameEngine.Base.Attributes;
using static GameEngine.Common.EngineConsts;

namespace GameEngine.ObjectPool
{
    [ServicePriority(ServicePriorityValue.ObjectPool)]
    public class ObjectPoolService : BaseService
    {
        private readonly Dictionary<TypeNamePair, IObjectPool> _objectPools =
            new Dictionary<TypeNamePair, IObjectPool>();

        public ObjectPool<T> GetObjectPool<T>(string poolName, bool createIfNotExist = true)
            where T : IRecyclable, new()
        {
            var objKey = new TypeNamePair(poolName, typeof(T));
            _objectPools.TryGetValue(objKey, out var objectPool);
            if (objectPool == null)
            {
                if (createIfNotExist)
                {
                    objectPool = NewObjectPool<T>(poolName);
                }
                else
                {
                    return null;
                }
            }

            return (ObjectPool<T>)objectPool;
        }

        private void RemoveObjectPool<T>(string poolName)
        {
            var objKey = new TypeNamePair(poolName, typeof(T));
            _objectPools.Remove(objKey);
        }

        public void ClearObjectPool<T>(string poolName) where T : IRecyclable, new()
        {
            var objectPool = GetObjectPool<T>(poolName, false);
            if (objectPool != null)
            {
                objectPool.ClearAll();
                RemoveObjectPool<T>(poolName);
            }
        }

        public ObjectPool<T> NewObjectPool<T>(string poolName, int capacity = 50, int initSize = 10,
            Func<T> newObjFunc = null)
            where T : IRecyclable, new()
        {
            var opKey = new TypeNamePair(poolName, typeof(T));
            if (_objectPools.ContainsKey(opKey))
            {
                return (ObjectPool<T>)_objectPools[opKey];
            }

            var op = new ObjectPool<T>(poolName, capacity, initSize, newObjFunc);
            _objectPools[opKey] = op;
            return op;
        }

        public void RemoveAllUnusedItems()
        {
            foreach (var pool in _objectPools.Values)
            {
                pool.RemoveAllUnusedItems();
            }
        }
    }
}