namespace MetaVirus.Logic.Service.Battle.UI.Pages
{
    public abstract class BattleUIComponent
    {
        internal BattleUIManager Manager { get; }
        internal BaseBattleInstance Battle { get; }

        public BattleUIComponent(BattleUIManager manager, BaseBattleInstance battle)
        {
            Manager = manager;
            Battle = battle;
        }

        internal abstract void Load();
        internal abstract void Release();

        internal abstract void OnUpdate(float elapseTime, float realElapseTime);
    }
}