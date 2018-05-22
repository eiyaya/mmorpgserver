#region using

using System.Collections.Generic;
using DataTable;
using EventSystem;
using NLog;
using Shared;
using System;
using DataContract;

#endregion

namespace Login
{
    public class ChannelServerInfo
    {
        /// <summary>
        /// 这个渠道的所有服务器
        /// </summary>
        public HashSet<ServerNameRecord> AllSevers = new HashSet<ServerNameRecord>();

        /// <summary>
        /// 这个渠道新开的服务器
        /// </summary>
        public List<ServerNameRecord> NewServers = new List<ServerNameRecord>();

        /// <summary>
        /// 这个渠道准备开的服务器
        /// </summary>
        public List<ServerNameRecord> PrepareServers = new List<ServerNameRecord>();

        public List<ServerState> AllServerStates = new List<ServerState>();
    }

    public static class StaticParam
    {
        public static int EnterGamePlayerCountMax;
        public static int GamePlayerCountMax;
        public static int LandingPlayerCountMax;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //服务器排队参数
        public static int WaitPlayerCountMax;

        //渠道可见列表
        public static Dictionary<string, ChannelServerInfo> ServerListWithPid = new Dictionary<string, ChannelServerInfo>();

        static StaticParam()
        {
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
            Init();
        }

        private static void Init()
        {
            ResetServerConfigValues();
            ReloadChannelPid();
        }

        private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v.tableName == "ServerConfig")
            {
                ResetServerConfigValues();
            }

            if (v.tableName == "ServerName")
            {
                ReloadChannelPid();
            }
        }

        private static void ResetServerConfigValues()
        {
            WaitPlayerCountMax = Table.GetServerConfig(12).ToInt();
            GamePlayerCountMax = Table.GetServerConfig(13).ToInt();
            LandingPlayerCountMax = Table.GetServerConfig(14).ToInt();
            EnterGamePlayerCountMax = Table.GetServerConfig(15).ToInt();
        }

        private static void ReloadChannelPid()
        {
            ServerListWithPid.Clear();
            Table.ForeachServerName((record) =>
            {
                var pids = record.Channels.Trim().Split('|');
                foreach (var pid in pids)
                {
                    var id = pid.Trim();
                    ChannelServerInfo serverlist;
                    if (!ServerListWithPid.TryGetValue(id, out serverlist))
                    {
                        serverlist = new ChannelServerInfo();
                        ServerListWithPid.Add(id, serverlist);
                    }

                    if (!serverlist.AllSevers.Contains(record))
                    {
                        serverlist.AllSevers.Add(record);
                    }
                }
                return true;
            });

            LoginServer.Instance.ServerControl.NextRefreshServerListTime = DateTime.Now;
        }

    }
}