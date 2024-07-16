using System.Collections.Generic;
using System.Linq;
using GameEngine.Base;

namespace MetaVirus.ResExplorer.Resource
{
    public class ExplorerResourceService : BaseService
    {
        private readonly List<string> _resPath = new();

        public override void ServiceReady()
        {
            LoadResPath();
        }

        public List<string> GetResourcePath(string prefix, string end)
        {
            return _resPath.Where(s => s.StartsWith(prefix) && s.EndsWith(end)).ToList();
        }

        private void LoadResPath()
        {
            // var resEnum = Addressables.ResourceLocators;
            //
            // foreach (var locator in resEnum)
            // {
            //     foreach (var t in locator.Keys)
            //     {
            //         _resPath.Add(t.ToString());
            //     }
            // }
        }
    }
}