using MetaVirus.Net.Messages.Common;

namespace MetaVirus.Logic.Data.Player
{
    public class PlayerPetInfo
    {
        //宠物Id
        public int PetId { get; set; }

        public string PetName { get; set; }

        //性格Id
        public int CharacterId { get; set; }

        //等级
        public int Level { get; set; }

        //PetData数据Id
        public int PetDataId { get; set; }
        
        public int Exp { get; set; }

        public static PlayerPetInfo FromPbPetData(PBPetData petData)
        {
            return new PlayerPetInfo
            {
                PetId = petData.PetId,
                PetName = petData.Name,
                CharacterId = petData.CharacterDataId,
                Level = petData.Level,
                PetDataId = petData.PetDataId,
                Exp = petData.Exp,
            };
        }
    }
}