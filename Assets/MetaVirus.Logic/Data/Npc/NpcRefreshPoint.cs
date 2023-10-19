using System;
using System.Collections;
using System.Threading.Tasks;
using cfg.map;
using GameEngine;
using GameEngine.Entity;
using GameEngine.ObjectPool;
using GameEngine.Utils;
using MetaVirus.Logic.Data.Entities;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Data.Npc
{
    public class NpcRefreshPoint : MonoBehaviour, IRecyclable
    {
        private MapNpc _info;
        private EntityService _entityService;

        public MapNpc Info
        {
            get => _info;
            set
            {
                _info = value;
                transform.position = _info.Position;
                transform.eulerAngles = _info.Rotation;
            }
        }

        public int MapId { get; set; }

        public void OnSpawn()
        {
            _entityService = GameFramework.GetService<EntityService>();
            gameObject.SetActive(true);
        }

        public void Refresh()
        {
            StartCoroutine(_Refresh());
        }

        private IEnumerator _Refresh()
        {
            var entity = new NpcEntity(_info, MapId);
            var task = _entityService.AddEntity(EntityGroupName.MapNpc, entity);
            yield return task.AsCoroution();
        }

        public void OnRecycle()
        {
            var entity = _entityService.GetEntity<NpcEntity>(EntityGroupName.MapNpc, _info.Id);

            if (entity != null)
            {
                _entityService.GetEntityGroup(EntityGroupName.MapNpc).RemoveEntity(entity);
            }

            _info = null;
            gameObject.SetActive(false);
        }

        public void OnDestroy()
        {
        }

        private void Update()
        {
            if (_info == null) return;
        }
    }
}