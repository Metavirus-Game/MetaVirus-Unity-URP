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
/// 游戏通用消息（服务器发给客户端的消息）
/// </summary>
public partial class GameMessages
{
    private readonly Dictionary<int, common.GameMessage> _dataMap;
    private readonly List<common.GameMessage> _dataList;
    
    public GameMessages(ByteBuf _buf)
    {
        _dataMap = new Dictionary<int, common.GameMessage>();
        _dataList = new List<common.GameMessage>();
        
        for(int n = _buf.ReadSize() ; n > 0 ; --n)
        {
            common.GameMessage _v;
            _v = common.GameMessage.DeserializeGameMessage(_buf);
            _dataList.Add(_v);
            _dataMap.Add(_v.Id, _v);
        }
        PostInit();
    }

    public Dictionary<int, common.GameMessage> DataMap => _dataMap;
    public List<common.GameMessage> DataList => _dataList;

    public common.GameMessage GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public common.GameMessage Get(int key) => _dataMap[key];
    public common.GameMessage this[int key] => _dataMap[key];

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