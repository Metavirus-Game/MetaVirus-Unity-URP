using GameEngine;
using GameEngine.Fsm;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Service;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.FsmStates.NpcFsm
{
    public class NpcStateBase : FsmState<NpcEntity>
    {
        //敌对Npc靠近玩家后进入战斗的距离
        protected static float EnterBattleDist =>
            GameFramework.GetService<GameDataService>().CommonConfig.PlayerEnterBattleDistance;

        protected static float InteractiveDist =>
            GameFramework.GetService<GameDataService>().CommonConfig.PlayerInteractiveDistance;

        //300毫秒检测一次敌人
        private const float ScanInterval = 0.3f;

        private float _scanTimer;

        /**
         * 是否进行敌人扫描
         */
        internal bool DoScanEnemy { get; set; } = true;

        /**
         * 是否检查近距离的敌人
         */
        internal bool DoCheckEnemy { get; set; } = true;

        /**
         * 是否检测和Player的交互
         */
        internal bool DoCheckInteractive { get; set; } = false;

        /**
         * 当navMeshAgent完毕后，是否自动将面向的方向转到起始方向
         */
        internal bool AutoTurnBack { get; set; } = true;

        public override void OnInit(FsmEntity<NpcEntity> fsm)
        {
        }

        public override void OnEnter(FsmEntity<NpcEntity> fsm)
        {
        }

        public override void OnLeave(FsmEntity<NpcEntity> fsm, bool isShutdown)
        {
        }

        public override void OnDestroy(FsmEntity<NpcEntity> fsm)
        {
        }

        public virtual void OnNavMeshAgentInterrupted()
        {
        }

        public virtual void OnNavMeshAgentResume()
        {
        }

        public override void OnUpdate(FsmEntity<NpcEntity> fsm, float elapseTime, float realElapseTime)
        {
            var playerEntity = PlayerEntity.Current;

            if (DoCheckEnemy)
            {
                var relation = GetNpcRelationWithPlayer(playerEntity, fsm.Owner);

                if (relation == NpcRelation.OpposedPassive && !playerEntity.AvoidBattle)
                {
                    //敌对被动的怪物，检查玩家是否进入战斗距离
                    var dist = Vector3.Distance(playerEntity.Position, fsm.Owner.NpcResObject.transform.position);
                    if (dist <= EnterBattleDist)
                    {
                        fsm.Owner.ChaseTarget = playerEntity.Player;
                        ChangeState<NpcStateToBattle>(fsm);
                        return;
                    }
                }

                if (fsm.Owner.ChaseTarget != null)
                {
                    var dist = Vector3.Distance(fsm.Owner.ChaseTarget.transform.position,
                        fsm.Owner.NpcResObject.transform.position);
                    if (dist <= EnterBattleDist && !playerEntity.AvoidBattle)
                    {
                        ChangeState<NpcStateToBattle>(fsm);
                        return;
                    }
                }
            }

            if (DoCheckInteractive)
            {
                var playerPos = playerEntity.Position;
                var dist = Vector3.Distance(playerPos, fsm.Owner.Position);
                if (dist <= InteractiveDist && fsm.Owner.HasInteraction)
                {
                    playerEntity.AddNpcToInteractiveList(fsm.Owner.Id);
                }
                else
                {
                    playerEntity.RemoveNpcFromInteractiveList(fsm.Owner.Id);
                }

                //stop walking and face to player
                var nav = fsm.Owner.NavMeshAgent;

                if (nav.hasPath)
                {
                    if (nav.isStopped != fsm.Owner.IsInteractingWithPlayer)
                    {
                        nav.isStopped = fsm.Owner.IsInteractingWithPlayer;
                        if (nav.isStopped)
                        {
                            OnNavMeshAgentInterrupted();
                        }
                        else
                        {
                            OnNavMeshAgentResume();
                        }
                    }
                }


                if (fsm.Owner.IsInteractingWithPlayer)
                {
                    var tarDir = playerEntity.Position - fsm.Owner.Position;
                    tarDir.y = 0;
                    tarDir.Normalize();

                    fsm.Owner.NpcResObject.transform.forward =
                        Vector3.MoveTowards(fsm.Owner.NpcResObject.transform.forward, tarDir, 5 * elapseTime);
                }
                else
                {
                    if (!nav.hasPath && AutoTurnBack)
                    {
                        fsm.Owner.NpcResObject.transform.forward = Vector3.MoveTowards(
                            fsm.Owner.NpcResObject.transform.forward, fsm.Owner.MapNpc.Direction,
                            1 * elapseTime);

                        // fsm.Owner.NpcResObject.transform.eulerAngles =
                        //     Vector3.MoveTowards(fsm.Owner.NpcResObject.transform.eulerAngles, fsm.Owner.MapNpc.Rotation,
                        //         120 * elapseTime);
                    }
                }
            }


            if (!DoScanEnemy) return;
            if (_scanTimer == 0)
            {
                var target = fsm.Owner.ScanEnemy();
                if (target != null)
                {
                    fsm.Owner.ChaseTarget = target;
                    ChangeState<NpcStateChase>(fsm);
                }
            }


            _scanTimer += Time.deltaTime;
            if (_scanTimer > ScanInterval)
            {
                _scanTimer = 0;
            }
        }
    }
}