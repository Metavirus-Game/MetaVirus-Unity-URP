using cfg.battle;
using cfg.buff;

namespace MetaVirus.Logic.Service.Battle.data
{
    public class BuffInfo
    {
        public BuffData BuffData { get; }
        public int Level { get; }

        public BuffEffect LevelEffect { get; }

        public BuffInfo(BuffData buffData, int level)
        {
            BuffData = buffData;
            Level = level;

            foreach (var buffEffect in buffData.Effect)
            {
                if (buffEffect.Level == Level)
                {
                    LevelEffect = buffEffect;
                }
            }

            LevelEffect ??= buffData.Effect.Count == 0 ? null : buffData.Effect[0];
        }

        public string ToLog()
        {
            return $"[{BuffData.Id}]{BuffData.Name} Lv.{Level} ";
        }
    }
}