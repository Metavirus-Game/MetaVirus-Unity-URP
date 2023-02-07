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

public sealed partial class MonsterGroupData :  Bright.Config.BeanBase 
{
    public MonsterGroupData(ByteBuf _buf) 
    {
        Id = _buf.ReadInt();
        Name = _buf.ReadString();
        {int n = System.Math.Min(_buf.ReadSize(), _buf.Size);Slot = new battle.MonsterGroupSlot[n];for(var i = 0 ; i < n ; i++) { battle.MonsterGroupSlot _e;_e = battle.MonsterGroupSlot.DeserializeMonsterGroupSlot(_buf); Slot[i] = _e;}}
        DropGroupId = _buf.ReadInt();
        PostInit();
    }

    public static MonsterGroupData DeserializeMonsterGroupData(ByteBuf _buf)
    {
        return new battle.MonsterGroupData(_buf);
    }

    /// <summary>
    /// 怪物组Id
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// 怪物组名字
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// 战斗单位阵型，有单位的位置填怪物Id，没有的填0<br/>阵型位置布局<br/><br/>位置7 位置8 位置9      <br/>位置4 位置5 位置6<br/>位置1 位置2 位置3   <br/>怪物面向方向↓<br/><br/>玩家↑
    /// </summary>
    public battle.MonsterGroupSlot[] Slot { get; private set; }
    /// <summary>
    /// 绑定掉落组Id
    /// </summary>
    public int DropGroupId { get; private set; }

    public const int __ID__ = -35557723;
    public override int GetTypeId() => __ID__;

    public  void Resolve(Dictionary<string, object> _tables)
    {
        foreach(var _e in Slot) { _e?.Resolve(_tables); }
        PostResolve();
    }

    public  void TranslateText(System.Func<string, string, string> translator)
    {
        foreach(var _e in Slot) { _e?.TranslateText(translator); }
    }

    public override string ToString()
    {
        return "{ "
        + "Id:" + Id + ","
        + "Name:" + Name + ","
        + "Slot:" + Bright.Common.StringUtil.CollectionToString(Slot) + ","
        + "DropGroupId:" + DropGroupId + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}

}
