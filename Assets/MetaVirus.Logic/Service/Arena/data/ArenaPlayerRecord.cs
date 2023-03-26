using MetaVirus.Net.Messages.Common;

namespace MetaVirus.Logic.Service.Arena.data
{
    public class ArenaPlayerRecord
    {
        public int RecordId { get; private set; }
        public int AttackId { get; private set; }
        public string AttackName { get; private set; }
        public int DefenceId { get; private set; }
        public string DefenceName { get; private set; }

        /// <summary>
        /// 战斗结果 0=win 1=draw 2=lose
        /// </summary>
        public int Result { get; private set; }

        public long BattleTime { get; private set; }
        public int BattleRecordVersion { get; private set; }
        public byte[] BattleRecord { get; set; }

        private ArenaPlayerRecord()
        {
        }

        public static ArenaPlayerRecord FromProtoBuf(PBArenaPlayerRecord record)
        {
            var ret = new ArenaPlayerRecord
            {
                RecordId = record.RecordId,
                AttackId = record.AttackId,
                AttackName = record.AttackName,
                DefenceId = record.DefenceId,
                DefenceName = record.DefenceName,
                Result = record.Result,
                BattleTime = record.BattleTime,
                BattleRecordVersion = record.BattleRecordVersion,
                BattleRecord = record.BattleRecord.ToByteArray()
            };

            return ret;
        }
    }
}