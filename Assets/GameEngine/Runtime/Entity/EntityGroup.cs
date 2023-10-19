using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameEngine.Entity
{
    public class EntityGroup
    {
        private readonly List<IEntity> _entities;

        public string Name { get; }

        public int Count => _entities.Count;

        public EntityGroup(string name)
        {
            _entities = new List<IEntity>();
            Name = name;
        }

        public async Task<IEntity> AddEntity(IEntity entity)
        {
            var e = GetEntity(entity.Id);
            if (e != null)
            {
                Debug.LogWarning($"[EntityService] Entity with Id[{e.Id}] Alread existed, will be overrided");
                RemoveEntity(e);
            }

            entity.Removed = false;
            _entities.Add(entity);

            entity.OnInit(this);
            await entity.LoadEntityAsync();
            if (entity.Removed)
            {
                //Load过程中被Release了，重新调用OnRelease确保资源释放正确
                entity.OnRelease();
            }
            return entity;
        }

        public IEntity GetEntity(long entityId)
        {
            var result = _entities.Find(entity => entity.Id == entityId);
            return result;
        }

        public T GetEntity<T>(int entityId) where T : IEntity
        {
            return (T)GetEntity(entityId);
        }

        public void SetEntityTimeScale(int entityId, float timeScale)
        {
            var entity = GetEntity(entityId);
            if (entity != null)
            {
                entity.TimeScale = timeScale;
            }
        }

        public void SetTimeScale(float timeScale)
        {
            foreach (var entity in _entities)
            {
                entity.TimeScale = timeScale;
            }
        }

        public T[] GetAllEntities<T>() where T : IEntity
        {
            var ret = new T[_entities.Count];

            for (var i = 0; i < _entities.Count; i++)
            {
                ret[i] = (T)_entities[i];
            }

            return ret;
        }


        public void OnUpdate(float elapseTime, float realElapseTime)
        {
            foreach (var entity in _entities)
            {
                entity.OnUpdate(elapseTime, realElapseTime);
            }
        }

        public void RemoveEntity(int entityId)
        {
            var entity = GetEntity(entityId);
            if (entity != null)
            {
                RemoveEntity(entity);
            }
        }

        public bool RemoveEntity(IEntity entity)
        {
            var ret = _entities.Remove(entity);
            if (ret)
            {
                entity.OnRelease();
                entity.Removed = true;
            }

            return ret;
        }

        public void RemoveAll()
        {
            foreach (var entity in _entities.ToArray())
            {
                RemoveEntity(entity);
            }
        }
    }
}