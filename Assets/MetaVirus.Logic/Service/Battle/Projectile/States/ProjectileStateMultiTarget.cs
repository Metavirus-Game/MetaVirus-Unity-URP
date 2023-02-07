using cfg.common;
using GameEngine.Utils;
using MetaVirus.Logic.Service.Vfx;

namespace MetaVirus.Logic.Service.Battle.Projectile.States
{
    public class ProjectileStateMultiTarget : ProjectileStateBase
    {
        public ProjectileStateMultiTarget(BattleProjectileObject projectile) : base(projectile)
        {
        }

        protected override void OnInit()
        {
            //目标位置类型，群体目标
            var pos = Projectile.Owner.UnitAni.GetVfxBindPos(VfxBindPos.BottomCenter).position;
            var target = Projectile.CastTargets[0];
            var tarPos = Projectile.Battle.GetFormationCenter(target.BattleUnit.Side).position;

            var transform = Projectile.transform;

            transform.position = tarPos;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!Projectile.IsHit)
            {
                Projectile.OnHit();

                void HitCallback(SkillHitInfo info)
                {
                    foreach (var data in Projectile.CastDatas)
                    {
                        var target = Projectile.Battle.GetUnitEntity(data.CastData.TargetId);
                        Projectile.OnHitTarget?.Invoke(data, target, info);
                    }

                    if (!info.IsHitFinished) return;
                    Projectile.OnHitFinished(false);
                }

                var delay = GameEngineUtils.GetObjectComponent<BattleVfxHitDelay>(Projectile.VFXObject);
                if (delay != null)
                {
                    delay.InvokeHitDelay(HitCallback,
                        () => Projectile.OnFinished());
                }
                else
                {
                    HitCallback(SkillHitInfo.Default);
                    Projectile.OnFinished();
                }
            }
        }
    }
}