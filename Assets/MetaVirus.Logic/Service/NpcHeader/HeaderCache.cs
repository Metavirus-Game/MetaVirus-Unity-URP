using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MetaVirus.Logic.Service.NpcHeader
{
    public enum HeaderType
    {
        Avatar,
        NpcRes,
    }

    internal class HeaderCacheKeyPair
    {
        private readonly HeaderType _type;
        private readonly ulong _id;

        public HeaderCacheKeyPair(HeaderType type, ulong id)
        {
            _type = type;
            _id = id;
        }

        public override bool Equals(object obj)
        {
            return obj is HeaderCacheKeyPair other && Equals(other);
        }

        private bool Equals(HeaderCacheKeyPair other)
        {
            return _type == other._type && _id == other._id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)_type, _id);
        }
    }

    public class HeaderCacheItem
    {
        public HeaderType Type { get; }
        public ulong Id { get; }
        public Texture Texture { get; }
        public int Ref { get; private set; }

        public HeaderCacheItem(HeaderType type, ulong id, Texture texture)
        {
            Type = type;
            Id = id;
            Texture = texture;
        }

        public int IncRef()
        {
            return Ref++;
        }

        public int DecRef()
        {
            Ref--;
            Ref = Mathf.Max(0, Ref);
            return Ref;
        }
    }


    public class HeaderCache
    {
        private readonly Dictionary<HeaderCacheKeyPair, HeaderCacheItem> _headerCache = new();

        public HeaderCacheItem Get(HeaderType type, ulong id)
        {
            HeaderCacheKeyPair pair = new(type, id);
            _headerCache.TryGetValue(pair, out var item);
            item?.IncRef();
            return item;
        }

        public void Release(HeaderType type, ulong id)
        {
            HeaderCacheKeyPair pair = new(type, id);
            _headerCache.TryGetValue(pair, out var item);
            item?.DecRef();
        }

        public void Add(HeaderType type, ulong id, Texture texture)
        {
            HeaderCacheKeyPair pair = new(type, id);
            var item = Get(type, id);
            if (item != null) return;

            item = new HeaderCacheItem(type, id, texture);
            _headerCache[pair] = item;
        }

        public void RemoveAllUnused()
        {
            var keys = _headerCache.Keys.ToArray();
            foreach (var key in keys)
            {
                var item = _headerCache[key];
                if (item.Ref != 0) continue;
                _headerCache.Remove(key);
                if (item.Texture != null)
                {
                    Object.DestroyImmediate(item.Texture);
                }
            }
        }
    }
}