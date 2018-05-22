#region using

using System.Collections.Concurrent;
using NLog;

#endregion

namespace Broker
{
    public class CharacterManagerBroker
    {
        public CharacterManagerBroker(SceneBroker broker, string ServiceName)
        {
            Logger = LogManager.GetLogger("Broker." + ServiceName + "Broker");
            mBroker = broker;
        }

        private Logger Logger;
        private SceneBroker mBroker;
        // 准备完整维护一份玩家角色数据， CharacterId ->  具体数据
        public readonly ConcurrentDictionary<ulong, CharacterSceneInfo> mCharacterInfo =
            new ConcurrentDictionary<ulong, CharacterSceneInfo>();

        //创建
        public bool CreateCharacter(ulong characterId, out CharacterSceneInfo characterInfo)
        {
            if (mCharacterInfo.TryGetValue(characterId, out characterInfo))
            {
                return false; //说明之前已经有这个人了，这次create是失败的
            }
            characterInfo = new CharacterSceneInfo(characterId);
            mCharacterInfo[characterId] = characterInfo;
            return true;
        }

        //查询
        public CharacterSceneInfo GetCharacter(ulong characterId)
        {
            CharacterSceneInfo characterInfo = null;
            if (mCharacterInfo.TryGetValue(characterId, out characterInfo))
            {
                return characterInfo;
            }
            return null;
        }
    }
}