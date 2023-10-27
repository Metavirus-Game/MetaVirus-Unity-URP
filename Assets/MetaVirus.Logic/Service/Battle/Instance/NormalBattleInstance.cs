using FairyGUI;
using GameEngine;
using GameEngine.FairyGUI;
using UnityEngine.Events;

namespace MetaVirus.Logic.Service.Battle
{
    /// <summary>
    /// 普通战斗实例
    /// </summary>
    public class NormalBattleInstance : BaseBattleInstance
    {
        private FairyGUIService _guiService;

        public NormalBattleInstance(BattleRecord battleRecord, BattleField battleField,
            EventCallback0 exitCallback = null, EventCallback0 replayCallback = null) : base(battleRecord, battleField)
        {
            BattleUIManager.ExitCallback = exitCallback;
            BattleUIManager.ReplayCallback = replayCallback;
            
            _guiService = GameFramework.GetService<FairyGUIService>();
        }

        public override void ShowBattleResult(UnityAction onExit, UnityAction onReplay)
        {
            var comp = UIPackage.CreateObject("BattlePage", "NormalResultUI").asCom;
            comp.alpha = 0;
            _guiService.AddToGRootFullscreen(comp);
            comp.TweenFade(1, 0.3f);

            var resultCtrl = comp.GetController("result");

            resultCtrl.SetSelectedIndex((int)BattleRecord.BattleResult);

            var btnExit = comp.GetChild("btnReturn").asButton;
            var btnReplay = comp.GetChild("btnReplay").asButton;
            btnExit.onClick.Set(() =>
            {
                onExit();
                comp.RemoveFromParent();
                comp.Dispose();
            });
            btnReplay.onClick.Set(() =>
            {
                onReplay();
                comp.RemoveFromParent();
                comp.Dispose();
            });
        }
    }
}