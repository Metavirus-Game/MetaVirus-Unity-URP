using System;
using System.Linq;
using cfg.common;
using cfg.skill;
using FairyGUI;
using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.Fsm;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Battle.Scene.Fsm;
using MilkShake;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace MetaVirus.Logic.Service.Battle.Scene
{
    public class BattleCamera : MonoBehaviour
    {
        public Camera Camera => battleCamera;

        public Camera battleCamera;
        public Camera battleCameraFocusLayer;
        public Camera battleVfxCamera;

        public Shaker cameraShaker;
        public ShakePreset shakePreset;

        internal BattleCameraFadeMask FadeMask { get; private set; }

        [Header("镜头的移动速度,m/s")] public float cameraMoveSpeed = 1;
        [Header("镜头目标的移动速度,m/s")] public float cameraAimMoveSpeed = 0.3f;
        [Header("特写镜头的移动速度,m/s")] public float closeUpCameraMoveSpeed = 3;
        [Header("特写镜头的转动速度,m/s")] public float closeUpCameraRotateSpeed = 6;


        private static Camera _floatingTextCamera;

        public BattleField.CameraControlPoint cameraPoint { get; private set; }
        public bool cameraPointRear { get; private set; } = true;

        private BattleField.CameraControlPoint _aimCameraPoint;

        // private Transform _rearTransform;
        public Transform aimTransform;

        public Vector3 AimPosition => _battleService.BattleField.GetCameraAimControlPoint(_aimCameraPoint).position;

        private BattleService _battleService;
        private EventService _eventService;
        private FsmService _fsmService;

        private FsmEntity<BattleCamera> _battleCameraFsm;

        private Vector3 CameraPosBackup { get; set; }
        private Quaternion CameraRotBackup { get; set; }
        private bool HasCameraTransBackup { get; set; } = false;

        internal BattleUnitEntity CloseUpUnit { get; private set; }
        internal BattleUnitEntity[] CloseUpUnitTargets { get; private set; }

        private void Start()
        {
            _battleService = GameFramework.GetService<BattleService>();
            _eventService = GameFramework.GetService<EventService>();
            _fsmService = GameFramework.GetService<FsmService>();

            aimTransform = new GameObject("BattleCameraAimTarget").transform;

            if (FadeMask == null)
            {
                FadeMask = GetComponentInChildren<BattleCameraFadeMask>();
            }

#if UNITY_EDITOR
            var gizmos = aimTransform.AddComponent<GizmoVisualizer>();
            gizmos.DebugColor = Color.red;
            gizmos.alpha = 0.8f;
            gizmos.gizmoType = GizmoVisualizer.GizmoType.Cube;
            gizmos.debugSize = 0.4f;
#endif

            if (battleCamera == null)
            {
                battleCamera = transform.Find("BattleCamera").GetComponent<Camera>();
            }

            if (battleVfxCamera == null)
            {
                battleVfxCamera = transform.Find("BattleVfxCamera").GetComponent<Camera>();
            }

            TurnOff();
            AddCamerasToStack();
            _eventService.On<BattleEvent>(GameEvents.BattleEvent.Battle, OnBattleEvent);
            _eventService.On<BattleOnUnitAction>(GameEvents.BattleEvent.OnUnitAction, OnUnitActionEvent);
            
        }

        internal void BackupCameraPos()
        {
            HasCameraTransBackup = true;
            CameraPosBackup = transform.position;
            CameraRotBackup = transform.rotation;
        }

        internal void RestoreCameraPos()
        {
            if (HasCameraTransBackup)
            {
                HasCameraTransBackup = false;
                transform.position = CameraPosBackup;
                transform.rotation = CameraRotBackup;
            }
        }


        private void OnDestroy()
        {
            TurnOff();
            _eventService.Remove<BattleEvent>(GameEvents.BattleEvent.Battle, OnBattleEvent);
            _eventService.Remove<BattleOnUnitAction>(GameEvents.BattleEvent.OnUnitAction, OnUnitActionEvent);
            if (aimTransform != null)
                Destroy(aimTransform.gameObject);
        }

        private void OnEnable()
        {
            AddCamerasToStack();
        }

        private void OnUnitActionEvent(BattleOnUnitAction evt)
        {
            //var battleField = _battleService.BattleField;
            var battle = _battleService.CurrentBattle;
            if (evt.ActionState == BattleOnUnitAction.Action.Starting)
            {
                var skillInfo = evt.SkillCastInfo?.CastSkill;
                var targets = evt.SkillCastInfo?.TargetIds.Select(battle.GetUnitEntity).ToArray();

                _aimCameraPoint = CalcAimControlPoint(battle, evt.UnitEntity, skillInfo, targets);
                CalcCameraPosition(battle, evt.UnitEntity, skillInfo, targets);

                if (skillInfo != null && skillInfo.Skill.CameraMode == CameraMode.Closeup &&
                    evt.UnitEntity.BattleUnit.Side == BattleUnitSide.Source)
                {
                    //处于屏幕下方的单位，进入特写模式
                    CloseUpUnit = evt.UnitEntity;
                    CloseUpUnitTargets = targets;
                    _battleCameraFsm.ChangeState<BattleCameraCloseupState>();
                }
            }
            else if (evt.ActionState == BattleOnUnitAction.Action.AtkKeyFrame)
            {
                //攻击关键帧
                var skill = evt.SkillCastInfo.CastSkill.Skill;
                if (skill.CameraShakeMode == CameraShakeMode.OnSkillHit && cameraShaker != null && shakePreset != null)
                {
                    cameraShaker.Shake(shakePreset);
                }
            }
            else if (evt.ActionState == BattleOnUnitAction.Action.Hitting)
            {
                //震动屏幕
                //投射物命中的震动
                var skill = evt.SkillCastInfo.CastSkill.Skill;
                if (skill.CameraShakeMode == CameraShakeMode.OnProjectileHit && cameraShaker != null &&
                    shakePreset != null)
                {
                    cameraShaker.Shake(shakePreset);
                }
            }
        }

        private BattleUnitSide GetTargetSide(BattleUnitEntity actionUnit, SkillInfo skillInfo)
        {
            if (skillInfo.Skill.AtkTarget == AtkTarget.Friend)
            {
                return actionUnit.BattleUnit.Side;
            }

            return actionUnit.BattleUnit.Side == BattleUnitSide.Source
                ? BattleUnitSide.Target
                : BattleUnitSide.Source;
        }

        private void CalcCameraPosition(BaseBattleInstance battle, BattleUnitEntity actionUnit,
            SkillInfo skillInfo, BattleUnitEntity[] targets)
        {
            var battleField = _battleService.BattleField;

            var targetSide = GetTargetSide(actionUnit, skillInfo);

            if (targetSide == BattleUnitSide.Target)
            {
                if (actionUnit.BattleUnit.Side == targetSide)
                {
                    //攻击者和目标都是上方单位，摄像机移动到中间位置
                    cameraPointRear = false;
                    cameraPoint = BattleField.CameraControlPoint.Center;
                    //_rearTransform = battleField.GetCameraControlPoint(BattleField.CameraControlPoint.Center, false);
                }
                else
                {
                    //攻击者是下方单位
                    //只有攻击者是下方单位，目标是上方单位，技能是移动到目标位置 的情况下，才拉近镜头
                    cameraPointRear = skillInfo.Skill.MoveAction.MovePosition == MovePosition.None;
                    cameraPoint = battleField.GetCloseControlPoint(actionUnit.Transform.position, cameraPointRear);
                    //_rearTransform = battleField.GetCloseControlPoint(actionUnit.Transform.position, rear);
                }
            }
            else
            {
                //目标是下方单位

                if (actionUnit.BattleUnit.Side == targetSide)
                {
                    //攻击者和目标是同侧单位
                    cameraPointRear = true;
                    cameraPoint = battleField.GetCloseControlPoint(actionUnit.Transform.position);
                    // _rearTransform = battleField.GetCameraControlPointCenter();
                }
                else
                {
                    //攻击者是上方单位，
                    if (targets.Length == 1)
                    {
                        //目标只有一个, 移动到slot对应的控制点
                        cameraPointRear = false;
                        cameraPoint = battleField.GetSlotControlPoint(targets[0].BattleUnit.Slot);
                        //cameraPoint = battleField.GetCloseControlPoint(targets[0].Transform.position);
                        //_cameraPoint = battleField.GetDiagonalPoint(_aimCameraPoint);
                        // _rearTransform = battleField.GetCloseControlPoint(targets[0].Transform.position);
                    }
                    else
                    {
                        //目标有多个,摄像机移动到攻击者对角线位置
                        cameraPointRear = true;
                        cameraPoint = battleField.GetDiagonalPoint(_aimCameraPoint);
                        //var pos = GetTargetCenterPosition(targets);
                        //_rearTransform = battleField.GetCloseControlPoint(pos);
                    }
                }
            }
        }

        private BattleField.CameraControlPoint CalcAimControlPoint(BaseBattleInstance battle,
            BattleUnitEntity actionUnit,
            SkillInfo skillInfo, BattleUnitEntity[] targets)
        {
            var battleField = _battleService.BattleField;

            var pos = _battleService.CurrentBattle.GetFormationFrontCenter(BattleUnitSide.Target).position;

            if (targets != null)
            {
                var targetSide = GetTargetSide(actionUnit, skillInfo);

                if (targetSide == BattleUnitSide.Target)
                {
                    pos = GetTargetCenterPosition(targets);
                }
                else
                {
                    //目标为下方单位
                    if (actionUnit.BattleUnit.Side == targetSide)
                    {
                        //攻击者也是下方单位，摄像机目标聚焦在目标阵型中心位置
                        pos = _battleService.CurrentBattle.GetFormationFrontCenter(BattleUnitSide.Target).position;
                    }
                    else
                    {
                        //攻击者是上方单位，摄像机目标聚焦在进攻单位的位置
                        pos = actionUnit.Transform.position;
                    }
                }
            }

            return battleField.GetCloseControlPointAim(pos);
        }

        private Vector3 GetTargetCenterPosition(BattleUnitEntity[] targets)
        {
            var count = 0;
            var pos = Vector3.zero;
            foreach (var unit in targets)
            {
                if (unit != null)
                {
                    count++;
                    pos += unit.Transform.position;
                }
            }

            pos /= count;
            return pos;
        }

        private void OnBattleEvent(BattleEvent evt)
        {
            var battleField = _battleService.BattleField;
            var battle = _battleService.CurrentBattle;

            if (battle == null || battleField == null) return;
            var type = evt.EventType;

            cameraPointRear = true;
            if (type == BattleEvent.BattleEventType.Ready)
            {
                _aimCameraPoint = BattleField.CameraControlPoint.Center;
                aimTransform.position = AimPosition;
                cameraPoint = BattleField.CameraControlPoint.Right;
                transform.position = battleField.GetCameraControlPoint(cameraPoint).position;
                transform.forward = AimPosition - transform.position;
            }
            else if (type == BattleEvent.BattleEventType.OverviewBattleField)
            {
                //浏览战场
                cameraPoint = BattleField.CameraControlPoint.Left;
                _battleCameraFsm.ChangeState<BattleCameraFreeState>();
            }
        }


        private void OnDisable()
        {
            var stack = battleCamera.GetUniversalAdditionalCameraData().cameraStack;

            if (_floatingTextCamera != null)
            {
                _floatingTextCamera.enabled = false;
                stack.Remove(_floatingTextCamera);
            }

            stack.Remove(StageCamera.main);

            if (FadeMask != null)
            {
                FadeMask.HideImmediately();
            }
        }

        private void AddCamerasToStack()
        {
            if (Camera == null)
            {
                return;
            }

            var stack = battleCamera.GetUniversalAdditionalCameraData().cameraStack;
            if (!stack.Contains(battleCameraFocusLayer))
            {
                stack.Add(battleCameraFocusLayer);
            }

            if (!stack.Contains(battleVfxCamera))
            {
                stack.Add(battleVfxCamera);
            }

            var textCamera = GetFloatingTextCamera();
            if (!stack.Contains(textCamera))
            {
                stack.Add(textCamera);
            }

            //battle ui camera
            var stageCamera = StageCamera.main;
            if (!stack.Contains(stageCamera))
            {
                stack.Add(stageCamera);
            }
        }

        private Camera GetFloatingTextCamera()
        {
            if (_floatingTextCamera == null)
            {
                var stageCamera = StageCamera.main;
                var textCamera = Instantiate(stageCamera);
                textCamera.gameObject.name = "FloatingTextCamera";
                textCamera.cullingMask = 1 << LayerMask.NameToLayer("FloatingTextUI");
                Destroy(textCamera.GetComponent<AudioListener>());
                //textCamera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
                DontDestroyOnLoad(textCamera);
                _floatingTextCamera = textCamera;
            }

            _floatingTextCamera.enabled = true;
            return _floatingTextCamera;
        }

        public void TurnOn()
        {
            this.enabled = true;
            battleCamera.enabled = true;
            battleCameraFocusLayer.enabled = true;
            battleVfxCamera.enabled = true;
            _battleCameraFsm = _fsmService.CreateFsm("BattleCameraFsm", this, new BattleCameraFreeState()
                , new BattleCameraCloseupState(), new BattleCameraIdleState());
            
            _battleCameraFsm.Start<BattleCameraIdleState>();
        }

        public void TurnOff()
        {
            this.enabled = false;
            Camera.enabled = false;
            battleCameraFocusLayer.enabled = false;
            battleVfxCamera.enabled = false;
            
            _fsmService.DestroyFsm<BattleCamera>("BattleCameraFsm");
        }

        private void Update()
        {
        }
    }
}