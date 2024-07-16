using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Firebase.Analytics;
using GameEngine;
using GameEngine.Base;
using GameEngine.Common;
using GameEngine.Config;
using GameEngine.DataNode;
using GameEngine.Network;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Network;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Protocols.User;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.UI;
using MetaVirus.Net.Messages.User;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace MetaVirus.Logic.Service
{
    internal abstract class Api
    {
        public static string LoginUrl => GameConfig.Inst.AccountServer + "account/loginRequestV2";
        public static string RegisterUrl => GameConfig.Inst.AccountServer + "account/registerRequest";
        public static string VerifyCode => GameConfig.Inst.AccountServer + "account/verifyCode";
        public static string ResendCode => GameConfig.Inst.AccountServer + "account/resendCode";
        public static string LoginCheck => GameConfig.Inst.AccountServer + "account/loginCheck";
        public static string DeleteAccount => GameConfig.Inst.AccountServer + "account/deleteAccount";
    }

    internal class LoginRequest
    {
        public string username;
        public string password;
        public string channel;
        public string serviceId;
        public string referralCode;
    }

    internal class VerifyCodeRequest
    {
        public string username;
        public string token;
        public string code;
    }

    public class LoginAccountResult
    {
        public string message;
        public int retCode;
    }

    public class LoginService : BaseService
    {
        public const string ChannelEmail = "metavirus_email";
        public const string ServiceIDEmail = "MetaVirus";

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
                        EngineConsts.SocketEvent.ConnectFailed => "network.error.connect.server.failed",
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

        private UnityWebRequestAsyncOperation HttpPost(string url, object data)
        {
            var json = JsonUtility.ToJson(data);
            var req = UnityWebRequest.PostWwwForm(url, json);
            req.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            var uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            uploadHandler.contentType = "application/json;charset=utf-8";
            req.uploadHandler = uploadHandler;
            var op = req.SendWebRequest();
            return op;
        }

        public IEnumerator DeleteAccount(long accountId, string email, string loginKey,
            UnityAction<bool> onDeleteResult)
        {
            var url = Api.DeleteAccount +
                      $"?accountId={accountId}&loginKey={loginKey}&username={email}&serviceId={ServiceIDEmail}&channel={ChannelEmail}";
            var req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                var r = JsonUtility.FromJson<RmiResult<string>>(req.downloadHandler.text);
                onDeleteResult?.Invoke(r.code == 0);
            }
            else
            {
                onDeleteResult?.Invoke(false);
            }
        }

        public IEnumerator SignInWithEmail(string email, string password, UnityAction<LoginResult> onLoginResult)
        {
            var loginUrl = Api.LoginUrl;
            var form = new LoginRequest
            {
                username = email,
                password = password,
                channel = ChannelEmail,
                serviceId = ServiceIDEmail,
                referralCode = "",
            };

            var op = HttpPost(loginUrl, form);
            yield return op;

            var req = op.webRequest;

            var failed = new LoginResult
            {
                accountId = -1,
                loginState = LoginResult.LoginStateFailed
            };

            if (req.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<RmiResult<LoginResult>>(req.downloadHandler.text);
                onLoginResult?.Invoke(result.code == 0 ? result.retObject : failed);
            }
            else
            {
                onLoginResult?.Invoke(failed);
            }
        }

        /// <summary>
        /// 使用邮箱进行注册，成功回调verifyCodeToken，失败回调null
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="onSinUpResult"></param>
        /// <returns></returns>
        public IEnumerator SignUpWithEmail(string email, string password, UnityAction<bool, string> onSinUpResult)
        {
            var regUrl = Api.RegisterUrl;
            var form = new LoginRequest
            {
                username = email,
                password = password,
                channel = ChannelEmail,
                serviceId = ServiceIDEmail,
                referralCode = "",
            };

            var op = HttpPost(regUrl, form);
            yield return op;

            var req = op.webRequest;

            if (req.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<RmiResult<string>>(req.downloadHandler.text);
                onSinUpResult?.Invoke(result.code == 0, result.msg);
            }
            else
            {
                onSinUpResult?.Invoke(false, "Please try again later");
            }
        }

        public IEnumerator LoginCheck(long accountId, string loginKey, UnityAction<bool> onCheckResult)
        {
            var url = Api.LoginCheck +
                      $"?accountId={accountId}&loginKey={loginKey}&serviceId={ServiceIDEmail}&channel={ChannelEmail}";
            var req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var r = JsonUtility.FromJson<RmiResult<string>>(req.downloadHandler.text);
                onCheckResult?.Invoke(r.code == 0);
            }
            else
            {
                onCheckResult?.Invoke(false);
            }
        }

        public IEnumerator ResendCode(string email, string token, UnityAction<bool, string> onResendResult)
        {
            var url = Api.ResendCode + "?username=" + email + "&token=" + token;
            var req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var r = JsonUtility.FromJson<RmiResult<string>>(req.downloadHandler.text);
                onResendResult?.Invoke(r.code == 0, r.msg);
            }
            else
            {
                onResendResult?.Invoke(false, "Network error, please try again later...");
            }
        }

        public IEnumerator VerifyRegCode(string email, string vcToken, string code, UnityAction<string> onVerifyResult)
        {
            var verifyUrl = Api.VerifyCode;
            var form = new VerifyCodeRequest
            {
                username = email,
                token = vcToken,
                code = code
            };

            var op = HttpPost(verifyUrl, form);
            yield return op;

            var req = op.webRequest;
            if (req.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<RmiResult<string>>(req.downloadHandler.text);
                if (result.code == 0)
                {
                    onVerifyResult?.Invoke("");
                }
                else
                {
                    onVerifyResult?.Invoke(result.msg);
                }
            }
            else
            {
                onVerifyResult?.Invoke("Please try again later...");
            }
        }

        public async Task<LoginAccountResult> LoginAccount(long loginId, string loginKey, string channel = null)
        {
            var accReq = new AccountLoginPbReq
            {
                Username = loginId.ToString(),
                Password = loginKey,
                ExtraInfo = channel ?? GameConfig.Inst.ChannelName
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
            string message = null;
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
                    if (resp.ProtoBufMsg.RetCode == -2)
                    {
                        //login closed, open a webpage
                        if (resp.ProtoBufMsg.HasMessage)
                        {
                            Application.OpenURL(resp.ProtoBufMsg.Message);
                            message = "server does not open";
                        }
                    }
                    else if (resp.ProtoBufMsg.RetCode == -1)
                    {
                        if (resp.ProtoBufMsg.HasMessage)
                        {
                            message = resp.ProtoBufMsg.Message;
                            sessionKey = null;
                        }
                        else
                        {
                            message = "";
                        }
                    }
                    else
                    {
                        sessionKey = resp.ProtoBufMsg.SessionKey;
                        if (sessionKey != "")
                        {
                            var accInfo = new AccountInfo(loginId, sessionKey,
                                channel ?? GameConfig.Inst.ChannelName);
                            Event.Emit(GameEvents.AccountEvent.AccountLogin);
                            _dataNodeService.SetData(Constants.DataKeys.AccountInfo, accInfo);
                        }
                    }
                }
            }

            var ret = new LoginAccountResult
            {
                message = message ?? sessionKey,
                retCode = message == null ? 0 : -1
            };

            return ret;
        }

        public async Task<int> LoginGame()
        {
            var accInfo = _dataNodeService.GetData<AccountInfo>(Constants.DataKeys.AccountInfo);
            var gameReq = new GameLoginPbReq()
            {
                SessionKey = accInfo.LoginKey
            };

            var t1 = _networkService.SendPacketToAsync(new GameLoginRequest(gameReq),
                GameConfig.Inst.WorldServerId);
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
                Event.Emit(GameEvents.AccountEvent.PlayerLogin);
            }

            return p;
        }
    }
}