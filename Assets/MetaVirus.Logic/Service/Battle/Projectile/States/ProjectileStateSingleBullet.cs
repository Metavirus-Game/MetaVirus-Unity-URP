using cfg.common;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Projectile.States
{
    public class ProjectileStateSingleBullet : ProjectileStateBase
    {
        public ProjectileStateSingleBullet(BattleProjectileObject projectile) : base(projectile)
        {
        }

        protected override void OnInit()
        {
            var pos = Projectile.Owner.UnitAni.GetVfxBindPos(VfxBindPos.Projectile).position;
            var target = Projectile.CastTargets[0];
            var tarPos = target.UnitAni.GetVfxBindPos(VfxBindPos.HitPos).position;
            var dir = tarPos - pos;
            var transform = Projectile.transform;
            transform.position = pos;
            transform.forward = dir;
        }

        public override void OnUpdate(float deltaTime)
        {
            var targetData = Projectile.CastDatas[0];
            var target = Projectile.CastTargets[0];

            var moveDistance = Projectile.GameDataService.BattleProjectileSpeed(Projectile.Info.Speed) * deltaTime;
            var tarPos = target.UnitAni.GetVfxBindPos(VfxBindPos.HitPos).position;

            var myTrans = Projectile.transform;
            var position = myTrans.position;

            var dir = tarPos - position;
            myTrans.forward = dir;

            position = Vector3.MoveTowards(position, tarPos, moveDistance);
            myTrans.position = position;

            if (position == tarPos && !Projectile.IsHit)
            {
                Projectile.OnHit();
                Projectile.OnHitTarget?.Invoke(targetData, target, SkillHitInfo.Default);
                Projectile.OnHitFinished();
            }
        }
    }
}