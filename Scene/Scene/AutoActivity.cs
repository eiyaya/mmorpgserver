#region using

using System;
using System.Collections.Generic;
using Shared;

#endregion

namespace Scene
{
    public static class AutoActivityManager
    {
        public static Dictionary<int, AutoActivity> dic = new Dictionary<int, AutoActivity>();

        public static void DeleteActivity(int fubenId)
        {
            AutoActivity o;
            if (dic.TryGetValue(fubenId, out o))
            {
                o.CloseTrigger();
                dic.Remove(fubenId);
            }
        }

        public static int GetActivity(int fubenId)
        {
            AutoActivity o;
            if (dic.TryGetValue(fubenId, out o))
            {
                return o.mCount;
            }
            return 1;
        }

        public static void PushActivity(int fubenId, DateTime start, DateTime end, int count)
        {
            if (count > 5)
            {
                count = 5;
            }
            if (count < 2)
            {
                return;
            }
            var a = new AutoActivity
            {
                mStart = start,
                mEnd = end,
                mFubenId = fubenId,
                mCount = count
            };
            AutoActivity o;
            if (dic.TryGetValue(fubenId, out o))
            {
                o.CloseTrigger();
            }
            dic[fubenId] = a;
        }

        public static void RemoveActivity(int fubenId)
        {
            dic.Remove(fubenId);
        }
    }

    public class AutoActivity
    {
        public int mCount;
        public DateTime mEnd;
        public int mFubenId;
        public DateTime mStart;
        public Trigger mTrigger;

        public void CloseTrigger()
        {
            if (mTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(mTrigger);
                mTrigger = null;
            }
        }

        public void StartTrigger()
        {
            if (DateTime.Now < mEnd)
            {
                mTrigger = SceneServerControl.Timer.CreateTrigger(mEnd, TimeOver);
            }
        }

        public void TimeOver()
        {
            AutoActivityManager.RemoveActivity(mFubenId);
        }
    }
}