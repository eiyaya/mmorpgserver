#region using

using System;
using C5;
using DataTable;
using EventSystem;
using Shared;

#endregion

namespace Team
{
    public static class StaticParam
    {
        public static int AllianceWarLevelLimit;
        //确认副本的等待时间
        public static int ConfirmDungeonWaitTime;
        public static DateTime FirstSaveDbTime;

        public static void Init()
        {
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
            ResetFirstTime();
            Reset();
        }

        private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v.tableName == "ServerConfig")
            {
                Reset();
            }
        }

        private static void Reset()
        {
	        try
	        {
		        var data = Table.GetServerConfig(222);
		        if (null != data)
		        {
			        ConfirmDungeonWaitTime = Table.GetServerConfig(222).ToInt();
		        }

		        data = Table.GetServerConfig(1153);
		        if (null != data)
		        {
			        AllianceWarLevelLimit = Table.GetServerConfig(1153).ToInt();
		        }
	        }
	        catch (Exception e)
	        {
		        Logger.Log(e.Message);
	        }
	        
        }

        //重置时间
        private static void ResetFirstTime()
        {
            var nowTime = DateTime.Now;
#if DEBUG
            nowTime = nowTime.AddMinutes(1);
#else
            nowTime = nowTime.AddMinutes(55 - nowTime.Minute);
#endif
            FirstSaveDbTime = nowTime.AddSeconds(MyRandom.Random(60));
        }
    }
}