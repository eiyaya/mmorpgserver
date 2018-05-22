#region using

using System;
using System.Collections;
using System.Collections.Generic;
using Database;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Rank
{



	public class ServerRankBackupManagerDefaultImpl : IServerRankBackupManager
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public void Init()
		{
// #if DEBUG
// 			var time = DateTime.Now.AddMinutes(1);
// 			RankServerControl.Timer.CreateTrigger(time, ServerRankBackupManager.BackupAllRank, 1000 * 60);
// #else
		var time = DateTime.Now.Date + ServerRankBackupManager.Span;
		RankServerControl.Timer.CreateTrigger(time,
			() => { ServerRankBackupManager.BackupAllRank(null,DateTime.Now.Date); }, //1月1号00：02分保存成1月1号的数据
			ServerRankBackupManager.Interval); //每24小时存储一次
//#endif
		}

		public void BackupAllRank(List<int> serverList, DateTime time)
		{
			CoroutineFactory.NewCoroutine(BackupAllRankCoroutine, serverList, time).MoveNext();
            CoroutineFactory.NewCoroutine(BackupTotalRankCoroutine).MoveNext();
		}

	    public IEnumerator BackupTotalRankCoroutine(Coroutine coroutine)
	    {
            var timeStr = ServerRankBackupManager.FormatDateTimeToKey(DateTime.Now);
			var rankMgr = ServerRankManager.TotalRank;

			DBRankBackupServer db = new DBRankBackupServer();
			db.ServerId = rankMgr.ServerId;

	        foreach (var rankMgrKV in rankMgr.rank)
	        {
	            DBRankList dbList = new DBRankList();
	            dbList.Type = rankMgrKV.Key;

	            var rank = rankMgrKV.Value;
	            int i = 0;
	            foreach (var charId in rank.RankUUIDList)
	            {
	                if (dbList.Items.Count >= ServerRankBackupManager.MAXMember)
	                {
	                    break;
	                }
	                DBRank_One one = null;
	                if (!rank.DBRankCache.TryGetValue(charId, out one))
	                {
	                    continue;
	                }

	                DBRankItem item = new DBRankItem();
	                item.CharacterId = one.Guid;
	                item.Name = one.Name;
	                item.Value = one.Value;
	                dbList.Items.Add(item);
	                i++;
	            }
	            db.List.Add(dbList);
	        }

	        var key = string.Format("{0}|{1}", timeStr, db.ServerId);

			Logger.Trace("BackupAllRankCoroutine-----begin[{0}]   key=[{1}]", DateTime.Now, key);

			var result = RankServer.Instance.DB.Set<DBRankBackupServer>(coroutine, DataCategory.RankBackup,
				key, db);

			yield return result;
			if (DataStatus.Ok != result.Status)
			{
				Logger.Fatal("RankServer.Instance.DB.Set  DataStatus.Ok != result.Status [{0}]", key);
			}
			Logger.Trace("BackupTotalRankCoroutine-----end[{0}] key=[{1}]", DateTime.Now, key);
	    }

		public IEnumerator BackupAllRankCoroutine(Coroutine coroutine, List<int> serverList, DateTime time)
		{
			Logger.Warn("BackupAllRankCoroutine[{0}]-------------------------begin", DateTime.Now);

			var timeStr = ServerRankBackupManager.FormatDateTimeToKey(time);

			foreach (var rankMgrkv in ServerRankManager.Ranks)
			{
				var rankMgr = rankMgrkv.Value;

				if (null != serverList && !serverList.Contains(rankMgr.ServerId))
				{
					continue;
				}

				DBRankBackupServer db = new DBRankBackupServer();
				db.ServerId = rankMgr.ServerId;
				
				foreach (var rankMgrKV in rankMgr.rank)
				{
					DBRankList dbList = new DBRankList();
					dbList.Type = rankMgrKV.Key;

					var rank = rankMgrKV.Value;
					int i = 0;
					foreach (var charId in rank.RankUUIDList)
					{
						if (dbList.Items.Count >= ServerRankBackupManager.MAXMember)
						{
							break;
						}
						DBRank_One one = null;
						if (!rank.DBRankCache.TryGetValue(charId, out one))
						{
							continue;
						}
						
						DBRankItem item = new DBRankItem();
						item.CharacterId = one.Guid;
						item.Name = one.Name;
						item.Value = one.Value;
						dbList.Items.Add(item);
						i++;
					}
					db.List.Add(dbList);
				}
				
				var key = string.Format("{0}|{1}", timeStr, db.ServerId);

				Logger.Trace("BackupAllRankCoroutine-----begin[{0}]   key=[{1}]", time, key);

				var result = RankServer.Instance.DB.Set<DBRankBackupServer>(coroutine, DataCategory.RankBackup,
					key, db);

				yield return result;
				if (DataStatus.Ok != result.Status)
				{
					Logger.Fatal("RankServer.Instance.DB.Set  DataStatus.Ok != result.Status [{0}]", key);
				}
				Logger.Trace("BackupAllRankCoroutine-----end[{0}] key=[{1}]", time, key);
			}
			Logger.Trace("BackupAllRankCoroutine[{0}]--------------------end", DateTime.Now);	
		}

		public IEnumerator GetRank(Coroutine coroutine, List<int> serverId, long time, int type)
		{
			/*
			string tempKey = "";

			var timeStr = FormatDateTimeToKey(DateTime.FromBinary(time));

			foreach (var sid in serverId)
			{
				tempKey = timeStr + "|" + sid.ToString();
				var result = RankServer.Instance.DB.Get<DBRankBackupServer>(coroutine, DataCategory.RankBackup, tempKey);

				yield return result;
				if (DataStatus.Ok != result.Status)
				{
					Logger.Fatal("RankServer.Instance.DB.Get  DataStatus.Ok != result.Status [{0}]", tempKey);
				}
				var data = result.Data;

				if (ServerRankBackupManager.DictRankDataCache.ContainsKey(tempKey))
				{
					ServerRankBackupManager.DictRankDataCache[tempKey] = data;
				}
				else
				{
					ServerRankBackupManager.DictRankDataCache.Add(tempKey, data);
				}
			}
			*/
			yield break;
		}

		public void Tick()
		{
			
		}
	}
}