using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.Fsm;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Scene.Fsm
{
    public class BattleCameraCloseupState : FsmState<BattleCamera>
    {
        private BattleService _battleService;
        private EventService _eventService;

        private CloseUpCameraAnchor.BattleUnitAnchor _closeUpAnchor;

        public override void OnInit(FsmEntity<BattleCamera> fsm)
        {
            _battleService = GameFramework.GetService<BattleService>();
            _eventService = GameFramework.GetService<EventService>();
        }

        public override void OnEnter(FsmEntity<BattleCamera> fsm)
        {
            var owner = fsm.Owner;
            owner.FadeMask.FadeTo(0.8f);
            var field = _battleService.BattleField;
            var closeUpUnit = owner.CloseUpUnit;

            var layer = LayerMask.NameToLayer("Battle_FocusUnit");
            closeUpUnit.GameObject.SetLayerAll(layer);

            foreach (var target in owner.CloseUpUnitTargets)
            {
                target?.GameObject.SetLayerAll(layer);
            }

            _closeUpAnchor = field.closeUpCameraAnchor.GetUnitCloseUpCameraPos(closeUpUnit);
            _eventService.On<BattleOnUnitAction>(GameEvents.BattleEvent.OnUnitAction, OnUnitActionEvent);

            owner.transform.position = _closeUpAnchor.position;
            owner.transform.rotation = _closeUpAnchor.rotation;


            //test
            // var t = fsm.Owner.CloseUpUnit;
            // owner.transform.position = field.cameraCloseUpRearPoint.position;
            // owner.transform.forward = field.GetCameraAimControlPoint(BattleField.CameraControlPoint.Center).position - owner.transform.position;
        }

        public override void OnLeave(FsmEntity<BattleCamera> fsm, bool isShutdown)
        {
            _eventService.Remove<BattleOnUnitAction>(GameEvents.BattleEvent.OnUnitAction, OnUnitActionEvent);
        }

        private void OnUnitActionEvent(BattleOnUnitAction evt)
        {
            if (evt.ActionState == BattleOnUnitAction.Action.Backing)
            {
                var owner = Fsm.Owner;
                owner.FadeMask.Hide();

                var closeUpUnit = owner.CloseUpUnit;
                var layer = LayerMask.NameToLayer("Default");
                closeUpUnit.GameObject.SetLayerAll(layer);
                foreach (var target in owner.CloseUpUnitTargets)
                {
                    target?.GameObject.SetLayerAll(layer);
                }

                ChangeState<BattleCameraFreeState>(Fsm);
            }
        }

        public override void OnUpdate(FsmEntity<BattleCamera> fsm, float elapseTime, float realElapseTime)
        {
            var field = _battleService.BattleField;
            var owner = fsm.Owner;
            var target = fsm.Owner.CloseUpUnit;

            var aimPos = target.Transform.position + Vector3.up * field.closeUpCameraAnchor.baseHeight;
            var dir = aimPos - owner.transform.position;

            Debug.DrawLine(owner.transform.position, aimPos, Color.red);


            var deltaTime = elapseTime * _battleService.TimeScale;

            owner.transform.forward =
                Vector3.MoveTowards(owner.transform.forward, dir, owner.closeUpCameraRotateSpeed * deltaTime);


            owner.transform.position = Vector3.MoveTowards(owner.transform.position,
                field.cameraCloseUpRearPoint.position, owner.closeUpCameraMoveSpeed * deltaTime);
        }
    }
}