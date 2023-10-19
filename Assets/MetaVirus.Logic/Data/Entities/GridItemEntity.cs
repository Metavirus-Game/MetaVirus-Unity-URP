using System.Threading.Tasks;
using cfg.common;
using GameEngine;
using GameEngine.Entity;
using GameEngine.ObjectPool;
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
            if (GridItemGo != null)
            {
                Addressables.ReleaseInstance(GridItemGo);
                GridItemGo = null;
            }
        }

        public static GridItemEntity NewGridItemEntity(GridItem item)
        {
            ObjectPool<NetPlayerGridItemEntity> pool = GameFramework
                .GetService<ObjectPoolService>()
                .GetObjectPool<NetPlayerGridItemEntity>("Pool_NewGridItemEntity");


            var type = (GridItemType)item.Type;
            var entity = type switch
            {
                GridItemType.Player => pool.Get<NetPlayerGridItemEntity>().SetGridItem(item),
                GridItemType.Bot => pool.Get<NetPlayerGridItemEntity>().SetGridItem(item),
                _ => null,
            };
            return entity;
        }

        public static void Recycle(IRecyclable recyclable)
        {
            ObjectPool<NetPlayerGridItemEntity> pool = GameFramework
                .GetService<ObjectPoolService>()
                .GetObjectPool<NetPlayerGridItemEntity>("Pool_NewGridItemEntity");
            pool.Release(recyclable);
        }
    }
}