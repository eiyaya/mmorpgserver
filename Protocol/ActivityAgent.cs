using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActivityClientService;
using Scorpion;
using NLog;

namespace Protocol
{
    public class ActivityAgentControl :ActivityAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActivityAgentControl(string addr, uint id)
            : base(addr)
        {
            Id = id;
        }
        public ActivityAgentControl(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
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
