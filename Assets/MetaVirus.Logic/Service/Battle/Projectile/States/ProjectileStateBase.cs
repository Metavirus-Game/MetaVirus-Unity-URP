using cfg.skill;

namespace MetaVirus.Logic.Service.Battle.Projectile.States
{
    public abstract class ProjectileStateBase
    {
        protected readonly BattleProjectileObject Projectile;

        public ProjectileType Type => Projectile.Info.Type;

        protected ProjectileStateBase(BattleProjectileObject projectile)
        {
            Projectile = projectile;
            OnInit();
        }

        protected abstract void OnInit();

        public abstract void OnUpdate(float deltaTime);

        public static ProjectileStateBase Make(BattleProjectileObject projectile)
        {
            return projectile.Info.Type switch
            {
                ProjectileType.Single_Bullet => new ProjectileStateSingleBullet(projectile),
                ProjectileType.Single_Target => new ProjectileStateSingleTarget(projectile),
                ProjectileType.Single_Summon => new ProjectileStateSingleSummon(projectile),
                ProjectileType.Single_Bomb => new ProjectileStateSingleBomb(projectile),
                ProjectileType.Multi_Target => new ProjectileStateMultiTarget(projectile),
                ProjectileType.Multi_Summon => new ProjectileStateMultiSummon(projectile),
                ProjectileType.Multi_Bomb => new ProjectileStateMultiBomb(projectile),
                ProjectileType.Shockwave => new ProjectileStateShockwave(projectile),
                ProjectileType.Across => new ProjectileStateAcross(projectile),
                _ => null
            };
        }
    }
}