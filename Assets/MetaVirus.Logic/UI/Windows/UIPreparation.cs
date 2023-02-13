using AmazingAssets.TerrainToMesh;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Logic.Service.UI;
using UnityEngine;
using Constants = MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.UI.Windows
{
    public class UIPreparartion : BaseUIWindow
    {
        private DataNodeService _dataNodeService;

        private int _currOpponentData;

        // private IArenaDataProvider CurrOpponentData
        // {
        //     get => _currOpponentData;
        //     set
        //     {
        //         if (_currOpponentData != value)
        //         {
        //             _currOpponentData = value;
        //             
        //         }
        //     }
        // }
        
        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "ArenaPreparationUI").asCom;
            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _currOpponentData = _dataNodeService.GetDataAndClear<int>(Constants.DataKeys.UIArenaMatchingOpponentData);
            Debug.Log(_currOpponentData);
        }
    }
}