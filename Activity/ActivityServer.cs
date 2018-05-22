#region using

using System.Collections.Concurrent;
using Database;
using Scorpion;
using NLog;
using Protocol;
using Shared;

#endregion

namespace Activity
{
    public class ActivityServer : ServerBase, IServer
    {
        private static ActivityServer _instance;

        public ActivityServer()
        {
            _instance = this;
        }

        public readonly Scorpion.ILogger Logger = new ScorpionLogger(LogManager.GetCurrentClassLogger());
        private string mAuth;
        private int mPort;
        public DataManager DB { get; private set; }
        public uint Id { get; set; }

        public static ActivityServer Instance
        {
            get { return _instance ?? (_instance = new ActivityServer()); }
        }

        public ActivityServerControl ServerControl { get; private set; }
        public UpdateManager UpdateManager { get; private set; }



        public new void Init(string[] args)
        {
            Logger.Info(@"Activity server initlizing...");

            CoroutineFactory.ExceptionHandler = exception =>
            {
                Logger.Error("ex " + exception.ToString());
                return true;
            };
            UpdateManager = new UpdateManager();
            UpdateManager.Init("Activity");

            ServerControl = new ActivityServerControl();

            if (!int.TryParse(args[1], out mPort))
            {
                Logger.Warn(@"Activity server initlizing Faild! args[1]={0}", args[1]);
                return;
            }
            base.Init(args);
            int nId;
            if (!int.TryParse(args[0], out nId))
            {
                Logger.Warn(@"Activity server initlizing Faild! args[0]={0}", args[0]);
                return;
            }
            Id = (uint) nId;
            mAuth = args[2];
            var strDbConfig = args[4];
            DB = new DataManager(strDbConfig, ServerControl, DataCategory.Activity, nId);

            dynamic ServersConfig = JsonConfig.Config.ApplyJson(args[5]);
            string PayServer = ServersConfig.PayServer;

            if (!PayServer.StartsWith("http"))
            {
                PayServer = string.Format("http://{0}", PayServer);
            }

            ServerControl.PayServerNotifyAddress = string.Format("{0}/notify/", PayServer);
            ServerControl.PayServerVerifyAddress = string.Format("{0}/verify/", PayServer);

        }

        public void Start()
        {
            Logger.Info(@"Activity server starting...");

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
            Logger.Info(@"Activity server stopping...");

            ServerControl.Stop();
            base.Stop();
        }
    }
}