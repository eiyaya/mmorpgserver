#region using

using System.Collections.Concurrent;
using Database;
using Scorpion;
using NLog;
using Protocol;
using Shared;

#endregion

namespace Scene
{
    public class SceneServer : ServerBase, IServer
    {
        private static SceneServer _instance;

        public SceneServer()
        {
            _instance = this;
        }

        private readonly Scorpion.ILogger Logger = new ScorpionLogger(LogManager.GetCurrentClassLogger());
        private string mAuth;
        private int mPort;
        public DataManager DB { get; private set; }
        public uint Id { get; private set; }

        public static SceneServer Instance
        {
            get { return _instance ?? (_instance = new SceneServer()); }
        }

        public SceneServerControl ServerControl { get; private set; }
        public UpdateManager UpdateManager { get; private set; }

        public new void Init(string[] args)
        {
            Logger.Info(@"Scene server initlizing...");

            UpdateManager = new UpdateManager();
            UpdateManager.Init("Scene");

            ServerControl = new SceneServerControl();

            if (!int.TryParse(args[1], out mPort))
            {
                Logger.Warn(@"Scene server initlizing Faild! args[1]={0}", args[1]);
                return;
            }

            base.Init(args);


            AttrBaseManager.Init(); //角色属性获得优化结构

            CoroutineFactory.ExceptionHandler = exception =>
            {
                Logger.Error("ex " + exception.ToString());
                return true;
            };
            int nId;
            if (!int.TryParse(args[0], out nId))
            {
                Logger.Warn(@"Scene server initlizing Faild! args[0]={0}", args[0]);
                return;
            }
            Id = (uint) nId;
            mAuth = args[2];
            var strDbConfig = args[4];
            DB = new DataManager(strDbConfig, ServerControl, DataCategory.Scene, nId);
        }

        public void Start()
        {
            Logger.Info(@"Scene server starting...");

            PathManager.Start();
            SceneManager.Instance.Init();
            //TestFight.Test();
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
            Logger.Info(@"Scene server stopping...");

            PathManager.Stop();
            ServerControl.Stop();
            base.Stop();
        }
    }
}