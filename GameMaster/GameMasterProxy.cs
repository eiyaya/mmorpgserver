#region using

using System.Collections;
using GameMasterServerService;
using Scorpion;
using NLog;

#endregion

namespace GameMaster
{
    public class GameMasterProxyDefaultImpl : IGameMasterCharacterProxy
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerator OnConnected(Coroutine coroutine, GameMasterCharacterProxy charProxy, uint packId)
        {
            yield break;
        }

        public IEnumerator OnLost(Coroutine coroutine, GameMasterCharacterProxy charProxy, uint packId)
        {
            yield break;
        }

        public bool OnSyncRequested(GameMasterCharacterProxy charProxy, ulong characterId, uint syncId)
        {
            return false;
        }
    }


    public class GameMasterProxy : GameMasterCharacterProxy
    {
        public GameMasterProxy(GameMasterService service, ulong characterId, ulong clientId)
            : base(service, characterId, clientId)
        {
        }
    }
}