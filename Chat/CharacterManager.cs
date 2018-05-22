#region using

using DataContract;
using Shared;

#endregion

namespace Chat
{
    public class CharacterManager :
        CharacterManager<ChatCharacterController, DBCharacterChat, DBCharacterChatSimple, DBCharacterChatVolatile>
    {
    }
}