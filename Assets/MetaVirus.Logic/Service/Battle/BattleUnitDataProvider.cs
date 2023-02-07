using cfg.battle;
using MetaVirus.Logic.Data.Player;

namespace MetaVirus.Logic.Service.Battle
{
    public interface IBattleUnitDataProvider
    {
        public string Name { get; }
        public int Level { get; }
    }

    public class MonsterDataProvider : IBattleUnitDataProvider
    {
        private readonly MonsterData _monsterData;

        public string Name => _monsterData.Name;
        public int Level => _monsterData.Level;

        public MonsterDataProvider(MonsterData monsterData)
        {
            _monsterData = monsterData;
        }
    }

    public class PlayerPetDataProvider : IBattleUnitDataProvider
    {
        private readonly PlayerPetData _playerPetData;

        public string Name => _playerPetData.Name;
        public int Level => _playerPetData.Level;

        public PlayerPetDataProvider(PlayerPetData playerPetData)
        {
            _playerPetData = playerPetData;
        }
    }
}