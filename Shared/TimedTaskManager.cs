#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Database;
using DataContract;
using DataTable;
using Scorpion;
using NLog;

#endregion

namespace Shared
{
    public class TimedTaskManager
    {
        private Logger Logger;
        private ICharacterManager mCharacterManager;
        private DataCategory mDataCategory;
        private DataManager mDbManager;
        private readonly Dictionary<int, TimedTaskItem> mEvents = new Dictionary<int, TimedTaskItem>();
        private bool mInitFinished;
        private Action<int> mServerAction;
        private int mServerId;
        private TimedTasks mServerTaskData;

        private DateTime AddDuration(DateTime time, int type, int value)
        {
            switch (type)
            {
                case 0:
                    return time.AddHours(value);
                case 1:
                    return time.AddDays(value);
                case 2:
                    return time.AddMonths(value);
                case 3:
                    return time.AddYears(value);
            }

            throw new ArgumentOutOfRangeException("type is unknown.");
        }

        public void ApplyCachedServerTasks()
        {
            foreach (var item in mEvents)
            {
                if (!item.Value.Enable)
                {
                    continue;
                }

                try
                {
                    ApplyServerTaskes(item.Value);
                }
                catch (Exception ex)
                {
                    Trace.TraceInformation(ex.ToString());
                }
            }
        }

        public void ApplyCachedTasks(ICharacterController character)
        {
            foreach (var item in mEvents)
            {
                if (!item.Value.Enable)
                {
                    continue;
                }

                try
                {
                    ApplyTaskes(character, item.Value);
                }
                catch (Exception ex)
                {
                    Trace.TraceInformation(ex.ToString());
                }
            }
        }

        public void ApplyServerTaskes(TimedTaskItem item)
        {
            var index = mServerTaskData.Items.FindIndex(t => t.Id == item.Id);
            if (index != -1)
            {
                var data = mServerTaskData.Items[index];
                var count = item.CacheCount;
                while (data.Stamp < item.Stamp && count > 0)
                {
                    try
                    {
                        mServerAction(item.Id);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "execute 1 ApplyServerTaskes id {0} error.", item.Id);
                    }
                    data.Stamp++;
                    count--;
                }
                data.Stamp = item.Stamp;
            }
            else
            {
                var data = new DataContract.TimedTaskItem
                {
                    Id = item.Id,
                    Stamp = 0
                };
                var count = item.CacheCount;
                while (data.Stamp < item.Stamp && count > 0)
                {
                    try
                    {
                        mServerAction(item.Id);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "execute 2 ApplyServerTaskes id {0} error.", item.Id);
                    }
                    data.Stamp++;
                    count--;
                }
                data.Stamp = item.Stamp;

                mServerTaskData.Items.Add(data);
            }
        }

        public void ApplyTaskes(ICharacterController character, TimedTaskItem item)
        {
            if (character.GetTimedTasks() == null)
            {
                return;
            }
            var index = character.GetTimedTasks().FindIndex(t => t.Id == item.Id);
            if (index != -1)
            {
                var data = character.GetTimedTasks()[index];
                var count = item.CacheCount;
                while (data.Stamp < item.Stamp && count > 0)
                {
                    try
                    {
                        Logger.Info("ApplyTaskes {0} id:{1} stamp:{2} ss:{3}", character, data.Id, data.Stamp,
                            item.Stamp);
                        item.Action(character, item.Stamp - data.Stamp);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "execute 1 ApplyTasks id {0} error", item.Id);
                    }
                    data.Stamp++;
                    count--;
                }
                data.Stamp = item.Stamp;
                var nodeBase = character as NodeBase;
                if (nodeBase != null)
                {
                    nodeBase.MarkDbDirty();
                }
            }
            else
            {
                var data = new DataContract.TimedTaskItem
                {
                    Id = item.Id,
                    Stamp = 0
                };
                var count = item.CacheCount;
                while (data.Stamp < item.Stamp && count > 0)
                {
                    try
                    {
                        Logger.Info("ApplyTaskes {0} id:{1} stamp:{2} ss:{3}", character, data.Id, data.Stamp,
                            item.Stamp);
                        item.Action(character, item.Stamp - data.Stamp);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "execute 2 ApplyTasks id {0} error", item.Id);
                    }
                    data.Stamp++;
                    count--;
                }
                data.Stamp = item.Stamp;
                character.GetTimedTasks().Add(data);
                var nodeBase = character as NodeBase;
                if (nodeBase != null)
                {
                    nodeBase.MarkDbDirty();
                }
            }
        }

        public void FlushAll()
        {
            var tasks = new TimedTasks();
            foreach (var item in mEvents)
            {
                tasks.Items.Add(new DataContract.TimedTaskItem
                {
                    Id = item.Value.Id,
                    Last = item.Value.Last.ToBinary(),
                    Stamp = item.Value.Stamp
                });
            }

            CoroutineFactory.NewCoroutine(FlushAllImpl, tasks).MoveNext();
        }

        private IEnumerator FlushAllImpl(Coroutine coroutine, TimedTasks tasks)
        {
            var result = mDbManager.Set(coroutine, mDataCategory, "timedtasks:" + mServerId, tasks);
            yield return result;

            result = mDbManager.Set(coroutine, DataCategory.TimeTask, mServerId.ToString(), mServerTaskData);
            yield return result;
        }

        public void Init(DataManager db,
                         ICharacterManager characterManager,
                         DataCategory category,
                         int serverId,
                         Logger logger,
                         Action<int> action)
        {
            mDataCategory = category;
            mServerId = serverId;
            Logger = logger;
            mDbManager = db;
            mCharacterManager = characterManager;
            mServerAction = action;
            RegisterTasks();
            CoroutineFactory.NewCoroutine(InitImpl).MoveNext();
        }

        private IEnumerator InitImpl(Coroutine coroutine)
        {
            var dbData = mDbManager.Get<TimedTasks>(coroutine, mDataCategory, "timedtasks:" + mServerId);
            yield return dbData;

            var tasks = dbData.Data;

            Table.ForeachEvent(record =>
            {
                var stamp = (tasks == null || tasks.Items.All(item => item.Id != record.Id))
                    ? 0
                    : tasks.Items.Find(item => item.Id == record.Id).Stamp;
                var last = (tasks == null || tasks.Items.All(item => item.Id != record.Id))
                    ? DateTime.Parse(record.TriggerTime)
                    : DateTime.FromBinary(tasks.Items.Find(item => item.Id == record.Id).Last);

//                 while (!mDbManager.AtomGetSet("tt:" + record.Id, old =>
//                 {
//                     if (string.IsNullOrEmpty(old))
//                     {
//                         return stamp.ToString();
//                     }
//                     var s = int.Parse(old);
//                     if (s > stamp)
//                     {
//                         stamp = s;
//                     }
//                     else if (s < stamp)
//                     {
//                         return stamp.ToString();
//                     }
// 
//                     return string.Empty;
//                 })) ;

                mEvents.Add(record.Id, new TimedTaskItem
                {
                    Id = record.Id,
                    Action = (character, c) => character.ApplyEvent(record.Id, record.Action, c),
                    CacheCount = record.CacheCount,
                    Duration = record.DurationParam,
                    DurationType = record.DurationType,
                    Stamp = stamp,
                    Time = DateTime.Parse(record.TriggerTime),
                    Last = last,
                    Enable = record.Enable != 0
                });
                return true;
            });

            var hasUninitialized = true;
            while (hasUninitialized)
            {
                hasUninitialized = false;
                foreach (var item in mEvents)
                {
                    if (!item.Value.Enable)
                    {
                        continue;
                    }

                    var next = AddDuration(item.Value.Last, item.Value.DurationType, item.Value.Duration);
                    if (DateTime.Now > next)
                    {
                        hasUninitialized = true;

                        item.Value.Stamp++;

                        // avoid tolerances
                        item.Value.Last = next;
                        var s = item.Value.Last.Second - item.Value.Time.Second;
                        item.Value.Last += TimeSpan.FromSeconds(s);
                        var m = item.Value.Last.Minute - item.Value.Time.Minute;
                        item.Value.Last += TimeSpan.FromMinutes(m);
                    }
                }
            }

            var serverData = mDbManager.Get<TimedTasks>(coroutine, DataCategory.TimeTask, mServerId.ToString());
            yield return serverData;

            mServerTaskData = serverData.Data ?? new TimedTasks();
            ApplyCachedServerTasks();

            FlushAll();

            mInitFinished = true;
        }

        internal void InitTasks(ICharacterController character)
        {
            foreach (var item in mEvents)
            {
                if (!item.Value.Enable)
                {
                    continue;
                }

                var data = new DataContract.TimedTaskItem
                {
                    Id = item.Value.Id,
                    Stamp = item.Value.Stamp
                };

                character.GetTimedTasks().Add(data);
            }
        }

        public void RegisterTasks()
        {
        }

        public void Tick()
        {
            if (!mInitFinished)
            {
                return;
            }

            var ditry = false;
            try
            {
                foreach (var item in mEvents)
                {
                    if (!item.Value.Enable)
                    {
                        continue;
                    }

                    var timedTaskItem = item.Value;

                    var next = AddDuration(item.Value.Last, item.Value.DurationType, item.Value.Duration);
                    if (DateTime.Now > next)
                    {
                        ditry = true;
                        item.Value.Stamp++;

                        ApplyServerTaskes(item.Value);

                        if (mCharacterManager != null)
                        {
                            mCharacterManager.ForeachCharacter(character =>
                            {
                                if (character.Online)
                                {
                                    ApplyTaskes(character, timedTaskItem);
                                }

                                return true;
                            });
                        }

                        // avoid tolerances
                        item.Value.Last = next;
                        var s = item.Value.Last.Second - item.Value.Time.Second;
                        item.Value.Last += TimeSpan.FromSeconds(s);
                        var m = item.Value.Last.Minute - item.Value.Time.Minute;
                        item.Value.Last += TimeSpan.FromMinutes(m);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("TimedTaskManager.Run " + e.Message);
            }
            finally
            {
                if (ditry)
                {
                    FlushAll();
                }
            }
        }

        public class TimedTaskItem
        {
            public Action<ICharacterController, int> Action { get; set; }
            public int CacheCount { get; set; }
            public int Duration { get; set; }
            public int DurationType { get; set; }
            public bool Enable { get; set; }
            public int Id { get; set; }
            public DateTime Last { get; set; }
            public int Stamp { get; set; }
            public DateTime Time { get; set; }
        }
    }
}