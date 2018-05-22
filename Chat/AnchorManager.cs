using System;
using System.Collections.Generic;
using System.Linq;
using JsonConfig;
using NLog;

namespace Chat
{
    public class Anchor
    {
        public string Name;
        public TimeSpan BeginTime;
        public TimeSpan EndTime;
    }

	public class AnchorConfig
	{
		public bool Open;
		public string ServerName = "test";
        public Dictionary<string, Anchor> AnchorDict = new Dictionary<string, Anchor>();
        public Dictionary<string, string> AnchorBeginTimeDict = new Dictionary<string, string>();
        public Dictionary<string, string> AnchorEndTimeDict = new Dictionary<string, string>();
		public int GuildSpeekLevel = 1;
	}

    public interface IAnchorManager
    {
        void Init();
        void LoadConfig(AnchorManager _this);
        void CharOnline(AnchorManager _this, ulong charId, string name);
        void CharOffline(AnchorManager _this, ulong charId);
        List<string> GetAnchorNameList(AnchorManager _this);
        List<string> GetAnchorBeginTimeList(AnchorManager _this);
        List<string> GetAnchorEndTimeList(AnchorManager _this);
        ulong GetCurrentAnchor(AnchorManager _this);
        string GetCurrentAnchorName(AnchorManager _this);
        void AnchorEnterRoom(AnchorManager _this, ulong charId, string name);
        void AnchorExitRoom(AnchorManager _this, ulong charId);
    }

    public class AnchorManagerDefaultImpl : IAnchorManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Init()
        {
            ChatServerControl.tm.CreateTrigger(
                DateTime.Now.AddMilliseconds(AnchorManager.s_RefreshInterval),
                Tick,
                AnchorManager.s_RefreshInterval);
        }

        public void LoadConfig(AnchorManager _this)
        {
            dynamic ServerConfig = JsonConfig.Config.ApplyJsonFromPath("../Config/anchor.config");

            _this.MyConfig = new AnchorConfig();

            _this.MyConfig.Open = (bool)ServerConfig.Open;
            _this.MyConfig.ServerName = (string)ServerConfig.ServerName;
            foreach (var val in ServerConfig.Anchors)
            {
                var anchor = new Anchor();
                anchor.Name = val.Name;
                anchor.BeginTime = convertConfigToDateTime(val.BeginTime);
                anchor.EndTime = convertConfigToDateTime(val.EndTime);
                _this.MyConfig.AnchorDict[anchor.Name] = anchor;
                _this.MyConfig.AnchorBeginTimeDict[anchor.Name] = val.BeginTime;
                _this.MyConfig.AnchorEndTimeDict[anchor.Name] = val.EndTime;
            }
            _this.MyConfig.GuildSpeekLevel = (int)ServerConfig.GuildSpeekLevel;

            UpdateCurrentAnchor(_this);            
        }

        public void CharOnline(AnchorManager _this, ulong charId, string name)
        {
            if (IsAnchor(_this, name))
            {
                AddOnlineAnchor(_this, charId, name);
            }            
        }
        public void CharOffline(AnchorManager _this, ulong charId)
        {
            if (_this.OnlineAnchorDict.ContainsKey(charId))
            {
                RemoveOnlineAnchor(_this, charId);
            }
        }

        public List<string> GetAnchorNameList(AnchorManager _this)
        {
            var names = _this.MyConfig.AnchorDict.Keys;
            return names.ToList();
        }

        public List<string> GetAnchorBeginTimeList(AnchorManager _this)
        {
            var beginTimes = _this.MyConfig.AnchorBeginTimeDict.Values;
            return beginTimes.ToList();
        }

        public List<string> GetAnchorEndTimeList(AnchorManager _this)
        {
            var endTimes = _this.MyConfig.AnchorEndTimeDict.Values;
            return endTimes.ToList();
        }

        public ulong GetCurrentAnchor(AnchorManager _this)
        {
            return _this.CurrentAnchorId;
        }

        public string GetCurrentAnchorName(AnchorManager _this)
        {
            return _this.CurrentAnchorName;
        }

        private void Tick()
        {
            UpdateCurrentAnchor(AnchorManager.Instance);
        }

        private TimeSpan convertConfigToDateTime(string time)
        {
            return TimeSpan.Parse(time);
        }

        // 增加在线主播
        private void AddOnlineAnchor(AnchorManager _this, ulong charId, string name)
        {
            if (IsAnchor(_this, name))
            {
                Anchor anchor;
                if (_this.MyConfig.AnchorDict.TryGetValue(name, out anchor))
                {
                    _this.OnlineAnchorDict[charId] = anchor;
                    if (IsInLiveTime(anchor))
                    {
                        SetCurrentAnchor(_this, charId, name);
                    }
                }
            }
        }

        // 删除在线主播
        private void RemoveOnlineAnchor(AnchorManager _this, ulong charId)
        {
            _this.OnlineAnchorDict.Remove(charId);
            if (_this.CurrentAnchorId == charId)
            {
                SetCurrentAnchor(_this, 0L, string.Empty);
                UpdateCurrentAnchor(_this);
            }
            
            _this.AnchorExitRoom(charId);
        }

        // 检查是否是主播
        private bool IsAnchor(AnchorManager _this, string name)
        {
            return _this.MyConfig.AnchorDict.ContainsKey(name);
        }

        // 是否在直播时间
        private bool IsInLiveTime(Anchor anchor)
        {
            var nowTime = DateTime.Now.TimeOfDay;
            if (nowTime >= anchor.BeginTime && nowTime < anchor.EndTime)
            {
                return true;
            }

            return false;
        }

        // 设置当前主播
        private void SetCurrentAnchor(AnchorManager _this, ulong charId, string name)
        {
            if (_this.CurrentAnchorId == charId)
                return;

            if (_this.CurrentAnchorId > 0)
            { // 下线了
                
            }

            if (charId > 0)
            { // 上线了
                ChatManager.BroadcastAllAnchorOnlineMessage(name, 1);
            }

            _this.CurrentAnchorId = charId;
            _this.CurrentAnchorName = name;
        }

        // 更新当前主播
        private void UpdateCurrentAnchor(AnchorManager _this)
        {
            foreach (var anchorPair in _this.OnlineAnchorDict)
            {
                var anchor = anchorPair.Value;
                if (IsInLiveTime(anchor))
                {
                    var charId = anchorPair.Key;
                    var name = anchor.Name;
                    SetCurrentAnchor(_this, charId, name);
                }
            }
        }

        public void AnchorEnterRoom(AnchorManager _this, ulong charId, string name)
        {
            if (!_this.InAnchorRoomDic.ContainsKey(charId))
            {
                _this.InAnchorRoomDic[charId] = name;

                _this.IsInAnchorRooml = 1;

                // 写日志
                Logger.Info("Anchor Enter Room {0} id={1}", name, charId);
            }
        }

        public void AnchorExitRoom(AnchorManager _this, ulong charId)
        {
            if (!_this.InAnchorRoomDic.ContainsKey(charId))
                return;

            // 写日志
            Logger.Info("Anchor Exit Room id={0}", charId);

            _this.InAnchorRoomDic.Remove(charId);
            if (0 == _this.InAnchorRoomDic.Count)
            {
                // 写日志
                _this.IsInAnchorRooml = 0;

                Logger.Info("Room is no Anchor!!!!!");
            }
        }
    }

	public class AnchorManager
	{
		private static AnchorManager _instance;
	    public static AnchorManager Instance
	    {
	        get { return _instance ?? (_instance = new AnchorManager()); }
	    }

        private static IAnchorManager mImpl;
	    static AnchorManager()
	    {
            ChatServer.Instance.UpdateManager.InitStaticImpl(typeof(AnchorManager), typeof(AnchorManagerDefaultImpl),
                o => { mImpl = (IAnchorManager)o; });	        
	    }

	    public int IsInAnchorRooml;//是否在直播房间内 1:在 0：不在
	    public Dictionary<ulong, string> InAnchorRoomDic = new Dictionary<ulong, string>();//主播进入房间内dic
        public AnchorConfig MyConfig { get; set; }
        // 在线主播
        public Dictionary<ulong, Anchor> OnlineAnchorDict = new Dictionary<ulong, Anchor>();
        // 当前主播
	    public ulong CurrentAnchorId;
	    public string CurrentAnchorName;
        // 刷新间隔
	    public static int s_RefreshInterval = 60000; // ms

		public AnchorManager()
        {
            _instance = this;
        }

        public void Init()
        {
            mImpl.Init();
        }

        public List<string> GetAnchorNameList()
        {
            return mImpl.GetAnchorNameList(this);
        }
        public List<string> GetAnchorBeginTimeList()
        {
            return mImpl.GetAnchorBeginTimeList(this);
        }
        public List<string> GetAnchorEndTimeList()
        {
            return mImpl.GetAnchorEndTimeList(this);
        }

		public void LoadConfig()
		{
            mImpl.LoadConfig(this);
		}

        public void CharOnline(ulong charId, string name)
        {
            mImpl.CharOnline(this, charId, name);
        }

        public void CharOffline(ulong charId)
	    {
            mImpl.CharOffline(this, charId);
	    }

	    public ulong GetCurrentAnchor()
	    {
	        return mImpl.GetCurrentAnchor(this);
	    }

        public string GetCurrentAnchorName()
        {
            return mImpl.GetCurrentAnchorName(this);
        }

	    public void AnchorEnterRoom(ulong charId, string name)
	    {
	        mImpl.AnchorEnterRoom(this, charId, name);
	    }

	    public void AnchorExitRoom(ulong charId)
	    {
	        mImpl.AnchorExitRoom(this, charId);
	    }
    }
}
