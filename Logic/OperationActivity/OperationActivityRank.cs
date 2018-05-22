#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using EventSystem;
using LogicServerService;
using NLog;
using Shared;
using Scorpion;

#endregion

namespace Logic
{
	public class OperationActivityRank : OperationActivity
	{
	    public const double ScoreDelayMinutes = 5.0; //固定结算延迟时间，注意这个时间一定要等待排行榜备份完之后 ServerRankBackupManagerDefaultImpl.BackupAllRank

        public RankType RankType = RankType.FightValue;
		public override OperationActivityType Type
		{
			get { return OperationActivityType.Rank; }
		}
		private bool hasRequested = false;

		public bool HasResult
		{
			get { return DictRankData.Count > 0; }
		}
		public Dictionary<int,MsgRankList> DictRankData = new Dictionary<int, MsgRankList>();
		private List<Trigger> mTriggers = new List<Trigger>();
		public override void Init()
		{
			base.Init();
			Cleanup();
			if (0 == ServerList.Count)
			{
				Table.ForeachServerName((tb) =>
				{
					ServerList.Add(tb.Id);
					return true;
				});	
			}

			if (eRechargeActivityOpenRule.NewServerAuto == OpenRule)
			{
				foreach (var id in ServerList)
				{

					var time = DateTime.Parse(Table.GetServerName(id).OpenTime).Date.AddHours(ScoreDelayHours);
					var list = new List<int>();
					list.Add(id);

                    //实际计算时间往后推延个ScoreDelayMinutes
                    var trigger = LogicServerControl.Timer.CreateTrigger(time.AddMinutes(ScoreDelayMinutes), () =>
					{
						CoroutineFactory.NewCoroutine(GetRankData, list, time.Date).MoveNext();
					});

					mTriggers.Add(trigger);
				}
			}

            //现在Reload ServerName，会重刷一边运营活动
            //EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
        }
        /*
        private void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v == null)
                return;
            if (v.tableName == "ServerName")
            {
                Table.ForeachServerName(record =>
                {
                    if (record.IsClientDisplay == 1 && ServerList.FindIndex(r=>r == record.Id) < 0)
                    {
                        ServerList.Add(record.Id);
                        if (eRechargeActivityOpenRule.NewServerAuto == OpenRule)
                        {
                            var time = DateTime.Parse(record.OpenTime).Date.AddHours(ScoreDelayHours);
                            var list = new List<int>();
                            list.Add(record.Id);
                            var trigger = LogicServerControl.Timer.CreateTrigger(time, () =>
                            {
                                CoroutineFactory.NewCoroutine(GetRankData, list, time.Date).MoveNext();
                            });
                            mTriggers.Add(trigger);
                        }
                    }
                    return true;
                });
            }
        }
        */
		protected override void OnStart()
		{

		}

		protected override void OnEnd()
		{
		}

		public override void Update()
		{
			if (hasRequested)
			{
				return;
			}

		    //实际计算时间往后推延个ScoreDelayMinutes
            if ((eRechargeActivityOpenRule.LimitTime == OpenRule || eRechargeActivityOpenRule.Last == OpenRule)
				&& DateTime.Now >= ScoreTime.AddMinutes(ScoreDelayMinutes))
			{
				hasRequested = true;
				CoroutineFactory.NewCoroutine(GetRankData, ServerList,ScoreTime.Date).MoveNext();
			}
		}

		protected override void OnDestroy()
		{
			Cleanup();
		}

		private void Cleanup()
		{
			foreach (var trigger in mTriggers)
			{
				LogicServerControl.Timer.DeleteTrigger(trigger);
			}
			mTriggers.Clear();
			DictRankData.Clear();
			hasRequested = false;
			ServerList.Clear();
		}
		private IEnumerator GetRankData(Coroutine co,List<int> serverList,DateTime time)
		{
			Int32Array a = new Int32Array();
			a.Items.AddRange(serverList);
			var msg = LogicServer.Instance.RankAgent.SSGetRankDataByServerId(0, a, time.Date.ToBinary(), (int)RankType);
			yield return msg.SendAndWaitUntilDone(co);
			if (msg.State != MessageState.Reply)
			{
				//Logger.("GetRankData(). SSGetRankDataByServerId Failed with state = {0}", msg.State);
				yield break;
			}
			foreach (var data in msg.Response.Data)
			{
				if (DictRankData.ContainsKey(data.ServerId))
				{
					DictRankData[data.ServerId] = data;
				}
				else
				{
					DictRankData.Add(data.ServerId, data);
				}
				
			}
			if (DictRankData.Count > 0)
			{
				OperationActivityManager.Instance.MarkAllCharacterDirty(serverList);						
			}
		}
	}
}