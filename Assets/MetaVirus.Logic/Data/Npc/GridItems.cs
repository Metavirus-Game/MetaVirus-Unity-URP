using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaVirus.Logic.Data.Npc
{
    /// <summary>
    /// 存放管理所有的GridItem
    /// 以地图Id分类
    /// </summary>
    public class GridItems
    {
        private Dictionary<int, MapGridItems> _gridItems = new();

        public MapGridItems this[int mapId]
        {
            get
            {
                _gridItems.TryGetValue(mapId, out var item);
                if (item == null)
                {
                    item = new MapGridItems(mapId);
                    _gridItems[mapId] = item;
                }

                return item;
            }
        }

        public void ClearAllItems(params int[] excludeMapIds)
        {
            var mapIds = _gridItems.Keys.ToArray();
            foreach (var mapId in mapIds)
            {
                if (Array.IndexOf(excludeMapIds, mapId) == -1)
                {
                    _gridItems.Remove(mapId);
                }
            }
        }

        public void ClearMapItems(int mapId)
        {
            _gridItems.Remove(mapId);
        }

        public void AddGridItem(GridItem item)
        {
            var mapGridItems = this[item.MapId];
            mapGridItems[item.ID] = item;
        }

        public GridItem GetGridItem(int mapId, int gridItemId)
        {
            var mapGridItems = this[mapId];
            return mapGridItems[gridItemId];
        }
    }

    /// <summary>
    /// 指定mapId上的所有GridItem
    /// </summary>
    public class MapGridItems
    {
        public int MapId { get; }

        private readonly Dictionary<int, GridItem> _gridItems = new();

        public GridItem[] Items => _gridItems.Values.ToArray();

        public GridItem this[int gridItemId]
        {
            get
            {
                _gridItems.TryGetValue(gridItemId, out var item);
                return item;
            }
            set => _gridItems[gridItemId] = value;
        }

        public MapGridItems(int mapId)
        {
            MapId = mapId;
        }

        public void RemoveGridItem(int itemId)
        {
            _gridItems.Remove(itemId);
        }
    }
}