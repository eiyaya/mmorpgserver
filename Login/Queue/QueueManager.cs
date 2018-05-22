#region using

using System;
using System.Collections.Generic;
using DataTable;
using EventSystem;
using NLog;
using Shared;

#endregion

namespace Login
{
    public interface IQueueManager
    {
        int EnterGamePlayerCount { get; }
        int LandingPlayerCount { get; }
        bool CheckPlayerEnterGame(ulong playerId);
        bool CheckPlayerLanding(ulong playerId);
        void ClinetIdLost(ulong clientId);
        void EnterPlayer(PlayerConnect pc);
        PlayerConnect GetEnterGameConnect(ulong playerId);
        PlayerConnect GetInGameConnect(ulong playerId);
        PlayerConnect GetLandingConnect(ulong playerId);
        PlayerConnect GetPlayerConnect(ulong clientId);
        QueueBase GetQueue(int serverId);
        PlayerConnect IsCache(string type, ulong clientId, string name);
        void LeaverPlayer(PlayerConnect pc);
        void PlayerEnterGameStart(ulong playerId);
        void PlayerEnterGameSuccess(ulong playerId);
        void PushLoginState(PlayerConnect connect);
        void ReloadTable(IEvent ievent);
        void Reset();
        void Update();
    }

    public class QueueManagerDefaultImpl : IQueueManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private void UpdateCheck()
        {
            var delList = new List<ulong>();
            foreach (var connet in QueueManager.EnterGamePlayerList)
            {
                if (connet.Value.StateTime.AddSeconds(90) < DateTime.Now)
                {
                    delList.Add(connet.Key);
                    Logger.Warn("UpdateCheck EnterGamePlayerList  now={0},start={1},key={2} ", DateTime.Now,
                        connet.Value.StateTime, connet.Value.GetKey()); //太长时间没进去了
                }
            }
            foreach (var connet in delList)
            {
                QueueManager.EnterGamePlayerList.Remove(connet);
                //ClinetIdLost(connet.ClientId);
            }
        }

        public void EnterPlayer(PlayerConnect pc)
        {
            var serverId = pc.Player.DbData.LastServerId;
            QueueManager.PlayerCount.modifyValue(serverId, 1);
            ++QueueManager.GamePlayerCount;
            //int nowValue;
            //if (QueueManager.PlayerCount.TryGetValue(serverId, out nowValue))
            //{
            //    QueueManager.PlayerCount[serverId] = nowValue + 1;
            //}
        }

        public void LeaverPlayer(PlayerConnect pc)
        {
            var serverId = pc.Player.DbData.LastServerId;
            var nowValue = QueueManager.PlayerCount.modifyValue(serverId, -1);
            if (nowValue < 0)
            {
                PlayerLog.WriteLog((ulong) LogType.LoginPlayerQueueCountError, "s={0},c={1}", serverId, nowValue);
                QueueManager.PlayerCount.modifyValue(serverId, 1);
                return;
            }
            --QueueManager.GamePlayerCount;
            //if (QueueManager.PlayerCount.TryGetValue(serverId, out nowValue))
            //{
            //    if (nowValue < 1)
            //    {
            //        PlayerLog.WriteLog((ulong)LogType.LoginPlayerQueueCountError, "s={0},c={1}", serverId, nowValue);
            //        nowValue = 1;
            //    }
            //    QueueManager.PlayerCount[serverId] = nowValue - 1;
            //    --QueueManager.GamePlayerCount;
            //}
        }

        public void Reset()
        {
            Table.ForeachServerName(record =>
            {
                if (record.LogicID == -1)
                {
                    return true;
                }
                if (record.IsClientDisplay != 1 && record.IsClientDisplay != 2)
                {
                    return true;
                }
                var serverId = record.Id;
                QueueBase queue;
                if (!QueueManager.ServerQueue.TryGetValue(serverId, out queue))
                {
                    QueueManager.ServerQueue.Add(serverId, new ServerQueue(serverId));
                    if (!QueueManager.PlayerCount.ContainsKey(serverId))
                    {
                        QueueManager.PlayerCount.Add(serverId, 0);
                    }
                    else
                    {
                        Logger.Error("QueueManagerDefaultImpl::Reset exist same serverid={0} !!!!", serverId);
                    }
                }
                return true;
            });
        }

        public void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v.tableName == "SeverName")
            {
                Reset();
            }
        }

        public QueueBase GetQueue(int serverId)
        {
            if (serverId == -1)
            {
                return QueueManager.MainQueue;
            }
            QueueBase queue;
            if (QueueManager.ServerQueue.TryGetValue(serverId, out queue))
            {
                return queue;
            }
            Logger.Error("In GetQueue(). Can not find queue for server {0}!!", serverId);
            return null;
        }

        public PlayerConnect IsCache(string type, ulong clientId, string name)
        {
            var key = PlayerConnect.GetLandingKey(type, name);
            PlayerConnect findConnect;
            if (QueueManager.CacheLost.TryGetValue(key, out findConnect))
            {
                return findConnect;
            }
            return null;
        }

        public void PushLoginState(PlayerConnect connect)
        {
            var playerId = connect.Player.DbData.Id;
            if (connect.State != ConnectState.InGame)
            {
                Logger.Warn("PushLoginState state = {0}", connect.State);
                connect.OnLost();
            }
            else
            {
                PlayerConnect oldPlayer;
                if (QueueManager.InGamePlayerList.TryGetValue(playerId, out oldPlayer))
                {
                    if (oldPlayer != null)
                    {
                        LeaverPlayer(oldPlayer);
                    }
                    QueueManager.InGamePlayerList.Remove(playerId);
                }
            }
            connect.State = ConnectState.Landing;
            QueueManager.LandingPlayerList.TryAdd(playerId, connect);
            QueueManager.TotalList.TryAdd(connect.ClientId, connect);
        }

        public PlayerConnect GetPlayerConnect(ulong clientId)
        {
            PlayerConnect temp;
            if (QueueManager.TotalList.TryGetValue(clientId, out temp))
            {
                return temp;
            }
            return null;
        }

        public PlayerConnect GetLandingConnect(ulong playerId)
        {
            PlayerConnect temp;
            if (QueueManager.LandingPlayerList.TryGetValue(playerId, out temp))
            {
                return temp;
            }
            return null;
        }

        public PlayerConnect GetEnterGameConnect(ulong playerId)
        {
            PlayerConnect temp;
            if (QueueManager.EnterGamePlayerList.TryGetValue(playerId, out temp))
            {
                return temp;
            }
            return null;
        }

        public PlayerConnect GetInGameConnect(ulong playerId)
        {
            PlayerConnect temp;
            if (QueueManager.InGamePlayerList.TryGetValue(playerId, out temp))
            {
                return temp;
            }
            return null;
        }

        public void ClinetIdLost(ulong clientId)
        {
            var oldPlayer = GetPlayerConnect(clientId);
            if (oldPlayer == null)
            {
                Logger.Error("player is not in Total.....playerId = {0}", clientId);
                return;
            }
            oldPlayer.IsOnline = false;
            oldPlayer.OnLost();
        }

        public void PlayerEnterGameSuccess(ulong playerId)
        {
            var oldPlayer = GetEnterGameConnect(playerId);
            if (oldPlayer == null)
            {
                Logger.Error("player is not in EnterGame.....playerId = {0}", playerId);
                return;
            }
            QueueManager.EnterGamePlayerList.Remove(playerId);
            QueueManager.TotalList.Remove(oldPlayer.ClientId);
            oldPlayer.State = ConnectState.InGame;
            oldPlayer.Player.Connect = oldPlayer;
            QueueManager.InGamePlayerList.TryAdd(playerId, oldPlayer);
        }

        public void PlayerEnterGameStart(ulong playerId)
        {
            var oldPlayer = GetLandingConnect(playerId);
            if (oldPlayer == null)
            {
                Logger.Error("player is not in landing.....playerId = {0}", playerId);
                return;
            }
            QueueManager.LandingPlayerList.Remove(playerId);
            QueueManager.EnterGamePlayerList.TryAdd(playerId, oldPlayer);
            oldPlayer.State = ConnectState.EnterGame;
        }

        public bool CheckPlayerLanding(ulong playerId)
        {
            var oldPlayer = GetLandingConnect(playerId);
            return oldPlayer != null;
        }

        public bool CheckPlayerEnterGame(ulong playerId)
        {
            var oldPlayer = GetEnterGameConnect(playerId);
            return oldPlayer != null;
        }

        public void Update()
        {
            ++QueueManager.Tick;
            QueueManager.MainQueue.Update();
            foreach (var queue in QueueManager.ServerQueue)
            {
                queue.Value.Update();
            }
            UpdateCheck();
            if (QueueManager.Tick >= 30)
            {
                QueueManager.Tick = 0;
            }
        }

        public int LandingPlayerCount
        {
            get { return QueueManager.LandingPlayerList.Count; }
        } //正在登陆玩家数

        public int EnterGamePlayerCount
        {
            get { return QueueManager.EnterGamePlayerList.Count; }
        } //正在进入游戏过程的玩家数
    }

    public static class QueueManager
    {
        public static Dictionary<string, PlayerConnect> CacheLost = new Dictionary<string, PlayerConnect>();
            //key = 渠道 + Name  ，掉线玩家历史

        public static Dictionary<ulong, PlayerConnect> EnterGamePlayerList = new Dictionary<ulong, PlayerConnect>();
            //playerid

        public static int GamePlayerCount; //游戏内在线玩家数

        public static Dictionary<ulong, PlayerConnect> InGamePlayerList = new Dictionary<ulong, PlayerConnect>();
            //playerid

        public static Dictionary<ulong, PlayerConnect> LandingPlayerList = new Dictionary<ulong, PlayerConnect>();
            //playerid

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static QueueBase MainQueue = new GameQueue();
        private static IQueueManager mImpl;
        public static Dictionary<int, int> PlayerCount = new Dictionary<int, int>(); //每个服务器的玩家
        //每个逻辑服务器的排队
        //server id => queue
        public static Dictionary<int, QueueBase> ServerQueue = new Dictionary<int, QueueBase>();
        public static int Tick;
        public static Dictionary<ulong, PlayerConnect> TotalList = new Dictionary<ulong, PlayerConnect>(); //clientId->

        static QueueManager()
        {
            LoginServer.Instance.UpdateManager.InitStaticImpl(typeof (QueueManager), typeof (QueueManagerDefaultImpl),
                o =>
                {
                    mImpl = (IQueueManager) o;
                    Reset();
                });
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
        }

        public static int EnterGamePlayerCount
        {
            get { return mImpl.EnterGamePlayerCount; }
        } //正在进入游戏过程的玩家数

        public static int LandingPlayerCount
        {
            get { return mImpl.LandingPlayerCount; }
        } //正在登陆玩家数

        public static bool CheckPlayerEnterGame(ulong playerId)
        {
            return mImpl.CheckPlayerEnterGame(playerId);
        }

        public static bool CheckPlayerLanding(ulong playerId)
        {
            return mImpl.CheckPlayerLanding(playerId);
        }

        public static void ClinetIdLost(ulong clientId)
        {
            mImpl.ClinetIdLost(clientId);
        }

        public static PlayerConnect GetEnterGameConnect(ulong playerId)
        {
            return mImpl.GetEnterGameConnect(playerId);
        }

        public static PlayerConnect GetInGameConnect(ulong playerId)
        {
            return mImpl.GetInGameConnect(playerId);
        }

        public static PlayerConnect GetLandingConnect(ulong playerId)
        {
            return mImpl.GetLandingConnect(playerId);
        }

        public static PlayerController GetPlayer(ulong playerId)
        {
            var playerConnect = GetEnterGameConnect(playerId);
            if (playerConnect != null)
            {
                return playerConnect.Player;
            }
            playerConnect = GetLandingConnect(playerId);
            if (playerConnect != null)
            {
                return playerConnect.Player;
            }
            playerConnect = GetInGameConnect(playerId);
            if (playerConnect != null)
            {
                return playerConnect.Player;
            }

            return null;
        }

        public static PlayerConnect GetPlayerConnect(ulong clientId)
        {
            return mImpl.GetPlayerConnect(clientId);
        }

        public static QueueBase GetQueue(int serverId)
        {
            return mImpl.GetQueue(serverId);
        }

        public static PlayerConnect IsCache(string type, ulong clientId, string name)
        {
            return mImpl.IsCache(type, clientId, name);
        }

        public static void PlayerEnterGameStart(ulong playerId)
        {
            mImpl.PlayerEnterGameStart(playerId);
        }

        public static void PlayerEnterGameSuccess(ulong playerId)
        {
            mImpl.PlayerEnterGameSuccess(playerId);
        }

        public static void PushLoginState(PlayerConnect connect)
        {
            mImpl.PushLoginState(connect);
        }

        public static void ReloadTable(IEvent ievent)
        {
            mImpl.ReloadTable(ievent);
        }

        public static void Reset()
        {
            mImpl.Reset();
        }

        public static void Update()
        {
            mImpl.Update();
        }

        #region 玩家数量统计

        public static void EnterPlayer(PlayerConnect pc)
        {
            mImpl.EnterPlayer(pc);
        }

        public static void LeaverPlayer(PlayerConnect pc)
        {
            mImpl.LeaverPlayer(pc);
        }

        #endregion
    }
}