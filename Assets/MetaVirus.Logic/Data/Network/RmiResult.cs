namespace MetaVirus.Logic.Data.Network
{
    public class RmiResult<T>
    {
        public int code;
        public string msg;
        public T retObject;
    }
}