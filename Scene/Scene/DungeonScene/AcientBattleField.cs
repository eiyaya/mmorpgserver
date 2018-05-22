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


    public class AcientBattleField : Scene
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

        static AcientBattleField()
        {
			PlayerEnterCost = Table.GetServerConfig(940).ToInt();
			PlayerIdleCost = Table.GetServerConfig(939).ToInt();
			PlayerDieCost = Table.GetServerConfig(938).ToInt();
			
        }

        #endregion

	    private const int ExdataIdx = (int) eExdataDefine.e632;
	    private static readonly int PlayerEnterCost = 0;
		private static readonly int PlayerDieCost = 0;
		private static readonly int PlayerIdleCost = 0;
        public int id = 0;
		private const int TickSecondInterval = 1000;
        public int BossScene = 0;
        public int isDie { get; set; }//0:未死亡     1：死亡
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region 数据
        // 体力
        private Dictionary<ulong, int> TiliDic = new Dictionary<ulong, int>();

        // 倒计时剔出状态
        private Dictionary<ulong, TimerState> StateDic = new Dictionary<ulong, TimerState>();

        // 玩家在副本中存在的累计时间
        private Dictionary<ulong, DateTime> StayTimeDic = new Dictionary<ulong, DateTime>();

        private Trigger mTrigger = null;

        #endregion

        #region 重写父类方法
        public override void OnCreate()
        {
            base.OnCreate();
            mTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now, LogicTick, TickSecondInterval);
        }
        public override void ExitDungeon(ObjPlayer player)
        {
            if (player == null) return;

            player.ExitDungeon();
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
            if (player == null) return;

			PetIslandReduceTili(player.ObjId, ExdataIdx, PlayerDieCost);
	        var obj = FindCharacter(characterId);
	        var killer = obj.GetRewardOwner();
	        if (killer.GetObjType()!= ObjType.PLAYER)
	        {
		        return;
	        }
			SceneServer.Instance.ServerControl.BroadcastSceneChat(EnumAllPlayerId(), killer.GetName() + "|" + player.GetName(), 272000);
        }
        public override void OnNpcRespawn(ObjNPC npc)
        {
            base.OnNpcRespawn(npc);
            bool bSend = false;
            Table.ForeachAcientBattleField(tb =>
            {
                id = tb.Id;
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
           
            ObjCharacter killer = null;
            if (npc.TableNpc.BelongType == 1)//古域战场，如果是队内伤害最高，就扣他的体力因为他会得宝物
            {
                 killer = FindCharacter(npc.GetTargetCharacterId());//古域战场Boss，是谁获得了奖励扣谁的体力
             }else{
                 killer = FindCharacter(characterId);//古域战场小怪，是谁获得了奖励扣谁的体力
             }
			if (killer == null)
			{
				return;
			}

            base.OnNpcDie(npc, characterId);
            var tbNpc = Table.GetNpcBase(npc.TypeId);
            if (tbNpc != null)
            {
                var player = killer as ObjPlayer;
                if (null != player)
                {
                    PetIslandReduceTili(player.ObjId, tbNpc.KillExpendType, tbNpc.KillExpendValue);
                }
            
            }



            bool bSend = false;
            Table.ForeachAcientBattleField(tb =>
            {
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
			CoroutineFactory.NewCoroutine(GetPlayerTili, player.ObjId, PlayerEnterCost).MoveNext();

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
            RemoveRecord(player.ObjId);
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
            if (obj == null || idx != ExdataIdx) return;
            base.OnPlayerExDataChanged(obj, idx, val);

            if (TiliDic.ContainsKey(obj.ObjId))
            {
                TiliDic[obj.ObjId] = val;
            }

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

        private void PetIslandReduceTili(ulong playerObjId, int exdataId, int num)
        {
	        if (-1 == exdataId || num <= 0)
	        {
		        return;
	        }
            CoroutineFactory.NewCoroutine(PetIslandReduceTili, playerObjId, exdataId, num).MoveNext();
        }

        private IEnumerator PetIslandReduceTili(Coroutine co, ulong playerObjId, int exdataId, int num)
        {
            if (exdataId != ExdataIdx)
            {
                Logger.Error("PetIslandReduceTili ExData table error not 630");
                yield break;
            }

            var refreshTili = 0;
            if (!TiliDic.TryGetValue(playerObjId, out refreshTili))
            {
                yield break;
            }

            var oldvalue = refreshTili;

            refreshTili -= num;

            if (refreshTili <= 0)
            {
                // 设置剔出状态 准备剔出
                if (StateDic.ContainsKey(playerObjId))
                {
                    if (StateDic[playerObjId].state != 0)
                    {
                        StateDic[playerObjId].state = 0;
                        StateDic[playerObjId].time = DateTime.Now.AddSeconds(10); 
                    }
                }
            }
            refreshTili = Math.Max(refreshTili, 0);
            TiliDic[playerObjId] = refreshTili;

            var data = new Dict_int_int_Data();
            if (refreshTili <= 0)
            {
                data.Data.Add(ExdataIdx, refreshTili - oldvalue); 
            }
            else
            {
                data.Data.Add(ExdataIdx, -num); 
            }

            var msg1 = SceneServer.Instance.LogicAgent.SSChangeExdata(playerObjId, data);
            yield return msg1.SendAndWaitUntilDone(co);
        }

        private IEnumerator GetPlayerTili(Coroutine co, ulong objId,int reduce = 0)
        {
            var ids = new Int32Array();
            ids.Items.Add(ExdataIdx);
            var msg = SceneServer.Instance.LogicAgent.SSFetchExdata(objId, ids);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply)
            {
                Logger.Error("PetIslandReduceTili SSFetchExdata return with state = {0}", msg.State);
                yield break;
            }
            if (msg.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("PetIslandReduceTili SSFetchExdata return with err = {0}", msg.ErrorCode);
                yield break;
            }
            if (msg.Response.Items.Count <= 0)
            {
                Logger.Error("PetIslandReduceTili SSFetchExdata return with err = {0}", msg.ErrorCode);
                yield break;
            }

            var outValue = 0;
            if (TiliDic.TryGetValue(objId, out outValue))
            {
                TiliDic[objId] = msg.Response.Items[0];
            }
            else
            {
                TiliDic.Add(objId, msg.Response.Items[0]); 
            }

            if (msg.Response.Items[0] > 0)
            {
                if (StateDic.ContainsKey(objId))
                {
                    StateDic[objId].state = 1;
                    StateDic[objId].time = DateTime.MaxValue;
                }
            }
	        if (reduce > 0)
	        {
		        PetIslandReduceTili(objId, ExdataIdx, reduce);
	        }
        }

        private void RemoveRecord(ulong characterId)
        {
            if (TiliDic.ContainsKey(characterId))
            {
                TiliDic.Remove(characterId);
            }

            if (StateDic.ContainsKey(characterId))
            {
                StateDic.Remove(characterId);
            }

            if (StayTimeDic.ContainsKey(characterId))
            {
                StayTimeDic.Remove(characterId);
            }
        }

        private void LogicTick()
        {
            // 每5分钟消耗一点体力
            foreach (var value in StayTimeDic)
            {
                TimeSpan delta = DateTime.Now - StayTimeDic[value.Key];
				if (delta.TotalSeconds > 1 && Math.Abs((int)delta.TotalSeconds % PlayerIdleCost) < 0.001f)
                {
                    PetIslandReduceTili(value.Key, ExdataIdx, 1);
                }
            }

            DynamicActivityRecord tbDynamic = Table.GetDynamicActivity(16);
            if (tbDynamic != null)
            {
                if (!Utils.CheckIsWeekLoopOk(tbDynamic.WeekLoop))
                {
                    foreach (var data in StateDic)
                    {
                        var character = FindCharacter(data.Key);
                        if (character != null)
                        {
                            //把玩家踢出去
                            var player = character as ObjPlayer;
                            if (player != null)
                            {
                                CoroutineFactory.NewCoroutine(player.ExitDungeon).MoveNext();
                            }
                        }
                    }
                    return;
                }
            }


            var tempRemove = new List<ulong>();
            tempRemove.Clear();
            foreach (var data in StateDic)
            {
                if (data.Value != null && data.Value.state == 0 && data.Value.time <= DateTime.Now)
                {
                    var character = FindCharacter(data.Key);
                    if (character != null)
                    {
                        //把玩家踢出去
                        var player = character as ObjPlayer;
                        if (player != null)
                        {
                            if (TiliDic.ContainsKey(data.Key))
                            {
                                if (TiliDic[data.Key] <= 0)
                                {
                                    CoroutineFactory.NewCoroutine(player.ExitDungeon).MoveNext();
                                    tempRemove.Add(data.Key);
                                    //RemoveRecord(data.Key);  迭代器失效
                                }
                                else
                                {
                                    if (StateDic.ContainsKey(data.Key))
                                    {
                                        StateDic[data.Key].state = 1;
                                        StateDic[data.Key].time = DateTime.MaxValue;
                                    } 
                                }
                            }
                        }
                    }
                }
            }

            foreach (var data in tempRemove)
            {
                if (TiliDic.ContainsKey(data))
                {
                    RemoveRecord(data);
                }
            }
        }
        private IEnumerator GetNPCDie(Coroutine co, ulong objId, int npcid = 0)
        {
            var msg = SceneServer.Instance.ActivityAgent.SSAcientBattleSceneRequest(objId, this.ServerId, BossScene, npcid, isDie);
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
        #endregion
    }
}