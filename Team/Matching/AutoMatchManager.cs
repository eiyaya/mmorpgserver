using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;
using TeamServerService;

namespace Team
{
    public class TeamTarget
    {
        public int teamTargetID { set; get; }
        public TeamTargetType type { set; get; }
    }
    public enum TeamTargetType
    {
        NOTTARGET = 0,      // 无目标
        COPYTEAM = 1,       // 副本类型
        ACTIVITYTEAM = 2,   // 活动类型
    }
    public class TeamMatchItem
    {
        public int characterID { set; get; }
        public Character chracter { set; get; }
        public TeamTarget teamTarget { set; get; }
    }
    public interface IAutoMatchManager
    {
        QueueCharacter GetMatchingCharacter(ulong guid);
        void Init();
        void OnLine(ulong characterId);
        void OnLost(ulong characterId);
        void PushLog();
        void beginAotuMatch(bool isHaveTeam, ulong characterId, TeamMatchItem item);
        void changeTeamTarget(ulong characterId, int type, int targetId, int levelMini, int levelMax, int readTableId);
    }

    public class AutoMatchManagerDefaultImpl : IAutoMatchManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //把一个新的characterId 加入到 character 里
        
        //初始化
        public void Init()
        {
        }
        

        //获得某个玩家
        public QueueCharacter GetMatchingCharacter(ulong guid)
        {
            //QueueCharacter character;
            //if (QueueManager.Characters.TryGetValue(guid, out character))
            //{
            //    return character;
            //}
            return null;
        }
        

        //上线通知
        public void OnLine(ulong characterId)
        {
            TeamCharacterProxy proxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(characterId, out proxy))
            {
                proxy.AutoMatchStateChange(0);
                var team = TeamManager.GetCharacterTeam(characterId);
                if(null != team)
                    proxy.NotifyChangetTeamTarget((int)team.team.type, team.team.teamTargetID, team.team.levelMini, team.team.levelMax, 0);
                else
                    proxy.NotifyChangetTeamTarget(0, 0, 0, 0, 0);
            }
        }

        //下线通知
        public void OnLost(ulong characterId)
        {
            if (null != AutoMatchManager.teamMatchDic)
            {
                if (AutoMatchManager.teamMatchDic.ContainsKey(characterId))
                    AutoMatchManager.teamMatchDic.Remove(characterId);
            }

            if (null != AutoMatchManager.nullTeamMatchDic)
            {
                if (AutoMatchManager.nullTeamMatchDic.ContainsKey(characterId))
                    AutoMatchManager.nullTeamMatchDic.Remove(characterId);
            }
        }

        //输出日志看看状态
        public void PushLog()
        {
           
        }

        public void beginAotuMatch(bool isHaveTeam, ulong characterId, TeamMatchItem item)
        {
            Logger.Info("TeamWorkRefrerrence  beginAotuMatch= " + characterId);

            if (isHaveTeam) // 有队伍开启自动匹配
            {
                List<ulong> oldList = new List<ulong>();
                foreach (var sNull in AutoMatchManager.nullTeamMatchDic)
                {
                    if (sNull.Key != characterId && sNull.Value.teamTarget.type == item.teamTarget.type && sNull.Value.teamTarget.teamTargetID == item.teamTarget.teamTargetID)
                    {
                        //增加到队伍
                        var theTeam = TeamManager.GetCharacterTeam(characterId);

                        oldList.Add(sNull.Key);

                        if (theTeam.team.TeamList.Count == 4)
                        {
                            TeamManager.addOneToTeam(theTeam.team, characterId, TeamState.Member);
                            //TeamChange(TeamChangedType.Request, theTeam, charaID);
                            
                            TeamCharacterProxy proxy;
                            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(characterId, out proxy))
                            {
                                proxy.AutoMatchStateChange(0);
                            }
                            AutoMatchManager.teamMatchDic.Remove(characterId);
                            break;
                        }

                        TeamManager.addOneToTeam(theTeam.team, sNull.Key, TeamState.Member);
                    }
                }

                for (int r = 0; r < oldList.Count; r++)
                {
                    var sItem = oldList[r];
                    if (AutoMatchManager.nullTeamMatchDic.ContainsKey(sItem)) AutoMatchManager.nullTeamMatchDic.Remove(sItem);

                    TeamCharacterProxy proxy;
                    if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(sItem, out proxy))
                    {
                        proxy.AutoMatchStateChange(0);
                    }
                }
                oldList.Clear();
            }
            else
            {  // 无队伍开启自动匹配
                bool isGetTeam = false;
                {   // 过滤 处于 自动匹配状态的队伍
                    foreach (var sTeam in AutoMatchManager.teamMatchDic)
                    {
                        if (sTeam.Key != characterId)
                        {
                            var theTeam = sTeam.Value.chracter.team;

                            if (null == theTeam || theTeam.TeamList == null || theTeam.TeamList.Count > 4)
                                continue;
                            if (theTeam.TeamList.Contains(characterId)) continue;
                            if (sTeam.Value.teamTarget.teamTargetID != item.teamTarget.teamTargetID || sTeam.Value.teamTarget.type != item.teamTarget.type)
                                continue;

                            //AutoMatchManager.teamMatchDic.Remove(sTeam.Key);
                            TeamCharacterProxy proxy;
                            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(characterId, out proxy))
                            {
                                proxy.AutoMatchStateChange(0);
                            }

                            if (AutoMatchManager.teamMatchDic.ContainsKey(sTeam.Key) && theTeam.TeamList.Count == 4)
                            {
                                TeamCharacterProxy proxy1;
                                if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(sTeam.Key, out proxy1))
                                {
                                    proxy1.AutoMatchStateChange(0);
                                }
                                AutoMatchManager.teamMatchDic.Remove(sTeam.Key);
                            }

                            //增加到队伍
                            TeamManager.addOneToTeam(theTeam, characterId, TeamState.Member);
                            //TeamChange(TeamChangedType.AcceptRequest, theTeam, charaID);
                            isGetTeam = true;
                            break;
                        }
                        else
                            continue;
                    }

                    // 过滤 没使用自动匹配但是勾选了“自动接收入队申请”的队伍
                    if (!isGetTeam)
                    {
                        if (TeamManager.mTeams.Count > 0)
                        {
                            foreach (var sFlag in AutoMatchManager.teamFlagDic)
                            {
                                if (sFlag.Key != characterId && sFlag.Value == true)
                                {
                                    if (TeamManager.mTeams.ContainsKey (sFlag.Key))
                                    {
                                        var theTeam = TeamManager.mTeams[sFlag.Key];
                                        if (theTeam.teamTargetID != item.teamTarget.teamTargetID || theTeam.type != item.teamTarget.type)
                                            continue;
                                        //增加到队伍
                                        TeamManager.addOneToTeam(theTeam, characterId, TeamState.Member);

                                        isGetTeam = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // 当无队伍可加入，且服务器收到的消息均为无队伍自动匹配申请时
                    if (!isGetTeam)
                    {
                        foreach (var sNullItem in AutoMatchManager.nullTeamMatchDic)
                        {
                            if (sNullItem.Key != characterId)
                            {
                                var theTeam = TeamManager.CreateTeam(sNullItem.Key);// 默认队长(即匹配对象时间靠前的为队长)

                                //增加到队伍
                                TeamManager.addOneToTeam(theTeam, characterId, TeamState.Member);
                                //TeamChange(TeamChangedType.AcceptRequest, sTeam, charaID);

                                if (AutoMatchManager.nullTeamMatchDic.ContainsKey(characterId)) AutoMatchManager.nullTeamMatchDic.Remove(characterId);
                                AutoMatchManager.nullTeamMatchDic.Remove(sNullItem.Key);
                                
                                TeamCharacterProxy proxy;
                                if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(characterId, out proxy))
                                {
                                    proxy.AutoMatchStateChange(0);
                                }

                                TeamCharacterProxy proxy1;
                                if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(sNullItem.Key, out proxy1))
                                {
                                    proxy1.AutoMatchStateChange(0);
                                }
                                isGetTeam = true;
                                break;
                            }
                        }

                    }
                }
            }
        }

        public void changeTeamTarget(ulong characterId, int type, int targetId, int levelMini, int levelMax, int readTableId)
        {
            var team = TeamManager.GetCharacterTeam(characterId);
            if (null != team)
            {
                TeamManager.mCharacters[characterId].team.type = (TeamTargetType)type;
                TeamManager.mCharacters[characterId].team.teamTargetID = targetId;
                TeamManager.mCharacters[characterId].team.levelMini = levelMini;
                TeamManager.mCharacters[characterId].team.levelMax = levelMax;
                
                TeamCharacterProxy proxy;
                for (int i = 0; i < TeamManager.mCharacters[characterId].team.TeamList.Count; i++)
                {
                    var temCharacId = TeamManager.mCharacters[characterId].team.TeamList[i];
                    if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(temCharacId, out proxy))
                    {
                        if (null != proxy)
                            proxy.NotifyChangetTeamTarget(type, targetId, levelMini, levelMax, readTableId);
                    }
                }
                
            }
        }

    }

    public static class AutoMatchManager
    {
        //public static Dictionary<ulong, QueueCharacter> Characters = new Dictionary<ulong, QueueCharacter>(); //排队玩家
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //public static Dictionary<int, QueueLogic> Matchings = new Dictionary<int, QueueLogic>(); //排队列表
        private static IAutoMatchManager mStaticImpl;

        public static Dictionary<ulong, bool> teamFlagDic = new Dictionary<ulong, bool>();
        //有队伍 自动匹配的列表
        public static Dictionary<ulong, TeamMatchItem> teamMatchDic = new Dictionary<ulong, TeamMatchItem>();
        public static Dictionary<ulong, TeamMatchItem> nullTeamMatchDic = new Dictionary<ulong, TeamMatchItem>();
        // 对等的 队伍列表
        //public static Dictionary<ulong, TeamMatchItem> teamDic = new Dictionary<ulong, TeamMatchItem>();

        static AutoMatchManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof(AutoMatchManager), typeof(AutoMatchManagerDefaultImpl),
                o => { mStaticImpl = (IAutoMatchManager)o; });
        }

        //清空某个排队ID下的所有正在排队的玩家
        public static void ClearQueue(int id)
        {
        }
        
        //获得某个玩家
        public static QueueCharacter GetMatchingCharacter(ulong guid)
        {
            return mStaticImpl.GetMatchingCharacter(guid);
        }

        //初始化
        public static void Init()
        {
            mStaticImpl.Init();
        }

        //上线通知
        public static void OnLine(ulong characterId)
        {
            mStaticImpl.OnLine(characterId);
        }

        //下线通知
        public static void OnLost(ulong characterId)
        {
            mStaticImpl.OnLost(characterId);
        }

        //输出日志看看状态
        public static void PushLog()
        {
            mStaticImpl.PushLog();
        }

        //自動匹配 開啓匹配
        public static void beginAotuMatch(bool isHaveTeam, ulong characterId, TeamMatchItem item)
        {
            mStaticImpl.beginAotuMatch(isHaveTeam, characterId, item);
        }

        public static void changeTeamTarget(ulong characterId, int type, int targetId, int levelMini, int levelMax, int readTableId)
        {
            mStaticImpl.changeTeamTarget(characterId, type, targetId, levelMini, levelMax, readTableId);
        }
    }
}
