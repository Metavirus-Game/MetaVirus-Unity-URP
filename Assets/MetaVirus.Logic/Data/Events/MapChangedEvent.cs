namespace MetaVirus.Logic.Data.Events
{
    public struct MapChangedEvent
    {
        public enum MapChangeEventType
        {
            Leave = 0,
            Enter = 1
        }

        public MapChangeEventType EvtType { get; }
        public int MapId { get; }

        public MapChangedEvent(MapChangeEventType evtType, int mapId)
        {
            EvtType = evtType;
            MapId = mapId;
        }
    }
}