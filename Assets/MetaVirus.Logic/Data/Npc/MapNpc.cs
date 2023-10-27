using cfg.common;
using cfg.map;
using UnityEngine;

namespace MetaVirus.Logic.Data.Npc
{
    public class MapNpc
    {
        public enum Type
        {
            Npc = 1,
            Monster = 2,
        }

        private int _id;

        public string Name { get; private set; }
        public int Level { get; private set; }
        public NpcRefreshInfo RefreshInfo { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Rotation { get; private set; }

        public Vector3 Direction
        {
            get
            {
                var qua = Quaternion.Euler(Rotation);
                var dir = (qua * Vector3.forward).normalized;
                return dir;
            }
        }

        public MapNpcState State { get; private set; }

        public int Id => _id;
        public int RefreshInfoId => GetNpcRefreshInfoId(_id);
        public Type NpcType => (Type)GetNpcType(_id);
        public int MapId => GetMapId(_id);


        public MapNpc(int mapNpcId, string name, int level, Vector3 position, Vector3 rotation, MapNpcState state,
            NpcRefreshInfo refreshInfo)
        {
            _id = mapNpcId;
            Name = name;
            Level = level;
            Position = position;
            Rotation = rotation;
            RefreshInfo = refreshInfo;
            State = state;
        }

        public static int GetNpcRefreshInfoId(int mapNpcId)
        {
            return mapNpcId & 0xff;
        }

        public static int GetNpcType(int mapNpcId)
        {
            return (mapNpcId >> 8) & 0xf;
        }

        public static int GetMapId(int mapNpcId)
        {
            return (mapNpcId >> 12) & 0xfffff;
        }
    }
}