using System.Collections;
using FairyGUI;
using GameEngine;
using GameEngine.Utils;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.ArenaList;
using UnityEngine;

namespace MetaVirus.Logic.UI.Windows
{   
    [UIWindow("ui_arena_matching")]
    public class UIArenaMatching : BaseUIWindow
    {
        private ArenaService _arenaService;
        private GList _listMatching;
        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "ArenaMatchingUI").asCom;
            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _arenaService = GameFramework.GetService<ArenaService>();
            GameFramework.Inst.StartCoroutine(GetArenaMatchList());
            _listMatching = content.GetChild("listMatching").asList;
        }

        private IEnumerator GetArenaMatchList()
        {
            var task = _arenaService.GetArenaMatchList(1);
            yield return task.AsCoroution();
            var data = task.Result;
            foreach (var listData in data.Result)
            {
                var listItem = new ArenaMatchingListItem();
                listItem.RenderMatchingListItem(listData);
                _listMatching.AddChild(listItem._matchingListItem);
            }
        }
    }
}