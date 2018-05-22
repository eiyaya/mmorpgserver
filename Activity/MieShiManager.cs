#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Database;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Activity
{
    public interface IMieShi
    {
        void Construct(MieShiActiGroup _this, int serverId);
        void SwtichActivityState(MieShiActiGroup _this, int activityId, int state,int result);
    }

    public class MieShiDefaultImpl : IMieShi
    {

        [Updateable("Mieshi")]
        public const string DbKey = "MieShi:";
        private static readonly Logger Logger = LogManager.GetLogger("MieShi");

        public void Construct(MieShiActiGroup _this, int serverId)
        {
            _this.BatteryTrigger = new Dictionary<int, Trigger>();
            _this.HadInPlayerDic = new List<ulong>();
            _this.CanInPlayerDic = new List<ulong>();
            _this.PlayerApplyDic = new List<ulong>();
            _this.OpenTriggerDic = null;
            _this.CanInTriggerDic = null;
            _this.StartTriggerDic = null;
            _this.EndTriggerDic = null;
            _this.CanPickUpBoxNpcList = new List<int>();
            _this.HadPickedPlayerList = new List<ulong>();

            _this.ServerId = serverId;
            GetDbActiData(_this);
            //定时保存
            if (MieShiManager.SaveTrigger == null)
            {
                MieShiManager.SaveTrigger = ActivityServerControl.Timer.CreateTrigger(DateTime.Now.AddMinutes(1),
                    () => Save(_this), 60000);
            }

            //开始贡献数据排行
            _this.RankDirty = false;
            _this.StartRankContriList();
        }

        public void SwtichActivityState(MieShiActiGroup _this, int activityId, int state,int result)
        {
            switch (state)
            {
                case (int)eMieShiState.WaitNext:
                    MieShiEnd(_this, activityId,result);
                    break;
                default:
                    break;
            }
        }

        #region DB
        public void SetActiData(MieShiActiGroup _this, DBGroupActivityData data)
        {
            if (data == null)
                return;
            _this.DBData = data;
            var tbServerCfg = Table.GetServerConfig(945);
            if (null == tbServerCfg || 0 == tbServerCfg.ToInt())
            {
                return;
            }

            {
                _this.DBData.Data.activityId = tbServerCfg.ToInt();
                _this.DBData.ActivityId = tbServerCfg.ToInt();
            }

        }

        private void Save(MieShiActiGroup oData)
        {
            if (MieShiManager.SaveTrigger == null)
            {
                return;
            }
            CoroutineFactory.NewCoroutine(SaveCoroutine, oData).MoveNext();
        }

        private IEnumerator SaveCoroutine(Coroutine co, MieShiActiGroup oData)
        {
            if (oData == null || oData.DBData == null)
                yield break;
            var ret = ActivityServer.Instance.DB.Set(co, DataCategory.MieShiActivity, DbKey + oData.ServerId, oData.DBData);
            yield return ret;
        }

        private void GetDbActiData(MieShiActiGroup _this)
        {
            CoroutineFactory.NewCoroutine(GetDbActiDataCoroutine, _this).MoveNext();
        }

        private IEnumerator GetDbActiDataCoroutine(Coroutine co, MieShiActiGroup _this)
        {
            var dbActiList = ActivityServer.Instance.DB.Get<DBGroupActivityData>(co, DataCategory.MieShiActivity,
                DbKey + _this.ServerId);
            yield return dbActiList;
            if (dbActiList.Status != DataStatus.Ok)
            {
                Logger.Fatal("GetDbActiData get data from db faild!");
                yield break;
            }
            if (dbActiList.Data != null)
            {
                SetActiData(_this, dbActiList.Data);
            }
            else
            {
                InitData(_this);
            }

            RefreshActivityData(_this);
        }
        #endregion

        #region 对外方法

        #endregion

        #region 私有方法
        private void InitData(MieShiActiGroup _this)
        {//改成只存在一个活动实例
            _this.DBData = new DBGroupActivityData();
            _this.DBData.passFirstRound = false;
            _this.DBData.LastResult = -1;
            _this.DBData.LastEndTime = DateTime.MinValue.ToBinary();
            _this.DBData.ActivityId = 1;
            var tb = Table.GetMieShi(1);
            if (tb == null)
                return;
            var actiOne = new DBCommonActivityData();
            actiOne.activityId = 1;
            actiOne.totalContir = 0;
            actiOne.lastTimeResult = -1;
            for (int i = 1; i <= 6; i++)
            {
                var batteryOne = new ActivityBatteryOne();
                batteryOne.batteryId = i;
                batteryOne.level = 1;
                batteryOne.curMaxHP = 0;
                batteryOne.skillLevel = 1;
                batteryOne.batteryGuid = 0;
                batteryOne.skillLvlEndTime = 0;
                batteryOne.promoteCount = 0;
                actiOne.batterys.Add(batteryOne);
            }
            _this.DBData.Data = actiOne;
        }

        private void ClearData(MieShiActiGroup _this, int activityId,bool bFirst)
        {
            if (_this.BatteryTrigger.Count > 0)
            {
                _this.BatteryTrigger.Clear();

            }
            if (_this.PlayerApplyDic.Count > 0)
            {
                _this.PlayerApplyDic.Clear();
            }
            _this.HadInPlayerDic.Clear();
            _this.CanPickUpBoxNpcList.Clear();
            _this.DBData.characters.Clear();
            if(bFirst)
            {
                _this.DBData.LastResult = -99;
                _this.DBData.portrait = null;
               
            }
           
            
            if (_this.DBData.Data != null)
            {
                _this.DBData.Data.applyDatas.Clear();
                _this.DBData.Data.batteryPoints.Clear();
                _this.DBData.Data.contriDatas.Clear();
                _this.DBData.Data.totalContir = 0;
                _this.DBData.Data.pointDatas.Clear();
                for (int j = 0; j < _this.DBData.Data.batterys.Count; j++)
                {
                    _this.DBData.Data.batterys[j].batteryGuid = 0;
                    _this.DBData.Data.batterys[j].curMaxHP = 0;
                    _this.DBData.Data.batterys[j].level = 1;
                    _this.DBData.Data.batterys[j].maxHP = 0;
                    _this.DBData.Data.batterys[j].promoteCount = 0;
                    _this.DBData.Data.batterys[j].skillLevel = 1;
                    _this.DBData.Data.batterys[j].skillLvlEndTime = 0;
                }
            }
            
            //清空数据后立即进行存库
            Save(_this);
        }

        private void ClearBatteryData(MieShiActiGroup _this)
        {
            if (_this.DBData.Data != null)
            {
                for (int j = 0; j < _this.DBData.Data.batterys.Count; j++)
                {
                    _this.DBData.Data.batterys[j].batteryGuid = 0;
                    _this.DBData.Data.batterys[j].curMaxHP = 0;
                    _this.DBData.Data.batterys[j].level = 1;
                    _this.DBData.Data.batterys[j].maxHP = 0;
                    _this.DBData.Data.batterys[j].promoteCount = 0;
                    _this.DBData.Data.batterys[j].skillLevel = 1;
                    _this.DBData.Data.batterys[j].skillLvlEndTime = 0;                    
                }

            }
        }

        private bool CheckServerMatch(int serverId, string[] serverList)
        {
           if (serverList == null)
               return false;
           for (int i = 0; i < serverList.Count(); i++)
           {
               if (serverId == int.Parse(serverList[i]))
               {
                   return true;
               }
           }
            return false;
        }

        //private static bool b = false;
        private static DateTime GetActiveTime(int activityId,DateTime ServerOpenDate,bool bResult = false)
        {//活动ID,第几天
            //Debug.Assert(b);
            //b = true;
            DateTime openTime = DateTime.Now.AddDays(365);

            if (bResult == false)
            {
                var record = Table.GetMieShi(activityId);
                if (record == null)
                    return openTime;

                var week = (int) ServerOpenDate.DayOfWeek;
                if (week == 0)
                    week = 7;

                var tbActivity = Table.GetMainActivity(week);
                if (tbActivity == null)
                    return openTime;


                for (int i = 0; i < 7; i++)
                {
                    week = (int) DateTime.Now.AddDays(i).DayOfWeek;
                    int id = tbActivity.Week[week];
                    if (id != 1)
                    {
//1是灭世2是野外争夺
                        continue;
                    }
                    DateTime t = DateTime.Now.AddDays(i).Date + TimeSpan.Parse(record.OpenTime);
                    if (t > DateTime.Now)
                    {
                        openTime = t;
                        break;
                    }
                }
            }
            else
            {
                openTime = DateTime.Now.AddSeconds(-DateTime.Now.Second).AddMinutes(10);
            }
         
            return openTime;
        }

        private void RefreshActivityData(MieShiActiGroup _this)
        {
            //Debug.Assert(false);
            var tbMieShiPublic = Table.GetMieShiPublic(1);

            var serverInfo = Table.GetServerName(_this.ServerId);
            DateTime ServerOpenDate = DateTime.Parse(serverInfo.OpenTime);
            if (_this.DBData.Data == null)
                return;
            //遍历保存的活动
            var actiOne = _this.DBData.Data;
            {
                var record = Table.GetMieShi(actiOne.activityId);
                if (null == record)
                    return;

                var tbFuben = Table.GetFuben(record.FuBenID);
                if (null == tbFuben)
                    return;
                DateTime openTime = GetActiveTime(actiOne.activityId,ServerOpenDate);
#if DEBUG
                //openTime = DateTime.Now.AddMinutes(5);
#endif
                //计算活动开启状态
                var diffTime = openTime - DateTime.Now;

                {//添加计时器 开启报名或者可进入
                    actiOne.actiTime = (ulong)openTime.ToBinary();
                    if ((int)diffTime.TotalSeconds <= tbMieShiPublic.CanApplyTime * 60)
                    {//开服时间已经在可报名时间内的话
                        actiOne.state = (int)eMieShiState.Open;
                        //创建可进入倒计时
                        var trigger =
                            ActivityServerControl.Timer.CreateTrigger(
                                DateTime.FromBinary((long)actiOne.actiTime), () => MieShiCanIn(_this, actiOne.activityId));
                        _this.CanInTriggerDic = trigger;
                    }
                    else
                    {
                        actiOne.state = (int)eMieShiState.WaitNext;
                        //创建报名倒计时
                        var trigger =
                            ActivityServerControl.Timer.CreateTrigger(
                                DateTime.FromBinary((long)actiOne.actiTime).AddMinutes(-tbMieShiPublic.CanApplyTime), () => MieShiOpen(_this, actiOne.activityId,true));
                        _this.OpenTriggerDic = trigger;
                    }
                }

                if (actiOne.state >= (int)eMieShiState.Open && actiOne.state <= (int)eMieShiState.Start)
                {
                    //计算世界等级
                    CaculateWorldLevel(_this.ServerId, actiOne.activityId);
                }
                _this.NearlyActiId = actiOne.activityId;
                //修复旧数据的错误
                for (int i = 0; i < actiOne.batterys.Count; i++)
                {
                    if (actiOne.batterys[i].curMaxHP > actiOne.batterys[i].maxHP)
                    {
                        actiOne.batterys[i].curMaxHP = actiOne.batterys[i].maxHP;
                    }
                    if (actiOne.state != (int)eMieShiState.CanIn && actiOne.batterys[i].batteryGuid > 0)
                    {
                        actiOne.batterys[i].batteryGuid = 0;
                    }
                    if (actiOne.state == (int)eMieShiState.WaitNext)
                    {
                        actiOne.batterys[i].batteryGuid = 0;
                        actiOne.batterys[i].curMaxHP = 0;
                        actiOne.batterys[i].level = 1;
                        actiOne.batterys[i].maxHP = 0;
                        actiOne.batterys[i].promoteCount = 0;
                        actiOne.batterys[i].skillLevel = 1;
                        actiOne.batterys[i].skillLvlEndTime = 0;
                    }
                }
                if (actiOne.state == (int)eMieShiState.WaitNext)
                {
                    actiOne.batteryPoints.Clear();
                    actiOne.applyDatas.Clear();
                    actiOne.contriDatas.Clear();
                    actiOne.totalContir = 0;
                }
                else if (actiOne.state >= (int)eMieShiState.Open && actiOne.state <= (int)eMieShiState.WillEnd)
                {
                    if (_this.DBData.portrait != null)
                    {
                        _this.DBData.portrait = null;
                        _this.DBData.characters.Clear();
                    }
                }
                //初始化最高人数
                var tbScene = Table.GetScene(tbFuben.Id);
                if (tbScene != null)
                {
                    _this.MaxCanInCount = tbScene.PlayersMaxB;
                }
                //初始化上次最佳积分数据
                if (actiOne.lastBestInfo == null)
                {
                    actiOne.lastBestInfo = new RankingInfoOne();
                }
            }
            
            //初始化雕像数据
            if (_this.DBData.portrait == null)
            {
                _this.DBData.portrait = new PlayerInfoMsg();
            }
        }
        private void MieShiOpen(MieShiActiGroup _this, int activityId, bool bFirst)   // bool 参数 是否当日首次进入 首次清除雕像,不是的话不清除
        {
            var activity = MieShiManager.GetCommonActivityData(_this.ServerId, activityId);
            if (activity.state == (int)eMieShiState.Open)
                return; //已经开放
            activity.state = (int)eMieShiState.Open; 
            MieShiManager.SetActivityState(_this.ServerId, activityId, (int)eMieShiState.Open);
            //广播活动状态
            ActivityServer.Instance.ServerControl.NotifyMieShiActivityState((uint)_this.ServerId, activityId,
                activity.state);
            //清除历史数据
            ClearData(_this, activityId,bFirst);


            {
                CommonActivityData msg = new CommonActivityData();
                msg.Datas = new CommonActivityInfo();

                msg.Datas.activityId = activity.activityId;
                msg.Datas.actiTime = activity.actiTime;
                msg.Datas.state = activity.state;
                msg.Datas.applyState = false;
                foreach (var item in activity.batterys)
                {
                    msg.Datas.batterys.Add(item);
                }
                msg.currentActivityId = activityId;
                ActivityServer.Instance.ServerControl.NotifyMieShiActivityInfo((uint)_this.ServerId, msg);
            }
            if (_this.OpenTriggerDic != null)
            {
                ActivityServerControl.Timer.DeleteTrigger(_this.OpenTriggerDic);
                _this.OpenTriggerDic = null;
            }
            MieShiCanPromote(_this, activityId);
        }

        private void MieShiCanPromote(MieShiActiGroup _this, int activityId)
        {
            var activity = MieShiManager.GetCommonActivityData(_this.ServerId, activityId);

            //计算世界等级
            CaculateWorldLevel(_this.ServerId, activityId);
            DateTime t = DateTime.FromBinary((long) activity.actiTime);
            if (_this.CanInTriggerDic != null)
            {
                ActivityServerControl.Timer.DeleteTrigger(_this.CanInTriggerDic);
                _this.CanInTriggerDic = null;
            }
            _this.CanInTriggerDic = ActivityServerControl.Timer.CreateTrigger(t, () => MieShiCanIn(_this, activityId));
        }

        private void MieShiCanIn(MieShiActiGroup _this, int activityId)
        {
            var tbFuben = Table.GetFuben(Table.GetMieShi(activityId).FuBenID);
            var activity = MieShiManager.GetCommonActivityData(_this.ServerId, activityId);
            if (activity.state == (int)eMieShiState.CanIn)
                return;
            MieShiManager.SetActivityState(_this.ServerId, activityId, (int)eMieShiState.CanIn);
            //广播活动状态
            ActivityServer.Instance.ServerControl.NotifyMieShiActivityState((uint)_this.ServerId, activityId,
                activity.state);
            //计算并且广播可以进入
            StartBroadcastCanIn(_this, activityId);

            if (_this.CanInTriggerDic != null)
            {
                ActivityServerControl.Timer.DeleteTrigger(_this.CanInTriggerDic);
                _this.CanInTriggerDic = null;
            }
            if (_this.StartTriggerDic != null)
            {
                ActivityServerControl.Timer.DeleteTrigger(_this.StartTriggerDic);
                _this.StartTriggerDic = null;
            }

            _this.StartTriggerDic = ActivityServerControl.Timer.CreateTrigger(DateTime.FromBinary((long)activity.actiTime).AddMinutes(tbFuben.OpenLastMinutes), () => MieShiStart(_this, activityId));
        }

        private void MieShiStart(MieShiActiGroup _this, int activityId)
        {
            var activity = MieShiManager.GetCommonActivityData(_this.ServerId, activityId);
            if (activity.state == (int)eMieShiState.Start)
                return;
            MieShiManager.SetActivityState(_this.ServerId, activityId, (int)eMieShiState.Start);
            //广播活动状态
            ActivityServer.Instance.ServerControl.NotifyMieShiActivityState((uint)_this.ServerId, activityId,
                activity.state);
            _this.HadPickedPlayerList.Clear();
            var tbFuben = Table.GetFuben(Table.GetMieShi(activityId).FuBenID);
            if (_this.StartTriggerDic != null)
            {
                ActivityServerControl.Timer.DeleteTrigger(_this.StartTriggerDic);
                _this.StartTriggerDic = null;
            }
            if (_this.EndTriggerDic != null)
            {
                ActivityServerControl.Timer.DeleteTrigger(_this.EndTriggerDic);
                _this.EndTriggerDic = null;
            }


            _this.EndTriggerDic = ActivityServerControl.Timer.CreateTrigger(DateTime.FromBinary((long)activity.actiTime).AddMinutes((tbFuben.TimeLimitMinutes)), () => MieShiEnd(_this, activityId, 0));


            {//英雄录
                var log = new MieshiHeroLog();
                log.Id = activityId;
                log.BattleTime = DateTime.Now.ToBinary();
                log.Result = -1;
                _this.DBData.HeroLog.Add(log);
                if (_this.DBData.HeroLog.Count > 30)
                {
                    _this.DBData.HeroLog.RemoveAt(0);
                }
            }

        }

        private void MieShiEnd(MieShiActiGroup _this, int activityId,int result)
        {
            var activity = MieShiManager.GetCommonActivityData(_this.ServerId, activityId);
            if (activity.state == (int)eMieShiState.WaitNext)
                return;
            MieShiManager.SetActivityState(_this.ServerId, activityId, (int)eMieShiState.WaitNext);
            {//删除计时器
                if (_this.EndTriggerDic != null)
                {
                    ActivityServerControl.Timer.DeleteTrigger(_this.EndTriggerDic);
                    _this.EndTriggerDic = null;
                }
                if (_this.OpenTriggerDic != null)
                {
                    ActivityServerControl.Timer.DeleteTrigger(_this.OpenTriggerDic);
                    _this.OpenTriggerDic = null;
                }
            }
            //广播活动状态
            ActivityServer.Instance.ServerControl.NotifyMieShiActivityState((uint)_this.ServerId, activityId,
                activity.state);
            //清除炮台数据
            ClearBatteryData(_this);
            var tbMieShiPublic = Table.GetMieShiPublic(1);
            if (result == 1)
            {
                //var tbServerCfg = Table.GetServerConfig(945);
                //if (null == tbServerCfg || 0 == tbServerCfg.ToInt())
                //{
                    if(Table.GetMieShi(1+activityId) != null)
                        activity.activityId = ++activityId;
                //}
            }
            {
                var serverInfo = Table.GetServerName(_this.ServerId);
                DateTime ServerOpenDate = DateTime.Parse(serverInfo.OpenTime);
                activity.actiTime = (ulong)GetActiveTime(activityId, ServerOpenDate,result != 0).ToBinary();                
            }

            _this.NearlyActiId = activity.activityId;

            if (_this.OpenTriggerDic != null)
            {
                ActivityServerControl.Timer.DeleteTrigger(_this.OpenTriggerDic);
                _this.OpenTriggerDic = null;
            }
            if (result == 1)
            {
                _this.OpenTriggerDic = ActivityServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(3), () => MieShiOpen(_this, activityId,false));
            }
            else
            {//失败了广播状态
                {
                    CommonActivityData msg = new CommonActivityData();
                    msg.Datas = new CommonActivityInfo();

                    msg.Datas.activityId = activity.activityId;
                    msg.Datas.actiTime = activity.actiTime;
                    msg.Datas.state = activity.state;
                    msg.Datas.applyState = false;
                    foreach (var item in activity.batterys)
                    {
                        msg.Datas.batterys.Add(item);
                    }
                    msg.currentActivityId = activityId;
                    ActivityServer.Instance.ServerControl.NotifyMieShiActivityInfo((uint)_this.ServerId, msg);
                }
                _this.OpenTriggerDic = ActivityServerControl.Timer.CreateTrigger(DateTime.FromBinary((long)activity.actiTime).AddMinutes(-tbMieShiPublic.CanApplyTime), () => MieShiOpen(_this, activityId,true));
            }

            {//英雄录
                if (_this.DBData.HeroLog.Count > 0)
                {
                    var tmp = _this.DBData.HeroLog[_this.DBData.HeroLog.Count - 1];
                    if (result == 0)
                    {
                        tmp.HeroName.Clear();
                        tmp.HeroName.Add(Table.GetDictionary(100003017).Desc[0]);
                    }
                    tmp.Result = result;
                }
            }

            {//贡献奖励
                _this.DBData.Data.contriDatas.Sort((RankingInfoOne rankA, RankingInfoOne rankB) => { return rankA.value > rankB.value ? -1 : 1; });
                int _count = _this.DBData.Data.contriDatas.Count;
                for (int idx = 0; idx < _count; idx++)
                {
                    int rank = idx + 1;
                    var tmp = _this.DBData.Data.contriDatas[idx];
                    if (tmp.value <= 0)
                        continue;
                    Table.ForeachDefendCityDevoteReward(record =>
                    {
                        if (rank >= record.Rank[0] && rank <= record.Rank[1])
                        {
                            var tbMail = Table.GetMail(record.MailId);
                            if (tbMail == null)
                                return false;

                            var gongxianitems = new Dict_int_int_Data();
                            var itemId = tbMail.ItemId;
                            var itemCount = tbMail.ItemCount;
                            for (int i = 0, imax = itemId.Length; i < imax; i++)
                            {
                                if (itemId[i] == -1)
                                {
                                    break;
                                }

                                gongxianitems.Data.modifyValue(itemId[i], itemCount[i]);
                            }
                            if (gongxianitems.Data.Count > 0)
                            {
                               
//                                _this.mBag.AddItemByMail(tbMail.Id, gongxianitems, null, eCreateItemType.Fuben, rank.ToString());
                                string content = string.Format(tbMail.Text, rank);
                                CoroutineFactory.NewCoroutine(SendMailCoroutine, tmp.characterId, tbMail.Title, content, gongxianitems).MoveNext();
                            }

                            return false;
                        }
                        return true;
                    });
                }
                _this.DBData.Data.contriDatas.Clear();
            }
        }
        private IEnumerator SendMailCoroutine(Coroutine co,
                                               ulong id,
                                               string title,
                                               string content,
                                               Dict_int_int_Data items)
        {
            var msg = ActivityServer.Instance.LogicAgent.SendMailToCharacter(id, title, content, items, 0);
            yield return msg.SendAndWaitUntilDone(co);
        }

        private void StartBroadcastCanIn(MieShiActiGroup _this, int activityId)
        {
            CoroutineFactory.NewCoroutine(CalculateAndBroadcastCanInCoroutine, _this, activityId).MoveNext();
        }

        private IEnumerator CalculateAndBroadcastCanInCoroutine(Coroutine co, MieShiActiGroup _this, int activityId)
        {
            var applyList = _this.PlayerApplyDic;
            var canInList = _this.CanInPlayerDic;
            if (applyList == null || canInList == null)
            {
                yield break;
            }

            var msg = ActivityServer.Instance.RankAgent.SSGetServerRankData(0, _this.ServerId, (int)RankType.FightValue);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply || msg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("In CalculateAndBroadcastCanInCoroutine(). SSGetServerRankData Failed with state = {0}", msg.State);
            }

            //查找可优先进入人员并广播
            var rankList = msg.Response.RankData;
            int broadcastCount = 0;
            List<ulong> players = new List<ulong>();
            List<ulong > addBackList = new List<ulong>();

            for (int i = 0; i < rankList.Count && i < 100; i++)
            {
                var rank = rankList[i];
                var apply = applyList.Find(d => d == rank.Id);
                if (apply <= 0)
                {
                    continue;
                }
                players.Add(rank.Id);
                broadcastCount++;
                //加入可优先进入列表
                canInList.Add(rank.Id);
                //将报名信息移除，并缓存
                addBackList.Add(rank.Id);
                applyList.Remove(rank.Id);
            }
            for (int i = 0; i < applyList.Count && broadcastCount < _this.MaxCanInCount; i++)
            {
                var playerId = applyList[i];
                var tmpId = canInList.Find(d => d == playerId);
                if (tmpId > 0)
                    continue;
                players.Add(playerId);
                broadcastCount++;
                canInList.Add(playerId);
                //将报名信息移除，并缓存
                addBackList.Add(playerId);
                applyList.Remove(playerId);
            }
            if (players.Count == 0)
            {
                yield break;
            }
            var tbMieShi = Table.GetMieShi(activityId);
            var activity = MieShiManager.GetCommonActivityData(_this.ServerId, activityId);
            ActivityServer.Instance.ServerControl.NotifyPlayerCanIn(players, tbMieShi.FuBenID, DateTime.FromBinary((long)activity.actiTime).AddSeconds(120).ToBinary());

            //倒计时2分钟清空可进入列表
            ActivityServerControl.Timer.CreateTrigger(DateTime.Now.AddMinutes(2), () => ClearCanInList(_this.ServerId, activityId));

            //将优先进入人员添加到报名列表末位
            for (int i = 0; i < addBackList.Count; i++)
            {
                var playerId = addBackList[i];
                var tmpId = applyList.Find(d => d == playerId);
                if (tmpId > 0)
                    continue;
                applyList.Add(playerId);
            }
            yield return co;
        }

        private void ClearCanInList(int serverId, int activityId)
        {
            var actiGroup = MieShiManager.Activity.GetValue(serverId);
            if (null == actiGroup)
                return;
            var canInList = actiGroup.CanInPlayerDic;
            if (null == canInList)
                return;
            canInList.Clear();
        }

        private void CaculateWorldLevel(int serverId, int activityId)
        {
            CoroutineFactory.NewCoroutine(CaculateWorldLevelCoroutine, serverId, activityId).MoveNext();
        }

        private IEnumerator CaculateWorldLevelCoroutine(Coroutine co, int serverid, int activityId)
        {
            var msg = ActivityServer.Instance.RankAgent.SSGetServerRankData(0, serverid, (int)RankType.Level);
            yield return msg.SendAndWaitUntilDone(co);

            if (msg.State != MessageState.Reply)
            {
                Logger.Error("In CaculateWorldLevelCoroutine(). SSGetServerRankData Failed with state = {0}", msg.State);
                yield break;
            }

            var rankList = msg.Response.RankData;
            int totalLvl = 0;
            int playerCount = 0;
            for (int i = 0; i < rankList.Count && i < MieShiManager.TopPlayerCount; i++)
            {
                var rankOne = rankList[i];
                totalLvl += rankOne.Value;
            }
            if (rankList.Count > MieShiManager.TopPlayerCount)
            {
                playerCount = MieShiManager.TopPlayerCount;
            }
            else
            {
                playerCount = rankList.Count;
            }
            if (playerCount == 0)
            {
                MieShiManager.SetWorldLevel(serverid, activityId, 1);
                yield break;
            }
            int averageLvl = totalLvl / playerCount;
            var tbMieShi = Table.GetMieShi(activityId);
            if(tbMieShi != null && tbMieShi.Difficult > 0)
            {
                averageLvl = (int)((float)averageLvl * tbMieShi.Difficult);
            }
            //var tbMieShi = Table.GetMieShi(activityId);
            //averageLvl *= tbMieShi.fff //增加难度系数
            MieShiManager.SetWorldLevel(serverid, activityId, averageLvl);
            yield break;
        }
        #endregion
    }

    public class MieShiActiGroup
    {
        #region 数据
        private static IMieShi mImpl;

        public int ServerId;
        public int WorldLevel;    //活动的世界等级
        public int NearlyActiId;  //最近的活动ID(即将开始或进行中)
        public int LastActiId;    //上一次活动ID
        public int MaxCanInCount; //活动最高人数

        public bool RankDirty;    //需要排行脏标记

        public DBGroupActivityData DBData;
        public Dictionary<int, Trigger> BatteryTrigger; //活动炮台技能的倒计时
        public List<ulong> HadInPlayerDic; //已进入活动的玩家列表   activityId->characterIds 
        public List<ulong> CanInPlayerDic; //可以进入活动的玩家列表   activityId->characterIds 
        public List<ulong> PlayerApplyDic; //玩家报名列表    activityId->characterIds 
        public Trigger OpenTriggerDic;  //活动开启的定时器
        //public Dictionary<int, Trigger> CanPromoteTriggerDic; //可以提升定时器 
        public Trigger CanInTriggerDic; //可以进入的定时器
        public Trigger StartTriggerDic; //副本开始的定时器 
        public Trigger EndTriggerDic;   //副本结束的定时器

        public List<int> CanPickUpBoxNpcList;  //可以点击领取的npc列表
        public List<ulong> HadPickedPlayerList; //已领取的玩家列表

        #endregion

        #region Init
        static MieShiActiGroup()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof(MieShiActiGroup), typeof(MieShiDefaultImpl),
                o => { mImpl = (IMieShi)o; });
        }

        public MieShiActiGroup(int serverId)
        {
            mImpl.Construct(this, serverId);
        }

        public void SwitchActivityState(int activityId, int state,int result)
        {
            mImpl.SwtichActivityState(this, activityId, state, result);
        }

        public void BatterySkillLevelEnd(ref Trigger trigger, ref ActivityBatteryOne battery, int activityId)
        {
            if (battery.skillLevel > 2)
            {
                var cdSecond = Table.GetMieShiPublic(1).LevelKeepTime;
                battery.skillLevel--;
                battery.skillLvlEndTime = (ulong)DateTime.Now.AddSeconds(cdSecond).ToBinary();
                ActivityServerControl.Timer.ChangeTime(ref trigger, DateTime.Now.AddSeconds(cdSecond));
            }
            else
            {
                BatteryTrigger.Remove(battery.batteryId);
                battery.skillLevel = 1;
                battery.skillLvlEndTime = 0;
                ActivityServerControl.Timer.DeleteTrigger(trigger);
            }

            //广播通知
            ActivityServer.Instance.ServerControl.NotifyBatteryData((uint) ServerId, activityId, battery);
        }

        public void StartRankContriList()
        {
            CoroutineFactory.NewCoroutine(LoopRankContriCoroutine).MoveNext();
        }

        //查询某个玩家是否已报名
        public bool CheckPlayerIsAppply(int activityId, ulong characterId)
        {
            var list = PlayerApplyDic;
            if (null == list)
                return false;
            var unit = list.Find(d => d == characterId);
            if (unit == characterId)
                return true;
            return false;
        }

        private IEnumerator LoopRankContriCoroutine(Coroutine co)
        {
            while (true)
            {
                yield return ActivityServer.Instance.ServerControl.Wait(co, TimeSpan.FromSeconds(10));

                var co1 = CoroutineFactory.NewSubroutine(RankContriListCoroutine, co);
                if (co1.MoveNext())
                {
                    yield return co1;
                }
            }
        }

        private IEnumerator RankContriListCoroutine(Coroutine co)
        {
            if (!RankDirty)
            {
                yield break;
            }
            DBCommonActivityData oActivity = DBData.Data;
           
            if (null == oActivity)
                yield break;
            //排名并赋予名次
            oActivity.contriDatas.Sort(MieShiManager.RuComparer);
            var idx = 1;
            foreach (var unit in oActivity.contriDatas)
            {
                unit.rank = idx++;
            }
            //改变脏标记
            RankDirty = false;
            yield break;
        }

        #endregion
    }

    public interface IMieShiManager
    {
        void Init();
        void UnInit();

        void GetActivityData(int serverId, ulong characterId, CommonActivityData actiList);
        void ApplyMieshiHeroLogData(int serverId, MieshiHeroLogList logList);
        DBCommonActivityData GetCommonActivityData(int serverId, int activityId);
        void SetActivityState(int serverId, int activityId, int state);
        void UpdateBatteryData(int serverId, int activityId, ActivityBatteryOne battery);
        void GetBatteryData(int serverId, int activityId, ulong characterId, BatteryDatas batteryList);
        ErrorCodes PromoteBatteryHp(int serverId, int activityId, int batteryId, int type, ulong characterId, string name, ref BatteryUpdateData update);
        ErrorCodes GetPlayerTowerUpTimes(int serverId, int activityId, ulong characterId, ref int times);
        ErrorCodes PromoteBatterySkill(int serverId, int activityId, int batteryId, int type, ulong characterId, string name, ref BatteryUpdateData update);
        void GetContriRankingData(int serverId, int activityId, ulong characterId, ContriRankingData data);
        void GetPointRankingData(int serverId, int activityId, ulong characterId, PointRankingData data);
        ErrorCodes ApplyJoinActivity(int serverId, int activityId, ulong characterId);
        void SetPointRankData(int serverId, int activityId, MieShiSceneData data);
        ErrorCodes OnPlayerGetTowerReward(int serverId, int activityId, ulong characterId, int idx, ref int flag);
        ErrorCodes ApplyContributeRate(int serverId, int activityId, ref ContriRateList rateList);
        void SetWorldLevel(int serverId, int activityId, int worldLevel);
        ErrorCodes SetActivityResult(int serverId, int activityId, int result);
        CommonActivityInfo SaveGuidAndGetActiInfo(ulong sceneId,int serverid, int activityId, MieShiBatteryGuid guildList);
        ErrorCodes ApplyEnterActivity(ulong characterId, int serverId, int activityId);
        ErrorCodes SyncCanPickUpBox(int serverId, int activityId, int npcId);
        ErrorCodes ApplyPickUpBox(int serverId, int activityId, int npcId, ulong characterId);
        ErrorCodes ApplyPortraitData(int serverId, ref PlayerInfoMsg data);
        ErrorCodes ApplyPortraitAward(int serverId, ulong characterId);
        ErrorCodes SaveBatteryDestroy(int serverId, int activityId, ulong objId);
        void SyncActivityAllPlayerExit(int serverId, int activityId);

        
    }

    public class MieShiManagerDefaultImpl : IMieShiManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Init()
        {
            //根据开关判断是否开启
            var tbServerCfg = Table.GetServerConfig(1301);
            if (null == tbServerCfg || 0 == tbServerCfg.ToInt())
            {
                return;
            }
            Table.ForeachServerName(record =>
            {
                var id = record.LogicID;
                if (MieShiManager.Activity.ContainsKey(id) == false && id == record.Id && (record.IsClientDisplay == 1 || record.IsClientDisplay == 2))
                {
                    MieShiManager.Activity.Add(id, new MieShiActiGroup(id));
                }
                return true;
            });

            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
        }

        private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v == null)
                return;
            if (v.tableName == "ServerName")
            {
                Table.ForeachServerName(record =>
                {
                    var id = record.LogicID;
                    if (MieShiManager.Activity.ContainsKey(id) == false && id == record.Id && (record.IsClientDisplay == 1 || record.IsClientDisplay == 2))
                    {
                        if (!MieShiManager.Activity.ContainsKey(id))
                        {
                            MieShiManager.Activity.Add(id, new MieShiActiGroup(id));
                        }
                    }
                    return true;
                });
            }
        }
        public void UnInit()
        {
            if (MieShiManager.SaveTrigger != null)
            {
                ActivityServerControl.Timer.DeleteTrigger(MieShiManager.SaveTrigger);
                MieShiManager.SaveTrigger = null;
            }
        }

        public void ApplyMieshiHeroLogData(int serverId, MieshiHeroLogList logList)
        {
             MieShiActiGroup actiGroup;
            if (MieShiManager.Activity.TryGetValue(serverId, out actiGroup))
            {
                logList.datas.AddRange(actiGroup.DBData.HeroLog);
            }
        }
        public void GetActivityData(int serverId, ulong characterId, CommonActivityData actiList)
        {
            MieShiActiGroup actiGroup;
            if (MieShiManager.Activity.TryGetValue(serverId, out actiGroup))
            {
                var acti = actiGroup.DBData.Data;
                {
                    var actiOne = new CommonActivityInfo();
                    actiOne.activityId = acti.activityId;
                    actiOne.actiTime = acti.actiTime;
                    actiOne.state = acti.state;
                    bool applyState = actiGroup.CheckPlayerIsAppply(acti.activityId, characterId);
                    actiOne.applyState = applyState;
                    foreach (var item in acti.batterys)
                    {
                        actiOne.batterys.Add(item);
                    }
                    actiList.Datas = actiOne;
                }
                //最近的活动ID
                actiList.currentActivityId = actiGroup.NearlyActiId;
            }
        }

        public DBCommonActivityData GetCommonActivityData(int serverId, int activityId)
        {
            var oActiGroup = MieShiManager.Activity.GetValue(serverId);
            if (oActiGroup != null)
            {
                return oActiGroup.DBData.Data;
            }
            return null;
        }

        public void UpdateBatteryData(int serverId, int activityId, ActivityBatteryOne battery)
        {
            var oActiGroup = MieShiManager.Activity.GetValue(serverId);
            if (oActiGroup != null)
            {
                if(oActiGroup.DBData.Data != null)
                {
                    var acti = oActiGroup.DBData.Data;
                    if (acti.activityId == activityId)
                    {
                        for (int i = 0; i < acti.batterys.Count; i++)
                        {
                            if (acti.batterys[i].batteryId == battery.batteryId)
                            {
                                acti.batterys[i].curMaxHP = battery.curMaxHP;
                                acti.batterys[i].maxHP = battery.maxHP;
                                acti.batterys[i].level = battery.level;
                                acti.batterys[i].promoteCount = battery.promoteCount;
                                acti.batterys[i].skillLevel = battery.skillLevel;
                                acti.batterys[i].skillLvlEndTime = battery.skillLvlEndTime;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void SetActivityState(int serverId, int activityId, int state)
        {
            var oActiGroup = MieShiManager.Activity.GetValue(serverId);
            if (oActiGroup != null)
            {
                if(oActiGroup.DBData.Data != null)
                {
                    oActiGroup.DBData.Data.state = state;
                }
            }
        }
        /// <summary>
        /// 新增方法 灭世时间结束才清除部分数据
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="activityId"></param>
        public void SyncActivityAllPlayerExit(int serverId, int activityId)
        {
            //var oActiGroup = MieShiManager.Activity.GetValue(serverId);
            //if (null == oActiGroup)
            //    return;

            //if (oActiGroup.HadPickedPlayerList != null)
            //    oActiGroup.HadPickedPlayerList.Clear();
        }

        public void SetWorldLevel(int serverId, int activityId, int level)
        {
            var oActiGroup = MieShiManager.Activity.GetValue(serverId);
            if (null == oActiGroup)
                return;
            var tbActiPublic = Table.GetMieShiPublic(1);

            oActiGroup.WorldLevel = level;
            if (oActiGroup.DBData.Data != null)
            {
                var actiOne = oActiGroup.DBData.Data;
                for (int j = 0; j < actiOne.batterys.Count; j++)
                {
                    var batteryOne = actiOne.batterys[j];
                    batteryOne.level = level;

                    var tbBatteryBaseNew = Table.GetBatteryBaseNew(activityId);
                    if (tbBatteryBaseNew == null)
                    {
                        continue;
                    }

                    var splitResult = tbBatteryBaseNew.BatteryNpcId.Split('|');
                    if (splitResult.Length <= j)
                    {
                        continue;
                    }
                    var idNew = int.Parse(splitResult[j]);

                    var tbCharacterBaseNew = Table.GetCharacterBase(idNew);
                    if (null == tbCharacterBaseNew)
                        continue;

                    if (tbCharacterBaseNew.Attr.Length <= 13)
                    {
                        continue;
                    }
                    var levelHp = tbCharacterBaseNew.Attr[13];
                    float newHP = (float)levelHp * (1.0f + (float)tbActiPublic.RaiseHP * batteryOne.promoteCount / 10000.0f);
                    float maxHP = (float)levelHp * (1.0f + (float)tbActiPublic.RaiseHP * tbActiPublic.MaxRaiseHP / 10000.0f);
                    batteryOne.curMaxHP = (int)newHP;
                    batteryOne.maxHP = (int)maxHP;
                }                
            }
        }

        public void GetBatteryData(int serverId, int activityId, ulong characterId, BatteryDatas batteryList)
        {
            var actiInfo = MieShiManager.Activity.GetValue(serverId);
            if (null == actiInfo)
                return;
            var item = actiInfo.DBData.Data;
            {
                if (item.activityId == activityId)
                {
                    foreach (var battery in item.batterys)
                    {
                        batteryList.batterys.Add(battery);
                    }
                    var contri = item.contriDatas.Find(d => d.characterId == characterId);
                    if (null != contri)
                    {
                        batteryList.contribute = contri.value;
                        batteryList.times = contri.param1;
                        batteryList.flag = contri.param2;
                    }
                }
            }
        }

        public ErrorCodes GetPlayerTowerUpTimes(int serverId, int activityId, ulong characterId,ref int times)
        {
            var tbActiGroup = MieShiManager.Activity.GetValue(serverId);
            if (null == tbActiGroup)
                return ErrorCodes.Error_MieShi_NoData;
            var tbActiPublic = Table.GetMieShiPublic(1);

            //找到对应的活动
            if(tbActiGroup.DBData.Data == null)
                return ErrorCodes.Error_MieShi_NoActivity;
            DBCommonActivityData oActivity = tbActiGroup.DBData.Data;

            var unit = oActivity.contriDatas.Find(d => d.characterId == characterId);
            if (unit == null)
            {
                times = 0;
            }
            else
            {
                times = unit.param1;
            }
            return ErrorCodes.OK;
        }
        public ErrorCodes PromoteBatteryHp(int serverId, int activityId, int batteryId, int type, ulong characterId, string name, ref BatteryUpdateData update)
        {
            var tbActiGroup = MieShiManager.Activity.GetValue(serverId);
            if (null == tbActiGroup)
                return ErrorCodes.Error_MieShi_NoData;
            var tbActiPublic = Table.GetMieShiPublic(1);

            //找到对应的活动
            if(tbActiGroup.DBData.Data == null)
                return ErrorCodes.Error_MieShi_NoActivity;
            DBCommonActivityData oActivity = tbActiGroup.DBData.Data;

            var tbMieShiPublic = Table.GetMieShiPublic(1);
            var diffTime =
                DateTime.FromBinary((long) oActivity.actiTime).AddMinutes(-tbMieShiPublic.BatteryPromoteTime) -
                DateTime.Now;
            //只有活动报名中可提升阶段，才能提升炮台血量
            if (oActivity.state != (int)eMieShiState.Open ||
                (oActivity.state == (int)eMieShiState.Open && diffTime.TotalSeconds > 0))
            { 
                return ErrorCodes.Error_MieShi_CanNotPromote;
            }
            int idx = 0;
            //找到对应的炮台
            ActivityBatteryOne oBattery = null;
            for (int i = 0; i < oActivity.batterys.Count; i++)
            {
                var battery = oActivity.batterys[i];
                if (battery.batteryId == batteryId)
                {
                    idx = i;
                    oBattery = battery;
                    break;
                }
            }
            if (null == oBattery)
                return ErrorCodes.Error_MieShi_NoBattery;

            var tbBatteryBaseNew = Table.GetBatteryBaseNew(activityId);
            if(tbBatteryBaseNew == null)
                return ErrorCodes.Error_MieShi_NoBattery;

            var tIds = tbBatteryBaseNew.BatteryNpcId.Split('|');
            if(tIds.Length < idx+1)
                return ErrorCodes.Error_MieShi_NoBattery;
            var idNew = int.Parse(tIds[idx]);
            var tbCharacterBaseNew = Table.GetCharacterBase(idNew);

            if (null == tbCharacterBaseNew)
                return ErrorCodes.Error_MieShi_Config;
         

            var unit = oActivity.contriDatas.Find(d => d.characterId == characterId);
           
            var levelHp =  tbCharacterBaseNew.Attr[13];

            //按比例提升
            oBattery.promoteCount++;
            float newHP = (float)levelHp * (1.0f + (float)tbActiPublic.RaiseHP * oBattery.promoteCount / 10000.0f);
            float maxHP = (float)levelHp * (1.0f + (float)tbActiPublic.RaiseHP * tbActiPublic.MaxRaiseHP / 10000.0f);

            oBattery.curMaxHP = (int)newHP;
            oBattery.maxHP = (int)maxHP;
            //更新炮台数据

            //增加贡献

            if (null == unit)
            {
                unit = new RankingInfoOne();
                unit.characterId = characterId;
                unit.rank = 0;
                unit.name = name;
                unit.value = tbActiPublic.GainContribute;
                oActivity.contriDatas.Add(unit);
            }
            else
            {
                unit.value += tbActiPublic.GainContribute;
            }
            unit.param1 ++;
            oActivity.totalContir += tbActiPublic.GainContribute;
            tbActiGroup.RankDirty = true;
            if (oBattery.skillLevel < tbActiPublic.MaxBatteryLevel)
                oBattery.skillLevel++;

            MieShiManager.UpdateBatteryData(serverId, activityId, oBattery);
            //广播通知
            ActivityServer.Instance.ServerControl.NotifyBatteryData((uint)serverId, activityId, oBattery);
            //修改脏标记
            tbActiGroup.RankDirty = true;
            //设置返回数据
            update.battery = oBattery;
            update.contribute = unit.value;
            update.times = unit.param1;
            return ErrorCodes.OK;
        }

        public ErrorCodes PromoteBatterySkill(int serverId, int activityId, int batteryId, int type, ulong characterId, string name, ref BatteryUpdateData update)
        {
            return ErrorCodes.OK;
        }

        public void GetContriRankingData(int serverId, int activityId, ulong characterId, ContriRankingData data)
        {
            var actiData = GetCommonActivityData(serverId, activityId);
            if (null == actiData)
                return;
            data.Datas.AddRange(actiData.contriDatas);
            data.Datas.Sort((RankingInfoOne rankA, RankingInfoOne rankB) =>{return rankA.value > rankB.value ? -1 : 1; });
            var idx = 1;
            foreach (var unit in data.Datas)
            {
                unit.rank = idx++;
            }
            var contri = actiData.contriDatas.Find(d => d.characterId == characterId);
            if (contri != null)
            {
                data.MyRank = contri.rank;
            }
        }

        public void GetPointRankingData(int serverId, int activityId, ulong characterId, PointRankingData data)
        {
            var actiData = GetCommonActivityData(serverId, activityId);
            if (null == actiData)
                return;
            data.Datas.AddRange(actiData.pointDatas);
            var point = actiData.pointDatas.Find(d => d.characterId == characterId);
            if (point != null)
            {
                data.MyRank = point.rank;
            }
        }

        public ErrorCodes ApplyJoinActivity(int serverId, int activityId, ulong characterId)
        {
            var oActiGroup = MieShiManager.Activity.GetValue(serverId);
            if (null == oActiGroup)
                return ErrorCodes.Error_MieShi_NoData;

            //判断活动的状态
            var actiData = GetCommonActivityData(serverId, activityId);
            if (actiData == null)
            {
                return ErrorCodes.Error_MieShi_NoData;
            }
            if (actiData.state < (int)eMieShiState.Open || actiData.state > (int)eMieShiState.Start)
            {
                return ErrorCodes.Error_MieShi_NoApplyTime;
            }

            if (null == oActiGroup.PlayerApplyDic)
            {
                oActiGroup.PlayerApplyDic = new List<ulong>();
             
            }
            foreach (var charId in oActiGroup.PlayerApplyDic)
            {
                if (charId == characterId)
                    return ErrorCodes.OK;
            }
            oActiGroup.PlayerApplyDic.Add(characterId);
            return ErrorCodes.OK;
        }

        public void SetPointRankData(int serverId, int activityId, MieShiSceneData data)
        {
            var actiData = GetCommonActivityData(serverId, activityId);
            if (null == actiData)
                return;
            actiData.pointDatas.Clear();
            foreach (var unit in data.playerPoints)
            {
                var item = new RankingInfoOne();
                item.characterId = unit.CharacterId;
                item.name = unit.Name;
                item.rank = unit.Rank;
                item.value = unit.Damage;
                actiData.pointDatas.Add(item);
            }

            actiData.batteryPoints.Clear();
            actiData.batteryPoints.AddRange(data.batteryPoints);

            if (data.characterId > 0)
            {
                //保存上次最佳积分数据
                var pointUnit = actiData.pointDatas.Find(d => d.characterId == data.characterId);
                if (null != pointUnit)
                {
                    actiData.lastBestInfo.characterId = pointUnit.characterId;
                    actiData.lastBestInfo.name = pointUnit.name;
                    actiData.lastBestInfo.rank = pointUnit.rank;
                    actiData.lastBestInfo.value = pointUnit.value;
                }
                //保存雕像数据
                CoroutineFactory.NewCoroutine(GetPortraitData, serverId, data.characterId).MoveNext();
            }


            var oActiGroup = MieShiManager.Activity.GetValue(serverId);
            var pointList = actiData.pointDatas.OrderByDescending(item => item.value).ToList();//倒序排序取前3名
            if (oActiGroup != null && oActiGroup.DBData.HeroLog.Count > 0 && (actiData.state == (int)eMieShiState.Start || actiData.state == (int)eMieShiState.WillEnd))
            {
                var tmp = oActiGroup.DBData.HeroLog[oActiGroup.DBData.HeroLog.Count-1];
                tmp.HeroName.Clear();
                for (int i = 0; i < 3 && i < pointList.Count; i++)
                {
                    tmp.HeroName.Add(pointList[i].name);    
                }                
            }

        }

        public ErrorCodes OnPlayerGetTowerReward(int serverId, int activityId, ulong characterId, int idx,ref int flag)
        {
            var actiData = GetCommonActivityData(serverId, activityId);
            if (null == actiData)
                return ErrorCodes.Error_MieShi_NoData;
            var unit = actiData.contriDatas.Find(d => d.characterId == characterId);
            if(unit == null)
                return ErrorCodes.Error_MieShi_NotApply;
            int temp = 1 << idx;
            if ((unit.param2 & temp) > 0)
            {
                return ErrorCodes.Error_MieShi_AlreadyGain;
            }
            {
                var tb = Table.GetMieshiTowerReward(idx);
                if (tb == null || tb.TimesStep.Count==0)
                {
                    return ErrorCodes.Unknow;
                }
                if (unit.param1 < tb.TimesStep[0])
                {
                    return ErrorCodes.Error_NO_Times;
                }
            }

            unit.param2 = unit.param2 | (1 << idx);
            flag = unit.param2;

            return ErrorCodes.OK;
        }
        private void DelayBroadcastStateEnd(int serverId, int activityId)
        {
            //广播活动状态
            ActivityServer.Instance.ServerControl.NotifyMieShiActivityState((uint)serverId, activityId,
                (int)eMieShiState.WaitNext);
        }

        private void DelayBroadcastStateOpen(int serverId, int activityId)
        {
            //清除历史数据
            var actiGroup = MieShiManager.Activity.GetValue(serverId);
            actiGroup.DBData.characters.Clear();
            actiGroup.DBData.portrait = null;
            //广播活动状态
            ActivityServer.Instance.ServerControl.NotifyMieShiActivityState((uint)serverId, activityId,
                    (int)eMieShiState.Open);
        }

        private IEnumerator GetPortraitData(Coroutine co, int serverId, ulong characterId)
        {
            var msg = ActivityServer.Instance.LogicAgent.GetLogicSimpleData(characterId, 0);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply)
            {
                Logger.Warn("GetPortraitData GetLogicSimpleData False! guid={0}", characterId);
                yield break;
            }
            var actiGroup = MieShiManager.Activity.GetValue(serverId);
            if (actiGroup.DBData.portrait == null)
            {
                actiGroup.DBData.portrait = new PlayerInfoMsg();
            }
            actiGroup.DBData.portrait.Id = msg.Response.Id;
            actiGroup.DBData.portrait.TypeId = msg.Response.TypeId;
            actiGroup.DBData.portrait.Name = msg.Response.Name;
            actiGroup.DBData.portrait.EquipsModel.Clear();
            actiGroup.DBData.portrait.EquipsModel.AddRange(msg.Response.EquipsModel);
            actiGroup.DBData.portrait.Level = msg.Response.Level;
            actiGroup.DBData.portrait.Equips = msg.Response.Equips;
            actiGroup.DBData.portrait.WorshipCount = msg.Response.WorshipCount;

            yield return co;
        }

        public ErrorCodes ApplyContributeRate(int serverId, int activityId, ref ContriRateList data)
        {
            var actiData = GetCommonActivityData(serverId, activityId);
            if (null == actiData)
                return ErrorCodes.Error_MieShi_NoData;
            foreach (var item in actiData.contriDatas)
            {
                float rate = (float)item.value / (float)actiData.totalContir;
                var unit = new ContriRateUnit();
                unit.characterId = item.characterId;
                unit.rate = rate;
                data.rateList.Add(unit);
            }
            return ErrorCodes.OK;
        }

        public ErrorCodes SetActivityResult(int serverId, int activityId, int result)
        {
            var actiData = GetCommonActivityData(serverId, activityId);
            if (null == actiData)
                return ErrorCodes.Error_MieShi_NoData;
            actiData.lastTimeResult = result;
            var actiGroup = MieShiManager.Activity.GetValue(serverId);
            if (actiGroup != null)
            {
                actiGroup.DBData.LastEndTime = DateTime.Now.ToBinary();    // 默认收到消息的时间
                if (actiGroup.DBData.LastResult == -99)
                    actiGroup.DBData.LastResult = result;
            }
            //确保活动状态的正确性
            if (actiData.state >= (int)eMieShiState.Open && actiData.state <= (int)eMieShiState.WillEnd)
            {
                actiGroup.SwitchActivityState(activityId, (int)eMieShiState.WaitNext,result);
            }
            //胜利了 要继续打下一场 清除灭世贡献的数据
            if (result == 1)
            {
                actiData.contriDatas.Clear();
            }
            return ErrorCodes.OK;
        }

        public ErrorCodes ApplyEnterActivity(ulong characterId, int serverId, int activityId)
        {
            var actiData = GetCommonActivityData(serverId, activityId);
            var actiGroup = MieShiManager.Activity.GetValue(serverId);
            if (null == actiData || null == actiGroup)
                return ErrorCodes.Error_MieShi_NoData;
            //判断活动状态
            if (actiData.state < (int)eMieShiState.CanIn || actiData.state >= (int)eMieShiState.WillEnd)
            {
                return ErrorCodes.Error_MieShi_NotCanInTime;
            }

            //是否人数已满
            var hadInList = actiGroup.HadInPlayerDic;
            if (hadInList == null)
                return ErrorCodes.Error_MieShi_PlayerFull;

            //是否可优先进入
            var canInList = GetPriorityCanInList(serverId, activityId);
            if (canInList != null && canInList.Count > 0 && hadInList.Count >= actiGroup.MaxCanInCount)
            {
                var findId = canInList.Find(d => d == characterId);
                if (findId <= 0)
                    return ErrorCodes.Error_MieShi_WaitTime;
            }

            //是否在剩余可进入人数内
            var applyList = actiGroup.PlayerApplyDic;
            int leftCount = actiGroup.MaxCanInCount - hadInList.Count;
            for (int i = 0; i < applyList.Count && i < leftCount; i++)
            {
                var applyId = applyList[i];
                if (applyId == characterId)
                {
                    hadInList.Add(characterId);
                    return ErrorCodes.OK;
                }
            }
            return ErrorCodes.Error_MieShi_NotApply;
        }

        public List<ulong> GetPriorityCanInList(int serverId, int activityId)
        {
            var actiGroup = MieShiManager.Activity.GetValue(serverId);
            if (null == actiGroup)
                return null;

            return actiGroup.CanInPlayerDic;
        }

        public ErrorCodes SyncCanPickUpBox(int serverId, int activityId, int npcId)
        {
            var oActiGroup = MieShiManager.Activity.GetValue(serverId);
            if (null == oActiGroup)
                return ErrorCodes.Error_MieShi_NoData;

            var findId = oActiGroup.CanPickUpBoxNpcList.Find(d => d == npcId);
            if (findId > 0)
            {
                return ErrorCodes.OK;
            }

            oActiGroup.CanPickUpBoxNpcList.Add(npcId);
            return ErrorCodes.OK;
        }

        public ErrorCodes ApplyPickUpBox(int serverId, int activityId, int npcId, ulong characterId)
        {
            var oActiGroup = MieShiManager.Activity.GetValue(serverId);
            if (null == oActiGroup)
                return ErrorCodes.Error_MieShi_NoData;
            //此处注释掉 灭世结束后获胜 只要人还在灭世副本中点击宝箱都可以领取
            /**var findId = oActiGroup.CanPickUpBoxNpcList.Find(d => d == npcId);
            if (findId <= 0)
            {
                return ErrorCodes.Error_MieShi_NoBossBox;
            }**/
            var charId = oActiGroup.HadPickedPlayerList.Find(d => d == characterId);
            if (charId > 0)
            {
                return ErrorCodes.Error_MieShi_BossHadPickUp;
            }

            oActiGroup.HadPickedPlayerList.Add(characterId);

            return ErrorCodes.OK;
        }

        public CommonActivityInfo SaveGuidAndGetActiInfo(ulong sceneId,int serverId, int activityId, MieShiBatteryGuid guidList)
        {
            var actiData = GetCommonActivityData(serverId, activityId);
            if (null == actiData)
                return null;
            actiData.sceneGuid = sceneId;
            for (int i = 0; i < guidList.data.Count; i++)
            {
                var unit = guidList.data[i];
                var battery = actiData.batterys.Find(d => d.batteryId == unit.batteryId);
                if (null == battery)
                    continue;
                battery.batteryGuid = unit.guid;
            }

            var oActiGroup = MieShiManager.Activity.GetValue(serverId);
            if (null == oActiGroup)
                return null;
            CommonActivityInfo info = new CommonActivityInfo();
            info.activityId = actiData.activityId;
            info.state = actiData.state;
            info.actiTime = actiData.actiTime;
            info.lastTimeResult = actiData.lastTimeResult;
            info.wordLevel = oActiGroup.WorldLevel;
            info.batterys.AddRange(actiData.batterys);



            {//提前广播开始前的贡献
                int _all = 0;
                foreach (var data in actiData.contriDatas)
                {
                    _all += data.value;
                }
                foreach(var data in actiData.contriDatas)
                {
                    if (data.value > 0 && _all>0)
                         CoroutineFactory.NewCoroutine(SyncPlayerContribution, sceneId, data.characterId, data.value, data.name, (float)data.value/_all).MoveNext();
                }
            }
            return info;
        }
        private IEnumerator SyncTowerLevel(Coroutine co, ulong sceneId, int towerId, int level)
        {
            var msg = ActivityServer.Instance.SceneAgent.SyncTowerSkillLevel(sceneId, towerId, level);
            msg.SendAndWaitUntilDone(co);
            yield break;
        }
        private IEnumerator SyncPlayerContribution(Coroutine co, ulong sceneId, ulong characterId, int contribution,string name,float rate)
        {
            var msg = ActivityServer.Instance.SceneAgent.SyncPlayerMieshiContribution(sceneId, characterId, contribution,name,rate);
            msg.SendAndWaitUntilDone(co);
            yield break;
        }
        public ErrorCodes ApplyPortraitData(int serverId, ref PlayerInfoMsg data)
        {
            var oActiGroup = MieShiManager.Activity.GetValue(serverId);
            if (null == oActiGroup)
                return ErrorCodes.Error_MieShi_NoData;
            data = oActiGroup.DBData.portrait;
            return ErrorCodes.OK;
        }

        public ErrorCodes ApplyPortraitAward(int serverId, ulong characterId)
        {
            var oActiGroup = MieShiManager.Activity.GetValue(serverId);
            if (null == oActiGroup)
                return ErrorCodes.Error_MieShi_NoData;

            //判断是否已领取
            var findId = oActiGroup.DBData.characters.Find(d => d == characterId);
            if (findId > 0)
            {
                return ErrorCodes.Error_MieShi_AlreadyGain;
            }
            //添加已领取
            oActiGroup.DBData.characters.Add(characterId);

            return ErrorCodes.OK;
        }

        public ErrorCodes SaveBatteryDestroy(int serverId, int activityId, ulong objId)
        {
            var actiData = GetCommonActivityData(serverId, activityId);
            if (null == actiData)
                return ErrorCodes.Error_MieShi_NoActivity;

            for (int i = 0; i < actiData.batterys.Count; i++)
            {
                var unit = actiData.batterys[i];
                if (unit.batteryGuid == objId)
                {
                    unit.batteryGuid = 0;
                    //广播通知炮台被摧毁
                    ActivityServer.Instance.ActivityAgent.NotifyBatteryData((uint) serverId, activityId, unit);
                    return ErrorCodes.OK;
                }
            }

            return ErrorCodes.Error_MieShi_NoBattery;
        }
    }

    public static class MieShiManager
    {
        // server logic id => WorldBoss
        public static Dictionary<int, MieShiActiGroup> Activity = new Dictionary<int, MieShiActiGroup>();
        private static IMieShiManager mImpl;
        public static RankingUnitComparer RuComparer = new RankingUnitComparer();
        public static Trigger SaveTrigger;

        public static int TopPlayerCount = 10;

        static MieShiManager()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof(MieShiManager),
                typeof (MieShiManagerDefaultImpl),
                o => { mImpl = (IMieShiManager) o; });
        }

        public static void Init()
        {
            mImpl.Init();
            var tbServerCfg = Table.GetServerConfig(1304);
            if (null != tbServerCfg || 0 == tbServerCfg.ToInt())
            {
                TopPlayerCount = tbServerCfg.ToInt();
            }
           
        }

        public static void UnInit()
        {
            mImpl.UnInit();
        }

        //获取活动的总数据
        public static void GetActivityData(int serverId, ulong characterId, CommonActivityData actiList)
        {
            mImpl.GetActivityData(serverId, characterId, actiList);
        }

        public static void ApplyMieshiHeroLogData(int serverId, MieshiHeroLogList logList)
        {
            mImpl.ApplyMieshiHeroLogData(serverId, logList);
        }
        //获取特定活动的数据
        public static DBCommonActivityData GetCommonActivityData(int serverId, int activityId)
        {
            return mImpl.GetCommonActivityData(serverId, activityId);
        }

        //改变活动的状态
        public static void SetActivityState(int serverId, int activityId, int state)
        {
            mImpl.SetActivityState(serverId, activityId, state);
        }

        public static void UpdateBatteryData(int serverId, int activityId, ActivityBatteryOne battery)
        {
            mImpl.UpdateBatteryData(serverId, activityId, battery);
        }

        //获取炮台的数据
        public static void GetBatteryData(int serverId, int activityId, ulong characterId, BatteryDatas batteryList)
        {
            mImpl.GetBatteryData(serverId, activityId, characterId, batteryList);
        }

        //提升炮台的血量
        public static ErrorCodes PromoteBatteryHp(int serverId, int activityId, int batteryId, int type, ulong characterId, string name, ref BatteryUpdateData promoteHP)
        {
            return mImpl.PromoteBatteryHp(serverId, activityId, batteryId, type, characterId, name, ref promoteHP);
        }
        public static ErrorCodes GetPlayerTowerUpTimes(int serverId, int activityId, ulong characterId, ref int times)
        {
            return mImpl.GetPlayerTowerUpTimes(serverId, activityId,characterId,ref times);
        }
        //提升炮台的技能
        public static ErrorCodes PromoteBatterySkill(int serverId, int activityId, int batteryId, int type, ulong characterId, string name, ref BatteryUpdateData newLvl)
        {
            return mImpl.PromoteBatterySkill(serverId, activityId, batteryId, type, characterId, name, ref newLvl);
        }

        //获取贡献排行榜数据
        public static void GetContriRankingData(int serverId, int activityId, ulong characterId, ContriRankingData data)
        {
            mImpl.GetContriRankingData(serverId, activityId, characterId, data);
        }

        //获取积分排行版数据
        public static void GetPointRankingData(int serverId, int activityId, ulong characterId, PointRankingData data)
        {
            mImpl.GetPointRankingData(serverId, activityId, characterId, data);
        }

        //请求活动报名
        public static ErrorCodes ApplyJoinActivity(int serverId, int activityId, ulong characterId)
        {
            return mImpl.ApplyJoinActivity(serverId, activityId, characterId);
        }

        //保存积分数据
        public static void SetPointRankData(int serverId, int activityId, MieShiSceneData data)
        {
            mImpl.SetPointRankData(serverId, activityId, data);
        }

        public static ErrorCodes OnPlayerGetTowerReward(int serverId, int activityId, ulong characterId, int idx, ref int flag)
        {
            return mImpl.OnPlayerGetTowerReward(serverId,activityId,characterId,idx,ref flag);
        }
        //获取贡献百分比
        public static ErrorCodes ApplyContributeRate(int serverId, int activityId, ref ContriRateList rate)
        {
            return mImpl.ApplyContributeRate(serverId, activityId, ref rate);
        }

        public static ErrorCodes SetActivityResult(int serverid, int activityId, int result)
        {
            return mImpl.SetActivityResult(serverid, activityId, result);
        }

        //设置世界等级
        public static void SetWorldLevel(int serverId, int activityId, int worldLevel)
        {
            mImpl.SetWorldLevel(serverId, activityId, worldLevel);
        }

        //获取服务器活动结果
        public static void ApplyActiResultList(int serverId, MieShiActivityResultList list)
        {
            foreach (var serverOne in Activity)
            {
                MieshiActiResultUnit result = new MieshiActiResultUnit();
                result.serverId = serverOne.Key;
                result.result = serverOne.Value.DBData.LastResult;
                list.Datas.Add(result);
            }
        }

        //请求进入活动
        public static ErrorCodes ApplyEnterActivity(ulong characterId, int serverId, int activityId)
        {
            return mImpl.ApplyEnterActivity(characterId, serverId, activityId);
        }

        public static CommonActivityInfo SaveGuidAndGetActiInfo(ulong sceneId,int serverId, int activityId, MieShiBatteryGuid guidList)
        {
            return mImpl.SaveGuidAndGetActiInfo(sceneId,serverId, activityId, guidList);
        }

        //请求进入活动
        public static void SyncActivityAllPlayerExit(ulong characterId, int serverId, int activityId)
        {
            mImpl.SyncActivityAllPlayerExit(serverId, activityId);
        }

        //同步可以拾取的宝箱id
        public static ErrorCodes SyncCanPickUpBox(int serverId, int activityId, int npcId)
        {
            return mImpl.SyncCanPickUpBox(serverId, activityId, npcId);
        }

        //请求获取宝箱奖励
        public static ErrorCodes ApplyPickUpBox(int serverId, int activityId, int npcId, ulong characterId)
        {
            return mImpl.ApplyPickUpBox(serverId, activityId, npcId, characterId);
        }

        //请求获取雕像数据
        public static ErrorCodes ApplyPortraitData(int serverId, ref PlayerInfoMsg data)
        {
            return mImpl.ApplyPortraitData(serverId, ref data);
        }

        //请求获取雕像奖励
        public static ErrorCodes ApplyPortraitAward(int serverId, ulong characterId)
        {
            return mImpl.ApplyPortraitAward(serverId, characterId);
        }

        //保存炮台摧毁数据
        public static ErrorCodes SaveBatteryDestroy(int serverId, int activityId, ulong objId)
        {
            return mImpl.SaveBatteryDestroy(serverId, activityId, objId);
        }
    }
}