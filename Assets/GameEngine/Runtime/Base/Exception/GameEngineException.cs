namespace GameEngine.Base.Exception
{
    public class GameEngineException : System.Exception
    {
        public GameEngineException(string reason) : base(reason)
        {
        }
    }
}