using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.ObjectPool
{
    public class ObjectPool<T> : IObjectPool where T : IRecyclable, new()
    {
        private readonly List<T> _usedObjectPool = new List<T>();
        private readonly Stack<T> _availableStack = new Stack<T>();

        private int _capacity;
        private string _name;
        private Type _itemType;
        public int Count => _usedObjectPool.Count + _availableStack.Count;
        public string Name => $"{_name}_{_itemType.Name}_Pool";

        private Func<T> _newObjFunc;

        public ObjectPool(string name, int capacity, int initSize, Func<T> newObjFunc = null)
        {
            _itemType = typeof(T);
            _name = name;
            _capacity = capacity;
            _newObjFunc = newObjFunc;
            ExtendPoolItemSpace(initSize);
        }

        public T Get<T>()
        {
            if (typeof(T) != _itemType)
            {
                return default;
            }

            return (T)Get();
        }

        public IRecyclable Get()
        {
            if (_availableStack.Count == 0)
            {
                ExtendPoolItemSpace(1);
            }

            var ret = _availableStack.Pop();
            _usedObjectPool.Add(ret);
            ret.OnSpawn();
            return ret;
        }

        public void Release(IEnumerable<IRecyclable> items)
        {
            foreach (var item in items)
            {
                Release(item);
            }
        }

        public void Release(IRecyclable item)
        {
            Release((T)item);
        }

        public void Release(T item)
        {
            if (typeof(T) != _itemType)
            {
                return;
            }

            if (_usedObjectPool.Contains(item))
            {
                _usedObjectPool.Remove(item);
                if (Count >= _capacity)
                {
                    Debug.LogWarning($"{Name} is full, can't recycle item, dropped");
                    item.OnDestroy();
                }
                else
                {
                    _availableStack.Push(item);
                }

                item.OnRecycle();
            }
            else
            {
                Debug.LogError($"{item} is not in the used pool!");
            }
        }

        public void ReleaseAll()
        {
            foreach (var obj in _usedObjectPool.ToArray())
            {
                Release(obj);
            }
        }

        public void RemoveAllUnusedItems()
        {
            foreach (var item in _availableStack)
            {
                item.OnDestroy();
            }

            _availableStack.Clear();
        }

        /// <summary>
        /// 清除对象池中所有的对象，并销毁
        /// </summary>
        public void ClearAll()
        {
            ReleaseAll();
            RemoveAllUnusedItems();
        }

        public void ExtendPoolItemSpace(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var item = _newObjFunc != null ? _newObjFunc() : new T();

                _availableStack.Push(item);
            }
        }
    }
}