﻿#region using

using System.Collections.Concurrent;
using Database;
using Scorpion;
using NLog;
using Protocol;
using Shared;

#endregion

namespace Team
{
    public class TeamServer : ServerBase, IServer
    {
        private static TeamServer _instance;

        public TeamServer()
        {
            _instance = this;
        }

        private readonly Scorpion.ILogger Logger = new ScorpionLogger(LogManager.GetCurrentClassLogger());
        private string mAuth;
        private int mPort;
        public DataManager DB { get; private set; }
        public uint Id { get; set; }

        public static TeamServer Instance
        {
            get { return _instance ?? (_instance = new TeamServer()); }
        }

        public TeamServerControl ServerControl { get; private set; }
        public UpdateManager UpdateManager { get; private set; }

        public new void Init(string[] args)
        {
            Logger.Info(@"Team server initlizing...");

            CoroutineFactory.ExceptionHandler = exception =>
            {
                Logger.Error("ex " + exception.ToString());
                return true;
            };
            UpdateManager = new UpdateManager();
            UpdateManager.Init("Team");

            ServerControl = new TeamServerControl();

            if (!int.TryParse(args[1], out mPort))
            {
                Logger.Warn(@"Team server initlizing Faild! args[1]={0}", args[1]);
                return;
            }
            base.Init(args);
            int nId;
            if (!int.TryParse(args[0], out nId))
            {
                Logger.Warn(@"Team server initlizing Faild! args[0]={0}", args[0]);
                return;
            }
            Id = (uint) nId;
            mAuth = args[2];
            var strDbConfig = args[4];
            DB = new DataManager(strDbConfig, ServerControl, DataCategory.Team, nId);
        }

        public void Start()
        {
            Logger.Info(@"Team server starting...");
            ServerControl.Start(mPort, mAuth, Logger);
        }

        public void Rescue()
        {
            ServerControl.StartEventLoop();
        }

        public new void Status(ConcurrentDictionary<string, string> dict)
        {
            try
            {
                if (ServerControl != null)
                    ServerControl.Status(dict);
            }
            catch
            {
                // ...
            }
        }

        public new void Stop()
        {
            Logger.Info(@"Team server stopping...");

            ServerControl.Stop();
            base.Stop();
        }
    }
}