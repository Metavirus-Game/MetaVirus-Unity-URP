using cfg.common;
using MetaVirus.Logic.Service.Vfx;

namespace MetaVirus.Logic.Service.Battle.Projectile.States
{
    public class ProjectileStateSingleTarget : ProjectileStateBase
    {
        public ProjectileStateSingleTarget(BattleProjectileObject projectile) : base(projectile)
        {
        }

        protected override void OnInit()
        {
            //目标位置类型，只接受一个target
            var pos = Projectile.Owner.UnitAni.GetVfxBindPos(VfxBindPos.BottomCenter).position;
            var target = Projectile.CastTargets[0];
            var tarPos = target.UnitAni.GetVfxBindPos(VfxBindPos.BottomCenter).position;

            var dir = pos - tarPos;
            var transform = Projectile.transform;

            transform.position = tarPos;
            transform.forward = dir;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!Projectile.IsHit)
            {
                var targetData = Projectile.CastDatas[0];
                var target = Projectile.CastTargets[0];

                Projectile.OnHit();
                Projectile.OnHitTarget?.Invoke(targetData, target, SkillHitInfo.Default);
                Projectile.OnHitFinished();
            }
        }
    }
}