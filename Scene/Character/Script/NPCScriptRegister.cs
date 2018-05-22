#region using

using System;
using System.Collections.Generic;

#endregion

namespace Scene
{
    public interface INPCScriptRegister
    {
        NPCScriptBase CreateScriptInstance(int id);
    }

    public class NPCScriptRegisterDefaultImpl : INPCScriptRegister
    {
        public NPCScriptBase CreateScriptInstance(int id)
        {
            Type t = null;
            if (NPCScriptRegister.mDict.TryGetValue(id, out t))
            {
                return (NPCScriptBase) Activator.CreateInstance(t);
            }

            return new NPCScriptBase();
        }
    }

    public static class NPCScriptRegister
    {
        public static Dictionary<int, Type> mDict = new Dictionary<int, Type>
        {
            {200000, Type.GetType("Scene.NPCScript200000")}, //调试用
            {200001, Type.GetType("Scene.NPCScript200001")}, //怪物基础AI
            {200002, Type.GetType("Scene.NPCScript200002")}, //箱子基础AI
            {210000, Type.GetType("Scene.NPCScript210000")}, //随从基础AI
            {300000, Type.GetType("Scene.NPCScript300000")}, //防御塔AI
            {300001, Type.GetType("Scene.NPCScript300001")}, //主动攻击怪的防御塔AI
            {300100, Type.GetType("Scene.NPCScript300100")}, //灭世防御塔AI
            {220000, Type.GetType("Scene.P1VP1AutoPlayer220000")}, //P1vP1玩家基础AI
            {250000, Type.GetType("Scene.NpcFrozenThrone250000")}, //冰封王座的怪物AI
			{250001, Type.GetType("Scene.NPCScript250001")}, //只攻击固定类型  小怪
			{250002, Type.GetType("Scene.NPCScript250002")}, //只攻击固定类型  BOSS
            {250100, Type.GetType("Scene.NpcMieShi250100")}, //灭世之战的怪物AI
            {200003, Type.GetType("Scene.NPCScript200003")}, //只会往目标走
            {200004, Type.GetType("Scene.NPCScript200004")}, //往目标走，遇到可攻击的就攻击
            {200005, Type.GetType("Scene.NPCScript200005")}, //会主动攻击怪
            {200006, Type.GetType("Scene.NPCScript200006")}, //往目标走，遇到可攻击的就攻击
            {400000, Type.GetType("Scene.NPCScript400000")}, //巡逻
            {260000, Type.GetType("Scene.AutoPlayer260000")}, //仿玩家行为npc
            {260001, Type.GetType("Scene.AutoPlayer260001")}, //仿玩家行为npc，但失去目标的时候根据配表决定是否回血
        };

        private static INPCScriptRegister mImpl;

        static NPCScriptRegister()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (NPCScriptRegister),
                typeof (NPCScriptRegisterDefaultImpl),
                o => { mImpl = (INPCScriptRegister) o; });
        }

        public static NPCScriptBase CreateScriptInstance(int id)
        {
            return mImpl.CreateScriptInstance(id);
        }
    }
}