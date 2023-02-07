namespace MetaVirus.Logic.Data.Events
{
    public struct NpcEvent
    {
        public enum NpcEventType
        {
            Spawn = 0,
            Destroy = 1,
        }

        public NpcEventType EvtType { get; }

        public int NpcId { get; }

        public int MapId { get; }

        public NpcEvent(NpcEventType evtType, int npcId, int mapId)
        {
            EvtType = evtType;
            NpcId = npcId;
            MapId = mapId;
        }
    }
}