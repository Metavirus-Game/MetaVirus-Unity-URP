namespace MetaVirus.Logic.Utils
{
    public class TaskProgressHandler
    {
        public int Progress => _progress;

        private int _progress = 0;

        public void ReportProgress(int p)
        {
            _progress = p;
        }
    }
}