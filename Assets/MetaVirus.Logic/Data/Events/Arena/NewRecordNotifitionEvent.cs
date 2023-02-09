using MetaVirus.Logic.Service.Arena.data;

namespace MetaVirus.Logic.Data.Events.Arena
{
    public class NewRecordNotifitionEvent
    {
        public ArenaPlayerRecord[] NewRecords { get; }

        public NewRecordNotifitionEvent(params ArenaPlayerRecord[] newRecords)
        {
            NewRecords = newRecords;
        }
    }
}