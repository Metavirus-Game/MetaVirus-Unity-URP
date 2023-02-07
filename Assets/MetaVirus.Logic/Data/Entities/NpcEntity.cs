using System;
using System.Threading.Tasks;
using cfg.common;
using cfg.map;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.Entity;
using GameEngine.Event;
using GameEngine.Fsm;
using MetaVirus.Logic.Data.Events;
using MetaVirus.Logic.Data.Events.Player;
using MetaVirus.Logic.Data.Npc;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.FsmStates.NpcFsm;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using static MetaVirus.Logic.Data.GameEvents;
using static MetaVirus.Logic.Data.Constants;
using Object = UnityEngine.Object;

namespace MetaVirus.Logic.Data.Entities
{
    public class NpcEntity : BaseEntity
    {
        private readonly FsmEntity<NpcEntity> _fsm;

        private readonly EntityService _entityService;
        private DataNodeService _dataNodeService;
        private readonly EventService _eventService;

        public sealed override int Id => MapNpc.Id;

        public MapNpc MapNpc { get; }
        public NpcRefreshInfo Info { get; }
        public int MapId { get; }

        private GameObject _npcResObj;
        public GameObject NpcResObject => _npcResObj;
        public GameObject NpcHUDPos { get; private set; }

        private NavMeshAgent _navMeshAgent;
        private Animator _animator;
        private static readonly int IsBorn = Animator.StringToHash("IsBorn");

        public GameObject ChaseTarget { get; set; }
        public NavMeshAgent NavMeshAgent => _navMeshAgent;
        public Animator Animator => _animator;

        public float WalkSpeed => Info.NpcTempId_Ref.Speed / 2;
        public float RunSpeed => Info.NpcTempId_Ref.Speed;

        public Vector3 Position => _npcResObj.transform.position;
        public Vector3 Rotation => _npcResObj.transform.eulerAngles;

        public bool HasFunction => (MapNpc.State & MapNpcState.Function) != 0;
        public bool HasTask => (MapNpc.State & MapNpcState.Task) != 0;

        /// <summary>
        /// 返回Npc是否有交互功能
        /// </summary>
        public bool HasInteraction => HasFunction || HasTask;

        /// <summary>
        /// Npc是否正在和玩家交互
        /// </summary>
        public bool IsInteractingWithPlayer { get; private set; } = false;


        public NpcEntity(MapNpc info, int mapId)
        {
            _entityService = GameFramework.GetService<EntityService>();
            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _eventService = GameFramework.GetService<EventService>();

            MapNpc = info;
            Info = MapNpc.RefreshInfo;

            MapId = mapId;
            _fsm = GameFramework.GetService<FsmService>().CreateFsm("npc-fsm-" + Id, this,
                new NpcStateIdle(),
                new NpcStateRandomWalk(),
                new NpcStatePartol(),
                new NpcStateChase(),
                new NpcStateGoBack(),
                new NpcStateToBattle()
            );
        }

        internal Type GetNormalState()
        {
            Type t = null;

            switch (Info.Behaviour)
            {
                case NpcBehaviour.Partol:
                    t = typeof(NpcStatePartol);
                    break;
                case NpcBehaviour.Random:
                    t = typeof(NpcStateRandomWalk);
                    break;
                case NpcBehaviour.Guard:
                default:
                    t = typeof(NpcStateIdle);
                    break;
            }

            return t;
        }

        public override void OnInit(EntityGroup group)
        {
            base.OnInit(group);
        }

        public override async Task<IEntity> LoadEntityAsync()
        {
            var npcRes = Info.NpcTempId_Ref;
            var npcResAddress = ResAddress.NpcRes(npcRes.ResDataId);

            Debug.Log($"Loading Npc From Address : {npcResAddress}");

            _npcResObj = await Addressables.InstantiateAsync(npcResAddress).Task;
            _npcResObj.name = $"MapNpc[{MapNpc.Id:X}]";

            Object.DontDestroyOnLoad(_npcResObj);
            _npcResObj.transform.position = MapNpc.Position;
            _npcResObj.transform.eulerAngles = MapNpc.Rotation;
            var scale = Info.NpcTempId_Ref.Scale;
            _npcResObj.transform.localScale = new Vector3(scale, scale, scale);

            _animator = _npcResObj.GetComponentInChildren<Animator>();
            _navMeshAgent = _npcResObj.GetComponentInChildren<NavMeshAgent>();

            if (_navMeshAgent != null)
            {
                _navMeshAgent.speed = Info.NpcTempId_Ref.Speed;
                _navMeshAgent.radius = 0.1f;
                _navMeshAgent.enabled = false;
            }

            _npcResObj.SetActive(true);
            _animator.SetBool(IsBorn, true);

            var state = GetNormalState();
            _fsm.Start(state);

            var trans = _npcResObj.transform.Find("HUDPos");

            if (trans == null)
            {
                var comps = _npcResObj.GetComponentsInChildren<Transform>();
                foreach (var comp in comps)
                {
                    if (comp.name == "HUDPos")
                    {
                        trans = comp;
                        break;
                    }
                }
            }

            if (trans != null)
            {
                NpcHUDPos = trans.gameObject;
            }
            else
            {
                NpcHUDPos = new GameObject("HUDPos");
                NpcHUDPos.transform.SetParent(_npcResObj.transform, false);
                NpcHUDPos.transform.localPosition = new Vector3(0, _navMeshAgent.height, 0);

                Debug.LogError($"Npc {Info.NpcTempId_Ref.ResDataId}: [HUDPos] node is missing");
            }

            _eventService.On<PlayerInteractingWithNpcEvent>(PlayerEvent.InteractingWithNpc, OnPlayerInteract);

            _eventService.Emit(MapEvent.NpcEvent,
                new NpcEvent(NpcEvent.NpcEventType.Spawn, Id, MapId));

            await Task.Delay(TimeSpan.FromMilliseconds(Time.deltaTime));
            _navMeshAgent.enabled = true;

            return this;
        }

        private void OnPlayerInteract(PlayerInteractingWithNpcEvent evt)
        {
            //interacting with player, face to player
            if (evt.NpcEntity.Id == Id)
            {
                IsInteractingWithPlayer = evt.Type == PlayerInteractingWithNpcEvent.EventType.Start;
            }
        }

        public GameObject ScanEnemy()
        {
            if (_npcResObj == null || Info.AttackMode == NpcAttackMode.Passive) return null;
            //var playerInfo = _dataNodeService.GetData<PlayerInfo>(DataKeys.PlayerInfo);
            var playerEntity = PlayerEntity.Current;
            if (playerEntity.AvoidBattle) return null;

            var relation = GetNpcRelationWithPlayer(playerEntity, this);

            if (relation == NpcRelation.OpposedPositive)
            {
                //敌对主动攻击的怪物
                if (Info.ScanRadius > 0)
                {
                    var dist = Vector3.Distance(playerEntity.Position, _npcResObj.transform.position);
                    if (dist <= Info.ScanRadius)
                    {
                        return playerEntity.PlayerObject;
                    }
                }
            }

            return null;
        }

        public override void OnUpdate(float timeElapse, float realTimeElapse)
        {
            if (_npcResObj == null) return;
        }

        public override void OnRelease()
        {
            if (_npcResObj != null)
            {
                Addressables.ReleaseInstance(_npcResObj);
            }

            GameFramework.GetService<FsmService>().DestroyFsm<NpcEntity>(_fsm.Name);

            _eventService.Emit(MapEvent.NpcEvent,
                new NpcEvent(NpcEvent.NpcEventType.Destroy, Id, MapId));

            _eventService.Remove<PlayerInteractingWithNpcEvent>(PlayerEvent.InteractingWithNpc, OnPlayerInteract);
        }

        public override void OnTimeScaleChanged()
        {
        }
    }
}