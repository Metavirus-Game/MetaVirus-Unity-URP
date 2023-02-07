using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Google.Protobuf;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Service.Battle.Frame;

namespace MetaVirus.Logic.Service.Battle
{
    public class BattleRecord
    {
        public int BattleId { get; }
        public int TickInterval { get; }
        public int RoundActionEnergy { get; }

        public List<BattleUnit> SrcUnits { get; } = new List<BattleUnit>();
        public List<BattleUnit> TarUnits { get; } = new List<BattleUnit>();

        public List<ActionFrame> Frames { get; } = new List<ActionFrame>();

        private BattleRecord(BattleRecordPb pBuf)
        {
            BattleId = pBuf.BattleId;

            TickInterval = pBuf.TickInterval;

            RoundActionEnergy = pBuf.RoundActionEnergy;

            foreach (var data in pBuf.SrcUnits)
            {
                SrcUnits.Add(BattleUnit.FromProtobuf(data));
            }

            foreach (var data in pBuf.TarUnits)
            {
                TarUnits.Add(BattleUnit.FromProtobuf(data));
            }

            foreach (var frame in pBuf.Frames)
            {
                var className = frame.TypeUrl.Substring(frame.TypeUrl.LastIndexOf(".", StringComparison.Ordinal) + 1);
                IMessage msgData = className switch
                {
                    nameof(SkillCastActionFramePb) => frame.Unpack<SkillCastActionFramePb>(),
                    nameof(BuffAttachActionFramePb) => frame.Unpack<BuffAttachActionFramePb>(),
                    nameof(BuffActionFramePb) => frame.Unpack<BuffActionFramePb>(),
                    nameof(PropertiesChangeActionFramePb) => frame.Unpack<PropertiesChangeActionFramePb>(),
                    _ => null
                };

                if (msgData == null) continue;
                var f = ActionFrame.Create(msgData);
                if (f != null)
                {
                    Frames.Add(f);
                }
            }
        }

        public static BattleRecord FromProtobuf(BattleRecordPb pBuf)
        {
            return new BattleRecord(pBuf);
        }

        public static BattleRecord FromGZipData(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var gzip = new GZipStream(ms, CompressionMode.Decompress);
                using (var resultMs = new MemoryStream())
                {
                    gzip.CopyTo(resultMs);
                    bytes = resultMs.ToArray();
                }
            }

            var record = BattleRecordPb.Parser.ParseFrom(bytes);
            return FromProtobuf(record);
        }
    }
}