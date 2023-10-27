using System.Collections.Generic;
using System.Threading.Tasks;
using GameEngine.Base;

namespace GameEngine.Entity
{
    public class EntityService : BaseService
    {
        private readonly Dictionary<string, EntityGroup> _entityGroups = new Dictionary<string, EntityGroup>();

        public EntityGroup this[string groupName]
        {
            get
            {
                if (!_entityGroups.ContainsKey(groupName))
                {
                    _entityGroups[groupName] = new EntityGroup(groupName);
                }

                return _entityGroups[groupName];
            }
        }

        public EntityGroup GetEntityGroup(string groupName, bool createIfNotExisted = true)
        {
            if (createIfNotExisted)
            {
                return this[groupName];
            }
            else
            {
                _entityGroups.TryGetValue(groupName, out var group);
                return group;
            }
        }

        public async Task AddEntity(string groupName, IEntity entity)
        {
            var group = this[groupName];
            await group.AddEntity(entity);
        }

        public void RemoveGroup(string groupName)
        {
            var group = GetEntityGroup(groupName, false);
            if (group == null) return;
            group.RemoveAll();
            _entityGroups.Remove(groupName);
        }

        public T[] GetEntities<T>(string groupName) where T : IEntity
        {
            var group = GetEntityGroup(groupName);
            return group.GetAllEntities<T>();
        }

        public T GetEntity<T>(string groupName, int entityId) where T : IEntity
        {
            var group = this[groupName];
            return group.GetEntity<T>(entityId);
        }

        public override void OnUpdate(float elapseTime, float realElapseTime)
        {
            foreach (var entityGruop in _entityGroups.Values)
            {
                entityGruop.OnUpdate(elapseTime, realElapseTime);
            }
        }
    }
}