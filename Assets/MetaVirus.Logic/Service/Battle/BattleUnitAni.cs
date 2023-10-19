using System;
using System.Collections;
using cfg.common;
using cfg.skill;
using GameEngine;
using GameEngine.Utils;
using GameEngine.Utils.StateMachineBehavior;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Battle
{
    /// <summary>
    /// 绑定在战斗单位的prefab上
    /// 用于接收动画事件
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class BattleUnitAni : MonoBehaviour, IAnimatorListener
    {
        private Animator _animator;
        public BaseBattleInstance BattleInstance { get; internal set; }

        public BattleUnit BattleUnit { get; set; }

        private Vector3 _originPos;
        private Vector3 _originForward;
        private Quaternion _originRot;

        private BattleVfxGameService _vfxGameService;
        private GameDataService _gameDataService;

        private FollowGhostEffect _ghostEffect;

        [SerializeField] [Header("头部节点的名称")] private string headNodeName = "RigHead";

        [SerializeField] [Header("受击特效绑定位置")] private Transform hitVfxPos;
        [SerializeField] [Header("被攻击位绑定位置")] private Transform beatPos;
        [SerializeField] [Header("投射物生成位")] private Transform projectileSpawnPos;
        [SerializeField] [Header("投射物顶部生成位置")] private Transform headupSpawnPos;
        [SerializeField] [Header("头顶绑定位置")] private Transform topPos;

        [Header("奔跑速度")] public float runSpeed = 12f;
        [Header("行走速度")] public float walkSpeed = 3f;
        [Header("后退速度")] public float backwardSpeed = 12f; 
        [Header("近战攻击距离")] public float meleeDistance = 1.5f;

        public UnityAction<string> OnAniEvent;

        private bool _isMoving = false;

        private SkinnedMeshRenderer _renderer;


        private SkinnedMeshRenderer Renderer
        {
            get
            {
#if UNITY_EDITOR
                _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
#endif
                return _renderer;
            }
        }

        private Transform Pelvis => Renderer == null ? transform : Renderer.rootBone;

        private Transform _rigHead;

        private Transform RigHead
        {
            get
            {
                if (_rigHead == null)
                {
                    var rigs = transform.GetComponentsInChildren<Transform>();
                    foreach (var rig in rigs)
                    {
                        if (rig.name == headNodeName)
                        {
                            _rigHead = rig;
                            break;
                        }
                    }

                    if (_rigHead == null)
                    {
                        _rigHead = transform;
                    }
                }

                return _rigHead;
            }
        }

        /// <summary>
        /// 动画事件接收函数，触发位置在需要触发事件的动画中配置
        /// </summary>
        /// <param name="evtName"></param>
        public void OnBattleAniEvent(string evtName)
        {
            OnAniEvent?.Invoke(evtName);
        }

        public void OnAnimatorBehaviourMessage(string message, object value)
        {
            OnAniEvent?.Invoke(message);
        }

        private Transform HitVfxPos
        {
            get
            {
                if (hitVfxPos == null)
                {
                    hitVfxPos = transform.Find("HitVfxPos");
                    if (hitVfxPos == null)
                    {
                        hitVfxPos = transform;
                    }
                }

                return hitVfxPos;
            }
        }

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponentInChildren<SkinnedMeshRenderer>();

            _ghostEffect = gameObject.AddComponent<FollowGhostEffect>();
            _ghostEffect.interval = 0.05f;
            _ghostEffect.aliveTime = 0.3f;
            _ghostEffect.alpha = 0.8f;

            _vfxGameService = GameFramework.GetService<BattleVfxGameService>();
            _gameDataService = GameFramework.GetService<GameDataService>();

        }

        public void OnActive()
        {
            var trans = transform;
            _originPos = trans.position;
            _originRot = trans.rotation;
            _originForward = trans.forward;
        }

        // private void OnDrawGizmos()
        // {
        //     var r = GetComponentInChildren<SkinnedMeshRenderer>();
        //     var bounds = r.bounds;
        //
        //     var center = bounds.center;
        //     var size = bounds.size;
        //     var scale = transform.localScale;
        //     // size = new Vector3(size.x * scale.x, size.y * scale.y, size.z * scale.z);
        //
        //     var m = Gizmos.matrix;
        //
        //     Gizmos.matrix = r.localToWorldMatrix;
        //
        //     Gizmos.DrawWireCube(center, size);
        //     Gizmos.matrix = m;
        //
        //     var ext = bounds.extents;
        //     var forward = transform.forward;
        //     forward = r.transform.InverseTransformDirection(forward);
        //     var down = -transform.up;
        //     down = r.transform.InverseTransformDirection(down);
        //     down.Scale(ext);
        //     ext.Scale(forward);
        //     center += down + ext;
        //
        //     var pos = r.transform.TransformPoint(center);
        //     pos.y = transform.position.y;
        //     Gizmos.DrawSphere(pos, 0.1f);
        //
        //     Gizmos.DrawWireSphere(GetBeatPosition(), 0.1f);
        // }

        public Transform GetBeatTransform()
        {
            if (beatPos == null)
            {
                if (Renderer == null)
                {
                    beatPos = transform;
                    return beatPos;
                }

                var bounds = _renderer.sharedMesh.bounds;
                var center = bounds.center;
                var ext = bounds.extents;
                var forward = transform.forward;
                forward = _renderer.transform.InverseTransformDirection(forward);
                var down = -transform.up;
                down = _renderer.transform.InverseTransformDirection(down);
                down.Scale(ext);
                ext.Scale(forward);
                center += down + ext;

                var pos = _renderer.transform.TransformPoint(center);
                var t = transform;
                pos.y = t.position.y;

                var go = new GameObject("BeatPos");
                go.transform.SetParent(t);
                beatPos = go.transform;
                beatPos.position = pos;
                beatPos.forward = transform.forward;
            }

            return beatPos;
        }

        private Transform GetTopTransform()
        {
            if (topPos == null)
            {
                if (Renderer == null)
                {
                    topPos = transform;
                    return topPos;
                }

                var bounds = _renderer.sharedMesh.bounds;
                var center = bounds.center;
                var ext = bounds.extents;
                var up = transform.up;
                up = _renderer.transform.InverseTransformDirection(up);
                up.Scale(ext);

                center += up;

                var pos = _renderer.transform.TransformPoint(center);

                var posObj = new GameObject("TopPos");
                posObj.transform.SetParent(RigHead, false);
                posObj.transform.position = pos;
                posObj.transform.forward = transform.up;

                topPos = posObj.transform;
            }

            return topPos;
        }

        private Transform GetHeadUpSpawnTransform()
        {
            if (headupSpawnPos == null)
            {
                var pos = new Vector3(0, _gameDataService.CommonConfig.BattleHeadupDistance, 0);
                var posObj = new GameObject("HeadUpSpawnPos");
                posObj.transform.SetParent(transform, false);
                posObj.transform.localPosition = pos;
                posObj.transform.forward = transform.forward;

                headupSpawnPos = posObj.transform;
            }

            return headupSpawnPos;
        }

        private Transform GetProjectileSpawnTransform()
        {
            if (projectileSpawnPos == null)
            {
                if (Renderer == null)
                {
                    projectileSpawnPos = transform;
                    return projectileSpawnPos;
                }

                var bounds = _renderer.sharedMesh.bounds;
                var center = bounds.center;
                var ext = bounds.extents;
                var forward = transform.forward;
                forward = _renderer.transform.InverseTransformDirection(forward);
                ext.Scale(forward);
                center += ext;

                var pos = _renderer.transform.TransformPoint(center);
                var t = transform;

                var go = new GameObject("ProjectileSpawnPos");
                go.transform.SetParent(Pelvis);
                projectileSpawnPos = go.transform;
                projectileSpawnPos.position = pos;
                projectileSpawnPos.forward = transform.forward;
            }

            return projectileSpawnPos;
        }

        public Vector3 GetBeatPosition()
        {
            return GetBeatTransform().position;
        }

        public Transform GetVfxBindPos(int vfxId)
        {
            var vfxData = _gameDataService.GetVfxData(vfxId);
            return vfxData == null ? transform : GetVfxBindPos(vfxData.BindPos);
        }

        public Transform GetVfxBindPos(VfxBindPos bindPos)
        {
            return bindPos switch
            {
                VfxBindPos.HitPos => HitVfxPos,
                VfxBindPos.Projectile => GetProjectileSpawnTransform(),
                VfxBindPos.HeadUp => GetHeadUpSpawnTransform(),
                VfxBindPos.FaceBottom => GetBeatTransform(),
                VfxBindPos.TopCenter => GetTopTransform(),
                _ => transform
            };
        }

        private IEnumerator MoveToTarget(Vector3 tarPos)
        {
            var toDir = (tarPos - transform.position).normalized;
            var atkPos = tarPos + (-toDir * meleeDistance);
            yield return MoveTo(atkPos, toDir, NpcAniState.Run, runSpeed);
        }

        public IEnumerator BackToOrigin()
        {
            yield return MoveTo(_originPos, _originForward, NpcAniState.WalkBackward, backwardSpeed);
        }

        private IEnumerator MoveTo(Vector3 tarPos, Vector3 toTar, int aniState, float moveSpeed)
        {
            if (_isMoving)
            {
                //当前正在移动，等待停止移动
                yield return new WaitUntil(() => !_isMoving);
            }

            //当前不是idle状态或者正在进行状态转换，等待
            yield return WaitUntilIdle();

            //move to pos
            _animator.SetInteger(AniParamName.State, aniState);
            _isMoving = true;
            while (transform.position != tarPos && !BattleUnit.IsDead)
            {
                transform.forward = Vector3.MoveTowards(transform.forward, toTar, BattleInstance.DeltaTime);
                transform.position =
                    Vector3.MoveTowards(transform.position, tarPos, moveSpeed * BattleInstance.DeltaTime);
                yield return null;
            }

            _animator.SetInteger(AniParamName.State, NpcAniState.Idle);
            _isMoving = false;
        }

        public bool CurrentAniIsInTransition()
        {
            return _animator.IsInTransition(0);
        }

        public bool CurrentAniStateIsName(string stateName)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(stateName);
        }

        public void TakeDamage()
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName(AniStateName.TakeDamage))
            {
                _animator.SetTrigger(AniParamName.TriggerTakeDamage);
            }
        }

        public IEnumerator DoDodgeAttack()
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName(AniStateName.Idle))
            {
                yield break;
            }

            _ghostEffect.active = true;

            var moveDistance = 1;
            var moveDuration = 0.15f;
            var dir = transform.forward;
            var oriPos = transform.position;
            var pos = oriPos + (-dir * moveDistance);

            while (transform.position != pos)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, pos,
                        moveDistance / moveDuration * BattleInstance.DeltaTime);
                yield return null;
            }

            pos = oriPos;
            yield return new WaitForSeconds(moveDuration);


            while (transform.position != pos)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, pos,
                        moveDistance / moveDuration * BattleInstance.DeltaTime);
                yield return null;
            }

            _ghostEffect.active = false;
        }

        public IEnumerator DoReliveAction()
        {
            yield return null;
            _animator.SetTrigger(AniParamName.TriggerResurrect);
        }

        public IEnumerator DoDeadAction()
        {
            //等待回到idle动作
            yield return new WaitUntil(() =>
                _animator.GetInteger(AniParamName.State) == NpcAniState.Idle && !_animator.IsInTransition(0));
            _animator.SetTrigger(AniParamName.TriggerDead);
        }

        private IEnumerator WaitUntilIdle()
        {
            yield return new WaitUntil(() =>
            {
                var state = _animator.GetCurrentAnimatorStateInfo(0);
                return state.IsName(AniStateName.Idle) && !_animator.IsInTransition(0);
            });
        }

        public IEnumerator DoSkillStartAction(SkillInfo skillInfo)
        {
            //播放技能起始动作 
            var startAction = skillInfo.Skill.StartAction;
            if (startAction.ActionName == UnitAnimationNames.None)
            {
                yield break;
            }

            //等待回到idle动作
            yield return WaitUntilIdle();

            var startVfx = startAction.AttachVfx;
            var hasVfx = _vfxGameService.IsVfxLoaded(startVfx);

            GameObject vfxObj = null;

            if (startAction.ActionName == UnitAnimationNames.Idle)
            {
                //idle动作作为起始动作，只需等待指定时间即可
                //绑定起始动作光效

                if (hasVfx)
                {
                    var bindPos = GetVfxBindPos(startVfx);
                    vfxObj = _vfxGameService.InstanceVfx(startVfx, bindPos.gameObject, false, false);
                }

                var waitTime = startAction.ActionDuration <= 0 ? 1 : startAction.ActionDuration;
                yield return new WaitForSeconds(waitTime * BattleInstance.TimeScale);
                //解除绑定

                _vfxGameService.ReleaseVfxInst(vfxObj);

                yield break;
            }

            _animator.SetInteger(AniParamName.State, NpcAniState.StartAction);
            _animator.SetInteger(AniParamName.StartAction,
                AniParamName.UnitActionNameToParamValue(startAction.ActionName));

            var enterStartAction = false;

            void OnAniEvt(string evtName)
            {
                if (evtName == BattleAniEvtNames.EnterStartAction.ToString("F"))
                {
                    //绑定起始动作光效
                    enterStartAction = true;
                    if (hasVfx)
                    {
                        var bindPos = GetVfxBindPos(startVfx);
                        vfxObj = _vfxGameService.InstanceVfx(startVfx, bindPos.gameObject, false, false);
                    }
                }
                else if (evtName == BattleAniEvtNames.ExitStartAction.ToString("F"))
                {
                    //解除光效绑定
                    OnAniEvent -= OnAniEvt;
                    _vfxGameService.ReleaseVfxInst(vfxObj);
                }
            }

            OnAniEvent += OnAniEvt;

            yield return new WaitUntil(() => enterStartAction && !_animator.IsInTransition(0));
            yield return _animator.WaitForCurrentAni(startAction.ActionDuration * BattleInstance.TimeScale);
            _animator.SetInteger(AniParamName.State, NpcAniState.Idle);

            yield return null;
        }

        public IEnumerator DoSkillMovement(SkillInfo skillInfo, Vector3 tarPos)
        {
            //等待回到idle动作
            yield return WaitUntilIdle();

            var moveAction = skillInfo.Skill.MoveAction;
            //绑定特效
            var moveVfx = moveAction.AttachVfx;
            var hasVfx = _vfxGameService.IsVfxLoaded(moveVfx);
            GameObject vfxObj = null;
            if (hasVfx)
            {
                var bindPos = GetVfxBindPos(moveVfx);
                vfxObj = _vfxGameService.InstanceVfx(moveVfx, bindPos.gameObject, false, false);
            }

            yield return MoveToTarget(tarPos);

            _vfxGameService.ReleaseVfxInst(vfxObj);
        }

        public IEnumerator DoSkillMovement(SkillInfo skillInfo, BattleUnitEntity target)
        {
            yield return DoSkillMovement(skillInfo, target.UnitAni.GetBeatPosition());
        }

        public IEnumerator DoSkillCast(SkillInfo skillInfo, UnityAction<BattleSkillCastEvents> keyFrameCallback = null)
        {
            //播放技能的施放
            var castAction = skillInfo.Skill.CastAction;

            var actionName = castAction.ActionName;

            if (actionName == UnitAnimationNames.None)
            {
                //没有定义施放技能的动作，根据技能自身的属性执行默认动作
                if (skillInfo.Skill.AtkAttribute == AtkAttribute.Physical)
                {
                    //物理技能，播放近战攻击动作
                    actionName = UnitAnimationNames.MeleeAttack1;
                }
                else
                {
                    //魔法技能，播放法术攻击动作
                    actionName = UnitAnimationNames.CastSpell1;
                }
            }

            var castVfx = castAction.AttachVfx;

            //如果施放动作中配置了投射物类型，则按照远程攻击逻辑进行攻击
            var spawn = castAction.Projectile != 0;
            yield return DoSkillCast(actionName, castVfx, keyFrameCallback, spawn);
        }

        public IEnumerator DoSkillCast(UnitAnimationNames actionName, int castVfx,
            UnityAction<BattleSkillCastEvents> keyFrameCallback = null, bool spawnProjectileOnHit = false)
        {
            var actionParamValue = AniParamName.UnitActionNameToParamValue(actionName);
            var aniState = AniParamName.UnitActionNameToStateValue(actionName);
            var actionParam = AniParamName.UnitActionNameToParamHash(actionName);
            var aniEvtNames = AniParamName.GetAniEventsByUnitActionName(actionName);

            _animator.SetInteger(AniParamName.State, aniState);
            if (actionParam != 0)
            {
                _animator.SetInteger(actionParam, actionParamValue);
            }

            var isAtkFinished = false;
            var isExitCast = false;

            void OnAniEvt(string evtName)
            {
                if (evtName == aniEvtNames[(int)BattleSkillCastEvents.HitTarget].ToString("F"))
                {
                    if (actionName != UnitAnimationNames.ProjectileAttack)
                    {
                        //攻击动画关键帧，远程攻击的攻击关键帧不在这里触发
                        //远程攻击关键帧详见BattleProjectileObject

                        //攻击关键帧默认触发 HitTarget 事件
                        //如果指定在攻击关键帧生成投射物，则触发 SpawnProjectile事件，攻击流程按照远程攻击进行
                        keyFrameCallback?.Invoke(spawnProjectileOnHit
                            ? BattleSkillCastEvents.SpawnProjectile
                            : BattleSkillCastEvents.HitTarget);
                    }
                }
                else if (evtName == aniEvtNames[(int)BattleSkillCastEvents.EnterSkillCast].ToString("F"))
                {
                    keyFrameCallback?.Invoke(BattleSkillCastEvents.EnterSkillCast);
                    //一旦进入攻击动作，设置state为idle，防止多次播放攻击动画
                    _animator.SetInteger(AniParamName.State, NpcAniState.Idle);
                }
                else if (evtName == aniEvtNames[(int)BattleSkillCastEvents.SpawnProjectile].ToString("F"))
                {
                    keyFrameCallback?.Invoke(BattleSkillCastEvents.SpawnProjectile);
                }
                else if (evtName == aniEvtNames[(int)BattleSkillCastEvents.ExitSkillCast].ToString("F"))
                {
                    keyFrameCallback?.Invoke(BattleSkillCastEvents.ExitSkillCast);
                    isExitCast = true;
                    if (actionName != UnitAnimationNames.ProjectileAttack && !spawnProjectileOnHit)
                    {
                        //非远程攻击，技能动画施放结束后，技能完成
                        isAtkFinished = true;
                    }
                }
                else if (evtName == BattleSkillCastEvents.ExitProjectile.ToString("F"))
                {
                    if (actionName == UnitAnimationNames.ProjectileAttack || spawnProjectileOnHit)
                    {
                        //远程攻击，收到ExitProjectile事件后，技能完成
                        //事件发送详见BattleProjectileObject
                        isAtkFinished = true;
                    }
                }
            }

            OnAniEvent += OnAniEvt;

            //附加伴随特效
            var hasVfx = _vfxGameService.IsVfxLoaded(castVfx);
            GameObject vfxObj = null;

            if (hasVfx)
            {
                var bindPos = GetVfxBindPos(castVfx);
                vfxObj = _vfxGameService.InstanceVfx(castVfx, bindPos.gameObject, false, false);
            }

            yield return new WaitUntil(() => isExitCast);
            _vfxGameService.ReleaseVfxInst(vfxObj);

            yield return new WaitUntil(() => isAtkFinished);

            OnAniEvent -= OnAniEvt;
            //Debug.Log("cast finished");
        }

        public IEnumerator DoSkillBackToPos(SkillInfo skillInfo)
        {
            //播放技能的回退动作
            yield return null;
        }

        /// <summary>
        /// 进行近战攻击
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <param name="keyFrameCallback">攻击动画关键帧回调</param>
        public IEnumerator DoMeleeAttack(BattleUnitEntity target, UnityAction<BattleAniEvtNames> keyFrameCallback)
        {
            var tarPos = target.Transform.position;
            var tarDir = target.Transform.forward;

            yield return MoveToTarget(tarPos);

            _animator.SetInteger(AniParamName.State, NpcAniState.MeleeAttack);

            var isAtkFinished = false;

            void OnAniEvt(string evtName)
            {
                if (evtName == BattleAniEvtNames.MeleeAttack.ToString("F"))
                {
                    //攻击动画关键帧
                    keyFrameCallback?.Invoke(BattleAniEvtNames.MeleeAttack);
                }
                else if (evtName == BattleAniEvtNames.EnterMeleeAttack.ToString("F"))
                {
                    keyFrameCallback?.Invoke(BattleAniEvtNames.EnterMeleeAttack);
                    //一旦进入攻击动作，设置state为idle，防止多次播放攻击动画
                    _animator.SetInteger(AniParamName.State, NpcAniState.Idle);
                }
                else if (evtName == BattleAniEvtNames.ExitMeleeAttack.ToString("F"))
                {
                    keyFrameCallback?.Invoke(BattleAniEvtNames.ExitMeleeAttack);
                    OnAniEvent -= OnAniEvt;
                    isAtkFinished = true;
                }
            }

            OnAniEvent += OnAniEvt;

            yield return new WaitUntil(() => isAtkFinished);

            //归位的同时，开启下一个单位的行动
            StartCoroutine(BackToOrigin());

            //等待归位再继续下一个单位的行动
            //yield return BackToOrigin();
        }
    }
}