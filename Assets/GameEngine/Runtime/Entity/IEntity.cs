using System.Threading.Tasks;

namespace GameEngine.Entity
{
    public interface IEntity
    {
        public EntityGroup Group { get; set; }

        public int Id { get; }

        public void OnInit(EntityGroup group);

        public Task<IEntity> LoadEntityAsync();

        public void OnUpdate(float timeElapse, float realTimeElapse);

        public void OnRelease();

        /// <summary>
        /// 记录当前Entity是否已经被Remove掉了<br/>
        /// 主要为了防止在调用Entity.LoadEntityAsync的过程中，Entity被release时，有可能无法正确释放资源
        /// </summary>
        public bool Removed { get; set; }

        public float TimeScale { get; set; }
    }
}