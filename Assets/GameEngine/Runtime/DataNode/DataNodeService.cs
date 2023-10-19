using System;
using System.Collections.Generic;
using GameEngine.Base;
using GameEngine.Base.Attributes;
using GameEngine.Common;
using UnityEngine;

namespace GameEngine.DataNode
{
    [ServicePriority(EngineConsts.ServicePriorityValue.DataNodeService)]
    public class DataNodeService : BaseService
    {
        private DataNode _rootNode;
        private static readonly string[] PathSeparator = { ".", "/", "\\" };

        public override void PostConstruct()
        {
            _rootNode = DataNode.Create("<root>");
        }

        public T GetDataAndClear<T>(string path, DataNode root = null)
        {
            return GetData<T>(path, root, true);
        }

        public T GetData<T>(string path, DataNode root = null, bool clear = false)
        {
            var node = GetNode(path, root);

            if (node != null)
            {
                var ret = node.GetValue<T>();
                if (clear)
                {
                    node.Value = null;
                }

                return ret;
            }


            return default;
        }

        public object GetData(string path, DataNode root = null)
        {
            var node = GetNode(path, root);
            return node?.Value;
        }

        public DataNode SetData(string path, object data, DataNode root = null)
        {
            var node = GetOrAddNode(path, root);
            node.Value = data;
            return node;
        }

        public DataNode GetOrAddNode(string path, DataNode root = null)
        {
            var currNode = root ?? _rootNode;
            var paths = SplitPath(path);
            foreach (var p in paths)
            {
                currNode = currNode.GetChild(p);
            }

            return currNode;
        }

        public DataNode GetNode(string path, DataNode root = null)
        {
            var currNode = root ?? _rootNode;
            var paths = SplitPath(path);
            foreach (var p in paths)
            {
                currNode = currNode.GetChild(p, false);
                if (currNode == null)
                {
                    return null;
                }
            }

            return currNode;
        }

        private static IEnumerable<string> SplitPath(string path)
        {
            return path.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}