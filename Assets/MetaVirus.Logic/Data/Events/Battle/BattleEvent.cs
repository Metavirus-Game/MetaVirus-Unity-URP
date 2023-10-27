namespace MetaVirus.Logic.Data.Events.Battle
{
    public struct BattleEvent
    {
        public enum BattleEventType
        {
            Ready = 0,
            Started = 1,
            OverviewBattleField,
            Running,
            EndWin,
            EndLose
        }

        public int BattleId { get; }
        public BattleEventType EventType { get; }

        public BattleEvent(int battleId, BattleEventType eventType)
        {
            BattleId = battleId;
            EventType = eventType;
        }
    }
}