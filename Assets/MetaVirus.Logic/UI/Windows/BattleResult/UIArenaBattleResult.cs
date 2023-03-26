using FairyGUI;
using MetaVirus.Logic.Service.UI;

namespace MetaVirus.Logic.UI.Windows.BattleResult
{
    
    /// <summary>
    /// 战斗结算界面，只有在战斗状态下才能开启
    /// </summary>
    [UIWindow("ui_arena_battle_result")]
    public class UIArenaBattleResult : BaseUIWindow
    {
        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("BattlePage", "ArenaResultUI").asCom;
            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
        }
    }
}