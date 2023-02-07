using System.Collections.Generic;
using System.Linq;

namespace GameEngine.DataNode
{
    public class DataNode
    {
        private string _name;
        private object _value;
        private DataNode _parent;
        private Dictionary<string, DataNode> _children;

        public string Name => _name;

        public DataNode Parennt
        {
            get => _parent;
            internal set => _parent = value;
        }

        public object Value
        {
            get => _value;
            set => _value = value;
        }

        public static DataNode Create(string name, DataNode parent = null)
        {
            var node = new DataNode(name)
            {
                _parent = parent
            };
            return node;
        }

        private DataNode(string name)
        {
            _name = name;
            _children = new Dictionary<string, DataNode>();
        }

        public T GetValue<T>(T defaultValue = default)
        {
            if (_value is T value)
            {
                return value;
            }

            return defaultValue;

            //return _value.GetType().IsAssignableFrom(typeof(T)) ? (T)_value : defaultValue;
        }

        /**
         * 获取子数据节点
         */
        public DataNode GetChild(string name, bool createIfNotExist = true)
        {
            //TODO validate the name

            _children.TryGetValue(name, out var node);
            if (node == null)
            {
                if (createIfNotExist)
                {
                    node = Create(name, this);
                    _children[name] = node;
                }
            }

            return node;
        }

        public DataNode[] GetAllChildren()
        {
            return _children.Values.ToArray();
        }
    }
}