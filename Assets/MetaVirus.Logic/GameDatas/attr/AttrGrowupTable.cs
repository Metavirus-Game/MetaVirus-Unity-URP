//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using System.Collections.Generic;



namespace cfg.attr
{

public sealed partial class AttrGrowupTable :  Bright.Config.BeanBase 
{
    public AttrGrowupTable(ByteBuf _buf) 
    {
        Id = _buf.ReadInt();
        Name = _buf.ReadString();
        {int n = System.Math.Min(_buf.ReadSize(), _buf.Size);Attributes = new System.Collections.Generic.List<attr.AttributeArr>(n);for(var i = 0 ; i < n ; i++) { attr.AttributeArr _e;  _e = attr.AttributeArr.DeserializeAttributeArr(_buf); Attributes.Add(_e);}}
        {int n = System.Math.Min(_buf.ReadSize(), _buf.Size);Resistances = new System.Collections.Generic.List<attr.ResistanceArr>(n);for(var i = 0 ; i < n ; i++) { attr.ResistanceArr _e;  _e = attr.ResistanceArr.DeserializeResistanceArr(_buf); Resistances.Add(_e);}}
        PostInit();
    }

    public static AttrGrowupTable DeserializeAttrGrowupTable(ByteBuf _buf)
    {
        return new attr.AttrGrowupTable(_buf);
    }

    /// <summary>
    /// 成长率表ID
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// 成长率表名称
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// 属性值, 每一条属性值数据，必须包含两行，第一行为基础属性，第二行位成长值
    /// </summary>
    public System.Collections.Generic.List<attr.AttributeArr> Attributes { get; private set; }
    /// <summary>
    /// 抗性值 每一条抗性值数据，必须包含两行，第一行为基础抗性，第二行位成长值
    /// </summary>
    public System.Collections.Generic.List<attr.ResistanceArr> Resistances { get; private set; }

    public const int __ID__ = -1110787662;
    public override int GetTypeId() => __ID__;

    public  void Resolve(Dictionary<string, object> _tables)
    {
        foreach(var _e in Attributes) { _e?.Resolve(_tables); }
        foreach(var _e in Resistances) { _e?.Resolve(_tables); }
        PostResolve();
    }

    public  void TranslateText(System.Func<string, string, string> translator)
    {
        foreach(var _e in Attributes) { _e?.TranslateText(translator); }
        foreach(var _e in Resistances) { _e?.TranslateText(translator); }
    }

    public override string ToString()
    {
        return "{ "
        + "Id:" + Id + ","
        + "Name:" + Name + ","
        + "Attributes:" + Bright.Common.StringUtil.CollectionToString(Attributes) + ","
        + "Resistances:" + Bright.Common.StringUtil.CollectionToString(Resistances) + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}

}
