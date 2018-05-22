namespace Scene
{
    public interface IScriptScene
    {
        void ExitDungeon(ObjPlayer player);
        void OnCreate();
        void OnDestroy();
        void OnNpcDie(ObjNPC npc, ulong characterId = 0);
        void OnPlayerDie(ObjPlayer player, ulong characterId = 0);
        void OnPlayerEnter(ObjPlayer player);
        void OnPlayerLeave(ObjPlayer player);
    }
}