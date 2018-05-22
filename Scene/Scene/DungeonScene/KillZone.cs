#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class KillZone : DungeonScene
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region 静态初始化

		public static List<Vector2> RelivePos = new List<Vector2>();
		public static Dictionary<int,float> DictExpRatio = new Dictionary<int, float>();

		static KillZone()
        {
			var tbSU = Table.GetSkillUpgrading(340);
			foreach (var value in tbSU.Values)
			{
				var tbPos = Table.GetRandomCoordinate(value);
				var pos = new Vector2(tbPos.PosX, tbPos.PosY);
				RelivePos.Add(pos);
            }

			var str = Table.GetServerConfig(1203).Value;
			var vars = str.Split('|');
			for (int i = 0; i < 24; i++)
			{
				DictExpRatio.Add(i, 1);
			}
			foreach (var r in vars)
			{
				var temp = r.Split(',');
				if (2 != temp.Count())
				{
					continue;
				}
				var hour = int.Parse(temp[0]);
				var ratio = float.Parse(temp[1]);
				DictExpRatio[hour] = ratio;
			}

			
			
        }

        #endregion

		#region 常量

	    private const int AddExpSecondInterval = 5000;
        private const int MaxRank = 5;
		//箱子给的buff
		public static readonly int[] BoxBuffId = { 1202, 1203, 1204 };
		public static readonly int[] DieBuffId = { 1200, 1201 };
		//无敌buff
		public const int InvincibleBuffId = 1205;

	    private const int ParamIdx_TimeExpBase = 0;
		private const int ParamIdx_KillExpBase = ParamIdx_TimeExpBase + 1;
		private const int ParamIdx_KillGoldBase = ParamIdx_KillExpBase + 1;
		private const int ParamIdx_TimeExpRate = ParamIdx_KillGoldBase + 1;
		private const int ParamIdx_KillExpRate = ParamIdx_TimeExpRate + 1;
		private const int ParamIdx_Max = ParamIdx_KillExpRate + 1;
		#endregion

		#region 数据

	    public class KillRecord
	    {
		    public int ContinuesKill = 0;
			public int TotalKill = 0;
            public int KillNum = 0;
	    }

		public Dictionary<ulong, KillRecord> DictKillRecord = new Dictionary<ulong, KillRecord>();

		private Trigger mTrigger = null;
	    private bool Dirty = false;
	    private List<int> TableParam;
        private DungeonInfo Msg = null;
	    private float TimeExpRatio = 1.0f;
        #endregion

        #region 重写父类方法

        public override void OnCreate()
        {
            base.OnCreate();

	        var tbFuben = Table.GetFuben(TableSceneData.FubenId);
#if DEBUG
	        if (null == tbFuben)
	        {
		        Debug.Assert(false);
	        }
			if (ParamIdx_Max!=tbFuben.lParam1.Count)
			{
				Debug.Assert(false);
			}
#endif
			TableParam = tbFuben.lParam1;

            Msg = new DungeonInfo();
            Msg.Type = 0;
			
			mTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now, LogicTick, AddExpSecondInterval);

            if (tbFuben.OpenTime.Count >= 1)
            {
                var startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, tbFuben.OpenTime[0] / 100, tbFuben.OpenTime[0] % 100, 0);
                var endTime = startTime.AddMinutes(tbFuben.CanEnterTime);
                StartTimer(eDungeonTimerType.WaitEnd, endTime, FubenTimeOver);
            }                           
        }  

        private void FubenTimeOver()
        {
            PushActionToAllPlayer(proxyPlayer =>
            {
                if (!proxyPlayer.Active)
                {
                    return;
                }
                proxyPlayer.Proxy.NotifyBattleReminder(27, Utils.WrapDictionaryId(200005023), 1);
            });            
            EnterAutoClose(3);
        }


        //当Obj进入场景前，(Obj还没有进入场景，他的Scene是空)这里只可以写场景逻辑(例如改个坐标)，不可以同步数据
         public override void OnObjBeforeEnterScene(ObjBase obj)
         {
	         if (obj.GetObjType() == ObjType.PLAYER)
	         {
		         (obj as ObjPlayer).SetPosition(RelivePos.Range());
	         }
         }

//         public override void OnPlayerEnter(ObjPlayer player)
//         {
//             base.OnPlayerEnter(player);
//             
//         }

        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            base.OnPlayerEnterOver(player);
            Dirty = true;
            CoroutineFactory.NewCoroutine(ApplyPlayerExdata, player).MoveNext();
			player.SyncCharacterPostion();
        }
        public IEnumerator ApplyPlayerExdata(Coroutine coroutine,ObjPlayer player)
        {
            Int32Array array = new Int32Array();
            array.Items.Add((int)eExdataDefine.e770);
            var result = SceneServer.Instance.LogicAgent.SSFetchExdata(player.ObjId, array);
            yield return result.SendAndWaitUntilDone(coroutine);
            if (result.State != MessageState.Reply || result.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Warn("ApplyPlayerExdata");
                yield break;
            }
            var nCount = result.Response.Items[0];
            KillRecord record;
            if (false == DictKillRecord.TryGetValue(player.ObjId, out record))
            {
                record = new KillRecord();
                record.KillNum = player.GetExdata((int)eExdataDefine.e770);
                DictKillRecord.Add(player.ObjId,record);
            }
            record.KillNum = nCount;
            AssignMsg();
            PushActionToAllPlayer(proxyPlayer =>
            {
                if (!proxyPlayer.Active)
                {
                    return;
                }
                AssignSelfMsg(proxyPlayer.ObjId);
                // 向客户端刷新副本信息
                Msg.Int32OneList.Clear();
                Msg.FloatList.Clear();
                Msg.Int32OneList.Add(GetAlivePlayerNum());
                Msg.Int32OneList.Add(PlayerCount);
                Msg.FloatList.Add(TimeExpRatio);
                proxyPlayer.Proxy.NotifyRefreshDungeonInfo(Msg);
            });
            yield break;
        }
        public override void OnPlayerLeave(ObjPlayer player)
        {
            base.OnPlayerLeave(player);
	        RemoveRecord(player.ObjId);

            PushActionToAllPlayer(proxyPlayer =>
            {
                if (!proxyPlayer.Active)
                {
                    return;
                }

                // 向客户端刷新副本信息
                Msg.Int32OneList.Clear();
                Msg.Int32OneList.Add(GetAlivePlayerNum());
                Msg.Int32OneList.Add(PlayerCount);
                proxyPlayer.Proxy.NotifyRefreshDungeonInfo(Msg);
            });
        }

        public override void ExitDungeon(ObjPlayer player)
        {
            player.ExitDungeon();
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {

	        var killer = FindPlayer(characterId);
			if (null == killer)
			{
				var obj = FindCharacter(characterId);
				if (null != obj && obj.GetObjType() == ObjType.RETINUE)
				{
					var retinue = obj as ObjRetinue;
					if (null != retinue.Owner && retinue.Owner.GetObjType() == ObjType.PLAYER)
					{
						killer = retinue.Owner as ObjPlayer;	
					}
				}

				if (null == killer)
				{
					return;	
				}
			}
	        var oldKills = GetContinuesKillRecord(player.ObjId);
	        bool bfirst = false;
            if (Msg.Int64List.Count > 0 && Msg.Int64List[0] == player.ObjId && oldKills >= 10)
            {
		        bfirst = true;
	        }
			AddKillOrDeadRecord(player.ObjId, false);
			AddKillOrDeadRecord(killer.ObjId, true);
			Dirty = true;


			CreateDropItem(99, new List<ulong> { characterId }, 0uL, 2, CalculateKillerGold(TableParam[ParamIdx_KillGoldBase], player.GetLevel(), oldKills, killer.GetLevel()), player.GetPosition());

			GiveItem(characterId, 1, CalculateKillerExp(TableParam[ParamIdx_KillExpBase], player.GetLevel(), oldKills, killer.GetLevel(), TableParam[ParamIdx_KillExpRate]));

			//移除死亡给叠加的buff
			foreach (var buffId in DieBuffId)
			{
				var datas = killer.BuffList.GetBuffById(buffId);
				foreach (var data in datas)
				{
					MissBuff.DoEffect(this, killer, data);
					killer.DeleteBuff(data, eCleanBuffType.EffectOver);
				}
			}

	        AssignMsg();

			PushActionToAllPlayer(proxyPlayer =>
			{
				if (!proxyPlayer.Active)
				{
					return;
				}
                AssignSelfMsg(proxyPlayer.ObjId);
                proxyPlayer.Proxy.NotifyRefreshDungeonInfo(Msg);
			});

	        if (bfirst)
	        {
				SceneServer.Instance.ServerControl.BroadcastSceneChat(EnumAllPlayerId(), killer.GetName()+"|"+player.GetName(), 272005);    
	        }

	        var num = GetContinuesKillRecord(killer.ObjId);
	        if (3 == num)
	        {
				SceneServer.Instance.ServerControl.BroadcastSceneChat(EnumAllPlayerId(), killer.GetName() + "|" + player.GetName(), 272001);//[{0}]正在大杀特杀！
	        }
			else if (6 == num)
			{
				SceneServer.Instance.ServerControl.BroadcastSceneChat(EnumAllPlayerId(), killer.GetName() + "|" + player.GetName(), 272002);//[{0}]已经主宰比赛了！
			}
			else if (8 == num)
			{
				SceneServer.Instance.ServerControl.BroadcastSceneChat(EnumAllPlayerId(), killer.GetName() + "|" + player.GetName(), 272003);//[{0}]已经无人能挡！
			}
			else if (num>=10 && num%5==0)
			{
				SceneServer.Instance.ServerControl.BroadcastSceneChat(EnumAllPlayerId(), killer.GetName() + "|" + player.GetName(), 272004);//[{0}]已经超越神了！
			}

        }
        

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            base.OnNpcDie(npc, characterId);
			if (59000 == npc.TypeId)
	        {
		    	var killer = FindCharacter(characterId);
				if (killer == null)
				{
					return;
				}

				killer = killer.GetRewardOwner() as ObjPlayer;
				if (null != killer)
				{
					killer.AddBuff(BoxBuffId[MyRandom.Random(BoxBuffId.Length)], 1, killer);
				}
	        }

        }

		public override void OnPlayerRelive(ObjPlayer player, bool byItem)
	    {
			base.OnPlayerRelive(player, byItem);
			foreach (var buffId in DieBuffId)
			{
				player.AddBuff(buffId, 1, player);
			}
			if (!byItem)
			{
				//player.SetPosition(RelivePos.Range());
				//player.SyncCharacterPostion();
				player.AddBuff(InvincibleBuffId, 1, player);
			}

            PushActionToAllPlayer(proxyPlayer =>
            {
                if (!proxyPlayer.Active)
                {
                    return;
                }

                // 向客户端刷新副本信息
                Msg.Int32OneList.Clear();
                Msg.Int32OneList.Add(GetAlivePlayerNum());
                Msg.Int32OneList.Add(PlayerCount);
                proxyPlayer.Proxy.NotifyRefreshDungeonInfo(Msg);
            });
	    }

        public override void AutoRelive(ObjPlayer player)
        {
            // 不需要调用基类 
            if (player == null)
                return;

            player.SetPosition(RelivePos.Range());
            player.Relive();
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

        #endregion

		#region 内部方法

		private void AddKillOrDeadRecord(ulong characterId, bool kill)
		{
			KillRecord record = null;
			if (!DictKillRecord.TryGetValue(characterId,out record))
			{
				record = new KillRecord();
				DictKillRecord.Add(characterId, record);
			}

			var oldval = record.ContinuesKill;
            var character = FindCharacter(characterId);
			if (kill)
			{
				if (oldval >= 0)
				{
					oldval++;
				}
				else
				{
					oldval = 1;
				}
				record.ContinuesKill = oldval;
				record.TotalKill++;
                ObjPlayer me = character as ObjPlayer;
                if (me != null)
                {
                    var dict = new Dict_int_int_Data();
                    record.KillNum++;
                    dict.Data.Add((int)eExdataDefine.e770, 1);
                    me.SendExDataChange(dict);
                }
			}
			else
			{
				if (oldval >= 0)
				{
					oldval=-1;
				}
				else
				{
					oldval --;
				}
			}
			record.ContinuesKill = oldval;
		}
	    private int GetContinuesKillRecord(ulong characterId)
	    {
			KillRecord record = null;
			if (DictKillRecord.TryGetValue(characterId, out record))
			{
				return record.ContinuesKill;
			}
		    return 0;
	    }

		private void RemoveRecord(ulong characterId)
	    {
		    if (DictKillRecord.ContainsKey(characterId))
		    {
                DictKillRecord.Remove(characterId);
		    }
		    Dirty = true;
		    AssignMsg();
	    }

	    public void AssignMsg()
	    {
		    if (!Dirty)
		    {
			    return;
		    }
			Dirty = false;

            var items = from k in DictKillRecord.Keys
                        orderby (DictKillRecord[k].KillNum) descending
                        select k;
            Msg.Int64List.Clear();
            Msg.Int32TwoList.Clear();
            Msg.StringList.Clear();
            Msg.Int32OneList.Clear();
		    int idx = 0;
		    foreach (var k in items)
		    {
			    if (idx >= MaxRank)
			    {
				    break;
			    }
			    var val = DictKillRecord[k].ContinuesKill;
                var killnum = DictKillRecord[k].KillNum;
			    var player = FindPlayer(k);
			    if (null == player || player.GetObjType()!=ObjType.PLAYER)
			    {
				    continue;
			    }

                Msg.Int64List.Add(k);
                Msg.Int32TwoList.Add(Math.Max(killnum, 0));
                Msg.StringList.Add(player.GetName());
                Msg.Int32OneList.Add(GetAlivePlayerNum());
                Msg.Int32OneList.Add(PlayerCount);

			    idx++;
		    }
	    }
        public void AssignSelfMsg(ulong id)
        {
            if (Msg.Int64List.Contains(id))
                return;
            var items = from k in DictKillRecord.Keys
                        orderby (DictKillRecord[k].KillNum) descending
                        select k;
            foreach (var k in items)
            {
                if (id == k)
                {
                    var val = DictKillRecord[k].ContinuesKill;
                    var killnum = DictKillRecord[k].KillNum;
                    var player = FindPlayer(k);
                    if (null == player || player.GetObjType() != ObjType.PLAYER)
                    {
                        continue;
                    }
                    Msg.Int64List.Add(k);
                    Msg.Int32TwoList.Add(Math.Max(killnum, 0));
                    Msg.StringList.Add(player.GetName());
                }
            }
        }


	    private void GiveItem(ulong characterId, int itemId, int itemCount)
	    {
		    if (itemCount <= 0)
		    {
			    return;
		    }
			CoroutineFactory.NewCoroutine(GiveItemCoroutine, characterId, itemId, itemCount).MoveNext();
	    }

		private IEnumerator GiveItemCoroutine(Coroutine coroutine, ulong characterId, int itemId, int itemCount)
	    {
			var result = SceneServer.Instance.LogicAgent.GiveItem(characterId, itemId, itemCount,-1);
			yield return result.SendAndWaitUntilDone(coroutine);
			if (result.State != MessageState.Reply)
			{
				Logger.Warn("GiveItemCoroutine time out");
				yield break;
			}
	    }

		#endregion

		#region 逻辑

	    private void LogicTick()
	    {
		    var e = EnumAllPlayer();
		    var deadCount = 0;
		    var total = Math.Max(DictKillRecord.Count, 1);
		    foreach (var objPlayer in e)
		    {
			    if (objPlayer.Active && objPlayer.IsDead())
			    {
				    deadCount++;
			    }
		    }

            var exp = (int)(CalculateTimeExp(TableParam[ParamIdx_TimeExpBase], deadCount, total, TableParam[ParamIdx_TimeExpRate]) * 1.0f * DictExpRatio[DateTime.Now.Hour]);
			foreach (var objPlayer in e)
			{
			    if (objPlayer.Active)
			    {
                    if (!objPlayer.IsDead())
                    {
                        GiveItem(objPlayer.ObjId, 1, exp);
                    }
                    else
                    {
                        var expDead = (int)(TableParam[ParamIdx_TimeExpBase] * 1.0f * DictExpRatio[DateTime.Now.Hour]);
                        GiveItem(objPlayer.ObjId, 1, expDead);
                    }
			    }
			}

            if (TableParam[ParamIdx_TimeExpBase] != 0)
            {
                if (Math.Abs(TimeExpRatio - (float)exp / TableParam[ParamIdx_TimeExpBase]) > 0.001f)
                {
                    PushActionToAllPlayer(proxyPlayer =>
                    {
                        if (!proxyPlayer.Active)
                        {
                            return;
                        }

                        // 向客户端刷新副本信息
                        Msg.FloatList.Clear();

                        Msg.FloatList.Add((float)exp / TableParam[ParamIdx_TimeExpBase]);
                        proxyPlayer.Proxy.NotifyRefreshDungeonInfo(Msg);
                    });
                }
                TimeExpRatio = (float)exp / TableParam[ParamIdx_TimeExpBase];
            }
	    }

        #endregion

		#region 公式

	    public int CalculateTimeExp(int baseExp, int dead, int total, float rate)
	    {
	        var result = (int) (baseExp*(1.0f + dead*1.0f/total*rate));
	        result = Math.Max(result, 0);
			return result;
	    }
		public int CalculateKillerExp(int baseExp,int playerLvl, int deadTimes, int killerLvl,int rate)
		{
			var times = deadTimes >= 0 ? deadTimes : deadTimes*1.0f/4;
			var ret = (int)((baseExp + times * baseExp / 4) * (1 + (playerLvl - killerLvl) / rate));
			return Math.Max(ret, 100);
		}

		public int CalculateKillerGold(int baseExp,int playerLvl, int deadTimes, int killerLvl)
		{

			var ret = (int)(baseExp * MyRandom.Random(0.5f, 1.5f) + Math.Max(0, deadTimes) * baseExp / 3.0f);
			return Math.Max(ret, 0);
		}
		#endregion
	}
}