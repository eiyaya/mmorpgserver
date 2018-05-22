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
    public interface IBossHomeManager
    {
        void Init();
        void OnBossDie(int ServerId, int NpcId);
        Dictionary<int,int> GetBossInfo(int ServerId);

    }

    public class BossHomeManagerDefaultImpl : IBossHomeManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
                
        public void Init()
        {
            Table.ForeachServerName(record =>
            {
                var id = record.LogicID;

                if (!BossHomeManager.Boss.ContainsKey(id) && record.IsClientDisplay == 1)
                {
                    BossHomeManager.Boss.Add(id, new DBBossHomeActivityData());

                    Table.ForeachBossHome(tb =>
                    {
                        //1:死亡   0:活着
                        BossHomeManager.Boss[id].BossState.Add(tb.Id, 1);
                        
                        return true;
                    });
                }
                return true;
            });
            
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);

            Dictionary<int, List<int>> dic = new Dictionary<int, List<int>>();
            {//刷新trigger
                Table.ForeachBossHome(tb =>
                {
                    var t = tb.RefreshTime.Split('|');
                    if (t.Length > 0)
                    {
                        foreach (var v in t)
                        {
                            var tt = v.Split(':');
                            if (tt.Length > 1)
                            {
                                var iTime = int.Parse(tt[0])*100 + int.Parse(tt[1]);
                                if (dic.ContainsKey(iTime) == false)
                                {
                                    dic.Add(iTime,new List<int>());
                                }
                                dic[iTime].Add(tb.Id);
                            }
                        }
                    }
                    return true;
                });
                //Debug.Assert(false);
                foreach (var v in dic)
                {
                    DateTime t = DateTime.Now.Date.AddHours(v.Key/100).AddMinutes(v.Key%100);
                    if (t < DateTime.Now)
                    {
                        //CoroutineFactory.NewCoroutine(BossRefreshTimer, v.Value).MoveNext();
                        t = t.AddDays(1);
                    }

                    ActivityServerControl.Timer.CreateTrigger(t, () =>
                    {
                        CoroutineFactory.NewCoroutine(BossRefreshTimer,v.Value).MoveNext();
                    },24*60*60*1000);
                }
            }
        }
        private IEnumerator BossRefreshTimer(Coroutine co, List<int> bossList)
        {
            foreach (var v in BossHomeManager.Boss)
            {

                foreach (var id in bossList)
                {
                    if (v.Value.BossState.ContainsKey(id) && v.Value.BossState[id] == 1)
                    {
                        v.Value.BossState[id] = 0;
                    }
                }
            }
            var arr = new Int32Array();
            arr.Items.AddRange(bossList);
            var msg = ActivityServer.Instance.SceneAgent.NotifyRefreshBossHome(arr);
            msg.SendAndWaitUntilDone(co);
            {//半小时后杀死boss
                ActivityServerControl.Timer.CreateTrigger(DateTime.Now.AddMinutes(30), () =>
                {
                    var msg2 = ActivityServer.Instance.SceneAgent.NotifyBossHomeKill(0);
                    msg2.SendAndWaitUntilDone(co);
                });                
            }
            yield break;
        }
        private void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v == null)
                return;
            if (v.tableName == "ServerName")
            {
                Table.ForeachServerName(record =>
                {
                    var id = record.LogicID;
                    if (!BossHomeManager.Boss.ContainsKey(id) && record.IsClientDisplay == 1)
                    {
                        BossHomeManager.Boss.Add(id, new DBBossHomeActivityData());
                        Table.ForeachBossHome(tb =>
                        {
                            //1:死亡   0:活着
                            BossHomeManager.Boss[id].BossState.Add(tb.Id, 1);
                            return true;
                        });
                    }
                    return true;
                });
            }
           
        }

        #region 私有方法

        

        public void OnBossDie(int ServerId, int NpcId)
        {

            var serverId = ServerId;
            var tb = Table.GetServerName(ServerId);
            if (tb != null)
            {
                serverId = tb.LogicID;
            }

            if (BossHomeManager.Boss.ContainsKey(serverId))
            {
                var id = 0;
                Table.ForeachBossHome(record =>
                {
                    if (record.CharacterBaseId == NpcId)
                    {
                        id = record.Id;
                        return false;
                    }
                    return true;
                });
                if (BossHomeManager.Boss[serverId].BossState.ContainsKey(id))
                    BossHomeManager.Boss[serverId].BossState[id] = 1;
            }
        }

        public Dictionary<int, int> GetBossInfo(int ServerId)
        {
            var serverId = ServerId;
            var tb = Table.GetServerName(ServerId);
            if (tb != null)
            {
                serverId = tb.LogicID;
            }

            if (BossHomeManager.Boss.ContainsKey(serverId))
            {
                return BossHomeManager.Boss[serverId].BossState;
            }
            return new Dictionary<int, int>();
        }
        
        #endregion
    }

    public static class BossHomeManager
    {
        public static Dictionary<int, DBBossHomeActivityData> Boss = new Dictionary<int, DBBossHomeActivityData>();
        private static IBossHomeManager mImpl;
        static BossHomeManager()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof(BossHomeManager),
                typeof(BossHomeManagerDefaultImpl),
                o => { mImpl = (IBossHomeManager)o; });
        }

        public static void Init()
        {
            mImpl.Init();
        }

        public static Dictionary<int, int> RefreshBossHomeData(int serverId)
        {
           return mImpl.GetBossInfo(serverId);
        }

        public static void OnBossDie(int serverId, int npcId)
        {
            mImpl.OnBossDie(serverId, npcId);
        }
    }
}
