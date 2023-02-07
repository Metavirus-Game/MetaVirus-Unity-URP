using System.Collections.Generic;
using GameEngine.Base;

namespace GameEngine.AddressablesHelper
{
    public class AddressablesService : BaseService
    {
        private Dictionary<string, List<object>> _loadedObjects =
            new Dictionary<string, List<object>>();
    }
}