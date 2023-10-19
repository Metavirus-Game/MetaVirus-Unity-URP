using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.FairyGUI;
using GameEngine.Procedure;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Service.Arena.data;
using UnityEngine;
using UnityEngine.Events;

namespace MetaVirus.Logic.Service.Battle
{
    /// <summary>
    /// 竞技场战斗实例
    /// </summary>
    public class ArenaBattleInstance : BaseBattleInstance
    {
        private ArenaBattleResult _arenaBattleResult;
        private GameDataService _gameDataService;
        private FairyGUIService _guiService;

        public ArenaBattleInstance(ArenaBattleResult arenaResult, BattleField battleField) : base(
            arenaResult.BattleRecord, battleField)
        {
            _arenaBattleResult = arenaResult;
            _guiService = GameFramework.GetService<FairyGUIService>();
            _gameDataService = GameFramework.GetService<GameDataService>();
        }

        public override void ShowBattleResult(UnityAction onExit, UnityAction onReplay)
        {
            var comp = UIPackage.CreateObject("BattlePage", "ArenaResultUI").asCom;
            comp.alpha = 0;
            _guiService.AddToGRootFullscreen(comp);
            comp.TweenFade(1, 0.3f);

            var resultCtrl = comp.GetController("result");
            resultCtrl.SetSelectedIndex((int)_arenaBattleResult.Result);

            var txtMainScore = comp.GetChild("text_mainScore").asTextField;
            var txtMainScoreChanged = comp.GetChild("text_score_changed").asTextField;

            var txtRank = comp.GetChild("text_ranking").asTextField;
            var txtRankChanged = comp.GetChild("text_ranking_changed").asTextField;

            var arrScoreUp = comp.GetChild("imgArrScoreUp").asImage;
            var arrScoreDown = comp.GetChild("imgArrScoreDown").asImage;

            var arrRankUp = comp.GetChild("imgArrRankUp").asImage;
            var arrRankDown = comp.GetChild("imgArrRankDown").asImage;

            var score = _arenaBattleResult.ArenaInfo.Score;
            var rank = _arenaBattleResult.ArenaInfo.Rank;
            var scoreChanged = _arenaBattleResult.ScoreChanged;
            var rankChanged = _arenaBattleResult.RankChanged;

            switch (scoreChanged)
            {
                case > 0:
                    arrScoreUp.visible = true;
                    arrScoreUp.color = txtMainScoreChanged.color = _gameDataService.BattleColorHpInc();
                    break;
                case < 0:
                    arrScoreDown.visible = true;
                    arrScoreDown.color = txtMainScoreChanged.color = _gameDataService.BattleColorHpDec();
                    scoreChanged = -scoreChanged;
                    break;
                default:
                    txtMainScoreChanged.color = Color.white;
                    break;
            }

            switch (rankChanged)
            {
                case > 0:
                    arrRankDown.visible = true;
                    arrRankDown.color = txtRankChanged.color = _gameDataService.BattleColorHpDec();
                    break;
                case < 0:
                    arrRankUp.visible = true;
                    arrRankUp.color = txtRankChanged.color = _gameDataService.BattleColorHpInc();
                    rankChanged = -rankChanged;
                    break;
                default:
                    txtRankChanged.color = Color.white;
                    break;
            }

            txtMainScore.text = score.ToString();
            txtMainScoreChanged.text = scoreChanged.ToString();

            txtRank.text = rank.ToString();
            txtRankChanged.text = rankChanged.ToString();

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
                //onReplay();
                var procedure = GameFramework.GetService<DataNodeService>()
                    .GetData<ProcedureBase>(Constants.DataKeys.BattleBackProcedure);
                BattleService.EnterArenaMatch(_arenaBattleResult, procedure);
                comp.RemoveFromParent();
                comp.Dispose();
            });
        }
    }
}