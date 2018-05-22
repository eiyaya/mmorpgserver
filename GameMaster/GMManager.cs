#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Database;
using DataContract;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace GameMaster
{
    public static class GMManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static DBGMCharacters mDb;
        private static bool mDbDirty;

        public static void AddCommand(ulong guid, List<string> commands)
        {
            DBGmCommands cc;
            if (!mDb.Commands.TryGetValue(guid, out cc))
            {
                cc = new DBGmCommands();
                mDb.Commands[guid] = cc;
            }
            var now = DateTime.Now.ToBinary();
            foreach (var command in commands)
            {
                cc.Commands.Add(new DBGmCommand
                {
                    Command = command,
                    Time = now
                });
            }
            mDbDirty = true;
        }

        public static void DelCommand(ulong guid, List<string> commands)
        {
            DBGmCommands cc;
            if (!mDb.Commands.TryGetValue(guid, out cc))
            {
                return;
            }
            cc.Commands.RemoveAll(c => commands.Contains(c.Command));
            if (cc.Commands.Count == 0)
            {
                mDb.Commands.Remove(guid);
            }
            mDbDirty = true;
        }

        public static IEnumerable<string> GetCommand(ulong guid)
        {
            DBGmCommands cc;
            if (!mDb.Commands.TryGetValue(guid, out cc))
            {
                return null;
            }
            return cc.Commands.Select(c => c.Command);
        }

        public static IEnumerator Init(Coroutine co)
        {
            var result = GameMasterServer.Instance.DB.Get<DBGMCharacters>(co, DataCategory.GameMasterCommand,
                "gm");
            yield return result;
            if (result.Data != null)
            {
                mDb = result.Data;
            }
            else
            {
                mDb = new DBGMCharacters();
            }

            var nowTime = DateTime.Now;
#if DEBUG
            nowTime = nowTime.AddMinutes(1);
#else
            nowTime = nowTime.AddMinutes(55 - nowTime.Minute);
#endif
            var saveTime = nowTime.AddSeconds(MyRandom.Random(60));
#if DEBUG
            GameMasterServerControl.Timer.CreateTrigger(saveTime, SaveDb, 60000);
#else
            GameMasterServerControl.Timer.CreateTrigger(saveTime, SaveDb, 60000 * 30);
#endif
        }

        private static void SaveDb()
        {
            CoroutineFactory.NewCoroutine(SaveDb).MoveNext();
        }

        public static IEnumerator SaveDb(Coroutine co)
        {
            if (!mDbDirty)
            {
                yield break;
            }
            var result = GameMasterServer.Instance.DB.Set(co, DataCategory.GameMasterCommand, "gm", mDb);
            yield return result;
            if (result.Status != DataStatus.Ok)
            {
                Logger.Error("SaveGmCommand failed!!");
                yield break;
            }
            mDbDirty = false;
        }
    }
}