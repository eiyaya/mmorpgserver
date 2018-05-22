#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DataTable;

#endregion

namespace Scene
{
	//只攻击固定类型   Boss，可以放特殊技能和切换阶段
	//NpcBase表的ServerParam[0]配成 1000|10001，只攻击1000|10001两种类型的怪
	public class NPCScript250002 : NPCScript200001
	{
		private List<int> mAddList = new List<int>();

        private ObjCharacter mLastEnemy = null;

		private List<ulong> MyTargetObjList = new List<ulong>();

		private ObjCharacter GetCanAttack(ObjNPC npc)
		{
			if (mLastEnemy != null && mLastEnemy.Active && !mLastEnemy.IsDead() && npc.Zone.ObjDict.ContainsKey(mLastEnemy.ObjId))
			{
				return mLastEnemy;
			}

			if (MyTargetObjList.Count == 0)
			{
				foreach (var pair in npc.Scene.mObjDict)
				{
					if (!pair.Value.IsCharacter())
					{
						continue;
					}
					var t = pair.Value as ObjCharacter;

					if (t.IsDead() || !t.Active)
					{
						continue;
					}
					if (!mAddList.Contains(t.TypeId))
					{
						continue;
					}
					MyTargetObjList.Add(t.ObjId);
				}
			}


			var e = npc.Zone.EnumAllVisiblePlayer();
			foreach (var objPlayer in e)
			{
				if (!MyTargetObjList.Contains(objPlayer.ObjId))
				{
					MyTargetObjList.Add(objPlayer.ObjId);
				}
			}

			ObjCharacter temp = null;
			var maxDis = 9999999.0f;
			for (int i = 0; i < MyTargetObjList.Count; )
			{
				var objId = MyTargetObjList[i];
				var target = npc.Scene.FindObj(objId);
				if (null == target)
				{
					MyTargetObjList.RemoveAt(i);
					continue;
				}
				var enemy = target as ObjCharacter;
				if (enemy.IsDead() || !enemy.Active)
				{
					MyTargetObjList.RemoveAt(i);
					continue;
				}
				var dis = (npc.GetPosition() - enemy.GetPosition()).LengthSquared();
				if (dis < maxDis)
				{
					temp = enemy;
					maxDis = dis;
				}

				i++;
			}

			mLastEnemy = temp;

			return temp ?? mLastEnemy;
		}
		public override void Init(ObjNPC npc)
		{
			mAddList.Clear();
			var array = npc.TableNpc.ServerParam[0].Split('|');
			foreach (var item in array)
			{
				mAddList.Add(int.Parse(item));
			}
			if (mAddList.Count<=0)
			{
				string msg = string.Format("npc.TableNpc.ServerParam = {0}", npc.TableNpc.ServerParam[0]);
#if DEBUG
				Debug.Assert(false, msg);
#endif
				Logger.Fatal(msg);
			}
		}

        public override void OnTickGoHome(ObjNPC npc, float delta)
        {
			npc.EnterState(BehaviorState.Idle);
        }

        //心跳普通
        public override void OnTickIdle(ObjNPC npc, float delta)
        {
			var enemy = GetAttackTarget(npc);
			if (null != enemy)
			{
				npc.EnterState(BehaviorState.Combat);
			}
        }
    }
}