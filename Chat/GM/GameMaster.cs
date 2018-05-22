#region using

using System;
using System.Collections;
using DataContract;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Chat
{
    public interface IGameMaster
    {
        IEnumerator GmCommand(Coroutine co, string command, AsyncReturnValue<ErrorCodes> err);
        void ReloadTable(string tableName);
        IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName);
        IEnumerator SystemChat(Coroutine coroutine, string strContent);
    }

    public class GameMasterDefaultImpl : IGameMaster
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerator SystemChat(Coroutine coroutine, string strContent)
        {
            foreach (var proxy in ChatServer.Instance.ServerControl.Proxys)
            {
                var chatProxy = proxy.Value as ChatProxy;
                if (chatProxy != null)
                {
                    chatProxy.SyncChatMessage((int) eChatChannel.System, 0, "",
                        new ChatMessageContent {Content = strContent});
                }
            }

            //TODO
            //向其他服务器发送广播命令
            yield break;
        }

        //重载表格
        public void ReloadTable(string tableName)
        {
            //Table.ReloadTable(tableName);
            CoroutineFactory.NewCoroutine(ReloadTableCoroutine, tableName).MoveNext();
        }

        public IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName)
        {
			var Reloadtable = ChatServer.Instance.ChatAgent.ServerGMCommand("ReloadTable",tableName);
            yield return Reloadtable.SendAndWaitUntilDone(coroutine);
        }

        public IEnumerator GmCommand(Coroutine co, string command, AsyncReturnValue<ErrorCodes> err)
        {
            err.Value = ErrorCodes.OK;

            var strs = command.Split(',');
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
            else if (String.Compare(strs[0], "!!SystemChat", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (strs.Length != 2)
                {
                    err.Value = ErrorCodes.ParamError;
                    yield break;
                }
                var gmCo = CoroutineFactory.NewSubroutine(GameMaster.SystemChat, co, strs[1]);
                if (gmCo.MoveNext())
                {
                    yield return gmCo;
                }
            }
			else if (String.Compare(strs[0], "!!UpdateAnchor", StringComparison.OrdinalIgnoreCase) == 0)
			{
				AnchorManager.Instance.LoadConfig();
				yield break;
			}
        }
    }

    public static class GameMaster
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IGameMaster mImpl;

        static GameMaster()
        {
            ChatServer.Instance.UpdateManager.InitStaticImpl(typeof (GameMaster),
                typeof (GameMasterDefaultImpl),
                o => { mImpl = (IGameMaster) o; });
        }

        public static IEnumerator GmCommand(Coroutine co, string command, AsyncReturnValue<ErrorCodes> err)
        {
            return mImpl.GmCommand(co, command, err);
        }

        //重载表格
        public static void ReloadTable(string tableName)
        {
            mImpl.ReloadTable(tableName);
        }

        public static IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName)
        {
            return mImpl.ReloadTableCoroutine(coroutine, tableName);
        }

        public static IEnumerator SystemChat(Coroutine coroutine, string strContent)
        {
            return mImpl.SystemChat(coroutine, strContent);
        }
    }
}