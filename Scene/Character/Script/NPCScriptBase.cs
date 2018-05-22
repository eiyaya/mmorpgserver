#region using

using System.Collections.Generic;
using DataTable;
using NLog;

#endregion

namespace Scene
{
    public interface INPCScriptBase
    {
        void ChangeAi(NPCScriptBase _this, ObjNPC npc, int nextId);
        ObjCharacter GetAttackTarget(NPCScriptBase _this, ObjNPC npc);
        void OnEnterGoHome(NPCScriptBase _this, ObjNPC npc);
		void Init(NPCScriptBase _this, ObjNPC npc);
    }

    public class NPCScriptBaseDefaultImpl : INPCScriptBase
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();

	    public virtual void Init(NPCScriptBase _this, ObjNPC npc)
	    {
		    
	    }

        //回家
        public virtual void OnEnterGoHome(NPCScriptBase _this, ObjNPC npc)
        {
            if(npc.TableNpc.LeaveFightAddBlood != 0)
                npc.ResetAttribute();

            npc.RemoveCantMoveBuff();
        }

		public virtual ObjCharacter GetAttackTarget(NPCScriptBase _this, ObjNPC npc)
        {
            switch (npc.TableAI.HatreType)
            {
                case 0: //仇恨的
                    if (npc.TableAI.SortType == 0)
                    {
                        return npc.GetMaxHatre();
                    }
                    if (npc.TableAI.SortType == 1)
                    {
                        return npc.GetMinHatre();
                    }
                    Logger.Warn("GetAttackTarget AiId={0} Config Error!! HatreType={1} , SortType={2}", npc.TableAI.Id,
                        npc.TableAI.HatreType, npc.TableAI.SortType);
                    return null;
                case 1: //距离的

                    if (npc.TableAI.SortType == 0)
                    {
                        return npc.GetMaxDistanceEnemy();
                    }
                    if (npc.TableAI.SortType == 1)
                    {
                        return npc.GetMinDistanceEnemy();
                    }
                    Logger.Warn("GetAttackTarget AiId={0} Config Error!! HatreType={1} , SortType={2}", npc.TableAI.Id,
                        npc.TableAI.HatreType, npc.TableAI.SortType);
                    return null;
                case 2: //按血量

                    if (npc.TableAI.SortType == 0)
                    {
                        return npc.GetMaxHpNow();
                    }
                    if (npc.TableAI.SortType == 1)
                    {
                        return npc.GetMinHpNow();
                    }
                    Logger.Warn("GetAttackTarget AiId={0} Config Error!! HatreType={1} , SortType={2}", npc.TableAI.Id,
                        npc.TableAI.HatreType, npc.TableAI.SortType);
                    return null;
                case 3: //按职业
                {
                    switch (npc.TableAI.SortType)
                    {
                        case 2: //战法猎
                            return npc.GetCharacterByRole(NPCScriptBase.tempTypeList2);
                        case 3:
                            return npc.GetCharacterByRole(NPCScriptBase.tempTypeList3);
                        case 4:
                            return npc.GetCharacterByRole(NPCScriptBase.tempTypeList4);
                        case 5:
                            return npc.GetCharacterByRole(NPCScriptBase.tempTypeList5);
                        case 6:
                            return npc.GetCharacterByRole(NPCScriptBase.tempTypeList6);
                        case 7:
                            return npc.GetCharacterByRole(NPCScriptBase.tempTypeList7);
                    }
                }
                    Logger.Warn("GetAttackTarget AiId={0} Config Error!! HatreType={1} , SortType={2}", npc.TableAI.Id,
                        npc.TableAI.HatreType, npc.TableAI.SortType);
                    return null;
            }
            return null;
        }

        public void ChangeAi(NPCScriptBase _this, ObjNPC npc, int nextId)
        {
            if (nextId == -1)
            {
                return;
            }
            var tbAI = Table.GetAI(nextId);
            if (tbAI == null)
            {
                return;
            }
            npc.TableAI = tbAI;
            Logger.Info("guid={0},sceneid={1}", npc.Scene.Guid, npc.Scene.TypeId);

            SceneServer.Instance.ServerControl.ObjSpeak(npc.EnumAllVisiblePlayerIdExclude(), npc.ObjId, tbAI.EnterSpeak,
                string.Empty);
        }
    }

    public class NPCScriptBase
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();
        private static INPCScriptBase mImpl;
        //获得目标
        public static List<int> tempTypeList2 = new List<int> {0, 1, 2}; //战法猎
        public static List<int> tempTypeList3 = new List<int> {0, 2, 1}; //战猎法
        public static List<int> tempTypeList4 = new List<int> {1, 0, 2}; //法战猎
        public static List<int> tempTypeList5 = new List<int> {1, 2, 0}; //法猎战
        public static List<int> tempTypeList6 = new List<int> {2, 0, 1}; //猎战法
        public static List<int> tempTypeList7 = new List<int> {2, 1, 0}; //猎法战

        static NPCScriptBase()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (NPCScriptBase), typeof (NPCScriptBaseDefaultImpl),
                o => { mImpl = (INPCScriptBase) o; });
        }

		public virtual void Init(ObjNPC npc)
		{
			mImpl.Init(this, npc);
		}

        public void ChangeAi(ObjNPC npc, int nextId)
        {
            mImpl.ChangeAi(this, npc, nextId);
        }

		public virtual ObjCharacter GetAttackTarget(ObjNPC npc)
        {
            return mImpl.GetAttackTarget(this, npc);
        }

        //是否是强制Tick(在该怪所在Zone没有玩家的情况依然Tick自己的AI)
        public virtual bool IsForceTick()
        {
            return false;
        }

        //被击
        public virtual void OnDamage(ObjNPC npc, ObjCharacter enemy, int damage)
        {
        }

        //死亡
        public virtual void OnDie(ObjNPC npc, ulong characterId, int viewTime, int damage)
        {
        }

        //消失
        public virtual void OnDisapeare(ObjNPC npc)
        {
        }

        //当我的一个敌人死亡
        public virtual void OnEnemyDie(ObjNPC npc, ObjCharacter enemy)
        {
        }

        //战斗
        public virtual void OnEnterCombat(ObjNPC npc)
        {
        }

        //死亡
        public virtual void OnEnterDie(ObjNPC npc)
        {
        }

        //回家
        public virtual void OnEnterGoHome(ObjNPC npc)
        {
            mImpl.OnEnterGoHome(this, npc);
        }

        //休闲
        public virtual void OnEnterIdle(ObjNPC npc)
        {
        }

        //进入场景
        public virtual void OnEnterScene(ObjNPC npc)
        {
        }

        public virtual void OnExitCombat(ObjNPC npc)
        {
        }

        public virtual void OnExitDie(ObjNPC npc)
        {
        }

        public virtual void OnExitGoHome(ObjNPC npc)
        {
        }

        public virtual void OnExitIdle(ObjNPC npc)
        {
        }

        //离开场景
        public virtual void OnLeaveScene(ObjNPC npc)
        {
        }

        //重生
        public virtual void OnRelive(ObjNPC npc)
        {
        }

        //刷出
        public virtual void OnRespawn(ObjNPC npc)
        {
        }

        public virtual void OnTickCombat(ObjNPC npc, float delta)
        {
        }

        public virtual void OnTickDie(ObjNPC npc, float delta)
        {
        }

        public virtual void OnTickGoHome(ObjNPC npc, float delta)
        {
        }

        public virtual void OnTickIdle(ObjNPC npc, float delta)
        {
        }
    }
}