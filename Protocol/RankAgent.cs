using System;
using System.Collections;
using Scorpion;
using NLog;
using RankClientService;

namespace Protocol
{
    public class RankAgentControl : RankAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public RankAgentControl(string addr, uint id)
            : base(addr)
        {
            Id = id;
        }

        public RankAgentControl(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
            : base(broker, directConnect, characterId2ServerId)
        {
            
        }
        public override IEnumerator OnServerLost(Coroutine arg, string target)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator OnServerConnected(Coroutine arg, string target)
        {
            throw new NotImplementedException();
        }

        public override void OnException(Exception ex)
        {
            Logger.Error(ex, "Network error");
        }
    }
}
