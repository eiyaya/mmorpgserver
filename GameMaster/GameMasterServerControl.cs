#region using

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;
using Database;
using DataContract;
using GameMasterServerService;
using GiftCodeDb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scorpion;
using NLog;
using OldPlayers;
using ProtoBuf;
using ServiceStack.Text;
using Shared;
using Table = DataTable.Table;

#endregion

namespace GameMaster
{
    public class GameMasterServerControlDefaultImpl : IGameMasterService, IStaticGameMasterServerControl
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerator GetServerCharacter(Coroutine coroutine,
                                              GameMasterService _this,
                                              GetServerCharacterInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;

            var accout = msg.Request.AccoutName;
            var dbAccoutGuid = GameMasterServer.Instance.DB.Get<DBUlong>(coroutine, DataCategory.LoginPlayerName,
                accout);
            yield return dbAccoutGuid;
            if (dbAccoutGuid.Data == null)
            {
                msg.Reply((int)ErrorCodes.Error_PlayerId_Not_Exist);
                yield break;
            }
            var accoutGuid = dbAccoutGuid.Data.Value;
            var dbAccout = GameMasterServer.Instance.DB.Get<DBPlayerLogin>(coroutine, DataCategory.LoginPlayer,
                accoutGuid);
            yield return dbAccout;
            if (dbAccout.Data == null)
            {
                msg.Reply((int)ErrorCodes.Error_PlayerId_Not_Exist);
                yield break;
            }

            msg.Response.Data.AddRange(dbAccout.Data.ServersPlayers);
            msg.Reply();
        }


        public IEnumerator UseGiftCode(Coroutine coroutine, GameMasterService _this, UseGiftCodeInMessage msg)
        {
            var ret = GameMasterServerControl.GiftCodeDb.UpdateStateAsync(coroutine, msg.Request.Code,msg.Request.ChannelId, GameMasterServer.Instance.GameMasterAgent);
            yield return ret;
            if (ret.Value == eDbReturn.Success)
            {
                msg.Reply((int)ErrorCodes.OK);
            }
            else
            {
                msg.Reply((int)ErrorCodes.Error_GiftCodeInvalid);
            }
        }

        public IEnumerator OnServerStart(Coroutine co, GameMasterService _this)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;

            //Thread.Sleep(GlobalVariable.WaitToConnectTimespan + 2000);
            GameMasterServer.Instance.Start(gameMasterServerControl);

            gameMasterServerControl.Init();
            GMCommandManager.Init();
            var co1 = CoroutineFactory.NewSubroutine(GMManager.Init, co);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            var co2 = CoroutineFactory.NewSubroutine(IdGenerator.Init, co, GameMasterServer.Instance.DB,
                GameMasterServerControl.Timer);
            if (co2.MoveNext())
            {
                yield return co2;
            }

            var dbConfig = File.ReadAllLines("../Config/giftcodedb.config");
            GameMasterServerControl.GiftCodeDb = new GiftCodeDbConnection(dbConfig[0]);


            const string oldPlayerConfig = "../Config/oldplayer.config";
            if (File.Exists(oldPlayerConfig))
            {
                var dbConfig2 = File.ReadAllLines(oldPlayerConfig);
                if (dbConfig2.Length > 0)
                {
                    var conn = new OldPlayersDbConnection(dbConfig2[0]);
                    if (conn.CreateConnection())
                    {
                        GameMasterServerControl.OldPlayersDb = conn;
                    }
                }
            }

            _this.Started = true;

            var lastTime = DateTime.Now;
            var waitTimeSeconds = 0.0f;

            while (true)
            {
                var deltaTime = (float)(DateTime.Now - lastTime).TotalSeconds;
                lastTime = DateTime.Now;

                try
                {
                    GameMasterServerControl.Timer.Update();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Tick error.");
                }

                if (deltaTime < GameMasterServerControl.Performance + waitTimeSeconds)
                {
                    waitTimeSeconds = GameMasterServerControl.Performance + waitTimeSeconds - deltaTime;
                    yield return gameMasterServerControl.Wait(co, TimeSpan.FromMilliseconds(waitTimeSeconds * 1000));
                }
                else
                {
                    waitTimeSeconds = 0.0f;
                }
            }
        }

        public IEnumerator OnServerStop(Coroutine co, GameMasterService _this)
        {
            var co1 = CoroutineFactory.NewSubroutine(GMManager.SaveDb, co);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            var co2 = CoroutineFactory.NewSubroutine(IdGenerator.SaveCoroutine, co);
            if (co2.MoveNext())
            {
                yield return co2;
            }
            GameMasterServer.Instance.DB.Dispose();
        }

        public IEnumerator CheckConnected(Coroutine co, GameMasterService _this, CheckConnectedInMessage msg)
        {
            var gmController = GameMasterManager.GetGM(msg.CharacterId);
            msg.Response = gmController == null ? 0 : 1;
            msg.Reply();
            yield break;
        }

        public IEnumerator CreateGmAccount(Coroutine co, GameMasterService _this, CreateGmAccountInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var gmName = msg.Request.Name;
            var password = msg.Request.Pwd;
            var priority = msg.Request.Priority;

            if (gmController.DbData.Priority > priority)
            {
                msg.Reply((int)ErrorCodes.Error_JurisdictionNotEnough);
                yield break;
            }

            var gmId = AsyncReturnValue<ulong>.Create();
            var returnValue = AsyncReturnValue<int>.Create();
            var co1 = CoroutineFactory.NewSubroutine(GameMasterManager.LoadGM, co, gmName, gmId, returnValue);
            if (co1.MoveNext())
            {
                yield return co1;
            }

            // 这个gm账号已经存在，功能变成修改密码
            if (returnValue.Value == 1)
            {
                var gm = GameMasterManager.GetGM(gmId.Value);
                if (gmController.DbData.Priority > gm.DbData.Priority)
                {
                    msg.Reply((int)ErrorCodes.Error_JurisdictionNotEnough);
                    yield break;
                }
                gm.DbData.Pwd = msg.Request.Pwd;
                gm.DbData.Priority = msg.Request.Priority;

                co1 = CoroutineFactory.NewSubroutine(GameMasterManager.ModifyGmAccount, co, gm, returnValue);
                if (co1.MoveNext())
                {
                    yield return co1;
                }

                if (returnValue.Value != 1)
                {
                    msg.Reply((int)ErrorCodes.Error_Create_GM_Account);
                    yield break;
                }
                msg.Response = 1;
                msg.Reply();
                yield break;
            }
            gmId.Dispose();
            // gm账号不存在，创建gm账号
            co1 = CoroutineFactory.NewSubroutine(GameMasterManager.CreateGmAccount, co, gmName, password, priority,
                returnValue);
            if (co1.MoveNext())
            {
                yield return co1;
            }

            if (returnValue.Value != 1)
            {
                msg.Reply((int)ErrorCodes.Error_Create_GM_Account);
                yield break;
            }
            returnValue.Dispose();
            msg.Reply();
        }

        public IEnumerator TakeOldPlayerReward(Coroutine coroutine, GameMasterService _this, TakeOldPlayerRewardInMessage msg)
        {
            var oldPlayerDb = GameMasterServerControl.OldPlayersDb;
            if (oldPlayerDb == null)
            {
                PlayerLog.WriteLog(msg.CharacterId, "TakeOldPlayerReward------NoOldPlayersDb-----CharacterId={1}", msg.CharacterId);
                yield break;
            }

            var characterId = msg.CharacterId;
            var msg1 = GameMasterServer.Instance.LoginAgent.GetUserId(msg.CharacterId, msg.Request.ClientId);
            yield return msg1.SendAndWaitUntilDone(coroutine);

            var userid = msg1.Response;//"StarJoys105id42150576";//;
            var ret = oldPlayerDb.GetUngetPlayerLevel(coroutine, userid, GameMasterServer.Instance.GameMasterAgent);
            yield return ret;
            var level = ret.Value1;
            if (level > 0)
            {
                var t = GameMasterServerControl.OldPlayersDb.UpdateStateAsync(coroutine, userid,
                    GameMasterServer.Instance.GameMasterAgent);
                yield return t;
                if (t.Value == 0)
                {
                    try
                    {
                        // 登录大礼包
                        //  var msg2 = GameMasterServer.Instance.LogicAgent.SendMailToCharacterById(characterId, 197, (int)eCreateItemType.OldPlayer, (int)SendToCharacterMailType.Normal, new Int32Array());
                        //  msg2.SendAndWaitUntilDone(coroutine);
                        //  PlayerLog.WriteLog(msg.CharacterId, "TakeOldPlayerReward success! CharacterId={0}, mail={1}", userid,197);
                        //  if (level >= 100)
                        //   {
                        // 等级大礼包
                        //  var msg3 = GameMasterServer.Instance.LogicAgent.SendMailToCharacterById(characterId, 198, (int)eCreateItemType.OldPlayer, (int)SendToCharacterMailType.Normal, new Int32Array());
                        //  msg3.SendAndWaitUntilDone(coroutine);
                        //   PlayerLog.WriteLog(msg.CharacterId, "TakeOldPlayerReward success! userid={0}, mail={1}", userid,198);
                        //  }
                        if (ret.Value2 > 0 || ret.Value3 > 0)
                        {
                            var array32 = new Int32Array();
                            array32.Items.Add(ret.Value2);
                            array32.Items.Add(ret.Value3);
                            var msgre = GameMasterServer.Instance.LogicAgent.SendMailToCharacterById(characterId, 600,(int) eCreateItemType.OldPlayer, (int) SendToCharacterMailType.RechargeRet, array32);
                            msgre.SendAndWaitUntilDone(coroutine);
                            PlayerLog.WriteLog(msg.CharacterId,"TakeOldPlayerReward------recharge success! CharacterId={0}, mail={1}", userid, 197);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error("TakeOldPlayerReward" + e.Message);
                    }
                }
                else
                {
                    PlayerLog.WriteLog(msg.CharacterId,"TakeOldPlayerReward------recharge Faile! CharacterId={0}, t.Value={1}", userid, t.Value);
                }
            }
            else
            {
                PlayerLog.WriteLog(msg.CharacterId,"TakeOldPlayerReward------recharge Faile! CharacterId={0}, level={1}", userid, level);
            }
        }

        public IEnumerator UpdateServer(Coroutine coroutine, GameMasterService _this, UpdateServerInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }
            GameMasterServer.Instance.UpdateManager.Update();
        }

        public IEnumerator UpdateServerAll(Coroutine co, GameMasterService _this, UpdateServerAllInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }
            GameMasterServer.Instance.UpdateManager.Update();

            var instance = GameMasterServer.Instance;

            var msg0 = instance.SceneAgent.UpdateServer(0);
            var msg1 = instance.LoginAgent.UpdateServer(0);
            var msg2 = instance.ActivityAgent.UpdateServer(0);
            var msg3 = instance.RankAgent.UpdateServer(0);
            var msg4 = instance.TeamAgent.UpdateServer(0);
            var msg5 = instance.LogicAgent.UpdateServer(0);
            var msg6 = instance.ChatAgent.UpdateServer(0);

            msg0.SendAndWaitUntilDone(co);
            msg1.SendAndWaitUntilDone(co);
            msg2.SendAndWaitUntilDone(co);
            msg3.SendAndWaitUntilDone(co);
            msg4.SendAndWaitUntilDone(co);
            msg5.SendAndWaitUntilDone(co);
            msg6.SendAndWaitUntilDone(co);

            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            msg.Reply();
        }

        public IEnumerator ReloadTable(Coroutine co, GameMasterService _this, ReloadTableInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }
            var instance = GameMasterServer.Instance;
            var name = msg.Request.TableName;

            var msg0 = instance.SceneAgent.ServerGMCommand("ReloadTable", name);
            var msg1 = instance.RankAgent.ServerGMCommand("ReloadTable", name);
            var msg2 = instance.TeamAgent.ServerGMCommand("ReloadTable", name);
            var msg3 = instance.LogicAgent.ServerGMCommand("ReloadTable", name);
            var msg4 = instance.ChatAgent.ServerGMCommand("ReloadTable", name);
            var msg5 = instance.ActivityAgent.ServerGMCommand("ReloadTable", name);
            var msg6 = instance.LoginAgent.ServerGMCommand("ReloadTable", name);

            msg0.SendAndWaitUntilDone(co);
            msg1.SendAndWaitUntilDone(co);
            msg2.SendAndWaitUntilDone(co);
            msg3.SendAndWaitUntilDone(co);
            msg4.SendAndWaitUntilDone(co);
            msg5.SendAndWaitUntilDone(co);
            msg6.SendAndWaitUntilDone(co);


            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            msg.Reply();
        }

        public IEnumerator GMCommand(Coroutine coroutine, GameMasterService _this, GMCommandInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var characterId = msg.Request.Id;
            var command = msg.Request.Command;
            if (command.Trim().Length == 0)
            {
                msg.Reply();
                yield break;
            }
            var commands = command.Split('\n').ToList();
            //先检查gm命令是否正确
            var invalidCmd = GMCommandManager.CheckCommands(commands);
            if (invalidCmd.Length > 0)
            {
                msg.Response = invalidCmd;
                msg.Reply((int)ErrorCodes.Error_GMCommandInvalid, true);
                yield break;
            }
            //检查玩家是否在线，如果在线，就直接把gm命令发过去
            //all
            var all = GMCommandManager.SplitCommands(commands, eGmCommandType.GMAll);
            var allOk = all.Count > 0;
            //logic
            {
                var cs = GMCommandManager.SplitCommands(commands, eGmCommandType.GMLogic);
                if (cs.Count > 0 || all.Count > 0)
                {
                    var sa = new StringArray();
                    sa.Items.AddRange(cs);
                    sa.Items.AddRange(all);
                    var msg1 = GameMasterServer.Instance.LogicAgent.GMCommand(characterId, sa);
                    yield return msg1.SendAndWaitUntilDone(coroutine);
                    if (msg1.State == MessageState.Reply)
                    {
                        if (msg1.ErrorCode == (int)ErrorCodes.OK)
                        {
                            msg.Response += msg1.Response.Items.GetDataString2();
                            commands.RemoveAll(c => cs.Contains(c));
                        }
                        else
                        {
                            allOk = false;
                        }
                    }
                    else
                    {
                        allOk = false;
                    }
                }
            }
            //scene
            {
                var cs = GMCommandManager.SplitCommands(commands, eGmCommandType.GMScene);
                if (cs.Count > 0 || all.Count > 0)
                {
                    var sa = new StringArray();
                    sa.Items.AddRange(cs);
                    sa.Items.AddRange(all);
                    var msg1 = GameMasterServer.Instance.SceneAgent.GMCommand(characterId, sa);
                    yield return msg1.SendAndWaitUntilDone(coroutine);
                    if (msg1.State == MessageState.Reply)
                    {
                        if (msg1.ErrorCode == (int)ErrorCodes.OK)
                        {
                            msg.Response += msg1.Response.Items.GetDataString2();
                            commands.RemoveAll(c => cs.Contains(c));
                        }
                        else
                        {
                            allOk = false;
                        }
                    }
                    else
                    {
                        allOk = false;
                    }
                }
            }
            //chat
            {
                var cs = GMCommandManager.SplitCommands(commands, eGmCommandType.GMChat);
                if (cs.Count > 0 || all.Count > 0)
                {
                    var sa = new StringArray();
                    sa.Items.AddRange(cs);
                    sa.Items.AddRange(all);
                    var msg1 = GameMasterServer.Instance.ChatAgent.GMCommand(characterId, sa);
                    yield return msg1.SendAndWaitUntilDone(coroutine);
                    if (msg1.State == MessageState.Reply)
                    {
                        if (msg1.ErrorCode == (int)ErrorCodes.OK)
                        {
                            msg.Response += msg1.Response.Items.GetDataString2();
                            commands.RemoveAll(c => cs.Contains(c));
                        }
                        else
                        {
                            allOk = false;
                        }
                    }
                    else
                    {
                        allOk = false;
                    }
                }
            }
            //rank
            {
                var cs = GMCommandManager.SplitCommands(commands, eGmCommandType.GMRank);
                if (cs.Count > 0 || all.Count > 0)
                {
                    var sa = new StringArray();
                    sa.Items.AddRange(cs);
                    sa.Items.AddRange(all);
                    var msg1 = GameMasterServer.Instance.RankAgent.GMCommand(characterId, sa);
                    yield return msg1.SendAndWaitUntilDone(coroutine);
                    if (msg1.State == MessageState.Reply)
                    {
                        if (msg1.ErrorCode == (int)ErrorCodes.OK)
                        {
                            msg.Response += msg1.Response.Items.GetDataString2();
                            commands.RemoveAll(c => cs.Contains(c));
                        }
                        else
                        {
                            allOk = false;
                        }
                    }
                    else
                    {
                        allOk = false;
                    }
                }
            }
            //team
            {
                var cs = GMCommandManager.SplitCommands(commands, eGmCommandType.GMTeam);
                if (cs.Count > 0 || all.Count > 0)
                {
                    var sa = new StringArray();
                    sa.Items.AddRange(cs);
                    sa.Items.AddRange(all);
                    var msg1 = GameMasterServer.Instance.TeamAgent.GMCommand(characterId, sa);
                    yield return msg1.SendAndWaitUntilDone(coroutine);
                    if (msg1.State == MessageState.Reply)
                    {
                        if (msg1.ErrorCode == (int)ErrorCodes.OK)
                        {
                            msg.Response += msg1.Response.Items.GetDataString2();
                            commands.RemoveAll(c => cs.Contains(c));
                        }
                        else
                        {
                            allOk = false;
                        }
                    }
                    else
                    {
                        allOk = false;
                    }
                }
            }
            if (allOk)
            {
                commands.RemoveAll(c => all.Contains(c));
            }
            if (commands.Count == 0)
            {
                msg.Reply();
                yield break;
            }
            //如果不在线，则把gm命令先存在gmserver里
            var oldCommands = GMManager.GetCommand(characterId);
            if (oldCommands != null)
            {
                commands.AddRange(oldCommands);
            }
            GMManager.AddCommand(characterId, commands);
            msg.Reply();
        }

        public IEnumerator CharacterConnected(Coroutine coroutine,
                                              GameMasterService _this,
                                              CharacterConnectedInMessage msg)
        {
            var request = msg.Request;
            var characterId = request.Id;
            var type = (eGmCommandType)request.ServerType;
            var oldCommands = GMManager.GetCommand(characterId);
            if (oldCommands == null || !oldCommands.Any())
            {
                yield break;
            }
            var commands = GMCommandManager.SplitCommands(oldCommands, type);
            if (commands.Count == 0)
            {
                yield break;
            }

            var cs = new StringArray();
            cs.Items.AddRange(commands);
            if (type == eGmCommandType.GMLogic)
            {
                var msg1 = GameMasterServer.Instance.LogicAgent.GMCommand(characterId, cs);
                yield return msg1.SendAndWaitUntilDone(coroutine);
                if (msg1.State == MessageState.Reply)
                {
                    if (msg1.ErrorCode == (int)ErrorCodes.OK)
                    {
                        GMManager.DelCommand(characterId, commands);
                    }
                }
            }
            else if (type == eGmCommandType.GMScene)
            {
                var msg1 = GameMasterServer.Instance.SceneAgent.GMCommand(characterId, cs);
                yield return msg1.SendAndWaitUntilDone(coroutine);
                if (msg1.State == MessageState.Reply)
                {
                    if (msg1.ErrorCode == (int)ErrorCodes.OK)
                    {
                        GMManager.DelCommand(characterId, commands);
                    }
                }
            }
        }

        public IEnumerator GenGiftCode(Coroutine coroutine, GameMasterService _this, GenGiftCodeInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var request = msg.Request;
            var type = request.Type;

            var gift = Table.GetGiftCode(type);
            if (null == gift)
            {
                msg.Reply((int)ErrorCodes.Error_GMCommandInvalid);
                yield break;
            }
            //var drop = Table.GetDropMother(gift.DropId);
            if (string.IsNullOrEmpty(gift.Drop1Id) && string.IsNullOrEmpty(gift.Drop2Id) &&
                string.IsNullOrEmpty(gift.Drop2Id))
            {
                msg.Reply((int)ErrorCodes.Error_GMCommandInvalid);
                yield break;
            }

            var count = request.Count;
            var prefix = IdGenerator.Confuse((ulong)type);
            var sb = new StringBuilder();
            var codes = new List<string>();
            for (var i = 0; i < count; i++)
            {
                var code = prefix + IdGenerator.Next(8);
                codes.Add(code);
                sb.Append(code).AppendLine();
            }

            var asyncRet = GameMasterServerControl.GiftCodeDb.ExecuteSqlTranAsync(coroutine, GiftCodeDbConnection.NewGiftCodeSqls(codes,msg.Request.ChannelId), GameMasterServer.Instance.GameMasterAgent);
            yield return asyncRet;
            var ret = asyncRet.Value;
            //var ret = GameMasterServerControl.GiftCodeDb.NewGiftCode(codes);
            if (ret > 0)
            {
                msg.Response = sb.ToString();
                msg.Reply();
            }
            else
            {
                msg.Reply((int)ErrorCodes.DataBase);
            }
        }

        //获得服务器人数
        public IEnumerator GetServerCharacterCount(Coroutine coroutine,
                                                   GameMasterService _this,
                                                   GetServerCharacterCountInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var msg1 = GameMasterServer.Instance.LoginAgent.GetServerCharacterCount(0, 0);
            yield return msg1.SendAndWaitUntilDone(coroutine);
            if (msg1.State == MessageState.Reply)
            {
                if (msg1.ErrorCode == (int)ErrorCodes.OK)
                {
                    msg.Response.Data.AddRange(msg1.Response.Data);
                    msg.Reply();
                    yield break;
                }
                msg.Reply(msg1.ErrorCode);
            }
        }

        public IEnumerator GetCharacterLogicDbInfo(Coroutine co,
                                                   GameMasterService _this,
                                                   GetCharacterLogicDbInfoInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var characterId = msg.Request.CharacterId;
            var dbLogic = GameMasterServer.Instance.DB.Get<DBCharacterLogic>(co, DataCategory.LogicCharacter,
                characterId);
            yield return dbLogic;
            if (dbLogic.Data == null)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterId_Not_Exist);
                yield break;
            }
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, dbLogic.Data);
                var buffer = ms.ToArray();
                msg.Response.LogicDbData = buffer.ToArray();
            }

            var dbLogin = GameMasterServer.Instance.DB.Get<DBCharacterLogin>(co, DataCategory.LoginCharacter,
                characterId);
            yield return dbLogin;
            if (dbLogin.Data == null)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterId_Not_Exist);
                yield break;
            }
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, dbLogin.Data);
                var buffer = ms.ToArray();
                msg.Response.LoginDbData = buffer.ToArray();
            }

            msg.Reply();
        }

        public IEnumerator AddAutoActvity(Coroutine coroutine, GameMasterService _this, AddAutoActvityInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var request = msg.Request;
            var logicAgent = GameMasterServer.Instance.SceneAgent;
            var logicMsg = logicAgent.AddAutoActvity(request.FubenId, request.StartTime, request.EndTime, request.Count);
            yield return logicMsg.SendAndWaitUntilDone(coroutine);
            msg.Reply();
        }
        public IEnumerator SendQuestion(Coroutine coroutine, GameMasterService _this, SendQuestionInMessage msg)
        {
            var sqls = new List<string>();
            sqls.Add(string.Format("insert into question(Title,Text,ById,ByName) value('{0}','{1}',{2},'{3}');", msg.Request.Mail.Title, msg.Request.Mail.Text, msg.Request.Mail.Guid, msg.Request.Mail.Name));

            var asyncRet = GameMasterServerControl.GiftCodeDb.ExecuteSqlTranAsync(coroutine, sqls, GameMasterServer.Instance.GameMasterAgent);
            yield return asyncRet;
            yield break;
        }

        public IEnumerator GetServerRankData(Coroutine coroutine,
                                             GameMasterService _this,
                                             GetServerRankDataInMessage msg)
        {
            var rankAgent = GameMasterServer.Instance.RankAgent;
            var rankMsg = rankAgent.SSGetServerRankData(0, msg.Request.ServerId, msg.Request.Ranktype);
            yield return rankMsg.SendAndWaitUntilDone(coroutine);

            if (rankMsg.State == MessageState.Reply)
            {
                if (rankMsg.ErrorCode == (int)ErrorCodes.OK)
                {
                    msg.Response = rankMsg.Response;
                    msg.Reply();
                }
                else
                {
                    msg.Reply(rankMsg.ErrorCode);
                }
            }
        }

        public IEnumerator GetTodayFunbenCount(Coroutine coroutine,
                                               GameMasterService _this,
                                               GetTodayFunbenCountInMessage msg)
        {
            var request = msg.Request;
            var logicAgent = GameMasterServer.Instance.LogicAgent;
            var logicMsg = logicAgent.SSGetTodayFunbenCount(msg.Request.CharacterId, msg.Request.ServerId,
                msg.Request.CharacterId, msg.Request.Selecttype);
            yield return logicMsg.SendAndWaitUntilDone(coroutine);
            if (logicMsg.State == MessageState.Reply)
            {
                if (logicMsg.ErrorCode == (int)ErrorCodes.OK)
                {
                    msg.Response = logicMsg.Response;
                    msg.Reply();
                }
                else
                {
                    msg.Reply(logicMsg.ErrorCode);
                }
            }
        }

        public IEnumerator SetFlag(Coroutine co, GameMasterService _this, SetFlagInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var request = msg.Request;
            var logicAgent = GameMasterServer.Instance.LogicAgent;
            var logicMsg = logicAgent.SSSetFlag(request.CharacterId, request.Changes);
            yield return logicMsg.SendAndWaitUntilDone(co);
            if (logicMsg.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }
            msg.Reply(logicMsg.ErrorCode);
        }

        public IEnumerator SetExdata(Coroutine co, GameMasterService _this, SetExdataInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var request = msg.Request;
            var logicAgent = GameMasterServer.Instance.LogicAgent;
            var logicMsg = logicAgent.SSSetExdata(request.CharacterId, request.Changes);
            yield return logicMsg.SendAndWaitUntilDone(co);
            if (logicMsg.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }
            msg.Reply(logicMsg.ErrorCode);
        }

        public IEnumerator GetLog(Coroutine co, GameMasterService _this, GetLogInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var request = msg.Request;
            var characterId = request.CharacterId;
            var date = request.Date;
            string data;
            var err = LogSettings.ReadBlock(characterId, date, out data);
            msg.Response = data;
            msg.Reply((int)err, true);
        }

        public IEnumerator ChangeServer(Coroutine co, GameMasterService _this, ChangeServerInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var characterId = msg.Request.CharacterId;
            var serverId = msg.Request.ServerId;
            var loginAgent = GameMasterServer.Instance.LoginAgent;
            var msg0 = loginAgent.GetLoginSimpleData(characterId, characterId);
            yield return msg0.SendAndWaitUntilDone(co);
            if (msg0.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }
            if (msg0.ErrorCode != 0)
            {
                msg.Reply(msg0.ErrorCode);
                yield break;
            }
            var oldServerId = msg0.Response.ServerId;
            //先检查该玩家有没有战盟，如果有战盟，提示让他先退战盟
            var msg1 = GameMasterServer.Instance.TeamAgent.SSGetAllianceData(characterId, oldServerId);
            yield return msg1.SendAndWaitUntilDone(co);
            if (msg1.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }
            if (msg1.ErrorCode == 0)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterInAlliance);
                yield break;
            }
            //修改我的排名
            {
                var msg2 = GameMasterServer.Instance.LogicAgent.ChangeServer(characterId, serverId);
                yield return msg2.SendAndWaitUntilDone(co);
                if (msg2.State != MessageState.Reply)
                {
                    msg.Reply((int)ErrorCodes.Error_TimeOut);
                    yield break;
                }
                if (msg2.ErrorCode != 0)
                {
                    msg.Reply(msg2.ErrorCode);
                    yield break;
                }
            }
            //通知login，转服
            var msg3 = loginAgent.ChangeServer(0, characterId, serverId);
            yield return msg3.SendAndWaitUntilDone(co);
            if (msg3.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }
            msg.Reply(msg3.ErrorCode);
        }

        public IEnumerator Login(Coroutine co, GameMasterService _this, LoginInMessage msg)
        {
            var gmName = msg.Request.Name;
            var password = msg.Request.Password;

            var gmId = AsyncReturnValue<ulong>.Create();
            var returnValue = AsyncReturnValue<int>.Create();
            var co1 = CoroutineFactory.NewSubroutine(GameMasterManager.LoadGM, co, gmName, gmId, returnValue);
            if (co1.MoveNext())
            {
                yield return co1;
            }

            if (returnValue.Value != 1)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var gmController = GameMasterManager.GetGM(gmId.Value);
            if (!password.Equals(gmController.DbData.Pwd))
            {
                msg.Reply((int)ErrorCodes.PasswordIncorrect);
                yield break;
            }
            gmId.Dispose();
            // update gm login info
            var lastTime = DateTime.FromBinary(gmController.DbData.LastTime);
            var now = DateTime.Now;
            if (lastTime.Year < now.Year || lastTime.Month < now.Month || lastTime.Day < now.Day)
            {
                gmController.DbData.LoginDay = 1;
            }
            else
            {
                ++gmController.DbData.LoginDay;
            }
            ++gmController.DbData.LoginTotal;
            gmController.DbData.LastTime = DateTime.Now.ToBinary();

            co1 = CoroutineFactory.NewSubroutine(gmController.SaveDb, co, returnValue);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            if (returnValue.Value != 1)
            {
                Logger.Error("update gm login info Error! GM name = " + gmController.DbData.Name);
            }
            returnValue.Dispose();
            msg.Response = new GMAccount
            {
                Id = gmController.DbData.Id,
                Name = gmController.DbData.Name,
                Priority = gmController.DbData.Priority
            };
            msg.Reply();

            Logger.Info("{0}: gm {1} called Login().", DateTime.Now, gmController.DbData.Name);
        }

        public IEnumerator GetPlayerDataByCharacterName(Coroutine co,
                                                        GameMasterService _this,
                                                        GetPlayerDataByCharacterNameInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var loginAgent = GameMasterServer.Instance.LoginAgent;
            var playerIdData = loginAgent.GetCharacterIdByName(0, msg.Request.Name);
            yield return playerIdData.SendAndWaitUntilDone(co);

            if (playerIdData.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }

            if (playerIdData.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(playerIdData.ErrorCode);
                yield break;
            }

            var charId = playerIdData.Response;
            var playerData1 = loginAgent.GetPlayerData(0, 0, charId);
            yield return playerData1.SendAndWaitUntilDone(co);

            if (playerData1.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }

            if (playerData1.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(playerData1.ErrorCode);
                yield break;
            }

            msg.Response = playerData1.Response;
            msg.Reply();

            Logger.Info("{0}: gm {1} called GetPlayerDataByName(), arg name = {2}.", DateTime.Now,
                gmController.DbData.Name, msg.Request.Name);
        }

        public IEnumerator GetPlayerDataByCharacterId(Coroutine co,
                                                      GameMasterService _this,
                                                      GetPlayerDataByCharacterIdInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var charId = msg.Request.Id;
            var loginAgent = GameMasterServer.Instance.LoginAgent;

            var playerData1 = loginAgent.GetPlayerData(0, 0, charId);
            yield return playerData1.SendAndWaitUntilDone(co);

            if (playerData1.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }

            if (playerData1.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(playerData1.ErrorCode);
                yield break;
            }

            msg.Response = playerData1.Response;
            msg.Reply();

            Logger.Info("{0}: gm {1} called GetPlayerDataByCharacterId(), arg id = {2}.", DateTime.Now,
                gmController.DbData.Name, charId);
        }

        public IEnumerator GetPlayerDataByPlayerName(Coroutine co,
                                                     GameMasterService _this,
                                                     GetPlayerDataByPlayerNameInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var loginAgent = GameMasterServer.Instance.LoginAgent;
            var playerIdData = loginAgent.GetPlayerIdByAccount(0, msg.Request.Name);
            yield return playerIdData.SendAndWaitUntilDone(co);

            if (playerIdData.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }

            if (playerIdData.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(playerIdData.ErrorCode);
                yield break;
            }

            var playerId = playerIdData.Response;
            var playerData1 = loginAgent.GetPlayerData(0, playerId, 0);
            yield return playerData1.SendAndWaitUntilDone(co);

            if (playerData1.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }

            if (playerData1.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(playerData1.ErrorCode);
                yield break;
            }

            msg.Response = playerData1.Response;
            msg.Reply();

            Logger.Info("{0}: gm {1} called GetPlayerDataByName(), arg name = {2}.", DateTime.Now,
                gmController.DbData.Name, msg.Request.Name);
        }

        public IEnumerator GetPlayerDataByPlayerId(Coroutine co,
                                                   GameMasterService _this,
                                                   GetPlayerDataByPlayerIdInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var playerId = msg.Request.Id;
            var loginAgent = GameMasterServer.Instance.LoginAgent;

            var playerData1 = loginAgent.GetPlayerData(0, playerId, 0);
            yield return playerData1.SendAndWaitUntilDone(co);

            if (playerData1.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }

            if (playerData1.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(playerData1.ErrorCode);
                yield break;
            }

            msg.Response = playerData1.Response;
            msg.Reply();

            Logger.Info("{0}: gm {1} called GetPlayerDataByCharacterId(), arg id = {2}.", DateTime.Now,
                gmController.DbData.Name, playerId);
        }

        public IEnumerator GetCharacterDataById(Coroutine co, GameMasterService _this, GetCharacterDataByIdInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var charId = msg.Request.CharacterId;

            GMCharacterDetailInfo charDetailInfo = null;

            // 先从logic的cache中获取一部分信息
            var logicAgent = GameMasterServer.Instance.LogicAgent;
            var playerData1 = logicAgent.GetCharacterData(charId, charId);
            yield return playerData1.SendAndWaitUntilDone(co);

            if (playerData1.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }

            if (playerData1.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(playerData1.ErrorCode);
                yield break;
            }

            charDetailInfo = playerData1.Response;

            var sceneAgent = GameMasterServer.Instance.SceneAgent;
            var playerData2 = sceneAgent.GetCharacterData(charId, charId);
            yield return playerData2.SendAndWaitUntilDone(co);

            if (playerData2.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }

            if (playerData2.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(playerData2.ErrorCode);
                yield break;
            }

            var charDataFromScene1 = playerData2.Response;
            charDetailInfo.Name = charDataFromScene1.Name;
            charDetailInfo.SceneId = charDataFromScene1.SceneId;
            charDetailInfo.SceneGuid = charDataFromScene1.SceneGuid;
            charDetailInfo.X = charDataFromScene1.X;
            charDetailInfo.Y = charDataFromScene1.Y;

            var loginAgent = GameMasterServer.Instance.LoginAgent;
            var playerData3 = loginAgent.GetTotleOnlineSeconds(charId, charId);
            yield return playerData3.SendAndWaitUntilDone(co);

            if (playerData3.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }

            if (playerData3.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(playerData3.ErrorCode);
                yield break;
            }

            charDetailInfo.TotleOlineTime = playerData3.Response;

            var chatAgent = GameMasterServer.Instance.ChatAgent;
            var chatData = chatAgent.GetSilenceState(charId, 0);
            yield return chatData.SendAndWaitUntilDone(co);

            if (chatData.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }

            if (chatData.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(chatData.ErrorCode);
                yield break;
            }

            charDetailInfo.SilenceMask = chatData.Response;

            msg.Response = charDetailInfo;
            msg.Reply();
        }

        public IEnumerator SendMailsById(Coroutine co, GameMasterService _this, SendMailsByIdInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var mailId = GameMasterServer.Instance.DB.GetNextId(co, (int)DataCategory.GameMasterMail,
                GameMasterServerControl.DBGmMailIdKey);
            yield return mailId;

            if (mailId.Status != DataStatus.Ok)
            {
                Logger.Error("Can not get mail id in func SendMailsById()!");
                yield break;
            }

            var mail = new GmMailData();
            mail.Id = mailId.Data;
            mail.Time = msg.Request.Time;
            mail.Title = msg.Request.Title;
            mail.Content = msg.Request.Content;
            mail.Gm = gmController.DbData.Name;
            mail.Items = msg.Request.Items;
            mail.Characters.AddRange(msg.Request.Ids.Items);

            gameMasterServerControl.mTimedMailData.AddData(mail);

            msg.Reply();

            Logger.Info("{0}: gm {1} called SendMailsById().", DateTime.Now, gmController.DbData.Name);
        }

        public IEnumerator SendMailsByName(Coroutine co, GameMasterService _this, SendMailsByNameInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            Logger.Info("{0}: gm {1} called SendMailsByName().", DateTime.Now,
                gmController.DbData.Name);
            var loginAgent = GameMasterServer.Instance.LoginAgent;
            var ids = new List<ulong>();
            foreach (var name in msg.Request.Names.Items)
            {
                var playerIdData = loginAgent.GetCharacterIdByName(0, name);
                yield return playerIdData.SendAndWaitUntilDone(co);

                if (playerIdData.State != MessageState.Reply)
                {
                    msg.Reply((int)ErrorCodes.Error_TimeOut);
                    yield break;
                }

                if (playerIdData.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply(playerIdData.ErrorCode);
                    yield break;
                }

                ids.Add(playerIdData.Response);

                yield return gameMasterServerControl.Wait(co, TimeSpan.FromMilliseconds(50));

                Logger.Info("{0}: arg name = {1}.", DateTime.Now, name);
            }

            var mailId = GameMasterServer.Instance.DB.GetNextId(co, (int)DataCategory.GameMasterMail,
                GameMasterServerControl.DBGmMailIdKey);
            yield return mailId;

            if (mailId.Status != DataStatus.Ok)
            {
                Logger.Error("Can not get mail id in func SendMailsById()!");
                yield break;
            }

            var mail = new GmMailData();
            mail.Id = mailId.Data;
            mail.Time = msg.Request.Time;
            mail.Title = msg.Request.Title;
            mail.Content = msg.Request.Content;
            mail.Gm = gmController.DbData.Name;
            mail.Items = msg.Request.Items;
            mail.Characters.AddRange(ids);

            gameMasterServerControl.mTimedMailData.AddData(mail);

            msg.Reply();
        }

        public IEnumerator SendMailsToServers(Coroutine co, GameMasterService _this, SendMailsToServersInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            msg.Reply();

            var mailId = GameMasterServer.Instance.DB.GetNextId(co, (int)DataCategory.GameMasterMail,
                GameMasterServerControl.DBGmMailIdKey);
            yield return mailId;

            if (mailId.Status != DataStatus.Ok)
            {
                Logger.Error("Can not get mail id in func SendMailsById()!");
                yield break;
            }

            var request = msg.Request;
            var serverIds = request.Servers;

            var mail = new GmMailData();
            mail.Id = mailId.Data;
            mail.Time = request.Time;
            mail.Title = request.Title;
            mail.Content = request.Content;
            mail.Gm = gmController.DbData.Name;
            mail.Items = request.Items;
            if (serverIds.Items.Count > 0)
            {
                mail.Servers.AddRange(serverIds.Items);
            }
            else
            {
                for (uint i = 0; i < Constants.ServerCount; i++)
                {
                    mail.Servers.Add(i);
                }
            }

            gameMasterServerControl.mTimedMailData.AddData(mail);

            Logger.Info("{0}: gm {1} called SendMailsToServers().", DateTime.Now, gmController.DbData.Name);
        }

        public IEnumerator GetWaitingMails(Coroutine co, GameMasterService _this, GetWaitingMailsInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            msg.Response = gameMasterServerControl.mTimedMailData.Mails;
            msg.Reply();
        }

        public IEnumerator DelWaitingMails(Coroutine co, GameMasterService _this, DelWaitingMailsInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var mails = gameMasterServerControl.mTimedMailData.Mails.Mails;
            foreach (var id in msg.Request.Ids.Items)
            {
                mails.Remove(mails.First(m => m.Id == id));
            }
            if (mails.Count > 0)
            {
                gameMasterServerControl.mTimedMailData.StartTimer();
            }
            gameMasterServerControl.mTimedMailData.Mails.NextIdx = 0;

            msg.Response = gameMasterServerControl.mTimedMailData.Mails;
            msg.Reply();

            var co1 = CoroutineFactory.NewSubroutine(gameMasterServerControl.SaveGmMails, co);
            if (co1.MoveNext())
            {
                yield return co1;
            }
        }

        public IEnumerator KickCharacter(Coroutine co, GameMasterService _this, KickCharacterInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            Logger.Info("{0}: gm {1} called KickCharacter(), arg id = {2}, name = {3}.", DateTime.Now,
                gmController.DbData.Name, msg.Request.CharacterId, msg.Request.Name);

            var kickMsg = GameMasterServer.Instance.LoginAgent.GMKickCharacter(0, msg.Request.CharacterId,
                msg.Request.Name);
            yield return kickMsg.SendAndWaitUntilDone(co);

            if (kickMsg.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }
            msg.Reply(kickMsg.ErrorCode);
        }

        public IEnumerator Broadcast(Coroutine co, GameMasterService _this, BroadcastInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            Logger.Info("{0}: gm {1} called Broadcast().", DateTime.Now, gmController.DbData.Name);

            var broadcastId = GameMasterServer.Instance.DB.GetNextId(co, (int)DataCategory.GameMasterBroadcast,
                GameMasterServerControl.DBGmBroadcastIdKey);
            yield return broadcastId;

            if (broadcastId.Status != DataStatus.Ok)
            {
                Logger.Error("Can not get mail id in func SendMailsById()!");
                yield break;
            }

            var request = msg.Request;
            var serverIds = request.Servers.Items;

            var broadcast = new GmBroadcastData();
            broadcast.Id = broadcastId.Data;
            broadcast.Time = request.Time;
            broadcast.Content = request.Content;
            broadcast.Gm = gmController.DbData.Name;
            if (serverIds.Count > 0)
            {
                broadcast.Servers.AddRange(serverIds);
            }
            else
            {
                for (uint i = 0; i < Constants.ServerCount; i++)
                {
                    broadcast.Servers.Add(i);
                }
            }

            gameMasterServerControl.mTimedBroadcasts.AddData(broadcast);

            msg.Reply();
        }

        public IEnumerator GetWaitingBroadcasts(Coroutine co, GameMasterService _this, GetWaitingBroadcastsInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            msg.Response = gameMasterServerControl.mTimedBroadcasts.Broadcasts;
            msg.Reply();
        }

        public IEnumerator DelWaitingBroadcasts(Coroutine co, GameMasterService _this, DelWaitingBroadcastsInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var broadcasts = gameMasterServerControl.mTimedBroadcasts.Broadcasts;
            var broadcastsInner = broadcasts.Broadcasts;

            foreach (var id in msg.Request.Ids.Items)
            {
                broadcastsInner.Remove(broadcastsInner.First(m => m.Id == id));
            }
            if (broadcastsInner.Count > 0)
            {
                gameMasterServerControl.mTimedBroadcasts.StartTimer();
            }
            broadcasts.NextIdx = 0;

            msg.Response = broadcasts;
            msg.Reply();

            var co1 = CoroutineFactory.NewSubroutine(gameMasterServerControl.SaveGmBroadcasts, co);
            if (co1.MoveNext())
            {
                yield return co1;
            }
        }

        public string Encrypt(string password)
        {
            byte[] salt = { 23, 21, 32, 33, 46, 59, 60, 74 };
            var rfc = new Rfc2898DeriveBytes("pwd", salt, 23);

            var key = rfc.GetBytes(16);
            var iv = rfc.GetBytes(8);

            var rc2 = new RC2CryptoServiceProvider { Key = key, IV = iv };

            var plaintext = Encoding.UTF8.GetBytes(password);
            string pass;
            using (var ms = new MemoryStream())
            {
                var cs = new CryptoStream(
                    ms, rc2.CreateEncryptor(), CryptoStreamMode.Write);

                cs.Write(plaintext, 0, plaintext.Length);
                cs.Close();
                var encrypted = ms.ToArray();
                pass = BitConverter.ToString(encrypted, 0);
            }
            return pass;
        }

        public IEnumerator CheckAdminAccount(GameMasterServerControl _this,
                                             Coroutine co,
                                             string gmAccount,
                                             string password)
        {
            var gmId = AsyncReturnValue<ulong>.Create();
            var returnValue = AsyncReturnValue<int>.Create();
            var co1 = CoroutineFactory.NewSubroutine(GameMasterManager.LoadGM, co, gmAccount, gmId, returnValue);
            gmId.Dispose();
            if (co1.MoveNext())
            {
                yield return co1;
            }
            var retValue = returnValue.Value;
            if (retValue != 1)
            {
                // admin 账号不存在，创建 admin 账号
                co1 = CoroutineFactory.NewSubroutine(GameMasterManager.CreateGmAccount, co, gmAccount, password,
                    (int)eGmPriority.Admin, returnValue);
                if (co1.MoveNext())
                {
                    yield return co1;
                }
                retValue = returnValue.Value;
                returnValue.Dispose();
                if (retValue != 1)
                {
                    Logger.Fatal("Create GM 'admin' Count Failed!!!");
                }
            }
            else
            {
                returnValue.Dispose();
            }
        }

        public IEnumerator LoadGmMails(GameMasterServerControl _this, Coroutine co)
        {
            var dbMail = GameMasterServer.Instance.DB.Get<GmMailList>(co, DataCategory.GameMasterMail,
                GameMasterServerControl.DBGmMailKey);
            yield return dbMail;
            if (dbMail.Data == null)
            {
                _this.mTimedMailData.Mails = new GmMailList();
                yield break;
            }
            _this.mTimedMailData.Mails = dbMail.Data;
            if (_this.mTimedMailData.Mails.Mails.Count > 0)
            {
                _this.mTimedMailData.StartTimer();
            }
        }

        public IEnumerator SaveGmMails(GameMasterServerControl _this, Coroutine co)
        {
            var result = GameMasterServer.Instance.DB.Set(co, DataCategory.GameMasterMail,
                GameMasterServerControl.DBGmMailKey,
                _this.mTimedMailData.Mails);
            yield return result;
            if (result.Status != DataStatus.Ok)
            {
                Logger.Error("SaveGmMails() Failed! With status = {0}", result.Status);
            }
        }

        public IEnumerator LoadGmBroadcasts(GameMasterServerControl _this, Coroutine co)
        {
            var dbBroadcast = GameMasterServer.Instance.DB.Get<GmBroadcastList>(co,
                DataCategory.GameMasterBroadcast,
                GameMasterServerControl.DBGmBroadcastKey);
            yield return dbBroadcast;
            if (dbBroadcast.Data == null)
            {
                _this.mTimedBroadcasts.Broadcasts = new GmBroadcastList();
                yield break;
            }
            _this.mTimedBroadcasts.Broadcasts = dbBroadcast.Data;
            if (_this.mTimedBroadcasts.Broadcasts.Broadcasts.Count > 0)
            {
                _this.mTimedBroadcasts.StartTimer();
            }
        }

        public IEnumerator SaveGmBroadcasts(GameMasterServerControl _this, Coroutine co)
        {
            var result = GameMasterServer.Instance.DB.Set(co, DataCategory.GameMasterBroadcast,
                GameMasterServerControl.DBGmBroadcastKey,
                _this.mTimedBroadcasts.Broadcasts);
            yield return result;
            if (result.Status != DataStatus.Ok)
            {
                Logger.Error("SaveGmBroadcasts() Failed! With status = {0}", result.Status);
            }
        }

        public IEnumerator DoSendMailToCharactersCoroutine(GameMasterServerControl _this, Coroutine co, GmMailData mail)
        {
            foreach (var id in mail.Characters)
            {
                var sendMailMsg = GameMasterServer.Instance.LogicAgent.SendMailToCharacter(id, mail.Title, mail.Content,
                    mail.Items, mail.IsFanKui);
                yield return sendMailMsg.SendAndWaitUntilDone(co);
                yield return _this.Wait(co, TimeSpan.FromMilliseconds(50));

                Logger.Info("{0}: gm {1} DoBroadcastCoroutine(), arg CharacterId = {2}.", DateTime.Now, mail.Gm, id);
            }
        }

        public IEnumerator DoSendMailToServersCoroutine(GameMasterServerControl _this, Coroutine co, GmMailData mail)
        {
            var db = GameMasterServer.Instance.DB;

            foreach (var serverId in mail.Servers)
            {
                var mailId = db.GetNextId(co, (int)DataCategory.GameMasterMail, serverId.ToString());
                yield return mailId;

                if (mailId.Status != DataStatus.Ok)
                {
                    Logger.Error("Can not get mail id in func DoSendMailForServersCoroutine() for serverId = {0}!",
                        serverId);
                    continue;
                }

                var mailOne = new DBMail_One();
                mailOne.Name = mail.Title;
                mailOne.Text = mail.Content;
                mailOne.Items.AddRange(mail.Items.Data.Select(item => new ItemBaseData
                {
                    ItemId = item.Key,
                    Count = item.Value
                }));
                mailOne.State = 0;
                mailOne.StartTime = DateTime.Now.ToBinary();
                mailOne.OverTime = mail.OverTime;//DateTime.Now.AddDays(15).ToBinary();
                mailOne.IsNew = mail.IsNew;
                mailOne.Guid = mailId.Data;
                var retSet = GameMasterServer.Instance.DB.Set(co, DataCategory.GameMasterMail,
                    serverId + ":" + mailOne.Guid, mailOne);
                yield return retSet;

                if (retSet.Status != DataStatus.Ok)
                {
                    Logger.Error("Save DBMail_List DB Error!");
                    yield break;
                }

                var sendMailMsg = GameMasterServer.Instance.LogicAgent.SendMailToServer(serverId, mailId.Data);
                yield return sendMailMsg.SendAndWaitUntilDone(co);

                Logger.Info("{0}: send mail id = {1} to serverId = {2}.", DateTime.Now, mailId.Data, serverId);

                yield return _this.Wait(co, TimeSpan.FromSeconds(2));
            }
        }

        public IEnumerator DoBroadcastCoroutine(GameMasterServerControl _this, Coroutine co, GmBroadcastData broadcast)
        {
            foreach (var serverId in broadcast.Servers)
            {
                GameMasterServer.Instance.ChatAgent.BroadcastWorldMessage(serverId,
                    (int)eChatChannel.System, 0,
                    string.Empty, new ChatMessageContent { Content = broadcast.Content });
                yield return _this.Wait(co, TimeSpan.FromMilliseconds(50));

                Logger.Info("{0}: gm {1} DoBroadcastCoroutine(), arg serverId = {2}.", DateTime.Now, broadcast.Gm,
                    serverId);
            }
        }

        public IEnumerator Silence(Coroutine co, GameMasterService _this, SilenceInMessage msg)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var gmController = gameMasterServerControl.CheckLogin(msg);
            if (gmController == null)
            {
                msg.Reply((int)ErrorCodes.Error_GM_Id_Not_Exist);
                yield break;
            }

            var characterId = msg.Request.CharacterId;
            var mask = msg.Request.Mask;

            var msg1 = GameMasterServer.Instance.ChatAgent.Silence(characterId, mask);
            yield return msg1.SendAndWaitUntilDone(co);
            if (msg1.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }

            var msg2 = GameMasterServer.Instance.SceneAgent.Silence(characterId, mask);
            yield return msg2.SendAndWaitUntilDone(co);
            if (msg2.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                yield break;
            }

            msg.Reply();
        }

        public IEnumerator LockAccount(Coroutine co, GameMasterService _this, LockAccountInMessage msg)
        {
            var playerId = msg.Request.PlayerId;
            var endTime = msg.Request.EndTime;
            var msg1 = GameMasterServer.Instance.LoginAgent.LockAccount(0, playerId, endTime);
            yield return msg1.SendAndWaitUntilDone(co);
            msg.Reply();
        }

        public IEnumerator ProcessRequestAsync(Coroutine co, GameMasterServerControl _this, HttpListenerContext context)
        {
            var subroutine = CoroutineFactory.NewSubroutine(GameMasterServerControlHelper.ProcessRequestAsync, co, _this, context);
            if (subroutine.MoveNext())
            {
                yield return subroutine;
            }
            yield break;
        }
    }

    public interface IStaticGameMasterServerControl
    {
        IEnumerator CheckAdminAccount(GameMasterServerControl _this, Coroutine co, string gmAccount, string password);
        IEnumerator DoBroadcastCoroutine(GameMasterServerControl _this, Coroutine co, GmBroadcastData broadcast);
        IEnumerator DoSendMailToCharactersCoroutine(GameMasterServerControl _this, Coroutine co, GmMailData mail);
        IEnumerator DoSendMailToServersCoroutine(GameMasterServerControl _this, Coroutine co, GmMailData mail);
        string Encrypt(string password);
        IEnumerator LoadGmBroadcasts(GameMasterServerControl _this, Coroutine co);
        IEnumerator LoadGmMails(GameMasterServerControl _this, Coroutine co);
        IEnumerator SaveGmBroadcasts(GameMasterServerControl _this, Coroutine co);
        IEnumerator SaveGmMails(GameMasterServerControl _this, Coroutine co);
        IEnumerator ProcessRequestAsync(Coroutine co, GameMasterServerControl _this, HttpListenerContext context);
    }

    public class GameMasterServerControl : GameMasterService
    {
        public const string DBGmBroadcastIdKey = "broadcast_id";
        public const string DBGmBroadcastKey = "broadcast";
        public const string DBGmMailIdKey = "mail_id";
        public const string DBGmMailKey = "mail";
        //心跳频率
        public const float Frequence = 20.0f;
        public static GiftCodeDbConnection GiftCodeDb;
        public static OldPlayersDbConnection OldPlayersDb;

        public static GameMasterServerControl Instance;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //每帧时长
        public const float Performance = 1.0f / Frequence;
        public static TimeManager Timer = new TimeManager();
        private long tickTime = 0;

        public GameMasterServerControl()
        {
            Instance = this;

            GameMasterServer.Instance.UpdateManager.InitStaticImpl(typeof(GameMasterServerControl),
                typeof(GameMasterServerControlDefaultImpl), o => { SetServiceImpl((IGameMasterService)o); });

            GameMasterServer.Instance.UpdateManager.InitStaticImpl(typeof(GameMasterProxy),
                typeof(GameMasterProxyDefaultImpl), o => { SetProxyImpl((IGameMasterCharacterProxy)o); });
        }

        public TimedBroadcastData mTimedBroadcasts = new TimedBroadcastData();
        public TimedMailData mTimedMailData = new TimedMailData();

        private void Broadcast()
        {
            var broadcasts = mTimedBroadcasts.Broadcasts;
            var broadcast = broadcasts.Broadcasts[broadcasts.NextIdx];
            DoBroadcast(broadcast);
            ++broadcasts.NextIdx;
            mTimedBroadcasts.OnSent();
        }

        /// 检查 admin 账号是否存在
        public IEnumerator CheckAdminAccount(Coroutine co, string gmAccount, string password)
        {
            return ((IStaticGameMasterServerControl)mImpl).CheckAdminAccount(this, co, gmAccount, password);
        }

        public GMAccountController CheckLogin(InMessage msg)
        {
            return GameMasterManager.GetGM(msg.CharacterId);
        }

        private void DoBroadcast(GmBroadcastData broadcast)
        {
            CoroutineFactory.NewCoroutine(DoBroadcastCoroutine, broadcast).MoveNext();
        }

        private IEnumerator DoBroadcastCoroutine(Coroutine co, GmBroadcastData broadcast)
        {
            return ((IStaticGameMasterServerControl)mImpl).DoBroadcastCoroutine(this, co, broadcast);
        }

        public void DoSendMail(GmMailData mail)
        {
            if (mail.Characters.Count > 0)
            {
                CoroutineFactory.NewCoroutine(DoSendMailToCharactersCoroutine, mail).MoveNext();
            }
            else
            {
                CoroutineFactory.NewCoroutine(DoSendMailToServersCoroutine, mail).MoveNext();
            }
        }

        public IEnumerator DoSendMailToCharactersCoroutine(Coroutine co, GmMailData mail)
        {
            return ((IStaticGameMasterServerControl)mImpl).DoSendMailToCharactersCoroutine(this, co, mail);
        }

        public IEnumerator DoSendMailToServersCoroutine(Coroutine co, GmMailData mail)
        {
            return ((IStaticGameMasterServerControl)mImpl).DoSendMailToServersCoroutine(this, co, mail);
        }

        public static string Encrypt(string password)
        {
            return ((IStaticGameMasterServerControl)mImpl).Encrypt(password);
        }

        public static ulong GetUniqueNumber()
        {
            var buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToUInt64(buffer, 0);
        }

        public void Init()
        {
            // 检查 admin 账号是否存在
            CoroutineFactory.NewCoroutine(CheckAdminAccount, "admin", "E1-2F-B9-D5-84-57-5E-C8").MoveNext();

            // 检查 http 账号是否存在
            CoroutineFactory.NewCoroutine(CheckAdminAccount, "wadmin", "11-CC-67-5F-54-21-8A-95-80-9B-4E-30-8B-18-3B-CD")
                .MoveNext();

            // 恢复数据库中尚未发出的邮件
            CoroutineFactory.NewCoroutine(LoadGmMails).MoveNext();

            // 恢复数据库中尚未发出的系统公告
            CoroutineFactory.NewCoroutine(LoadGmBroadcasts).MoveNext();
        }

        /// 从数据库中读取 GmBroadcastList
        public IEnumerator LoadGmBroadcasts(Coroutine co)
        {
            return ((IStaticGameMasterServerControl)mImpl).LoadGmBroadcasts(this, co);
        }

        /// 从数据库中读取 GmMailList
        public IEnumerator LoadGmMails(Coroutine co)
        {
            return ((IStaticGameMasterServerControl)mImpl).LoadGmMails(this, co);
        }

        public override GameMasterCharacterProxy NewCharacterIn(ulong characterId, ulong clientId)
        {
            return new GameMasterProxy(this, characterId, clientId);
        }

        public override IEnumerator OnClientConnected(Coroutine co,
                                                      string target,
                                                      ulong clientId,
                                                      ulong characterId,
                                                      uint packId)
        {
            //var proxy = NewCharacterIn(characterId, clientId);
            //if (proxy != null)
            //{
            //    Proxys[clientId] = proxy;
            //    return mProxyImpl.OnConnected(co, proxy, packId);
            //}
            return null;
        }

        public override IEnumerator OnClientLost(Coroutine co,
                                                 string target,
                                                 ulong clientId,
                                                 ulong characterId,
                                                 uint packId)
        {
            //GameMasterCharacterProxy proxy;
            //if (Proxys.TryGetValue(clientId, out proxy))
            //{
            //    var co1 = CoroutineFactory.NewSubroutine(mProxyImpl.OnLost, co, proxy, packId);
            //    if (co1.MoveNext())
            //    {
            //        yield return co1;
            //    }
            //    Proxys.Remove(clientId);
            //}
            return null;
        }

        public override void OnException(Exception ex)
        {
            Logger.Error(ex, "Network error");
        }

        public override IEnumerator OnServerStart(Coroutine co)
        {
            return mImpl.OnServerStart(co, this);
        }

        public override IEnumerator OnServerStop(Coroutine co)
        {
            return mImpl.OnServerStop(co, this);
        }

        public override IEnumerator PerformenceTest(Coroutine co, ServerClient client, ServiceDesc desc)
        {
            client.SendMessage(desc);
            return null;
        }

        /// 把 GmBroadcastList 存到数据库中
        public IEnumerator SaveGmBroadcasts(Coroutine co)
        {
            return ((IStaticGameMasterServerControl)mImpl).SaveGmBroadcasts(this, co);
        }

        /// 把 GmMailList 存到数据库中
        public IEnumerator SaveGmMails(Coroutine co)
        {
            return ((IStaticGameMasterServerControl)mImpl).SaveGmMails(this, co);
        }

        public IEnumerator ProcessRequestAsync(Coroutine co, HttpListenerContext context)
        {
            return ((IStaticGameMasterServerControl)mImpl).ProcessRequestAsync(co, this, context);
        }

        public void SendMail()
        {
            var mails = mTimedMailData.Mails;
            var mail = mails.Mails[mails.NextIdx];
            DoSendMail(mail);
            ++mails.NextIdx;
            mTimedMailData.OnSent();
        }

        public void Status(ConcurrentDictionary<string, string> dict)
        {

            dict.TryAdd("_Listening", Listening.ToString());
            dict.TryAdd("Started", Started.ToString());
            dict.TryAdd("TickTime", tickTime.ToString());
            //dict.TryAdd("ByteReceivedPerSecond", ByteReceivedPerSecond.ToString());
            //dict.TryAdd("ByteSendPerSecond", ByteSendPerSecond.ToString());
            //dict.TryAdd("MessageReceivedPerSecond", MessageReceivedPerSecond.ToString());
            //dict.TryAdd("MessageSendPerSecond", MessageSendPerSecond.ToString());
            //dict.TryAdd("ConnectionCount", ConnectionCount.ToString());

            //foreach (var agent in GameMasterServer.Instance.Agents.ToArray())
            //{
            //    dict.TryAdd(agent.Key + " Latency", agent.Value.Latency.ToString());
            //    dict.TryAdd(agent.Key + " ByteReceivedPerSecond", agent.Value.ByteReceivedPerSecond.ToString());
            //    dict.TryAdd(agent.Key + " ByteSendPerSecond", agent.Value.ByteSendPerSecond.ToString());
            //    dict.TryAdd(agent.Key + " MessageReceivedPerSecond", agent.Value.MessageReceivedPerSecond.ToString());
            //    dict.TryAdd(agent.Key + " MessageSendPerSecond", agent.Value.MessageSendPerSecond.ToString());
            //}
        }

        public override IEnumerator Tick(Coroutine coroutine)
        {
            tickTime++;
            return null;
        }

        public class TimedMailData
        {
            public GmMailList Mails;
            public Trigger TimerObj;

            public void AddData(GmMailData mail)
            {
                // 如果已经到时间了，就发出去
                if (DateTime.FromBinary(mail.Time) <= DateTime.Now)
                {
                    Instance.DoSendMail(mail);
                    return;
                }

                Mails.Mails.Add(mail);
                Mails.Mails.Sort(delegate(GmMailData l, GmMailData r)
                {
                    if (l.Time < r.Time)
                    {
                        return -1;
                    }
                    return l.Time > r.Time ? 1 : 0;
                });

                // 每加一个新的 mail 任务，就保存一次
                CoroutineFactory.NewCoroutine(Instance.SaveGmMails).MoveNext();

                StartTimer();
            }

            public void OnSent()
            {
                Mails.Mails.RemoveRange(0, Mails.NextIdx);
                Mails.NextIdx = 0;
                CoroutineFactory.NewCoroutine(Instance.SaveGmMails).MoveNext();

                if (Mails.Mails.Count > Mails.NextIdx)
                {
                    Timer.ChangeTime(ref TimerObj, DateTime.FromBinary(Mails.Mails[Mails.NextIdx].Time));
                }
                else
                {
                    Timer.DeleteTrigger(TimerObj);
                    TimerObj = null;
                }
            }

            public void StartTimer()
            {
                var firstMailTime = DateTime.FromBinary(Mails.Mails[0].Time);
                if (TimerObj == null || TimerObj.T == null)
                {
                    TimerObj = Timer.CreateTrigger(firstMailTime, Instance.SendMail);
                }
                else if (TimerObj.Time > firstMailTime)
                {
                    Timer.ChangeTime(ref TimerObj, firstMailTime);
                }
            }
        }

        public class TimedBroadcastData
        {
            public GmBroadcastList Broadcasts;
            public Trigger TimerObj;

            public void AddData(GmBroadcastData broadcast)
            {
                // 如果已经到时间了，就发出去
                if (broadcast.Time <= DateTime.Now.ToBinary())
                {
                    Instance.DoBroadcast(broadcast);
                    return;
                }

                Broadcasts.Broadcasts.Add(broadcast);
                Broadcasts.Broadcasts.Sort(delegate(GmBroadcastData l, GmBroadcastData r)
                {
                    if (l.Time < r.Time)
                    {
                        return -1;
                    }
                    return l.Time > r.Time ? 1 : 0;
                });

                // 每加一个新的 mail 任务，就保存一次
                CoroutineFactory.NewCoroutine(Instance.SaveGmBroadcasts).MoveNext();

                StartTimer();
            }

            public void OnSent()
            {
                Broadcasts.Broadcasts.RemoveRange(0, Broadcasts.NextIdx);
                Broadcasts.NextIdx = 0;
                CoroutineFactory.NewCoroutine(Instance.SaveGmBroadcasts).MoveNext();

                if (Broadcasts.Broadcasts.Count > Broadcasts.NextIdx)
                {
                    Timer.ChangeTime(ref TimerObj, DateTime.FromBinary(Broadcasts.Broadcasts[Broadcasts.NextIdx].Time));
                }
                else
                {
                    Timer.DeleteTrigger(TimerObj);
                    TimerObj = null;
                }
            }

            public void StartTimer()
            {
                var firstBroadcastTime = DateTime.FromBinary(Broadcasts.Broadcasts[0].Time);
                if (TimerObj == null || TimerObj.T == null)
                {
                    TimerObj = Timer.CreateTrigger(firstBroadcastTime, Instance.Broadcast);
                }
                else if (TimerObj.Time > firstBroadcastTime)
                {
                    Timer.ChangeTime(ref TimerObj, firstBroadcastTime);
                }
            }
        }
    }
}