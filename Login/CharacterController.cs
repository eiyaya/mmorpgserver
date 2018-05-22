#region using

using System;
using System.Collections;
using System.Collections.Generic;
using DataContract;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Login
{
    public interface ICharacterController
    {
        void ApplyEvent(CharacterController _this, int eventId, string evt, int count);
        void ApplySimpleData(CharacterController _this, DBCharacterLoginSimple simpleData);
        IEnumerator FirstOnline(Coroutine co, ulong clientId, ulong characterId, int Continuedays, CharacterController _this);
        void FirstOnlineMsg(CharacterController _this, ulong clientId);
        //获得连续登陆的天数
        int GetContinueday(CharacterController _this);
        DBCharacterLoginSimple GetSimpleData(CharacterController _this);
        List<TimedTaskItem> GetTimedTasks(CharacterController _this);
        //获得当前角色当天在线时间
        long GetTodayOnlineTime(CharacterController _this);
        ulong GetLoginDays(CharacterController _this);
        void SetLoginDays(CharacterController _this, int days);
        DBCharacterLogin InitByBase(CharacterController _this, ulong characterId, object[] args = null);
        bool InitByDb(CharacterController _this, ulong characterId, DBCharacterLogin dbData);
        //下线
        void LostLine(CharacterController _this, bool isOnline);
        void NotifyEnterScene(CharacterController _this);
        IEnumerator NotifyEnterSceneCoroutine(Coroutine co, CharacterController _this);
        void OnDestroy(CharacterController _this);
        ////获取Guid
        //public IEnumerator GetGuid(Coroutine co,AsyncReturnValue<ulong> asyValue)
        //{
        //    asyValue = LoginServer.Instance.DB.GetNextId(coroutine, (int)DataCategory.LoginPlayer);
        //    yield return asyValue;
        //}

        //上线
        void OnLine(CharacterController _this);
        void OnSaveData(CharacterController _this, DBCharacterLogin data, DBCharacterLoginSimple simpleData);

        IEnumerator SyncCharacterLevelData(Coroutine co, CharacterController _this);
    }


    public class CharacterControllerDefaultImpl : ICharacterController
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);
        private static readonly Logger CheckLoginLogger = LogManager.GetLogger("CheckLogin");

        public bool InitByDb(CharacterController _this, ulong characterId, DBCharacterLogin dbData)
        {
            PlayerLog.WriteLog(characterId, "----------Login--------------------InitByDb--------------------{0}",
                dbData.SaveCount);
            _this.mDbData = dbData;
            _this.FirstLanding = false;
            return true;
        }

        public DBCharacterLogin InitByBase(CharacterController _this, ulong characterId, object[] args = null)
        {
            _this.mDbData = new DBCharacterLogin();
            var nextId = (ulong) args[0];
            var name = (string) args[1];
            var type = (int) args[2];
            var serverId = (int) args[3];
            var playerId = (ulong) args[4];
            _this.mDbData.Id = nextId;
            _this.mDbData.PlayerId = playerId;
            _this.mDbData.Name = name;
            _this.mDbData.TypeId = type;
            _this.mDbData.ServerId = serverId;
            _this.mDbData.FoundTime = DateTime.Now.ToBinary();
            return _this.mDbData;
        }

        public void ApplySimpleData(CharacterController _this, DBCharacterLoginSimple simpleData)
        {

        }

        public DBCharacterLoginSimple GetSimpleData(CharacterController _this)
        {
            DBCharacterLoginSimple dbSimple;
            CharacterManager<CharacterController, DBCharacterLogin, DBCharacterLoginSimple>.DataItem data;
            var dic = CharacterManager.Instance.mDictionary;
            if (dic.TryGetValue(_this.CharacterId, out data))
            {
                dbSimple = data.SimpleData;
            }
            else
            {
                Logger.Info("GetSimpleData return null, id = {0}", _this.CharacterId);
                dbSimple = new DBCharacterLoginSimple();
            }
            var db = _this.mDbData;
            dbSimple.Id = db.Id;
            dbSimple.Name = db.Name;
            dbSimple.TypeId = db.TypeId;
            dbSimple.ServerId = db.ServerId;
            dbSimple.LoginOut = db.LoginOut;
            dbSimple.TotleOlineTime = db.TotleOlineTime;
            return dbSimple;
        }

        public List<TimedTaskItem> GetTimedTasks(CharacterController _this)
        {
            return null;
        }

        public void ApplyEvent(CharacterController _this, int eventId, string evt, int count)
        {
        }

        public void OnLine(CharacterController _this)
        {
            if (_this.mDbData.SaveState == 1)
            {
                Logger.Warn("Login OnLine Error! SaveState = {0},CharacterId={1},name={2},loginOut={3},loginIn={4}",
                    _this.mDbData.SaveState, _this.CharacterId, _this.mDbData.Name,
                    DateTime.FromBinary(_this.mDbData.LoginOut), DateTime.FromBinary(_this.mDbData.LoginIn));
                _this.mDbData.LoginIn = DateTime.Now.ToBinary();
                _this.MarkDirty();
                return;
            }

            var li = DateTime.Now;
            _this.mDbData.LoginIn = li.ToBinary();
            _this.mDbData.LandCount++;
            _this.FirstLanding = false;
            if (_this.mDbData.LoginOut == 0)
            {
                _this.mDbData.Continuedays = 1;
                _this.FirstLanding = true;
            }
            else
            {
                var lo = DateTime.FromBinary(_this.mDbData.LoginOut);
                var liClean = li.ExtensionModDay();
                var days = (int) ((liClean - lo).TotalDays);
                if (days > 0)
                {
                    _this.mDbData.Continuedays = 1;
                    _this.mDbData.TodayOlineTime = 0;
                    _this.FirstLanding = true;
                }
                else
                {
                    if ((int) ((liClean.AddDays(1) - lo).TotalDays) > 0)
                    {
                        _this.mDbData.Continuedays++;
                        _this.mDbData.TodayOlineTime = 0;
                        _this.FirstLanding = true;
                    }
                }
            }
            _this.mDbData.SaveState = 1;
            _this.MarkDirty();

            UpdateUserLoginDays(_this);
        }

        public void FirstOnlineMsg(CharacterController _this, ulong clientId)
        {
            CoroutineFactory.NewCoroutine(_this.FirstOnline, clientId, _this.CharacterId, _this.mDbData.Continuedays).MoveNext();
        }

        public void UpdateUserLoginDays(CharacterController _this)
        {
            var createTime = DateTime.FromBinary(_this.mDbData.FoundTime);
            var start = Convert.ToDateTime(createTime.ToShortDateString());
            var end = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            var sp = end.Subtract(start);
            SetLoginDays(_this, sp.Days + 1);
        }

        public void NotifyEnterScene(CharacterController _this)
        {
            CoroutineFactory.NewCoroutine(_this.NotifyEnterSceneCoroutine).MoveNext();
        }

        public IEnumerator NotifyEnterSceneCoroutine(Coroutine co, CharacterController _this)
        {
            //通知scene进入场景
            var changeSceneMsg = LoginServer.Instance.SceneAgent.LoginEnterScene(_this.CharacterId,
                _this.mDbData.ServerId, _this.mDbData.LoginOut);
            yield return changeSceneMsg.SendAndWaitUntilDone(co);
        }

        public IEnumerator FirstOnline(Coroutine co, ulong clientId, ulong characterId, int Continuedays, CharacterController _this)
        {
            var msg = LoginServer.Instance.LogicAgent.FirstOnline(characterId, clientId, characterId, Continuedays);
            yield return msg.SendAndWaitUntilDone(co);
        }

        public void LostLine(CharacterController _this, bool isOnline)
        {
            if (_this == null)
            {
                CheckLoginLogger.Error("Login LostLine ! CharacterController = NULL");
                return;
            }
            if (_this.mDbData == null)
            {
                CheckLoginLogger.Error("Login LostLine ! CharacterId = {0}", _this.CharacterId);
                return;
            }

            if (_this.mDbData.LandCount < 1)
            {
                return;
            }
            if (_this.mDbData.SaveState == 0)
            {
                if (!isOnline)
                {
                    CheckLoginLogger.Warn(
                        "Login LostLine Error! SaveState = {0},CharacterId={1},name={2},loginOut={3},loginIn={4}",
                        _this.mDbData.SaveState, _this.CharacterId, _this.mDbData.Name,
                        DateTime.FromBinary(_this.mDbData.LoginOut), DateTime.FromBinary(_this.mDbData.LoginIn));
                    _this.mDbData.LoginOut = DateTime.Now.ToBinary();
                    _this.MarkDirty();
                }
                return;
            }

            
//            Error("LostLine TodayOlineTime id={1} ,seconds = {0}", mDbData.TodayOlineTime, CharacterId);

            var lo = DateTime.Now;
            var li = DateTime.FromBinary(_this.mDbData.LoginIn);
            var temp = DateTime.FromBinary(_this.mDbData.LoginOut);

            CheckLoginLogger.Info("Login LostLine ! CharacterId={0},name={1},loginOut={2},loginIn={3}", _this.CharacterId, _this.mDbData.Name,li,temp);
            _this.mDbData.LoginOut = lo.ToBinary();
            var seconds = (long) (lo - li).TotalSeconds;
            _this.mDbData.TotleOlineTime += seconds;
            var loClean = lo.ExtensionModDay();
            //var loUp = loClean.AddDays(1);
            var tttt = lo.Date - li.Date;
            var nChangeDays = tttt.Days;


            if (seconds > 60*60*6)
            {//做一个异常捕获,持续在线6小时姑且认为是异常了
                var trace = new System.Diagnostics.StackTrace();

                CheckLoginLogger.Warn("LostLine !  Keep online 6 Hours ! CharacterId={0},name={1},loginOut={2},loginIn={3} trace={4}", _this.CharacterId, _this.mDbData.Name, li, temp, trace.ToString());
            }
            if (nChangeDays > 0)
            {
                _this.mDbData.TodayOlineTime = (long) (lo - loClean).TotalSeconds;
                _this.mDbData.Continuedays += nChangeDays;
            }
            else
            {
                _this.mDbData.TodayOlineTime += seconds;
            }
            _this.mDbData.SaveState = 0;
            //Logger.Error("LostLine TodayOlineTime id={1} ,seconds = {0}", mDbData.TodayOlineTime, CharacterId);
            _this.MarkDirty();

            UpdateUserLoginDays(_this);
        }

        public int GetContinueday(CharacterController _this)
        {
            return _this.mDbData.Continuedays;
        }

        public long GetTodayOnlineTime(CharacterController _this)
        {
            if (_this.mDbData.LandCount == 0)
            {
                return 0;
            }
            return _this.mDbData.TodayOlineTime;
            //var lo = DateTime.Now;
            //var li = DateTime.FromBinary(_this.mDbData.LoginIn);
            ////var ll = DateTime.FromBinary(mDbData.LoginOut);
            ////if (ll > li && ll < lo)
            ////{
            ////    return mDbData.TodayOlineTime;
            ////}
            //var diffDay = li.GetDiffDays(lo);
            //if (diffDay > 0)
            //{
            //    var loClean = lo.ExtensionModDay();
            //    var ThisOlineTime = (long) (lo - loClean).TotalSeconds;
            //    return ThisOlineTime + _this.mDbData.TodayOlineTime;
            //}
            //else
            //{
            //    var ThisOlineTime = (long) (lo - li).TotalSeconds;
            //    return ThisOlineTime + _this.mDbData.TodayOlineTime;
            //}
        }

        public ulong GetLoginDays(CharacterController _this)
        {
            return _this.mDbData.LoginDays;
        }

        public void SetLoginDays(CharacterController _this, int theday)
        {
            if (theday < 1 || theday > 60)  // 只记前60天
                return;

            var dbLoginDays = _this.mDbData.LoginDays;
            dbLoginDays |= (1UL << (theday - 1));
            _this.mDbData.LoginDays = dbLoginDays;
        }

        public void OnDestroy(CharacterController _this)
        {
            try
            {
                //<add key="gamedayonlinetime" value="insert into smcdb.gamedayonlinetime (account, server, onlinetime, logdate) values ('{0}', {1},  {2}, '{3}')" />
                DateTime dt = DateTime.Now;
                dt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
                if (_this != null)
                {
                    string gamedayonlinetime = string.Format("gamedayonlinetime#{0}|{1}|{2}|{3}",
                        _this.mDbData.Id,
                        _this.mDbData.ServerId,
                        _this.GetTodayOnlineTime(),
                        dt.ToString("yyyy/MM/dd HH:mm:ss")
                        );
                    kafaLogger.Info(gamedayonlinetime);
                }

                string v = string.Format("characters#{0}|{1}|{2}|{3}|{4}|{5}",
                _this.mDbData.Id,
                _this.mDbData.PlayerId,
                DateTime.FromBinary(_this.mDbData.FoundTime).ToString("yyyy/MM/dd HH:mm:ss"),
                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                _this.mDbData.ServerId,
                _this.mDbData.TypeId);
                kafaLogger.Info(v);

//                 string vv2 = string.Format("adminplayerlogintime#{0}|{1}|{2}|{3}|{4}|{5}",
//                     _this.mDbData.ServerId,
//                     _this.mDbData.PlayerId,
//                     _this.mDbData.Id,
//                     DateTime.FromBinary(_this.mDbData.LoginIn).ToString("yyyy/MM/dd HH:mm:ss"),
//                     DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
//                     2);
//                 kafaLogger.Info(vv2);
            }
            catch (Exception)
            {
            }
        }

        public void OnSaveData(CharacterController _this, DBCharacterLogin data, DBCharacterLoginSimple simpleData)
        {
            PlayerLog.WriteLog(_this.CharacterId, "----------Login--------------------OnSaveData--------------------{0}",
                data.SaveCount++);
        }

        public IEnumerator SyncCharacterLevelData(Coroutine co, CharacterController _this)
        {
            var msg1 = LoginServer.Instance.LogicAgent.ApplyMayaSkill(_this.CharacterId, 0);
            yield return msg1.SendAndWaitUntilDone(co);
            var msg = LoginServer.Instance.LogicAgent.GetLogicSimpleData(_this.CharacterId, 0);
            yield return msg.SendAndWaitUntilDone(co);

            if (msg.State == MessageState.Reply)
            {
                if (msg.ErrorCode == (int)ErrorCodes.OK)
                {
                    var logindb = _this.mDbData;
                    logindb.Level = msg.Response.Level;
                    logindb.Ladder = msg.Response.Ladder;
                    _this.MarkDbDirty();
                }
            }
        }
    }

    public class CharacterController : NodeBase, ICharacterControllerBase<DBCharacterLogin, DBCharacterLoginSimple>
    {
        private static ICharacterController mImpl;

        static CharacterController()
        {
            LoginServer.Instance.UpdateManager.InitStaticImpl(typeof (CharacterController),
                typeof (CharacterControllerDefaultImpl),
                o => { mImpl = (ICharacterController) o; });
        }

        public ulong ClientId;
        public bool FirstLanding;

        public ulong CharacterId
        {
            get { return mDbData.Id; }
        }

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public DBCharacterLogin mDbData { get; set; }

        public void ApplySimpleData(DBCharacterLoginSimple simpleData)
        {
            mImpl.ApplySimpleData(this, simpleData);
        }

        public IEnumerator FirstOnline(Coroutine co, ulong clientId, ulong characterId, int Continuedays)
        {
            return mImpl.FirstOnline(co, clientId, characterId, Continuedays, this);
        }

        public void FirstOnlineMsg(ulong clientId)
        {
            mImpl.FirstOnlineMsg(this, clientId);
        }

        //获得连续登陆的天数
        public int GetContinueday()
        {
            return mImpl.GetContinueday(this);
        }

        //获得当前角色当天在线时间
        public long GetTodayOnlineTime()
        {
            return mImpl.GetTodayOnlineTime(this);
        }

        // 獲取前60天登陸數據
        public ulong GetLoginDays()
        {
            return mImpl.GetLoginDays(this);
        }

        // 设置第几天登陆了（从1开始）
        public void SetLoginDays(int theday)
        {
            mImpl.SetLoginDays(this, theday);
        }

        //下线
        public void LostLine(bool isOnline = false)
        {
            mImpl.LostLine(this, isOnline);
        }

        public void NotifyEnterScene()
        {
            mImpl.NotifyEnterScene(this);
        }

        public IEnumerator NotifyEnterSceneCoroutine(Coroutine co)
        {
            return mImpl.NotifyEnterSceneCoroutine(co, this);
        }

        ////获取Guid
        //public IEnumerator GetGuid(Coroutine co,AsyncReturnValue<ulong> asyValue)
        //{
        //    asyValue = LoginServer.Instance.DB.GetNextId(coroutine, (int)DataCategory.LoginPlayer);
        //    yield return asyValue;
        //}

        //上线
        public void OnLine()
        {
            mImpl.OnLine(this);
        }

        public bool InitByDb(ulong characterId, DBCharacterLogin dbData)
        {
            return mImpl.InitByDb(this, characterId, dbData);
        }

        public DBCharacterLogin InitByBase(ulong characterId, object[] args = null)
        {
            return mImpl.InitByBase(this, characterId, args);
        }

        public DBCharacterLoginSimple GetSimpleData()
        {
            return mImpl.GetSimpleData(this);
        }

        public DBCharacterLogin GetData()
        {
            return mDbData;
        }

        public void Tick()
        {
        }

        public List<TimedTaskItem> GetTimedTasks()
        {
            return mImpl.GetTimedTasks(this);
        }

        public void ApplyEvent(int eventId, string evt, int count)
        {
            mImpl.ApplyEvent(this, eventId, evt, count);
        }

        public void OnDestroy()
        {
            mImpl.OnDestroy(this);
        }

        public void OnSaveData(DBCharacterLogin data, DBCharacterLoginSimple simpleData)
        {
            mImpl.OnSaveData(this, data, simpleData);
        }

        public bool Online
        {
            get { return CharacterManager.Instance.GetCharacterControllerFromMemroy(mDbData.Id) != null; }
        }

        public CharacterState State { get; set; }

        public IEnumerator SyncCharacterLevelData(Coroutine co)
        {
            return mImpl.SyncCharacterLevelData(co, this);
        }
    }
}