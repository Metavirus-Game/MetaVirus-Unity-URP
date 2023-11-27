using System;
using System.Threading.Tasks;
using GameEngine;
using GameEngine.Base;
using GameEngine.Common;
using GameEngine.Config;
using GameEngine.DataNode;
using GameEngine.Network;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Protocols.User;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.UI;
using MetaVirus.Net.Messages.User;
using UnityEngine;

namespace MetaVirus.Logic.Service
{
    public class LoginService : BaseService
    {
        private NetworkService _networkService;
        private DataNodeService _dataNodeService;
        private PlayerService _playerService;

        public override void ServiceReady()
        {
            _networkService = GameFramework.GetService<NetworkService>();
            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _playerService = GameFramework.GetService<PlayerService>();
        }

        public async Task<string> ConnectServer()
        {
            string connectMsg = null;

            if (_networkService.IsConnected())
            {
                connectMsg = "connected";
            }
            else
            {
                _networkService.ConnectServer((evt, msg) =>
                {
                    connectMsg = evt switch
                    {
                        EngineConsts.SocketEvent.Connected => "connected",
                        EngineConsts.SocketEvent.ConnectFailed => "Network_Connect_Server_Failed",
                        EngineConsts.SocketEvent.Exception => msg,
                        _ => "unknown"
                    };
                });

                // yield return new WaitUntil(() => connectMsg != null);

                while (connectMsg == null)
                {
                    await Task.Delay(100);
                }
            }

            return connectMsg;
        }

        public async Task<string> LoginAccount(long loginId, string loginKey)
        {
            var accReq = new AccountLoginPbReq
            {
                Username = loginId.ToString(),
                Password = loginKey,
                ExtraInfo = GameConfig.Inst.ChannelName
            };


            //Guest Login协议，专门为了测试和审核使用的，正式版本热更去掉SignInAsGuest的按钮
            if (loginId == -1)
            {
                var guid = GameConfig.Inst.SavePlayerId ? PlayerPrefs.GetString("user_guid") : "";
                if (guid == "")
                {
                    guid = Guid.NewGuid().ToString();
                    PlayerPrefs.SetString("user_guid", guid);
                }

                //-1，以guest登陆
                accReq.Username = "Guest";
                accReq.Password = guid;
            }

            string sessionKey = null;

            //登陆账号
            var t1 = _networkService.SendPacketToAsync(new AccountLoginRequest(accReq), GameConfig.Inst.WorldServerId);
            await t1;
            var r = t1.Result;
            if (r.IsTimeout)
            {
                sessionKey = "";
            }
            else
            {
                var resp = r.GetPacket<AccountLoginResponse>();
                if (resp != null)
                {
                    sessionKey = resp.ProtoBufMsg.SessionKey;
                    if (sessionKey != "")
                    {
                        var accInfo = new AccountInfo(loginId, sessionKey);
                        _dataNodeService.SetData(Constants.DataKeys.AccountInfo, accInfo);
                    }
                }
            }

            return sessionKey;
        }

        public async Task<int> LoginGame()
        {
            var accInfo = _dataNodeService.GetData<AccountInfo>(Constants.DataKeys.AccountInfo);
            var gameReq = new GameLoginPbReq()
            {
                SessionKey = accInfo.LoginKey
            };

            var t1 = _networkService.SendPacketToAsync(new GameLoginRequest(gameReq), GameConfig.Inst.WorldServerId);
            await t1;

            var pId = -1;
            var pName = "";

            var gameResp = t1.Result.GetPacket<GameLoginResponse>();
            if (gameResp != null)
            {
                pId = gameResp.ProtoBufMsg.PlayerIds[0];
                pName = gameResp.ProtoBufMsg.PlayerNames[0];
                if (pId > 0)
                {
                    _dataNodeService.SetData(Constants.DataKeys.LoginPlayerId, pId);
                }
            }

            return pId;
        }

        public async Task<PlayerInfo> LoginPlayer()
        {
            var pId = _dataNodeService.GetData<int>(Constants.DataKeys.LoginPlayerId);
            var playerReq = new PlayerLoginPbReq()
            {
                PlayerId = pId
            };

            var t1 = _networkService.SendPacketToAsync(new PlayerLoginRequest(playerReq),
                GameConfig.Inst.WorldServerId);
            await t1;

            if (t1.Result.IsTimeout)
            {
                return null;
            }

            var playerResp = t1.Result.GetPacket<PlayerLoginResponse>();
            var playerData = playerResp.ProtoBufMsg.PlayerData;
            var p = PlayerInfo.FromPlayerData(playerData, playerResp.ProtoBufMsg.SceneServer);

            if (p.sceneServerId != -1)
            {
                await _playerService.LoadPlayer(p);
                foreach (var pbPet in playerResp.ProtoBufMsg.Pets)
                {
                    var petInfo = PlayerPetInfo.FromPbPetData(pbPet);
                    _playerService.AddPetData(petInfo);
                }

                _playerService.OnPlayerLoaded();
            }

            return p;
        }
    }
}