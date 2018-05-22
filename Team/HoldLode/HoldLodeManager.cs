#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using Database;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using NLog;
using Shared;
using TeamServerService;

#endregion

namespace Team
{
    #region 矿点管理类
    public interface ILodeManager
    {
        #region 数据
        void Init(LodeManager _this, int serverId);
        IEnumerator FlushAll(Coroutine coroutine, LodeManager _this);
        string GetDbName(int serverId);
        void InitByBase(LodeManager _this);
        #endregion
        #region 逻辑

        int OnPlayerHoldLode(LodeManager _this, int allianceId, int sceneId);
        int OnPlayerCollectLode(LodeManager _this, ulong characterId, int allianceId, int sceneId, int lodeId, int add, FieldRankBaseData data);
        int ApplyHoldLode(LodeManager _this, ref MsgSceneLodeList msg);
        int ApplyHoldLode(LodeManager _this, int sceneId, ref MsgSceneLode msg);
        void StartActiveLogic(LodeManager _this);
        void EndActiveLogic(LodeManager _this);
        void FinalLogic(LodeManager _this);

        ErrorCodes OnPlayerApplyMissionReward(LodeManager _this, int allianceId, ulong charId, int missionId,int score,int add,FieldRankBaseData data);
        ErrorCodes CheckAllianceMission(LodeManager _this, Alliance alliance, int missionId);

        void OnAllianceEvent(LodeManager _this,int allianceId, int EventType, int param = 1);
        DBAllianceTaskList GetAllianceActive(LodeManager _this, Alliance alliance);
        void OnPlayerAddScore(LodeManager _this, ulong charId,int allianceId,int add, FieldRankBaseData data);
        void OnAllianceAddScore(LodeManager _this, int allianceId,int add);

        //void ChangeNameTitle(ulong characterId, Dict_int_int_Data change);
        //IEnumerator ChangeNameTitleCoroutine(Coroutine co, ulong characterId, Dict_int_int_Data change);


        #endregion
    }

    public class ILodeManagerDefaultImpl //单个服务器的帮派信息（支持合服后，多服务器对应同一个类）
        : ILodeManager
    {
        [Updateable("Lode")]
        private const int eHoldFlag = 0;
        [Updateable("LogicProxy")]
        private const int eCollectCount = 1;
        [Updateable("LogicProxy")]
        private const int eGive = 2;
        [Updateable("LogicProxy")]
        private const int eSaveEquip = 3;
        #region 数据
        public void Init(LodeManager _this, int serverId)
        {
            //Debug.Assert(false);
            CoroutineFactory.NewCoroutine(ReadDb, _this, serverId).MoveNext();
        }

        public void InitByBase(LodeManager _this)
        {

            Table.ForeachWarFlag(tb =>
            {
                if (_this.mDbData.SceneLodeList.ContainsKey(tb.FlagInMap) == true)
                    return true;
                var tmp = new DBSceneLode();
                tmp.SceneId = tb.FlagInMap;
                tmp.FlagId = tb.Id;
                DateTime t = DateTime.Now.Date.AddHours((int)(tb.BelongToTime[0] / 100)).AddMinutes(tb.BelongToTime[0]%100) ;
                if (t < DateTime.Now)
                {
                    t.AddDays(1);
                }
                tmp.ResetTime = (int)(t - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                _this.mDbData.SceneLodeList.Add(tb.FlagInMap, tmp);
                return true;
            });
            Table.ForeachLode(record =>
            {
                var sceneId = record.LodeInMap;
                if(_this.mDbData.SceneLodeList.ContainsKey(sceneId) == false)
                {
                    return true;
                }
                var l = _this.mDbData.SceneLodeList[sceneId];
                if (l.LodeList.ContainsKey(record.Id) == false)
                {
                    var lode = new DBLode();
                    lode.Id = record.Id;
                    lode.Times = record.CanCollectNum;
                    l.LodeList.Add(record.Id, lode);                    
                }
                return true;
            });

            {//重点活动相关
                if (_this.mDbData.ActivityInfo == null)
                    _this.mDbData.ActivityInfo = new DBActiveTask();
                //一上来先调用一次
                OnDayTimer(_this);
                var t = DateTime.Now.Date.AddDays(1).AddSeconds(5);
                TeamServerControl.tm.CreateTrigger(t, ()=> { OnDayTimer(_this); }, 60*60*24*1000);
            }
        }

        private void OnDayTimer(LodeManager _this)
        {//每日回调
            var serverInfo = Table.GetServerName(_this.ServerId);
            DateTime ServerOpenDate = DateTime.Parse(serverInfo.OpenTime);
            var week = (int)ServerOpenDate.DayOfWeek;
            var todayWeek = (int)DateTime.Now.DayOfWeek;
            if (week == 0)
                week = 7;
            var tbActivity = Table.GetMainActivity(week);
            if (tbActivity == null)
                return;
            //下次更新的时间
            if (tbActivity.Week[todayWeek] == 2)
            {//开始活动
                StartActiveLogic(_this);
            }
            else
            {//未到更新时间,继续活动
                EndActiveLogic(_this);
            }
            _this.bDirty = true;
        }

        public void StartActiveLogic(LodeManager _this)
        {
            if (_this.mDbData.ActivityInfo.ActivityStatus == 0 || _this.mDbData.ActivityInfo.aTaskIDs.Count > 3 || _this.mDbData.ActivityInfo.pTaskIDs.Count>3
                || _this.mDbData.ActivityInfo.UpdateTime == 0 || DateTime.FromBinary(_this.mDbData.ActivityInfo.UpdateTime).DayOfYear != DateTime.Now.DayOfYear)
            {//重新生成任务列表
                _this.mDbData.ActivityInfo.PlayerTaskList.Clear();
                _this.mDbData.ActivityInfo.AllianceTaskList.Clear();
                _this.mDbData.ActivityInfo.aTaskIDs.Clear();
                _this.mDbData.ActivityInfo.pTaskIDs.Clear();
                _this.mDbData.ActivityInfo.UpdateTime = DateTime.Now.ToBinary(); 
                List<int> al = new List<int>();
                List<int> pl = new List<int>();
                Table.ForeachObjectTable(tb =>
                {
                    if (tb.TaskType == 0)
                    {
                        pl.Add(tb.Id);
                    }
                    else if(tb.TaskType == 1)
                    {
                        al.Add(tb.Id);
                    }
                    return true;
                });
                for (int i = 0; i < 10; i++)
                {
                    int tmp = 0;
                    {
                        int half = (int)al.Count / 2;
                        int a = MyRandom.Random(0, half - 1);
                        int b = MyRandom.Random(half, al.Count - 1);
                        tmp = al[a];
                        al[a] = al[b];
                        al[b] = tmp;
                    }
                    {
                        int half = (int)pl.Count / 2;
                        int a = MyRandom.Random(0, half - 1);
                        int b = MyRandom.Random(half, pl.Count - 1);
                        tmp = pl[a];
                        pl[a] = pl[b];
                        pl[b] = tmp;
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    if (pl.Count > i + 1)
                    {
                        _this.mDbData.ActivityInfo.pTaskIDs.Add(pl[i]);
                    }
                    if (al.Count > i + 1)
                    {
                        _this.mDbData.ActivityInfo.aTaskIDs.Add(al[i]);
                    }
                }
            }
            _this.mDbData.ActivityInfo.ActivityStatus = 1;
            var l = Table.GetServerConfig(3007).Value.Split('|');
            if (l == null || l.Count() != 2)
                return;
            {//开始 加了10分钟容错
                var n = int.Parse(l[0]);
                var t = DateTime.Now.Date.AddHours(Math.Floor((double)n / 100)).AddMinutes(n % 100);
                if (t.AddMinutes(10) < DateTime.Now)
                    return;
                TeamServerControl.tm.CreateTrigger(t, () => { StartActivityReFreshLode(_this); });                
            }
            {//结算
                var n = int.Parse(l[1]);
                var t = DateTime.Now.Date.AddHours(Math.Floor((double)n / 100)).AddMinutes(n % 100);
                if (t.AddMinutes(10) < DateTime.Now)
                    return;
                TeamServerControl.tm.CreateTrigger(t, () => { BroadCastWarFlagInfo(_this); });                
            }
        }

        private void StartActivityReFreshLode(LodeManager _this)
        {
            List<int> l = new List<int>();
            foreach(var v in _this.mDbData.SceneLodeList)
            {
                foreach (var lode in v.Value.LodeList)
                {
                    var tb = Table.GetLode(lode.Key);
                    if (tb == null)
                        continue;
                    lode.Value.Times = tb.CanCollectNum;
                    lode.Value.UpdateTime = 0;
                }
                l.Add(v.Key);
            }
            CoroutineFactory.NewCoroutine(BroadCastLodeInfo,_this.ServerId,l).MoveNext();
            _this.bDirty = true;
        }
        private IEnumerator BroadCastLodeInfo(Coroutine co,int serverId, List<int> ids)
        {
            Int32Array l = new Int32Array();
            l.Items.AddRange(ids);
            var msg = TeamServer.Instance.SceneAgent.NotifyRefreshLodeTimer(serverId,l);
            yield return msg.SendAndWaitUntilDone(co);
        }

        private void BroadCastWarFlagInfo(LodeManager _this)
        {
            MsgWarFlagInfoList msg = new MsgWarFlagInfoList();
            foreach (var v in _this.mDbData.SceneLodeList)
            {
                MsgWarFlagInfo cell = new MsgWarFlagInfo();
                cell.id = v.Value.FlagId;
                cell.allianceId = v.Value.TeamId;
                cell.name = v.Value.TeamName;
                msg.list.Add(cell);
            }
            TeamServer.Instance.TeamAgent.NotifyFieldFinal((uint)_this.ServerId, msg);            
        }

        private void ChangeNameTitle(ulong characterId, Dict_int_int_Data change)
        {
            CoroutineFactory.NewCoroutine(ChangeNameTitleCoroutine, characterId, change).MoveNext();
        }

        private IEnumerator ChangeNameTitleCoroutine(Coroutine co, ulong characterId, Dict_int_int_Data change)
        {
            var msg = TeamServer.Instance.LogicAgent.SSSetFlag(characterId, change);
            yield return msg.SendAndWaitUntilDone(co);
        }

        public void FinalLogic(LodeManager _this)
        {
            #region 给排行前的玩家添加称号
            //给排行前的玩家添加称号
            do
            {
                List<DBAllianceTaskList> l = new List<DBAllianceTaskList>();
                List<int> lName = new List<int>();
                foreach (var v in _this.mDbData.ActivityInfo.AllianceTaskList)
                {
                    lName.Add(v.Key);
                }
                for (int i = 0; i < lName.Count; i++)
                {
                    l.Add(_this.mDbData.ActivityInfo.AllianceTaskList[lName[i]]);
                }
                l.Sort((a, b) =>
                {
                    if (b.Flags > a.Flags)
                        return 1;
                    if (b.Flags < a.Flags)
                        return -1;
                    return b.Score - a.Score;
                });
                if (l.Count == 0)
                    break;
                var al = l[0];

                var alliance = ServerAllianceManager.GetAllianceById(al.Id);
                if (alliance == null)
                {
                    break;
                }
                {
                    Dictionary<int,Dict_int_int_Data> title = new Dictionary<int, Dict_int_int_Data>();
                    {
                        var tb = Table.GetNameTitle(8000);
                        if (tb == null)
                            break;
                        Dict_int_int_Data tmp = new Dict_int_int_Data();
                        tmp.Data.Add(tb.FlagId,1);
                        title.Add((int)eAllianceLadder.Chairman,tmp);
                    }
                    {
                        var tb = Table.GetNameTitle(8001);
                        if (tb == null)
                            break;
                        Dict_int_int_Data tmp = new Dict_int_int_Data();
                        tmp.Data.Add(tb.FlagId,1);
                        title.Add((int)eAllianceLadder.ViceChairman,tmp);
                    }
                    {
                        var tb = Table.GetNameTitle(8002);
                        if (tb == null)
                            break;
                        Dict_int_int_Data tmp = new Dict_int_int_Data();
                        tmp.Data.Add(tb.FlagId,1);
                        title.Add((int)eAllianceLadder.Elder,tmp);
                    }
                    {
                        var tb = Table.GetNameTitle(8003);
                        if (tb == null)
                            break;
                        Dict_int_int_Data tmp = new Dict_int_int_Data();
                        tmp.Data.Add(tb.FlagId,1);
                        title.Add((int)eAllianceLadder.Member,tmp);
                    }

                    var dad = alliance.Dad;
                    foreach (var id in alliance.mDBData.Members)
                    {
                        var m = dad.GetCharacterData(id);
                        if (m == null)
                        {
                            continue;
                        }
                        if (title.ContainsKey(m.Ladder) == true)
                        {
                            ChangeNameTitle(id, title[m.Ladder]);
                        }
                    }
                }
            } while (false);

            #endregion
            #region 给排行前的工会中的玩家添加称号
            //给排行前的工会中的玩家添加称号
            {
                Dictionary<int, Dict_int_int_Data> title = new Dictionary<int, Dict_int_int_Data>();
                {
                    var tb = Table.GetNameTitle(9000);
                    if (tb != null)
                    {
                        Dict_int_int_Data tmp = new Dict_int_int_Data();
                        tmp.Data.Add(tb.FlagId, 1);
                        title.Add(0, tmp);                        
                    }
                }
                {
                    var tb = Table.GetNameTitle(9001);
                    if (tb != null)
                    {
                        Dict_int_int_Data tmp = new Dict_int_int_Data();
                        tmp.Data.Add(tb.FlagId, 1);
                        title.Add(1, tmp);
                    }
                }
                {
                    var tb = Table.GetNameTitle(9002);
                    if (tb != null)
                    {
                        Dict_int_int_Data tmp = new Dict_int_int_Data();
                        tmp.Data.Add(tb.FlagId, 1);
                        title.Add(2, tmp);
                    }
                }

                for (int i = 0; i < 3 && i < _this.mDbData.ActivityInfo.PlayerTaskList.Count; i++)
                {
                    var id = _this.mDbData.ActivityInfo.PlayerTaskList[i].guid;
                    if (title.ContainsKey(i) == true)
                        ChangeNameTitle(id, title[i]);
                }
            }
            #endregion


        }
        public void EndActiveLogic(LodeManager _this)
        {
            if (_this.mDbData.ActivityInfo.ActivityStatus == 1)
            {//结算逻辑
                FinalLogic(_this);
                //_this.mDbData.ActivityInfo.PlayerTaskList.Clear();
                //_this.mDbData.ActivityInfo.AllianceTaskList.Clear();
                //_this.mDbData.ActivityInfo.aTaskIDs.Clear();
                //_this.mDbData.ActivityInfo.pTaskIDs.Clear();

            }
            _this.mDbData.ActivityInfo.ActivityStatus = 0;
        }
        public IEnumerator FlushAll(Coroutine coroutine, LodeManager _this)
        {
            if(_this.bDirty == false)
                yield break;
            _this.bDirty = false;
            //_this.mDBData
            var co = CoroutineFactory.NewSubroutine(SaveDb, coroutine, _this);
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        public string GetDbName(int serverId)
        {
            return string.Format("HoldLode_{0}", serverId);
        }

        private IEnumerator SaveDb(Coroutine coroutine, LodeManager _this)
        {
            if (_this.mDbData != null)
            {
                PlayerLog.WriteLog((int)LogType.LodeSave,
                    "--------------------SaveLodeData--------------------s={0}", _this.mDbData);
                var ret = TeamServer.Instance.DB.Set(coroutine, DataCategory.TeamLode,
                    GetDbName(_this.ServerId), _this.mDbData);
                yield return ret;
            }
        }

        //读取数据
        private IEnumerator ReadDb(Coroutine coroutine, LodeManager _this, int serverId)
        {
            var tasks = TeamServer.Instance.DB.Get<DBLodeManager>(coroutine, DataCategory.TeamLode,
                GetDbName(serverId));
            yield return tasks;
            _this.ServerId = serverId;
            if (tasks.Data == null)
            {
                _this.InitByBase();
                yield break;
            }
            //Debug.Assert(false);
            foreach (var lodedata in tasks.Data.SceneLodeList)
            {
                var tbFlag = Table.GetWarFlag(lodedata.Value.FlagId);
                if (tbFlag == null)
                    continue;
                var tmp = new DBSceneLode();
                foreach (var l in lodedata.Value.LodeList)
                {
                    var lode = l.Value;
                    var tb = Table.GetLode(lode.Id);
                    if (tb == null)
                        continue;
                    tmp.LodeList.Add(l.Key,l.Value);                    
                }

                tmp.SceneId = lodedata.Value.SceneId;
                tmp.TeamId = lodedata.Value.TeamId;
                tmp.TeamName = lodedata.Value.TeamName;
                
                DateTime rstTime = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(lodedata.Value.ResetTime);
                if (rstTime < DateTime.Now)
                {
                    tmp.TeamId = 0;
                    tmp.TeamName = "";
                }
                rstTime = DateTime.Now.Date.AddHours((int)(tbFlag.BelongToTime[0] / 100)).AddMinutes(tbFlag.BelongToTime[0] % 100);
                if (rstTime < DateTime.Now)
                {
                    rstTime.AddDays(1);
                }
                tmp.ResetTime = (int)(rstTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

                tmp.FlagId = lodedata.Value.FlagId;
                _this.mDbData.SceneLodeList.Add(lodedata.Value.SceneId, tmp);
            }
            _this.mDbData.ActivityInfo = tasks.Data.ActivityInfo;

            _this.InitByBase();
        }
#endregion
        #region 逻辑

        public int OnPlayerHoldLode(LodeManager _this, int allianceId, int sceneId)
        {
            if (_this.mDbData.ActivityInfo.ActivityStatus == 0)
                return (int)ErrorCodes.Error_Lode_Hold_ErrorTime; 
            DBSceneLode l;
            if (false == _this.mDbData.SceneLodeList.TryGetValue(sceneId, out l))
            {
                return (int)(ErrorCodes.Error_Lode_ErrorId);
            }
            var tbFlag = Table.GetWarFlag(l.FlagId);
            if(tbFlag == null)
                return (int)(ErrorCodes.Error_Lode_ErrorId);
            DateTime st = DateTime.Now.Date.AddHours((int)(tbFlag.BelongToTime[0]/100)).AddMinutes(tbFlag.BelongToTime[0]%100);
            DateTime ed = DateTime.Now.Date.AddHours((int)(tbFlag.BelongToTime[1] / 100)).AddMinutes(tbFlag.BelongToTime[1] % 100);
            DateTime rstTime = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(l.ResetTime);
            if (rstTime < DateTime.Now)
            {
                l.ResetTime += 60 * 60 * 24;
                l.TeamId = 0;
                l.TeamName = "";
            }
            if (DateTime.Now < st || DateTime.Now > ed)
            {
                return (int) ErrorCodes.Error_Lode_Hold_ErrorTime;
            }

            l.TeamId = allianceId;
            l.TeamName = ServerAllianceManager.GetAllianceName(allianceId);
            _this.bDirty = true;
            OnAllianceEvent(_this, allianceId, eHoldFlag);
            return (int)ErrorCodes.OK;
        }
        public int OnPlayerCollectLode(LodeManager _this,ulong characterId, int allianceId, int sceneId,int lodeId,int add,FieldRankBaseData data)
        {
            var tb = Table.GetLode(lodeId);
            if(tb == null)
                return (int)(ErrorCodes.Error_Lode_ErrorId);
            DBSceneLode l;
            if (false == _this.mDbData.SceneLodeList.TryGetValue(sceneId, out l))
            {
                return (int)(ErrorCodes.Error_Lode_ErrorId);
            }
            DBLode dbLode;
            if (false == l.LodeList.TryGetValue(lodeId,out dbLode))
            {
                return (int)(ErrorCodes.Error_Lode_ErrorId);
            }
            System.DateTime startTime = DataTimeExtension.EpochStart; // 当地时区
            var now = DateTime.Now;
            if (dbLode.Times <= 0)
            {
                DateTime dt = startTime.AddSeconds(dbLode.UpdateTime);
                if (dt > DateTime.Now)
                    return (int) (ErrorCodes.Error_Lode_Collect_ErrorTimes);
                else
                {
                    dbLode.UpdateTime = 0;
                    dbLode.Times = tb.CanCollectNum;
                }
            }
            if (--dbLode.Times == 0)
            {
                dbLode.UpdateTime = (long)(now.AddSeconds(tb.LodeRefreshTime) - startTime).TotalSeconds;
            }

            _this.bDirty = true;
            OnAllianceEvent(_this, allianceId, eCollectCount);
            if (add > 0)
            {
                //OnAllianceAddScore(_this,alliance:)
                OnPlayerAddScore(_this,characterId,allianceId,add,data);
            }
            return (int)ErrorCodes.OK;
        }
        public int ApplyHoldLode(LodeManager _this,ref MsgSceneLodeList msg)
        {
            System.DateTime startTime = DataTimeExtension.EpochStart; // 当地时区
            List<int> keys = new List<int>(_this.mDbData.SceneLodeList.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                //foreach (var tmp in _this.mDbData.SceneLodeList)                
                var tmp = _this.mDbData.SceneLodeList[keys[i]];
                MsgSceneLode sceneLode = new MsgSceneLode();
                sceneLode.SceneId = tmp.SceneId;
                sceneLode.TeamId = tmp.TeamId;
                sceneLode.TeamName = tmp.TeamName;
                sceneLode.FlagId = tmp.FlagId;
                DateTime rstTime = new DateTime(1970,1,1,0,0,0).AddSeconds(tmp.ResetTime);
                if (rstTime < DateTime.Now)
                {
                    tmp.ResetTime += 60*60*24;
                    sceneLode.TeamId = 0;
                    sceneLode.TeamName = "";
                }

                foreach (var v in tmp.LodeList)
                {
                    msgLode l = new msgLode();
                    l.Id = v.Value.Id;
                    if (null == v.Value)
                    {
                        DateTime dt = startTime.AddSeconds(v.Value.UpdateTime);
                        if (dt < DateTime.Now && l.Times <= 0)
                        {
                            var tb = Table.GetLode(l.Id);
                            if (tb != null)
                            {
                                v.Value.Times = tb.CanCollectNum;
                                v.Value.UpdateTime = 0;
                            }
                        }                        
                    }
                    l.Times = v.Value.Times;
                    l.UpdateTime = v.Value.UpdateTime;
                    sceneLode.LodeList.Add(v.Key, l);
                }
                msg.LodeList.Add(sceneLode);
            }
            return (int)ErrorCodes.OK;
        }

        public int ApplyHoldLode(LodeManager _this, int sceneId, ref MsgSceneLode msg)
        {
            DBSceneLode sl = null;
            if (false == _this.mDbData.SceneLodeList.TryGetValue(sceneId, out sl))
            {
                return (int) ErrorCodes.Unknow;
            }
            System.DateTime startTime = DataTimeExtension.EpochStart; // 当地时区
            DateTime rstTime = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(sl.ResetTime);
            if (rstTime < DateTime.Now)
            {
                sl.ResetTime += 60 * 60 * 24;
                sl.TeamId = 0;
                sl.TeamName = "";
            }

            foreach (var v in sl.LodeList)
            {
                msgLode l = new msgLode();
                l.Id = v.Value.Id;
                if (null == v.Value)
                {
                    DateTime dt = startTime.AddSeconds(v.Value.UpdateTime);
                    if (dt < DateTime.Now && v.Value.Times <= 0)
                    {
                        var tb = Table.GetLode(l.Id);
                        if (tb != null)
                        {
                            v.Value.Times = tb.CanCollectNum;
                        }
                    }
                }
                l.Times = v.Value.Times;
                l.UpdateTime = v.Value.UpdateTime;
                msg.LodeList.Add(v.Key, l);
            }
            msg.TeamId = sl.TeamId;
            msg.TeamName = sl.TeamName;
            msg.FlagId = sl.FlagId;
            msg.ResetTime = sl.ResetTime;
            return (int)ErrorCodes.OK;
        }

        public void OnPlayerAddScore(LodeManager _this, ulong charId, int allianceId, int add, FieldRankBaseData data)
        {
            if (_this.mDbData.ActivityInfo.ActivityStatus == 1)
            {
                var idx = _this.mDbData.ActivityInfo.PlayerTaskList.FindIndex(o => { return o.guid == charId; });
                DBPlayerTask db = null;
                if (idx >= 0)
                {
                    db = _this.mDbData.ActivityInfo.PlayerTaskList[idx];
                    _this.mDbData.ActivityInfo.PlayerTaskList.RemoveAt(idx);
                }
                if (db == null)
                    db = new DBPlayerTask();
                db.fight = data.FightPoint;
                db.guid = data.Guid;
                db.name = data.Name;
                db.role = data.TypeId;
                db.level = data.Level;
                db.score = data.Score;

                idx = _this.mDbData.ActivityInfo.PlayerTaskList.FindIndex(o => { return o.score < db.score; });
                if (idx >= 0)
                {
                    _this.mDbData.ActivityInfo.PlayerTaskList.Insert(idx, db);
                }
                else if (_this.mDbData.ActivityInfo.PlayerTaskList.Count < 50)
                {
                    _this.mDbData.ActivityInfo.PlayerTaskList.Add(db);
                }
                if (_this.mDbData.ActivityInfo.PlayerTaskList.Count > 50)
                {
                    _this.mDbData.ActivityInfo.PlayerTaskList.RemoveAt(50);
                }   
            }
            //玩家在公会内的贡献
            if (allianceId > 0)
            {
                OnAllianceAddScore(_this,allianceId,add);
            }
            _this.bDirty = true;
        }

        public void OnAllianceAddScore(LodeManager _this, int allianceId, int add)
        {
            var alliance = ServerAllianceManager.GetAllianceById(allianceId);
            if (alliance == null)
            {
                return;
            }
            if (_this.mDbData.ActivityInfo.ActivityStatus == 1)
            {//活动数据
                DBAllianceTaskList db = GetAllianceActive(_this, alliance);
                if (db == null)
                    return;
                db.Score += add;
            }
        }
        public ErrorCodes OnPlayerApplyMissionReward(LodeManager _this, int allianceId, ulong charId, int missionId, int meritPoint, int add, FieldRankBaseData data)
        {
            if (_this.mDbData.ActivityInfo.ActivityStatus == 0)
                return ErrorCodes.Error_ActivityOver;
            var tb = Table.GetObjectTable(missionId);
            if (allianceId >0 )
            {
                var alliance = ServerAllianceManager.GetAllianceById(allianceId);
                if (alliance == null)
                {
                    return ErrorCodes.Error_CharacterNoAlliance ;
                }
                if (tb.TaskType == 1)
                {
                    ErrorCodes err = CheckAllianceMission(_this, alliance, missionId);
                    if (err != ErrorCodes.OK)
                    {
                        return err;
                    }                                    
                }

                //if (meritPoint > 0)
                //{
                //    var member = alliance.Dad.GetCharacterData(charId);
                //    if (member != null)
                //    {
                //        member.MeritPoint += meritPoint;
                //    }
                //}

            }

            OnPlayerAddScore(_this, charId,allianceId, add, data);

            return ErrorCodes.OK;
        }

        public void OnAllianceEvent(LodeManager _this,int allianceId, int EventType, int param = 1)
        {
            if (_this.mDbData.ActivityInfo.ActivityStatus == 0)
                return;
            var alliance = ServerAllianceManager.GetAllianceById(allianceId);
            if (alliance == null)
            {
                return ;
            }
            bool bSend = false;
            DBAllianceTaskList db = GetAllianceActive(_this, alliance);

            for (int i = 0; i < _this.mDbData.ActivityInfo.aTaskIDs.Count; i++)
            {
                int id = _this.mDbData.ActivityInfo.aTaskIDs[i];
                DBAllianceTask t;
                if (db.TaskList.TryGetValue(id , out t) == false)
                {
                    continue;
                }
                if (t.Type == EventType && t.Count<t.Need)
                {
                    t.Count += param;
                    if (t.Count > t.Need)
                        t.Count = t.Need;
                }
                if (t.Count >= t.Need)
                    bSend = true;
            }
            if (EventType == eHoldFlag)
            {
                db.Flags ++;
            }

            if (bSend == true)
            {
                foreach (var id in alliance.mDBData.Members)
                {
                    TeamCharacterProxy Proxy;
                    if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(id, out Proxy))
                        if (null != Proxy)
                            Proxy.SCNotifyAllianceActiveTask(_this.mDbData.ActivityInfo);
                }                    
            }
        }

        public DBAllianceTaskList GetAllianceActive(LodeManager _this, Alliance alliance)
        {
            DBAllianceTaskList task ;
            if (false == _this.mDbData.ActivityInfo.AllianceTaskList.TryGetValue(alliance.AllianceId, out task))
            {
                task = new DBAllianceTaskList();
                task.Id = alliance.AllianceId;
                task.Name = alliance.Name;
                task.Score = 0;
                for (int i = 0; i < _this.mDbData.ActivityInfo.aTaskIDs.Count; i++)
                {
                    DBAllianceTask tt = new DBAllianceTask();
                    var tab = Table.GetObjectTable(_this.mDbData.ActivityInfo.aTaskIDs[i]);
                    if (tab == null)
                        continue;
                    tt.Id = tab.Id;
                    tt.Need = tab.NeedCount;
                    tt.Type = tab.EventType;
                    task.TaskList.Add(tt.Id, tt);
                }
                _this.mDbData.ActivityInfo.AllianceTaskList.Add(alliance.AllianceId, task);
            }

            task.Level = alliance.Level;
            task.Fight = alliance.GetTotleFightPoint();

            _this.bDirty = true;
            return task;
        }
        public ErrorCodes CheckAllianceMission(LodeManager _this, Alliance alliance, int missionId)
        {
            DBAllianceTaskList task = GetAllianceActive(_this,alliance);
            DBAllianceTask t = null;
            if (false == task.TaskList.TryGetValue(missionId, out t))
            {
                return ErrorCodes.Error_MissionID;
            }
            if (t.Count < t.Need)
            {
                return ErrorCodes.Error_ConditionNoEnough;
            }
            return ErrorCodes.OK;
        }
        #endregion 
    }

    public class LodeManager //单个服务器的帮派信息（支持合服后，多服务器对应同一个类）
    {
        public DBLodeManager mDbData = new DBLodeManager();
        public int ServerId;
        public bool bDirty = false;
        private static ILodeManagerDefaultImpl mImpl;
        static LodeManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof(LodeManager),
                typeof(ILodeManagerDefaultImpl),
                o => { mImpl = (ILodeManagerDefaultImpl)o; });
        }
        #region 数据
        public void InitByBase()
        {
            mImpl.InitByBase(this);
        }


        public void Init(int serverId)
        {
            mImpl.Init(this,serverId);
        }

        public IEnumerator FlushAll(Coroutine coroutine)
        {
            return mImpl.FlushAll(coroutine, this);
        }   

        public string GetDbName(int serverId)
        {
            return mImpl.GetDbName(serverId);
        }
        #endregion 
        #region 逻辑
        public int OnPlayerHoldLode(int allianceId, int sceneId)
        {
            return mImpl.OnPlayerHoldLode(this, allianceId, sceneId);
        }
        public int OnPlayerCollectLode(ulong characterId, int allianceId, int sceneId, int lodeId, int add, FieldRankBaseData data)
        {
            return mImpl.OnPlayerCollectLode(this,characterId,allianceId,sceneId,lodeId,add,data);
        }
        public int ApplyHoldLode(ref MsgSceneLodeList msg)
        {
            return mImpl.ApplyHoldLode(this,ref msg);
        }

        public int ApplyHoldLode( int sceneId, ref MsgSceneLode msg)
        {
            return mImpl.ApplyHoldLode(this, sceneId, ref msg);
        }

        public ErrorCodes OnPlayerApplyMissionReward(int allianceId, ulong charId, int missionId, int score,int add,FieldRankBaseData data)
        {
            return mImpl.OnPlayerApplyMissionReward(this, allianceId, charId, missionId,score,add,data);
        }

        public void OnAllianceEvent(int allianceId, int EventType, int param = 1)
        {
            mImpl.OnAllianceEvent(this,allianceId,EventType,param);
        }
        #endregion 

    }

#endregion


    public interface IServerLodeManager
    {
        void Init();
        int OnPlayerHoldLode(int serverId,int allianceId, int sceneId);
        int OnPlayerCollectLode(int serverId, ulong characterId, int allianceId, int sceneId, int lodeId, int add, FieldRankBaseData data);
        int ApplyHoldLode(int serverId, ref MsgSceneLodeList msg);
        int ApplyHoldLode(int serverId, int sceneId, ref MsgSceneLode msg);
        LodeManager GetLodeManager(int serverId);
        void Update();
        void OnAlliaceEvent(int serverId, int AllianceId, int EventId, int param = 1);
    }


    public class ServerLodeManagerDefaultImpl : IServerLodeManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Init()
        {
            Table.ForeachServerName(record =>
            {
                if (record.Id == record.LogicID && ServerLodeManagerManager.Servers.ContainsKey(record.LogicID)==false&&(record.IsClientDisplay == 1 || record.IsClientDisplay == 2))
                {
                    LodeManager temp = new LodeManager();
                    temp.Init(record.LogicID);
                    ServerLodeManagerManager.Servers.Add(record.LogicID, temp);                    
                }
                return true;
            });
            TeamServerControl.tm.CreateTrigger(DateTime.Now.AddSeconds(30), Update, 30000); //30秒一次
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
                    if (record.LogicID == record.Id && ServerLodeManagerManager.Servers.ContainsKey(record.LogicID) == false && (record.IsClientDisplay == 1 || record.IsClientDisplay == 2))
                    {
                        LodeManager temp = new LodeManager();
                        temp.Init(record.LogicID);
                        ServerLodeManagerManager.Servers.Add(record.LogicID, temp);
                    }
                    return true;
                });
            }
        }
        public void Update()
        {
            CoroutineFactory.NewCoroutine(RefreshAll).MoveNext();
        }

        private IEnumerator RefreshAll(Coroutine coroutine)
        {
            foreach (var lodeMgr in ServerLodeManagerManager.Servers)
            {
                var co = CoroutineFactory.NewSubroutine(lodeMgr.Value.FlushAll, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

   
        public int OnPlayerHoldLode(int serverId,int allianceId, int sceneId)
        {
            LodeManager mgr = GetLodeManager(serverId);
            if (mgr == null)
                return (int) ErrorCodes.Unknow;

            return mgr.OnPlayerHoldLode(allianceId, sceneId);
        }

        public int OnPlayerCollectLode(int serverId, ulong characterId, int allianceId, int sceneId, int lodeId, int add, FieldRankBaseData data)
        {
            LodeManager mgr = GetLodeManager(serverId);
            if (mgr == null)
                return (int)ErrorCodes.Unknow;

            return mgr.OnPlayerCollectLode(characterId,allianceId, sceneId,lodeId,add,data);
        }

        public int ApplyHoldLode(int serverId, ref MsgSceneLodeList msg)
        {
            var mgr = GetLodeManager(serverId);
            if (mgr == null)
            {
                return (int)ErrorCodes.Unknow;
            }
            mgr.ApplyHoldLode(ref msg);
            return (int)ErrorCodes.OK;
        }
        public int ApplyHoldLode(int serverId,int sceneId, ref MsgSceneLode msg)
        {
            var mgr = GetLodeManager(serverId);
            if (mgr == null)
            {
                return (int)ErrorCodes.Unknow;
            }
            mgr.ApplyHoldLode(sceneId,ref msg);
            return (int)ErrorCodes.OK;
        }

        public LodeManager GetLodeManager(int serverId)
        {
            LodeManager LodeMgr = null;
            if (ServerLodeManagerManager.Servers.TryGetValue(serverId, out LodeMgr) == false)
            {
                Logger.Error("GetLodeManager ========== is null serverId={0},{1}",serverId,new StackTrace().ToString());
                return null;
            }
            return LodeMgr;
        }

        public void OnAlliaceEvent(int serverId, int AllianceId, int EventId, int param = 1)
        {
            var lodeMgr = GetLodeManager(serverId);
            if (lodeMgr != null)
            {
                lodeMgr.OnAllianceEvent(AllianceId, EventId, param);
            }
        }
    }

    public static class ServerLodeManagerManager
    {
        public static Dictionary<int, LodeManager> Servers = new Dictionary<int, LodeManager>();
        private static IServerLodeManager mImpl;
        static ServerLodeManagerManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof(ServerLodeManagerManager),
                typeof(ServerLodeManagerDefaultImpl),
                o => { mImpl = (IServerLodeManager)o; });
        }

        public static void Init()
        {
            mImpl.Init();
        }

        public static int OnPlayerHoldLode(int serverId,int allianceId,int sceneId)
        {
            return mImpl.OnPlayerHoldLode(serverId, allianceId, sceneId);
        }

        public static int OnPlayerCollectLode(int serverId, ulong characterId, int allianceId, int sceneId, int lodeId, int add, FieldRankBaseData data)
        {
            return mImpl.OnPlayerCollectLode(serverId, characterId, allianceId, sceneId,lodeId,add,data);
        }
        public static int ApplyHoldLode(int serverId,int sceneId, ref MsgSceneLode msg)
        {
            return mImpl.ApplyHoldLode(serverId,sceneId,ref msg);
        }

        public static int ApplyHoldLode(int serverId ,ref MsgSceneLodeList msg)
        {
            return mImpl.ApplyHoldLode(serverId,ref msg);
        }

        public static void OnAlliaceEvent(int serverId, int AllianceId, int EventId, int param = 1)
        {
            mImpl.OnAlliaceEvent(serverId, AllianceId, EventId, param);
        }
    }
}