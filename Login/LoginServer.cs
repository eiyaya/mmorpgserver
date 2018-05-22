#region using

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Database;
using DataContract;
using Scorpion;
using NLog;
using Protocol;
using Shared;

#endregion

namespace Login
{
    public class LoginServer : ServerBase, IServer
    {
        private static LoginServer _instance;

        public LoginServer()
        {
            _instance = this;
        }

        private readonly Scorpion.ILogger Logger = new ScorpionLogger(LogManager.GetCurrentClassLogger());
        private string mAuth;
        public int mPort;
        public DataManager DB { get; private set; }
        public uint Id { get; set; }

        public static LoginServer Instance
        {
            get { return _instance ?? (_instance = new LoginServer()); }
        }

		public SpecialAccount s_SpecialAccount = new SpecialAccount();

        public LoginServerControl ServerControl { get; private set; }
        public UpdateManager UpdateManager { get; private set; }

        public new void Init(string[] args)
        {
            Logger.Info(@"Login server initlizing...");
			s_SpecialAccount.LoadConfig();

            CoroutineFactory.ExceptionHandler = exception =>
            {
                Logger.Error("ex " + exception.ToString());
                return true;
            };

            UpdateManager = new UpdateManager();
            UpdateManager.Init("Login");

            ServerControl = new LoginServerControl();

            if (!int.TryParse(args[1], out mPort))
            {
                Logger.Warn(@"Login server initlizing Faild! args[1]={0}", args[1]);
                return;
            }
            base.Init(args);


            int nId;
            if (!int.TryParse(args[0], out nId))
            {
                Logger.Warn(@"Login server initlizing Faild! args[0]={0}", args[0]);
                return;
            }
            Id = (uint) nId;
            mAuth = args[2];
            var strDbConfig = args[4];
            DB = new DataManager(strDbConfig, ServerControl, DataCategory.Login, nId);
        }

        public void Start()
        {
            Logger.Info(@"Login server starting...");

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
            Logger.Info(@"Login server stopping...");

            ServerControl.Stop();
            base.Stop();
        }
    }
}