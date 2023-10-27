using cfg.common;
using FairyGUI;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.UI.Component.MonsterPanel.Formation;

namespace MetaVirus.Logic.UI
{
    public class BattleLoadingPage
    {
        public GComponent LoadingPageCom { get; private set; }
        public GProgressBar ProgressBar { get; private set; }

        private static readonly int[] FormationInfo = { 2, 3, 0 };

        public void SetBattleRecord(BattleRecord record)
        {
            var frameOpponent = LoadingPageCom.GetChild("frame_opponent").asCom;
            var framePlayer = LoadingPageCom.GetChild("frame_player").asCom;

            var txtSource = LoadingPageCom.GetChild("text_playerName").asTextField;
            var txtTarget = LoadingPageCom.GetChild("text_oppoName").asTextField;

            txtSource.text = record.SrcName;
            txtTarget.text = record.TarName;

            var formationSrc = new MonsterFormationComp(framePlayer, FormationInfo, false, false);
            var formationTar = new MonsterFormationComp(frameOpponent, FormationInfo, true, false);

            for (var i = 0; i < record.SrcUnits.Count; i++)
            {
                formationSrc.SetSlotPetData(i, new BattleUnitDataProvider(record.SrcUnits[i]));
            }

            for (var i = 0; i < record.TarUnits.Count; i++)
            {
                formationTar.SetSlotPetData(i, new BattleUnitDataProvider(record.TarUnits[i]));
            }
        }

        public static BattleLoadingPage Create(BattleRecord record)
        {
            var ret = new BattleLoadingPage
            {
                LoadingPageCom = UIPackage.CreateObject("LoadingPage", "BattleLoadingPage").asCom
            };
            ret.LoadingPageCom.sortingOrder = Constants.UizOrders.LoadingPage;
            ret.LoadingPageCom.alpha = 0;

            ret.ProgressBar = ret.LoadingPageCom.GetChild("loading").asProgress;
            ret.ProgressBar.value = 0;
            if (record != null)
            {
                ret.SetBattleRecord(record);
            }

            return ret;
        }
    }
}