//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using System.Collections.Generic;



namespace cfg.skill
{

/// <summary>
/// 技能伤害
/// </summary>
public sealed partial class AtkValue :  Bright.Config.BeanBase 
{
    public AtkValue(ByteBuf _buf) 
    {
        CalcType = (skill.AtkCalcType)_buf.ReadInt();
        Value = _buf.ReadFloat();
        PostInit();
    }

    public static AtkValue DeserializeAtkValue(ByteBuf _buf)
    {
        return new skill.AtkValue(_buf);
    }

    /// <summary>
    /// 技能伤害计算方式
    /// </summary>
    public skill.AtkCalcType CalcType { get; private set; }
    /// <summary>
    /// 技能伤害值
    /// </summary>
    public float Value { get; private set; }

    public const int __ID__ = -419128106;
    public override int GetTypeId() => __ID__;

    public  void Resolve(Dictionary<string, object> _tables)
    {
        PostResolve();
    }

    public  void TranslateText(System.Func<string, string, string> translator)
    {
    }

    public override string ToString()
    {
        return "{ "
        + "CalcType:" + CalcType + ","
        + "Value:" + Value + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}

}
