namespace Scene
{
    public class TypeDefine
    {
        public const ulong INVALID_ULONG = ulong.MaxValue;
    }

    //移动结果
    public enum MoveResult
    {
        Ok = 0, //ok
        AlreadyThere, //已经在目标点了
        CannotReach, //目标点无法到达
        CannotMoveByBuff //定身buff不能移动
    }
}