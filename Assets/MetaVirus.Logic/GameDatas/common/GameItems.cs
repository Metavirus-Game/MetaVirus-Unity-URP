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
   
/// <summary>
/// 道具表
/// </summary>
public partial class GameItems
{
    private readonly Dictionary<int, common.GameItem> _dataMap;
    private readonly List<common.GameItem> _dataList;
    
    public GameItems(ByteBuf _buf)
    {
        _dataMap = new Dictionary<int, common.GameItem>();
        _dataList = new List<common.GameItem>();
        
        for(int n = _buf.ReadSize() ; n > 0 ; --n)
        {
            common.GameItem _v;
            _v = common.GameItem.DeserializeGameItem(_buf);
            _dataList.Add(_v);
            _dataMap.Add(_v.Id, _v);
        }
        PostInit();
    }

    public Dictionary<int, common.GameItem> DataMap => _dataMap;
    public List<common.GameItem> DataList => _dataList;

    public common.GameItem GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public common.GameItem Get(int key) => _dataMap[key];
    public common.GameItem this[int key] => _dataMap[key];

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