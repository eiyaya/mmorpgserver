#region using

using System.Collections.Concurrent;
using Database;
using Scorpion;
using NLog;
using Protocol;
using Shared;

#endregion

namespace Chat
{
    public class ChatServer : ServerBase, IServer
    {
        private static ChatServer _instance;

        public ChatServer()
        {
            _instance = this;
        }

        private readonly Scorpion.ILogger Logger = new ScorpionLogger(LogManager.GetCurrentClassLogger());
        private string mAuth;
        private int mPort;
        public DataManager DB { get; private set; }
        public uint Id { get; set; }

        public static ChatServer Instance
        {
            get { return _instance ?? (_instance = new ChatServer()); }
        }

        public ChatServerControl ServerControl { get; private set; }
        public UpdateManager UpdateManager { get; private set; }

        public new void Init(string[] args)
        {
            Logger.Info(@"Chat server initlizing...");

            CoroutineFactory.ExceptionHandler = exception =>
            {
                Logger.Error("ex " + exception.ToString());
                return true;
            };
            UpdateManager = new UpdateManager();
            UpdateManager.Init("Chat");

            ServerControl = new ChatServerControl();

            if (!int.TryParse(args[1], out mPort))
            {
                Logger.Warn(@"Chat server initlizing Faild! args[1]={0}", args[1]);
                return;
            }
            base.Init(args);


            int nId;
            if (!int.TryParse(args[0], out nId))
            {
                Logger.Warn(@"Chat server initlizing Faild! args[0]={0}", args[0]);
                return;
            }
            Id = (uint) nId;
            mAuth = args[2];
            var strDbConfig = args[4];

            //Test t = new Test();
            //t.Start(new[] { strDbConfig });
            DB = new DataManager(strDbConfig, ServerControl, DataCategory.Chat, nId);

			AnchorManager.Instance.LoadConfig();
        }

        public void Start()
        {
            Logger.Info(@"Chat server starting...");

            ServerControl.Start(mPort, mAuth, Logger);
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
            Logger.Info(@"Chat server stopping...");

            ServerControl.Stop();
            base.Stop();
        }


        public void Rescue()
        {
            ServerControl.StartEventLoop();
        }
    }
}