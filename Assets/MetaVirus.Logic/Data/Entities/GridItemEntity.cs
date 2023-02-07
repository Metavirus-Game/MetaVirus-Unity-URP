using System.Threading.Tasks;
using cfg.common;
using GameEngine.Entity;
using MetaVirus.Logic.Data.Npc;
using MetaVirus.Logic.Data.Player;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MetaVirus.Logic.Data.Entities
{
    public abstract class GridItemEntity : BaseEntity
    {
        public GridItem GridItem { get; protected set; }
        protected GameObject GridItemGo;
        public override int Id => GridItem.ID;

        public GridItemType Type => GridItem.Type;

        public Camp Camp => GridItem.Camp;

        public int MapId => GridItem.MapId;

        public abstract GameObject NpcHUDPos { get; protected set; }

        public GridItemEntity(GridItem gridItem)
        {
            GridItem = gridItem;
        }

        public override void OnRelease()
        {
            Addressables.ReleaseInstance(GridItemGo);
        }

        public static GridItemEntity NewGridItemEntity(GridItem item)
        {
            var type = (GridItemType)item.Type;
            var entity = type switch
            {
                GridItemType.Player => new NetPlayerGridItemEntity(item),
                GridItemType.Bot => new NetPlayerGridItemEntity(item),
                _ => null,
            };
            return entity;
        }
    }
}