#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using Mono.GameMath;
using NLog;
using ProtoBuf;

using Shared;

#endregion

namespace Scene
{
    public interface IObjPlayer
    {
        void AddEnemy(ObjPlayer _this, ulong objId);
        void ApplyBookAttr(ObjPlayer _this, Dictionary<int, int> attrList, Dictionary<int, int> monsterAttrs);
        void ApplyEquip(ObjPlayer _this, BagBaseData bag);
        void ApplyEvent(ObjPlayer _this, int eventId, string evt, int count);
        void ApplySimpleData(ObjPlayer _this, DBCharacterSceneSimple simpleData);
        void ApplySkill(ObjPlayer _this, Dictionary<int, int> skills);
        void ApplyTalent(ObjPlayer _this, Dictionary<int, int> Talents);
        void ApplyTitles(ObjPlayer _this, List<int> titles, int type);
        void ApplyElf(ObjPlayer _this, ElfData data);
        void ApplyMount(ObjPlayer _this, MountMsgData data);

        void AutoRelive(ObjPlayer _this);
        void ChangeOutLineTime(ObjPlayer _this);
        void ChatSpeek(ObjPlayer _this, eChatChannel type, string content, List<ulong> toList);
        void CleanFreind(ObjPlayer _this);
        void Destroy(ObjPlayer _this);
        ObjData DumpObjData(ObjPlayer _this, ReasonType reason);
        IEnumerator DurableDownToLogic(Coroutine coroutine, ObjPlayer _this, Dictionary<int, int> equipList);
        void EnterSceneOver(ObjPlayer _this);
        void ThroughPos(ObjPlayer _this, Vector2 pos);
        void EquipDurableDown(ObjPlayer _this, Dictionary<int, int> equipList, bool refreshAttr);
        void ExitDungeon(ObjPlayer _this);
        IEnumerator ExitDungeon(Coroutine coroutine, ObjPlayer _this);
        void FriendsDirtyTriggerTimeOver(ObjPlayer _this);
        void FriendsLostTriggerTimeOver(ObjPlayer _this);
        int GetAllianceId(ObjPlayer _this);
        DBCharacterScene GetData(ObjPlayer _this);
        Vector2 GetDirection(ObjPlayer _this);
        SceneSyncData GetMySyncData(ObjPlayer _this);
        int GetNowLeaveExp(ObjPlayer _this, ref int maxExp);
        ObjType GetObjType(ObjPlayer _this);
        PKValueRecord GetPKValue(ObjPlayer _this);
        Vector2 GetPosition(ObjPlayer _this);
        DBCharacterSceneSimple GetSimpleData(ObjPlayer _this);
        ulong GetTeamId(ObjPlayer _this);
        List<TimedTaskItem> GetTimedTasks(ObjPlayer _this);
        ErrorCodes GmCommand(ObjPlayer _this, string command);
        void Init(ObjPlayer _this, ulong characterId, int dataId, int level);
        DBCharacterScene InitByBase(ObjPlayer _this, ulong characterId, object[] args);
        bool InitByDb(ObjPlayer _this, ulong characterId, DBCharacterScene dbData);
        void InitDB(ObjPlayer _this);
        bool InitEquip(ObjPlayer _this, int level);
        void InitKillerTrigger(ObjPlayer _this);
        void InitKillerValue(ObjPlayer _this, int value);
        bool InitSkill(ObjPlayer _this, int level);
        int InitTableData(ObjPlayer _this, int level);

        /// <summary>
        ///     是否是我的敌方
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        bool IsMyEnemy(ObjPlayer _this, ObjCharacter character);

        void MergeSceneByTeam(ObjPlayer _this);

        /// <summary>
        ///     移动角色（寻路是异步的）
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="pos">目标位置</param>
        /// <param name="offset">目标位置偏移</param>
        /// <param name="searchPath">是否需要寻路，false表示直线移动到目的地</param>
        /// <param name="pushLastPos">是否需要精确移动到目标点，因为寻路是以格子中心点为坐标的，最后可能会产生半格的误差</param>
        /// <param name="callback">寻路结果callback</param>
        /// <returns>能走返回true，不能走返回false</returns>
        MoveResult MoveTo(ObjPlayer _this,
                          Vector2 pos,
                          float offset = 0.05f,
                          bool searchPath = true,
                          bool pushLastPos = false,
                          Action<List<Vector2>> callback = null);

        void MoveToTarget(ObjPlayer _this, List<Vector2> targetList, float offset);
        IEnumerator SendPlayerEnterSceneOverToLogic(Coroutine co, ObjPlayer _this);
        IEnumerator NotifyLogicAddFriendCoroutine(Coroutine co, ulong toC, ulong addC);
        ErrorCodes NpcService(ObjPlayer _this, ulong npcId, int serviceId);
        void OnDamage(ObjPlayer _this, ObjCharacter enemy, int damage);
        void OnTrapped(ObjPlayer _this, ObjCharacter enemy);//被限制移动
        void OnExDataChanged(ObjPlayer _this, int idx, int val);
        int GetSycExData(ObjPlayer _this, int idx);
        void SendExDataChange(ObjPlayer _this, Dict_int_int_Data data);
        void OnDestroy(ObjPlayer _this);
        void OnDie(ObjPlayer _this, ulong characterId, int viewTime, int damage = 0);
        void OnEnterScene(ObjPlayer _this);
        void OnLeaveScene(ObjPlayer _this);
        bool OnMySyncRequested(ObjPlayer _this, ulong characterId, uint syncId);
        void OnSaveData(ObjPlayer _this, DBCharacterScene data, DBCharacterSceneSimple simpleData);
        void ProcessPositionChanged(ObjPlayer _this);
        void PushFriend(ObjPlayer _this, int type, ulong id);
        void RegisterMySyncData(ObjPlayer _this);
        void Relive(ObjPlayer _this, bool byItem = false);
        void RemoveAllMySyncData(ObjPlayer _this);
        void RemoveFriend(ObjPlayer _this, int type, ulong id);
        void Reset(ObjPlayer _this);
        void ResetRelivePos(ObjPlayer _this);
        void SaveBeforeScene(ObjPlayer _this);
        IEnumerator SendFriendsDataCoroutine(Coroutine co, ObjPlayer _this, int isOnline);

        void SetAllianceInfo(ObjPlayer _this, int allianceId, int ladder, string name);
        void SetDirection(ObjPlayer _this, Vector2 dir);
        void SetDirection(ObjPlayer _this, float x, float y);
        void SetFriendsDirty(ObjPlayer _this, bool value);
        void SetItemCount(ObjPlayer _this, int itemId, int count);
        void SetKillerValue(ObjPlayer _this, int value);
        void SetOutLineTime(ObjPlayer _this, DateTime value);
        void SetPkModel(ObjPlayer _this, int value);
        void SetPkTime(ObjPlayer _this, int value);
        void SetPosition(ObjPlayer _this, Vector2 p);
        void SetPosition(ObjPlayer _this, float x, float y);
        void SetTeamId(ObjPlayer _this, ulong teamId, int state);
        void StartAutoRelive(ObjPlayer _this, int seconds);
        void StopAutoRelive(ObjPlayer _this);
#if DEBUG
        void Tick(ObjPlayer _this, float delta);
#endif
        void Tick(ObjPlayer _this);
        void UpdataSceneInfoData(ObjPlayer _this, uint sceneId, ulong sceneGuid = ulong.MaxValue);
        eAreaState UpdateAreaState(ObjPlayer _this, Scene scene, bool set);
        void UpdateDbAttribute(ObjPlayer _this, eAttributeType type);
        void SummonBookMonster(ObjPlayer _this);
        void SetBookMonsterId(ObjPlayer _this, int id);
        int GetBookMonsterId(ObjPlayer _this);

        void SendForceStopMove(ObjPlayer _this);
        IEnumerator OnPlayerEnterGetTeamData(Coroutine co, ObjPlayer _this);
        void SetMountId(ObjPlayer _this, int MountId);
        int GetMountId(ObjPlayer _this);
        bool IsRiding(ObjPlayer _this);
        int GetRole(ObjPlayer _this);
        float GetAdditionExp(ObjPlayer _this);
        float GetAdditionLode(ObjPlayer _this);
    }

    public class ObjPlayerDefaultImpl : IObjPlayer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private void BeKillByPlayer(ObjPlayer _this, ulong characterId)
        {
            var Killer = _this.Scene.FindPlayer(characterId);
            if (Killer == null)
            {
                return;
            }
            var tbPvP = Table.GetPVPRule(_this.Scene.PvpRuleId);
            if (tbPvP == null)
            {
                return;
            }
            var pkValue = _this.GetPKValue();
            if (pkValue == null)
            {
                return;
            }

            //if (Killer.PkModel == 1)
            //{

            //}
            if (tbPvP.IsKillAdd == 1 && pkValue.IsKilledAddValue == 0)
            {
                if (Killer.PkModel == (int)ePkModel.GoodEvil && _this.PkTime > 0)
                {
                }
                else
                {
                    Killer.KillerValue = Killer.KillerValue + ObjPlayer.AddKillValue;
                }
            }
        }

        private IEnumerator GmCommandCoroutine(Coroutine co,
                                               ObjPlayer _this,
                                               string command,
                                               AsyncReturnValue<ErrorCodes> err)
        {
            err.Value = ErrorCodes.OK;
            var strs = command.Split(',');
            if (strs.Length < 1)
            {
                yield break;
            }
            if (String.Compare(strs[0], "!!ReloadTable", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (strs.Length < 2)
                {
                    yield break;
                }
                GameMaster.ReloadTable(strs[1]);
                yield break;
            }
            var nIndex = 0;
            var IntData = new List<long>();
            foreach (var s in strs)
            {
                if (nIndex != 0)
                {
                    long TempInt;
                    if (!long.TryParse(s, out TempInt))
                    {
                        err.Value = ErrorCodes.Gm;
                        yield break;
                    }
                    IntData.Add(TempInt);
                }
                nIndex++;
            }

            if (String.Compare(strs[0], "!!Goto", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 3)
                {
                    //GM命令切换场景
                    var targetSceneId = (int)IntData[0];
                    var x = (int)IntData[1];
                    var y = (int)IntData[2];
                    if (targetSceneId == _this.Scene.TypeId)
                    {
                        _this.SetPosition(x, y);
                        _this.BroadcastSelfPostion();
                        yield break;
                    }

                    if (_this.IsChangingScene())
                    {
                        err.Value = ErrorCodes.StateError;
                        yield break;
                    }

                    var error = AsyncReturnValue<ErrorCodes>.Create();
                    var gmCo = CoroutineFactory.NewSubroutine(GameMaster.NotifyCreateChangeSceneCoroutine, co, _this,
                        targetSceneId, x, y, error);

                    if (gmCo.MoveNext())
                    {
                        yield return gmCo;
                    }
                    err.Value = error.Value;
                    error.Dispose();
                    yield break;
                }
            }
            if (String.Compare(strs[0], "!!EnterScene", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    var player = _this;
                    if (null == player)
                    {
                        yield break;
                    }

                    if (null == player.Scene)
                    {
                        yield break;
                    }

                    //GM命令,根据GUID切换场景
                    var guid = (ulong)IntData[0];
                    var scene = SceneManager.Instance.GetScene(guid);
                    if (null != scene)
                    {
                        if (scene.Guid == player.Scene.Guid)
                        {
                            yield break;
                        }
                    }

                    var subCo = CoroutineFactory.NewSubroutine(SceneServer.Instance.ServerControl.CreateAndEnterScene,
                        co,
                        player.ObjId,
                        player.ServerId,
                        -1,
                        guid,
                        eScnenChangeType.Normal,
                        new SceneParam());
                    if (subCo.MoveNext())
                    {
                        yield return subCo;
                    }
                }
            }
            else if (String.Compare(strs[0], "!!LookScene", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 0)
                {
                    GameMaster.LookSceneManager();
                }
            }
            else if (String.Compare(strs[0], "!!SpeedSet", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    //设置速度
                    var speed = (int)IntData[0];
                    GameMaster.SetSpeed(_this, speed);
                }
            }
            else if (String.Compare(strs[0], "!!HpSet", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    //设置血量
                    var Hp = (int)IntData[0];
                    GameMaster.SetHp(_this, Hp);
                }
            }
            else if (String.Compare(strs[0], "!!AddBuff", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 2)
                {
                    //增加Buff
                    GameMaster.AddBuff(_this, (int)IntData[0], (int)IntData[1]);
                }
                else if (IntData.Count == 3)
                {
                    //增加Buff
                    GameMaster.AddBuff(_this, (int)IntData[0], (int)IntData[1], (ulong)IntData[2]);
                }
            }
            else if (String.Compare(strs[0], "!!DelBuff", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    //删除Buff
                    GameMaster.DelBuff(_this, (int)IntData[0]);
                }
            }
            else if (String.Compare(strs[0], "!!MpSet", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    //设置蓝量
                    var Mp = (int)IntData[0];
                    GameMaster.SetMp(_this, Mp);
                }
            }
            else if (String.Compare(strs[0], "!!CreateNpc", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    //创建一个怪
                    GameMaster.CreateNpc(_this, (int)IntData[0]);
                }
                else if (IntData.Count == 2)
                {
                    //创建一个怪
                    GameMaster.CreateNpc(_this, (int)IntData[0], 1 == IntData[1]);
                }
                else if (IntData.Count == 3)
                {
                    //创建一个怪
                    GameMaster.CreateNpc(_this, (int)IntData[0], 1 == IntData[1], (int)IntData[2]);
                }
                else
                {
                    var count = IntData[3];
                    for (var n = 0; n < count; n++)
                    {
                        GameMaster.CreateNpc(_this, (int)IntData[0], 1 == IntData[1], (int)IntData[2]);
                    }
                }
            }
            else if (String.Compare(strs[0], "!!DumpScene", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 0)
                {
                    var scene = _this.Scene;
                    if (null != scene)
                    {
                        scene.DumpInfo();
                    }
                }
                else if (IntData.Count == 1)
                {
                    var scene = SceneManager.Instance.GetScene((ulong)IntData[0]);
                    if (null != scene)
                    {
                        scene.DumpInfo();
                    }
                }
            }
            else if (String.Compare(strs[0], "!!ShowObjInfo", StringComparison.OrdinalIgnoreCase) == 0)
            {
                var scene = _this.Scene;
                var o = scene.FindObj((ulong)IntData[0]);
                var ids = new List<ulong>();
                ids.Add(_this.ObjId);
                SceneServer.Instance.ChatAgent.ChatNotify(ids, (int)eChatChannel.System, 0, string.Empty,
                    new ChatMessageContent { Content = o.ToString() });
            }
            else if (String.Compare(strs[0], "!!ResetObj", StringComparison.OrdinalIgnoreCase) == 0)
            {
                var scene = _this.Scene;
                var o = scene.FindObj((ulong)IntData[0]);
                o.Reset();
            }
        }

        private void Sync(ObjPlayer _this)
        {
            if (!_this.mMySyncDirtyFlag)
            {
                return;
            }

            var data = _this.GetMySyncData();
            SceneServer.Instance.ServerControl.SyncMyDataToClient(_this.mObjId, _this.Proxy.ClientId, data);
        }

        //Obj类型
        public ObjType GetObjType(ObjPlayer _this)
        {
            return ObjType.PLAYER;
        }

        public void SetPosition(ObjPlayer _this, Vector2 p)
        {
            _this.SetPosition(p.X, p.Y);
        }

        public void SetMountId(ObjPlayer _this, int MountId)
        {
            _this.mDbData.MountId = MountId;
            if (MountId > 0)
            {
                var tbMount = Table.GetMount(MountId);
                if (tbMount != null)
                {
                    _this.Attr.MoveSpeedModify = (int)tbMount.speed + ObjPlayer.NormalSpeedModify;
                    _this.Attr.SetFlag(eAttributeType.MoveSpeed);
                    return;
                }
            }
            _this.Attr.MoveSpeedModify = ObjPlayer.NormalSpeedModify;
            _this.Attr.SetFlag(eAttributeType.MoveSpeed);
        }

        public int GetMountId(ObjPlayer _this)
        {
            return _this.mDbData.MountId;
        }
        public bool IsRiding(ObjPlayer _this)
        {
            return GetMountId(_this) > 0;
        }

        public int GetRole(ObjPlayer _this)
        {
            return _this.TypeId;
        }
        public void SetPosition(ObjPlayer _this, float x, float y)
        {
            _this.mDbData.Postion.X = x;
            _this.mDbData.Postion.Y = y;
            if (Math.Abs(_this.mDirtyPosition.X - x) + Math.Abs(_this.mDirtyPosition.Y - y) > 0.1f)
            {
                _this.mPositionChanged = true;
                //TODO
                _this.DbDirty = true;
                //_this.ProcessPositionChanged();
                _this.mDirtyPosition = new Vector2(x, y);
            }
        }

        public Vector2 GetPosition(ObjPlayer _this)
        {
            return new Vector2(_this.mDbData.Postion.X, _this.mDbData.Postion.Y);
        }

        public void SetDirection(ObjPlayer _this, Vector2 dir)
        {
            dir.Normalize();
            _this.mDbData.Postion.DirX = dir.X;
            _this.mDbData.Postion.DirY = dir.Y;
        }

        public void SetDirection(ObjPlayer _this, float x, float y)
        {
            _this.SetDirection(new Vector2(x, y));
        }

        public Vector2 GetDirection(ObjPlayer _this)
        {
            return new Vector2(_this.mDbData.Postion.DirX, _this.mDbData.Postion.DirY);
        }

        public void SetItemCount(ObjPlayer _this, int itemId, int count)
        {
            _this.ItemCount[itemId] = count;
        }

        public PKValueRecord GetPKValue(ObjPlayer _this)
        {
            PKValueRecord result = null;
            Table.ForeachPKValue(record =>
            {
                result = record;
                if (_this.KillerValue <= record.Id)
                {
                    return false;
                }
                return true;
            });
            return result;
        }

        public void SetPkModel(ObjPlayer _this, int value)
        {
            if (value == _this.mDbData.PKModel)
            {
                return;
            }
            _this.mDbData.PKModel = value;
            _this.MarkDbDirty();
            _this.OnPropertyChanged((uint)eSceneSyncId.SyncPkModel);
        }

        #region  NPC服务

        public ErrorCodes NpcService(ObjPlayer _this, ulong npcId, int serviceId)
        {
            if (_this.Scene == null)
            {
                return ErrorCodes.Unknow;
            }
            var npc = _this.Scene.FindCharacter(npcId);
            if (npc == null)
            {
                return ErrorCodes.Error_NpcNotFind;
            }
            if ((npc.GetPosition() - _this.GetPosition()).LengthSquared() > 100)
            {
                return ErrorCodes.Error_NpcTooFar;
            }
            var tbNpc = Table.GetNpcBase(npc.TypeId);
            if (tbNpc == null)
            {
                return ErrorCodes.Error_NpcBaseID;
            }
            if (!tbNpc.Service.Contains(serviceId))
            {
                return ErrorCodes.Error_NpcNotHaveService;
            }
            var tbService = Table.GetService(serviceId);
            if (tbService == null)
            {
                return ErrorCodes.Error_ServiceID;
            }
            switch (tbService.Type)
            {
                case 0: //商店
                    {
                        return ErrorCodes.Need_2_Logic;
                    }
                case 1: //修理
                    {
                        return ErrorCodes.Need_2_Logic;
                    }
                case 2: //治疗
                    {
                        _this.Attr.SetDataValue(eAttributeType.HpNow, _this.Attr.GetDataValue(eAttributeType.HpMax));
                        _this.Attr.SetDataValue(eAttributeType.MpNow, _this.Attr.GetDataValue(eAttributeType.MpMax));
                        return ErrorCodes.Need_2_Logic;
                    }
                case 3: //仓库
                    {
                        return ErrorCodes.Need_2_Logic;
                    }
                default:
                    {
                    }
                    break;
            }
            return ErrorCodes.Unknow;
        }

        #endregion

        public bool InitEquip(ObjPlayer _this, int level)
        {
            return true;
        }

        public virtual void InitDB(ObjPlayer _this)
        {
            _this.BuffList.mData.Clear();
            foreach (var buffData in _this.mDbData.Buffs)
            {
                var tt = DateTime.FromBinary(buffData.OverTime);
                if (tt < DateTime.Now)
                {
                    continue;
                }
                //var buff = _this.AddBuff(buffData.BuffId, buffData.BuffLevel, this);
                var tbBuff = new BuffRecord();
                //BuffRecord tbBuff = Table.GetBuff(buffData.BuffId);
                tbBuff.Id = buffData.BuffId;
                tbBuff.Effect[0] = buffData.Effect0;
                tbBuff.Effect[1] = buffData.Effect1;
                tbBuff.Duration = buffData.Duration;
                tbBuff.Type = buffData.Type;
                tbBuff.DownLine = buffData.DownLine;
                tbBuff.IsView = buffData.IsView;
                tbBuff.Die = buffData.Die;
                tbBuff.SceneDisappear = buffData.SceneDisappear;
                tbBuff.DieDisappear = buffData.DieDisappear;
                tbBuff.HuchiId = buffData.HuchiId;
                tbBuff.TihuanId = buffData.TihuanId;
                tbBuff.PriorityId = buffData.PriorityId;
                tbBuff.BearMax = buffData.BearMax;
                tbBuff.LayerMax = buffData.LayerMax;
                tbBuff.effectid[0] = buffData.effectid0;
                tbBuff.effectid[1] = buffData.effectid1;
                tbBuff.effectid[2] = buffData.effectid2;
                tbBuff.effectid[3] = buffData.effectid3;
                tbBuff.effectpoint[0] = buffData.effectpoint0;
                tbBuff.effectpoint[1] = buffData.effectpoint1;
                tbBuff.effectpoint[2] = buffData.effectpoint2;
                tbBuff.effectpoint[3] = buffData.effectpoint3;
                tbBuff.effectparam[0, 0] = buffData.effectparam00;
                tbBuff.effectparam[0, 1] = buffData.effectparam01;
                tbBuff.effectparam[0, 2] = buffData.effectparam02;
                tbBuff.effectparam[0, 3] = buffData.effectparam03;
                tbBuff.effectparam[0, 4] = buffData.effectparam04;
                tbBuff.effectparam[0, 5] = buffData.effectparam05;
                tbBuff.effectparam[1, 0] = buffData.effectparam10;
                tbBuff.effectparam[1, 1] = buffData.effectparam11;
                tbBuff.effectparam[1, 2] = buffData.effectparam12;
                tbBuff.effectparam[1, 3] = buffData.effectparam13;
                tbBuff.effectparam[1, 4] = buffData.effectparam14;
                tbBuff.effectparam[1, 5] = buffData.effectparam15;
                tbBuff.effectparam[2, 0] = buffData.effectparam20;
                tbBuff.effectparam[2, 1] = buffData.effectparam21;
                tbBuff.effectparam[2, 2] = buffData.effectparam22;
                tbBuff.effectparam[2, 3] = buffData.effectparam23;
                tbBuff.effectparam[2, 4] = buffData.effectparam24;
                tbBuff.effectparam[2, 5] = buffData.effectparam25;
                tbBuff.effectparam[3, 0] = buffData.effectparam30;
                tbBuff.effectparam[3, 1] = buffData.effectparam31;
                tbBuff.effectparam[3, 2] = buffData.effectparam32;
                tbBuff.effectparam[3, 3] = buffData.effectparam33;
                tbBuff.effectparam[3, 4] = buffData.effectparam34;
                tbBuff.effectparam[3, 5] = buffData.effectparam35;
                tbBuff.EffectPointParam[0] = buffData.EffectPoint0Param;
                tbBuff.EffectPointParam[1] = buffData.EffectPoint1Param;
                tbBuff.EffectPointParam[2] = buffData.EffectPoint2Param;
                tbBuff.EffectPointParam[3] = buffData.EffectPoint3Param;
                var buff = _this.AddBuff(tbBuff, buffData.BuffLevel, _this);
                if (true)
                {
                    //按结束时间为准
                    buff.SetDuration(tt.GetDiffSeconds(DateTime.Now), true);
                }
                //else
                //{//按剩余时间为准
                //    buff.SetDuration(buffData.Seconds * 1000);
                //}
            }
            _this.mDbData.Buffs.Clear();
            InitKillerTrigger(_this);
            SetMountId(_this, _this.mDbData.MountId);
        }

        public bool InitSkill(ObjPlayer _this, int level)
        {
            return true;
        }

        public void OnEnterScene(ObjPlayer _this)
        {
            _this.mIsForceMoving = false;
            _this.mIsMoving = false;
            _this.mWaitingToMove = false;
            _this.mTargetPos.Clear();
            _this.RegisterAllSyncData();
            _this.RegisterMySyncData();
//            _this.ResetChangeSceneTime();
            //移到进副本前修正
            //             if (null != _this.Scene)
            //             {
            //                 var temp = _this.Scene.FindNearestValidPosition(_this.GetPosition());
            //                 if (null != temp)
            //                 {
            //                     _this.SetPosition(temp.Value);
            //                 }
            //                 else
            //                 {
            //                     _this.SetPosition((float)_this.Scene.TableSceneData.Safe_x, (float)_this.Scene.TableSceneData.Safe_z);
            //                 }
            // 
            //                 // 先设置好坐标，才能获取场景信息
            //                 var p = _this.GetPosition();
            //                 var tag = _this.Scene.GetObstacleValue(p.X, p.Y);
            //                 if (tag == SceneObstacle.ObstacleValue.Runable)
            //                 {
            //                     _this.NormalAttr.AreaState = eAreaState.Wild;
            //                 }
            //                 else
            //                 {
            //                     _this.NormalAttr.AreaState = eAreaState.City;
            //                 }
            //             }
            _this.ClearEnemy();
            _this.BuffList.OnEnterScene();
            _this.FriendsDirty = true;
        }

        public void OnLeaveScene(ObjPlayer _this)
        {
            _this.mIsForceMoving = false;
            _this.mWaitingToMove = false;
            _this.mIsMoving = false;
            _this.mTargetPos.Clear();
            _this.BuffList.OnLeaveScene();
            _this.ChangeOutLineTime();
            ObjCharacter.GetImpl().OnLeaveScene(_this);

            _this.RemoveAllMySyncData();
            _this.RemoveAllSyncData();
        }

#if DEBUG //debug调试用，如果需要修改Tick逻辑，请把宏去掉

        public void Tick(ObjPlayer _this, float delta)
        {
            ObjCharacter.GetImpl().Tick(_this, delta);
        }
#endif

        public void ThroughPos(ObjPlayer _this, Vector2 pos)
        {
            _this.StopMove();
            _this.SetPosition(pos);
            _this.SyncCharacterPostion();
            var retinues = _this.GetRetinueList();

            if (retinues != null)
            {
                foreach (var retinue in retinues)
                {
                    var rePos = retinue.GetPosition();
                    var distance = (pos - rePos).LengthSquared();
                    if (distance > 400)
                    {
                        retinue.SetPosition(pos);
                    }
                }
            }
        }
        public void EnterSceneOver(ObjPlayer _this)
        {
            _this.mActive = true;
            _this.EndChangeScene();
            if (_this.Zone == null)
            {
                return;
            }
            _this.Zone.MarkDirty();


            var msg2Me = new CreateObjMsg();
            foreach (var zone in _this.Zone.VisibleZoneList)
            {
                foreach (var pair in zone.ObjDict)
                {
                    var obj = pair.Value;

                    if (!obj.Active)
                    {
                        continue;
                    }

                    if (obj.ObjId == _this.ObjId)
                    {
                        continue;
                    }

                    //如果obj
                    if (!obj.IsVisibleTo(_this))
                    {
                        continue;
                    }

                    var data = obj.DumpObjData(ReasonType.VisibilityChanged);
                    msg2Me.Data.Add(data);
                }
            }

            if (msg2Me.Data.Count > 0)
            {
                _this.Proxy.CreateObj(new[] { _this.ObjId }, msg2Me);
            }

            _this.BroadcastCreateMe();

            _this.Scene.OnPlayerEnterOver(_this);

            if (_this.BuffList.mData.Count > 0)
            {
                var replyMsg = new BuffResultMsg();
                foreach (var buff in _this.BuffList.mData)
                {
                    var buffResult = new BuffResult
                    {
                        SkillObjId = _this.ObjId,
                        TargetObjId = _this.ObjId,
                        BuffTypeId = buff.GetBuffId(),
                        BuffId = buff.mId,
                        Type = BuffType.HT_CHANGE_SCENE,
                        ViewTime = 0
                    };
                    if (buff.mBuff.IsView == 1)
                    {
                        buffResult.Param.Add(buff.GetLastSeconds());
                        buffResult.Param.Add(buff.GetLayer());
                        buffResult.Param.Add(buff.m_nLevel);
                    }
                    replyMsg.buff.Add(buffResult);
                }
                _this.BroadcastBuffList(replyMsg);
            }
            CoroutineFactory.NewCoroutine(SendPlayerEnterSceneOverToLogic, _this).MoveNext();

            if (0 != _this.Scene.TableSceneData.CanSummonMonster)
            {
                _this.SummonBookMonster();
            }

            //队伍：有队员场景typeId变化时，同步队员数据
            {
                CoroutineFactory.NewCoroutine(OnPlayerEnterGetTeamData, _this).MoveNext();
            }
            _this.Scene.AfterPlayerEnterOver(_this);
        }


        //输出成一个objdata用于客户端创建
        public ObjData DumpObjData(ObjPlayer _this, ReasonType reason)
        {
            var data = ObjCharacter.GetImpl().DumpObjData(_this, reason);
            var logicServerId = SceneExtension.GetServerLogicId(_this.ServerId);
            data.ExtData.Add(logicServerId);
            data.PkModel = _this.PkModel;
            data.PkValue = _this.KillerValue;
            data.Reborn = _this.Attr.Ladder;
            data.AreaState = (int)_this.NormalAttr.AreaState;
            _this.GetEquipsModel(data.EquipsModel);
            data.Titles.AddRange(_this.Attr.mEquipedTitles);
            data.AllianceName = _this.AllianceName;
            data.MountId = _this.GetMountId();
            return data;
        }

        public void MoveToTarget(ObjPlayer _this, List<Vector2> targetList, float offset)
        {
            if (targetList.Count <= 0)
            {
                return;
            }

            _this.mTargetPos = targetList;
            _this.MoveTo(_this.mTargetPos[0]);
        }

        /// <summary>
        ///     移动角色（寻路是异步的）
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="pos">目标位置</param>
        /// <param name="offset">目标位置偏移</param>
        /// <param name="searchPath">是否需要寻路，false表示直线移动到目的地</param>
        /// <param name="pushLastPos">是否需要精确移动到目标点，因为寻路是以格子中心点为坐标的，最后可能会产生半格的误差</param>
        /// <param name="callback">寻路结果callback</param>
        /// <returns>能走返回true，不能走返回false</returns>
        public MoveResult MoveTo(ObjPlayer _this,
                                 Vector2 pos,
                                 float offset = 0.01f,
                                 bool searchPath = true,
                                 bool pushLastPos = false,
                                 Action<List<Vector2>> callback = null)
        {
            if (!_this.CanMove())
            {
                return MoveResult.CannotMoveByBuff;
            }

            if (_this.mTargetPos.Count == 1)
            {
                _this.mTargetPos[0] = _this.mTargetPos[0] - _this.GetDirection() * offset;
            }
            else
            {
                _this.mTargetPos[_this.mTargetPos.Count - 1] = _this.mTargetPos[_this.mTargetPos.Count - 1] -
                                                               Vector2.Normalize(
                                                                   _this.mTargetPos[_this.mTargetPos.Count - 1] -
                                                                   _this.mTargetPos[_this.mTargetPos.Count - 2]) * offset;
            }

            _this.OnMoveBegin();

            return MoveResult.Ok;
        }

        public void ProcessPositionChanged(ObjPlayer _this)
        {
            if (!_this.mPositionChanged)
            {
                return;
            }

            _this.mPositionChanged = false;

            try
            {
                _this.UpdateZone();
                _this.UpdateTriggerArea();
                //UpdatePostionData(mPosition, mDirection);
                UpdateAreaState(_this, _this.Scene, true);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public eAreaState UpdateAreaState(ObjPlayer _this, Scene scene, bool set)
        {
            if (null == scene)
            {
                return eAreaState.City;
            }

            var tag = scene.GetObstacleValue(_this.GetPosition().X, _this.GetPosition().Y);
            if (tag == SceneObstacle.ObstacleValue.Runable)
            {
                //跑
                if (_this.Attr.MoveSpeedModify == ObjPlayer.NormalSpeedModify)
                {
                    _this.Attr.MoveSpeedModify = ObjPlayer.RunSpeedModify;
                    _this.Attr.SetFlag(eAttributeType.MoveSpeed);
                }
                if (set)
                {
                    _this.NormalAttr.AreaState = eAreaState.Wild;
                }
                return eAreaState.Wild;
            }
            if (tag == SceneObstacle.ObstacleValue.Walkable)
            {
                //走
                if (_this.Attr.MoveSpeedModify == ObjPlayer.RunSpeedModify)
                {
                    _this.Attr.MoveSpeedModify = ObjPlayer.NormalSpeedModify;
                    _this.Attr.SetFlag(eAttributeType.MoveSpeed);
                }
                if (set)
                {
                    _this.NormalAttr.AreaState = eAreaState.City;
                }
                return eAreaState.City;
            }

            return eAreaState.City;
        }

        public void UpdateDbAttribute(ObjPlayer _this, eAttributeType type)
        {
            if (type == eAttributeType.HpNow)
            {
                _this.mDbData.Hp = _this.Attr.GetDataValue(eAttributeType.HpNow);
                _this.MarkDbDirty();
            }
            else if (type == eAttributeType.MpNow)
            {
                _this.mDbData.Mp = _this.Attr.GetDataValue(eAttributeType.MpNow);
                _this.MarkDbDirty();
            }
        }

        public void UpdataSceneInfoData(ObjPlayer _this, uint sceneId, ulong sceneGuid = ulong.MaxValue)
        {
            if (sceneGuid != ulong.MaxValue && _this.mDbData.SceneGuid != sceneGuid)
            {
                _this.mDbData.SceneGuid = sceneGuid;
                _this.DbDirty = true;
            }
            if (_this.mDbData.SceneId != sceneId)
            {
                _this.mDbData.SceneId = sceneId;
                _this.DbDirty = true;
            }
        }

        public void OnDie(ObjPlayer _this, ulong characterId, int viewTime, int damage = 0)
        {
            ObjCharacter.GetImpl().OnDie(_this, characterId, viewTime, damage);
            if (_this.GetMountId() > 0)
            {
                _this.RideMount(0);
                _this.SetMountId(0);
            }
            if (_this.Proxy != null)
            {
                ObjCharacter c = null;
                if (_this.Scene != null)
                {
                    c = _this.Scene.FindCharacter(characterId);
                }
                _this.Proxy.NotifyMessage((int)eSceneNotifyType.Die, c == null ? _this.GetName() : c.GetName(), 0);
            }

            int step = 0;
            try
            {
                var isInPvp = false;
                //pvp被杀了之后，仇人不会加入到仇人列表中
                if (_this.Scene != null)
                {
                    step = 1;
                    if (_this.Scene.TableSceneData != null)
                    {
                        if (_this.Scene.TableSceneData.Type == (int)eSceneType.Pvp || _this.Scene.TableSceneData.PvPRule == 5)
                        {
                            //pvp中不增加杀气和仇人
                            isInPvp = true;
                        }
                    }
                    _this.Scene.OnPlayerDie(_this, characterId);
                }
                if (isInPvp == false)
                {
                    if (characterId != _this.ObjId)
                    {
                        step = 2;
                        //自己死亡不触发逻辑
                        if (!StaticData.IsRobot(characterId))
                        {
                            if (characterId != _this.ObjId)
                            {
                                //添加邮件 杀死自己的是谁
                                var Killer = _this.Scene.FindPlayer(characterId);
                                CoroutineFactory.NewCoroutine(NotifyLogicKillSelfCoroutine, _this.ObjId, characterId, Killer.mName).MoveNext();

                                CoroutineFactory.NewCoroutine(NotifyLogicKillOtherCoroutine, characterId, _this.ObjId, _this.mName).MoveNext();

                                BeKillByPlayer(_this, characterId);
                                CoroutineFactory.NewCoroutine(NotifyLogicAddFriendCoroutine, _this.ObjId, characterId)
                                    .MoveNext();
                            }
                        }
                    }

                    step = 3;
                    //装备耐久相关
                    var equipDurability = 1;
                    var pkValue = _this.GetPKValue();
                    if (pkValue != null)
                    {
                        if (pkValue.IsDeadDouble == 1)
                        {
                            equipDurability = 2;
                        }
                    }
                    var durableList = new Dictionary<int, int>();
                    var refreshAttr = false;
                    step = 4;
                    foreach (var itemEquip2 in _this.Equip)
                    {
                        if (itemEquip2.Key == 120)
                        {
                            continue;
                        }
                        var equip = itemEquip2.Value;
                        if(null == equip)
                            continue;

                        var now = equip.GetExdata(22);
                        if (now <= 0)
                        {
                            continue;
                        }
                        var tbEquip = Table.GetEquip(equip.GetId());
                        if (null == tbEquip || tbEquip.DurableType < 1)
                        {
                            continue;
                        }
                        var diff = -tbEquip.Durability * equipDurability * 5 / 100;
                        var durable = now + diff;
                        equip.SetDurable(durable);
                        if (durable <= 0)
                        {
                            refreshAttr = true;
                        }
                        durableList.Add(itemEquip2.Key, diff);
                    }
                    if (durableList.Count > 0)
                    {
                        _this.EquipDurableDown(durableList, refreshAttr);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "step{0}", step);
            }

            if (_this.Scene != null)
            {
                var tbScene = _this.Scene.TableSceneData;
                if (tbScene != null && tbScene.SafeReliveCD == -1 && tbScene.FubenId != -1)
                {
                    _this.StartAutoRelive(600);
                }
                else
                {
                    _this.StartAutoRelive(60);
                }
            }
            else
            {
                _this.StartAutoRelive(60);
            }

            _this.ClearRetinue();
            //SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(3000), Relive);
        }
        //通知玩家杀死自己的是谁
        public IEnumerator NotifyLogicKillSelfCoroutine(Coroutine co, ulong characterId, ulong killId, string killName)
        {
            var msgNoticeScene = SceneServer.Instance.LogicAgent.SSSendMailById(characterId, 250, (int)SendToCharacterMailType.BeKillInfo, killId.ToString(), killName);
            yield return msgNoticeScene.SendAndWaitUntilDone(co);
        }
        //通知击杀其他玩家
        public IEnumerator NotifyLogicKillOtherCoroutine(Coroutine co, ulong characterId, ulong killId, string killTime)
        {
            var msgNoticeScene = SceneServer.Instance.LogicAgent.SSSendMailById(characterId, 251, (int)SendToCharacterMailType.BeKillInfo, killId.ToString(), killTime);
            yield return msgNoticeScene.SendAndWaitUntilDone(co);
        }
        public IEnumerator NotifyLogicAddFriendCoroutine(Coroutine co, ulong toC, ulong addC)
        {
            var msgChgScene = SceneServer.Instance.LogicAgent.SSAddFriendById(toC, addC, 2);
            yield return msgChgScene.SendAndWaitUntilDone(co);
        }
        public IEnumerator SendPlayerEnterSceneOverToLogic(Coroutine co, ObjPlayer _this)
        {
            _this.dicFlagTemp.Clear();
            var msg = SceneServer.Instance.LogicAgent.OnPlayerEnterSceneOver(_this.ObjId, _this.Scene.TypeId);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Timeout)
            {
                if (msg.ErrorCode == (int)ErrorCodes.OK)
                {
                    foreach (var v in msg.Response.exData)
                    {
                        _this.dicFlagTemp[v.Key] = v.Value;
                    }
                    foreach (var v in msg.Response.flagData)
                    {
                        _this.SetFlag(v.Key, v.Value > 0);
                    }
                }
            }
        }

        public void StartAutoRelive(ObjPlayer _this, int seconds)
        {
            var autoRelive = DateTime.Now.AddSeconds(seconds);
            _this.mDbData.AutoRelive = autoRelive.ToBinary();
            if (_this.mAutoRelive != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.mAutoRelive);
            }
            _this.mAutoRelive = SceneServerControl.Timer.CreateTrigger(autoRelive, () => { _this.AutoRelive(); });
        }

        public void StopAutoRelive(ObjPlayer _this)
        {
            if (_this.mAutoRelive != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.mAutoRelive);
                _this.mAutoRelive = null;
            }
        }

        public void AutoRelive(ObjPlayer _this)
        {
            _this.StopAutoRelive();
            if (_this.SelectReliveType != 0)
            {
                //已经向logic请求其他的复活方式了
                _this.SelectReliveType = 1;
                return;
            }

            if (_this.Scene == null)
            {
                Logger.Warn("AutoRelive scene is null!CharacterId={0}", _this.ObjId);
                return;
            }

            _this.Scene.AutoRelive(_this);

            //             BuffResultMsg msg = new BuffResultMsg();
            //             msg.buff.Add(new BuffResult
            //             {
            //                 SkillObjId = TypeDefine.INVALID_ULONG,
            //                 TargetObjId = _this.ObjId,
            //                 Type = BuffType.HT_RELIVE
            //             });
            //             _this.BroadcastBuffList(msg);
        }

        public void Reset(ObjPlayer _this)
        {
            _this.ResetChangeSceneTime();
            _this.mIsMoving = false;
            _this.mIsForceMoving = false;
            _this.mWaitingToMove = false;
            _this.mTargetPos.Clear();
            _this.ClearEnemy();
            _this.CleanBullet();
            _this.BuffList.DelBuffByOnDie();
            _this.ResetAttribute();
        }

        //AsyncReturnValue<ErrorCodes> error = AsyncReturnValue<ErrorCodes>.Create();
        ////var gmCo = CoroutineFactory.NewSubroutine(GameMaster.GmGoto, coroutine, Character, targetSceneId, x, y, error);
        //var gmCo = CoroutineFactory.NewSubroutine(GameMaster.NotifyCreateChangeSceneCoroutine, coroutine, Character, targetSceneId, x, y, error);

        //if (gmCo.MoveNext())
        //{
        //    yield return gmCo;
        //}
        public bool InitByDb(ObjPlayer _this, ulong characterId, DBCharacterScene dbData)
        {
            PlayerLog.WriteLog(characterId, "----------Scene--------------------InitByDb--------------------{0}",
                dbData.SaveCount);
            _this.mDbData = dbData;
            _this.Init(characterId, _this.mDbData.TypeId, 1);
            _this.InitDB();
            //之所以单独写个这玩意就是为了计算初始的速度用

            //Attr.SetDataValue(eAttributeType.HpNow, Attr.GetDataValue(eAttributeType.HpMax));
            //Attr.SetDataValue(eAttributeType.MpNow, Attr.GetDataValue(eAttributeType.MpMax)); 
            return true;
        }

        public void Init(ObjPlayer _this, ulong characterId, int dataId, int level)
        {
            _this.mObjId = characterId;
            _this.mTypeId = dataId;
            _this.mDirection = new Vector2(1, 0);
            _this.BuffList = new BuffList();
            _this.BuffList.InitByBase(_this);
            _this.Attr = new FightAttr(_this);
            _this.Skill = new SkillManager(_this);
            _this.InitTableData(level);
            _this.InitEquip(level);
            _this.InitSkill(level);
            _this.InitBuff(level);
            _this.Attr.PlayerInit = true;
        }

        public void RegisterMySyncData(ObjPlayer _this)
        {
            PlayerLog.WriteLog(_this.mObjId, "----------Scene----------RegisterMySyncData----------");
            for (var i = eSceneSyncId.SyncLevel; i < eSceneSyncId.SyncMax; i++)
            {
                if (i >= eSceneSyncId.Count && i < eSceneSyncId.SyncCountNext)
                {
                    continue;
                }

                if (i == eSceneSyncId.SyncHpMax ||
                    i == eSceneSyncId.SyncMpMax ||
                    i == eSceneSyncId.SyncHpNow ||
                    i == eSceneSyncId.SyncMpNow ||
                    i == eSceneSyncId.SyncMoveSpeed ||
                    i == eSceneSyncId.SyncLevel ||
                    i == eSceneSyncId.SyncAreaState ||
                    i == eSceneSyncId.SyncFightValue ||
                    i == eSceneSyncId.SyncPkModel ||
                    i == eSceneSyncId.SyncPkValue)
                {
                    continue;
                }
                _this.OnMySyncRequested(_this.mObjId, (uint)i);
            }
        }

        public SceneSyncData GetMySyncData(ObjPlayer _this)
        {
            _this.mMySyncData.Datas.Clear();
            if (!_this.mMySyncDirtyFlag)
            {
                return _this.mMySyncData;
            }

            foreach (var s in _this.mMyDirtyFlag)
            {
                foreach (var sync in s.Value)
                {
                    if (sync.Value.Dirty)
                    {
                        _this.mMySyncData.Datas.Add(new SceneSyncDataItem
                        {
                            Data = sync.Value.Getter(),
                            CharacterId = _this.mObjId,
                            Id = sync.Key
                        });

                        sync.Value.Dirty = false;
                    }
                }
            }

            _this.mMySyncDirtyFlag = false;

            return _this.mMySyncData;
        }

        public void RemoveAllMySyncData(ObjPlayer _this)
        {
            PlayerLog.WriteLog(_this.mObjId, "----------Scene----------RemoveAllSyncData----------");
            foreach (var flag in _this.mMyDirtyFlag)
            {
                flag.Key.PropertyChanged -= _this.PropertyChangedHandler;
                flag.Value.Clear();
            }

            _this.mMyDirtyFlag.Clear();
        }

        public bool OnMySyncRequested(ObjPlayer _this, ulong characterId, uint syncId)
        {
            var syncType = (eSceneSyncId)syncId;
            switch (syncType)
            {
                case eSceneSyncId.SyncLevel:
                    _this.MyAddSyncData(syncId, _this.Attr, () => _this.Attr.GetDataValue(eAttributeType.Level));
                    break;
                case eSceneSyncId.SyncStrength:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.Strength));
                    break;
                case eSceneSyncId.SyncAgility:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.Agility));
                    break;
                case eSceneSyncId.SyncIntelligence:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.Intelligence));
                    break;
                case eSceneSyncId.SyncEndurance:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.Endurance));
                    break;
                case eSceneSyncId.SyncPhyPowerMin:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.PhyPowerMin));
                    break;
                case eSceneSyncId.SyncPhyPowerMax:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.PhyPowerMax));
                    break;
                case eSceneSyncId.SyncMagPowerMin:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.MagPowerMin));
                    break;
                case eSceneSyncId.SyncMagPowerMax:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.MagPowerMax));
                    break;
                case eSceneSyncId.SyncAddPower:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.AddPower));
                    break;
                case eSceneSyncId.SyncPhyArmor:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.PhyArmor));
                    break;
                case eSceneSyncId.SyncMagArmor:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.MagArmor));
                    break;
                case eSceneSyncId.SyncDamageResistance:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.DamageResistance));
                    break;
                case eSceneSyncId.SyncHpMax:
                    _this.MyAddSyncData(syncId, _this.Attr, () =>
                    {
                        var hpMax = _this.Attr.GetDataValue(eAttributeType.HpMax);
                        if (_this.Scene != null && _this.Scene.isNeedDamageModify)
                        {
                            if (Scene.IsNeedChangeHp(_this) != null)
                            {
                                hpMax = (int)(hpMax / _this.Scene.BeDamageModify);
                            }
                        }

                        return hpMax;
                    });
                    break;
                case eSceneSyncId.SyncMpMax:
                    _this.MyAddSyncData(syncId, _this.Attr, () =>
                    {
                        var mpMax = _this.Attr.GetDataValue(eAttributeType.MpMax);
                        return mpMax;
                    });
                    break;
                case eSceneSyncId.SyncLuckyPro:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.LuckyPro));
                    break;
                case eSceneSyncId.SyncLuckyDamage:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.LuckyDamage));
                    break;
                case eSceneSyncId.SyncExcellentPro:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.ExcellentPro));
                    break;
                case eSceneSyncId.SyncExcellentDamage:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.ExcellentDamage));
                    break;
                case eSceneSyncId.SyncHit:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.Hit));
                    break;
                case eSceneSyncId.SyncDodge:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.Dodge));
                    break;
                case eSceneSyncId.SyncDamageAddPro:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.DamageAddPro));
                    break;
                case eSceneSyncId.SyncDamageResPro:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.DamageResPro));
                    break;
                case eSceneSyncId.SyncDamageReboundPro:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.DamageReboundPro));
                    break;
                case eSceneSyncId.SyncIgnoreArmorPro:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.IgnoreArmorPro));
                    break;
                case eSceneSyncId.SyncMoveSpeed:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => { return _this.Attr.GetDataValue(eAttributeType.MoveSpeed); });
                    break;
                case eSceneSyncId.SyncHitRecovery:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.HitRecovery));
                    break;
                case eSceneSyncId.SyncHpNow:
                    _this.MyAddSyncData(syncId, _this.Attr, () =>
                    {
                        var hpNow = _this.Attr.GetDataValue(eAttributeType.HpNow);
                        if (_this.Scene != null && _this.Scene.isNeedDamageModify)
                        {
                            if (Scene.IsNeedChangeHp(_this) != null)
                            {
                                hpNow = (int)(hpNow / _this.Scene.BeDamageModify);
                            }
                        }
                        return hpNow;
                    });

                    break;
                case eSceneSyncId.SyncMpNow:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.MpNow));
                    break;
                case eSceneSyncId.SyncFightValue:
                    _this.MyAddSyncData(syncId, _this.Attr,
                        () => { return _this.Attr.GetFightPoint(); });
                    break;
                case eSceneSyncId.SyncAreaState:
                    _this.MyAddSyncData(syncId, _this.NormalAttr,
                        () => { return ((int)(_this.NormalAttr.AreaState)); });
                    break;
                case eSceneSyncId.SyncPkModel:
                    {
                        var player = _this;
                        if (player != null)
                        {
                            _this.MyAddSyncData(syncId, _this, () => player.PkModel);
                        }
                    }
                    break;
                case eSceneSyncId.SyncPkValue:
                    {
                        var player = _this;
                        if (player != null)
                        {
                            _this.MyAddSyncData(syncId, player,
                                () => player.KillerValue + player.PkTime);
                        }
                    }
                    break;
                case eSceneSyncId.SyncTitle0:
                    {
                        _this.MyAddSyncData(syncId, _this.Attr, () => _this.Attr.mEquipedTitles[0]);
                    }
                    break;
                case eSceneSyncId.SyncTitle1:
                    {
                        _this.MyAddSyncData(syncId, _this.Attr, () => _this.Attr.mEquipedTitles[1]);
                    }
                    break;
                case eSceneSyncId.SyncTitle2:
                    {
                        _this.MyAddSyncData(syncId, _this.Attr, () => _this.Attr.mEquipedTitles[2]);
                    }
                    break;
                case eSceneSyncId.SyncTitle3:
                    {
                        _this.MyAddSyncData(syncId, _this.Attr, () => _this.Attr.mEquipedTitles[3]);
                    }
                    break;
                case eSceneSyncId.SyncTitle4:
                    {
                        _this.MyAddSyncData(syncId, _this.Attr, () => _this.Attr.mEquipedTitles[4]);
                    }
                    break;
                case eSceneSyncId.SyncAllianceName:
                    {
                        _this.MyAddSyncData(syncId, _this, () => _this.AllianceName);
                    }
                    break;
                case eSceneSyncId.SyncFireAttack:
                    {
                        _this.MyAddSyncData(syncId, _this.Attr,
                           () => _this.Attr.GetDataValue(eAttributeType.FireAttack));
                    }
                    break;

                case eSceneSyncId.SyncIceAttack:
                    {
                        _this.MyAddSyncData(syncId, _this.Attr,
                           () => _this.Attr.GetDataValue(eAttributeType.IceAttack));
                    }
                    break;
                case eSceneSyncId.SyncPoisonAttack:
                    {
                        _this.MyAddSyncData(syncId, _this.Attr,
                           () => _this.Attr.GetDataValue(eAttributeType.PoisonAttack));
                    }
                    break;
                case eSceneSyncId.SyncFireResistance:
                    {
                        _this.MyAddSyncData(syncId, _this.Attr,
                           () => _this.Attr.GetDataValue(eAttributeType.FireResistance));
                    }
                    break;
                case eSceneSyncId.SyncIceResistance:
                    {
                        _this.MyAddSyncData(syncId, _this.Attr,
                           () => _this.Attr.GetDataValue(eAttributeType.IceResistance));
                    }
                    break;
                case eSceneSyncId.SyncPoisonResistance:
                    {
                        _this.MyAddSyncData(syncId, _this.Attr,
                           () => _this.Attr.GetDataValue(eAttributeType.PoisonResistance));
                    }
                    break;
                case eSceneSyncId.SyncPlayerName:
                    {
                        _this.MyAddSyncData(syncId, _this, () => _this.GetName());
                    }
                    break;
            }

            return true;
        }

        public DBCharacterScene InitByBase(ObjPlayer _this, ulong characterId, object[] args)
        {
            if (args.Length < 1)
            {
                Logger.Fatal("Scene Character InitByBase Faild!! args is null");
                return null;
            }
            var typeId = (int)args[0];
            var tbRole = Table.GetActor(typeId);
            if (tbRole == null)
            {
                Logger.Fatal("Scene Character InitByBase Faild!! TypeId = {0}", typeId);
                return null;
            }
            //int sceneId = int.Parse(Table.GetServerConfig(0).Value);
            //int x = int.Parse(Table.GetServerConfig(1).Value);
            //int y = int.Parse(Table.GetServerConfig(2).Value);
            var sceneId = tbRole.BirthScene;
            var x = (int)tbRole.BirthPosX;
            var y = (int)tbRole.BirthPosY;


            var dbCharacterScene = new DBCharacterScene
            {
                Id = characterId,
                Postion = new DBPositionData
                {
                    X = x,
                    Y = y,
                    DirX = 0,
                    DirY = 0
                },
                SceneId = (uint)sceneId,
                FormerSceneId = (uint)sceneId,
                FormerPostion = new DBPositionData
                {
                    X = x,
                    Y = y,
                    DirX = 0,
                    DirY = 0
                },
                Mp = 0,
                Hp = 0
            };

            _this.mDbData = dbCharacterScene;
            //SetPosition(mDbData.Postion.Pos.x, mDbData.Postion.Pos.y);
            _this.Init(characterId, (int)args[0], 1);
            //_this.Attr.SetDataValue(eAttributeType.Level, 1);
            _this.mDbData.Hp = _this.Attr.GetDataValue(eAttributeType.HpMax);
            _this.mDbData.Mp = _this.Attr.GetDataValue(eAttributeType.MpMax);
            _this.mDbData.TypeId = typeId;
            if (args.Length > 1)
            {
                _this.mDbData.IsGM = (bool)args[1];
            }
            return _this.mDbData;
        }

        public void ApplySimpleData(ObjPlayer _this, DBCharacterSceneSimple simpleData)
        {
        }

        public int InitTableData(ObjPlayer _this, int level)
        {
            level = ObjCharacter.GetImpl().InitTableData(_this, level);
            _this.mCamp = _this.TableCharacter.Camp; //注意这里不用NPC表里的阵营id了
            _this.TableCamp = Table.GetCamp(_this.TableCharacter.Camp);
            return level;
        }

        public DBCharacterSceneSimple GetSimpleData(ObjPlayer _this)
        {
            DBCharacterSceneSimple simple;
            CharacterManager<ObjPlayer, DBCharacterScene, DBCharacterSceneSimple>.DataItem data;
            var dic = CharacterManager.Instance.mDictionary;
            if (dic.TryGetValue(_this.ObjId, out data))
            {
                simple = data.SimpleData;
            }
            else
            {
                Logger.Info("GetSimpleData return null, id = {0}", _this.ObjId);
                simple = new DBCharacterSceneSimple();
            }
            simple.Id = _this.ObjId;
            simple.TypeId = _this.TypeId;
            simple.Name = _this.mName;
            if (_this.Scene != null)
            {
                simple.SceneId = _this.Scene.TypeId;
            }
            else
            {
                simple.SceneId = (int)_this.mDbData.SceneId;
            }
            simple.FightPoint = _this.Attr.GetFightPoint();
            simple.Level = _this.GetLevel();
            simple.Ladder = _this.Attr.Ladder;
            simple.ServerId = _this.ServerId;
            simple.Vip = _this.VipLevel;
            var StarNum = 0;
            _this.dicFlagTemp.TryGetValue((int)eExdataDefine.e688, out StarNum);
            simple.StarNum = StarNum;//_this.GetExdata((int)eExdataDefine.e688);
            simple.CheckAttr.Clear();
            if (_this.TypeId == 0 || _this.TypeId == 2)
            {
                simple.CheckAttr.Add(_this.Attr.GetDataValue(eAttributeType.PhyPowerMin));
                simple.CheckAttr.Add(_this.Attr.GetDataValue(eAttributeType.PhyPowerMax));
            }
            else
            {
                simple.CheckAttr.Add(_this.Attr.GetDataValue(eAttributeType.MagPowerMin));
                simple.CheckAttr.Add(_this.Attr.GetDataValue(eAttributeType.MagPowerMax));
            }
            simple.CheckAttr.Add(_this.Attr.GetDataValue(eAttributeType.HpMax));
            simple.CheckAttr.Add(_this.Attr.GetDataValue(eAttributeType.MpMax));
            simple.CheckAttr.Add(_this.Attr.GetDataValue(eAttributeType.PhyArmor));
            simple.CheckAttr.Add(_this.Attr.GetDataValue(eAttributeType.MagArmor));
            simple.LastTime = DateTime.Now.ToBinary();
            _this.Attr.CopyToAttr(simple.AttrList);
            return simple;
        }

        public DBCharacterScene GetData(ObjPlayer _this)
        {
            _this.mDbData.Buffs.Clear();
            foreach (var data in _this.BuffList.mData)
            {
                if (data.mBuff.SceneDisappear == 1)
                {
                    continue;
                }
                var lastSeconds = data.GetLastSeconds();
                if (lastSeconds == -1)
                {
                    continue;
                }
                var temp = new DBBuffData
                {
                    BuffId = data.GetBuffId(),
                    BuffLevel = data.m_nLevel,
                    OverTime = DateTime.Now.AddSeconds(lastSeconds).ToBinary(),
                    Seconds = lastSeconds,
                    //OverTime = data.GetLastSeconds
                    Effect0 = data.mBuff.Effect[0],
                    Effect1 = data.mBuff.Effect[1],
                    Duration = data.mBuff.Duration,
                    Type = data.mBuff.Type,
                    DownLine = data.mBuff.DownLine,
                    IsView = data.mBuff.IsView,
                    Die = data.mBuff.Die,
                    SceneDisappear = data.mBuff.SceneDisappear,
                    DieDisappear = data.mBuff.DieDisappear,
                    HuchiId = data.mBuff.HuchiId,
                    TihuanId = data.mBuff.TihuanId,
                    PriorityId = data.mBuff.PriorityId,
                    BearMax = data.mBuff.BearMax,
                    LayerMax = data.mBuff.LayerMax,
                    effectid0 = data.mBuff.effectid[0],
                    effectid1 = data.mBuff.effectid[1],
                    effectid2 = data.mBuff.effectid[2],
                    effectid3 = data.mBuff.effectid[3],
                    effectpoint0 = data.mBuff.effectpoint[0],
                    effectpoint1 = data.mBuff.effectpoint[1],
                    effectpoint2 = data.mBuff.effectpoint[2],
                    effectpoint3 = data.mBuff.effectpoint[3],
                    effectparam00 = data.mBuff.effectparam[0, 0],
                    effectparam01 = data.mBuff.effectparam[0, 1],
                    effectparam02 = data.mBuff.effectparam[0, 2],
                    effectparam03 = data.mBuff.effectparam[0, 3],
                    effectparam04 = data.mBuff.effectparam[0, 4],
                    effectparam05 = data.mBuff.effectparam[0, 5],
                    effectparam10 = data.mBuff.effectparam[1, 0],
                    effectparam11 = data.mBuff.effectparam[1, 1],
                    effectparam12 = data.mBuff.effectparam[1, 2],
                    effectparam13 = data.mBuff.effectparam[1, 3],
                    effectparam14 = data.mBuff.effectparam[1, 4],
                    effectparam15 = data.mBuff.effectparam[1, 5],
                    effectparam20 = data.mBuff.effectparam[2, 0],
                    effectparam21 = data.mBuff.effectparam[2, 1],
                    effectparam22 = data.mBuff.effectparam[2, 2],
                    effectparam23 = data.mBuff.effectparam[2, 3],
                    effectparam24 = data.mBuff.effectparam[2, 4],
                    effectparam25 = data.mBuff.effectparam[2, 5],
                    effectparam30 = data.mBuff.effectparam[3, 0],
                    effectparam31 = data.mBuff.effectparam[3, 1],
                    effectparam32 = data.mBuff.effectparam[3, 2],
                    effectparam33 = data.mBuff.effectparam[3, 3],
                    effectparam34 = data.mBuff.effectparam[3, 4],
                    effectparam35 = data.mBuff.effectparam[3, 5],
                    EffectPoint0Param = data.mBuff.EffectPointParam[0],
                    EffectPoint1Param = data.mBuff.EffectPointParam[1],
                    EffectPoint2Param = data.mBuff.EffectPointParam[2],
                    EffectPoint3Param = data.mBuff.EffectPointParam[3]
                };
                _this.mDbData.Buffs.Add(temp);
            }
            return _this.mDbData;
        }

        public void Tick(ObjPlayer _this)
        {
            if (_this.Proxy != null && _this.Scene != null && DateTime.Now > _this.NextSyncTime)
            {
                Sync(_this);
                _this.NextSyncTime = DateTime.Now + ObjPlayer.MinSyncTimeSpan;
            }
        }

        public List<TimedTaskItem> GetTimedTasks(ObjPlayer _this)
        {
            return _this.mDbData.TimedTasks;
        }

        public void ApplyEvent(ObjPlayer _this, int eventId, string evt, int count)
        {
            if (evt.Contains(StaticVariable.ExpBattleFieldResetEventStr))
            {
                //如果古战场游戏时间该刷新了
                if (_this.Scene is ExpBattleField)
                {
                    //如果正在玩古战场，则发出事件通知刷新
                    EventDispatcher.Instance.DispatchEvent(new ExpBattleFieldPlayTimeResetEvent(_this.ObjId));
                }
            }
        }

        /// <summary>
        ///     是否是我的敌方
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public bool IsMyEnemy(ObjPlayer _this, ObjCharacter character)
        {
            return ObjCharacter.GetImpl().IsMyEnemy(_this, character);
        }

        #region 聊天相关

        public virtual void ChatSpeek(ObjPlayer _this, eChatChannel type, string content, List<ulong> toList)
        {
            switch (type)
            {
                case eChatChannel.System:
                    break;
                case eChatChannel.World:
                    break;
                case eChatChannel.City:
                    break;
                case eChatChannel.Scene:
                    {
                        _this.Scene.PushActionToAllPlayer(player => { toList.Add(player.ObjId); });
                    }
                    break;
                case eChatChannel.Guild:
                    break;
                case eChatChannel.Team:
                    break;
                case eChatChannel.Whisper:
                    break;
                case eChatChannel.Horn:
                    break;
                case eChatChannel.Count:
                    break;
                default:
                    break;
            }
        }

        #endregion

        public void ExitDungeon(ObjPlayer _this)
        {
            CoroutineFactory.NewCoroutine(ExitDungeon, _this).MoveNext();
        }

        public void SendForceStopMove(ObjPlayer _this)
        {
            if (!_this.mActive)
            {
                return;
            }
            if (null == _this.Zone)
            {
                return;
            }

            var Pos = new PositionData
            {
                Pos = new Vector2Int32
                {
                    x = Utility.MultiplyPrecision(_this.GetPosition().X),
                    y = Utility.MultiplyPrecision(_this.GetPosition().Y)
                },
                Dir = new Vector2Int32
                {
                    x = Utility.MultiplyPrecision(_this.GetDirection().X),
                    y = Utility.MultiplyPrecision(_this.GetDirection().Y)
                }
            };
            _this.Proxy.ForceStopMove(Pos);
        }

        public IEnumerator ExitDungeon(Coroutine coroutine, ObjPlayer _this)
        {
            var db = _this.GetData();

            if (_this.Scene != null)
            {
                if (_this.Scene.TableSceneData.FubenId != -1)
                {
                    var tbfuben = Table.GetFuben(_this.Scene.TableSceneData.FubenId);
                    if (tbfuben != null)
                    {
                        if (tbfuben.MainType == (int) eDungeonMainType.PhaseFuben)
                        {
                            _this.mDbData.FormerSceneId = (uint) _this.Scene.TableSceneData.ReturnSceneID;
                            _this.mDbData.FormerPostion.X = _this.GetPosition().X;
                            _this.mDbData.FormerPostion.Y = _this.GetPosition().Y;
                        }
                    }
                }
            }
            var co = CoroutineFactory.NewSubroutine(SceneServer.Instance.ServerControl.CreateAndEnterScene, coroutine,
                _this.ObjId,
                _this.ServerId,
                (int)db.FormerSceneId,
                0ul,
                eScnenChangeType.ExitDungeon,
                new SceneParam());
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        //数据库销毁
        public void OnDestroy(ObjPlayer _this)
        {
            _this.BuffList.OnDestroy();
            _this.Attr.RankSendChanges();
            StopKillerTrigger(_this);
            StopFriendsDirtyTrigger(_this);
        }

        //逻辑销毁
        public void Destroy(ObjPlayer _this)
        {
            _this.Skill.Reset();
            _this.StopAutoRelive();
            ObjCharacter.GetImpl().Destroy(_this);
        }

        public void ResetRelivePos(ObjPlayer _this)
        {
        }

        public void Relive(ObjPlayer _this, bool byItem = false)
        {
            if (_this == null)
            {
                PlayerLog.WriteLog(_this.ObjId, "ObjPlayer  Relive  Get NUll");
                return;
            }
            PlayerLog.WriteLog(_this.ObjId, "Relive {0} to pos:{1}", _this.ObjId, _this.GetPosition());
            _this.StopAutoRelive();
            if (_this.Scene != null && _this.Scene.TableSceneData.Type == (int) eSceneType.BossHome)
            {
                var bossHome = _this.Scene as BossHome;
                if (bossHome != null && byItem == false)
                {//boss之家随机复活点
                    bossHome.BeforPlayerRelive(_this);
                }
            }
            ObjCharacter.GetImpl().Relive(_this, byItem);
            if (_this.Scene != null)
            {
                _this.Scene.OnPlayerRelive(_this, byItem);

                if (0 != _this.Scene.TableSceneData.CanSummonMonster)
                {
                    _this.SummonBookMonster();
                }
            }
        }

        public void OnSaveData(ObjPlayer _this, DBCharacterScene data, DBCharacterSceneSimple simpleData)
        {
            PlayerLog.WriteLog(_this.ObjId, "----------Scene--------------------OnSaveData--------------------{0}",
                data.SaveCount++);
        }

        //添加obj到我的敌人列表
        public void AddEnemy(ObjPlayer _this, ulong objId)
        {
            if (objId == _this.ObjId)
            {
                return;
            }

            if (null == _this.Scene)
            {
                return;
            }

            var obj = _this.Scene.FindCharacter(objId);
            if (null == obj)
            {
                return;
            }

            if (obj.GetObjType() != ObjType.NPC)
            {
                return;
            }

            ObjCharacter.GetImpl().AddEnemy(_this, objId);
        }

        public void OnDamage(ObjPlayer _this, ObjCharacter enemy, int damage)
        {
            ObjCharacter.GetImpl().OnDamage(_this, enemy, damage);
            var casterPlayer = enemy as ObjPlayer;
            if (casterPlayer != null && _this.Scene != null && _this.Scene.TableSceneData != null && enemy != _this &&
                _this.Scene.TableSceneData.Type != 3 && casterPlayer.PkModel != (int)ePkModel.Peace)
            {
                casterPlayer.PkTime = 10000;
            }
            //防御装备耐久相关
            if (MyRandom.Random(10000) >= ObjPlayer.tbDefEquip)
            {
                return;
            }
            var defEquips = new Dictionary<int, ItemEquip2>();
            foreach (var itemEquip2 in _this.Equip)
            {
                if (itemEquip2.Key == 120)
                {
                    continue;
                }
                var equip = itemEquip2.Value;
                var now = equip.GetExdata(22);
                if (now <= 0)
                {
                    continue;
                }
                if (Table.GetEquip(equip.GetId()).DurableType != 2)
                {
                    continue;
                }
                defEquips.Add(itemEquip2.Key, itemEquip2.Value);
                //equip.SetDurable(now - 1);
                //DurableList.Add(itemEquip2.Key, -1);
            }
            if (defEquips.Count < 1)
            {
                return;
            }
            var durableList = new Dictionary<int, int>();
            var rrr = defEquips.Random();
            var newDurable = rrr.Value.GetExdata(22) - 1;
            rrr.Value.SetDurable(newDurable);
            durableList.Add(rrr.Key, -1);
            if (durableList.Count > 0)
            {
                EquipDurableDown(_this, durableList, newDurable <= 0);
            }
        }

        //被限制移动
        public void OnTrapped(ObjPlayer _this, ObjCharacter enemy)
        {
            ObjCharacter.GetImpl().OnTrapped(_this, enemy);
        }

        public void SendExDataChange(ObjPlayer _this, Dict_int_int_Data data)
        {
            CoroutineFactory.NewCoroutine(SendExDataChangeCoroutine, _this, data).MoveNext();
        }

        private IEnumerator SendExDataChangeCoroutine(Coroutine co,
            ObjPlayer _this,
            Dict_int_int_Data data)
        {
            var msg = SceneServer.Instance.LogicAgent.SSChangeExdata(_this.ObjId, data);
            yield return msg.SendAndWaitUntilDone(co);
        }
        public void OnExDataChanged(ObjPlayer _this, int idx, int val)
        {
            var scene = _this.Scene;
            if (null != scene)
            {
                scene.OnPlayerExDataChanged(_this, idx, val);
                _this.dicFlagTemp[idx] = val;
            }
        }

        public int GetSycExData(ObjPlayer _this, int idx)
        {
            int v = 0;
            if (_this.dicFlagTemp.TryGetValue(idx, out v))
            {
                return v;
            }
            return 0;
        }

        public void SaveBeforeScene(ObjPlayer _this)
        {
            var db = _this.GetData();
            var sceneId = (int)db.SceneId;
            var tbScene = Table.GetScene(sceneId);
            if (tbScene == null)
            {
                Logger.Error("tbScene == null db.SceneId read table {0}", sceneId);
                return;
            }
            if (tbScene.Type == (int)eSceneType.Normal
                || tbScene.Type == (int)eSceneType.City)
            {
                db.FormerSceneId = (uint)sceneId;
                db.FormerPostion = new DBPositionData
                {
                    X = _this.GetPosition().X,
                    Y = _this.GetPosition().Y,
                    DirX = _this.GetDirection().X,
                    DirY = _this.GetDirection().Y
                };
                _this.DbDirty = true;
            }
        }

        public ErrorCodes GmCommand(ObjPlayer _this, string command)
        {
            var err = AsyncReturnValue<ErrorCodes>.Create();
            CoroutineFactory.NewCoroutine(GmCommandCoroutine, _this, command, err).MoveNext();
            var result = err.Value;
            err.Dispose();
            return result;
        }

        #region 杀气值

        public void InitKillerTrigger(ObjPlayer _this)
        {
            if (_this.KillerValue > 0)
            {
                var oldValue = _this.KillerValue;
                var nextKillerTime = DateTime.FromBinary(_this.mDbData.NextKillerTime);
                while (true)
                {
                    if (nextKillerTime > DateTime.Now)
                    {

                        //KillerValue = oldValue;
                        _this.InitKillerValue(oldValue);
                        if (_this.KillerTrigger != null)
                        {
                            SceneServerControl.Timer.DeleteTrigger(_this.KillerTrigger);
                        }
                        _this.KillerTrigger = SceneServerControl.Timer.CreateTrigger(nextKillerTime,
                            () => { KillerTriggerTimeOver(_this); }, ObjPlayer.SubCdTime * 1000);
                        return;
                    }
                    oldValue = oldValue - ObjPlayer.SubKillValue;
                    if (oldValue < 0)
                    {
                        _this.KillerValue = 0;
                        return;
                    }
                    nextKillerTime = nextKillerTime.AddSeconds(ObjPlayer.SubCdTime);
                    _this.mDbData.NextKillerTime = nextKillerTime.ToBinary();
                }
            }
        }

        private void StartKillerTrigger(ObjPlayer _this)
        {
            if (_this.KillerTrigger == null)
            {
                var nextTime = DateTime.Now.AddSeconds(ObjPlayer.SubCdTime);
                _this.mDbData.NextKillerTime = nextTime.ToBinary();
                _this.KillerTrigger = SceneServerControl.Timer.CreateTrigger(nextTime,
                    () => { KillerTriggerTimeOver(_this); }, ObjPlayer.SubCdTime * 1000);
            }
        }

        private void StopKillerTrigger(ObjPlayer _this)
        {
            if (_this.KillerTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.KillerTrigger);
                _this.KillerTrigger = null;
            }
        }

        private void KillerTriggerTimeOver(ObjPlayer _this)
        {
            _this.KillerValue = _this.KillerValue - ObjPlayer.SubKillValue;
            _this.mDbData.NextKillerTime = DateTime.Now.AddSeconds(ObjPlayer.SubCdTime).ToBinary();
        }

        public void SetKillerValue(ObjPlayer _this, int value)
        {
            if (value > ObjPlayer.MaxKillValue)
            {
                value = ObjPlayer.MaxKillValue;
            }
            else if (value < 0)
            {
                value = 0;
            }
            var oldValue = _this.GetPKValue();
            _this.mDbData.KillerValue = value;
            _this.MarkDbDirty();
            var newValue = _this.GetPKValue();
            if (newValue != oldValue)
            {
                if (oldValue.BuffId > 0)
                {
                    _this.DeleteBuff(oldValue.BuffId, eCleanBuffType.TimeOver);
                }
                if (newValue.BuffId > 0)
                {
                    double lastTime = Math.Ceiling((double)_this.KillerValue / ObjPlayer.SubKillValue) * ObjPlayer.SubCdTime;
                    _this.AddBuff(newValue.BuffId, 1, _this, 0, eHitType.Hit, 1, lastTime);
                }
                if (value == 0)
                {
                    StopKillerTrigger(_this);
                }
                else
                {
                    StartKillerTrigger(_this);
                }
            }
            _this.OnPropertyChanged((uint)eSceneSyncId.SyncPkValue);
        }

        public void InitKillerValue(ObjPlayer _this, int value)
        {
            var oldvalue = _this.KillerValue;
            _this.mDbData.KillerValue = value;
            if (oldvalue == value && value > 0)
            {
                var newValue = _this.GetPKValue();
                if (newValue.BuffId > 0)
                {
                    var nextKillerTime = DateTime.FromBinary(_this.mDbData.NextKillerTime);
                    var seconds = nextKillerTime.GetDiffSeconds(DateTime.Now);
                    double lastTime = (Math.Ceiling((double)_this.KillerValue / ObjPlayer.SubKillValue) -1)* ObjPlayer.SubCdTime + seconds;
                    _this.AddBuff(newValue.BuffId, 1, _this, 0, eHitType.Hit, 1, lastTime);
                }
            }
        }

        public void SetPkTime(ObjPlayer _this, int value)
        {
            if (_this.mPkTime != value)
            {
                if (_this.KillerValue < 100)
                {
                    _this.OnPropertyChanged((uint)eSceneSyncId.SyncPkValue);
                }
                _this.mPkTime = value;
            }
            if (value == 0)
            {
                if (_this.mPkTimeTrigger != null)
                {
                    SceneServerControl.Timer.DeleteTrigger(_this.mPkTimeTrigger);
                    _this.mPkTimeTrigger = null;
                }
            }
            else
            {
                if (_this.mPkTimeTrigger == null)
                {
                    _this.mPkTimeTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(60),
                        () => { PkTimeTriggerTimeOver(_this); });
                }
                else
                {
                    SceneServerControl.Timer.ChangeTime(ref _this.mPkTimeTrigger, DateTime.Now.AddSeconds(60));
                }
            }
        }

        private void PkTimeTriggerTimeOver(ObjPlayer _this)
        {
            _this.PkTime = 0;
        }

        #endregion

        #region 离线挂机

        public void SetOutLineTime(ObjPlayer _this, DateTime value)
        {
            _this.mDbData.OutLineTime = value.ToBinary();
            _this.MarkDbDirty();
        }

        //最后一次时间修改时  (移动、用技能、切换场景)
        public void ChangeOutLineTime(ObjPlayer _this)
        {
            if (_this.GetLevel() < ObjPlayer.OutLineLevelMin)
            {
                _this.OutLineTime = DateTime.Now.AddMinutes(1);
                return;
            }
            if (_this.OutLineTime > DateTime.Now)
            {
                return;
            }
            var seconds = _this.OutLineTime.GetDiffSeconds(DateTime.Now);
            if (seconds >= 60)
            {
                OverOutLineTrigger(_this, (int)seconds / 60);
            }
            _this.OutLineTime = DateTime.Now.AddMinutes(1);
        }

        //按分钟给予离线经验
        private void OverOutLineTrigger(ObjPlayer _this, int minutes)
        {
            var tbLevel = Table.GetLevelData(_this.GetLevel());
            if (tbLevel == null)
            {
                return;
            }
            if (_this.OutLineExp >= tbLevel.LeaveExpBase)
            {
                return;
            }
            var addExp = _this.OutLineExp + 1.0f * tbLevel.DynamicExp * ObjPlayer.OutLineExpRef * minutes / 10000;
            if (addExp > tbLevel.LeaveExpBase)
            {
                _this.OutLineExp = tbLevel.LeaveExpBase;
            }
            else
            {
                _this.OutLineExp = (int)addExp;
            }
        }

        //获得当前时间可以获得的经验
        public int GetNowLeaveExp(ObjPlayer _this, ref int maxExp)
        {
            if (_this.GetLevel() < ObjPlayer.OutLineLevelMin)
            {
                return 0;
            }
            var seconds = _this.OutLineTime.GetDiffSeconds(DateTime.Now);
            var minutes = (int)seconds / 60;
            var tbLevel = Table.GetLevelData(_this.GetLevel());
            if (tbLevel == null)
            {
                return _this.OutLineExp;
            }
            maxExp = tbLevel.LeaveExpBase;
            if (seconds >= 60)
            {
                if (_this.OutLineExp >= maxExp)
                {
                    return maxExp;
                }
                var addExp = _this.OutLineExp + 1.0f * tbLevel.DynamicExp * ObjPlayer.OutLineExpRef * minutes / 10000;
                if (addExp > maxExp)
                {
                    return maxExp;
                }
                return (int)addExp;
            }
            return _this.OutLineExp;
        }

        #endregion

        #region 维护好友的广播关系

        public void SetFriendsDirty(ObjPlayer _this, bool value)
        {
            if (value)
            {
                StartFriendsDirtyTrigger(_this);
            }
            _this.mFriendsDirty = value;
        }

        private void StartFriendsDirtyTrigger(ObjPlayer _this)
        {
            if (_this.FriendsDirtyTrigger == null)
            {
                _this.FriendsDirtyTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(3),
                    () => { _this.FriendsDirtyTriggerTimeOver(); });
            }
        }

        public void MergeSceneByTeam(ObjPlayer _this)
        {
        }

        public IEnumerator SendFriendsDataCoroutine(Coroutine co, ObjPlayer _this, int isOnline)
        {
            if (_this.BroadCastFriends.Count < 1)
            {
                yield break;
            }
            var simple2 = _this.GetSimpleData();
            var simple = new CharacterSimpleData
            {
                Id = simple2.Id,
                TypeId = simple2.TypeId,
                Name = simple2.Name,
                SceneId = simple2.SceneId,
                FightPoint = simple2.FightPoint,
                Level = simple2.Level,
                Ladder = simple2.Ladder,
                ServerId = simple2.ServerId,
                Online = isOnline,
                Vip = simple2.Vip,
                StarNum = simple2.StarNum
            };
            foreach (var broadCastFriend in _this.BroadCastFriends.Keys.ToArray())
            {
                var msgChgScene = SceneServer.Instance.LogicAgent.SSSendSimpleData(broadCastFriend,
                    broadCastFriend, simple);
                yield return msgChgScene.SendAndWaitUntilDone(co);
            }
        }

        public void FriendsDirtyTriggerTimeOver(ObjPlayer _this)
        {
            _this.FriendsDirty = false;
            StopFriendsDirtyTrigger(_this);
            CoroutineFactory.NewCoroutine(SendFriendsDataCoroutine, _this, 1).MoveNext();
        }

        public void FriendsLostTriggerTimeOver(ObjPlayer _this)
        {
            _this.FriendsDirty = false;
            StopFriendsDirtyTrigger(_this);
            CoroutineFactory.NewCoroutine(SendFriendsDataCoroutine, _this, 0).MoveNext();
        }

        private void StopFriendsDirtyTrigger(ObjPlayer _this)
        {
            if (_this.FriendsDirtyTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.FriendsDirtyTrigger);
                _this.FriendsDirtyTrigger = null;
            }
        }

        private Dictionary<ulong, bool> GetFrienList(ObjPlayer _this, int type)
        {
            switch (type)
            {
                case 1:
                    return _this.BeHaveFriends;
                case 2:
                    return _this.BeHaveEnemys;
                case 3:
                    return _this.BeHaveShield;
            }
            return null;
        }

        public void PushFriend(ObjPlayer _this, int type, ulong id)
        {
            var list = GetFrienList(_this, type);
            if (list == null)
            {
                return;
            }
            list[id] = true;
            _this.BroadCastFriends.modifyValue(id, 1);
        }

        public void RemoveFriend(ObjPlayer _this, int type, ulong id)
        {
            var list = GetFrienList(_this, type);
            if (list == null)
            {
                return;
            }
            if (list.Remove(id))
            {
                var newCount = _this.BroadCastFriends.modifyValue(id, -1);
                if (newCount < 1)
                {
                    _this.BroadCastFriends.Remove(id);
                }
            }
        }

        public void CleanFreind(ObjPlayer _this)
        {
            _this.BeHaveFriends.Clear();
            _this.BeHaveEnemys.Clear();
            _this.BeHaveShield.Clear();
            _this.BroadCastFriends.Clear();
        }

        #endregion

        #region 数据同步

        //同步天赋
        public void ApplyTalent(ObjPlayer _this, Dictionary<int, int> Talents)
        {
            _this.Skill.ResetAllTalent();
            foreach (var i in Talents)
            {
                _this.Skill.AddTalent(i.Key, i.Value);
            }
            //     _this.Attr.SetFightPointFlag();
        }

        //同步技能
        public void ApplySkill(ObjPlayer _this, Dictionary<int, int> skills)
        {
            //-------temp------
            if (_this.Skill.mData.Count > 0)
            {
                Logger.Warn("ApplySkill SKillCount={0}", _this.Skill.mData.Count);
            }
            _this.Skill.mData.Clear();
            //-------temp------//
            foreach (var i in skills)
            {
                if (i.Key == -5)
                {
                    _this.Attr.LifeCardFlag = i.Value;
                }
                else if (i.Key == -4)
                {
                    _this.Attr.Vip = i.Value;
                }
                else if (i.Key == -3)
                {
                    _this.Attr.Honor = i.Value;
                }
                else if (i.Key == -2)
                {
                    _this.Attr.Ladder = i.Value;
                }
                else if (i.Key == -1)
                {
                    _this.Attr.SetDataValue(eAttributeType.Level, i.Value);
                }
                else
                {
                    _this.Skill.AddSkill(i.Key, i.Value, eAddskillType.ApplySkill);
                }
            }
        }

        //同步装备
        public void ApplyEquip(ObjPlayer _this, BagBaseData bag)
        {
            _this.Equip.Clear();
            foreach (var item in bag.Items)
            {
                var ib = new ItemEquip2();
                ib.SetId(item.ItemId);
                ib.SetCount(item.Count);
                ib.CopyFrom(item.Exdata);
                ib.CheckTrialEquip();
                _this.Equip[item.Index] = ib;

                _this.AddEquipBuff(ib);
            }
            //for (int i = 7; i != 20; ++i)
            //{
            //    if (mDbData.LogicDB.Bag.Bags.Count <= i)
            //    {
            //        break;
            //    }
            //    ItemBaseData item = mDbData.LogicDB.Bag.Bags[i].Items[0];
            //    //mDbData.LogicDB.Bag.Bags[i].Items.TryGetValue(0, out item);
            //    if (item == null) continue;
            //    ItemBase ib = new ItemBase();
            //    ib.SetId(item.Index);
            //    ib.SetCount(item.Count);
            //    ib.CopyFrom(item.Exdata);
            //    ItemEquip ib1 = (ItemEquip)ib;
            //    Equip[i] = ib1;
            //}

            if (!_this.Attr.PlayerInit)
            {
                _this.Attr.EquipRefresh();
            }
        }

        //同步附加属性
        public void ApplyBookAttr(ObjPlayer _this, Dictionary<int, int> attrList, Dictionary<int, int> monsterAttrs)
        {
            _this.Attr.BookRefresh(attrList, monsterAttrs);
        }

        //同步称号
        public void ApplyTitles(ObjPlayer _this, List<int> titles, int type)
        {
            if (titles.Count > 0)
            {
                _this.Attr.TitleRefresh(titles, type);
            }
        }

        public void ApplyElf(ObjPlayer _this, ElfData data)
        {
            _this.ElfChange(new List<int>(), data.Buff, data.FightPoint);
        }

        public void ApplyMount(ObjPlayer _this, MountMsgData data)
        {
            foreach (var buf in data.Buff)
            {
                _this.AddBuff(buf.Key, buf.Value, _this, 1);
            }
        }

        #endregion

        #region  装备耐久度变化

        public void EquipDurableDown(ObjPlayer _this, Dictionary<int, int> equipList, bool refreshAttr)
        {
            if (refreshAttr)
            {
                _this.Attr.EquipRefresh();
            }
            CoroutineFactory.NewCoroutine(DurableDownToLogic, _this, equipList).MoveNext();
        }

        public IEnumerator DurableDownToLogic(Coroutine coroutine, ObjPlayer _this, Dictionary<int, int> equipList)
        {
            var temp = new Dict_int_int_Data();
            temp.Data.AddRange(equipList);
            var result = SceneServer.Instance.LogicAgent.DurableDown(_this.ObjId, temp);
            yield return result.SendAndWaitUntilDone(coroutine);
        }

        #endregion

        #region 队伍相关

        public ulong GetTeamId(ObjPlayer _this)
        {
            return _this.mDbData.TeamId;
        }

        public void SetTeamId(ObjPlayer _this, ulong teamId, int state)
        {
            ObjCharacter.GetImpl().SetTeamId(_this, teamId, state);
            _this.mDbData.TeamId = teamId;
            _this.teamState = state;
        }

        #endregion

        #region 战盟相关

        public int GetAllianceId(ObjPlayer _this)
        {
            return _this.mDbData.AllianceId;
        }

        public void SetAllianceInfo(ObjPlayer _this, int allianceId, int ladder, string name)
        {
            _this.mDbData.AllianceId = allianceId;
            _this.Ladder = (eAllianceLadder)ladder;
            if (name == null)
            {
                //不能序列化为空
                name = "";
            }
            if (_this.AllianceName != name)
            {
                _this.AllianceName = name;
                _this.OnPropertyChanged((uint)(eSceneSyncId.SyncAllianceName));
            }
        }

        #endregion

        public void SummonBookMonster(ObjPlayer _this)
        {
            _this.SummonBookMonster();
        }

        public void SetBookMonsterId(ObjPlayer _this, int id)
        {
            _this.SetBookMonsterId(id);
        }

        public int GetBookMonsterId(ObjPlayer _this)
        {
            return _this.GetBookMonsterId();
        }

        public IEnumerator OnPlayerEnterGetTeamData(Coroutine co, ObjPlayer _this)
        {
            var sceneId = _this.Scene.Guid;
            var teamid = _this.GetTeamId();
            var player = _this as ObjPlayer;
            var characteId = _this.ObjId;

            var msg = SceneServer.Instance.TeamAgent.SSGetTeamCharacters(characteId, characteId);
            yield return msg.SendAndWaitUntilDone(co);

            if (msg.State != MessageState.Timeout)
            {
                if (msg.ErrorCode == (int)ErrorCodes.OK)
                {
                    if (null != msg.Response && null != msg.Response.Items)
                    {
                        foreach (var item in msg.Response.Items)
                        {
                            SceneServerService.SceneCharacterProxy proxy;
                            if (SceneServer.Instance.ServerControl.Proxys.TryGetValue(item, out proxy))
                            {
                                proxy.NotifyTeamMemberScene(item, characteId, sceneId);
                            }
                        }
                    }


                    foreach (var ite in msg.Response.Items)
                    {
                        var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(ite);
                        if (character != null && character.Scene != null)
                        {
                            try
                            {
                                SceneServerService.SceneCharacterProxy proxy;
                                if (SceneServer.Instance.ServerControl.Proxys.TryGetValue(characteId, out proxy))
                                {
                                    proxy.NotifyTeamMemberScene(characteId, character.ObjId, character.Scene.Guid);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);
                            }
                        }
                    }
                }


            }
            yield break;
        }

        public float GetAdditionExp(ObjPlayer _this)
        {
            if (_this == null || _this.Scene == null)
                return 1.0f;
            if (_this.Scene.OwnerAllianceId > 0 && _this.Scene.OwnerAllianceId == _this.GetAllianceId())
            {
                return _this.Scene.AdditionExp;
            }
            return 1.0f;
        }

        public float GetAdditionLode(ObjPlayer _this)
        {
            if (_this == null || _this.Scene == null)
                return 1.0f;
            if (_this.Scene.OwnerAllianceId > 0 && _this.Scene.OwnerAllianceId == _this.GetAllianceId())
            {
                return _this.Scene.AdditionLode;
            }
            return 1.0f;
        }
    }

    public class ObjPlayer : ObjCharacter, ICharacterControllerBase<DBCharacterScene, DBCharacterSceneSimple>,
                             ObjCharacter.INotifyPropertyChanged
    {
       
        public static int AddKillValue = Table.GetServerConfig(373).ToInt();
        public static int AddKillValueNotMain = Table.GetServerConfig(374).ToInt();
        public static int MaxKillValue = Table.GetServerConfig(370).ToInt();
        private static IObjPlayer mImpl;
        public static TimeSpan MinSyncTimeSpan = TimeSpan.FromSeconds(1);
        public const int NormalSpeedModify = 10000;
        public static int OutLineDiamond = Table.GetServerConfig(583).ToInt(); //离线经验钻石比例
        public static int OutLineExpRef = Table.GetServerConfig(580).ToInt(); //离线经验修正比例
        public static int OutLineLevelMin = Table.GetServerConfig(104).ToInt(); //离线经验最大值
        public static int OutLineMoney = Table.GetServerConfig(582).ToInt(); //离线经验金钱比例
        public static int RunSpeedModify = Table.GetServerConfig(3).ToInt();
        public static int SubCdTime = Table.GetServerConfig(371).ToInt();
        public static int SubKillValue = Table.GetServerConfig(372).ToInt();
        public static readonly int tbDefEquip = Table.GetServerConfig(360).ToInt();
        public Dictionary<int, int> dicFlagTemp = new Dictionary<int, int>();
        static ObjPlayer()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof(ObjPlayer), typeof(ObjPlayerDefaultImpl),
                o => { mImpl = (IObjPlayer)o; });
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
        }

        public string AllianceName = "";
        public Dictionary<int, int> ItemCount = new Dictionary<int, int>();
        //切换场景状态
        public DateTime LastChangeSceneTime = DateTime.MinValue;
        public bool lastNomoveFlag = false;
        public DateTime lastNomoveTime = DateTime.Now;
        public Vector2 mDirtyPosition = Vector2.Zero;

        public Dictionary<INotifyPropertyChanged, Dictionary<uint, SourceBinding>> mMyDirtyFlag =
            new Dictionary<INotifyPropertyChanged, Dictionary<uint, SourceBinding>>();

        public SceneSyncData mMySyncData = new SceneSyncData();
        public bool mMySyncDirtyFlag = true;
        public DateTime NextSyncTime = DateTime.Now;

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public uint CurrentSceneTypeId
        {
            get { return mDbData.SceneId; }
        }

        public int PkModel
        {
            get { return mDbData.PKModel; }
            set { mImpl.SetPkModel(this, value); }
        }

        public override int ServerId
        {
            get { return mDbData.ServerId; }
            set { mDbData.ServerId = value; }
        }

        public int VipLevel
        {
            get
            {
                if (ItemCount.ContainsKey((int)eResourcesType.VipLevel))
                {
                    return ItemCount[(int)eResourcesType.VipLevel];
                }
                return 0;
            }
        }

        protected int mSummonBookMonsterId = -1;

        public override void SetName(string name)
        {
            base.SetName(name);
            OnPropertyChanged((uint)eSceneSyncId.SyncPlayerName);
        }

        //添加obj到我的敌人列表
        public override void AddEnemy(ulong objId)
        {
            mImpl.AddEnemy(this, objId);
        }

        public void SendForceStopMove()
        {
            mImpl.SendForceStopMove(this);
        }

        public void ApplySimpleData(DBCharacterSceneSimple simpleData)
        {
            mImpl.ApplySimpleData(this, simpleData);
        }

        public void AutoRelive()
        {
            mImpl.AutoRelive(this);
        }

        //开始切换场景
        public void BeginChangeScene()
        {
            LastChangeSceneTime = DateTime.Now;
        }

        #region 聊天相关

        public virtual void ChatSpeek(eChatChannel type, string content, List<ulong> toList)
        {
            mImpl.ChatSpeek(this, type, content, toList);
        }

        #endregion

        public override void Destroy()
        {
            mImpl.Destroy(this);
        }

        //输出成一个objdata用于客户端创建
        public override ObjData DumpObjData(ReasonType reason)
        {
            return mImpl.DumpObjData(this, reason);
        }

        //结束切换场景
        public void EndChangeScene()
        {
            LastChangeSceneTime = DateTime.MinValue;
        }

        public void ThroughPos(Vector2 pos)
        {
            mImpl.ThroughPos(this, pos);
        }
        public void EnterSceneOver()
        {
            mImpl.EnterSceneOver(this);
        }

        public void ExitDungeon()
        {
            mImpl.ExitDungeon(this);
        }

        public IEnumerator ExitDungeon(Coroutine coroutine)
        {
            return mImpl.ExitDungeon(coroutine, this);
        }

        public override Vector2 GetDirection()
        {
            return mImpl.GetDirection(this);
        }

        public SceneSyncData GetMySyncData()
        {
            return mImpl.GetMySyncData(this);
        }

        //public List<int> c = new List<int>();
        //Obj类型
        public override ObjType GetObjType()
        {
            return ObjType.PLAYER;
        }

        public PKValueRecord GetPKValue()
        {
            return mImpl.GetPKValue(this);
        }

        public override Vector2 GetPosition()
        {
            return mImpl.GetPosition(this);
        }

        public ErrorCodes GmCommand(string command)
        {
            return mImpl.GmCommand(this, command);
        }

        public override void Init(ulong characterId, int dataId, int level)
        {
            mImpl.Init(this, characterId, dataId, level);
        }

        public virtual void InitDB()
        {
            mImpl.InitDB(this);
        }

        public override bool InitEquip(int level)
        {
            return mImpl.InitEquip(this, level);
        }

        public override bool InitSkill(int level)
        {
            return mImpl.InitSkill(this, level);
        }

        public override int InitTableData(int level)
        {
            return mImpl.InitTableData(this, level);
        }

        public float GetAdditionLode()
        {
            return mImpl.GetAdditionLode(this);
        }

        public float GetAdditionExp()
        {
            return mImpl.GetAdditionExp(this);
        }
        //是否是正在切换场景中
        public bool IsChangingScene()
        {
            if (DateTime.MinValue.Equals(LastChangeSceneTime))
            {
                return false;
            }

            //这时间是个安全时间，不可能这么长时间都没切完场景
            if ((DateTime.Now - LastChangeSceneTime).TotalSeconds > 5)
            {
                LastChangeSceneTime = DateTime.MinValue;
                return false;
            }

            return true;
        }

        /// <summary>
        ///     是否是我的敌方
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public override bool IsMyEnemy(ObjCharacter character)
        {
            return mImpl.IsMyEnemy(this, character);
        }

        /// <summary>
        ///     移动角色（寻路是异步的）
        /// </summary>
        /// <param name="pos">目标位置</param>
        /// <param name="offset">目标位置偏移</param>
        /// <param name="searchPath">是否需要寻路，false表示直线移动到目的地</param>
        /// <param name="pushLastPos">是否需要精确移动到目标点，因为寻路是以格子中心点为坐标的，最后可能会产生半格的误差</param>
        /// <param name="callback">寻路结果callback</param>
        /// <returns>能走返回true，不能走返回false</returns>
        public override MoveResult MoveTo(Vector2 pos,
                                          float offset = 0.05f,
                                          bool searchPath = true,
                                          bool pushLastPos = false,
                                          Action<List<Vector2>> callback = null)
        {
            return mImpl.MoveTo(this, pos, offset, searchPath, pushLastPos, callback);
        }

        public void MoveToTarget(List<Vector2> targetList, float offset)
        {
            mImpl.MoveToTarget(this, targetList, offset);
        }

        public void MyAddSyncData(uint id, INotifyPropertyChanged holder, Func<int> getter)
        {
            MyAddSyncData(id, holder, () =>
            {
                var buffer = new byte[4];
                SerializerUtility.WriteInt(buffer, getter());
                return buffer;
            });
        }

        public void MyAddSyncData(uint id, INotifyPropertyChanged holder, Func<string> getter)
        {
            MyAddSyncData(id, holder, () =>
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, new TString { Data = getter() });
                    return ms.ToArray();
                }
            });
        }

        public void MyAddSyncData(uint id, INotifyPropertyChanged holder, Func<byte[]> getter)
        {
            Dictionary<uint, SourceBinding> dict;
            if (!mMyDirtyFlag.TryGetValue(holder, out dict))
            {
                holder.PropertyChanged += PropertyChangedHandler;
                dict = new Dictionary<uint, SourceBinding>();
                mMyDirtyFlag[holder] = dict;
            }

            SourceBinding binding;
            if (!dict.TryGetValue(id, out binding))
            {
                dict[id] = new SourceBinding
                {
                    Getter = getter
                };
            }
        }

        public static IEnumerator NotifyLogicAddFriendCoroutine(Coroutine co, ulong toC, ulong addC)
        {
            return mImpl.NotifyLogicAddFriendCoroutine(co, toC, addC);
        }

        public static IEnumerator SendPlayerEnterSceneOverToLogic(Coroutine co, ObjPlayer _this)
        {
            return mImpl.SendPlayerEnterSceneOverToLogic(co, _this);
        }

        #region  NPC服务

        public ErrorCodes NpcService(ulong npcId, int serviceId)
        {
            return mImpl.NpcService(this, npcId, serviceId);
        }

        #endregion

        public override void OnDamage(ObjCharacter enemy, int damage)
        {
            mImpl.OnDamage(this, enemy, damage);
        }

        public override void OnTrapped(ObjCharacter enemy)
        {
            mImpl.OnTrapped(this, enemy);
        }

        public void OnExDataChanged(int idx, int val)
        {
            mImpl.OnExDataChanged(this, idx, val);
        }

        public int GetSycExData(int idx)
        {
            return mImpl.GetSycExData(this, idx);
        }
        public void SendExDataChange(Dict_int_int_Data data)
        {
            mImpl.SendExDataChange(this, data);
        }

        public override void OnDie(ulong characterId, int viewTime, int damage = 0)
        {
            mImpl.OnDie(this, characterId, viewTime, damage);
        }

        public override void OnEnterScene()
        {
            mImpl.OnEnterScene(this);
        }

        public override void OnLeaveScene()
        {
            mImpl.OnLeaveScene(this);
        }

        public bool OnMySyncRequested(ulong characterId, uint syncId)
        {
            return mImpl.OnMySyncRequested(this, characterId, syncId);
        }

        public virtual void OnPropertyChanged(uint propertyId)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyId));
            }
        }

        public override void ProcessPositionChanged()
        {
            mImpl.ProcessPositionChanged(this);
        }

        public new void PropertyChangedHandler(INotifyPropertyChanged obj, PropertyChangedEventArgs args)
        {
            Dictionary<uint, SourceBinding> dict;
            SourceBinding binding;
            if (mMyDirtyFlag.TryGetValue(obj, out dict))
            {
                if (dict.TryGetValue(args.Id, out binding))
                {
                    binding.Dirty = true;
                    mMySyncDirtyFlag = true;
                }
            }
        }

        public void RegisterMySyncData()
        {
            mImpl.RegisterMySyncData(this);
        }

        public override void Relive(bool byItem = false)
        {
            mImpl.Relive(this, byItem);
        }

        private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v.tableName == "ServerConfig")
            {
                MaxKillValue = Table.GetServerConfig(370).ToInt();
                SubCdTime = Table.GetServerConfig(371).ToInt();
                SubKillValue = Table.GetServerConfig(372).ToInt();
                AddKillValue = Table.GetServerConfig(373).ToInt();
                AddKillValueNotMain = Table.GetServerConfig(374).ToInt();

                OutLineExpRef = Table.GetServerConfig(580).ToInt(); //离线经验修正比例
                OutLineLevelMin = Table.GetServerConfig(104).ToInt(); //离线经验最大值
                OutLineMoney = Table.GetServerConfig(582).ToInt(); //离线经验金钱比例
                OutLineDiamond = Table.GetServerConfig(583).ToInt(); //离线经验钻石比例
            }
        }

        public void RemoveAllMySyncData()
        {
            mImpl.RemoveAllMySyncData(this);
        }

        public override void Reset()
        {
            mImpl.Reset(this);
        }

        //开始切换场景
        public void ResetChangeSceneTime()
        {
            LastChangeSceneTime = DateTime.MinValue;
        }

        public void ResetRelivePos()
        {
            mImpl.ResetRelivePos(this);
        }

        public void SaveBeforeScene()
        {
            mImpl.SaveBeforeScene(this);
        }

        public override void SetDirection(Vector2 dir)
        {
            mImpl.SetDirection(this, dir);
        }

        public override void SetDirection(float x, float y)
        {
            mImpl.SetDirection(this, x, y);
        }

        #region Player属性

        public void SetItemCount(int itemId, int count)
        {
            mImpl.SetItemCount(this, itemId, count);
        }

        #endregion

        public override void SetPosition(Vector2 p)
        {
            mImpl.SetPosition(this, p);
        }

        public override void SetPosition(float x, float y)
        {
            mImpl.SetPosition(this, x, y);
        }

        public void StartAutoRelive(int seconds)
        {
            mImpl.StartAutoRelive(this, seconds);
        }

        public void StopAutoRelive()
        {
            mImpl.StopAutoRelive(this);
        }

#if DEBUG //debug调试用，如果需要修改Tick逻辑，请把宏去掉

        public override void Tick(float delta)
        {
            mImpl.Tick(this, delta);
        }
#endif

        public override string ToString()
        {
            return ObjId.ToString();
        }

        public void UpdataSceneInfoData(uint sceneId, ulong sceneGuid = ulong.MaxValue)
        {
            mImpl.UpdataSceneInfoData(this, sceneId, sceneGuid);
        }

        public eAreaState UpdateAreaState(Scene scene, bool set)
        {
            return mImpl.UpdateAreaState(this, scene, set);
        }

        public override void UpdateDbAttribute(eAttributeType type)
        {
            mImpl.UpdateDbAttribute(this, type);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        //AsyncReturnValue<ErrorCodes> error = AsyncReturnValue<ErrorCodes>.Create();
        ////var gmCo = CoroutineFactory.NewSubroutine(GameMaster.GmGoto, coroutine, Character, targetSceneId, x, y, error);
        //var gmCo = CoroutineFactory.NewSubroutine(GameMaster.NotifyCreateChangeSceneCoroutine, coroutine, Character, targetSceneId, x, y, error);

        //if (gmCo.MoveNext())
        //{
        //    yield return gmCo;
        //}
        public bool InitByDb(ulong characterId, DBCharacterScene dbData)
        {
            return mImpl.InitByDb(this, characterId, dbData);
        }

        public DBCharacterScene InitByBase(ulong characterId, object[] args)
        {
            return mImpl.InitByBase(this, characterId, args);
        }

        public DBCharacterSceneSimple GetSimpleData()
        {
            return mImpl.GetSimpleData(this);
        }

        public DBCharacterScene GetData()
        {
            return mImpl.GetData(this);
        }

        public void Tick()
        {
            mImpl.Tick(this);
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

        public void OnSaveData(DBCharacterScene data, DBCharacterSceneSimple simpleData)
        {
            mImpl.OnSaveData(this, data, simpleData);
        }

        public bool Online
        {
            get { return Proxy != null && Proxy.Connected; }
        }

        public CharacterState State { get; set; }

        public void SummonBookMonster()
        {
            ClearRetinue();
            if (0 < mSummonBookMonsterId)
            {
                var tb = Table.GetHandBook(mSummonBookMonsterId);
                if (null != tb)
                {
                    CreateRetinue(tb.NpcId, 1, GetPosition(), GetDirection(), GetCamp());
                }
            }

        }

        public void SetBookMonsterId(int id)
        {
            mSummonBookMonsterId = id;
        }

        public int GetBookMonsterId()
        {
            return mSummonBookMonsterId;
        }
        #region 杀气值

        public Trigger KillerTrigger;

        public void InitKillerTrigger()
        {
            mImpl.InitKillerTrigger(this);
        }

        public int KillerValue
        {
            get { return mDbData.KillerValue; }
            set { mImpl.SetKillerValue(this, value); }
        }

        public void InitKillerValue(int value)
        {
            mImpl.InitKillerValue(this, value);
        }

        //主动PK
        public int mPkTime;
        public Trigger mPkTimeTrigger;

        public int PkTime
        {
            get { return mPkTime; }
            set { mImpl.SetPkTime(this, value); }
        }

        #endregion

        #region 离线挂机

        public int OutLineExp
        {
            get { return mDbData.OutLineExp; }
            set { mDbData.OutLineExp = value; }
        }

        public DateTime OutLineTime
        {
            get { return DateTime.FromBinary(mDbData.OutLineTime); }
            set { mImpl.SetOutLineTime(this, value); }
        }

        //最后一次时间修改时  (移动、用技能、切换场景)
        public void ChangeOutLineTime()
        {
            mImpl.ChangeOutLineTime(this);
        }

        //获得当前时间可以获得的经验
        public int GetNowLeaveExp(ref int maxExp)
        {
            return mImpl.GetNowLeaveExp(this, ref maxExp);
        }

        #endregion

        #region 维护好友的广播关系

        public bool mFriendsDirty;
        public Trigger FriendsDirtyTrigger;
        public Dictionary<ulong, int> BroadCastFriends = new Dictionary<ulong, int>();
        public Dictionary<ulong, bool> BeHaveFriends = new Dictionary<ulong, bool>();
        public Dictionary<ulong, bool> BeHaveEnemys = new Dictionary<ulong, bool>();
        public Dictionary<ulong, bool> BeHaveShield = new Dictionary<ulong, bool>();


        public bool FriendsDirty
        {
            get { return mFriendsDirty; }
            set { mImpl.SetFriendsDirty(this, value); }
        }

        public void MergeSceneByTeam()
        {
            mImpl.MergeSceneByTeam(this);
        }

        public IEnumerator SendFriendsDataCoroutine(Coroutine co, int isOnline)
        {
            return mImpl.SendFriendsDataCoroutine(co, this, isOnline);
        }

        public void FriendsDirtyTriggerTimeOver()
        {
            mImpl.FriendsDirtyTriggerTimeOver(this);
        }

        public void FriendsLostTriggerTimeOver()
        {
            mImpl.FriendsLostTriggerTimeOver(this);
        }

        public void PushFriend(int type, ulong id)
        {
            mImpl.PushFriend(this, type, id);
        }

        public void RemoveFriend(int type, ulong id)
        {
            mImpl.RemoveFriend(this, type, id);
        }

        public void CleanFreind()
        {
            mImpl.CleanFreind(this);
        }

        #endregion

        #region Player属性

        //Client
        public SceneProxy Proxy { get; set; }

        //public string Name { get; public set; }
        public DBCharacterScene mDbData { get; set; }
        public Trigger mAutoRelive { get; set; }
        public int SelectReliveType { get; set; }

        #endregion

        #region 数据同步

        //同步天赋
        public void ApplyTalent(Dictionary<int, int> Talents)
        {
            mImpl.ApplyTalent(this, Talents);
        }

        //同步技能
        public void ApplySkill(Dictionary<int, int> skills)
        {
            mImpl.ApplySkill(this, skills);
        }

        //同步装备
        public void ApplyEquip(BagBaseData bag)
        {
            mImpl.ApplyEquip(this, bag);
        }


        //同步精灵
        public void ApplyElf(ElfData data)
        {
            mImpl.ApplyElf(this, data);
        }

        public void ApplyMountData(MountMsgData data)
        {
            mImpl.ApplyMount(this, data);
        }


        //同步附加属性
        public void ApplyBookAttr(Dictionary<int, int> attrList, Dictionary<int, int> monsterAttrs)
        {
            mImpl.ApplyBookAttr(this, attrList, monsterAttrs);
        }

        //同步称号
        public void ApplyTitles(List<int> titles, int type)
        {
            mImpl.ApplyTitles(this, titles, type);
        }

        public void SetMountId(int MountId)
        {
            mImpl.SetMountId(this, MountId);
        }

        public int GetMountId()
        {
            return mImpl.GetMountId(this);
        }

        public override bool IsRiding()
        {
            return mImpl.IsRiding(this);
        }

        public int GetRole()
        {
            return mImpl.GetRole(this);
        }
        #endregion

        #region  装备耐久度变化

        public void EquipDurableDown(Dictionary<int, int> equipList, bool refreshAttr)
        {
            mImpl.EquipDurableDown(this, equipList, refreshAttr);
        }

        public IEnumerator DurableDownToLogic(Coroutine coroutine, Dictionary<int, int> equipList)
        {
            return mImpl.DurableDownToLogic(coroutine, this, equipList);
        }

        #endregion

        #region 队伍相关

        public int teamState;

        public override ulong GetTeamId()
        {
            return mImpl.GetTeamId(this);
        }

        public override void SetTeamId(ulong teamId, int state)
        {
            mImpl.SetTeamId(this, teamId, state);
        }

        #endregion

        #region 战盟相关

        public eAllianceLadder Ladder;

        public int GetAllianceId()
        {
            return mImpl.GetAllianceId(this);
        }

        public void SetAllianceInfo(int allianceId, int ladder, string name)
        {
            mImpl.SetAllianceInfo(this, allianceId, ladder, name);
        }

        #endregion
    }
}