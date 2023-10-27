using System.Collections.Generic;
using System.Threading.Tasks;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.Entity;
using GameEngine.Event;
using MetaVirus.Logic.Data.Events;
using MetaVirus.Logic.Data.Events.Player;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Player;
using MetaVirus.Logic.Service.Player;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Data.Entities
{
    public class PlayerEntity : BaseEntity
    {
        private readonly struct InteractiveNpc
        {
            public int Id { get; }

            public InteractiveNpc(int npcId)
            {
                Id = npcId;
            }

            public NpcEntity NpcEntity => GameFramework.GetService<EntityService>()
                .GetEntity<NpcEntity>(EntityGroupName.MapNpc, Id);

            public float Distance => Vector3.Distance(Current.Position, NpcEntity.Position);

            public override bool Equals(object obj)
            {
                if (obj is InteractiveNpc n)
                {
                    return n.Id == Id;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return Id;
            }
        }

        public PlayerInfo PlayerInfo { get; }

        public override int Id => PlayerInfo.PlayerId;

        public Vector3 Position => Player.transform.position;

        public Vector3 Rotation => PlayerObject.transform.eulerAngles;

        public GameObject Player { get; private set; }
        public PlayerController PlayerController { get; private set; }

        //可交互的npc列表
        private readonly List<InteractiveNpc> _interactiveNpcList = new();

        private float _avoidBattleTimer = 0;

        private EventService _eventService;

        public float AvoidBattleTimer
        {
            set => _avoidBattleTimer = value;
        }

        public bool AvoidBattle
        {
            get => _avoidBattleTimer > 0;
            set => _avoidBattleTimer = value ? 2 : 0;
        }

        public GameObject PlayerObject => PlayerController.PlayerObject;

        public static PlayerEntity Current => GameFramework.GetService<PlayerService>().CurrentPlayer;

        public PlayerEntity(PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
            _eventService = GameFramework.GetService<EventService>();
            _eventService.On<NpcEvent>(GameEvents.MapEvent.NpcEvent, OnNpcEvent);
        }

        private void OnNpcEvent(NpcEvent evt)
        {
            if (evt.EvtType == NpcEvent.NpcEventType.Destroy)
            {
                RemoveNpcFromInteractiveList(evt.NpcId);
            }
        }

        private int[] GetInteractiveNpcList()
        {
            var ids = new int[_interactiveNpcList.Count];
            for (var i = 0; i < _interactiveNpcList.Count; i++)
            {
                ids[i] = _interactiveNpcList[i].Id;
            }

            return ids;
        }

        public void AddNpcToInteractiveList(int npcId)
        {
            var iNpc = new InteractiveNpc(npcId);

            if (!_interactiveNpcList.Contains(iNpc))
            {
                _interactiveNpcList.Add(iNpc);
                _interactiveNpcList.Sort((npc1, npc2) => (int)(npc1.Distance - npc2.Distance));
                _eventService.Emit(GameEvents.PlayerEvent.InteractiveNpcListChanged,
                    new PlayerInteractiveNpcListChangedEvent(PlayerInteractiveNpcListChangedEvent.EventType.Added,
                        npcId, GetInteractiveNpcList()));
            }
        }

        public void RemoveNpcFromInteractiveList(int npcId)
        {
            if (_interactiveNpcList.Remove(new InteractiveNpc(npcId)))
            {
                _eventService.Emit(GameEvents.PlayerEvent.InteractiveNpcListChanged,
                    new PlayerInteractiveNpcListChangedEvent(PlayerInteractiveNpcListChangedEvent.EventType.Removed,
                        npcId, GetInteractiveNpcList()));
            }
        }

        public override void OnInit(EntityGroup group)
        {
            base.OnInit(group);
        }

        public override void OnRelease()
        {
            Addressables.ReleaseInstance(PlayerController.PlayerObject);
            Addressables.ReleaseInstance(Player);
            _eventService.Remove<NpcEvent>(GameEvents.MapEvent.NpcEvent, OnNpcEvent);
        }

        public override void OnTimeScaleChanged()
        {
        }

        public override void OnUpdate(float timeElapse, float realTimeElapse)
        {
            if (_avoidBattleTimer > 0)
            {
                _avoidBattleTimer -= Time.deltaTime;
                if (_avoidBattleTimer < 0)
                {
                    _avoidBattleTimer = 0;
                }
            }
        }

        public override async Task<IEntity> LoadEntityAsync()
        {
            var prefabAddress = ResAddress.PlayerResPrefab(PlayerInfo.Gender);
            const string playerAddress = ResAddress.PlayerPrefab;

            //加载角色资源
            var playerObj = await Addressables.InstantiateAsync(prefabAddress).Task;

            //加载Player
            var player = await Addressables.InstantiateAsync(playerAddress).Task;

            var p = player.GetComponent<PlayerController>();
            playerObj.SetActive(true);
            p.PlayerObject = playerObj;

            PlayerController = p;
            Player = p.gameObject;

            var template = playerObj.GetComponent<CharacterTemplate>();
            if (template != null)
            {
                template.ParseFromLongData(PlayerInfo.AvatarSetting);
            }

            Object.DontDestroyOnLoad(Player);
            return this;
        }
    }
}