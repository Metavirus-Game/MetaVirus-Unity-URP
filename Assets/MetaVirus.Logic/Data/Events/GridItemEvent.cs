using cfg.common;

namespace MetaVirus.Logic.Data.Events
{
    public struct GridItemEvent
    {
        public enum GridItemEventType
        {
            Spawn = 0,
            Destroy = 1,
        }

        public GridItemEventType EvtType { get; }

        public int ItemId { get; }

        public int MapId { get; }

        public GridItemType ItemType { get; }

        public GridItemEvent(GridItemEventType evtType, int npcId, int mapId, GridItemType type)
        {
            EvtType = evtType;
            ItemId = npcId;
            MapId = mapId;
            ItemType = type;
        }
    }
}