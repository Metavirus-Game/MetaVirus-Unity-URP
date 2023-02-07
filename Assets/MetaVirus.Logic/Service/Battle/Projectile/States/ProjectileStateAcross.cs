using System.Linq;
using cfg.common;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Projectile.States
{
    public class ProjectileStateAcross : ProjectileStateBase
    {
        private bool[] _hitTargets;

        public ProjectileStateAcross(BattleProjectileObject projectile) : base(projectile)
        {
        }

        protected override void OnInit()
        {
            //穿过类型，接受多个target，目标位为阵型中心
            var pos = Projectile.Owner.UnitAni.GetVfxBindPos(VfxBindPos.Projectile).position;
            var target = Projectile.CastTargets[0];
            var tarPos = Projectile.Battle.GetFormationCenter(target.BattleUnit.Side).position;

            var dir = tarPos - pos;

            dir = Vector3.ProjectOnPlane(dir, Vector3.up);

            var transform = Projectile.transform;
            transform.position = pos;
            transform.forward = dir;

            _hitTargets = new bool[Projectile.CastTargets.Length];
        }

        public override void OnUpdate(float deltaTime)
        {
            var target = Projectile.CastTargets[0];

            var myTrans = Projectile.transform;

            var moveDistance = Projectile.GameDataService.CommonConfig.BattleProjectileSpeed *
                               Projectile.Info.Speed * deltaTime;
            var tarPos = Projectile.Battle.GetFormationCenter(target.BattleUnit.Side).position + myTrans.forward * 10;

            myTrans.position = Vector3.MoveTowards(myTrans.position, tarPos, moveDistance);

            for (var i = 0; i < Projectile.CastDatas.Length; i++)
            {
                var tar = Projectile.CastTargets[i];
                var pos = Projectile.transform.InverseTransformPoint(tar.Transform.position);
                if (pos.z <= 0 && !_hitTargets[i])
                {
                    _hitTargets[i] = true;
                    Projectile.OnHitTarget?.Invoke(Projectile.CastDatas[i], tar, SkillHitInfo.Default);
                }
            }

            var notHit = _hitTargets.Any(b => !b);
            if (!notHit && !Projectile.IsHit)
            {
                Projectile.DestroyProjectileVfx();
                Projectile.OnHit();
            }

            if (tarPos == myTrans.position && Projectile.IsHit)
            {
                Projectile.OnHitFinished();
            }
        }
    }
}