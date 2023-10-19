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
/// 技能驱散状态
/// </summary>
public sealed partial class BuffDispel :  Bright.Config.BeanBase 
{
    public BuffDispel(ByteBuf _buf) 
    {
        DispelTarget = (skill.AttachEffectTarget)_buf.ReadInt();
        DispelProb = _buf.ReadFloat();
        {int n = System.Math.Min(_buf.ReadSize(), _buf.Size);DispelBuffIds = new int[n];for(var i = 0 ; i < n ; i++) { int _e;_e = _buf.ReadInt(); DispelBuffIds[i] = _e;}}
        PostInit();
    }

    public static BuffDispel DeserializeBuffDispel(ByteBuf _buf)
    {
        return new skill.BuffDispel(_buf);
    }

    /// <summary>
    /// 驱散对象，目标 or 自身
    /// </summary>
    public skill.AttachEffectTarget DispelTarget { get; private set; }
    /// <summary>
    /// 驱散几率(0-1)，0表示根据命中属性计算，&gt;0表示根据实际几率值计算
    /// </summary>
    public float DispelProb { get; private set; }
    /// <summary>
    /// 驱散buff &amp; debuff id列表
    /// </summary>
    public int[] DispelBuffIds { get; private set; }
    public battle.BuffData[] DispelBuffIds_Ref { get; private set; }

    public const int __ID__ = -701173799;
    public override int GetTypeId() => __ID__;

    public  void Resolve(Dictionary<string, object> _tables)
    {
        { int __n = DispelBuffIds.Length; battle.BuffDatas __table = (battle.BuffDatas)_tables["battle.BuffDatas"]; this.DispelBuffIds_Ref = new battle.BuffData[__n]; for(int i = 0 ; i < __n ; i++) { this.DispelBuffIds_Ref[i] =  __table.GetOrDefault(DispelBuffIds[i]); } }
        PostResolve();
    }

    public  void TranslateText(System.Func<string, string, string> translator)
    {
    }

    public override string ToString()
    {
        return "{ "
        + "DispelTarget:" + DispelTarget + ","
        + "DispelProb:" + DispelProb + ","
        + "DispelBuffIds:" + Bright.Common.StringUtil.CollectionToString(DispelBuffIds) + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}

}
