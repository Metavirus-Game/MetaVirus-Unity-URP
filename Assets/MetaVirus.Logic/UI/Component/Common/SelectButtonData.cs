namespace MetaVirus.Logic.UI.Component.Common
{
    public class SelectButtonData
    {
        public int DataId { get; }

        public string DataTitle { get; }
        public object Data { get; }


        public SelectButtonData(int id, object data, string title)
        {
            DataId = id;
            Data = data;
            DataTitle = title;
        }

        public T GetData<T>()
        {
            if (Data is T data)
                return data;
            return default;
        }
    }
}