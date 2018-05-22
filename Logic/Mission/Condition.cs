#region using

using System;
using System.Collections.Generic;
using DataTable;
using NLog;

#endregion

namespace Logic
{
    public interface IConditionManager
    {
        Dictionary<int, int> EventTriggerCondition(eEventType type, int nParam);
        void Init();
        void Subscribe(eEventType e, Action act);
    }

    public class ConditionManagerDefaultImpl : IConditionManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region 初始化

        //初始化优化结构
        public void Init()
        {
            //初始化条件相关的静态触发数据
            Table.ForeachConditionTable(record =>
            {
                foreach (var i in record.TrueFlag)
                {
                    if (i >= 0)
                    {
                        InitOneType(eEventType.Trueflag, record.Id, i);
                    }
                }
                foreach (var i in record.FalseFlag)
                {
                    if (i >= 0)
                    {
                        InitOneType(eEventType.Falseflag, record.Id, i);
                    }
                }
                foreach (var i in record.ExdataId)
                {
                    if (i >= 0)
                    {
                        InitOneType(eEventType.ExDataChange, record.Id, i);
                    }
                }
                foreach (var i in record.ItemId)
                {
                    if (i >= 0)
                    {
                        InitOneType(eEventType.ItemChange, record.Id, i);
                    }
                }
                return true;
            });
        }


        //初始化一个条件(当事件来的时候，知道哪个条件会被有修改)
        private void InitOneType(eEventType type, int nConditionId, int nParam)
        {
            //是否只有任务需要整理条件，如果是的话，发现条件能触发任务才继续整理
            var IsOnlyMission = false;
            if (IsOnlyMission)
            {
                if (!MissionManager.CheckConditionEventMission(nConditionId))
                {
                    return;
                }
            }
            //检查类型是否存在,不存在则造一个
            Dictionary<int, List<int>> paramCondi;
            if (!ConditionManager.EventTypeList.TryGetValue(type, out paramCondi))
            {
                paramCondi = new Dictionary<int, List<int>>();
                ConditionManager.EventTypeList[type] = paramCondi;
            }
            //检查相应参数的Key是否存在,不存在则造一个
            List<int> condiList;
            if (!paramCondi.TryGetValue(nParam, out condiList))
            {
                condiList = new List<int>();
                paramCondi[nParam] = condiList;
            }
            //添加数据
            condiList.Add(nConditionId);
        }

        #endregion

        #region 事件相关

        public void Subscribe(eEventType e, Action act)
        {
        }

        //事件触发
        private void PushEvent(CharacterController character, eEventType type, int param0 = 0, int param1 = 0)
        {
            Logger.Trace("type={0},param0={1},param1={2}", type, param0, param1);
            if (character == null)
            {
                return;
            }
            MissionManager.TriggerMissionByEvent(character, type, param0, param1);
            switch (type)
            {
                case eEventType.Trueflag:
                {
                    AchievementManager.AchievementTrueflagEvent(character, param0);
                }
                    break;
                case eEventType.Falseflag:
                {
                }
                    break;
                case eEventType.ExDataChange:
                {
                    AchievementManager.AchievementExDataChangeEvent(character, param0, param0);
                    //目前没有任务监测扩展计数的  MissionManager.Event(character, type, param0, param1);
                }
                    break;
                case eEventType.ItemChange:
                {
                    MissionManager.EventByItemChange(character, param0, param1);
                }
                    break;
                case eEventType.KillMonster:
                {
                    MissionManager.EventByKillMonster(character, param0, param1);
                }
                    break;
                case eEventType.EnterArea:
                {
                    if (param1 == 1)
                    {
                        MissionManager.EventByEnterArea(character, param0, true);
                    }
                    else
                    {
                        MissionManager.EventByEnterArea(character, param0, false);
                    }
                }
                    break;
                case eEventType.Tollgate:
                {
                    MissionManager.EventByTollgate(character, param0, param1);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }


        //根据事件找到所有影响的条件
        public Dictionary<int, int> EventTriggerCondition(eEventType type, int nParam)
        {
            //检查类型是否存在
            Dictionary<int, List<int>> ParamCondi;
            if (!ConditionManager.EventTypeList.TryGetValue(type, out ParamCondi))
            {
                return null;
            }
            List<int> CondiList;
            //检查相应参数的Key是否存在
            if (!ParamCondi.TryGetValue(nParam, out CondiList))
            {
                return null;
            }
            //整理任务
            var result = new Dictionary<int, int>();
            foreach (var i in CondiList)
            {
                result[i] = 1;
            }
            return result;
        }

        #endregion
    }

    public static class ConditionManager
    {
        //public static Dictionary<eEventType, List<int>> EventTypeList = new Dictionary<eEventType, List<int>>(); //所有响应事件的条件ID
        public static Dictionary<eEventType, Dictionary<int, List<int>>> EventTypeList =
            new Dictionary<eEventType, Dictionary<int, List<int>>>(); //所有响应事件的条件ID,事件类型，事件参数，条件ID

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //
        private static IConditionManager mImpl;

        static ConditionManager()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (ConditionManager),
                typeof (ConditionManagerDefaultImpl),
                o => { mImpl = (IConditionManager) o; });
        }

        #region 初始化

        //初始化优化结构
        public static void Init()
        {
            mImpl.Init();
        }

        #endregion

        #region 事件相关

        public static Dictionary<eEventType, List<Action>> mDictionary = new Dictionary<eEventType, List<Action>>();

        public static void Subscribe(eEventType e, Action act)
        {
            mImpl.Subscribe(e, act);
        }

        //根据事件找到所有影响的条件
        public static Dictionary<int, int> EventTriggerCondition(eEventType type, int nParam)
        {
            return mImpl.EventTriggerCondition(type, nParam);
        }

        #endregion
    }
}