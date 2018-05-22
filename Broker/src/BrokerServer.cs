#region using

using System.Collections.Concurrent;
using System.Threading;
using JsonConfig;
using NLog;
using Scorpion;

#endregion

namespace Broker
{
    public class Broker : IServer
    {
        private int Id;
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IBrokerBase mBroker;
        private string mStatus;
        private int Port;
        private ConfigObject[] ServerList;
        private string Type;

        public void Init(string[] args)
        {
            Type = args[0];
            Port = int.Parse(args[1]);
            Id = int.Parse(args[2]);
            dynamic co = Config.ApplyJson(args[3]);
            switch (args[0])
            {
                case "Login":
                    mBroker = new LoginBroker();
                    ServerList = co.LoginServer;
                    break;
                case "Scene":
                    mBroker = new SceneBroker();
                    ServerList = co.SceneServer;
                    break;
                case "Activity":
                    mBroker = new CommonBroker();
                    ServerList = co.ActivityServer;
                    break;

                case "Logic":
                    mBroker = new CommonBroker();
                    ServerList = co.LogicServer;
                    break;

                case "Rank":
                    mBroker = new CommonBroker();
                    ServerList = co.RankServer;
                    break;

                case "Chat":
                    mBroker = new CommonBroker();
                    ServerList = co.ChatServer;
                    break;
                case "Team":
                    mBroker = new CommonBroker();
                    ServerList = co.TeamServer;
                    break;
                case "GameMaster":
                    mBroker = new CommonBroker();
                    ServerList = co.GameMasterServer;
                    break;
                default:
                    return;
            }
        }

        public void Start()
        {
            mBroker.Start(Id, Port, Type, ServerList);

            mStatus = "Started";

            while (mStatus == "Started")
            {
                //mBroker.ClearLostClient();

                Thread.Sleep(1000);
            }
        }

        public void Status(ConcurrentDictionary<string, string> dict)
        {
            try
            {
                if (mBroker != null)
                    mBroker.Status(dict);
            }
            catch
            {
                // ...
            }
        }

        public void Stop()
        {
            Logger.Debug(@"Gate stopping...");
            mStatus = "Stopped";
            mBroker.Stop();
        }


        public void Rescue()
        {
            throw new System.NotImplementedException();
        }
    }
}