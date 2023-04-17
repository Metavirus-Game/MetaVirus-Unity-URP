using System;
using System.Collections.Generic;
using System.Linq;
using cfg.battle;
using cfg.common;
using GameEngine;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Logic.Service.Player;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Battle
{
    public class BattleUnit
    {
        private readonly GameDataService _gameDataService;
        private readonly PlayerService _playerService;

        //private readonly BattleUnitDataPb _data;
        private readonly IMonsterDataProvider _data;

        public int Id { get; }
        public BattleUnitSide Side { get; }
        public int Slot { get; }
        public int SourceId { get; }

        /// <summary>
        /// 战斗单位类型
        /// </summary>
        public BattleSourceType SourceType { get; }

        /// <summary>
        /// 战斗单位使用的资源Id
        /// </summary>
        public int ResourceId { get; }

        public int Level => _data.Level;
        public int Quality => (int)_data.Quality;
        public float Scale { get; }

        private readonly Dictionary<int, int> _properties = new();
        private readonly Dictionary<int, int> _resistance = new();

        public BattleSkillData[] AtkSkills { get; }
        public IBattleUnitDataProvider UnitDataProvider { get; }

        public NpcResourceData ResourceData { get; }

        public int[] AtkSkillLevel { get; }


        public bool IsDead { get; set; }

        public string Name => UnitDataProvider?.Name ?? ResourceData.Name;

        public int RoundActionEnergy => GetProperty(AttributeId.CalcRoundActionEnergy);

        private float _actionEnergy;

        public float CurrActionEnergy => _actionEnergy;

        private BattleUnit(int unitId, int monsterId, int level, BattleUnitSide side, int slot)
        {
            _gameDataService = GameFramework.GetService<GameDataService>();
            _playerService = GameFramework.GetService<PlayerService>();

            Id = unitId;

            var md = _gameDataService.GetMonsterData(monsterId);
            _data = new MonsterDataProvider(md, level);

            Side = side;
            Slot = slot;
            SourceId = monsterId;
            SourceType = BattleSourceType.MonsterData;
            ResourceId = md.ResDataId;
            Scale = md.Scale;
            AtkSkillLevel = md.AtkSkillLevel;

            AtkSkills = md.AtkSkill.Select(id => _gameDataService.GetSkillData(id)).ToArray();

            UnitDataProvider = new MonsterBasicDataProvider(md);
        }

        private BattleUnit(BattleUnitDataPb data)
        {
            _data = new BattleUnitDataPbProvider(data);
            Id = _data.Id;

            _gameDataService = GameFramework.GetService<GameDataService>();
            _playerService = GameFramework.GetService<PlayerService>();

            Side = SideFromUnitId(_data.Id);
            Slot = SlotFromUnitId(_data.Id);
            SourceId = data.SourceId;
            SourceType = (BattleSourceType)data.SourceType;
            ResourceId = data.ResourceId;
            Scale = data.Scale;
            AtkSkillLevel = data.AtkSkillLevels.ToArray();

            for (var i = 0; i < data.Properties.Count; i++)
            {
                _properties[i + 1] = data.Properties[i];
            }

            for (var i = 0; i < data.Resistances.Count; i++)
            {
                _resistance[i + 1] = data.Resistances[i];
            }

            List<BattleSkillData> skills = new();

            foreach (var id in data.AtkSkillIds)
            {
                skills.Add(_gameDataService.GetSkillData(id));
            }

            AtkSkills = skills.ToArray();


            switch (SourceType)
            {
                case BattleSourceType.MonsterData:
                    var md = _gameDataService.GetMonsterData(SourceId);
                    UnitDataProvider = new MonsterBasicDataProvider(md);
                    break;
                case BattleSourceType.PlayerData:
                    //var pd = _playerService.GetPetData(SourceId);
                    var pd = _gameDataService.gameTable.PetDatas.GetOrDefault(SourceId);
                    UnitDataProvider = new PetDataBasicDataProvider(pd, Level);
                    break;
            }

            ResourceData = _gameDataService.GetResourceData(data.ResourceId);

            _actionEnergy = GetProperty(AttributeId.CalcActionEnergy);
        }

        public int GetProperty(AttributeId attributeId)
        {
            _properties.TryGetValue((int)attributeId, out var value);
            return value;
        }

        public void SetProperty(int attrId, int value)
        {
            _properties[attrId] = value;
        }

        public void SetProperty(AttributeId attrId, int value)
        {
            if (attrId == AttributeId.CalcActionEnergy)
            {
                _actionEnergy = value;
            }

            _properties[(int)attrId] = value;
        }


        public float IncProperty(int attributeId, int value)
        {
            var attrId = (AttributeId)attributeId;
            return IncProperty(attrId, value);
        }

        public int IncProperty(AttributeId attributeId, int value)
        {
            switch (attributeId)
            {
                case AttributeId.CalcHp:
                    return IncHp(value);
                case AttributeId.AttrMp:
                    return IncMp(value);
                default:
                    var v = GetProperty(attributeId);
                    if (value < 0)
                    {
                        return v;
                    }

                    v += value;
                    SetProperty(attributeId, v);
                    return v;
            }
        }

        public int DecProperty(int attributeId, int value)
        {
            var attrId = (AttributeId)attributeId;
            return DecProperty(attrId, value);
        }

        public int DecProperty(AttributeId attributeId, int value)
        {
            switch (attributeId)
            {
                case AttributeId.CalcHp:
                    return DecHp(value);
                case AttributeId.CalcMp:
                    return DecMp(value);
                case AttributeId.CalcHpMax:
                    return DecHpMax(value);
                case AttributeId.CalcMpMax:
                    return DecMpMax(value);
                default:
                    var v = GetProperty(attributeId);
                    if (value < 0)
                    {
                        return v;
                    }

                    v -= value;
                    if (v < 0) v = 0;
                    SetProperty(attributeId, v);
                    return v;
            }
        }

        public int IncHp(int valueInc)
        {
            var hp = GetProperty(AttributeId.CalcHp);
            var hpMax = GetProperty(AttributeId.CalcHpMax);
            if (valueInc < 0)
            {
                return hp;
            }

            hp += valueInc;
            hp = Mathf.Min(hp, hpMax);

            SetProperty(AttributeId.CalcHp, hp);

            return hp;
        }

        public int DecHp(int valueDec)
        {
            var hp = GetProperty(AttributeId.CalcHp);
            var hpMax = GetProperty(AttributeId.CalcHpMax);
            if (valueDec < 0)
            {
                return hp;
            }

            hp -= valueDec;
            hp = Mathf.Max(hp, 0);

            SetProperty(AttributeId.CalcHp, hp);

            return hp;
        }

        public int DecHpMax(int valueDec)
        {
            var hp = GetProperty(AttributeId.CalcHp);
            var hpMax = GetProperty(AttributeId.CalcHpMax);
            if (valueDec < 0)
            {
                return hpMax;
            }

            hpMax -= valueDec;
            hp = Mathf.Min(hp, hpMax);

            SetProperty(AttributeId.CalcHp, hp);
            SetProperty(AttributeId.CalcHpMax, hpMax);
            return hpMax;
        }

        public int IncMp(int valueInc)
        {
            var mp = GetProperty(AttributeId.CalcMp);
            var mpMax = GetProperty(AttributeId.CalcMpMax);
            if (valueInc < 0)
            {
                return mp;
            }

            mp += valueInc;
            mp = Mathf.Min(mp, mpMax);

            SetProperty(AttributeId.CalcMp, mp);

            return mp;
        }

        public int DecMp(int valueDec)
        {
            var mp = GetProperty(AttributeId.CalcMp);
            var mpMax = GetProperty(AttributeId.CalcMpMax);
            if (valueDec < 0)
            {
                return mp;
            }

            mp -= valueDec;
            mp = Mathf.Max(mp, 0);

            SetProperty(AttributeId.CalcMp, mp);

            return mp;
        }

        public int DecMpMax(int valueDec)
        {
            var mp = GetProperty(AttributeId.CalcMp);
            var mpMax = GetProperty(AttributeId.CalcMpMax);
            if (valueDec < 0)
            {
                return mpMax;
            }

            mpMax -= valueDec;
            mp = Mathf.Min(mp, mpMax);

            SetProperty(AttributeId.CalcMp, mp);
            SetProperty(AttributeId.CalcMpMax, mpMax);

            return mpMax;
        }

        public float IncTick(float tickElapse)
        {
            float ae = _actionEnergy; //GetProperty(AttributeId.CalcActionEnergy);
            var aeMax = GetProperty(AttributeId.CalcRoundActionEnergy);
            if (IsDead)
            {
                return ae;
            }

            //speed = 每个tick增长的actionEnergy
            var spd = GetProperty(AttributeId.CalcSpd);
            var aeInc = spd * tickElapse;

            ae += aeInc;
            if (ae > aeMax)
            {
                ae = aeMax;
            }

            SetProperty(AttributeId.CalcActionEnergy, (int)ae);
            _actionEnergy = ae;
            return ae;
        }

        /// <summary>
        /// 增长actionEnergy
        /// </summary>
        /// <param name="timeDelta">时间增量，单位毫秒</param>
        /// <param name="tickInterval">每个tick的间隔，单位毫秒</param>
        /// <returns></returns>
        public float IncActionEnergy(int timeDelta, int tickInterval)
        {
            var ae = GetProperty(AttributeId.CalcActionEnergy);
            var aeMax = GetProperty(AttributeId.CalcRoundActionEnergy);
            if (IsDead)
            {
                return ae;
            }

            //speed = 每个tick增长的actionEnergy
            var spd = GetProperty(AttributeId.CalcSpd);

            //经过了多少个tick
            var tick = (float)timeDelta / tickInterval;
            var aeInc = spd * tick;

            ae += (int)aeInc;
            if (ae > aeMax)
            {
                ae = aeMax;
            }

            SetProperty(AttributeId.CalcActionEnergy, ae);

            return ae;
        }

        public static BattleUnit FromProtobuf(BattleUnitDataPb protobuf)
        {
            return new BattleUnit(protobuf);
        }

        public static BattleUnit FromMonsterData(int unitId, int monsterId, int level, BattleUnitSide side, int slot)
        {
            return new BattleUnit(unitId, monsterId, level, side, slot);
        }

        public static int MakeUnitId(BattleUnitSide side, int slot)
        {
            var s = (int)side;
            return (s << 16) | slot;
        }

        public static BattleUnitSide SideFromUnitId(int unitId)
        {
            return (BattleUnitSide)(unitId >> 16);
        }

        public static int SlotFromUnitId(int unitId)
        {
            return unitId & 0xffff;
        }
    }
}