using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.ObjectPool
{
    public class GameObjectPool
    {
        public string Name { get; set; }
        public Transform ObjectRoot { get; set; }

        public GameObject GameObjectPrefab { get; set; }

        private List<GameObject> usedObjectPool = new List<GameObject>();
        private Stack<GameObject> availableStack = new Stack<GameObject>();

        public GameObject Get(Transform parent = null, bool worldPositionStays = false)
        {
            if (availableStack.Count == 0)
            {
                ExtendAvailable(1);
            }

            var obj = availableStack.Pop();
            obj.SetActive(true);
            usedObjectPool.Add(obj);
            if (parent != null)
            {
                obj.transform.SetParent(parent, worldPositionStays);
            }

            return obj;
        }

        public T Get<T>(Transform parent = null, bool worldPositionStays = false) where T : Component
        {
            var obj = Get(parent, worldPositionStays);
            return obj.GetComponent<T>();
        }

        public void InitPool(string name, int initSize)
        {
            Name = name;
            ExtendAvailable(initSize);
        }

        private void ExtendAvailable(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var newObj = Object.Instantiate(GameObjectPrefab);
                newObj.transform.SetParent(ObjectRoot, false);
                availableStack.Push(newObj);
            }
        }

        public void Recycle(GameObject poolObject)
        {
            if (usedObjectPool.Contains(poolObject))
            {
                poolObject.SetActive(false);
                usedObjectPool.Remove(poolObject);
                availableStack.Push(poolObject);
                poolObject.transform.SetParent(ObjectRoot, false);
            }
        }

        public void Destroy()
        {
            var used = usedObjectPool.ToArray();
            foreach (var go in used)
            {
                Recycle(go);
            }

            while (availableStack.Count > 0)
            {
                var go = availableStack.Pop();
                Object.Destroy(go);
            }
            
            Object.Destroy(ObjectRoot.gameObject);
        }
    }
}