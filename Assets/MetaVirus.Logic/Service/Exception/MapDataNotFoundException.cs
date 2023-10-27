namespace MetaVirus.Logic.Service.Exception
{
    public class MapDataNotFoundException : System.Exception
    {
        public int mapId { get; }

        public MapDataNotFoundException(int mapId)
        {
            this.mapId = mapId;
        }
    }
}