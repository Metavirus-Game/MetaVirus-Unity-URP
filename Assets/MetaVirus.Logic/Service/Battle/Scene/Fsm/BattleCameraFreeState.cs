using GameEngine.Fsm;
using UnityEngine;
using static GameEngine.GameFramework;

namespace MetaVirus.Logic.Service.Battle.Scene.Fsm
{
    public class BattleCameraFreeState : FsmState<BattleCamera>
    {
        private BattleService _battleService;

        public override void OnInit(FsmEntity<BattleCamera> fsm)
        {
            _battleService = GetService<BattleService>();
        }

        public override void OnEnter(FsmEntity<BattleCamera> fsm)
        {
            var owner = fsm.Owner;
            owner.RestoreCameraPos();
        }

        public override void OnLeave(FsmEntity<BattleCamera> fsm, bool isShutdown)
        {
            fsm.Owner.BackupCameraPos();
        }

        public override void OnUpdate(FsmEntity<BattleCamera> fsm, float elapseTime, float realElapseTime)
        {
            var owner = fsm.Owner;

            var battleField = _battleService.BattleField;
            var battle = _battleService.CurrentBattle;

            var deltaTime = elapseTime * _battleService.TimeScale;

            if (battle == null || battleField == null || !battle.Started)
            {
                return;
            }

            var aimPos = owner.AimPosition;

            if (owner.aimTransform != null && aimPos != Vector3.zero && aimPos != owner.aimTransform.position)
            {
                owner.aimTransform.position =
                    Vector3.MoveTowards(owner.aimTransform.position, owner.AimPosition,
                        deltaTime * owner.cameraAimMoveSpeed);
            }

            var myTrans = owner.transform;

            var cameraPosition = battleField.GetCameraControlPoint(owner.cameraPoint, owner.cameraPointRear).position;

            Debug.DrawLine(myTrans.position, cameraPosition, Color.red);

            if (cameraPosition != myTrans.position)
            {
                myTrans.position = Vector3.MoveTowards(myTrans.position, cameraPosition,
                    deltaTime * owner.cameraMoveSpeed);

                var dir = owner.aimTransform.position - myTrans.position;
                dir.y = 0;
                dir.Normalize();

                // var forward = myTrans.forward;
                // forward.y = 0;
                // forward.Normalize();
                // var maxDelta = deltaTime * cameraRotateSpeed * Mathf.Deg2Rad;

                //forward = Vector3.RotateTowards(forward, dir, maxDelta, maxDelta);
                var rot = Quaternion.LookRotation(dir).eulerAngles;

                var euler = myTrans.eulerAngles;
                euler.y = rot.y;
                myTrans.eulerAngles = euler;
            }
        }
    }
}