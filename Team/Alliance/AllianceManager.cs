#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Database;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using NLog;
using Shared;
using TeamServerService;

#endregion

namespace Team
{
    public enum TeamAllianceState
    {
        NewCreate = 0, //刚创建，还未确认
        Already = 1, //创建已确认
        WillDisband = 2 //准备解散
    }

    public enum AllianceMissionState
    {
        Wait = 0, //等待刷新
        Normal = 1 //未完成
    }

    public enum AllianceDepotOperationType
    {
        Donate = 0, //捐赠
        Takeout = 1, //取出
        ClearUp = 2,  //清理
        Arrange = 3  //整理
    }

    public class InviteData
    {
        public List<ulong> InviteFrom = new List<ulong>();
        public Trigger trigger;
    }

    public interface IAlliance
    {
        DBAllianceOne AddCharacter(Alliance _this, ulong guid, int ladder = 0);
        void AddMission(Alliance _this);
        void AddMoney(Alliance _this, int value);
        ErrorCodes AgreeApply(Alliance _this, ulong characterGuid);
        ErrorCodes AgreeInvite(Alliance _this, ulong characterGuid);
        ErrorCodes AllianceAgreeApplyList(Alliance _this, DBAllianceOne from, List<ulong> characterGuids);
        ErrorCodes ClearAllianceApplyList(Alliance _this, DBAllianceOne from, List<ulong> characterGuids);
        ErrorCodes AllianceRefuseApplyList(Alliance _this, DBAllianceOne from, List<ulong> characterGuids);
        ErrorCodes ApplyCancel(Alliance _this, ulong guid);
        ErrorCodes ApplyJoin(Alliance _this, ulong guid);
        ErrorCodes ChangeAllianceAutoJoin(Alliance _this, DBAllianceOne guid, int value);
        ErrorCodes ChangeJurisdiction(Alliance _this, DBAllianceOne from, DBAllianceOne to, int type);
        ErrorCodes DepotDonateEquip(Alliance _this, ulong characterId, string name, ItemBaseData item);
        ErrorCodes DepotTakeOutEquip(Alliance _this, ulong characterId, string name, int bagindex, int itemId, out ItemBaseData item);
        ErrorCodes DepotClearUp(Alliance _this, ulong characterId, string name, ClearUpInfo info);
        ErrorCodes DepotItemRemove(Alliance _this, ulong characterId, string name, int bagindex, int itemId);
        ErrorCodes DepotArrange(Alliance _this, string name);
        int DonationAllianceItem(Alliance _this, ulong guid, int type, string name, ref int itemCount);
        void GetAllianceData(Alliance _this, AllianceData data);
        void GetAllianceDonationData(Alliance _this, AllianceDonationData data);
        void GetAllianceDepotLogData(Alliance _this, AllianceDepotLogData data);
        void GetAllianceDepotData(Alliance _this, DBAllianceDepotDataOne data);
        void GetAllianceMissionData(Alliance _this, List<AllianceMissionDataOne> data);
        GuildMissionRecord GetFreeMission(Alliance _this, List<GuildMissionRecord> mis);
        int GetMemberCount(Alliance _this);

        void GetMemberData(Alliance _this,
                           AllianceMemberData netData,
                           DBAllianceOne dbData,
                           SceneSimpleData dbSceneSimple);

        int GetMemberMaxCount(Alliance _this);
        int GetTotleFightPoint(Alliance _this);
        DBAllianceData InitByBase(Alliance _this, int serverId, string Name, int aId, AllianceManager d);
        void InitByDB(Alliance _this, DBAllianceData DB, AllianceManager d);
        ErrorCodes InviteJoin(Alliance _this, ulong fromGuid, ulong guid);
        ErrorCodes Leave(Alliance _this, ulong guid, bool isLeader = false);
        void RefreshMission(Alliance _this);
        ErrorCodes RefuseApply(Alliance _this, ulong characterGuid);
        ErrorCodes RefuseInvite(Alliance _this, ulong characterGuid);
        void SetAutoAgree(Alliance _this, int value);
        void SetFlag(Alliance _this, bool b = true);
        void SetLeader(Alliance _this, ulong value);
        void SetLevel(Alliance _this, int value);
        void SetMoney(Alliance _this, int value);
        void SetDepot(Alliance _this, BagBaseData value);
        void SetNotice(Alliance _this, string value);
        void SetState(Alliance _this, TeamAllianceState value);
        void SetTotleFightPointFlag(Alliance _this);
        void TotleFightPointChange(Alliance _this, int changeValue);
        ErrorCodes UpgradeAllianceLevel(Alliance _this, ulong characterId);
        void AddRes(Alliance _this, Dictionary<int, int> res);
        void DealRes(Alliance _this);
        void CheckLadder(Alliance _this);
    }

    public class AllianceDefaultImpl : IAlliance
    {
        #region 网络通知

        private ErrorCodes SendMessage(ulong toCharacterId, int type, int param1, int param2)
        {
            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(toCharacterId, out toCharacterProxy))
            {
                var ChattoCharacterProxy = toCharacterProxy as TeamProxy;
                ChattoCharacterProxy.ChangeAllianceData(type, param1, param2);
                return ErrorCodes.OK;
            }
            return ErrorCodes.Error_CharacterOutLine;
        }

        #endregion

        #region 数据结构

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [Updateable("Alliance")]
        public static readonly int applycount = Table.GetServerConfig(323).ToInt();
        [Updateable("Alliance")]
        public static readonly int DonationMax = Table.GetServerConfig(324).ToInt();
        [Updateable("Alliance")]
        public static readonly int DepotLogMax = 30;//需要策划配置

        #endregion

        #region 便捷读取

        public void SetState(Alliance _this, TeamAllianceState value)
        {
            if ((int) value != _this.mDBData.State)
            {
                SetFlag(_this);
            }
            _this.mDBData.State = (int) value;
        }

        //盟主
        public void SetLeader(Alliance _this, ulong value)
        {
            if (value != _this.mDBData.Leader)
            {
                SetFlag(_this);
            }
            _this.mDBData.Leader = value;
        }

        //通知
        public void SetNotice(Alliance _this, string value)
        {
            if (value != _this.mDBData.Notice)
            {
                SetFlag(_this);
            }
            _this.mDBData.Notice = value;
        }

        //战盟等级
        public void SetLevel(Alliance _this, int value)
        {
            if (value != _this.mDBData.Level)
            {
                SetFlag(_this);
            }
            _this.mDBData.Level = value;
        }

        //自动同意
        public void SetAutoAgree(Alliance _this, int value)
        {
            if (value != _this.mDBData.AutoAgree)
            {
                SetFlag(_this);
            }
            _this.mDBData.AutoAgree = value;
        }

        //战盟资金
        public void SetMoney(Alliance _this, int value)
        {
            if (value != _this.mDBData.Money)
            {
                SetFlag(_this);
            }
            _this.mDBData.Money = value;
        }

        //战盟仓库
        public void SetDepot(Alliance _this, BagBaseData value)
        {
            SetFlag(_this);
            _this.mDBData.Depot = value;
        }

        //获得联盟数据
        public void GetAllianceData(Alliance _this, AllianceData data)
        {
            data.Id = _this.AllianceId;
            data.ServerId = _this.ServerId;
            data.Name = _this.Name;
            data.Leader = _this.Leader;
            data.Notice = _this.Notice;
            data.Level = _this.Level;
            data.FightPoint = GetTotleFightPoint(_this);
            data.Money = _this.Money;
            data.AutoAgree = _this.AutoAgree;
            data.Depot = _this.Depot;
            data.Res.AddRange(_this.Res);
            //data.Contribution = _this.Contribution;
            //data.Buffs.AddRange(_this.mDBData.Buffs);
        }

        //获得联盟的任务数据
        public void GetAllianceMissionData(Alliance _this, List<AllianceMissionDataOne> data)
        {
            data.Clear();
            foreach (var mission in _this.mDBData.Missions)
            {
                data.Add(new AllianceMissionDataOne
                {
                    Id = mission.Id,
                    State = mission.State, //状态
                    NextTime = mission.NextTime, //下次时间	
                    MaxCount = mission.MaxCount, //最大物品数量
                    NowCount = mission.NowCount //当前物品数量
                });
            }
        }

        //获得成员数据
        public void GetMemberData(Alliance _this,
                                  AllianceMemberData netData,
                                  DBAllianceOne dbData,
                                  SceneSimpleData dbSceneSimple)
        {
            netData.Guid = dbData.Guid;
            netData.Ladder = dbData.Ladder;
            netData.MeritPoint = dbData.MeritPoint;
            netData.LostTime = dbData.LastTime;

            if (dbSceneSimple == null)
            {
                netData.SceneId = dbData.SceneId; //dbSceneSimple.SceneId;
                netData.Name = dbData.Name; //dbSceneSimple.Name;
                netData.Level = dbData.Level; //dbSceneSimple.Level;
                netData.TypeId = dbData.RoleId; //dbSceneSimple.TypeId;
                netData.FightPoint = dbData.FightPoint; //dbSceneSimple.FightPoint;
            }
            else
            {
                //更新Team数据
                dbData.SceneId = dbSceneSimple.SceneId;
                dbData.Name = dbSceneSimple.Name;
                dbData.Level = dbSceneSimple.Level;
                dbData.RoleId = dbSceneSimple.TypeId;
                dbData.FightPoint = dbSceneSimple.FightPoint;
                //更新Net数据
                netData.SceneId = dbData.SceneId; //dbSceneSimple.SceneId;
                netData.Name = dbData.Name; //dbSceneSimple.Name;
                netData.Level = dbData.Level; //dbSceneSimple.Level;
                netData.TypeId = dbData.RoleId; //dbSceneSimple.TypeId;
                netData.FightPoint = dbData.FightPoint; //dbSceneSimple.FightPoint;
            }
        }

        //获得捐献记录
        public void GetAllianceDonationData(Alliance _this, AllianceDonationData data)
        {
            foreach (var dataOne in _this.mDBData.Donation)
            {
                data.Datas.Add(new AllianceDonationDataOne
                {
                    Name = dataOne.Name,
                    Time = dataOne.Time,
                    ItemId = dataOne.ItemId,
                    Count = dataOne.Count
                });
            }
        }

        //获得仓库操作记录
        public void GetAllianceDepotLogData(Alliance _this, AllianceDepotLogData data)
        {
            //foreach (var LogData in _this.mDBData.DepotLog)
            //{
            //    data.Datas.Add(new AllianceDepotLogDataOne
            //    {
            //        Time = LogData.Time,
            //        Name = LogData.Name,
            //        Type = LogData.Type,
            //        ItemId = LogData.ItemId,
            //    });
            //}
            for (int i = _this.mDBData.DepotLog.Count - 1; i >= 0; i--)
            {
                data.Datas.Add(new AllianceDepotLogDataOne
                {
                    Time = _this.mDBData.DepotLog[i].Time,
                    Name = _this.mDBData.DepotLog[i].Name,
                    Type = _this.mDBData.DepotLog[i].Type,
                    ItemId = _this.mDBData.DepotLog[i].ItemId,
                });
            }
        }

        //获得仓库操作记录
        public void GetAllianceDepotData(Alliance _this, DBAllianceDepotDataOne data)
        {
            data.DepotData = _this.mDBData.Depot;
        }

        #endregion

        #region 初始化

        public DBAllianceData InitByBase(Alliance _this, int serverId, string Name, int aId, AllianceManager d)
        {
            _this.mDBData = new DBAllianceData();
            _this.mDBData.Id = aId;
            _this.mDBData.Name = Name;
            _this.mDBData.ServerId = serverId;
            _this.mDBData.Level = 1;
            _this.mDBData.CreateTime = DateTime.Now.ToBinary();
            _this.Dad = d;
            InitDepot(_this);
            //_this.mDBData.Buffs.Add(101);
            //_this.mDBData.Buffs.Add(201);
            //_this.mDBData.Buffs.Add(301);
            //_this.mDBData.Buffs.Add(401);
            RefreshMission(_this);
            return _this.mDBData;
        }

        public void InitDepot(Alliance _this)
        {
            _this.mDBData.Depot = new BagBaseData();
            var capacity = GetDepotCapacity(_this);
            for (int i = 0; i < capacity; i++)
            {
                var itemData = new ItemBaseData();
                itemData.ItemId = -1;
                _this.mDBData.Depot.Items.Add(itemData);
            }
        }

        public void InitByDB(Alliance _this, DBAllianceData DB, AllianceManager d)
        {
            _this.mDBData = DB;
            if (null == _this.mDBData.Depot || _this.mDBData.Depot.Items.Count == 0)
            {
                InitDepot(_this);
            }
            _this.Dad = d;
            var isNeedAdd = false;
            foreach (var data in _this.mDBData.Missions)
            {
                if (data.State != (int) AllianceMissionState.Wait)
                {
                    continue;
                }
                var nextTime = DateTime.FromBinary(data.NextTime);
                if (nextTime > DateTime.Now)
                {
                    TeamServerControl.tm.CreateTrigger(nextTime, _this.AddMission);
                }
                else
                {
                    isNeedAdd = true;
                }
            }
            if (isNeedAdd)
            {
                AddMission(_this);
            }
            ServerAllianceManager.CheckAllianceId(_this.AllianceId);
            SetTotleFightPointFlag(_this);
        }

        //获得成员数量
        public int GetMemberCount(Alliance _this)
        {
            return _this.mDBData.Members.Count;
        }

        //获得总人数
        public int GetMemberMaxCount(Alliance _this)
        {
            return Table.GetGuild(_this.mDBData.Level).MaxCount;
        }

        //获得总战斗力
        public int GetTotleFightPoint(Alliance _this)
        {
            if (_this.TFPFlag)
            {
                RefreshFightPoint(_this);
                _this.TFPFlag = false;
            }
            return _this.TotleFightPoint;
        }

        //修改战斗力的总值
        public void TotleFightPointChange(Alliance _this, int changeValue)
        {
            _this.TotleFightPoint += changeValue;
        }

        //设置战斗力脏标记
        public void SetTotleFightPointFlag(Alliance _this)
        {
            _this.TFPFlag = true;
        }

        //重新计算成员战斗力
        private void RefreshFightPoint(Alliance _this)
        {
            var tfp = 0;
            foreach (var member in _this.mDBData.Members)
            {
                var m = _this.Dad.GetCharacterData(member);
                if (m == null)
                {
                    continue;
                }
                tfp += m.FightPoint;
            }
            _this.TotleFightPoint = tfp;
        }

        ////获得总战斗力
        //public int GetMoney()
        //{
        //    return MyRandom.Random(98765);
        //}
        //刷新任务
        public void RefreshMission(Alliance _this)
        {
            var mis = ServerAllianceManager.GetMission(_this.Level);
            if (mis == null)
            {
                return;
            }
            _this.Missions.Clear();
            var temp = mis.RandRange(0, 4);
            foreach (var record in temp)
            {
                _this.Missions.Add(new DBAllianceMissionData
                {
                    Id = record.Id,
                    MaxCount = MyRandom.Random(record.MinCount, record.MaxCount),
                    NextTime = DateTime.Now.ToBinary(),
                    State = (int) AllianceMissionState.Normal,
                    NowCount = 0
                });
            }
        }

        //获取一个当前没有的任务
        public GuildMissionRecord GetFreeMission(Alliance _this, List<GuildMissionRecord> mis)
        {
            //Dictionary<GuildMissionRecord,int> frees = new Dictionary<GuildMissionRecord, int>();
            var frees = new List<GuildMissionRecord>();
            //int totleValue = 0;
            foreach (var record in mis)
            {
                var find = false;
                foreach (var data in _this.Missions)
                {
                    if (data.State == 0)
                    {
                        continue;
                    }
                    if (data.Id == record.Id)
                    {
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    //int value = 1;
                    //frees.Add(record,value);
                    //totleValue += value;
                    frees.Add(record);
                }
            }
            if (frees.Count > 0)
            {
                return frees.Range();
                //int r = MyRandom.Random(totleValue);
                //var t = frees.Random();
                //foreach (var i in frees)
                //{
                //    if (i.Value < r)
                //    {
                //        return i.Key;
                //    }
                //    else
                //    {
                //        r -= i.Value;
                //    }
                //}
            }
            return null;
        }

        //增加任务
        public void AddMission(Alliance _this)
        {
            var mis = ServerAllianceManager.GetMission(_this.Level);
            if (mis == null)
            {
                return;
            }
            if (_this.Missions.Count < 4)
            {
                var record = GetFreeMission(_this, mis);
                if (record == null)
                {
                    Logger.Warn("AddMission not Free!Level={0}", _this.Level);
                    return;
                }
                _this.Missions.Add(new DBAllianceMissionData
                {
                    Id = record.Id,
                    MaxCount = MyRandom.Random(record.MinCount, record.MaxCount),
                    NextTime = DateTime.Now.ToBinary(),
                    State = (int) AllianceMissionState.Normal,
                    NowCount = 0
                });
                return;
            }
            foreach (var data in _this.Missions)
            {
                if (data.State != (int) AllianceMissionState.Wait)
                {
                    continue;
                }
                if (DateTime.FromBinary(data.NextTime) > DateTime.Now)
                {
                    continue;
                }

                var record = GetFreeMission(_this, mis);
                if (record == null)
                {
                    Logger.Warn("AddMission not Free!Level={0}", _this.Level);
                    return;
                }

                data.Id = record.Id;
                data.MaxCount = MyRandom.Random(record.MinCount, record.MaxCount);
                data.State = (int) AllianceMissionState.Normal;
                data.NowCount = 0;
                //break;
            }
        }

        #endregion

        #region 操作

        //邀请加入
        public ErrorCodes InviteJoin(Alliance _this, ulong fromGuid, ulong guid)
        {
            //攻城战期间，参战公会不能有人员变动
            var serverId = SceneExtension.GetServerLogicId(_this.ServerId);
            var war = AllianceWarManager.WarDatas[serverId];
            if (war.GetStatus() >= (int)eAllianceWarState.WaitStart && war.AllianceIds.Contains(_this.AllianceId))
            {
                return ErrorCodes.Error_AllianceWarFighting;
            }
            if (_this.Dad.GetCharacterData(guid) != null)
            {
                return ErrorCodes.Error_CharacterHaveAlliance;
            }
            InviteData inviteData;
            if (_this.Invites.TryGetValue(guid, out inviteData))
            {
                if (!inviteData.InviteFrom.Contains(fromGuid))
                {
                    inviteData.InviteFrom.Add(fromGuid);
                }
                TeamServerControl.tm.ChangeTime(ref inviteData.trigger, DateTime.Now.AddSeconds(32));
                return ErrorCodes.OK;
            }
            var inviteTrigger = TeamServerControl.tm.CreateTrigger(DateTime.Now.AddSeconds(32),
                () => RemoveInvite(_this, guid, 5));
            var temp = new InviteData
            {
                trigger = inviteTrigger
            };
            temp.InviteFrom.Add(fromGuid);
            _this.Invites[guid] = temp;
            return ErrorCodes.OK;
        }

        //取消邀请
        private void RemoveInvite(Alliance _this, ulong guid, int result)
        {
            InviteData inviteData;
            if (_this.Invites.TryGetValue(guid, out inviteData))
            {
                _this.Invites.Remove(guid);
                TeamServerControl.tm.DeleteTrigger(inviteData.trigger);
                foreach (var uId in inviteData.InviteFrom)
                {
                    var one = _this.Dad.GetCharacterData(uId);
                    if (one != null)
                    {
                        CoroutineFactory.NewCoroutine(SendCharacterMessage, _this, uId, result, guid, _this.AllianceId,
                            _this.Name).MoveNext();
                    }
                }
            }
        }

        private IEnumerator SendCharacterMessage(Coroutine coroutine,
                                                 Alliance _this,
                                                 ulong sendId,
                                                 int type,
                                                 ulong cId,
                                                 int aId,
                                                 string aName)
        {
            var logicSimpleData = TeamServer.Instance.LogicAgent.GetLogicSimpleData(cId, 0);
            yield return logicSimpleData.SendAndWaitUntilDone(coroutine);
            if (logicSimpleData.State != MessageState.Reply)
            {
                yield break;
            }
            if (logicSimpleData.ErrorCode != (int) ErrorCodes.OK)
            {
                yield break;
            }
            ServerAllianceManager.SendMessage(sendId, type, logicSimpleData.Response.Name, _this.AllianceId, _this.Name);
        }

        //申请加入
        public ErrorCodes ApplyJoin(Alliance _this, ulong guid)
        {
            if (_this.Dad.GetCharacterData(guid) != null)
            {
                return ErrorCodes.Error_CharacterHaveAlliance;
            }
            var result = _this.Dad.CheckApplyJoin(_this.ServerId, _this.AllianceId, guid);
            if (result != ErrorCodes.OK)
            {
                return result;
            }
            if (_this.Dad.mExitTimer.ContainsKey(guid) == true)
            {
                DateTime t = DateTime.FromBinary(_this.Dad.mExitTimer[guid]);
                if ((DateTime.Now - t).TotalHours < 12)
                {
                    return ErrorCodes.Error_Alliance_Limit_Time;
                }
            }
            if (_this.mDBData.Applys.Contains(guid))
            {
                return ErrorCodes.Error_AlreadyApply;
            }
            if (_this.mDBData.Applys.Count >= applycount)
            {
                return ErrorCodes.Error_AllianceApplyIsFull;
            }
            if (GetMemberCount(_this) >= GetMemberMaxCount(_this))
            {
                return ErrorCodes.Error_AllianceIsFull;
            }
            if (_this.mDBData.AutoAgree == 1)
            {
                //攻城战期间，参战公会不能有人员变动
                var serverId = SceneExtension.GetServerLogicId(_this.ServerId);
                var war = AllianceWarManager.WarDatas[serverId];
                if (war.GetStatus() >= (int)eAllianceWarState.WaitStart && war.AllianceIds.Contains(_this.AllianceId))
                {
                    return ErrorCodes.Error_AllianceWarFighting;
                }
                AddCharacter(_this, guid);
                //玩家加入战盟后清除对其他战盟的申请标记
                var ServerManager = ServerAllianceManager.GetAllianceByServer(_this.ServerId);
                if(ServerManager != null)
                {
                    ServerManager.ClearPlayerApplyLog(_this.ServerId, guid);
                }
                //var Server = ServerManager.GetServerData(_this.ServerId);
                //if (Server == null)
                //{
                //    return ErrorCodes.ServerID;
                //}
                //if (Server.Applys.ContainsKey(guid))
                //{
                //    var dbapply = Server.Applys[guid];
                //    if (dbapply.Applys.Count > 0)
                //    {
                //        for (int i = 0; i < dbapply.Applys.Count; i++)
                //        {
                //            var allianceid = dbapply.Applys[i];
                //            var AllianceData = ServerAllianceManager.GetAllianceById(allianceid);
                //            if (AllianceData.mDBData.Applys.Contains(guid))
                //            {
                //                AllianceData.mDBData.Applys.Remove(guid);
                //            }
                //        }
                //    }
                //    dbapply.Applys.Clear();
                //}
                return ErrorCodes.Error_AllianceApplyJoinOK;
            }
            var nowCount = _this.mDBData.Applys.Count;
            if (nowCount >= applycount)
            {
                for (var i = 0; i < nowCount - applycount + 1; i++)
                {
                    _this.mDBData.Applys.RemoveAt(0);
                }
            }
            if (nowCount == 0)
            {
                foreach (var member in _this.mDBData.Members)
                {
                    var s = _this.Dad.GetCharacterData(member);
                    if (s == null)
                    {
                        continue;
                    }
                    if (s.Ladder > 0)
                    {
                        var mtype = 1; // 表示applylist从0成员到1成员
                        ServerAllianceManager.SendMessage(member, 6, "", mtype, _this.Name);
                    }
                }
            }
            _this.mDBData.Applys.Add(guid);
            _this.Dad.SuccessApplyJoin(_this.ServerId, _this.AllianceId, guid);
            SetFlag(_this);
            return ErrorCodes.OK;
        }

        //取消申请
        public ErrorCodes ApplyCancel(Alliance _this, ulong guid)
        {
            if (!_this.mDBData.Applys.Contains(guid))
            {
                return ErrorCodes.Error_AllianceNoApply;
            }
            var ServerManager = ServerAllianceManager.GetAllianceByServer(_this.ServerId);
            var Server = ServerManager.GetServerData(_this.ServerId);
            if (Server == null)
            {
                return ErrorCodes.ServerID;
            }
            ServerManager.ClearPlayerApplyLog(_this.ServerId, guid, _this.AllianceId);
            SetFlag(_this);
            _this.Dad.CancelApplyJoin(_this.ServerId, _this.AllianceId, guid);
            return ErrorCodes.OK;
        }

        //退出
        public ErrorCodes Leave(Alliance _this, ulong guid, bool isLeader = false)
        {
            //攻城战期间，参战公会不能有人员变动
            var serverId = SceneExtension.GetServerLogicId(_this.ServerId);
            var war = AllianceWarManager.WarDatas[serverId];
            if (war.GetStatus() >= (int)eAllianceWarState.WaitStart && war.AllianceIds.Contains(_this.AllianceId))
            {
                return ErrorCodes.Error_AllianceWarFighting;
            }
            if (!_this.mDBData.Members.Contains(guid))
            {
                return ErrorCodes.Error_CharacterNotFind;
            }
            string name = _this.GetCharacterName(guid);
            string leaderName = "";
            ulong leaderId = 0;

            _this.mDBData.Members.Remove(guid);
            SetTotleFightPointFlag(_this);
            _this.Dad.RemoveCharacterData(guid);
            if (_this.mDBData.Members.Count < 1)
            {
//需要解散了
                SetState(_this, TeamAllianceState.WillDisband);
            }
            else if (isLeader)
            {
//会长退了
                DBAllianceOne willLadder = null;
                foreach (var member in _this.mDBData.Members)
                {
                    var m = _this.Dad.GetCharacterData(member);
                    if (m == null)
                    {
                        continue;
                    }
                    if (willLadder == null)
                    {
                        willLadder = m;
                    }
                    else if (m.Ladder > willLadder.Ladder)
                    {
                        willLadder = m;
                    }
                    else if (m.Ladder == willLadder.Ladder && m.MeritPoint > willLadder.MeritPoint)
                    {
                        willLadder = m;
                    }
                }
                if (willLadder != null)
                {
                    leaderId = willLadder.Guid;
                    leaderName = _this.GetCharacterName(willLadder.Guid);
                    SetLeader(_this, willLadder.Guid);
                    willLadder.Ladder = 3;
                    ToLogicAllianceData(3, willLadder.Guid, _this.AllianceId, 3, "");
                }
            }
            if (_this.mDBData.Members.Count > 0)
            {
                foreach (var member in _this.mDBData.Members)
                {//广播玩家退出公会
                    TeamCharacterProxy Proxy;
                    if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(member, out Proxy))
                        if (null != Proxy)
                            Proxy.SCNotifyPlayerExitAlliance(guid,name,isLeader,leaderId,leaderName);
                }                
            }





            SetFlag(_this);
            return ErrorCodes.OK;
        }

        //同意邀请
        public ErrorCodes AgreeInvite(Alliance _this, ulong characterGuid)
        {
            //攻城战期间，参战公会不能有人员变动
            var serverId = SceneExtension.GetServerLogicId(_this.ServerId);
            var war = AllianceWarManager.WarDatas[serverId];
            if (war.GetStatus() >= (int)eAllianceWarState.WaitStart && war.AllianceIds.Contains(_this.AllianceId))
            {
                return ErrorCodes.Error_AllianceWarFighting;
            }
            if (!_this.Invites.ContainsKey(characterGuid))
            {
                return ErrorCodes.Error_CharacterNotFind;
            }
            if (_this.mDBData.Members.Count >= _this.GetMemberMaxCount())
            {
                return ErrorCodes.Error_AllianceIsFull;
            }
            RemoveInvite(_this, characterGuid, 3);
            CoroutineFactory.NewCoroutine(AddNewCharacter, _this, characterGuid, 0).MoveNext();

            return ErrorCodes.OK;
        }

        //拒绝邀请
        public ErrorCodes RefuseInvite(Alliance _this, ulong characterGuid)
        {
            if (!_this.Invites.ContainsKey(characterGuid))
            {
                return ErrorCodes.Error_CharacterNotFind;
            }
            RemoveInvite(_this, characterGuid, 4);
            return ErrorCodes.OK;
        }

        //批量同意的战盟申请
        public ErrorCodes AllianceAgreeApplyList(Alliance _this, DBAllianceOne from, List<ulong> characterGuids)
        {
            //攻城战期间，参战公会不能有人员变动
            var serverId = SceneExtension.GetServerLogicId(_this.ServerId);
            var war = AllianceWarManager.WarDatas[serverId];
            if (war.GetStatus() >= (int)eAllianceWarState.WaitStart && war.AllianceIds.Contains(_this.AllianceId))
            {
                return ErrorCodes.Error_AllianceWarFighting;
            }
            if (characterGuids.Count + GetMemberCount(_this) > GetMemberMaxCount(_this))
            {
                return ErrorCodes.Error_AllianceIsFull;
            }
            //check
            if (characterGuids.Count > 1)
            {
                for (int i = 0; i < characterGuids.Count; i++)
                {
                    var characterGuid = characterGuids[i];
                    if (!_this.mDBData.Applys.Contains(characterGuid))
                    {
                        characterGuids.Remove(characterGuid);
                        continue;
                    }
                    var character = _this.Dad.GetCharacterData(characterGuid);
                    if (character != null)
                    {
                        _this.mDBData.Applys.Remove(characterGuid);
                        characterGuids.Remove(characterGuid);
                        Logger.Warn("AgreeApply Error_CharacterHaveAlliance a={0},c={1},n={2}", _this.AllianceId,
                            characterGuid, character.AllianceId);
                        continue;
                    }
                }
            }
            else
            {
                foreach (var characterGuid in characterGuids)
                {
                    if (!_this.mDBData.Applys.Contains(characterGuid))
                    {
                        return ErrorCodes.Error_CharacterNotFind;
                    }
                    var character = _this.Dad.GetCharacterData(characterGuid);
                    if (character != null)
                    {
                        _this.mDBData.Applys.Remove(characterGuid);
                        Logger.Warn("AgreeApply Error_CharacterHaveAlliance a={0},c={1},n={2}", _this.AllianceId,
                            characterGuid, character.AllianceId);
                        return ErrorCodes.Error_CharacterHaveAlliance;
                    }
                }
            }


            //do
            foreach (var characterGuid in characterGuids)
            {
                AddCharacter(_this, characterGuid);
                _this.Dad.SuccessJoin(_this.ServerId, _this.AllianceId, characterGuid);
                CoroutineFactory.NewCoroutine(ToLogicAllianceDataCoroutine, 0, characterGuid, _this.AllianceId, 0,
                    _this.Name).MoveNext();
            }
            if (_this.mDBData.Applys.Count == 0)
            {
                foreach (var member in _this.mDBData.Members)
                {
                    var s = _this.Dad.GetCharacterData(member);
                    if (s == null)
                    {
                        continue;
                    }
                    if (s.Ladder > 0)
                    {
                        var mtype = 0; // 表示applylist从n成员到0成员
                        ServerAllianceManager.SendMessage(member, 6, "", mtype, _this.Name);
                    }
                }
            }
            foreach (var guid in characterGuids)
            {
                ToChatAllianceMsgCoroutine(_this, 0, guid, 0);
            }
            return ErrorCodes.OK;
        }
        public ErrorCodes ClearAllianceApplyList(Alliance _this, DBAllianceOne from, List<ulong> characterGuids)
        {
            if (characterGuids.Count > 0)
            {
                for (int i = 0; i < characterGuids.Count; i++)
                {
                    var characterGuid = characterGuids[i];
                    if (_this.mDBData.Applys.Contains(characterGuid))
                    {
                        _this.mDBData.Applys.Remove(characterGuid);
                    }
                }
            }
            else
            {
                return ErrorCodes.Error_CharacterNotFind;
            }
            return ErrorCodes.OK;
        }

        //同意申请
        public ErrorCodes AgreeApply(Alliance _this, ulong characterGuid)
        {
            if (!_this.mDBData.Applys.Contains(characterGuid))
            {
                return ErrorCodes.Error_CharacterNotFind;
            }
            //攻城战期间，参战公会不能有人员变动
            var serverId = SceneExtension.GetServerLogicId(_this.ServerId);
            var war = AllianceWarManager.WarDatas[serverId];
            if (war.GetStatus() >= (int)eAllianceWarState.WaitStart && war.AllianceIds.Contains(_this.AllianceId))
            {
                return ErrorCodes.Error_AllianceWarFighting;
            }
            var character = _this.Dad.GetCharacterData(characterGuid);
            if (character != null)
            {
                _this.mDBData.Applys.Remove(characterGuid);
                Logger.Warn("AgreeApply Error_CharacterHaveAlliance a={0},c={1},n={2}", _this.AllianceId, characterGuid,
                    character.AllianceId);
                return ErrorCodes.Error_CharacterHaveAlliance;
            }
            if (_this.mDBData.Members.Count >= _this.GetMemberMaxCount())
            {
                return ErrorCodes.Error_AllianceIsFull;
            }
            AddCharacter(_this, characterGuid);
            _this.Dad.SuccessJoin(_this.ServerId, _this.AllianceId, characterGuid);
            CoroutineFactory.NewCoroutine(ToLogicAllianceDataCoroutine, 0, characterGuid, _this.AllianceId, 0,
                _this.Name).MoveNext();


            return ErrorCodes.OK;
        }

        //通知logic战盟变化
        private static void ToLogicAllianceData(int type, ulong characterGuid, int aId, int ladder, string name)
        {
            CoroutineFactory.NewCoroutine(ToLogicAllianceDataCoroutine, type, characterGuid, aId, ladder, name)
                .MoveNext();
        }

        //通知logic战盟变化
        private static IEnumerator ToLogicAllianceDataCoroutine(Coroutine co,
                                                                int type,
                                                                ulong characterGuid,
                                                                int aId,
                                                                int ladder,
                                                                string name)
        {
            var msg = TeamServer.Instance.LogicAgent.AllianceDataChange(characterGuid, type, aId, ladder, name);
            yield return msg.SendAndWaitUntilDone(co);
        }

        //批量拒绝的战盟申请
        public ErrorCodes AllianceRefuseApplyList(Alliance _this, DBAllianceOne from, List<ulong> characterGuids)
        {
            //check
            foreach (var characterGuid in characterGuids)
            {
                if (!_this.mDBData.Applys.Contains(characterGuid))
                {
                    return ErrorCodes.Error_CharacterNotFind;
                }
                var character = _this.Dad.GetCharacterData(characterGuid);
                if (character != null)
                {
                    Logger.Warn("AgreeApply Error_CharacterHaveAlliance a={0},c={1},n={2}", _this.AllianceId,
                        characterGuid, character.AllianceId);
                    return ErrorCodes.Error_CharacterHaveAlliance;
                }
            }
            //do
            foreach (var characterGuid in characterGuids)
            {
                _this.Dad.CancelApplyJoin(_this.ServerId, _this.AllianceId, characterGuid);
                CoroutineFactory.NewCoroutine(ToLogicAllianceDataCoroutine, 2, characterGuid, _this.AllianceId, 0,
                    _this.Name).MoveNext();
                ServerAllianceManager.SendMessage(characterGuid, 2, from.Name, _this.AllianceId, _this.Name);
  
            }
            if (_this.mDBData.Applys.Count == 0)
            {
                foreach (var member in _this.mDBData.Members)
                {
                    var s = _this.Dad.GetCharacterData(member);
                    if (s == null)
                    {
                        continue;
                    }
                    if (s.Ladder > 0)
                    {
                        var mtype = 0; // 表示applylist从n成员到0成员
                        ServerAllianceManager.SendMessage(member, 6, "", mtype, _this.Name);
                    }
                }
            }
            SetFlag(_this);
            return ErrorCodes.OK;
        }

        //拒绝申请
        public ErrorCodes RefuseApply(Alliance _this, ulong characterGuid)
        {
            if (!_this.mDBData.Applys.Contains(characterGuid))
            {
                return ErrorCodes.Error_CharacterNotFind;
            }
            _this.Dad.CancelApplyJoin(_this.ServerId, _this.AllianceId, characterGuid);
            SetFlag(_this);
            return ErrorCodes.OK;
        }

        //权限变更 
        public ErrorCodes ChangeJurisdiction(Alliance _this, DBAllianceOne from, DBAllianceOne to, int type)
        {
            var oldLadder = to.Ladder;
            if (from.Ladder <= to.Ladder)
            {
                return ErrorCodes.Error_JurisdictionNotEnough;
            }
            if (from.Ladder < 2)
            {
                return ErrorCodes.Error_JurisdictionNotEnough;
            }
            if (type >= from.Ladder && from.Ladder != 3)
            {
                return ErrorCodes.Error_JurisdictionNotEnough;
            }
            //if (to.Ladder == type)
            //{
            //    return ErrorCodes.Unknow;
            //}
            //var tb = Table
            switch (type)
            {
                case -1: //踢出
                {
                    if (from.Ladder != 3)
                    {
                        return ErrorCodes.Error_JurisdictionNotEnough;
                    }
                    var name = _this.GetCharacterName(to.Guid);
                    var resultCodes = Leave(_this, to.Guid);
                    if (resultCodes == ErrorCodes.OK)
                    {
                        CoroutineFactory.NewCoroutine(ToLogicAllianceDataCoroutine, 1, to.Guid, 0, 0, "").MoveNext();
                        ToChatAllianceMsgCoroutine(_this, 2, name);
                        //CoroutineFactory.NewCoroutine(ToChatAllianceMsgCoroutine, _this, 2, from.Guid, tempList, 0).MoveNext();
                        SetFlag(_this);
                    }
                    return resultCodes;
                }
                case 0: //改为：成员
                {
                    to.Ladder = 0;
                    SetFlag(_this);
                }
                    break;
                case 1: //改为：左右护法
                {
                    to.Ladder = 1;
                    SetFlag(_this);
                }
                    break;
                case 2: //改为：副首领
                {
                    to.Ladder = 2;
                    SetFlag(_this);
                }
                    break;
                case 3: //改为：首领
                {
                    SetLeader(_this, to.Guid);
                    to.Ladder = 3;
                    from.Ladder = 0;
                    SetFlag(_this);
                    CoroutineFactory.NewCoroutine(ToLogicAllianceDataCoroutine, 3, from.Guid,
                        _this.AllianceId, from.Ladder, "").MoveNext();
                }
                    break;
                default:
                    return ErrorCodes.Error_DataOverflow;
            }
            CoroutineFactory.NewCoroutine(ToLogicAllianceDataCoroutine, 3, to.Guid, _this.AllianceId, to.Ladder,
                "").MoveNext();
            if (oldLadder < type && type > 0)
            {
                ToChatAllianceMsgCoroutine(_this, 1, to.Guid, type);
                //var tempList = new List<ulong>();
                //tempList.Add(to.Guid);
                //CoroutineFactory.NewCoroutine(ToChatAllianceMsgCoroutine, _this, 1, from.Guid, tempList, type).MoveNext();
            }
            return ErrorCodes.OK;
        }

        //添加成员
        public DBAllianceOne AddCharacter(Alliance _this, ulong guid, int ladder = 0)
        {
            var temp = new DBAllianceOne
            {
                Guid = guid,
                Ladder = ladder,
                AllianceId = _this.AllianceId
            };
            _this.mDBData.Members.Add(guid);
            SetTotleFightPointFlag(_this);
            _this.Dad.AddCharacterData(_this.ServerId, guid, temp);
            SetFlag(_this);
            CoroutineFactory.NewCoroutine(GetAcData, _this, temp).MoveNext();


            Dict_int_int_Data change = new Dict_int_int_Data();
            change.Data.Add(379, _this.Level);

            ChangeExData(guid, change);
            return temp;
        }

        //异步添加成员
        private IEnumerator AddNewCharacter(Coroutine co, Alliance _this, ulong guid, int ladder = 0)
        {
            var temp = new DBAllianceOne
            {
                Guid = guid,
                Ladder = ladder,
                AllianceId = _this.AllianceId
            };
            _this.mDBData.Members.Add(guid);
            SetTotleFightPointFlag(_this);
            _this.Dad.AddCharacterData(_this.ServerId, guid, temp);
            SetFlag(_this);
            var co1 = CoroutineFactory.NewSubroutine(GetAcData, co, _this, temp);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            ToChatAllianceMsgCoroutine(_this, 0, guid);
        }

        //获取成员的完整信息
        private static IEnumerator GetAcData(Coroutine coroutine, Alliance _this, DBAllianceOne dbAllianceOne)
        {
            var sceneSimpleData = TeamServer.Instance.SceneAgent.GetSceneSimpleData(dbAllianceOne.Guid, 0);
            yield return sceneSimpleData.SendAndWaitUntilDone(coroutine);
            if (sceneSimpleData.State != MessageState.Reply)
            {
                yield break;
            }
            if (sceneSimpleData.ErrorCode != (int) ErrorCodes.OK)
            {
                yield break;
            }
            dbAllianceOne.Name = sceneSimpleData.Response.Name;
            dbAllianceOne.Level = sceneSimpleData.Response.Level;
            dbAllianceOne.FightPoint = sceneSimpleData.Response.FightPoint;
            dbAllianceOne.RoleId = sceneSimpleData.Response.TypeId;
            dbAllianceOne.SceneId = sceneSimpleData.Response.SceneId;
            dbAllianceOne.ServerId = sceneSimpleData.Response.ServerId;
        }

        //修改自动同意设置
        public ErrorCodes ChangeAllianceAutoJoin(Alliance _this, DBAllianceOne guid, int value)
        {
            var tbGuildAccess = Table.GetGuildAccess(guid.Ladder);
            if (tbGuildAccess == null)
            {
                return ErrorCodes.Unknow;
            }
            if (tbGuildAccess.CanAddMember == 1)
            {
                SetAutoAgree(_this, value);
                return ErrorCodes.OK;
            }
            return ErrorCodes.Error_JurisdictionNotEnough;
        }

        public void SetFlag(Alliance _this, bool b = true)
        {
            _this.bFlag = b;
            _this.Dad.SetFlag(_this.mDBData.ServerId);
        }

        //增加战盟资金
        public void AddMoney(Alliance _this, int value)
        {
            SetMoney(_this, _this.Money + value);
            ServerLodeManagerManager.OnAlliaceEvent(SceneExtension.GetServerLogicId(_this.ServerId), _this.AllianceId, 2);
        }

        public void AddRes(Alliance _this, Dictionary<int, int> res)
        {
            foreach (var v in res)
            {
                if (v.Key == 0)
                {
                    AddMoney(_this, v.Value);
                }
                else
                {
                    _this.mDBData.Res.modifyValue(v.Key, v.Value);
                }
            }
            SetFlag(_this);
        }

        public void DealRes(Alliance _this)
        {
            if (_this.mDBData.Res.Count == 0)
                return;
            foreach (var v in _this.mDBData.Members)
            {
                var member = _this.Dad.GetCharacterData(v);
                if (member == null)
                {
                    continue;
                }
                var tbGA = Table.GetGuildAccess(member.Ladder);
                if (tbGA == null)
                {
                    Logger.Error("In SendReward().tbGA == null");
                    continue;
                }
                var mail = Table.GetMail(tbGA.LodeMailId);
                if (mail == null)
                {
                    Logger.Error("In SendReward().mail [{0}]== null",tbGA.LodeMailId);
                    continue;
                }
                var data = new Dict_int_int_Data();
                foreach (var res in _this.mDBData.Res)
                {
                    var val = (long) res.Value*tbGA.LodeResRatio/10000;
                    data.Data.Add(res.Key,(int)val);
                }
                CoroutineFactory.NewCoroutine(SendMailCoroutine, v, mail.Title,mail.Text, data).MoveNext();
            }            
            _this.mDBData.Res.Clear();
            SetFlag(_this);
        }
        private IEnumerator SendMailCoroutine(Coroutine co,
                                                ulong id,
                                                string title,
                                                string content,
                                                Dict_int_int_Data items)
        {
            var msg = TeamServer.Instance.LogicAgent.SendMailToCharacter(id, title, content, items, 0);
            yield return msg.SendAndWaitUntilDone(co);
        }
        //仓库添加Item
        public bool AddDepotItem(Alliance _this, ItemBaseData item)
        {
            var items = _this.mDBData.Depot.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ItemId != -1)
                {
                    continue;
                }
                else
                {
                    _this.mDBData.Depot.Items[i] = item;
                    _this.mDBData.Depot.NowCount += 1;
                    return true;
                }
            }
            return false;
        }

        //仓库移除Item
        public void RemoveDepotItem(Alliance _this, int bagindex)
        {
            var itemData = new ItemBaseData();
            itemData.ItemId = -1;
            _this.mDBData.Depot.Items[bagindex] = itemData;
            _this.mDBData.Depot.NowCount -= 1;
        }

        //升级仓库容量
        public void UpgradeDepot(Alliance _this)
        {
            var upgradeCount = GetDepotCapacity(_this);
            var nowCount = _this.mDBData.Depot.Items.Count;
            List<ItemBaseData> tempList = new List<ItemBaseData>();
            for (int i = 0; i < upgradeCount - nowCount; i++)
            {
                var itemData = new ItemBaseData();
                itemData.ItemId = -1;
                tempList.Add(itemData);
            }
            _this.mDBData.Depot.Items.AddRange(tempList);
        }

        //战盟仓库捐赠
        public ErrorCodes DepotDonateEquip(Alliance _this, ulong characterId, string name, ItemBaseData item)
        {
            var depotCapacity = GetDepotCapacity(_this);
            //判断仓库是否满
            if (_this.mDBData.Depot.NowCount >= depotCapacity)
            {
                return ErrorCodes.Error_AllianceDepotIsFull;
            }
            AddDepotItem(_this, item);
            SetDepot(_this, _this.mDBData.Depot);
            ServerLodeManagerManager.OnAlliaceEvent(SceneExtension.GetServerLogicId(_this.ServerId), _this.AllianceId, 3);
            //添加LOG
            {
                _this.mDBData.DepotLog.Add(new DBAllianceDepotOperationData
                {
                    Time = DateTime.Now.ToBinary(),
                    Name = name,
                    Type = (int)AllianceDepotOperationType.Donate,
                    ItemId = item.ItemId,
                });
                SetDepotLogCount(_this);
            }

            return ErrorCodes.OK;
        }

        private int GetDepotCapacity(Alliance _this)
        {
            var level = _this.Level;
            var tbGuild = Table.GetGuild(level);
            if (null == tbGuild)
            {
                return -1;
            }
            return tbGuild.DepotCapacity;
        }

        //战盟仓库取出
        public ErrorCodes DepotTakeOutEquip(Alliance _this, ulong characterId, string name, int bagindex,int itemId, out ItemBaseData item)
        {
            if (bagindex < 0 || bagindex >= GetDepotCapacity(_this))
            {
                item = null;
                return ErrorCodes.Error_BagIndexOverflow;
            }

            //已经被取走或仓库进行了整理
            if (_this.mDBData.Depot.Items[bagindex].ItemId != itemId)
            {
                item = null;
                return ErrorCodes.Error_AllianceDepotItemChanged;
            }

            item = _this.mDBData.Depot.Items[bagindex];
            RemoveDepotItem(_this, bagindex);
            SetDepot(_this, _this.mDBData.Depot);

            //添加LOG
            {
                _this.mDBData.DepotLog.Add(new DBAllianceDepotOperationData
                {
                    Time = DateTime.Now.ToBinary(),
                    Name = name,
                    Type = (int)AllianceDepotOperationType.Takeout,
                    ItemId = itemId
                });
                SetDepotLogCount(_this);
            }

            return ErrorCodes.OK;
        }

        //战盟仓库单个清理
        public ErrorCodes DepotItemRemove(Alliance _this, ulong characterId, string name, int bagindex, int itemId)
        {
            if (bagindex < 0 || bagindex >= GetDepotCapacity(_this))
            {
                return ErrorCodes.Error_BagIndexOverflow;
            }

            //已经被取走或仓库进行了整理
            if (_this.mDBData.Depot.Items[bagindex].ItemId != itemId)
            {
                return ErrorCodes.Error_AllianceDepotItemChanged;
            }

            RemoveDepotItem(_this, bagindex);
            var tbItemBase = Table.GetItemBase(itemId);
            if (null == tbItemBase)
                return ErrorCodes.Error_ItemNotFind;
            var removePoints = tbItemBase.GuildPionts;
            AddMoney(_this, removePoints);//销毁增加战盟资金
            SetDepot(_this, _this.mDBData.Depot);

            //添加LOG
            //{
            //    _this.mDBData.DepotLog.Add(new DBAllianceDepotOperationData
            //    {
            //        Time = DateTime.Now.ToBinary(),
            //        Name = name,
            //        Type = (int)AllianceDepotOperationType.Takeout,
            //        ItemId = itemId
            //    }); 
            //    SetDepotLogCount(_this);
            //}

            return ErrorCodes.OK;
        }

        //战盟仓库清理
        public ErrorCodes DepotClearUp(Alliance _this, ulong characterId, string name, ClearUpInfo info)
        {
            var items = _this.mDBData.Depot.Items;
            var clearUpGetPoints = 0;
            for (int i = 0; i < items.Count; i++)
            {
                var itemId = items[i].ItemId;
                if (itemId == -1)
                {
                    continue;
                }
                var tbItemBase = Table.GetItemBase(itemId);
                if (null == tbItemBase)
                {
                    return ErrorCodes.Error_ItemNotFind;
                }
                var tbEquipBase = Table.GetEquip(itemId);
                if (null == tbEquipBase)
                {
                    return ErrorCodes.Error_ItemNotFind;
                }
                for (int j = 0; j < info.Infos.Count; j++)
                {
                    var ladder = info.Infos[j].Ladder;
                    var quality = info.Infos[j].Quality;
                    if (tbItemBase.Quality == quality && tbEquipBase.Ladder == ladder)
                    {
                        clearUpGetPoints += tbItemBase.GuildPionts;
                        RemoveDepotItem(_this, i);
                    }
                }
            }
            AddMoney(_this, clearUpGetPoints);//销毁增加战盟资金
            SetDepot(_this, _this.mDBData.Depot);

            //添加LOG
            {
                _this.mDBData.DepotLog.Add(new DBAllianceDepotOperationData
                {
                    Time = DateTime.Now.ToBinary(),
                    Name = name,
                    Type = (int)AllianceDepotOperationType.ClearUp,
                    ItemId = -1
                });
                SetDepotLogCount(_this);
            }

            return ErrorCodes.OK;
        }

        //战盟仓库整理
        public ErrorCodes DepotArrange(Alliance _this, string name)
        {
            //排序
            {
                var sort_list = new List<KeyValuePair<ItemBaseData, long>>();
                var count = 0;
                foreach (var item in _this.mDBData.Depot.Items)
                {
                    if (item.ItemId == -1)
                    {
                        sort_list.Add(new KeyValuePair<ItemBaseData, long>(item, 0));
                        continue;
                    }
                    count ++;
                    var tb_item = Table.GetItemBase(item.ItemId);
                    long thissortvalue = tb_item.SortLadder;
                    sort_list.Add(new KeyValuePair<ItemBaseData, long>(item, thissortvalue));
                }
                var result_array = (from item in sort_list orderby -item.Value select item).ToArray();

                var dbdata = new BagBaseData();
                foreach (var keyValuePair in result_array)
                {
                    dbdata.Items.Add(keyValuePair.Key);
                }
                dbdata.NowCount = count;
                SetDepot(_this, dbdata);
            }

            ////添加LOG
            //{
            //    _this.mDBData.DepotLog.Add(new DBAllianceDepotOperationData
            //    {
            //        Time = DateTime.Now.ToBinary(),
            //        Name = name,
            //        Type = (int)AllianceDepotOperationType.Arrange,
            //        ItemId = -1
            //    });
            //    SetDepotLogCount(_this);
            //}

            return ErrorCodes.OK;
        }

        //捐献
        public int DonationAllianceItem(Alliance _this, ulong guid, int type, string name, ref int itemCount)
        {
            var character = _this.Dad.GetCharacterData(guid);
            if (character == null)
            {
                return -1;
            }
            if (character.AllianceId != _this.AllianceId)
            {
                return -1;
            }
            switch (type)
            {
                case 0:
                {
                    var tbGuild = Table.GetGuild(_this.Level);
                    if (tbGuild == null)
                    {
                        return -1;
                    }
                    AddMoney(_this, tbGuild.LessUnionMoney);
                    //character.MeritPoint += tbGuild.LessUnionDonation;
                    //_this.Contribution += tbGuild.LessUnionDonation;
                    //Dad.GetCharacterData(guid).
                    _this.mDBData.Donation.Add(new DBAllianceDonationDataOne
                    {
                        Name = name,
                        Time = DateTime.Now.ToBinary(),
                        ItemId = 2,
                        Count = tbGuild.LessNeedCount
                    });
                    SetDonationLogCount(_this);
                }
                    break;
                case 1:
                {
                    var tbGuild = Table.GetGuild(_this.Level);
                    if (tbGuild == null)
                    {
                        return -1;
                    }
                    AddMoney(_this, tbGuild.MoreUnionMoney);
                    //character.MeritPoint += tbGuild.MoreUnionDonation;
                    //_this.Contribution += tbGuild.MoreUnionDonation;
                    _this.mDBData.Donation.Add(new DBAllianceDonationDataOne
                    {
                        Name = name,
                        Time = DateTime.Now.ToBinary(),
                        ItemId = 2,
                        Count = tbGuild.MoreNeedCount
                    });
                    SetDonationLogCount(_this);
                }
                    break;
                case 2:
                {
                    var tbGuild = Table.GetGuild(_this.Level);
                    if (tbGuild == null)
                    {
                        return -1;
                    }
                    AddMoney(_this, tbGuild.DiaUnionMoney);
                    //character.MeritPoint += tbGuild.DiaUnionDonation;
                    //_this.Contribution += tbGuild.DiaUnionDonation;

                    _this.mDBData.Donation.Add(new DBAllianceDonationDataOne
                    {
                        Name = name,
                        Time = DateTime.Now.ToBinary(),
                        ItemId = 3,
                        Count = tbGuild.DiaNeedCount
                    });
                    SetDonationLogCount(_this);
                }
                    break;
                default:
                    var tbGuildMiss = Table.GetGuildMission(type);
                    if (tbGuildMiss == null)
                    {
                        Logger.Warn("DonationAllianceItem type={0}", type);
                        return -1;
                    }
                    foreach (var data in _this.Missions)
                    {
                        if (data.Id == type)
                        {
                            //character.MeritPoint += tbGuildMiss.GetDonation;
                            //_this.Contribution += tbGuildMiss.GetDonation;
                            if (data.State == (int) AllianceMissionState.Wait)
                            {
                                itemCount = data.NowCount;
                                break;
                            }
                            data.NowCount++;
                            var tbGuild = Table.GetGuild(_this.Level);
                            if (tbGuild == null)
                            {
                                return -1;
                            }
                            if (data.NowCount >= data.MaxCount)
                            {
                                AddMoney(_this, tbGuildMiss.GetMoney);
                                data.State = (int) AllianceMissionState.Wait;
                                var refreshTime = DateTime.Now.AddSeconds(tbGuild.TaskRefresh);
                                data.NextTime = refreshTime.ToBinary();
                                TeamServerControl.tm.CreateTrigger(refreshTime, _this.AddMission);
                            }
                            itemCount = data.NowCount;

                            _this.mDBData.Donation.Add(new DBAllianceDonationDataOne
                            {
                                Name = name,
                                Time = DateTime.Now.ToBinary(),
                                ItemId = tbGuildMiss.ItemID,
                                Count = 1
                            });
                            SetDonationLogCount(_this);
                            break;
                        }
                    }
                    break;
            }
            return -1;
        }

        //捐赠log设置
        private void SetDonationLogCount(Alliance _this)
        {
            if (_this.mDBData.Donation.Count > DonationMax)
            {
                for (var i = 0; i < _this.mDBData.Donation.Count - DonationMax; i++)
                {
                    _this.mDBData.Donation.RemoveAt(0);
                }
            }
        }

        //仓库log设置
        private void SetDepotLogCount(Alliance _this)
        {
            if (_this.mDBData.DepotLog.Count > DepotLogMax)
            {
                for (var i = 0; i < _this.mDBData.DepotLog.Count - DepotLogMax; i++)
                {
                    _this.mDBData.DepotLog.RemoveAt(0);
                }
            }
        }

        public void ChangeExData(ulong characterId, Dict_int_int_Data change)
        {
            CoroutineFactory.NewCoroutine(ChangeExDataCoroutine, characterId, change).MoveNext();
        }

        public IEnumerator ChangeExDataCoroutine(Coroutine co, ulong characterId, Dict_int_int_Data change)
        {
            var msg = TeamServer.Instance.LogicAgent.SSSetExdata(characterId, change);
            yield return msg.SendAndWaitUntilDone(co);
        }

        //升级联盟等级
        public ErrorCodes UpgradeAllianceLevel(Alliance _this, ulong characterId)
        {
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(_this.ServerId);
            if (serverAlliance == null)
            {
                return ErrorCodes.ServerID;
            }
            var c = serverAlliance.GetCharacterData(characterId);
            if (c == null)
            {
                return ErrorCodes.Error_CharacterNoAlliance;
            }
            if (c.AllianceId != _this.AllianceId)
            {
                return ErrorCodes.Error_AllianceIsNotSame;
            }
            var tbGuild = Table.GetGuild(_this.Level);
            var UpgradeNeedMoney = tbGuild.ConsumeUnionMoney;
            if (UpgradeNeedMoney <= 0)
            {
                return ErrorCodes.Error_AllianceMoneyNotEnough;
            }
            if (_this.Money < UpgradeNeedMoney)
            {
                return ErrorCodes.Error_AllianceMoneyNotEnough;
            }
            _this.Money = _this.Money - UpgradeNeedMoney;
            SetLevel(_this, _this.Level + 1);
            UpgradeDepot(_this);

            Dict_int_int_Data change = new Dict_int_int_Data();
            change.Data.Add(379,_this.Level);
            foreach (var member in _this.mDBData.Members)
            {
                SendMessage(member, 0, _this.Level, _this.Money);
                ChangeExData(member,change);
            }
            return ErrorCodes.OK;
        }

        //战盟聊天消息
        public void ToChatAllianceMsgCoroutine(Alliance _this, int type, ulong modifyGuid, int leader = 0)
        {
            var dicId = 0;
            switch (type)
            {
                case 0: // 加入帮会
                    dicId = 220986;
                    break;
                case 1: //权限变更
                    if (leader == 3)
                    {
                        dicId = 220983;
                    }
                    else if (leader == 2)
                    {
                        dicId = 220985;
                    }
                    else if (leader == 1)
                    {
                        dicId = 220984;
                    }
                    break;
                case 2: //退出工会
                    dicId = 220987;
                    break;
            }
            var str = _this.GetCharacterName(modifyGuid);
            if (str != null)
            {
                var message = Utils.WrapDictionaryId(dicId, new List<string> {str});
                foreach (var member in _this.mDBData.Members)
                {
                    TeamCharacterProxy toCharacterProxy;
                    if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(member, out toCharacterProxy))
                    {
                        var chattoCharacterProxy = toCharacterProxy as TeamProxy;
                        if (chattoCharacterProxy != null)
                        {
                            chattoCharacterProxy.SyncAllianceChatMessage((int) eChatChannel.Guild, 0, string.Empty,
                                new ChatMessageContent {Content = message});
                        }
                    }
                }
            }
            else
            {
                Logger.Error("ToChatAllianceMsgCoroutine GetCharacterName is null "+modifyGuid.ToString());
            }

        }

        //战盟聊天消息
        public void ToChatAllianceMsgCoroutine(Alliance _this, int type, string name)
        {
            var dicId = 220988;
            var message = Utils.WrapDictionaryId(dicId, new List<string> {name});
            foreach (var member in _this.mDBData.Members)
            {
                TeamCharacterProxy toCharacterProxy;
                if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(member, out toCharacterProxy))
                {
                    var chattoCharacterProxy = toCharacterProxy as TeamProxy;
                    if (chattoCharacterProxy != null)
                    {
                        chattoCharacterProxy.SyncAllianceChatMessage((int) eChatChannel.Guild, 0, string.Empty,
                            new ChatMessageContent {Content = message});
                    }
                }
            }
        }

        private void ChangeNameTitle(ulong characterId, Dict_int_int_Data change)
        {
            CoroutineFactory.NewCoroutine(ChangeNameTitleCoroutine, characterId, change).MoveNext();
        }

        private IEnumerator ChangeNameTitleCoroutine(Coroutine co, ulong characterId, Dict_int_int_Data change)
        {
            var msg = TeamServer.Instance.LogicAgent.SSSetFlag(characterId, change);
            yield return msg.SendAndWaitUntilDone(co);
        }


        public void CheckLadder(Alliance _this)
        {
            int nMax = -1;
            DBAllianceOne _new = null;
            DBAllianceOne _old = null; 
            foreach (var guid in _this.mDBData.Members)
            {
                var member = _this.Dad.GetCharacterData(guid);
                if (member == null)
                {
                    continue;
                }
                if ((DateTime.Now - DateTime.FromBinary(member.LastTime)).TotalHours < 24 && member.MeritPoint>nMax)
                {
                    nMax = member.MeritPoint;
                    _new = member;
                }
                else if ((DateTime.Now - DateTime.FromBinary(member.LastTime)).TotalHours > 48 && member.Ladder == 3)
                {
                    _old = member;
                }
            }
            if (_old != null && _new != null)
            {
                _old.Ladder = 0;
                _new.Ladder = 3;
                SetLeader(_this,_new.Guid);
                SetFlag(_this);
                var tbMail = Table.GetMail(157);
                if (tbMail == null)
                    return;
                var content = string.Format(tbMail.Text, _this.mDBData.Name, _old.Name, _new.Name);
                foreach (var guid in _this.mDBData.Members)
                {//邮件 157
                    Utility.SendMail(guid, tbMail.Title, content, new Dict_int_int_Data());                    
                }


                {
                    Dict_int_int_Data tmp = new Dict_int_int_Data();
                    tmp.Data.Add(2000, 1);
                    ChangeNameTitle(_new.Guid,tmp);
                }
                {
                    Dict_int_int_Data tmp = new Dict_int_int_Data();
                    tmp.Data.Add(2003, 1);
                    ChangeNameTitle(_old.Guid, tmp);
                }
            }


        }
        #endregion

        #region Buff

        //升Buff等级
        //public ErrorCodes UpgradeBuff(Alliance _this, ulong characterId, int buffId)
        //{
        //    AllianceManager serverAlliance = ServerAllianceManager.GetAllianceByServer(_this.ServerId);
        //    if (serverAlliance == null)
        //    {
        //        return ErrorCodes.ServerID;
        //    }
        //    var c = serverAlliance.GetCharacterData(characterId);
        //    if (c == null)
        //    {
        //        return ErrorCodes.Error_CharacterNoAlliance;
        //    }
        //    if (c.AllianceId != _this.AllianceId)
        //    {
        //        return ErrorCodes.Error_AllianceIsNotSame;
        //    }
        //    int buffindex = 0;
        //    GuildBuffRecord tbGuildBuff = null;
        //    foreach (var i in _this.mDBData.Buffs)
        //    {
        //        if (i == buffId)
        //        {
        //            tbGuildBuff = Table.GetGuildBuff(buffId);
        //            break;
        //        }
        //        buffindex++;
        //    }
        //    //if (mDBData.Buffs.TryGetValue(buffId, out oldLevel))
        //    //{
        //    //    Table.ForeachGuildBuff(record =>
        //    //    {
        //    //        if (record.BuffID == buffId && record.BuffLevel == oldLevel)
        //    //        {
        //    //            tbGuildBuff = record;
        //    //            return false;
        //    //        }
        //    //        return true;
        //    //    });
        //    //}
        //    //else
        //    //{
        //    //    Table.ForeachGuildBuff(record =>
        //    //    {
        //    //        if (record.BuffID == buffId && record.BuffLevel == 0)
        //    //        {
        //    //            tbGuildBuff = record;
        //    //            return false;
        //    //        }
        //    //        return true;
        //    //    });
        //    //}
        //    if (tbGuildBuff == null)
        //    {
        //        return ErrorCodes.Error_BuffID;
        //    }
        //    if (tbGuildBuff.NextLevel == -1)
        //    {
        //        return ErrorCodes.Error_AllianceBuffMax;
        //    }
        //    //if (_this.Money >= tbGuildBuff.UpConsumeMoney)
        //    //{
        //    //    AddMoney(_this, -tbGuildBuff.UpConsumeMoney);
        //    //}
        //    //else
        //    //{
        //    //    return ErrorCodes.MoneyNotEnough;
        //    //}
        //    _this.mDBData.Buffs[buffindex] = tbGuildBuff.NextLevel;
        //    CoroutineFactory.NewCoroutine(BuffLevelChange, _this, tbGuildBuff.NextLevel).MoveNext();
        //    return ErrorCodes.OK;
        //}

        //通知玩家Buff等级变化了
        //public IEnumerator BuffLevelChange(Coroutine co,Alliance _this, int buffIds)
        //{
        //    foreach (var m in _this.mDBData.Members)
        //    {
        //        TeamCharacterProxy toCharacterProxy;
        //        if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(m, out toCharacterProxy))
        //        {
        //            var result = TeamServer.Instance.SceneAgent.SSAllianceBuffDataChange(m, buffIds);
        //            yield return result.SendAndWaitUntilDone(co);
        //        }
        //    }
        //}

        //获得Buff等级
        //public int GetBuffLevel(Alliance _this, int buffId)
        //{
        //    if (_this.mDBData.Buffs.Contains(buffId))
        //    {
        //        return Table.GetGuildBuff(buffId).BuffLevel;
        //    }
        //    return 0;
        //}

        #endregion
    }

    public class Alliance
    {
        #region 数据结构

        public AllianceManager Dad;
        public DBAllianceData mDBData;
        public bool bFlag;
        public Dictionary<ulong, InviteData> Invites = new Dictionary<ulong, InviteData>();
        private static IAlliance mImpl;

        #endregion

        #region 便捷读取

        public TeamAllianceState State
        {
            get { return (TeamAllianceState) mDBData.State; }
        }

        public void SetState(TeamAllianceState value)
        {
            mImpl.SetState(this, value);
        }

        public int AllianceId
        {
            get { return mDBData.Id; }
            set { mDBData.Id = value; }
        } //战盟ID

        public int ServerId
        {
            get { return mDBData.ServerId; }
            set { mDBData.ServerId = value; }
        } //服务器ID

        public string Name
        {
            get { return mDBData.Name; }
            set { mDBData.Name = value; }
        } //战盟名字

        public ulong Leader
        {
            get { return mDBData.Leader; }
        } //盟主

        public void SetLeader(ulong value)
        {
            mImpl.SetLeader(this, value);
        }

        public string Notice
        {
            get { return mDBData.Notice; }
        } //通知

        public void SetNotice(string value)
        {
            mImpl.SetNotice(this, value);
        }

        public int Level
        {
            get { return mDBData.Level; }
        } //战盟等级

        public void SetLevel(int value)
        {
            mImpl.SetLevel(this, value);
        }

        public int AutoAgree
        {
            get { return mDBData.AutoAgree; }
        } //自动同意

        public void SetAutoAgree(int value)
        {
            mImpl.SetAutoAgree(this, value);
        }

        public int Money
        {
            get { return mDBData.Money; }
            set { mDBData.Money = value; }
        } //战盟资金

        public BagBaseData Depot
        {
            get { return mDBData.Depot; }
            set { mDBData.Depot = value; }
        } //战盟仓库

        public Dictionary<int, int> Res
        {
            get { return mDBData.Res; }
        }

        public void SetDepot(BagBaseData value)
        {
            mImpl.SetDepot(this, value);
        }

        public void SetMoney(int value)
        {
            mImpl.SetMoney(this, value);
        }

        public DateTime CreateTime
        {
            get { return DateTime.FromBinary(mDBData.CreateTime); }
        } //创建时间
        //public int Contribution
        //{
        //    get { return mDBData.Contribution; }
        //    set
        //    {
        //        mDBData.Contribution = value;
        //    }
        //}  //总贡献
        //总战斗变化标记
        public bool TFPFlag;

        public int TotleFightPoint
        {
            get { return mDBData.TotleFightPoint; }
            set { mDBData.TotleFightPoint = value; }
        } //总战斗力

        public List<DBAllianceMissionData> Missions
        {
            get { return mDBData.Missions; }
        }

        //获得联盟数据
        public void GetAllianceData(AllianceData data)
        {
            mImpl.GetAllianceData(this, data);
        }

        //获得联盟的任务数据
        public void GetAllianceMissionData(List<AllianceMissionDataOne> data)
        {
            mImpl.GetAllianceMissionData(this, data);
        }

        //获得成员数据
        public void GetMemberData(AllianceMemberData netData, DBAllianceOne dbData, SceneSimpleData dbSceneSimple)
        {
            mImpl.GetMemberData(this, netData, dbData, dbSceneSimple);
        }

        //获得捐献记录
        public void GetAllianceDonationData(AllianceDonationData data)
        {
            mImpl.GetAllianceDonationData(this, data);
        }

        //获得仓库记录
        public void GetAllianceDepotLogData(AllianceDepotLogData data)
        {
            mImpl.GetAllianceDepotLogData(this, data);
        }

        //获得仓库数据
        public void GetAllianceDepotData(DBAllianceDepotDataOne data)
        {
            mImpl.GetAllianceDepotData(this, data);
        }

        //获得某玩家的名字
        public string GetCharacterName(ulong cId)
        {
            var t = Dad.GetCharacterData(cId);
            if (t == null)
            {
                return null;
            }
            return t.Name;
        }

        #endregion

        #region 初始化

        static Alliance()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (Alliance), typeof (AllianceDefaultImpl),
                o => { mImpl = (IAlliance) o; });
        }

        public DBAllianceData InitByBase(int serverId, string name, int aId, AllianceManager d)
        {
            return mImpl.InitByBase(this, serverId, name, aId, d);
        }

        public void InitByDB(DBAllianceData db, AllianceManager d)
        {
            mImpl.InitByDB(this, db, d);
        }

        //获得成员数量
        public int GetMemberCount()
        {
            return mImpl.GetMemberCount(this);
        }

        //获得总人数
        public int GetMemberMaxCount()
        {
            return mImpl.GetMemberMaxCount(this);
        }

        //获得总战斗力
        public int GetTotleFightPoint()
        {
            return mImpl.GetTotleFightPoint(this);
        }

        //修改战斗力的总值
        public void TotleFightPointChange(int changeValue)
        {
            mImpl.TotleFightPointChange(this, changeValue);
        }

        //设置战斗力脏标记
        public void SetTotleFightPointFlag()
        {
            mImpl.SetTotleFightPointFlag(this);
        }

        ////获得总战斗力
        //public int GetMoney()
        //{
        //    return MyRandom.Random(98765);
        //}
        //刷新任务
        public void RefreshMission()
        {
            mImpl.RefreshMission(this);
        }

        //获取一个当前没有的任务
        public GuildMissionRecord GetFreeMission(List<GuildMissionRecord> mis)
        {
            return mImpl.GetFreeMission(this, mis);
        }

        //增加任务
        public void AddMission()
        {
            mImpl.AddMission(this);
        }

        #endregion

        #region 操作

        //邀请加入
        public ErrorCodes InviteJoin(ulong fromGuid, ulong guid)
        {
            return mImpl.InviteJoin(this, fromGuid, guid);
        }

        //申请加入
        public ErrorCodes ApplyJoin(ulong guid)
        {
            return mImpl.ApplyJoin(this, guid);
        }

        //取消申请
        public ErrorCodes ApplyCancel(ulong guid)
        {
            return mImpl.ApplyCancel(this, guid);
        }

        //退出
        public ErrorCodes Leave(ulong guid, bool isLeader = false)
        {
            return mImpl.Leave(this, guid, isLeader);
        }

        //同意邀请
        public ErrorCodes AgreeInvite(ulong characterGuid)
        {
            return mImpl.AgreeInvite(this, characterGuid);
        }

        //拒绝邀请
        public ErrorCodes RefuseInvite(ulong characterGuid)
        {
            return mImpl.RefuseInvite(this, characterGuid);
        }

        //批量同意的战盟申请
        public ErrorCodes AllianceAgreeApplyList(DBAllianceOne from, List<ulong> characterGuids)
        {
            return mImpl.AllianceAgreeApplyList(this, from, characterGuids);
        }
        public ErrorCodes ClearAllianceApplyList(DBAllianceOne from, List<ulong> characterGuids)
        {
            return mImpl.ClearAllianceApplyList(this, from, characterGuids);
        }

        //同意申请
        public ErrorCodes AgreeApply(ulong characterGuid)
        {
            return mImpl.AgreeApply(this, characterGuid);
        }

        //批量拒绝的战盟申请
        public ErrorCodes AllianceRefuseApplyList(DBAllianceOne from, List<ulong> characterGuids)
        {
            return mImpl.AllianceRefuseApplyList(this, from, characterGuids);
        }

        //拒绝申请
        public ErrorCodes RefuseApply(ulong characterGuid)
        {
            return mImpl.RefuseApply(this, characterGuid);
        }

        //权限变更 
        public ErrorCodes ChangeJurisdiction(DBAllianceOne from, DBAllianceOne to, int type)
        {
            return mImpl.ChangeJurisdiction(this, from, to, type);
        }

        //添加成员
        public DBAllianceOne AddCharacter(ulong guid, int ladder = 0)
        {
            return mImpl.AddCharacter(this, guid, ladder);
        }

        //修改自动同意设置
        public ErrorCodes ChangeAllianceAutoJoin(DBAllianceOne guid, int value)
        {
            return mImpl.ChangeAllianceAutoJoin(this, guid, value);
        }

        public void SetFlag(bool b = true)
        {
            mImpl.SetFlag(this, b);
        }

        //增加战盟资金
        public void AddMoney(int value)
        {
            mImpl.AddMoney(this, value);
        }

        //捐献
        public int DonationAllianceItem(ulong guid, int type, string name, ref int itemCount)
        {
            return mImpl.DonationAllianceItem(this, guid, type, name, ref itemCount);
        }

        //战盟仓库捐献
        public ErrorCodes DepotDonateEquip(ulong characterId, string name, ItemBaseData item)
        {
            return mImpl.DepotDonateEquip(this, characterId, name, item);
        }

        //战盟仓库取出
        public ErrorCodes DepotTakeOutEquip(ulong characterId, string name, int bagIndex, int itemId, out ItemBaseData item)
        {
            return mImpl.DepotTakeOutEquip(this, characterId, name, bagIndex, itemId, out item);
        }
        
        //战盟仓库单个清理
        public ErrorCodes DepotItemRemove(ulong characterId, string name, int bagIndex, int itemId)
        {
            return mImpl.DepotItemRemove(this, characterId, name, bagIndex, itemId);
        }

        //战盟仓库清理
        public ErrorCodes DepotClearUp(ulong characterId, string name, ClearUpInfo info)
        {
            return mImpl.DepotClearUp(this, characterId, name, info);
        }

        public ErrorCodes DepotArrange(string name)
        {
            return mImpl.DepotArrange(this, name);
        }
        
        //升级联盟等级
        public ErrorCodes UpgradeAllianceLevel(ulong characterId)
        {
            return mImpl.UpgradeAllianceLevel(this, characterId);
        }

        public void AddRes(Dictionary<int, int> res)
        {
            mImpl.AddRes(this, res);
        }

        public void DealRes()
        {
            mImpl.DealRes(this);
        }

        public void CheckLadder()
        {
            mImpl.CheckLadder(this);
        }
        #endregion

        #region Buff

        //升Buff等级
        //public ErrorCodes UpgradeBuff(ulong characterId, int buffId)
        //{
        //    return mImpl.UpgradeBuff(this, characterId, buffId);
        //}

        ////获得Buff等级
        //public int GetBuffLevel(int buffId)
        //{
        //    return mImpl.GetBuffLevel(this, buffId);
        //}

        #endregion
    }

    public interface IAllianceManager
    {
        void AddCharacterData(AllianceManager _this, int serverId, ulong guid, DBAllianceOne dbone);
        void BidOver(AllianceManager _this);
        void CancelApplyJoin(AllianceManager _this, int serverId, int allianceId, ulong guid);
        ErrorCodes CheckApplyJoin(AllianceManager _this, int serverId, int allianceId, ulong guid);
        ErrorCodes CreateAlliance(AllianceManager _this, int serverId, ulong leaderId, string name, int aId);
        ErrorCodes DeleteAlliance(AllianceManager _this, int serverId, int aId);
        IEnumerator FlushAll(Coroutine coroutine, AllianceManager _this);
        Alliance GetAlliance(AllianceManager _this, int aId);
        DBAllianceOne GetCharacterData(AllianceManager _this, ulong guid);
        int GetCharacterLadder(AllianceManager _this, ulong guid);
        string GetDbName(AllianceManager _this, int serverId);
        DBServerAllianceData GetServerData(AllianceManager _this, int serverId);
        void Init(AllianceManager _this, int serverId, int newServerId);
        void RemoveCharacterData(AllianceManager _this, ulong guid);
        void SetFlag(AllianceManager _this, int serverId);
        void SuccessApplyJoin(AllianceManager _this, int serverId, int allianceId, ulong guid);
        void SuccessJoin(AllianceManager _this, int serverId, int allianceId, ulong guid);

        void ClearPlayerApplyLog(AllianceManager _this, int serverId, ulong guid, int allianceId);
    }

    public class AllianceManagerDefaultImpl //单个服务器的帮派信息（支持合服后，多服务器对应同一个类）
        : IAllianceManager
    {
        #region 服务器方法

        public DBServerAllianceData GetServerData(AllianceManager _this, int serverId)
        {
            DBServerAllianceData find;
            if (_this.mDBData.TryGetValue(serverId, out find))
            {
                return find;
            }
            return null;
        }

        #endregion

        #region 攻城战相关

        public void BidOver(AllianceManager _this)
        {
            var tbMail1 = Table.GetMail(131);
            var tbMail2 = Table.GetMail(132);
            var tbMail3 = Table.GetMail(138);
            var tbMail4 = Table.GetMail(139);
            var mailItems = new Dict_int_int_Data();

            var bidDatas = new Dictionary<int, int>();
            foreach (var data in _this.mDBData.Values)
            {
                bidDatas.AddRange(data.BidDatas);
                if(data.BidDatas.Count>0)
                    data.Challengers.Clear();
                data.BidDatas.Clear();
                _this.SetFlag(data.ServerId);
            }
            
            DBServerAllianceData serverData;
            if (!_this.mDBData.TryGetValue(_this.mNewServerId, out serverData))
            {
                return;
            }
            var order = bidDatas.OrderByDescending(o => o.Value);
            var e = order.GetEnumerator();
            for (var i = 0; e.MoveNext(); i++)
            {
                var cur = e.Current;
                var alliance = ServerAllianceManager.GetAllianceById(cur.Key);
                if (alliance == null)
                {
                    Logger.Error("In BidOver().alliance == null! id = {0}", cur.Key);
                    continue;
                }
                MailRecord leaderMail;
                MailRecord memberMail;
                if (i < 2)
                {
                    serverData.Challengers.Add(cur.Key);
                    leaderMail = tbMail1;
                    memberMail = tbMail3;
                }
                else
                {
                    //返还竞标资金
                    alliance.AddMoney(cur.Value);
                    leaderMail = tbMail2;
                    memberMail = tbMail4;
                }
                //给会长发邮件
                var content = string.Format(leaderMail.Text, cur.Value, i + 1);
                Utility.SendMail(alliance.Leader, leaderMail.Title, content, mailItems);
                //给会员发邮件
                content = string.Format(memberMail.Text, i + 1);
                foreach (var id in alliance.mDBData.Members)
                {
                    if (alliance.Leader == id)
                    {
                        continue;
                    }
                    Utility.SendMail(id, memberMail.Title, content, mailItems);
                }
            }
        }

        #endregion

        #region 基础结构

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //初始化战盟
        public void Init(AllianceManager _this, int serverId, int newServerId)
        {
            _this.mNewServerId = newServerId;
            CoroutineFactory.NewCoroutine(ReadDb, _this, serverId).MoveNext();
        }

        //存储战盟
        public IEnumerator FlushAll(Coroutine coroutine, AllianceManager _this)
        {
            foreach (var oneServer in _this.mDBData)
            {
                if (_this.mDBFlag.GetValue(oneServer.Key))
                {
                    var co = CoroutineFactory.NewSubroutine(SaveDb, coroutine, _this, oneServer.Value);
                    if (co.MoveNext())
                    {
                        yield return co;
                    }
                }
                _this.mDBFlag[oneServer.Key] = false;
            }
        }

        //DB名称获取
        public string GetDbName(AllianceManager _this, int serverId)
        {
            return string.Format("Alliance_{0}", serverId);
        }

        //存储数据
        private IEnumerator SaveDb(Coroutine coroutine, AllianceManager _this, DBServerAllianceData dbData)
        {
            if (dbData != null)
            {
                PlayerLog.WriteLog((int) LogType.SaveAllianceData,
                    "--------------------SaveAllianceData--------------------s={0}", dbData.ServerId);
                var ret = TeamServer.Instance.DB.Set(coroutine, DataCategory.TeamAlliance,
                    GetDbName(_this, dbData.ServerId), dbData);
                yield return ret;
            }
        }

        //读取数据
        private IEnumerator ReadDb(Coroutine coroutine, AllianceManager _this, int serverId)
        {
            var tasks = TeamServer.Instance.DB.Get<DBServerAllianceData>(coroutine, DataCategory.TeamAlliance,
                GetDbName(_this, serverId));
            yield return tasks;
            do
            {
                if (tasks.Data == null)
                {
                    var data = new DBServerAllianceData();
                    data.ServerId = serverId;
                    data.Occupant = -1;
                    _this.mDBData[serverId] = data;
                    _this.mDBFlag[serverId] = false;
                    break;
                }
                _this.mDBData[serverId] = tasks.Data;
                _this.mDBFlag[serverId] = false;
                var delList = new List<int>();
                foreach (var dbAllianceData in tasks.Data.Alliances)
                {
                    var temp = new Alliance();
                    temp.InitByDB(dbAllianceData.Value, _this);
                    ServerAllianceManager.PushName(dbAllianceData.Value.Name, dbAllianceData.Value.Id, temp);
                    _this.Alliances[dbAllianceData.Key] = temp;
                    if (temp.State == TeamAllianceState.NewCreate)
                    {
                        delList.Add(temp.AllianceId);
                    }
                }
                foreach (var dbAllianceOne in tasks.Data.Members)
                {
                    _this.mCharacters[dbAllianceOne.Key] = dbAllianceOne.Value;
                }
                foreach (var i in delList)
                {
                    ServerAllianceManager.DeleteAlliance(i);
                }
            } while (false);
            
            AllianceWarManager.Init(serverId);
          
        }

        //设置某个数据为脏
        public void SetFlag(AllianceManager _this, int serverId)
        {
            _this.mDBFlag[serverId] = true;
        }

        #endregion

        #region 角色方法

        //获取一个玩家联盟内的数据
        public DBAllianceOne GetCharacterData(AllianceManager _this, ulong guid)
        {
            DBAllianceOne dbOne;
            if (_this.mCharacters.TryGetValue(guid, out dbOne))
            {
                return dbOne;
            }
            return null;
        }

        //添加一个玩家数据
        public void AddCharacterData(AllianceManager _this, int serverId, ulong guid, DBAllianceOne dbone)
        {
            _this.mCharacters[guid] = dbone;
            var server = GetServerData(_this, serverId);
            if (server != null)
            {
                server.Members[guid] = dbone;
            }
        }


        //移除一个玩家数据
        public void RemoveCharacterData(AllianceManager _this, ulong guid)
        {
            DBAllianceOne one;
            if (_this.mCharacters.TryGetValue(guid, out one))
            {
                _this.mCharacters.Remove(guid);
                var server = GetServerData(_this, one.ServerId);
                if (server == null)
                {
                    return;
                }
                server.Members.Remove(guid);
            }
        }

        //获取一个玩家的职位
        public int GetCharacterLadder(AllianceManager _this, ulong guid)
        {
            var c = GetCharacterData(_this, guid);
            if (c == null)
            {
                return -1;
            }
            return c.Ladder;
        }

        //检查申请加入了某个战盟
        public ErrorCodes CheckApplyJoin(AllianceManager _this, int serverId, int allianceId, ulong guid)
        {
            var server = GetServerData(_this, serverId);
            if (server == null)
            {
                return ErrorCodes.ServerID;
            }
            DBAllianceApplyOne applyOne;
            if (!server.Applys.TryGetValue(guid, out applyOne))
            {
                return ErrorCodes.OK;
            }
            if (applyOne.Applys.Count >= 3)
            {
                return ErrorCodes.Error_AllianceApplyIsFull;
            }
            if (applyOne.Applys.Contains(allianceId))
            {
                return ErrorCodes.Error_AlreadyApply;
            }
            return ErrorCodes.OK;
        }


        //成功申请加入了某个战盟
        public void SuccessApplyJoin(AllianceManager _this, int serverId, int allianceId, ulong guid)
        {
            var server = GetServerData(_this, serverId);
            if (server == null)
            {
                return;
            }
            DBAllianceApplyOne applyOne;
            if (!server.Applys.TryGetValue(guid, out applyOne))
            {
                applyOne = new DBAllianceApplyOne();
                applyOne.Guid = guid;
            }
            server.Applys[guid] = applyOne;
            applyOne.Applys.Add(allianceId);
        }

        //取消申请加入了某个战盟
        public void CancelApplyJoin(AllianceManager _this, int serverId, int allianceId, ulong guid)
        {
            var server = GetServerData(_this, serverId);
            if (server == null)
            {
                return;
            }
            _this.ClearPlayerApplyLog(serverId, guid, allianceId);
        }

        //成功加入了某个战盟
        public void SuccessJoin(AllianceManager _this, int serverId, int allianceId, ulong guid)
        {
            var server = GetServerData(_this, serverId);
            if (server == null)
            {
                return;
            }
            _this.ClearPlayerApplyLog(serverId, guid);
            //DBAllianceApplyOne applyOne;
            //if (server.Applys.TryGetValue(guid, out applyOne))
            //{
            //    foreach (var aId in applyOne.Applys)
            //    {
            //        var temp = ServerAllianceManager.GetAllianceById(aId);
            //        temp.mDBData.Applys.Remove(guid);
            //    }
            //    server.Applys.Remove(guid);
            //    applyOne.Applys.Clear();
            //}
        }
        public void ClearPlayerApplyLog(AllianceManager _this, int serverId, ulong guid, int allianceId)
        {
            var Server = _this.GetServerData(serverId);
            if (Server == null)
            {
                return;
            }

            DBAllianceApplyOne tmp;
            if (false == Server.Applys.TryGetValue(guid, out tmp))
                return;
            if(allianceId>0)
            {
                if(Server.Applys.ContainsKey(guid))
                {
                    var m = Server.Applys[guid];
                    m.Applys.Remove(allianceId);
                }
                var AllianceData = ServerAllianceManager.GetAllianceById(allianceId);
                if (AllianceData != null)
                {
                     AllianceData.mDBData.Applys.Remove(guid);
                }
            }
            else
            {
                foreach (var Id in tmp.Applys)
                {
                    var AllianceData = ServerAllianceManager.GetAllianceById(Id);
                    if (null != AllianceData)
                    {
                        if (AllianceData.mDBData.Applys.Contains(guid))
                        {//删除战盟的
                            AllianceData.mDBData.Applys.Remove(guid);
                        }                        
                    }
                }
                Server.Applys.Remove(guid);
            }
        }
        #endregion

        #region 战盟方法

        public ErrorCodes CreateAlliance(AllianceManager _this, int serverId, ulong leaderId, string name, int aId)
        {
            var server = GetServerData(_this, serverId);
            if (server == null)
            {
                return ErrorCodes.ServerID;
            }
            if (_this.mCharacters.ContainsKey(leaderId))
            {
                return ErrorCodes.Error_CharacterHaveAlliance;
            }
            var temp = new Alliance();
            var db = temp.InitByBase(serverId, name, aId, _this);
            server.Alliances[aId] = db;
            var dbAllianceOne = temp.AddCharacter(leaderId, ServerAllianceManager.MaxLadder);
            temp.SetLeader(leaderId);
            temp.ChangeAllianceAutoJoin(dbAllianceOne, 1);
            _this.Alliances[aId] = temp;
            temp.SetFlag();
            ServerAllianceManager.PushName(name, aId, temp);

            return ErrorCodes.OK;
        }

        public ErrorCodes DeleteAlliance(AllianceManager _this, int serverId, int aId)
        {
            var server = GetServerData(_this, serverId);
            if (server == null)
            {
                return ErrorCodes.ServerID;
            }
            DBAllianceData dbAlliance;
            if (!server.Alliances.TryGetValue(aId, out dbAlliance))
            {
                return ErrorCodes.Error_AllianceNotFind;
            }
            _this.Alliances.Remove(aId);
            server.Alliances.Remove(aId);
            if (server.Occupant == aId)
            {
                server.Occupant = 0;

                //通知全服，城主变了
                var data = new AllianceWarOccupantData();
                data.OccupantId = 0;
                data.OccupantName = string.Empty;
                var id = SceneExtension.GetServerLogicId(server.ServerId);
                TeamServer.Instance.TeamAgent.NotifyAllianceWarOccupantData((uint) id, data);
            }
            else if (server.BidDatas.ContainsKey(aId))
            {
                server.BidDatas.Remove(aId);
            }
            foreach (var member in dbAlliance.Members)
            {
                RemoveCharacterData(_this, member);
                var gmMsg = TeamServer.Instance.LogicAgent.GMDeleteMessage(member,member);
                gmMsg.SendAndWaitUntilDone(null);
            }
            ServerAllianceManager.PopName(aId);
            return ErrorCodes.OK;
        }

        public Alliance GetAlliance(AllianceManager _this, int aId)
        {
            Alliance a;
            if (_this.Alliances.TryGetValue(aId, out a))
            {
                return a;
            }
            return null;
        }

        #endregion
    }

    public class AllianceManager //单个服务器的帮派信息（支持合服后，多服务器对应同一个类）
    {
        #region 攻城战相关

        public void BidOver()
        {
            mImpl.BidOver(this);
        }

        #endregion

        #region 服务器方法

        public DBServerAllianceData GetServerData(int serverId)
        {
            return mImpl.GetServerData(this, serverId);
        }

        #endregion

        #region 基础结构

        public int mNewServerId;

        public Dictionary<int, Alliance> Alliances = new Dictionary<int, Alliance>();
            //key=allianceId value = Alliance   id到具体内容

        public Dictionary<int, DBServerAllianceData> mDBData = new Dictionary<int, DBServerAllianceData>();
            //每个服务器的战盟DB数据

        public Dictionary<int, bool> mDBFlag = new Dictionary<int, bool>(); //每个服务器的战盟的脏标记
        public Dictionary<ulong, DBAllianceOne> mCharacters = new Dictionary<ulong, DBAllianceOne>(); //每个服务器的战盟玩家数据
        private static IAllianceManager mImpl;
        public Dictionary<ulong,long> mExitTimer = new Dictionary<ulong, long>(); 
        static AllianceManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (AllianceManager),
                typeof (AllianceManagerDefaultImpl),
                o => { mImpl = (IAllianceManager) o; });
        }

        //初始化战盟
        public void Init(int serverId, int newServerId)
        {
            mImpl.Init(this, serverId, newServerId);
        }

        //存储战盟
        public IEnumerator FlushAll(Coroutine coroutine)
        {
            return mImpl.FlushAll(coroutine, this);
        }

        //DB名称获取
        public string GetDbName(int serverId)
        {
            return mImpl.GetDbName(this, serverId);
        }

        //设置某个数据为脏
        public void SetFlag(int serverId)
        {
            mImpl.SetFlag(this, serverId);
        }

        #endregion

        #region 角色方法

        //获取一个玩家联盟内的数据
        public DBAllianceOne GetCharacterData(ulong guid)
        {
            return mImpl.GetCharacterData(this, guid);
        }

        //添加一个玩家数据
        public void AddCharacterData(int serverId, ulong guid, DBAllianceOne dbone)
        {
            mImpl.AddCharacterData(this, serverId, guid, dbone);
        }

        //移除一个玩家数据
        public void RemoveCharacterData(ulong guid)
        {
            mImpl.RemoveCharacterData(this, guid);
        }

        //获取一个玩家的职位
        public int GetCharacterLadder(ulong guid)
        {
            return mImpl.GetCharacterLadder(this, guid);
        }

        //检查申请加入了某个战盟
        public ErrorCodes CheckApplyJoin(int serverId, int allianceId, ulong guid)
        {
            return mImpl.CheckApplyJoin(this, serverId, allianceId, guid);
        }

        //成功申请加入了某个战盟
        public void SuccessApplyJoin(int serverId, int allianceId, ulong guid)
        {
            mImpl.SuccessApplyJoin(this, serverId, allianceId, guid);
        }

        //取消申请加入了某个战盟
        public void CancelApplyJoin(int serverId, int allianceId, ulong guid)
        {
            mImpl.CancelApplyJoin(this, serverId, allianceId, guid);
        }

        //成功加入了某个战盟
        public void SuccessJoin(int serverId, int allianceId, ulong guid)
        {
            mImpl.SuccessJoin(this, serverId, allianceId, guid);
        }
        public void ClearPlayerApplyLog(int serverId, ulong guid,int allianceId = -1)
        {
            mImpl.ClearPlayerApplyLog(this, serverId, guid, allianceId);
        }
        #endregion

        #region 战盟方法

        public ErrorCodes CreateAlliance(int serverId, ulong leaderId, string name, int aId)
        {
            return mImpl.CreateAlliance(this, serverId, leaderId, name, aId);
        }

        public ErrorCodes DeleteAlliance(int serverId, int aId)
        {
            return mImpl.DeleteAlliance(this, serverId, aId);
        }

        public Alliance GetAlliance(int aId)
        {
            return mImpl.GetAlliance(this, aId);
        }

        #endregion
    }

    public interface IServerAllianceManager
    {
        ErrorCodes ApplyCancel(int allianceId, ulong characterGuid);
        ErrorCodes ApplyCharacter(int allianceId, ulong characterGuid);
        ErrorCodes ApplyResult(int allianceId, ulong characterGuid, int result);
        void CheckAllianceId(int aId);
        ErrorCodes CreateAlliance(int serverId, ulong leaderId, string name, ref int allianceId);
        void CreateAllianceFaild(string name);
        void CreateAllianceSuccess(string name);
        ErrorCodes CreateNewAlliance(int serverId, ulong leaderId, string name, ref int allianceId);
        ErrorCodes DeleteAlliance(int allianceId);
        Alliance GetAllianceByCharacterId(ulong characterId);
        Alliance GetAllianceById(int allianceId);
        Alliance GetAllianceByName(string name);
        AllianceManager GetAllianceByServer(int serverId);
        string GetAllianceName(int allianceId);
        List<GuildMissionRecord> GetMission(int level);
        int GetNextAllianceId();
        void Init();
        ErrorCodes InviteCharacter(ulong fromGuid, int allianceId, ulong characterGuid);
        ErrorCodes InviteResult(int allianceId, ulong characterGuid, int result);
        ErrorCodes LeaveCharacter(int serverId, ulong characterGuid);
        void OnLost(ulong guid);
        void PopName(int allianceId);
        void PushMission(GuildMissionRecord record);
        void PushName(string AllianceName, int AllianceId, Alliance alliance);
        ErrorCodes SendMessage(ulong toCharacterId, int type, string name1, int allianceId, string name2);
        void Updata();
        void WeekRefresh();
        void OnAllianceAddRes(int allianceId, Dictionary<int, int> res);
        void OnDay();
    }

    public class ServerAllianceManagerDefaultImpl : IServerAllianceManager
    {
        #region 基础结构

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //获取下一个Id
        public int GetNextAllianceId()
        {
            ServerAllianceManager.mIdFlag = true;
            return ServerAllianceManager.NextAllianceId++;
        }

        //防止ID错误,有这种可能（比如数据库回档，战盟数据存储了，但是下一个ID没有存储）
        public void CheckAllianceId(int aId)
        {
            if (aId >= ServerAllianceManager.NextAllianceId)
            {
                ServerAllianceManager.NextAllianceId = aId + 1;
            }
        }

        public void PushMission(GuildMissionRecord record)
        {
            for (var i = record.MinLevel; i <= record.MaxLevel; i++)
            {
                List<GuildMissionRecord> mis;
                if (!ServerAllianceManager.AllianceMissions.TryGetValue(i, out mis))
                {
                    mis = new List<GuildMissionRecord>();
                    ServerAllianceManager.AllianceMissions[i] = mis;
                }
                mis.Add(record);
            }
        }

        private void InitTable()
        {
            Table.ForeachGuildMission(record =>
            {
             //   PushMission(record);
                return true;
            });
        }

        public List<GuildMissionRecord> GetMission(int level)
        {
            List<GuildMissionRecord> mis;
            if (ServerAllianceManager.AllianceMissions.TryGetValue(level, out mis))
            {
                return mis;
            }
            return null;
        }

        //初始化
        public void Init()
        {
            InitTable();
            CoroutineFactory.NewCoroutine(GetDb).MoveNext();

            Table.ForeachServerName(record =>
            {
                ServerAllianceManager.ServerToAlliance.Add(record.Id, record.LogicID);
                return true;
            });
            foreach (var i in ServerAllianceManager.ServerToAlliance)
            {
                List<int> temp;
                if (!ServerAllianceManager.AllianceToServer.TryGetValue(i.Value, out temp))
                {
                    temp = new List<int>();
                    ServerAllianceManager.AllianceToServer[i.Value] = temp;
                    ServerAllianceManager.Servers[i.Value] = new AllianceManager();
                }
                temp.Add(i.Key);
                var Rank = ServerAllianceManager.Servers[i.Value];
                Rank.Init(i.Key, i.Value);
            }
            TeamServerControl.tm.CreateTrigger(DateTime.Now.AddSeconds(30), Updata, 30000); //30秒一次
            TeamServerControl.tm.CreateTrigger(DateTime.Now.AddDays(1).Date.AddMinutes(1), OnDay, 3600*24*1000);
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
        }
        private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v == null)
                return;
            if (v.tableName == "ServerName")
            {
                Table.ForeachServerName(record =>
                {
                    var id = record.LogicID;
                    if (ServerAllianceManager.ServerToAlliance.ContainsKey(record.Id) == false)
                    {
                        ServerAllianceManager.ServerToAlliance.Add(record.Id, record.LogicID);
                        List<int> temp;
                        if (!ServerAllianceManager.AllianceToServer.TryGetValue(record.LogicID, out temp))
                        {
                            temp = new List<int>();
                            ServerAllianceManager.AllianceToServer[record.LogicID] = temp;
                            ServerAllianceManager.Servers[record.LogicID] = new AllianceManager();
                            temp.Add(record.Id);
                            var rank = ServerAllianceManager.Servers[record.LogicID];
                            rank.Init(record.Id, record.LogicID);
                        }
                    }
                    return true;
                });
            }
        }
        //存储
        private IEnumerator RefreshAll(Coroutine coroutine)
        {
            if (ServerAllianceManager.mIdFlag)
            {
                ServerAllianceManager.mIdFlag = false;
                var codb = CoroutineFactory.NewSubroutine(SaveDb, coroutine);
                if (codb.MoveNext())
                {
                    yield return codb;
                }
            }
            foreach (var alliance in ServerAllianceManager.Servers)
            {
                var co = CoroutineFactory.NewSubroutine(alliance.Value.FlushAll, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        //存储ID 
        private IEnumerator SaveDb(Coroutine coroutine)
        {
            PlayerLog.WriteLog((int) LogType.SaveAllianceData,
                "--------------------SaveAllianceNextId--------------------id={0}", ServerAllianceManager.NextAllianceId);
            var ret = TeamServer.Instance.DB.Set(coroutine, DataCategory.Team, "AllianceNextId",
                ServerAllianceManager.NextAllianceId.ToDbInt());
            yield return ret;
        }

        //读取
        private IEnumerator GetDb(Coroutine coroutine)
        {
            var tasks = TeamServer.Instance.DB.Get<DBInt>(coroutine, DataCategory.Team, "AllianceNextId");
            yield return tasks;
            if (tasks.Data == null)
            {
                ServerAllianceManager.NextAllianceId = 1;
            }
            else
            {
                ServerAllianceManager.NextAllianceId = tasks.Data.Value;
            }
            PlayerLog.WriteLog((int) LogType.GetAllianceData,
                "--------------------GetAllianceNextId--------------------id={0}", ServerAllianceManager.NextAllianceId);
        }

        //心跳
        public void Updata()
        {
            CoroutineFactory.NewCoroutine(RefreshAll).MoveNext();
        }

        #endregion

        #region 服务器接口

        //靠服务器Id 获得服务器所在的数据管理器
        public AllianceManager GetAllianceByServer(int serverId)
        {
            int amId;
            if (!ServerAllianceManager.ServerToAlliance.TryGetValue(serverId, out amId))
            {
                return null;
            }
            AllianceManager find;
            if (!ServerAllianceManager.Servers.TryGetValue(amId, out find))
            {
                return null;
            }
            return find;
        }

        public void OnLost(ulong guid)
        {
            foreach (var manager in ServerAllianceManager.Servers)
            {
                var c = manager.Value.GetCharacterData(guid);
                if (c != null)
                {
                    c.LastTime = DateTime.Now.ToBinary();
                    return;
                }
            }
        }

        #endregion

        #region 名字相关

        //缓存名字到战盟ID的映射
        public void PushName(string AllianceName, int AllianceId, Alliance alliance)
        {
            ServerAllianceManager.Alliances[AllianceId] = alliance;
            int findId;
            if (!ServerAllianceManager.AllianceNames.TryGetValue(AllianceName, out findId))
            {
                ServerAllianceManager.AllianceNames[AllianceName] = AllianceId;
                return;
            }
            Logger.Warn("same allianceName={0},New={1},Old={2}", AllianceName, AllianceId, findId);
        }

        //删除缓存的名字
        public void PopName(int allianceId)
        {
            var a = GetAllianceById(allianceId);
            if (a == null)
            {
                return;
            }
            ServerAllianceManager.Alliances.Remove(allianceId);
            ServerAllianceManager.AllianceNames.Remove(a.Name);
        }

        //获得某个战盟的名字
        public string GetAllianceName(int allianceId)
        {
            var a = GetAllianceById(allianceId);
            if (a == null)
            {
                return "";
            }
            return a.Name;
        }

        #endregion

        #region 增删改查

        //新增创建
        public ErrorCodes CreateAlliance(int serverId, ulong leaderId, string name, ref int allianceId)
        {
            var server = GetAllianceByServer(serverId);
            if (server == null)
            {
                return ErrorCodes.ServerID;
            }
            var nextId = GetNextAllianceId();
            allianceId = nextId;
            return server.CreateAlliance(serverId, leaderId, name, nextId);
        }

        //新增创建:成功
        public void CreateAllianceSuccess(string name)
        {
            var a = GetAllianceByName(name);
            if (a == null)
            {
                return;
            }
            if (a.State == TeamAllianceState.NewCreate)
            {
                a.SetState(TeamAllianceState.Already);
            }
        }

        //新增创建:失败
        public void CreateAllianceFaild(string name)
        {
            var a = GetAllianceByName(name);
            if (a == null)
            {
                return;
            }
            DeleteAlliance(a.AllianceId);
        }

        //删除
        public ErrorCodes DeleteAlliance(int allianceId)
        {
            var a = GetAllianceById(allianceId);
            if (a == null)
            {
                return ErrorCodes.Error_CharacterNoAlliance;
            }

            var server = GetAllianceByServer(a.ServerId);
            if (server == null)
            {
                return ErrorCodes.Error_CharacterNoAlliance;
            }
            server.DeleteAlliance(a.ServerId, a.AllianceId);
            a.SetFlag();

            return ErrorCodes.OK;
        }

        //查询
        public Alliance GetAllianceById(int allianceId)
        {
            Alliance find;
            if (ServerAllianceManager.Alliances.TryGetValue(allianceId, out find))
            {
                return find;
            }
            return null;
        }

        //查询
        public Alliance GetAllianceByName(string name)
        {
            int allianceId;
            if (!ServerAllianceManager.AllianceNames.TryGetValue(name, out allianceId))
            {
                return null;
            }
            return GetAllianceById(allianceId);
        }

        //获得某个玩家所在的战盟
        public Alliance GetAllianceByCharacterId(ulong characterId)
        {
            foreach (var manager in ServerAllianceManager.Servers)
            {
                var c = manager.Value.GetCharacterData(characterId);
                if (c != null)
                {
                    return GetAllianceById(c.AllianceId);
                }
            }
            return null;
        }

        #endregion

        #region 操作

        //创建新战盟
        public ErrorCodes CreateNewAlliance(int serverId, ulong leaderId, string name, ref int allianceId)
        {
            int aId;
            if (ServerAllianceManager.AllianceNames.TryGetValue(name, out aId))
            {
                var a = GetAllianceById(aId);
                if (a.State != TeamAllianceState.WillDisband)
                {
                    return ErrorCodes.Error_AllianceNameSame;
                }
            }
            CreateAlliance(serverId, leaderId, name, ref allianceId);
            return ErrorCodes.OK;
        }

        //邀请加入
        public ErrorCodes InviteCharacter(ulong fromGuid, int allianceId, ulong characterGuid)
        {
            var a = GetAllianceById(allianceId);
            if (a == null)
            {
                return ErrorCodes.Error_AllianceNotFind;
            }
            var result = a.InviteJoin(fromGuid, characterGuid);
            return result;
        }

        //邀请回复结果
        public ErrorCodes InviteResult(int allianceId, ulong characterGuid, int result)
        {
            var a = GetAllianceById(allianceId);
            if (a == null)
            {
                return ErrorCodes.Error_AllianceNotFind;
            }
            if (result == 1)
            {
                return a.AgreeInvite(characterGuid);
            }
            return a.RefuseInvite(characterGuid);
        }

        //申请加入
        public ErrorCodes ApplyCharacter(int allianceId, ulong characterGuid)
        {
            var a = GetAllianceById(allianceId);
            if (a == null)
            {
                return ErrorCodes.Error_AllianceNotFind;
            }
            return a.ApplyJoin(characterGuid);
        }

        //取消申请
        public ErrorCodes ApplyCancel(int allianceId, ulong characterGuid)
        {
            var a = GetAllianceById(allianceId);
            if (a == null)
            {
                return ErrorCodes.Error_AllianceNotFind;
            }
            return a.ApplyCancel(characterGuid);
        }

        //退出战盟
        public ErrorCodes LeaveCharacter(int serverId, ulong characterGuid)
        {
            var server = GetAllianceByServer(serverId);
            if (server == null)
            {
                return ErrorCodes.ServerID;
            }
            var one = server.GetCharacterData(characterGuid);
            if (one == null)
            {
                return ErrorCodes.Error_CharacterNoAlliance;
            }
            var a = GetAllianceById(one.AllianceId);
            if (a == null)
            {
                return ErrorCodes.Error_AllianceNotFind;
            }
            var result = ErrorCodes.OK;
            if (one.Ladder == ServerAllianceManager.MaxLadder && a.GetMemberCount() != 1)
            {
                result = a.Leave(characterGuid, true);
            }
            else
            {
                result = a.Leave(characterGuid);
            }
            if (a.State == TeamAllianceState.WillDisband)
            {
                DeleteAlliance(a.AllianceId);
            }

            if (a.Dad.mExitTimer.ContainsKey(characterGuid))
            {
                a.Dad.mExitTimer[characterGuid] = DateTime.Now.ToBinary();
            }
            else
            {
                a.Dad.mExitTimer.Add(characterGuid, DateTime.Now.ToBinary());
            }
            return result;
        }

        //申请回复结果
        public ErrorCodes ApplyResult(int allianceId, ulong characterGuid, int result)
        {
            var a = GetAllianceById(allianceId);
            if (a == null)
            {
                return ErrorCodes.Error_AllianceNotFind;
            }

            if (result == 1)
            {
                //同意
                return a.AgreeApply(characterGuid);
            }
            //拒绝
            return a.RefuseApply(characterGuid);
        }

        //通知
        public ErrorCodes SendMessage(ulong toCharacterId, int type, string name1, int allianceId, string name2)
        {
            PlayerLog.WriteLog((int) LogType.SyncAllianceMessage,
                "SC->TeamSyncAllianceMessage toCharacterId={0}, type={1}, name1={2}, allianceId={3},name2={4}",
                toCharacterId, type, name1, allianceId, name2);
            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(toCharacterId, out toCharacterProxy))
            {
                var ChattoCharacterProxy = toCharacterProxy as TeamProxy;
                ChattoCharacterProxy.TeamSyncAllianceMessage(type, name1, allianceId, name2);
                return ErrorCodes.OK;
            }
            return ErrorCodes.Error_CharacterOutLine;
        }

        //时间触发器（战盟修改）
        public void WeekRefresh()
        {
            List<Alliance> dels = new List<Alliance>();
            foreach (var alliance in ServerAllianceManager.Alliances)
            {
                var a = alliance.Value;
                if (alliance.Key == 89)
                {
                    Logger.Info("WeekRefresh");
                }
                if (a.State != TeamAllianceState.Already)
                {
                    continue;
                }
                var tbGuild = Table.GetGuild(a.Level);
                if (tbGuild == null)
                {
                    Logger.Warn("WeekRefresh alliance level is overflow,id={0},level={1}", a.AllianceId, a.Level);
                    continue;
                }
                if ((DateTime.Now - a.CreateTime).TotalDays < 7)
                {
                    continue;
                }
                var needMoney = tbGuild.MaintainMoney;
                if (needMoney > a.Money)
                {
                    a.SetMoney(0);
                    a.SetState(TeamAllianceState.WillDisband);
                    dels.Add(a);
                }
                else
                {
                    a.AddMoney(-needMoney);
                    //todo 是否需要提醒玩家，资金不够下次的了
                    //if (needMoney > a.Money)
                    //{

                    //}
                }
            }
            foreach (var del in dels)
            {
                DeleteAlliance(del.AllianceId);
            }
        }
        public void OnAllianceAddRes(int allianceId, Dictionary<int, int> res)
        {
            var al = GetAllianceById(allianceId);
            if (al == null)
            {
                return ;
            }
            al.AddRes(res) ;
        }

        public void OnDay()
        {
            //不在线盟主转换
            foreach (var mgr in ServerAllianceManager.Servers)
            {
                foreach (var v in mgr.Value.mCharacters)
                {
                    var member = v.Value;
                    if (member.Ladder == 3 && (DateTime.Now - DateTime.FromBinary(member.LastTime)).TotalHours > 48)
                    {
                        var al = ServerAllianceManager.GetAllianceById(member.AllianceId);
                        if (al != null)
                        {
                            al.CheckLadder();
                        }
                    }
                }
            }





            //矿资源分配
            foreach (var v in ServerAllianceManager.Alliances)
            {
                var al = v.Value;
                al.DealRes();
            }                
            
        }
        #endregion
    }

    public static class ServerAllianceManager
    {
        #region 基础结构

        public static int NextAllianceId = 1;
        public static readonly int MaxLadder = 3;
        public static bool mIdFlag;

        public static Dictionary<int, int> ServerToAlliance = new Dictionary<int, int>();
            //serverId -> AllianceManagerId

        public static Dictionary<int, List<int>> AllianceToServer = new Dictionary<int, List<int>>();
            //AllianceManagerId -> serverId

        public static Dictionary<int, AllianceManager> Servers = new Dictionary<int, AllianceManager>();
            //AllianceManagerId -> AllianceManager

        public static Dictionary<string, int> AllianceNames = new Dictionary<string, int>(); //key=Name value=AllianceId

        public static Dictionary<int, Alliance> Alliances = new Dictionary<int, Alliance>();
            //key=AllianceId value=Alliance  名字到id的转换

        public static Dictionary<int, List<GuildMissionRecord>> AllianceMissions =
            new Dictionary<int, List<GuildMissionRecord>>(); //key = 战盟等级  value = {任务ID，任务概率}

        private static IServerAllianceManager mImpl;

        static ServerAllianceManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (ServerAllianceManager),
                typeof (ServerAllianceManagerDefaultImpl),
                o => { mImpl = (IServerAllianceManager) o; });
        }

        //获取下一个Id
        public static int GetNextAllianceId()
        {
            return mImpl.GetNextAllianceId();
        }

        //防止ID错误,有这种可能（比如数据库回档，战盟数据存储了，但是下一个ID没有存储）
        public static void CheckAllianceId(int aId)
        {
            mImpl.CheckAllianceId(aId);
        }

        //初始化表格静态数据
        public static void PushMission(GuildMissionRecord record)
        {
            mImpl.PushMission(record);
        }

        //初始化表格静态数据
        public static List<GuildMissionRecord> GetMission(int level)
        {
            return mImpl.GetMission(level);
        }

        //初始化
        public static void Init()
        {
            mImpl.Init();
        }

        //存储
        public static IEnumerator RefreshAll(Coroutine coroutine)
        {
            if (mIdFlag)
            {
                mIdFlag = false;
                var codb = CoroutineFactory.NewSubroutine(SaveDb, coroutine);
                if (codb.MoveNext())
                {
                    yield return codb;
                }
            }
            foreach (var alliance in Servers)
            {
                var co = CoroutineFactory.NewSubroutine(alliance.Value.FlushAll, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        //存储ID 
        public static IEnumerator SaveDb(Coroutine coroutine)
        {
            PlayerLog.WriteLog((int) LogType.SaveAllianceData,
                "--------------------SaveAllianceNextId--------------------id={0}", NextAllianceId);
            var ret = TeamServer.Instance.DB.Set(coroutine, DataCategory.Team, "AllianceNextId",
                NextAllianceId.ToDbInt());
            yield return ret;
        }

        //读取
        public static IEnumerator GetDb(Coroutine coroutine)
        {
            var tasks = TeamServer.Instance.DB.Get<DBInt>(coroutine, DataCategory.Team, "AllianceNextId");
            yield return tasks;
            if (tasks.Data == null)
            {
                NextAllianceId = 1;
            }
            else
            {
                NextAllianceId = tasks.Data.Value;
            }
            PlayerLog.WriteLog((int) LogType.GetAllianceData,
                "--------------------GetAllianceNextId--------------------id={0}", NextAllianceId);
        }

        //心跳
        public static void Updata()
        {
            mImpl.Updata();
        }

        #endregion

        #region 服务器接口

        //靠服务器Id 获得服务器所在的数据管理器
        public static AllianceManager GetAllianceByServer(int serverId)
        {
            return mImpl.GetAllianceByServer(serverId);
        }

        public static void OnLost(ulong guid)
        {
            mImpl.OnLost(guid);
        }

        #endregion

        #region 名字相关

        //缓存名字到战盟ID的映射
        public static void PushName(string allianceName, int allianceId, Alliance alliance)
        {
            mImpl.PushName(allianceName, allianceId, alliance);
        }

        //删除缓存的名字
        public static void PopName(int allianceId)
        {
            mImpl.PopName(allianceId);
        }

        //获得某个战盟的名字
        public static string GetAllianceName(int allianceId)
        {
            return mImpl.GetAllianceName(allianceId);
        }

        #endregion

        #region 增删改查

        //新增创建
        public static ErrorCodes CreateAlliance(int serverId, ulong leaderId, string name, ref int allianceId)
        {
            return mImpl.CreateAlliance(serverId, leaderId, name, ref allianceId);
        }

        //新增创建:成功
        public static void CreateAllianceSuccess(string name)
        {
            mImpl.CreateAllianceSuccess(name);
        }

        //新增创建:失败
        public static void CreateAllianceFaild(string name)
        {
            mImpl.CreateAllianceFaild(name);
        }

        //删除
        public static ErrorCodes DeleteAlliance(int allianceId)
        {
            return mImpl.DeleteAlliance(allianceId);
        }

        //查询
        public static Alliance GetAllianceById(int allianceId)
        {
            return mImpl.GetAllianceById(allianceId);
        }

        //查询
        public static Alliance GetAllianceByName(string name)
        {
            return mImpl.GetAllianceByName(name);
        }

        //获得某个玩家所在的战盟
        public static Alliance GetAllianceByCharacterId(ulong characterId)
        {
            return mImpl.GetAllianceByCharacterId(characterId);
        }

        #endregion

        #region 操作

        //创建新战盟
        public static ErrorCodes CreateNewAlliance(int serverId, ulong leaderId, string name, ref int allianceId)
        {
            return mImpl.CreateNewAlliance(serverId, leaderId, name, ref allianceId);
        }

        //邀请加入
        public static ErrorCodes InviteCharacter(ulong fromGuid, int allianceId, ulong characterGuid)
        {
            return mImpl.InviteCharacter(fromGuid, allianceId, characterGuid);
        }

        //邀请回复结果
        public static ErrorCodes InviteResult(int allianceId, ulong characterGuid, int result)
        {
            return mImpl.InviteResult(allianceId, characterGuid, result);
        }

        //申请加入
        public static ErrorCodes ApplyCharacter(int allianceId, ulong characterGuid)
        {
            return mImpl.ApplyCharacter(allianceId, characterGuid);
        }

        //取消申请
        public static ErrorCodes ApplyCancel(int allianceId, ulong characterGuid)
        {
            return mImpl.ApplyCancel(allianceId, characterGuid);
        }

        //退出战盟
        public static ErrorCodes LeaveCharacter(int serverId, ulong characterGuid)
        {
            return mImpl.LeaveCharacter(serverId, characterGuid);
        }

        //申请回复结果
        public static ErrorCodes ApplyResult(int allianceId, ulong characterGuid, int result)
        {
            return mImpl.ApplyResult(allianceId, characterGuid, result);
        }

        //通知
        public static ErrorCodes SendMessage(ulong toCharacterId, int type, string name1, int allianceId, string name2)
        {
            return mImpl.SendMessage(toCharacterId, type, name1, allianceId, name2);
        }

        //时间触发器（战盟修改）
        public static void WeekRefresh()
        {
            mImpl.WeekRefresh();
        }

        public static void OnAllianceAddRes(int allianceId, Dictionary<int, int> res)
        {
            mImpl.OnAllianceAddRes(allianceId,res);
        }

        public static void OnDay()
        {
            mImpl.OnDay();
        }
        #endregion
    }
}