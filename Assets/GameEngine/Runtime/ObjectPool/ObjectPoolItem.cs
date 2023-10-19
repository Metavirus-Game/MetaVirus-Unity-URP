namespace GameEngine.ObjectPool
{
    public class ObjectPoolItem<T> where T : IRecyclable, new()
    {
        private T _item;
        private int _reference;

        public int Reference => _reference;

        public ObjectPoolItem(T item)
        {
            _item = item;
            _reference = 0;
        }

        public void OnSpawn()
        {
            _item.OnSpawn();
            _reference++;
        }

        public void OnRecycle()
        {
            _item.OnRecycle();
            _reference--;
        }
    }
}