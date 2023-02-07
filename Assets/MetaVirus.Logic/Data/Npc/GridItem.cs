using cfg.common;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Utils;
using MetaVirus.Net.Messages.Common;
using UnityEngine;

namespace MetaVirus.Logic.Data.Npc
{
    public class GridItem
    {
        public int ID { get; }
        public GridItemType Type { get; }
        public int State { get; }
        public string Name { get; }
        public int Level { get; }
        public Gender Gender { get; }

        public Camp Camp { get; }

        public int MapId { get; }
        public ulong Avatar { get; }

        public Vector3 Position { get; private set; }

        public Vector3 Rotation { get; private set; }

        private GridItem(int id, int type, int state, string name, int level, int gender, long avatar, Vector3 position,
            Vector3 rotation, int camp, int mapId)
        {
            ID = id;
            Type = (GridItemType)type;
            State = state;
            Name = name;
            Level = level;
            Gender = (Gender)gender;
            Avatar = (ulong)avatar;
            Position = position;
            Rotation = rotation;
            Camp = (Camp)camp;
            MapId = mapId;
        }

        public void UpdatePosition(Vector3 position)
        {
            Position = position;
        }

        public void UpdateRotation(Vector3 rotation)
        {
            Rotation = rotation;
        }

        public static GridItem FromPbGridItem(PBGridItem pbGridItem)
        {
            var item = new GridItem(pbGridItem.Id, pbGridItem.Type, pbGridItem.State, pbGridItem.Name, pbGridItem.Level,
                pbGridItem.Gender, pbGridItem.Avatar, pbGridItem.Position.ToVector3(), pbGridItem.Rotation.ToVector3(),
                pbGridItem.Camp, pbGridItem.MapId);
            return item;
        }
    }
}