#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;
using TeamClientService;

#endregion

namespace Logic
{

    #region 接口类

    public interface ICityManager
    {
        void AddToBuyItems(CityManager _this, long id);
        void BuyItemSuccess(CityManager _this, long itemGuid, int Count);
        ErrorCodes BuyRefreshMission(CityManager _this, int index, int costClient);
        void CacheHistoryItems(CityManager _this, GroupShopItemAllForServer data);
        void CacheItems(CityManager _this, SSApplyGroupShopItemsOutMessage msg);
        DateTime ChangeTime(CityManager _this, DateTime beforeTime, int before, int after);
        ErrorCodes CheckMissionRefresh(CityManager _this);
        void CityAddExp(CityManager _this, int value);
        ErrorCodes CommitMission(CityManager _this, int index, int misId);
        ErrorCodes CreateBuild(CityManager _this, int tableId, int areaId);
        ErrorCodes DestroyBuild(CityManager _this, int areaId);
        ErrorCodes DropMission(CityManager _this, DBBuildMission mis);
        List<long> GetAllCachedItems(CityManager _this);
        BuildingBase GetBuildByArea(CityManager _this, int areaId);
        BuildingBase GetBuildByAreaId(CityManager _this, int idx);
        BuildingBase GetBuildByGuid(CityManager _this, int guid);
        BuildingBase GetBuildByType(CityManager _this, int type);
        int GetBuildCount(CityManager _this, int type);
        List<long> GetBuyedItems(CityManager _this);
        int GetCityMissionLevel(CityManager _this);
        List<DBBuildMission> GetCityMissions(CityManager _this);
        int GetComposePro(CityManager _this);
        int GetCurrentUpgradingOrBuildingNumber(CityManager _this);
        List<long> GetHistoryItems(CityManager _this);
        List<Int64Array> GetItems(CityManager _this, List<int> types);
        int GetMaxLevel(CityManager _this);
        DBBuildMission GetMission(CityManager _this, int misIndex);
        int GetNeedExp(CityManager _this, int lvl);
        int GetRandomCityMission(CityManager _this, ref int misValue);
        int GetRandomCityMission2(CityManager _this);
        Dictionary<int, int> GetRefreshAttr(CityManager _this);
        void GivePetExp(CityManager _this);
        int HomeLevel(CityManager _this);
        DBBuild_List InitByBase(CityManager _this, CharacterController character);
        void InitByDB(CityManager _this, CharacterController character, DBBuild_List builds);
        bool IsContainsItemid(CityManager _this, long itemId);
        void NetDirtyHandle(CityManager _this);
        void OnDestroy(CityManager _this);
        void RefreshAttr(CityManager _this);
        void RefreshMission(CityManager _this);
        void SetAttrFlag(CityManager _this);
        ErrorCodes UpgradeBuild(CityManager _this, int areaId);

        ErrorCodes UseBuildService(CityManager _this,
                                   int areaId,
                                   int serviceId,
                                   List<int> param,
                                   ref UseBuildServiceResult result);
    }

    #endregion

    public class CityManagerDefaultImpl : ICityManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region   初始化

        public DBBuild_List InitByBase(CityManager _this, CharacterController character)
        {
            _this.mDbData = new DBBuild_List();
            _this.mDbData.NextGuid = 1;
            _this.mCharacter = character;

            Table.ForeachHomeSence(table =>
            {
                if (-1 != table.BuildId)
                {
                    var tableBuilding = Table.GetBuilding(table.BuildId);
                    var building = CreateBuild(_this, tableBuilding, table.Id);
                    if (null != building)
                    {
                        building.State = BuildStateType.Idle;
                        building.StateOverTime = DateTime.Now;
                    }
                }
                return true;
            });
            RefreshMission(_this);
            InitDbShop(_this);
            _this.MarkDirty();
            return _this.mDbData;
        }

        public void InitByDB(CityManager _this, CharacterController character, DBBuild_List builds)
        {
            _this.mCharacter = character;
            _this.mDbData = builds;
            foreach (var dbbuild in builds.mData)
            {
                var build = new BuildingBase(character, dbbuild);
                _this.Buildings.Add(dbbuild.Guid, build);
                _this.BuildingsByArea.Add(dbbuild.AreaId, build);
                _this.AddChild(build);
                if (build.State == BuildStateType.Building || build.State == BuildStateType.Upgrading)
                {
                    if (DateTime.Now >= build.StateOverTime)
                    {
                        build.TimeOver();
                    }
                    else
                    {
                        build.StartTrigger(build.StateOverTime);
                    }
                }
            }
            //取消建筑按时间给宠物经验
            //GivePetExp();
            SetAttrFlag(_this);
        }

        private int GetNextId(CityManager _this)
        {
            return _this.mDbData.NextGuid++;
        }

        #endregion

        #region 基础接口

        //家园等级
        public int HomeLevel(CityManager _this)
        {
            //第0个建筑就是议事大厅
//             if (_this.BuildingsByArea.Count > 0)
//                 return _this.BuildingsByArea[0].TbBuild.Level;
// 
//             return 1;
            return _this.Level;
        }

        //获得某个建筑类型的数量
        public int GetBuildCount(CityManager _this, int type)
        {
            var count = 0;
            foreach (var building in _this.BuildingsByArea)
            {
                if (building.Value.TbBuild.Type == type)
                {
                    ++count;
                }
            }
            return count;
        }

        //靠Type获取建筑
        public BuildingBase GetBuildByType(CityManager _this, int type)
        {
            foreach (var pair in _this.BuildingsByArea)
            {
                if (pair.Value.TbBuild.Type == type)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        //靠Area获取建筑
        public BuildingBase GetBuildByArea(CityManager _this, int areaId)
        {
            BuildingBase build;
            if (_this.BuildingsByArea.TryGetValue(areaId, out build))
            {
                return build;
            }
            return null;
        }

        //靠GUID获取建筑
        public BuildingBase GetBuildByGuid(CityManager _this, int guid)
        {
            BuildingBase build;
            if (_this.Buildings.TryGetValue(guid, out build))
            {
                return build;
            }
            return null;
        }

        //用AreaId获取建筑
        public BuildingBase GetBuildByAreaId(CityManager _this, int idx)
        {
            BuildingBase build;
            if (_this.BuildingsByArea.TryGetValue(idx, out build))
            {
                return build;
            }
            return null;
        }

        //尝试建造建筑
        public ErrorCodes CreateBuild(CityManager _this, int tableId, int areaId)
        {
            var tbBuild = Table.GetBuilding(tableId);
            if (tbBuild == null)
            {
                return ErrorCodes.Error_BuildID;
            }

            //当前建造的和当前是否一样
            var building = GetBuildByAreaId(_this, areaId);
            if (null != building && tableId == building.TypeId)
            {
                return ErrorCodes.Error_BuildID;
            }

            //判断条件
            var result = CheckBuild(_this, tbBuild, areaId);
            if (result != ErrorCodes.OK)
            {
                return result;
            }
            //资源消耗
            for (var i = 0; i < tbBuild.NeedItemId.Length; i++)
            {
                _this.mCharacter.mBag.DeleteItem(tbBuild.NeedItemId[i], tbBuild.NeedItemCount[i],
                    eDeleteItemType.CreateBuild);
            }
            //建造
            CreateBuild(_this, tbBuild, areaId);
            //潜规则引导标记位
//             if (tableId == 110)
//             {
//                 //_this.mCharacter.SetFlag(501);
//                 _this.mCharacter.SetFlag(512);
//             }
//             else if (tableId == 50)
//             {
//                 if (_this.mCharacter.GetFlag(503))
//                 {
//                     //_this.mCharacter.SetFlag(505);
//                     _this.mCharacter.SetFlag(503, false);
//                 }
//             }
//             else if (tableId == 10)
//             {
//                 if (_this.mCharacter.GetFlag(510))
//                 {
//                     _this.mCharacter.SetFlag(510, false);
//                 }
//             }
            return ErrorCodes.OK;
        }

        //检查建造
        private ErrorCodes CheckBuild(CityManager _this, BuildingRecord tbBuild, int areaId)
        {
            //区域空的检查
            BuildingBase building;
            if (_this.BuildingsByArea.TryGetValue(areaId, out building))
            {
                if (building.Type != BuildingType.Space)
                {
                    return ErrorCodes.Error_BuildAreaNotEmpty;
                }
            }
            //所需人物等级
            if (tbBuild.Level > _this.Level)
            {
                return ErrorCodes.Error_HomeLevelNoEnough;
            }
            //资源检查
            for (var i = 0; i < tbBuild.NeedItemId.Length; i++)
            {
                if (_this.mCharacter.mBag.GetItemCount(tbBuild.NeedItemId[i]) < tbBuild.NeedItemCount[i])
                {
                    return ErrorCodes.Error_ResNoEnough;
                }
            }
            //数量上限检查
            var cityLevel = HomeLevel(_this);
            if (GetBuildCount(_this, tbBuild.Type) >= Table.GetBuildingRule(tbBuild.Type).CityLevel[cityLevel - 1])
            {
                return ErrorCodes.Error_BuildCountMax;
            }
            //大本营等级检查
            if (tbBuild.NeedHomeLevel > cityLevel)
            {
                return ErrorCodes.Error_NeedCityLevelMore;
            }
            //当前是否可建造新的建筑
            if (GetCurrentUpgradingOrBuildingNumber(_this) >= 1)
            {
                return ErrorCodes.Error_CityCanotBuildMore;
            }
            return ErrorCodes.OK;
        }

        //建造
        private BuildingBase CreateBuild(CityManager _this, BuildingRecord tbBuild, int areaId)
        {
            var guid = GetNextId(_this);
            BuildingBase build;

            if (_this.BuildingsByArea.TryGetValue(areaId, out build))
            {
                _this.Buildings.Remove(build.Guid);
                //BuildingsByArea.Remove(build.AreaId);
                //mDbData.mData.Remove(build.mDbData);
                build.Reset(guid, areaId, tbBuild, BuildStateType.Building);
                _this.Buildings.Add(build.Guid, build);
                //BuildingsByArea.Add(build.AreaId, build);
                //mDbData.mData.Add(build.mDbData);
                build.MarkDirty();
            }
            else
            {
                build = new BuildingBase(_this.mCharacter, guid, tbBuild);
                build.AreaId = areaId;
                build.Reset(guid, areaId, tbBuild, BuildStateType.Idle);
                _this.Buildings.Add(build.Guid, build);
                _this.BuildingsByArea.Add(build.AreaId, build);
                _this.mDbData.mData.Add(build.mDbData);
                _this.AddChild(build);
                build.MarkDirty();
            }
            return build;
        }

        //摧毁
        public ErrorCodes DestroyBuild(CityManager _this, int areaId)
        {
            BuildingBase build;
            if (!_this.BuildingsByArea.TryGetValue(areaId, out build))
            {
                return ErrorCodes.Error_BuildNotFind;
            }
            if (build.TbBuild.CanRemove != 1)
            {
                return ErrorCodes.Unknow;
            }
            if (build.TbBuild.RemoveNeedCityLevel > HomeLevel(_this))
            {
                return ErrorCodes.Error_NeedCityLevelMore;
            }

            var needId = build.TbBuild.RemoveNeedRes;
            if (needId > 0)
            {
                var needCount = build.TbBuild.RemoveNeedCount;
                if (_this.mCharacter.mBag.GetItemCount(needId) < needCount)
                {
                    return ErrorCodes.Error_ResNoEnough;
                }
                _this.mCharacter.mBag.DeleteItem(needId, needCount, eDeleteItemType.DestroyBuild);
            }
            var removeId = build.TbBuild.RemovedBuildID;
            var tbRemoveBuild = Table.GetBuilding(removeId);
            if (tbRemoveBuild == null)
            {
                return ErrorCodes.Error_BuildID;
            }
            build.Reset(build.Guid, build.AreaId, tbRemoveBuild, BuildStateType.Idle);
            build.StateOverTime = DateTime.Now;
            //_this.CityAddExp(tbRemoveBuild.GetMainHouseExp);
            //潜规则引导标记位
            if (build.TypeId == 110)
            {
//修复许愿池
                if (!_this.mCharacter.GetFlag(501))
                {
                    _this.mCharacter.SetFlag(501);
                    _this.mCharacter.SetFlag(512);
                }
            }
            else if (build.TypeId == 50)
            {
//                 if (_this.mCharacter.GetFlag(503))
//                 {
//                     //_this.mCharacter.SetFlag(505);
//                     _this.mCharacter.SetFlag(503, false);
//                 }
            }
//             else if (build.TypeId == 10)
//             {
//                 if (_this.mCharacter.GetFlag(510))
//                 {
//                     _this.mCharacter.SetFlag(510, false);
//                 }
//             }
            else if (build.TypeId == 20)
            {
//农场
                if (!_this.mCharacter.GetFlag(521))
                {
                    _this.mCharacter.SetFlag(521);
                    _this.mCharacter.SetFlag(522);
                }
            }
// 			else if (build.TypeId == 100)
// 			{//合成屋
// 				if (!_this.mCharacter.GetFlag(527))
// 				{
// 					_this.mCharacter.SetFlag(527);
// 					_this.mCharacter.SetFlag(525,false);
// 				}
// 			}
            else if (build.TypeId == 60)
            {
//决斗圣殿
                if (!_this.mCharacter.GetFlag(530))
                {
                    _this.mCharacter.SetFlag(530);
                    _this.mCharacter.SetFlag(529, false);
                }
            }
            else if (build.TypeId == 90)
            {
//交易所
                if (!_this.mCharacter.GetFlag(533))
                {
                    _this.mCharacter.SetFlag(533);
                    _this.mCharacter.SetFlag(532, false);
                }
            }
            build.MarkDirty();
            return ErrorCodes.OK;
        }

        //升级建筑
        public ErrorCodes UpgradeBuild(CityManager _this, int areaId)
        {
            var build = GetBuildByArea(_this, areaId);
            if (build == null)
            {
                return ErrorCodes.Error_BuildNotFind;
            }
            var tbBuild = build.TbBuild;
            //数量上限检查， （策划要求建筑升级不在判断这个条件）
            var cityLevel = HomeLevel(_this);
            //          if (GetBuildCount(tbBuild.Type) >= Table.GetBuildingRule(tbBuild.Type).CityLevel[cityLevel - 1])
            //          {
            //              return ErrorCodes.Error_BuildCountMax;
            //          }
            var tbNextBuild = Table.GetBuilding(tbBuild.NextId);
            //资源检查
            for (var i = 0; i < tbNextBuild.NeedItemId.Length; i++)
            {
                if (_this.mCharacter.mBag.GetItemCount(tbNextBuild.NeedItemId[i]) < tbNextBuild.NeedItemCount[i])
                {
                    return ErrorCodes.Error_ResNoEnough;
                }
            }
            if (tbNextBuild == null)
            {
                return ErrorCodes.Error_BuildLevelMax;
            }
            ////所需人物等级
            //if (tbNextBuild.NeedCharacterLevel > mCharacter.GetLevel())
            //{
            //    return ErrorCodes.Error_LevelNoEnough;
            //}

            //当前是否可建造新的建筑
            if (GetCurrentUpgradingOrBuildingNumber(_this) >= 1)
            {
                return ErrorCodes.Error_CityCanotBuildMore;
            }

            if (BuildingType.BaseCamp == (BuildingType) tbBuild.Type)
            {
                //如果是建筑是议事大厅
                if (_this.Level < tbNextBuild.NeedHomeLevel)
                {
                    return ErrorCodes.Error_HomeLevelNoEnough;
                }
            }
            else
            {
                //普通建筑
                if (tbNextBuild.NeedHomeLevel > cityLevel)
                {
                    //大本营等级检查
                    return ErrorCodes.Error_HomeLevelNoEnough;
                }
            }

            //资源消耗
            for (var i = 0; i < tbNextBuild.NeedItemId.Length; i++)
            {
                _this.mCharacter.mBag.DeleteItem(tbNextBuild.NeedItemId[i], tbNextBuild.NeedItemCount[i],
                    eDeleteItemType.UpgradeBuild);
            }
            //升级
            return build.Upgrade();
        }

        //时间改变
        public DateTime ChangeTime(CityManager _this, DateTime beforeTime, int before, int after)
        {
            var now = DateTime.Now;
            var second = (int) (beforeTime - now).TotalSeconds;
            if (second <= 0)
            {
                return beforeTime;
            }
            var totleSecond = second*before/after + 1;
            //举例：目前还有3600秒，之前before = 15000（150%速度） , after = 13000（130%）, 变化后4153+1
            return now.AddSeconds(totleSecond);
        }

        //获得当前正在建造或者升级的建筑个数
        public int GetCurrentUpgradingOrBuildingNumber(CityManager _this)
        {
            var count = 0;
            foreach (var pair in _this.BuildingsByArea)
            {
                if (BuildStateType.Building == pair.Value.State ||
                    BuildStateType.Upgrading == pair.Value.State)
                {
                    count++;
                }
            }
            return count;
        }

        #endregion

        #region 特殊建筑接口

        //使用建筑服务功能
        public ErrorCodes UseBuildService(CityManager _this,
                                          int areaId,
                                          int serviceId,
                                          List<int> param,
                                          ref UseBuildServiceResult result)
        {
            var build = GetBuildByArea(_this, areaId);
            if (build == null)
            {
                return ErrorCodes.Error_BuildNotFind;
            }
            return build.UseService(serviceId, param, ref result);
        }


        //获得合成屋额外的合成概率
        public int GetComposePro(CityManager _this)
        {
            var build = GetBuildByType(_this, (int) BuildingType.CompositeHouse);
            if (build == null)
            {
                return 0;
            }
            var basePro = build.TbBs.Param[0];
            var pets = build.GetPets();
            foreach (var pet in pets)
            {
                var petRef = BuildingBase.GetPetRef((int) BuildingType.CompositeHouse, build.TbBs, pet, 1, 10000);
                basePro += (petRef - 10000);
            }

            return basePro;
        }

        #endregion

        #region 经验等级相关

        public void CityAddExp(CityManager _this, int value)
        {
            //int oldLevel = Level;
            //var tbLvl = Table.GetLevelData(oldLevel);
            //if (tbLvl == null)
            //{
            //    Logger.Warn("City AddExp Error! level={0}",oldLevel);
            //    return;
            //}
            //if (tbLvl.UpNeedExp < 1)
            //{
            //    Logger.Warn("City AddExp Error! level={0}", oldLevel);
            //    return;
            //}
            //int oldExp = Exp + value;
            //while (oldExp >= tbLvl.UpNeedExp)
            //{
            //    oldLevel++;
            //    oldExp -= tbLvl.UpNeedExp;
            //    tbLvl = Table.GetLevelData(oldLevel);
            //}
            //Level = oldLevel;
            //Exp = oldExp;

            // 注掉CityAddExp实现
            //var oldlevel = _this.Level;
            //_this.mCharacter.mBag.GetFunctionImpl().ResChange(_this.mCharacter.mBag, eResourcesType.HomeExp, value);
            //_this.AddExp(value);

            //潜规则引导标记位
            /*
 			if (1 == oldlevel && 2 == _this.Level)
 	        {
 		        if (!_this.mCharacter.GetFlag(526))
 		        {
 			        _this.mCharacter.SetFlag(526);
 					_this.mCharacter.SetFlag(525,false);
 		        }
 	        }
			if (2 == oldlevel && 3 == _this.Level)
 	        {
 		        if (!_this.mCharacter.GetFlag(529))
 		        {
 			        _this.mCharacter.SetFlag(529);
 					_this.mCharacter.SetFlag(528,false);
 		        }
 	        }
			else if (3 == oldlevel && 4 == _this.Level)
			{
				if (!_this.mCharacter.GetFlag(532))
				{
					_this.mCharacter.SetFlag(532);
					_this.mCharacter.SetFlag(531, false);
				}
			}
			else if (5 == oldlevel && 6 == _this.Level)
			{
				if (!_this.mCharacter.GetFlag(535))
				{
					_this.mCharacter.SetFlag(535);
					_this.mCharacter.SetFlag(534, false);
				}
			}
			 * */
        }


        public int GetMaxLevel(CityManager _this)
        {
            return 100;
        }

        public int GetNeedExp(CityManager _this, int lvl)
        {
            var tbLvl = Table.GetLevelData(lvl);
            if (tbLvl == null)
            {
                Logger.Error("In GetNeedExp(). character id = {0},name = {1},lvl = {2}!", _this.mCharacter.mGuid,
                    _this.mCharacter.GetName(), lvl);
                return 10000;
            }
            return tbLvl.UpNeedExp;
        }

        #endregion

        #region 任务相关

        //任务数据
        public List<DBBuildMission> GetCityMissions(CityManager _this)
        {
            return _this.mDbData.Missions;
        }

        //获得农场的任务随机等级
        public int GetCityMissionLevel(CityManager _this)
        {
            return 1;
        }

        //重置所有任务
        public void RefreshMission(CityManager _this)
        {
            var build = GetBuildByType(_this, (int) BuildingType.Farm);
            if (build == null)
            {
                return;
            }
            var cc = build.TbBs.Param[4];
            for (var i = 0; i != cc; ++i)
            {
                if (i >= _this.mDbData.Missions.Count)
                {
                    if (i == 0)
                    {
                        var mis = new DBBuildMission();
                        _this.mDbData.Missions.Add(mis);
                        var role = _this.mCharacter.GetRole();
                        var misId = 0;
                        var itemId = 90108;
                        if (role == 1)
                        {
                            misId = 200;
                            itemId = 90105;
                        }
                        else if (role == 2)
                        {
                            misId = 180;
                            itemId = 90100;
                        }

                        ResetCityMission(_this, mis, misId, itemId, 8);
                    }
                    else
                    {
                        _this.mDbData.Missions.Add(new DBBuildMission());
                        ResetCityMission(_this, i);
                    }
                }
            }
        }

        //获得一个随机的任务ID
        public int GetRandomCityMission2(CityManager _this)
        {
            var mislevel = GetCityMissionLevel(_this);
            var tbOrder = Table.GetOrderUpdate(mislevel);
            if (tbOrder == null)
            {
                return -1;
            }
            var index = 0;
            foreach (var i in tbOrder.OrderID)
            {
                if (i == -1)
                {
                    break;
                }
                index++;
            }
            if (index < 1)
            {
                return -1;
            }
            return tbOrder.OrderID[MyRandom.Random(index)];
        }

        //新的获得随机任务的方法
        public int GetRandomCityMission(CityManager _this, ref int misValue)
        {
            _this.misList.Clear();
            var index = 0;
            foreach (var building in _this.Buildings)
            {
                var tbOu = building.Value.TbOu;
                if (tbOu == null)
                {
                    continue;
                }
                foreach (var i in tbOu.OrderID)
                {
                    if (i == -1)
                    {
                        break;
                    }
                    index++;
                    _this.misList[i] = tbOu;
                }
            }
            if (index < 1)
            {
                return -1;
            }
            var r = _this.misList.Random();
            misValue = r.Value.TotalValue;
            return r.Key;
        }

        //重置某个任务
        private ErrorCodes ResetCityMission(CityManager _this, int index)
        {
            if (index < 0 || _this.mDbData.Missions.Count <= index)
            {
                return ErrorCodes.Error_DataOverflow;
            }
            var mis = _this.mDbData.Missions[index];
            var refValue = 0;
            var misId = GetRandomCityMission(_this, ref refValue);
            ResetCityMission(_this, mis, misId, refValue);
            return ErrorCodes.OK;
        }

        //按照要求不随机掉落的重置一个任务
        private void ResetCityMission(CityManager _this, DBBuildMission mis, int missId, int itemId, int itemCount)
        {
            var tbMis = Table.GetOrderForm(missId);
            if (tbMis == null)
            {
                return;
            }
            var items = new Dictionary<int, int>();
            _this.mCharacter.DropMother(tbMis.Item, items);
            var count = items.Count;
            if (count < 1 || count > 5)
            {
                Logger.Warn("ResetCityMission id={0},count ={1}", missId, count);
                return;
            }
            //重置 任务属性
            mis.MissionId = missId;
            mis.State = (int) CityMissionState.Normal;
            var value = 0;
            mis.ItemIdList.Clear();
            mis.ItemCountList.Clear();
            mis.ItemIdList.Add(itemId);
            mis.ItemCountList.Add(itemCount);
            value += Table.GetItemBase(itemId).ItemValue*itemCount;
            items.Clear();
            mis.GiveItem = -1;
            if (tbMis.ExtraDropID > -1 && value > tbMis.ExtraRewardValue)
            {
                if (MyRandom.Random(10000) < tbMis.ExtraRewardProb)
                {
                    _this.mCharacter.DropSon(tbMis.ExtraDropID, items);
                    if (items.Count == 1)
                    {
                        value -= tbMis.ExtraRewardValue;
                        mis.GiveItem = items.First().Key;
                    }
                }
            }
            var moneyValue = MyRandom.Random(tbMis.RewardLess100, tbMis.RewardMore100);
            mis.GiveMoney = (int) (value*moneyValue/tbMis.MoneyValue);
            mis.GiveExp = (int) (value*(100 - moneyValue)/tbMis.ExpValue);
        }


        //重置一个任务数据
        private void ResetCityMission(CityManager _this, DBBuildMission mis, int missId, int misValue)
        {
            var tbMis = Table.GetOrderForm(missId);
            if (tbMis == null)
            {
                return;
            }
            var items = new Dictionary<int, int>();
            _this.mCharacter.DropMother(tbMis.Item, items);
            var count = items.Count;
            if (count < 1 || count > 5)
            {
                Logger.Warn("ResetCityMission id={0},count ={1}", missId, count);
                return;
            }
            //根据价值需求，随机调整物品数量
            var willValue = misValue; //_this.mCharacter.mBag.GetRes(eResourcesType.HomeLevel) * 2000 + 15000;
            willValue = willValue*MyRandom.Random(90, 130)/100; //总价值随机
            var lastCount = count;
            var lastValue = willValue;
            var keys = items.Keys.ToList();
            foreach (var i in keys)
            {
                var baseValue = lastValue/lastCount;
                var thisValue = MyRandom.Random((int) (baseValue*0.7), (int) (baseValue*1.3));
                var itemValue = Table.GetItemBase(i).ItemValue;
                var thisCount = thisValue/itemValue;
                if (thisCount < 1)
                {
                    thisCount = 1;
                }
                //item.Value = thisCount;
                items[i] = thisCount;
                lastValue = lastValue - thisCount*itemValue;
                lastCount--;
            }
            //foreach (var item in items)
            //{
            //    int baseValue = lastValue / lastCount;
            //    int thisValue = MyRandom.Random((int)(baseValue * 0.7), (int)(baseValue * 1.3));
            //    int itemValue = Table.GetItemBase(item.Key).ItemValue;
            //    int thisCount = thisValue / itemValue;
            //    if (thisCount < 1) thisCount = 1;
            //    //item.Value = thisCount;
            //    items[item.Key] = thisCount;
            //    lastValue = lastValue - thisCount * itemValue;
            //    lastCount--;
            //}
            //重置 任务属性
            mis.MissionId = missId;
            mis.State = (int) CityMissionState.Normal;
            var value = 0;
            mis.ItemIdList.Clear();
            mis.ItemCountList.Clear();
            foreach (var i in items)
            {
                mis.ItemIdList.Add(i.Key);
                mis.ItemCountList.Add(i.Value);
                value += Table.GetItemBase(i.Key).ItemValue*i.Value;
            }
            items.Clear();
            mis.GiveItem = -1;
            if (tbMis.ExtraDropID > -1 && value > tbMis.ExtraRewardValue)
            {
                if (MyRandom.Random(10000) < tbMis.ExtraRewardProb)
                {
                    _this.mCharacter.DropSon(tbMis.ExtraDropID, items);
                    if (items.Count == 1)
                    {
                        value -= tbMis.ExtraRewardValue;
                        mis.GiveItem = items.First().Key;
                    }
                }
            }
            var moneyValue = MyRandom.Random(tbMis.RewardLess100, tbMis.RewardMore100);
            mis.GiveMoney = (int) (value*moneyValue/tbMis.MoneyValue);
            mis.GiveExp = (int) (value*(100 - moneyValue)/tbMis.ExpValue);
        }

        //提交任务
        public ErrorCodes CommitMission(CityManager _this, int index, int misId)
        {
            if (index < 0 || _this.mDbData.Missions.Count <= index)
            {
                return ErrorCodes.Error_DataOverflow;
            }
            var mis = _this.mDbData.Missions[index];
            if (mis.MissionId != misId)
            {
                return ErrorCodes.Error_CityMissionNotFind;
            }
            if (mis.State != (int) CityMissionState.Normal)
            {
                return ErrorCodes.Error_CityMissionState;
            }
            //物品数量判断
            var itemIndex = 0;
            foreach (var i in mis.ItemIdList)
            {
                if (_this.mCharacter.mBag.GetItemCount(i) < mis.ItemCountList[itemIndex])
                {
                    return ErrorCodes.ItemNotEnough;
                }
                itemIndex ++;
            }
            if (mis.GiveItem != -1)
            {
                var resultCodes = _this.mCharacter.mBag.CheckAddItem(mis.GiveItem, 1);
                if (resultCodes != ErrorCodes.OK)
                {
                    return resultCodes;
                }
            }
            //消耗
            itemIndex = 0;
            foreach (var i in mis.ItemIdList)
            {
                _this.mCharacter.mBag.DeleteItem(i, mis.ItemCountList[itemIndex], eDeleteItemType.CityMission);
                itemIndex++;
            }
            //奖励
            _this.mCharacter.mBag.AddRes(eResourcesType.GoldRes, mis.GiveMoney, eCreateItemType.CityMission);
            //CityAddExp(_this, mis.GiveExp);
            if (mis.GiveItem != -1)
            {
                _this.mCharacter.mBag.AddItem(mis.GiveItem, 1, eCreateItemType.CityMission);
                //mCharacter.mBag.AddItemOrMail(50, new Dictionary<int, int>() { { mis.GiveItem, 1 } }, new List<ItemBaseData>(), eCreateItemType.CityMission);
            }
            mis.State = (int) CityMissionState.Wait;
            _this.mCharacter.AddExData((int) eExdataDefine.e335, 1);
            _this.mCharacter.AddExData((int) eExdataDefine.e412, 1);
            var build = GetBuildByType(_this, (int) BuildingType.Farm);
            var refleshCd = 30;
            if (build != null)
            {
                refleshCd = build.TbBs.Param[3];
            }
            mis.RefreshTime = DateTime.Now.AddMinutes(refleshCd).ToBinary();
            _this.MarkDbDirty();

            //潜规则引导标记位
            if (!_this.mCharacter.GetFlag(541))
            {
                _this.mCharacter.SetFlag(541);
                if (!_this.mCharacter.GetFlag(525))
                {
                    _this.mCharacter.SetFlag(525);
                    _this.mCharacter.SetFlag(524, false);
                }
            }

            return ErrorCodes.OK;
        }

        //放弃任务
        public ErrorCodes DropMission(CityManager _this, DBBuildMission mis)
        {
            //if (index < 0 || mDbData.Missions.Count <= index)
            //{
            //    return ErrorCodes.Error_DataOverflow;
            //}
            //var mis = mDbData.Missions[index];
            //if (mis.MissionId != misId)
            //{
            //    return ErrorCodes.Error_CityMissionNotFind;
            //}
            if (mis.State != (int) CityMissionState.Normal)
            {
                return ErrorCodes.Error_CityMissionState;
            }
            mis.State = (int) CityMissionState.Wait;
            var build = GetBuildByType(_this, (int) BuildingType.Farm);
            if (build == null)
            {
                return ErrorCodes.Error_BuildNotFind;
            }

            var vipLevel = _this.mCharacter.mBag.GetRes(eResourcesType.VipLevel);
            var tbVip = Table.GetVIP(vipLevel);
            var ex325 = _this.mCharacter.GetExData(325);

            //潜规则引导标记位
            if (!_this.mCharacter.GetFlag(541))
            {
                _this.mCharacter.SetFlag(541);
                if (!_this.mCharacter.GetFlag(525))
                {
                    _this.mCharacter.SetFlag(525);
                    _this.mCharacter.SetFlag(524, false);
                }
            }

            if (ex325 + tbVip.FarmAddRefleshCount > 0)
            {
                var refValue = 0;
                var misId = GetRandomCityMission(_this, ref refValue);
                ResetCityMission(_this, mis, misId, refValue);

                //ResetCityMission(_this, mis, GetRandomCityMission(_this));
                //mCharacter.AddExData(325, 1);
                _this.mCharacter.SetExData(325, ex325 - 1);
                _this.MarkDbDirty();


                return ErrorCodes.OK;
            }
            mis.RefreshTime = DateTime.Now.AddMinutes(build.TbBs.Param[3]).ToBinary();
            _this.MarkDbDirty();
            return ErrorCodes.Error_CityMissionFreeCd;
        }

        //直接刷新任务
        [Updateable("cityMgr")]
        public static int OrderRefreshCost = Table.GetServerConfig(571).ToInt();

        public ErrorCodes BuyRefreshMission(CityManager _this, int index, int costClient)
        {
            if (index < 0 || _this.mDbData.Missions.Count <= index)
            {
                return ErrorCodes.Error_DataOverflow;
            }
            var mis = _this.mDbData.Missions[index];
            if (mis.State == (int) CityMissionState.Normal)
            {
                return ErrorCodes.Error_CityMissionState;
            }

            var dif = DateTime.FromBinary(mis.RefreshTime) - DateTime.Now;
            var costServer = (int) Math.Ceiling((float) dif.TotalSeconds/(60.0f*5))*OrderRefreshCost;
            if (costServer > costClient)
            {
                return ErrorCodes.Error_CityMissionTime;
            }
            if (costServer > 0)
            {
                if (_this.mCharacter.mBag.GetRes(eResourcesType.DiamondRes) < costServer)
                {
                    return ErrorCodes.DiamondNotEnough;
                }
                _this.mCharacter.mBag.DelRes(eResourcesType.DiamondRes, costServer, eDeleteItemType.RefreshCityMission);
            }

            var refValue = 0;
            var misId = GetRandomCityMission(_this, ref refValue);
            ResetCityMission(_this, mis, misId, refValue);

            //潜规则引导标记位
            if (!_this.mCharacter.GetFlag(541))
            {
                _this.mCharacter.SetFlag(541);
                if (!_this.mCharacter.GetFlag(525))
                {
                    _this.mCharacter.SetFlag(525);
                    _this.mCharacter.SetFlag(524, false);
                }
            }
            //ResetCityMission(_this, mis, GetRandomCityMission(_this));
            return ErrorCodes.OK;
        }

        //获取任务数据
        public DBBuildMission GetMission(CityManager _this, int misIndex)
        {
            if (misIndex < 0 || _this.mDbData.Missions.Count <= misIndex)
            {
                return null;
            }
            return _this.mDbData.Missions[misIndex];
        }

        //检查是否有任务需要重置了
        public ErrorCodes CheckMissionRefresh(CityManager _this)
        {
            var result = ErrorCodes.Unknow;
            foreach (var mis in _this.mDbData.Missions)
            {
                if (mis.State != (int) CityMissionState.Wait)
                {
                    continue;
                }
                if (DateTime.FromBinary(mis.RefreshTime) > DateTime.Now)
                {
                    continue;
                }

                var refValue = 0;
                var misId = GetRandomCityMission(_this, ref refValue);
                ResetCityMission(_this, mis, misId, refValue);
                //ResetCityMission(_this, mis, GetRandomCityMission(_this));
                result = ErrorCodes.OK;
            }
            return result;
        }

        #endregion

        #region 团购相关

        private void InitDbShop(CityManager _this)
        {
            _this.mDbData.Shop = new DBCityGroupShop();
            _this.mDbData.Shop.CacheItems.Add(new Int64Array());
            _this.mDbData.Shop.CacheItems.Add(new Int64Array());
            _this.mDbData.Shop.CacheItems.Add(new Int64Array());
            _this.mDbData.Shop.CacheItems.Add(new Int64Array());
        }

        //获取缓存
        public List<Int64Array> GetItems(CityManager _this, List<int> types)
        {
            var items = _this.mDbData.Shop.CacheItems;
            var ret = new List<Int64Array>();
            foreach (var type in types)
            {
                ret.Add(items[type]);
            }
            return ret;
        }

        //获取竞标历史
        public List<long> GetAllCachedItems(CityManager _this)
        {
            var ret = new List<long>();
            foreach (var cacheItem in _this.mDbData.Shop.CacheItems)
            {
                ret.AddRange(cacheItem.Items);
            }
            return ret;
        }

        //根据类型记录缓存
        public void CacheItems(CityManager _this, SSApplyGroupShopItemsOutMessage msg)
        {
            var response = msg.Response;
            if (!response.Dirty)
            {
                return;
            }

            var cached = _this.mDbData.Shop.CacheItems;
            var types = msg.Request.Types.Items;
            var lists = response.Items.Lists;
            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                var items = cached[type].Items;
                items.Clear();
                items.AddRange(lists[i].Items.Select(item => item.Guid));
            }
        }

        //
        public bool IsContainsItemid(CityManager _this, long itemId)
        {
            return _this.mDbData.Shop.CacheItems.Any(cacheItem => cacheItem.Items.Contains(itemId));
        }

        //获取已结束的竞标历史
        public List<long> GetHistoryItems(CityManager _this)
        {
            return _this.mDbData.Shop.OldItem;
        }

        //根据类型记录缓存
        public void CacheHistoryItems(CityManager _this, GroupShopItemAllForServer data)
        {
            if (data.Dirty)
            {
                var history = data.Items.Lists[0].Items;
                var oldItems = _this.mDbData.Shop.OldItem;
                oldItems.Clear();
                oldItems.AddRange(history.Select(item => item.Guid));
            }

            var expired = data.Expired;
            if (expired.Count > 0)
            {
                var buyedItems = _this.mDbData.Shop.BuyItem;
                buyedItems.RemoveAll(i => expired.Contains(i));
            }
        }

        //获取我买过的物品
        public List<long> GetBuyedItems(CityManager _this)
        {
            return _this.mDbData.Shop.BuyItem;
        }

        //
        public void AddToBuyItems(CityManager _this, long id)
        {
            var buyItem = _this.mDbData.Shop.BuyItem;
            if (!buyItem.Contains(id))
            {
                buyItem.Add(id);
            }
        }

        //购买成功
        public void BuyItemSuccess(CityManager _this, long itemGuid, int Count)
        {
            if (!_this.mDbData.Shop.MyItem.Contains(itemGuid))
            {
                _this.mDbData.Shop.MyItem.Add(itemGuid);
            }
        }

        #endregion

        #region 节点相关

        public void NetDirtyHandle(CityManager _this)
        {
            var msg = new BuildingList();
            foreach (var build in _this.Children)
            {
                if (build.NetDirty) //脏任务
                {
                    var tempBuild = (BuildingBase) build;
                    msg.Data.Add(tempBuild.GetBuildingData());
                }
            }
            _this.mCharacter.Proxy.SyncCityBuildingData(msg);
            //CoroutineFactory.NewCoroutine(NotifyCitySceneDataCoroutine, _this, msg).MoveNext(); 不要这功能了
        }

        private IEnumerator NotifyCitySceneDataCoroutine(Coroutine coroutine, CityManager _this, BuildingList data)
        {
            var msg = LogicServer.Instance.SceneAgent.NotifyScenePlayerCityData(_this.mCharacter.mGuid, data);
            yield return msg.SendAndWaitUntilDone(coroutine);
        }

        //给宠物经验
        public void GivePetExp(CityManager _this)
        {
            foreach (var pair in _this.BuildingsByArea)
            {
                pair.Value.GivePetExp(_this.LastPetExpTime);
            }
            _this.LastPetExpTime = DateTime.Now;
            _this.MarkDbDirty();
        }

        //计算家园提供的属性
        public void RefreshAttr(CityManager _this)
        {
            _this.mAttrs.Clear();
            _this.AttrFlag = false;
            foreach (var pair in _this.BuildingsByArea)
            {
                var build = pair.Value;
                if (build.TbBuild == null)
                {
                    continue;
                }
                if (build.TbBuild.Type != 6)
                {
                    continue;
                }
                //var buildRef = BuildingBase.GetBSParamByIndex(6, build.TbBs, build.GetPets(), 1);
                for (var i = 0; i != 5; ++i)
                {
                    var tbStatue = Table.GetStatue(pair.Value.GetExdata32(i));
                    //基础属性
                    var index = 0;
                    foreach (var attrId in tbStatue.PropID)
                    {
                        if (attrId != -1)
                        {
                            _this.mAttrs.modifyValue(attrId, tbStatue.propValue[index]);
                        }
                        index++;
                    }
                    //继承属性
                    index = 0;
                    var petId = build.PetList[i];
                    if (petId == -1)
                    {
                        continue;
                    }
                    var pet = _this.mCharacter.GetPet(petId);
                    if (pet == null)
                    {
                        continue;
                    }
                    var buildRef = BuildingBase.GetPetRef(build.TbBuild.Type, build.TbBs, pet, 1, 10000);
                    //var buildRef = BuildingBase.GetBSParamByIndex(6, build.TbBs, build.GetPets(), 1);
                    if (pet.GetState() != (int) PetStateType.Building)
                    {
                        Logger.Warn("City RefreshAttr petId={0},State={1}", petId, pet.GetState());
                        continue;
                    }
                    foreach (var attrId in tbStatue.FuseID)
                    {
                        if (attrId != -1)
                        {
                            var aId = attrId;
                            if (aId == 5 && _this.mCharacter.GetAttackType() == 1)
                            {
                                aId = 7;
                            }
                            else if (aId == 6 && _this.mCharacter.GetAttackType() == 1)
                            {
                                aId = 8;
                            }
                            var value = pet.GetPetAttribut((eAttributeType) aId);
                            if (value > 0)
                            {
                                var f = (float) value*(tbStatue.FuseValue[0] + buildRef - 10000)/10000;
                                _this.mAttrs.modifyValue(aId, (int) f);
                            }
                        }
                        index++;
                    }
                }
            }
        }

        //设置属性脏标记
        public void SetAttrFlag(CityManager _this)
        {
            _this.AttrFlag = true;
        }

        //获得家园提供的经验
        public Dictionary<int, int> GetRefreshAttr(CityManager _this)
        {
            if (_this.AttrFlag)
            {
                RefreshAttr(_this);
            }
            return _this.mAttrs;
        }

        public void OnDestroy(CityManager _this)
        {
            foreach (var pair in _this.BuildingsByArea)
            {
                pair.Value.OnDestroy();
            }
        }

        #endregion
    }

    public class CityManager : NodeBase, LevelExp
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static ICityManager mStaticImpl;

        static CityManager()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (CityManager), typeof (CityManagerDefaultImpl),
                o => { mStaticImpl = (ICityManager) o; });
        }

        public bool AttrFlag;
        public Dictionary<int, BuildingBase> Buildings = new Dictionary<int, BuildingBase>();
        public Dictionary<int, BuildingBase> BuildingsByArea = new Dictionary<int, BuildingBase>();
        public readonly Dictionary<int, int> mAttrs = new Dictionary<int, int>();
        public CharacterController mCharacter; //所在角色
        //public CityMission Mission = new CityMission();
        public DBBuild_List mDbData;
        public readonly Dictionary<int, OrderUpdateRecord> misList = new Dictionary<int, OrderUpdateRecord>();

        public DateTime LastPetExpTime
        {
            get { return DateTime.FromBinary(mDbData.LastPetExpTime); }
            set { mDbData.LastPetExpTime = value.ToBinary(); }
        }

        public int Level
        {
            get { return mCharacter.mBag.GetRes(eResourcesType.HomeLevel); }
            set { mCharacter.mBag.SetRes(eResourcesType.HomeLevel, value); }
        }

        public int Exp
        {
            get { return mCharacter.mBag.GetRes(eResourcesType.HomeExp); }
            set { mCharacter.mBag.SetRes(eResourcesType.HomeExp, value); }
        }

        #region   初始化

        public DBBuild_List InitByBase(CharacterController character)
        {
            return mStaticImpl.InitByBase(this, character);
        }

        public void InitByDB(CharacterController character, DBBuild_List builds)
        {
            mStaticImpl.InitByDB(this, character, builds);
        }

        #endregion

        #region 基础接口

        //城堡等级
        public int CityLevel()
        {
            return mStaticImpl.HomeLevel(this);
        }

        //获得某个建筑类型的数量
        public int GetBuildCount(int type)
        {
            return mStaticImpl.GetBuildCount(this, type);
        }

        //靠Type获取建筑
        public BuildingBase GetBuildByType(int type)
        {
            return mStaticImpl.GetBuildByType(this, type);
        }

        //靠Area获取建筑
        public BuildingBase GetBuildByArea(int areaId)
        {
            return mStaticImpl.GetBuildByArea(this, areaId);
        }

        //靠GUID获取建筑
        public BuildingBase GetBuildByGuid(int guid)
        {
            return mStaticImpl.GetBuildByGuid(this, guid);
        }

        //用AreaId获取建筑
        public BuildingBase GetBuildByAreaId(int idx)
        {
            return mStaticImpl.GetBuildByAreaId(this, idx);
        }

        //尝试建造建筑
        public ErrorCodes CreateBuild(int tableId, int areaId)
        {
            return mStaticImpl.CreateBuild(this, tableId, areaId);
        }

        //摧毁
        public ErrorCodes DestroyBuild(int areaId)
        {
            return mStaticImpl.DestroyBuild(this, areaId);
        }

        //升级建筑
        public ErrorCodes UpgradeBuild(int areaId)
        {
            return mStaticImpl.UpgradeBuild(this, areaId);
        }

        //时间改变
        public DateTime ChangeTime(DateTime beforeTime, int before, int after)
        {
            return mStaticImpl.ChangeTime(this, beforeTime, before, after);
        }

        //获得当前正在建造或者升级的建筑个数
        public int GetCurrentUpgradingOrBuildingNumber()
        {
            return mStaticImpl.GetCurrentUpgradingOrBuildingNumber(this);
        }

        #endregion

        #region 特殊建筑接口

        //使用建筑服务功能
        public ErrorCodes UseBuildService(int areaId, int serviceId, List<int> param, ref UseBuildServiceResult result)
        {
            return mStaticImpl.UseBuildService(this, areaId, serviceId, param, ref result);
        }


        //获得合成屋额外的合成概率
        public int GetComposePro()
        {
            return mStaticImpl.GetComposePro(this);
        }

        #endregion

        #region 经验等级相关

        public void CityAddExp(int value)
        {
            mStaticImpl.CityAddExp(this, value);
        }


        public int GetMaxLevel()
        {
            return mStaticImpl.GetMaxLevel(this);
        }

        public int GetNeedExp(int lvl)
        {
            return mStaticImpl.GetNeedExp(this, lvl);
        }

        #endregion

        #region 任务相关

        //任务数据
        public List<DBBuildMission> GetCityMissions()
        {
            return mDbData.Missions;
        }

        //获得农场的任务随机等级
        public int GetCityMissionLevel()
        {
            return mStaticImpl.GetCityMissionLevel(this);
        }

        //重置所有任务
        public void RefreshMission()
        {
            mStaticImpl.RefreshMission(this);
        }

        //获得一个随机的任务ID
        public int GetRandomCityMission2()
        {
            return mStaticImpl.GetRandomCityMission2(this);
        }

        //提交任务
        public ErrorCodes CommitMission(int index, int misId)
        {
            return mStaticImpl.CommitMission(this, index, misId);
        }

        //放弃任务
        public ErrorCodes DropMission(DBBuildMission mis)
        {
            return mStaticImpl.DropMission(this, mis);
        }

        //直接刷新任务

        public static int OrderRefreshCost = Table.GetServerConfig(571).ToInt();

        public ErrorCodes BuyRefreshMission(int index, int costClient)
        {
            return mStaticImpl.BuyRefreshMission(this, index, costClient);
        }

        //获取任务数据
        public DBBuildMission GetMission(int misIndex)
        {
            return mStaticImpl.GetMission(this, misIndex);
        }

        //检查是否有任务需要重置了
        public ErrorCodes CheckMissionRefresh()
        {
            return mStaticImpl.CheckMissionRefresh(this);
        }

        #endregion

        #region 团购相关

        //获取缓存
        public List<Int64Array> GetItems(List<int> types)
        {
            return mStaticImpl.GetItems(this, types);
        }

        //获取竞标历史
        public List<long> GetAllCachedItems()
        {
            return mStaticImpl.GetAllCachedItems(this);
        }

        //根据类型记录缓存
        public void CacheItems(SSApplyGroupShopItemsOutMessage msg)
        {
            mStaticImpl.CacheItems(this, msg);
        }

        //
        public bool IsContainsItemid(long itemId)
        {
            return mStaticImpl.IsContainsItemid(this, itemId);
        }

        //获取已结束的竞标历史
        public List<long> GetHistoryItems()
        {
            return mStaticImpl.GetHistoryItems(this);
        }

        //根据类型记录缓存
        public void CacheHistoryItems(GroupShopItemAllForServer data)
        {
            mStaticImpl.CacheHistoryItems(this, data);
        }

        //获取我买过的物品
        public List<long> GetBuyedItems()
        {
            return mStaticImpl.GetBuyedItems(this);
        }

        //
        public void AddToBuyItems(long id)
        {
            mStaticImpl.AddToBuyItems(this, id);
        }

        //购买成功
        public void BuyItemSuccess(long itemGuid, int Count)
        {
            mStaticImpl.BuyItemSuccess(this, itemGuid, Count);
        }

        #endregion

        #region 节点相关

        public override IEnumerable<NodeBase> Children
        {
            get { return BuildingsByArea.Values; }
        }

        public override void NetDirtyHandle()
        {
            mStaticImpl.NetDirtyHandle(this);
        }

        //给宠物经验
        public void GivePetExp()
        {
            mStaticImpl.GivePetExp(this);
        }

        //计算家园提供的属性
        public void RefreshAttr()
        {
            mStaticImpl.RefreshAttr(this);
        }

        //设置属性脏标记
        public void SetAttrFlag()
        {
            mStaticImpl.SetAttrFlag(this);
        }

        //获得家园提供的经验
        public Dictionary<int, int> GetRefreshAttr()
        {
            return mStaticImpl.GetRefreshAttr(this);
        }

        public void OnDestroy()
        {
            mStaticImpl.OnDestroy(this);
        }

        #endregion
    }
}