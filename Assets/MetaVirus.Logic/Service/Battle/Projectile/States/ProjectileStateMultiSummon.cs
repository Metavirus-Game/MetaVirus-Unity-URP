using cfg.common;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Projectile.States
{
    public class ProjectileStateMultiSummon : ProjectileStateBase
    {
        public ProjectileStateMultiSummon(BattleProjectileObject projectile) : base(projectile)
        {
        }

        protected override void OnInit()
        {
            //召唤类型，接受多个target，目标位为阵型中心
            var pos = Projectile.Owner.UnitAni.GetVfxBindPos(VfxBindPos.HeadUp).position;
            var target = Projectile.CastTargets[0];
            var tarPos = Projectile.Battle.GetFormationCenter(target.BattleUnit.Side).position;

            var dir = tarPos - pos;

            var transform = Projectile.transform;
            transform.position = pos;
            transform.forward = dir;
        }

        public override void OnUpdate(float deltaTime)
        {
            var target = Projectile.CastTargets[0];

            var moveDistance = Projectile.GameDataService.CommonConfig.BattleProjectileSpeed * Projectile.Info.Speed *
                               deltaTime;
            var tarPos = Projectile.Battle.GetFormationCenter(target.BattleUnit.Side).position;

            var myTrans = Projectile.transform;
            var position = myTrans.position;

            var dir = tarPos - position;
            myTrans.forward = dir;

            position = Vector3.MoveTowards(position, tarPos, moveDistance);
            myTrans.position = position;

            if (position == tarPos && !Projectile.IsHit)
            {
                Projectile.OnHit();
                Projectile.DestroyProjectileVfx();
                var gameObject = Projectile.gameObject;

                gameObject.transform.forward = Projectile.Owner.Transform.forward;
                var hitVfxObj = Projectile.VFXGameService.InstanceVfx(Projectile.Info.HitVfx, gameObject);

                void HitCallback(SkillHitInfo info)
                {
                    foreach (var data in Projectile.CastDatas)
                    {
                        target = Projectile.Battle.GetUnitEntity(data.CastData.TargetId);
                        Projectile.OnHitTarget?.Invoke(data, target, info);
                    }

                    if (!info.IsHitFinished) return;
                    Projectile.OnHitFinished(false);
                }

                var delay = hitVfxObj.GetComponent<BattleVfxHitDelay>();
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