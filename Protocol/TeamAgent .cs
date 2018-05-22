using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContract;
using Scorpion;
using NLog;
using SceneClientService;

namespace Protocol
{
    public class SceneAgentControl : SceneAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public SceneAgentControl(string addr, uint id)
            : base(addr)
        {
            Id = id;
        }
        public SceneAgentControl(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
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
