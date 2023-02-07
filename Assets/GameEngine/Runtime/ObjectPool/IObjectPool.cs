namespace GameEngine.ObjectPool
{
    public interface IObjectPool
    {
        public string Name { get; }
        public int Count { get; }
        void ExtendPoolItemSpace(int count);

        public IRecyclable Get();

        public void Release(IRecyclable item);

        public void RemoveAllUnusedItems();
    }
}