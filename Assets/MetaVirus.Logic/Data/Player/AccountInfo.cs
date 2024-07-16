namespace MetaVirus.Logic.Data.Player
{
    public class AccountInfo
    {
        public long AccountId { get; }
        public string LoginKey { get; }

        public string Channel { get; }

        public AccountInfo(long id, string key, string channel)
        {
            AccountId = id;
            LoginKey = key;
            Channel = channel;
        }
    }
}