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
   
/// <summary>
/// 成长率配置表
/// </summary>
public partial class AttrGrowupTables
{
    private readonly Dictionary<int, attr.AttrGrowupTable> _dataMap;
    private readonly List<attr.AttrGrowupTable> _dataList;
    
    public AttrGrowupTables(ByteBuf _buf)
    {
        _dataMap = new Dictionary<int, attr.AttrGrowupTable>();
        _dataList = new List<attr.AttrGrowupTable>();
        
        for(int n = _buf.ReadSize() ; n > 0 ; --n)
        {
            attr.AttrGrowupTable _v;
            _v = attr.AttrGrowupTable.DeserializeAttrGrowupTable(_buf);
            _dataList.Add(_v);
            _dataMap.Add(_v.Id, _v);
        }
        PostInit();
    }

    public Dictionary<int, attr.AttrGrowupTable> DataMap => _dataMap;
    public List<attr.AttrGrowupTable> DataList => _dataList;

    public attr.AttrGrowupTable GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public attr.AttrGrowupTable Get(int key) => _dataMap[key];
    public attr.AttrGrowupTable this[int key] => _dataMap[key];

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