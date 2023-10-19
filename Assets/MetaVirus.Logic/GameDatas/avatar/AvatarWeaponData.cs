//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using System.Collections.Generic;



namespace cfg.avatar
{

public sealed partial class AvatarWeaponData :  Bright.Config.BeanBase 
{
    public AvatarWeaponData(ByteBuf _buf) 
    {
        Id = _buf.ReadInt();
        ObjectName = _buf.ReadString();
        LeftHand = _buf.ReadBool();
        RightHand = _buf.ReadBool();
        TwoHands = _buf.ReadBool();
        WeaponType = (avatar.AvatarWeaponType)_buf.ReadInt();
        CreateActor = _buf.ReadBool();
        PostInit();
    }

    public static AvatarWeaponData DeserializeAvatarWeaponData(ByteBuf _buf)
    {
        return new avatar.AvatarWeaponData(_buf);
    }

    /// <summary>
    /// id
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// GameObject名称
    /// </summary>
    public string ObjectName { get; private set; }
    /// <summary>
    /// 左手可用
    /// </summary>
    public bool LeftHand { get; private set; }
    /// <summary>
    /// 右手可用
    /// </summary>
    public bool RightHand { get; private set; }
    /// <summary>
    /// 双手武器
    /// </summary>
    public bool TwoHands { get; private set; }
    /// <summary>
    /// 武器类型
    /// </summary>
    public avatar.AvatarWeaponType WeaponType { get; private set; }
    /// <summary>
    /// 是否开放给玩家创建角色使用
    /// </summary>
    public bool CreateActor { get; private set; }

    public const int __ID__ = 689593908;
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
        + "Id:" + Id + ","
        + "ObjectName:" + ObjectName + ","
        + "LeftHand:" + LeftHand + ","
        + "RightHand:" + RightHand + ","
        + "TwoHands:" + TwoHands + ","
        + "WeaponType:" + WeaponType + ","
        + "CreateActor:" + CreateActor + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}

}
