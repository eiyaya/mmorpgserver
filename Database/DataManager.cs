#region using

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using JsonConfig;
using Scorpion;
using NLog;
using ProtoBuf;
using ServiceStack;
using ServiceStack.Redis;
using ZLibNet;

#endregion

namespace Database
{
    public static class Helper
    {
        public static int AddSeven(int v)
        {
            return v + 7;
        }
    }

    public enum DataCategory
    {
        Activity = 1000,
        SpecialMonster = 1001,
        UninonBattle = 1002,
        Chat = 2000,
        ChatCharacter = 2001,
        Logic = 3000,
        LogicCharacter = 3001,
        Login = 4000,
        LoginPlayerName = 4001,
        LoginPlayerThird = 4002,
        LoginPlayer = 4003,
        LoginCharacter = 4004,
        LoginCharacterName = 4005,
        LoginPlayerLastLoginTime = 4007,
        Rank = 5000,
		RankBackup = 5100,
        Scene = 6000,
        SceneCharacter = 6001,
        SceneWorldBoss = 6002,
        MieShiActivity = 6003,
        Team = 7000,
        TeamAlliance = 7001,
        TeamExchange = 7002,
        TeamShop = 7003,
        TeamAllianceWar = 7004,
        TeamAuctions = 7005,
        TeamLode = 7006,
        TimeTask = 8000,
        Common = 10000,
        GameMaster = 20000,
        GameMasterMail = 20001,
        GameMasterBroadcast = 20002,
        GameMasterCommand = 20003, //gm command
        GiftCode = 20004,
        BossHomeActivity = 2005,
        MysteryStore = 2006,
        BlackStore = 2007,
        GeneralActivity=2008,
        AcientBattle = 2009,
        Chicken = 2010,
    }

    public enum DataStatus
    {
        Ok = 0,
        DatabaseError = 1,
        LockRequired = 2,
        BadCategory = 3,
        NoDataNeedToSave = 4,
        Failed = 5
    }

    public enum SetOption
    {
        Normal = 0,
        SetIfExist = 1,
        SetIfNotExist = 2
    }

    public class GetDataResult<T>
    {
        public T Data { get; set; }
        public DataStatus Status { get; set; }
    }

    public class SetDataResult
    {
        public DataStatus Status { get; set; }
    }

//     public sealed class DataManager : IDisposable
//     {
// 
//         private MysqlShardKVClient mMySqlClient;
//         private MemcachedClient mCacheClient;
//         private readonly DBShardConfig mMySqlConfig = new DBShardConfig();
//         private readonly MemcachedClientConfiguration mCacheConfig = new MemcachedClientConfiguration();
//         private InMemoryCache mInMemoryCache;
//         private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
// 
//         public bool EnableInMemoryCache = true;
//         public bool EnableMemCached = true; 
// 
//         private DataCategory mDataCategory;
//         private int mShardIndex;
// 
//         private object mLock = new object();
// 
//         private ServerAgentBase mServerAgentBase;
// 
//         private const string LockPrefix = "lock_";
//         public DataManager(string strConfig, ServerAgentBase agentBase, DataCategory category, int index)
//         {
//             mDataCategory = category;
//             //mShardCount = count;
//             mShardIndex = index;
// 
//             mServerAgentBase = agentBase;
// 
//             Logger.Info("Start initialize DataManager...");
// 
//             mCacheConfig.Servers.Add(new IPEndPoint(Dns.GetHostAddresses("127.0.0.1")[0], 11211));
//             mCacheConfig.Transcoder = new NetTranscoder();
// 
//             mCacheClient = new MemcachedClient(mCacheConfig);
// 
//             dynamic config = Config.ApplyJson(strConfig);
//             mMySqlConfig.SlotCount = config.SlotCount;
//             dynamic[] entitieList = config.Entities;
//             if (entitieList.Length > 0 )
//             {
//                 mMySqlConfig.Entities = new DBShardEntityConfig[entitieList.Length];
//                 for (int i = 0; i < entitieList.Length; i++)
//                 {
//                     mMySqlConfig.Entities[i] = new DBShardEntityConfig();
//                     mMySqlConfig.Entities[i].ShardIndex = entitieList[i].ShardIndex;
//                     int[] slots = entitieList[i].Slots;
//                     if (slots.Length > 0)
//                     {
//                         mMySqlConfig.Entities[i].Slots = new int[slots.Length];
//                         for (int j = 0; j < slots.Length; j++)
//                         {
//                             mMySqlConfig.Entities[i].Slots[j] = slots[j];
//                         }
//                     }                    
//                     mMySqlConfig.Entities[i].ConnectionString = entitieList[i].ConnectionString;
//                 }    
//             }
//             dynamic[] idEntitieList = config.IdEntities;
//             if (idEntitieList.Length > 0)
//             {
//                 mMySqlConfig.IdEntities = new DBIdGenerationConfig[idEntitieList.Length];
//                 for (int i = 0; i < idEntitieList.Length; i++)
//                 {
//                     mMySqlConfig.IdEntities[i] = new DBIdGenerationConfig();
//                     mMySqlConfig.IdEntities[i].IdIncrement = idEntitieList[i].IdIncrement;
//                     mMySqlConfig.IdEntities[i].IdOffset = idEntitieList[i].IdOffset;
//                     mMySqlConfig.IdEntities[i].ConnectionString = idEntitieList[i].ConnectionString;
//                 }
//             }
// 
//             mMySqlClient = new MysqlShardKVClient {Config = mMySqlConfig};
//             mMySqlClient.Connect(index);
// 
//             mInMemoryCache = new InMemoryCache();
// 
//             Logger.Info("Initialize DataManager finished.");
//         }
// 
//         public SetDataResult Set<T>(Coroutine co, DataCategory cat, string key, T t) where T : IExtensible
//         {
//             Logger.Info("Set key: {0}, {1}", cat, key);
// 
//             SetDataResult result = new SetDataResult();
// 
//             MemoryStream ms = new MemoryStream();
//             Serializer.Serialize(ms, t);
// 
//             if (ms.Length == 0)
//             {
//                 mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                 result.Status = DataStatus.NoDataNeedToSave;
//                 return result;
//             }
// 
//             Task.Run(() =>
//             {
//                 // this lock may too expensive, we can optimize it later.
//                 var watch = Stopwatch.StartNew();
//                 lock (mLock)
//                 {
//                     try
//                     {
//                         if (((int) cat >= (int) mDataCategory + 1000) && (int) cat < (int) mDataCategory)
//                         {
//                             Logger.Error("Try to write data to a data category which is not owned by this server.");
//                             result.Status = DataStatus.BadCategory;
//                             mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                             return;
//                         }
// 
//                         if (key.Any(item => item > 255))
//                         {
//                             key = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
//                         }
//                         var strKey = ((int) cat).ToString() + key;
//                         if (EnableInMemoryCache && mInMemoryCache.Exists(strKey))
//                         {
//                             mInMemoryCache.Set(t, strKey);
//                         }
// 
//                         var _lock = mCacheClient.Get<int>(LockPrefix + strKey);
//                         if (_lock != mShardIndex && _lock != 0)
//                         {
//                             result.Status = DataStatus.LockRequired;
//                             mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                             return;
//                         }
// 
//                         if (mMySqlClient.Set((int) cat, key, ms.ToArray()))
//                         {
//                             if (EnableMemCached)
//                             {
//                                 mCacheClient.Store(StoreMode.Set, strKey, t);
//                                 mCacheClient.Store(StoreMode.Set, LockPrefix + strKey, mShardIndex);
//                             }
// 
//                             result.Status = DataStatus.Ok;
//                             mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                             return;
//                         }
// 
//                         result.Status = DataStatus.DatabaseError;
//                         mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                         return;
//                     }
//                     catch (Exception ex)
//                     {
//                         Logger.Error("Set data from db error.", ex);
//                     }
//                     finally
//                     {
//                         Logger.Info("DB.Set {0} {1} use {2} time.", cat, key, watch.Elapsed.TotalMilliseconds);
//                     }
//                 }
//             });
// 
//             return result;
//         }
// 
//         public SetDataResult SetAndUnlock<T>(Coroutine co, DataCategory cat, string key, T t) where T : IExtensible
//         {
//             Logger.Info("Set key and unlock: {0}, {1}", cat, key);
// 
//             SetDataResult result = new SetDataResult();
// 
//             MemoryStream ms = new MemoryStream();
//             Serializer.Serialize(ms, t);
// 
//             if (ms.Length == 0)
//             {
//                 mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                 result.Status = DataStatus.NoDataNeedToSave;
//                 return result;
//             }
// 
//             Task.Run(() =>
//             {
//                 // this lock may too expensive, we can optimize it later.
//                 var watch = Stopwatch.StartNew();
//                 lock (mLock)
//                 {
//                     try
//                     {
//                         if (((int) cat >= (int) mDataCategory + 1000) && (int) cat < (int) mDataCategory)
//                         {
//                             Logger.Error("Try to write data to a data category which is not owned by this server.");
//                             result.Status = DataStatus.BadCategory;
//                             mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                             return;
//                         }
//                         if (key.Any(item => item > 255))
//                         {
//                             key = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
//                         }
//                         key = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
//                         var strKey = ((int) cat).ToString() + key;
//                         if (EnableInMemoryCache)
//                         {
//                             mInMemoryCache.Clear(strKey);
//                         }
// 
//                         var _lock = mCacheClient.Get<int>(LockPrefix + strKey);
//                         if (_lock != mShardIndex && _lock != 0)
//                         {
//                             result.Status = DataStatus.LockRequired;
//                             mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                             return;
//                         }
// 
//                         if (mMySqlClient.SetAndUnlock((int)cat, key, ms.ToArray()))
//                         {
//                             if (EnableMemCached)
//                             {
//                                 mCacheClient.Store(StoreMode.Set, strKey, t);
//                                 mCacheClient.Store(StoreMode.Set, LockPrefix + strKey, _lock);
//                             }
// 
//                             result.Status = DataStatus.Ok;
//                             mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                             return;
//                         }
// 
//                         result.Status = DataStatus.DatabaseError;
//                         mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                         return;
//                     }
//                     catch (Exception ex)
//                     {
//                         Logger.Error("SetAndUnlock data from db error.", ex);
//                     }
//                     finally
//                     {
//                         Logger.Info("DB.SetAndUnlock {0} {1} use {2} time.", cat, key, watch.Elapsed.TotalMilliseconds);
//                     }
//                 }
//             });
// 
//             return result;
//         }
// 
//         public GetDataResult<T> Get<T>(Coroutine co, DataCategory cat, string key) where T : IExtensible
//         {
//             Logger.Info("Get key: {0}, {1}", cat, key);
// 
//             GetDataResult<T> result = new GetDataResult<T>();
//             Task.Run(() =>
//             {
//                 // this lock may too expensive, we can optimize it later.
//                 var watch = Stopwatch.StartNew();
//                 lock (mLock)
//                 {
//                     try
//                     {
//                         if (key.Any(item => item > 255))
//                         {
//                             key = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
//                         }
//                         var strKey = ((int) cat).ToString() + key;
// 
//                         if (EnableInMemoryCache && (((int) cat >= (int) mDataCategory + 1000) && (int) cat < (int) mDataCategory))
//                         {
//                             var inMemoryData = mInMemoryCache.Get<T>(strKey);
//                             if (inMemoryData != null)
//                             {
//                                 Logger.Info("Get data from memory: {0}, {1}", cat, key);
//                                 result.Data = inMemoryData;
//                                 mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                 return;
//                             }
//                         }
// 
//                         if (EnableMemCached)
//                         {
//                             var cachedData = mCacheClient.Get<T>(strKey);
//                             if (cachedData != null)
//                             {
//                                 Logger.Info("Get data from memcached: {0}, {1}", cat, key);
//                                 result.Data = cachedData;
//                                 mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                 return;
//                             }
//                         }
// 
//                         int _lock;
//                         byte[] _data;
//                         if (mMySqlClient.Get((int) cat, key, out _lock, out _data))
//                         {
//                             using (var ms = new MemoryStream(_data))
//                             {
//                                 var w = Stopwatch.StartNew();
//                                 var data = Serializer.Deserialize<T>(ms);
//                                 Logger.Info("Serializer use {0} type:{1} isnull:{2} size:{3}", w.Elapsed.TotalMilliseconds, typeof(T), data == null, ms.Length);
// 
//                                 mCacheClient.Store(StoreMode.Set, strKey, data);
//                                 mCacheClient.Store(StoreMode.Set, LockPrefix + strKey, _lock);
// 
//                                 // only cache owned data in local memory
//                                 if (_lock == mShardIndex)
//                                 {
//                                     mInMemoryCache.Set(data, strKey);
//                                 }
// 
//                                 Logger.Info("Get data from mysql: {0}, {1}", cat, key);
// 
//                                 result.Data = data;
//                                 mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                 return;
//                             }
//                         }
// 
//                         result.Data = default(T);
//                         result.Status = DataStatus.DatabaseError;
//                         mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                         return;
//                     }
//                     catch(Exception ex)
//                     {
//                         Logger.Error("Get data from db error.", ex);
//                     }
//                     finally
//                     {
//                         Logger.Info("DB.Get {0} {1} use {2} time.", cat, key, watch.Elapsed.TotalMilliseconds);
//                     }
//                 }
// 
//             });
// 
//             return result;
//         }
// 
//         public GetDataResult<T> GetAndLock<T>(Coroutine co, DataCategory cat, string key) where T : IExtensible
//         {
//             Logger.Info("Get key and lock: {0}, {1}", cat, key);
// 
//             GetDataResult<T> result = new GetDataResult<T>();
//             Task.Run(() =>
//             {
//                 // this lock may too expensive, we can optimize it later.
//                 var watch = Stopwatch.StartNew();
//                 lock (mLock)
//                 {
//                     try
//                     {
//                         if (key.Any(item => item > 255))
//                         {
//                             key = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
//                         }
//                         var strKey = ((int)cat).ToString() + key;
// 
//                         if (EnableInMemoryCache && ((int) cat >= (int) mDataCategory + 1000) && (int) cat < (int) mDataCategory)
//                         {
//                             var inMemoryData = mInMemoryCache.Get<T>(strKey);
//                             if (inMemoryData != null)
//                             {
//                                 Logger.Info("Get data from memory: {0}, {1}", cat, key);
//                                 result.Data = inMemoryData;
//                                 mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                 return;
//                             }
//                         }
// 
//                         int _lock;
//                         if (EnableMemCached)
//                         {
//                             _lock = mCacheClient.Get<int>(LockPrefix + strKey);
//                             if (_lock == mShardIndex)
//                             {
//                                 var cachedData = mCacheClient.Get<T>(strKey);
//                                 if (cachedData != null)
//                                 {
//                                     Logger.Info("Get data from memcached: {0}, {1}", cat, key);
//                                     result.Data = cachedData;
//                                     mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                     return;
//                                 }
//                             }
//                         }
// 
//                         byte[] _data;
//                         if (mMySqlClient.GetAndLock((int) cat, key, out _lock, out _data))
//                         {
//                             using (var ms = new MemoryStream(_data))
//                             {
//                                 var data = Serializer.Deserialize<T>(ms);
//                                 mCacheClient.Store(StoreMode.Set, strKey, data);
//                                 mCacheClient.Store(StoreMode.Set, LockPrefix + strKey, _lock);
// 
//                                 // only cache owned data in local memory
//                                 if (_lock == mShardIndex)
//                                 {
//                                     mInMemoryCache.Set(data, strKey);
//                                 }
// 
//                                 Logger.Info("Get data from mysql: {0}, {1}", cat, key);
// 
//                                 result.Data = data;
//                                 mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                 return;
//                             }
//                         }
// 
//                         result.Data = default(T);
//                         result.Status = DataStatus.DatabaseError;
//                         mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                         return;
//                     }
//                     catch (Exception ex)
//                     {
//                         Logger.Error("GetAndLock data from db error.", ex);
//                     }
//                     finally
//                     {
//                         Logger.Info("DB.GetAndLock {0} {1} use {2} time.", cat, key, watch.Elapsed.TotalMilliseconds);
//                     }
//                 }
//             });
// 
//             return result;
//         }
// 
//         public GetDataResult<T> GetAndDelete<T>(Coroutine co, DataCategory cat, string key) where T : IExtensible
//         {
//             GetDataResult<T> result = new GetDataResult<T>();
//             Task.Run(() =>
//             {
//                 // this lock may too expensive, we can optimize it later.
//                 var watch = Stopwatch.StartNew();
//                 lock (mLock)
//                 {
//                     try
//                     {
//                         if (key.Any(item => item > 255))
//                         {
//                             key = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
//                         }
//                         var strKey = ((int)cat).ToString() + key;
// 
//                         if (EnableInMemoryCache && (((int)cat >= (int)mDataCategory + 1000) && (int)cat < (int)mDataCategory))
//                         {
//                             var inMemoryData = mInMemoryCache.Get<T>(strKey);
//                             if (inMemoryData != null)
//                             {
//                                 Logger.Info("Get data from memory: {0}, {1}", cat, key);
//                                 result.Data = inMemoryData;
// 
//                                 var __lock = mCacheClient.Get<int>(LockPrefix + strKey);
//                                 if (__lock != mShardIndex && __lock != 0)
//                                 {
//                                     result.Status = DataStatus.LockRequired;
//                                     mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                     return;
//                                 }
// 
//                                 if (mMySqlClient.Delete((int)cat, key))
//                                 {
//                                     if (EnableMemCached)
//                                     {
//                                         mCacheClient.Remove(strKey);
//                                     }
// 
//                                     mInMemoryCache.Clear(strKey);
// 
//                                     result.Status = DataStatus.Ok;
//                                     mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                     return;
//                                 }
//                                 else
//                                 {
//                                     result.Status = DataStatus.DatabaseError;
//                                     mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                 }
//                             }
//                         }
// 
//                         if (EnableMemCached)
//                         {
//                             var cachedData = mCacheClient.Get<T>(strKey);
//                             if (cachedData != null)
//                             {
//                                 Logger.Info("Get data from memcached: {0}, {1}", cat, key);
//                                 result.Data = cachedData;
//                                 
//                                 var __lock = mCacheClient.Get<int>(LockPrefix + strKey);
//                                 if (__lock != mShardIndex && __lock != 0)
//                                 {
//                                     result.Status = DataStatus.LockRequired;
//                                     mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                     return;
//                                 }
// 
//                                 if (mMySqlClient.Delete((int)cat, key))
//                                 {
//                                     if (EnableMemCached)
//                                     {
//                                         mCacheClient.Remove(strKey);
//                                     }
// 
//                                     mInMemoryCache.Clear(strKey);
// 
//                                     result.Status = DataStatus.Ok;
//                                     mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                     return;
//                                 }
//                                 else
//                                 {
//                                     result.Status = DataStatus.DatabaseError;
//                                     mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                 }
//                             }
//                         }
// 
//                         int _lock;
//                         byte[] _data;
//                         if (mMySqlClient.Get((int)cat, key, out _lock, out _data))
//                         {
//                             using (var ms = new MemoryStream(_data))
//                             {
//                                 var data = Serializer.Deserialize<T>(ms);
//                                 Logger.Info("Get data from mysql: {0}, {1}", cat, key);
// 
//                                 result.Data = data;
// 
//                                 var __lock = mCacheClient.Get<int>(LockPrefix + strKey);
//                                 if (__lock != mShardIndex && __lock != 0)
//                                 {
//                                     result.Status = DataStatus.LockRequired;
//                                     mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                     return;
//                                 }
// 
//                                 if (mMySqlClient.Delete((int)cat, key))
//                                 {
//                                     if (EnableMemCached)
//                                     {
//                                         mCacheClient.Remove(strKey);
//                                     }
// 
//                                     mInMemoryCache.Clear(strKey);
// 
//                                     result.Status = DataStatus.Ok;
//                                     mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                     return;
//                                 }
//                                 else
//                                 {
//                                     result.Status = DataStatus.DatabaseError;
//                                     mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                                 }
// 
//                                 return;
//                             }
//                         }
// 
//                         result.Data = default(T);
//                         result.Status = DataStatus.DatabaseError;
//                         mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                         return;
//                     }
//                     catch (Exception ex)
//                     {
//                         Logger.Error("Get data from db error.", ex);
//                     }
//                     finally
//                     {
//                         Logger.Info("DB.Get {0} {1} use {2} time.", cat, key, watch.Elapsed.TotalMilliseconds);
//                     }
//                 }
// 
//             });
// 
//             return result;
//         }
// 
//         public SetDataResult Delete(Coroutine co, DataCategory cat, string key)
//         {
//             Logger.Info("Delete key: {0}, {1}", cat, key);
// 
//             SetDataResult result = new SetDataResult();
//             Task.Run(() =>
//             {
//                 // this lock may too expensive, we can optimize it later.
//                 var watch = Stopwatch.StartNew();
//                 lock (mLock)
//                 {
//                     try
//                     {
//                         if (((int) cat >= (int) mDataCategory + 1000) && (int) cat < (int) mDataCategory)
//                         {
//                             Logger.Error("Try to delete data from a data category which is not owned by this server.");
//                             result.Status = DataStatus.BadCategory;
//                             mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                             return;
//                         }
// 
//                         if (key.Any(item => item > 255))
//                         {
//                             key = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
//                         }
//                         var strKey = ((int) cat).ToString() + key;
//                         if (EnableInMemoryCache)
//                         {
//                             mInMemoryCache.Clear(strKey);
//                         }
// 
//                         var _lock = mCacheClient.Get<int>(LockPrefix + strKey);
//                         if (_lock != mShardIndex && _lock != 0)
//                         {
//                             result.Status = DataStatus.LockRequired;
//                             mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                             return;
//                         }
// 
//                         if (mMySqlClient.Delete((int) cat, key))
//                         {
//                             if (EnableMemCached)
//                             {
//                                 mCacheClient.Remove(strKey);
//                             }
// 
//                             result.Status = DataStatus.Ok;
//                             mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                             return;
//                         }
// 
//                         result.Status = DataStatus.DatabaseError;
//                         mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                         return;
//                     }
//                     catch (Exception ex)
//                     {
//                         Logger.Error("Delete data from db error.", ex);
//                     }
//                     finally
//                     {
//                         Logger.Info("DB.Delete {0} {1} use {2} time.", cat, key, watch.Elapsed.TotalMilliseconds);
//                     }
//                 }
//             });
// 
//             return result;
//         }
// 
//         public SetDataResult Set<T>(Coroutine co, DataCategory cat, ulong key, T t) where T : IExtensible
//         {
//             return Set(co, cat, key.ToString(), t);
//         }
// 
//         public SetDataResult SetAndUnlock<T>(Coroutine co, DataCategory cat, ulong key, T t) where T : IExtensible
//         {
//             return SetAndUnlock(co, cat, key.ToString(), t);
//         }
// 
//         public GetDataResult<T> Get<T>(Coroutine co, DataCategory cat, ulong key) where T : IExtensible
//         {
//             return Get<T>(co, cat, key.ToString());
//         }
// 
//         public GetDataResult<T> GetAndLock<T>(Coroutine co, DataCategory cat, ulong key) where T : IExtensible
//         {
//             return GetAndLock<T>(co, cat, key.ToString());
//         }
// 
//         public SetDataResult Delete(Coroutine co, DataCategory cat, ulong key)
//         {
//             return Delete(co, cat, key.ToString());
//         }
//         public GetDataResult<ulong> GetNextId(Coroutine co, int keyType)
//         {
//             GetDataResult<ulong> result = new GetDataResult<ulong>();
//             Task.Run(() =>
//             {
//                 lock (mLock)
//                 {
//                     try
//                     {
//                         var id = mMySqlClient.GetNextId(keyType);
//                         if (id == 0)
//                         {
//                             result.Status = DataStatus.DatabaseError;
//                         }
//                         else
//                         {
//                             result.Status = DataStatus.Ok;
//                             result.Data = id + 10000000001;
//                         }
// 
//                         mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
//                     }
//                     catch (Exception ex)
//                     {
//                         Logger.Error("GetNextId from db error.", ex);
//                     }
//                 }
//             });
// 
//             return result;
//         }
// 
//         public void ClearInMemoryCache()
//         {
//             Logger.Info("Clear all inmemory cache.");
// 
//             Task.Run(() =>
//             {
//                 mInMemoryCache.Clear();
//             });                                                                                                                   
//         }
// 
//         public void Dispose()
//         {
//             mInMemoryCache.Dispose();
//             mCacheClient.Dispose();
//         }
// 
//     }


    public interface IDataManager
    {
        SetDataResult Delete(Coroutine co, DataCategory cat, string key);
        SetDataResult Delete(Coroutine co, DataCategory cat, ulong key);
        GetDataResult<T> Get<T>(Coroutine co, DataCategory cat, ulong key) where T : IExtensible;
        GetDataResult<T> Get<T>(Coroutine co, DataCategory cat, string key) where T : IExtensible;
        GetDataResult<T> GetAndDelete<T>(Coroutine co, DataCategory cat, string key) where T : IExtensible;
        GetDataResult<ulong> GetCurrentId(Coroutine co, int keyType, string key = null);
        GetDataResult<ulong> GetNextId(Coroutine co, int keyType, string key = null);

        SetDataResult Set<T>(Coroutine co,
            DataCategory cat,
            string key,
            T t,
            SetOption option = SetOption.Normal,
            bool fireAndForgot = false) where T : IExtensible;

        SetDataResult Set<T>(Coroutine co, DataCategory cat, ulong key, T t, SetOption option = SetOption.Normal)
            where T : IExtensible;

        object Wait(Coroutine co, TimeSpan span);
        void Dispose();
    }

#if !COUCHBASE

    public sealed class DataManager : IDisposable, IDataManager
    {
        private const string LockPrefix = "lock_";
        //private StackExchange.Redis.ConnectionMultiplexer mRedisClient;
        //private static object sLock = new object();
        //private IDatabase mDatabase;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DataManager(string strConfig, ServerAgentBase agentBase, DataCategory category, int index)
        {
            mServerAgentBase = agentBase;

            Logger.Info("Start initialize DataManager...");

            dynamic config = Config.ApplyJson(strConfig);
            var servers = new List<string>();
            foreach (var addr in config.Addrs)
            {
                servers.Add(addr);
            }

            try
            {
                if (!IsPowerOfTwo(servers.Count))
                {
                    throw new Exception("Database configuration error, count of redis should be power of 2.");
                }

                mRunning = true;
                mShardCount = servers.Count;
                for (var i = 0; i < mShardCount; i++)
                {
                    RedisClient client = null;
                    var strs = servers[i].Split(':');
                    if (strs.Length == 1)
                    {
                        client = new RedisClient(strs[0]);
                    }
                    else if (strs.Length == 2)
                    {
                        client = new RedisClient(strs[0], int.Parse(strs[1]));
                    }
                    else if (strs.Length == 3)
                    {
                        client = new RedisClient(strs[0], int.Parse(strs[1]), strs[2]);
                    }
                    else if (strs.Length == 4)
                    {
                        client = new RedisClient(strs[0], int.Parse(strs[1]), string.Format("{0}:{1}", strs[2], strs[3]));
                    }

                    if (client == null)
                    {
                        throw new Exception("Database configuration error, " + servers[i]);
                    }

                    mRedisClients.Add(client);
                    var commandQueue = new ConcurrentQueue<object>();
                    mCommandQueues.Add(commandQueue);
                    var evt = new AutoResetEvent(false);
                    mEvents.Add(evt);
                    var thread = new Thread(() => { ExecuteQueuedCommand(client, commandQueue, evt); });

                    thread.Start();
                    mThreads.Add(thread);
                }
                //mRedisClientManager.Start();

                //如果有修改，先跑通下面的测试用例
                //CoroutineFactory.NewCoroutine(Test).MoveNext();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Create PooledRedisClientManager failed.");
            }

            Logger.Info("Initialize DataManager finished.");
        }

        //private readonly Dictionary<string, IDBOne> Cache =
        //    new Dictionary<string, IDBOne>(new Dictionary<string, IDBOne>());

        private readonly List<ConcurrentQueue<object>> mCommandQueues = new List<ConcurrentQueue<object>>();
        private readonly List<AutoResetEvent> mEvents = new List<AutoResetEvent>();
        private readonly MemoryStream mMemoryStream = new MemoryStream();
        private readonly List<RedisClient> mRedisClients = new List<RedisClient>();
        private bool mRunning;
        private readonly ServerAgentBase mServerAgentBase;
        private readonly int mShardCount;
        private readonly List<Thread> mThreads = new List<Thread>();
        //private readonly Cache<string, object> mCache = new Cache<string, object>();

        private MemoryStream MemoryStream
        {
            get
            {
                mMemoryStream.SetLength(0);
                mMemoryStream.Seek(0, SeekOrigin.Begin);
                return mMemoryStream;
            }
        }

        private GetDataResult<T> _Get<T>(Coroutine co, DataCategory cat, string key)
            where T : IExtensible
        {
            Logger.Info("Get key: {0}, {1}", cat, key);
            var result = new GetDataResult<T>();
            var _key = (int) cat + ":" + key;
            var index = _key[_key.Length - 1]%mShardCount;

            if (UseHashKeyValue(cat))
            {
                string hashId;
                string hashKey;
                if (-1 != key.IndexOf(":"))
                {
                    hashId = (int)cat + ":" + key.Substring(0, key.IndexOf(":"));
                    hashKey = key.Substring(key.IndexOf(":") + 1);
                }
                else
                {
                    hashId = "" + (int)cat;
                    hashKey = key;
                }
                mCommandQueues[index].Enqueue(new QueueByteArrayCommand
                {
                    Command = client => ((RedisNativeClient)client).HGet(hashId, hashKey.ToUtf8Bytes()),
                    Successor = mem =>
                    {
                        if (mem != null)
                        {
                            // 解压缩
                            if (NeedCompress(cat))
                            {
                                mem = Compressor(mem, false);
                            }

                            using (var ms = new MemoryStream(mem, false))
                            {
                                var data = Serializer.Deserialize<T>(ms);
                                result.Data = data;
                            }
                        }
                        result.Status = DataStatus.Ok;
                        mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                    },
                    Error = ex =>
                    {
                        Logger.Error(ex, "Redis Get key:{0} error.", key);
                        result.Status = DataStatus.DatabaseError;
                        mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                    }
                });
            }
            else
            {
                mCommandQueues[index].Enqueue(new QueueByteArrayCommand
                {
                    Command = client => client.Get<byte[]>(_key),
                    Successor = mem =>
                    {
                        if (mem != null)
                        {
                            using (var ms = new MemoryStream(mem, false))
                            {
                                var data = Serializer.Deserialize<T>(ms);
                                result.Data = data;
                            }
                        }
                        result.Status = DataStatus.Ok;
                        mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                    },
                    Error = ex =>
                    {
                        Logger.Error(ex, "Redis Get key:{0} error.", _key);
                        result.Status = DataStatus.DatabaseError;
                        mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                    }
                });
            }
            mEvents[index].Set();

            return result;
        }

        public SetDataResult Delete(Coroutine co, DataCategory cat, string key)
        {
            Logger.Info("Delete key: {0}, {1}", cat, key);

            var result = new SetDataResult();

            var _key = (int) cat + ":" + key;

            //mCache.Delete(_key);

            var index = _key[_key.Length - 1]%mShardCount;
            mCommandQueues[index].Enqueue(new QueueBoolCommand
            {
                Command = client =>
                {
                    if (UseHashKeyValue(cat))
                    {
                        string hashId = "";
                        string hashKey = "";
                        if (-1 != key.IndexOf(":"))
                        {
                            hashId = (int)cat + ":" + key.Substring(0, key.IndexOf(":"));
                            hashKey = key.Substring(key.IndexOf(":") + 1);
                        }
                        else
                        {
                            hashId = "" + (int)cat;
                            hashKey = key;
                        }
                        return client.RemoveEntryFromHash(hashId, hashKey);
                    }
                    return client.Remove(_key);
                },
                Successor = b =>
                {
                    result.Status = b ? DataStatus.Ok : DataStatus.Failed;
                    mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                },
                Error = ex =>
                {
                    Logger.Error(ex, "Redis Delete key:{0} error.", _key );
                    result.Status = DataStatus.DatabaseError;
                    mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                }
            });
            mEvents[index].Set();

            return result;
        }

        //public SetDataResult SetAndUnlock<T>(Coroutine co, DataCategory cat, ulong key, T t) where T : IExtensible
        //{
        //    return SetAndUnlock(co, cat, key.ToString(), t);
        //}

        public SetDataResult Delete(Coroutine co, DataCategory cat, ulong key)
        {
            return Delete(co, cat, key.ToString());
        }

        private void ExecuteQueuedCommand(IRedisClient client, ConcurrentQueue<object> commandQueue, AutoResetEvent evt)
        {
            var watch = Stopwatch.StartNew();
            var lastPingTime = watch.ElapsedMilliseconds;
            while (mRunning)
            {
                try
                {
                    while (commandQueue.Count == 0)
                    {
                        evt.WaitOne(TimeSpan.FromMinutes(1));
                        if (!mRunning)
                        {
                            return;
                        }

                        // 1 分钟ping一次 Redis Server，防止掉线，如果掉线，会重连
                        if (watch.ElapsedMilliseconds - lastPingTime > 60000)
                        {
                            if (((RedisNativeClient) client).Ping())
                            {
                                lastPingTime = watch.ElapsedMilliseconds;
                            }
                        }
                    }

                    using (var pipe = client.CreatePipeline())
                    {
                        var maxCount = 50;
                        while (commandQueue.Count > 0)
                        {
                            maxCount--;
                            if (maxCount == 0)
                            {
                                break;
                            }

                            object command;
                            if (commandQueue.TryDequeue(out command))
                            {
                                if (command is QueueByteArrayCommand)
                                {
                                    var c = (QueueByteArrayCommand) command;
                                    pipe.QueueCommand(c.Command, c.Successor, c.Error);
                                }
                                else if (command is QueueBoolCommand)
                                {
                                    var c = (QueueBoolCommand) command;
                                    pipe.QueueCommand(c.Command, c.Successor, c.Error);
                                }
                                else if (command is QueueLongCommand)
                                {
                                    var c = (QueueLongCommand) command;
                                    pipe.QueueCommand(c.Command, c.Successor, c.Error);
                                }
                                else if (command is QueueVoidCommand)
                                {
                                    var c = (QueueVoidCommand) command;
                                    pipe.QueueCommand(c.Command, c.Successor, c.Error);
                                }
                                else if (command is QueueStringCommand)
                                {
                                    var c = (QueueStringCommand) command;
                                    pipe.QueueCommand(c.Command, c.Successor, c.Error);
                                }
                            }
                            command = null;
                        }

                        try
                        {
                            pipe.Flush();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Redis command flush exception.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Redis execution exception.");
                }
            }
        }

        public GetDataResult<T> Get<T>(Coroutine co, DataCategory cat, ulong key) where T : IExtensible
        {
            return Get<T>(co, cat, key.ToString());
        }

        public GetDataResult<T> Get<T>(Coroutine co, DataCategory cat, string key) where T : IExtensible
        {
            Logger.Info("Get key: {0}, {1}", cat, key);
            return _Get<T>(co, cat, key);
        }

        public GetDataResult<T> GetAndDelete<T>(Coroutine co, DataCategory cat, string key) where T : IExtensible
        {
            var result = new GetDataResult<T>();
            var _key = (int) cat + ":" + key;
            var index = _key[_key.Length - 1]%mShardCount;

            if (UseHashKeyValue(cat))
            {
                string hashId;
                string hashKey;
                if (-1 != key.IndexOf(":"))
                {
                    hashId = (int) cat + ":" + key.Substring(0, key.IndexOf(":"));
                    hashKey = key.Substring(key.IndexOf(":") + 1);
                }
                else
                {
                    hashId = "" + (int) cat;
                    hashKey = key;
                }
                mCommandQueues[index].Enqueue(new QueueByteArrayCommand
                {
                    Command = client => ((RedisNativeClient) client).HGet(hashId, hashKey.ToUtf8Bytes()),
                    Successor = mem =>
                    {
                        if (mem != null)
                        {
                            // 解压缩
                            if (NeedCompress(cat))
                            {
                                mem = Compressor(mem, false);
                            }

                            using (var ms = new MemoryStream(mem, false))
                            {
                                var data = Serializer.Deserialize<T>(ms);
                                result.Data = data;
                            }
                        }
                        result.Status = DataStatus.Ok;
                        mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                    },
                    Error = ex =>
                    {
                        Logger.Error(ex, "Redis Get key:{0} error.", key);
                        result.Status = DataStatus.DatabaseError;
                        mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                    }
                });
            }
            else
            {
                mCommandQueues[index].Enqueue(new QueueByteArrayCommand
                {
                    Command = client => client.Get<byte[]>(key),
                    Successor = mem =>
                    {
                        if (mem != null)
                        {
                            // 解压缩
                            if (NeedCompress(cat))
                            {
                                mem = Compressor(mem, false);
                            }

                            using (var ms = new MemoryStream(mem, false))
                            {
                                var data = Serializer.Deserialize<T>(ms);
                                result.Data = data;
                            }
                        }
                        result.Status = DataStatus.Ok;
                        mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                    },
                    Error = ex =>
                    {
                        Logger.Error(ex, "Redis Get key:{0} error.", key);
                        result.Status = DataStatus.DatabaseError;
                        mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                    }
                });
            }

            //mCache.Delete(_key);
            mCommandQueues[index].Enqueue(new QueueBoolCommand
            {
                Command = client =>
                {
                    if (UseHashKeyValue(cat))
                    {
                        string hashId = "";
                        string hashKey = "";
                        if (-1 != key.IndexOf(":"))
                        {
                            hashId = (int)cat + ":" + key.Substring(0, key.IndexOf(":"));
                            hashKey = key.Substring(key.IndexOf(":") + 1);
                        }
                        else
                        {
                            hashId = "" + (int)cat;
                            hashKey = key;
                        }
                        return client.RemoveEntryFromHash(hashId, hashKey);
                    }

                    return client.Remove(_key);
                },
                Successor = b =>
                {
                    result.Status = b ? DataStatus.Ok : DataStatus.Failed;
                    mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                },
                Error = ex =>
                {
                    Logger.Error(ex, "Redis GetAndLock key:{0} error.", _key );
                    result.Status = DataStatus.DatabaseError;
                    mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                }
            });
            mEvents[index].Set();

            return result;
        }

        public GetDataResult<ulong> GetCurrentId(Coroutine co, int keyType, string key = null)
        {
            var result = new GetDataResult<ulong>();

            var _key = "getnextid:" + keyType;
            if (!string.IsNullOrEmpty(key))
            {
                _key += ":" + key;
            }

            var index = _key[_key.Length - 1]%mShardCount;
            mCommandQueues[index].Enqueue(new QueueStringCommand
            {
                Command = client => client.GetValue(_key),
                Successor = id =>
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        result.Data = 100000000;
                    }
                    else
                    {
                        result.Data = ulong.Parse(id);
                    }
                    result.Status = DataStatus.Ok;
                    mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                },
                Error = ex =>
                {
                    Logger.Error(ex, "Redis GetCurrentId key: {0} error.", _key );
                    result.Status = DataStatus.DatabaseError;
                    mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                }
            });
            mEvents[index].Set();

            return result;
        }

        public GetDataResult<ulong> GetNextId(Coroutine co, int keyType, string key = null)
        {
            var result = new GetDataResult<ulong>();

            var _key = "getnextid:" + keyType;
            if (!string.IsNullOrEmpty(key))
            {
                _key += ":" + key;
            }

            var index = _key[_key.Length - 1]%mShardCount;
            mCommandQueues[index].Enqueue(new QueueLongCommand
            {
                Command = client => client.IncrementValue(_key),
                Successor = id =>
                {
                    if (id < 100000000)
                    {
                        id = 100000000;
                        mCommandQueues[index].Enqueue(new QueueLongCommand
                        {
                            Command = client => client.IncrementValueBy(_key, id),
                            Successor = newId =>
                            {
                                result.Data = (ulong) newId;
                                result.Status = DataStatus.Ok;
                                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                            },
                            Error = ex =>
                            {
                                Logger.Error(ex, "Redis GetNextId key: {0} error.", _key);
                                result.Status = DataStatus.DatabaseError;
                                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                            }
                        });
                        mEvents[index].Set();
                    }
                    else
                    {
                        result.Data = (ulong) id;
                        result.Status = DataStatus.Ok;
                        mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                    }
                },
                Error = ex =>
                {
                    Logger.Error(ex, "Redis GetNextId key: {0} error.", _key);
                    result.Status = DataStatus.DatabaseError;
                    mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                }
            });
            mEvents[index].Set();

            return result;
        }

        private bool IsPowerOfTwo(int x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        //private GetDataResult<T> Load<T>(Coroutine co, DataCategory cat, string key) where T : IExtensible
        //{
        //    var newkey = (int) cat + ":" + key;
        //    IDBOne one;
        //    if (!Cache.TryGetValue(newkey, out one))
        //    {
        //        one = new DBOne<T>();
        //        one.State = DBState.None;
        //        one.Key = newkey;
        //        Cache.Add(newkey, one);
        //    }
        //    ++one.Refrence;
        //    switch (one.State)
        //    {
        //        case DBState.None:
        //        {
        //            one.State = DBState.Read;
        //            var r = _Get<T>(co, cat, key, result =>
        //            {
        //                mServerAgentBase.mWaitingEvents.Add(new ActionEvent(() =>
        //                {
        //                    if (result.Data == null)
        //                    {
        //                        one.State = DBState.None;
        //                        Cache.Remove(newkey);
        //                    }
        //                    else
        //                    {
        //                        one.State = DBState.Use;
        //                        one.Data = result.Data;
        //                    }
        //                    foreach (var action in one.Actions)
        //                    {
        //                        action();
        //                    }
        //                    one.Actions.Clear();
        //                }));
        //            });
        //            return r;
        //        }
        //        case DBState.Read:
        //        {
        //            var r = new GetDataResult<T>();
        //            one.Actions.Add(() =>
        //            {
        //                r.Data = one.Data is T ? (T) one.Data : default(T);
        //                co.MoveNext();
        //            });
        //            return r;
        //        }
        //        case DBState.Use:
        //        {
        //            var data = new GetDataResult<T>();
        //            var d = one.Data;
        //            data.Data = d is T ? (T) d : default(T);
        //            mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
        //            return data;
        //        }
        //    }
        //    return new GetDataResult<T>();
        //}

        public SetDataResult Set<T>(Coroutine co,
                                    DataCategory cat,
                                    string key,
                                    T t,
                                    SetOption option = SetOption.Normal,
                                    bool fireAndForgot = false) where T : IExtensible
        {
            Logger.Info("Set key: {0}, {1}", cat, key);

            var result = new SetDataResult();

            var ms = MemoryStream;
            Serializer.Serialize(ms, t);
            var buffer = ms.ToArray();

            // 压缩
            if (NeedCompress(cat))
            {
                buffer = Compressor(buffer, true);
            }

            string hashId = "";
            string hashKey = "";
            if (UseHashKeyValue(cat))
            {
                if (-1 != key.IndexOf(":"))
                {
                    hashId = (int) cat + ":" + key.Substring(0, key.IndexOf(":"));
                    hashKey = key.Substring(key.IndexOf(":") + 1);
                }
                else
                {
                    hashId = "" + (int) cat;
                    hashKey = key;
                }
            }

            var _key = string.Format("{0}:{1}", (int) cat, key);
            var index = _key[_key.Length - 1]%mShardCount;
            switch (option)
            {
                case SetOption.Normal:
                    mCommandQueues[index].Enqueue(new QueueVoidCommand
                    {
                        Command = client =>
                        {
                            if (UseHashKeyValue(cat))
                            {
                                ((RedisNativeClient) client).HSet(hashId, hashKey.ToUtf8Bytes(), buffer);
                            }
                            else
                            {
                                client.Set(_key, buffer);
                            }
                        },
                        Successor = () =>
                        {
                            result.Status = DataStatus.Ok;
                            //mCache.Set(_key, t);

                            if (!fireAndForgot)
                            {
                                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                            }
                        },
                        Error = ex =>
                        {
                            Logger.Error(ex, "Redis Set key:{0} error.", _key);
                            result.Status = DataStatus.DatabaseError;

                            if (!fireAndForgot)
                            {
                                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                            }
                        }
                    });
                    break;
                case SetOption.SetIfExist:
                    mCommandQueues[index].Enqueue(new QueueByteArrayCommand
                    {
                        Command = client =>
                        {
                            if (UseHashKeyValue(cat))
                            {
                                ((RedisNativeClient) client).HSet(hashId, hashKey.ToUtf8Bytes(), buffer);
                            }
                            else
                            {
                                client.Replace(_key, buffer);
                                
                            }
                            return null;
                        },
                        Successor = b =>
                        {
                            result.Status = b != null ? DataStatus.Ok : DataStatus.Failed;

                            //if (result.Status == DataStatus.Ok)
                            //    mCache.Set(_key, t);

                            if (!fireAndForgot)
                            {
                                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                            }
                        },
                        Error = ex =>
                        {
                            Logger.Error(ex, "Redis Set key:{0} error.", _key);
                            result.Status = DataStatus.DatabaseError;

                            if (!fireAndForgot)
                            {
                                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                            }
                        }
                    });
                    break;
                case SetOption.SetIfNotExist:
                    mCommandQueues[index].Enqueue(new QueueByteArrayCommand
                    {
                        Command = client =>
                        {
                            if (UseHashKeyValue(cat))
                            {
                                ((RedisNativeClient) client).HSetNX(hashId, hashKey.ToUtf8Bytes(), buffer);
                            }
                            else
                            {
                                client.Add(_key, buffer);
                                
                            }
                            return null;
                        },
                        Successor = b =>
                        {
                            result.Status = b != null ? DataStatus.Ok : DataStatus.Failed;

                            //if(result.Status == DataStatus.Ok)
                            //    mCache.Set(_key, t);

                            if (!fireAndForgot)
                            {
                                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                            }
                        },
                        Error = ex =>
                        {
                            Logger.Error(ex, "Redis Set key:{0} error.", _key );
                            result.Status = DataStatus.DatabaseError;

                            if (!fireAndForgot)
                            {
                                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                            }
                        }
                    });
                    break;
            }
            mEvents[index].Set();

            return result;
        }

        public SetDataResult Set<T>(Coroutine co, DataCategory cat, ulong key, T t, SetOption option = SetOption.Normal)
            where T : IExtensible
        {
            return Set(co, cat, key.ToString(), t, option);
        }

        public IEnumerator Test(Coroutine co)
        {
            Logger.Debug("--------------123--------------");

            //for (int i = 0; i < 10; i++)
            //{
            //    Unload(DataCategory.LogicCharacter, 100000001);
            //}

            //for (int i = 0; i < 100; ++i)
            //{
            //    var r = GetNextId(co, 1, "abc");
            //    yield return r;
            //}

            //Logger.Debug("Test GetNextId ok **************************");

            //for (int i = 0; i < 100; ++i)
            //{
            //    var r = Set(co, DataCategory.Activity, (ulong)i, new DataContract.DBInt { Value = i }, SetOption.SetIfNotExist);
            //    yield return r;

            //    var k = Get<DataContract.DBInt>(co, DataCategory.Activity, (ulong)i);
            //    yield return k;
            //    if (k.Data == null)
            //    {
            //        Logger.Error("Test SetIfNotExist failed **************************");
            //        break;
            //    }
            //}

            //Logger.Debug("Test SetIfNotExist ok **************************");

            //for (int i = 0; i < 100; ++i)
            //{
            //    var r = Set(co, DataCategory.Activity, (ulong) i, new DataContract.DBInt {Value = i}, SetOption.Normal);
            //    yield return r;
            //}

            //Logger.Debug("Test Set Normal ok **************************");

            //for (int i = 0; i < 100; ++i)
            //{
            //    var r = Set(co, DataCategory.Activity, (ulong)i, new DataContract.DBInt { Value = i }, SetOption.SetIfNotExist);
            //    yield return r;

            //    if (r.Status == DataStatus.Ok)
            //    {
            //        Logger.Error("Test SetIfNotExist failed **************************");
            //        break;
            //    }
            //}

            //for (int i = 0; i < 100; ++i)
            //{
            //    var r = Set(co, DataCategory.Activity, (ulong)i, new DataContract.DBInt { Value = i }, SetOption.SetIfExist);
            //    yield return r;

            //    if(r.Status != DataStatus.Ok)
            //    {
            //        Logger.Error("Test SetIfExist failed **************************");
            //        break;
            //    }
            //}

            //Logger.Debug("Test SetIfExist ok **************************");

            //for (int i = 0; i < 100; ++i)
            //{
            //    var r = Get<DataContract.DBInt>(co, DataCategory.Activity, (ulong) i);
            //    yield return r;
            //    if (r.Data == null)
            //    {
            //        Logger.Error("Test Get failed **************************");
            //        break;
            //    }
            //}

            //Logger.Debug("Test Get ok **************************");

            //for (int i = 0; i < 100; ++i)
            //{
            //    var r = Delete(co, DataCategory.Activity, (ulong)i);
            //    yield return r;

            //    var k = Get<DataContract.DBInt>(co, DataCategory.Activity, (ulong)i);
            //    yield return k;
            //    if (k.Data != null)
            //    {
            //        Logger.Error("Test Delete failed **************************");
            //        break;
            //    }
            //}

            //Logger.Debug("Test Delete ok **************************");

            //for (int i = 0; i < 100; ++i)
            //{
            //    var r = Set(co, DataCategory.Activity, (ulong)i, new DataContract.DBInt { Value = i }, SetOption.Normal);
            //    yield return r;


            //    var k = Get<DataContract.DBInt>(co, DataCategory.Activity, (ulong)i);
            //    yield return k;
            //    if (k.Data.Value != i)
            //    {
            //        Logger.Error("Test Set Normal failed **************************");
            //        break;
            //    }
            //}

            //Logger.Debug("Test Set Normal ok **************************");

            //for (int i = 0; i < 100; ++i)
            //{
            //    var r = GetAndDelete<DataContract.DBInt>(co, DataCategory.Activity, i.ToString());
            //    yield return r;
            //    if (r.Data.Value != i)
            //    {
            //        Logger.Error("Test GetAndDelete failed **************************");
            //    }

            //    r = Get<DataContract.DBInt>(co, DataCategory.Activity, (ulong) i);
            //    yield return r;
            //    if (r.Data != null)
            //    {
            //        Logger.Error("Test GetAndDelete failed **************************");
            //        break;
            //    }
            //}

            //Logger.Debug("Test GetAndDelete ok **************************");


            //Logger.Debug("Test all finished. **************************");
            yield break;
        }

        //public void Unload(DataCategory cat, ulong key, bool force = false)
        //{
        //    Unload(cat, key.ToString(), force);
        //}

        //public void Unload(DataCategory cat, string key, bool force = false)
        //{
        //    var newkey = (int) cat + ":" + key;
        //    IDBOne one;
        //    if (Cache.TryGetValue(newkey, out one))
        //    {
        //        if (force)
        //        {
        //            if (one.Refrence > 1)
        //            {
        //                Logger.Error("In Unload() key: {0}, {1}, refrence = {2}", cat, key, one.Refrence);
        //            }
        //            one.Refrence = 0;
        //            Cache.Remove(newkey);
        //        }
        //        else if (--one.Refrence <= 0)
        //        {
        //            one.Refrence = 0;
        //            Cache.Remove(newkey);
        //        }
        //    }
        //}

        public object Wait(Coroutine co, TimeSpan span)
        {
            mServerAgentBase.Wait(co, span);
            return null;
        }

        public void Dispose()
        {
            for (var i = 0; i < mShardCount; i++)
            {
                mRedisClients[i].Dispose();
            }

            mRunning = false;

            for (var i = 0; i < mThreads.Count; i++)
            {
                mEvents[i].Set();
                mThreads[i].Join();
            }
        }

        public class QueueBoolCommand
        {
            public Func<IRedisClient, bool> Command;
            public Action<Exception> Error;
            public Action<bool> Successor;
        }

        public class QueueVoidCommand
        {
            public Action<IRedisClient> Command;
            public Action<Exception> Error;
            public Action Successor;
        }

        public class QueueByteArrayCommand
        {
            public Func<IRedisClient, byte[]> Command;
            public Action<Exception> Error;
            public Action<byte[]> Successor;
        }

        public class QueueLongCommand
        {
            public Func<IRedisClient, long> Command;
            public Action<Exception> Error;
            public Action<long> Successor;
        }

        public class QueueStringCommand
        {
            public Func<IRedisClient, String> Command;
            public Action<Exception> Error;
            public Action<String> Successor;
        }

        public class QueueByteArray2Command
        {
            public Func<IRedisClient, byte[][]> Command;
            public Action<Exception> Error;
            public Action<byte[][]> Successor;
        }

        //public class DBOne<T> : IDBOne where T : IExtensible
        //{
        //    private List<Action> acts = new List<Action>();
        //    private T date { get; set; }

        //    public object Data
        //    {
        //        get { return date; }
        //        set { date = value is T ? (T) value : default(T); }
        //    }

        //    public DBState State { get; set; }

        //    public List<Action> Actions
        //    {
        //        get { return acts; }
        //        set { acts = value; }
        //    }

        //    public int Refrence { get; set; }
        //    public string Key { get; set; }
        //}

        //public interface IDBOne
        //{
        //    List<Action> Actions { get; set; }
        //    object Data { get; set; }
        //    string Key { get; set; }
        //    int Refrence { get; set; }
        //    DBState State { get; set; }
        //}

        /// <summary>
        /// 是否需要压缩
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        public bool NeedCompress(DataCategory cat)
        {
            // 注释掉，关闭压缩，使用时去掉注释
            //switch (cat)
            //{
            //    case DataCategory.LogicCharacter:
            //    case DataCategory.Rank:
            //    case DataCategory.RankBackup:
            //    case DataCategory.MieShiActivity:
            //        return true;
            //}
            return false;
        }

        /// <summary>
        /// 是否需要使用hash表存储
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        public bool UseHashKeyValue(DataCategory cat)
        {
            // 注释掉，关闭hash存储，使用时去掉注释
            //switch (cat)
            //{
            //    case DataCategory.LogicCharacter:
            //    case DataCategory.LoginCharacter:
            //    case DataCategory.ChatCharacter:
            //    case DataCategory.LoginPlayerName:
            //    case DataCategory.Rank:
            //    case DataCategory.RankBackup:
            //    case DataCategory.Scene:
            //    case DataCategory.SceneCharacter:
            //    case DataCategory.MieShiActivity:
            //        return true;
            //}
            return false;
        }

        /// <summary>
        /// 压缩/解压缩处理
        /// </summary>
        /// <param name="input"></param>
        /// <param name="compress"></param>
        /// <returns></returns>
        public static byte[] Compressor(byte[] input, bool compress)
        {
            if (compress)
            {
                return ZLibCompressor.Compress(input);
            }

            return ZLibCompressor.DeCompress(input);
        }
    }


#else

    public sealed class DataManager : IDisposable, IDataManager
    {
        private readonly ServerAgentBase mServerAgentBase = null;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private Cache<string, object> mCache = new Cache<string, object>();

        private static Couchbase.Configuration.Client.ClientConfiguration mConfig;
        private static BlockingCollection<Func<System.Threading.Tasks.Task>> mQueue = new BlockingCollection<Func<System.Threading.Tasks.Task>>();
        private static int mInited = 0;
        private static Couchbase.Core.IBucket mBucket = null;
        private static Thread mThread;
        public DataManager(string strConfig, ServerAgentBase agentBase, DataCategory cat, int i)
        {
            mServerAgentBase = agentBase;

            if (Interlocked.CompareExchange(ref mInited, 1, 0) != 0)
            {
                return;
            }

            dynamic config = Config.ApplyJson(strConfig);
            var servers = new List<string>();
            foreach (var addr in config.Addrs)
            {
                servers.Add(addr);
            }

            mConfig = new Couchbase.Configuration.Client.ClientConfiguration
            {
                Servers = servers.Select(item => new Uri(item)).ToList()
            };

            Couchbase.ClusterHelper.Initialize(mConfig);
            mBucket = Couchbase.ClusterHelper.GetBucket("scorpion");

            var result = mBucket.Upsert("server-start", DateTime.Now);
            while (!result.Success)
            {
                result = mBucket.Upsert("server-start", DateTime.Now);
                Thread.Sleep(500);
            }
            
            mThread = new Thread(async () =>
            {
                while (mInited == 1)
                {
                    try
                    {
                        Func<System.Threading.Tasks.Task> act = mQueue.Take();
                        await act();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Couchbase execution exception.");
                    }
                }
            });
            mThread.Start();


            //如果有修改，先跑通下面的测试用例
            //CoroutineFactory.NewCoroutine(Test).MoveNext();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref mInited, 0, 1) != 1)
            {
                return;
            }

            mThread.Join();
            mBucket.Dispose();
        }

        public SetDataResult Delete(Coroutine co, DataCategory cat, string key)
        {
            var result = new SetDataResult();
            var _key = (int)cat + ":" + key;

            mCache.Delete(_key);

            mQueue.Add(async () =>
            {
                var _result = await mBucket.RemoveAsync(_key);
                result.Status = _result.Status == Couchbase.IO.ResponseStatus.Success
                    ? DataStatus.Ok
                    : DataStatus.Failed;
                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
            });
            return result;
        }

        public SetDataResult Delete(Coroutine co, DataCategory cat, ulong key)
        {
            return Delete(co, cat, key.ToString());
        }

        public GetDataResult<T> Get<T>(Coroutine co, DataCategory cat, string key) where T : IExtensible
        {
            var result = new GetDataResult<T>();
            var _key = (int)cat + ":" + key;

            result.Data = (T)mCache.Get(_key);
            if (result.Data != null)
            {
                result.Status = DataStatus.Ok;
                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                return result;
            }

            mQueue.Add(async () =>
            {
                var _result = await mBucket.GetAsync<T>(_key);
                result.Status = (_result.Status == Couchbase.IO.ResponseStatus.Success || _result.Status == Couchbase.IO.ResponseStatus.KeyNotFound)
                    ? DataStatus.Ok
                    : DataStatus.Failed;
                result.Data = _result.Value;
                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
            });
            return result;
        }

        public GetDataResult<T> Get<T>(Coroutine co, DataCategory cat, ulong key) where T : IExtensible
        {
            return Get<T>(co, cat, key.ToString());
        }


        public GetDataResult<T> GetAndDelete<T>(Coroutine co, DataCategory cat, string key) where T : IExtensible
        {
            var result = new GetDataResult<T>();
            var _key = (int)cat + ":" + key;

            result.Data = (T)mCache.Get(_key);
            if (result.Data != null)
            {
                mCache.Delete(_key);
                mQueue.Add(async () =>
                {
                    var _result = await mBucket.RemoveAsync(_key);
                    result.Status = (_result.Status == Couchbase.IO.ResponseStatus.Success || _result.Status == Couchbase.IO.ResponseStatus.KeyNotFound)
                        ? DataStatus.Ok
                        : DataStatus.Failed;
                    mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                });
                return result;
            }

            mQueue.Add(async () =>
            {
                var _value = await mBucket.GetAsync<T>(_key);
                var _result = await mBucket.RemoveAsync(_key);
                result.Status = _result.Status == Couchbase.IO.ResponseStatus.Success
                    ? DataStatus.Ok
                    : DataStatus.Failed;
                result.Data = _value.Value;
                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
            });
            return result;
        }

        public GetDataResult<ulong> GetCurrentId(Coroutine co, int keyType, string key = null)
        {
            var result = new GetDataResult<ulong>();
            mQueue.Add(async () =>
            {
                var _key = "getnextid:" + keyType;
                if (!string.IsNullOrEmpty(key))
                {
                    _key += ":" + key;
                }

                var _result = await mBucket.GetAsync<ulong>(_key);
                if (_result.Status != Couchbase.IO.ResponseStatus.Success || _result.Value < 100000000)
                {
                    result.Data = 100000000;
                }
                else
                {
                    result.Data = _result.Value;    
                }
                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
            });
            return result;
        }

        public GetDataResult<ulong> GetNextId(Coroutine co, int keyType, string key = null)
        {
            var result = new GetDataResult<ulong>();
            mQueue.Add(async () =>
            {
                var _key = "getnextid:" + keyType;
                if (!string.IsNullOrEmpty(key))
                {
                    _key += ":" + key;
                }

                var _result = await mBucket.IncrementAsync(_key);
                if (_result.Status != Couchbase.IO.ResponseStatus.Success || _result.Value < 100000000)
                {
                    var id = 100000000ul;
                    await mBucket.UpsertAsync(_key, id);
                    result.Data = id;
                }
                else
                {
                    result.Data = _result.Value;
                }
                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
            });
            return result;
        }

        public SetDataResult Set<T>(Coroutine co, DataCategory cat, string key, T t, SetOption option = SetOption.Normal, bool fireAndForgot = false) where T : IExtensible
        {
            var result = new SetDataResult();
            var _key = (int)cat + ":" + key;

            mCache.Set(_key, t);

            mQueue.Add(async () =>
            {
                Couchbase.IOperationResult<T> _result;
                if (option == SetOption.Normal)
                {
                    _result = await mBucket.UpsertAsync(_key, t);
                }
                else if (option == SetOption.SetIfExist)
                {
                    _result = await mBucket.ReplaceAsync(_key, t);
                }
                else
                {
                    _result = await mBucket.GetAsync<T>(key);
                    if (_result.Success)
                    {
                        result.Status = DataStatus.Failed;
                        mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                        return;
                    }

                    _result = await mBucket.InsertAsync(_key, t);
                }
                result.Status = _result.Success ? DataStatus.Ok : DataStatus.Failed;
                mServerAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
            });
            return result;
        }

        public SetDataResult Set<T>(Coroutine co, DataCategory cat, ulong key, T t, SetOption option = SetOption.Normal) where T : IExtensible
        {
            return Set(co, cat, key.ToString(), t, option);
        }

        public object Wait(Coroutine co, TimeSpan span)
        {
            mServerAgentBase.Wait(co, span);
            return null;
        }


        //public IEnumerator Test(Coroutine co)
        //{
        //    Logger.Debug("--------------123--------------");

        //    for (int i = 0; i < 100; ++i)
        //    {
        //        var r = GetNextId(co, 1, "abc");
        //        yield return r;
        //    }

        //    Logger.Debug("Test GetNextId ok **************************");

        //    for (int i = 0; i < 100; ++i)
        //    {
        //        var r = Set(co, DataCategory.Activity, (ulong)i, new DataContract.DBInt { Value = i }, SetOption.SetIfNotExist);
        //        yield return r;

        //        var k = Get<DataContract.DBInt>(co, DataCategory.Activity, (ulong)i);
        //        yield return k;
        //        if (k.Data == null)
        //        {
        //            Logger.Error("Test SetIfNotExist failed **************************");
        //            break;
        //        }
        //    }

        //    Logger.Debug("Test SetIfNotExist ok **************************");

        //    for (int i = 0; i < 100; ++i)
        //    {
        //        var r = Set(co, DataCategory.Activity, (ulong)i, new DataContract.DBInt { Value = i }, SetOption.Normal);
        //        yield return r;
        //    }

        //    Logger.Debug("Test Set Normal ok **************************");

        //    for (int i = 0; i < 100; ++i)
        //    {
        //        var r = Set(co, DataCategory.Activity, (ulong)i, new DataContract.DBInt { Value = i }, SetOption.SetIfNotExist);
        //        yield return r;

        //        if (r.Status == DataStatus.Ok)
        //        {
        //            Logger.Error("Test SetIfNotExist failed **************************");
        //            break;
        //        }
        //    }

        //    for (int i = 0; i < 100; ++i)
        //    {
        //        var r = Set(co, DataCategory.Activity, (ulong)i, new DataContract.DBInt { Value = i }, SetOption.SetIfExist);
        //        yield return r;

        //        if (r.Status != DataStatus.Ok)
        //        {
        //            Logger.Error("Test SetIfExist failed **************************");
        //            break;
        //        }
        //    }

        //    Logger.Debug("Test SetIfExist ok **************************");

        //    for (int i = 0; i < 100; ++i)
        //    {
        //        var r = Get<DataContract.DBInt>(co, DataCategory.Activity, (ulong)i);
        //        yield return r;
        //        if (r.Data == null)
        //        {
        //            Logger.Error("Test Get failed **************************");
        //            break;
        //        }
        //    }

        //    Logger.Debug("Test Get ok **************************");

        //    for (int i = 0; i < 100; ++i)
        //    {
        //        var r = Delete(co, DataCategory.Activity, (ulong)i);
        //        yield return r;

        //        var k = Get<DataContract.DBInt>(co, DataCategory.Activity, (ulong)i);
        //        yield return k;
        //        if (k.Data != null)
        //        {
        //            Logger.Error("Test Delete failed **************************");
        //            break;
        //        }
        //    }

        //    Logger.Debug("Test Delete ok **************************");

        //    for (int i = 0; i < 100; ++i)
        //    {
        //        var r = Set(co, DataCategory.Activity, (ulong)i, new DataContract.DBInt { Value = i }, SetOption.Normal);
        //        yield return r;


        //        var k = Get<DataContract.DBInt>(co, DataCategory.Activity, (ulong)i);
        //        yield return k;
        //        if (k.Data.Value != i)
        //        {
        //            Logger.Error("Test Set Normal failed **************************");
        //            break;
        //        }
        //    }

        //    Logger.Debug("Test Set Normal ok **************************");

        //    for (int i = 0; i < 100; ++i)
        //    {
        //        var r = GetAndDelete<DataContract.DBInt>(co, DataCategory.Activity, i.ToString());
        //        yield return r;
        //        if (r.Data.Value != i)
        //        {
        //            Logger.Error("Test GetAndDelete failed **************************");
        //        }

        //        r = Get<DataContract.DBInt>(co, DataCategory.Activity, (ulong)i);
        //        yield return r;
        //        if (r.Data != null)
        //        {
        //            Logger.Error("Test GetAndDelete failed **************************");
        //            break;
        //        }
        //    }

        //    Logger.Debug("Test GetAndDelete ok **************************");


        //    Logger.Error("Test all finished. **************************");
        //    yield break;
        //}
    }

#endif
}