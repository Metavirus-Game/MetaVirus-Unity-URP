using GameEngine;
using GameEngine.Base;
using MetaVirus.Logic.Service.UI;
using UnityEngine;

namespace MetaVirus.Logic.Service
{
    public class NpcFunctionService : BaseService
    {
        private UIService _uiService;
        private GameDataService _gameDataService;

        public override void ServiceReady()
        {
            _uiService = GameFramework.GetService<UIService>();
            _gameDataService = GameFramework.GetService<GameDataService>();
        }

        public void ProcessNpcFunction(int npcFuncDataId, string param)
        {
            var funcData = _gameDataService.gameTable.NpcFunctionDatas.Get(npcFuncDataId);
            if (funcData == null)
            {
                Debug.LogError($"Npc Funcion Data [{npcFuncDataId}] Not Found!");
                return;
            }

            switch (funcData.Type)
            {
                case 1:
                    //open ui
                    _uiService.OpenWindow(param);
                    break;
            }
        }
    }
}