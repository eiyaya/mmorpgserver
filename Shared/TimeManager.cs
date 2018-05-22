#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Scorpion;
using NLog;

#endregion

namespace Shared
{

    #region 触发器

    public class Trigger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public Trigger(HierarchicalTimeWheel timer, object t, DateTime time, Action act, int autoInterval = 0)
        {
            _timer = timer;
            T = t;
            Act = act;
            AutoInterval = autoInterval;
            Time = time;
        }

        private HierarchicalTimeWheel _timer;
        public int AutoInterval = 0;
        public object T = null;
        public Action Act;
        public DateTime Time;

        public void Repeater()
        {
            if (Act != null)
            {
                try
                {
                    Act();
                }
                catch(Exception ex)
                {
                    Logger.Error(ex, "Execute repeated trigger error.");
                }
                finally
                {
                    Time = DateTime.Now.AddMilliseconds(AutoInterval);
                    T = _timer.Add(AutoInterval, Repeater);
                }
            }
        }
    }

    public class TriggerImpl : IComparable<TriggerImpl>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TriggerImpl(DateTime tt, ulong triggerId, Action func, int autoInterval)
        {
            DueTime = tt;
            TriggerId = triggerId;
            Func = func;
            AutoInterval = autoInterval;
        }

        public int AutoInterval; //循环触发器的间隔(毫秒)
        private Action Func; //回调函数
        public Trigger T;
        public bool bDolready { get; set; } //在一次Updata中保证只会触发一次
        public DateTime DueTime { get; private set; } //触发器的下次生效时间
        public ulong TriggerId { get; private set; } //触发器的唯一ID

        public TriggerImpl Clone(DateTime dueTime)
        {
            return new TriggerImpl(dueTime, TriggerId, Func, AutoInterval);
        }

        //执行触发后的效果
        public void DoFunction()
        {
            if (bDolready)
            {
                Logger.Warn("Trigger::DoFunction  bDolready is True!");
                return; //保证只会触发修改一遍
            }
            bDolready = true;
            //执行lambda表达式
            try
            {
                Func();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Trigger::DoFunction Do Func Fatal!TriggerId={0}  Time={1}",
                    TriggerId, DueTime);
                AutoInterval = 0;
            }
        }

        public Action GetFunc()
        {
            return Func;
        }

        //获取下次生效时间
        public DateTime GetNextTime()
        {
            return DueTime;
        }

        public bool IsFuncNull()
        {
            return Func == null;
        }

        public void SetFuncNull()
        {
            Func = null;
        }

        public int CompareTo(TriggerImpl other)
        {
            if (DueTime < other.DueTime)
            {
                return -1;
            }
            if (DueTime == other.DueTime)
            {
                return (int) (TriggerId - other.TriggerId);
            }
            return 1;
        }
    }

    #endregion

    //触发器管理器
    public class TimeManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //
        public static Stopwatch Timer = Stopwatch.StartNew();

        #region 心跳

        //心跳
        public void Update(int threshold = 1000)
        {
            try
            {
                mTimers.Update();
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "TimeManager::Update error.");
            }
        }

        #endregion

        #region 数据结构

        private readonly HierarchicalTimeWheel mTimers = new HierarchicalTimeWheel();

        #endregion

        #region 对外接口(增删改查)

        //创建触发器
        public Trigger CreateTrigger(DateTime triggerTime,
                                     Action act,
                                     int autoInterval = 0,
                                     [CallerFilePath] string filename = "",
                                     [CallerMemberName] string member = "",
                                     [CallerLineNumber] int line = 0)
        {
            if (act == null)
            {
                Logger.Error("TimeManager::CreateTrigger act == null, called from {0} at {1}:{2}.", member, filename,
                    line);
                return null;
            }

            //Logger.Trace("TimeManager::CreateTrigger Id={0} Time={1} ", getid, triggerTime);
            Trigger t = new Trigger(mTimers, null, triggerTime, act, autoInterval);

            if (autoInterval > 0)
            {
                t.T = mTimers.Add(triggerTime, t.Repeater);
            }
            else
            {
                t.T = mTimers.Add(triggerTime, act);
            }
            
            return t;
        }

        //删除触发器(直接删 或者标记将要删)

        public void DeleteTrigger(Trigger trigger,
                                  [CallerFilePath] string filename = "",
                                  [CallerMemberName] string member =
                                      "",
                                  [CallerLineNumber] int line = 0)
        {
            if (trigger == null || trigger.T == null)
            {
                Logger.Error("TimeManager::DeleteTrigger  obj == null, called from {0} at {1}:{2}.", member, filename,
                    line);
                return;
            }

            mTimers.Cancel(trigger.T);
            trigger.Act = null;
        }

        //修改触发器时间
        public void ChangeTime(ref Trigger trigger,
                               DateTime newTime,
                               int autoInterval = -1,
                               [CallerFilePath] string filename = "",
                               [CallerMemberName] string member = "",
                               [CallerLineNumber] int line = 0)
        {
            if (trigger == null || trigger.T == null)
            {
                Logger.Warn("ChangeTime obj == null, called from {0} at {1}:{2}.", member, filename, line);
                return;
            }

            mTimers.Cancel(trigger.T);

            trigger.Time = newTime;

            if (autoInterval > 0)
            {
                trigger.AutoInterval = autoInterval;
                trigger.T = mTimers.Add(newTime, trigger.Repeater);
            }
            else
            {
                trigger.T = mTimers.Add(newTime, trigger.Act);
            }
        }

        //获取下次生效时间
        public DateTime GetNextTime(Trigger obj)
        {
            var trigger = obj;
            if (trigger == null || trigger.T == null)
            {
                return DateTime.Now;
            }
            return trigger.Time;
        }

        #endregion

        #region 私有(Private)

        //执行触发器
        private bool DoTrigger(TriggerImpl value)
        {
            //Logger.Trace("DoTrigger id={0}", value.TriggerId);
            value.DoFunction();
            if (value.AutoInterval > 0)
            {
                value.bDolready = false;
                var temp = value.DueTime.AddMilliseconds(value.AutoInterval);
#if DEBUG
                var now = DateTime.Now;
                if (temp < now)
                {
                    var interval = (now - temp).TotalMilliseconds;
                    interval = (interval + value.AutoInterval - 1)/value.AutoInterval*value.AutoInterval;
                    temp = temp.AddMilliseconds(interval);
                }
#endif
                ChangeTime(ref value.T, temp);
                return false;
            }
            return true;
        }

        #endregion
    }
}