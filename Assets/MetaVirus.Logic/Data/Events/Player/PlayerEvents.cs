using MetaVirus.Logic.Data.Entities;

namespace MetaVirus.Logic.Data.Events.Player
{
    public struct PlayerInteractingWithNpcEvent
    {
        public enum EventType
        {
            Start,
            End,
        }

        public EventType Type { get; }
        public NpcEntity NpcEntity { get; }

        public PlayerInteractingWithNpcEvent(EventType type, NpcEntity npcEntity)
        {
            NpcEntity = npcEntity;
            Type = type;
        }
    }

    public struct PlayerInteractiveNpcListChangedEvent
    {
        public enum EventType
        {
            Added = 0,
            Removed = 1,
        }

        public int NpcId { get; }

        public EventType Type { get; }

        public int[] NpcList { get; }

        public PlayerInteractiveNpcListChangedEvent(EventType type, int npcId, int[] npcList)
        {
            Type = type;
            NpcId = npcId;
            NpcList = npcList;
        }
    }
}