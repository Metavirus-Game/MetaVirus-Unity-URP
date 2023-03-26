using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameEngine;
using GameEngine.Entity;
using GameEngine.Event;
using GameEngine.ObjectPool;
using GameEngine.Utils;
using MetaVirus.Logic.Data.Events;
using MetaVirus.Logic.Data.Npc;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Player;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace MetaVirus.Logic.Data.Entities
{
    public class NetPlayerGridItemEntity : GridItemEntity, IRecyclable
    {
        private GameObject _netPlayerObj;
        private NetPlayerController _netPlayerController;
        private CharacterTemplate _characterTemplate;

        public NetPlayerController Controller => _netPlayerController;

        public override GameObject NpcHUDPos { get; protected set; }

        public NetPlayerGridItemEntity() : base(null)
        {
        }

        public NetPlayerGridItemEntity(GridItem gridItem) : base(gridItem)
        {
        }

        public NetPlayerGridItemEntity SetGridItem(GridItem gridItem)
        {
            GridItem = gridItem;
            return this;
        }


        public override async Task<IEntity> LoadEntityAsync()
        {
            if (_netPlayerObj != null)
            {
                _netPlayerController.SetControlData(GridItemGo, GridItem);
                _netPlayerController.SetRotation(GridItem.Rotation);
                _characterTemplate.ParseFromLongData(GridItem.Avatar);
                
                _netPlayerObj.transform.position = GridItem.Position;
                _netPlayerObj.name = $"NetPlayer_{GridItem.ID}-{GridItem.Name}";
                _netPlayerObj.SetActive(true);

                GameFramework.GetService<EventService>().Emit(GameEvents.MapEvent.GridItemEvent,
                    new GridItemEvent(GridItemEvent.GridItemEventType.Spawn, Id, MapId, Type));
                return this;
            }

            const string netPlayerAddress = Constants.ResAddress.NetPlayerPrefab;
            var prefabAddress = Constants.ResAddress.PlayerResPrefab(GridItem.Gender);


            var pTask = Addressables.InstantiateAsync(netPlayerAddress).Task;
            var gTask = Addressables.InstantiateAsync(prefabAddress).Task;

            //加载NetPlayer
            var player = await pTask;
            player.SetActive(false);
            player.name = $"NetPlayer_{GridItem.ID}-{GridItem.Name}";
            GridItemGo = await gTask;
            GridItemGo.SetActive(true);

            //setup
            _netPlayerController = player.GetComponent<NetPlayerController>();
            GridItemGo.transform.SetParent(player.transform, false);

            _characterTemplate = GridItemGo.GetComponent<CharacterTemplate>();
            _characterTemplate.ParseFromLongData(GridItem.Avatar);

            player.transform.position = GridItem.Position;
            _netPlayerController.SetControlData(GridItemGo, GridItem);
            _netPlayerController.SetRotation(GridItem.Rotation);

            player.SetActive(true);

            foreach (var comp in GridItemGo.GetComponentsInChildren<Transform>())
            {
                if (comp.gameObject.name == "HUDPos")
                {
                    NpcHUDPos = comp.gameObject;
                    break;
                }
            }

            if (NpcHUDPos == null)
            {
                NpcHUDPos = GridItemGo;
            }

            Object.DontDestroyOnLoad(player);

            GameFramework.GetService<EventService>().Emit(GameEvents.MapEvent.GridItemEvent,
                new GridItemEvent(GridItemEvent.GridItemEventType.Spawn, Id, MapId, Type));

            _netPlayerObj = player;

            return this;
        }

        public void UpdateMove(GridItem gridItem)
        {
            //GridItem = gridItem;
            if (_netPlayerController != null)
                _netPlayerController.AddWayPoint(gridItem.Position, gridItem.Rotation);
        }

        public override void OnUpdate(float timeElapse, float realTimeElapse)
        {
            if (_netPlayerObj != null)
            {
                //每帧将位置数据会写至GridItem中
                GridItem.UpdatePosition(_netPlayerObj.transform.position);
                GridItem.UpdateRotation(_netPlayerObj.transform.eulerAngles);
            }
        }

        public override void OnTimeScaleChanged()
        {
        }

        public override void OnRelease()
        {
            GameFramework.GetService<EventService>().Emit(GameEvents.MapEvent.GridItemEvent,
                new GridItemEvent(GridItemEvent.GridItemEventType.Destroy, Id, MapId, Type));

            Recycle(this);

            //base.OnRelease();
        }

        public void OnSpawn()
        {
        }

        public void OnRecycle()
        {
            _netPlayerObj.SetActive(false);
            _netPlayerController.ClearControlData();
        }

        public void OnDestroy()
        {
            if (GridItemGo != null)
            {
                Addressables.ReleaseInstance(GridItemGo);
                GridItemGo = null;
            }

            if (_netPlayerObj != null)
            {
                Addressables.ReleaseInstance(_netPlayerObj);
                _netPlayerObj = null;
            }
        }
    }
}