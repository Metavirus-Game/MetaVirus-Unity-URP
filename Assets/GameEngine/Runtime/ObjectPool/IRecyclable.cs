namespace GameEngine.ObjectPool
{
    public interface IRecyclable
    {
        /**
         * 对象被对象池生成时调用
         */
        public void OnSpawn();
        /**
         * 对象被对象池回收时调用
         */
        public void OnRecycle();

        /**
         * 对象被销毁时调用(从对象池移除，或对象池被销毁)
         */
        public void OnDestroy();

    }
}