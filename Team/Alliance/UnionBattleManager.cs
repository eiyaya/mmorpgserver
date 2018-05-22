/***************
 * 跨服盟战 
 ***************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Database;
using DataContract;
using DataTable;
using EventSystem;
using NLog;
using Scorpion;
using Shared;
using TeamServerService;

namespace Team
{
    public enum UnionBattleStage
    {
        Wait = -1,  // 等待
        Enroll = 0, // 报名
        Breakout1,  // 突围赛
        Breakout2,
        Breakout3,
        Breakout4,
        Quarterfinal,   // 1/4
        Semifinal, // 半决赛
        Final, // 决赛
        Over,
    }

    public interface IUnionBattleManager
    {
        void Init();
        long GetOpenTime();
        ErrorCodes Enroll(int allianceId);
        void NotifyBattleResult(int victoryId, int failId);
        ErrorCodes CanEnter(int allianceId);
        IEnumerator EnterFight(Coroutine coroutine, ulong characterId, int allianceId);
        IEnumerator SaveCoroutine(Coroutine co);
        bool FillBattleInfo(MsgUnionBattleInfo info, int allianceId);
        void FillMathInfo(MsgUnionBattleMathInfo info, int allianceId);
    }

    public class UnionBattleManagerDefaultImpl : IUnionBattleManager
    {
        [Updateable("UnionBattleManager")] private const string DbKey = "UnionBattle";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Init()
        {
            CoroutineFactory.NewCoroutine(LoadDbCoroutine).MoveNext();

            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);

            if (UnionBattleManager.SaveTrigger == null)
            {
                UnionBattleManager.SaveTrigger = TeamServerControl.tm.CreateTrigger(
                    DateTime.Now, Save, 60000);
            }
        }

        private void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v == null)
                return;
            //if (v.tableName == "ServerName")
            //{
            //}
        }

        /// <summary>
        /// 获取活动开启时间
        /// </summary>
        /// <returns></returns>
        public long GetOpenTime()
        {
            var dbData = UnionBattleManager.mDBData;
            if (dbData != null)
            {
                return dbData.OpenTime;
            }

            return -1L;
        }

        /// <summary>
        /// 填充战盟消息数据
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool FillBattleInfo(MsgUnionBattleInfo info, int allianceId)
        {
            var dbData = UnionBattleManager.mDBData;
            if (dbData != null && info != null)
            {
                info.EnrollStartTime = UnionBattleManager.StartEnrollTime.ToBinary();
                info.EnrollEndTime = UnionBattleManager.EndEnrollTime.ToBinary();
                if (allianceId == -1 || !dbData.EnrollList.Contains(allianceId))
                {
                    info.IsEnroll = 0;
                }
                else
                {
                    info.IsEnroll = 1;
                }

                return true;
            }

            return false;
        }

        public void FillMathInfo(MsgUnionBattleMathInfo info, int allianceId)
        {
            var dbData = UnionBattleManager.mDBData;
            if (dbData == null)
                return;

            if (allianceId != -1)
            {
                
            }

            if (dbData.FinalGroup.Count > 0)
            {
                //var group = dbData.FinalGroup[0];
                //foreach (var aId in group.Ids)
                //{
                //    var name = ServerAllianceManager.GetAllianceName(aId);
                //    info.FinalNames[aId] = name;
                //    info.Quarterfinals.Add(aId);
                //}
            }
        }

    public ErrorCodes Enroll(int allianceId)
        {
            if (UnionBattleManager.mDBData.Stage == (int) UnionBattleStage.Wait)
                return ErrorCodes.Error_AllianceNotInTime;

            if (UnionBattleManager.mDBData.Stage != (int)UnionBattleStage.Enroll)
                return ErrorCodes.Error_UnionBattleEnrollOver;

            var enrollList = UnionBattleManager.mDBData.EnrollList;
            if (enrollList.Contains(allianceId))
            {
                return ErrorCodes.Error_AlreadyApply;
            }
            enrollList.Add(allianceId);

            OnEnrollSuccess(allianceId);

            return ErrorCodes.OK;
        }

        /// <summary>
        /// 报名成功
        /// </summary>
        private void OnEnrollSuccess(int allianceId)
        {
            var alliance = ServerAllianceManager.GetAllianceById(allianceId);
            if (alliance == null)
            {
                return;
            }

            var tbMail = Table.GetMail(5001);
            var message = Utils.WrapDictionaryId(50001020);

            foreach (var memId in alliance.mDBData.Members)
            {
                if (tbMail != null)
                {
                    Utility.SendMail(memId, tbMail.Title, tbMail.Text, new Dict_int_int_Data());
                }

                TeamCharacterProxy toCharacterProxy;
                if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(memId, out toCharacterProxy))
                {
                    var chattoCharacterProxy = toCharacterProxy as TeamProxy;
                    if (chattoCharacterProxy != null)
                    {
                        chattoCharacterProxy.SyncAllianceChatMessage((int)eChatChannel.Guild, 0, string.Empty,
                            new ChatMessageContent { Content = message });
                    }
                }
            }
        }

        // 按对应轮次的比赛积分进行排名，积分相同时则比较战力
        private IOrderedEnumerable<KeyValuePair<int, long>> GetSortScore(DBUnionBattleData dbData)
        {
            if (dbData == null)
                return null;

            var fpDict = new Dictionary<int, long>();
            foreach (var allianceId in dbData.EnrollList)
            {
                long w = 0L;
                int score;
                if (dbData.Scores.TryGetValue(allianceId, out score))
                {
                    w += (score * 1000000000L);
                }

                var alliance = ServerAllianceManager.GetAllianceById(allianceId);
                if (alliance != null)
                {
                    w += alliance.GetTotleFightPoint();
                }
                fpDict[allianceId] = w;
            }
            var sortList = fpDict.OrderBy(s => s.Value);
            return sortList;
        }

        // 打乱排序
        private void RandomGroup(List<int> group)
        {
            var ids = group;
            var newIds = new List<int>();
            foreach (var id in ids)
            {
                var randPos = MyRandom.Random(0, newIds.Count);
                newIds.Insert(randPos, id);
            }

            group.Clear();
            group.AddRange(newIds);
        }

        /// <summary>
        /// 分组
        /// </summary>
        private List<List<int>> DiviveIntoGroups()
        {
            var dbData = UnionBattleManager.mDBData;
            if (dbData == null)
            {
                Logger.Error("DiviveIntoGroups no dbData");
                return new List<List<int>>();
            }

            var sortList = GetSortScore(dbData);
            var groupList = new List<List<int>>();
            var i = 0;
            List<int> group = null;
            foreach (var keyValuePair in sortList)
            {
                if (i % 8 == 0)
                {
                    if (group != null)
                    {
                        RandomGroup(group);
                        groupList.Add(group);
                    }

                    group = new List<int>();    
                }
                if (group != null)
                {
                    group.Add(keyValuePair.Key);    
                }
                ++i;
            }
            if (group != null)
            {
                RandomGroup(group);
                groupList.Add(group);
            }

            if (groupList.Count > 1)
            {   
                // 尾部小于4个的时候并入前1组；
                var last = groupList[groupList.Count - 1];
                if (last.Count < 4)
                {
                    groupList[groupList.Count - 2].AddRange(last);
                    groupList.RemoveAt(groupList.Count - 1);
                }
            }

            var lastGroup = groupList[groupList.Count - 1];
            if (lastGroup.Count % 2 == 1)
            { // 最后一组是单数
                var randPos = MyRandom.Random(0, lastGroup.Count);
                lastGroup.Insert(randPos, -1);
            }

            return groupList;
        }

        /// <summary>
        /// 设置分组
        /// </summary>
        /// <param name="g"></param>
        private void SetGroup(List<DBUnionBattleGroupData> g)
        {
            var groupList = DiviveIntoGroups();
            g.Clear();
            foreach (var group in groupList)
            {
                for (var i = 0; i < group.Count / 2; ++i)
                {
                    var data = new DBUnionBattleGroupData();
                    data.Id1 = group[i * 2];
                    data.Id2 = group[i * 2 + 1];
                    g.Add(data);
                }
            }
        }

        /// <summary>
        /// 盟战结束
        /// </summary>
        private void BattleOver()
        {
        }

        /// <summary>
        /// 获取日期date所在的周一（一周第一天是周一）
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime GetMonday(DateTime date)
        {
            var days = date.DayOfWeek - DayOfWeek.Monday;
            if (days < 0)
                days += 7;
            return date.AddDays(-days);
        }

        /// <summary>
        /// 获取第一次开启时间（以最早开启服务器时间3天后的首个周1为起点，并记其为首周）
        /// </summary>
        /// <returns></returns>
        private DateTime GetFirstBattleTime()
        {
            var firstServerTime = DateTime.MaxValue;
            Table.ForeachServerName(record =>
            {
                var oppenTime = DateTime.Parse(record.OpenTime);
                if (oppenTime < firstServerTime)
                {
                    firstServerTime = oppenTime;
                }
                return true;
            });  
  
            // 3天后的首个周1
            var mondayDate = GetMonday(firstServerTime.AddDays(3 - 1 + 7));
            return new DateTime(mondayDate.Year, mondayDate.Month, mondayDate.Day, 0, 0, 0);
        }

        /// <summary>
        /// 初始化盟战数据库
        /// </summary>
        private void InitDataBase()
        {
            var data = new DBUnionBattleData();
            var firstBattleTime = GetFirstBattleTime();
            if (DateTime.Now > firstBattleTime.AddDays(7))
            { // 如果当前周超过开启时间一周
                firstBattleTime = GetMonday(
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                        firstBattleTime.Hour, firstBattleTime.Minute, firstBattleTime.Second));
                if (DateTime.Now > firstBattleTime.AddDays(UnionBattleManager.EnrollDurationDays / 2.0))
                { // 如果报名时间超过了报名时间的一半，则推迟到下周
                    firstBattleTime = firstBattleTime.AddDays(7);
                }
            }
            data.OpenTime = firstBattleTime.ToBinary();
            if (DateTime.Now > firstBattleTime)
                data.Stage = (int)UnionBattleStage.Enroll;
            else
                data.Stage = (int)UnionBattleStage.Wait;
            UnionBattleManager.mDBData = data;

            UnionBattleManager.Dirty = true;
        }

        /// <summary>
        /// 读取服务器数据
        /// </summary>
        /// <param name="dbData"></param>
        private void LoadFromDataBase(DBUnionBattleData dbData)
        {
            UnionBattleManager.mDBData = dbData;

            var battleTime = DateTime.FromBinary(UnionBattleManager.mDBData.OpenTime);
            var dt = new DateTime(battleTime.Year, battleTime.Month, battleTime.Day, 0, 0, 0);
            if (DateTime.Now >= dt.AddDays(7))
            { // 超过1周了
                battleTime = dt.AddDays(7 * UnionBattleManager.WeekInterval);
                if (DateTime.Now > battleTime)
                {
                    battleTime = GetMonday(battleTime.AddDays(7));
                }
                ChangeStage(UnionBattleStage.Enroll);
                ClearDataBase();
            }
            dbData.OpenTime = battleTime.ToBinary();
        }

        /// <summary>
        /// 清楚数据
        /// </summary>
        private void ClearDataBase()
        {
            var db = UnionBattleManager.mDBData;
            if (db != null)
            {
                db.EnrollList.Clear();
            }

            UnionBattleManager.Dirty = true;
        }

        private void Test()
        {
            var firstBattleTime = DateTime.Now;
            var data = UnionBattleManager.mDBData;
            if (DateTime.Now > firstBattleTime)
                data.Stage = (int)UnionBattleStage.Enroll;
            else
                data.Stage = (int)UnionBattleStage.Wait;
            UnionBattleManager.StartEnrollTime = DateTime.Now.AddMinutes(1);

            UnionBattleManager.EndEnrollTime = UnionBattleManager.StartEnrollTime.AddMinutes(1);
            UnionBattleManager.BreakoutGroupTime.Clear();
            UnionBattleManager.BreakoutFightTime.Clear();
            for (var i = 0; i < 4; ++i)
            {
                var dt = UnionBattleManager.EndEnrollTime.AddMinutes(1 + i * 2);
                UnionBattleManager.BreakoutGroupTime.Add(dt);
                UnionBattleManager.BreakoutFightTime.Add(dt.AddMinutes(1));
            }
            UnionBattleManager.QuarterGroupTime = UnionBattleManager.BreakoutGroupTime[UnionBattleManager.BreakoutGroupTime.Count - 1].AddMinutes(10);
            var ldt = UnionBattleManager.QuarterGroupTime;
            ldt = ldt.AddMinutes(1);         
            UnionBattleManager.QuarterfinalTime = ldt;
            UnionBattleManager.SemifinalTime = ldt.AddMinutes(1);
            UnionBattleManager.FinalTime = UnionBattleManager.SemifinalTime.AddMinutes(1);            
        }

        /// <summary>
        /// 根据开始时间计算其它时间
        /// </summary>
        private void CalcAllTime()
        {
            var openDays = DateTime.FromBinary(UnionBattleManager.mDBData.OpenTime);
            UnionBattleManager.StartEnrollTime = new DateTime(openDays.Year, openDays.Month, openDays.Day, 10, 0, 0);
            UnionBattleManager.EndEnrollTime =
                UnionBattleManager.StartEnrollTime.AddDays(UnionBattleManager.EnrollDurationDays);
            for (var i = 0; i < 4; ++i)
            {
                var dt = UnionBattleManager.EndEnrollTime.AddDays(i);
                UnionBattleManager.BreakoutGroupTime.Add(new DateTime(dt.Year, dt.Month, dt.Day, 12, 0, 0));
                UnionBattleManager.BreakoutFightTime.Add(new DateTime(dt.Year, dt.Month, dt.Day, 20, 0, 0));
            }
            UnionBattleManager.QuarterGroupTime = UnionBattleManager.BreakoutGroupTime[UnionBattleManager.BreakoutGroupTime.Count - 1].AddDays(1);
            var ldt = UnionBattleManager.BreakoutFightTime[UnionBattleManager.BreakoutFightTime.Count - 1];
            ldt = ldt.AddDays(1);
            UnionBattleManager.QuarterfinalTime = ldt;
            UnionBattleManager.SemifinalTime = ldt.AddMinutes(30);
            UnionBattleManager.FinalTime = UnionBattleManager.SemifinalTime.AddMinutes(30);            
        }

        private IEnumerator LoadDbCoroutine(Coroutine co)
        {
            // 读数据库,如果存在数据则
            var dbData = TeamServer.Instance.DB.Get<DBUnionBattleData>(co, DataCategory.UninonBattle, DbKey);
            yield return dbData;
            if (dbData.Status != DataStatus.Ok)
            {
                Logger.Error("Load DB {0} failed!!", (int)DataCategory.UninonBattle);
                yield break;
            }

            if (dbData.Data == null)
            { // 未开启过
                InitDataBase();
            }
            else
            {
                LoadFromDataBase(dbData.Data);
            }

            CalcAllTime();

            // 测试用
            //Test();

            CreateTrigger();
        }

        private void Save()
        {
            if (UnionBattleManager.Dirty == false)
                return;

            UnionBattleManager.Dirty = false;

            if (UnionBattleManager.SaveTrigger == null)
            {
                return;
            }
            CoroutineFactory.NewCoroutine(SaveCoroutine).MoveNext();
        }

        public IEnumerator SaveCoroutine(Coroutine co)
        {
            var data = UnionBattleManager.mDBData;
            if (data == null)
                yield break;
            var ret = TeamServer.Instance.DB.Set(co, DataCategory.UninonBattle, DbKey, data);
            yield return ret;
        }

        /// <summary>
        /// 获取突围赛分组时间
        /// </summary>
        /// <returns></returns>
        private DateTime GetBreakoutGroupTime(UnionBattleStage stage)
        {
            if (stage >= UnionBattleStage.Breakout1 && stage <= UnionBattleStage.Breakout4)
            {
                return UnionBattleManager.BreakoutGroupTime[stage - UnionBattleStage.Breakout1];
            }

            return DateTime.MaxValue;
        }

        public void ChangeStage(UnionBattleStage stage)
        {
            UnionBattleManager.mDBData.Stage = (int)stage;

            if (stage == UnionBattleStage.Breakout1)
            {
                SetGroup(UnionBattleManager.mDBData.Group1);
            }
            else if (stage == UnionBattleStage.Breakout2)
            {
                SetGroup(UnionBattleManager.mDBData.Group2);
            }
            else if (stage == UnionBattleStage.Breakout3)
            {
                SetGroup(UnionBattleManager.mDBData.Group3);
            }
            else if (stage == UnionBattleStage.Breakout4)
            {
                SetGroup(UnionBattleManager.mDBData.Group4);
            }
            else if (stage == UnionBattleStage.Quarterfinal)
            {
                var groupList = DiviveIntoGroups();
                foreach (var group in groupList)
                { // 取第一组进决赛
                    for (var i = 0; i < group.Count / 2; ++i)
                    {
                        var data = new DBUnionBattleGroupData();
                        data.Id1 = group[i * 2];
                        data.Id2 = group[i * 2 + 1];
                        UnionBattleManager.mDBData.FinalGroup.Add(data);
                    }
                    break;
                }
            }
            else if (stage == UnionBattleStage.Over)
            {
                BattleOver();
            }
            if (stage != UnionBattleStage.Over)
            {
                CreateTrigger();
            }

            UnionBattleManager.Dirty = true;
        }

        public UnionBattleStage GetNextStage()
        {
            var stage = (UnionBattleStage)UnionBattleManager.mDBData.Stage;
            return stage + 1;
        }

        /// <summary>
        /// 完成某一阶段
        /// </summary>
        /// <param name="stage"></param>
        private void FinishStage(UnionBattleStage stage)
        {
            var dbData = UnionBattleManager.mDBData;


            if (stage == UnionBattleStage.Final)
            {
                GameOver();
            }

            dbData.Stage = (int)GetNextStage();
            CreateTrigger();
            UnionBattleManager.Dirty = true;
        }

        private void GameOver()
        { // 发奖
            Award();

            Reset();
        }

        /// <summary>
        /// 重置
        /// </summary>
        private void Reset()
        {
            UnionBattleManager.StartEnrollTime = GetMonday(UnionBattleManager.StartEnrollTime.AddDays(UnionBattleManager.EnrollDurationDays * 7));
            CalcAllTime();
            CreateTrigger();
        }

        /// <summary>
        /// 发奖
        /// </summary>
        private void Award()
        {
            var dbData = UnionBattleManager.mDBData;
            // 参与奖
            for (var i = 1; i < dbData.Group4.Count; ++i)
            {
                var groupData = dbData.Group4[i];
                //foreach (var allianceId in groupData.Ids)
                //{
                //    var alliance = ServerAllianceManager.GetAllianceById(allianceId);
                //    if (alliance != null)
                //    {
                //        foreach (var member in alliance.mDBData.Members)
                //        {
                //            TeamServer.Instance.LogicAgent.SSSendMailById(member, 999,
                //                (int)SendToCharacterMailType.Normal, string.Empty, string.Empty);
                //        }
                //    }
                //}
            }

            var finalRank = new List<int>();
            finalRank.Add(dbData.FinalVictory);
            foreach (var i in dbData.SemiVictory)
            {
                if (!finalRank.Contains(i))
                    finalRank.Add(i);
            }

            foreach (var i in dbData.QuarterVictory)
            {
                if (!finalRank.Contains(i))
                    finalRank.Add(i);
            }

            //foreach (var i in dbData.FinalGroup[0].Ids)
            //{
            //    if (!finalRank.Contains(i))
            //        finalRank.Add(i);
            //}

            foreach (var allianceId in finalRank)
            {
                var alliance = ServerAllianceManager.GetAllianceById(allianceId);
                if (alliance != null)
                {
                    foreach (var member in alliance.mDBData.Members)
                    {
                        TeamServer.Instance.LogicAgent.SSSendMailById(member, 999,
                            (int)SendToCharacterMailType.Normal, string.Empty, string.Empty);
                    }
                }
            }
            UnionBattleManager.Dirty = true;
        }

        public void CreateTrigger()
        {
            var stage = (UnionBattleStage)UnionBattleManager.mDBData.Stage;

            if (UnionBattleManager.UnionTrigger != null)
            {
                TeamServerControl.tm.DeleteTrigger(UnionBattleManager.UnionTrigger);
                UnionBattleManager.UnionTrigger = null;
            }

            if (stage == UnionBattleStage.Wait)
            {
                UnionBattleManager.UnionTrigger = TeamServerControl.tm.CreateTrigger(
                    UnionBattleManager.StartEnrollTime,
                    () =>
                    {
                        ChangeStage(UnionBattleStage.Enroll);
                    });                
            }
            else if (stage >= UnionBattleStage.Enroll && stage < UnionBattleStage.Breakout4)
            { // 定时触发分组
                var nextStage = GetNextStage();
                var groupTime = GetBreakoutGroupTime(nextStage);
                if (groupTime == DateTime.MaxValue)
                {
                    Logger.Error("UnionBattle GetBreakoutGroupTime Error!");
                    return;
                }
                UnionBattleManager.UnionTrigger = TeamServerControl.tm.CreateTrigger(
                    groupTime, 
                    () =>
                    { // 分组
                        if (UnionBattleManager.mDBData.EnrollList.Count <= 1)
                        { // 如果报名战盟小于一个
                            ChangeStage(UnionBattleStage.Over);
                        }
                        else
                        {
                            ChangeStage(nextStage);
                        }
                    });
            }
            else if (stage == UnionBattleStage.Breakout4)
            {
                UnionBattleManager.UnionTrigger = TeamServerControl.tm.CreateTrigger(
                    UnionBattleManager.QuarterGroupTime,
                    () =>
                    {
                        ChangeStage(UnionBattleStage.Quarterfinal);
                    });
            }
            else if (stage == UnionBattleStage.Quarterfinal)
            {
                UnionBattleManager.UnionTrigger = TeamServerControl.tm.CreateTrigger(
                    UnionBattleManager.SemifinalTime,
                    () =>
                    {
                        ChangeStage(UnionBattleStage.Semifinal);
                    });
            }
            else if (stage == UnionBattleStage.Semifinal)
            {
                UnionBattleManager.UnionTrigger = TeamServerControl.tm.CreateTrigger(
                    UnionBattleManager.FinalTime,
                    () =>
                    {
                        ChangeStage(UnionBattleStage.Final);
                    });
            }
            else if (stage == UnionBattleStage.Over)
            {
                UnionBattleManager.UnionTrigger = TeamServerControl.tm.CreateTrigger(
                    UnionBattleManager.FinalTime,
                    () =>
                    {
                        ChangeStage(UnionBattleStage.Final);
                    });
            }

            UnionBattleManager.Dirty = true;
        }

        /// <summary>
        /// 盟战结果
        /// </summary>
        /// <param name="victoryId"></param>
        /// <param name="failId"></param>
        public void NotifyBattleResult(int victoryId, int failId)
        {
            var dbData = UnionBattleManager.mDBData;
            dbData.Scores.modifyValue(victoryId, 10);
            if (dbData.Stage == (int)UnionBattleStage.Quarterfinal)
            {
                if (!dbData.QuarterVictory.Contains(victoryId))
                    dbData.QuarterVictory.Add(victoryId);

                if (dbData.QuarterVictory.Count == dbData.FinalGroup.Count / 2)
                {
                    FinishStage(UnionBattleStage.Quarterfinal);
                }
            }
            else if (dbData.Stage == (int)UnionBattleStage.Semifinal)
            {
                if (!dbData.SemiVictory.Contains(victoryId))
                    dbData.SemiVictory.Add(victoryId);

                if (dbData.SemiVictory.Count == dbData.FinalGroup.Count / 2)
                {
                    FinishStage(UnionBattleStage.Semifinal);
                }
            }
            else if (dbData.Stage == (int) UnionBattleStage.Final)
            {
                dbData.FinalVictory = victoryId;
                FinishStage(UnionBattleStage.Final);
            }
        }

        public ErrorCodes CanEnter(int allianceId)
        {
            // 检测时间
            var stage = UnionBattleManager.mDBData.Stage;
            if (stage < 0 || stage >= UnionBattleManager.BreakoutFightTime.Count)
                return ErrorCodes.Unknow;

            var time = UnionBattleManager.BreakoutFightTime[stage];
            if (DateTime.Now < time)
                return ErrorCodes.Error_UnionBattleNotTime;

            if (DateTime.Now > time.AddMinutes(5))
                return ErrorCodes.Error_UnionBattleOverTimeEnter;

            // 检测战盟
            if (!UnionBattleManager.mDBData.EnrollList.Contains(allianceId))
                return ErrorCodes.Error_AllianceNotEnroll;
            return ErrorCodes.OK;
        }

        public IEnumerator EnterFight(Coroutine coroutine, ulong characterId, int allianceId)
        {
            // 进入副本  
            var stage = UnionBattleManager.mDBData.Stage;
            if (stage < 0 || stage >= UnionBattleManager.BreakoutFightTime.Count)
                yield break;

            var fighTime = UnionBattleManager.BreakoutFightTime[stage];

            var param = new SceneParam();
            param.Param.Add(fighTime.Hour);
            param.Param.Add(fighTime.Minute);
            var msg = TeamServer.Instance.SceneAgent.AskEnterDungeon(characterId, -1, 2200, 123123, param);
            yield return msg.SendAndWaitUntilDone(coroutine);
        }
    }

    public static class UnionBattleManager
    {
        public static IUnionBattleManager mImpl;
        public static DBUnionBattleData mDBData;
        public static int EnrollDurationDays = 2;   // 报名持续时间
        public static int WeekInterval = 2;   // 每几周开启一次
        public static DateTime StartEnrollTime; // 报名开始时间
        public static DateTime EndEnrollTime;   // 报名结束时间
        public static List<DateTime> BreakoutGroupTime = new List<DateTime>();  // 突围赛分组时间
        public static List<DateTime> BreakoutFightTime = new List<DateTime>();  // 突围赛时间
        public static DateTime QuarterGroupTime;    // 1/4 分组时间
        public static DateTime QuarterfinalTime;    // 1/4决赛
        public static DateTime SemifinalTime;  // 半决赛
        public static DateTime FinalTime;   // 决赛
        public static Trigger UnionTrigger; // 定时器
        public static Trigger SaveTrigger;
        public static bool Dirty;

        static UnionBattleManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof(UnionBattleManager),
                typeof(UnionBattleManagerDefaultImpl),
                o => { mImpl = (IUnionBattleManager)o; });
        }

        public static void Init()
        {
            mImpl.Init();
        }

        public static long GetOpenTime()
        {
            return mImpl.GetOpenTime();
        }

        public static bool FillBattleInfo(MsgUnionBattleInfo info, int allianceId)
        {
            return mImpl.FillBattleInfo(info, allianceId);
        }

        /// <summary>
        /// 报名
        /// </summary>
        /// <param name="allianceId"></param>
        /// <returns></returns>
        public static ErrorCodes Enroll(int allianceId)
        {
            return mImpl.Enroll(allianceId);
        }

        /// <summary>
        /// 战斗结果
        /// </summary>
        /// <param name="victoryId"></param>
        /// <param name="failId"></param>
        public static void NotifyBattleResult(int victoryId, int failId)
        {
            mImpl.NotifyBattleResult(victoryId, failId);    
        }

        /// <summary>
        /// 是否可以进入副本
        /// </summary>
        /// <param name="allianceId"></param>
        /// <returns></returns>
        public static ErrorCodes CanEnter(int allianceId)
        {
            return mImpl.CanEnter(allianceId);
        }

        /// <summary>
        /// 请求进入战斗
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="allianceId"></param>
        public static IEnumerator EnterFight(Coroutine coroutine, ulong characterId, int allianceId)
        {
            return mImpl.EnterFight(coroutine, characterId, allianceId);    
        }

        public static IEnumerator SaveCoroutine(Coroutine coroutine)
        {
            return mImpl.SaveCoroutine(coroutine);
        }

        /// <summary>
        /// 填充比赛信息
        /// </summary>
        public static void FillMathInfo(MsgUnionBattleMathInfo info, int allianceId)
        {
            mImpl.FillMathInfo(info, allianceId);    
        }
    }
}
