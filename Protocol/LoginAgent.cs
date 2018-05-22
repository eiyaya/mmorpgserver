using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using LoginClientService;
using Scorpion;
using NLog;

namespace Protocol
{
    public class LoginAgentControl : LoginAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public LoginAgentControl(string addr, uint id) : base(addr)
        {
            Id = id;
        }


        public LoginAgentControl(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
            : base(broker, directConnect, characterId2ServerId)
        {
            
        }

        public override IEnumerator OnServerLost(Coroutine arg, string target)
        {
            return null;
            //throw new NotImplementedException();
        }

        public override IEnumerator OnServerConnected(Coroutine arg, string target)
        {
            return null;
            //throw new NotImplementedException();
        }

        public override void OnException(Exception ex)
        {
            Logger.Error(ex, "Network error");
        }
    }
}
