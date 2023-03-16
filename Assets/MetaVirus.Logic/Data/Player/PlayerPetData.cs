using cfg.attr;
using cfg.battle;
using cfg.common;
using GameEngine;
using MetaVirus.Logic.AttrsCalculator;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Logic.Service;
using MetaVirus.Net.Messages.Common;
using UnityEngine;

namespace MetaVirus.Logic.Data.Player
{
    public class PlayerPetData : IMonsterDataProvider
    {
        private readonly GameDataService _gameDataService;

        private readonly IAttrsCalculator _attrsCalculator;

        public PetData PetData { get; }

        public int Id => _info.PetId;

        public int PetDataId => PetData.Id;

        public int ModelResId => PetData.ResDataId;
        public CharacterData Character { get; }

        public Quality Quality => PetData.Quality;

        public MonsterType Type => PetData.Type_Ref;

        public LevelUpTable LevelUpTable => PetData.LevelUpTable == 0
            ? _gameDataService.QualityToLevelUpTable(Quality)
            : PetData.LevelUpTable_Ref;

        public AttrGrowTable GrowTable => PetData.GrowupTable_Ref;

        public int Level
        {
            get => _info.Level;
            set => _info.Level = value;
        }

        public string Name
        {
            get => _info.PetName == "" ? PetData.Name : _info.PetName;
            set => _info.PetName = value;
        }

        public int CurrExp => _info.Exp;

        public int ExpToNextLevel
        {
            get
            {
                var lvTable = LevelUpTable.LevelUpExps;
                var lv = Level;
                if (lv >= lvTable.Length)
                {
                    lv = lvTable.Length - 1;
                }

                return lv >= LevelUpTable.LvMax ? 0 : lvTable[lv];
            }
        }

        private PlayerPetInfo _info;

        public PlayerPetData(PlayerPetInfo info)
        {
            _gameDataService = GameFramework.GetService<GameDataService>();
            _info = info;
            PetData = _gameDataService.gameTable.PetDatas.Get(info.PetDataId);
            Character = _gameDataService.gameTable.CharacterDatas.Get(info.CharacterId);
            _attrsCalculator = new MonsterAttrsCalculator(this);
        }

        public int GetAttribute(AttributeId attr)
        {
            return _attrsCalculator[attr];
        }

        public int GetResistance(ResistanceId resiId)
        {
            var resIdx = (int)resiId - 1;
            var baseRes = GrowTable.Resistances[0].Resis;
            var growRes = GrowTable.Resistances[1].Resis;

            return (int)(baseRes[resIdx] + growRes[resIdx] * Level);
        }

        public int GetBaseAttribute(AttributeId attr)
        {
            var attrIdx = (int)attr - 1;
            var baseAttrs = GrowTable.Attributes[0];
            var growAttrs = GrowTable.Attributes[1];

            var l = Level - 1;
            l = Mathf.Max(l, 0);

            return (int)(baseAttrs.Attrs[attrIdx] + growAttrs.Attrs[attrIdx] * l);
        }

        public float GetBaseAttributeGrow(AttributeId attr)
        {
            var attrIdx = (int)attr - 1;
            var growAttrs = GrowTable.Attributes[1];
            return growAttrs.Attrs[attrIdx];
        }
    }
}