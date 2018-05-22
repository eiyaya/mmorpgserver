#region using

using System;
using System.Collections;
using System.Collections.Generic;
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
    public interface IAcientBattle
    {
        void Construct(AcientBattle _this, int serverId);
    }

    public class AcientBattleDefaultImpl : IAcientBattle
    {
        [Updateable("AcientBattle")]
        public const string DbKey = "WAcientBattle:";
        private static readonly Logger Logger = LogManager.GetLogger("AcientBattle");

        public void Construct(AcientBattle _this, int serverId)
        {
            _this.ServerId = serverId;
            GetDbActiData(_this);
        }

        #region DB

        public void SetActiData(AcientBattle _this, DBAcientBattleActivityData data)
        {
            if (data == null)
                return;
            _this.DBData = data;
            if (AcientBattleManager.Boss == null)
                AcientBattleManager.Boss = new Dictionary<int, AcientBattle>();
            if (AcientBattleManager.Boss.ContainsKey(_this.ServerId))
                AcientBattleManager.Boss[_this.ServerId] = _this;
            else
                AcientBattleManager.Boss.Add(_this.ServerId, _this);

        }

        private void Save(AcientBattle oData)
        {
            CoroutineFactory.NewCoroutine(SaveCoroutine, oData).MoveNext();
        }

        private IEnumerator SaveCoroutine(Coroutine co, AcientBattle oData)
        {
            if (oData == null || AcientBattleManager.Boss == null || AcientBattleManager.Boss[oData.ServerId] == null)
                yield break;
            var ret = ActivityServer.Instance.DB.Set(co, DataCategory.AcientBattle, DbKey + oData.ServerId,
                AcientBattleManager.Boss[oData.ServerId].DBData);
            yield return ret;
        }

        private void GetDbActiData(AcientBattle _this)
        {
            CoroutineFactory.NewCoroutine(GetDbActiDataCoroutine, _this).MoveNext();
        }

        private IEnumerator GetDbActiDataCoroutine(Coroutine co, AcientBattle _this)
        {
            if (_this == null)
                yield break;
            var dbActiList = ActivityServer.Instance.DB.Get<DBAcientBattleActivityData>(co, DataCategory.AcientBattle,
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
        }

        #endregion



        private void InitData(AcientBattle _this)
        {
            var item = new DBAcientBattleActivityData();
            Table.ForeachAcientBattleField(record =>
            {
                //1:死亡   0:活着
                item.BossState.Add(record.Id, 0);
                //-1:未死亡     不是-1：死亡倒计时
                item.BossDieTime.Add(record.Id, -1);
                return true;
            });
            _this.DBData = item;
            if (AcientBattleManager.Boss == null)
                AcientBattleManager.Boss = new Dictionary<int, AcientBattle>();
            if (AcientBattleManager.Boss.ContainsKey(_this.ServerId))
                AcientBattleManager.Boss[_this.ServerId] = _this;
            else
                AcientBattleManager.Boss.Add(_this.ServerId, _this);
        }

    }

    public class AcientBattle
    {
        #region 数据

        public int ServerId;

        private static IAcientBattle mImpl;

        public DBAcientBattleActivityData DBData;
        #endregion

        #region Init

        static AcientBattle()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof(AcientBattle), typeof(AcientBattleDefaultImpl),
                o => { mImpl = (IAcientBattle)o; });
        }

        public AcientBattle(int serverId)
        {
            mImpl.Construct(this, serverId);
        }
        #endregion
    }

    public interface IAcientBattleManager
    {
        void Init();
        Dictionary<int, int> RefreshActivityData(int serverId, int npcId);
    }

    public class AcientBattleManagerDefaultImpl : IAcientBattleManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Init()
        {
            Table.ForeachServerName(record =>
            {
                var id = record.LogicID;
                if (!AcientBattleManager.Boss.ContainsKey(id) && record.IsClientDisplay == 1)
                {
                    var serverTemp = new AcientBattle(id);
                    AcientBattleManager.Boss.Add(id, serverTemp);
                    if (serverTemp.DBData == null)
                        serverTemp.DBData = new DBAcientBattleActivityData();
                    Table.ForeachAcientBattleField(recordAcient =>
                    {
                        //1:死亡   0:活着
                        serverTemp.DBData.BossState.Add(recordAcient.Id, 0);

                        //-1:未死亡     不是-1：死亡倒计时
                        serverTemp.DBData.BossDieTime.Add(recordAcient.Id, -1);
                        return true;
                    });

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
                    if (!AcientBattleManager.Boss.ContainsKey(id) && record.IsClientDisplay == 1)
                    {
                        var serverTemp = new AcientBattle(id);
                        AcientBattleManager.Boss.Add(id, serverTemp);
                        if (serverTemp.DBData == null)
                            serverTemp.DBData = new DBAcientBattleActivityData();
                        Table.ForeachAcientBattleField(recordAcient =>
                        {
                            //1:死亡   0:活着
                            serverTemp.DBData.BossState.Add(recordAcient.Id, 0);

                            //-1:未死亡     不是-1：死亡倒计时
                            serverTemp.DBData.BossDieTime.Add(recordAcient.Id, -1);
                            return true;
                        });

                    }
                    return true;
                });
            }
        }

        #region 私有方法

        public Dictionary<int, int> RefreshActivityData(int serverId, int npcId)
        {
            if (AcientBattleManager.Boss == null || AcientBattleManager.Boss[serverId] == null || AcientBattleManager.Boss[serverId].DBData == null)
            {
                Logger.Warn("AcientBattleManage   DBData.BossState    is      null");
                return null;
            }
            var acient = AcientBattleManager.Boss[serverId];
            Table.ForeachAcientBattleField(tb =>
            {
                if (tb.CharacterBaseId == npcId)
                {
                    acient.DBData.BossState[tb.Id] = 1;
                    acient.DBData.BossDieTime[tb.Id] = (int)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                    return false;
                }
                return true;
            });
            List<int> DieBossID = new List<int>();
            foreach (var time in acient.DBData.BossDieTime)
            {
                if (time.Value != -1)
                {
                    var cha = (int)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds - time.Value;
                    var refreshTime = Table.GetAcientBattleField(time.Key);
                    var npsRefresh = Table.GetNpcBase(refreshTime.CharacterBaseId);
                    if (cha >= npsRefresh.ReviveTime / 1000)
                    {
                        DieBossID.Add(time.Key);
                    }
                }
            }
            foreach (var id in DieBossID)
            {
                acient.DBData.BossDieTime[id] = -1;
            }
            return acient.DBData.BossDieTime;
        }

        #endregion
    }

    public static class AcientBattleManager
    {
        public static Dictionary<int, AcientBattle> Boss = new Dictionary<int, AcientBattle>();
        private static IAcientBattleManager mImpl;
        static AcientBattleManager()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof(AcientBattleManager),
                typeof(AcientBattleManagerDefaultImpl),
                o => { mImpl = (IAcientBattleManager)o; });
        }

        public static void Init()
        {
            mImpl.Init();
        }

        public static Dictionary<int, int> RefreshAcientBattleData(int serverId, int npcId)
        {
            var temp = Table.GetServerName(serverId);
            if (temp == null)
                return null;
            return mImpl.RefreshActivityData(temp.LogicID, npcId);
        }

        //public static void SaveDB()
        //{
        //    mImpl.SaveDB();
        //}
    }
}
