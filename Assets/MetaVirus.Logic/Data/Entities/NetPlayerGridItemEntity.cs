using System.Collections.Generic;
using System.Threading.Tasks;
using GameEngine;
using GameEngine.Entity;
using GameEngine.Event;
using GameEngine.Utils;
using MetaVirus.Logic.Data.Events;
using MetaVirus.Logic.Data.Npc;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Player;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MetaVirus.Logic.Data.Entities
{
    public class NetPlayerGridItemEntity : GridItemEntity
    {
        private GameObject _netPlayerObj;
        private NetPlayerController _netPlayerController;
        private CharacterTemplate _characterTemplate;

        public NetPlayerController Controller => _netPlayerController;

        public override GameObject NpcHUDPos { get; protected set; }

        public NetPlayerGridItemEntity(GridItem gridItem) : base(gridItem)
        {
        }


        public override async Task<IEntity> LoadEntityAsync()
        {
            const string netPlayerAddress = Constants.ResAddress.NetPlayerPrefab;

            //加载NetPlayer

            var player = await Addressables.InstantiateAsync(netPlayerAddress).Task;
            player.SetActive(false);
            player.name = $"NetPlayer_{GridItem.ID}-{GridItem.Name}";

            _netPlayerController = player.GetComponent<NetPlayerController>();
            //加载角色资源

            var prefabAddress = Constants.ResAddress.PlayerResPrefab(GridItem.Gender);

            GridItemGo = await Addressables.InstantiateAsync(prefabAddress).Task;
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
            base.OnRelease();
            Addressables.ReleaseInstance(_netPlayerObj);
        }
    }
}