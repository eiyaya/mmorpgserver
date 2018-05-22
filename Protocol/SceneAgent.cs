using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContract;
using Scorpion;
using NLog;
using TeamClientService;

namespace Protocol
{
    public class TeamAgentControl : TeamAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public TeamAgentControl(string addr, uint id)
            : base(addr)
        {
            Id = id;
        }
        public TeamAgentControl(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
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
