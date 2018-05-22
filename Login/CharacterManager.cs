#region using

using System;
using System.Collections.Generic;
using System.Text;
using DataContract;
using Shared;

#endregion

namespace Login
{
    public class CountManager
    {
        public CountManager(int sId)
        {
            serverId = sId;
        }

        public int maxCount;
        public int nowCount;
        public int serverId;

        public int GetAndDeleteMax()
        {
            var result = maxCount;
            if (maxCount > nowCount)
            {
                maxCount = nowCount;
            }
            return result;
        }

        public void ModifyCount(int value)
        {
            if (value > 0)
            {
                nowCount += value;
                if (nowCount > maxCount)
                {
                    maxCount = nowCount;
                }
            }
            else
            {
                nowCount += value;
                if (nowCount < 0)
                {
                    PlayerLog.PlayerLogger(666, "PopServerPlayer severId={0},count={1}", serverId, nowCount);
                    nowCount = 0;
                }
            }
        }
    }

    public class CharacterManager : CharacterManager<CharacterController, DBCharacterLogin, DBCharacterLoginSimple>
    {
        //服务器Id -> 服务器人数
        //public static Dictionary<int, int> ServerCount = new Dictionary<int, int>();

        public static Dictionary<int, CountManager> ServerCount = new Dictionary<int, CountManager>();

        static CharacterManager()
        {
            LoginServerControl.Timer.CreateTrigger(DateTime.Now.AddMinutes(1), OnMinuteChangeEvent, 60000);
        }

        public static void AddtoRange(Dictionary<int, int> dic)
        {
            foreach (var i in ServerCount)
            {
                dic[i.Key] = i.Value.nowCount;
            }
        }

        //private static void TimeUpdata()
        //{
        //    StringBuilder str = new StringBuilder(32);
        //    foreach (KeyValuePair<int, int> i in ServerCount)
        //    {
        //        str.Append(i);
        //    }
        //    PlayerLog.PlayerLogger(666, str.ToString());
        //}

        public static void OnMinuteChangeEvent()
        {
            var str = new StringBuilder();
            var index = 0;
            foreach (var i in ServerCount)
            {
                if (index > 0)
                {
                    str.Append(",");
                }
                str.Append(i.Key);
                str.Append("|");
                str.Append(i.Value.GetAndDeleteMax());
                index++;
            }
            PlayerLog.BackDataLogger((int) BackDataType.OnLineCount, "{0}", str.ToString());
        }

        //减少服务器人数
        public static void PopServerPlayer(int serverId)
        {
            CountManager old;
            if (!ServerCount.TryGetValue(serverId, out old))
            {
                old = new CountManager(serverId);
                ServerCount[serverId] = old;
            }
            old.ModifyCount(-1);
        }

        //增加服务器人数
        public static void PushServerPlayer(int serverId)
        {
            CountManager old;
            if (!ServerCount.TryGetValue(serverId, out old))
            {
                old = new CountManager(serverId);
                ServerCount[serverId] = old;
            }
            old.ModifyCount(1);
        }
    }
}