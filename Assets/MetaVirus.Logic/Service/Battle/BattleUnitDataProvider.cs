using cfg.battle;
using MetaVirus.Logic.Data.Player;

namespace MetaVirus.Logic.Service.Battle
{
    public interface IBattleUnitDataProvider
    {
        public string Name { get; }
        public int Level { get; }
    }

    public class MonsterBasicDataProvider : IBattleUnitDataProvider
    {
        private readonly MonsterData _monsterData;

        public string Name => _monsterData.Name;
        public int Level => _monsterData.Level;

        public MonsterBasicDataProvider(MonsterData monsterData)
        {
            _monsterData = monsterData;
        }
    }

    public class PlayerPetBasicDataProvider : IBattleUnitDataProvider
    {
        private readonly PlayerPetData _playerPetData;

        public string Name => _playerPetData.Name;
        public int Level => _playerPetData.Level;

        public PlayerPetBasicDataProvider(PlayerPetData playerPetData)
        {
            _playerPetData = playerPetData;
        }
    }

    public class PetDataBasicDataProvider : IBattleUnitDataProvider
    {
        private readonly PetData _petData;
        private int _level;

        public string Name => _petData.Name;
        public int Level => _level;

        public PetDataBasicDataProvider(PetData petData, int level)
        {
            _petData = petData;
            _level = level;
        }
    }
}