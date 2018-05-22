#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DataContract;
using DataTable;
using Scorpion;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene
{   
    public class BossHome : Scene
    {
		public class TimerState
		{
			public TimerState(int State)
			{
				state = State;
				time = DateTime.MaxValue;
			}
			public int state { get; set; }
			public DateTime time { get; set; }
		}

        #region 刷新表格

        static BossHome()
        {
   
        }

        #endregion

        //private const int ExdataIdx = (int)eDeleteItemType.UseItem;
        private int PlayerEnterCost = 0;
        public uint sceneID = 0;
        public int isDie { get; set; }//0:未死亡     1：死亡
        public int id = 0;//bosshome之家表格的怪物ID
        public int BossScene = 0;
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private Dictionary<int,int> dicBoss = new Dictionary<int, int>();
        public List<Vector2> RelivePos = new List<Vector2>();
        #region 数据

        // 倒计时剔出状态
        private Dictionary<ulong, TimerState> StateDic = new Dictionary<ulong, TimerState>();

        // 玩家在副本中存在的累计时间
        private Dictionary<ulong, DateTime> StayTimeDic = new Dictionary<ulong, DateTime>();

        private Trigger mTrigger = null;

        //每个玩家的复活时间
        private readonly Dictionary<ulong, int> PlayerReliveTime = new Dictionary<ulong, int>();

        #endregion

        #region 重写父类方法
        public override void OnCreate()
        {
            base.OnCreate();
            foreach (var v in this.MapNpcInfo.Data)
            {
                var tb = Table.GetMapTransfer(v.TableId);
                if (tb != null)
                {
                    v.Alive = tb.NpcID < 0 ;//给传送门做了个特例                    
                }
            }
            var tbFuben = Table.GetFuben(this.TableSceneData.FubenId);
            if (tbFuben != null)
            {
                for (int i = 0; i < tbFuben.lParam1.Count; i++)
                {
                    var tbPos = Table.GetRandomCoordinate(tbFuben.lParam1[i]);
                    if (tbPos != null)
                    {
                        RelivePos.Add(new Vector2(tbPos.PosX,tbPos.PosY));
                    }
                }
            }

//            RelivePos
        }
        public override void ExitDungeon(ObjPlayer player)
        {
            if (player == null) return;

            player.ExitDungeon();
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
            if (player == null) return;
		
	        var obj = FindCharacter(characterId);
	        var killer = obj.GetRewardOwner();
	        if (killer.GetObjType()!= ObjType.PLAYER)
	        {
		        return;
	        }
			SceneServer.Instance.ServerControl.BroadcastSceneChat(EnumAllPlayerId(), killer.GetName() + "|" + player.GetName(), 272000);
        }
        public void BeforPlayerRelive(ObjPlayer player)
        {
            player.SetPosition(RelivePos.Range());
        }
        public override void OnObjBeforeEnterScene(ObjBase obj)
        {
            var player = obj as ObjPlayer;
            if (null == player)
            {
                return;
            }

            if (!PlayerReliveTime.ContainsKey(player.ObjId))
            {
                PlayerReliveTime.Add(player.ObjId, StaticVariable.ReliveTimeInit);
            }
        }

        //private void PlayerRelive(ObjPlayer player)
        //{
        //    var campId = player.GetCamp();
        //    player.SetPosition(59f,16f);
        //    player.Relive();
        //}
        public override void OnNpcEnter(ObjNPC npc)
        {
            base.OnNpcEnter(npc);
            bool bSend = false;
            Table.ForeachBossHome(tb =>
            {
                id = tb.Id;
                BossScene = tb.Scene;
                if (tb.CharacterBaseId == npc.TypeId)
                {
                    bSend = true;
                    return false;
                }
                return true;
            });
            if (bSend == true)
            {
                var idx = MapNpcRecords.FindIndex(r => r.NpcID == npc.TypeId);
                if (idx == -1)
                {
                    return;
                }
                var info = MapNpcInfo.Data[idx];
                info.Alive = true;
                var data = new MapNpcInfos();
                data.Data.Add(info);
                PushActionToAllPlayer(p =>
                {
                    if (p.Proxy == null)
                    {
                        return;
                    }
                    p.Proxy.NotifyNpcStatus(data);
                });
            }
        }
        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            if (npc == null) return;
            base.OnNpcDie(npc, characterId);
            bool bSend = false;
            Table.ForeachBossHome(tb =>
            {
                id = tb.Id;
                isDie = 0;
                BossScene = tb.Scene;
                if (tb.CharacterBaseId == npc.TypeId)
                {
                    bSend = true;
                    isDie = 1;
                    dicBoss.Remove(tb.SceneNpcId);
                    return false;
                }   
                return true;
            });
            if (bSend == true)
            {
                var idx = MapNpcRecords.FindIndex(r => r.NpcID == npc.TypeId);
                if (idx == -1)
                {
                    return;
                }
                var info = MapNpcInfo.Data[idx];
                info.Alive = false;
                var data = new MapNpcInfos();
                data.Data.Add(info);
                PushActionToAllPlayer(p =>
                {
                    if (p.Proxy == null)
                    {
                        return;
                    }
                    p.Proxy.NotifyNpcStatus(data);
                });              
            }
            CoroutineFactory.NewCoroutine(GetNPCDie, (ulong)0, npc.TypeId).MoveNext();
        }

        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            if (player == null) return;
            base.OnPlayerEnterOver(player);

            //sceneID = player.GetData().SceneId;
            
            //if(sceneID == 22000)
            //{
            //     PlayerEnterCost = Table.GetServerConfig(3003).ToInt();
            //}
            //else if(sceneID == 22001)
            //{
            //    PlayerEnterCost = Table.GetServerConfig(3004).ToInt();
            //}

            //CoroutineFactory.NewCoroutine(GetPlayerDia, player.ObjId, PlayerEnterCost).MoveNext();
            MapTransferRecord MapTransfer = new MapTransferRecord();            
            TimerState outValue = null;
            if (StateDic.TryGetValue(player.ObjId, out outValue))
            {
                StateDic[player.ObjId].state = 1;
                StateDic[player.ObjId].time = DateTime.MaxValue;
            }
            else
            {
                StateDic.Add(player.ObjId, new TimerState(1));
            }

            DateTime outData;
            if (StayTimeDic.TryGetValue(player.ObjId, out outData))
            {
                StayTimeDic[player.ObjId] = DateTime.Now;
            }
            else
            {
                StayTimeDic.Add(player.ObjId, DateTime.Now);
            }

        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            if (player == null) return;

            base.OnPlayerLeave(player);
            //RemoveRecord(player.ObjId);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (null != mTrigger)
            {
                SceneServerControl.Timer.DeleteTrigger(mTrigger);
                mTrigger = null;
            }
        }

        public override void OnPlayerExDataChanged(ObjPlayer obj, int idx, int val)
        {
            if (obj == null ) return;
            base.OnPlayerExDataChanged(obj, idx, val);

            if (val > 0)
            {
                if (StateDic.ContainsKey(obj.ObjId))
                {
                    StateDic[obj.ObjId].state = 1;
                    StateDic[obj.ObjId].time = DateTime.MaxValue;
                }
            }
            else
            {
                // 设置剔出状态 准备剔出
                if (StateDic.ContainsKey(obj.ObjId))
                {
                    if (StateDic[obj.ObjId].state != 0)
                    {
                        StateDic[obj.ObjId].state = 0;
                        if (StateDic[obj.ObjId].time == DateTime.MaxValue)
                        {
                            StateDic[obj.ObjId].time = DateTime.Now.AddSeconds(10);
                        }
                    }
                }
            }
        }

        #endregion

        #region 内部逻辑

        private IEnumerator GetPlayerDia(Coroutine co, ulong objId,int reduce = 0)
        {
            var msg = SceneServer.Instance.LogicAgent.DeleteItem(objId, 3, PlayerEnterCost, (int)eDeleteItemType.UseItem);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply)
            {
                Logger.Error("GetPlayerDia DeleteItem return with state = {0}", msg.State);
                yield break;
            }
            if (msg.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("GetPlayerDia DeleteItem return with err = {0}", msg.ErrorCode);
                yield break;
            }         
        }
        private IEnumerator GetNPCDie(Coroutine co, ulong objId, int npcid = 0)
        {
            var msg = SceneServer.Instance.ActivityAgent.BossHomeSceneRequest(objId, this.ServerId, BossScene, npcid, isDie);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply)
            {
                Logger.Error("GetNPCDie BossHomeSceneRequest return with state = {0}", msg.State);
                yield break;
            }
            if (msg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("GetNPCDie BossHomeSceneRequest return with err = {0}", msg.ErrorCode);
                yield break;
            }               
        }

        public void RefreshBoss(int SceneNpcId)
        {
            if (dicBoss.ContainsKey(SceneNpcId))
                return;
            var npc = CreateSceneNpc(SceneNpcId);
            if (npc == null)
                return;
            dicBoss.Add(SceneNpcId,0);
            {
                if (mTrigger == null)
                    mTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddHours(1), () =>
                    {
                        KillBoss();
                    });
            }
        }

        public void KillBoss()
        {

            List<ObjNPC> l = new List<ObjNPC>();
            PushActionToAllObj(obj =>
            {

                if (obj.GetObjType() != ObjType.NPC)
                {
                    return;
                }

                ObjNPC npc = obj as ObjNPC;
                if (npc != null && npc.GetCamp() == 2)
                {
                    l.Add(npc);
                }
            });

            foreach (var npc in l)
            {
                npc.mDropOnDie = false;
                npc.Die(0, 0);
            }

            mTrigger = null;
        }
        #endregion
    }
}