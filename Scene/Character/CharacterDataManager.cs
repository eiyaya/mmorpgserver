#region using

using DataContract;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public interface ICharacterManager
    {
        void LookCharacters(CharacterManager _this);
    }

    public class CharacterManagerDefaultImpl : ICharacterManager
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public void LookCharacters(CharacterManager _this)
        {
            PlayerLog.WriteLog((int) LogType.CharacterManager,
                "--------------------------------------Begin--------------------------------------------------------");
            foreach (var c in _this.mDictionary)
            {
                if (c.Value.Controller == null)
                {
                    if (c.Value.SimpleData == null)
                    {
                        PlayerLog.WriteLog((int) LogType.CharacterManager,
                            "c={0}!  Controller is null! SimpleData is null!", c.Key);
                    }
                    else
                    {
                        PlayerLog.WriteLog((int) LogType.CharacterManager,
                            "c={0}!  Controller is null! SimpleData not null!", c.Key);
                    }
                }
                else
                {
                    var proxy = c.Value.Controller.Proxy == null ? "is" : "not ";
                    if (c.Value.SimpleData == null)
                    {
                        PlayerLog.WriteLog((int) LogType.CharacterManager,
                            "c={0}! p {1} null! Controller not null! SimpleData is null!", c.Key, proxy);
                    }
                    else
                    {
                        PlayerLog.WriteLog((int) LogType.CharacterManager,
                            "c={0}! p {1} null! Controller not null! SimpleData not null!", c.Key, proxy);
                    }
                }
            }
            PlayerLog.WriteLog((int) LogType.CharacterManager,
                "--------------------------------------End--------------------------------------------------------");
        }
    }

    public class CharacterManager : CharacterManager<ObjPlayer, DBCharacterScene, DBCharacterSceneSimple>
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private static ICharacterManager mImpl;
        public static FindString<ObjPlayer> playerName = new FindString<ObjPlayer>();

        static CharacterManager()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (CharacterManager),
                typeof (CharacterManagerDefaultImpl),
                o => { mImpl = (ICharacterManager) o; });
        }

        public override void LookCharacters()
        {
            mImpl.LookCharacters(this);
        }
    }
}