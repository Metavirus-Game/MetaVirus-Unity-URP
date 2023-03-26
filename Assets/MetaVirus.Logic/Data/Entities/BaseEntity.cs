using System.Threading.Tasks;
using GameEngine.Entity;

namespace MetaVirus.Logic.Data.Entities
{
    public abstract class BaseEntity : IEntity
    {
        private float _timeScale = 1;

        public float TimeScale
        {
            get => _timeScale;
            set
            {
                _timeScale = value;
                OnTimeScaleChanged();
            }
        }

        public EntityGroup Group { get; set; }
        public abstract int Id { get; }

        public virtual void OnInit(EntityGroup group)
        {
            Group = group;
        }

        public abstract Task<IEntity> LoadEntityAsync();

        public abstract void OnUpdate(float timeElapse, float realTimeElapse);

        public abstract void OnRelease();
        public bool Removed { get; set; } = false;
        public abstract void OnTimeScaleChanged();
    }
}