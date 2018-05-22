#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Database;
using Scorpion;
using NLog;
using ProtoBuf;

#endregion

namespace Shared
{
    public interface ICharacterManager
    {
        void ForeachCharacter(Func<ICharacterController, bool> action);
    }

    public class CharacterManager<CT, DT, SDT> : ICharacterManager
        where CT : NodeBase, ICharacterControllerBase<DT, SDT>, ICharacterController, new()
        where DT : IExtensible
        where SDT : IExtensible
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly CharacterManager<CT, DT, SDT> sInstance = new CharacterManager<CT, DT, SDT>();
        public DataManager DB;
        public DataCategory mCategory;
        public Dictionary<ulong, DataItem> mDictionary = new Dictionary<ulong, DataItem>();
        private readonly List<ulong> mRemoveList = new List<ulong>();

        public static CharacterManager<CT, DT, SDT> Instance
        {
            get { return sInstance; }
        }

        public int CharacterCount()
        {
            return mDictionary.Count;
        }

        public IEnumerator CreateCharacterController(Coroutine coroutine,
                                                     ulong characterId,
                                                     AsyncReturnValue<CT> controller,
                                                     object[] args)
        {
            controller.Value = null;
            DataItem dataItem;
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                Logger.Error("Item already exist.");
                controller.Value = dataItem.Controller;
                yield break;
            }

            dataItem = new DataItem();

            dataItem.Controller = new CT();
            dataItem.Controller.State = CharacterState.Created;
            dataItem.Controller.InitByBase(characterId, args);
            dataItem.SimpleData = dataItem.Controller.GetSimpleData();
            dataItem.LastSaveTime = DateTime.Now;

            mDictionary.Add(characterId, dataItem);

            var co = CoroutineFactory.NewSubroutine(SaveData, coroutine, characterId, dataItem, true);
            if (co.MoveNext())
            {
                yield return co;
            }

            controller.Value = dataItem.Controller;
        }

        public IEnumerator DeleteCharacter(Coroutine coroutine, ulong id)
        {
            DataItem dataItem;
            if (mDictionary.TryGetValue(id, out dataItem))
            {
                var co = CoroutineFactory.NewSubroutine(DeleteData, coroutine, id, dataItem);
                if (co.MoveNext())
                {
                    yield return co;
                }

                Remove(id);
            }
        }

        private IEnumerator DeleteData(Coroutine coroutine, ulong id, DataItem item)
        {
            if (item.Controller == null)
            {
                yield break;
            }
            var co = CoroutineFactory.NewSubroutine(DeleteData, coroutine, mCategory, id.ToString());
            if (co.MoveNext())
            {
                yield return co;
            }

            co = CoroutineFactory.NewSubroutine(DeleteData, coroutine, mCategory, "__s_:" + id);
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        private IEnumerator DeleteData(Coroutine coroutine, DataCategory cat, string characterId)
        {
            var result = DB.Delete(coroutine, cat, characterId);
            yield return result;
            if (result.Status != DataStatus.Ok)
            {
                Logger.Error("Save data for character {0} failed.", characterId);
            }
        }

        /// <summary>
        ///     Get a character from cache, Not from DB.
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public CT GetCharacterControllerFromMemroy(ulong characterId)
        {
            DataItem dataItem;
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                return dataItem.Controller;
            }

            return default(CT);
        }

        public IEnumerator GetOrCreateCharacterController(Coroutine coroutine,
                                                          ulong characterId,
                                                          object[] args,
                                                          bool createIfNotExist,
                                                          AsyncReturnValue<CT> result)
        {
            if (result == null)
            {
                Logger.Error("AsyncReturnValue can not be null");
                yield break;
            }
            result.Value = null;
            if (DB == null)
            {
                Logger.Error("CharacterManager must initialize before use.");
                yield break;
            }

            DataItem dataItem;
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    result.Value = dataItem.Controller;
                    yield break;
                }
                mDictionary.Remove(characterId);
            }

            dataItem = new DataItem();
            var controller = new CT();
            controller.State = CharacterState.Created;

            var cat = mCategory;

            var data = DB.Get<DT>(coroutine, cat, characterId);
            yield return data;

            DataItem temp;
            if (mDictionary.TryGetValue(characterId, out temp))
            {
                if (dataItem.Controller != null)
                {
                    result.Value = dataItem.Controller;
                    yield break;
                }
                mDictionary.Remove(characterId);
            }

            // can not get data from db
            if (data.Data == null)
            {
                if (createIfNotExist)
                {
                    try
                    {
                        controller.InitByBase(characterId, args);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    result.Value = controller;
                    dataItem.Controller = controller;
                    dataItem.SimpleData = controller.GetSimpleData();
                    mDictionary.Add(characterId, dataItem);

                    // when we create new character, we should save it immediately.
                    var co = CoroutineFactory.NewSubroutine(SaveData, coroutine, characterId, dataItem, true);
                    if (co.MoveNext())
                    {
                        yield return co;
                    }

                    yield break;
                }

                Logger.Error("can not load character {0} 's data from db.", characterId);
                yield break;
            }

            try
            {
                controller.InitByDb(characterId, data.Data);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                yield break;
            }

            result.Value = controller;
            dataItem.Controller = controller;
            dataItem.SimpleData = controller.GetSimpleData();
            dataItem.LastSaveTime = DateTime.Now;
            mDictionary.Add(characterId, dataItem);
        }

        public IEnumerator GetSimpeData(Coroutine coroutine, ulong characterId, AsyncReturnValue<SDT> returnValue)
        {
            returnValue.Value = default(SDT);
            if (DB == null)
            {
                Logger.Error("CharacterManager must initialize before use.");
                yield break;
            }

            DataItem dataItem;
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    if (dataItem.Controller.DbDirty)
                    {
                        var simpeData = dataItem.Controller.GetSimpleData();
                        dataItem.SimpleData = simpeData;
                        returnValue.Value = simpeData;
                        yield break;
                    }
                }

                returnValue.Value = dataItem.SimpleData;
                yield break;
            }

            var cat = mCategory;
            var data = DB.Get<SDT>(coroutine, cat, "__s_:" + characterId);
            yield return data;

            // check again.
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    if (dataItem.Controller.DbDirty)
                    {
                        var simpeData = dataItem.Controller.GetSimpleData();
                        dataItem.SimpleData = simpeData;
                        returnValue.Value = simpeData;
                        yield break;
                    }
                }

                returnValue.Value = dataItem.SimpleData;
                yield break;
            }

            if (data.Data != null)
            {
                dataItem = new DataItem();
                dataItem.SimpleData = data.Data;
                dataItem.LastSaveTime = DateTime.Now;
                mDictionary.Add(characterId, dataItem);
                returnValue.Value = data.Data;
                yield break;
            }

            returnValue.Value = default(SDT);
        }

        private IEnumerator GetSimpeData(Coroutine coroutine, ulong characterId, Action<SDT> callback)
        {
            if (callback == null)
            {
                Logger.Error("Callback can not be null");
                yield break;
            }

            if (DB == null)
            {
                Logger.Error("CharacterManager must initialize before use.");
                callback(default(SDT));
                yield break;
            }

            DataItem dataItem;
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    if (dataItem.Controller.DbDirty)
                    {
                        var simpeData = dataItem.Controller.GetSimpleData();
                        dataItem.SimpleData = simpeData;
                        callback(simpeData);
                        yield break;
                    }
                }

                callback(dataItem.SimpleData);
                yield break;
            }

            var cat = mCategory;
            var data = DB.Get<SDT>(coroutine, cat, "__s_:" + characterId);
            yield return data;

            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    if (dataItem.Controller.DbDirty)
                    {
                        var simpeData = dataItem.Controller.GetSimpleData();
                        dataItem.SimpleData = simpeData;
                        callback(simpeData);
                        yield break;
                    }
                }

                callback(dataItem.SimpleData);
                yield break;
            }

            if (data.Data != null)
            {
                dataItem = new DataItem();
                dataItem.SimpleData = data.Data;
                dataItem.LastSaveTime = DateTime.Now;
                mDictionary.Add(characterId, dataItem);
                callback(data.Data);
                yield break;
            }

            callback(default(SDT));
        }

        public void GetSimpeData(ulong characterId, Action<SDT> callback)
        {
            CoroutineFactory.NewCoroutine(GetSimpeData, characterId, callback).MoveNext();
        }

        public void Init(DataManager db, DataCategory cat)
        {
            DB = db;
            mCategory = cat;
            CoroutineFactory.NewCoroutine(SaveAtEvery55Minutes).MoveNext();
        }

        public virtual void LookCharacters()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Character Count: {0}", mDictionary.Count));
            sb.AppendLine(string.Format("Character only has SimpleData"));
            var c = 0;
            foreach (var kv in mDictionary)
            {
                if (kv.Value.Controller == null)
                {
                    sb.AppendLine(string.Format("Id: {0}", kv.Key));
                    c++;
                }
            }
            sb.AppendLine(string.Format("Total Count: {0}", c));

            c = 0;
            sb.AppendLine(string.Format("Character only has Controller"));
            foreach (var kv in mDictionary)
            {
                if (kv.Value.Controller != null)
                {
                    sb.AppendLine(string.Format("Id: {0}", kv.Key));
                    c++;
                }
            }
            sb.AppendLine(string.Format("Total Count: {0}", c));
            sb.AppendLine("_______________________________________");

            Logger.Info(sb.ToString());
        }

        public int OnlineCharacterCount()
        {
            return mDictionary.Count;
        }

        private void Remove(ulong id)
        {
            DataItem item;
            if (mDictionary.TryGetValue(id, out item))
            {
                mDictionary.Remove(id);
            }
        }

        public IEnumerator RemoveCharacter(Coroutine co, ulong id)
        {
            DataItem dataItem;
            if (mDictionary.TryGetValue(id, out dataItem))
            {
                mDictionary.Remove(id);
                var sub = CoroutineFactory.NewSubroutine(SaveData, co, id, dataItem, true);
                if (sub.MoveNext())
                {
                    yield return sub;
                }

                try
                {
                    dataItem.Controller.OnDestroy();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Dispose character error.");
                }
            }
        }

        public IEnumerator SaveAllCharacter(Coroutine coroutine, TimeSpan s = default(TimeSpan))
        {
            var clone = new Dictionary<ulong, DataItem>(mDictionary);
            foreach (var ct in clone)
            {
                var id = ct.Key;
                var item = ct.Value;
                if (item.Controller != null)
                {
                    if (item.Controller.DbDirty)
                    {
                        Logger.Info("Save data for character {0}.", id);

                        var co = CoroutineFactory.NewSubroutine(SaveData, coroutine, ct.Key, item, true);
                        if (co.MoveNext())
                        {
                            yield return co;
                        }

                        if (s != default(TimeSpan))
                        {
                            yield return DB.Wait(coroutine, s);
                        }
                    }
                }
            }
        }

        private IEnumerator SaveAtEvery55Minutes(Coroutine co)
        {
            while (true)
            {
                var t = DateTime.Now;
                yield return DB.Wait(co, new DateTime(t.Year, t.Month, t.Day, t.Hour, 55, 0).AddHours(1) - t);
                yield return SaveAllCharacter(co, TimeSpan.FromMilliseconds(1));
            }
        }

        private IEnumerator SaveData(Coroutine coroutine, ulong id, DataItem item, bool forceSave = false)
        {
            if (item.Controller == null)
            {
                yield break;
            }

            if (forceSave || item.Controller.DbDirty)
            {
                Logger.Info("Save data for character {0}.", id);


                item.SimpleData = item.Controller.GetSimpleData();
                var data = item.Controller.GetData();
                item.Controller.OnSaveData(data, item.SimpleData);

                item.Controller.CleanDbDirty();

                item.LastSaveTime = DateTime.Now;

                var co = CoroutineFactory.NewSubroutine(SaveData, coroutine, mCategory, id.ToString(), data);
                if (co.MoveNext())
                {
                    yield return co;
                }

                co = CoroutineFactory.NewSubroutine(SaveData, coroutine, mCategory, "__s_:" + id, item.SimpleData);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        public IEnumerator SaveDataForClone(Coroutine coroutine, ulong id, DataItem item, bool forceSave = false)
        {
            if (item.Controller == null)
            {
                yield break;
            }

            if (forceSave || item.Controller.DbDirty)
            {
                Logger.Info("Save data for character {0}.", id);


                item.SimpleData = item.Controller.GetSimpleData();
                var data = item.Controller.GetData();
                item.Controller.OnSaveData(data, item.SimpleData);

                item.Controller.CleanDbDirty();

                item.LastSaveTime = DateTime.Now;

                var co = CoroutineFactory.NewSubroutine(SaveData, coroutine, mCategory, id.ToString(), data);
                if (co.MoveNext())
                {
                    yield return co;
                }

                co = CoroutineFactory.NewSubroutine(SaveData, coroutine, mCategory, "__s_:" + id, item.SimpleData);
                if (co.MoveNext())
                {
                    yield return co;
                }
                //重要，克隆出来的不存cache
                Remove(id);
            }
        }

        private IEnumerator SaveData<T>(Coroutine coroutine, DataCategory cat, string characterId, T data)
            where T : IExtensible
        {
            var result = DB.Set(coroutine, cat, characterId, data);
            yield return result;
            if (result.Status != DataStatus.Ok)
            {
                Logger.Error("Save data for character {0} failed.", characterId);
            }
        }

        public void Tick()
        {
            mRemoveList.Clear();
            foreach (var item in mDictionary)
            {
                if (item.Value.Controller == null)
                {
                    if ((DateTime.Now - item.Value.LastSaveTime).TotalMinutes > 30)
                    {
                        mRemoveList.Add(item.Key);
                    }
                    continue;
                }

                item.Value.Controller.Tick();
                try
                {
                    if (DateTime.Now - item.Value.LastSaveTime > TimeSpan.FromSeconds(60))
                    {
                        CoroutineFactory.NewCoroutine(SaveData, item.Key, item.Value, false).MoveNext();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Save data for character {0} error", item.Key);
                }
            }

            foreach (var id in mRemoveList)
            {
                Remove(id);
            }
        }

        public bool UpdateSimpleData(ulong characterId)
        {
            DataItem dataItem;
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    var simpeData = dataItem.Controller.GetSimpleData();
                    dataItem.SimpleData = simpeData;
                    return true;
                }
            }

            return false;
        }

        public void ForeachCharacter(Func<ICharacterController, bool> action)
        {
            foreach (var dataItem in mDictionary)
            {
                try
                {
                    if (dataItem.Value.Controller == null)
                    {
                        //Logger.Error("ForeachCharacter is null characterId={0}", dataItem.Key);
                        continue;
                    }

                    if (dataItem.Value.Controller.State != CharacterState.Connected)
                    {
                        continue;
                    }

                    if (!action(dataItem.Value.Controller))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "ForeachCharacter got a exception.");
                }
            }
        }

        public class DataItem
        {
            public CT Controller;
            public DateTime LastSaveTime;
            public SDT SimpleData;
        }
    }

    public class CharacterManager<CT, DT, SDT, VDT> : ICharacterManager
        where CT : NodeBase, ICharacterControllerBase<DT, SDT, VDT>, ICharacterController, new()
        where DT : IExtensible
        where SDT : IExtensible
        where VDT : IExtensible, new()
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly CharacterManager<CT, DT, SDT, VDT> sInstance = new CharacterManager<CT, DT, SDT, VDT>();
        public DataManager DB;
        public DataCategory mCategory;
        public Dictionary<ulong, DataItem> mDictionary = new Dictionary<ulong, DataItem>();
        private readonly List<ulong> mRemoveList = new List<ulong>();
        public Dictionary<ulong, Queue<Func<VDT, VDT>>> mVolatileQueues = new Dictionary<ulong, Queue<Func<VDT, VDT>>>();

        public static CharacterManager<CT, DT, SDT, VDT> Instance
        {
            get { return sInstance; }
        }

        public int CharacterCount()
        {
            return mDictionary.Count;
        }

        private IEnumerator ClearUselessControllers(Coroutine co)
        {
            while (true)
            {
                var removal =
                    mDictionary.Where(
                        i => i.Value.Controller == null && (DateTime.Now - i.Value.LastSaveTime).TotalMinutes > 30)
                        .ToArray();
                foreach (var item in removal)
                {
                    Remove(item.Key);
                }

                yield return DB.Wait(co, TimeSpan.FromMinutes(5));
            }
        }

        public IEnumerator CreateCharacterController(Coroutine coroutine,
                                                     ulong characterId,
                                                     AsyncReturnValue<CT> controller,
                                                     object[] args)
        {
            DataItem dataItem;
            controller.Value = default(CT);
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                Logger.Error("Item already exist.");
                controller.Value = dataItem.Controller;
                yield break;
            }
            dataItem = new DataItem();

            dataItem.Controller = new CT();
            dataItem.Controller.State = CharacterState.Created;
            dataItem.Controller.InitByBase(characterId, args);
            dataItem.SimpleData = dataItem.Controller.GetSimpleData();
            dataItem.LastSaveTime = DateTime.Now;

            mDictionary.Add(characterId, dataItem);

            try
            {
                dataItem.Controller.LoadFinished();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            var co = CoroutineFactory.NewSubroutine(SaveData, coroutine, characterId, dataItem, true);
            if (co.MoveNext())
            {
                yield return co;
            }

            controller.Value = dataItem.Controller;
        }

        public IEnumerator DeleteCharacter(Coroutine coroutine, ulong id)
        {
            DataItem dataItem;
            if (mDictionary.TryGetValue(id, out dataItem))
            {
                Remove(id);
                var co = CoroutineFactory.NewSubroutine(DeleteData, coroutine, id, dataItem);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        private IEnumerator DeleteData(Coroutine coroutine, ulong id, DataItem item)
        {
            if (item.Controller == null)
            {
                yield break;
            }
            var co = CoroutineFactory.NewSubroutine(DeleteData, coroutine, mCategory, id.ToString());
            if (co.MoveNext())
            {
                yield return co;
            }

            co = CoroutineFactory.NewSubroutine(DeleteData, coroutine, mCategory, "__s_:" + id);
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        private IEnumerator DeleteData(Coroutine coroutine, DataCategory cat, string characterId)
        {
            var result = DB.Delete(coroutine, cat, characterId);
            yield return result;
            if (result.Status != DataStatus.Ok)
            {
                Logger.Error("Save data for character {0} failed.", characterId);
            }
        }

        /// <summary>
        ///     Get a character from cache, Not from DB.
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public CT GetCharacterControllerFromMemroy(ulong characterId)
        {
            DataItem dataItem;
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                return dataItem.Controller;
            }

            return default(CT);
        }

        public IEnumerator GetOrCreateCharacterController(Coroutine coroutine,
                                                          ulong characterId,
                                                          object[] args,
                                                          bool createIfNotExist,
                                                          AsyncReturnValue<CT> result)
        {
            if (result == null)
            {
                Logger.Error("AsyncReturnValue can not be null");
                yield break;
            }
            result.Value = default(CT);

            if (DB == null)
            {
                Logger.Error("CharacterManager must initialize before use.");
                yield break;
            }

            DataItem dataItem;
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    result.Value = dataItem.Controller;
                    yield break;
                }
                mDictionary.Remove(characterId);
            }

            dataItem = new DataItem();
            var controller = new CT();
            controller.State = CharacterState.Created;

            var cat = mCategory;

            var data = DB.Get<DT>(coroutine, cat, characterId);
            yield return data;

            DataItem temp;
            if (mDictionary.TryGetValue(characterId, out temp))
            {
                if (dataItem.Controller != null)
                {
                    result.Value = dataItem.Controller;
                    yield break;
                }
                mDictionary.Remove(characterId);
            }

            // can not get data from db
            if (data.Data == null)
            {
                if (createIfNotExist)
                {
                    try
                    {
                        controller.InitByBase(characterId, args);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                    result.Value = controller;
                    dataItem.Controller = controller;
                    dataItem.SimpleData = controller.GetSimpleData();
                    mDictionary.Add(characterId, dataItem);

                    try
                    {
                        controller.LoadFinished();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    // when we create new character, we should save it immediately.
                    var co = CoroutineFactory.NewSubroutine(SaveData, coroutine, characterId, dataItem, true);
                    if (co.MoveNext())
                    {
                        yield return co;
                    }

                    yield break;
                }

                Logger.Error("can not load character {0} 's data from db.", characterId);
                yield break;
            }

            try
            {
                controller.InitByDb(characterId, data.Data);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            result.Value = controller;
            dataItem.Controller = controller;
            dataItem.SimpleData = controller.GetSimpleData();
            dataItem.LastSaveTime = DateTime.Now;
            mDictionary.Add(characterId, dataItem);

            var volatileData = DB.GetAndDelete<VDT>(coroutine, cat, "__v_:" + characterId);
            yield return volatileData;
            if (volatileData.Data != null)
            {
                try
                {
                    controller.ApplyVolatileData(volatileData.Data);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            try
            {
                controller.LoadFinished();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public IEnumerator GetSimpeData(Coroutine coroutine, ulong characterId, AsyncReturnValue<SDT> returnValue)
        {
            returnValue.Value = default(SDT);
            if (DB == null)
            {
                Logger.Error("CharacterManager must initialize before use.");
                yield break;
            }

            DataItem dataItem;
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    if (dataItem.Controller.DbDirty)
                    {
                        var simpeData = dataItem.Controller.GetSimpleData();
                        dataItem.SimpleData = simpeData;
                        returnValue.Value = simpeData;
                        yield break;
                    }
                }

                returnValue.Value = dataItem.SimpleData;
                yield break;
            }

            var cat = mCategory;
            var data = DB.Get<SDT>(coroutine, cat, "__s_:" + characterId);
            yield return data;

            // check again.
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    if (dataItem.Controller.DbDirty)
                    {
                        var simpeData = dataItem.Controller.GetSimpleData();
                        dataItem.SimpleData = simpeData;
                        returnValue.Value = simpeData;
                        yield break;
                    }
                }

                returnValue.Value = dataItem.SimpleData;
                yield break;
            }

            if (data.Data != null)
            {
                dataItem = new DataItem();
                dataItem.SimpleData = data.Data;
                dataItem.LastSaveTime = DateTime.Now;
                mDictionary.Add(characterId, dataItem);
                returnValue.Value = data.Data;
                yield break;
            }

            returnValue.Value = default(SDT);
        }

        private IEnumerator GetSimpeData(Coroutine coroutine, ulong characterId, Action<SDT> callback)
        {
            if (callback == null)
            {
                Logger.Error("Callback can not be null");
                yield break;
            }

            if (DB == null)
            {
                Logger.Error("CharacterManager must initialize before use.");
                callback(default(SDT));
                yield break;
            }

            DataItem dataItem;
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    if (dataItem.Controller.DbDirty)
                    {
                        var simpeData = dataItem.Controller.GetSimpleData();
                        dataItem.SimpleData = simpeData;
                        callback(simpeData);
                        yield break;
                    }
                }

                callback(dataItem.SimpleData);
                yield break;
            }

            var cat = mCategory;
            var data = DB.Get<SDT>(coroutine, cat, "__s_:" + characterId);
            yield return data;

            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    if (dataItem.Controller.DbDirty)
                    {
                        var simpeData = dataItem.Controller.GetSimpleData();
                        dataItem.SimpleData = simpeData;
                        callback(simpeData);
                        yield break;
                    }
                }

                callback(dataItem.SimpleData);
                yield break;
            }

            if (data.Data != null)
            {
                dataItem = new DataItem();
                dataItem.SimpleData = data.Data;
                dataItem.LastSaveTime = DateTime.Now;
                mDictionary.Add(characterId, dataItem);
                callback(data.Data);
                yield break;
            }

            callback(default(SDT));
        }

        public void GetSimpeData(ulong characterId, Action<SDT> callback)
        {
            CoroutineFactory.NewCoroutine(GetSimpeData, characterId, callback).MoveNext();
        }

        public void Init(DataManager db, DataCategory cat)
        {
            DB = db;
            mCategory = cat;
            CoroutineFactory.NewCoroutine(SaveAtEvery55Minutes).MoveNext();
            CoroutineFactory.NewCoroutine(ClearUselessControllers).MoveNext();
        }

        public virtual void LookCharacters()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Character Count: {0}", mDictionary.Count));
            sb.AppendLine(string.Format("Character only has SimpleData"));
            var c = 0;
            foreach (var kv in mDictionary)
            {
                if (kv.Value.Controller == null)
                {
                    sb.AppendLine(string.Format("Id: {0}", kv.Key));
                    c++;
                }
            }
            sb.AppendLine(string.Format("Total Count: {0}", c));

            c = 0;
            sb.AppendLine(string.Format("Character only has Controller"));
            foreach (var kv in mDictionary)
            {
                if (kv.Value.Controller != null)
                {
                    sb.AppendLine(string.Format("Id: {0}", kv.Key));
                    c++;
                }
            }
            sb.AppendLine(string.Format("Total Count: {0}", c));
            sb.AppendLine("_______________________________________");

            Logger.Info(sb.ToString());
        }

        public void ModifyVolatileData(ulong characterId, DataCategory cat, Func<VDT, VDT> call)
        {
            CoroutineFactory.NewCoroutine(ModifyVolatileDataImpl, characterId, cat, call)
                .MoveNext();
        }

        private IEnumerator ModifyVolatileDataImpl(Coroutine coroutine,
                                                   ulong characterId,
                                                   DataCategory cat,
                                                   Func<VDT, VDT> call)
        {
            Queue<Func<VDT, VDT>> q;
            // try get lock
            if (mVolatileQueues.TryGetValue(characterId, out q))
            {
                q.Enqueue(call);
                yield break;
            }

            // lock
            q = new Queue<Func<VDT, VDT>>();
            mVolatileQueues.Add(characterId, q);
            q.Enqueue(call);

            var volatileData = DB.GetAndDelete<VDT>(coroutine, cat, "__v_:" + characterId);
            yield return volatileData;

            var result = default(VDT);
            while (q.Count > 0)
            {
                var callback = q.Dequeue();
                if (volatileData.Data != null)
                {
                    result = callback(volatileData.Data);
                }
                else
                {
                    result = callback(new VDT());
                }

                volatileData.Data = result;
            }

            // release lock
            mVolatileQueues.Remove(characterId);

            DataItem dataItem;
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    dataItem.Controller.ApplyVolatileData(result);
                    yield break;
                }
            }

            var setVolatileData = DB.Set(coroutine, cat, "__v_:" + characterId, result);
            yield return setVolatileData;
        }

        public int OnlineCharacterCount()
        {
            return mDictionary.Count;
        }

        private void Remove(ulong id)
        {
            DataItem item;
            if (mDictionary.TryGetValue(id, out item))
            {
                mDictionary.Remove(id);
            }
        }

        public IEnumerator RemoveCharacter(Coroutine co, ulong id)
        {
            DataItem dataItem;
            if (mDictionary.TryGetValue(id, out dataItem))
            {
                mDictionary.Remove(id);

                var sub = CoroutineFactory.NewSubroutine(SaveData, co, id, dataItem, false);
                if (sub.MoveNext())
                {
                    yield return sub;
                }

                try
                {
                    if (dataItem != null && dataItem.Controller != null)
                        dataItem.Controller.OnDestroy();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Dispose character error.");
                }
            }
        }

        public IEnumerator SaveAllCharacter(Coroutine coroutine, TimeSpan s = default(TimeSpan))
        {
            var clone = new Dictionary<ulong, DataItem>(mDictionary);
            foreach (var ct in clone)
            {
                var id = ct.Key;
                var item = ct.Value;
                if (item.Controller != null)
                {
                    if (item.Controller.DbDirty)
                    {
                        Logger.Info("Save data for character {0}.", id);

                        var co = CoroutineFactory.NewSubroutine(SaveData, coroutine, id, item, true);
                        if (co.MoveNext())
                        {
                            yield return co;
                        }

                        if (s != default(TimeSpan))
                        {
                            yield return DB.Wait(coroutine, s);
                        }
                    }
                }
            }
        }

        private IEnumerator SaveAtEvery55Minutes(Coroutine co)
        {
            while (true)
            {
                var t = DateTime.Now;
                yield return DB.Wait(co, new DateTime(t.Year, t.Month, t.Day, t.Hour, 55, 0).AddHours(1) - t);
                yield return SaveAllCharacter(co, TimeSpan.FromMilliseconds(1));
            }
        }

        public IEnumerator SaveData(Coroutine coroutine, ulong id, DataItem item, bool forceSave = false)
        {
            if (item.Controller == null)
            {
                yield break;
            }

            if (forceSave || item.Controller.DbDirty)
            {
                Logger.Info("Save data for character {0}.", id);


                item.SimpleData = item.Controller.GetSimpleData();
                var data = item.Controller.GetData();
                item.Controller.OnSaveData(data, item.SimpleData);

                item.Controller.CleanDbDirty();

                item.LastSaveTime = DateTime.Now;

                var co = CoroutineFactory.NewSubroutine(SaveData, coroutine, mCategory, "__s_:" + id, item.SimpleData);
                if (co.MoveNext())
                {
                    yield return co;
                }

                co = CoroutineFactory.NewSubroutine(SaveData, coroutine, mCategory, id.ToString(), data);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        public IEnumerator SaveDataForClone(Coroutine coroutine, ulong id, DataItem item, bool forceSave = false)
        {
             if (item.Controller == null)
            {
                yield break;
            }

            if (forceSave || item.Controller.DbDirty)
            {
                Logger.Info("Save data for character {0}.", id);


                item.SimpleData = item.Controller.GetSimpleData();
                var data = item.Controller.GetData();
                item.Controller.OnSaveData(data, item.SimpleData);

                item.Controller.CleanDbDirty();

                item.LastSaveTime = DateTime.Now;

                var co = CoroutineFactory.NewSubroutine(SaveData, coroutine, mCategory, "__s_:" + id, item.SimpleData);
                if (co.MoveNext())
                {
                    yield return co;
                }

                co = CoroutineFactory.NewSubroutine(SaveData, coroutine, mCategory, id.ToString(), data);
                if (co.MoveNext())
                {
                    yield return co;
                }
            
                //重要，克隆出来的不存cache
                Remove(id);
            }
        }



        private IEnumerator SaveData<T>(Coroutine coroutine, DataCategory cat, string characterId, T data)
            where T : IExtensible
        {
            var result = DB.Set(coroutine, cat, characterId, data);
            yield return result;
            if (result.Status != DataStatus.Ok)
            {
                Logger.Error("Save data for character {0} failed.", characterId);
            }
        }

        public void Tick()
        {
            mRemoveList.Clear();
            foreach (var item in mDictionary)
            {
                if (item.Value.Controller == null)
                {
                    if ((DateTime.Now - item.Value.LastSaveTime).TotalMinutes > 30)
                    {
                        mRemoveList.Add(item.Key);
                    }
                    continue;
                }

                item.Value.Controller.Tick();

                try
                {
                    if (DateTime.Now - item.Value.LastSaveTime > TimeSpan.FromSeconds(60))
                    {
                        CoroutineFactory.NewCoroutine(SaveData, item.Key, item.Value, false).MoveNext();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Save data for character {0} error", item.Key);
                }
            }

            foreach (var id in mRemoveList)
            {
                Remove(id);
            }
        }

        public bool UpdateSimpleData(ulong characterId)
        {
            DataItem dataItem;
            if (mDictionary.TryGetValue(characterId, out dataItem))
            {
                if (dataItem.Controller != null)
                {
                    var simpeData = dataItem.Controller.GetSimpleData();
                    dataItem.SimpleData = simpeData;
                    return true;
                }
            }

            return false;
        }

        public void ForeachCharacter(Func<ICharacterController, bool> action)
        {
            foreach (var dataItem in mDictionary)
            {
                try
                {
                    if (dataItem.Value.Controller == null)
                    {
                        continue;
                    }

                    if (dataItem.Value.Controller.State != CharacterState.Connected)
                    {
                        continue;
                    }

                    if (!action(dataItem.Value.Controller))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "ForeachCharacter got a exception.");
                }
            }
        }

        public class DataItem
        {
            public CT Controller;
            public DateTime LastSaveTime;
            public SDT SimpleData;
        }
    }


    public interface ICharacterSimpleManager
    {
    }

    public class CharacterSimpleManager<SDT, VDT> : ICharacterSimpleManager
    {
    }
}