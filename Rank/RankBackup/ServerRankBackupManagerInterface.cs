#region using

using System;
using System.Collections;
using System.Collections.Generic;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Rank
{
	public interface IServerRankBackupManager
	{
		void Init();

		IEnumerator BackupAllRankCoroutine(Coroutine coroutine, List<int> serverList, DateTime time);
		IEnumerator GetRank(Coroutine coroutine, List<int> serverId, long time, int type);

		void BackupAllRank(List<int> serverList, DateTime time);

		void Tick();
	}

	public static class ServerRankBackupManager
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public static Dictionary<string, DBRankBackupServer> DictRankDataCache = new Dictionary<string, DBRankBackupServer>();

		public static IServerRankBackupManager mImpl;

		public static readonly TimeSpan Span = new TimeSpan(0, 2, 20); //每日0点2分10秒备份一份
		public static readonly int Interval = 1000*60*60*24;//每天

		static ServerRankBackupManager()
		{
			RankServer.Instance.UpdateManager.InitStaticImpl(typeof(IServerRankBackupManager),
				typeof(ServerRankBackupManagerDefaultImpl),
				o => { mImpl = (IServerRankBackupManager)o; });
		}

		public static string FormatDateTimeToKey(DateTime time)
		{
			return time.Date.ToString("yyyyMMdd");
		}

		public static int MAXMember = 20;
		//初始化
		public static void Init()
		{
			mImpl.Init();
		}

		public static void BackupAllRank(List<int> serverList, DateTime time)
		{
			mImpl.BackupAllRank(serverList,time);
		}

		public static IEnumerator BackupAllRankCoroutine(Coroutine coroutine, List<int> serverList, DateTime time)
		{
			return mImpl.BackupAllRankCoroutine(coroutine, serverList, time);
		}

		public static IEnumerator GetRank(Coroutine coroutine, List<int> serverId, long time, int type)
		{
			return mImpl.GetRank(coroutine, serverId, time, type);
		}

		public static void Tick()
		{
			mImpl.Tick();
		}
	}
}