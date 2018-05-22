#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    public interface IGeneralActivity
    {
        void Init(GeneralActivity _this,int serverId);
        IEnumerator SaveDb(Coroutine coroutine, GeneralActivity _this);
        void FinalActivity(GeneralActivity _this);
        void AddPlayerScore(GeneralActivity _this, ulong guid, int score);
        void AddPlayerScore(GeneralActivity _this, Dictionary<ulong, int> dic);
    }

    public class GeneralActivityDefaultImpl : IGeneralActivity
    {
        public void Init(GeneralActivity _this,int serverId)
        {
            _this.serverId = serverId;
            CoroutineFactory.NewCoroutine(ReadDb, _this, serverId).MoveNext();
            ActivityServerControl.Timer.CreateTrigger(DateTime.Now.Date.AddDays(1).AddSeconds(5), () => { OnDayTimer(_this); }, 60 * 60 * 24 * 1000);
            OnDayTimer(_this);
        }
        public string GetDbName(int serverId)
        {
            return string.Format("GeneralActivity_{0}", serverId);
        }
        private IEnumerator ReadDb(Coroutine coroutine, GeneralActivity _this, int serverId)
        {
            var tasks = ActivityServer.Instance.DB.Get<DBGeneralActivityData>(coroutine, DataCategory.GeneralActivity,
                GetDbName(serverId));
            yield return tasks;
            if (tasks.Data == null)
            {
                yield break;
            }
            _this.mDbData.ID2Score.AddRange(tasks.Data.ID2Score);
        }
        public IEnumerator SaveDb(Coroutine coroutine, GeneralActivity _this)
        {
            if (_this.mDbData != null)
            {
                var ret = ActivityServer.Instance.DB.Set(coroutine, DataCategory.GeneralActivity,
                    GetDbName(_this.serverId), _this.mDbData);
                yield return ret;
            }
        }
        public ArenaRewardRecord GetArenaReward(int id)
        {
            ArenaRewardRecord tbAr = null;
            Table.ForeachArenaReward(record =>
            {
                if (id <= record.Id)
                {
                    tbAr = record;
                    return false;
                }
                return true;
            });
            return tbAr;
        }
        public void FinalActivity(GeneralActivity _this)
        {
            List<KeyValuePair<ulong,int>> tmpList = new List<KeyValuePair<ulong, int>>();
            foreach (var v in _this.mDbData.ID2Score)
            {
                tmpList.Add(v);
            }

            tmpList.Sort((a,b)=> { return a.Value - b.Value; });
            for (int i = 0; i < tmpList.Count; i++)
            {
                var tbArenaReward = GetArenaReward(i);
                if (tbArenaReward != null)
                {
                    var tbMail = Table.GetMail(97);
                    var reward = new Dictionary<int, int>();
                    if (tbArenaReward.DayMoney > 0)
                    {
                        reward.Add((int)eResourcesType.GoldRes, tbArenaReward.DayMoney);
                    }
                    if (tbArenaReward.DayDiamond > 0)
                    {
                        reward.Add((int)eResourcesType.DiamondBind, tbArenaReward.DayDiamond);
                    }
                    var index = 0;
                    foreach (var v in tbArenaReward.DayItemID)
                    {
                        if (i > 0 && tbArenaReward.DayItemCount[index] > 0)
                        {
                            reward.Add(v, tbArenaReward.DayItemCount[index]);
                        }
                        index++;
                    }
                    string content = string.Format(tbMail.Text, i);
                    var items = new Dict_int_int_Data();
                    items.Data.AddRange(reward);
                    CoroutineFactory.NewCoroutine(SendMailCoroutine, tmpList[i].Key, tbMail.Title, content, items).MoveNext();
                }
            }

            _this.mDbData.ID2Score.Clear();
            _this.bDirty = true;
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
        public void AddPlayerScore(GeneralActivity _this, ulong guid, int score)
        {
            _this.mDbData.ID2Score.modifyValue(guid, score);
            _this.bDirty = true;
        }

        public void AddPlayerScore(GeneralActivity _this, Dictionary<ulong, int> dic)
        {
            foreach (var v in dic)
            {
                _this.mDbData.ID2Score.modifyValue(v.Key, v.Value);
            }
            _this.bDirty = true;
        }

        private void OnDayTimer(GeneralActivity _this)
        {//每日回调
            var serverInfo = Table.GetServerName(_this.serverId);
            DateTime ServerOpenDate = DateTime.Parse(serverInfo.OpenTime);
            var week = (int)ServerOpenDate.DayOfWeek;
            var todayWeek = (int)DateTime.Now.DayOfWeek;
            if (week == 0)
                week = 7;
            var tbActivity = Table.GetMainActivity(week);
            if (tbActivity == null)
                return;
            //本次状态
            _this.bActivity = tbActivity.Week[todayWeek] == 3;
            if (_this.mDbData.LastTime != 0)
            {
                DateTime last = DateTime.FromBinary(_this.mDbData.LastTime);
                if (last.Date != DateTime.Now.Date)
                {
                    FinalActivity(_this);
                }                
            }
            if (_this.bActivity == true)
            {
                _this.mDbData.LastTime = DateTime.Now.ToBinary();
            }
            _this.bDirty = true;
        }
    }

    public class GeneralActivity
    {
        public  int serverId;
        public bool bDirty = false;
        public bool bActivity = false;
        public  DBGeneralActivityData mDbData = new DBGeneralActivityData();
        private static GeneralActivityDefaultImpl mImpl;
        static GeneralActivity()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof(GeneralActivity),
                typeof(GeneralActivityDefaultImpl),
                o => { mImpl = (GeneralActivityDefaultImpl)o; });
        }

        public void Init(int serverId)
        {
            mImpl.Init(this,serverId);
        }
        public IEnumerator SaveDb(Coroutine coroutine)
        {
            return mImpl.SaveDb(coroutine,this);
        }

        public void AddPlayerScore(ulong guid, int score)
        {
            mImpl.AddPlayerScore(this,guid,score);
        }

        public void AddPlayerScore(Dictionary<ulong, int> dic)
        {
            mImpl.AddPlayerScore(this,dic);
        }
    }





    public interface IGeneralActivityManager
    {
        void Init();
        GeneralActivity GetActivity(int serverId);
    }


    public class GeneralActivityManagerDefaultImpl : IGeneralActivityManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public void Init()
        {
            Table.ForeachServerName(record =>
            {
                if (record.LogicID == record.Id && (record.IsClientDisplay == 1 || record.IsClientDisplay == 2) && GeneralActivityManager.servers.ContainsKey(record.LogicID) == false)
                {
                    GeneralActivity act = new GeneralActivity();
                    act.Init(record.LogicID);
                    GeneralActivityManager.servers.Add(record.LogicID,act);
                }
                return true;
            });
            ActivityServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(30), Update, 30000); //30秒一次
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
                    if (record.LogicID == record.Id && (record.IsClientDisplay == 1 || record.IsClientDisplay == 2) && GeneralActivityManager.servers.ContainsKey(record.LogicID)==false)
                    {
                        GeneralActivity act = new GeneralActivity();
                        act.Init(record.LogicID);
                        GeneralActivityManager.servers.Add(record.LogicID, act);
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
            foreach (var server in GeneralActivityManager.servers)
            {
                if (server.Value.bDirty == false)
                    continue;
                server.Value.bDirty = false;

                var co = CoroutineFactory.NewSubroutine(server.Value.SaveDb, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        public GeneralActivity GetActivity(int serverId)
        {
            GeneralActivity tmp = null;
            GeneralActivityManager.servers.TryGetValue(serverId, out tmp);
            return tmp;
        }
    }

    public static class GeneralActivityManager
    {
        public static Dictionary<int, GeneralActivity> servers = new Dictionary<int, GeneralActivity>();
        private static IGeneralActivityManager mImpl;
        static GeneralActivityManager()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof(GeneralActivityManager),
                typeof(GeneralActivityManagerDefaultImpl),
                o => { mImpl = (IGeneralActivityManager)o; });
        }
        public static void Init()
        {
            mImpl.Init();
        }

        public static GeneralActivity GetActivity(int serverId)
        {
            return mImpl.GetActivity(serverId);
        }
    }
}
