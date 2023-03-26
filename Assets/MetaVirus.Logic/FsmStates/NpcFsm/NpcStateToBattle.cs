using GameEngine.Fsm;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Service;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.FsmStates.NpcFsm
{
    public class NpcStateToBattle : NpcStateBase
    {
        public override void OnInit(FsmEntity<NpcEntity> fsm)
        {
            DoScanEnemy = false;
            DoCheckEnemy = false;
        }

        public override void OnEnter(FsmEntity<NpcEntity> fsm)
        {
            var player = fsm.Owner.ChaseTarget;
            var monster = fsm.Owner.NpcResObject;

            var playerDir = player.transform.forward;
            var monsterDir = monster.transform.position - player.transform.position;
            monsterDir.y = 0;
            monsterDir.Normalize();

            var a = Mathf.Acos(Vector3.Dot(playerDir, monsterDir)) * Mathf.Rad2Deg;

            if (a < 110)
            {
                //Npc被正面进攻
                fsm.Owner.Animator.SetTrigger(AniParamName.TriggerTakeDamage);
            }
            else
            {
                //Npc背后偷袭
                fsm.Owner.Animator.SetTrigger(AniParamName.TriggerAttack);
            }

            BattleService.EnterBattle(fsm.Owner.Id, fsm.Owner.Info);
        }

        public override void OnLeave(FsmEntity<NpcEntity> fsm, bool isShutdown)
        {
            fsm.Owner.Animator.SetInteger(AniParamName.State, 0);
        }

        public override void OnDestroy(FsmEntity<NpcEntity> fsm)
        {
        }

        public override void OnUpdate(FsmEntity<NpcEntity> fsm, float elapseTime, float realElapseTime)
        {
            base.OnUpdate(fsm, elapseTime, realElapseTime);
        }
    }
}