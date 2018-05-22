#region using

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Database;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;
using TeamServerService;

#endregion

namespace Team
{
    public class TeamServerControlDefaultImpl : ITeamService, IStaticTeamServerControl, ITickable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerator SSLeaveTeam(Coroutine coroutine, TeamService _this, SSLeaveTeamInMessage msg)
        {
            var team = TeamManager.GetCharacterTeam(msg.CharacterId);
            if (null != team)
            {
                var teamId = team.team.TeamId;
                TeamManager.Leave(msg.CharacterId, teamId);
                // 通知客户端
                TeamManager.SendMessage(msg.CharacterId, 10, teamId, 0);
            }
            return null;
        }

        public IEnumerator OnConnected(Coroutine coroutine, TeamCharacterProxy charProxy, AsyncReturnValue<bool> ret)
        {
            ret.Value = true;
            var proxy = (TeamProxy) charProxy;
            TeamManager.OnLine(proxy.CharacterId);
            QueueManager.OnLine(proxy.CharacterId);
            AutoMatchManager.OnLine(proxy.CharacterId);

            ((TeamProxy) charProxy).Connected = true;
            proxy.SendGroupMessage(GroupShop.DbData.Notifys);
            yield break;
        }

        public IEnumerator UpdateServer(Coroutine coroutine, TeamService _this, UpdateServerInMessage msg)
        {
            TeamServer.Instance.UpdateManager.Update();
            return null;
        }
        public IEnumerator PlayerHoldLode(Coroutine coroutine,
            TeamService _this,
            PlayerHoldLodeInMessage msg)
        {//玩家占领一个包含矿点的场景
            {//逻辑 
                var resultCode = ServerLodeManagerManager.OnPlayerHoldLode(msg.Request.ServerId,msg.Request.AllianceId,msg.Request.SceneId);
                if(resultCode != (int)ErrorCodes.OK)
                {
                    msg.Reply((int)resultCode);
                    yield break;
                }
            }
            MsgSceneLode tmp = new MsgSceneLode();
            tmp.SceneId = msg.Request.SceneId;
            tmp.TeamId = msg.Request.AllianceId;
            tmp.TeamName = ServerAllianceManager.GetAllianceName(tmp.TeamId);
            //tmp.LodeList = msg.Response.LodeList;
            ServerLodeManagerManager.ApplyHoldLode(msg.Request.ServerId, msg.Request.SceneId, ref tmp);
            msg.Response = tmp;
            msg.Reply();

            yield break;
        }
        public IEnumerator PlayerCollectLode(Coroutine coroutine,
            TeamService _this,
            PlayerCollectLodeInMessage msg)
        {
            {//逻辑 
                var resultCode = ServerLodeManagerManager.OnPlayerCollectLode(msg.Request.ServerId,msg.Request.CharacterId, msg.Request.AllianceId, msg.Request.SceneId,msg.Request.LodeId,msg.Request.AddScore,msg.Request.BaseData);
                if (resultCode != (int)ErrorCodes.OK)
                {
                    msg.Reply((int)resultCode);
                    yield break;
                }
            }
            MsgSceneLode tmp = new MsgSceneLode();
            ServerLodeManagerManager.ApplyHoldLode(msg.Request.ServerId, msg.Request.SceneId, ref tmp);
            msg.Response = tmp;
            msg.Reply();
            if (tmp.TeamId > 0)
            {
//战盟收益
                Dictionary<int, int> dicRes = new Dictionary<int, int>();
                var tb = Table.GetLode(msg.Request.LodeId);
                if (tb != null)
                {
                    if (tb.AllianceRes > 0)
                    {
                        dicRes.Add(0, tb.AllianceRes);
                    }
                    for (int i = 0; i < tb.AllianceOutput.Length && i < tb.AllianceOutputNum.Length; i++)
                    {
                        if (tb.AllianceOutput[i] > 0)
                        {
                            dicRes.Add(tb.AllianceOutput[i], tb.AllianceOutputNum[i]);
                        }
                    }
                }
                ServerAllianceManager.OnAllianceAddRes(tmp.TeamId, dicRes);
            }

            //if (msg.Request.AllianceId > 0)
            //{//增加战盟贡献
            //    //msg.Request.MeritPoint
            //    var alliance = ServerAllianceManager.GetAllianceByServer(msg.Request.ServerId);
            //    if (alliance != null)
            //    {
            //        var member = alliance.GetCharacterData(msg.CharacterId);
            //        if (member != null)
            //            member.MeritPoint += msg.Request.MeritPoint;
            //    }
            //}
        }

        public IEnumerator SSAddAllianceContribution(Coroutine coroutine,
            TeamService _this,
            SSAddAllianceContributionInMessage msg)
        {

            if (msg.Request.AllianceId > 0)
            {//增加战盟贡献
                //msg.Request.MeritPoint
                var alliance = ServerAllianceManager.GetAllianceByServer(msg.Request.ServerId);
                if (alliance != null)
                {
                    var member = alliance.GetCharacterData(msg.CharacterId);
                    if (member != null)
                        member.MeritPoint += msg.Request.Contribution;
                }
            }
            yield break;
        }
        public IEnumerator ApplyHoldLode(Coroutine coroutine,
            TeamService _this,
            ApplyHoldLodeInMessage msg)
        {//服务器请求
            //msg.Response
            MsgSceneLode tmp = new MsgSceneLode();
            ServerLodeManagerManager.ApplyHoldLode(msg.Request.ServerId, msg.Request.SceneId, ref tmp);
            msg.Response = tmp;
            msg.Reply();
            yield break;
        }
        
         public IEnumerator SSSyncTeamMemberLevelChange(Coroutine coroutine,
            TeamService _this,
            SSSyncTeamMemberLevelChangeInMessage msg)
        {//服务器请求
            //msg.Response
           TeamManager.OnCharacterLevelUp(msg.Request.CharacterId,msg.Request.TeamId,msg.Request.Reborn,msg.Request.Level);
            yield break;
        }


         public IEnumerator NodifyModifyPlayerName(Coroutine coroutine, TeamService _this, NodifyModifyPlayerNameInMessage msg)
         {
             TeamManager.OnCharacterChangeName(msg.Request.CharacterId, msg.Request.TeamId, msg.Request.ModifyName);
             yield break;
         }
         public IEnumerator NodifyModifyAllianceMemberName(Coroutine coroutine, TeamService _this, NodifyModifyAllianceMemberNameInMessage msg)
         {
             var serverAlliance = ServerAllianceManager.GetAllianceByServer(msg.Request.ServerId);
             if (serverAlliance == null)
             {
                 //msg.Reply((int)ErrorCodes.ServerID);
                 yield break;
             }
             var server = serverAlliance.GetServerData(msg.Request.ServerId);
             if (server == null)
             {
                 //msg.Reply((int)ErrorCodes.ServerID);
                 yield break;
             }
             var character = serverAlliance.GetCharacterData(msg.Request.CharacterId);
            
             if (character == null)
             {
                 //msg.Reply((int)ErrorCodes.Unline);
                 yield break;
             }
             character.Name = msg.Request.ModifyName;
             //msg.Reply();
         }
        public IEnumerator SSNotifyCharacterOnConnet(Coroutine coroutine,
                                                     TeamService _this,
                                                     SSNotifyCharacterOnConnetInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            var clientId = msg.Request.ClientId;
            var proxy = new TeamProxy(_this, characterId, clientId);
            _this.Proxys[characterId] = proxy;
            var ret = AsyncReturnValue<bool>.Create();
            var subCo = CoroutineFactory.NewSubroutine(OnConnected, coroutine, proxy, ret);
            if (subCo.MoveNext())
            {
                yield return subCo;
            }
            var isOk = ret.Value;
            ret.Dispose();
            if (isOk)
            {
                msg.Reply((int) ErrorCodes.OK);
            }
            else
            {
                msg.Reply((int) ErrorCodes.ConnectFail);
            }
        }

        public IEnumerator SSApplyFieldActivityReward(Coroutine coroutine,
            TeamService _this,
            SSApplyFieldActivityRewardInMessage msg)
        {

            LodeManager mgr;
            if (false == ServerLodeManagerManager.Servers.TryGetValue(msg.Request.ServerId, out mgr))
            {
                msg.Reply((int)ErrorCodes.ServerID);
            }
            msg.Reply((int)mgr.OnPlayerApplyMissionReward(msg.Request.AllianceId, msg.Request.CharacterId, msg.Request.MissionId, msg.Request.Score, msg.Request.AddScore,msg.Request.Data));
            yield break;
        }

        public IEnumerator BSNotifyCharacterOnLost(Coroutine coroutine,
                                                   TeamService _this,
                                                   BSNotifyCharacterOnLostInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            TeamCharacterProxy charProxy;
            if (!_this.Proxys.TryGetValue(characterId, out charProxy))
            {
                yield break;
            }
            var proxy = (TeamProxy) charProxy;
            TeamManager.OnLost(proxy.CharacterId);
            QueueManager.OnLost(proxy.CharacterId);
            ServerAllianceManager.OnLost(proxy.CharacterId);
            AutoMatchManager.OnLost(proxy.CharacterId);
            proxy.Connected = false;
        }

        public IEnumerator GMCommand(Coroutine co, TeamService _this, GMCommandInMessage msg)
        {
            var request = msg.Request;
            var characterId = msg.CharacterId;
            var commands = request.Commonds.Items;
            var errs = msg.Response.Items;
            var err = new AsyncReturnValue<ErrorCodes>();
            foreach (var command in commands)
            {
                var co1 = CoroutineFactory.NewSubroutine(Utility.GmCommand, co, characterId, command, err);
                if (co1.MoveNext())
                {
                    yield return co1;
                }
                errs.Add((int) err.Value);
            }
            err.Dispose();
            msg.Reply();
        }

        public IEnumerator OnServerStart(Coroutine coroutine, TeamService _this)
        {
            var teamServerControl = (TeamServerControl) _this;
            //Thread.Sleep(GlobalVariable.WaitToConnectTimespan + 2000);

            //拍卖行功能屏蔽了.
            //ServerAuctionManager.instance.Init();
            TeamServer.Instance.Start(_this);
            ServerAllianceManager.Init();
            ExchangeManager.Init();
            GroupShop.Init();
            AllianceWarManager.Init();
            ServerLodeManagerManager.Init();
            teamServerControl.Init();
            var __this = (TeamServerControl)_this;
            TeamManager.WebRequestManager = new RequestManager(__this);
            teamServerControl.mTimedTaskManager.Init(TeamServer.Instance.DB, null, DataCategory.Team,
                (int) TeamServer.Instance.Id, Logger,
                teamServerControl.ApplyTasks);
            TeamServer.Instance.IsReadyToEnter = true;
            UnionBattleManager.Init();

            _this.TickDuration = 0.5f;

            _this.Started = true;

            Console.WriteLine("TeamServer startOver. [{0}]", TeamServer.Instance.Id);
            yield break;
        }

        public IEnumerator Tick(Coroutine co, ServerAgentBase server)
        {
            var __this = ((TeamServerControl) server);

            __this.TickCount++;
            try
            {
                TeamServerControl.tm.Update();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Tick error.");
            }
            try
            {
                // 每秒Tick一次
                if (__this.TickCount%2 == 0)
                {
                    ((TeamServerControl) server).mTimedTaskManager.Tick();
					TeamServerMonitor.TickRate.Mark();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "TimedTaskManager tick error");
            }

            return null;
        }

        public IEnumerator OnServerStop(Coroutine coroutine, TeamService _this)
        {
            {
                var co = CoroutineFactory.NewSubroutine(ServerAllianceManager.RefreshAll, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }

            {
                var co = CoroutineFactory.NewSubroutine(ExchangeManager.SaveDB, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }

            {
                var co = CoroutineFactory.NewSubroutine(GroupShop.SaveDB, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
            {
                var co = CoroutineFactory.NewSubroutine(ServerAuctionManager.instance.Save, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }

            {
                var co = CoroutineFactory.NewSubroutine(UnionBattleManager.SaveCoroutine, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }

            TeamServer.Instance.DB.Dispose();
        }

        public IEnumerator PrepareDataForEnterGame(Coroutine coroutine,
                                                   TeamService _this,
                                                   PrepareDataForEnterGameInMessage msg)
        {
            msg.Reply();
            Logger.Info("Enter Game {0} - PrepareDataForEnterGame - 1 - {1}", msg.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);
            return null;
        }

        public IEnumerator PrepareDataForCreateCharacter(Coroutine coroutine,
                                                         TeamService _this,
                                                         PrepareDataForCreateCharacterInMessage msg)
        {
            msg.Reply();
            Logger.Info("Reply PrepareDataForCreateCharacter Team {0}", msg.CharacterId);
            return null;
        }

        public IEnumerator PrepareDataForCommonUse(Coroutine coroutine,
                                                   TeamService _this,
                                                   PrepareDataForCommonUseInMessage msg)
        {
            msg.Reply();
            return null;
        }

        public IEnumerator PrepareDataForLogout(Coroutine coroutine,
                                                TeamService _this,
                                                PrepareDataForLogoutInMessage msg)
        {
            msg.Reply();
            yield break;
        }

        public IEnumerator CheckConnected(Coroutine coroutine, TeamService _this, CheckConnectedInMessage msg)
        {
            Logger.Error("Team CheckConnected, {0}", msg.CharacterId);

            //TeamCharacterProxy proxy = null;
            //if (_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            //{
            //    if ((proxy as TeamProxy).Connected)
            //    {
            //        msg.Response = 1;
            //        msg.Reply();
            //        return null;
            //    }

            //    (proxy as TeamProxy).WaitingCheckConnectedInMessages.Add(msg);
            //}

            return null;
        }

        public IEnumerator CheckLost(Coroutine coroutine, TeamService _this, CheckLostInMessage msg)
        {
            Logger.Error("Team CheckLost, {0}", msg.CharacterId);

            //TeamCharacterProxy proxy = null;
            //if (!_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            //{
            //    msg.Reply();
            //}
            //else
            //{
            //    if ((proxy as TeamProxy).Connected)
            //    {
            //        (proxy as TeamProxy).WaitingCheckLostInMessages.Add(msg);
            //    }
            //    else
            //    {
            //        msg.Reply();
            //    }
            //}

            return null;
        }

        public IEnumerator QueryStatus(Coroutine coroutine, TeamService _this, QueryStatusInMessage msg)
        {
            var teamServer = (TeamServerControl) _this;
            var common = new ServerCommonStatus();
            common.Id = TeamServer.Instance.Id;
            common.ByteReceivedPerSecond = teamServer.ByteReceivedPerSecond;
            common.ByteSendPerSecond = teamServer.ByteSendPerSecond;
            common.MessageReceivedPerSecond = teamServer.MessageReceivedPerSecond;
            common.MessageSendPerSecond = teamServer.MessageSendPerSecond;
            common.ConnectionCount = teamServer.ConnectionCount;

            msg.Response.CommonStatus = common;

            msg.Response.ConnectionInfo.AddRange(TeamServer.Instance.Agents.Select(kv =>
            {
                var conn = new ConnectionStatus();
                var item = kv.Value;
                conn.ByteReceivedPerSecond = item.ByteReceivedPerSecond;
                conn.ByteSendPerSecond = item.ByteSendPerSecond;
                conn.MessageReceivedPerSecond = item.MessageReceivedPerSecond;
                conn.MessageSendPerSecond = item.MessageSendPerSecond;
                conn.Target = item.Id;
                conn.Latency = item.Latency;

                return conn;
            }));

            msg.Reply();

            yield break;
        }

        public IEnumerator TeamDungeonLeaderChangeScene(Coroutine coroutine,
                                                        TeamService _this,
                                                        TeamDungeonLeaderChangeSceneInMessage msg)
        {
            //Character CharacterTeam = TeamManager.GetCharacterTeam(msg.CharacterId);
            //Logger.Info("TeamDungeonLeaderChangeScene id={0}", msg.CharacterId);
            //var sceneGuid = msg.Request.SceneGuid;
            //MatchingCharacter character = MatchingManager.GetMatchingCharacter(msg.CharacterId);
            //if (character == null)
            //{
            //    msg.Reply((int)ErrorCodes.Error_MatchingTeamNotFindCharacter);
            //    yield break;
            //}
            //if (character.IsChangeDungeon == false)
            //{
            //    msg.Reply();
            //    yield break;
            //}
            //character.IsChangeDungeon = false;
            //if (character.Team != null && character.Team.List[0] == character)
            //{
            //    foreach (MatchingCharacter matchingCharacter in character.Team.List)
            //    {
            //        if (matchingCharacter != character)
            //        {
            //            Logger.Info("TeamDungeonLeaderChangeScene NotifyCreateChangeSceneCoroutine id={0}", matchingCharacter.Guid);
            //            //CoroutineFactory.NewCoroutine(TempTeam.NotifyCreateChangeSceneCoroutine, matchingCharacter.Guid, matchingCharacter.ServerId, -1, sceneGuid).MoveNext();
            //        }
            //    }
            //    foreach (MatchingCharacter matchingCharacter in character.Team.List)
            //    {
            //        MatchingManager.Pop(matchingCharacter.Guid, eLeaveMatchingType.Success);
            //    }
            //    msg.Reply();
            //    yield break;
            //}
            msg.Reply((int) ErrorCodes.Unknow);
            yield break;
        }

        //离开副本
        public IEnumerator LeaveDungeon(Coroutine coroutine, TeamService _this, LeaveDungeonInMessage msg)
        {
            var id = msg.Request.CharacterId;
            var teamCharacter = TeamManager.GetCharacterTeam(id);
            if (teamCharacter != null)
            {
                TeamManager.Leave(id, teamCharacter.team.TeamId);
            }
            msg.Reply();
            return null;
        }

		public IEnumerator ServerGMCommand(Coroutine coroutine, TeamService _this, ServerGMCommandInMessage msg)
        {
			var cmd = msg.Request.Cmd;
			var param = msg.Request.Param;

			Logger.Info("Team----------ServerGMCommand----------cmd={0}|param={1}", cmd, param);

			try
			{
				if ("ReloadTable" == cmd)
				{
					Table.ReloadTable(param);
				}
			}
			catch (Exception e)
			{
				Logger.Error("Team----------ServerGMCommand----------error={0}", e.Message);
			}
			finally
			{
			}
			yield break;
        }

        //战盟操作:创建
        public IEnumerator Logic2TeamCreateAlliance(Coroutine coroutine,
                                                    TeamService _this,
                                                    Logic2TeamCreateAllianceInMessage msg)
        {
            var guid = msg.Request.Guid;
            var State = msg.Request.State;
            var name = msg.Request.Name;
            var serverId = msg.Request.ServerId;
            PlayerLog.WriteLog((int) LogType.SyncAllianceMessage,
                "SS->Logic2TeamCreateAlliance toCharacterId={0}, type={1}, name1={2},serverId={3} ", guid, State, name,
                serverId);
            switch (State)
            {
                case 0:
                    //新增创建
                {
                    var allianceId = -1;
                    var resultCodes = ServerAllianceManager.CreateNewAlliance(serverId, guid, name,
                        ref allianceId);
                    msg.Response = allianceId;
                    msg.Reply((int) resultCodes);
                }
                    break;
                case 1:
                    //确认创建:成功
                {
                    ServerAllianceManager.CreateAllianceSuccess(name);
                    msg.Reply();
                }
                    break;
                case -1:
                    //确认创建:失败
                {
                    ServerAllianceManager.CreateAllianceFaild(name);
                    msg.Reply();
                }
                    break;
            }
            yield break;
        }

        //战盟操作:其他操作 type：0=申请加入（value=战盟ID）  1=取消申请（value=战盟ID）  2=退出战盟   3=同意邀请（value=战盟ID）  4=拒绝邀请（value=战盟ID）
        public IEnumerator Logic2TeamAllianceOperation(Coroutine coroutine,
                                                       TeamService _this,
                                                       Logic2TeamAllianceOperationInMessage msg)
        {
            var type = msg.Request.Type;
            var cId = msg.CharacterId;
            var value = msg.Request.Value;
            var resultCodes = ErrorCodes.OK;
            PlayerLog.WriteLog((int) LogType.SyncAllianceMessage,
                "SS->Logic2TeamAllianceOperation toCharacterId={0}, type={1}, value={2} ", cId, type, value);
            switch (type)
            {
                case 0:
                {
                    resultCodes = ServerAllianceManager.ApplyCharacter(value, cId);
                    if (resultCodes == ErrorCodes.Error_AllianceApplyJoinOK)
                    {
                        var a = ServerAllianceManager.GetAllianceById(value);
                        if (a != null)
                        {
                            msg.Response = a.Name;
                        }
                    }
                }
                    break;
                case 1:
                {
                    resultCodes = ServerAllianceManager.ApplyCancel(value, cId);
                }
                    break;
                case 2:
                {
                    resultCodes = ServerAllianceManager.LeaveCharacter(value, cId);
                }
                    break;
                case 3:
                {
                    resultCodes = ServerAllianceManager.InviteResult(value, cId, 1);
                    if (resultCodes == ErrorCodes.OK)
                    {
                        var a = ServerAllianceManager.GetAllianceById(value);
                        if (a != null)
                        {
                            msg.Response = a.Name;
                        }
                    }
                }
                    break;
                case 4:
                {
                    resultCodes = ServerAllianceManager.InviteResult(value, cId, 0);
                }
                    break;
            }
            msg.Reply((int) resultCodes);
            yield break;
        }

        //战盟操作:其他操作     type：0=邀请加入 1=同意申请加入 2：拒绝申请加入
        public IEnumerator Logic2TeamAllianceOperationCharacter(Coroutine coroutine,
                                                                TeamService _this,
                                                                Logic2TeamAllianceOperationCharacterInMessage msg)
        {
            var fromGuid = msg.CharacterId;
            var result = ErrorCodes.OK;
            var toGuid = msg.Request.Guid;
            var allianceId = msg.Request.AllianceId;
            var type = msg.Request.Type;
            var name = msg.Request.Name;
            PlayerLog.WriteLog((int) LogType.SyncAllianceMessage,
                "SS->Logic2TeamAllianceOperationCharacter toCharacterId={0}, type={1}, name={2}, allianceId={3}", toGuid,
                type, name, allianceId);
            switch (type)
            {
                case 0:
                {
                    result = ServerAllianceManager.InviteCharacter(fromGuid, allianceId, toGuid);
                    if (result == ErrorCodes.OK)
                    {
                        ServerAllianceManager.SendMessage(toGuid, 0, name, allianceId,
                            ServerAllianceManager.GetAllianceName(allianceId));
                    }
                }
                    break;
                case 1:
                {
                    result = ServerAllianceManager.ApplyResult(allianceId, toGuid, 1);
                    if (result == ErrorCodes.OK)
                    {
                        ServerAllianceManager.SendMessage(toGuid, 1, name, allianceId,
                            ServerAllianceManager.GetAllianceName(allianceId));
                    }
                }
                    break;
                case 2:
                {
                    result = ServerAllianceManager.ApplyResult(allianceId, toGuid, 0);
                    if (result == ErrorCodes.OK)
                    {
                        ServerAllianceManager.SendMessage(toGuid, 2, name, allianceId,
                            ServerAllianceManager.GetAllianceName(allianceId));
                    }
                }
                    break;
            }
            msg.Reply((int) result);
            yield break;
        }

        //同步玩家的帮派数据
        public IEnumerator GetAllianceCharacterData(Coroutine coroutine,
                                                    TeamService _this,
                                                    GetAllianceCharacterDataInMessage msg)
        {
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(msg.Request.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int) ErrorCodes.ServerID);
                yield break;
            }
            var server = serverAlliance.GetServerData(msg.Request.ServerId);
            if (server == null)
            {
                msg.Reply((int) ErrorCodes.ServerID);
                yield break;
            }
            var character = serverAlliance.GetCharacterData(msg.Request.Guid);
            if (character == null)
            {
                //说明没有
                DBAllianceApplyOne applyOne;
                if (server.Applys.TryGetValue(msg.Request.Guid, out applyOne))
                {
                    foreach (var i in applyOne.Applys)
                    {
                        msg.Response.Applys.Add(i);
                    }
                }
                msg.Reply();
                yield break;
            }
            var t = ServerAllianceManager.GetAllianceById(character.AllianceId);
            if (t == null || t.State == TeamAllianceState.WillDisband)
            {
                serverAlliance.RemoveCharacterData(msg.Request.Guid);
                DBAllianceApplyOne applyOne;
                if (server.Applys.TryGetValue(msg.Request.Guid, out applyOne))
                {
                    foreach (var i in applyOne.Applys)
                    {
                        msg.Response.Applys.Add(i);
                    }
                }
                msg.Reply();
                yield break;
            }

            msg.Response.AllianceId = character.AllianceId;
            msg.Response.Ladder = character.Ladder;
            msg.Response.MeritPoint = character.MeritPoint;
            msg.Reply();
        }

        //请求玩家的队伍信息
        public IEnumerator SSGetTeamData(Coroutine coroutine, TeamService _this, SSGetTeamDataInMessage msg)
        {
            var character = TeamManager.GetCharacterTeam(msg.Request.Guid);
            if (character != null)
            {
                msg.Response.TeamId = character.team.TeamId;
                if (character.TeamState == TeamState.Leader)
                {
                    msg.Response.State = 0;
                }
                else
                {
                    msg.Response.State = 1;
                }
            }
            msg.Reply();
            yield break;
        }

        //请求玩家的战盟信息
        public IEnumerator SSGetAllianceData(Coroutine coroutine, TeamService _this, SSGetAllianceDataInMessage msg)
        {
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(msg.Request.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int) ErrorCodes.ServerID);
                yield break;
            }
            var response = msg.Response;
            var c = serverAlliance.GetCharacterData(msg.CharacterId);
            if (c == null)
            {
                msg.Reply((int) ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            response.AllianceId = c.AllianceId;
            response.Ladder = c.Ladder;
            var a = serverAlliance.GetAlliance(c.AllianceId);
            if (a != null)
            {
                response.Name = a.Name;
                response.Level = a.Level;
            }
            msg.Reply();
        }

        //玩家捐献，这边需要记录
        public IEnumerator Logic2TeamDonationAllianceItem(Coroutine coroutine,
                                                          TeamService _this,
                                                          Logic2TeamDonationAllianceItemInMessage msg)
        {
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(msg.Request.ServerId);
            if (serverAlliance == null)
            {
                Logger.Warn("Logic2TeamDonationAllianceItem not find 1!server={0},type={1},character={2},",
                    msg.Request.ServerId, msg.Request.Type, msg.CharacterId);
                msg.Reply((int) ErrorCodes.ServerID);
                yield break;
            }
            var character = serverAlliance.GetCharacterData(msg.CharacterId);
            if (character == null)
            {
                Logger.Warn("Logic2TeamDonationAllianceItem not find 2!server={0},type={1},character={2},",
                    msg.Request.ServerId, msg.Request.Type, msg.CharacterId);
                msg.Reply((int) ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            var a = ServerAllianceManager.GetAllianceById(character.AllianceId);
            if (a == null)
            {
                Logger.Warn("Logic2TeamDonationAllianceItem not find 3!AllianceId={0},type={1},character={2},",
                    character.AllianceId, msg.Request.Type, msg.CharacterId);
                msg.Reply((int) ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var tbGuild = Table.GetGuild(a.Level);
            if (tbGuild == null)
            {
                msg.Reply((int) ErrorCodes.Error_GuildID);
                yield break;
            }
            var type = msg.Request.Type;
            var resId = -1;
            var value = 0;
            switch (type)
            {
                case 0:
                {
                    resId = 2;
                    value = tbGuild.LessNeedCount;
                    msg.Response = a.Level;
                }
                    break;
                case 1:
                {
                    resId = 2;
                    value = tbGuild.MoreNeedCount;
                    msg.Response = a.Level;
                }
                    break;
                case 2:
                {
                    resId = 3;
                    value = tbGuild.DiaNeedCount;
                    msg.Response = a.Level;
                }
                    break;
                default:
                {
                    var tbGuildMiss = Table.GetGuildMission(type);
                    if (tbGuildMiss == null)
                    {
                        Logger.Warn("Logic2TeamDonationAllianceItem type={0}", type);
                        msg.Reply((int) ErrorCodes.Error_AllianceMissionNotFind);
                        yield break;
                    }
                    foreach (var data in a.Missions)
                    {
                        if (data.Id == type && data.State == (int) AllianceMissionState.Normal)
                        {
                            resId = tbGuildMiss.ItemID;
                            value = 1;
                            break;
                        }
                    }
                    if (resId == -1)
                    {
                        msg.Reply((int) ErrorCodes.Error_AllianceMissionNotFind);
                        yield break;
                    }
                }
                    break;
            }
            var dbLogicSimple = TeamServer.Instance.LogicAgent.DeleteItem(msg.CharacterId, resId, value, (int)eDeleteItemType.ZhanMenJuanXian);
            yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
            if (dbLogicSimple.State != MessageState.Reply)
            {
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }
            if (dbLogicSimple.ErrorCode != (int) ErrorCodes.OK)
            {
                msg.Reply(dbLogicSimple.ErrorCode);
                yield break;
            }
            var itemCount = msg.Response;
            a.DonationAllianceItem(msg.CharacterId, msg.Request.Type, msg.Request.Name, ref itemCount);
            msg.Response = itemCount;
            msg.Reply();
        }

        //获得战盟的Buff等级
        //public  IEnumerator SSGetAllianceBuffLevel(Coroutine coroutine, TeamService _this, SSGetAllianceBuffLevelInMessage msg)
        //{
        //    AllianceManager serverAlliance = ServerAllianceManager.GetAllianceByServer(msg.Request.ServerId);
        //    if (serverAlliance == null)
        //    {
        //        Logger.Warn("SSGetAllianceBuffLevel not find 1!server={0},BuffId={1},character={2},",
        //            msg.Request.ServerId, msg.Request.BuffId, msg.CharacterId);
        //        msg.Reply((int)ErrorCodes.ServerID);
        //        yield break;
        //    }
        //    var character = serverAlliance.GetCharacterData(msg.CharacterId);
        //    if (character == null)
        //    {
        //        Logger.Warn("SSGetAllianceBuffLevel not find 2!server={0},BuffId={1},character={2},",
        //            msg.Request.ServerId, msg.Request.BuffId, msg.CharacterId);
        //        msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
        //        yield break;
        //    }
        //    var a = ServerAllianceManager.GetAllianceById(character.AllianceId);
        //    if (a == null)
        //    {
        //        Logger.Warn("SSGetAllianceBuffLevel not find 3!AllianceId={0},BuffId={1},character={2},",
        //            character.AllianceId, msg.Request.BuffId, msg.CharacterId);
        //        msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
        //        yield break;
        //    }
        //    //msg.Response = a.GetBuffLevel(msg.Request.BuffId);
        //    msg.Reply();
        //}

        //获得战盟的名字
        public IEnumerator SSGetAllianceName(Coroutine coroutine, TeamService _this, SSGetAllianceNameInMessage msg)
        {
            var aId = msg.Request.AllianceId;
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int) ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            msg.Response = alliance.Name;
            msg.Reply();
        }

        //获得战盟的名字
        public IEnumerator SSGetAlliance(Coroutine coroutine, TeamService _this, SSGetAllianceInMessage msg)
        {
            try
            {
                var serverId = msg.Request.ServerId;
                var alliance = ServerAllianceManager.GetAllianceByServer(serverId);
                if (alliance == null)
                {
                    msg.Reply((int) ErrorCodes.Error_CharacterNoAlliance);
                    yield break;
                }

                ServerAllianceInfo tmp = new ServerAllianceInfo();
                tmp.alliances.Clear();
                var startIndex = msg.Request.StartIndex;
                var endIndex = msg.Request.EndIndex;
                var name = msg.Request.Name;
                var i = 0;
                foreach (var data in alliance.Alliances)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        if (i < startIndex || i > endIndex)
                        {
                            continue;
                        }
                        i++;
                    }
                    else
                    {
                        if (data.Value.Name != name)
                        {
                            continue;
                        }
                    }

                    if (alliance.Alliances != null && data.Value != null)
                    {
                        AllianceInfo ali = new AllianceInfo();
                        ali.Id = data.Value.AllianceId;
                        ali.Name = data.Value.Name;
                        ali.Leader = data.Value.Leader;
                        ali.LeaderName = data.Value.GetCharacterName(data.Value.Leader);
                        ali.Members.Clear();
                        foreach (var value in data.Value.mDBData.Members)
                        {
                            var dt = data.Value.Dad.GetCharacterData(value);
                            if (dt == null)
                            {
                                continue;
                            }
                            AlianceMember mem = new AlianceMember();
                            mem.Id = dt.Guid;
                            mem.Name = dt.Name;
                            mem.Level = dt.Level;
                            mem.RoleId = dt.RoleId;
                            ali.Members.Add(mem);
                        }

                        ali.ServerId = data.Value.ServerId;
                        ali.State = (int) data.Value.State;
                        ali.Notice = data.Value.Notice;
                        ali.Level = data.Value.Level;
                        ali.CreateTime = data.Value.CreateTime.ToString("yyyy/MM/dd HH:mm:ss");
                        tmp.alliances.Add(ali);
                    }
                }
                msg.Response = tmp;
            }
            catch (Exception e)
            {
                Logger.Error("SSGetAlliance error {0}", e.Message);
            }
            finally
            {
                msg.Reply();
            }
        }

        public IEnumerator SSAllianceDepotTakeOut(Coroutine coroutine, TeamService _this, SSAllianceDepotTakeOutInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            var serverId = msg.Request.ServerId;
            var name = msg.Request.CharacterName;
            var bagIndex = msg.Request.BagIndex;
            var itemId = msg.Request.ItemId;
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(serverId);
            if (null == serverAlliance)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var character = serverAlliance.GetCharacterData(characterId);
            if (null == character)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            var alliance = ServerAllianceManager.GetAllianceById(character.AllianceId);
            if (null == alliance)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var itemData = new ItemBaseData();
            var result = alliance.DepotTakeOutEquip(characterId, name, bagIndex, itemId, out itemData);
            if (result != ErrorCodes.OK)
            {
                msg.Reply((int)result);
            }
            msg.Response = itemData;
            msg.Reply((int)ErrorCodes.OK);
        }

        public IEnumerator SSAllianceDepotDonate(Coroutine coroutine, TeamService _this, SSAllianceDepotDonateInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            var serverId = msg.Request.ServerId;
            var name = msg.Request.CharacterName;
            var Item = msg.Request.Item;
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(serverId);
            if (null == serverAlliance)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var character = serverAlliance.GetCharacterData(characterId);
            if (null == character)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            var alliance = ServerAllianceManager.GetAllianceById(character.AllianceId);
            if (null == alliance)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var result = alliance.DepotDonateEquip(characterId, name, Item);
            if (result != ErrorCodes.OK)
            {
                msg.Reply((int)result);
            }
            msg.Reply((int)ErrorCodes.OK);
        }

        public IEnumerator GMChangeJurisdiction(Coroutine coroutine, TeamService _this, GMChangeJurisdictionInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            var allianceId = msg.Request.AllianceId;
            var opGuid = msg.Request.Guid;
            var type = msg.Request.Type;

            var allianceServer = ServerAllianceManager.GetAllianceByServer(serverId);
            if (allianceServer == null)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            Alliance alliance = new Alliance();
            if (!allianceServer.Alliances.TryGetValue(allianceId, out alliance))
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }

            var requestCharacter = allianceServer.GetCharacterData(alliance.Leader);
            if (requestCharacter == null)
            {
                Logger.Warn("ChangeJurisdiction requestCharacter is not have proxy alliance[{0}]", allianceId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            var operationCharacter = allianceServer.GetCharacterData(opGuid);
            if (operationCharacter == null)
            {
                Logger.Warn("ChangeJurisdiction operationCharacter is not have proxy alliance[{0}]", opGuid);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            if (requestCharacter.AllianceId != allianceId)
            {
                Logger.Warn("ChangeJurisdiction requestCharacter is not same! requestAlliance={0},proxyAlliance={1}",
                    requestCharacter.AllianceId, allianceId);
                msg.Reply((int)ErrorCodes.Error_AllianceIsNotSame);
                yield break;
            }

            if (operationCharacter.AllianceId != allianceId)
            {
                Logger.Warn("ChangeJurisdiction requestCharacter is not same! operationAlliance={0},proxyAlliance={1}",
                    operationCharacter.AllianceId, allianceId);
                msg.Reply((int)ErrorCodes.Error_AllianceIsNotSame);
                yield break;
            }

            var err = alliance.ChangeJurisdiction(requestCharacter, operationCharacter, type);
            msg.Reply((int)err);
        }

        public IEnumerator GMChangeAllianceNotice(Coroutine coroutine, TeamService _this, GMChangeAllianceNoticeInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            var allianceId = msg.Request.AllianceId;
            var content = msg.Request.Content;

            var allianceServer = ServerAllianceManager.GetAllianceByServer(serverId);
            if (allianceServer == null)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            Alliance alliance = new Alliance();
            if (!allianceServer.Alliances.TryGetValue(allianceId, out alliance))
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            alliance.SetNotice(content);

            msg.Reply();
        }

        public IEnumerator GMDelAllicance(Coroutine coroutine, TeamService _this, GMDelAllicanceInMessage msg)
        {
            var allianceId = msg.Request.AllianceId;
            var err = ServerAllianceManager.DeleteAlliance(allianceId);

            msg.Reply((int)err);
            yield break;
        }

        //广播交易道具
        public IEnumerator BroadcastExchangeItem(Coroutine coroutine,
                                                 TeamService _this,
                                                 BroadcastExchangeItemInMessage msg)
        {
            var result = ExchangeManager.PushItem(msg.Request.CharacterId, msg.Request.CharacterName, msg.Request.Item,
                msg.Request.NeedCount, msg.Request.ContinueMinutes);
            msg.Response = result;
            msg.Reply();
            yield break;
        }

        //拍卖行添加道具
        public IEnumerator SSOnItemAuction(Coroutine coroutine, TeamService _this, SSOnItemAuctionInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            var auc = ServerAuctionManager.instance.GetAuction(serverId);
            if (auc == null)
            {
                msg.Reply((int) ErrorCodes.ServerID);
                yield break;
            }
            var aucItem = new Auctions
            {
                serverId = serverId
            };
            var dbItem = new AuctionItemOne();
            aucItem.dbData = dbItem;
            dbItem.ItemGuid = msg.Request.ItemGuid;
            dbItem.ManagerId = ServerAuctionManager.instance.GetNextId();
            dbItem.SellCharacterName = msg.Request.CharacterName;
            dbItem.SellCharacterId = msg.Request.CharacterId;
            dbItem.NeedType = msg.Request.NeedType;
            dbItem.NeedCount = msg.Request.NeedCount;
            dbItem.ItemData = msg.Request.Item;
            dbItem.OverTime = DateTime.Now.AddHours(ServerAuctionManager.AuctionTime).ToBinary();
            auc.AddItem(aucItem);
            msg.Response = dbItem.ManagerId;
            msg.Reply();
        }

        //拍卖行：取消交易道具
        public IEnumerator SSDownItemAuction(Coroutine coroutine, TeamService _this, SSDownItemAuctionInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            var auc = ServerAuctionManager.instance.GetAuction(serverId);
            if (auc == null)
            {
                //msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var item = auc.GetItem(msg.Request.ItemGuid);
            if (item == null)
            {
                yield break;
            }
            auc.RemoveItem(item);
        }

        //拍卖行：检查是否存在道具
        public IEnumerator SSSelectItemAuction(Coroutine coroutine, TeamService _this, SSSelectItemAuctionInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            var auc = ServerAuctionManager.instance.GetAuction(serverId);
            if (auc == null)
            {
                msg.Response = -1;
                msg.Reply((int) ErrorCodes.ServerID);
                yield break;
            }
            var item = auc.GetItem(msg.Request.ItemManagerId);
            if (item == null)
            {
                msg.Response = -1;
                msg.Reply((int) ErrorCodes.Error_ItemNotFind);
                yield break;
            }
            if (item.dbData.SellCharacterId != msg.Request.CharacterId)
            {
                msg.Response = -1;
                msg.Reply((int) ErrorCodes.Error_CharacterNotFind);
                yield break;
            }
            //if (item.serverId != serverId)
            //{
            //    msg.Response = -1;
            //    msg.Reply((int)ErrorCodes.ServerID);
            //    yield break;
            //}
            msg.Response = item.serverId;
            msg.Reply();
        }

        //请求可看到的交易所道具
        public IEnumerator GetExchangeItem(Coroutine coroutine, TeamService _this, GetExchangeItemInMessage msg)
        {
            ExchangeManager.GetItems(msg.Request.Type, msg.Request.CharacterId, msg.Request.Level - 1, msg.Request.Count,
                msg.Response);
            msg.Reply();
            yield break;
        }

        //取消广播交易道具
        public IEnumerator CancelExchangeItem(Coroutine coroutine, TeamService _this, CancelExchangeItemInMessage msg)
        {
            ExchangeManager.CancelItem(msg.Request.CharacterId, msg.Request.ItemGuid);
            //msg.Reply();
            yield break;
        }

        //购买一个道具
        public IEnumerator SSBuyGroupShopItem(Coroutine coroutine, TeamService _this, SSBuyGroupShopItemInMessage msg)
        {
            var guid = msg.Request.Guid;
            var item = GroupShop.GetItem(guid);
            if (item == null || item.State != (int) eGroupShopItemState.OnSell)
            {
                msg.Reply((int) ErrorCodes.Error_GroupShopOver);
                yield break;
            }
            msg.Response = item.NowCount;
            var count = msg.Request.Count;
            var max = item.MaxCount;
            var canBuy = max - item.NowCount;
            if (canBuy < count)
            {
                count = canBuy;
            }
            if (count < 1)
            {
                msg.Reply((int) ErrorCodes.Error_GroupShopOver);
                yield break;
            }
            if (item.CheckCountLimit(msg.CharacterId, count))
            {
                msg.Reply((int) ErrorCodes.Error_GroupShopCountNotEnough);
                yield break;
            }

            var dbLogicSimple = TeamServer.Instance.LogicAgent.DeleteItem(msg.CharacterId, item.tbGroupShop.SaleType,
                item.tbGroupShop.SaleCount * count, (int)eDeleteItemType.XuYuanShuTuanGou);
            yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
            if (dbLogicSimple.State != MessageState.Reply)
            {
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }
            if (dbLogicSimple.ErrorCode != (int) ErrorCodes.OK)
            {
                msg.Reply(dbLogicSimple.ErrorCode);
                yield break;
            }
            //检查guid所标示的item是否已经卖掉了
            item = GroupShop.GetItem(guid);
            if (item == null || item.State != (int) eGroupShopItemState.OnSell)
            {
                //已经卖掉了，把扣掉的资源，加回去
                msg.Reply((int) ErrorCodes.Error_GroupShopOver);
                var logicGiveItemMsg = TeamServer.Instance.LogicAgent.GiveItem(msg.CharacterId,
                    item.tbGroupShop.SaleType, item.tbGroupShop.SaleCount*count,-1);
                yield return logicGiveItemMsg.SendAndWaitUntilDone(coroutine);
                if (logicGiveItemMsg.State != MessageState.Reply)
                {
                    Logger.Error(
                        "TeamServerControl.SSBuyGroupShopItem() item sold out,resource add back failed for character[{0}],logicGiveItemMsg.State = {1}",
                        msg.CharacterId, logicGiveItemMsg.State);
                    yield break;
                }
                if (logicGiveItemMsg.ErrorCode != (int) ErrorCodes.OK)
                {
                    Logger.Error(
                        "TeamServerControl.SSBuyGroupShopItem() item sold out,resource add back failed for character[{0}],logicGiveItemMsg.ErrorCode = {1}",
                        msg.CharacterId, logicGiveItemMsg.ErrorCode);
                    yield break;
                }
                yield break;
            }
            msg.Response = item.CharacterBuy(msg.CharacterId, count);
            msg.Reply();
        }

        //获取 msg.CharacterId 该用户，已经买过，但尚未开奖的所有商品
        public IEnumerator SSGetBuyedGroupShopItems(Coroutine coroutine,
                                                    TeamService _this,
                                                    SSGetBuyedGroupShopItemsInMessage msg)
        {
            var characterId = msg.CharacterId;

            var itemAll = new GroupShopItemAll();
            var itemList = new GroupShopItemList();
            itemAll.Lists.Add(itemList);

            var buyed = msg.Request.Buyed.Items;
            var buyedItems = GroupShop.GetItems(buyed);
            itemList.Items.AddRange(buyedItems.Select(item => item.GetNetData(characterId)));
            msg.Response = itemAll;
            msg.Reply();
            yield return null;
        }

        //获取 msg.CharacterId 该用户的购买历史
        public IEnumerator SSGetGroupShopHistory(Coroutine coroutine,
                                                 TeamService _this,
                                                 SSGetGroupShopHistoryInMessage msg)
        {
            var characterId = msg.CharacterId;
            var response = new GroupShopItemAllForServer();
            var items = new GroupShopItemAll();
            var list = new GroupShopItemList();
            items.Lists.Add(list);

            var history = msg.Request.History.Items;
            var historyItems = GroupShop.GetHistorys(history);
            if (historyItems.Count < history.Count)
            {
                response.Dirty = true;
            }
            var buyed = msg.Request.Buyed.Items;
            var buyedItems = GroupShop.GetHistorys(buyed);
            if (buyedItems.Count > 0)
            {
                response.Dirty = true;
                buyedItems.AddRange(historyItems);
                historyItems = buyedItems;
                if (historyItems.Count > 20)
                {
                    historyItems.RemoveRange(20, historyItems.Count - 20);
                }
            }
            foreach (var one in historyItems)
            {
                list.Items.Add(one.GetNetData(characterId));
            }
            response.Items = items;
            response.Expired.AddRange(GroupShop.GetExpired(buyed));

            msg.Response = response;
            msg.Reply();
            yield return null;
        }

        //战场有人进去了
        public IEnumerator SSCharacterEnterBattle(Coroutine coroutine,
                                                  TeamService _this,
                                                  SSCharacterEnterBattleInMessage msg)
        {
            var fubenId = msg.Request.FubenId;
            var sceneGuid = msg.Request.SceneGuid;
            var characterId = msg.Request.CharacterId;
            PlayerLog.WriteLog((int) LogType.BattleLog, "SSCharacterEnterBattle c={0},s={1}", characterId, sceneGuid);
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                Logger.Error("In SSCharacterEnterBattle(). tbFuben == null! id = {0}", fubenId);
                yield break;
            }
            var type = (eDungeonAssistType) tbFuben.AssistType;
            switch (type)
            {
                case eDungeonAssistType.AllianceWar:
                {
                    AllianceWarManager.PlayerEnterSuccess(msg.Request.CharacterId);
                }
                    break;
                default:
                {
                    QueueSceneManager.EnterScene(msg.Request.CharacterId, msg.Request.SceneGuid);
                }
                    break;
            }
        }

        //战场有人离开了
        public IEnumerator SSCharacterLeaveBattle(Coroutine coroutine,
                                                  TeamService _this,
                                                  SSCharacterLeaveBattleInMessage msg)
        {
            var fubenId = msg.Request.FubenId;
            var sceneGuid = msg.Request.SceneGuid;
            var characterId = msg.Request.CharacterId;
            PlayerLog.WriteLog((int) LogType.BattleLog, "SSCharacterLeaveBattle      c={0},s={1}", characterId,
                sceneGuid);
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                Logger.Error("In SSCharacterEnterBattle(). tbFuben == null! id = {0}", fubenId);
                yield break;
            }
            var type = (eDungeonAssistType) tbFuben.AssistType;
            switch (type)
            {
                case eDungeonAssistType.AllianceWar:
                {
                    AllianceWarManager.PlayerLeave(msg.Request.CharacterId);
                }
                    break;
                default:
                {
                    QueueTeamManager.LeaveScene(msg.Request.CharacterId);
                    QueueSceneManager.LeaveScene(msg.Request.CharacterId, msg.Request.SceneGuid);
                }
                    break;
            }
        }

        //某个战场结束了
        public IEnumerator SSBattleEnd(Coroutine coroutine, TeamService _this, SSBattleEndInMessage msg)
        {
            PlayerLog.WriteLog((int) LogType.BattleLog, "SSBattleEnd      s={0}", msg.Request.SceneGuid);
            QueueSceneManager.OverScene(msg.Request.SceneGuid);
            yield break;
        }

        //查询某个队伍ID的人数
        public IEnumerator SSGetTeamCount(Coroutine coroutine, TeamService _this, SSGetTeamCountInMessage msg)
        {
            foreach (var teamId in msg.Request.TeamIds.Items)
            {
                var team = TeamManager.GetCharacterTeam(teamId);
                if (team == null)
                {
                    msg.Response.Items.Add(0);
                    continue;
                }
                msg.Response.Items.Add(team.team.GetTeamCount());
            }
            msg.Reply();
            yield break;
        }

        //查询某个队伍ID的人数
        public IEnumerator SSGetTeamSceneData(Coroutine coroutine, TeamService _this, SSGetTeamSceneDataInMessage msg)
        {
            var id = msg.Request.CharacterId;
            var c = TeamManager.GetCharacterTeam(id);
            if (c == null)
            {
                msg.Reply((int) ErrorCodes.Error_CharacterNoTeam);
                yield break;
            }
            //msg.Response.Items.AddRange(c.team.TeamList);
            foreach (var guid in c.team.TeamList)
            {
                if (guid == id)
                {
                    continue;
                }
                if (!((TeamServerControl) _this).IsOnline(id))
                {
                    continue;
                }

                var dbSceneSimple = TeamServer.Instance.SceneAgent.SSGetCharacterSceneData(guid, guid);
                yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                if (dbSceneSimple.State != MessageState.Reply)
                {
                    continue;
                }
                if (dbSceneSimple.ErrorCode != (int) ErrorCodes.OK)
                {
                    continue;
                }
                msg.Response.Objs.Add(dbSceneSimple.Response);
            }
            msg.Reply((int)ErrorCodes.OK, true);
        }

        //获得本队伍其他人的Id
        public IEnumerator SSGetTeamCharacters(Coroutine coroutine, TeamService _this, SSGetTeamCharactersInMessage msg)
        {
            var id = msg.Request.CharacterId;
            var c = TeamManager.GetCharacterTeam(id);
            if (c == null)
            {
                msg.Reply((int) ErrorCodes.Error_CharacterNoTeam);
                yield break;
            }
            msg.Response.Items.AddRange(c.team.TeamList);
            msg.Reply();
        }

        //玩家切换场景的通知
        public IEnumerator SSNotifyPlayerChangeScene(Coroutine coroutine,
                                                     TeamService _this,
                                                     SSNotifyPlayerChangeSceneInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            var s = ServerAllianceManager.GetAllianceByServer(serverId);
            if (s == null)
            {
                yield break;
            }
            var c = s.GetCharacterData(msg.Request.Guid);
            if (c == null)
            {
                yield break;
            }
            c.SceneId = msg.Request.SceneId;
            c.Level = msg.Request.Level;
            var fp = msg.Request.FightPoint;
            if (c.FightPoint != fp)
            {
                var a = ServerAllianceManager.GetAllianceById(c.AllianceId);
                if (a != null)
                {
                    a.TotleFightPointChange(fp - c.FightPoint);
                    //a.SetTotleFightPointFlag();
                }
                c.FightPoint = fp;
            }
        }

        public IEnumerator ReadyToEnter(Coroutine coroutine, TeamService _this, ReadyToEnterInMessage msg)
        {
            if (TeamServer.Instance.IsReadyToEnter && TeamServer.Instance.AllAgentConnected())
            {
                msg.Response = 1;
            }
            else
            {
                msg.Response = 0;
            }

            msg.Reply();

            return null;
        }

        /// <summary>
        ///     获得团购列表
        /// </summary>
        /// <param name="msg.Request.Types">需要申请的团购级别的列表</param>
        /// <param name="msg.Request.Items">Logic上缓存的商品列表，每个商品级别都有一个列表</param>
        /// <param name="coroutine"></param>
        /// <param name="_this"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public IEnumerator SSApplyGroupShopItems(Coroutine coroutine,
                                                 TeamService _this,
                                                 SSApplyGroupShopItemsInMessage msg)
        {
            var characterId = msg.CharacterId;
            var profession = msg.Request.Profession;
            var types = msg.Request.Types.Items;
            var items = msg.Request.Items.Items;
            var response = new GroupShopItemAllForServer();
            response.Items = new GroupShopItemAll();

            var errCode = ErrorCodes.OK;
            for (int i = 0, imax = types.Count; i < imax; i++)
            {
                bool dirty;
                GroupShopItemList itemList;
                var err = GroupShop.GetList(characterId, types[i], profession, items[i].Items, out itemList, out dirty);
                if (err != ErrorCodes.OK)
                {
                    errCode = err;
                }
                if (dirty)
                {
                    response.Dirty = true;
                }
                response.Items.Lists.Add(itemList);
            }
            msg.Response = response;
            msg.Reply((int) errCode);
            yield break;
        }

        public void NotifyQueueResult(TeamServerControl _this, ulong toCharacterId, ulong guid, int type)
        {
            TeamCharacterProxy toCharacterProxy;
            if (_this.Proxys.TryGetValue(toCharacterId, out toCharacterProxy))
            {
                toCharacterProxy.NotifyQueueResult(guid, type);
            }
        }

        public void TeamServerMessage(TeamServerControl _this, ulong guid, int type, string args)
        {
            PlayerLog.WriteLog((int) LogType.TeamServerMessage, "SC->TeamServerMessage characterId={0}, type={1}", guid,
                type);
            TeamCharacterProxy toCharacterProxy;
            if (_this.Proxys.TryGetValue(guid, out toCharacterProxy))
            {
                toCharacterProxy.TeamServerMessage(type, args);
            }
        }

        public IEnumerator NotifyAllianceWarResult(Coroutine coroutine,
                                                   TeamService _this,
                                                   NotifyAllianceWarResultInMessage msg)
        {
            var result = msg.Request.Result;
            var serverId = result.ServerId;
            var winner = result.Winner;
            var err = AllianceWarManager.BattleOver(serverId, winner);
            msg.Reply((int) err);
            yield break;
        }

        public void ApplyTasks(TeamServerControl _this, int id)
        {
            if (id == 0) //event表中 每月0点时候
            {
                AllianceWarManager.SendReward();
            }
            else if (id == 2) //event表中 每月0点时候
            {
                ServerAllianceManager.WeekRefresh();
            }
        }

        public IEnumerator QueryAllianceWarInfo(Coroutine coroutine,
                                                TeamService _this,
                                                QueryAllianceWarInfoInMessage msg)
        {
            var infos = new AllianceWarInfos();
            foreach (var server in ServerAllianceManager.Servers.Values)
            {
                foreach (var data in server.mDBData.Values)
                {
                    var info = new AllianceWarInfo();
                    info.ServerId = data.ServerId;
                    info.OccupantId = data.Occupant;
                    infos.Data.Add(info);
                }
            }
            msg.Response = infos;
            msg.Reply();
            yield break;
        }

        public void Status(TeamServerControl _this, ConcurrentDictionary<string, string> dict)
        {
            try
            {
                dict.TryAdd("_Listening", _this.Listening.ToString());
                dict.TryAdd("Started", _this.Started.ToString());
                //dict.TryAdd("ByteReceivedPerSecond", _this.ByteReceivedPerSecond.ToString());
                //dict.TryAdd("ByteSendPerSecond", _this.ByteSendPerSecond.ToString());
                //dict.TryAdd("MessageReceivedPerSecond", _this.MessageReceivedPerSecond.ToString());
                //dict.TryAdd("MessageSendPerSecond", _this.MessageSendPerSecond.ToString());
                //dict.TryAdd("ConnectionCount", _this.ConnectionCount.ToString());

                //foreach (var agent in TeamServer.Instance.Agents.ToArray())
                //{
                //    dict.TryAdd(agent.Key + " Latency", agent.Value.Latency.ToString());
                //    dict.TryAdd(agent.Key + " ByteReceivedPerSecond", agent.Value.ByteReceivedPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " ByteSendPerSecond", agent.Value.ByteSendPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " MessageReceivedPerSecond", agent.Value.MessageReceivedPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " MessageSendPerSecond", agent.Value.MessageSendPerSecond.ToString());
                //}
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "TeamServerControl Status Error1!{0}");
            }
        }

        public bool IsOnline(TeamServerControl _this, ulong guid)
        {
            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(guid, out toCharacterProxy))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 队伍：自动匹配  logic广播玩家： 是否接受自动入队 flag标记
        /// </summary>
        public IEnumerator SSGetCharacterTeamFlag(Coroutine coroutine, TeamService _this, SSGetCharacterTeamFlagInMessage msg)
        {
            ulong characterId = msg.CharacterId;
            //TeamCharacterProxy toCharacterProxy;
            //if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(characterId, out toCharacterProxy))
            //{
            //    Logger.Error("TeamWorkRefrerrence chracterID=" + characterId + " flag=" + msg.Request.Flag);
            //}

            if (null != AutoMatchManager.teamFlagDic)
            {
                if (AutoMatchManager.teamFlagDic.ContainsKey(characterId))
                {
                    AutoMatchManager.teamFlagDic[characterId] = msg.Request.Flag;
                }
                else
                {
                    AutoMatchManager.teamFlagDic.Add(characterId,msg.Request.Flag);
                }
            }
            return null;
        }
    }


    public interface IStaticTeamServerControl
    {
        void ApplyTasks(TeamServerControl _this, int id);
        bool IsOnline(TeamServerControl _this, ulong guid);
        void NotifyQueueResult(TeamServerControl _this, ulong toCharacterId, ulong guid, int type);
        void Status(TeamServerControl _this, ConcurrentDictionary<string, string> dict);
        void TeamServerMessage(TeamServerControl _this, ulong guid, int type, string args);
    }


    public class TeamServerControl : TeamService
    {
        public static TeamServerControl Instance;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static TimeManager tm = new TimeManager();

        public TeamServerControl()
        {
            Instance = this;

            StaticParam.Init();

            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (TeamServerControl),
                typeof (TeamServerControlDefaultImpl),
                o => { SetServiceImpl((ITeamService) o); });

            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (TeamProxy), typeof (TeamProxyDefaultImpl),
                o => { SetProxyImpl((ITeamCharacterProxy) o); });
        }

        public readonly TimedTaskManager mTimedTaskManager = new TimedTaskManager();
        public int TickCount;
        //事件回调
        public void ApplyTasks(int id)
        {
            ((IStaticTeamServerControl) mImpl).ApplyTasks(this, id);
        }

        public void Init()
        {
        }

        public bool IsOnline(ulong guid)
        {
            return ((IStaticTeamServerControl) mImpl).IsOnline(this, guid);
        }

        public override TeamCharacterProxy NewCharacterIn(ulong characterId, ulong clientId)
        {
            return new TeamProxy(this, characterId, clientId);
        }

        public void NotifyQueueResult(ulong toCharacterId, ulong guid, int type)
        {
            ((IStaticTeamServerControl) mImpl).NotifyQueueResult(this, toCharacterId, guid, type);
        }

        public override void OnException(Exception ex)
        {
            Logger.Error(ex, "Network error");
        }

        public override IEnumerator OnServerStart(Coroutine coroutine)
        {
            return mImpl.OnServerStart(coroutine, this);
        }

        public override IEnumerator OnServerStop(Coroutine coroutine)
        {
            return mImpl.OnServerStop(coroutine, this);
        }

        public override IEnumerator PerformenceTest(Coroutine coroutine, ServerClient client, ServiceDesc desc)
        {
            client.SendMessage(desc);
            yield break;
        }

        public void Status(ConcurrentDictionary<string, string> dict)
        {
            ((IStaticTeamServerControl) mImpl).Status(this, dict);
        }

        public void TeamServerMessage(ulong guid, int type, string args)
        {
            ((IStaticTeamServerControl) mImpl).TeamServerMessage(this, guid, type, args);
        }

        public override IEnumerator Tick(Coroutine coroutine)
        {
            return ((ITickable) mImpl).Tick(coroutine, this);
        }
    }
}