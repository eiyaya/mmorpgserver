#region using

using System;
using System.Collections;
using System.Collections.Generic;
using Database;
using DataContract;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace GameMaster
{
    public interface IGMAccountController
    {
        IEnumerator SaveDb(Coroutine co, GMAccountController _this, AsyncReturnValue<int> ret);
        IEnumerator SaveDbName(Coroutine co, GMAccountController _this, AsyncReturnValue<int> ret);
    }

    public class GMAccountControllerDefaultImpl : IGMAccountController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerator SaveDbName(Coroutine co, GMAccountController _this, AsyncReturnValue<int> ret)
        {
            var retSet = GameMasterServer.Instance.DB.Set(co, DataCategory.GameMaster, _this.DbData.Name,
                _this.DbData.Id.ToDBUlong());
            yield return retSet;
            if (retSet.Status != DataStatus.Ok)
            {
                Logger.Error("Save GM DB Error! GM name = " + _this.DbData.Name);
                yield break;
            }
            Logger.Info("Save GM DB OK! GM name = " + _this.DbData.Name);
            ret.Value = 1;
        }

        public IEnumerator SaveDb(Coroutine co, GMAccountController _this, AsyncReturnValue<int> ret)
        {
            var retSet = GameMasterServer.Instance.DB.Set(co, DataCategory.GameMaster, _this.DbData.Id, _this.DbData);
            yield return retSet;
            if (retSet.Status != DataStatus.Ok)
            {
                Logger.Error("Save GM DB Error! GM name = " + _this.DbData.Name);
                yield break;
            }
            Logger.Info("Save GM DB OK! GM name = " + _this.DbData.Name);
            ret.Value = 1;
        }
    }

    public class GMAccountController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IGMAccountController mImpl;

        static GMAccountController()
        {
            GameMasterServer.Instance.UpdateManager.InitStaticImpl(typeof (GMAccountController),
                typeof (GMAccountControllerDefaultImpl), o => { mImpl = (IGMAccountController) o; });
        }

        public DBGmAccount DbData;

        public IEnumerator SaveDb(Coroutine co, AsyncReturnValue<int> ret)
        {
            return mImpl.SaveDb(co, this, ret);
        }

        public IEnumerator SaveDbName(Coroutine co, AsyncReturnValue<int> ret)
        {
            return mImpl.SaveDbName(co, this, ret);
        }
    }

    public interface IGameMasterManager
    {
        void AddGM(ulong clientId, GMAccountController gm);
        IEnumerator CreateGmAccount(Coroutine co, string name, string pwd, int priority, AsyncReturnValue<int> status);
        void DelGM(ulong clientId);
        GMAccountController GetGM(ulong clientId);
        IEnumerator LoadGM(Coroutine coroutine, string name, AsyncReturnValue<ulong> gmId, AsyncReturnValue<int> ret);
        IEnumerator ModifyGmAccount(Coroutine co, GMAccountController gmController, AsyncReturnValue<int> status);
        IEnumerator NofifyAllAccounts(Coroutine co, ulong clientID, LoginAllAccounts AllAccounts);
    }

    public class GameMasterManagerDefaultImpl : IGameMasterManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public GMAccountController GetGM(ulong clientId)
        {
            GMAccountController gm;
            if (GameMasterManager.Gms.TryGetValue(clientId, out gm))
            {
                return gm;
            }
            return null;
        }

        public void AddGM(ulong clientId, GMAccountController gm)
        {
            if (GetGM(clientId) != null)
            {
                return;
            }

            GameMasterManager.Gms[clientId] = gm;
        }

        public void DelGM(ulong clientId)
        {
            GMAccountController controller;
            if (GameMasterManager.Gms.TryGetValue(clientId, out controller))
            {
                GameMasterManager.Gms.Remove(clientId);
            }
        }

        public IEnumerator LoadGM(Coroutine coroutine,
                                  string name,
                                  AsyncReturnValue<ulong> gmId,
                                  AsyncReturnValue<int> ret)
        {
            var dbAccoutGuid = GameMasterServer.Instance.DB.Get<DBUlong>(coroutine, DataCategory.GameMaster,
                name);
            yield return dbAccoutGuid;
            if (dbAccoutGuid.Data == null)
            {
                yield break;
            }

            gmId.Value = dbAccoutGuid.Data.Value;
            if (GetGM(gmId.Value) != null)
            {
                ret.Value = 1;
                yield break;
            }

            var dbAccout = GameMasterServer.Instance.DB.Get<DBGmAccount>(coroutine, DataCategory.GameMaster,
                dbAccoutGuid.Data.Value);
            yield return dbAccout;
            if (dbAccout.Data == null)
            {
                yield break;
            }
            var gmController = new GMAccountController();
            gmController.DbData = dbAccout.Data;
            AddGM(gmController.DbData.Id, gmController);

            ret.Value = 1;
			/*
            OneAccent acc = new OneAccent() { Name = dbAccout.Data.Name };
            LoginAllAccounts allAcc = new LoginAllAccounts();
            allAcc.acc.Add (acc);
            CoroutineFactory.NewCoroutine(GameMasterManager.NofifyAllAccounts, (ulong)0, allAcc).MoveNext ();
			 */
        }

        public IEnumerator CreateGmAccount(Coroutine co,
                                           string name,
                                           string pwd,
                                           int priority,
                                           AsyncReturnValue<int> status)
        {
            status.Value = 0;
            var uuid = GameMasterServer.Instance.DB.GetNextId(co, (int) DataCategory.GameMaster);
            yield return uuid;
            if (uuid.Status != DataStatus.Ok)
            {
                yield break;
            }

            var gmController = new GMAccountController
            {
                DbData = new DBGmAccount
                {
                    Id = uuid.Data,
                    Name = name,
                    Pwd = pwd,
                    Priority = priority,
                    FoundTime = DateTime.Now.ToBinary()
                }
            };

            var returnValue = AsyncReturnValue<int>.Create();
            var co1 = CoroutineFactory.NewSubroutine(gmController.SaveDbName, co, returnValue);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            if (returnValue.Value != 1)
            {
                Logger.Error("gmController.SaveDbName Error! GM name = " + gmController.DbData.Name);
                yield break;
            }

            co1 = CoroutineFactory.NewSubroutine(gmController.SaveDb, co, returnValue);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            if (returnValue.Value != 1)
            {
                Logger.Error("gmController.SaveDb Error! GM name = " + gmController.DbData.Name);
                yield break;
            }
            returnValue.Dispose();
            AddGM(gmController.DbData.Id, gmController);

            status.Value = 1;
        }

        public IEnumerator ModifyGmAccount(Coroutine co, GMAccountController gmController, AsyncReturnValue<int> status)
        {
            status.Value = 0;
            var returnValue = AsyncReturnValue<int>.Create();
            var co1 = CoroutineFactory.NewSubroutine(gmController.SaveDb, co, returnValue);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            if (returnValue.Value != 1)
            {
                Logger.Error("gmController.SaveDb Error! GM name = " + gmController.DbData.Name);
                yield break;
            }

            AddGM(gmController.DbData.Id, gmController);

            returnValue.Dispose();
            status.Value = 1;
        }

        public IEnumerator NofifyAllAccounts(Coroutine co, ulong clientID, LoginAllAccounts AllAccounts)
        {

            var data = GameMasterServer.Instance.LoginAgent.NotiffyGMAccount(clientID, AllAccounts);
            yield return data.SendAndWaitUntilDone(co);
        }
    }

    public static class GameMasterManager
    {
        public static Dictionary<ulong, GMAccountController> Gms = new Dictionary<ulong, GMAccountController>();
        private static IGameMasterManager mStaticImpl;

        static GameMasterManager()
        {
            GameMasterServer.Instance.UpdateManager.InitStaticImpl(typeof (GameMasterManager),
                typeof (GameMasterManagerDefaultImpl),
                o => { mStaticImpl = (IGameMasterManager) o; });
        }

        public static void AddGM(ulong clientId, GMAccountController gm)
        {
            mStaticImpl.AddGM(clientId, gm);
        }

        public static IEnumerator CreateGmAccount(Coroutine co,
                                                  string name,
                                                  string pwd,
                                                  int priority,
                                                  AsyncReturnValue<int> status)
        {
            return mStaticImpl.CreateGmAccount(co, name, pwd, priority, status);
        }

        public static void DelGM(ulong clientId)
        {
            mStaticImpl.DelGM(clientId);
        }

        public static GMAccountController GetGM(ulong clientId)
        {
            return mStaticImpl.GetGM(clientId);
        }

        public static IEnumerator LoadGM(Coroutine co,
                                         string name,
                                         AsyncReturnValue<ulong> gmId,
                                         AsyncReturnValue<int> ret)
        {
            return mStaticImpl.LoadGM(co, name, gmId, ret);
        }

        public static IEnumerator ModifyGmAccount(Coroutine co,
                                                  GMAccountController gmController,
                                                  AsyncReturnValue<int> status)
        {
            return mStaticImpl.ModifyGmAccount(co, gmController, status);
        }

        public static IEnumerator NofifyAllAccounts(Coroutine co, ulong clientID, LoginAllAccounts AllAccounts)
        {
            return mStaticImpl.NofifyAllAccounts(co, clientID, AllAccounts);
        }
    }
}