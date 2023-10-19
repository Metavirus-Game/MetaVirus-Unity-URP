using System.Collections.Generic;
using System.Linq;
using cfg.battle;
using cfg.common;
using cfg.skill;
using GameEngine;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;
using UnityEngine.Events;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Battle.Projectile
{
    public class BattleProjectileGroup
    {
        private readonly BaseBattleInstance _battle;
        private readonly BattleUnitEntity _owner;

        private readonly ProjectileData _projectileInfo;

        private readonly HitTargetAction _onHitTarget;
        private readonly FrameSkillCastDataPb[] _castDatas;

        private readonly BattleProjectileManager _manager;

        private readonly List<BattleProjectileObject> _projectiles = new();

        public BattleProjectileObject[] Projectiles => _projectiles.ToArray();

        private BattleVfxGameService _vfxGameService;

        public bool IsHitComplete { get; private set; } = false;
        public bool IsComplete { get; private set; } = false;

        public BattleProjectileGroup(BattleProjectileManager manager, BaseBattleInstance battle, BattleUnitEntity owner,
            ProjectileData projectileInfo, HitTargetAction onHitTarget,
            params FrameSkillCastDataPb[] castDatas)
        {
            _manager = manager;
            _battle = battle;
            _owner = owner;
            _projectileInfo = projectileInfo;
            _onHitTarget = onHitTarget;
            _castDatas = castDatas;

            _vfxGameService = GameFramework.GetService<BattleVfxGameService>();
        }


        /// <summary>
        /// 创建投射物发射特效，同时发射多个投射物，只有一个发射特效
        /// </summary>
        private void MakeMuzzle()
        {
            if (_projectileInfo.MuzzleVfx > 0)
            {
                var pos = Vector3.zero;
                var rot = Quaternion.identity;
                if (_projectileInfo.Type is ProjectileType.Single_Summon or ProjectileType.Multi_Summon)
                {
                    //单体和群体召唤物技能
                    //特效绑定在头顶发射位置
                    var trans = _owner.UnitAni.GetVfxBindPos(VfxBindPos.HeadUp);
                    pos = trans.position;

                    if (_castDatas.Length > 0)
                    {
                        var target = _battle.GetUnitEntity(_castDatas[0].CastData.TargetId);
                        //特效朝向目标位置
                        var tarPos = _battle.GetFormationCenter(target.BattleUnit.Side);
                        rot = Quaternion.LookRotation(tarPos.position - pos);
                    }
                }
                else
                {
                    var trans = _owner.UnitAni.GetVfxBindPos(VfxBindPos.Projectile);
                    pos = trans.position;
                    rot = trans.rotation;
                }

                _vfxGameService.InstanceVfx(_projectileInfo.MuzzleVfx, pos, rot);
            }
        }

        public void Start()
        {
            MakeMuzzle();
            var type = _projectileInfo.Type;
            if (BattleVfxGameService.IsSingleProjectile(type))
            {
                //为每个目标发射一个投射物
                foreach (var target in _castDatas)
                {
                    var projectile = _manager.GetProjectile(_battle, _owner, _projectileInfo, target);
                    projectile.OnHitTarget = _onHitTarget;
                    projectile.gameObject.SetActive(true);
                    _projectiles.Add(projectile);
                }
            }
            else
            {
                //发射一个投射物，攻击所有目标
                var projectile = _manager.GetProjectile(_battle, _owner, _projectileInfo, _castDatas);
                projectile.OnHitTarget = _onHitTarget;
                projectile.gameObject.SetActive(true);
                _projectiles.Add(projectile);
            }
        }

        public void OnDestory()
        {
        }

        public void OnUpdate(float elapseTime, float realElapseTime)
        {
            if (IsHitComplete)
            {
                IsComplete = IsProjectileCompleted;
                return;
            }

            IsHitComplete = IsProjectileHitCompleted;
            if (!IsHitComplete)
            {
                return;
            }

            //远程攻击完成，通知owner
            _owner.UnitAni.OnBattleAniEvent(BattleSkillCastEvents.ExitProjectile.ToString("F"));
        }

        private bool IsProjectileHitCompleted => _projectiles.All(p => p.IsHitCompleted);
        private bool IsProjectileCompleted => _projectiles.All(p => p.IsProjectileCompleted);
    }
}