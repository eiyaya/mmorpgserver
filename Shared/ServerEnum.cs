namespace Shared
{
    //Gate的链接状态
    public enum GateClientState
    {
        NotAuthorized = 0,
        Login = 1,
        GamePlay = 2, //有CharacterId，而且已经知道包需要发给scene login chat rank
        Lost = 3 //需要断开的链接
    }
}