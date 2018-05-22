#region using

using System.Collections.Generic;
using DataTable;
using EventSystem;
using Shared;

#endregion

namespace Logic
{
    //成就类型
    public enum eAchievementType
    {
        FlagList = 0, //标记位型
        Number = 1 //计数型
    }

    public interface IAchievementManager
    {
        void AchievementExDataChangeEvent(CharacterController character, int nId, int nValue);
        void AchievementTrueflagEvent(CharacterController character, int nId);
        bool CheckAchievement(CharacterController character, int achievementId);
        void ExDataChangeEvent(IEvent ievent);
        void FinishAchievement(CharacterController character, int achievementId);
        void FinishAchievementFlag(CharacterController character, int FlagId);
        void FlagTrueEvent(IEvent ievent);
        void Init();
        ErrorCodes RewardAchievement(CharacterController character, int achievementId);
    }

    public class AchievementManagerDefaultImpl : IAchievementManager
    {
        //构造所有成就被什么所影响的列表(当某个条件被触发时，可以快速知道哪个成就有更新了)
        private void InitOneAchievement(AchievementRecord tbAchievement)
        {
            List<int> tempList;
            var nExdataId = tbAchievement.Exdata;
            if (nExdataId != -1)
            {
//计数型
                if (AchievementManager.ExdataAchievement.TryGetValue(nExdataId, out tempList))
                {
                    tempList.Add(tbAchievement.Id);
                }
                else
                {
                    tempList = new List<int> {tbAchievement.Id};
                    AchievementManager.ExdataAchievement[nExdataId] = tempList;
                }
            }
            else
            {
                foreach (var i in tbAchievement.FlagList)
                {
                    if (i < 0)
                    {
                        continue;
                    }
                    if (AchievementManager.FlagAchievement.TryGetValue(i, out tempList))
                    {
                        tempList.Add(tbAchievement.Id);
                    }
                    else
                    {
                        tempList = new List<int> {tbAchievement.Id};
                        AchievementManager.FlagAchievement[i] = tempList;
                    }
                }
            }
        }

        //初始化静态数据
        public void Init()
        {
            //初始化成就相关的静态触发数据
            Table.ForeachAchievement(record =>
            {
                InitOneAchievement(record);
                return true;
            });
            //注册事件
            EventDispatcher.Instance.AddEventListener(ChacacterFlagTrue.EVENT_TYPE, AchievementManager.FlagTrueEvent);
            EventDispatcher.Instance.AddEventListener(CharacterExdataChange.EVENT_TYPE,
                AchievementManager.ExDataChangeEvent);
        }

        public void FlagTrueEvent(IEvent ievent)
        {
            var ee = ievent as ChacacterFlagTrue;

            AchievementTrueflagEvent(ee.character, ee.FlagId);
        }

        public void ExDataChangeEvent(IEvent ievent)
        {
            var ee = ievent as CharacterExdataChange;
            AchievementExDataChangeEvent(ee.character, ee.ExdataId, ee.ExdataValue);
        }

        #region  领取

        //领取奖励
        public ErrorCodes RewardAchievement(CharacterController character, int achievementId)
        {
            var tbachi = Table.GetAchievement(achievementId);
            if (tbachi == null)
            {
                return ErrorCodes.Error_AchievementID;
            }
            if (!character.GetFlag(tbachi.FinishFlagId))
            {
                return ErrorCodes.Error_AchievementNotFinished;
            }
            if (character.GetFlag(tbachi.RewardFlagId))
            {
                return ErrorCodes.Error_RewardAlready;
            }
            if (tbachi.ItemId[2] > 0)
            {
                var result = character.mBag.CheckAddItem(tbachi.ItemId[2], tbachi.ItemCount[2]);
                if (result != ErrorCodes.OK)
                {
                    return result;
                }
                character.mBag.AddItem(tbachi.ItemId[2], tbachi.ItemCount[2], eCreateItemType.Achievement);
            }
            for (var i = 0; i < 2; i++)
            {
                character.mBag.AddItem(tbachi.ItemId[i], tbachi.ItemCount[i], eCreateItemType.Achievement);
            }
            character.SetFlag(tbachi.RewardFlagId);
            return ErrorCodes.OK;
        }

        #endregion

        #region  判断

        //检查是否完成了
        public bool CheckAchievement(CharacterController character, int achievementId)
        {
            var tbachi = Table.GetAchievement(achievementId);
            if (tbachi == null)
            {
                return false;
            }
            //是否早已经完成
            if (character.GetFlag(tbachi.FinishFlagId))
            {
                return true;
            }
            //if (character.GetFlag(tbachi.RewardFlagId)) return true;
            //等级检查
            //if (character.GetLevel() < tbachi.ViewLevel)
            //{
            //    return false;
            //}
            //前置标记检查
            //if (!character.GetFlag(tbachi.BeforeFalgId))
            //{
            //    return false;
            //}
            //数据检查
            if (tbachi.Exdata != -1)
            {
                if (character.GetExData(tbachi.Exdata) >= tbachi.ExdataCount)
                {
                    return true;
                }
                return false;
            }
            //标记位检查
            foreach (var i in tbachi.FlagList)
            {
                if (i < 0)
                {
                    continue;
                }
                if (!character.GetFlag(i))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region  达成

        //成就达成
        public void FinishAchievement(CharacterController character, int achievementId)
        {
            //todo
            Dictionary<int, int> cAchie;
            if (AchievementManager.Triggers.TryGetValue(character.mGuid, out cAchie))
            {
                if (cAchie.ContainsKey(achievementId))
                {
                    return;
                }
                cAchie[achievementId] = 0;
            }
            else
            {
                cAchie = new Dictionary<int, int>();
                AchievementManager.Triggers[character.mGuid] = cAchie;
                cAchie[achievementId] = 0;
            }
            var tbachi = Table.GetAchievement(achievementId);
            if (tbachi == null)
            {
                return;
            }
            if (!character.GetFlag(tbachi.FinishFlagId))
            {
                character.SetFlag(tbachi.FinishFlagId); //设置完成标记
                character.mBag.AddRes(eResourcesType.AchievementScore, tbachi.AchievementPoint,
                    eCreateItemType.Achievement);
                int maxCount = character.GetExData((int)eExdataDefine.e686);
                if (tbachi.AchievementPoint > 0)
                {
                    character.SetExData((int)eExdataDefine.e686, maxCount + tbachi.AchievementPoint);
                }
                
                //character.AddExData((int) eExdataDefine.e50, tbachi.AchievementPoint); //给予成就点数
                if (character.Proxy != null)
                {
                    character.Proxy.FinishAchievement(achievementId);
                }
            }
        }


        //成就的一个单元达成
        public void FinishAchievementFlag(CharacterController character, int FlagId)
        {
            character.SetFlag(FlagId);
        }

        #endregion

        #region 事件相关

        //响应事件（标记位)
        public void AchievementTrueflagEvent(CharacterController character, int nId)
        {
            List<int> tempList;
            if (AchievementManager.FlagAchievement.TryGetValue(nId, out tempList))
            {
                foreach (var i in tempList)
                {
                    var isFinished = CheckAchievement(character, i);
                    if (isFinished)
                    {
                        FinishAchievement(character, i);
                    }
                }
            }
        }

        //响应事件(扩展数据)
        public void AchievementExDataChangeEvent(CharacterController character, int nId, int nValue)
        {
            List<int> tempList;
            if (AchievementManager.ExdataAchievement.TryGetValue(nId, out tempList))
            {
                foreach (var i in tempList)
                {
                    var isFinished = CheckAchievement(character, i);
                    if (isFinished)
                    {
                        FinishAchievement(character, i);
                    }
                }
            }
        }

        ////响应事件（资源变化）
        //public static void AchievementItemChangeEvent(CharacterController character, int nId, int nValue)
        //{

        //}

        #endregion
    }

    //成就系统
    public static class AchievementManager
    {
        public static Dictionary<int, List<int>> ExdataAchievement = new Dictionary<int, List<int>>();
            //Key=扩展数据ID     Value=影响的成就列表 

        public static Dictionary<int, List<int>> FlagAchievement = new Dictionary<int, List<int>>();
            //Key=标记位ID      Value=影响的成就列表

        private static IAchievementManager mImpl;
        public static Dictionary<ulong, Dictionary<int, int>> Triggers = new Dictionary<ulong, Dictionary<int, int>>();

        static AchievementManager()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (AchievementManager),
                typeof (AchievementManagerDefaultImpl),
                o => { mImpl = (IAchievementManager) o; });
        }

        #region  判断

        //检查是否完成了
        public static bool CheckAchievement(this CharacterController character, int achievementId)
        {
            return mImpl.CheckAchievement(character, achievementId);
        }

        #endregion

        //初始化静态数据
        public static void Init()
        {
            mImpl.Init();
        }

        #region  领取

        //领取奖励
        public static ErrorCodes RewardAchievement(this CharacterController character, int achievementId)
        {
            return mImpl.RewardAchievement(character, achievementId);
        }

        #endregion

        #region  达成

        //成就达成
        public static void FinishAchievement(CharacterController character, int achievementId)
        {
            mImpl.FinishAchievement(character, achievementId);
        }

        //成就的一个单元达成
        public static void FinishAchievementFlag(CharacterController character, int FlagId)
        {
            mImpl.FinishAchievementFlag(character, FlagId);
        }

        #endregion

        #region 事件相关

        //响应事件（标记位)
        public static void AchievementTrueflagEvent(CharacterController character, int nId)
        {
            mImpl.AchievementTrueflagEvent(character, nId);
        }

        //响应事件(扩展数据)
        public static void AchievementExDataChangeEvent(CharacterController character, int nId, int nValue)
        {
            mImpl.AchievementExDataChangeEvent(character, nId, nValue);
        }

        ////响应事件（资源变化）
        //public static void AchievementItemChangeEvent(CharacterController character, int nId, int nValue)
        //{

        //}
        public static void FlagTrueEvent(IEvent ievent)
        {
            mImpl.FlagTrueEvent(ievent);
        }

        public static void ExDataChangeEvent(IEvent ievent)
        {
            mImpl.ExDataChangeEvent(ievent);
        }

        #endregion
    }
}