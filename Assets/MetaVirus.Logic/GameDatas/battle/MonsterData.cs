//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using System.Collections.Generic;



namespace cfg.battle
{

public sealed partial class MonsterData :  Bright.Config.BeanBase 
{
    public MonsterData(ByteBuf _buf) 
    {
        Id = _buf.ReadInt();
        Name = _buf.ReadString();
        Desc = _buf.ReadString();
        Type = _buf.ReadInt();
        Quality = (common.Quality)_buf.ReadInt();
        ResDataId = _buf.ReadInt();
        Level = _buf.ReadInt();
        Character = _buf.ReadInt();
        {int n = System.Math.Min(_buf.ReadSize(), _buf.Size);AtkSkill = new int[n];for(var i = 0 ; i < n ; i++) { int _e;_e = _buf.ReadInt(); AtkSkill[i] = _e;}}
        {int n = System.Math.Min(_buf.ReadSize(), _buf.Size);AtkSkillLevel = new int[n];for(var i = 0 ; i < n ; i++) { int _e;_e = _buf.ReadInt(); AtkSkillLevel[i] = _e;}}
        LevelUpTable = _buf.ReadInt();
        GrowupTable = _buf.ReadInt();
        AttrPercent = _buf.ReadFloat();
        Scale = _buf.ReadFloat();
        Attributes = attr.AttributeArr.DeserializeAttributeArr(_buf);
        Resistances = attr.ResistanceArr.DeserializeResistanceArr(_buf);
        PostInit();
    }

    public static MonsterData DeserializeMonsterData(ByteBuf _buf)
    {
        return new battle.MonsterData(_buf);
    }

    /// <summary>
    /// 怪物Id
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// 怪物名称(战斗中显示)
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// 说明信息
    /// </summary>
    public string Desc { get; private set; }
    /// <summary>
    /// 怪物类型
    /// </summary>
    public int Type { get; private set; }
    public common.MonsterType Type_Ref { get; private set; }
    /// <summary>
    /// 怪物品质
    /// </summary>
    public common.Quality Quality { get; private set; }
    /// <summary>
    /// 模型资源Id
    /// </summary>
    public int ResDataId { get; private set; }
    public common.NpcResourceData ResDataId_Ref { get; private set; }
    /// <summary>
    /// 怪物等级
    /// </summary>
    public int Level { get; private set; }
    /// <summary>
    /// 怪物性格
    /// </summary>
    public int Character { get; private set; }
    public battle.CharacterData Character_Ref { get; private set; }
    /// <summary>
    /// 技能列表，多个id用 | 分割
    /// </summary>
    public int[] AtkSkill { get; private set; }
    public battle.BattleSkillData[] AtkSkill_Ref { get; private set; }
    /// <summary>
    /// 技能等级，多个等级用 | 分割，要和技能数量对应
    /// </summary>
    public int[] AtkSkillLevel { get; private set; }
    /// <summary>
    /// 怪物升级经验表<br/>不填默认为0，根据怪物的品质自动匹配
    /// </summary>
    public int LevelUpTable { get; private set; }
    public attr.LevelUpTable LevelUpTable_Ref { get; private set; }
    /// <summary>
    /// 成长率表Id
    /// </summary>
    public int GrowupTable { get; private set; }
    public attr.AttrGrowTable GrowupTable_Ref { get; private set; }
    /// <summary>
    /// 怪物的属性比例，不填默认为100(100%)
    /// </summary>
    public float AttrPercent { get; private set; }
    /// <summary>
    /// 模型缩放比例，不填默认为1
    /// </summary>
    public float Scale { get; private set; }
    /// <summary>
    /// 怪物属性附加值
    /// </summary>
    public attr.AttributeArr Attributes { get; private set; }
    /// <summary>
    /// 怪物抗性附加值
    /// </summary>
    public attr.ResistanceArr Resistances { get; private set; }

    public const int __ID__ = 1110215246;
    public override int GetTypeId() => __ID__;

    public  void Resolve(Dictionary<string, object> _tables)
    {
        this.Type_Ref = (_tables["common.MonsterTypes"] as common.MonsterTypes).GetOrDefault(Type);
        this.ResDataId_Ref = (_tables["common.NpcResourceDatas"] as common.NpcResourceDatas).GetOrDefault(ResDataId);
        this.Character_Ref = (_tables["battle.CharacterDatas"] as battle.CharacterDatas).GetOrDefault(Character);
        { int __n = AtkSkill.Length; battle.SkillDatas __table = (battle.SkillDatas)_tables["battle.SkillDatas"]; this.AtkSkill_Ref = new battle.BattleSkillData[__n]; for(int i = 0 ; i < __n ; i++) { this.AtkSkill_Ref[i] =  __table.GetOrDefault(AtkSkill[i]); } }
        this.LevelUpTable_Ref = (_tables["attr.LevelUpTables"] as attr.LevelUpTables).GetOrDefault(LevelUpTable);
        this.GrowupTable_Ref = (_tables["attr.AttrGrowTables"] as attr.AttrGrowTables).GetOrDefault(GrowupTable);
        Attributes?.Resolve(_tables);
        Resistances?.Resolve(_tables);
        PostResolve();
    }

    public  void TranslateText(System.Func<string, string, string> translator)
    {
        Attributes?.TranslateText(translator);
        Resistances?.TranslateText(translator);
    }

    public override string ToString()
    {
        return "{ "
        + "Id:" + Id + ","
        + "Name:" + Name + ","
        + "Desc:" + Desc + ","
        + "Type:" + Type + ","
        + "Quality:" + Quality + ","
        + "ResDataId:" + ResDataId + ","
        + "Level:" + Level + ","
        + "Character:" + Character + ","
        + "AtkSkill:" + Bright.Common.StringUtil.CollectionToString(AtkSkill) + ","
        + "AtkSkillLevel:" + Bright.Common.StringUtil.CollectionToString(AtkSkillLevel) + ","
        + "LevelUpTable:" + LevelUpTable + ","
        + "GrowupTable:" + GrowupTable + ","
        + "AttrPercent:" + AttrPercent + ","
        + "Scale:" + Scale + ","
        + "Attributes:" + Attributes + ","
        + "Resistances:" + Resistances + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}

}
