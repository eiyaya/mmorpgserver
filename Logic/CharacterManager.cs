#region using

using DataContract;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public class CharacterManager :
        CharacterManager<CharacterController, DBCharacterLogic, DBCharacterLogicSimple, DBCharacterLogicVolatile>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override void LookCharacters()
        {
            foreach (var c in mDictionary)
            {
                if (c.Value.Controller == null)
                {
                    if (c.Value.SimpleData == null)
                    {
                        Logger.Info("c={0}!  Controller is null! SimpleData is null!", c.Key);
                    }
                    else
                    {
                        Logger.Info("c={0}!  Controller is null! SimpleData not null!", c.Key);
                    }
                }
                else
                {
                    var proxy = c.Value.Controller.Proxy == null ? "is" : "not ";
                    if (c.Value.SimpleData == null)
                    {
                        Logger.Info("c={0}! p {1} null! Controller not null! SimpleData is null!", c.Key, proxy);
                    }
                    else
                    {
                        Logger.Info("c={0}! p {1} null! Controller not null! SimpleData not null!", c.Key, proxy);
                    }
                }
            }
        }
    }
}