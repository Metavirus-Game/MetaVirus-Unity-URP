using System.Linq;

namespace MetaVirus.Logic.Data.Player
{
    /// <summary>
    /// 玩家小队数据<br/>
    /// 玩家小队只允许上5个人，slost下标为0-4，具体放在那个位置，看阵型配置
    /// </summary>
    public class PlayerParty
    {
        /// <summary>
        /// 队伍Id，0-4，一共有5个
        /// </summary>
        public int PartyId { get; }

        public int FormationDataId { get; set; }
        public string Name { get; set; }
        public int SlotCount => Slots.Length;

        public int[] Slots { set; get; }

        public bool HasMonster => Slots.Any(slot => slot > 0);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">slotId, 0 to 4</param>
        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= Slots.Length) return -1;
                return Slots[index];
            }
            set
            {
                if (index < 0 || index >= Slots.Length) return;
                Slots[index] = value;
            }
        }

        public PlayerParty(int id, int formationDataId, string name)
        {
            PartyId = id;
            FormationDataId = formationDataId;
            Name = name;
            Slots = new[] { -1, -1, -1, -1, -1 };
        }
    }
}