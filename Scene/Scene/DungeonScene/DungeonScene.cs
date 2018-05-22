#region using

using System;
using System.Collections;
using System.Linq;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;
using System.Collections.Generic;

#endregion

namespace Scene
{
    public enum eDungeonTimerType
    {
        WaitStart, //等待副本开始
        WaitExtraTimeStart, //等待加时赛开始
        WaitEnd, //等待副本结束（策划表里配的时间）
        WaitClose, //等待副本关闭
        SafeClose, //安全关闭，避免副本被泄露
        CreateMonster, //用于延迟造怪
        CreateRandomMonster,
        Count
    }

    public interface IDungeonScene
    {
        IEnumerator CloseDungeon(Coroutine coroutine, DungeonScene _this);
        void CloseTimer(DungeonScene _this, eDungeonTimerType type);
        void Complete(DungeonScene _this, ulong playerId, FubenResult result);
        void CompleteToAll(DungeonScene _this, FubenResult result, int seconds = 20);
        void EndDungeon(DungeonScene _this);
        void EnterAutoClose(DungeonScene _this, int seconds = 20);
        DateTime GetTriggerTime(DungeonScene _this, eDungeonTimerType type);
        void NotifyDungeonTime(DungeonScene _this, ObjPlayer player, eDungeonTimerType type);
        void NotifyDungeonTime(DungeonScene _this, eDungeonTimerType type);
        void OnCreate(DungeonScene _this);
        void OnNpcDie(DungeonScene _this, ObjNPC npc, ulong characterId);
        void OnPlayerEnter(DungeonScene _this, ObjPlayer player);
        void OnPlayerEnterOver(DungeonScene _this, ObjPlayer player);
        void AfterPlayerEnterOver(DungeonScene _this, ObjPlayer player);
        
        void OnPlayerLeave(DungeonScene _this, ObjPlayer player);
        void RemoveObj(DungeonScene _this, int id);
        void StartDungeon(DungeonScene _this);
        void StartTimer(DungeonScene _this, eDungeonTimerType type, DateTime time, Action act, int interval = -1);
        void TimeOverClose(DungeonScene _this);
        void TimeOverEnd(DungeonScene _this);
        void TimeOverStart(DungeonScene _this);
        double GetDungeonTimeProgress(DungeonScene _this);
    }

    public class DungeonSceneDefaultImpl : IDungeonScene
    {

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IEnumerator CompleteCoroutine(Coroutine coroutine,
                                              DungeonScene _this,
                                              ulong playerId,
                                              FubenResult result)
        {
            var trigger = _this.Trggers[(int) eDungeonTimerType.WaitEnd];
            var useSec = 0;
            if (trigger != null)
            {
                var closeTime = trigger.Time;
                useSec = (int)(_this.mFubenRecord.TimeLimitMinutes*60 - (int) closeTime.GetDiffSeconds(DateTime.Now));
                if (useSec < 0)
                {
                    useSec = 0;
                }
            }
            result.FubenId = _this.TableSceneData.FubenId;
            result.UseSeconds = useSec;
            var msg = SceneServer.Instance.LogicAgent.CompleteFuben(playerId, result);
            yield return msg.SendAndWaitUntilDone(coroutine);

            //后台统计
            try
            {
                var tbFuben = Table.GetFuben(result.FubenId);
                var character = _this.FindCharacter(playerId);
                if (tbFuben != null && character != null)
                {
                    var level = character.GetLevel();
                    var v = string.Format("fuben#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                        playerId,
                        result.FubenId,
                        tbFuben.Name,
                        tbFuben.AssistType,
                        1,  // 0  进入   1 完成   2 退出
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        level,
                        character.Attr.GetFightPoint()
                    ); // 时间
                    PlayerLog.Kafka(v);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        private void NotifySceneFinished(DungeonScene _this)
        {
            CoroutineFactory.NewCoroutine(NotifySceneFinishedCoroutine, _this).MoveNext();
        }

        private IEnumerator NotifySceneFinishedCoroutine(Coroutine co, DungeonScene _this)
        {
            //通知broke，我已经要关闭了
            var msg = SceneServer.Instance.SceneAgent.NotifySceneFinished(0, _this.Guid);
            yield return msg.SendAndWaitUntilDone(co);
        }

        public double GetDungeonTimeProgress(DungeonScene _this)
        {
            var t = DateTime.Now;
            if (t > _this.EndTime)
            {
                return 0;                                                                                                                                                                                                                                                                                                                                                      
            }
            else
            {
                return (_this.EndTime - t).TotalMinutes;
            }
        }
        public void StartTimer(DungeonScene _this, eDungeonTimerType type, DateTime time, Action act, int interval = -1)
        {
            if (_this.Trggers[(int) type] != null)
            {
                _this.DeleteTimer(_this.Trggers[(int) type]);
            }
            _this.Trggers[(int) type] = _this.CreateTimer(time, act, interval);
            if (type == eDungeonTimerType.WaitStart)
                _this.StartTime = time;
            if (type == eDungeonTimerType.WaitEnd)
                _this.EndTime = time;
        }


        public void CloseTimer(DungeonScene _this, eDungeonTimerType type)
        {
            if (_this.Trggers[(int) type] != null)
            {
                _this.DeleteTimer(_this.Trggers[(int) type]);
                _this.Trggers[(int) type] = null;
            }
        }

        public DateTime GetTriggerTime(DungeonScene _this, eDungeonTimerType type)
        {
            var trigger = _this.Trggers[(int) type];
            if (trigger != null && trigger.T != null)
            {
                return trigger.Time;
            }
            return DateTime.Now.AddYears(-10);
        }

        public void NotifyDungeonTime(DungeonScene _this, ObjPlayer player, eDungeonTimerType type)
        {
            var trigger = _this.Trggers[(int) type];
            if (trigger != null && trigger.T != null)
            {
                player.Proxy.NotifyDungeonTime((int) _this.State, (ulong) trigger.Time.ToBinary());
            }
        }

        public void NotifyDungeonTime(DungeonScene _this, eDungeonTimerType type)
        {
            var trigger = _this.Trggers[(int) type];
            if (trigger != null && trigger.T != null)
            {
                _this.PushActionToAllPlayer(
                    player =>
                    {
                        player.Proxy.NotifyDungeonTime((int) _this.State, (ulong) trigger.Time.ToBinary());
                    });
            }
        }

        public void OnCreate(DungeonScene _this)
        {
            var fubenId = _this.TableSceneData.FubenId;
            _this.mFubenRecord = Table.GetFuben(fubenId);
            _this.isNeedDamageModify = _this.mFubenRecord.IsDyncDifficulty == 1;
            _this.StartTimer(eDungeonTimerType.SafeClose, DateTime.Now.AddHours(1), () => { _this.TimeOverClose(); });

            TrigerBusinessman(_this);
        }

        // 触发黑市npc
        public void TrigerBusinessman(DungeonScene _this)
        {
            var sceneNpcId = _this.mFubenRecord.BusinessManSceneId;
            var pr = _this.mFubenRecord.BusinessManPR;
            if (sceneNpcId >= 0 && pr >= 0 && MyRandom.Random(10000) < pr)
            {
                _this.CreateSceneNpc(sceneNpcId);

                // 初始化商店物品数量
                var sceneNpc = Table.GetSceneNpc(sceneNpcId);
                if (sceneNpc == null)
                    return;

                var npc = Table.GetNpcBase(sceneNpc.DataID);
                if (npc == null)
                    return;

                _this.MapShopItems.Clear();
                foreach (var serviceId in npc.Service)
                {
                    if (serviceId >= 0)
                    {
                        var serviceR = Table.GetService(serviceId);
                        if (serviceR == null || serviceR.Param[0] < 0)
                            continue;

                        var shopType = serviceR.Param[0];
                        _this.MapShopItems[shopType] = new Dictionary<int, int>();
                        SceneServer.Instance.ServerControl.InitFubenStoreCounts(shopType, _this.MapShopItems[shopType]);
                    }
                }              
            }
        }

        public void OnPlayerEnterOver(DungeonScene _this, ObjPlayer player)
        {
            Scene.GetImpl().OnPlayerEnterOver(_this, player);
            
        }
        public void AfterPlayerEnterOver(DungeonScene _this, ObjPlayer player)
        {
            Scene.GetImpl().AfterPlayerEnterOver(_this, player);
            
        }


        public void OnPlayerEnter(DungeonScene _this, ObjPlayer player)
        {
            Scene.GetImpl().OnPlayerEnter(_this, player);

            //修改副本难度
            _this.ChangeDifficulty(_this.PlayerCount);

            var now = DateTime.Now;
            if (_this.GetTriggerTime(eDungeonTimerType.WaitStart) > now)
            {
                _this.NotifyDungeonTime(player, eDungeonTimerType.WaitStart);
            }
            else if (_this.GetTriggerTime(eDungeonTimerType.WaitExtraTimeStart) > now)
            {
                _this.NotifyDungeonTime(player, eDungeonTimerType.WaitExtraTimeStart);
            }
            else if (_this.GetTriggerTime(eDungeonTimerType.WaitEnd) > now)
            {
                _this.NotifyDungeonTime(player, eDungeonTimerType.WaitEnd);
            }
            else
            {
                _this.NotifyDungeonTime(player, eDungeonTimerType.WaitClose);
            }
        }

        public void OnPlayerLeave(DungeonScene _this, ObjPlayer player)
        {
            Scene.GetImpl().OnPlayerLeave(_this, player);

            //修改副本难度
            _this.ChangeDifficulty(_this.PlayerCount);
        }

        public void OnNpcDie(DungeonScene _this, ObjNPC npc, ulong characterId)
        {
            var idx = _this.MapNpcRecords.FindIndex(r => r.NpcID == npc.TypeId);
            if (idx == -1)
            {
                return;
            }
            var info = _this.MapNpcInfo.Data[idx];
            info.Alive = false;
            var data = new MapNpcInfos();
            data.Data.Add(info);
            _this.PushActionToAllPlayer(p =>
            {
                if (p.Proxy == null)
                {
                    return;
                }
                p.Proxy.NotifyNpcStatus(data);
            });
        }

        public void TimeOverStart(DungeonScene _this)
        {
            _this.StartDungeon();
        }

        public virtual void StartDungeon(DungeonScene _this)
        {
            _this.State = eDungeonState.Start;
            //通知客户端更新倒计时显示
            _this.NotifyDungeonTime(eDungeonTimerType.WaitEnd);
        }

        public void TimeOverEnd(DungeonScene _this)
        {
            _this.RemoveAllNPC();
            _this.EndDungeon();
        }

        public virtual void EndDungeon(DungeonScene _this)
        {
            _this.EnterAutoClose();
        }

        public void TimeOverClose(DungeonScene _this)
        {
            CoroutineFactory.NewCoroutine(_this.CloseDungeon).MoveNext();
        }

        public virtual IEnumerator CloseDungeon(Coroutine coroutine, DungeonScene _this)
        {
            _this.State = eDungeonState.Closing;

            //通知broke，我已经要关闭了
            var msg = SceneServer.Instance.SceneAgent.NotifySceneFinished(0, _this.Guid);
            yield return msg.SendAndWaitUntilDone(coroutine);
            var playerIds = new Uint64Array();
            playerIds.Items.AddRange(_this.mPlayerDict.Keys);
            if (playerIds.Items.Count > 0 && _this.dicGetRewardPlayers.ContainsKey(playerIds.Items[0]) == false)
            {
                var msg2 = SceneServer.Instance.LogicAgent.NotifyDungeonClose(playerIds.Items[0], _this.TypeId, playerIds);
                yield return msg2.SendAndWaitUntilDone(coroutine);
            }
            //把玩家都踢出去
            var array = _this.mPlayerDict.Values.ToArray();
            foreach (var player in array)
            {
                var co = CoroutineFactory.NewSubroutine(player.ExitDungeon, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }

            //关闭副本
            {
                var co = CoroutineFactory.NewSubroutine(_this.TryDeleteScene, coroutine, RemoveScene.SceneClose);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        public void EnterAutoClose(DungeonScene _this, int seconds = 20)
        {
            _this.State = eDungeonState.WillClose;
            _this.StartTimer(eDungeonTimerType.WaitClose, DateTime.Now.AddSeconds(seconds),
                () => { _this.TimeOverClose(); });
            _this.NotifyDungeonTime(eDungeonTimerType.WaitClose);
            NotifySceneFinished(_this);
        }

        public void Complete(DungeonScene _this, ulong playerId, FubenResult result)
        {
            CoroutineFactory.NewCoroutine(CompleteCoroutine, _this, playerId, result).MoveNext();
        }

        public virtual void CompleteToAll(DungeonScene _this, FubenResult result, int seconds = 20)
        {
            _this.PushActionToAllPlayer(player =>
            {
                if (player != null)
                {
                    _this.Complete(player.ObjId, result);
                }
            });

            _this.EnterAutoClose(seconds);
        }

        public void RemoveObj(DungeonScene _this, int id)
        {
            var toRemoveList = (from obj in _this.mObjDict where obj.Value.TypeId == id select obj.Value).ToList();
            foreach (var obj in toRemoveList)
            {
                _this.LeaveScene(obj);
            }
        }
    }

    public abstract class DungeonScene : Scene
    {
        public List<DungeonSceneExpItem> addExp = new List<DungeonSceneExpItem>();
        private static IDungeonScene mImpl;

        static DungeonScene()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (DungeonScene), typeof (DungeonSceneDefaultImpl),
                o => { mImpl = (IDungeonScene) o; });
        }

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public FubenRecord mFubenRecord;
        public eDungeonState state = eDungeonState.WillStart;
        public Trigger[] Trggers = new Trigger[(int) eDungeonTimerType.Count];
        public Dictionary<int, Dictionary<int, int>> MapShopItems = new Dictionary<int, Dictionary<int, int>>();   // 黑市商人物品计数

        public eDungeonState State
        {
            get { return state; }
            set { state = value; }
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public virtual IEnumerator CloseDungeon(Coroutine coroutine)
        {
            return mImpl.CloseDungeon(coroutine, this);
        }

        public void CloseTimer(eDungeonTimerType type)
        {
            mImpl.CloseTimer(this, type);
        }

        //public void EnterEmptyClose(int seconds = 180)
        //{
        //    mImpl.EnterEmptyClose(this,seconds);
        //}

        //public void LeaveEmptyClose()
        //{
        //    mImpl.LeaveEmptyClose(this);
        //}

        public virtual void Complete(ulong playerId, FubenResult result)
        {
            mImpl.Complete(this, playerId, result);
        }

        public virtual void CompleteToAll(FubenResult result, int seconds = 20)
        {
            mImpl.CompleteToAll(this, result, seconds);
        }

        public virtual void EndDungeon()
        {
            mImpl.EndDungeon(this);
        }

        public void EnterAutoClose(int seconds = 20)
        {
            mImpl.EnterAutoClose(this, seconds);
        }

        public DateTime GetTriggerTime(eDungeonTimerType type)
        {
            return mImpl.GetTriggerTime(this, type);
        }

        public void NotifyDungeonTime(ObjPlayer player, eDungeonTimerType type)
        {
            mImpl.NotifyDungeonTime(this, player, type);
        }

        public void NotifyDungeonTime(eDungeonTimerType type)
        {
            mImpl.NotifyDungeonTime(this, type);
        }

        public override void OnCreate()
        {
            mImpl.OnCreate(this);
        }

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            mImpl.OnNpcDie(this, npc, characterId);
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            mImpl.OnPlayerEnter(this, player);
        }

        public override void AfterPlayerEnterOver(ObjPlayer player)
        {
            mImpl.AfterPlayerEnterOver(this, player);
        }
        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            mImpl.OnPlayerEnterOver(this,player);
        }
        public override void OnPlayerLeave(ObjPlayer player)
        {
            mImpl.OnPlayerLeave(this, player);
        }

        public void RemoveObj(int id)
        {
            mImpl.RemoveObj(this, id);
        }

        public virtual void StartDungeon()
        {
            mImpl.StartDungeon(this);
        }

        public void StartTimer(eDungeonTimerType type, DateTime time, Action act, int interval = -1)
        {
            mImpl.StartTimer(this, type, time, act, interval);
        }

        public void TimeOverClose()
        {
            mImpl.TimeOverClose(this);
        }

        public void TimeOverEnd()
        {
            mImpl.TimeOverEnd(this);
        }

        public void TimeOverStart()
        {
            mImpl.TimeOverStart(this);
        }

        public double GetDungeonTimeProgress()
        {
            return mImpl.GetDungeonTimeProgress(this);
        }
    }

    public class DungeonSceneExpItem
    {
        public ulong characterId { get; set; }
        public int exp { get; set; }
    }
}