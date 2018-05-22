#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Team
{
    public interface IAllianceWar
    {
		void Init(AllianceWar _this);
        void BattleOver(AllianceWar _this, int occupantId);
        void BattleTimeOver(AllianceWar _this);
        void BidOver(AllianceWar _this);
        ErrorCodes CheckPlayerEnter(AllianceWar _this, int serverId, ulong characterId);
        void Construct(AllianceWar _this, int serverId);

        IEnumerator PlayerEnter(Coroutine co,
                                AllianceWar _this,
                                ulong characterId,
                                int allianceId,
                                AsyncReturnValue<ErrorCodes> err);

        void PlayerEnterSuccess(AllianceWar _this, ulong characterId, int allianceId);
        void PlayerLeave(AllianceWar _this, ulong characterId, int allianceId);
        void StartActivity(AllianceWar _this);
        void StartBid(AllianceWar _this);
        void StartFight(AllianceWar _this);

        int GetStatus(AllianceWar _this);


        void SetStatus(AllianceWar _this,int stat);
        void NotifyAllianceWarInfo(AllianceWar _this, int occupantId);
    }

    public class AllianceWarDefaultImpl : IAllianceWar
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private void ChangeNameTitle(ulong characterId, Dict_int_int_Data change)
        {
            CoroutineFactory.NewCoroutine(ChangeNameTitleCoroutine, characterId, change).MoveNext();
        }

        private IEnumerator ChangeNameTitleCoroutine(Coroutine co, ulong characterId, Dict_int_int_Data change)
        {
            var msg = TeamServer.Instance.LogicAgent.SSSetFlag(characterId, change);
            yield return msg.SendAndWaitUntilDone(co);
        }

        //
        private void ClearData(AllianceWar _this)
        {
            _this.BattleFieldGuid = 0;
            _this.EnterPlayerCount.Clear();
            _this.AlliancePlayers.Clear();
            _this.AllianceIds.Clear();
            foreach (var players in _this.AlliancePlayers.Values)
            {
                players.Clear();
            }
        }
        public void Init(AllianceWar _this)
        {
            foreach (var trigger in _this.WarTrigger)
            {
                if (trigger != null)
                {
                    TeamServerControl.tm.DeleteTrigger(trigger);
                }
            }
            _this.WarTrigger.Clear();

            var tb = Table.GetServerName(_this.ServerId);
            if (1 != tb.IsClientDisplay)
            {
                return;
            }

            var am = ServerAllianceManager.GetAllianceByServer(_this.ServerId);
            if (am == null)
            {
                Logger.Error("StartActivity not find GetAllianceByServer!server={0},", _this.ServerId);
                return;
            }

            DBServerAllianceData data;
            if (!am.mDBData.TryGetValue(_this.ServerId, out data))
            {
                //热更后如果开放新服,这里需要new一个出来
                data = new DBServerAllianceData();
                data.ServerId = _this.ServerId;
                data.Occupant = -1;
                am.mDBData[_this.ServerId] = data;
                am.SetFlag(_this.ServerId);
                NotifyAllianceWarInfo(_this, -1);
                //Logger.Error("In BidOver() can't find data!");
                //return;
            }
            _this.dbData = data;

            //特殊处理开服前11天
            int AllianceWarType = 200;
           // Debug.Assert(false);
            //为攻城战注册一些定时器
            var tbFuben = Table.GetFuben(Constants.AllianceWarDungeonId);
            var now = DateTime.Now;
            //开服时间
            var serverOpenTime = DateTime.Parse(tb.OpenTime);
            var baseTime = DateTime.Now.Date.AddDays(365);
            var TimeToday = DateTime.Now > serverOpenTime ? DateTime.Now : serverOpenTime;
            var KaifuId = (int)(DateTime.Now.Date - serverOpenTime.Date).TotalDays + 1;
            if (KaifuId < 1)
                KaifuId = 1;
            var tbKaifu = Table.GetKaiFu(KaifuId);
            var openWeek = (int)serverOpenTime.DayOfWeek;
            

            //开放精确时间
            var opentime = tbFuben.OpenTime[0];
            var openhour = opentime / 100;
            var openmin = opentime % 100;

            //Debug.Assert(false);

            var nextOpen = DateTime.Now.AddDays(-1);
            if (_this.dbData.OpenTime > 0)
            {
                nextOpen = DateTime.FromBinary(_this.dbData.OpenTime);
            }
            if (nextOpen < DateTime.Now)
            {
                bool b = false;
                while (tbKaifu != null)
                {
                    if (tbKaifu.Week[openWeek] == AllianceWarType &&
                        serverOpenTime.AddDays(KaifuId - 1).Date.AddHours(openhour).AddMinutes(openmin) > DateTime.Now)
                    {
                        b = true;
                        break;
                    }
                    tbKaifu = Table.GetKaiFu(++KaifuId);
                }
                if (b)
                {
//活动中找到了目标
                    baseTime = serverOpenTime.AddDays(KaifuId - 1).Date.AddHours(openhour).AddMinutes(openmin);
                }
                else
                {
//没有找到目标,按照自己的套路每周二六开放
                    var strConf0 = Table.GetServerConfig(901).Value;
                    var confs0 = strConf0.Split('|');
                    var tmp = 7;
                    foreach (var v in confs0)
                    {
                        var n = int.Parse(v);
                        var d = n - (int) DateTime.Now.DayOfWeek;
                        if (d < 0)
                            d += 7;
                        if (d < tmp &&
                            DateTime.Now.AddDays(d).Date.AddHours(openhour).AddMinutes(openmin) > DateTime.Now)
                        {
                            tmp = d;
                        }
                    }
                    baseTime = DateTime.Now.AddDays(tmp).Date.AddHours(openhour).AddMinutes(openmin);
                }
                _this.SetStatus((int)eAllianceWarState.WaitBid);
            }
            else
            {
                baseTime = nextOpen;
            }

           
            //已经获得了开放的标准时间,下面只需要根据这个时间开放不同的计时器就可以了
            //开放竞标计时器  //清楚之前的竞标信息
            var bidTime = baseTime.Date.AddDays(-1);    // 竞标时间(攻城战开始前一天)
            var waitEnterTime = baseTime.Date;  // 等待进入战场时间（攻城战当天零点开始）
            var enterTime = baseTime; // 开始进入时间
            var startTime = baseTime.AddMinutes(tbFuben.OpenLastMinutes);  // 开始战争时间
            var timeOverTime = baseTime.AddMinutes(tbFuben.TimeLimitMinutes + tbFuben.OpenLastMinutes);  // 超时时间

            if ((int)eAllianceWarState.WaitBid == _this.dbData.Status)
            {
                if (DateTime.Now >= bidTime) // 已经过了竞标时间了
                    StartBid(_this);
                else
                    _this.WarTrigger.Add(TeamServerControl.tm.CreateTrigger(bidTime, () => { StartBid(_this); }));
            }

            //每周二周六0点，竞标结束，算出参战公会
            if ((int)eAllianceWarState.Bid == _this.dbData.Status && DateTime.Now >= waitEnterTime)
                BidOver(_this);
            else
                _this.WarTrigger.Add(TeamServerControl.tm.CreateTrigger(waitEnterTime, () => { BidOver(_this); }));

            ////每周二周六攻城战开始进入时，进行相关处理
            _this.WarTrigger.Add(TeamServerControl.tm.CreateTrigger(enterTime, () => { StartActivity(_this); }));
            ////每周二周六攻城战开始打时，进行相关处理
            _this.WarTrigger.Add(TeamServerControl.tm.CreateTrigger(startTime, () => { StartFight(_this); }));
            ////每周二周六攻城战结束后，清理相关数据
            _this.WarTrigger.Add(TeamServerControl.tm.CreateTrigger(timeOverTime, () => { BattleTimeOver(_this); }));

            _this.dbData.OpenTime = baseTime.ToBinary();
            am.SetFlag(_this.ServerId);
        }

        public void NotifyAllianceWarInfo(AllianceWar _this, int occupantId)
        {
            CoroutineFactory.NewCoroutine(NotifyAllianceWarInfoCoroutine, _this, occupantId).MoveNext();
        }

        private IEnumerator NotifyAllianceWarInfoCoroutine(Coroutine co, AllianceWar _this, int occupantId)
        {
            var info = new AllianceWarInfo();
            info.ServerId = _this.ServerId;
            info.OccupantId = occupantId;
            var msg = TeamServer.Instance.LogicAgent.NotifyAllianceWarInfo(info);
            yield return msg.SendAndWaitUntilDone(co);
        }

        public void Construct(AllianceWar _this, int serverId)
        {
            _this.ServerId = serverId;
        }

        //
        public void BidOver(AllianceWar _this)
        {
            var am = ServerAllianceManager.GetAllianceByServer(_this.ServerId);
            if (am == null)
            {
                Logger.Error("StartActivity not find GetAllianceByServer!server={0},", _this.ServerId);
                return;
            }
            DBServerAllianceData data;
            if (!am.mDBData.TryGetValue(_this.ServerId, out data))
            {
                Logger.Error("In BidOver() can't find data!");
                return;
            }
            am.BidOver();
            var msgData = new AllianceWarChallengerData();
            foreach (var id in data.Challengers)
            {
                var allliance = ServerAllianceManager.GetAllianceById(id);
                if (allliance == null)
                {
                    Logger.Error("In BidOver(). alliance == null!! id = {0}", id);
                    continue;
                }
                msgData.ChallengerId.Add(id);
                msgData.ChallengerName.Add(allliance.Name);
            }
            _this.SetStatus((int)eAllianceWarState.WaitEnter);

            var sId = SceneExtension.GetServerLogicId(_this.ServerId);
            TeamServer.Instance.TeamAgent.NotifyAllianceWarChallengerData((uint) sId, msgData);
            PlayerLog.WriteLog((ulong) LogType.AllianceWar, "BidOver(), ServerId = {0}, Challengers = {1}",
                _this.ServerId, data.Challengers.GetDataString());
        }

        public void StartBid(AllianceWar _this)
        {
            PlayerLog.WriteLog((ulong) LogType.AllianceWar, "StartBid(), ServerId = {0}", _this.ServerId);
            _this.SetStatus((int)eAllianceWarState.Bid);
            ClearData(_this);
        }

        public void StartActivity(AllianceWar _this)
        {
            var am = ServerAllianceManager.GetAllianceByServer(_this.ServerId);
            if (am == null)
            {
                Logger.Error("StartActivity not find GetAllianceByServer!server={0},", _this.ServerId);
                return;
            }
            DBServerAllianceData data;
            if (!am.mDBData.TryGetValue(_this.ServerId, out data))
            {
                Logger.Error("In BidOver() can't find data!");
                return;
            }
            PlayerLog.WriteLog((ulong) LogType.AllianceWar, "StartActivity(), ServerId = {0}", _this.ServerId);
            _this.SetStatus((int)eAllianceWarState.WaitStart);
            var now = DateTime.Now;
            _this.StartHour = now.Hour;
            _this.StartMin = now.Minute;

            _this.AllianceIds.Clear();
            _this.AlliancePlayers.Clear();
            _this.EnterPlayerCount.Clear();
            _this.AllianceIds.Add(data.Occupant);
            _this.AllianceIds.AddRange(data.Challengers);
            foreach (var id in _this.AllianceIds)
            {
                _this.AlliancePlayers.Add(id, new List<ulong>());
                _this.EnterPlayerCount.Add(id, 0);
            }
        }

        public void StartFight(AllianceWar _this)
        {
            PlayerLog.WriteLog((ulong) LogType.AllianceWar, "StartFight(), ServerId = {0}", _this.ServerId);
            _this.SetStatus((int)eAllianceWarState.Fight);
        }
        public int GetStatus(AllianceWar _this)
        {
            if (_this.dbData != null)
                return _this.dbData.Status;
            return 0;
        }

        public void SetStatus(AllianceWar _this, int stat)
        {
            if (_this.dbData == null)
                return;
            _this.dbData.Status = stat;
            var allianceManager = ServerAllianceManager.GetAllianceByServer(_this.ServerId);
            if(allianceManager != null)
                allianceManager.SetFlag(_this.ServerId);
        }
        //战斗结束
        public void BattleOver(AllianceWar _this, int occupantId)
        {
            _this.SetStatus((int)eAllianceWarState.WaitBid);

            var allianceManager = ServerAllianceManager.GetAllianceByServer(_this.ServerId);
            var dbData = allianceManager.GetServerData(_this.ServerId);
            var oldOccupant = dbData.Occupant;
            dbData.Challengers.Clear();
            dbData.LastBattleTime = DateTime.Now.ToBinary();
            allianceManager.SetFlag(_this.ServerId);

            PlayerLog.WriteLog((ulong) LogType.AllianceWar,
                "BattleOver(), ServerId = {0}, occupantId = {1}, old occupantId = {2}", _this.ServerId, occupantId,
                dbData.Occupant);

            //如果城主没变就返回
            if (dbData.Occupant == occupantId)
            {
                return;
            }

            dbData.Occupant = occupantId;
            var alliance = ServerAllianceManager.GetAllianceById(occupantId);
            if (alliance != null)
            {
                //通知全服，城主变了
                var data = new AllianceWarOccupantData();
                data.OccupantId = occupantId;
                data.OccupantName = alliance.Name;
                TeamServer.Instance.TeamAgent.NotifyAllianceWarOccupantData((uint) _this.ServerId, data);

                //修改玩家的称号
                var change0 = new Dict_int_int_Data();
                var tbTitle = Table.GetNameTitle(5000);
                change0.Data.Add(tbTitle.FlagId, 1);

                var change1 = new Dict_int_int_Data();
                tbTitle = Table.GetNameTitle(5001);
                change1.Data.Add(tbTitle.FlagId, 1);

                var dad = alliance.Dad;
                foreach (var id in alliance.mDBData.Members)
                {
                    var m = dad.GetCharacterData(id);
                    if (m == null)
                    {
                        continue;
                    }
                    if (m.Ladder == (int) eAllianceLadder.Chairman)
                    {
                        ChangeNameTitle(id, change0);
                    }
                    else
                    {
                        ChangeNameTitle(id, change1);
                    }
                }
            }

            //修改玩家的称号
            alliance = ServerAllianceManager.GetAllianceById(oldOccupant);
            if (alliance != null)
            {
                var change = new Dict_int_int_Data();
                var tbTitle = Table.GetNameTitle(5000);
                change.Data.Add(tbTitle.FlagId, 0);
                tbTitle = Table.GetNameTitle(5001);
                change.Data.Add(tbTitle.FlagId, 0);
                foreach (var id in alliance.mDBData.Members)
                {
                    ChangeNameTitle(id, change);
                }
            }

            //通知logic，本服的王城占领者变了
            NotifyAllianceWarInfo(_this, occupantId);
            Init(_this);
        }

        //战斗结束
        public void BattleTimeOver(AllianceWar _this)
        {
            PlayerLog.WriteLog((ulong) LogType.AllianceWar, "BattleTimeOver(), ServerId = {0}", _this.ServerId);
            _this.SetStatus((int)eAllianceWarState.WaitBid);

            var allianceManager = ServerAllianceManager.GetAllianceByServer(_this.ServerId);
            if (allianceManager != null)
            {
                var dbData = allianceManager.GetServerData(_this.ServerId);
                if (dbData != null)
                {
                    dbData.Challengers.Clear();
                    dbData.LastBattleTime = DateTime.Now.ToBinary();
                }
                allianceManager.SetFlag(_this.ServerId);
            }

            ClearData(_this);
            Init(_this);
        }

        public ErrorCodes CheckPlayerEnter(AllianceWar _this, int serverId, ulong characterId)
        {
            var allianceManager = ServerAllianceManager.GetAllianceByServer(serverId);
            var character = allianceManager.GetCharacterData(characterId);
            if (character.Level < StaticParam.AllianceWarLevelLimit)
            {
                return ErrorCodes.Error_LevelNoEnough;
            }
            var now = DateTime.Now;
            var serverData = allianceManager.GetServerData(serverId);
            var lastTime = DateTime.FromBinary(serverData.LastBattleTime);
            if (lastTime.Year == now.Year && lastTime.DayOfYear == now.DayOfYear)
            {
                return ErrorCodes.Error_AllianceWarOver;
            }
            if (serverData.Challengers.Count == 0)
            {
//无人报名，本次城战取消
                return ErrorCodes.Error_AllianceWarCancel;
            }
            return ErrorCodes.OK;
        }

        public IEnumerator PlayerEnter(Coroutine co,
                                       AllianceWar _this,
                                       ulong characterId,
                                       int allianceId,
                                       AsyncReturnValue<ErrorCodes> err)
        {
            err.Value = ErrorCodes.OK;
            if (!_this.AllianceIds.Contains(allianceId))
            {
                Logger.Error("In CheckPlayerEnter().!AllianceIds.Contains(allianceId), characterId = {0}", characterId);
                err.Value = ErrorCodes.Error_AllianceWarQualification;
                yield break;
            }
            if (_this.GetStatus() < (int)eAllianceWarState.WaitStart)
            {
                err.Value = ErrorCodes.Error_FubenNotInOpenTime;
                yield break;
            }

            //锁，避免多进入，且在等待进入的人数不足50时，不锁
            var count = _this.AlliancePlayers[allianceId].Count;
            while (count + _this.EnterPlayerCount[allianceId] >= Constants.AllianceMaxPlayer)
            {
                if (count >= Constants.AllianceMaxPlayer)
                {
                    err.Value = ErrorCodes.Error_AllianceWarFull;
                    yield break;
                }
                yield return TeamServer.Instance.ServerControl.Wait(co, TimeSpan.FromMilliseconds(50));
                count = _this.AlliancePlayers[allianceId].Count;
            }

            ++_this.EnterPlayerCount[allianceId];

            var sceneId = -1;
            var sceneParam = new SceneParam();
            if (_this.BattleFieldGuid == 0)
            {
                _this.BattleFieldGuid = ulong.MaxValue;
                sceneId = Constants.AllianceWarSceneId;
                sceneParam.Param.Add(_this.StartHour);
                sceneParam.Param.Add(_this.StartMin);
                sceneParam.Param.AddRange(_this.AllianceIds);
                while (sceneParam.Param.Count < 5)
                {
                    sceneParam.Param.Add(-1);
                }
            }
            else if (_this.BattleFieldGuid == ulong.MaxValue)
            {
                do
                {
                    yield return TeamServer.Instance.ServerControl.Wait(co, TimeSpan.FromMilliseconds(50));
                } while (_this.BattleFieldGuid == ulong.MaxValue);
            }

            //帮战根据合服ID进行
            var serverLogicId = SceneExtension.GetServerLogicId(_this.ServerId);
            var msg = TeamServer.Instance.SceneAgent.SBChangeScene(characterId, characterId, serverLogicId, sceneId,
                _this.BattleFieldGuid, (int) eScnenChangeType.EnterDungeon, sceneParam);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply)
            {
                Logger.Error("In PlayerEnterCoroutine().SBChangeScene not replied!");
                err.Value = ErrorCodes.Unknow;
                yield break;
            }
            if (msg.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("In PlayerEnterCoroutine().SBChangeScene error = {0}", msg.ErrorCode);
                err.Value = (ErrorCodes) msg.ErrorCode;
                yield break;
            }
            if (_this.BattleFieldGuid == ulong.MaxValue)
            {
                _this.BattleFieldGuid = msg.Response;
            }

            PlayerLog.WriteLog((ulong) LogType.AllianceWar,
                "In PlayerEnter() player entered, characterId = {0}, allianceId = {1}", characterId, allianceId);
            err.Value = ErrorCodes.OK;
        }

        public void PlayerEnterSuccess(AllianceWar _this, ulong characterId, int allianceId)
        {
            PlayerLog.WriteLog((ulong) LogType.AllianceWar,
                "In PlayerEnterSuccess(), characterId = {0}, allianceId = {1}", characterId, allianceId);
            --_this.EnterPlayerCount[allianceId];
            _this.AlliancePlayers[allianceId].Add(characterId);
        }

        public void PlayerLeave(AllianceWar _this, ulong characterId, int allianceId)
        {
            PlayerLog.WriteLog((ulong) LogType.AllianceWar,
                "In PlayerLeave(), characterId = {0}, allianceId = {1}", characterId, allianceId);
            _this.AlliancePlayers[allianceId].Remove(characterId);
        }
    }

    public class AllianceWar
    {
        private static IAllianceWar mImpl;

        static AllianceWar()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (AllianceWar), typeof (AllianceWarDefaultImpl),
                o => { mImpl = (IAllianceWar) o; });
        }

        public AllianceWar(int serverId)
        {
            mImpl.Construct(this, serverId);
        }

        public int StartHour;
        public int StartMin;
        //战斗结束
        public void BattleOver(int occupantId)
        {
            mImpl.BattleOver(this, occupantId);
        }
		public void Init()
		{
			mImpl.Init(this);
		}
        //战斗结束
        public void BattleTimeOver()
        {
            mImpl.BattleTimeOver(this);
        }

        public void NotifyAllianceWarInfo(int occupantId)
        {
            mImpl.NotifyAllianceWarInfo(this, occupantId);
        }
        //
        public void BidOver()
        {
            mImpl.BidOver(this);
        }

        public ErrorCodes CheckPlayerEnter(int serverId, ulong characterId)
        {
            return mImpl.CheckPlayerEnter(this, serverId, characterId);
        }

        public IEnumerator PlayerEnter(Coroutine co, ulong characterId, int allianceId, AsyncReturnValue<ErrorCodes> err)
        {
            return mImpl.PlayerEnter(co, this, characterId, allianceId, err);
        }

        public void PlayerEnterSuccess(ulong characterId, int allianceId)
        {
            mImpl.PlayerEnterSuccess(this, characterId, allianceId);
        }

        public void PlayerLeave(ulong characterId, int allianceId)
        {
            mImpl.PlayerLeave(this, characterId, allianceId);
        }

        public void StartActivity()
        {
            mImpl.StartActivity(this);
        }

        public void StartBid()
        {
            mImpl.StartBid(this);
        }

        public void StartFight()
        {
            mImpl.StartFight(this);
        }

        public int GetStatus()
        {
            return mImpl.GetStatus(this);
        }

        public void SetStatus(int stat)
        {
            mImpl.SetStatus(this,stat);
        }
        #region 数据

        public int ServerId;
        public List<int> AllianceIds = new List<int>();
        public Dictionary<int, List<ulong>> AlliancePlayers = new Dictionary<int, List<ulong>>();
        public Dictionary<int, int> EnterPlayerCount = new Dictionary<int, int>();
        public ulong BattleFieldGuid;
//		public eAllianceWarState State = eAllianceWarState.WaitBid;
        public DBServerAllianceData dbData = null;
		public List<Trigger> WarTrigger = new List<Trigger>();
        //public List<Trigger> StartBidTrigger = new List<Trigger>();
        //public List<Trigger> BidOverTrigger = new List<Trigger>();
        //public List<Trigger> StartActivityTrigger = new List<Trigger>();
        //public List<Trigger> StartFightTrigger = new List<Trigger>();
        //public List<Trigger> BattleTimeOverTrigger = new List<Trigger>();

        #endregion
    }

    public interface IAllianceWarManager
    {
        ErrorCodes BattleOver(int serverId, int occupantId);
        void BattleTimeOver();
        void BidOver();
        void Init();
        void Init(int serverId);
        void PlayerEnterSuccess(ulong characterId);
        void PlayerLeave(ulong characterId);
        void SendReward();
        void StartActivity();
        void StartBid();
        void StartFight();
    }

    public class AllianceWarManagerDefaultImpl : IAllianceWarManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void ReloadTable(IEvent ievent)
        {
            var datas = new Dictionary<int, AllianceWar>();
            Table.ForeachServerName(record =>
            {
                if (record.IsClientDisplay != 1 || record.LogicID != record.Id)
                    return true;
                var serverId = record.LogicID;
                AllianceWar war;
                if (!AllianceWarManager.WarDatas.TryGetValue(serverId, out war))
                {
                    war = new AllianceWar(serverId);
                    AllianceWarManager.WarDatas.Add(serverId,war);
                    war.Init();
                    war.NotifyAllianceWarInfo(war.dbData.Occupant);
                }
                return true;
            });
        }

        //private static void ResetForServerConfig()
        //{
        //    var strConf0 = Table.GetServerConfig(901).Value;
        //    var confs0 = strConf0.Split('|');
        //    AllianceWarManager.FightWeekDay.ReSetAllFlag();
        //    foreach (var s in confs0)
        //    {
        //        var weekDay = int.Parse(s);
        //        AllianceWarManager.FightWeekDay.SetFlag(weekDay);
        //    }
        //    AllianceWarManager.BidMin = Table.GetServerConfig(903).ToInt();
        //}

        private static void ResetForServerName()
        {
            var datas = new Dictionary<int, AllianceWar>();
            Table.ForeachServerName(record =>
            {
                if (record.IsClientDisplay != 1 || record.LogicID != record.Id)
                    return true;
                var serverId = record.LogicID;
                AllianceWar war;
                if (!AllianceWarManager.WarDatas.TryGetValue(serverId, out war))
                {
                    war = new AllianceWar(serverId);
                }
                if (!datas.ContainsKey(serverId))
                {
                    datas.Add(serverId, war);
                }
                return true;
            });
            AllianceWarManager.WarDatas = datas;
        }

        private void SendMailToCharacter(ulong characterId, int mailId, ItemBaseData item, StringArray args)
        {
            CoroutineFactory.NewCoroutine(SendMailToCharacterCoroutine, characterId, mailId, item, args).MoveNext();
        }

        private IEnumerator SendMailToCharacterCoroutine(Coroutine co,
                                                         ulong characterId,
                                                         int mailId,
                                                         ItemBaseData item,
                                                         StringArray args)
        {
            var msg = TeamServer.Instance.LogicAgent.SendMailToCharacterByItems(characterId, mailId, item, args);
            yield return msg.SendAndWaitUntilDone(co);
        }

        public void Init()
        {
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
            ResetForServerName();
        }

		

        public void Init(int serverId)
        {
            AllianceWar tmp;
            if (false == AllianceWarManager.WarDatas.TryGetValue(serverId, out tmp))
            {
                tmp = new AllianceWar(serverId);
            }
            tmp.Init();
        }

        //竞标开始
        public void StartBid()
        {
            foreach (var war in AllianceWarManager.WarDatas.Values)
            {
                war.StartBid();
            }
        }

        //竞标结束
        public void BidOver()
        {
            foreach (var war in AllianceWarManager.WarDatas.Values)
            {
                war.BidOver();
            }
        }

        public void StartActivity()
        {
            foreach (var war in AllianceWarManager.WarDatas.Values)
            {
                war.StartActivity();
            }
        }

        public void StartFight()
        {
            foreach (var war in AllianceWarManager.WarDatas.Values)
            {
                war.StartFight();
            }
        }

        //战斗结束
        public ErrorCodes BattleOver(int serverId, int occupantId)
        {
            serverId = SceneExtension.GetServerLogicId(serverId);
            AllianceWar war;
            if (!AllianceWarManager.WarDatas.TryGetValue(serverId, out war))
            {
                return ErrorCodes.ServerID;
            }
            war.BattleOver(occupantId);
            return ErrorCodes.OK;
        }

        //活动结束，如果攻城战没打，也需要清理数据
        public void BattleTimeOver()
        {
            foreach (var server in ServerAllianceManager.Servers.Values)
            {
                foreach (var data in server.mDBData.Values)
                {
                    data.BidDatas.Clear();
                    data.Challengers.Clear();
                    server.SetFlag(data.ServerId);
                }
            }
            foreach (var war in AllianceWarManager.WarDatas.Values)
            {
                war.BattleTimeOver();
            }
        }

        public void PlayerEnterSuccess(ulong characterId)
        {
            var alliance = ServerAllianceManager.GetAllianceByCharacterId(characterId);
            if (alliance == null)
            {
                Logger.Error("In PlayerEnterSuccess(). alliance == null! characterId = {0}", characterId);
                return;
            }
            var serverId = alliance.ServerId;
            serverId = SceneExtension.GetServerLogicId(serverId);
            var war = AllianceWarManager.WarDatas[serverId];
            war.PlayerEnterSuccess(characterId, alliance.AllianceId);
        }

        public void PlayerLeave(ulong characterId)
        {
            var alliance = ServerAllianceManager.GetAllianceByCharacterId(characterId);
            if (alliance == null)
            {
                Logger.Error("In PlayerEnterSuccess(). alliance == null! characterId = {0}", characterId);
                return;
            }
            var serverId = alliance.ServerId;
            serverId = SceneExtension.GetServerLogicId(serverId);
            var war = AllianceWarManager.WarDatas[serverId];
            war.PlayerLeave(characterId, alliance.AllianceId);
        }

        //发放每日奖励
        public void SendReward()
        {
            var item = new ItemBaseData();
            item.ItemId = -1;
            var args = new StringArray();
            foreach (var server in ServerAllianceManager.Servers.Values)
            {
                foreach (var data in server.mDBData.Values)
                {
                    var alliance = ServerAllianceManager.GetAllianceById(data.Occupant);
                    if (alliance == null)
                    {
                        continue;
                    }
                    var serverAlliance = alliance.Dad;
                    foreach (var id in alliance.mDBData.Members)
                    {
                        var member = serverAlliance.GetCharacterData(id);
                        if (member == null)
                        {
                            continue;
                        }
                        var tbGA = Table.GetGuildAccess(member.Ladder);
                        if (tbGA == null)
                        {
                            Logger.Error("In SendReward().tbGA == null");
                            continue;
                        }
                        SendMailToCharacter(id, tbGA.MailId, item, args);
                    }
                }
            }
        }
    }

    public static class AllianceWarManager
    {
        private static IAllianceWarManager mImpl;

        static AllianceWarManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (AllianceWarManager),
                typeof (AllianceWarManagerDefaultImpl),
                o => { mImpl = (IAllianceWarManager) o; });
        }

        #region 数据

        public static Dictionary<int, AllianceWar> WarDatas = new Dictionary<int, AllianceWar>();
//        public static BitFlag FightWeekDay = new BitFlag(7);
        public static int BidMin;

        #endregion

        #region 对外接口

        public static void Init()
        {
            mImpl.Init();
        }

        public static void Init(int ServerId)
        {
            mImpl.Init(ServerId);
        }

        //竞标开始
        public static void StartBid()
        {
            mImpl.StartBid();
        }

        //竞标结束
        public static void BidOver()
        {
            mImpl.BidOver();
        }

        public static void StartActivity()
        {
            mImpl.StartActivity();
        }

        public static void StartFight()
        {
            mImpl.StartFight();
        }

        //战斗结束
        public static ErrorCodes BattleOver(int serverId, int occupantId)
        {
            return mImpl.BattleOver(serverId, occupantId);
        }

        //活动结束，如果攻城战没打，也需要清理数据
        public static void BattleTimeOver()
        {
            mImpl.BattleTimeOver();
        }

        public static void PlayerEnterSuccess(ulong characterId)
        {
            mImpl.PlayerEnterSuccess(characterId);
        }

        public static void PlayerLeave(ulong characterId)
        {
            mImpl.PlayerLeave(characterId);
        }

        //发放每日奖励
        public static void SendReward()
        {
            mImpl.SendReward();
        }

        #endregion
    }
}