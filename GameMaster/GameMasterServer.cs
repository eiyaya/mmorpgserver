#region using

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using Database;
using DataContract;
using Scorpion;
using NLog;
using ProtoBuf;
using Protocol;
using Shared;

#endregion

namespace GameMaster
{
    public class GameMasterServer : ServerBase, IServer
    {
        private static GameMasterServer _instance;
        private HttpListen mListen;
        public GameMasterServer()
        {
            _instance = this;
        }

        private readonly Scorpion.ILogger Logger = new ScorpionLogger(LogManager.GetCurrentClassLogger());
        private string mAuth;
        private int mPort;
        private GameMasterServerControl mServerControl;
        public DataManager DB { get; private set; }
        public uint Id { get; set; }
        private int isOpen = 0;
        private int httpPort = 0;

        public static GameMasterServer Instance
        {
            get { return _instance ?? (_instance = new GameMasterServer()); }
        }

        public UpdateManager UpdateManager { get; private set; }

        protected IEnumerator SaveData<T>(Coroutine coroutine, DataCategory cat, string GMName, T data)
            where T : IExtensible
        {
            var result = DB.Set(coroutine, cat, GMName, data);
            yield return result;
            if (result.Status != DataStatus.Ok)
            {
                Logger.Error("Save data for GMName {0} failed.", GMName);
            }
        }

        public new void Init(string[] args)
        {
            Logger.Info(@"GameMasterServer server initlizing...");

            UpdateManager = new UpdateManager();
            UpdateManager.Init("GameMaster");

            mServerControl = new GameMasterServerControl();

            if (!int.TryParse(args[1], out mPort))
            {
                Logger.Warn(@"GameMasterServer server initlizing Faild! args[1]={0}", args[1]);
                return;
            }
            base.Init(args);
            int nId;
            if (!int.TryParse(args[0], out nId))
            {
                Logger.Warn(@"GameMasterServer server initlizing Faild! args[0]={0}", args[0]);
                return;
            }
            Id = (uint) nId;
            mAuth = args[2];
            var strDbConfig = args[4];
            DB = new DataManager(strDbConfig, mServerControl, DataCategory.GameMaster, nId);

            LogSettings.Init();

            var dbConfig = File.ReadAllLines("../Config/gmhttp.config");
            isOpen = 0;
            httpPort = 0; 
            if (dbConfig.Count() > 1)
            {
                var configStr = dbConfig[1].Trim();
                var configStr2 = configStr.Split('=');
                if (configStr2.Count() > 1 && configStr2[0].Equals("isopen"))
                {
                    isOpen = int.Parse(configStr2[1]); 
                }

                var portStr = dbConfig[0].Trim();
                var portStr2 = portStr.Split('=');
                if (portStr2.Count() > 1 && portStr2[0].Equals("port"))
                {
                    httpPort = int.Parse(portStr2[1]);
                }
            }
                
            if (isOpen == 1 && httpPort > 0)
            {
                mListen = new HttpListen(new GameMasterResponser(), httpPort);
                mListen.Start();
            }
        }

        public void Start()
        {
            Logger.Info(@"GameMasterServer server starting...");

            mServerControl.Start(mPort, mAuth, Logger);
        }

        public void Rescue()
        {
            mServerControl.StartEventLoop();
        }

        public new void Status(ConcurrentDictionary<string, string> dict)
        {
            try
            {
                if (mServerControl != null)
                    mServerControl.Status(dict);
            }
            catch
            {
                // ...
            }
        }

        public new void Stop()
        {
            Logger.Info(@"GameMasterServer server stopping...");

            if (isOpen == 1 && httpPort > 0)
            {
                mListen.Stop();
            }
            mServerControl.Stop();
            base.Stop();
        }
    }
}