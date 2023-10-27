using cfg.battle;
using cfg.common;
using GameEngine;
using GameEngine.ObjectPool;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Battle.Projectile.States;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;
using UnityEngine.Events;

namespace MetaVirus.Logic.Service.Battle.Projectile
{
    public enum ProjectileState
    {
        Born = 1,
        Hit,
        HitFinished,
        AllFinished,
        Recycling,
        Recycled
    }

    /// <summary>
    /// 战斗中产生的投射物体
    /// </summary>
    public class BattleProjectileObject : MonoBehaviour, IRecyclable
    {
        internal ProjectileState State;
        internal ProjectilePath Path;
        internal BaseBattleInstance Battle;
        internal BattleUnitEntity Owner;
        internal FrameSkillCastDataPb[] CastDatas;
        internal BattleUnitEntity[] CastTargets;

        internal ProjectileData Info;

        public bool IsHit => State >= ProjectileState.Hit;
        public bool IsHitCompleted => State >= ProjectileState.HitFinished;
        public bool IsProjectileCompleted => State >= ProjectileState.Recycled;

        // public bool IsHit { get; private set; } = false;
        //
        // public bool IsHitCompleted { get; private set; } = false;
        //
        // public bool IsProjectileCompleted { get; private set; } = false;

        public HitTargetAction OnHitTarget { get; set; }

        internal GameDataService GameDataService;
        internal BattleVfxGameService VFXGameService;

        internal GameObject VFXObject;
        private ProjectileStateBase _projectile;

        public void SetProjectileInfo(ProjectileData info, BaseBattleInstance battle, BattleUnitEntity owner,
            FrameSkillCastDataPb[] castDatas)
        {
            Info = info;
            Battle = battle;
            Owner = owner;
            CastDatas = castDatas;
            CastTargets = new BattleUnitEntity[CastDatas.Length];

            for (var i = 0; i < CastDatas.Length; i++)
            {
                CastTargets[i] = Battle.GetUnitEntity(CastDatas[i].CastData.TargetId);
            }

            State = ProjectileState.Born;

            GameDataService = GameFramework.GetService<GameDataService>();
            VFXGameService = GameFramework.GetService<BattleVfxGameService>();

            _projectile = ProjectileStateBase.Make(this);
            SetupVfx();
            //SetupPositionAndDir();
        }

        private void SetupVfx()
        {
            if (Info.ProjectileVfx > 0)
            {
                VFXObject = VFXGameService.InstanceVfx(Info.ProjectileVfx, gameObject, false, false);
            }
        }

        private void ChangeState(ProjectileState toState)
        {
            if (toState > State)
            {
                State = toState;
            }
        }

        public void OnHit()
        {
            ChangeState(ProjectileState.Hit);
        }

        public void OnHitFinished(bool allFinished = true)
        {
            ChangeState(ProjectileState.HitFinished);
            if (allFinished) OnFinished();
        }

        public void OnFinished()
        {
            ChangeState(ProjectileState.AllFinished);
        }


        /*
        private void SetupPositionAndDir()
        {
            if (CastDatas.Length == 0) return;
            switch (Info.Type)
            {
                case ProjectileType.Bullet:
                {
                    //子弹类型，只接受一个target
                    var pos = Owner.UnitAni.GetVfxBindPos(VfxBindPos.Projectile).position;
                    var target = CastTargets[0];
                    var tarPos = target.UnitAni.GetVfxBindPos(VfxBindPos.HitPos).position;
                    var dir = tarPos - pos;
                    transform.position = pos;
                    transform.forward = dir;
                    break;
                }
                case ProjectileType.Target:
                {
                    //目标位置类型，只接受一个target
                    var pos = Owner.UnitAni.GetVfxBindPos(VfxBindPos.BottomCenter).position;
                    var target = CastTargets[0];
                    var tarPos = target.UnitAni.GetVfxBindPos(VfxBindPos.BottomCenter).position;

                    var dir = pos - tarPos;
                    transform.position = tarPos;
                    transform.forward = dir;
                    break;
                }
                case ProjectileType.Bomb:
                {
                    //炸弹类型，接受多个target，目标位为阵型中心
                    var pos = Owner.UnitAni.GetVfxBindPos(VfxBindPos.Projectile).position;
                    var target = CastTargets[0];
                    var tarPos = Battle.GetFormationCenter(target.BattleUnit.Side).position;

                    var dir = tarPos - pos;
                    transform.position = pos;
                    transform.forward = dir;
                    break;
                }
                case ProjectileType.Summon:
                {
                    //召唤类型，接受多个target，目标位为阵型中心
                    var pos = Owner.UnitAni.GetVfxBindPos(VfxBindPos.HeadUp).position;
                    var target = CastTargets[0];
                    var tarPos = Battle.GetFormationCenter(target.BattleUnit.Side).position;

                    var dir = tarPos - pos;
                    transform.position = pos;
                    transform.forward = dir;
                    break;
                }
                case ProjectileType.Across:
                {
                    //穿过类型，接受多个target，目标位为阵型中心
                    var pos = Owner.UnitAni.GetVfxBindPos(VfxBindPos.Projectile).position;
                    var target = CastTargets[0];
                    var tarPos = Battle.GetFormationCenter(target.BattleUnit.Side).position;

                    var dir = tarPos - pos;

                    dir = Vector3.ProjectOnPlane(dir, Vector3.up);

                    transform.position = pos;
                    transform.forward = dir;
                    break;
                }
                case ProjectileType.Shockwave:
                {
                    //震荡波类型，接受多个target，目标位置为施放位置
                    var pos = Owner.UnitAni.GetVfxBindPos(Info.ProjectileVfx_Ref.BindPos).position;
                    var target = CastTargets[0];
                    var tarPos = Battle.GetFormationCenter(target.BattleUnit.Side).position;

                    var dir = tarPos - pos;
                    dir = Vector3.ProjectOnPlane(dir, Vector3.up);

                    transform.position = pos;
                    transform.forward = dir;

                    break;
                }
            }
        }
*/
        private void Start()
        {
            GameDataService = GameFramework.GetService<GameDataService>();
            VFXGameService = GameFramework.GetService<BattleVfxGameService>();
        }

        private void Update()
        {
            if (State >= ProjectileState.HitFinished)
            {
                if (State == ProjectileState.AllFinished)
                {
                    //投射物逻辑执行完毕，检测相关的特效是否播放完毕
                    if (VFXObject == null)
                    {
                        ChangeState(ProjectileState.Recycled);
                    }
                    else
                    {
                        ChangeState(ProjectileState.Recycling);
                        DestroyProjectileVfx(() => ChangeState(ProjectileState.Recycled));
                    }
                }

                return;
            }

            if (CastDatas.Length == 0)
            {
                return;
            }

            if (_projectile == null)
            {
                for (var i = 0; i < CastDatas.Length; i++)
                {
                    OnHitTarget?.Invoke(CastDatas[i], CastTargets[i], SkillHitInfo.Default);
                }

                OnHitFinished();
            }
            else
            {
                _projectile.OnUpdate(Battle.DeltaTime);
            }

            /*
            switch (Info.Type)
            {
                case ProjectileType.Bullet:
                    Update_TypeBullet();
                    break;
                case ProjectileType.Target:
                    Update_TypeTarget();
                    break;
                case ProjectileType.Summon:
                case ProjectileType.Bomb:
                    Update_Nova();
                    break;
                case ProjectileType.Across:
                    Update_Across();
                    break;
                case ProjectileType.Shockwave:
                    Update_Shockwave();
                    break;
            }
        
        */
        }

/*
        private void Update_Shockwave()
        {
            if (!IsHit)
            {
                IsHit = true;

                void HitCallback()
                {
                    for (var i = 0; i < CastDatas.Length; i++)
                    {
                        OnHitTarget?.Invoke(CastDatas[i], CastTargets[i]);
                    }

                    IsComplete = true;
                }

                var delay = VFXObject.GetComponent<BattleVfxHitDelay>();
                if (delay != null)
                {
                    delay.InvokeHitDelay(HitCallback);
                }
                else
                {
                    HitCallback();
                }
            }
        }

        private void Update_Across()
        {
            var target = CastTargets[0];

            var myTrans = transform;

            var moveDistance = GameDataService.CommonConfig.BattleProjectileSpeed * Info.Speed * Battle.DeltaTime;
            var tarPos = Battle.GetFormationCenter(target.BattleUnit.Side).position + myTrans.forward * 10;

            myTrans.position = Vector3.MoveTowards(myTrans.position, tarPos, moveDistance);

            for (var i = 0; i < CastDatas.Length; i++)
            {
                var tar = CastTargets[i];
                var pos = transform.InverseTransformPoint(tar.Transform.position);
                if (pos.z <= 0 && !_hitTargets[i])
                {
                    _hitTargets[i] = true;
                    OnHitTarget?.Invoke(CastDatas[i], tar);
                }
            }

            var notHit = _hitTargets.Any(b => !b);
            if (!notHit && !IsHit)
            {
                DestroyProjectileVfx();
                IsHit = true;
            }

            if (tarPos == myTrans.position && IsHit)
            {
                IsComplete = true;
            }
        }

        private void Update_Nova()
        {
            var targetData = CastDatas[0];
            var target = CastTargets[0];

            var moveDistance = GameDataService.CommonConfig.BattleProjectileSpeed * Info.Speed *
                               Battle.DeltaTime;
            var tarPos = Battle.GetFormationCenter(target.BattleUnit.Side).position;

            var myTrans = transform;
            var position = myTrans.position;

            var dir = tarPos - position;
            myTrans.forward = dir;

            position = Vector3.MoveTowards(position, tarPos, moveDistance);
            myTrans.position = position;

            if (position == tarPos && !IsHit)
            {
                IsHit = true;
                DestroyProjectileVfx();
                gameObject.transform.forward = Owner.Transform.forward;
                var hitVfxObj = VFXGameService.InstanceVfx(Info.HitVfx, gameObject);

                void HitCallback()
                {
                    foreach (var data in CastDatas)
                    {
                        target = Battle.GetUnitEntity(data.CastData.TargetId);
                        OnHitTarget?.Invoke(data, target);
                    }

                    IsComplete = true;
                }

                var delay = hitVfxObj.GetComponent<BattleVfxHitDelay>();
                if (delay != null)
                {
                    delay.InvokeHitDelay(HitCallback);
                }
                else
                {
                    HitCallback();
                }
            }
        }

        private void Update_TypeTarget()
        {
            if (!IsHit)
            {
                var targetData = CastDatas[0];
                var target = CastTargets[0];

                IsHit = true;
                OnHitTarget?.Invoke(targetData, target);
                IsComplete = true;
            }
        }

        private void Update_TypeBullet()
        {
            var targetData = CastDatas[0];
            var target = CastTargets[0];

            var moveDistance = GameDataService.CommonConfig.BattleProjectileSpeed * Info.Speed *
                               Battle.DeltaTime;
            var tarPos = target.UnitAni.GetVfxBindPos(VfxBindPos.HitPos).position;

            var myTrans = transform;
            var position = myTrans.position;

            var dir = tarPos - position;
            myTrans.forward = dir;

            position = Vector3.MoveTowards(position, tarPos, moveDistance);
            myTrans.position = position;

            if (position == tarPos && !IsHit)
            {
                IsHit = true;
                OnHitTarget?.Invoke(targetData, target);
                IsComplete = true;
            }
        }
*/

        public void OnSpawn()
        {
            State = ProjectileState.Born;
        }


        internal void DestroyProjectileVfx(UnityAction onReleased = null)
        {
            if (VFXObject != null)
            {
                VFXGameService.ReleaseVfxInst(VFXObject, onReleased);
                VFXObject = null;
            }
        }

        public void OnRecycle()
        {
            DestroyProjectileVfx();
            _projectile = null;
            gameObject.SetActive(false);
            transform.rotation = Quaternion.identity;
        }

        public void OnDestroy()
        {
            DestroyProjectileVfx();
            _projectile = null;
            Destroy(gameObject);
        }
    }
}