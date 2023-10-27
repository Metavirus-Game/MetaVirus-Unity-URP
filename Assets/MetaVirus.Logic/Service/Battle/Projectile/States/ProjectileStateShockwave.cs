using GameEngine.Utils;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Projectile.States
{
    public class ProjectileStateShockwave : ProjectileStateBase
    {
        public ProjectileStateShockwave(BattleProjectileObject projectile) : base(projectile)
        {
        }

        protected override void OnInit()
        {
            //震荡波类型，接受多个target，目标位置为施放位置
            var pos = Projectile.Owner.UnitAni.GetVfxBindPos(Projectile.Info.ProjectileVfx_Ref.BindPos).position;
            var target = Projectile.CastTargets[0];
            var tarPos = Projectile.Battle.GetFormationCenter(target.BattleUnit.Side).position;

            var dir = tarPos - pos;
            dir = Vector3.ProjectOnPlane(dir, Vector3.up);

            var transform = Projectile.transform;
            transform.position = pos;
            transform.forward = dir;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!Projectile.IsHit)
            {
                Projectile.OnHit();

                void HitCallback(SkillHitInfo info)
                {
                    for (var i = 0; i < Projectile.CastDatas.Length; i++)
                    {
                        Projectile.OnHitTarget?.Invoke(Projectile.CastDatas[i], Projectile.CastTargets[i], info);
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