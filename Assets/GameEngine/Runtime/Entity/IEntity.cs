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

        public float TimeScale { get; set; }
    }
}