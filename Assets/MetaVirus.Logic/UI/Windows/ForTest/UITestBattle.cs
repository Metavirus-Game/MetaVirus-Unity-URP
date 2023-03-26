using FairyGUI;
using GameEngine;
using GameEngine.Network;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.Protocols.Test;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Net.Messages.Test;
using UnityEngine;

namespace MetaVirus.Logic.UI.Windows.ForTest
{
    [UIWindow("ui_test_battle")]
    public class UITestBattle : BaseUIWindow
    {
        private PlayerService _playerService;
        private NetworkService _networkService;

        protected override GComponent MakeContent()
        {
            return UIPackage.CreateObject("Common", "TestBattleUI").asCom;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _playerService = GameFramework.GetService<PlayerService>();
            _networkService = GameFramework.GetService<NetworkService>();

            var button = content.GetChild("btnRunBattle").asButton;

            button.onClick.Set(() =>
            {
                var loading = button.GetController("loading");
                loading.selectedIndex = 1;
                var req = new TestBattleRequest(new TestBattleRequestCsPb());
                _networkService.SendPacketTo(req, _playerService.CurrentPlayerInfo.sceneServerId, resp =>
                {
                    loading.selectedIndex = 0;
                    if (resp.IsTimeout)
                    {
                        Debug.Log("timeout!!");
                        return;
                    }

                    var r = resp.GetPacket<TestBattleResponse>();
                    var bytes = r.ProtoBufMsg.BattleResult.ToByteArray();

                    var result = BattleRecord.FromGZipData(bytes);
                    BattleService.EnterBattle(result);
                    Hide();
                });
            });
        }
    }
}