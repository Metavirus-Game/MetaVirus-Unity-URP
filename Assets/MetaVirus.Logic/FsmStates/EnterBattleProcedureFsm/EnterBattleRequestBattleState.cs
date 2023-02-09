using GameEngine;
using GameEngine.Fsm;
using GameEngine.Network;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.Protocols.Player;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.UI;
using MetaVirus.Net.Messages.Player;
using static MetaVirus.Logic.Service.GameDataService;

namespace MetaVirus.Logic.FsmStates.EnterBattleProcedureFsm
{
    //向服务器请求战斗并等待结果
    public class EnterBattleRequestBattleState : FsmState<EnterBattleProcedure>
    {
        private NetworkService _networkService;

        public override void OnInit(FsmEntity<EnterBattleProcedure> fsm)
        {
            _networkService = GameFramework.GetService<NetworkService>();
        }

        public override void OnEnter(FsmEntity<EnterBattleProcedure> fsm)
        {
            var owner = fsm.Owner;
            var player = PlayerEntity.Current;

            //向服务器请求npc战斗
            var request = new MapNpcBattleRequestCs(new MapNpcBattleRequestCsPb
            {
                MapNpcId = owner.NpcId
            });

            _networkService.SendPacketTo(request, player.PlayerInfo.sceneServerId, resp =>
            {
                if (resp.IsTimeout)
                {
                    //超时错误处理
                    UIDialog.ShowTimeoutMessage((btn, str, dialog) =>
                    {
                        dialog.Hide();
                        ChangeMapProcedure.BackToCurrentMap();
                    });
                }
                else
                {
                    var packet = resp.GetPacket<MapNpcBattleResponseSc>();
                    // if (packet.ProtoBufMsg.Result != 0)
                    // {
                    //     //战斗数据错误处理
                    //     UIDialog.ShowErrorMessage(LT("common.dialog.title.error"), LM(packet.ProtoBufMsg.Result),
                    //         (btn, s, dialog) =>
                    //         {
                    //             dialog.Hide();
                    //             ChangeMapProcedure.BackToCurrentMap();
                    //         });
                    // }
                    // else
                    {
                        var br = BattleRecord.FromGZipData(packet.ProtoBufMsg.BattleResult.ToByteArray());
                        //收到战斗数据
                        owner.BattleRecord = br;
                        //进入战斗
                        fsm.ChangeState<EnterBattleState>();
                    }
                }
            });
        }
    }
}