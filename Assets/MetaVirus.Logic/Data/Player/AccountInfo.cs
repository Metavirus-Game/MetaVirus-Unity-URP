namespace MetaVirus.Logic.Data.Player
{
    public class AccountInfo
    {
        public long AccountId { get; }
        public string LoginKey { get; }

        public AccountInfo(long id, string key)
        {
            AccountId = id;
            LoginKey = key;
        }
    }
}