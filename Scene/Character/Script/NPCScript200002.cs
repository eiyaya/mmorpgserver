namespace Scene
{
    public class NPCScript200002 : NPCScriptBase
    {
        //战斗
        public override void OnEnterCombat(ObjNPC npc)
        {
        }

        //死亡
        public override void OnEnterDie(ObjNPC npc)
        {
        }

        //回家
        public override void OnEnterGoHome(ObjNPC npc)
        {
        }

        public override void OnExitCombat(ObjNPC npc)
        {
        }

        public override void OnExitDie(ObjNPC npc)
        {
        }

        public override void OnExitGoHome(ObjNPC npc)
        {
        }

        public override void OnTickCombat(ObjNPC npc, float delta)
        {
        }

        public override void OnTickDie(ObjNPC npc, float delta)
        {
        }

        public override void OnTickGoHome(ObjNPC npc, float delta)
        {
            npc.EnterState(BehaviorState.Idle);
        }
    }
}