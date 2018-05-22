using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonConfig;
using Scorpion;
using System.Collections.Concurrent;

namespace Protocol
{
    public class AgentControl
    {
        public LoginAgentControl LoginAgent;
        public SceneAgentControl SceneAgent;
        public LogicAgentControl LogicAgent;
        public RankAgentControl RankAgent;
        public ActivityAgentControl ActivityAgent;
        public ChatAgentControl ChatAgent;
        public TeamAgentControl TeamAgent;
        public GameMasterAgentControl GameMasterAgent;
        public Dictionary<string, ClientAgentBase> Agents = new Dictionary<string, ClientAgentBase>();
        public dynamic ServerConfig;

        public dynamic GetServerConfig(string serverName)
        {
            if (serverName == "Login")
            {
                return ServerConfig.LoginServer;
            }
            else if (serverName == "Activity")
            {
                return ServerConfig.ActivityServer;
            }
            else if (serverName == "Logic")
            {
                return ServerConfig.LogicServer;
            }
            else if (serverName == "Rank")
            {
                return ServerConfig.RankServer;
            }
            else if (serverName == "Scene")
            {
                return ServerConfig.SceneServer;
            }
            else if (serverName == "Chat")
            {
                return ServerConfig.ChatServer;
            }
            else if (serverName == "Team")
            {
                return ServerConfig.TeamServer;
            }
            else if (serverName == "GameMaster")
            {
                return ServerConfig.GameMasterServer;
            }
            else
            {
                throw new Exception("Unkonw server name " + serverName);
            }
        }

        public bool Init(string broker, string server)
        {
            dynamic brokers = Config.ApplyJson(broker);
            ServerConfig = Config.ApplyJson(server);

            if (brokers.ContainsKey("Login"))
            {
                List<ServerInfo> infos = new List<ServerInfo>();
                var serverConfig = GetServerConfig("Login");
                foreach (var config in serverConfig)
                {
                    infos.Add(new ServerInfo
                    {
                        Id = config.Id,
                        Ip = config.Ip,
                        Port = config.Port
                    });
                }

                LoginAgent = new LoginAgentControl(new ServerInfo
                {
                    Id = brokers.Login.Id,
                    Ip = brokers.Login.Ip,
                    Port = brokers.Login.Port
                }, infos.ToArray(), arg => (int) (arg%(ulong) infos.Count));

                Agents.Add("Login", LoginAgent);                
            }
            if (brokers.ContainsKey("Scene"))
            {
                SceneAgent = new SceneAgentControl(brokers.Scene.Ip.ToString() + ":" + brokers.Scene.Port.ToString(), (uint)(brokers.Scene.Id));
                Agents.Add("Scene", SceneAgent);   
            }
            if (brokers.ContainsKey("Logic"))
            {
                List<ServerInfo> infos = new List<ServerInfo>();
                var serverConfig = GetServerConfig("Logic");
                foreach (var config in serverConfig)
                {
                    infos.Add(new ServerInfo
                    {
                        Id = config.Id,
                        Ip = config.Ip,
                        Port = config.Port
                    });
                }

                LogicAgent = new LogicAgentControl(new ServerInfo
                {
                    Id = brokers.Logic.Id,
                    Ip = brokers.Logic.Ip,
                    Port = brokers.Logic.Port
                }, infos.ToArray(), arg => (int)(arg % (ulong)infos.Count));

                Agents.Add("Logic", LogicAgent);                
            }
            if (brokers.ContainsKey("Rank"))
            {
                List<ServerInfo> infos = new List<ServerInfo>();
                var serverConfig = GetServerConfig("Rank");
                foreach (var config in serverConfig)
                {
                    infos.Add(new ServerInfo
                    {
                        Id = config.Id,
                        Ip = config.Ip,
                        Port = config.Port
                    });
                }

                RankAgent = new RankAgentControl(new ServerInfo
                {
                    Id = brokers.Rank.Id,
                    Ip = brokers.Rank.Ip,
                    Port = brokers.Rank.Port
                }, infos.ToArray(), arg => (int)(arg % (ulong)infos.Count));
                Agents.Add("Rank", RankAgent);   
            }
            if (brokers.ContainsKey("Activity"))
            {
                List<ServerInfo> infos = new List<ServerInfo>();
                var serverConfig = GetServerConfig("Activity");
                foreach (var config in serverConfig)
                {
                    infos.Add(new ServerInfo
                    {
                        Id = config.Id,
                        Ip = config.Ip,
                        Port = config.Port
                    });
                }

                ActivityAgent = new ActivityAgentControl(new ServerInfo
                {
                    Id = brokers.Activity.Id,
                    Ip = brokers.Activity.Ip,
                    Port = brokers.Activity.Port
                }, infos.ToArray(), arg => (int)(arg % (ulong)infos.Count));
                Agents.Add("Activity", ActivityAgent);                            
            }
            if (brokers.ContainsKey("Chat"))
            {
                List<ServerInfo> infos = new List<ServerInfo>();
                var serverConfig = GetServerConfig("Chat");
                foreach (var config in serverConfig)
                {
                    infos.Add(new ServerInfo
                    {
                        Id = config.Id,
                        Ip = config.Ip,
                        Port = config.Port
                    });
                }

                ChatAgent = new ChatAgentControl(new ServerInfo
                {
                    Id = brokers.Chat.Id,
                    Ip = brokers.Chat.Ip,
                    Port = brokers.Chat.Port
                }, infos.ToArray(), arg => (int)(arg % (ulong)infos.Count));
                Agents.Add("Chat", ChatAgent);                
            }
            if (brokers.ContainsKey("Team"))
            {
                List<ServerInfo> infos = new List<ServerInfo>();
                var serverConfig = GetServerConfig("Team");
                foreach (var config in serverConfig)
                {
                    infos.Add(new ServerInfo
                    {
                        Id = config.Id,
                        Ip = config.Ip,
                        Port = config.Port
                    });
                }

                TeamAgent = new TeamAgentControl(new ServerInfo
                {
                    Id = brokers.Team.Id,
                    Ip = brokers.Team.Ip,
                    Port = brokers.Team.Port
                }, infos.ToArray(), arg => (int)(arg % (ulong)infos.Count));
                Agents.Add("Team", TeamAgent);
            }
            if (brokers.ContainsKey("GameMaster"))
            {
                List<ServerInfo> infos = new List<ServerInfo>();
                var serverConfig = GetServerConfig("GameMaster");
                foreach (var config in serverConfig)
                {
                    infos.Add(new ServerInfo
                    {
                        Id = config.Id,
                        Ip = config.Ip,
                        Port = config.Port
                    });
                }

                GameMasterAgent = new GameMasterAgentControl(new ServerInfo
                {
                    Id = brokers.GameMaster.Id,
                    Ip = brokers.GameMaster.Ip,
                    Port = brokers.GameMaster.Port
                }, infos.ToArray(), arg => (int)(arg % (ulong)infos.Count));
                Agents.Add("GameMaster", GameMasterAgent);

                //GameMasterAgent = new GameMasterAgentControl(brokers.GameMaster.Ip.ToString() + ":" + brokers.GameMaster.Port.ToString(), (uint)(brokers.GameMaster.Id));
                //Agents.Add("GameMaster", GameMasterAgent);
            }
            return true;
        }


        public bool Start(ServerAgentBase server)
        {
            foreach (var agent in Agents)
            {
                agent.Value.Start(server);
            }
            return true;
        }

        public void ConnetedInfo(ConcurrentDictionary<string, string> dict)
        {
            foreach (var agent in Agents)
            {
                if (agent.Value.Connected == false)
                {
                    dict.TryAdd(agent.Key, "False");
                }
            }
        }

        public void Stop()
        {
            foreach (var agent in Agents)
            {
                agent.Value.Stop();
            }
        }
    }
}
