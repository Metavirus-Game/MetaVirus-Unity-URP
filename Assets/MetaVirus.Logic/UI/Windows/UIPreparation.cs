using FairyGUI;
using MetaVirus.Logic.Service.UI;
using UnityEngine;

namespace MetaVirus.Logic.UI.Windows
{
    public class UIPreparartion : BaseUIWindow
    {
        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "ArenaPreparationUI").asCom;
            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            
        }
    }
}