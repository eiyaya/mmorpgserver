#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using EventSystem;
using ICSharpCode.NRefactory.TypeSystem;
using LogicServerService;
using Scorpion;
using NLog;
using Shared;
using System.Diagnostics;

#endregion

namespace Logic
{
    public interface ICharacterController
    {
        ErrorCodes AddAttrPoint(CharacterController _this, int Strength, int Agility, int Intelligence, int Endurance);
        void AddBuff(CharacterController _this, int buffId, int bufflevel);
        ItemBase AddElfItem(CharacterController _this, ItemBase item);
        ErrorCodes PetIslandBuyTili(CharacterController _this);
        int PetIslandReduceTili(CharacterController _this, int num);
        int PetIslandGetTili(CharacterController _this);

        void AddExData(CharacterController _this, int index, int value, bool forceNotToClient = false);
        void AddFubenCount(CharacterController _this, FubenRecord tbFuben);
        void ApplyEquipDurable(CharacterController _this, Dictionary<int, int> durables);
        void ApplyEvent(CharacterController _this, int eventId, string evt, int count);
        void ApplyVolatileData(CharacterController _this, DBCharacterLogicVolatile data);

        IEnumerator AskEnterDungeon(Coroutine coroutine,
                                    CharacterController _this,
                                    int serverId,
                                    int sceneId,
                                    ulong guid,
                                    SceneParam param);

        void BattleResult(CharacterController _this, int fubenId, int type);
        void BooksChange(CharacterController _this);
        ErrorCodes BuyFubenCount(CharacterController _this, int fubenId);
        ErrorCodes BuySpaceBag(CharacterController _this, int bagId, int bagIndex, int needCount);
        ErrorCodes BuySpaceBagByPaid(CharacterController _this, int bagId, int bagIndex, int needCount);
        ErrorCodes ChangeRole(CharacterController _this);
        void ChatSpeek(CharacterController _this, eChatChannel type, string content, List<ulong> toList);
        bool CheckBagCanIn(CharacterController _this, int value);
        int CheckCondition(CharacterController _this, int nConditionId);
        void CheckDailyActivity(CharacterController _this, List<DailyActivityRecord> records);
        bool CheckDungeonReward(CharacterController _this, FubenRecord tbFuben);
        int GetMieShiFailTitleFlag(CharacterController _this);
        int GetMieShiSuccessTitleFlag(CharacterController _this);
        void CheckEquipEnhanceTitle(CharacterController _this);
        void RemoveOverTimeTitles(CharacterController _this);
        ErrorCodes CheckEquipOn(CharacterController _this, EquipRecord tbEquip, int nEquipPart);
        ErrorCodes CheckFuben(CharacterController _this, int fubenId);
        ErrorCodes CheckFubenDetail(CharacterController _this, int fubenId, ref int DetailCode);
        ErrorCodes CheckP1vP1(CharacterController _this);
        void CompleteFuben(CharacterController _this, FubenResult result, bool isByMail = false);
        void CompleteFubenSaveData(CharacterController _this, int fubenId, int seconds);
        void DailyFirstOnline(CharacterController _this, int continuedLanding);
        ErrorCodes DepotPutIn(CharacterController _this, int nBagId, int nIndex);
        ErrorCodes DepotTakeOut(CharacterController _this, int nIndex);
        void DurableDown(CharacterController _this, int bagIdandIndex, int diffValue);

        IEnumerator EnterFuben(Coroutine co,
                               CharacterController _this,
                               int fubenId,
                               AsyncReturnValue<ErrorCodes> errCode,
                               AsyncReturnValue<int> check);

        void EquipModelStateChange(CharacterController _this, int nPart, int nState, ItemBase item);

        void EquipChange(CharacterController _this, int nType, int nPart, int nIndex, ItemBase item);
        void EquipSkillChange(CharacterController _this, Int32Array dels, Int32Array adds, Int32Array Lvls);
        int GetAttackType(CharacterController _this);
        int GetAttrPoint(CharacterController _this, eAttributeType attrId);
        BagBase GetBag(CharacterController _this, int bagid);
        int GetBooksAttr(CharacterController _this, Dictionary<int, int> attrs, Dictionary<int, int> monsterAttrs);
        IEnumerable<NodeBase> GetChildren(CharacterController _this);
        DBCharacterLogic GetData(CharacterController _this);
        List<int> GetExData(CharacterController _this);
        int GetExData(CharacterController _this, int index);
        long GetExData64(CharacterController _this, int index);
        bool GetFlag(CharacterController _this, int index);
        ItemBase GetItemByBagByIndex(CharacterController _this, int bagid, int bagindex);
        int GetLevel(CharacterController _this);
        string GetName(CharacterController _this);
        bool GetOnline(CharacterController _this);
        void GetP1vP1OldList(CharacterController _this, List<P1vP1Change_One> list);
        PetItem GetPet(CharacterController _this, int petId);
        int GetRole(CharacterController _this);
        PetItem GetSamePet(CharacterController _this, int petId);
        DBCharacterLogicSimple GetSimpleData(CharacterController _this);
        List<TimedTaskItem> GetTimedTasks(CharacterController _this);
        int GetTitle(CharacterController _this, int idx);
        WingItem GetWing(CharacterController _this);
        ErrorCodes GmCommand(CharacterController _this, string command);
        DBCharacterLogic InitByBase(CharacterController _this, ulong characterId, object[] args);
        bool InitByDb(CharacterController _this, ulong characterId, DBCharacterLogic dbData);
        void InitCharacterController(CharacterController _this);
        void LoadFinished(CharacterController _this);
        void ModityTitle(CharacterController _this, int titleId, bool active);
        void ModityTitles(CharacterController _this, List<int> titles, List<bool> states);
        ErrorCodes NpcService(CharacterController _this, int serviceId);
        void OnDestroy(CharacterController _this);
        void OnLevelUp(CharacterController _this, int lv);
        void OnRechargeSuccess(CharacterController _this, string platform, int type, float price);
        void OnSaveData(CharacterController _this, DBCharacterLogic data, DBCharacterLogicSimple simpleData);
        void OnShareScuess(CharacterController _this);
        void OnVipLevelChanged(CharacterController _this,int oldlevel,int newlevel);
        ErrorCodes OperatePet(CharacterController _this, int petId, PetOperationType type, int param);
        void OutLine(CharacterController _this);
        void RefreshTrialTime(CharacterController _this);
        ErrorCodes PassFuben(CharacterController _this, int fubenId, DrawResult dataResult);
        int PushDraw(CharacterController _this, int drawId, out ItemBase tempItemBase, bool isAddItem = true, int dungeonId = -1);
        P1vP1Change_One PushP1vP1Change(CharacterController _this, int type, string pvpName, int oldRank, int newRank);
        ErrorCodes ReceiveAllCompensation(CharacterController _this, int type);
        ErrorCodes ReceiveCompensation(CharacterController _this, int comId, int type);
        ErrorCodes TakeMultyExpAward(CharacterController _this, int id);
        ErrorCodes Recycletem(CharacterController _this, int nBagId, int nIndex, int nItemId, int nCount);
        ErrorCodes RefreshAttrPoint(CharacterController _this, ref int newPoint);
        void RefreshSkillTitle(CharacterController _this);
        ErrorCodes RepairEquip(CharacterController _this);

        IEnumerator SceneBooksChange(Coroutine coroutine,
                                     CharacterController _this,
                                     ulong characterId,
                                     Dict_int_int_Data dic, Dict_int_int_Data monsterDic);

        IEnumerator SceneEquipChange(Coroutine coroutine,
                                     CharacterController _this,
                                     ulong characterId,
                                     int nType,
                                     int nPart,
                                     ItemBaseData Equip);

        IEnumerator SceneEquipModelStateChange(Coroutine coroutine,
                                              CharacterController _this,
                                              ulong characterId,
                                              int nPart,
                                              int nState,
                                              ItemBaseData Equip);

        void ElfChange(CharacterController _this, List<int> removeBuff, Dictionary<int, int> addBuff);

        IEnumerator SceneElfChange(Coroutine coroutine,
            CharacterController _this,
            ulong characterId,
            List<int> removeBuff,
            Dictionary<int, int> addBuff);

        void GetElfBuff(CharacterController _this, Dictionary<int, int> buffs);
        void GetMountBuff(CharacterController _this, Dictionary<int, int> buffs);

        IEnumerator SceneEquipSkillChange(Coroutine coroutine,
                                          CharacterController _this,
                                          ulong characterId,
                                          Int32Array dels,
                                          Int32Array adds,
                                          Int32Array lvls);

        IEnumerator SceneInnateChange(Coroutine coroutine,
                                      CharacterController _this,
                                      ulong characterId,
                                      int nType,
                                      int nTalent,
                                      int nLevel);

        IEnumerator SceneSkillChange(Coroutine coroutine,
                                     CharacterController _this,
                                     ulong characterId,
                                     int nType,
                                     int nSkillId,
                                     int nLevel);

        void OnAddCharacterContribution(CharacterController _this, int nCount);
        IEnumerator SSAddCharacterContribution(Coroutine coroutine, CharacterController _this, int nCount);

        ErrorCodes SelectDungeonReward(CharacterController _this, int fubenId, int selIdx, bool isByMail = false);
        ErrorCodes SellItem(CharacterController _this, int nBagId, int nIndex, int nItemId, int nCount);
        ErrorCodes SendNormalReward(CharacterController _this, FubenRecord tbFuben, bool useDiamond, bool isByMail);
        void SendQuitReward(CharacterController _this, FubenRecord tbFuben, bool isByMail);
        void BroadCastGetEquip(CharacterController _this, int itemId, int dictId);
        void SendSystemNoticeInfo(CharacterController _this,
                                  int dictId,
                                  List<string> strs = null,
                                  List<int> exInt = null);

        void SetExData(CharacterController _this, int index, int value, bool forceNotToClient = false);
        void SetFlag(CharacterController _this, int index, bool flag = true, int forceNotToClient = 0);
        void SetTitleFlag(CharacterController _this, int index, bool flag, int forceNotToClient, DateTime titleStartTime);

        void SetName(CharacterController _this, string name);
        void SetRankFlag(CharacterController _this, RankType type);
        ErrorCodes SetTitle(CharacterController _this, int id);
        void SkillChange(CharacterController _this, int nType, int nSkillId, int nLevel);
        void TalentChange(CharacterController _this, int nType, int nTalent, int nLevel);
        void TestBagDbIndex(CharacterController _this);
        void TestBagLogicIndex(CharacterController _this);
        void Tick(CharacterController _this);
        ErrorCodes UpgradeHonor(CharacterController _this, int honorId);
        int GetElfFightPoint(CharacterController _this);
        void SetRefreshFightPoint(CharacterController _this, bool refresh);

        /// <summary>
        ///     使用装备
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nBagIndex">装备包裹的索引</param>
        /// <param name="nEquipPart">部位ID</param>
        /// <param name="index">部位索引</param>
        /// <returns></returns>
        ErrorCodes UseEquip(CharacterController _this, int nBagIndex, int nEquipPart, int index);

        bool DeleteEquip(CharacterController _this,int ItemId,int deleteType);

        ErrorCodes UseShiZhuang(CharacterController _this, int BagId, int BagItemIndex, int EquipPart);
        ErrorCodes DeleteShiZhuang(CharacterController _this, int BagId, int BagItemIndex);
        ErrorCodes SetEquipModelState(CharacterController _this, List<int> Part, int State);
        void RefreshFashionState(CharacterController _this);

        ErrorCodes TowerSweep(CharacterController _this,TowerSweepResult respone);
        ErrorCodes TowerBuySweepTimes(CharacterController _this);
        ErrorCodes CheckTowerDailyInfo(CharacterController _this);
        
        IEnumerator UseItem(Coroutine coroutine, CharacterController _this, UseItemInMessage msg);
        ErrorCodes AutoUseItem(CharacterController _this, int itemId);
        ErrorCodes WishingPoolDepotTakeOut(CharacterController _this, int nIndex);

        ErrorCodes ApplyActivityData(CharacterController _this, int serverId, CommonActivityData responeData);
        void Mount(CharacterController _this, int MountId);
        void SetMoniterData(CharacterController _this, MsgChatMoniterData data);
    }

    public class CharacterControllerDefaultImpl : ICharacterController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger RechargeLogger = LogManager.GetLogger("RechargeLogger");
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);

        private int calculateCompensationCost(Dictionary<int, int> data, int type)
        {
            //计算金币消耗
            var needGold = 0;
            foreach (var i in data)
            {
                if (i.Key == 1)
                {
                    needGold += (i.Value + StaticParam.CompensationExp - 1)/StaticParam.CompensationExp;
                }
                else if (i.Key == 2)
                {
                    needGold += (i.Value + StaticParam.CompensationGold - 1)/StaticParam.CompensationGold;
                }
                else
                {
                    var tbItem = Table.GetItemBase(i.Key);
                    if (tbItem == null)
                    {
                        Logger.Warn("ReceiveCompensation itemId = {0},itemCount = {1}", i.Key, i.Value);
                        continue;
                    }
                    needGold += i.Value*tbItem.ItemValue;
                }
            }
            if (type == 1)
            {
                var needDiamond = (needGold + StaticParam.CompensationDiamond - 1)/StaticParam.CompensationDiamond;
                return needDiamond;
            }
            return needGold;
        }

        //发送到Broker分发聊天数据
        private void ChatToServer(CharacterController _this, int chatType, string content)
        {
            var serverId = SceneExtension.GetServerLogicId(_this.serverId);
            LogicServer.Instance.ChatAgent.BroadcastWorldMessage((uint) serverId, chatType, _this.mGuid, _this.Name,
                new ChatMessageContent {Content = content});
        }

        private void CheckActiveTitle(CharacterController _this, int flagId, DateTime startTime)
        {
            if (!StaticParam.TitleFlags.ContainsKey(flagId))
            {
                return;
            }
            var titleId = StaticParam.TitleFlags[flagId];
            var tbNameTitle = Table.GetNameTitle(titleId);
            if (tbNameTitle == null)
            {
                return;
            }
            var timeStamp = 0;
            if (tbNameTitle.ValidityPeriod == 1)
            { // 168 hour
                var endTime = startTime.AddHours(168);
                if (endTime <= DateTime.Now)
                    return;
                timeStamp = endTime.GetTimeStampSeconds();
            }
            else if (tbNameTitle.ValidityPeriod == 2)
            { // 24 hour 
                var endTime = startTime.AddHours(24);
                if (endTime <= DateTime.Now)
                    return;
                timeStamp = endTime.GetTimeStampSeconds();
            }
            if (titleId == 5051 && _this.mDbData.Titles.ContainsKey(5052)) //为灭世的称号做个特例
            {//当发现劫后余生时判断救世传说,如果有则返回
                return;
            }
            if (IsTitleActived(_this, titleId))
            {
                if (timeStamp > 0 && timeStamp > _this.mDbData.Titles[titleId])
                { // 重置时间
                    _this.mDbData.Titles[titleId] = timeStamp;
                }
                return;
            }
            _this.mDbData.Titles.Add(titleId, timeStamp);

            CheckSetTitle(_this, tbNameTitle);
            //通知scene，称号列表变了，刷新称号所加的属性
            NotifySceneTitleList(_this);
        }

        private void CheckDeactiveTitle(CharacterController _this, int flagId)
        {
            if (!StaticParam.TitleFlags.ContainsKey(flagId))
            {
                return;
            }
            var titleId = StaticParam.TitleFlags[flagId];
            var tbNameTitle = Table.GetNameTitle(titleId);
            if (tbNameTitle == null)
            {
                return;
            }
            if (!IsTitleActived(_this, titleId))
            {
                return;
            }
            _this.mDbData.Titles.Remove(titleId);
            var pos = tbNameTitle.Pos;
            NameTitleRecord newTitleRecard = null;
            Table.ForeachNameTitle(record =>
            {
                if (record.FlagId < 0)
                {
                    return true;
                }
                if (record.Pos != pos)
                {
                    return true;
                }
                if (!_this.GetFlag(record.FlagId))
                {
                    return true;
                }
                if (newTitleRecard == null || newTitleRecard.PeriodEndSort < record.PeriodEndSort)
                {
                    newTitleRecard = record;
                }
                return true;
            });
            SetTitle(_this, newTitleRecard == null ? -1 : newTitleRecard.Id, pos);
            //通知scene，称号列表变了，刷新称号所加的属性
            NotifySceneTitleList(_this);
        }

        private void CheckSetTitle(CharacterController _this, int titleId)
        {
            var tbNameTitle = Table.GetNameTitle(titleId);
            if (tbNameTitle != null)
            {
                CheckSetTitle(_this, tbNameTitle);
            }
        }

        private void CheckSetTitle(CharacterController _this, NameTitleRecord tbNameTitle)
        {
            var pos = tbNameTitle.Pos;
            var changeTitle = false;
            var titleIdOld = GetTitle(_this, pos);
            if (titleIdOld == -1)
            {
                changeTitle = true;
            }
            else
            {
                var tbNameTitleOld = Table.GetNameTitle(titleIdOld);
                if (tbNameTitleOld == null)
                {
                    changeTitle = true;
                }
                else
                {
                   
                    while (tbNameTitleOld != null && tbNameTitleOld.PostId >= 0)
                    {
                        if (tbNameTitleOld.PostId == tbNameTitle.Id)
                        {
                            changeTitle = true;
                            break;
                        }
                        tbNameTitleOld = Table.GetNameTitle(tbNameTitleOld.PostId);
                    }

                    if(tbNameTitleOld.PostId <0 && titleIdOld != tbNameTitle.Id)
                    {
                        changeTitle = true;
                    }
                }
                
            }
            if (changeTitle)
            {
                SetTitle(_this, tbNameTitle.Id);
            }
        }

        private ErrorCodes CompensationAddItem(CharacterController _this, int type, int cost, Dictionary<int, int> data)
        {
            if (type == 1) //钻石
            {
                var nowD = _this.mBag.GetRes(eResourcesType.DiamondRes);
                if (nowD < cost)
                {
                    return ErrorCodes.DiamondNotEnough;
                }
                _this.mBag.DelRes(eResourcesType.DiamondRes, cost, eDeleteItemType.Compensation);
                foreach (var i in data)
                {
                    _this.mBag.AddItem(i.Key, i.Value, eCreateItemType.Compensation);
                }
            }
            else //金币
            {
                var nowG = _this.mBag.GetRes(eResourcesType.GoldRes);
                if (nowG < cost)
                {
                    return ErrorCodes.MoneyNotEnough;
                }
                _this.mBag.DelRes(eResourcesType.GoldRes, cost, eDeleteItemType.Compensation);
                foreach (var i in data)
                {
                    _this.mBag.AddItem(i.Key, (int) Math.Ceiling(1.0*i.Value*StaticParam.CompensationGoldRef/10000),
                        eCreateItemType.Compensation);
                }
            }
            return ErrorCodes.OK;
        }

        private bool IsTitleActived(CharacterController _this, int titleId)
        {
            var titles = _this.mDbData.Titles;
            return titles.ContainsKey(titleId);
        }

        private void NotifyItemCountToScene(CharacterController _this, int itemId, int count)
        {
            CoroutineFactory.NewCoroutine(NotifyItemCountToSceneCoroutine, _this, itemId, count).MoveNext();
        }

        private IEnumerator NotifyItemCountToSceneCoroutine(Coroutine co,
                                                            CharacterController _this,
                                                            int itemId,
                                                            int count)
        {
            var msg = LogicServer.Instance.SceneAgent.NotifyItemCount(_this.mGuid, itemId, count);
            yield return msg.SendAndWaitUntilDone(co);
        }

        private IEnumerator AddPlayerToChatMonitorCoroutine(Coroutine co,CharacterController _this)
        {
            var msg = LogicServer.Instance.ChatAgent.AddPlayerToChatMonitor(_this.mGuid,0);
            yield return msg.SendAndWaitUntilDone(co);
            
        }

        private void NotifySceneTitleList(CharacterController _this)
        {
            CoroutineFactory.NewCoroutine(NotifySceneTitleListCoroutine, _this).MoveNext();
        }

        private IEnumerator NotifySceneTitleListCoroutine(Coroutine co, CharacterController _this)
        {
            var titles = _this.mDbData.Titles.Keys;
            var titleArray = new Int32Array();
            titleArray.Items.AddRange(titles);
            var msg = LogicServer.Instance.SceneAgent.SceneTitleChange(_this.mGuid, titleArray, 1);
            yield return msg.SendAndWaitUntilDone(co);
        }

        public void RankSendChanges(CharacterController _this)
        {
            if (_this.RankTrigger != null)
            {
                LogicServerControl.Timer.DeleteTrigger(_this.RankTrigger);
                _this.RankTrigger = null;
            }
            if (_this.RankFlag == 0)
            {
                return;
            }
            CoroutineFactory.NewCoroutine(RankSendChangesCoroutine, _this).MoveNext();
        }

        public void RankSendChangesBytrigger(CharacterController _this)
        {
            _this.RankTrigger = null;
            if (_this.RankFlag == 0)
            {
                return;
            }
            CoroutineFactory.NewCoroutine(RankSendChangesCoroutine, _this).MoveNext();
        }

        private IEnumerator RankSendChangesCoroutine(Coroutine co, CharacterController _this)
        {
            var tempList = new RankChangeDataList
            {
                CharacterId = _this.mGuid,
                Name = GetName(_this),
                ServerId = _this.serverId
            };
            if (BitFlag.GetLow(_this.RankFlag, (int) RankType.Level))
            {
                var temp = new RankChangeData
                {
                    RankType = (int) RankType.Level,
                    Value =
                        (_this.mBag.GetRes(eResourcesType.LevelRes))*4000000000 +
                        _this.mBag.GetRes(eResourcesType.ExpRes)
                };
                tempList.Changes.Add(temp);
            }
            if (BitFlag.GetLow(_this.RankFlag, (int) RankType.Money))
            {
                var temp = new RankChangeData
                {
                    RankType = (int) RankType.Money,
                    Value = _this.mBag.GetRes(eResourcesType.GoldRes)
                };
                tempList.Changes.Add(temp);
				_this.mOperActivity.OnRankDataChange(RankType.Money, temp.Value);
            }
            if (BitFlag.GetLow(_this.RankFlag, (int) RankType.CityLevel))
            {
                var temp = new RankChangeData
                {
                    RankType = (int) RankType.CityLevel,
                    Value =
                        (_this.mBag.GetRes(eResourcesType.HomeLevel))*4000000000 +
                        _this.mBag.GetRes(eResourcesType.HomeExp)
                };
                tempList.Changes.Add(temp);
				_this.mOperActivity.OnRankDataChange(RankType.CityLevel, temp.Value);
            }
            if (BitFlag.GetLow(_this.RankFlag, (int) RankType.WingsFight))
            {
                var item = _this.GetBag((int) eBagType.Wing).GetItemByIndex(0) as WingItem;
                if (item != null)
                {
                    var temp = new RankChangeData
                    {
                        RankType = (int) RankType.WingsFight,
                        Value = item.GetFightPoint(_this.GetLevel(), _this.GetAttackType())
                    };
                    tempList.Changes.Add(temp);
					_this.mOperActivity.OnRankDataChange(RankType.WingsFight, temp.Value);
                }
            }
            if (BitFlag.GetLow(_this.RankFlag, (int) RankType.PetFight))
            {
                var item = _this.GetBag((int) eBagType.Elf).GetItemByIndex(0) as ElfItem;
                if (item != null)
                {
                    var temp = new RankChangeData
                    {
                        RankType = (int) RankType.PetFight,
                        Value = item.GetFightPoint(_this.GetLevel(), _this.GetAttackType(), 10000)
                    };
                    tempList.Changes.Add(temp);
					_this.mOperActivity.OnRankDataChange(RankType.PetFight, temp.Value);
                }
            }
			if (BitFlag.GetLow(_this.RankFlag, (int)RankType.RechargeTotal))
			{
				var temp = new RankChangeData
				{
					RankType = (int)RankType.RechargeTotal,
					Value = _this.GetExData((int)eExdataDefine.e78_TotalRechargeDiamond)
				};
				tempList.Changes.Add(temp);
				_this.mOperActivity.OnRankDataChange(RankType.RechargeTotal, temp.Value);
			}
            if (BitFlag.GetLow(_this.RankFlag, (int)RankType.Mount))
            {
                var fightPoint = _this.mMount.GetFightPoint(_this.GetLevel(), _this.GetRole());
                if (fightPoint > 0)
                {
                    var temp = new RankChangeData
                    {
                        RankType = (int)RankType.Mount,
                        Value = fightPoint
                    };
                    tempList.Changes.Add(temp);
                    _this.mOperActivity.OnRankDataChange(RankType.Mount, temp.Value);                    
                }
            }
            var msg = LogicServer.Instance.RankAgent.SSCharacterChangeDataList(_this.mGuid, tempList);
            yield return msg.SendAndWaitUntilDone(co);
            _this.RankFlag = 0;
        }

        private void SceneTitleChange(ulong id, Int32Array data, int type)
        {
            CoroutineFactory.NewCoroutine(SceneTitleChangeCoroutine, id, data, type).MoveNext();
        }

        private IEnumerator SceneTitleChangeCoroutine(Coroutine co, ulong id, Int32Array data, int type)
        {
            var msg = LogicServer.Instance.SceneAgent.SceneTitleChange(id, data, type);
            yield return msg.SendAndWaitUntilDone(co);
        }

        private ErrorCodes SetTitle(CharacterController _this, int id, int pos)
        {
            
            if (pos < 0 || pos >= StaticParam.TitlesMaxCount)
            {
                Logger.Warn("SetTitle id={0},index ={1},id={2}", _this.mGuid, pos, id);
                return ErrorCodes.Error_DataOverflow;
            }
            var titles = _this.mDbData.ViewTitles;
            
            if (titles[pos] != id)
            {
                int titleId = id;
                if (pos < titles.Count && titleId == -1)
                {
                    titleId = titles[pos];
                }

                {
                    titles[pos] = id;
                    _this.MarkDbDirty();

                    var data = new Int32Array();
                    data.Items.Add(id);
                    SceneTitleChange(_this.mGuid, data, 0);                    
                }

                if (null != _this.moniterData)
                {
                    try
                    {
                        string name = "empty";
                        var tb = Table.GetNameTitle(titleId);
                        if (tb != null)
                        {
                            name = tb.Name;
                        }

                        var klog = string.Format("chars_title#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                            _this.moniterData.channel,
                            _this.serverId,
                            _this.moniterData.uid,
                            _this.moniterData.pid,
                            _this.mGuid,
                            _this.GetName(),
                            titleId,
                            name,
                            DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                            id > 0 ? 1 : 0);
                        PlayerLog.Kafka(klog, 2);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception.Message);
                    }
                }

            }

            

            return ErrorCodes.OK;
        }

        private void UpdateServer()
        {
            CoroutineFactory.NewCoroutine(UpdateServerCoroutine).MoveNext();
        }

        private IEnumerator UpdateServerCoroutine(Coroutine co)
        {
            var msg1 = LogicServer.Instance.ActivityAgent.UpdateServer(0);
            msg1.SendAndWaitUntilDone(co);
            var msg2 = LogicServer.Instance.SceneAgent.UpdateServer(0);
            msg2.SendAndWaitUntilDone(co);
            var msg3 = LogicServer.Instance.LogicAgent.UpdateServer(0);
            msg3.SendAndWaitUntilDone(co);
            var msg4 = LogicServer.Instance.LoginAgent.UpdateServer(0);
            msg4.SendAndWaitUntilDone(co);
            var msg5 = LogicServer.Instance.RankAgent.UpdateServer(0);
            msg5.SendAndWaitUntilDone(co);
            var msg6 = LogicServer.Instance.TeamAgent.UpdateServer(0);
            msg6.SendAndWaitUntilDone(co);
            var msg7 = LogicServer.Instance.ChatAgent.UpdateServer(0);
            msg7.SendAndWaitUntilDone(co);
            var msg8 = LogicServer.Instance.GameMasterAgent.UpdateServer(0, 0);
            msg8.SendAndWaitUntilDone(co);
            yield break;
        }

        public void SetName(CharacterController _this, string name)
        {
            _this.Name = name;
        }

        public string GetName(CharacterController _this)
        {
            return _this.Name;
        }

        public void InitCharacterController(CharacterController _this)
        {
            _this.mSkill = new SkillData();
            _this.mTalent = new Talent();
            _this.mBag = new BagManager();
            _this.mTask = new MissionManager();
            _this.lExdata = new Exdata();
            _this.lExdata64 = new Exdata64();
            _this.lFlag = new NodeFlag();
            _this.mBook = new BookManager();
            _this.mMount = new MountManager();
            _this.mFriend = new FriendManager();
            _this.mStone = new StoneManager();
            _this.mMail = new MailManager();
            _this.mCity = new CityManager();
            _this.mPetMission = new PetMissionManager2();
            _this.mAlliance = new AllianceManager();
            _this.mExchange = new Exchange();
			_this.mOperActivity = new PlayerOperationActivityManager();
            //mFlag = new BitFlag(2048);
            //mExdata = new int[256]; 
            _this.childs[0] = _this.mBag;
            _this.childs[1] = _this.mTask;
            _this.childs[2] = _this.mSkill;
            _this.childs[3] = _this.mTalent;
            _this.childs[4] = _this.lFlag;
            _this.childs[5] = _this.lExdata;
            _this.childs[6] = _this.mBook;
            _this.childs[7] = _this.mFriend;
            _this.childs[8] = _this.mStone;
            _this.childs[9] = _this.mMail;
            _this.childs[10] = _this.mCity;
            _this.childs[11] = _this.mPetMission;
            _this.childs[12] = _this.lExdata64;
            _this.childs[13] = _this.mAlliance;
            _this.childs[14] = _this.mExchange;
			_this.childs[15] = _this.mOperActivity;
            Logger.Info("********New CharacterController********");
        }

        public int GetLevel(CharacterController _this)
        {
            return _this.mBag.GetRes(0);
        }

        public int GetAttackType(CharacterController _this)
        {
            if (GetRole(_this) == 1)
            {
                return 1;
            }
            return 0;
        }

        public int GetRole(CharacterController _this)
        {
            return _this.mDbData.TypeId;
            //var temp = Table.GetCharacterBase(mDbData.TypeId);
            //return temp.ExdataId;
        }

        #region 聊天相关

        public virtual void ChatSpeek(CharacterController _this, eChatChannel type, string content, List<ulong> toList)
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
                    break;
                case eChatChannel.Guild:
                    break;
                case eChatChannel.Team:
                    break;
                case eChatChannel.Whisper:
                    break;
                case eChatChannel.Horn:
                {
                }
                    break;
                case eChatChannel.Count:
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region NPC服务相关

        public ErrorCodes NpcService(CharacterController _this, int serviceId)
        {
            if (serviceId == -1)
            {
                _this.mServiceId = serviceId;
                return ErrorCodes.OK;
            }
            var e = new NpcServeEvent(_this, serviceId);
            EventDispatcher.Instance.DispatchEvent(e);
            var tbService = Table.GetService(serviceId);
            if (tbService == null)
            {
                return ErrorCodes.Error_ServiceID;
            }
            switch (tbService.Type)
            {
                case 0: //商店
                {
                    _this.mServiceId = serviceId;
                }
                    break;
                case 1: //修理
                {
                    return RepairEquip(_this);
                }
                case 2: //治疗
                {
                }
                    break;
                case 3: //仓库
                {
                    _this.mServiceId = serviceId;
                }
                    break;
                default:
                {
                    return ErrorCodes.Unknow;
                }
            }
            return ErrorCodes.OK;
        }

        #endregion
#if DEBUG
        //[Updateable("character")]
	    //private static readonly float SaveRankIntervalSecond = 10; //10秒
#else
		//private static readonly float SaveRankIntervalSecond = 60*5;
#endif
        public void SetRankFlag(CharacterController _this, RankType type)
        {
            _this.RankFlag = BitFlag.IntSetFlag(_this.RankFlag, (int) type);
            if (_this.RankTrigger == null)
            {
                var now = DateTime.Now.AddMinutes(5);
                var nextTime =
                    new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / 5 * 5, 0)
                        .AddSeconds(-(_this.mGuid % 10.0f));

                if (nextTime < DateTime.Now)
                {
                    nextTime = nextTime.AddMinutes(5);
                }

                _this.RankTrigger = LogicServerControl.Timer.CreateTrigger(nextTime,
                    () => { RankSendChangesBytrigger(_this); });
            }
        }

        public DBCharacterLogicSimple GetSimpleData(CharacterController _this)
        {
            DBCharacterLogicSimple dbSimple;
            CharacterManager<CharacterController, DBCharacterLogic, DBCharacterLogicSimple, DBCharacterLogicVolatile>.
                DataItem data;
            var dic = CharacterManager.Instance.mDictionary;
            if (dic.TryGetValue(_this.mGuid, out data))
            {
                dbSimple = data.SimpleData;
                dbSimple.EquipsModel.Clear();
                dbSimple.Equips.ItemsChange.Clear();
                dbSimple.Exdatas.Clear();
            }
            else
            {
                Logger.Info("GetSimpleData get null, id = {0}", _this.mGuid);
                dbSimple = new DBCharacterLogicSimple();
            }
            dbSimple.Id = _this.mGuid;
            dbSimple.Level = GetLevel(_this);
            dbSimple.TypeId = _this.mDbData.TypeId;
            dbSimple.Ladder = GetExData(_this, 51);
            dbSimple.Vip = _this.mBag.GetRes(eResourcesType.VipLevel);
            dbSimple.MountId = _this.mMount.mDbData.Id;

            dbSimple.TitleList.Clear();
            if (_this.mDbData != null && _this.mDbData.ViewTitles != null)
            {
                dbSimple.TitleList.AddRange(_this.mDbData.ViewTitles);
            }

            foreach (var bagid in EquipExtension.EquipModelBagId)
            {
                var equip = GetItemByBagByIndex(_this, bagid, 0);
                if (equip == null)
                {
                    continue;
                }
                if (equip.GetId() == -1)
                {
                    continue;
                }
                if (bagid == (int)eBagType.Wing)
                {
                    dbSimple.EquipsModel.Add(bagid, equip.GetId() * 100 + equip.GetExdata(11));
                }
                else if (bagid == (int)eBagType.WeaponShiZhuang ||
                         bagid == (int)eBagType.EquipShiZhuang ||
                         bagid == (int)eBagType.WingShiZhuang)
                {
                    dbSimple.EquipsModel.Add(bagid, equip.GetId() * 100 + equip.GetExdata(31));
                }
                else
                {
                    dbSimple.EquipsModel.Add(bagid, equip.GetId() * 100 + equip.GetExdata(0));
                }
            }

            if (dbSimple.Equips == null)
            {
                dbSimple.Equips = new ItemsChangeData();
            }
            foreach (var i in EquipExtension.Equips)
            {
                BagBase bag;
                if (!_this.mBag.mBags.TryGetValue(i, out bag))
                {
                    continue;
                }
                foreach (var itemBase in bag.mLogics)
                {
                    if (itemBase.GetId() < 0)
                    {
                        continue;
                    }
                    dbSimple.Equips.ItemsChange.Add(i*100 + itemBase.GetIndex(), itemBase.mDbData);
                }
            }

            //加入精灵数据
            var elfBag = _this.mBag.GetBag((int) eBagType.Elf);
            if (elfBag != null)
            {
                if (elfBag.mLogics != null)
                {
                    if (elfBag.mLogics.Count > 0)
                    {
                        var itemBase = elfBag.mLogics[0];
                        if (itemBase != null)
                        {
                            if (itemBase.GetId() != -1)
                            {
                                dbSimple.Equips.ItemsChange.Add((int) eBagType.Elf, itemBase.mDbData);
                            }
                        }
                    }
                }
            }
            //
            //dbSimple.Equips.ItemsChange
            dbSimple.City = new BuildingList();
            foreach (var buildingBase in _this.mCity.BuildingsByArea)
            {
                dbSimple.City.Data.Add(buildingBase.Value.GetBuildingData());
            }

            foreach (var skillId in _this.mDbData.Skill.EquipSkills)
            {
                if (skillId > 0)
                {
                    if (!dbSimple.Skills.TryAdd(skillId, _this.mSkill.GetSkillLevel(skillId)))
                    {
                        var logs = "";
                        foreach (var skill in _this.mDbData.Skill.EquipSkills)
                        {
                            logs = string.Format("{0},{1}", logs, skill);
                        }
                        Logger.Error("GetSimpleData EquipSkills={0}", logs);
                    }
                }
            }
            dbSimple.WorshipCount = GetExData(_this, 313);
            dbSimple.Exchange = _this.mDbData.Exchange;
            dbSimple.Name = GetName(_this);
            dbSimple.Exdatas.TryAdd((int) eExdataDefine.e282, GetExData(_this, (int) eExdataDefine.e282));
            dbSimple.Exdatas.TryAdd((int)eExdataDefine.e688, GetExData(_this, (int)eExdataDefine.e688));
            dbSimple.StarNum = GetExData(_this, (int)eExdataDefine.e688);
            return dbSimple;
        }

        public DBCharacterLogic GetData(CharacterController _this)
        {
            return _this.mDbData;
        }

        public void Tick(CharacterController _this)
        {
			if (_this.Proxy == null)
			{
				return;
			}
			if (null != _this.mOperActivity)
			{
				_this.mOperActivity.Tick();
			}

            if (!_this.NetDirty)
            {
                return;
            }
           
            foreach (var child in _this.Children)
            {
                if (child.NetDirty)
                {
                    child.NetDirtyHandle();
                }
            }
            _this.CleanNetDirty();
	        
        }

        public IEnumerable<NodeBase> GetChildren(CharacterController _this)
        {
            return _this.childs;
            //get
            //{

            //    //return new NodeBase[]
            //    //{
            //    //    mBag, //玩家包裹
            //    //    mTask, //玩家邮件
            //    //    mSkill, //玩家技能
            //    //    mTalent, //玩家天赋
            //    //    lFlag,//玩家标记
            //    //    lExdata,//玩家扩展数据
            //    //    mBook
            //    //}; 
            //}
        }

        public bool GetOnline(CharacterController _this)
        {
            return _this.Proxy != null && _this.Proxy.Connected;
            //  get { return _this.Proxy != null && _this.Proxy.Connected; }
        }

        public List<TimedTaskItem> GetTimedTasks(CharacterController _this)
        {
            return _this.mDbData.TimedTasks;
        }

        public void ApplyEvent(CharacterController _this, int eventId, string evt, int count)
        {
            _this.mStone.EventTrigger(eventId);
            int type = -1, hour = -1;
            Logger.Info("ApplyEvent evt={0}", evt);
            DataTimeExtension.TimeEvent(evt, ref type, ref hour);
            if (type != -1 && hour >= 0 && hour < 24)
            {
                if (type == 0 && hour == 0)
                {
                    _this.mOperActivity.NeedReset = true;
                    DailyFirstOnlineByEvent(_this, type, hour, count);
                }
                else
                {
                    if (type == 0 && hour == 12)
                    {
                        SettleArenaReward(_this, count);
                    }
                    _this.lFlag.ResetByTime(type, hour);
                    _this.lExdata.ResetByTime(type, hour);
                }
                return;
            }
            switch (evt)
            {
                //case "RefreshPetBossMission":
                //    {
                //        mPetMission.RefreshBoss();
                //    }
                //    break;
                case "RefreshPetMission":
                {
                    _this.mPetMission.Refresh();
                }
                    break;
                case "FreePetEgg":
                {
                    if (GetExData(_this, 92) < 2)
                    {
                        AddExData(_this, 92, 1);
                    }
                }
                    break;
                //case "CityBuildGivePetExp":
                //    {
                //        mCity.GivePetExp();
                //    }
                //    break;
            }
        }

        public void OnDestroy(CharacterController _this)
        {
            if (_this.mPetMission != null)
            {
                _this.mPetMission.OnDestroy();
            }
            if (_this.mCity != null)
            {
                _this.mCity.OnDestroy();
            }
            RankSendChanges(_this);
        }

        public void OnSaveData(CharacterController _this, DBCharacterLogic data, DBCharacterLogicSimple simpleData)
        {
            PlayerLog.WriteLog(_this.mGuid, "----------Logic--------------------OnSaveData--------------------{0}",
                data.SaveCount++);
        }

        public void SendSystemNoticeInfo(CharacterController _this,
                                         int dictId,
                                         List<string> strs = null,
                                         List<int> exInt = null)
        {
            var str = Utils.WrapDictionaryId(dictId, strs, exInt);
            ChatToServer(_this, (int) eChatChannel.SystemScroll, str);
        }
        IEnumerator NotifyPlayerPickUpFubenReward(Coroutine co, CharacterController _this)
        {
            var reslut = LogicServer.Instance.SceneAgent.NotifyPlayerPickUpFubenReward(_this.mGuid, _this.mGuid);
            yield return reslut.SendAndWaitUntilDone(co);
        }
        public ErrorCodes SelectDungeonReward(CharacterController _this, int fubenId, int selIdx, bool isByMail = false)
        {
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                Logger.Error("Wrong fuben id [{0}] for SelectDungeonReward()", fubenId);
                return ErrorCodes.Error_FubenID;
            }
            if (tbFuben.AssistType < 4 || tbFuben.AssistType > 5)
            {
                //Logger.Error("Wrong fuben id [{0}] for SelectDungeonReward()", fubenId);
                return ErrorCodes.Error_FubenID;
            }
            var index = tbFuben.AssistType - 4;
            var exDataIdx = (int) eExdataDefine.e408 + index;
            var data = (uint) _this.GetExData(exDataIdx);
            if (data != 0)
            {
                var type = (data >> ActivityDungeonConstants.CompleteTypeStartIdx) &
                           ActivityDungeonConstants.CompleteTypeMask;
                var bQuited = type == (uint) eDungeonCompleteType.Quit;
                //清理掉保存的数据
                _this.SetExData(exDataIdx, 0);

                var err = ErrorCodes.OK;
                if (bQuited)
                {
                    SendQuitReward(_this, tbFuben, isByMail);
                }
                else
                {
                    err = SendNormalReward(_this, tbFuben, selIdx == 1, isByMail);
                }
                if (err == ErrorCodes.OK)
                {
                    CoroutineFactory.NewCoroutine(NotifyPlayerPickUpFubenReward, _this).MoveNext();
                }
                return err;
            }
            //Logger.Error("The last reward has already been received for fuben {0}!!!!", fubenId);
            return ErrorCodes.Error_NoFubenReward;
        }

        public void SendQuitReward(CharacterController _this, FubenRecord tbFuben, bool isByMail)
        {
            var expTime = 1;
            int exp;
            if (tbFuben.IsDynamicExp == 1)
            {
                exp = (int) (1.0*tbFuben.ScanDynamicExpRatio*Table.GetLevelData(_this.GetLevel()).DynamicExp/10000);
            }
            else
            {
                exp = tbFuben.ScanExp;
            }

            if (isByMail)
            {
                var items = new Dictionary<int, int>();
                items.Add((int) eResourcesType.ExpRes, exp);
                if (tbFuben.ScanGold > 0)
                {
                    items.Add((int) eResourcesType.GoldRes, tbFuben.ScanGold);
                }
                _this.mBag.AddItemByMail(53, items, null, eCreateItemType.Fuben, tbFuben.Name);
            }
            else
            {
                //发奖励
                _this.mBag.AddItem((int) eResourcesType.ExpRes, exp, eCreateItemType.Fuben);
                if (tbFuben.ScanGold > 0)
                {
                    _this.mBag.AddItem((int) eResourcesType.GoldRes, tbFuben.ScanGold, eCreateItemType.Fuben);
                }
            }

            var maxExpTimes = ActivityDungeonConstants.MaxExpTimes;
            var haploidDia = StaticParam.HaploidDia;
            SaveMultyExp(_this, tbFuben.AssistType, exp * (maxExpTimes - expTime), haploidDia * (maxExpTimes - expTime));
        }

        public void BroadCastGetEquip(CharacterController _this, int itemId, int dictId)
        {
            if (itemId < 600000)
            {
                // 服务器没有区分，先写死吧
                return;
            }

            var character = _this;
            var tbItem = Table.GetItemBase(itemId);
            if (tbItem == null)
                return;

            var strs = new List<string>();
            var characterName = Utils.AddCharacter(character.mGuid, character.Name);
            var itemName = Utils.AddItemId(tbItem.Id);
            strs.Add(characterName);
            strs.Add(itemName);
            //strs.Add(character.Name);
            //strs.Add(tbItem.Name);           

            var data = new ItemBaseData();          
            ShareItemFactory.Create(itemId, data);
            var content = Utils.WrapDictionaryId(dictId, strs, data.Exdata);
            var serverId = SceneExtension.GetServerLogicId(character.serverId);
            var chatAgent = LogicServer.Instance.ChatAgent;
            chatAgent.BroadcastWorldMessage((uint)serverId, (int)eChatChannel.SystemScroll, 0, string.Empty,
                new ChatMessageContent { Content = content });
        }

        public ErrorCodes SendNormalReward(CharacterController _this,
                                           FubenRecord tbFuben,
                                           bool useDiamond,
                                           bool isByMail)
        {
            var haploidDia = StaticParam.HaploidDia;
            var mBag = _this.mBag;
            if (useDiamond)
            { // 检查资源够不够
                if (mBag.GetRes(eResourcesType.DiamondRes) < haploidDia)
                {
                    return ErrorCodes.Error_ResNoEnough;
                }
                mBag.DelRes(eResourcesType.DiamondRes, haploidDia, eDeleteItemType.MultyExp);
            }

            var expTime = useDiamond ? ActivityDungeonConstants.MaxExpTimes : 1;
            var haploidExp = 0;
            if (tbFuben.IsDynamicExp == 1)
            {
                haploidExp = (int) (1.0*tbFuben.DynamicExpRatio*Table.GetLevelData(_this.GetLevel()).DynamicExp/10000);
            }
            var exp = haploidExp * expTime;

            //发奖励
            var itemIds = tbFuben.RewardId;
            var itemCounts = tbFuben.RewardCount;
            var items = new Dictionary<int, int>();
            for (int i = 0, imax = itemIds.Length; i < imax; i++)
            {
                var itemId = itemIds[i];
                if (itemId == -1)
                {
                    break;
                }
                var itemCount = itemCounts[i];
                items.Add(itemId, itemCount);
            }
            if (exp > 0)
            {
                items.modifyValue((int) eResourcesType.ExpRes, exp);
            }
            if (isByMail)
            {
                mBag.AddItemByMail(53, items, null, eCreateItemType.Fuben, tbFuben.Name);
            }
            else
            {
                mBag.AddItemOrMail(53, items, null, eCreateItemType.Fuben, tbFuben.Name);
            }

            const int maxExpTimes = ActivityDungeonConstants.MaxExpTimes;
            SaveMultyExp(_this, tbFuben.AssistType, haploidExp * (maxExpTimes - expTime), haploidDia * (maxExpTimes - expTime));

            return ErrorCodes.OK;
        }

        public static void GetMultyExpExData(int dungeonType, ref int expDefine, ref int diamondDefine)
        {
            switch ((eDungeonAssistType)dungeonType)
            {
                case eDungeonAssistType.DevilSquare: //恶魔广场
                {
                    expDefine = (int) eExdataDefine.e592;
                    diamondDefine = (int) eExdataDefine.e594;
                }
                    break;
                case eDungeonAssistType.BloodCastle: //血色城堡
                {
                    expDefine = (int)eExdataDefine.e591;
                    diamondDefine = (int)eExdataDefine.e593;
                }
                    break;
                default:
                    break;
            }            
        }

        // 计算血色城堡和恶魔广场多倍经验
        public void SaveMultyExp(CharacterController _this, int dungeonType, int extralExp, int extralDiamond)
        {
            if (extralExp <= 0 || extralDiamond < 0)
                return;

            int expDefine = -1, diamondDefine = -1;
            GetMultyExpExData(dungeonType, ref expDefine, ref diamondDefine);

            if (expDefine > 0 && diamondDefine > 0)
            {
                var oldExp = _this.GetExData(expDefine);
                var oldDiamond = _this.GetExData(diamondDefine);
                _this.SetExData(expDefine, oldExp + extralExp); // exp
                _this.SetExData(diamondDefine, oldDiamond + extralDiamond); // diamond
            }
        }

        public ErrorCodes TakeMultyExpAward(CharacterController _this, int id)
        {
            int dungeonType = -1;
            if (id == 0)
            {
                dungeonType = (int) eDungeonAssistType.DevilSquare;
            }
            else if (id == 1)
            {
                dungeonType = (int)eDungeonAssistType.BloodCastle;
            }
            else
            {
                return ErrorCodes.ParamError;
            }
            int expDefine = -1, diamondDefine = -1;
            GetMultyExpExData(dungeonType, ref expDefine, ref diamondDefine);
            var exp = _this.GetExData(expDefine);
            var diamond = _this.GetExData(diamondDefine);
            if (exp > 0 && diamond > 0)
            {
                if (_this.mBag.GetRes(eResourcesType.DiamondRes) < diamond)
                {
                    return ErrorCodes.Error_ResNoEnough;
                }
                _this.mBag.DelRes(eResourcesType.DiamondRes, diamond, eDeleteItemType.MultyExp);
                _this.SetExData(expDefine, 0); // exp
                _this.SetExData(diamondDefine, 0); // diamond

                var items = new Dictionary<int, int>();
                items.modifyValue((int) eResourcesType.ExpRes, exp);
                _this.mBag.AddItemOrMail(53, items, null, eCreateItemType.Fuben);

                return ErrorCodes.OK;                
            }

            return ErrorCodes.Error_ExpNotEnough;
        }

        public ErrorCodes ReceiveCompensation(CharacterController _this, int comId, int type)
        {
            var comList = _this.mDbData.Compensations;
            Compensation com;
            if (!comList.Compensations.TryGetValue(comId, out com))
            {
                return ErrorCodes.Error_CompensationNotFind;
            }
            var cost = calculateCompensationCost(com.Data, type);
            var err = CompensationAddItem(_this, type, cost, com.Data);
            if (err == ErrorCodes.OK)
            {
                comList.Compensations.Remove(comId);
            }
            return err;
        }

        public ErrorCodes ReceiveAllCompensation(CharacterController _this, int type)
        {
            var comList = _this.mDbData.Compensations.Compensations;
            var cost = 0;
            var data = new Dictionary<int, int>();
            foreach (var com in comList)
            {
                cost += calculateCompensationCost(com.Value.Data, type);
                foreach (var i in com.Value.Data)
                {
                    data.modifyValue(i.Key, i.Value);
                }
            }
            var err = CompensationAddItem(_this, type, cost, data);
            if (err == ErrorCodes.OK)
            {
                comList.Clear();
            }
            return err;
        }

        public ErrorCodes SetTitle(CharacterController _this, int id)
        {
            var tbNameTitle = Table.GetNameTitle(id);
            if (tbNameTitle == null)
            {
                return ErrorCodes.Error_NameTitleID;
            }
            return SetTitle(_this, id, tbNameTitle.Pos);
        }

        public int GetTitle(CharacterController _this, int idx)
        {
            var titles = _this.mDbData.ViewTitles;
            if (titles.Count <= idx)
            {
                return -1;
            }
            return titles[idx];
        }

        private ErrorCodes CheckServerIds(CharacterController cl, int serverId, List<int> serverIds, RechargeActiveRecord tbRA)
        {
            serverId = SceneExtension.GetServerLogicId(serverId);
            if (!serverIds.Contains(-1) && !serverIds.Contains(serverId))
            {
                return ErrorCodes.ServerID;
            }
            var now = DateTime.Now;
            var rule = (eRechargeActivityOpenRule)tbRA.OpenRule;
            switch (rule)
            {
                case eRechargeActivityOpenRule.Last:
                    break;
                case eRechargeActivityOpenRule.LimitTime:
                    {
                        if (!string.IsNullOrWhiteSpace(tbRA.StartTime))
                        {
                            var startTime = DateTime.Parse(tbRA.StartTime);
                            if (now < startTime)
                            {
                                //没在活动时间内
                                return ErrorCodes.Error_AnswerNotTime;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(tbRA.EndTime))
                        {
                            var endTime = DateTime.Parse(tbRA.EndTime);

                            if (tbRA.Type == 2) //是投资活动 而且买了 就延长7天
                            {
                                var sonId = tbRA.SonType;
                                if (sonId >= 0)
                                {
                                    var tbTouZi = Table.GetRechargeActiveCumulative(sonId);
                                    if (tbTouZi != null && cl != null)
                                    {
                                        var exdataId = tbTouZi.FlagTrueId;
                                        if (cl.GetFlag(exdataId))
                                        {
                                            endTime = endTime.AddDays(7);
                                        }
                                    }
                                }
                            }

                            if (now > endTime)
                            {
                                //没在活动时间内
                                return ErrorCodes.Error_AnswerNotTime;
                            }
                        }
                    }
                    break;
                case eRechargeActivityOpenRule.NewServerAuto:
                    {
                        //开服一段时间内可投资
                        int startHour, endHour;
                        if (int.TryParse(tbRA.StartTime, out startHour) && int.TryParse(tbRA.EndTime, out endHour))
                        {
                            if (tbRA.Type == 2) //是投资活动 而且买了 就延长7天
                            {
                                var sonId = tbRA.SonType;
                                if (sonId >= 0)
                                {
                                    var tbTouZi = Table.GetRechargeActiveCumulative(sonId);
                                    if (tbTouZi != null && cl != null)
                                    {
                                        var exdataId = tbTouZi.FlagTrueId;
                                        if (cl.GetFlag(exdataId))
                                        {
                                            endHour = endHour + (7 * 24);
                                        }
                                    }
                                }
                            }
                            var age = Utils.GetServerAge(serverId);
                            var hour = age.TotalHours;
                            if (hour < startHour || hour > endHour)
                            {
                                return ErrorCodes.Error_AnswerNotTime;
                            }
                        }
                    }
                    break;
            }
            return ErrorCodes.OK;
        }

        private void TouZiSuccess(CharacterController _this, int payType)
        {
            if (_this == null)
            {
                Logger.Error("Tou Zi faild CharacterController == null when add");
                return;
            }
            var dd = payType / 1000;
            if (dd != 3)
            {
                Logger.Error("Tou Zi faild payType error when add charid");
                return;
            }
            var ss = Table.GetRecharge(payType % 1000);
            if (ss == null || ss.Param.Length < 1)
            {
                Logger.Error("Tou Zi faild table error when add");
                return;
            }
            var tbRAC = Table.GetRechargeActiveCumulative(ss.Param[0]);
            if (tbRAC == null)
            {
                Logger.Error("Tou Zi faild table error when add");
                return;
            }
            var id = tbRAC.Id;

            PlayerLog.WriteLog(_this.mGuid, "----------Logic----------Investment----------:{0}", id);
            var character = _this;
            var tbRA = Table.GetRechargeActive(tbRAC.ActivityId);
            if (tbRA == null)
            {
                Logger.Error("RechargeActive Table value is not correct! id = {0}", tbRAC.ActivityId);
                return;
            }
            var err = CheckServerIds(_this, character.serverId, tbRA.ServerIds, tbRA);
            if (err != ErrorCodes.OK)
            {
                return;
            }
            var result = character.CheckCondition(tbRAC.ConditionId);
            if (result != -2)
            {
                return;
            }

            //character.mBag.DeleteItem(tbRAC.NeedItemId, tbRAC.NeedItemCount, eDeleteItemType.Investment);
            character.SetFlag(tbRAC.FlagTrueId);
            foreach (var flagId in tbRAC.FlagFalseId)
            {
                character.SetFlag(flagId, false);
            }
            if (tbRAC.ResetCount != -1)
            {
                var tbExdata = Table.GetExdata(tbRAC.ExtraId);
                if (tbExdata != null)
                {
                    character.SetExData(tbRAC.ExtraId,
                        MyRandom.Random(tbExdata.RefreshValue[0], tbExdata.RefreshValue[1]));
                }
            }
        }
        public void OnRechargeSuccess(CharacterController _this, string platform, int type, float price)
        {

            var payType = type;
            if (type >= 1000)
            {
                type = type / 1000;
            }

            var characterid = _this.mGuid;

            RechargeLogger.Info("OnRechargeSuccess ,characterid:{0},platfrom:{1},type:{2},price:{3} step 1",
                characterid, platform, type, price);
            //查奖励
            var rechargeId = -1;
            var rechargeData = StaticParam.RechargeData;
            Dictionary<int, Dictionary<int, int>> types;
            if (rechargeData.TryGetValue(platform, out types))
            {
                Dictionary<int, int> prices;
                if (type == 3)
                {
                    var tableId = payType % 1000;
                    var tbRechargeTable = Table.GetRecharge(tableId);
                    if (Math.Abs(tbRechargeTable.Price - price) < 0.01f)
                    {
                        rechargeId = tableId;
                    }
                    else
                    {
                        rechargeId = -1;
                    }
                }
                else
                {
                    if (types.TryGetValue(type, out prices))
                    {
                        if (!prices.TryGetValue((int)price, out rechargeId))
                        {
                            RechargeLogger.Warn(
                                "OnRechargeSuccess can't find price on table,characterid:{0},platfrom:{1},type:{2},price:{3} warn 1",
                                characterid, platform, type, price);
                            rechargeId = -1;
                        }
                    } 
                }
            }

            var now = DateTime.Now;
            StaticParam.RLogger.Info("OnRechargeSuccess! rechargeId = {0} step 2", rechargeId);
            RechargeLogger.Info("OnRechargeSuccess! rechargeId = {0}", rechargeId);
            var items = new Dictionary<int, int>();
            var tbRecharge = Table.GetRecharge(rechargeId);
            if (tbRecharge != null)
            {
                var count = GetExData(_this, tbRecharge.ExdataId) + 1;
                var addDiamond = 0;
                if (count == tbRecharge.ExTimes || tbRecharge.ExTimes < 0)
                {
                    addDiamond = tbRecharge.ExDiamond;
                }
                else if (tbRecharge.Param[2] < 0 || count <  tbRecharge.Param[2])
                {
                    addDiamond = tbRecharge.NormalDiamond;
                }
                items.modifyValue((int) eResourcesType.DiamondRes, tbRecharge.Diamond);
                if (addDiamond > 0)
                {
                    items.modifyValue((int)eResourcesType.DiamondRes, addDiamond);
                }
                items.modifyValue((int) eResourcesType.VipExpRes, tbRecharge.VipExp);
                RechargeLogger.Info("OnRechargeSuccess! modify diamond:{0},vipexp:{1} step 3", addDiamond,
                    tbRecharge.VipExp);
                // exdata
                AddExData(_this, tbRecharge.ExdataId, 1);
                AddExData(_this, (int) eExdataDefine.e69, 1);
                //AddExData(_this, StaticParam.CumulativeRechargeExdataId, tbRecharge.Diamond);
                AddExData(_this, (int)eExdataDefine.e561, tbRecharge.Diamond);
                AddExData(_this, StaticParam.CumulativeRechargeEverydayExdataId, tbRecharge.Diamond);
				AddExData(_this, (int)eExdataDefine.e78_TotalRechargeDiamond, tbRecharge.Diamond);
                RechargeLogger.Info("OnRechargeSuccess! modify exdata:{0} step 4", tbRecharge.ExdataId);

                var exRechargeId = tbRecharge.Param[3];
                if (exRechargeId != -1)
                {
                    var tbExRecharge = Table.GetRecharge(exRechargeId);
                    if (tbExRecharge.Type == 5)//周卡
                    {
                        var date = _this.lExdata64.GetTime(Exdata64TimeType.WeekCardExpirationDate);
                        if (date < now)
                        {
                            date = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Local);
                            _this.lExdata64.SetTime(Exdata64TimeType.WeekCardStartDate, date);
                            _this.SetExData((int)eExdataDefine.e779, 0);
                        }
                        _this.lExdata64.SetTime(Exdata64TimeType.WeekCardExpirationDate, date.AddDays(7));
                    }
                }

                if (type == 0)
                {
                    var date = _this.lExdata64.GetTime(Exdata64TimeType.MonthCardExpirationDate);
                    if (date < now)
                    {
                        date = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Local);
                        _this.lExdata64.SetTime(Exdata64TimeType.MonthCardStartDate, date);
                        _this.SetExData((int)eExdataDefine.e778, 0);
                    }
                    _this.lExdata64.SetTime(Exdata64TimeType.MonthCardExpirationDate, date.AddDays(30));
                    RechargeLogger.Info("OnRechargeSuccess! modify montycard expiration date:{0} step 4",
                        date.AddDays(30));
                }

                if (type == 4)//终身卡
                {
                    SetFlag(_this, 2682);//设置终身卡获得状态
                    var buffId = tbRecharge.Param[1];
                    var tbBuff = Table.GetBuff(buffId);
                    if (null != tbBuff)
                    {
                        SkillChange(_this, tbBuff.Type, -5, 1);
                    }
                }

				try
				{
					if (null != _this.mOperActivity)
					{
						//_this.mOperActivity.OnRechargeSuccess(tbRecharge.Diamond);
						_this.mOperActivity.OnRechargeSuccess(Math.Max(0,(int)Math.Floor(price * 10)));
					}
				}
				catch (Exception e)
				{
					Logger.Fatal(e.Message);
				}

	            try
	            {
					LogicServerMonitor.DiamondNumber.Increment(addDiamond);
	            }
				catch (Exception e)
	            {
					Logger.Fatal(e.Message);
	            }
            }
            else
            {
                var isOpen = Table.GetServerConfig(8);
                if (isOpen.Value.Equals("1"))
                {
                    var diamond = (int) Math.Round(price*10);
                    items.modifyValue((int) eResourcesType.DiamondRes, diamond);
                    items.modifyValue((int) eResourcesType.VipExpRes, diamond);
                    RechargeLogger.Warn("OnRechargeSuccess! Just modify DiamondRes price:{0}  warn 2", price);

                    // exdata
                    AddExData(_this, (int) eExdataDefine.e69, 1);
                    //AddExData(_this, StaticParam.CumulativeRechargeExdataId, diamond);
                    AddExData(_this, (int) eExdataDefine.e561, diamond);
                    AddExData(_this, StaticParam.CumulativeRechargeEverydayExdataId, diamond);
                    AddExData(_this, (int) eExdataDefine.e78_TotalRechargeDiamond, diamond);

                    try
                    {
                        if (null != _this.mOperActivity)
                        {
                            //_this.mOperActivity.OnRechargeSuccess(diamond);
                            _this.mOperActivity.OnRechargeSuccess(Math.Max(0, (int) Math.Floor(price*10)));
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Fatal(e.Message);
                    }
                    try
                    {
                        LogicServerMonitor.DiamondNumber.Increment(diamond);
                    }
                    catch (Exception e)
                    {
                        Logger.Fatal(e.Message);
                    }
                }
                else
                {
                    RechargeLogger.Warn("OnRechargeSuccess! cant find price on table ,price :", price);
                    return;
                }
            }
            AddExData(_this, (int)eExdataDefine.e654, (int)price);
            AddExData(_this, (int)eExdataDefine.e653, (int)price);
            Table.ForeachSuperVip(temp =>
            {
                if (temp.ServerID == _this.serverId)
                {
                    if (_this.GetExData((int)eExdataDefine.e654) >= temp.MonthRechargeNum || _this.GetExData((int)eExdataDefine.e653) >= temp.DayRechargeNum)
                    {
                        SetExData(_this, (int)eExdataDefine.e753, 1);
                    }
                    return false;
                }
                return true;
            });
            // 增加累计充值的现金（人民币）
            AddExData(_this, (int)eExdataDefine.e625, (int)price);
            SetExData(_this, (int)eExdataDefine.e652, (int)price);
            // 添加物品
            foreach (var item in items)
            {
                StaticParam.RLogger.Info("Add item {0}:{1}", Table.GetItemBase(item.Key).Name, item.Value);
                _this.mBag.AddItem(item.Key, item.Value, eCreateItemType.Recharge);
            }

            //计算连续充值天数
            var dayDiamond = GetExData(_this, StaticParam.CumulativeRechargeEverydayExdataId);
            if (dayDiamond >= StaticParam.CumulativeRechargeMinDiamonds)
            {
                var time = _this.lExdata64.GetTime(Exdata64TimeType.LastRechargeTime);
                var exdataId = StaticParam.CumulativeRechargeDaysExdataId;
                var continuous = GetExData(_this, exdataId);
                if (continuous == 0)
                {
                    SetExData(_this, exdataId, 1);
                }
                else
                {
                    var duration = now.Date - time.Date;
                    var day = (int) Math.Floor(duration.TotalDays);
                    if (day == 1)
                    {
                        AddExData(_this, exdataId, 1);
                    }
                    else if (day > 1)
                    {
                        SetExData(_this, exdataId, 1);
                    }
                }
                _this.lExdata64.SetTime(Exdata64TimeType.LastRechargeTime, now);
                //_this.SetRankFlag(RankType.RechargeDaily);
            }

			_this.SetRankFlag(RankType.RechargeTotal);

            // 通知客户端，充值成功
            if (_this.Proxy != null)
            {
                _this.Proxy.NotifyRechargeSuccess(rechargeId);
                RechargeLogger.Info("OnRechargeSuccess! NotifyRechargeSuccess characterid:{0}, rechargeid:{1} step 5",
                    _this.Proxy.CharacterId, rechargeId);
            }

            // 投资相关处理
            if (rechargeId != -1 && payType >= 1000)
            {
                TouZiSuccess(_this, payType);
            }

            var chargeNum = GetExData(_this, (int) eExdataDefine.e69);
            if (chargeNum == 1 && _this.Proxy != null && _this.Proxy.Character != null && _this.Proxy.Character.mBag != null && tbRecharge != null)
            {
                try
                {
                    // 首冲日志 charid, serverid, viplv, charlevel, charjob, createtime, charname, gold, gamegold, firstrechargetime
                    string v = string.Format("chardesc#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                                        _this.Proxy.CharacterId,
                                        _this.serverId,
                                        _this.Proxy.Character.mBag.GetRes(eResourcesType.VipLevel),
                                        _this.Proxy.Character.GetLevel(),
                                        _this.Proxy.Character.GetRole(),
                                        DateTime.MinValue,
                                        _this.Proxy.Character.GetName(),
                                        price,
                                        tbRecharge.Diamond,
                                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                    kafaLogger.Info(v);
                }
                catch (Exception e)
                { 
                    Logger.Error("kafka chardesc error{0}", e.Message);
                }
            }

// 	        try
// 	        {
// 		        if (null != _this.mOperActivity)
// 		        {
// 			        _this.mOperActivity.OnRechargeSuccess(platform, type, price);
// 		        }
// 	        }
// 	        catch (Exception e)
// 	        {
// 		        
// 		        Logger.Error(e.Message);
// 	        }
        }

        public void ModityTitle(CharacterController _this, int titleId, bool active)
        {
            var tbNameTitle = Table.GetNameTitle(titleId);
            if (tbNameTitle == null)
            {
                Logger.Error("In ModityTitle(). tbNameTitle == null, id = {0}", titleId);
                return;
            }
            SetFlag(_this, tbNameTitle.FlagId, active);
        }

        public void ModityTitles(CharacterController _this, List<int> titleIds, List<bool> states)
        {
            if (titleIds.Count != states.Count)
            {
                return;
            }
            var titles = _this.mDbData.Titles;
            var viewTitles = _this.mDbData.ViewTitles;
            for (int i = 0, imax = titleIds.Count; i < imax; ++i)
            {
                var title = titleIds[i];
                var state = states[i];
                if (state)
                {
                    if (!titles.ContainsKey(title))
                    {
                        titles.Add(title, 0);
                        CheckSetTitle(_this, title);
                    }
                }
                else
                {
                    if (titles.ContainsKey(title))
                    {
                        titles.Remove(title);
                    }
                    for(int idx=0;idx<viewTitles.Count ;idx ++)
                    {
                        if(viewTitles[idx] == title)
                        {
                            viewTitles[idx] = -1;
                            break ;
                        }
                    }
                }
            }
            NotifySceneTitleList(_this);
            for (int i = 0, imax = titleIds.Count; i < imax; ++i)
            {
                var title = titleIds[i];
                var state = states[i];
                ModityTitle(_this, title, state);
            }
        }

        public void OnVipLevelChanged(CharacterController _this,int oldLevel,int newLevel)
        {
            //vip level发生变化时，修改称号
            var vipLv = _this.mBag.GetRes(eResourcesType.VipLevel);
            var titles = new List<int>();
            var states = new List<bool>();
            for (var i = 0; i < vipLv; i++)
            {
                titles.Add(4000 + i);
                states.Add((i + 1) == vipLv);
            }
            ModityTitles(_this, titles, states);
            //VIP6以上发公告
            if (newLevel >= 6)
            {
                var args = new List<string>
                {
                    Utils.AddCharacter(_this.mGuid,_this.GetName()),
                    newLevel.ToString()
                };
                var exExdata = new List<int>();
                _this.SendSystemNoticeInfo(100003100, args, exExdata);
            }
            if(oldLevel != newLevel)
            {
                //每日只有第一次VIP变化客户端奖励显示“领取”，以后每次显示“已领取”然后以邮件形式发送奖励
                _this.AddExData(646, 1);
                if (_this.GetExData(646) <= 1)
                {
                    if (_this.GetFlag(2506))
                    {
                        for (int i = oldLevel + 1; i < newLevel; i++)
                        {
                            SendVIPRewardMail(_this, i);
                        }
                    }
                    else
                    {
                        for (int i = oldLevel; i < newLevel; i++)
                        {
                            if (i != 0)
                            {
                                SendVIPRewardMail(_this, i);
                            }
                        }
                    }
                    _this.SetFlag(2506, false);
                }
                else
                {
                    if (_this.GetFlag(2506))
                    {
                        for (int i = oldLevel + 1; i <= newLevel; i++)
                        {
                            SendVIPRewardMail(_this, i);
                        }
                    }
                    else
                    {
                        for (int i = oldLevel; i <= newLevel; i++)
                        {
                            if (i != 0)
                            {
                                SendVIPRewardMail(_this, i);
                            }
                        }
                    }
                    _this.SetFlag(2506, true);
                }
            }
            //处理一下pvp cd的问题
            var tbVip = Table.GetVIP(vipLv);
            if (tbVip.PKChallengeCD == 1)
            {
                var now = DateTime.Now;
                if (_this.lExdata64.GetTime(Exdata64TimeType.P1vP1CoolDown) > now)
                {
                    _this.lExdata64.SetTime(Exdata64TimeType.P1vP1CoolDown, now.AddYears(-1));
                }
            }
            //通知scene，我的viplevel改变了
            NotifyItemCountToScene(_this, (int)eResourcesType.VipLevel, vipLv);
            if(oldLevel<3 && vipLv>=3)
            {
                CoroutineFactory.NewCoroutine(AddPlayerToChatMonitorCoroutine, _this).MoveNext();
            }
        }
        private void SendVIPRewardMail(CharacterController _this, int viplevel)
        {
            var RewardDic = new Dictionary<int, int>();
            var tbVip = Table.GetVIP(viplevel);
            var vipReward = tbVip.PackageId;
            if (null == vipReward)
            {
                return;
            }
            var rewards = vipReward.Split('|');
            foreach (var item in rewards)
            {
                var reward =  item.Split('*');
                var id = int.Parse(reward[0]);
                var count = int.Parse(reward[1]);
                RewardDic.Add(id, count);
            }
            var tbMail = Table.GetMail(156);
            var title = tbMail.Title;
            var content = tbMail.Text;
            _this.mMail.PushMail(title, content, RewardDic, tbMail.Sender);
        }
        public void CheckEquipEnhanceTitle(CharacterController _this)
        {
            var nowMinLevel = -1;
            foreach (var i in StaticParam.EquipTitles)
            {
                if (_this.GetFlag(i.Value))
                {
                    nowMinLevel = i.Key;
                    break;
                }
            }

            var minLevel = 999;
            foreach (var equipParam in StaticParam.EquipList)
            {
                if (equipParam.BagId == (int)eBagType.Equip12)
                {
                    //跳过副手
                    continue;
                }
                var equip = _this.GetItemByBagByIndex(equipParam.BagId, equipParam.BagIdx);
                if (equip == null)
                {
                    minLevel = 0;
                    break;
                }
                minLevel = Math.Min(minLevel, equip.GetExdata(0));
            }
            if (minLevel <= nowMinLevel + 1)
            {
                return;
            }
            var trueFlagId = -1;
            switch (minLevel)
            {
                case 7:
                case 8:
                    trueFlagId = StaticParam.EquipTitles[0].Value;
                    SetExData(_this, (int)eExdataDefine.e688, (int)Star.Star1);
                    break;
                case 9:
                case 10:
                    trueFlagId = StaticParam.EquipTitles[1].Value;
                    SetExData(_this, (int)eExdataDefine.e688, (int)Star.Star2);
                    break;
                case 11:
                case 12:
                    trueFlagId = StaticParam.EquipTitles[2].Value;
                    SetExData(_this, (int)eExdataDefine.e688, (int)Star.Star3);
                    break;
                case 13:
                case 14:
                    trueFlagId = StaticParam.EquipTitles[3].Value;
                    SetExData(_this, (int)eExdataDefine.e688, (int)Star.Star4);
                    break;
                case 15:
                    trueFlagId = StaticParam.EquipTitles[4].Value;
                    SetExData(_this, (int)eExdataDefine.e688, (int)Star.Star5);
                    break;
                default:
                    SetExData(_this, (int)eExdataDefine.e688, (int)Star.Star0);
                    return;
            }
            foreach (var i in StaticParam.EquipTitles)
            {
                SetFlag(_this, i.Value, i.Value == trueFlagId);
            }
        }

        public void RemoveOverTimeTitles(CharacterController _this)
        {
            var toRemoveKeys = new List<int>();
            var nowTimeSpame = DateTime.Now.GetTimeStampSeconds();
            foreach (var title in _this.mDbData.Titles)
            {
                if (title.Value > 0 && nowTimeSpame >= title.Value)
                { // 时间到了
                    toRemoveKeys.Add(title.Key);
                }
            }

            foreach (var removeKey in toRemoveKeys)
            {
                var tbNameTitle = Table.GetNameTitle(removeKey);
                if (tbNameTitle != null)
                {
                    SetFlag(_this, tbNameTitle.FlagId, false);
                }
            }
        }

        public void RefreshSkillTitle(CharacterController _this)
        {
            var skills = _this.mSkill.mDbData.Skills;
            var i = 3103;
            for (; i >= 3100; --i)
            {
                var tbNameTitle = Table.GetNameTitle(i);
                if (GetFlag(_this, tbNameTitle.FlagId))
                {
                    break;
                }
                var param = StaticParam.SkillTitleParams[i];
                var count = skills.Count(skill => skill.Value >= param.Level);
                if (count >= param.Count)
                {
                    SetFlag(_this, tbNameTitle.FlagId);
                    break;
                }
            }
            for (var j = i - 1; j >= 3100; --j)
            {
                var tbNameTitle = Table.GetNameTitle(j);
                SetFlag(_this, tbNameTitle.FlagId, false);
            }
        }

        public void CheckDailyActivity(CharacterController _this, List<DailyActivityRecord> records)
        {
            foreach (var record in records)
            {
//每日活跃度
                if (record.ActivityCount == -1 || GetExData(_this, record.ExDataId) <= record.ActivityCount)
                {
                    AddExData(_this, (int) eExdataDefine.e15, record.ActivityValue);
                }
            }
        }

        public void OnShareScuess(CharacterController _this)
        {
            var table = Table.GetServerConfig(236);
            var addDiamond = int.Parse(table.Value);
            if (addDiamond < 1)
            {
                return;
            }
            _this.mBag.AddItem((int) eResourcesType.DiamondRes, addDiamond, eCreateItemType.ShareSuccess);
        }

        public ErrorCodes GmCommand(CharacterController _this, string command)
        {
            PlayerLog.WriteLog(_this.mGuid, "----------Logic----------GMLogic----------{0}", command);
            var result = GameMaster.mData.DoCommand(command, _this.mGuid);
            if (result != null)
            {
                return (ErrorCodes) result;
            }

            var strs = command.Split(',');
            if (strs.Length < 1)
            {
                return ErrorCodes.ParamError;
            }

            if (String.Compare(strs[0], "!!ReloadTable", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (strs.Length < 2)
                {
                    return ErrorCodes.ParamError;
                }
                GameMaster.ReloadTable(strs[1]);
                return ErrorCodes.OK;
            }
            if (String.Compare(strs[0], "!!UpdateServer", StringComparison.OrdinalIgnoreCase) == 0)
            {
                UpdateServer();
                return ErrorCodes.OK;
            }
            if (String.Compare(strs[0], "!!PetMissionDone", StringComparison.OrdinalIgnoreCase) == 0)
            {
                GameMaster.PetMissionDone(_this);
                return ErrorCodes.OK;
            }
            if (String.Compare(strs[0], "!!PetMissionRefresh", StringComparison.OrdinalIgnoreCase) == 0)
            {
                GameMaster.PetMissionRefresh(_this);
                return ErrorCodes.OK;
            }
            if (String.Compare(strs[0], "!!PushQAMail", StringComparison.OrdinalIgnoreCase) == 0)
            {
                var characterId = _this.mGuid;
                GameMaster.PushMailToSomeone(characterId,"title","text",new Dictionary<int, int>(),1);
                return ErrorCodes.OK;
            }
            if (String.Compare(strs[0], "!!PushMail", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (strs.Length < 2)
                {
                    return ErrorCodes.ParamError;
                }

                var attachments = new Dictionary<int, int>();
                var characterId = _this.mGuid;
                if (strs.Length == 4)
                {
                    int TempInt;
                    if (!Int32.TryParse(strs[3], out TempInt))
                    {
                        return ErrorCodes.ParamError;
                    }
                    // 添加附件
                    attachments.Add(TempInt, 1);
                }
                else if (strs.Length == 5)
                {
                    int TempInt;
                    if (!Int32.TryParse(strs[3], out TempInt))
                    {
                        return ErrorCodes.ParamError;
                    }
                    int TempInt2;
                    if (!Int32.TryParse(strs[4], out TempInt2))
                    {
                        return ErrorCodes.ParamError;
                    }
                    attachments.Add(TempInt, TempInt2);
                }
                else if (strs.Length == 6)
                {
                    ulong Guid;
                    if (!ulong.TryParse(strs[5], out Guid))
                    {
                        return ErrorCodes.ParamError;
                    }
                    characterId = Guid;

                    int TempInt;
                    if (!Int32.TryParse(strs[3], out TempInt))
                    {
                        return ErrorCodes.ParamError;
                    }

                    int TempInt2;
                    if (!Int32.TryParse(strs[4], out TempInt2))
                    {
                        return ErrorCodes.ParamError;
                    }

                    attachments.Add(TempInt, TempInt2);
                }

                GameMaster.PushMailToSomeone(characterId, strs[1], strs[2], attachments);
                return ErrorCodes.OK;
            }

            if (String.Compare(strs[0], "!!Save", StringComparison.OrdinalIgnoreCase) == 0)
            {
                switch (strs.Length)
                {
                    case 2:
                        CoroutineFactory.NewCoroutine(GameMaster.SaveSnapShot, _this, strs[1], false).MoveNext();
                        break;
                    case 3:
                        CoroutineFactory.NewCoroutine(GameMaster.SaveSnapShot, _this, strs[1],
                            strs[2].Equals("1") || strs[2].Equals("true")).MoveNext();
                        break;
                    default:
                        return ErrorCodes.ParamError;
                }
                return ErrorCodes.OK;
            }
            if (String.Compare(strs[0], "!!Load", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (strs.Length == 2)
                {
                    CoroutineFactory.NewCoroutine(GameMaster.LoadSnapShot, _this, strs[1]).MoveNext();
                }
                else
                {
                    return ErrorCodes.ParamError;
                }
                return ErrorCodes.OK;
            }

            var nIndex = 0;
            var IntData = new List<int>();
            foreach (var s in strs)
            {
                if (nIndex != 0)
                {
                    int TempInt;
                    if (!Int32.TryParse(s, out TempInt))
                    {
                        return ErrorCodes.ParamError;
                    }
                    IntData.Add(TempInt);
                }
                nIndex++;
            }

            if (String.Compare(strs[0], "!!FubenEnter", StringComparison.OrdinalIgnoreCase) == 0)
            {
                var param = new SceneParam();
                for (var i = 1; i < IntData.Count; ++i)
                {
                    param.Param.Add(IntData[i]);
                }
                GameMaster.EnterFuben(_this, IntData[0], param);
            }
            else if (String.Compare(strs[0], "!!AddItem", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 2)
                {
                    GameMaster.GmAddItem(_this, IntData[0], IntData[1]);
                }
                else
                {
                    return ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!SetLevel", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    GameMaster.GmSetLevel(_this, IntData[0]);
                }
                else
                {
                    return ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!SetEquip", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    GameMaster.GmSetEquip(_this, IntData[0]);
                }
                else
                {
                    return ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!SetEquipAttr", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 2)
                {
                    GameMaster.GmSetEquipAttr(_this, IntData[0], IntData[1]);
                }
                else
                {
                    return ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!SetSkillLevel", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 2)
                {
                    GameMaster.GmSetSkillLevel(_this, IntData[0], IntData[1]);
                }
                else
                {
                    return ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!AddSkill", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 2)
                {
                    GameMaster.GmAddSkillLevel(_this, IntData[0], IntData[1]);
                }
                else
                {
                    return ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!AddTalent", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    GameMaster.GmAddTalent(_this, IntData[0]);
                }
                else if (IntData.Count == 2)
                {
                    GameMaster.GmAddTalent(_this, IntData[0], IntData[1]);
                }
                else
                {
                    return ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!ResetTalent", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 0)
                {
                    GameMaster.GmResetTalent(_this);
                }
                else if (IntData.Count == 1)
                {
                    GameMaster.GmResetTalent(_this, IntData[0]);
                }
                else
                {
                    return ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!MissionComplete", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    GameMaster.GmMissionComplete(_this, IntData[0]);
                }
                else if (IntData.Count == 2)
                {
                    GameMaster.GmMissionComplete(_this, IntData[0], IntData[1]);
                }
                else
                {
                    return ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!TestDrop", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 2)
                {
                    GameMaster.GmTestDrop(_this, IntData[0], IntData[1]);
                }
                else
                {
                    return ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!MissionParam", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 3)
                {
                    GameMaster.GmSetMissionParam(_this, IntData[0], IntData[1], IntData[2]);
                }
                else
                {
                    return ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!SetEquipGem", StringComparison.OrdinalIgnoreCase) == 0)
            {
                switch (IntData.Count)
                {
                    case 2:
                    {
                        GameMaster.GmSetEquipGem(_this, IntData[0], IntData[1]);
                    }
                        break;
                    case 3:
                    {
                        GameMaster.GmSetEquipGem(_this, IntData[0], IntData[1], IntData[2]);
                    }
                        break;
                    case 4:
                    {
                        GameMaster.GmSetEquipGem(_this, IntData[0], IntData[1], IntData[2], IntData[3]);
                    }
                        break;
                    case 5:
                    {
                        GameMaster.GmSetEquipGem(_this, IntData[0], IntData[1], IntData[2], IntData[3], IntData[4]);
                    }
                        break;
                    default:
                        return ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!Recharge", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (strs.Length == 2)
                {
                    CoroutineFactory.NewCoroutine(SendRechargeSuccess, _this, int.Parse(strs[1]), 1).MoveNext();
                }
                else if (strs.Length == 3)
                {
                    CoroutineFactory.NewCoroutine(SendRechargeSuccess, _this, int.Parse(strs[1]), int.Parse(strs[2])).MoveNext();
                }
            }
			else if (String.Compare(strs[0], "!!ReloadOperationActivity", StringComparison.OrdinalIgnoreCase) == 0)
			{
				OperationActivityManager.Instance.Reload();
			}
            else if (String.Compare(strs[0], "!!MissionCommit", StringComparison.OrdinalIgnoreCase) == 0)
            {
                try
                {
                    if (strs.Length >= 2)
                    {
                        bool isOver = false;
                        int num = 0;
                        while (!isOver)
                        {
                            if (num > 100000)
                            {
                                break;
                            }

                            var tbMis = Table.GetMission(int.Parse(strs[1]));
                            if (tbMis == null)
                            {
                                break;
                            }
                            if (_this.GetFlag(tbMis.FlagId))
                            {
                                break;
                            }

                            foreach (var data in _this.mTask.mDbData.Missions)
                            {
                                if (data.Value.Id == int.Parse(strs[1]))
                                {
                                    _this.mTask.Commit(_this, data.Value.Id, true);
                                    isOver = true;
                                    break;
                                }
                            }

                            if (isOver)
                            {
                                break;
                            }

                            foreach (var data in _this.mTask.mDbData.Missions)
                            {
                                var tbMission = Table.GetMission(data.Key);
                                if (tbMission.ViewType == 0)
                                {
                                    _this.mTask.Commit(_this, data.Value.Id, true);
                                    if (data.Value.Id == int.Parse(strs[1]))
                                    {
                                        isOver = true;
                                    }
                                    break;
                                }
                            }
                            ++num;
                        }
                    }
                }
                catch (Exception)
                {

                }
            }

            return ErrorCodes.OK;
        }

        IEnumerator SendRechargeSuccess(Coroutine co, CharacterController _this, int count, int type)
        {
            var reslut = LogicServer.Instance.LogicAgent.RechargeSuccess(_this.mGuid, "android", type, count, Guid.NewGuid().ToString(), "GMCommand.GMCommand");
            yield return reslut.SendAndWaitUntilDone(co);
        }

        #region 每天登陆

        //补偿算法
        public int CompensationCount(CharacterController _this, int type, int baseValue)
        {
            switch (type)
            {
                case 1:
                {
                    var tbs = Table.GetSkillUpgrading(baseValue);
                    if (tbs == null)
                    {
                        return 0;
                    }
                    return tbs.GetSkillUpgradingValue(GetLevel(_this));
                }
                case 2:
                {
                    var tbl = Table.GetLevelData(GetLevel(_this));
                    if (tbl == null)
                    {
                        return 0;
                    }
                    return (int) (tbl.DynamicExp*1.0f*baseValue/10000);
                }
                case 3:
                {
                    var tbs = Table.GetSkillUpgrading(baseValue);
                    if (tbs == null)
                    {
                        return 0;
                    }
                    var tbl = Table.GetLevelData(GetLevel(_this));
                    if (tbl == null)
                    {
                        return 0;
                    }
                    var value = tbs.GetSkillUpgradingValue(GetLevel(_this));
                    return (int) (tbl.DynamicExp*1.0f*value/10000);
                }
                default:
                    return baseValue;
            }
            //return 0;
        }

        //事件处理
        private void DailyFirstOnlineByEvent(CharacterController _this, int type, int hour, int count)
        {
            PlayerLog.WriteLog(_this.mGuid, "DailyFirstOnlineByEvent");
            //P1VP1的被崇拜奖励
            var ex324 = GetExData(_this, 324);
            if (ex324 > CharacterController.MaxMobaiCount)
            {
                ex324 = CharacterController.MaxMobaiCount;
            }
            if (ex324 > 0)
            {
                var tbLevel = Table.GetLevelData(GetLevel(_this));
                if (tbLevel != null)
                {
                    var tbMail = Table.GetMail(89);
                    var mail = _this.mMail.PushMail(tbMail.Title, string.Format(tbMail.Text, GetName(_this), ex324),
                        new Dictionary<int, int>
                        {
                            {(int) eResourcesType.ExpRes, tbLevel.BeWorshipedExp*ex324},
                            {(int) eResourcesType.GoldRes, tbLevel.BeWorshipedMoney*ex324}
                        }, tbMail.Sender);
                    mail.OverTime = DateTime.Now.Date.AddDays(15).ToBinary();
                }
            }
            //清空昨天已崇拜的人
            _this.mDbData.Worships.Clear();
            //战斗历史列表，获取当前的名次
            if (_this.mDbData.P1vP1Change == null)
            {
                _this.mDbData.P1vP1Change = new P1vP1ChangeList();
            }
            var cc = _this.mDbData.P1vP1Change.Data.Count;
            if (cc > 0)
            {
                var oneChange = _this.mDbData.P1vP1Change.Data[cc - 1];
                var newRank = oneChange.NewRank;
                if (newRank == -1)
                {
                    newRank = oneChange.OldRank;
                }
                var tbArenaReward = Reward.GetArenaReward(newRank);
                if (tbArenaReward != null)
                {
                    var tbMail = Table.GetMail(97);
                    var reward = new Dictionary<int, int>();
                    if (tbArenaReward.DayMoney > 0)
                    {
                        reward.Add((int) eResourcesType.GoldRes, tbArenaReward.DayMoney);
                    }
                    if (tbArenaReward.DayDiamond > 0)
                    {
                        reward.Add((int) eResourcesType.DiamondBind, tbArenaReward.DayDiamond);
                    }
                    var index = 0;
                    foreach (var i in tbArenaReward.DayItemID)
                    {
                        if (i > 0 && tbArenaReward.DayItemCount[index] > 0)
                        {
                            reward.Add(i, tbArenaReward.DayItemCount[index]);
                        }
                        index++;
                    }
                    for (var i = 0; i < count; i++)
                    {
                        if (i >= 15)
                        {
                            break;
                        }
                        var mail = _this.mMail.PushMail(tbMail.Title,
                            string.Format(tbMail.Text, GetName(_this), newRank), reward, tbMail.Sender);
                        mail.OverTime = DateTime.Now.Date.AddDays(15 - i).ToBinary();
                    }
                }
            }
            //持续登陆时间
            var oldTime = _this.lExdata64.GetTime(Exdata64TimeType.FirstOnlineTime);
            var ex17 = 0;
            var isOnlineNextDay = false;
            if (DateTime.Now.Date == oldTime.AddDays(1).Date)
            {
                ex17 = GetExData(_this, 17);
                isOnlineNextDay = true;
            }
            //检查补偿数据是否已经过天
            var lastTime = _this.lExdata64.GetTime(Exdata64TimeType.LastOutlineTime);
            var compensationsIsNeedLookExdata = true;
            if (lastTime > oldTime)
            {
//刚登陆，上次离线时间>上次登陆时间(如果是已在线触发时，肯定是上次登陆>上次离线）
                if ((DateTime.Now.Date - lastTime.Date).TotalDays > 1)
                {
                    compensationsIsNeedLookExdata = false;
                }
            }
            //在清空这些标记位之前，需要记录一些数据（补偿数据）
            if (_this.mDbData.Compensations == null)
            {
                _this.mDbData.Compensations = new CompensationList();
            }
            var compensations = _this.mDbData.Compensations.Compensations;
            compensations.Clear();
            Table.ForeachCompensation(record =>
            {
                if (CheckCondition(_this, record.ConditionId) != -2)
                {
                    return true;
                }
                var needCount = 0;
                if (record.ExtraData != -1)
                {
                    if (compensationsIsNeedLookExdata)
                    {
                        var nowCount = GetExData(_this, record.ExtraData);
                        if (nowCount < record.MaxCount)
                        {
                            needCount = record.MaxCount - nowCount;
                        }
                    }
                    else
                    {
                        needCount = record.MaxCount;
                    }
                }
                if (record.Sign != -1)
                {
                    if (!compensationsIsNeedLookExdata)
                    {
                        needCount = 1;
                    }
                    else if (!GetFlag(_this, record.Sign))
                    {
                        needCount = 1;
                    }
                }
                if (needCount == 0)
                {
                    return true;
                }

                //补偿道具
                Compensation com;
                if (!compensations.TryGetValue(record.Type, out com))
                {
                    com = new Compensation();
                    com.Id = record.Type;
                    compensations.Add(record.Type, com);
                }
                com.Count += needCount;

                //经验
                var giveExp = CompensationCount(_this, record.ExpType, record.UnitExp);
                if (giveExp > 0)
                {
                    com.Data.modifyValue((int) eResourcesType.ExpRes, giveExp*needCount);
                }
                //金币 
                var giveGold = CompensationCount(_this, record.GoldType, record.UnitGold);
                if (giveGold > 0)
                {
                    com.Data.modifyValue((int) eResourcesType.GoldRes, giveGold*needCount);
                }
                //道具
                var index = -1;
                foreach (var i in record.UnitItem)
                {
                    index++;
                    if (i == -1)
                    {
                        continue;
                    }

                    var giveCount = CompensationCount(_this, record.ItemYype[index], record.ItemCount[index])*needCount;
                    com.Data.modifyValue(i, giveCount);
                }
                return true;
            });

            AddExData(_this, StaticParam.InvestmentLoginExdataId[0], 1);
            AddExData(_this, StaticParam.InvestmentLoginExdataId[1], 1);
            if (count == 1)
            {
                var c = GetExData(_this, (int) eExdataDefine.e564);
                SetExData(_this, (int) eExdataDefine.e564, c);
            }
            else if (count > 1)
            {
                SetExData(_this, (int) eExdataDefine.e564, 0);
            }
            var time = _this.lExdata64.GetTime(Exdata64TimeType.LastRechargeTime);
            var now = DateTime.Now;
            if (Math.Round((now.Date - time.Date).TotalDays) > 1)
            {
                SetExData(_this, StaticParam.CumulativeRechargeDaysExdataId, 0); //连续充值天数置0
            }
            //检查活动循环id
            foreach (var pair in StaticParam.RechargeActivityCircleIds)
            {
                var data = pair.Value;
                if (GetExData(_this, pair.Key) != data.CircleCount)
                {
                    SetExData(_this, pair.Key, data.CircleCount);
                    SetExData(_this, data.ExdataId, data.ExdataInit);
                    SetFlag(_this, data.FlagTrue, false);
                    foreach (var id in data.FlagFalse)
                    {
                        SetFlag(_this, id, false);
                    }
                }
            }
            //重置标记位，扩展数据
            _this.lFlag.ResetByTime(type, hour);
            _this.lExdata.ResetByTime(type, hour);
            _this.TodayTimes = 0;
            _this.OnlineTime = DateTime.Now;
            if (isOnlineNextDay)
            {
                DailyFirstOnline(_this, ex17 + 1);
            }
        }

        /// <summary>
        /// 竞技场结算时间(12点结算，0点结算再DailyFirstOnlineByEvent中)
        /// </summary>
        private void SettleArenaReward(CharacterController _this, int count)
        {
            var cc = _this.mDbData.P1vP1Change.Data.Count;
            if (cc > 0)
            {
                var oneChange = _this.mDbData.P1vP1Change.Data[cc - 1];
                var newRank = oneChange.NewRank;
                if (newRank == -1)
                {
                    newRank = oneChange.OldRank;
                }
                var tbArenaReward = Reward.GetArenaReward(newRank);
                if (tbArenaReward != null)
                {
                    var tbMail = Table.GetMail(97);
                    var reward = new Dictionary<int, int>();
                    if (tbArenaReward.DayMoney > 0)
                    {
                        reward.Add((int) eResourcesType.GoldRes, tbArenaReward.DayMoney);
                    }
                    if (tbArenaReward.DayDiamond > 0)
                    {
                        reward.Add((int) eResourcesType.DiamondBind, tbArenaReward.DayDiamond);
                    }
                    var index = 0;
                    foreach (var i in tbArenaReward.DayItemID)
                    {
                        if (i > 0 && tbArenaReward.DayItemCount[index] > 0)
                        {
                            reward.Add(i, tbArenaReward.DayItemCount[index]);
                        }
                        index++;
                    }
                    for (var i = 0; i < count; i++)
                    {
                        if (i >= 15)
                        {
                            break;
                        }
                        var mail = _this.mMail.PushMail(tbMail.Title,
                            string.Format(tbMail.Text, GetName(_this), newRank), reward, tbMail.Sender);
                        mail.OverTime = DateTime.Now.Date.AddDays(15 - i).ToBinary();
                    }
                }
            }
        }

        //登陆处理
        public void DailyFirstOnline(CharacterController _this, int continuedLanding)
        {
            var lastTime = _this.lExdata64.GetTime(Exdata64TimeType.FirstOnlineTime);
            if (DateTime.Now.Date == lastTime.Date)
            {
                return;
            }
            _this.lExdata64.SetTime(Exdata64TimeType.FirstOnlineTime, DateTime.Now);
            PlayerLog.DataLog(_this.mGuid, "lf");
            PlayerLog.WriteLog(_this.mGuid, "-----Logic:DailyFirstOnline={0}", continuedLanding);
            //结算上一天的内容
            //重置今天的内容
            SetExData(_this, (int) eExdataDefine.e17, continuedLanding);
            AddExData(_this, (int) eExdataDefine.e94, 1);
            //每日登陆奖励
            //_this.Gift(eActivationRewardType.TableGift, 101);
            //每日任务可能会有刷新
            _this.mTask.GetCanAcceptMission();


            

        }

        public void CheckEquipTrialEnd(CharacterController _this)
        {
            var bag = _this.mBag.GetBag((int)eBagType.Equip);
        }

        //下线
        public void OutLine(CharacterController _this)
        {
            Logger.Info("Logic Character OutLine {0}", _this.mGuid);
            PlayerLog.DataLog(_this.mGuid, "ld");
            SetFlag(_this, 513);
            foreach (var bagBase in _this.mBag.mBags)
            {
                var oldTimes = bagBase.Value.GetNextTime();
                if (oldTimes <= 0)
                {
                    continue;
                }
                var onLineCount = (int) DateTime.Now.GetDiffSeconds(_this.OnlineTime) - bagBase.Value.RemoveBuyTimes;
                if (oldTimes <= onLineCount)
                {
                    bagBase.Value.SetNextTime(0);
                }
                else
                {
                    var newCount = oldTimes - onLineCount;
                    bagBase.Value.SetNextTime(newCount);
                }
                bagBase.Value.MarkDbDirty();
            }
            RefreshTrialTime(_this);

            _this.lExdata64.SetTime(Exdata64TimeType.LastOutlineTime, DateTime.Now);
        }

        private void RefreshTrialTimeByBag(CharacterController _this, BagBase thisBagBase, bool equiped)
        {
            foreach (var ib in thisBagBase.mLogics)
            {
                if (ib.GetId() >= 0)
                {
                    var trialEnd = ib.TrialTimeCost();
                    if (equiped && trialEnd)
                    {
                        _this.EquipChange(2, thisBagBase.GetBagId(), ib.GetIndex(), ib);
                        _this.SetRankFlag(RankType.FightValue);
                    }

                }
            }
        }

        public void RefreshTrialTime(CharacterController _this)
        {
            foreach (var i in EquipExtension.Equips)
            {
                BagBase thisBagBase;
                if (_this.mBag.mBags.TryGetValue(i, out thisBagBase))
                {
                    RefreshTrialTimeByBag(_this, thisBagBase, true);
                }
            }

            RefreshTrialTimeByBag(_this, _this.GetBag((int)eBagType.Equip), false);
            RefreshTrialTimeByBag(_this, _this.GetBag((int)eBagType.Depot), false);
        }

        #endregion

        #region 扩展计数相关

        public List<int> GetExData(CharacterController _this)
        {
            return _this.lExdata.mData;
        }

        public int GetExData(CharacterController _this, int index)
        {
            if (0 > index || _this.lExdata.mData == null || _this.lExdata.mData.Count <= index)
            {
                Logger.Warn("GetExData Out Id={0}", index);
                return 0;
            }

            return _this.lExdata.mData[index];
        }

        public long GetExData64(CharacterController _this, int index)
        {
            if (0 > index || _this.lExdata64.mData.Count <= index)
            {
                Logger.Warn("GetExData64 Out Id={0}", index);
                return 0;
            }

            return _this.lExdata64.mData[index];
        }

        //设置扩展计数
        public void SetExData(CharacterController _this, int index, int value, bool forceNotToClient = false)
        {
            if (0 > index || _this.lExdata.mData.Count <= index)
            {
                return;
            }
            var oldValue = _this.lExdata.mData[index];
            if (value == oldValue)
            {
                return;
            }
            if (value > _this.lExdata.mData[index])
            {
                var eAdd = new CharacterExdataAddEvent(_this, index, value - _this.lExdata.mData[index]);
                EventDispatcher.Instance.DispatchEvent(eAdd);
            }
            PlayerLog.DataLog(_this.mGuid, "xe,{0},{1},{2}", index, oldValue, value);
            _this.lExdata.mData[index] = value;
            //触发事件
            var e = new CharacterExdataChange(_this, index, value);
            EventDispatcher.Instance.DispatchEvent(e);
            if (forceNotToClient)
            {
                _this.lExdata.MarkDbDirty();
            }
            else
            {
                _this.lExdata.MarkDirty();
                _this.lExdata.mNetDirtyList[index] = value;
            }
            if (index == (int) eExdataDefine.e51)
            {
                SkillChange(_this, 2, -2, value);
            }
            else if (index == (int) eExdataDefine.e250)
            {
                SkillChange(_this, 2, -3, value);
            }
            else if (index == (int) eExdataDefine.e428)
            {
                if (value >= StaticParam.ExpBattleFieldMaxPlayTimeSec)
                {
//古战场游戏时间到了，则修改古战场游戏次数为1
                    SetExData(_this, (int) eExdataDefine.e545, 1);
                }
            }

            //通知scene服务器
            if (index == (int)eExdataDefine.e630||
                index == (int)eExdataDefine.e632 || _this.syncSceneExDataIdx.ContainsKey(index) == true)
            {
                var data = new Dict_int_int_Data();
				data.Data.Add(index, value);
                CoroutineFactory.NewCoroutine(SyncSceneExData, _this.mGuid, data).MoveNext();
            }
        }

        //增加扩展计数
        public void AddExData(CharacterController _this, int index, int value, bool forceNotToClient = false)
        {
            if (0 > index || _this.lExdata.mData.Count <= index)
            {
                Logger.Warn("AddExData Out Id={0} addValue={1}", index, value);
                return;
            }
            var newValue = GetExData(_this, index) + value;
            SetExData(_this, index, newValue, forceNotToClient);
        }

        public void OnLevelUp(CharacterController _this, int lv)
        {
            //int oldLevel = mBag.GetRes(eResourcesType.LevelRes);
            var oldLevel = GetExData(_this, (int) eExdataDefine.e46);
            if (lv > oldLevel)
            {
                var roleid = GetRole(_this);
                var tbActor = Table.GetActor(roleid);
                var ladder = GetExData(_this, 51);
                if (tbActor == null || ladder < 0 || ladder > 4)
                {
                    Logger.Error("SetLevelRes  roleid={0},ladder={1}", roleid, ladder);
                }
                else
                {
                    AddExData(_this, (int) eExdataDefine.e52, tbActor.FreePoint[ladder]*(lv - oldLevel));
                }

                //自动加点放客户端做了
                //bool isAutoAdd = GetFlag(1001);
                //if (isAutoAdd)
                //{
                //    var add = GetExData((int) eExdataDefine.e52);
                //}
				_this.mOperActivity.OnPlayerLevelUp(lv);
            }
        }

        #endregion

        #region 标记位相关

        public bool GetFlag(CharacterController _this, int index)
        {
            return _this.lFlag.mData.GetFlag(index) == 1;
        }

        //设置扩展标记
        public void SetFlag(CharacterController _this, int index, bool flag = true, int forceNotToClient = 0)
        {
            if (-1 == index)
            {
                return;
            }
            if ((_this.lFlag.mData.GetFlag(index) == 1) == flag)
            {
                return;
            }
            if (flag)
            {
                PlayerLog.DataLog(_this.mGuid, "fe,{0},1", index);
                _this.lFlag.SetFlag(index, true, forceNotToClient == 1);
                if (index == PetMissionManager2.IsRefreshFlag)
                {
                    _this.mPetMission.Refresh();
                }
                //触发事件
                var e = new ChacacterFlagTrue(_this, index);
                EventDispatcher.Instance.DispatchEvent(e);
                //检查称号的激活
                CheckActiveTitle(_this, index, DateTime.Now);

                //每天第一次分享成功奖励钻石
                if (index == 559)
                {
                    OnShareScuess(_this);
                }
            }
            else
            {
                //分享标记为只能设置为true，不可设置为false，每天会自动重置为false
                if (index == 559)
                {
                    return;
                }
                PlayerLog.DataLog(_this.mGuid, "fe,{0},0", index);
                _this.lFlag.SetFlag(index, false, forceNotToClient == 1);
                //触发事件
                var e = new ChacacterFlagFalse(_this, index);
                EventDispatcher.Instance.DispatchEvent(e);
                //检查称号的激活
                CheckDeactiveTitle(_this, index);
            }

            if ( _this.syncSceneFlagIdx.ContainsKey(index) == true)
            {
                var data = new Dict_int_int_Data();
				data.Data.Add(index, 1);
                CoroutineFactory.NewCoroutine(SyncSceneFlagData, _this.mGuid, data).MoveNext();
            }
            
        }

        public void SetTitleFlag(CharacterController _this, int index, bool flag, int forceNotToClient, DateTime titleStartTime)
        {
            if (-1 == index)
            {
                return;
            }
            var lastFlag = (_this.lFlag.mData.GetFlag(index) == 1);
            if (flag)
            {
                if (lastFlag)
                { // 重置时间
                    CheckActiveTitle(_this, index, titleStartTime);
                    return;
                }
                //检查称号的激活
                CheckActiveTitle(_this, index, titleStartTime);

                PlayerLog.DataLog(_this.mGuid, "fe,{0},1", index);
                _this.lFlag.SetFlag(index, true, forceNotToClient == 1);

                //触发事件
                var e = new ChacacterFlagTrue(_this, index);
                EventDispatcher.Instance.DispatchEvent(e);
            }
            else
            {
                if (lastFlag == false)
                    return;

                PlayerLog.DataLog(_this.mGuid, "fe,{0},0", index);
                _this.lFlag.SetFlag(index, false, forceNotToClient == 1);
                //触发事件
                var e = new ChacacterFlagFalse(_this, index);
                EventDispatcher.Instance.DispatchEvent(e);

                CheckDeactiveTitle(_this, index);
            }            
        }

        #endregion

        #region 判断相关

        //判断条件
        public int CheckCondition(CharacterController _this, int nConditionId)
        {
            if (nConditionId == -1)
            {
                return -2;
            }
            var tbcon = Table.GetConditionTable(nConditionId);
	        if (null == tbcon)
	        {
				Logger.Fatal("null==Table.GetConditionTable({0})", nConditionId);
		        return -2;
	        }
            if (tbcon.Role != -1)
            {
                if (!BitFlag.GetLow(tbcon.Role, GetRole(_this)))
                {
                    return tbcon.RoleDict;
                }
            }
            for (var i = 0; i != tbcon.TrueFlag.Length; ++i)
            {
                if (tbcon.TrueFlag[i] == -1)
                {
                    continue;
                }
                if (!GetFlag(_this, tbcon.TrueFlag[i]))
                {
                    return tbcon.FlagTrueDict;
                }
            }
            for (var i = 0; i != tbcon.FalseFlag.Length; ++i)
            {
                if (tbcon.FalseFlag[i] == -1)
                {
                    continue;
                }
                if (GetFlag(_this, tbcon.FalseFlag[i]))
                {
                    return tbcon.FlagFalseDict;
                }
            }
            for (var i = 0; i != tbcon.ExdataId.Length; ++i)
            {
                if (tbcon.ExdataId[i] == -1)
                {
                    continue;
                }
                var nExdataValue = GetExData(_this, tbcon.ExdataId[i]);
                if (tbcon.ExdataMin[i] != -1 && nExdataValue < tbcon.ExdataMin[i])
                {
                    return tbcon.ExdataDict[i];
                }
                if (tbcon.ExdataMax[i] != -1 && nExdataValue > tbcon.ExdataMax[i])
                {
                    return tbcon.ExdataDict[i];
                }
            }
            for (var i = 0; i != tbcon.ItemId.Length; ++i)
            {
                var nItemId = tbcon.ItemId[i];
                if (nItemId < 0)
                {
                    continue;
                }
                var nCount = _this.mBag.GetItemCount(nItemId);
                if (tbcon.ItemCountMin[i] != -1 && nCount < tbcon.ItemCountMin[i])
                {
                    return tbcon.ItemDict[i];
                }
                if (tbcon.ItemCountMax[i] != -1 && nCount > tbcon.ItemCountMax[i])
                {
                    return tbcon.ItemDict[i];
                }
            }
            if (tbcon.OpenTime[0] > 0)
            {
                var endTime = DateTime.Now;
                var startTime = DateTime.Parse(Table.GetServerName(_this.serverId).OpenTime);
                var sm = startTime.Month;
                var em = endTime.Month;
                var sy = startTime.Year;
                var ey = endTime.Year;
                var diffMonth = (ey - sy)*12 + (em - sm);
                if (diffMonth < tbcon.OpenTime[0])
                {
                    return tbcon.OpenTimeDict[0];
                }
            }
            if (tbcon.OpenTime[1] > 0)
            {
                var endTime = DateTime.Now;
                var startTime = DateTime.Parse(Table.GetServerName(_this.serverId).OpenTime);
                var sy = startTime.Year;
                var ey = endTime.Year;
                var diffYear = ey - sy;
                if (diffYear < tbcon.OpenTime[1])
                {
                    return tbcon.OpenTimeDict[1];
                }
            }
            return -2;
        }

        //是否满足天梯判断条件
        public ErrorCodes CheckP1vP1(CharacterController _this)
        {
            if (GetExData(_this, 98) < 1)
            {
                return ErrorCodes.Error_CountNotEnough;
            }
            if (_this.lExdata64.GetTime(Exdata64TimeType.P1vP1CoolDown) > DateTime.Now)
            {
                return ErrorCodes.Error_LadderTime;
            }

            return ErrorCodes.OK;
        }

        #endregion

        #region 道具相关

        //测试包裹索引的合法性
        public void TestBagLogicIndex(CharacterController _this)
        {
            foreach (var bagBase in _this.mBag.mBags)
            {
                var index = 0;
                foreach (var item in bagBase.Value.mLogics)
                {
                    if (item.GetIndex() != index)
                    {
                        Logger.Error("LogicIndex Error bagid[{0}] nowIndex[{1}]!=realIndex[{2}]", bagBase.Key,
                            item.GetIndex(), index);
                        //item.SetIndex(index);
                    }
                    index++;
                }
            }
        }

        public void TestBagDbIndex(CharacterController _this)
        {
            foreach (var bagBase in _this.mDbData.Bag.Bags)
            {
                var index = 0;
                foreach (var item in bagBase.Value.Items)
                {
                    if (item.Index != index)
                    {
                        Logger.Error("DbaseIndex Error bagid[{0}] nowIndex[{1}]!=realIndex[{2}]", bagBase.Key,
                            item.Index, index);
                        //item.Index= index;
                    }
                    index++;
                }
            }
        }

        public ErrorCodes CheckEquipOn(CharacterController _this, EquipRecord tbEquip, int nEquipPart)
        {
            //转生条件检查
            var ladder = GetExData(_this, (int)eExdataDefine.e51);
            if (tbEquip.NeedRebornLevel > ladder)
            {
                return ErrorCodes.RoleIdError;
            }

            //职业检查
            if (tbEquip.Occupation != -1 && tbEquip.Occupation != GetRole(_this))
            {
                return ErrorCodes.RoleIdError;
            }
            //属性需求
            for (var i = 0; i < tbEquip.NeedAttrId.Length; i++)
            {
                if (tbEquip.NeedAttrId[i] > 0)
                {
                    if (GetAttrPoint(_this, (eAttributeType) tbEquip.NeedAttrId[i]) < tbEquip.NeedAttrValue[i])
                    {
                        return ErrorCodes.Error_AttrNotEnough;
                    }
                }
            }
            //获得装备点
            if (!ItemEquip2.IsCanEquip(tbEquip, nEquipPart))
            {
                return ErrorCodes.Error_EquipPart;
            }
            return ErrorCodes.OK;
        }


        private void ReFreshEquipAttr(CharacterController _this,ItemBase _item,int _type,int bagId,int bagIdx,int maxAdd)
        {
            int nPosLv = Utils.GetEquipLevelExPos(bagId, bagIdx);
            int nPosAdd = Utils.GetEquipAddtionalExPos(bagId, bagIdx);
            if (nPosLv > 0 || nPosAdd > 0)
            {
                if (_type == 0)
                {//卸下
                    _item.SetExdata(0,0);
                    _item.SetExdata(1,0);
                }
                else
                {//装上
                    _item.SetExdata(0,_this.GetExData(nPosLv));
                    int additional = _this.GetExData(nPosAdd);
                    _item.SetExdata(1, additional < maxAdd ? additional : maxAdd);
                }
                _item.MarkDbDirty();
            }
        }

        public ErrorCodes UseShiZhuang(CharacterController _this, int BagId, int BagItemIndex, int EquipPart)
        {
            //索引是否有效
            ItemBase item = _this.mBag.mBags[BagId].GetItemByIndex(BagItemIndex);
            if (item == null || item.GetId() < 0)
            {
                return ErrorCodes.Error_BagIndexNoItem;
            }
            //获取数据
            var tbItem = Table.GetItemBase(item.GetId());
            if (tbItem == null)
            {
                return ErrorCodes.Error_ItemID;
            }
            //穿戴条件检查
            if (GetLevel(_this) < tbItem.UseLevel) //等级
            {
                return ErrorCodes.Error_LevelNoEnough;
            }
            var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            if (tbEquip == null)
            {
                return ErrorCodes.Error_EquipID;
            }
            //职业检查
            if (tbEquip.Occupation != -1 && tbEquip.Occupation != GetRole(_this))
            {
                return ErrorCodes.RoleIdError;
            }
            //转生条件检查
            var ladder = GetExData(_this, (int)eExdataDefine.e51);
            if (tbEquip.NeedRebornLevel > ladder)
            {
                return ErrorCodes.Error_RebornLevelNotEnough;
            }

            //检查装备点
            if (!ItemEquip2.IsShiZhuangCanEquip(tbEquip, EquipPart))
            {
                return ErrorCodes.Error_EquipPart;
            }

            //{//限时检查
            //    if (tbItem.TimeLimit != -1)
            //    {
            //        var limitTime = item.GetExdata(32);
            //        if (limitTime == -1)//未限时则设置限时时长
            //        {
            //            item.SetExdata(32, DataTimeExtension.GetTimeStampSeconds(DateTime.Now) + tbItem.TimeLimit);
            //        }
            //        else
            //        {
            //            var nowTime = DataTimeExtension.GetTimeStampSeconds(DateTime.Now);
            //            if (nowTime >= limitTime)
            //            {
            //                //删除时装
            //                //返回对应错误码
            //            }
            //        }
            //    }
            //}

            ItemBase oldItem = GetItemByBagByIndex(_this, EquipPart, 0);
            if (oldItem != null)
            {
                var oldState = oldItem.GetExdata(31);
                item.SetExdata(31, oldState);//传承显隐状态
            }

            EquipChange(_this, oldItem == null ? 1 : 2, EquipPart, 0, item);
            _this.mBag.MoveItem(BagId, BagItemIndex, EquipPart, 0, 1);
            var e = new EquipItemEvent(_this, tbEquip.Part);
            EventDispatcher.Instance.DispatchEvent(e);

            return ErrorCodes.OK;
        }

        public ErrorCodes DeleteShiZhuang(CharacterController _this, int BagId, int BagItemIndex)
        {
            //索引是否有效
            ItemBase item = _this.mBag.mBags[BagId].GetItemByIndex(BagItemIndex);
            if (item == null || item.GetId() < 0)
            {
                return ErrorCodes.Error_BagIndexNoItem;
            }
            //获取数据
            var tbItem = Table.GetItemBase(item.GetId());
            if (tbItem == null)
            {
                return ErrorCodes.Error_ItemID;
            }
            var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            if (tbEquip == null)
            {
                return ErrorCodes.Error_EquipID;
            }

            if (BagId >= (int)eBagType.EquipShiZhuangBag && BagId <= (int)eBagType.WeaponShiZhuangBag)
            {
                _this.mBag.mBags[BagId].ForceDeleteItem(item.GetId(), 1);
            }
            else if (BagId >= (int)eBagType.EquipShiZhuang && BagId <= (int)eBagType.WeaponShiZhuang)
            {
                item.SetExdata(31, 0);
                item.MarkDbDirty();
                EquipChange(_this, 0, BagId, 0, item);
                _this.mBag.mBags[BagId].ForceDeleteItem(item.GetId(), 1);
            }

            return ErrorCodes.OK;
        }

        public ErrorCodes SetEquipModelState(CharacterController _this, List<int> Parts, int State)
        {
            foreach (var part in Parts)
            {
                ItemBase item = GetItemByBagByIndex(_this, part, 0);
                if (item != null)
                {
                    if (part == (int)eBagType.Wing)
                    {
                        item.SetExdata(11, State);
                    }
                    else
                    {
                        item.SetExdata(31, State);
                    }
                    item.MarkDbDirty();
                    _this.EquipChange(2, part, 0, item);
                }
            }
            return ErrorCodes.OK;
        }

        public void RefreshFashionState(CharacterController _this)
        {
            var toDeleteData = new Dictionary<int, int>();
            for (int i = (int)eBagType.EquipShiZhuangBag; i < (int)eBagType.WeaponShiZhuang; i++)
            {
                var equipBag = _this.mBag.mBags[i];
                if (null != equipBag)
                {
                    foreach (var itemBase in equipBag.mLogics)
                    {
                        if (itemBase.GetId() == -1)
                            continue;

                        var limitTime = itemBase.GetExdata(32);
                        if (limitTime == -1)
                            continue;

                        var nowTime = DataTimeExtension.GetTimeStampSeconds(DateTime.Now);
                        if (nowTime >= limitTime)
                        {
                            toDeleteData.Add(i, itemBase.GetIndex());
                        }
                    }
                }
            }

            foreach (var data in toDeleteData)
            {
                var bagId = data.Key;
                var bagItemIndex = data.Value;
                DeleteShiZhuang(_this, bagId, bagItemIndex);
            }
        }

        //使用装备
        /// <summary>
        ///     使用装备
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nBagIndex">装备包裹的索引</param>
        /// <param name="nEquipPart">部位ID</param>
        /// <param name="index">部位索引</param>
        /// <returns></returns>
        public ErrorCodes UseEquip(CharacterController _this, int nBagIndex, int nEquipPart, int index)
        {
            //索引是否有效
            ItemBase item = _this.mBag.mBags[0].GetItemByIndex(nBagIndex);
            if (item == null || item.GetId() < 0)
            {
                return ErrorCodes.Error_BagIndexNoItem;
            }
            PlayerLog.WriteLog(_this.mGuid, "----------UseEquip----------{0},{1}", nBagIndex, item.GetId());
            //道具是否装备
            if (!CheckGeneral.CheckItemType(item.GetId(), eItemType.Equip))
            {
                return ErrorCodes.Error_ItemIsNoEquip;
            }
            //获取数据
            var tbItem = Table.GetItemBase(item.GetId());
            if (tbItem == null)
            {
                return ErrorCodes.Error_ItemID;
            }
            //穿戴条件检查
            if (GetLevel(_this) < tbItem.UseLevel) //等级
            {
                return ErrorCodes.Error_LevelNoEnough;
            }
            var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            if (tbEquip == null)
            {
                return ErrorCodes.Error_EquipID;
            }
            if (item.IsTrialEnd())
            {
                return ErrorCodes.Error_TiralEnd;
            }
            //职业检查
            if (tbEquip.Occupation != -1 && tbEquip.Occupation != GetRole(_this))
            {
                return ErrorCodes.RoleIdError;
            }
            //转生条件检查
            var ladder = GetExData(_this, (int)eExdataDefine.e51);
            if (tbEquip.NeedRebornLevel > ladder)
            {
                return ErrorCodes.Error_RebornLevelNotEnough;
            }
            //属性需求
            for (var i = 0; i < tbEquip.NeedAttrId.Length; i++)
            {
                if (tbEquip.NeedAttrId[i] > 0)
                {
                    if (GetAttrPoint(_this, (eAttributeType)tbEquip.NeedAttrId[i]) < tbEquip.NeedAttrValue[i])
                    {
                        return ErrorCodes.Error_AttrNotEnough;
                    }
                }
            }

            //获得装备点
            if (!ItemEquip2.IsCanEquip(tbEquip, nEquipPart))
            {
                return ErrorCodes.Error_EquipPart;
            }

            if (index < 0 || index > 1)
            {
                return ErrorCodes.Error_EquipIndex;
            }
            if (nEquipPart != 13 && index == 1)
            {
                return ErrorCodes.Error_EquipIndex;
            }





            ItemBase oldItem = GetItemByBagByIndex(_this, nEquipPart, index);
            ItemBase oldItem2 = null;
            if (tbItem.Type == 10099)
            {//双手武器卸下副手
                oldItem2 = GetItemByBagByIndex(_this, (int)eBagType.Equip12, 0);
            }
            else if (tbItem.Type == 10098 || tbItem.Type == 10011)
            {//单手、副手装备卸下双手
                var LeftEquip = GetItemByBagByIndex(_this, (int)eBagType.Equip11, 0);
                if (LeftEquip != null)
                {
                    var tbLeftEquip = Table.GetItemBase(LeftEquip.GetId());
                    if (tbLeftEquip.Type == 10099)
                    {
                        oldItem2 = LeftEquip;
                        if (oldItem == oldItem2)
                        {//副手武器装到主手的时候只需要卸下一次
                            oldItem2 = null;
                        }
                    }
                        
                }
            }
            int freeIdx = -1;
            if (oldItem != null && oldItem2 != null)
            {
                freeIdx = _this.mBag.mBags[0].GetFirstFreeIndex();
                if (freeIdx == -1)
                {
                    return ErrorCodes.Error_ItemNoInBag_All;
                }
            }
            else
            {
                freeIdx = nBagIndex;
            }
            //设置绑定状态
            var equipItem = item as ItemEquip2;
            if (equipItem != null)
            {
                equipItem.SetBinding();
            }


            //卸下替换1
            if (oldItem != null)
            {
                ReFreshEquipAttr(_this, oldItem, 0, nEquipPart, index, tbEquip.AddAttrMaxValue);
            }
        
            ////装上装备
           
            ReFreshEquipAttr(_this, item, 1, nEquipPart, index, tbEquip.AddAttrMaxValue);
            EquipChange(_this, oldItem == null ? 1 : 2, nEquipPart, index, item);
            _this.mBag.MoveItem((int)eBagType.Equip, nBagIndex, nEquipPart, index, 1);
            var e = new EquipItemEvent(_this, tbEquip.Part);
            EventDispatcher.Instance.DispatchEvent(e);
            
            //卸下替换2
            if (oldItem2 != null)
            {
                ReFreshEquipAttr(_this, oldItem2, 0, nEquipPart, index, tbEquip.AddAttrMaxValue);
                EquipChange(_this, 0, oldItem2.GetBagId(), oldItem2.GetIndex(), oldItem2);
                _this.mBag.MoveItem(oldItem2.GetBagId(), oldItem2.GetIndex(), (int)eBagType.Equip, freeIdx, 1);
            }
          
            return ErrorCodes.OK;
        }

        public bool DeleteEquip(CharacterController _this, int ItemId,int deleteType)
        {
            var tbItem = Table.GetItemBase(ItemId);
            if (tbItem != null)
            {
                var tbEquip = Table.GetEquip(tbItem.Exdata[0]);


                for (int i = 0; i <= (int)eBagType.Equip12 ; i++)
                {
                    if (BitFlag.GetLow(tbItem.CanInBag, i) == false)
                        continue;
                    var bag = _this.mBag.GetBag(i);
                    if (bag == null)
                    {
                        continue;
                    }
                    ItemBase item = bag.GetFirstByItemId(ItemId);
                    if (item == null)
                        continue;
                    if (i >= (int) eBagType.Equip01 && i <= (int) eBagType.Equip12)
                    {
                        if (tbEquip != null)
                        {
                            ReFreshEquipAttr(_this, item, 0, i, item.GetIndex(), tbEquip.AddAttrMaxValue);
                            EquipChange(_this, 0, item.GetBagId(), item.GetIndex(), item);                                    
                        }
                    }
                    var nLast = bag.ForceDeleteItem(ItemId, 1);
                    PlayerLog.DataLog(_this.mGuid, "id,{0},{1},{2}", ItemId, 1, (int)deleteType);
                    var e2 = new ItemChange(_this, ItemId, -1);
                    EventDispatcher.Instance.DispatchEvent(e2);
                    return true;
                }
            }
            return false;
        }


       
        //降低耐久度
        public void DurableDown(CharacterController _this, int bagIdandIndex, int diffValue)
        {
            var bagId = bagIdandIndex/10;
            var bagIndex = bagIdandIndex%10;
            var equip = GetItemByBagByIndex(_this, bagId, bagIndex) as ItemEquip2;
            if (equip == null)
            {
                return;
            }
            if (equip.GetId() < 0)
            {
                return;
            }
            var tbEquip = Table.GetEquip(equip.GetId());
            if (tbEquip == null)
            {
                return;
            }
            var oldValue = equip.GetExdata(22);
            if (oldValue <= 0)
            {
                return;
            }
            var newValue = oldValue + diffValue;
            if (_this.Proxy != null)
            {
                if (tbEquip.Durability >= newValue*10 && tbEquip.Durability < oldValue*10)
                {
                    PlayerLog.WriteLog(_this.mGuid,
                        "----------Scene2Logic2Client-----EquipDurableBroken----------{0},{1}", bagIdandIndex, newValue);
                    _this.Proxy.EquipDurableBroken(bagIdandIndex, newValue);
                }
                else if (newValue <= 0)
                {
                    PlayerLog.WriteLog(_this.mGuid,
                        "----------Scene2Logic2Client-----EquipDurableBroken----------{0},{1}", bagIdandIndex, newValue);
                    _this.Proxy.EquipDurableBroken(bagIdandIndex, newValue);
                }
            }
            if (_this.mBag.EquipDurableChange == false)
            {
                PlayerLog.WriteLog(_this.mGuid, "----------Scene2Logic2Client-----DurableDown----------");
                _this.Proxy.EquipDurableChange(0);
                _this.mBag.EquipDurableChange = true;
            }
            equip.SetDurable(newValue);
            //equip.SetExdata(22, now);
            equip.MarkDbDirty();
        }

        //请求耐久度
        public void ApplyEquipDurable(CharacterController _this, Dictionary<int, int> durables)
        {
            //for (int i = 7; i <= 18; ++i)
            foreach (var i in EquipExtension.Equips)
            {
                if (i == 12)
                {
                    continue;
                }
                BagBase bag;
                if (!_this.mBag.mBags.TryGetValue(i, out bag))
                {
                    continue;
                }
                var index = -1;
                foreach (var itemBase in bag.mLogics)
                {
                    index++;
                    if (itemBase.GetId() < 0)
                    {
                        continue;
                    }
                    var durable = itemBase.GetExdata(22);
                    if (durable < 1)
                    {
                        continue;
                    }
                    durables.Add(i*10 + index, itemBase.GetExdata(22));
                }
            }
            _this.mBag.EquipDurableChange = false;
        }

        //修理
        public ErrorCodes RepairEquip(CharacterController _this)
        {
            var needMoney = 0;
            for (var i = 7; i <= 18; ++i)
            {
                BagBase bag;
                if (!_this.mBag.mBags.TryGetValue(i, out bag))
                {
                    continue;
                }
                foreach (var itemBase in bag.mLogics)
                {
                    if (itemBase.GetId() < 0)
                    {
                        continue;
                    }
                    var tbEquip = Table.GetEquip(itemBase.GetId());
                    if (tbEquip.DurableType == 0)
                    {
                        continue;
                    }
                    var durable = itemBase.GetExdata(22);
                    needMoney += (tbEquip.Durability - durable)*tbEquip.DurableMoney;
                }
            }
            var nowMoney = _this.mBag.GetRes(eResourcesType.GoldRes);
            if (nowMoney < needMoney)
            {
                return ErrorCodes.MoneyNotEnough;
            }
            _this.mBag.DelRes(eResourcesType.GoldRes, needMoney, eDeleteItemType.RepairEquip);
            var refreshEquips = new List<ItemBase>();
            foreach (var i in EquipExtension.Equips)
            {
                BagBase bag;
                if (!_this.mBag.mBags.TryGetValue(i, out bag))
                {
                    continue;
                }
                foreach (var itemBase in bag.mLogics)
                {
                    if (itemBase.GetId() < 0)
                    {
                        continue;
                    }
                    var tbEquip = Table.GetEquip(itemBase.GetId());
                    if (tbEquip.DurableType == 0)
                    {
                        continue;
                    }

                    // 有耐久减少的装备才需要修理
                    int dur = itemBase.GetExdata(22);
                    if (dur >= tbEquip.Durability)
                        continue;

                    itemBase.SetExdata(22, tbEquip.Durability);
                    itemBase.MarkDirty();
                }
            }
            foreach (var itemBase in refreshEquips)
            {
                EquipChange(_this, 2, itemBase.GetBagId(), itemBase.GetIndex(), itemBase);
            }
            return ErrorCodes.OK;
        }

        //获得某个包裹的某个索引的道具
        public ItemBase GetItemByBagByIndex(CharacterController _this, int bagid, int bagindex)
        {
            BagBase thisbag;
            if (!_this.mBag.mBags.TryGetValue(bagid, out thisbag))
            {
                Logger.Warn("GetItemByBagByIndex bagid={0} not find!", bagid);
            }
            if (thisbag != null)
            {
                return thisbag.GetItemByIndex(bagindex);
            }
            Logger.Warn("GetItemByBagByIndex bagid={0} is null!", bagid);
            return null;
        }

        //获得某个包裹的某个索引的道具
        public BagBase GetBag(CharacterController _this, int bagid)
        {
            BagBase thisbag;
            if (!_this.mBag.mBags.TryGetValue(bagid, out thisbag))
            {
                Logger.Warn("GetItemByBagByIndex bagid={0} not find!", bagid);
                return null;
            }
            return thisbag;
        }

        //出售物品
        public ErrorCodes SellItem(CharacterController _this, int nBagId, int nIndex, int nItemId, int nCount)
        {
            if (nBagId == 5)
            {
                return ErrorCodes.Error_ItemNotSell;
            }
            var item = GetItemByBagByIndex(_this, nBagId, nIndex);
            if (item == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            if (item.GetId() != nItemId)
            {
                return ErrorCodes.Error_ItemID;
            }
            if (item.GetCount() < nCount || nCount < 1)
            {
                return ErrorCodes.Error_ResNoEnough;
            }
            var tbItem = Table.GetItemBase(nItemId);
            if (tbItem.Sell <= 0)
            {
                return ErrorCodes.Error_ItemNotSell;
            }
            _this.mBag.AddItem((int) eResourcesType.GoldRes, tbItem.Sell*nCount, eCreateItemType.Sell);
            _this.mBag.mBags[nBagId].ReduceCountByIndex(nIndex, nCount, eDeleteItemType.Sell);
            return ErrorCodes.OK;
        }

        //回收物品
        public ErrorCodes Recycletem(CharacterController _this, int nBagId, int nIndex, int nItemId, int nCount)
        {
            if (nBagId != 1)
            {
                return ErrorCodes.Error_ItemNotSell;
            }
            var item = GetItemByBagByIndex(_this, nBagId, nIndex);
            if (item == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            if (item.GetId() != nItemId)
            {
                return ErrorCodes.Error_ItemID;
            }
            if (item.GetCount() < nCount || nCount < 1)
            {
                return ErrorCodes.Error_ResNoEnough;
            }
            var tbItem = Table.GetItemBase(nItemId);
            if (tbItem.CallBackType <= 0)
            {
                return ErrorCodes.Error_ItemNotSell;
            }
            var result = _this.mBag.CheckAddItem(tbItem.CallBackType, tbItem.CallBackPrice*nCount);
            if (result != ErrorCodes.OK)
            {
                return result;
            }
            _this.mBag.AddItem(tbItem.CallBackType, tbItem.CallBackPrice*nCount, eCreateItemType.Recycle);
            _this.mBag.mBags[nBagId].ReduceCountByIndex(nIndex, nCount, eDeleteItemType.Recycle);
            return ErrorCodes.OK;
        }

        public bool CheckBagCanIn(CharacterController _this, int value)
        {
            if (value == -1)
            {
                return true;
            }
            CharacterController.bagCount[1] = value%100;
            value = value/100;
            CharacterController.bagId[1] = value%100;
            value = value/100;
            CharacterController.bagCount[0] = value%100;
            value = value/100;
            CharacterController.bagId[0] = value%100;
            for (var i = 0; i < 2; i++)
            {
                if (CharacterController.bagCount[i] < 1)
                {
                    continue;
                }
                var bag = _this.mBag.GetBag(CharacterController.bagId[i]);
                if (bag == null)
                {
                    continue;
                }
                if (bag.GetFreeCount() < CharacterController.bagCount[i])
                {
                    return false;
                }
            }

            return true;
        }

        //使用物品
        public IEnumerator UseItem(Coroutine coroutine, CharacterController _this, UseItemInMessage msg)
        {
            var bagId = msg.Request.BagType;
            var bagIndex = msg.Request.BagIndex;
            var theItem = GetItemByBagByIndex(_this, bagId, bagIndex);
            if (theItem == null)
            {
                PlayerLog.WriteLog(_this.mGuid, "----------Logic----------UseItem----------{0},{1}", bagId, bagIndex);
                Logger.Warn("UseItem bagId({0})[{1}] is Empty!", bagId, bagIndex);
                msg.Reply((int) ErrorCodes.Error_ItemNotFind);
                yield break;
            }
            var ItemId = theItem.GetId();
            PlayerLog.WriteLog(_this.mGuid, "----------Logic----------UseItem----------{0},{1},{2}", bagId, bagIndex,
                ItemId);
            var Count = msg.Request.Count;
            if (Count <= 0)
            {
                Logger.Warn("UseItem bagId({0})[{1}] is not enough!{2}/{3}", bagId, bagIndex, theItem.GetCount(), Count);
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }
            if (theItem.GetCount() < Count)
            {
                Logger.Warn("UseItem bagId({0})[{1}] is not enough!{2}/{3}", bagId, bagIndex, theItem.GetCount(), Count);
                msg.Reply((int) ErrorCodes.ItemNotEnough);
                yield break;
            }
            var tbItem = Table.GetItemBase(ItemId);
            if (tbItem == null)
            {
                Logger.Warn("UseItem ItemId={0} not find", ItemId);
                msg.Reply((int) ErrorCodes.Error_ItemID);
                yield break;
            }
            //使用等级检查
            if (_this.GetLevel() < tbItem.UseLevel) //等级
            {
                msg.Reply((int) ErrorCodes.Error_LevelNoEnough);
                yield break;
            }
            if (tbItem.OccupationLimit != -1)
            {
//统一检查职业限制
                if (tbItem.OccupationLimit != GetRole(_this))
                {
                    Logger.Warn("UseItem ItemId={0}  Occupation not Match", ItemId);
                    msg.Reply((int) ErrorCodes.RoleIdError);
                    yield break;
                }
            }

            var needDependItemCount = tbItem.DependItemNum;
            var needDependItem = ((tbItem.DependItemId > 0) && (needDependItemCount > 0));
            if (needDependItem)
            {
                var dependItemCount = _this.mBag.GetItemCount(tbItem.DependItemId);
                if (dependItemCount <= 0)
                {
                    PlayerLog.WriteLog(_this.mGuid, "----------Logic----------DependItem:{0} not enough}", tbItem.DependItemId);
                    Logger.Warn("UseItem donot has item DependItemId({0})!", tbItem.DependItemId);
                    msg.Reply((int) ErrorCodes.ItemNotEnough);
                    yield break;
                }
                Count = Math.Min(Count, dependItemCount / needDependItemCount);
                if (Count <= 0)
                {
                    msg.Reply((int)ErrorCodes.ItemNotEnough);
                    yield break;                    
                }
            }

            switch (tbItem.Type)
            {
                case 21000: //技能书
                {
//                         if (tbItem.Exdata[1] != GetRole())
//                         {
//                             msg.Reply((int)ErrorCodes.RoleIdError);
//                             yield break;
//                         }
                    if (tbItem.Exdata[2] > 0)
                    {
                        if (GetAttrPoint(_this, (eAttributeType) tbItem.Exdata[2]) < tbItem.Exdata[3])
                        {
                            msg.Reply((int) ErrorCodes.Error_AttrNotEnough);
                            yield break;
                        }
                    }
                    if (_this.mSkill.GetSkillLevel(tbItem.Exdata[0]) == 0)
                    {
                        _this.mSkill.LearnSkill(tbItem.Exdata[0], 1);
                    }
                    else
                    {
                        var oldPoint = _this.mTalent.GetSkillTalentCount(tbItem.Exdata[0]);
                        if (oldPoint >= Table.GetSkill(tbItem.Exdata[0]).TalentMax)
                        {
                            msg.Reply((int) ErrorCodes.Error_SkillTalentMax);
                            yield break;
                        }
                        _this.mTalent.AddSkillPoint(tbItem.Exdata[0], 1);
                    }
                    AddExData(_this, (int) eExdataDefine.e279, 1);
                }
                    break;
                case 23000: //固定礼包
                {
                    for (var c = 0; c != Count; ++c)
                    {
                        var value = tbItem.Exdata[1];
                        if (!CheckBagCanIn(_this, value))
                        {
                            if (c != 0)
                            {
                                GetBag(_this, bagId).ReduceCountByIndex(bagIndex, c, eDeleteItemType.UseItem);
                                if (needDependItem)
                                {
                                    _this.mBag.DeleteItem(tbItem.DependItemId, c * needDependItemCount, eDeleteItemType.UseItem);
                                }
                            }
                            msg.Reply((int) ErrorCodes.Error_ItemNoInBag_All);
                            yield break;
                        }

                        var result = _this.Gift(eActivationRewardType.TableGift, tbItem.Exdata[0]);
                        if (result != ErrorCodes.OK)
                        {
                            if (c != 0)
                            {
                                GetBag(_this, bagId).ReduceCountByIndex(bagIndex, c, eDeleteItemType.UseItem);
                                if (needDependItem)
                                {
                                    _this.mBag.DeleteItem(tbItem.DependItemId, c * needDependItemCount, eDeleteItemType.UseItem);
                                }
                            }
                            msg.Reply((int) result);
                            yield break;
                        }


                        if (tbItem.ShowAfterUse == 1)
                        {
                            var tbGift = Table.GetGift(tbItem.Exdata[0]);
                            for (var i = 0; i != 4; ++i)
                            {
                                if (tbGift.Param[i * 2] != -1)
                                {
                                    msg.Response.Data.modifyValue(tbGift.Param[i * 2], tbGift.Param[i * 2 + 1]);
                                }
                            }                           
                        }
                    }
                }
                    break;
                case 23500: //随机礼包
                {
                    for (var c = 0; c != Count; ++c)
                    {
                        var value = tbItem.Exdata[1];
                        if (!CheckBagCanIn(_this, value))
                        {
                            if (c != 0)
                            {
                                GetBag(_this, bagId).ReduceCountByIndex(bagIndex, c, eDeleteItemType.UseItem);
                                if (needDependItem)
                                {
                                    _this.mBag.DeleteItem(tbItem.DependItemId, c * needDependItemCount, eDeleteItemType.UseItem);
                                }
                            }
                            msg.Reply((int) ErrorCodes.Error_ItemNoInBag_All);
                            yield break;
                        }
                        var broadMieshi22055 = false;
                        var broadMieshi22054 = false;
                        var broad22055 = false;
                        var broad22054 = false;
                        var broad22007 = false;
                        CharacterController.itemList.Clear();
                        CharacterController.itemList2.Clear();
                        _this.DropMother(tbItem.Exdata[0], CharacterController.itemList);
                        foreach (var i in CharacterController.itemList)
                        {
                           if (515002 == ItemId)//灭世宝箱ID
                            {
                                if (i.Key == 22055)//开出的黄金龙鳞
                                {
                                    broadMieshi22055 = true;
                                }
                                if (i.Key == 22054)//开出的天空石碎片
                                {
                                    broadMieshi22054 = true;
                                }
                            }
                           if (23180 == ItemId)
                           {
                               if (i.Key == 22055)
                               {
                                   broad22055 = true;
                               }
                               if (i.Key == 22054)
                               {
                                   broad22054 = true;
                               }
                           }
                            if(23181 == ItemId)
                            {
                                if(i.Key == 22007)
                                {
                                    broad22007 = true;
                                }
                            }

                            if (tbItem.ShowAfterUse == 1)
                            {
                                msg.Response.Data.modifyValue(i.Key, i.Value);
                            }

                            if (_this.mBag.AddItem(i.Key, i.Value, eCreateItemType.UseItem) != ErrorCodes.OK)
                            {
                                CharacterController.itemList2.modifyValue(i.Key, i.Value);
                            }
                        }
                        if (CharacterController.itemList2.Count > 1)
                        {
                            var tbMail = Table.GetMail(122);
                            _this.mMail.PushMail(tbMail.Title, tbMail.Text, CharacterController.itemList2);
                        }
                        
                        if (broadMieshi22054)
                        {
                             var args = new List<string>();
                             args.Add(_this.Name);
                             var content = Utils.WrapDictionaryId(300000096, args);

                             var chatAgent = LogicServer.Instance.ChatAgent;
                             var serverId = SceneExtension.GetServerLogicId(_this.serverId);
                             chatAgent.BroadcastWorldMessage((uint)serverId, (int)eChatChannel.SystemScroll, 0, string.Empty,
                                    new ChatMessageContent { Content = content });
                        }

                        if (broad22007)
                        {
                            var args = new List<string>();
                            args.Add(_this.Name);
                            var content = Utils.WrapDictionaryId(100003315, args);

                            var chatAgent = LogicServer.Instance.ChatAgent;
                            var serverId = SceneExtension.GetServerLogicId(_this.serverId);
                            chatAgent.BroadcastWorldMessage((uint)serverId, (int)eChatChannel.SystemScroll, 0, string.Empty,
                                   new ChatMessageContent { Content = content });
                        }

                        if (broadMieshi22055)
                        {
                             var args = new List<string>();
                             args.Add(_this.Name);
                             var content = Utils.WrapDictionaryId(300000097, args);
                             var chatAgent = LogicServer.Instance.ChatAgent;
                             var serverId = SceneExtension.GetServerLogicId(_this.serverId);
                             chatAgent.BroadcastWorldMessage((uint)serverId, (int)eChatChannel.SystemScroll, 0, string.Empty,
                                    new ChatMessageContent { Content = content });
                        }

                        if (broad22054)
                        {
                            var args = new List<string>();
                            args.Add(_this.Name);
                            var content = Utils.WrapDictionaryId(100001255, args);//String.Format("玩家{0}开启[FFFF00]精致天空石礼包[-]获得了[D83BFC]天空石碎片[-]，手气好到爆！", _this.Name);
                            var chatAgent = LogicServer.Instance.ChatAgent;
                            var serverId = SceneExtension.GetServerLogicId(_this.serverId);
                            chatAgent.BroadcastWorldMessage((uint)serverId, (int)eChatChannel.SystemScroll, 0, string.Empty,
                                new ChatMessageContent { Content = content });
                        }
                        if (broad22055)
                        {
                            var args = new List<string>();
                            args.Add(_this.Name);
                            var content = Utils.WrapDictionaryId(100001256, args);//String.Format("玩家{0}开启[FFFF00]精致天空石礼包[-]获得了[FFFF00]黄金龙鳞[-]，高阶翅膀马上入手！", _this.Name);
                            var chatAgent = LogicServer.Instance.ChatAgent;
                            var serverId = SceneExtension.GetServerLogicId(_this.serverId);
                            chatAgent.BroadcastWorldMessage((uint)serverId, (int)eChatChannel.SystemScroll, 0, string.Empty,
                                new ChatMessageContent { Content = content });
                        }
                    }
                }
                    break;
                case 23600: //职业随机礼包
                {
                    for (var c = 0; c != Count; ++c)
                    {
                        var value = tbItem.Exdata[3];
                        if (!CheckBagCanIn(_this, value))
                        {
                            if (c != 0)
                            {
                                GetBag(_this, bagId).ReduceCountByIndex(bagIndex, c, eDeleteItemType.UseItem);
                                if (needDependItem)
                                {
                                    _this.mBag.DeleteItem(tbItem.DependItemId, c * needDependItemCount, eDeleteItemType.UseItem);
                                }
                            }
                            msg.Reply((int) ErrorCodes.Error_ItemNoInBag_All);
                            yield break;
                        }
                        var roldId = GetRole(_this);
                        if (roldId < 0 || roldId > 2)
                        {
                            msg.Reply((int) ErrorCodes.RoleIdError);
                            yield break;
                        }
                        var motherId = tbItem.Exdata[roldId];
                        CharacterController.itemList.Clear();
                        CharacterController.itemList2.Clear();
                        _this.DropMother(motherId, CharacterController.itemList);


                       
                        foreach (var i in CharacterController.itemList)
                        {

                            if (tbItem.ShowAfterUse == 1)
                            {
                                msg.Response.Data.modifyValue(i.Key, i.Value);
                            }

                            //if (_this.mBag.AddItem(i.Key, i.Value, eCreateItemType.UseItem) != ErrorCodes.OK)
                            //{
                            //    CharacterController.itemList2.modifyValue(i.Key, i.Value);
                            //}
                            if (_this.mBag.CheckAddItem(i.Key, i.Value)!= ErrorCodes.OK)
                            {
                                CharacterController.itemList2.modifyValue(i.Key, i.Value);
                            }
                            else
                            {
                                var tempItemBase = _this.mBag.AddItemGetItem(i.Key, i.Value, eCreateItemType.UseItem);
                                var tempItem = Table.GetItemBase(i.Key);
                                var strs = new List<string>();
                                var characterName = Utils.AddCharacter(_this.mGuid, _this.Name);
                                var itemName = Utils.AddItemId(tbItem.Id);
                                var tempName = Utils.AddItemId(tempItem.Id);

                                var data = new ItemBaseData();
                                data.ItemId = tempItem.Id;
                                data.Count = 1;
                                if (tempItemBase != null) data.Exdata.AddRange(tempItemBase.mDbData.Exdata);
                                int dicId = 0;

                                if (tempItem.Quality == 5)//红色品质
                                {
                                    strs.Add(characterName);
                                    strs.Add(itemName);
                                    strs.Add(tempName);

                                    var content = Utils.WrapDictionaryId(291018, strs, data.Exdata);//String.Format("恭喜玩家[ADFF00]{0}[-]受到玛雅女神眷顾，开启[ff3a3a]{1}[-]获得了[ff3a3a]{2}[-]。", _this.Name);
                                    var chatAgent = LogicServer.Instance.ChatAgent;
                                    var serverId = SceneExtension.GetServerLogicId(_this.serverId);
                                    chatAgent.BroadcastWorldMessage((uint)serverId, (int)eChatChannel.SystemScroll, 0, string.Empty,
                                        new ChatMessageContent { Content = content });

                                }
                                else 
                                {
                                    if (i.Key == 20306)
                                    {
                                        dicId = 100001487;
                                    }
                                    else if (i.Key == 20004)
                                    {
                                        dicId = 100001488;
                                    }
                                    else if (i.Key == 20104)
                                    {
                                        dicId = 100001489;
                                    }
                                    else if (i.Key == 20204)
                                    {
                                        dicId = 100001490;
                                    }
                                    else if (i.Key == 20003)
                                    {
                                        dicId = 100001491;
                                    }
                                    else if (i.Key == 20103) { dicId = 100001492; }
                                    else if (i.Key == 20203) { dicId = 100001493; }
                                    else if (i.Key == 20005) { dicId = 100001494; }
                                    else if (i.Key == 20105) { dicId = 100001495; }
                                    else if (i.Key == 20205) { dicId = 100001496; }
                                    if (dicId != 0)
                                    {
                                        strs.Add(characterName);

                                        var content = Utils.WrapDictionaryId(dicId, strs, data.Exdata);
                                        var chatAgent = LogicServer.Instance.ChatAgent;
                                        var serverId = SceneExtension.GetServerLogicId(_this.serverId);
                                        chatAgent.BroadcastWorldMessage((uint)serverId, (int)eChatChannel.SystemScroll, 0, string.Empty,
                                            new ChatMessageContent { Content = content });
                                    }
                                }
                            }
                        }
                        if (CharacterController.itemList2.Count > 1)
                        {
                            var tbMail = Table.GetMail(122);
                            _this.mMail.PushMail(tbMail.Title, tbMail.Text, CharacterController.itemList2);
                        }
                    }
                }
                    break;
                case 23700: //职业装备礼包
                {
                    var roleId = GetRole(_this);
                    var newEquipId = -1;
                    switch (roleId)
                    {
                        case 0:
                        {
                            newEquipId = tbItem.Exdata[0];
                        }
                            break;
                        case 1:
                        {
                            newEquipId = tbItem.Exdata[1];
                        }
                            break;
                        case 2:
                        {
                            newEquipId = tbItem.Exdata[2];
                        }
                            break;
                        default:
                            msg.Reply((int) ErrorCodes.ParamError);
                            yield break;
                    }

                    var tbEquip = Table.GetEquip(newEquipId);
                    if (tbEquip == null)
                    {
                        msg.Reply((int) ErrorCodes.ParamError);
                        yield break;
                    }
                    var bagEquip = _this.GetBag((int) eBagType.Equip);
                    var firstFreeIndex = bagEquip.GetFirstFreeIndex();
                    if (firstFreeIndex == -1)
                    {
                        msg.Reply((int) ErrorCodes.Error_ItemNoInBag_All);
                        yield break;
                    }
                    var bagItem = bagEquip.mLogics[firstFreeIndex];
                    var newEquip = new ItemEquip2(newEquipId, bagItem.mDbData, tbItem.Exdata[3]);
                    newEquip.SetIndex(firstFreeIndex);
                    newEquip.SetBagId(bagEquip.GetBagId());
                    bagEquip.mLogics[firstFreeIndex] = newEquip;
                    bagEquip.AddChild(newEquip);
                    newEquip.MarkDirty();

                    if (tbItem.ShowAfterUse == 1)
                    {
                        msg.Response.Data.modifyValue(newEquipId, 1);
                    }
                    //var t =_this.mBag.AddItem(newEquipId, 1, eCreateItemType.UseItem);
                }
                    break;
                case 23800: //随机礼包(可出售礼包)
                {
                    for (var c = 0; c != Count; ++c)
                    {
                        var value = tbItem.Exdata[1];
                        if (!CheckBagCanIn(_this, value))
                        {
                            if (c != 0)
                            {
                                GetBag(_this, bagId).ReduceCountByIndex(bagIndex, c, eDeleteItemType.UseItem);
                                if (needDependItem)
                                {
                                    _this.mBag.DeleteItem(tbItem.DependItemId, c * needDependItemCount, eDeleteItemType.UseItem);
                                }
                            }
                            msg.Reply((int) ErrorCodes.Error_ItemNoInBag_All);
                            yield break;
                        }

                        CharacterController.itemList.Clear();
                        CharacterController.itemList2.Clear();
                        _this.DropMother(tbItem.Exdata[0], CharacterController.itemList);
                        foreach (var i in CharacterController.itemList)
                        {
                            if (tbItem.ShowAfterUse == 1)
                            {
                                msg.Response.Data.modifyValue(i.Key, i.Value);
                            }

                            if (_this.mBag.AddItem(i.Key, i.Value, eCreateItemType.UseItem) != ErrorCodes.OK)
                            {
                                CharacterController.itemList2.modifyValue(i.Key, i.Value);
                            }
                        }
                        if (CharacterController.itemList2.Count > 1)
                        {
                            var tbMail = Table.GetMail(122);
                            _this.mMail.PushMail(tbMail.Title, tbMail.Text, CharacterController.itemList2);
                        }
                      }
                }
                    break;
                case 23900: //经验药（直接升级) 扩展数据1 是条件表iD，true走扩展数据2的掉落母表ID，false走扩展数据3的礼包ID
                {
                    var condition = tbItem.Exdata[0];
                    CharacterController.itemList.Clear();
                    CharacterController.itemList2.Clear();
                    if (_this.CheckCondition(condition) == -2)
                    {
                        _this.DropMother(tbItem.Exdata[1], CharacterController.itemList);
                    }
                    else
                    {
                        _this.DropMother(tbItem.Exdata[2], CharacterController.itemList);
                    }

                    foreach (var i in CharacterController.itemList)
                    {
                        if (tbItem.ShowAfterUse == 1)
                        {
                            msg.Response.Data.modifyValue(i.Key, i.Value);
                        }

                        if (_this.mBag.AddItem(i.Key, i.Value, eCreateItemType.UseItem) != ErrorCodes.OK)
                        {
                            CharacterController.itemList2.modifyValue(i.Key, i.Value);
                        }

                        if (CharacterController.itemList2.Count > 1)
                        {
                            var tbMail = Table.GetMail(122);
                            _this.mMail.PushMail(tbMail.Title, tbMail.Text, CharacterController.itemList2);
                        }
                    }
                }
                    break;
                case 24000: //红蓝药类（技能实现CD）
                {
                    var msg2 = LogicServer.Instance.SceneAgent.UseSkillItem(_this.mGuid, ItemId, Count, bagId, bagIndex);
                    yield return msg2.SendAndWaitUntilDone(coroutine);
                    if (msg2.State != MessageState.Reply)
                    {
                        msg.Reply();
                        yield break;
                    }
                    if (msg2.ErrorCode != (int) ErrorCodes.OK)
                    {
                        msg.Reply(msg2.ErrorCode);
                        yield break;
                    }
                    //GetBag(bagId).ReduceCountByIndex(bagIndex, 1, eDeleteItemType.UseItem);
                    msg.Reply();
                    yield break;
                }
                case 24500: //加BUFF药
                {
                    var msg2 = LogicServer.Instance.SceneAgent.UseSkillItem(_this.mGuid, ItemId, Count, bagId, bagIndex);
                    yield return msg2.SendAndWaitUntilDone(coroutine);
                    if (msg2.State != MessageState.Reply)
                    {
                        msg.Reply();
                        yield break;
                    }
                    if ((ErrorCodes) msg2.ErrorCode != ErrorCodes.OK)
                    {
                        msg.Reply(msg2.ErrorCode);
                        yield break;
                    }
                    //GetBag(bagId).ReduceCountByIndex(bagIndex, 1, eDeleteItemType.UseItem);
                    msg.Reply();
                    yield break;
                }
                case 24900: //修改杀气药
                {
                    var msg2 = LogicServer.Instance.SceneAgent.UseSkillItem(_this.mGuid, ItemId, Count, bagId, bagIndex);
                    yield return msg2.SendAndWaitUntilDone(coroutine);
                    if (msg2.State != MessageState.Reply)
                    {
                        msg.Reply();
                        yield break;
                    }
                    if ((ErrorCodes) msg2.ErrorCode != ErrorCodes.OK)
                    {
                        msg.Reply(msg2.ErrorCode);
                        yield break;
                    }
                    //GetBag(bagId).ReduceCountByIndex(bagIndex, 1, eDeleteItemType.UseItem);
                    msg.Reply();
                    yield break;
                }
                case 24950: //设置杀气药
                {
                    var msg2 = LogicServer.Instance.SceneAgent.UseSkillItem(_this.mGuid, ItemId, Count, bagId, bagIndex);
                    yield return msg2.SendAndWaitUntilDone(coroutine);
                    if (msg2.State != MessageState.Reply)
                    {
                        msg.Reply();
                        yield break;
                    }
                    if ((ErrorCodes) msg2.ErrorCode != ErrorCodes.OK)
                    {
                        msg.Reply(msg2.ErrorCode);
                        yield break;
                    }
                    //GetBag(bagId).ReduceCountByIndex(bagIndex, 1, eDeleteItemType.UseItem);
                    msg.Reply();
                    yield break;
                }
                case 99:  //测试用的直接增加资源的危险接口回头不用了就给它干掉
                {
                    var resCount = tbItem.Exdata[0];
                    OnRechargeSuccess(_this,"gift",1, resCount);
                    break;
                }
                case 25000: //永久加成道具
                {
                    var tbLv = Table.GetLevelData(GetLevel(_this));
                    if (tbLv == null)
                    {
                        msg.Reply((int) ErrorCodes.Unknow);
                        yield break;
                    }
                    var exId = tbItem.Exdata[0];
                    var exAdd = tbItem.Exdata[1]*Count;
                    var oldValue = GetExData(_this, exId);
                    var maxValue = tbLv.FruitLimit[tbItem.Exdata[2]];
                    if (oldValue + exAdd > maxValue)
                    {
                        msg.Reply((int) ErrorCodes.Error_Item_Use_Fruit_Limit);
                        yield break;
                    }
                    SetExData(_this, exId, oldValue + exAdd);
                    //for (int i = 0; i < Count; i++)
                    //{
                    //    AddExData(tbItem.Exdata[0], tbItem.Exdata[1]);
                    //}
                    if (exId >= 9 && exId <= 12)
                    {
                        BooksChange(_this);
                    }
                }
                    break;
                case 26300: //藏宝图
                {
                    var sceneId = theItem.GetExdata(0);
                    var x = theItem.GetExdata(1);
                    var y = theItem.GetExdata(2);
                    var dropMotherId = tbItem.Exdata[1];

                    //检查包裹是否放得下
                    var checkBag = tbItem.Exdata[2];
                    if (!CheckBagCanIn(_this, checkBag))
                    {
                        msg.Reply((int) ErrorCodes.Error_ItemNoInBag_All);
                        yield break;
                    }

                    var sceneDataMsg = LogicServer.Instance.SceneAgent.SSGetCharacterSceneData(_this.mGuid, _this.mGuid);
                    yield return sceneDataMsg.SendAndWaitUntilDone(coroutine);
                    if (sceneDataMsg.State != MessageState.Reply)
                    {
                        msg.Reply((int) ErrorCodes.Unknow);
                        yield break;
                    }
                    if (sceneDataMsg.ErrorCode != (int) ErrorCodes.OK)
                    {
                        msg.Reply(sceneDataMsg.ErrorCode);
                        yield break;
                    }
                    var sceneData = sceneDataMsg.Response;
                    if (sceneData.SceneId != sceneId)
                    {
                        msg.Reply((int) ErrorCodes.Error_SceneIdNotMatch);
                        yield break;
                    }
                    var pos = sceneData.Pos.Pos;
                    if (Math.Abs(pos.x/100 - x) > 2 || Math.Abs(pos.y/100 - y) > 2)
                    {
                        msg.Reply((int) ErrorCodes.Error_PositionNotMatch);
                        yield break;
                    }

                    CharacterController.itemList.Clear();
                    CharacterController.itemList2.Clear();
                    _this.DropMother(dropMotherId, CharacterController.itemList);

                    var chatAgent = LogicServer.Instance.ChatAgent;
                    foreach (var i in CharacterController.itemList)
                    {
                        var itemId = i.Key;

                        var data = new ItemBaseData();
                        var count = i.Value;
                        if (count < 0)
                        {
                            count = 1;
                        }
                        ShareItemFactory.Create(itemId, data, count);
                        if (_this.mBag.AddItem(data, eCreateItemType.TreasureMap) != ErrorCodes.OK)
                        {
                            CharacterController.itemList2.modifyValue(itemId, i.Value);
                        }

                        var tbItem1 = Table.GetItemBase(itemId);
                        if (tbItem1.IsDigNotic == 1)
                        {
//发全服通告
                            var strs = new List<string>();
                            strs.Add(_this.GetName());
                            strs.Add(Utils.AddItemId(itemId));
                            strs.Add(i.Value.ToString());
                            var content = new ChatMessageContent();
                            content.Content = Utils.WrapDictionaryId(300909, strs, data.Exdata);
                            var serverId = SceneExtension.GetServerLogicId(_this.serverId);
                            chatAgent.BroadcastWorldMessage((uint) serverId, (int) eChatChannel.SystemScroll, 0,
                                string.Empty, content);
                        }
                    }
                    if (CharacterController.itemList2.Count > 1)
                    {
                        var tbMail = Table.GetMail(129);
                        _this.mMail.PushMail(tbMail.Title, tbMail.Text, CharacterController.itemList2);
                    }
                }
                    break;
                case 27000: //装备强化卷轴
                {
                    var exdata = tbItem.Exdata;
                    var enhance = exdata[0];
                    var bagId1 = exdata[1];
                    var equip = _this.GetItemByBagByIndex(bagId1, 0) as ItemEquip2;
                    if (equip == null)
                    {
                        msg.Reply((int) ErrorCodes.Error_ItemIsNoEquip);
                        yield break;
                    }
                    if (equip.GetExdata(0) >= enhance)
                    {
//强化过高
                        msg.Reply((int) ErrorCodes.Error_EnhanceTooHigh);
                        yield break;
                    }
                    equip.SetExdata(0, enhance);
                    equip.MarkDirty();
                    _this.EquipChange(2, bagId1, 0, equip);
                }
                    break;
                case 30000: //碎片合成
                {
                    var pieceCount = _this.mBag.GetItemCount(tbItem.Id);
                    if (pieceCount < tbItem.Exdata[1])
                    {
                        msg.Reply((int) ErrorCodes.ItemNotEnough);
                        yield break;
                    }
                    var temp = _this.mBag.AddItem(tbItem.Exdata[0], 1, eCreateItemType.Piece);
                    if (temp != ErrorCodes.OK)
                    {
                        msg.Reply((int) temp);
                        yield break;
                    }
                    _this.mBag.DeleteItem(tbItem.Id, tbItem.Exdata[1], eDeleteItemType.Piece);
                    msg.Reply();

                    //潜规则引导标记位
                    if (_this.GetFlag(42) && !_this.GetFlag(549))
                    {
                        _this.SetFlag(549);
                    }
                    yield break;
                }
                case 15000: //坐骑
                {
                    if (true == _this.mMount.AddGift(tbItem.Exdata[2]))
                    {//发坐骑
                        _this.Proxy.SendMountData(_this.mMount.GetMountData());
                    }
                    else
                    {//发礼包
                        for (var c = 0; c != Count; ++c)
                        {
                            var value = tbItem.Exdata[1];
                            if (!CheckBagCanIn(_this, value))
                            {
                                if (c != 0)
                                {
                                    GetBag(_this, bagId).ReduceCountByIndex(bagIndex, c, eDeleteItemType.UseItem);
                                    if (needDependItem)
                                    {
                                        _this.mBag.DeleteItem(tbItem.DependItemId, c * needDependItemCount, eDeleteItemType.UseItem);
                                    }
                                }
                                msg.Reply((int)ErrorCodes.Error_ItemNoInBag_All);
                                yield break;
                            }

                            var result = _this.Gift(eActivationRewardType.TableGift, tbItem.Exdata[0]);
                            if (result != ErrorCodes.OK)
                            {
                                if (c != 0)
                                {
                                    GetBag(_this, bagId).ReduceCountByIndex(bagIndex, c, eDeleteItemType.UseItem);
                                    if (needDependItem)
                                    {
                                        _this.mBag.DeleteItem(tbItem.DependItemId, c * needDependItemCount, eDeleteItemType.UseItem);
                                    }
                                }
                                msg.Reply((int)result);
                                yield break;
                            }


                            if (tbItem.ShowAfterUse == 1)
                            {
                                var tbGift = Table.GetGift(tbItem.Exdata[0]);
                                for (var i = 0; i != 4; ++i)
                                {
                                    if (tbGift.Param[i * 2] != -1)
                                    {
                                        msg.Response.Data.modifyValue(tbGift.Param[i * 2], tbGift.Param[i * 2 + 1]);
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
                case 19000: //挂机时间道具
                {
                    var vipLevel = _this.mBag.GetRes(eResourcesType.VipLevel);
                    var tbVip = Table.GetVIP(vipLevel);
                    if (tbVip == null)
                    {
                        msg.Reply((int)ErrorCodes.Unknow);
                        yield break;  
                    }

                    var exId = tbItem.Exdata[0];
                    var exAdd = tbItem.Exdata[1] * Count;
                    var oldValue = GetExData(_this, exId);
                    var maxValue = tbVip.OfflineTimeMax;

                    if (oldValue >= maxValue)
                    {
                        msg.Reply((int)ErrorCodes.Error_Offline_Item_Use_Failed);
                        yield break;
                    }
                    var resultEx = oldValue + exAdd;
                    if (resultEx > maxValue)
                    {
                        resultEx = maxValue;
                    }

                    SetExData(_this, exId, resultEx);
                    break;
                }
                case 800: // GM 命令物品
                {
                    var command = Table.GetDictionary(tbItem.Exdata[0]).Desc[0];
                    for (int i = 1; i < 4; ++i)
                    {
                        if (tbItem.Exdata[i] != -1)
                        {
                            command += ",";
                            command += tbItem.Exdata[i];
                        }
                    }

                    var error = _this.GmCommand(command);
                    if (error != ErrorCodes.OK)
                    {
                        msg.Reply((int) error);
                        yield break;
                    }

                    break;
                }
                default:
                    msg.Reply((int) ErrorCodes.Error_ItemNotUse);
                    yield break;
            }
            //消耗道具
            GetBag(_this, bagId).ReduceCountByIndex(bagIndex, Count, eDeleteItemType.UseItem);
            if (needDependItem)
            {
                _this.mBag.DeleteItem(tbItem.DependItemId, Count * needDependItemCount, eDeleteItemType.UseItem);
            }
            msg.Reply();
        }

        public ErrorCodes AutoUseItem(CharacterController _this, int itemId)
        {
            var tbItem = Table.GetItemBase(itemId);
            if (tbItem == null)
                return ErrorCodes.Error_ItemNotFind;
            switch (tbItem.Type)
            {

                case 21000: //技能书
                {
                    if (tbItem.Exdata[2] > 0)
                    {
                        if (GetAttrPoint(_this, (eAttributeType) tbItem.Exdata[2]) < tbItem.Exdata[3])
                        {
                            return ErrorCodes.Error_AttrNotEnough;
                        }
                    }
                    if (_this.mSkill.GetSkillLevel(tbItem.Exdata[0]) == 0)
                    {
                        _this.mSkill.LearnSkill(tbItem.Exdata[0], 1);
                    }
                    else
                    {
                        var oldPoint = _this.mTalent.GetSkillTalentCount(tbItem.Exdata[0]);
                        if (oldPoint >= Table.GetSkill(tbItem.Exdata[0]).TalentMax)
                        {
                             return ErrorCodes.Error_SkillTalentMax;
                        }
                        _this.mTalent.AddSkillPoint(tbItem.Exdata[0], 1);
                        _this.Proxy.AddSkillPoint(tbItem.Exdata[0], 1);
                    }
                    AddExData(_this, (int) eExdataDefine.e279, 1);
                }
                break;
                case 15000: //坐骑
                    {
                        if (true == _this.mMount.AddGift(tbItem.Exdata[2]))
                        {//发坐骑
                            _this.Proxy.SendMountData(_this.mMount.GetMountData());
                        }
                        else
                        {//发礼包
                                var value = tbItem.Exdata[1];
                                if (!CheckBagCanIn(_this, value))
                                {
                                    return ErrorCodes.Error_ItemNoInBag_All;
                                }
                                var result = _this.Gift(eActivationRewardType.TableGift, tbItem.Exdata[0]);
                                if (result != ErrorCodes.OK)
                                {
                                   return result;
                                }
                         }
                        break;
                    }
                default:
                    return ErrorCodes.Error_ItemNotUse;
                    
            }
            return ErrorCodes.OK;
        }

        //存放物品
        public ErrorCodes DepotPutIn(CharacterController _this, int nBagId, int nIndex)
        {
            var item = GetItemByBagByIndex(_this, nBagId, nIndex);
            if (item == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            if (item.GetId() == -1)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            var storehouse = _this.mBag.mBags[3];
            var ErrorResult = storehouse.CheckItem(item.GetId());
            if (ErrorResult != ErrorCodes.OK)
            {
                return ErrorResult;
            }
            var tb_item = Table.GetItemBase(item.GetId());
            var totleCount = item.GetCount();
            var nLast = totleCount;
            if (tb_item.MaxCount > 1)
            {
                nLast = storehouse.ForceAddMultiItem(item.GetId(), totleCount);
                if (nLast > 0 && totleCount != nLast)
                {
                    item.SetCount(nLast);
                    item.MarkDirty();
                }
                else if (nLast == 0)
                {
                    _this.mBag.mBags[nBagId].ReduceCountByIndex(nIndex, totleCount, eDeleteItemType.None);
                    return ErrorCodes.OK;
                }
            }
            var freeIndex = storehouse.GetFirstFreeIndex();
            if (freeIndex == -1)
            {
                return ErrorCodes.Error_ItemNoInBag_All;
            }
            return _this.mBag.MoveItem(nBagId, nIndex, 3, freeIndex, nLast);
        }

        //取出物品
        public ErrorCodes DepotTakeOut(CharacterController _this, int nIndex)
        {
            return DepotTakeOutInner(_this, (int) eBagType.Depot, nIndex);
        }

        //从许愿池仓库取出物品
        public ErrorCodes WishingPoolDepotTakeOut(CharacterController _this, int nIndex)
        {
            if (nIndex == -1)
            {
                var err = ErrorCodes.OK;
                var items = _this.mBag.mBags[(int) eBagType.WishingPool].mLogics.ToArray();
                foreach (var item in items)
                {
                    if (item.GetId() <= 0)
                    {
                        continue;
                    }

                    var _err = DepotTakeOutInner(_this, item);
                    if (_err != ErrorCodes.OK)
                    {
                        err = _err;
                    }
                }
                //潜规则引导标记位
                if (GetFlag(_this, 502))
                {
                    SetFlag(_this, 503);
                    SetFlag(_this, 502, false);
                }
                return err;
            }
            return DepotTakeOutInner(_this, (int) eBagType.WishingPool, nIndex);
        }

        //从某仓库取出物品
        private ErrorCodes DepotTakeOutInner(CharacterController _this, int bagId, int bagIndex)
        {
            var item = GetItemByBagByIndex(_this, bagId, bagIndex);
            if (item == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            if (item.GetId() == -1)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            return DepotTakeOutInner(_this, item);
        }

        //从某仓库取出物品
        private ErrorCodes DepotTakeOutInner(CharacterController _this, ItemBase item)
        {
            var tb_item = Table.GetItemBase(item.GetId());
            var toBag = _this.mBag.mBags[tb_item.InitInBag];
            var totleCount = item.GetCount();
            var nLast = totleCount;
            if (tb_item.MaxCount > 1)
            {
                nLast = toBag.ForceAddMultiItem(item.GetId(), totleCount);
                if (nLast > 0 && totleCount != nLast)
                {
                    item.SetCount(nLast);
                    item.MarkDirty();
                }
                else if (nLast == 0)
                {
                    _this.mBag.mBags[item.GetBagId()].ReduceCountByIndex(item.GetIndex(), totleCount,
                        eDeleteItemType.None);
                    //触发事件
                    var e = new CharacterDepotTakeOutEvent(_this, tb_item.Id, totleCount);
                    EventDispatcher.Instance.DispatchEvent(e);
                    return ErrorCodes.OK;
                }
            }
            var freeIndex = toBag.GetFirstFreeIndex();
            if (freeIndex == -1)
            {
                //触发事件
                var e = new CharacterDepotTakeOutEvent(_this, tb_item.Id, totleCount - nLast);
                EventDispatcher.Instance.DispatchEvent(e);
                return ErrorCodes.Error_ItemNoInBag_All;
            }
            //触发事件
            var e2 = new CharacterDepotTakeOutEvent(_this, tb_item.Id, totleCount);
            EventDispatcher.Instance.DispatchEvent(e2);
            return _this.mBag.MoveItem(item.GetBagId(), item.GetIndex(), toBag.GetBagId(), freeIndex, nLast);
        }

        //获得某个Id的宠物
        public PetItem GetPet(CharacterController _this, int petId)
        {
            return _this.mBag.mBags[(int) eBagType.Pet].GetFirstByItemId(petId) as PetItem;
        }

        //获得某个Id的宠物
        public PetItem GetSamePet(CharacterController _this, int petId)
        {
            return _this.mBag.mBags[(int) eBagType.Pet].GetSamePetByPetId(petId);
        }

        //宠物操作
        public ErrorCodes OperatePet(CharacterController _this, int petId, PetOperationType type, int param)
        {
            var pet = GetPet(_this, petId);
            if (pet == null)
            {
                return ErrorCodes.Error_PetNotFind;
            }

            var state = (PetStateType) pet.GetState();

            if (PetOperationType.EMPLOY == type)
            {
                if (PetStateType.NoEmploy != state)
                {
                    return ErrorCodes.Error_PetState;
                }
                pet.SetState(PetStateType.Idle);
                pet.MarkDirty();
            }
            else if (PetOperationType.FIRE == type)
            {
                if (PetStateType.Idle != state)
                {
                    return ErrorCodes.Error_PetState;
                }
                pet.SetState(PetStateType.NoEmploy);
                pet.MarkDirty();
            }
            else if (PetOperationType.RECYCLESOUL == type)
            {
//回收魂魄
                if (PetStateType.Piece == state)
                {
                    return ErrorCodes.Error_PetState;
                }
                //查表
                var tablePet = Table.GetPet(pet.GetId());
                if (null == tablePet)
                {
                    return ErrorCodes.Error_PetNotFind;
                }

                //兑换需要的物品
                var tableItem = Table.GetItemBase(tablePet.NeedItemId);
                if (null == tableItem)
                {
                    return ErrorCodes.Error_PetNotFind;
                }

                //必须满阶了
                if (tablePet.Ladder < 5)
                {
                    return ErrorCodes.Error_PetState;
                }

                //兑换的个数判断
                var fragmentNum = pet.GetExdata(4);
                if (param <= 0 || fragmentNum <= 0 || fragmentNum < param)
                {
                    return ErrorCodes.Error_PetState;
                }

                //设置剩余碎片数
                var ret = fragmentNum - param;
                pet.SetExdata(4, ret);

                //设置增加魂魄数
                var addCount = Math.Max(0, param*tableItem.CallBackPrice);
                var retCode = _this.mBag.AddRes(eResourcesType.PetSoul, addCount, eCreateItemType.PetSoul);

                //如果失败了
                if (ErrorCodes.OK != retCode)
                {
                    pet.SetExdata(4, fragmentNum); //获得物品失败了 给还原回去
                    return retCode;
                }

                pet.MarkDirty();
            }
            else
            {
                return ErrorCodes.Unknow;
            }

            return ErrorCodes.OK;
        }

        //获得翅膀
        public WingItem GetWing(CharacterController _this)
        {
            var item = GetItemByBagByIndex(_this, (int) eBagType.Wing, 0);
            return item as WingItem;
        }

        //翻牌的接口
        public int PushDraw(CharacterController _this, int drawId, out ItemBase tempItemBase, bool isAddItem = true, int dungeonId = -1)
        {
            var tbDraw = Table.GetDraw(drawId);
            if (tbDraw == null)
            {
                tempItemBase = null;
                return -1;
            }
            var pro = MyRandom.Random(10000);
            for (var i = 0; i != 4; ++i)
            {
                pro -= tbDraw.Probability[i];
                if (pro < 0)
                {
                    if (isAddItem)
                    {
                        if (_this.mBag.CheckAddItem(tbDraw.DropItem[i], tbDraw.Count[i]) == ErrorCodes.OK)
                        {
                            tempItemBase = _this.mBag.AddItemGetItem(tbDraw.DropItem[i], tbDraw.Count[i],
                                eCreateItemType.Draw);
                        }
                        else
                        {
                            var temp = new List<ItemBaseData>();
                            var tbMail = Table.GetMail(51);

                            if (tbMail == null)
                                continue;

                            var name = tbMail.Title;
                            var content = tbMail.Text;

                            if (dungeonId != -1)
                            {
                                var r = Table.GetFuben(dungeonId);
                                if (r != null)
                                {
                                    name = string.Format(name, r.Name);
                                    content = string.Format(content, r.Name);                                    
                                }
                            }

                            _this.mMail.PushMail(name, content,
                                new Dictionary<int, int> {{tbDraw.DropItem[i], tbDraw.Count[i]}}, tbMail.Sender, temp);
                            if (temp.Count > 0)
                            {
                                tempItemBase = ShareItemFactory.CreateByDb(temp[0]);
                                if (temp.Count > 1)
                                {
                                    Logger.Warn("PushDraw to MoreItem! drawId ={0}", drawId);
                                }
                            }
                            else
                            {
                                tempItemBase = null;
                            }
                        }
                    }
                    else
                    {
                        tempItemBase = new ItemBase();
                        tempItemBase.SetId(tbDraw.DropItem[i]);
                        tempItemBase.SetCount(tbDraw.Count[i]);
                    }
                    return i;
                }
            }
            Logger.Warn("PushDraw not random one!Id={0} ", drawId);
            tempItemBase = null;
            return -1;
        }

        //购买包裹
        public ErrorCodes BuySpaceBag(CharacterController _this, int bagId, int bagIndex, int needCount)
        {
            var tbBag = Table.GetBagBase(bagId);
            if (tbBag == null)
            {
                return ErrorCodes.Error_BagID;
            }
            var bag = GetBag(_this, bagId);
            if (bag == null)
            {
                return ErrorCodes.Error_BagID;
            }
            var bagCount = bagIndex - bag.GetNowCount(); //计算需要买几格
            if (bagCount < 0 || bagIndex >= tbBag.MaxCapacity)
            {
                return ErrorCodes.ParamError;
            }
            var needTimes = 0;
            for (var i = bag.GetNowCount(); i <= bagIndex; ++i)
            {
                needTimes += bag.GetNeedTime(i, tbBag);
            }
            var bili = tbBag.TimeMult*60;
            var needRes = needTimes/bili + (needTimes%bili > 0 ? 1 : 0);
            if (needRes > needCount)
            {
                Logger.Error("BuySpaceBag bagId={0},bagIndex={1},needCount={2},realNeed={3}", bagId, bagIndex, needCount,
                    needRes);
                return ErrorCodes.DiamondNotEnough;
            }
            Logger.Info("BuySpaceBag bagId={0},bagIndex={1},needCount={2},realNeed={3}", bagId, bagIndex, needCount,
                needRes);

            if (needRes > 0)
            {
                var c = _this.mBag.GetRes(eResourcesType.DiamondRes);
                if (c < needRes)
                {
                    return ErrorCodes.DiamondNotEnough;
                }
                _this.mBag.DelRes(eResourcesType.DiamondRes, needRes, eDeleteItemType.BagCellBuy);
            }
            AddExData(_this, (int) eExdataDefine.e304, bagCount + 1);
            AddExData(_this, (int) eExdataDefine.e305, bagCount + 1);
            bag.SetNowCount(bagIndex + 1);
            bag.SetNextTime(bag.GetNeedTime());
            
            if (bag.GetBagId() == (int)eBagType.Equip)
            {
                AddExData(_this,(int)eExdataDefine.e334, bagCount+1);
            }
            else if (bag.GetBagId() == (int)eBagType.BaseItem)
            {
                AddExData(_this, (int)eExdataDefine.e333, bagCount+1);
            }
            
            return ErrorCodes.OK;
        }

        //使用道具和钥匙购买包裹
        public ErrorCodes BuySpaceBagByPaid(CharacterController _this, int bagId, int bagIndex, int needKeyCount)
        {
            var tbBag = Table.GetBagBase(bagId);
            if (tbBag == null)
            {
                return ErrorCodes.Error_BagID;
            }
            var bag = GetBag(_this, bagId);
            if (bag == null)
            {
                return ErrorCodes.Error_BagID;
            }
            var bagCount = bagIndex - bag.GetNowCount(); //计算需要买几格
            if (bagCount < 0 || bagIndex >= tbBag.MaxCapacity)
            {
                return ErrorCodes.ParamError;
            }
            var needKeys = 0;
            for (var i = bag.GetNowCount(); i <= bagIndex; ++i)
            {
                needKeys += bag.GetNeedKey(i, tbBag);
            }
            if (needKeys != needKeyCount)//如果和客户端计算的数量不一致
            {
                return ErrorCodes.ParamError;
            }
            var keyCounts = _this.mBag.GetItemCount(StaticParam.OpenBagKeyId);
            if (needKeys > keyCounts)
            {
                var needDiamond = (needKeys - keyCounts) * tbBag.TimeMult;
                var diamondCounts = _this.mBag.GetRes(eResourcesType.DiamondRes);
                if (needDiamond > diamondCounts)
                {
                    return ErrorCodes.DiamondNotEnough;
                }
                else
                {
                    _this.mBag.DelRes(eResourcesType.DiamondRes, needDiamond, eDeleteItemType.BagCellBuy);
                    if (keyCounts > 0)
                    {
                        _this.mBag.DeleteItem(StaticParam.OpenBagKeyId, keyCounts, eDeleteItemType.BagCellBuy);
                    }
                }
            }
            else
            {
                _this.mBag.DeleteItem(StaticParam.OpenBagKeyId, needKeys, eDeleteItemType.BagCellBuy);
            }

            AddExData(_this, (int)eExdataDefine.e304, bagCount + 1);
            AddExData(_this, (int)eExdataDefine.e305, bagCount + 1);
            bag.SetNowCount(bagIndex + 1);
            if (bag.GetBagId() == (int)eBagType.Equip)
            {
                AddExData(_this, (int)eExdataDefine.e334, bagCount + 1);
            }
            else if (bag.GetBagId() == (int)eBagType.BaseItem)
            {
                AddExData(_this, (int)eExdataDefine.e333, bagCount + 1);
            }

            return ErrorCodes.OK;
        }

        #endregion

        #region 精灵相关

        //精灵
        ////Type(0上阵，1下阵，2出战，3休息)
        /// 
//         public ErrorCodes ElfState(int index, int type, ref int state)
//         {
//             var elfBag = GetBag((int)eBagType.Elf);
//             var item = elfBag.GetItemByIndex(index);
//             if (item == null || item.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
//             var tbElf = Table.GetElf(item.GetId());
//             if (tbElf == null) return ErrorCodes.Error_ElfNotFind;
//             switch (type)
//             {
//                 case 0:
//                 {
//                     if (item.GetExdata(1) != 0)
//                     {
//                         return ErrorCodes.Error_ElfAlreadyBattle;
//                     }
// 
//                     int count = 0;
// 
//                     bool hasFight = false;
//                     foreach (var itemBase in elfBag.mLogics)
//                     {
//                         if (itemBase.GetId() != -1 )
//                         {
//                             if (itemBase.GetExdata(1) != 0)
//                             {
//                                 count++;    
//                             }
//                             if (itemBase.GetExdata(1) == 2)
//                             {
//                                 hasFight = true;
//                             }
//                         }
//                     }
//                     if (count == 3)
//                     {
//                         return ErrorCodes.Error_ElfBattleMax;
//                     }
//                     else
//                     {
//                         if (hasFight)
//                         {
//                             item.SetExdata(1, 1);
//                             state = 1;
//                         }
//                         else
//                         {
//                             item.SetExdata(1, 2);
//                             state = 2;
//                         }
//                     }
//                 }
//                     break;
//                 case 1:
//                     {
//                         if (item.GetExdata(1) == 0)
//                         {
//                             return ErrorCodes.Error_ElfNotBattle;
//                         }
//                         item.SetExdata(1, 0);
//                     }
//                     break;
//                 case 2:
//                     {//fight
//                         if (item.GetExdata(1) == 0)
//                         {
//                             return ErrorCodes.Error_ElfNotBattleMain;
//                         }
//                         if (item.GetExdata(1) == 2)
//                         {
//                             return ErrorCodes.Error_ElfIsBattleMain;
//                         }
//                         foreach (var itemBase in elfBag.mLogics)
//                         {
//                             if (itemBase.GetExdata(1) == 2)
//                             {
//                                 itemBase.MarkDbDirty();
//                                 itemBase.SetExdata(1,1); 
//                                 break;
//                             }
//                         }
//                         item.SetExdata(1,2);
//                     }
//                     break;
//                 case 3:
//                     {//disfight
//                         if (item.GetExdata(1) == 0)
//                         {
//                             return ErrorCodes.Error_ElfNotBattle;
//                         }
//                         if (item.GetExdata(1) == 1)
//                         {
//                             return ErrorCodes.Error_ElfNotBattleMain;
//                         }
//                         item.SetExdata(1, 1);
//                     }
//                     break;
//             }
//             item.MarkDbDirty();
//             mBag.RefreshElfAttr();
//             BooksChange();
//             return ErrorCodes.OK;
//         }
//         public ErrorCodes ElfReplace(int form, int to)
//         {
//             ErrorCodes ret = ErrorCodes.OK;
//             var elfBag = GetBag((int)eBagType.Elf);
//             var elfForm = elfBag.GetItemByIndex(form);
//             var elfTo = elfBag.GetItemByIndex(to);
// 
//             if (elfForm == null || elfForm.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
//             if (elfTo == null || elfTo.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
// 
//             if (elfForm.GetExdata(1) == 0 && elfTo.GetExdata(1) == 0)
//             {
//                 return ErrorCodes.Error_ElfNotBattle;
//             }
//             var state = elfForm.GetExdata(1);
//             elfForm.SetExdata(1,elfTo.GetExdata(1));
//             elfForm.MarkDbDirty();
//             elfTo.MarkDbDirty();
//             elfTo.SetExdata(1, state);
//             mBag.RefreshElfAttr();
//             BooksChange();
//             return ret;
//         }
        //当获得精灵类物品时
        public ItemBase AddElfItem(CharacterController _this, ItemBase item)
        {
            var tbElf = Table.GetElf(item.GetId());
            if (tbElf == null)
            {
                return item;
            }
            var elfBag = GetBag(_this, (int) eBagType.Elf);
            var freeIndex = elfBag.GetFirstFreeIndex();
            var nowFightCount = _this.GetElfFightCount();

            if (freeIndex < nowFightCount)
            {
                if (_this.CheckElfType(tbElf, freeIndex) != -1)
                {
                    return item;
                }
                if (_this.mBag.MoveItem((int) eBagType.Elf, item.GetIndex(), (int) eBagType.Elf, freeIndex, 1) ==
                    ErrorCodes.OK)
                {
                    _this.mBag.RefreshElfAttr();
                    BooksChange(_this);
                    if (freeIndex == 0)
                    {
                        _this.SetRankFlag(RankType.PetFight);
                    }
                    return elfBag.GetItemByIndex(freeIndex);
                }
            }
            return item;

            //    bool hasFight = false;
            //    var elfBag = GetBag((int)eBagType.Elf);
            //    var formatCount = 0;
            //    var hasSame = false;
            //    foreach (var itemBase in elfBag.mLogics)
            //    {
            //        if (itemBase.GetId() != -1)
            //        {
            //            if (itemBase.GetExdata(1) == 2)
            //            {
            //                hasFight = true;
            //            }
            //            if (itemBase.GetExdata(1) > 0)
            //            {
            //                formatCount++;

            //                if (itemBase.GetId() == item.GetId())
            //                {
            //                    hasSame = true;
            //                }

            //            }
            //        }
            //    }

            //    if (hasSame == false)
            //    {
            //        if (hasFight == false)
            //        {
            //            item.SetExdata(1, 2);
            //        }
            //        else
            //        {
            //            if (formatCount < 3)
            //            {
            //                item.SetExdata(1, 1);
            //            }
            //        }                
            //    }
            //    item.MarkDirty();
        }

//         //上阵精灵
//         public ErrorCodes BattleElf(int index)
//         {
//             if (index > 0 && index < 3)
//             {
//                 return ErrorCodes.Error_ElfAlreadyBattle;
//             }
//             var elfBag = GetBag((int) eBagType.Elf);
//             var item =  elfBag.GetItemByIndex(index);
//             if (item == null || item.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
//             var tbElf = Table.GetElf(item.GetId());
//             if (tbElf == null) return ErrorCodes.Error_ElfNotFind;
//             for (int i = 0; i != 3; ++i)
//             {
//                 var tempItem = elfBag.GetItemByIndex(i);
//                 if (tempItem == null) continue;
//                 var tbTempElf = Table.GetElf(tempItem.GetId());
//                 if (tbTempElf == null) continue;
//                 if (tbTempElf.ElfType == tbElf.ElfType)
//                 {
//                     return ErrorCodes.Error_ElfTypeSame;
//                 }
//             }
// 
//             for (int i = 0; i != 3; ++i)
//             {
//                 var tempItem = elfBag.GetItemByIndex(i);
//                 if (tempItem == null)
//                 {
//                     mBag.MoveItem((int)eBagType.Elf, index, (int)eBagType.Elf, i, 1);
//                     mBag.RefreshElfAttr();
//                     BooksChange();
//                     return ErrorCodes.OK;
//                 }
//             }
//             return ErrorCodes.Error_ElfBattleMax;
//         }
//         //下阵精灵
//         public ErrorCodes DisBattleElf(int index)
//         {
//             if (index < 0 && index >= 3)
//             {
//                 return ErrorCodes.Error_ElfNotBattle;
//             }
//             var item = GetItemByBagByIndex((int)eBagType.Elf, index);
//             if (item == null || item.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
//             int freeindex=mBag.mBags[(int) eBagType.Elf].GetFirstFreeIndex(3);
//             if (freeindex != -1)
//             {
//                 item.SetExdata(1, 0);
//                 mBag.MoveItem((int)eBagType.Elf, index, (int)eBagType.Elf, freeindex, 1);
//                 mBag.RefreshElfAttr();
//                 BooksChange();
//                 return ErrorCodes.OK;
//             }
//             return ErrorCodes.Error_ItemNoInBag_All;
//         }
//         //出战精灵
//         public ErrorCodes BattleMainElf(int index)
//         {
//             if (index < 0 && index >= 3)
//             {
//                 return ErrorCodes.Error_ElfNotBattle;
//             }
//             var item = GetItemByBagByIndex((int)eBagType.Elf, index);
//             if (item == null || item.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
//             if (item.GetExdata(1) == 1)
//             {
//                 return ErrorCodes.Error_ElfIsBattleMain;
//             }
//             //取消其他的出战（如果有的话）
//             for (int i = 0; i != 3; ++i)
//             {
//                 if(i==index) continue;
//                 var tempItem = GetItemByBagByIndex((int)eBagType.Elf, i);
//                 if (tempItem == null)
//                 {
//                     Logger.Warn("BattleMainElf GetItemByBagByIndex[{0}][{1}] is null", (int)eBagType.Elf, i);
//                     continue;
//                 }
//                 if (tempItem.GetId() < 0) continue;
//                 if (tempItem.GetExdata(1) == 1)
//                 {
//                     tempItem.SetExdata(1, 0);
//                     tempItem.MarkDirty();
//                 }
//             }
//             //出战
//             item.SetExdata(1, 1);
//             item.MarkDirty();
//             mBag.RefreshElfAttr();
//             BooksChange();
//             return ErrorCodes.OK;
//         }
//         //休息精灵
//         public ErrorCodes DisBattleMainElf(int index)
//         {
//             if (index < 0 && index >= 3)
//             {
//                 return ErrorCodes.Error_ElfNotBattle;
//             }
//             var item = GetItemByBagByIndex((int)eBagType.Elf, index);
//             if (item == null || item.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
//             if (item.GetExdata(1) == 0)
//             {
//                 return ErrorCodes.Error_ElfNotBattleMain;
//             }
//             //不出战
//             item.SetExdata(1, 0);
//             item.MarkDirty();
//             mBag.RefreshElfAttr();
//             BooksChange();
//             return ErrorCodes.OK;
//         }
        //增加物品冷却
        private void PushItemCd(CharacterController _this, int skillId)
        {
        }

        //物品冷却结束
        private void ItemCdClean(CharacterController _this, int skillId)
        {
        }

        //精灵岛购买体力
        public ErrorCodes PetIslandBuyTili(CharacterController _this)
        {
            //减次数
            var buyTimes = GetExData(_this, (int)eExdataDefine.e631);

            var vipLevel = _this.mBag.GetRes(eResourcesType.VipLevel);
            var tbVip = Table.GetVIP(vipLevel);
            if (tbVip == null)
            {
                return ErrorCodes.Unknow;
            }

            if (buyTimes >= tbVip.PetIslandBuyTimes)
            {
                return ErrorCodes.Error_BuyTili_NO_Times;
            }
            buyTimes += 1;
            SetExData(_this, (int)eExdataDefine.e631, buyTimes);

            //加体力
            var refreshTili = GetExData(_this, (int) eExdataDefine.e630); //每天刷新体力 最大到100
            var addTili = Table.GetServerConfig(935).ToInt();
            SetExData(_this, (int)eExdataDefine.e630, refreshTili + addTili);

            return ErrorCodes.OK;
        }

        public IEnumerator SyncSceneExData(Coroutine coroutine, ulong characterID, Dict_int_int_Data data)
        {
            var sceneMsg = LogicServer.Instance.SceneAgent.SyncExData(characterID, data);
            yield return sceneMsg.SendAndWaitUntilDone(coroutine);
            if (sceneMsg.State != MessageState.Reply)
            {
                Logger.Error("SyncSceneExData() return with state = {0}", sceneMsg.State);
                yield break;
            }
        }
        public IEnumerator SyncSceneFlagData(Coroutine coroutine, ulong characterID, Dict_int_int_Data data)
        {
            var sceneMsg = LogicServer.Instance.SceneAgent.SyncFlagData(characterID, data);
            yield return sceneMsg.SendAndWaitUntilDone(coroutine);
            if (sceneMsg.State != MessageState.Reply)
            {
                Logger.Error("SyncFlagData() return with state = {0}", sceneMsg.State);
                yield break;
            }
        }

        public int PetIslandReduceTili(CharacterController _this, int num)
        {
            var refreshTili = GetExData(_this, (int)eExdataDefine.e630); //每天刷新体力 最大到100
            var result = refreshTili - num;

            result = Math.Max(result, 0);
            SetExData(_this, (int)eExdataDefine.e630, result);

            return result;
        }

        public int PetIslandGetTili(CharacterController _this)
        {
            var refreshTili = GetExData(_this, (int)eExdataDefine.e630); //每天刷新体力 最大到100
            return refreshTili;
        }

        #endregion

        #region 同步数据 To Scene



        public void EquipModelStateChange(CharacterController _this, int nPart, int nState, ItemBase item)
        {
            CoroutineFactory.NewCoroutine(SceneEquipModelStateChange, _this, _this.mGuid, nPart * 10, nState, item.mDbData).MoveNext();
        }

        public IEnumerator SceneEquipModelStateChange(Coroutine coroutine,
                                                     CharacterController _this,
                                                     ulong characterId,
                                                     int nPart,
                                                     int nState,
                                                     ItemBaseData Equip)
        {
            var msg = LogicServer.Instance.SceneAgent.SceneEquipModelStateChange(_this.mGuid, nPart, nState, Equip);
            yield return msg.SendAndWaitUntilDone(coroutine);

            if (msg.State == MessageState.Reply)
            {
                Logger.Info("SceneShiZhuangStateChange is Success  Part={0}", nPart);
            }
            else if (msg.State == MessageState.Error)
            {
                Logger.Error("SceneShiZhuangStateChange is Error={0}", msg.ErrorCode);
            }
            else if (msg.State == MessageState.Timeout)
            {
                Logger.Warn("SceneShiZhuangStateChange Timeout CharacterId={0}", characterId);
            }
        }

        //已穿装备修改
        public void
            EquipChange(CharacterController _this, int nType, int nPart, int nIndex, ItemBase item)
        {
            PlayerLog.WriteLog(_this.mGuid, "----------EquipChange----------{0},{1},{2},{3}", nType, nPart, nIndex,
                item.GetId());
            CoroutineFactory.NewCoroutine(SceneEquipChange, _this, _this.mGuid, nType, nPart*10 + nIndex, item.mDbData)
                .MoveNext();
        }

        public IEnumerator SceneEquipChange(Coroutine coroutine,
                                            CharacterController _this,
                                            ulong characterId,
                                            int nType,
                                            int nPart,
                                            ItemBaseData Equip)
        {
            //int index = 0;
            //while (true)
            {
                var msg = LogicServer.Instance.SceneAgent.SceneEquipChange(_this.mGuid, nType, nPart, Equip);
                yield return msg.SendAndWaitUntilDone(coroutine);

                if (msg.State == MessageState.Reply)
                {
                    Logger.Info("SceneEquipChange is Success  Part={0}", nPart);
                    //break;
                }
                else if (msg.State == MessageState.Error)
                {
                    //异常
                    Logger.Error("SceneEquipChange is Error={0}", msg.ErrorCode);
                    // break;
                }
                else if (msg.State == MessageState.Timeout)
                {
                    //超时
                    Logger.Warn("SceneEquipChange Timeout CharacterId={0}", characterId);
                    //break;
                }
                //index++;
                //if (index >=5)
                //{
                //    yield break;
                //}
            }
        }

        public void ElfChange(CharacterController _this, List<int> removeBuff, Dictionary<int, int> addBuff)
        {
            if (addBuff.Count > 0 || removeBuff.Count > 0)
                CoroutineFactory.NewCoroutine(SceneElfChange, _this, _this.mGuid, removeBuff, addBuff).MoveNext();
        }

        public IEnumerator SceneElfChange(Coroutine coroutine,
            CharacterController _this,
            ulong characterId,
            List<int> removeBuff,
            Dictionary<int, int> addBuff)
        {
            var rb = new Int32Array();
            rb.Items.AddRange(removeBuff);
            var ab = new Dict_int_int_Data();
            ab.Data.AddRange(addBuff);

            SetRefreshFightPoint(_this, true);
            var fightPoint = GetElfFightPoint(_this);
            var msg = LogicServer.Instance.SceneAgent.SSSceneElfChange(characterId, rb, ab, fightPoint);
            yield return msg.SendAndWaitUntilDone(coroutine);

            if (msg.State == MessageState.Reply)
            {
                Logger.Info("SceneElfBuff is Success");
            }
            else if (msg.State == MessageState.Error)
            {
                //异常
                Logger.Error("SceneElfBuff is Error={0}", msg.ErrorCode);
            }
            else if (msg.State == MessageState.Timeout)
            {
                //超时
                Logger.Warn("SceneElfBuff Timeout CharacterId={0}", characterId);
            }
        }

        public void GetElfBuff(CharacterController _this, Dictionary<int, int> buffs)
        {
            //var item = _this.GetItemByBagByIndex((int)eBagType.Elf, bagIndex) as ElfItem;
            var elfBag = _this.GetBag((int) eBagType.Elf);
            if (elfBag == null)
            {
                return;
            }

            for (var i = 0; i < 3; ++i)
            {
                var item = elfBag.GetItemByIndex(i) as ElfItem;
                if (item == null)
                {
                    continue;
                }
                if (item.GetId() < 0)
                {
                    continue;
                }

                item.FillAllBuff(buffs);
            }
        }
        public void GetMountBuff(CharacterController _this, Dictionary<int, int> buffs)
        {
            foreach (var v in _this.mMount.mDbData.Skills)
            {
                if(v.Value>0)
                    buffs.Add(v.Key,v.Value);
            }
        } //已穿装备修改


        public void Mount(CharacterController _this, int MountId)
        {
            CoroutineFactory.NewCoroutine(SceneMount, _this,_this.mGuid, MountId)
                .MoveNext();            
        }

        public void SetMoniterData(CharacterController _this, MsgChatMoniterData data)
        {
            _this.moniterData = data;
        }
        public IEnumerator SceneMount(Coroutine coroutine,
            CharacterController _this,
            ulong characterId,
            int MountId)
        {
                var msg = LogicServer.Instance.SceneAgent.SyncSceneMount(_this.mGuid,MountId);
                yield return msg.SendAndWaitUntilDone(coroutine);
        }

        public void SetRefreshFightPoint(CharacterController _this, bool refresh)
        {
            _this.RefreshElfFightPoint = refresh;
        }

        public int GetElfFightPoint(CharacterController _this)
        {
            if (_this.RefreshElfFightPoint)
            {
                _this.RefreshElfFightPoint = false;

                var value = 0;
                var buffDict = new Dictionary<int, int>();
                GetElfBuff(_this, buffDict); 
                foreach (var buff in buffDict)
                {
                    value += ElfItem.GetOneBuffFightPoint(buff.Key, buff.Value);
                }
                _this.ElfFightPoint = value;
            }

            return _this.ElfFightPoint;
        }

        //天赋修改
        public void TalentChange(CharacterController _this, int nType, int nTalent, int nLevel)
        {
            PlayerLog.WriteLog(_this.mGuid, "----------TalentChange----------{0},{1}", nTalent, nLevel);
            CoroutineFactory.NewCoroutine(SceneInnateChange, _this, _this.mGuid, nType, nTalent, nLevel).MoveNext();
        }

        public IEnumerator SceneInnateChange(Coroutine coroutine,
                                             CharacterController _this,
                                             ulong characterId,
                                             int nType,
                                             int nTalent,
                                             int nLevel)
        {
            var index = 0;
            while (true)
            {
                var msg = LogicServer.Instance.SceneAgent.SceneInnateChange(_this.mGuid, nType, nTalent, nLevel);
                yield return msg.SendAndWaitUntilDone(coroutine);

                if (msg.State == MessageState.Reply)
                {
                    Logger.Info("SceneInnateChange is Success  Type={0} Talent={1} Level={2}", nType, nTalent, nLevel);
                    break;
                }
                if (msg.State == MessageState.Error)
                {
                    //异常
                    Logger.Error("SceneInnateChange is Error={0}", msg.ErrorCode);
                    break;
                }
                if (msg.State == MessageState.Timeout)
                {
                    //超时
                    Logger.Warn("SceneInnateChange Timeout CharacterId={0}", characterId);
                    break;
                }
                index++;
                if (index >= 1)
                {
                    yield break;
                }
            }
        }

        public void OnAddCharacterContribution(CharacterController _this,int nCount)
        {
            CoroutineFactory.NewCoroutine(SSAddCharacterContribution, _this,nCount).MoveNext();
        }
        public IEnumerator SSAddCharacterContribution(Coroutine coroutine, CharacterController _this, int nCount)
        {
            var msg = LogicServer.Instance.TeamAgent.SSAddAllianceContribution(_this.mGuid,_this.serverId,_this.mGuid,_this.mAlliance.AllianceId,nCount);
            yield return msg.SendAndWaitUntilDone(coroutine);
        }

        //技能修改
        public void SkillChange(CharacterController _this, int nType, int nSkillId, int nLevel)
        {
            PlayerLog.WriteLog(_this.mGuid, "----------SkillChange----------{0},{1}", nSkillId, nLevel);

            CoroutineFactory.NewCoroutine(SceneSkillChange, _this, _this.mGuid, nType, nSkillId, nLevel).MoveNext();
        }

        public IEnumerator SceneSkillChange(Coroutine coroutine,
                                            CharacterController _this,
                                            ulong characterId,
                                            int nType,
                                            int nSkillId,
                                            int nLevel)
        {
            var index = 0;
            while (true)
            {
                var msg = LogicServer.Instance.SceneAgent.SceneSkillChange(_this.mGuid, nType, nSkillId, nLevel);
                yield return msg.SendAndWaitUntilDone(coroutine);

                if (msg.State == MessageState.Reply)
                {
                    Logger.Info("SceneSkillChange is Success  Type={0} Talent={1} Level={2}", nType, nSkillId, nLevel);
                    break;
                }
                if (msg.State == MessageState.Error)
                {
                    //异常
                    Logger.Error("SceneSkillChange is Error={0}", msg.ErrorCode);
                    break;
                }
                if (msg.State == MessageState.Timeout)
                {
                    //超时
                    Logger.Warn("SceneSkillChange Timeout CharacterId={0}", characterId);
                    break;
                }
                index++;
                if (index >= 1)
                {
                    yield break;
                }
            }
        }

        //重新装备技能
        public void EquipSkillChange(CharacterController _this, Int32Array dels, Int32Array adds, Int32Array Lvls)
        {
            PlayerLog.WriteLog(_this.mGuid, "----------EquipSkillChange----------{0},{1}", dels.Items.GetDataString(),
                adds.Items.GetDataString());

            CoroutineFactory.NewCoroutine(SceneEquipSkillChange, _this, _this.mGuid, dels, adds, Lvls).MoveNext();
        }

        //重新装备技能
        public IEnumerator SceneEquipSkillChange(Coroutine coroutine,
                                                 CharacterController _this,
                                                 ulong characterId,
                                                 Int32Array dels,
                                                 Int32Array adds,
                                                 Int32Array lvls)
        {
            var index = 0;
            while (true)
            {
                var msg = LogicServer.Instance.SceneAgent.SceneEquipSkill(_this.mGuid, dels, adds, lvls);
                yield return msg.SendAndWaitUntilDone(coroutine);

                if (msg.State == MessageState.Reply)
                {
                    //Logger.Info("SceneSkillChange is Success  Type={0} Talent={1} Level={2}", nType, nSkillId, nLevel);
                    break;
                }
                if (msg.State == MessageState.Error)
                {
                    //异常
                    //Logger.Error("SceneSkillChange is Error={0}", msg.ErrorCode);
                    break;
                }
                if (msg.State == MessageState.Timeout)
                {
                    //超时
                    //Logger.Warn("SceneSkillChange Timeout CharacterId={0}", characterId);
                    break;
                }
                index++;
                if (index >= 1)
                {
                    yield break;
                }
            }
        }

        public int GetBooksAttr(CharacterController _this, Dictionary<int, int> attrs, Dictionary<int, int> monsterAttrs)
        {
            //图鉴
            attrs.AddRange(_this.mBook.attrs);
            monsterAttrs.AddRange(_this.mBook.MonsterAttrs);
            //精灵
            foreach (var i in _this.mBag.Elfattrs)
            {
                attrs.modifyValue(i.Key, i.Value);
            }
            //勋章
            foreach (var i in _this.mBag.Medalattrs)
            {
                attrs.modifyValue(i.Key, i.Value);
            }
            //宝石属性
            foreach (var i in _this.mBag.Gemattrs)
            {
                attrs.modifyValue(i.Key, i.Value);
            }
            //家园属性
            foreach (var i in _this.mCity.GetRefreshAttr())
            {
                attrs.modifyValue(i.Key, i.Value);
            }
            //坐骑属性
            foreach (var i in _this.mMount.Mountattrs)
            {
                attrs.modifyValue(i.Key, i.Value);
            }
            //手动分配点数  吃果子点数
            for (var i = 1; i < 5; ++i)
            {
                attrs.modifyValue(i, GetExData(_this, i + 4) + GetExData(_this, i + 8));
            }
            //转生属性
            var ladder = GetExData(_this, (int) eExdataDefine.e51);
            if (ladder > 0)
            {
                var tbL = Table.GetTransmigration(ladder);
                if (tbL == null)
                {
                    return -1;
                }
                attrs.modifyValue(105, tbL.AttackAdd);
                attrs.modifyValue(10, tbL.PhyDefAdd);
                attrs.modifyValue(11, tbL.MagicDefAdd);
                attrs.modifyValue(19, tbL.HitAdd);
                attrs.modifyValue(20, tbL.DodgeAdd);
                attrs.modifyValue(13, tbL.LifeAdd);
            }
	        return _this.mBook.Fight;
        }

        //图鉴，加点，果子，精灵（属性修改）
        public void BooksChange(CharacterController _this)
        {
            if (_this.Proxy != null)
            {
                PlayerLog.WriteLog(_this.mGuid, "----------BooksChange----------");
                var attrList = new Dict_int_int_Data();
                var monsterAttrs = new Dict_int_int_Data();
                GetBooksAttr(_this, attrList.Data, monsterAttrs.Data);

                CoroutineFactory.NewCoroutine(SceneBooksChange, _this, _this.mGuid, attrList, monsterAttrs).MoveNext();
            }
        }

        public IEnumerator SceneBooksChange(Coroutine coroutine,
                                            CharacterController _this,
                                            ulong characterId,
                                            Dict_int_int_Data dic, Dict_int_int_Data monsterdic)
        {
            var index = 0;
            while (true)
            {
                var msg = LogicServer.Instance.SceneAgent.SceneBookAttrChange(characterId, dic, monsterdic);
                yield return msg.SendAndWaitUntilDone(coroutine);

                if (msg.State == MessageState.Reply)
                {
                    Logger.Info("SceneBooksChange is Success ! characterId={0}", characterId);
                    break;
                }
                if (msg.State == MessageState.Error)
                {
                    //异常
                    Logger.Error("SceneBooksChange is Error={0}", msg.ErrorCode);
                    break;
                }
                if (msg.State == MessageState.Timeout)
                {
                    //超时
                    Logger.Warn("SceneBooksChange Timeout CharacterId={0}", characterId);
                    break;
                }
                index++;
                if (index >= 5)
                {
                    yield break;
                }
            }
        }

        #endregion

        #region 一级属性相关

        //分配加点
        public ErrorCodes AddAttrPoint(CharacterController _this,
                                       int Strength,
                                       int Agility,
                                       int Intelligence,
                                       int Endurance)
        {
            var freePoint = GetExData(_this, 52);
            if (Strength < 0 || Agility < 0 || Intelligence < 0 || Endurance < 0)
            {
                return ErrorCodes.Unknow;
            }
            var needPoint = (long)Strength + Agility + Intelligence + Endurance;
            if (needPoint > freePoint)
            {
                return ErrorCodes.Error_AttrPointNotEnough;
            }
            AddExData(_this, 52, -(int)needPoint, true);
            AddExData(_this, 5, Strength, true);
            AddExData(_this, 6, Agility, true);
            AddExData(_this, 7, Intelligence, true);
            AddExData(_this, 8, Endurance, true);
            BooksChange(_this);

            if (GetFlag(_this, 2))
            {
                if (!GetFlag(_this, 518))
                {
                    SetFlag(_this, 518);
                }
            }

            return ErrorCodes.OK;
        }

        //获得基础点数
        public int GetAttrPoint(CharacterController _this, eAttributeType attrId)
        {
            var value = 0;
            switch (attrId)
            {
                case eAttributeType.Strength:
                {
                    value = GetExData(_this, 5) + GetExData(_this, 9) +
                            Attributes.GetAttrValue(GetRole(_this), GetLevel(_this), (int) attrId - 1);
                }
                    break;
                case eAttributeType.Agility:
                {
                    value = GetExData(_this, 6) + GetExData(_this, 10) +
                            Attributes.GetAttrValue(GetRole(_this), GetLevel(_this), (int) attrId - 1);
                }
                    break;
                case eAttributeType.Intelligence:
                {
                    value = GetExData(_this, 7) + GetExData(_this, 11) +
                            Attributes.GetAttrValue(GetRole(_this), GetLevel(_this), (int) attrId - 1);
                }
                    break;
                case eAttributeType.Endurance:
                {
                    value = GetExData(_this, 8) + GetExData(_this, 12) +
                            Attributes.GetAttrValue(GetRole(_this), GetLevel(_this), (int) attrId - 1);
                }
                    break;
                default:
                    return -1;
            }
            return value;
        }

        //洗点
        public ErrorCodes RefreshAttrPoint(CharacterController _this, ref int newPoint)
        {
            var Strength = GetExData(_this, 5);
            var Agility = GetExData(_this, 6);
            var Intelligence = GetExData(_this, 7);
            var Endurance = GetExData(_this, 8);
            var totle = Strength + Agility + Intelligence + Endurance;
            if (totle == 0)
            {
                return ErrorCodes.OK;
            }
            AddExData(_this, (int) eExdataDefine.e52, totle, true);
            newPoint = GetExData(_this, (int) eExdataDefine.e52);
            SetExData(_this, 5, 0, true);
            SetExData(_this, 6, 0, true);
            SetExData(_this, 7, 0, true);
            SetExData(_this, 8, 0, true);
            BooksChange(_this);
            return ErrorCodes.OK;
        }

        //专职
        public ErrorCodes ChangeRole(CharacterController _this)
        {
            return ErrorCodes.OK;
        }

        #endregion

        #region 副本相关

        //通知Scene
        public IEnumerator AskEnterDungeon(Coroutine coroutine,
                                           CharacterController _this,
                                           int serverId,
                                           int sceneId,
                                           ulong guid,
                                           SceneParam param)
        {
            //请求进入副本时，根据合服ID进行
            serverId = SceneExtension.GetServerLogicId(serverId);
            var msg = LogicServer.Instance.SceneAgent.AskEnterDungeon(_this.mGuid, serverId, sceneId, guid, param);
            yield return msg.SendAndWaitUntilDone(coroutine);

            if (msg.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("AskEnterDungeon msg.ErrorCode={0}", msg.ErrorCode);
                yield break;
            }

            if (msg.State == MessageState.Reply)
            {
                Logger.Info("AskEnterDungeon is Success  sceneId={0}", sceneId);
            }
            else if (msg.State == MessageState.Error)
            {
                //异常
                Logger.Error("AskEnterDungeon is Error={0}", msg.ErrorCode);
            }
            else if (msg.State == MessageState.Timeout)
            {
                //超时
                Logger.Warn("AskEnterDungeon Timeout serverId={0}", serverId);
            }
        }

        //购买副本次数
        public ErrorCodes BuyFubenCount(CharacterController _this, int fubenId)
        {
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                return ErrorCodes.Error_FubenID;
            }
            //vip
            var vipLevel = _this.mBag.GetRes(eResourcesType.VipLevel);
            var tbVip = Table.GetVIP(vipLevel);
            if (tbFuben.TodayBuyCount + tbVip.PlotFubenResetCount <= GetExData(_this, tbFuben.ResetExdata))
            {
                return ErrorCodes.Error_FubenResetCountNotEnough;
            }
            if (_this.mBag.GetItemCount(tbFuben.ResetItemId) < tbFuben.ResetItemCount)
            {
                return ErrorCodes.ItemNotEnough;
            }
            _this.mBag.DeleteItem(tbFuben.ResetItemId, tbFuben.ResetItemCount, eDeleteItemType.FubenCountBuy);
            AddExData(_this, tbFuben.ResetExdata, 1);

            try
            {
                var klog = string.Format("fubenreset_info#{0}|{1}|{2}|{3}|{4}|{5}",
                    _this.serverId,
                    _this.mGuid,
                    _this.GetLevel(),
                    fubenId,
                    tbFuben.AssistType,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }

            return ErrorCodes.OK;
        }
        public ErrorCodes TowerSweep(CharacterController _this, TowerSweepResult respone)
        {

            //int nMaxFloor = _this.GetExData((int)eExdataDefine.e621);
            int nCurFloor = _this.GetExData((int)eExdataDefine.e623);
            int nSweep = _this.GetExData((int) eExdataDefine.e622);
            var tb = Table.GetClimbingTower(nCurFloor);

            if (tb == null)
            {
                return ErrorCodes.Error_CanNot_Sweep_Limit;
            }
            if (nSweep <= 0)
            {
                return ErrorCodes.Error_CanNot_Sweep_Limit;
            }

            int nDstFloor = tb.SweepFloor;
            int nRealFloor = 0;
            for (int i = 1; i <= nDstFloor; i++)
            {
                var tbTower = Table.GetClimbingTower(i);
                if (tbTower != null)
                {
                    var tbFuben = Table.GetFuben(tbTower.FubenId);
                    if (tbFuben == null)
                        continue;
                    nRealFloor = i;
                    var itemList = new Dictionary<int, int>();
                    TowerSweepReward item = new TowerSweepReward();
                    for (int idx = 0; idx < tbTower.RewardList.Count && idx < tbTower.NumList.Count; idx++)
                    {

                        item.IDList.Add(tbTower.RewardList[idx]);
                        item.NumList.Add(tbTower.NumList[idx]);
                        itemList.modifyValue(tbTower.RewardList[idx], tbTower.NumList[idx]);
                    }
                    item.Floor = i;
                    respone.RewardList.Add(item);

                    var ret = _this.mBag.AddItemOrMail(50, itemList, null , eCreateItemType.PassFuben, tbFuben.Name);
                }
            }
          // respone.CurFloor = nRealFloor;
          // _this.SetExData((int)eExdataDefine.e623,nRealFloor);
            _this.SetExData((int)eExdataDefine.e622,nSweep-1);
            return ErrorCodes.OK;
        }


        public ErrorCodes CheckTowerDailyInfo(CharacterController _this)
        {
            DateTime _last = _this.lExdata64.GetTime(Exdata64TimeType.LastTowerCheckTime);
            DateTime _now = DateTime.Now;
            _last = Convert.ToDateTime(string.Format("{0}-{1}-{2}", _last.Year, _last.Month, _last.Day));
            _now = Convert.ToDateTime(string.Format("{0}-{1}-{2}", _now.Year, _now.Month, _now.Day));
            int days = (_now - _last).Days;
            if (days == 0)
            {
                return ErrorCodes.OK;
            }

            int cur = _this.GetExData((int) eExdataDefine.e623);

            for (int i = 0; i < days; i++)
            {
                var tb = Table.GetClimbingTower(cur);
                if (tb == null)
                    break;
                cur = tb.SweepFloor + 1;
            }
            _this.SetExData((int)eExdataDefine.e623, cur);
            _this.lExdata64.SetTime(Exdata64TimeType.LastTowerCheckTime, _now);
            return ErrorCodes.OK;
        }
        public ErrorCodes TowerBuySweepTimes(CharacterController _this)
        {
            int nCurBuyTimes = _this.GetExData((int)eExdataDefine.e624);
            int nCurTimes = _this.GetExData((int) eExdataDefine.e622);
            int nCurFloor = _this.GetExData((int) eExdataDefine.e623);
            var tbTower = Table.GetClimbingTower(nCurFloor);
            if (tbTower == null)
            {
                return ErrorCodes.Unknow;
            }
            int costIdx = tbTower.SweepFloor;
            var vipLv = _this.mBag.GetRes(eResourcesType.VipLevel);

            var tbVip = Table.GetVIP(vipLv);
            if (nCurBuyTimes >= tbVip.TowerSweepTimes)
            {
                return ErrorCodes.Error_CanNot_Sweep_Buy_Times;
            }

            var tbSkill = Table.GetSkillUpgrading(140000);
            if (tbSkill == null)
                return ErrorCodes.Unknow;
            int cost = tbSkill.GetSkillUpgradingValue(costIdx - 1);
            
            var diamond = _this.mBag.GetRes(eResourcesType.DiamondRes);
            if (diamond < cost)
            {
                return ErrorCodes.DiamondNotEnough;
            }
            _this.mBag.DelRes(eResourcesType.DiamondRes, cost, eDeleteItemType.TowerSweepBuy);
            _this.SetExData((int) eExdataDefine.e624, nCurBuyTimes + 1);
            _this.SetExData((int)eExdataDefine.e622,nCurTimes+1);

            return ErrorCodes.OK;
        }
        //扫荡副本
        public ErrorCodes PassFuben(CharacterController _this, int fubenId, DrawResult dataResult)
        {
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                return ErrorCodes.Error_FubenID;
            }

            if (tbFuben.MainType != (int) eDungeonMainType.Fuben)
            {
                return ErrorCodes.Error_FubenNoPass;
            }
            //扫荡条件
            if (tbFuben.TimeExdata < 0)
            {
                return ErrorCodes.Error_FubenNoPass;
            }
            var oldTimes = GetExData(_this, tbFuben.TimeExdata);
            if (oldTimes > tbFuben.SweepLimitMinutes*60 && oldTimes > 0)
            {
                return ErrorCodes.Error_PassFubenTimeNotEnough;
            }
            //条件表
            if (CheckCondition(_this, tbFuben.EnterConditionId) != -2)
            {
                return ErrorCodes.Error_LevelNoEnough;
            }
            for (var i = 0; i != tbFuben.NeedItemId.Length; i++)
            {
                if (tbFuben.NeedItemId[i] >= 0)
                {
                    if (_this.mBag.GetItemCount(tbFuben.NeedItemId[i]) < tbFuben.NeedItemCount[i])
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                }
            }
            if (tbFuben.TodayCount != -1)
            {
//副本没有次数限制
                if (GetExData(_this, tbFuben.TodayCountExdata) >=
                    tbFuben.TodayCount + GetExData(_this, tbFuben.ResetExdata))
                {
                    return ErrorCodes.Error_FubenCountNotEnough;
                }
            }

            #region 检查并扣除扫荡券

            var count = _this.mBag.GetItemCount(StaticParam.SweepCouponId);
            if (count <= 0)
            {
//扫荡券不足
                return ErrorCodes.Error_SweepCouponNotEnough;
            }
            _this.mBag.DeleteItem(StaticParam.SweepCouponId, 1, eDeleteItemType.EnterFuben);

            #endregion

            //完成副本
            AddFubenCount(_this, tbFuben);

            //掉落
            var itemList = new Dictionary<int, int>();
            foreach (var i in tbFuben.ScanReward)
            {
                if (i < 0)
                {
                    continue;
                }
                _this.DropMother(i, itemList);
            }

            //扫荡的处理
            if (tbFuben.IsDynamicExp == 1)
            {
                var exp = (int) (tbFuben.DynamicExpRatio/10000.0f*Table.GetLevelData(GetLevel(_this)).DynamicExp);
                itemList.modifyValue((int) eResourcesType.ExpRes, exp);
            }
            else if (tbFuben.ScanExp > 0)
            {
                itemList.modifyValue((int) eResourcesType.ExpRes, tbFuben.ScanExp);
            }
            if (tbFuben.ScanGold > 0)
            {
                itemList.modifyValue((int) eResourcesType.GoldRes, tbFuben.ScanGold);
            }

            var ret = _this.mBag.AddItemOrMail(50, itemList, dataResult.Items, eCreateItemType.PassFuben, tbFuben.Name);

            //翻牌
            ItemBase tempItemBase;
            dataResult.SelectIndex = PushDraw(_this, tbFuben.DrawReward, out tempItemBase, true, fubenId);
            if (tempItemBase == null)
            {
                dataResult.DrawItem = new ItemBaseData
                {
                    ItemId = -1
                };
            }
            else
            {
                dataResult.DrawItem = tempItemBase.mDbData;
            }
            dataResult.DrawId = tbFuben.DrawReward;


            try
            {
                var klog = string.Format("fubensweep_info#{0}|{1}|{2}|{3}|{4}|{5}",
                    _this.serverId,
                    _this.mGuid,
                    _this.GetLevel(),
                    fubenId,
                    tbFuben.AssistType,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }

            return ret;
        }

        //条件判断
        public ErrorCodes CheckFuben(CharacterController _this, int fubenId)
        {
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                return ErrorCodes.Error_FubenID;
            }
            //次数条件
            if (tbFuben.TodayCount != -1)
            {
//副本没有次数限制
                var maxCount = tbFuben.TodayCount + GetExData(_this, tbFuben.ResetExdata);
                var vipLevel = _this.mBag.GetRes(eResourcesType.VipLevel);
                var tbVip = Table.GetVIP(vipLevel);
                if (tbVip != null)
                {
                    if (tbFuben.AssistType == 4)
                    {
                        maxCount += tbVip.DevilBuyCount;
                    }
                    else if (tbFuben.AssistType == 5)
                    {
                        maxCount += tbVip.BloodBuyCount;
                    }
                }
                
                if (GetExData(_this, tbFuben.TodayCountExdata) >= maxCount)
                {
                    return ErrorCodes.Error_FubenCountNotEnough;
                }
            }
            //道具
            for (var i = 0; i != tbFuben.NeedItemId.Length; i++)
            {
                if (tbFuben.NeedItemId[0] >= 0)
                {
                    if (_this.mBag.GetItemCount(tbFuben.NeedItemId[0]) < tbFuben.NeedItemCount[0])
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                }
            }
            //条件表
            if (CheckCondition(_this, tbFuben.EnterConditionId) != -2)
            {
                return ErrorCodes.Error_LevelNoEnough;
            }
            ////检查有没有未领取的奖励(血色，恶魔)
            //if (!CheckDungeonReward(_this, tbFuben))
            //{
            //    return ErrorCodes.Error_FubenRewardNotReceived;
            //}
            return ErrorCodes.OK;
        }
        public ErrorCodes CheckFubenDetail(CharacterController _this, int fubenId, ref int DetailCode)
        {
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                return ErrorCodes.Error_FubenID;
            }
            //次数条件
            if (tbFuben.TodayCount != -1)
            {
                //副本没有次数限制
                var maxCount = tbFuben.TodayCount + GetExData(_this, tbFuben.ResetExdata);
                var vipLevel = _this.mBag.GetRes(eResourcesType.VipLevel);
                var tbVip = Table.GetVIP(vipLevel);
                if (tbVip != null)
                {
                    if (tbFuben.AssistType == 4)
                    {
                        maxCount += tbVip.DevilBuyCount;
                    }
                    else if (tbFuben.AssistType == 5)
                    {
                        maxCount += tbVip.BloodBuyCount;
                    }
                }

                if (GetExData(_this, tbFuben.TodayCountExdata) >= maxCount)
                {
                    return ErrorCodes.Error_FubenCountNotEnough;
                }
            }
            //道具
            for (var i = 0; i != tbFuben.NeedItemId.Length; i++)
            {
                if (tbFuben.NeedItemId[0] >= 0)
                {
                    if (_this.mBag.GetItemCount(tbFuben.NeedItemId[0]) < tbFuben.NeedItemCount[0])
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                }
            }
            //条件表
            if (CheckCondition(_this, tbFuben.EnterConditionId) != -2)
            {
                var _assistType = (eDungeonAssistType)tbFuben.AssistType;
                if (_assistType == eDungeonAssistType.BloodCastle || _assistType == eDungeonAssistType.DevilSquare)
                {
                    var _fubenCount = _this.GetExData(tbFuben.TotleExdata);
                    if (_fubenCount > 0)
                    {
                        DetailCode = 491;
                    }
                    else
                    {
                        DetailCode = 489;

                    }
                }

                return ErrorCodes.Error_LevelNoEnough;
            }
            ////检查有没有未领取的奖励(血色，恶魔)
            //if (!CheckDungeonReward(_this, tbFuben))
            //{
            //    return ErrorCodes.Error_FubenRewardNotReceived;
            //}
            return ErrorCodes.OK;
        }
        //进入副本
        public IEnumerator EnterFuben(Coroutine co,
                                      CharacterController _this,
                                      int fubenId,
                                      AsyncReturnValue<ErrorCodes> errCode,
                                      AsyncReturnValue<int> check)
        {
            #region 检查，如果在当前副本内，则不能再次进入

            check.Value = -1;
            errCode.Value = ErrorCodes.OK;
            var sceneSimpleMsg = LogicServer.Instance.SceneAgent.GetSceneSimpleData(_this.mGuid, 0);
            yield return sceneSimpleMsg.SendAndWaitUntilDone(co);
            if (sceneSimpleMsg.State != MessageState.Reply)
            {
                Logger.Error("In EnterFuben(), GetSceneSimpleData return with dbSceneSimple.State = {0}",
                    sceneSimpleMsg.State);
                errCode.Value = ErrorCodes.Unknow;
                yield break;
            }
            if (sceneSimpleMsg.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("In EnterFuben(), GetSceneSimpleData return with dbSceneSimple.ErrorCode = {0}",
                    sceneSimpleMsg.ErrorCode);
                errCode.Value = ErrorCodes.Unknow;
                yield break;
            }
            var tbScene = Table.GetScene(sceneSimpleMsg.Response.SceneId);
            if (tbScene != null && tbScene.FubenId == fubenId)
            {
                Logger.Warn("Error_AlreadyInThisDungeon {0} {1}", _this.mGuid, tbScene.Id);
                errCode.Value = ErrorCodes.Error_AlreadyInThisDungeon;
                yield break;
            }

            #endregion

            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                Logger.Error("Fuben is null {0} {1}", _this.mGuid, fubenId);
                errCode.Value = ErrorCodes.Error_FubenID;
                yield break;
            }

            //条件表
            var checkResult = CheckCondition(_this, tbFuben.EnterConditionId);
            if (checkResult != -2)
            {
                check.Value = checkResult;
                errCode.Value = ErrorCodes.Error_Condition;
                yield break;
            }

            if (tbFuben.TodayCount != -1)
            {
//副本有次数限制
                var totalCount = tbFuben.TodayCount + GetExData(_this, tbFuben.ResetExdata);
                if (tbFuben.AssistType == 4 || tbFuben.AssistType == 5)
                {
                    var vipLevel = _this.mBag.GetRes(eResourcesType.VipLevel);
                    var tbVip = Table.GetVIP(vipLevel);
                    if (tbFuben.AssistType == 4)
                    {
//恶魔广场
                        totalCount += tbVip.DevilBuyCount;
                    }
                    else if (tbFuben.AssistType == 5)
                    {
//血色城堡
                        totalCount += tbVip.BloodBuyCount;
                    }
                }
                if (GetExData(_this, tbFuben.TodayCountExdata) >= totalCount)
                {
                    errCode.Value = ErrorCodes.Error_FubenCountNotEnough;
                    yield break;
                }
            }

            var param = new SceneParam();
            //检查副本开启时间
            if (tbFuben.OpenTime[0] != -1)
            {
                int hour;
                int min;

                if (Utils.GetDungeonOpenTime(tbFuben, out hour, out min))
                {
                    param.Param.Add(hour);
                    param.Param.Add(min);
                }
                else
                {
                    errCode.Value = ErrorCodes.Error_FubenNotInOpenTime;
                    yield break;
                }

                if (tbFuben.AssistType == (int) eDungeonAssistType.WorldBoss)
                {
                    //询问活动服务器，是否可以进入
                    var msg = LogicServer.Instance.ActivityAgent.SSApplyActivityState(_this.mGuid,
                        (int) eActivity.WorldBoss, _this.serverId);
                    yield return msg.SendAndWaitUntilDone(co);

                    if (msg.State != MessageState.Reply)
                    {
                        Logger.Error("IsActivityCanEnter() return with state = {0}", msg.State);
                        errCode.Value = ErrorCodes.Unknow;
                        yield break;
                    }
                    if (msg.ErrorCode != (int) ErrorCodes.OK)
                    {
                        Logger.Error("IsActivityCanEnter() return with ErrorCode = {0}", msg.ErrorCode);
                        errCode.Value = (ErrorCodes) msg.ErrorCode;
                        yield break;
                    }
                    if (msg.Response != (int)eActivityState.Start && msg.Response != (int)eActivityState.WillStart)
                    {
//活动不能进，返回错误码
                        errCode.Value = ErrorCodes.Error_ActivityOver;
                        yield break;
                    }
                }
            }
            //检查灭世活动是否可进
            if (tbFuben.AssistType == (int)eDungeonAssistType.MieShiWar)
            {
                var activityId = 0;
                Table.ForeachMieShi(record =>
                {
                    if (record.FuBenID == fubenId)
                    {
                        activityId = record.Id;
                        return false;
                    }
                    return true;
                });
                //询问活动服务器，是否可以进入
                var msg = LogicServer.Instance.ActivityAgent.SSApplyMieShiCanIn(_this.mGuid,
                    _this.serverId, activityId);
                yield return msg.SendAndWaitUntilDone(co);

                if (msg.State != MessageState.Reply)
                {
                    Logger.Error("SSApplyMieShiCanIn() return with state = {0}", msg.State);
                    errCode.Value = ErrorCodes.Unknow;
                    yield break;
                }
                if (msg.ErrorCode != (int)ErrorCodes.OK)
                {
                    Logger.Error("SSApplyMieShiCanIn() return with ErrorCode = {0}", msg.ErrorCode);
                    errCode.Value = (ErrorCodes)msg.ErrorCode;
                    yield break;
                }
            }

            //检查物品够不够
            for (int i = 0, imax = tbFuben.NeedItemId.Length; i != imax; ++i)
            {
                if (tbFuben.NeedItemId[0] >= 0)
                {
                    if (_this.mBag.GetItemCount(tbFuben.NeedItemId[0]) < tbFuben.NeedItemCount[0])
                    {
                        errCode.Value = ErrorCodes.ItemNotEnough;
                        yield break;
                    }
                }
            }

            //消耗
            var j = 0;
            for (var jmax = tbFuben.NeedItemId.Length; j != jmax; ++j)
            {
                var itemId = tbFuben.NeedItemId[j];
                var itemCount = tbFuben.NeedItemCount[j];
                if (itemId == -1)
                {
                    break;
                }
                _this.mBag.DeleteItem(itemId, itemCount, eDeleteItemType.EnterFuben);
            }
            if (j > 0)
            {
                SetExData(_this, (int) eExdataDefine.e421, fubenId);
            }

            //增加副本次数
            if (tbFuben.FubenCountNode == (int) eDungeonSettlementNode.Start)
            {
                AddFubenCount(_this, tbFuben);
            }

            tbScene = Table.GetScene(tbFuben.SceneId);
            var serverId = tbScene.CanCrossServer == 1 ? -1 : _this.serverId;
            CoroutineFactory.NewCoroutine(AskEnterDungeon, _this, serverId, tbFuben.SceneId, (ulong) 0, param)
                .MoveNext();

            errCode.Value = ErrorCodes.OK;
        }

        public bool CheckDungeonReward(CharacterController _this, FubenRecord tbFuben)
        {
            //检查是否有上次未领取的奖励，血色城堡和恶魔广场
            if (tbFuben.AssistType >= 4 && tbFuben.AssistType <= 5)
            {
                var index = tbFuben.AssistType - 4;
                var exDataIdx = (int) eExdataDefine.e408 + index;
                var data = (uint) GetExData(_this, exDataIdx);
                if (data != 0)
                {
                    var type = (data >> ActivityDungeonConstants.CompleteTypeStartIdx) &
                               ActivityDungeonConstants.CompleteTypeMask;
                    var useSec = (data >> ActivityDungeonConstants.DungeonTimeStartIdx) &
                                 ActivityDungeonConstants.DungeonTimeMask;
                    var level = (data >> ActivityDungeonConstants.PlayerLevelStartIdx) &
                                ActivityDungeonConstants.PlayerLevelMask;

                    var result = new FubenResult();
                    result.CompleteType = (int) type;
                    result.UseSeconds = (int) useSec;
                    result.Args.Add((int) level);

                    _this.Proxy.DungeonComplete(result);
                    return false;
                }
            }
            return true;
        }

        public int GetMieShiFailTitleFlag(CharacterController _this)
        {
            var failFlag = -1;
            var titleId = Table.GetServerConfig(1305).ToInt();
            if (titleId > 0)
            {
                var tbNameTitle = Table.GetNameTitle(titleId);
                if (tbNameTitle != null && tbNameTitle.FlagId >= 0)
                {
                    failFlag = tbNameTitle.FlagId;
                }
            }
            return failFlag;
        }

        public int GetMieShiSuccessTitleFlag(CharacterController _this)
        {
            var failFlag = -1;
            var titleId = Table.GetServerConfig(1307).ToInt();
            if (titleId > 0)
            {
                var tbNameTitle = Table.GetNameTitle(titleId);
                if (tbNameTitle != null && tbNameTitle.FlagId >= 0)
                {
                    failFlag = tbNameTitle.FlagId;
                }
            }
            return failFlag;
        }


        public void FubenResultSaveData(CharacterController _this, int fubenId, int seconds,eDungeonCompleteType comType = eDungeonCompleteType.Success)
        {
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                Logger.Error("In CompleteFuben, tbFuben == null! fuben id = {0}", fubenId);
                return;
            }
            var assType = (eDungeonAssistType)tbFuben.AssistType;
            if (assType == eDungeonAssistType.Team)
            {
                if (comType == eDungeonCompleteType.Success)
                {
                    CompleteFubenSaveData(_this, fubenId, seconds);
                }
                else
                {
                    if (tbFuben.FubenCountNode == (int)eDungeonSettlementNode.End)
                    {
                        SaveFubenTime(_this, tbFuben, seconds);                        
                    }                                         
                }                
            }
            else
            {
                CompleteFubenSaveData(_this, fubenId, seconds);
            }           
        }

        //完成副本
        public void CompleteFuben(CharacterController _this, FubenResult result, bool isByMail = false)
        {
            var fubenId = result.FubenId;
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                Logger.Error("In CompleteFuben, tbFuben == null! fuben id = {0}", fubenId);
                return;
            }
            string strName = tbFuben.Name;
            result.EraId = -1;
            if (result.CompleteType == (int)eDungeonCompleteType.Success)
            {
                var mayaBase = _this.mTask.GetEraByFubenId(_this, fubenId);
                if (mayaBase != null && !_this.GetFlag(mayaBase.FinishFlagId))
                {
                    result.EraId = mayaBase.Id;
                }
            }

            var seconds = Math.Max(0, result.UseSeconds);
            var completeType = (eDungeonCompleteType) result.CompleteType;
            var args = result.Args;
            var Rank = 0;//贡献排名
            FubenResultSaveData(_this, fubenId, seconds, completeType);            
            //清除副本未完成标记位
            _this.SetExData((int) eExdataDefine.e421, 0);

            var mailId = 53;
            var items = new Dictionary<int, int>();
            var assistType = (eDungeonAssistType) tbFuben.AssistType;
            var sMailInfo = tbFuben.Name;
            switch (assistType)
            {
                case eDungeonAssistType.Story: //剧情副本
                {
                    mailId = 55;

                    //副本奖励
                    for (int i = 0, imax = tbFuben.RewardId.Length; i < imax; ++i)
                    {
                        var itemId = tbFuben.RewardId[i];
                        var itemCount = tbFuben.RewardCount[i];
                        if (itemId == -1)
                        {
                            break;
                        }
                        items.modifyValue(itemId, itemCount);
                    }

                    #region 抽奖

                    if (result.Draw == null)
                    {
                        result.Draw = new DrawResult();
                    }
                    var drawResult = result.Draw;
                    drawResult.DrawId = tbFuben.DrawReward;
                    ItemBase tempItemBase;
                    drawResult.SelectIndex = _this.PushDraw(tbFuben.DrawReward, out tempItemBase, !isByMail, fubenId);
                    if (tempItemBase == null)
                    {
                        drawResult.DrawItem = new ItemBaseData
                        {
                            ItemId = -1
                        };
                    }
                    else
                    {
                        drawResult.DrawItem = tempItemBase.mDbData;
                        if (isByMail)
                        {
                            items.modifyValue(tempItemBase.GetId(), tempItemBase.GetCount());
                        }
                    }

                    #endregion
                }
                    break;
                case eDungeonAssistType.Team: //组队副本
                {
                    mailId = 55;

                    if (result.Draw == null)
                    {
                        result.Draw = new DrawResult();
                    }
                    var drawResult = result.Draw;

                    if (completeType == eDungeonCompleteType.Success)
                    {
                        //是否动态奖励
                        var isDynamicReward = tbFuben.IsDyncReward == 1;
                        if (isDynamicReward)
                        {
                            for (int i = 0, imax = tbFuben.RewardId.Length; i < imax; ++i)
                            {
                                var itemId = tbFuben.RewardId[i];
                                var skillUpgradeId = tbFuben.RewardCount[i];
                                if (itemId == -1)
                                {
                                    break;
                                }
                                var todayCount = _this.GetExData(tbFuben.TodayCountExdata);
                                var tbSkillUp = Table.GetSkillUpgrading(skillUpgradeId);
                                if (tbSkillUp == null)
                                {
                                    continue;
                                }
                                var itemCount = tbSkillUp.GetSkillUpgradingValue(todayCount);
                                items.modifyValue(itemId, itemCount);
                                var item = new ItemBaseData();
                                item.ItemId = itemId;
                                item.Count = itemCount;
                                drawResult.Items.Add(item);
                            }
                        }
                        else
                        {
                            for (int i = 0, imax = tbFuben.RewardId.Length; i < imax; ++i)
                            {
                                var itemId = tbFuben.RewardId[i];
                                var itemCount = tbFuben.RewardCount[i];
                                if (itemId == -1)
                                {
                                    break;
                                }
                                items.modifyValue(itemId, itemCount);
                                var item = new ItemBaseData();
                                item.ItemId = itemId;
                                item.Count = itemCount;
                                drawResult.Items.Add(item);
                            }
                        } 
                    }
                }
                    break;
                case eDungeonAssistType.DevilSquare: //恶魔广场
                case eDungeonAssistType.BloodCastle: //血色城堡
                {
                    var myLevel = _this.GetLevel();
                    args.Add(myLevel);
                    var index = tbFuben.AssistType - 4;
                    var exDataIdx = (int) eExdataDefine.e408 + index;
                    var data = _this.GetExData(exDataIdx);
                    if (data != 0)
                    {
                        Logger.Error("The last reward was not received for fuben {0}!!!!", fubenId);
                    }
                    else if (completeType == eDungeonCompleteType.Quit)
                    {
                        //中途退出的情况下，自动替他加1倍奖励
                        _this.SendQuitReward(tbFuben, true);
                        return;
                    }
                    else
                    {
                        data =
                            (int)
                                (((uint) seconds << ActivityDungeonConstants.DungeonTimeStartIdx) |
                                 ((uint) completeType << ActivityDungeonConstants.CompleteTypeStartIdx) |
                                 ((uint) myLevel << ActivityDungeonConstants.PlayerLevelStartIdx));
                        _this.SetExData(exDataIdx, data);
                    }
                }
                    break;
                case eDungeonAssistType.WorldBoss: //世界boss
                {
                    isByMail = true;
                    if (result.Draw == null)
                    {
                        result.Draw = new DrawResult();
                    }
                    var drawResult = result.Draw;
                    Rank = args[0];
                    Table.ForeachWorldBOSSAward(record =>
                    {
                        if (Rank >= record.MinRanking && Rank <= record.MaxRanking)
                        {
                            var tbMail = Table.GetMail(record.PostIndex);
                            var itemId = tbMail.ItemId;
                            var itemCount = tbMail.ItemCount;
                            for (int i = 0, imax = itemId.Length; i < imax; i++)
                            {
                                if (itemId[i] == -1)
                                {
                                    break;
                                }
                                var item = new ItemBaseData();
                                item.ItemId = itemId[i];
                                item.Count = itemCount[i];
                                drawResult.Items.Add(item);
                                items.modifyValue(item.ItemId, item.Count);
                            }
                            mailId = tbMail.Id;
                            return false;
                        }
                        return true;
                    });
                }
                    break;
                case eDungeonAssistType.FrozenThrone: //冰封王座
                {
                    Rank = args[0];
                    for (int i = 0, imax = tbFuben.RewardId.Length; i < imax; ++i)
                    {
                        var itemId = tbFuben.RewardId[i];
                        if (itemId == -1)
                        {
                            break;
                        }
                        var itemCount = Utils.GetRewardCount(tbFuben, tbFuben.RewardCount[i], Rank, _this.GetLevel());
                        items.modifyValue(itemId, itemCount);
                    }
                }
                    break;
                case eDungeonAssistType.MieShiWar: //灭世之战
                {
                    mailId = 505; 
                    var failFlag = GetMieShiFailTitleFlag(_this);
                    var successFlag = GetMieShiSuccessTitleFlag(_this);
                    if (result.CompleteType == (int)eDungeonCompleteType.Failed)
                     // 失败title
                        _this.SetTitleFlag(failFlag, true, 0, DateTime.Now);           
                    else
                        _this.SetTitleFlag(failFlag, false, 0, DateTime.Now);

                    if (result.CompleteType == (int)eDungeonCompleteType.Success)
                        //成功title   
                        _this.SetTitleFlag(successFlag, true, 0, DateTime.Now);
                    else
                        _this.SetTitleFlag(successFlag, false, 0, DateTime.Now);

                    isByMail = true;
                    var rank = args[0];
                    if (result.CompleteType == (int) eDungeonCompleteType.Failed)
                    {
                        MailRecord tbMail = Table.GetMail(505);
                        if (tbMail != null)
                        {
                            var itemId = tbMail.ItemId;
                            var itemCount = tbMail.ItemCount;
                            for (int i = 0, imax = itemId.Length; i < imax; i++)
                            {
                                if (itemId[i] == -1)
                                {
                                    break;
                                }
                                var item = new ItemBaseData();
                                item.ItemId = itemId[i];
                                item.Count = itemCount[i];
                                items.modifyValue(item.ItemId, item.Count);
                            }
                        }
                    }
                    else
                    {
                        Table.ForeachDefendCityReward(record =>
                        {
                            if (rank >= record.Rank[0] && rank <= record.Rank[1] && result.ActivityId == record.ActivityId)
                            {
                                Rank = rank;
                                strName = rank.ToString();
                                MailRecord tbMail = null;
                                if (args[3] == 6)
                                    tbMail = Table.GetMail(record.MailId6);
                                else if (args[3] == 5)
                                    tbMail = Table.GetMail(record.MailId5);
                                else if (args[3] == 4)
                                    tbMail = Table.GetMail(record.MailId4);
                                else if (args[3] == 3)
                                    tbMail = Table.GetMail(record.MailId3);
                                else if (args[3] == 2)
                                    tbMail = Table.GetMail(record.MailId2);
                                else
                                    tbMail = Table.GetMail(record.MailId);
                                mailId = tbMail.Id;
                                var itemId = tbMail.ItemId;
                                var itemCount = tbMail.ItemCount;
                                for (int i = 0, imax = itemId.Length; i < imax; i++)
                                {
                                    if (itemId[i] == -1)
                                    {
                                        break;
                                    }
                                    var item = new ItemBaseData();
                                    item.ItemId = itemId[i];
                                    item.Count = itemCount[i];
                                    items.modifyValue(item.ItemId, item.Count);
                                }
                                if (record.ActivateTitle > -1 && result.CompleteType == (int)eDungeonCompleteType.Success)
                                {
                                    var tbNameTitle = Table.GetNameTitle(record.ActivateTitle);
                                    if (tbNameTitle != null && tbNameTitle.FlagId >= 0)
                                        _this.SetTitleFlag(tbNameTitle.FlagId, true, 0, DateTime.Now);
                                }
                                return false;
                            }
                            return true;
                        });
                    }
                }
                    break;
                case eDungeonAssistType.CityExpMulty: //家园多人经验活动
                case eDungeonAssistType.CityExpSingle: //家园单人经验
                {
                    if (assistType == eDungeonAssistType.CityExpMulty)
                    {
                        //每人每天只能获得一次通关奖励，拿过奖励的再次挑战副本不再弹出奖励界面
                        var todayCount = _this.GetExData(tbFuben.TodayCountExdata);
                        if (todayCount > 1)
                        {
                            if(tbFuben.Id == 1050 || tbFuben.Id == 1019)
                                break;
                            else
                                return;
                        }
                    }

                    var strs = result.Strs;
                    Rank = args[0];
                    var myLevel = _this.GetLevel();
                    for (int i = 0, imax = tbFuben.RewardId.Length; i < imax; ++i)
                    {
                        var itemId = tbFuben.RewardId[i];
                        if (itemId == -1)
                        {
                            break;
                        }
                        var itemCount = Utils.GetRewardCount(tbFuben, tbFuben.RewardCount[i], Rank, myLevel);
                        items.modifyValue(itemId, itemCount);
                    }

                    if (assistType == eDungeonAssistType.CityExpMulty)
                    {
                        var iAmLeader = strs[0] == _this.GetName();
                        if (iAmLeader)
                        {
                            var count = tbFuben.ScanReward[0];
                            if (count > 0)
                            {
                                count = Utils.GetRewardCount(tbFuben, count, Rank, myLevel);
                                items.modifyValue((int) eResourcesType.AchievementScore, count);
                            }

                            count = tbFuben.ScanReward[1];
                            if (count > 0)
                            {
                                count = Utils.GetRewardCount(tbFuben, count, Rank, myLevel);
                                items.modifyValue((int)eResourcesType.CityWood, count);
                            }
                        }
                    }
                }
                    break;
                case eDungeonAssistType.CityGoldSingle: //家园单人金币
                {
                    if (args.Count < 11)
                    {
                        break;
                    }
                    //每击杀一个“机灵鬼哥布林”，增加20%额外奖励
                    var rewardScale = 1f + 0.2f*args[10];
                    for (int i = 0, imax = tbFuben.RewardId.Length; i < imax; ++i)
                    {
                        var itemId = tbFuben.RewardId[i];
                        if (itemId == -1)
                        {
                            break;
                        }
                        var itemCount = (int) (tbFuben.RewardCount[i]*rewardScale + 0.5f);
                        items.modifyValue(itemId, itemCount);
                    }
                }
                    break;
                case eDungeonAssistType.CastleCraft1:
                case eDungeonAssistType.CastleCraft2:
                case eDungeonAssistType.CastleCraft3:
                case eDungeonAssistType.CastleCraft4:
                case eDungeonAssistType.CastleCraft5:
                case eDungeonAssistType.CastleCraft6:
                {
                    Rank = args[0];
                    var score = args[1];
                    var isDynamicReward = tbFuben.IsDynamicExp == 1;
                    //基础奖励
                    for (int i = 0, imax = tbFuben.RewardId.Length; i < imax; ++i)
                    {
                        var itemId = tbFuben.RewardId[i];
                        if (itemId == -1)
                        {
                            break;
                        }
                        var itemCount = tbFuben.RewardCount[i];
                        items.modifyValue(itemId, itemCount);
                    }

                    //额外经验
                    var exp = 0;
                    if (isDynamicReward)
                    {
                        exp =
                            (int)
                                (1.0*tbFuben.DynamicExpRatio*Table.GetLevelData(_this.GetLevel()).DynamicExp/10000*score);
                    }
                    if (exp > 0)
                    {
                        items.modifyValue((int) eResourcesType.ExpRes, exp);
                    }

                    //额外荣誉
                    var honor = tbFuben.ScanReward[0];
                    if (isDynamicReward)
                    {
                        honor = Table.GetSkillUpgrading(honor).GetSkillUpgradingValue(Rank);
                    }
                    if (honor > 0)
                    {
                        items.modifyValue((int) eResourcesType.Honor, honor);
                    }
                }
                    break;
                case eDungeonAssistType.AllianceWar:
                {
                    //副本奖励
                    for (int i = 0, imax = tbFuben.RewardId.Length; i < imax; ++i)
                    {
                        var itemId = tbFuben.RewardId[i];
                        var itemCount = tbFuben.RewardCount[i];
                        if (itemId == -1)
                        {
                            break;
                        }
                        items.modifyValue(itemId, itemCount);
                    }
                }
                    break;
                case eDungeonAssistType.Chiji:
                {
                    var ChijiRank = args[0];
                        var tbReward = Table.GetCheckenReward(ChijiRank);
                        if (result.Draw == null)
                        {
                            result.Draw = new DrawResult();
                        }
                        if (tbReward != null)
                        {
                            for (int i = 0; i < tbReward.RankItemID.Length && i < tbReward.RankItemCount.Length; i++)
                            {
                                if (tbReward.RankItemID[i] <= 0 || tbReward.RankItemCount[i] <= 0)
                                    continue;
                                items.modifyValue(tbReward.RankItemID[i], tbReward.RankItemCount[i]);
                                var item = new ItemBaseData();
                                item.ItemId = tbReward.RankItemID[i];
                                item.Count = tbReward.RankItemCount[i];
                                if (result.Draw != null)
                                    result.Draw.Items.Add(item);
                            }
                        }
                    }
                    break;
                case eDungeonAssistType.ClimbingTower:
                {
                    AddExData(_this, (int)eExdataDefine.e799,1);
                    int cur = GetExData(_this, (int)eExdataDefine.e623) + 1;
                    var tbTower = Table.GetClimbingTower(cur);
                    if (tbTower != null)
                    {
                        if (result.Draw == null)
                        {
                            result.Draw = new DrawResult();
                        }
                         var drawResult = result.Draw;
  
                        for (int i = 0; i < tbTower.RewardList.Count && i < tbTower.NumList.Count; i++)
                        {
                            var itemId = tbTower.RewardList[i];
                            var itemCount = tbTower.NumList[i];
                            items.modifyValue(itemId, itemCount);
                            {
                                var item = new ItemBaseData();
                                item.ItemId = itemId;
                                item.Count = itemCount;
                                if (drawResult != null)
                                {
                                    drawResult.Items.Add(item);
                                }
                            }
                        }
                        int ifirst = 0;
                        if (cur > GetExData(_this, (int)eExdataDefine.e621))
                        {
                            ifirst = 1;
                            SetExData(_this, (int)eExdataDefine.e621,cur);
                            for (int i = 0; i < tbTower.OnceRewardList.Count && i < tbTower.OnceNumList.Count; i++)
                            {
                                var itemId = tbTower.OnceRewardList[i];
                                var itemCount = tbTower.OnceNumList[i];
                                items.modifyValue(itemId, itemCount);
                            }
                        }
                        args.Add(ifirst);
                        SetExData(_this, (int)eExdataDefine.e623, cur);
                    }
                    
                }
                break;
                default:
                    break;
            }

            if (items.Count > 0)
            {
                if (isByMail)
                {
                    _this.mBag.AddItemByMail(mailId, items, null, eCreateItemType.Fuben, strName);
                }
                else
                {
                    _this.mBag.AddItemOrMail(mailId, items, null, eCreateItemType.Fuben, strName);
                }
            }

            if (_this.Proxy != null)
            {
                _this.Proxy.DungeonComplete(result);
            }
        }

        //完成副本
        public void CompleteFubenSaveData(CharacterController _this, int fubenId, int seconds)
        {
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                Logger.Warn("CompleteFubenSaveData not find,fubenId={0}", fubenId);
                return;
            }

            if (tbFuben.FubenCountNode == (int) eDungeonSettlementNode.End)
            {
                AddFubenCount(_this, tbFuben);
            }
            SaveFubenTime(_this, tbFuben, seconds);
        }

        //增加副本次数
        public void AddFubenCount(CharacterController _this, FubenRecord tbFuben)
        {
            switch (tbFuben.Difficulty)
            {
                case 0:
                {
                    AddExData(_this, (int) eExdataDefine.e21, 1);
                    AddExData(_this, (int) eExdataDefine.e53, 1);
                }
                    break;
                case 1:
                {
                    AddExData(_this, (int) eExdataDefine.e22, 1);
                    AddExData(_this, (int) eExdataDefine.e54, 1);
                }
                    break;
                case 2:
                {
                    AddExData(_this, (int) eExdataDefine.e23, 1);
                    AddExData(_this, (int) eExdataDefine.e55, 1);
                }
                    break;
            }

            var type = (eDungeonAssistType) tbFuben.AssistType;
            switch (type)
            {
                case eDungeonAssistType.Story:
                    AddExData(_this, tbFuben.ScriptId, 1);
                    break;
                case eDungeonAssistType.Team:
                    AddExData(_this, tbFuben.ScriptId, 1);
                    AddExData(_this, (int) eExdataDefine.e36, 1);
                    break;
            }
            AddExData(_this, tbFuben.TodayCountExdata, 1);
            AddExData(_this, tbFuben.TotleExdata, 1);
            AddExData(_this, (int) eExdataDefine.e42, 1);
            AddExData(_this, (int) eExdataDefine.e56, 1);

            var e = new TollgateFinish(_this, tbFuben.Id);
            EventDispatcher.Instance.DispatchEvent(e);
        }

        //记录副本时间
        private void SaveFubenTime(CharacterController _this, FubenRecord tbFuben, int seconds)
        {
            if (tbFuben.TimeExdata > 0 && seconds > 0)
            {
                var oldseconds = GetExData(_this, tbFuben.TimeExdata);
                if (oldseconds == 0 || seconds < oldseconds)
                {
                    SetExData(_this, tbFuben.TimeExdata, seconds);
                }
            }
        }

        //战场结果
        public void BattleResult(CharacterController _this, int fubenId, int type)
        {
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                return;
            }
            var todayCount = GetExData(_this, tbFuben.TodayCountExdata);
            //tbFuben.ResetExdata用作记录今日胜利次数
            var todayWinCount = GetExData(_this, tbFuben.ResetExdata);
            var rewards = new Dictionary<int, int>();
            GetBattleReward(tbFuben, todayCount, todayWinCount, type, rewards);

            //特殊时段奖励
            var now = DateTime.Now;
            var begin = Table.GetServerConfig(282).ToInt();
            var end = Table.GetServerConfig(283).ToInt();
            var beginTime = new DateTime(now.Year, now.Month, now.Day, begin/100, begin%100, 0, DateTimeKind.Local);
            var endTime = new DateTime(now.Year, now.Month, now.Day, end/100, end%100, 0, DateTimeKind.Local);
            if (now >= beginTime && now <= endTime)
            {
//在特殊时段内
                //tbFuben.TimeExdata用作记录今日额外奖励次数
                var extraCount = GetExData(_this, tbFuben.TimeExdata);
                AddExData(_this, tbFuben.TimeExdata, 1);
                GetBattleReward(tbFuben, extraCount, extraCount, 1, rewards);
            }

            //发家园经验
//             var count = GetExData(_this, (int) eExdataDefine.e530);
//             if (count <= StaticParam.BFCExpMaxCount)
//             {
//                 var exp = type == 1 ? StaticParam.BFCSuccessExp : StaticParam.BFCFailExp;
//                 rewards.modifyValue((int) eResourcesType.HomeExp, exp);
//             }

            //加奖励
            foreach (var reward in rewards)
            {
                _this.mBag.AddItem(reward.Key, reward.Value, eCreateItemType.Battle);
            }

            //增加次数
            AddExData(_this, tbFuben.TotleExdata, 1);
            AddExData(_this, tbFuben.TodayCountExdata, 1);
            var isFirst = 0;
            if (type == 1)
            {
                AddExData(_this, tbFuben.ResetExdata, 1);
                var isFirstFlag = tbFuben.ScriptId;
                if (!GetFlag(_this, isFirstFlag))
                {
                    SetFlag(_this, isFirstFlag);
                    isFirst = 1;
                }
            }

            if (_this.Proxy != null)
            {
                _this.Proxy.BattleResult(fubenId, type, isFirst);
            }
        }

        private void GetBattleReward(FubenRecord tbFuben, int count0, int count1, int type, Dictionary<int, int> rewards)
        {
            var isDynamicReward = tbFuben.IsDyncReward == 1;
            if (tbFuben.ScanExp > 0)
            {
                var value = tbFuben.ScanExp;
                if (isDynamicReward)
                {
                    value = Table.GetSkillUpgrading(value).GetSkillUpgradingValue(count0);
                }
                rewards.modifyValue((int) eResourcesType.ExpRes, value);
            }
            if (tbFuben.ScanGold > 0)
            {
                var value = tbFuben.ScanGold;
                if (isDynamicReward)
                {
                    value = Table.GetSkillUpgrading(value).GetSkillUpgradingValue(count0);
                }
                rewards.modifyValue((int) eResourcesType.GoldRes, value);
            }
            if (type == 1) //胜利
            {
                var value = tbFuben.ScanReward[0];
                if (isDynamicReward)
                {
                    value = Table.GetSkillUpgrading(value).GetSkillUpgradingValue(count1);
                }
                rewards.modifyValue((int) eResourcesType.Honor, value);
            }
            else //失败
            {
                var value = tbFuben.ScanReward[1];
                if (isDynamicReward)
                {
                    value = Table.GetSkillUpgrading(value).GetSkillUpgradingValue(count0);
                }
                rewards.modifyValue((int) eResourcesType.Honor, value);
            }
        }

        #endregion

        #region 灭世活动相关

        public ErrorCodes ApplyActivityData(CharacterController _this, int serverId, CommonActivityData responeData)
        {

            return ErrorCodes.OK;
        }
        #endregion

        #region PvP

        //升级军衔
        public ErrorCodes UpgradeHonor(CharacterController _this, int honorId)
        {
            var oldValue = GetExData(_this, 250);
            if (oldValue == -1)
            {
                oldValue = 0;
            }
            if (honorId != oldValue)
            {
                return ErrorCodes.Unknow;
            }
            var tbHonor = Table.GetHonor(oldValue);
            if (tbHonor == null)
            {
                return ErrorCodes.Error_HonorID;
            }
            var oldHonor = _this.mBag.GetRes(eResourcesType.Honor);
            if (tbHonor.NextRank == -1)
            {
                return ErrorCodes.Error_HonorMax;
            }
            if (oldHonor < tbHonor.NeedHonor)
            {
                return ErrorCodes.Error_ResNoEnough;
            }
            _this.mBag.DelRes(eResourcesType.Honor, tbHonor.NeedHonor, eDeleteItemType.UpgradeHonor);
            SetExData(_this, (int) eExdataDefine.e250, tbHonor.NextRank);
            //BooksChange(_this);
            //激活称号
            RefreshHonorTitle(_this, tbHonor.NextRank);

            return ErrorCodes.OK;
        }

        private void RefreshHonorTitle(CharacterController _this, int rank)
        {
            var titles = new List<int>();
            var states = new List<bool>();
            for (var i = 0; i < rank; i++)
            {
                titles.Add(i + 3001);
                states.Add(i == rank - 1);
            }
            ModityTitles(_this, titles, states);
        }

        public void GetP1vP1OldList(CharacterController _this, List<P1vP1Change_One> list)
        {
            if (_this.mDbData.P1vP1Change == null)
            {
                return;
            }
            foreach (var one in _this.mDbData.P1vP1Change.Data)
            {
                list.Add(one);
            }
        }

        public P1vP1Change_One PushP1vP1Change(CharacterController _this,
                                               int type,
                                               string pvpName,
                                               int oldRank,
                                               int newRank)
        {
            var one = new P1vP1Change_One
            {
                Type = type,
                Name = pvpName,
                OldRank = oldRank,
                NewRank = newRank
            };
            if (_this.mDbData.P1vP1Change == null)
            {
                _this.mDbData.P1vP1Change = new P1vP1ChangeList();
            }
            _this.mDbData.P1vP1Change.Data.Add(one);
            if (_this.mDbData.P1vP1Change.Data.Count > _this.P1vP1ChangeCountMax)
            {
                _this.mDbData.P1vP1Change.Data.RemoveAt(0);
            }
            return one;
        }

        #endregion

        #region Buff相关

        //上buff
        public void AddBuff(CharacterController _this, int buffId, int bufflevel)
        {
            CoroutineFactory.NewCoroutine(AddBuffCoroutine, _this.mGuid, buffId, bufflevel).MoveNext();
        }


        private IEnumerator AddBuffCoroutine(Coroutine co, ulong guid, int buffId, int bufflevel)
        {
            var msg = LogicServer.Instance.SceneAgent.SSAddBuff(guid, guid, buffId, bufflevel);
            yield return msg.SendAndWaitUntilDone(co);
        }

        #endregion

        #region 初始化

        public bool InitByDb(CharacterController _this, ulong characterId, DBCharacterLogic dbData)
        {
            PlayerLog.WriteLog(characterId, "----------Logic--------------------InitByDb--------------------{0}",
                dbData.SaveCount);
            _this.mDbData = dbData;

			//初始化标记为
	        try
	        {
				_this.lFlag.InitByDB(_this, _this.mDbData);
	        }
	        catch (Exception e)
	        {
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
	        }
	        finally
	        {
				_this.AddChild(_this.lFlag);    
	        }


			//初始化扩展数据
	        try
	        {
				_this.lExdata.InitByDB(_this, _this.mDbData);
	        }
	        catch (Exception e)
	        {
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
	        }
			finally
			{
				_this.AddChild(_this.lExdata);

			}

			//初始化包裹数据
	        try
	        {
		        _this.mBag.InitByDB(_this, _this.mDbData.Bag);
	        }
	        catch (Exception e)
	        {
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
	        }
	        finally
	        {
				_this.AddChild(_this.mBag);    
	        }
            
			//初始化技能数据
			try
			{
				_this.mSkill.InitByDB(_this, _this.mDbData.Skill);
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{
				_this.AddChild(_this.mSkill);
			}

			//初始化天赋
			try
			{
				_this.mTalent.InitByDB(_this, _this.mDbData.Innate);
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{
				_this.AddChild(_this.mTalent);
			}

			//初始化扩展数据
			try
			{
				_this.lExdata64.InitByDB(_this, _this.mDbData);
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{
				_this.AddChild(_this.lExdata64);
			}


            //初始化战盟数据
            try
            {
                if (_this.mDbData.Alliance == null)
                {
                    _this.mDbData.Alliance = _this.mAlliance.InitByBase(_this);
                }
                else
                {
                    _this.mAlliance.InitByDB(_this, _this.mDbData.Alliance);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
                Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
            }
            finally
            {
                _this.AddChild(_this.mAlliance);
            }

            //初始化图鉴
            try
            {
                _this.mBook.InitByDB(_this, _this.mDbData.Book);

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
                Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
            }
            finally
            {
                _this.AddChild(_this.mBook);
            }

			//初始化任务
			try
			{
				_this.mTask.InitByDB(_this, _this.mDbData.Mission);
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{
				_this.AddChild(_this.mTask);
			}

            //初始化坐骑
            try
            {
                _this.mMount.InitByDB(_this, _this.mDbData.MountData);

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
                Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
            }
            finally
            {
                _this.AddChild(_this.mMount);
            }

			//初始化好友信息
			try
			{
	            _this.mFriend.InitByDB(_this, _this.mDbData.Friends);
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{
				_this.AddChild(_this.mFriend);
			}

			//初始化商店数据
			try
			{
				_this.mStone.InitByDB(_this, _this.mDbData.Store);
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{
				_this.AddChild(_this.mStone);
			}

			//初始化邮件数据
			try
			{
				if (_this.mDbData.Mail == null)
				{
					_this.mDbData.Mail = _this.mMail.InitByBase(_this);
				}
				else
				{
					_this.mMail.InitByDB(_this, _this.mDbData.Mail);
				}
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{
				_this.AddChild(_this.mMail);
			}

			try
			{
				if (_this.mDbData.Exchange == null)
				{
					_this.mDbData.Exchange = _this.mExchange.InitByBase(_this);
				}
				else
				{
					_this.mExchange.InitByDB(_this, _this.mDbData.Exchange);
				}
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{
				_this.AddChild(_this.mExchange);

			}

			//初始化运营活动数据
			try
			{
				if (_this.mDbData.OperActivity == null)
				{
					_this.mDbData.OperActivity = _this.mOperActivity.InitByBase(_this);
				}
				else
				{
					_this.mOperActivity.InitByDB(_this, _this.mDbData.OperActivity);
				}
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{
				_this.AddChild(_this.mOperActivity);

			}

			//初始化家园数据
			try
			{
				if (_this.mDbData.City == null)
				{
					_this.mDbData.City = _this.mCity.InitByBase(_this);
				}
				else
				{
					_this.mCity.InitByDB(_this, _this.mDbData.City);
				}
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{
				_this.AddChild(_this.mCity);
			}

			//初始化随从数据
			try
			{
				if (_this.mDbData.PetMission == null)
				{
					_this.mDbData.PetMission = _this.mPetMission.InitByBase(_this);
				}
				else
				{
					_this.mPetMission.InitByDB(_this, _this.mDbData.PetMission);
				}
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{
				_this.AddChild(_this.mPetMission);

			}

			try
			{
				var titles = _this.mDbData.ViewTitles;
				for (var i = titles.Count; i < StaticParam.TitlesMaxCount; i++)
				{
					titles.Add(-1);
				}
				PlayerLog.DataLog(_this.mGuid, "lo");
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{

			}

			//重新计算vip level
			try
			{
				            //重新计算vip level
            var vipLevel = _this.mBag.GetRes(eResourcesType.VipLevel);
            var vipExp = _this.mBag.GetRes(eResourcesType.VipExpRes);
            var vipLevelNew = _this.mBag.CaculateVipLevel(vipExp, 0);
            if (vipLevelNew != vipLevel)
            {
                _this.mBag.SetRes(eResourcesType.VipLevel, vipLevelNew);
            }
			}
			catch (Exception e)
			{
				Logger.Fatal(e.Message + "----" + e.StackTrace);
#if DEBUG
				Debug.Assert(false, e.Message + "----" + e.StackTrace);
#endif
			}
			finally
			{

			}

            // 初始化活动数据


            return true;
        }

        public DBCharacterLogic InitByBase(CharacterController _this, ulong characterId, object[] args)
        {
            PlayerLog.WriteLog(characterId, "----------InitByBase----------");
            _this.mDbData = new DBCharacterLogic();
            _this.OnlineTime = DateTime.Now;
            _this.mDbData.Id = characterId;
            var roleId = (int) args[0];
	        var serverId = 0;
	        if (args.Length > 1)
	        {
				serverId = (int)args[1];
	        }
			if (args.Length > 2)
			{
				_this.mDbData.IsGM = (bool)args[2];
			}
			
            _this.mDbData.TypeId = roleId;
	        _this.mDbData.ServerId = serverId;
            _this.lFlag.InitByBase(_this, _this.mDbData);
            _this.lExdata.InitByBase(_this, _this.mDbData);
            _this.lExdata64.InitByBase(_this, _this.mDbData);
            _this.mDbData.Bag = _this.mBag.InitByBase(_this);
            _this.mDbData.Book = _this.mBook.InitByBase(_this);      // 因为mission依赖book，把book放到mission前面
            _this.mDbData.Mission = _this.mTask.InitByBase(_this);
            _this.mDbData.Skill = _this.mSkill.InitByBase(_this, roleId);
            _this.mDbData.Innate = _this.mTalent.InitByBase(_this);
            _this.mDbData.MountData = _this.mMount.InitByBase(_this);
            _this.mDbData.Friends = _this.mFriend.InitByBase(_this);
            _this.mDbData.Store = _this.mStone.InitByBase(_this);
            _this.mDbData.Mail = _this.mMail.InitByBase(_this);
            _this.mDbData.City = _this.mCity.InitByBase(_this);
            _this.mDbData.PetMission = _this.mPetMission.InitByBase(_this);
            _this.mDbData.Alliance = _this.mAlliance.InitByBase(_this);
            _this.mDbData.Exchange = _this.mExchange.InitByBase(_this);
			_this.mDbData.OperActivity = _this.mOperActivity.InitByBase(_this);
            InitSettingData(_this);

            try
			{
				_this.AddChild(_this.mBag);
	            _this.AddChild(_this.mTask);
	            _this.AddChild(_this.mSkill);
	            _this.AddChild(_this.mTalent);
	            _this.AddChild(_this.lFlag);
	            _this.AddChild(_this.lExdata);
	            _this.AddChild(_this.mBook);
                _this.AddChild(_this.mMount);
	            _this.AddChild(_this.mFriend);
	            _this.AddChild(_this.mStone);
	            _this.AddChild(_this.mMail);
	            _this.AddChild(_this.mCity);
	            _this.AddChild(_this.mPetMission);
	            _this.AddChild(_this.mAlliance);
	            _this.AddChild(_this.mExchange);
				_this.AddChild(_this.mOperActivity);
			}
			catch (System.Exception ex)
			{
				Logger.Fatal("DBCharacterLogic InitByBase", ex.Message);
			}

	        try
	        {

		        Table.ForeachInitItem(record =>
		        {
			        if (record.ItemId == -1)
			        {
				        return true;
			        }
			        if (record.Type != -1 && record.Type != roleId)
			        {
				        return true;
			        }

			        if (-1 == record.BagId)
			        {
				        _this.mBag.AddItem(record.ItemId, record.ItemCount, eCreateItemType.Init);
				        return true;
			        }

			        var itemTable = Table.GetItemBase(record.ItemId);
			        if (null == itemTable)
			        {
#if DEBUG
				        Debug.Assert(false, "Table.GetItemBase = null", "({0})", record.ItemId);
#endif
				        return true;
			        }
			        if (!BitFlag.GetLow(itemTable.CanInBag, record.BagId))
			        {
#if DEBUG
				        Debug.Assert(false, "!BitFlag.GetLow(point, record.BagId)", "!BitFlag.GetLow({0}, {1})", itemTable.CanInBag,
					        record.BagId);
#endif
				        return true;
			        }
			        _this.mBag.GetBag(record.BagId).ResetItemByItemId(0, record.ItemId);

			        return true;
		        });
		        for (var i = 0; i < StaticParam.TitlesMaxCount; i++)
		        {
			        _this.mDbData.ViewTitles.Add(-1);
		        }
	        }
	        catch (System.Exception e)
	        {
				Logger.Fatal("DBCharacterLogic InitByBase", e.Message);
	        }

            SetFlag(_this, 1001, false);
            return _this.mDbData;
        }

        public void ApplyVolatileData(CharacterController _this, DBCharacterLogicVolatile data)
        {
            foreach (var dbmail in data.NewMails)
            {
                _this.mMail.PushNewMailByDb(dbmail);
            }
            foreach (var p1vp1 in data.NewP1vP1s)
            {
                if (_this.mDbData.P1vP1Change == null)
                {
                    _this.mDbData.P1vP1Change = new P1vP1ChangeList();
                }
                _this.mDbData.P1vP1Change.Data.Add(p1vp1);
            }
            foreach (var i in data.ExdataChange)
            {
                SetExData(_this, i.Key, GetExData(_this, i.Key) + i.Value);
            }
            foreach (var pair in data.ExchangeBuyed)
            {
                var item = _this.mExchange.GetItemByStoreId(pair.Key);
                if (item == null)
                {
                    PlayerLog.WriteLog(_this.mGuid,
                        "Error! ApplyVolatileData ExchangeBuyed storeId = {0}, characterId={1}", pair.Key, pair.Value);
                    continue;
                }
                item.mDbdata.BuyCharacterId = pair.Value.Key;
                item.mDbdata.BuyCharacterName = pair.Value.Value;
                item.State = StoreItemType.Buyed;
            }

            var friendChanges = data.FriendChanges;
            if (friendChanges != null)
            {
                foreach (var friend in friendChanges.Friends)
                {
                    _this.mFriend.SetBehaveData(0, friend.Key, friend.Value);
                }
                foreach (var enemy in friendChanges.Enemys)
                {
                    _this.mFriend.SetBehaveData(1, enemy.Key, enemy.Value);
                }
                foreach (var black in friendChanges.Blacks)
                {
                    _this.mFriend.SetBehaveData(2, black.Key, black.Value);
                }
            }

            //下线后完成的副本
            var results = data.FubenResult;
            foreach (var result in results)
            {
                CompleteFuben(_this, result, true);
            }

            //set flag
            var flagDatas = data.FlagSetValue;
            foreach (var flagData in flagDatas)
            {
                SetFlag(_this, flagData.Key, flagData.Value == 1);
            }

            //set exdata
            var exdatas = data.ExdataSetValue;
            foreach (var exdata in exdatas)
            {
                SetExData(_this, exdata.Key, exdata.Value);
            }

            //recharge data
            var recharges = data.Recharge;
            foreach (var recharge in recharges)
            {
                StaticParam.RLogger.Info("In ApplyVolatileData(). Send recharge item! Character Id = {0}", _this.mGuid);
                try
                {
                    OnRechargeSuccess(_this, recharge.Platform, recharge.Type, recharge.Price);
                }
                catch (Exception ex)
                {
                    PlayerLog.WriteLog((int) LogType.OfflineRecharge,
                        "OnRechargeSuccess error! Platform = {0}, Type = {1}， Price = {2}, ex = {3}", recharge.Platform,
                        recharge.Type, recharge.Price, ex);
                }
            }
            if (data.SellHistory != null)
            {
                if (_this.mDbData.SellHistory == null)
                {
                    _this.mDbData.SellHistory = new SellHistoryList();
                }

                foreach (var item in data.SellHistory.items)
                {
                    if (_this.mDbData.SellHistory.items.Count >= 50)
                    {
                        _this.mDbData.SellHistory.items.RemoveAt(0);
                    }
                    _this.mDbData.SellHistory.items.Add(item);
                }
                data.SellHistory.items.Clear();
            }
        }

        public void LoadFinished(CharacterController _this)
        {
            LogicServer.Instance.ServerControl.TaskManager.ApplyCachedTasks(_this);
        }

        private void InitSettingData(CharacterController _this)
        {
            SetExData(_this, 61, 496);
        }

        #endregion
    }

    public enum FunctionBlock
    {
        EnterFuben = 0,

        Count
    }

    public class CharacterController : NodeBase,
                                       ICharacterControllerBase
                                           <DBCharacterLogic, DBCharacterLogicSimple, DBCharacterLogicVolatile>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //
        public static readonly int MaxMobaiCount = Table.GetServerConfig(245).ToInt();
        public static ICharacterController mImpl;
        public MsgChatMoniterData moniterData { get; set; }
        static CharacterController()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (CharacterController),
                typeof (CharacterControllerDefaultImpl),
                o => { mImpl = (ICharacterController) o; });
        }

        public Dictionary<int, int> syncSceneExDataIdx = new Dictionary<int, int>();
        public Dictionary<int, int> syncSceneFlagIdx = new Dictionary<int, int>();
        public Dictionary<int,int> dicOfflineItems = new Dictionary<int, int>();  
        
        public CharacterController()
        {
            mImpl.InitCharacterController(this);
            {//initdicflagtemp
                var str = Table.GetServerConfig(3005).Value.Trim();
                if (string.IsNullOrEmpty(str))
                {
                    return;
                }
                var Ids = str.Split('|');
                foreach (var id in Ids)
                {
                    syncSceneExDataIdx.Add(int.Parse(id), 0);
                }
            }
            {//syncSceneFlagIdx
                var str = Table.GetServerConfig(3006).Value.Trim();
                if (string.IsNullOrEmpty(str))
                {
                    return;
                }
                var Ids = str.Split('|');
                foreach (var id in Ids)
                {
                    syncSceneFlagIdx.Add(int.Parse(id), 0);
                }
            }
        }

        public int RankFlag;
        //向排行榜同步数
        public Trigger RankTrigger;

        public override IEnumerable<NodeBase> Children
        {
            get { return mImpl.GetChildren(this); }
        }

        //基础方法
        public DBCharacterLogic mDbData { get; set; }

        public ulong mGuid
        {
            get { return mDbData.Id; }
        }

        public LogicProxy Proxy { get; set; }

        #region 精灵相关

        //精灵
        ////Type(0上阵，1下阵，2出战，3休息)
        /// 
//         public ErrorCodes ElfState(int index, int type, ref int state)
//         {
//             var elfBag = GetBag((int)eBagType.Elf);
//             var item = elfBag.GetItemByIndex(index);
//             if (item == null || item.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
//             var tbElf = Table.GetElf(item.GetId());
//             if (tbElf == null) return ErrorCodes.Error_ElfNotFind;
//             switch (type)
//             {
//                 case 0:
//                 {
//                     if (item.GetExdata(1) != 0)
//                     {
//                         return ErrorCodes.Error_ElfAlreadyBattle;
//                     }
// 
//                     int count = 0;
// 
//                     bool hasFight = false;
//                     foreach (var itemBase in elfBag.mLogics)
//                     {
//                         if (itemBase.GetId() != -1 )
//                         {
//                             if (itemBase.GetExdata(1) != 0)
//                             {
//                                 count++;    
//                             }
//                             if (itemBase.GetExdata(1) == 2)
//                             {
//                                 hasFight = true;
//                             }
//                         }
//                     }
//                     if (count == 3)
//                     {
//                         return ErrorCodes.Error_ElfBattleMax;
//                     }
//                     else
//                     {
//                         if (hasFight)
//                         {
//                             item.SetExdata(1, 1);
//                             state = 1;
//                         }
//                         else
//                         {
//                             item.SetExdata(1, 2);
//                             state = 2;
//                         }
//                     }
//                 }
//                     break;
//                 case 1:
//                     {
//                         if (item.GetExdata(1) == 0)
//                         {
//                             return ErrorCodes.Error_ElfNotBattle;
//                         }
//                         item.SetExdata(1, 0);
//                     }
//                     break;
//                 case 2:
//                     {//fight
//                         if (item.GetExdata(1) == 0)
//                         {
//                             return ErrorCodes.Error_ElfNotBattleMain;
//                         }
//                         if (item.GetExdata(1) == 2)
//                         {
//                             return ErrorCodes.Error_ElfIsBattleMain;
//                         }
//                         foreach (var itemBase in elfBag.mLogics)
//                         {
//                             if (itemBase.GetExdata(1) == 2)
//                             {
//                                 itemBase.MarkDbDirty();
//                                 itemBase.SetExdata(1,1); 
//                                 break;
//                             }
//                         }
//                         item.SetExdata(1,2);
//                     }
//                     break;
//                 case 3:
//                     {//disfight
//                         if (item.GetExdata(1) == 0)
//                         {
//                             return ErrorCodes.Error_ElfNotBattle;
//                         }
//                         if (item.GetExdata(1) == 1)
//                         {
//                             return ErrorCodes.Error_ElfNotBattleMain;
//                         }
//                         item.SetExdata(1, 1);
//                     }
//                     break;
//             }
//             item.MarkDbDirty();
//             mBag.RefreshElfAttr();
//             BooksChange();
//             return ErrorCodes.OK;
//         }
//         public ErrorCodes ElfReplace(int form, int to)
//         {
//             ErrorCodes ret = ErrorCodes.OK;
//             var elfBag = GetBag((int)eBagType.Elf);
//             var elfForm = elfBag.GetItemByIndex(form);
//             var elfTo = elfBag.GetItemByIndex(to);
// 
//             if (elfForm == null || elfForm.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
//             if (elfTo == null || elfTo.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
// 
//             if (elfForm.GetExdata(1) == 0 && elfTo.GetExdata(1) == 0)
//             {
//                 return ErrorCodes.Error_ElfNotBattle;
//             }
//             var state = elfForm.GetExdata(1);
//             elfForm.SetExdata(1,elfTo.GetExdata(1));
//             elfForm.MarkDbDirty();
//             elfTo.MarkDbDirty();
//             elfTo.SetExdata(1, state);
//             mBag.RefreshElfAttr();
//             BooksChange();
//             return ret;
//         }
        //当获得精灵类物品时
        public ItemBase AddElfItem(ItemBase item)
        {
            return mImpl.AddElfItem(this, item);
        }

        public ErrorCodes PetIslandBuyTili()
        {
            return mImpl.PetIslandBuyTili(this);
        }

        public int PetIslandReduceTili(int num)
        {
            return mImpl.PetIslandReduceTili(this, num);
        }

        public int PetIslandGetTili()
        {
            return mImpl.PetIslandGetTili(this);
        }

//         //上阵精灵
//         public ErrorCodes BattleElf(int index)
//         {
//             if (index > 0 && index < 3)
//             {
//                 return ErrorCodes.Error_ElfAlreadyBattle;
//             }
//             var elfBag = GetBag((int) eBagType.Elf);
//             var item =  elfBag.GetItemByIndex(index);
//             if (item == null || item.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
//             var tbElf = Table.GetElf(item.GetId());
//             if (tbElf == null) return ErrorCodes.Error_ElfNotFind;
//             for (int i = 0; i != 3; ++i)
//             {
//                 var tempItem = elfBag.GetItemByIndex(i);
//                 if (tempItem == null) continue;
//                 var tbTempElf = Table.GetElf(tempItem.GetId());
//                 if (tbTempElf == null) continue;
//                 if (tbTempElf.ElfType == tbElf.ElfType)
//                 {
//                     return ErrorCodes.Error_ElfTypeSame;
//                 }
//             }
// 
//             for (int i = 0; i != 3; ++i)
//             {
//                 var tempItem = elfBag.GetItemByIndex(i);
//                 if (tempItem == null)
//                 {
//                     mBag.MoveItem((int)eBagType.Elf, index, (int)eBagType.Elf, i, 1);
//                     mBag.RefreshElfAttr();
//                     BooksChange();
//                     return ErrorCodes.OK;
//                 }
//             }
//             return ErrorCodes.Error_ElfBattleMax;
//         }
//         //下阵精灵
//         public ErrorCodes DisBattleElf(int index)
//         {
//             if (index < 0 && index >= 3)
//             {
//                 return ErrorCodes.Error_ElfNotBattle;
//             }
//             var item = GetItemByBagByIndex((int)eBagType.Elf, index);
//             if (item == null || item.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
//             int freeindex=mBag.mBags[(int) eBagType.Elf].GetFirstFreeIndex(3);
//             if (freeindex != -1)
//             {
//                 item.SetExdata(1, 0);
//                 mBag.MoveItem((int)eBagType.Elf, index, (int)eBagType.Elf, freeindex, 1);
//                 mBag.RefreshElfAttr();
//                 BooksChange();
//                 return ErrorCodes.OK;
//             }
//             return ErrorCodes.Error_ItemNoInBag_All;
//         }
//         //出战精灵
//         public ErrorCodes BattleMainElf(int index)
//         {
//             if (index < 0 && index >= 3)
//             {
//                 return ErrorCodes.Error_ElfNotBattle;
//             }
//             var item = GetItemByBagByIndex((int)eBagType.Elf, index);
//             if (item == null || item.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
//             if (item.GetExdata(1) == 1)
//             {
//                 return ErrorCodes.Error_ElfIsBattleMain;
//             }
//             //取消其他的出战（如果有的话）
//             for (int i = 0; i != 3; ++i)
//             {
//                 if(i==index) continue;
//                 var tempItem = GetItemByBagByIndex((int)eBagType.Elf, i);
//                 if (tempItem == null)
//                 {
//                     Logger.Warn("BattleMainElf GetItemByBagByIndex[{0}][{1}] is null", (int)eBagType.Elf, i);
//                     continue;
//                 }
//                 if (tempItem.GetId() < 0) continue;
//                 if (tempItem.GetExdata(1) == 1)
//                 {
//                     tempItem.SetExdata(1, 0);
//                     tempItem.MarkDirty();
//                 }
//             }
//             //出战
//             item.SetExdata(1, 1);
//             item.MarkDirty();
//             mBag.RefreshElfAttr();
//             BooksChange();
//             return ErrorCodes.OK;
//         }
//         //休息精灵
//         public ErrorCodes DisBattleMainElf(int index)
//         {
//             if (index < 0 && index >= 3)
//             {
//                 return ErrorCodes.Error_ElfNotBattle;
//             }
//             var item = GetItemByBagByIndex((int)eBagType.Elf, index);
//             if (item == null || item.GetId() < 0) return ErrorCodes.Error_ElfNotFind;
//             if (item.GetExdata(1) == 0)
//             {
//                 return ErrorCodes.Error_ElfNotBattleMain;
//             }
//             //不出战
//             item.SetExdata(1, 0);
//             item.MarkDirty();
//             mBag.RefreshElfAttr();
//             BooksChange();
//             return ErrorCodes.OK;
//         }
        //增加物品冷却

        #endregion

        #region 聊天相关

        public virtual void ChatSpeek(eChatChannel type, string content, List<ulong> toList)
        {
            mImpl.ChatSpeek(this, type, content, toList);
        }

        #endregion

        //检查是否有可以完成的活跃度内容
        public void CheckDailyActivity(List<DailyActivityRecord> records)
        {
            mImpl.CheckDailyActivity(this, records);
        }

        //检测装备强化带来的称号
        public void CheckEquipEnhanceTitle()
        {
            mImpl.CheckEquipEnhanceTitle(this);
        }

        public void RemoveOverTimeTitles()
        {
            mImpl.RemoveOverTimeTitles(this);
        }

        public int GetAttackType()
        {
            return mImpl.GetAttackType(this);
        }

        public int GetLevel()
        {
            return mImpl.GetLevel(this);
        }

        public string GetName()
        {
            return mImpl.GetName(this);
        }

        public int GetRole()
        {
            return mImpl.GetRole(this);
        }

        public ErrorCodes GmCommand(string command)
        {
            return mImpl.GmCommand(this, command);
        }

        //尝试激活某个称号
        public void ModityTitle(int titleId, bool active)
        {
            mImpl.ModityTitle(this, titleId, active);
        }

        //尝试激活某个称号
        public void ModityTitles(List<int> titles, List<bool> states)
        {
            mImpl.ModityTitles(this, titles, states);
        }

        #region NPC服务相关

        public ErrorCodes NpcService(int serviceId)
        {
            return mImpl.NpcService(this, serviceId);
        }

        #endregion

        //处理充值成功
        public void OnRechargeSuccess(string platform, int type, float price)
        {
            mImpl.OnRechargeSuccess(this, platform, type, price);
        }

        //当vip level变化时
        public void OnVipLevelChanged(int oldlevel,int newlevel)
        {
            mImpl.OnVipLevelChanged(this,oldlevel,newlevel);
        }

        //领取所有补偿
        public ErrorCodes ReceiveAllCompensation(int type)
        {
            return mImpl.ReceiveAllCompensation(this, type);
        }

        //领取补偿
        public ErrorCodes ReceiveCompensation(int comId, int type)
        {
            return mImpl.ReceiveCompensation(this, comId, type);
        }

        public ErrorCodes TakeMultyExpAward(int id)
        {
            return mImpl.TakeMultyExpAward(this, id);
        }

        public void RefreshSkillTitle()
        {
            mImpl.RefreshSkillTitle(this);
        }

        //血色，恶魔，选择副本奖励
        public ErrorCodes SelectDungeonReward(int fubenId, int selIdx, bool isByMail = false)
        {
            return mImpl.SelectDungeonReward(this, fubenId, selIdx, isByMail);
        }

        public void OnAddCharacterContribution(int nCount)
        {
            mImpl.OnAddCharacterContribution(this,nCount);
        }
        public void SendQuitReward(FubenRecord tbFuben, bool isByMail)
        {
            mImpl.SendQuitReward(this, tbFuben, isByMail);
        }
        public void BroadCastGetEquip(int itemId, int dictId)
        {
            mImpl.BroadCastGetEquip(this, itemId, dictId);
        }

        public void SendSystemNoticeInfo(int dictId, List<string> strs = null, List<int> exInt = null)
        {
            mImpl.SendSystemNoticeInfo(this, dictId, strs, exInt);
        }

        public void SetName(string name)
        {
            mImpl.SetName(this, name);
        }

        public void SetRankFlag(RankType type)
        {
            mImpl.SetRankFlag(this, type);
        }

        //设置称号
        public ErrorCodes SetTitle(int id)
        {
            return mImpl.SetTitle(this, id);
        }

        public override string ToString()
        {
            return mDbData.Id.ToString();
        }

        public DBCharacterLogicSimple GetSimpleData()
        {
            return mImpl.GetSimpleData(this);
        }

        public DBCharacterLogic GetData()
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

        public void OnSaveData(DBCharacterLogic data, DBCharacterLogicSimple simpleData)
        {
            mImpl.OnSaveData(this, data, simpleData);
        }

        public bool Online
        {
            get { return mImpl.GetOnline(this); }
        }

        public CharacterState State { get; set; }

        #region 数据

        public int serverId
        {
            get { return mDbData.ServerId; }
            set { mDbData.ServerId = value; }
        }

        public BitFlag mFuncBlock = new BitFlag((int) FunctionBlock.Count);
        public int mServiceId = -1;
        public string Name;
        //private ulong mGuid;                                      //玩家的Guid
        public BagManager mBag; //玩家包裹
        public MissionManager mTask; //玩家邮件
        public SkillData mSkill; //玩家技能
        public Talent mTalent; //玩家天赋
        public NodeFlag lFlag; //玩家标记位
        public Exdata lExdata; //玩家扩展数据
        public Exdata64 lExdata64; //玩家64位扩展数据
        public BookManager mBook; //玩家图鉴
        public MountManager mMount; //坐骑
        public FriendManager mFriend; //玩家好友
        public StoneManager mStone; //玩家商店
        public MailManager mMail; //邮件
        public CityManager mCity; //家园
        public PetMissionManager2 mPetMission; //宠物任务
        public AllianceManager mAlliance; //战盟
        public Exchange mExchange; //交易所
		public PlayerOperationActivityManager mOperActivity;
        public long TodayTimes; //今天在线的总时间
        public DateTime OnlineTime; //上线时间
        //public BitFlag mFlag;                                     //玩家标记位
        //public int[] mExdata;                                     //玩家扩展数据
        //public Dictionary<int, object> ItemCd = new Dictionary<int, object>(); //存储物品使用CD时间

		public const int MaxNodeNum = 16;
        //构造
        public NodeBase[] childs = new NodeBase[MaxNodeNum];
        public bool RefreshElfFightPoint = true;
        public int ElfFightPoint;

        public struct ChallengeInvitor
        {
            public ulong CharacterId { get; set; }
            public string Name { get; set; }
            public int ServerId { get; set; }
        }

        //角斗邀请者列表
        public Dictionary<ulong, ChallengeInvitor> ChallengeInvitors = new Dictionary<ulong, ChallengeInvitor>();

        #endregion

        #region 每天登陆

        //事件处理
        //public void DailyFirstOnlineByEvent(int type, int hour, int count)
        //{
        //    mImpl.DailyFirstOnlineByEvent(this, type, hour, count);
        //}
        //登陆处理
        public void DailyFirstOnline(int continuedLanding)
        {
            mImpl.DailyFirstOnline(this, continuedLanding);
        }

        //下线
        public void OutLine()
        {
            mImpl.OutLine(this);
        }

        public void RefreshTrialTime()
        {
            mImpl.RefreshTrialTime(this);
        }

        #endregion

        #region 扩展计数相关

        public List<int> GetExData()
        {
            return mImpl.GetExData(this);
        }

        public int GetExData(int index)
        {
            return mImpl.GetExData(this, index);
        }

        public long GetExData64(int index)
        {
            return mImpl.GetExData64(this, index);
        }

        //设置扩展计数
        public void SetExData(int index, int value, bool forceNotToClient = false)
        {
            mImpl.SetExData(this, index, value, forceNotToClient);
        }

        //增加扩展计数
        public void AddExData(int index, int value, bool forceNotToClient = false)
        {
            mImpl.AddExData(this, index, value, forceNotToClient);
        }

        //设置某个扩展计数，但是必须要大于之前的值
        public void SetExdataToMore(int index, int value, bool forceNotToClient = false)
        {
            var nowValue = GetExData(index);
            if (nowValue >= value)
            {
                return;
            }
            SetExData(index, value, forceNotToClient);
        }

        public void OnLevelUp(int lv)
        {
            mImpl.OnLevelUp(this, lv);
        }

        #endregion

        #region 标记位相关

        public bool GetFlag(int index)
        {
            return mImpl.GetFlag(this, index);
        }

        //设置扩展标记
        public void SetFlag(int index, bool flag = true, int forceNotToClient = 0)
        {
            mImpl.SetFlag(this, index, flag, forceNotToClient);
        }

        public void SetTitleFlag(int index, bool flag, int forceNotToClient, DateTime titleStartTime)
        {
            mImpl.SetTitleFlag(this, index, flag, forceNotToClient, titleStartTime);
        }


        #endregion

        #region 判断相关

        //判断条件
        //返回值 -2 表示测试通过
        public int CheckCondition(int nConditionId)
        {
            return mImpl.CheckCondition(this, nConditionId);
        }

        //是否满足天梯判断条件
        public ErrorCodes CheckP1vP1()
        {
            return mImpl.CheckP1vP1(this);
        }

        #endregion

        #region 道具相关

        //测试包裹索引的合法性
        public void TestBagLogicIndex()
        {
            mImpl.TestBagLogicIndex(this);
        }

        public void TestBagDbIndex()
        {
            mImpl.TestBagDbIndex(this);
        }

        public ErrorCodes CheckEquipOn(EquipRecord tbEquip, int nEquipPart)
        {
            return mImpl.CheckEquipOn(this, tbEquip, nEquipPart);
        }

        //使用装备
        /// <summary>
        ///     使用装备
        /// </summary>
        /// <param name="nBagIndex">装备包裹的索引</param>
        /// <param name="nEquipPart">部位ID</param>
        /// <param name="index">部位索引</param>
        /// <returns></returns>
        public ErrorCodes UseEquip(int nBagIndex, int nEquipPart, int index)
        {
            return mImpl.UseEquip(this, nBagIndex, nEquipPart, index);
        }

        public bool DeleteEquip(int ItemId,int deleteType)
        {
            return mImpl.DeleteEquip(this, ItemId, deleteType);
        }

        public ErrorCodes UseShiZhuang(int BagId, int BagIndex, int EquipPart)
        {
            return mImpl.UseShiZhuang(this, BagId, BagIndex, EquipPart);
        }

        public ErrorCodes DeleteShiZhuang(int BagId, int BagIndex)
        {
            return mImpl.DeleteShiZhuang(this, BagId, BagIndex);
        }

        public ErrorCodes SetEquipModelState(List<int> Part, int State)
        {
            return mImpl.SetEquipModelState(this, Part, State);
        }

        public void RefreshFashionState()
        {
            mImpl.RefreshFashionState(this);
        }

        public ErrorCodes TowerSweep(TowerSweepResult respone)
        {
            return mImpl.TowerSweep(this,respone);
        }
        public ErrorCodes TowerBuySweepTimes()
        {
            return mImpl.TowerBuySweepTimes(this);
        }
        public ErrorCodes CheckTowerDailyInfo()
        {
            return mImpl.CheckTowerDailyInfo(this);
        }
        
        //降低耐久度
        public void DurableDown(int bagIdandIndex, int diffValue)
        {
            mImpl.DurableDown(this, bagIdandIndex, diffValue);
        }

        //请求耐久度
        public void ApplyEquipDurable(Dictionary<int, int> durables)
        {
            mImpl.ApplyEquipDurable(this, durables);
        }

        //修理
        public ErrorCodes RepairEquip()
        {
            return mImpl.RepairEquip(this);
        }

        //获得某个包裹的某个索引的道具
        public ItemBase GetItemByBagByIndex(int bagid, int bagindex)
        {
            return mImpl.GetItemByBagByIndex(this, bagid, bagindex);
        }

        //获得某个包裹的某个索引的道具
        public BagBase GetBag(int bagid)
        {
            return mImpl.GetBag(this, bagid);
        }

        //出售物品
        public ErrorCodes SellItem(int nBagId, int nIndex, int nItemId, int nCount)
        {
            return mImpl.SellItem(this, nBagId, nIndex, nItemId, nCount);
        }

        //回收物品
        public ErrorCodes Recycletem(int nBagId, int nIndex, int nItemId, int nCount)
        {
            return mImpl.Recycletem(this, nBagId, nIndex, nItemId, nCount);
        }

        //检查所需包裹格数
        public static int[] bagId = new int[2];
        public static int[] bagCount = new int[2];
        public static Dictionary<int, int> itemList = new Dictionary<int, int>();
        public static Dictionary<int, int> itemList2 = new Dictionary<int, int>();

        public bool CheckBagCanIn(int value)
        {
            return mImpl.CheckBagCanIn(this, value);
        }

        //使用物品
        public IEnumerator UseItem(Coroutine coroutine, UseItemInMessage msg)
        {
            return mImpl.UseItem(coroutine, this, msg);
        }

        public ErrorCodes AutoUseItem(int itemId)
        {
            return mImpl.AutoUseItem(this, itemId);
        }
        

        //存放物品
        public ErrorCodes DepotPutIn(int nBagId, int nIndex)
        {
            return mImpl.DepotPutIn(this, nBagId, nIndex);
        }

        //取出物品
        public ErrorCodes DepotTakeOut(int nIndex)
        {
            return mImpl.DepotTakeOut(this, nIndex);
        }

        //从许愿池仓库取出物品
        public ErrorCodes WishingPoolDepotTakeOut(int nIndex)
        {
            return mImpl.WishingPoolDepotTakeOut(this, nIndex);
        }

        //获得某个Id的宠物
        public PetItem GetPet(int petId)
        {
            return mImpl.GetPet(this, petId);
        }

        //获得某个Id的宠物
        public PetItem GetSamePet(int petId)
        {
            return mImpl.GetSamePet(this, petId);
        }

        //宠物操作
        public ErrorCodes OperatePet(int petId, PetOperationType type, int param)
        {
            return mImpl.OperatePet(this, petId, type, param);
        }

        //获得翅膀
        public WingItem GetWing()
        {
            return mImpl.GetWing(this);
        }

        //翻牌的接口
        public int PushDraw(int drawId, out ItemBase tempItemBase, bool isAddItem = true, int dungeonId = -1)
        {
            return mImpl.PushDraw(this, drawId, out tempItemBase, isAddItem, dungeonId);
        }

        //购买包裹
        public ErrorCodes BuySpaceBag(int bagId, int bagIndex, int needCount)
        {
            //return mImpl.BuySpaceBag(this, bagId, bagIndex, needCount);
            return mImpl.BuySpaceBagByPaid(this, bagId, bagIndex, needCount);
        }

        #endregion

        #region 同步数据 To Scene

        //已穿装备修改
        public void EquipChange(int nType, int nPart, int nIndex, ItemBase item)
        {
            mImpl.EquipChange(this, nType, nPart, nIndex, item);
        }

        public IEnumerator SceneEquipChange(Coroutine coroutine,
                                            ulong characterId,
                                            int nType,
                                            int nPart,
                                            ItemBaseData Equip)
        {
            return mImpl.SceneEquipChange(coroutine, this, characterId, nType, nPart, Equip);
        }

        public IEnumerator SceneEquipModelStateChange(Coroutine coroutine,
                                                      ulong characterId,
                                                      int nPart,
                                                      int nState,
                                                      ItemBaseData Equip)
        {
            return mImpl.SceneEquipModelStateChange(coroutine, this, characterId, nPart, nState, Equip);
        }

        public void ElfChange(List<int> removeBuff, Dictionary<int, int> addBuff)
        {
            mImpl.ElfChange(this, removeBuff, addBuff);
        }

        public IEnumerator SceneElfChange(Coroutine coroutine,
            ulong characterId,
            List<int> removeBuff,
            Dictionary<int, int> addBuff)
        {
            return mImpl.SceneElfChange(coroutine, this, characterId, removeBuff, addBuff);
        }

        public void GetElfBuff(Dictionary<int, int> buffs)
        {
            mImpl.GetElfBuff(this, buffs);
        }

        public void GetMountBuff(Dictionary<int, int> buffs)
        {
            mImpl.GetMountBuff(this,buffs);
        }

        //天赋修改
        public void TalentChange(int nType, int nTalent, int nLevel)
        {
            mImpl.TalentChange(this, nType, nTalent, nLevel);
        }

        public IEnumerator SceneInnateChange(Coroutine coroutine, ulong characterId, int nType, int nTalent, int nLevel)
        {
            return mImpl.SceneInnateChange(coroutine, this, characterId, nType, nTalent, nLevel);
        }


        //技能修改
        public void SkillChange(int nType, int nSkillId, int nLevel)
        {
            mImpl.SkillChange(this, nType, nSkillId, nLevel);
        }

        public IEnumerator SceneSkillChange(Coroutine coroutine, ulong characterId, int nType, int nSkillId, int nLevel)
        {
            return mImpl.SceneSkillChange(coroutine, this, characterId, nType, nSkillId, nLevel);
        }

        //重新装备技能
        public void EquipSkillChange(Int32Array dels, Int32Array adds, Int32Array Lvls)
        {
            mImpl.EquipSkillChange(this, dels, adds, Lvls);
        }

        //重新装备技能
        public IEnumerator SceneEquipSkillChange(Coroutine coroutine,
                                                 ulong characterId,
                                                 Int32Array dels,
                                                 Int32Array adds,
                                                 Int32Array lvls)
        {
            return mImpl.SceneEquipSkillChange(coroutine, this, characterId, dels, adds, lvls);
        }

        public int GetBooksAttr(Dictionary<int, int> attrs, Dictionary<int, int> monsterAttrs)
        {
            return mImpl.GetBooksAttr(this, attrs, monsterAttrs);
        }

        //图鉴，加点，果子，精灵（属性修改）
        public void BooksChange()
        {
            mImpl.BooksChange(this);
        }

        public void Mount(int MountId)
        {
            mImpl.Mount(this,MountId);
        }
        public IEnumerator SceneBooksChange(Coroutine coroutine, ulong characterId, Dict_int_int_Data dic, Dict_int_int_Data dic2)
        {
            return mImpl.SceneBooksChange(coroutine, this, characterId, dic,dic2);
        }

        public int GetElfFightPoint()
        {
            return mImpl.GetElfFightPoint(this);
        }
        public void SetRefreshFightPoint(bool refresh)
        {
            mImpl.SetRefreshFightPoint(this, refresh);
        }
        #endregion

        #region 一级属性相关

        //分配加点
        public ErrorCodes AddAttrPoint(int Strength, int Agility, int Intelligence, int Endurance)
        {
            return mImpl.AddAttrPoint(this, Strength, Agility, Intelligence, Endurance);
        }

        //获得基础点数
        public int GetAttrPoint(eAttributeType attrId)
        {
            return mImpl.GetAttrPoint(this, attrId);
        }

        //洗点
        public ErrorCodes RefreshAttrPoint(ref int newPoint)
        {
            return mImpl.RefreshAttrPoint(this, ref newPoint);
        }

        //专职
        public ErrorCodes ChangeRole()
        {
            return mImpl.ChangeRole(this);
        }

        #endregion

        #region 副本相关

        //通知Scene
        public IEnumerator AskEnterDungeon(Coroutine coroutine, int serverId, int sceneId, ulong guid, SceneParam param)
        {
            return mImpl.AskEnterDungeon(coroutine, this, serverId, sceneId, guid, param);
        }

        //购买副本次数
        public ErrorCodes BuyFubenCount(int fubenId)
        {
            return mImpl.BuyFubenCount(this, fubenId);
        }

        //扫荡副本
        public ErrorCodes PassFuben(int fubenId, DrawResult dataResult)
        {
            return mImpl.PassFuben(this, fubenId, dataResult);
        }

        //条件判断
        public ErrorCodes CheckFuben(int fubenId)
        {
            return mImpl.CheckFuben(this, fubenId);
        }
        public ErrorCodes CheckFubenDetail(int fubenId, ref int DetailCode)
        {
            return mImpl.CheckFubenDetail(this, fubenId,ref DetailCode);
        }
        //进入副本
        public IEnumerator EnterFuben(Coroutine co,
                                      int fubenId,
                                      AsyncReturnValue<ErrorCodes> errCode,
                                      AsyncReturnValue<int> check)
        {
            return mImpl.EnterFuben(co, this, fubenId, errCode, check);
        }

        public bool CheckDungeonReward(FubenRecord tbFuben)
        {
            return mImpl.CheckDungeonReward(this, tbFuben);
        }

        public int GetMieShiFailTitleFlag()
        {
            return mImpl.GetMieShiFailTitleFlag(this);            
        }

        public int GetMieShiSuccessTitleFlag()
        {
            return mImpl.GetMieShiSuccessTitleFlag(this);
        }

        //完成副本
        public void CompleteFuben(FubenResult result)
        {
            mImpl.CompleteFuben(this, result);
        }

        //完成副本
        public void CompleteFubenSaveData(int fubenId, int seconds)
        {
            mImpl.CompleteFubenSaveData(this, fubenId, seconds);
        }

        //增加副本次数
        public void AddFubenCount(FubenRecord tbFuben)
        {
            mImpl.AddFubenCount(this, tbFuben);
        }

        //战场结果
        public void BattleResult(int fubenId, int type)
        {
            mImpl.BattleResult(this, fubenId, type);
        }

        #endregion

        #region 灭世活动相关
        //购买副本次数
        public ErrorCodes ApplyActivityData(int serverId, CommonActivityData responeData)
        {
            return mImpl.ApplyActivityData(this, serverId, responeData);
        }

        #endregion

        #region PvP

        //升级军衔
        public ErrorCodes UpgradeHonor(int honorId)
        {
            return mImpl.UpgradeHonor(this, honorId);
        }

        public void GetP1vP1OldList(List<P1vP1Change_One> list)
        {
            mImpl.GetP1vP1OldList(this, list);
        }

        public int P1vP1ChangeCountMax = Table.GetServerConfig(325).ToInt();

        public P1vP1Change_One PushP1vP1Change(int type, string pvpName, int oldRank, int newRank)
        {
            return mImpl.PushP1vP1Change(this, type, pvpName, oldRank, newRank);
        }

        #endregion

        #region  升级战盟Buff

        private int GetBuffExdataId(int buffId)
        {
            if (buffId < 200)
            {
                return 550;
            }
            if (buffId < 300)
            {
                return 551;
            }
            if (buffId < 400)
            {
                return 552;
            }
            if (buffId < 500)
            {
                return 553;
            }
            return -1;
        }

        public ErrorCodes CheckAllianceBuff(int buffId)
        {
            if (mAlliance.AllianceId <= 0)
            {
                return ErrorCodes.Error_CharacterNoAlliance;
            }
            var tbBuff = Table.GetGuildBuff(buffId);
            if (tbBuff == null)
            {
                return ErrorCodes.Error_AllianceBuffID;
            }
            var exId = GetBuffExdataId(buffId);
            if (exId == -1)
            {
                return ErrorCodes.Error_AllianceBuffID;
            }
            var nowId = GetExData(exId);
            GuildBuffRecord tbNowBuff = null;
            if (nowId > 0)
            {
                tbNowBuff = Table.GetGuildBuff(nowId);
            }
            if (tbNowBuff != null)
            {
                if (tbBuff.NeedUnionLevel > tbNowBuff.NeedUnionLevel)
                {
                    return ErrorCodes.Error_CheckAllianceLevel;
                }
                if (tbNowBuff.NextLevel != buffId)
                {
                    return ErrorCodes.Error_AllianceBuffID;
                }
            }
            else
            {
                if (tbBuff.BuffLevel != 1)
                {
                    return ErrorCodes.Error_AllianceBuffID;
                }
            }

            var myres = mBag.GetRes(eResourcesType.Contribution);
            if (myres < tbBuff.UpConsumeGongji)
            {
                return ErrorCodes.Error_GongjiNotEnough;
            }
            if (tbBuff.LevelLimit > GetLevel())
            {
                return ErrorCodes.Error_LevelNoEnough;
            }
            return ErrorCodes.OK;
        }

        public ErrorCodes UpgradeAllianceBuff(int buffId)
        {
            var result = CheckAllianceBuff(buffId);
            if (result != ErrorCodes.OK && result != ErrorCodes.Error_CheckAllianceLevel)
            {
                return result;
            }

            var exId = GetBuffExdataId(buffId);
            var tbBuff = Table.GetGuildBuff(buffId);
            mBag.DelRes(eResourcesType.Contribution, tbBuff.UpConsumeGongji, eDeleteItemType.AllianceBuff);
            SetExData(exId, buffId);
            AddExData(954,1);
            CoroutineFactory.NewCoroutine(BuffLevelChange, buffId).MoveNext();
            return ErrorCodes.OK;
        }

        //通知玩家Buff等级变化了
        private IEnumerator BuffLevelChange(Coroutine co, int buffIds)
        {
            var result = LogicServer.Instance.SceneAgent.SSAllianceBuffDataChange(mGuid, buffIds);
            yield return result.SendAndWaitUntilDone(co);
        }

        #endregion

        #region 初始化

        public bool InitByDb(ulong characterId, DBCharacterLogic dbData)
        {
            return mImpl.InitByDb(this, characterId, dbData);
        }

        public DBCharacterLogic InitByBase(ulong characterId, object[] args)
        {
            return mImpl.InitByBase(this, characterId, args);
        }

        public void ApplyVolatileData(DBCharacterLogicVolatile data)
        {
            mImpl.ApplyVolatileData(this, data);
        }

        public void LoadFinished()
        {
            mImpl.LoadFinished(this);
        }
        public void SetMoniterData(MsgChatMoniterData data)
        {
            mImpl.SetMoniterData(this, data);
        }
        #endregion
    }
}