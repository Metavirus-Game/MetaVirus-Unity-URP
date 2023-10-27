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
   
/// <summary>
/// Avatar头发和五官数据
/// </summary>
public partial class AvatarSenseDatas
{
    private readonly Dictionary<int, avatar.AvatarSenseData> _dataMap;
    private readonly List<avatar.AvatarSenseData> _dataList;
    
    public AvatarSenseDatas(ByteBuf _buf)
    {
        _dataMap = new Dictionary<int, avatar.AvatarSenseData>();
        _dataList = new List<avatar.AvatarSenseData>();
        
        for(int n = _buf.ReadSize() ; n > 0 ; --n)
        {
            avatar.AvatarSenseData _v;
            _v = avatar.AvatarSenseData.DeserializeAvatarSenseData(_buf);
            _dataList.Add(_v);
            _dataMap.Add(_v.Id, _v);
        }
        PostInit();
    }

    public Dictionary<int, avatar.AvatarSenseData> DataMap => _dataMap;
    public List<avatar.AvatarSenseData> DataList => _dataList;

    public avatar.AvatarSenseData GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public avatar.AvatarSenseData Get(int key) => _dataMap[key];
    public avatar.AvatarSenseData this[int key] => _dataMap[key];

    public void Resolve(Dictionary<string, object> _tables)
    {
        foreach(var v in _dataList)
        {
            v.Resolve(_tables);
        }
        PostResolve();
    }

    public void TranslateText(System.Func<string, string, string> translator)
    {
        foreach(var v in _dataList)
        {
            v.TranslateText(translator);
        }
    }
    
    partial void PostInit();
    partial void PostResolve();
}

}