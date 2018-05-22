#region using

using System;
using System.Collections;
using System.Collections.Generic;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Rank
{
    public interface IGameMaster
    {
        IEnumerator GmCommand(Coroutine co, string commond, AsyncReturnValue<ErrorCodes> err);
        void LookRank();
        void ReloadTable(string tableName);
        IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName);
    }

    public class GameMasterDefaultImpl : IGameMaster
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void LookRank()
        {
            ServerRankManager.ShowLog();
        }

        public void ReloadTable(string tableName)
        {
            CoroutineFactory.NewCoroutine(ReloadTableCoroutine, tableName).MoveNext();
        }

        public IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName)
        {
			var Reloadtable = RankServer.Instance.RankAgent.ServerGMCommand("ReloadTable",tableName);
            yield return Reloadtable.SendAndWaitUntilDone(coroutine);
        }

        public IEnumerator GmCommand(Coroutine co, string commond, AsyncReturnValue<ErrorCodes> err)
        {
            err.Value = ErrorCodes.OK;

            var strs = commond.Split(',');
            if (strs.Length < 1)
            {
                err.Value = ErrorCodes.ParamError;
                yield break;
            }
            if (String.Compare(strs[0], "!!ReloadTable", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (strs.Length < 2)
                {
                    err.Value = ErrorCodes.ParamError;
                    yield break;
                }
                GameMaster.ReloadTable(strs[1]);
                yield break;
            }
            var nIndex = 0;
            var IntData = new List<int>();
            foreach (var s in strs)
            {
                if (nIndex != 0)
                {
                    int TempInt;
                    if (!Int32.TryParse(s, out TempInt))
                    {
                        err.Value = ErrorCodes.ParamError;
                        yield break;
                    }
                    IntData.Add(TempInt);
                }
                nIndex++;
            }
            if (String.Compare(strs[0], "!!LookRank", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 0)
                {
                    GameMaster.LookRank();
                }
            }
			else if (String.Compare(strs[0], "!!BackupRank", StringComparison.OrdinalIgnoreCase) == 0)
			{
				if (1 == strs.Length)
				{
					ServerRankBackupManager.BackupAllRank(null,DateTime.Now);
				}
				else
				{
					var serverList = new List<int>();
					for (int i = 1; i < strs.Length; i++)
					{
						int ret = -1;
						if (int.TryParse(strs[i], out ret))
						{
							serverList.Add(ret);
						}
					}
					ServerRankBackupManager.BackupAllRank(serverList,DateTime.Now);
				}
				
			}
        }
    }

    //Rank所有GM命令
    public static class GameMaster
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IGameMaster mImpl;

        static GameMaster()
        {
            RankServer.Instance.UpdateManager.InitStaticImpl(typeof (GameMaster), typeof (GameMasterDefaultImpl),
                o => { mImpl = (IGameMaster) o; });
        }

        public static IEnumerator GmCommand(Coroutine co, string commond, AsyncReturnValue<ErrorCodes> err)
        {
            return mImpl.GmCommand(co, commond, err);
        }

        public static void LookRank()
        {
            mImpl.LookRank();
        }

        public static void ReloadTable(string tableName)
        {
            mImpl.ReloadTable(tableName);
        }

        public static IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName)
        {
            return mImpl.ReloadTableCoroutine(coroutine, tableName);
        }
    }
}