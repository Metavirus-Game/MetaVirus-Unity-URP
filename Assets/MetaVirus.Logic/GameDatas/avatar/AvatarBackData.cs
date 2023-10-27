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

public sealed partial class AvatarBackData :  Bright.Config.BeanBase 
{
    public AvatarBackData(ByteBuf _buf) 
    {
        Id = _buf.ReadInt();
        ObjectName = _buf.ReadString();
        Type = _buf.ReadInt();
        CreateActor = _buf.ReadBool();
        PostInit();
    }

    public static AvatarBackData DeserializeAvatarBackData(ByteBuf _buf)
    {
        return new avatar.AvatarBackData(_buf);
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
    /// 背部类型(0=披风，1=背包)
    /// </summary>
    public int Type { get; private set; }
    /// <summary>
    /// 是否开放给玩家创建角色使用
    /// </summary>
    public bool CreateActor { get; private set; }

    public const int __ID__ = 1999077119;
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
        + "Type:" + Type + ","
        + "CreateActor:" + CreateActor + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}

}
