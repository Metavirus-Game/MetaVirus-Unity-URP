//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using System.Collections.Generic;



namespace cfg.common
{

public sealed partial class LocalizationData :  Bright.Config.BeanBase 
{
    public LocalizationData(ByteBuf _buf) 
    {
        Key = _buf.ReadString();
        TextCn = _buf.ReadString();
        TextEn = _buf.ReadString();
        PostInit();
    }

    public static LocalizationData DeserializeLocalizationData(ByteBuf _buf)
    {
        return new common.LocalizationData(_buf);
    }

    /// <summary>
    /// 本地化key
    /// </summary>
    public string Key { get; private set; }
    /// <summary>
    /// 中文
    /// </summary>
    public string TextCn { get; private set; }
    /// <summary>
    /// 英文
    /// </summary>
    public string TextEn { get; private set; }

    public const int __ID__ = 1004529574;
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
        + "Key:" + Key + ","
        + "TextCn:" + TextCn + ","
        + "TextEn:" + TextEn + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}

}
