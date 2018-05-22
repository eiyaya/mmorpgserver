#region using

using EventSystem;

#endregion

namespace Scene
{
    public class ExpBattleFieldPlayTimeResetEvent : EventBase
    {
        public static string EVENT_TYPE = "ExpBattleFieldPlayTimeResetEvent";

        public ExpBattleFieldPlayTimeResetEvent(ulong id)
            : base(EVENT_TYPE)
        {
            PlayerId = id;
        }

        public ulong PlayerId;
    }
}