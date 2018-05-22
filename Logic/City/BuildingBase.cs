#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Logic
{

    #region 接口类

    public interface IBuildingBase
    {
        void AddPetExdata64(BuildingBase _this, Int64 nValue);
        void AddStatueLevel(BuildingBase _this, StatueRecord tbStatue, int index);
        ErrorCodes AssignPet(BuildingBase _this, int petId);
        ErrorCodes AssignPet(BuildingBase _this, int index, int petId);
        ErrorCodes AssignPetIndex(BuildingBase _this, int index, int petId);
        int GetBSParamByIndex(int buildType, BuildingServiceRecord tbBS, List<PetItem> petItems, int index);
        BuildingData GetBuildingData(BuildingBase _this);
        DateTime GetIndexTime(BuildingBase _this, int index);
        bool GetIsQuality(BuildingBase _this, ItemBaseData item);
        Int64 GetPetExdata64(BuildingBase _this, int nIndex);
        int GetPetIndex(BuildingBase _this, int petId);
        DateTime GetPetIndexTime(BuildingBase _this, int index);
        int GetPetRef(int buildType, BuildingServiceRecord tbBS, PetItem pet, int paramIndex, int oldValue);
        List<PetItem> GetPets(BuildingBase _this);
        DateTime GetPetWorkTime(BuildingBase _this, int index);
        int GetSpeed(BuildingBase _this, BuildingServiceRecord tbBS);
        void GivePetExp(BuildingBase _this, DateTime lastTime);
        void GivePetExpByIndex(BuildingBase _this, int index, DateTime lastTime);

        void GiveReward(BuildingBase _this,
                        int exdataId,
                        int addValue,
                        BuildingServiceRecord TbBS,
                        int maildId,
                        eCreateItemType because,
                        bool rewardIsEmpty = true);

        bool GiveStatueExp(BuildingBase _this, StatueRecord tbStatue, int index, int addExp);
        void Init(BuildingBase _this, CharacterController character, DBBuild_One dbdata);
        void Init(BuildingBase _this, CharacterController character, int guid, BuildingRecord tbBuild);
        bool IsDoPlay(BuildingBase _this, int index);
        bool IsHaveService(BuildingBase _this, int serviceId);
        void LineState(BuildingBase _this, int index, SailType sailType);
        void OnDestroy(BuildingBase _this);
        void OnPetChanged(BuildingBase _this, List<PetItem> oldPetList);
        void PointState(BuildingBase _this, int index, SailType sailType);
        void Reset(BuildingBase _this, int guid, int areaId, BuildingRecord tbBuild, BuildStateType type);
        void ResetExdata(BuildingBase _this);
        ErrorCodes Service1_Mine(BuildingBase _this, BuildingServiceRecord tbBS, ref UseBuildServiceResult result);

        ErrorCodes Service11_Wishing(BuildingBase _this,
                                     BuildingServiceRecord tbBS,
                                     List<int> param,
                                     ref UseBuildServiceResult resultValue);

        ErrorCodes Service12_Sail(BuildingBase _this,
                                  BuildingServiceRecord tbBS,
                                  List<int> param,
                                  ref UseBuildServiceResult result);

        ErrorCodes Service2_Plant(BuildingBase _this,
                                  BuildingServiceRecord tbBS,
                                  List<int> param,
                                  ref UseBuildServiceResult result);

        ErrorCodes Service3_Astrology(BuildingBase _this,
                                      BuildingServiceRecord tbBS,
                                      List<int> param,
                                      ref UseBuildServiceResult resultValue);

        ErrorCodes Service5_Hatch(BuildingBase _this,
                                  BuildingServiceRecord tbBS,
                                  List<int> param,
                                  ref UseBuildServiceResult result);

        ErrorCodes Service6_ArenaTemple(BuildingBase _this,
                                        BuildingServiceRecord tbBS,
                                        List<int> param,
                                        ref UseBuildServiceResult result);

        ErrorCodes Service8_Casting(BuildingBase _this,
                                    BuildingServiceRecord tbBS,
                                    List<int> param,
                                    ref UseBuildServiceResult result);

        void SetPetExdata64(BuildingBase _this, int nIndex, Int64 nValue);
        void SetPetIndexTime(BuildingBase _this, int nIndex, DateTime nValue);
        ErrorCodes Speedup(BuildingBase _this);
        void StartTrigger(BuildingBase _this, DateTime dateTime);
        ErrorCodes TakeBackPet(BuildingBase _this, int petId);
        ErrorCodes TakeBackPet(BuildingBase _this, int index, int petId);
        void TimeOver(BuildingBase _this);
        void UpdataExdata(BuildingBase _this);
        ErrorCodes Upgrade(BuildingBase _this);
        void UpgradeOver(BuildingBase _this);
        ErrorCodes UseService(BuildingBase _this, int serviceId, List<int> param, ref UseBuildServiceResult result);
    }

    #endregion

    public enum SailType
    {
        DisPlay = 0, //不可航行
        CanPlay = 10, //可航行
        DoPlay = 2, //正在航行
        OverPlay = 3, //正在航行
        CanDoPlay = 12 //正在航行,可进行下次航行
    }

    public class BuildingBaseDefaultImpl : IBuildingBase
    {
        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [Updateable("Building")]       
        public const int SmithyBuildingFirstId = 80;
        [Updateable("Building")]
        public static int SmithyFurnaceMaxIndex = 4;
        
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);

        public void Init(BuildingBase _this, CharacterController character, DBBuild_One dbdata)
        {
            _this.mCharacter = character;
            _this.mDbData = dbdata;
            UpdataExdata(_this);

            var tbBuilding = Table.GetBuilding(SmithyBuildingFirstId);
            while (tbBuilding.NextId >= 0)
            {
                tbBuilding = Table.GetBuilding(tbBuilding.NextId);
            }
            var tbBuildingService = Table.GetBuildingService(tbBuilding.ServiceId);
            SmithyFurnaceMaxIndex = tbBuildingService.Param[2] - 1;
        }

        public void Init(BuildingBase _this, CharacterController character, int guid, BuildingRecord tbBuild)
        {
            _this.mCharacter = character;
            _this.mDbData = new DBBuild_One();
            _this.mDbData.Guid = guid;
            _this.TypeId = tbBuild.Id;
            _this.TbBuild = tbBuild;
            if (_this.TbBuild != null)
            {
                if (_this.TbBuild.ServiceId != -1)
                {
                    _this.TbBs = Table.GetBuildingService(_this.TbBuild.ServiceId);
                }
                if (_this.TbBuild.OrderRefleshRule != -1)
                {
                    _this.TbOu = Table.GetOrderUpdate(_this.TbBuild.OrderRefleshRule);
                }
            }

            ResetExdata(_this);
            if (tbBuild.NeedMinutes <= 0)
            {
                _this.State = BuildStateType.Idle;
            }
            else
            {
                _this.State = BuildStateType.Building;
                _this.mDbData.StateOverTime = DateTime.Now.AddMinutes(tbBuild.NeedMinutes).ToBinary();
                _this.StartTrigger(_this.StateOverTime);
            }
        }

        public BuildingData GetBuildingData(BuildingBase _this)
        {
            var data = new BuildingData
            {
                TypeId = _this.TypeId,
                AreaId = _this.AreaId,
                Guid = _this.Guid,
                State = _this.mDbData.State
            };
            data.Exdata.AddRange(_this.Exdata32);
            data.PetList.AddRange(_this.PetList);
            data.Exdata64.AddRange(_this.Exdata64);
            data.PetTime.AddRange(_this.PetTime);
            data.OverTime = _this.StateOverTime.ToBinary();
            return data;
        }

        #region 农场

        public ErrorCodes Service2_Plant(BuildingBase _this,
                                         BuildingServiceRecord tbBS,
                                         List<int> param,
                                         ref UseBuildServiceResult result)
        {
            if (param.Count < 1)
            {
                return ErrorCodes.ParamError;
            }
            switch (param[0])
            {
                case 0:
                    //种植
                {
                    if (param.Count < 3)
                    {
                        return ErrorCodes.ParamError;
                    }
                    var index = param[1];
                    if (index < 0 || index >= _this.Exdata32.Count)
                    {
                        return ErrorCodes.Error_DataOverflow;
                    }
                    var oldValue = _this.GetExdata32(index);
                    if (oldValue != -1)
                    {
                        return ErrorCodes.Error_AlreadyHaveSeed;
                    }

                    var seedId = param[2];
                    var tbPlant = Table.GetPlant(seedId);
                    if (tbPlant == null)
                    {
                        return ErrorCodes.Error_ItemID;
                    }

                    if (_this.mCharacter.mBag.GetItemCount(tbPlant.PlantItemID) < 1)
                    {
                        return ErrorCodes.ItemNotEnough;
                    }

                    if (tbPlant.PlantLevel > tbBS.Param[1])
                    {
                        return ErrorCodes.Error_NeedFarmLevelMore; //特长不会修改此参数:种植等级
                    }
                    _this.mCharacter.mBag.DeleteItem(tbPlant.PlantItemID, 1, eDeleteItemType.Plant0);
                    _this.SetExdata32(index, seedId);
                    var needTime = tbPlant.MatureCycle*60; //按秒为最小单位去加速
                    var pets = GetPets(_this);
                    if (pets.Count > 0)
                    {
                        var timeRef = GetPlantNeedTime(tbPlant, pets);
                        needTime = needTime*(timeRef + 10000)/10000;
                    }
                    _this.SetExdata64(index, DateTime.Now.AddSeconds(needTime).ToBinary());
                    _this.mCharacter.AddExData((int) eExdataDefine.e260, 1);
                    _this.MarkDbDirty();

                    //潜规则引导标记位 种一个就置标记位
                    if (!_this.mCharacter.GetFlag(543))
                    {
                        _this.mCharacter.SetFlag(543);
                        if (!_this.mCharacter.GetFlag(524))
                        {
                            _this.mCharacter.SetFlag(524);
                            _this.mCharacter.SetFlag(523, false);
                        }
                    }
                }
                    break;
                case 1:
                    //摘取
                {
                    if (param.Count < 2)
                    {
                        return ErrorCodes.ParamError;
                    }
                    var index = param[1];
                    if (index < 0 || index >= _this.Exdata32.Count)
                    {
                        return ErrorCodes.Error_DataOverflow;
                    }
                    var seedId = _this.GetExdata32(index);
                    if (seedId == -1)
                    {
                        return ErrorCodes.Error_NotFindSeed;
                    }
                    var overTime = GetIndexTime(_this, index);
                    if (overTime > DateTime.Now)
                    {
                        return ErrorCodes.Error_SeedTimeNotOver;
                    }
                    var tbPlant = Table.GetPlant(seedId);
                    var hMinCount = tbPlant.HarvestCount[0];
                    var hMaxCount = tbPlant.HarvestCount[1];
                    if (tbPlant.HarvestItemID == -1)
                    {
                        var dropId = tbPlant.ExtraRandomDrop;
                        _this.itemReward.Clear();
                        _this.mCharacter.DropMother(dropId, _this.itemReward);
                    }
                    else
                    {
                        if (hMinCount > 0 && hMaxCount >= hMinCount)
                        {
                            var count = MyRandom.Random(hMinCount, hMaxCount);
                            var pets = GetPets(_this);
                            if (pets.Count > 0)
                            {
                                count += GetPlantHarvestAdd(tbPlant, pets);
                            }
                            _this.itemReward.Clear();
                            _this.itemReward.modifyValue(tbPlant.HarvestItemID, count);
                        }
                    }

                    if (_this.itemReward.Count > 0)
                    {
                        foreach (var i in _this.itemReward)
                        {
                            result.Data32.Add(i.Key);
                            result.Data32.Add(i.Value);
                        }
                        GiveReward(_this, 403, 1, tbBS, 112, eCreateItemType.Farm, false);
                    }

                    _this.SetExdata32(index, -1);
                    _this.SetExdata64(index, 0);
                    //_this.mCharacter.mCity.CityAddExp(tbPlant.GetHomeExp);
                    _this.mCharacter.AddExData((int) eExdataDefine.e262, 1);
                    _this.mCharacter.AddExData((int) eExdataDefine.e413, 1);
                    _this.MarkDbDirty();

                    //潜规则引导标记位 收一个就置标记位
                    if (!_this.mCharacter.GetFlag(542))
                    {
                        _this.mCharacter.SetFlag(542);
                        if (!_this.mCharacter.GetFlag(523))
                        {
                            _this.mCharacter.SetFlag(523);
                            _this.mCharacter.SetFlag(522, false);
                        }
                    }
                }
                    break;
                case 2:
                    //施肥
                {
                    if (param.Count < 3)
                    {
                        return ErrorCodes.ParamError;
                    }
                    var index = param[1];
                    if (index < 0 || index >= _this.Exdata32.Count)
                    {
                        return ErrorCodes.Error_DataOverflow;
                    }
                    var seedId = _this.GetExdata32(index);
                    if (seedId == -1)
                    {
                        return ErrorCodes.Error_NotFindSeed;
                    }
                    var overTime = GetIndexTime(_this, index);
                    if (overTime < DateTime.Now)
                    {
                        return ErrorCodes.Error_FarmNotAddSpeed;
                    }
                    var itemId = param[2];
                    var tbItem = Table.GetItemBase(itemId);
                    if (tbItem == null)
                    {
                        return ErrorCodes.Error_ItemID;
                    }
                    if (tbItem.Type != 91000)
                    {
                        return ErrorCodes.Error_ItemNot91000;
                    }
                    if (_this.mCharacter.mBag.GetItemCount(itemId) < 1)
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                    _this.mCharacter.mBag.DeleteItem(itemId, 1, eDeleteItemType.Plant2);
                    if (tbItem.Exdata[0] <= 0)
                    {
                        _this.SetExdata64(index, DateTime.Now.ToBinary());
                        result.Data32.Add(1);
                    }
                    else
                    {
                        _this.SetExdata64(index, overTime.AddMinutes((-tbItem.Exdata[0])).ToBinary());
                        result.Data32.Add(0);
                    }
                    _this.mCharacter.AddExData((int) eExdataDefine.e261, 1);
                    _this.MarkDbDirty();
                }
                    break;

                case 3:
                    //铲除
                {
                    if (param.Count < 2)
                    {
                        return ErrorCodes.ParamError;
                    }
                    var index = param[1];
                    if (index < 0 || index >= _this.Exdata32.Count)
                    {
                        return ErrorCodes.Error_DataOverflow;
                    }
                    var seedId = _this.GetExdata32(index);
                    if (seedId == -1)
                    {
                        return ErrorCodes.Error_NotFindSeed;
                    }
                    _this.SetExdata32(index, -1);
                    _this.SetExdata64(index, 0);
                    _this.mCharacter.AddExData((int) eExdataDefine.e263, 1);
                    _this.MarkDbDirty();
                }
                    break;
            }
            return ErrorCodes.OK;
        }

        #endregion

        #region 占星台

        public ErrorCodes Service3_Astrology(BuildingBase _this,
                                             BuildingServiceRecord tbBS,
                                             List<int> param,
                                             ref UseBuildServiceResult resultValue)
        {
            if (param.Count > 1)
            {
                return ErrorCodes.Unknow;
            }
            var result = ErrorCodes.OK;
            var Character = _this.mCharacter;
            //List<DropMotherRecord> drops = new List<DropMotherRecord>();
            switch (param[0])
            {
                case 100:
                {
                    //占星台金币抽奖
                    var bag = _this.mCharacter.GetBag((int) eBagType.GemBag);
                    if (bag.GetFreeCount() < 3)
                    {
                        return ErrorCodes.Error_ItemNoInBag_All;
                    }
                    var needMoneyCount = Table.GetServerConfig(350).ToInt();
                    var exdataCount = Character.GetExData((int) eExdataDefine.e340);
                    if (exdataCount <= 0)
                    {
                        return ErrorCodes.Error_NotDrawCount;
                    }
                    if (Character.lExdata64.GetTime(Exdata64TimeType.AstrologyMoneyTime) < DateTime.Now)
                    {
                        Character.lExdata64.SetTime(Exdata64TimeType.AstrologyMoneyTime,
                            DateTime.Now.AddSeconds(Convert.ToDouble(Table.GetServerConfig(351)))
                            );
                    }
                    else
                    {
                        return ErrorCodes.Error_TimeNotOver;
                    }
                    if (Character.mBag.GetItemCount(2) < needMoneyCount)
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                    //消耗
                    Character.mBag.DeleteItem(2, needMoneyCount, eDeleteItemType.AstrologyDraw);
                    var temps = new DrawItemResult();
                    var itemList = new Dictionary<int, int>();
                    Character.DropMother(10100, itemList);

                    foreach (var j in itemList)
                    {
                        _this.mCharacter.mBag.AddItemToAstrologyBag(j.Key, j.Value, temps.Items);
                    }
                    itemList.Clear();

                    if (_this.mCharacter.Proxy != null)
                    {
                        _this.mCharacter.Proxy.AstrologyDrawOver(temps, DateTime.Now.ToBinary());
                    }
                    Character.SetExData((int) eExdataDefine.e340, exdataCount - 1);
                }
                    break;

                case 101:
                {
                    //占星台矿物抽奖
                    var bag = _this.mCharacter.GetBag((int) eBagType.GemBag);
                    if (bag.GetFreeCount() < 3)
                    {
                        return ErrorCodes.Error_ItemNoInBag_All;
                    }
                    var needResCount = Table.GetServerConfig(353).ToInt();
                    var exdataCount = Character.GetExData((int) eExdataDefine.e341);
                    if (exdataCount <= 0)
                    {
                        return ErrorCodes.Error_NotDrawCount;
                    }
                    if (Character.lExdata64.GetTime(Exdata64TimeType.AstrologyResTime) < DateTime.Now)
                    {
                        Character.lExdata64.SetTime(Exdata64TimeType.AstrologyResTime,
                            DateTime.Now.AddSeconds(Convert.ToDouble(Table.GetServerConfig(351)))
                            );
                    }
                    else
                    {
                        return ErrorCodes.Error_TimeNotOver;
                    }
                    if (Character.mBag.GetItemCount(9) < needResCount)
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                    //消耗
                    Character.mBag.DeleteItem(9, needResCount, eDeleteItemType.AstrologyDraw);
                    var temps = new DrawItemResult();
                    var itemList = new Dictionary<int, int>();
                    Character.DropMother(10100, itemList);

                    foreach (var j in itemList)
                    {
                        _this.mCharacter.mBag.AddItemToAstrologyBag(j.Key, j.Value, temps.Items);
                    }
                    itemList.Clear();

                    if (_this.mCharacter.Proxy != null)
                    {
                        _this.mCharacter.Proxy.AstrologyDrawOver(temps, DateTime.Now.ToBinary());
                    }
                    Character.SetExData((int) eExdataDefine.e341, exdataCount - 1);
                }
                    break;
            }

            return result;
        }

        #endregion

        #region 铁匠屋

        public ErrorCodes Service8_Casting(BuildingBase _this,
                                           BuildingServiceRecord tbBS,
                                           List<int> param,
                                           ref UseBuildServiceResult result)
        {
            if (param.Count < 1)
            {
                return ErrorCodes.Unknow;
            }
            switch (param[0])
            {
                case 0:
                    //铸造
                {
                    if (param.Count < 3)
                    {
                        return ErrorCodes.ParamError;
                    }
                    var furnaceId = param[1];
                    if (furnaceId > SmithyFurnaceMaxIndex)
                    {
                        return ErrorCodes.ParamError;
                    }
                    var forgedId = param[2];
                    var tbForged = Table.Getforged(forgedId);
                    if (tbForged == null)
                    {
                        return ErrorCodes.Error_forgedID;
                    }
                    //消耗检查
                    var index = 0;
                    foreach (var i in tbForged.NeedItemID)
                    {
                        if (i == -1)
                        {
                            break;
                        }
                        if (_this.mCharacter.mBag.GetItemCount(i) < tbForged.NeedItemCount[index])
                        {
                            return ErrorCodes.ItemNotEnough;
                        }
                        index++;
                    }
                    index = 0;
                    foreach (var i in tbForged.NeedResID)
                    {
                        if (i == -1)
                        {
                            break;
                        }
                        if (_this.mCharacter.mBag.GetRes((eResourcesType) i) < tbForged.NeedItemCount[index])
                        {
                            return ErrorCodes.ItemNotEnough;
                        }
                        index++;
                    }
                    //消耗
                    index = 0;
                    foreach (var i in tbForged.NeedItemID)
                    {
                        if (i == -1)
                        {
                            break;
                        }
                        _this.mCharacter.mBag.DeleteItem(i, tbForged.NeedItemCount[index], eDeleteItemType.Casting0);
                        index++;
                    }
                    index = 0;
                    foreach (var i in tbForged.NeedResID)
                    {
                        if (i == -1)
                        {
                            break;
                        }
                        _this.mCharacter.mBag.DelRes((eResourcesType) i, tbForged.NeedItemCount[index],
                            eDeleteItemType.Casting0);
                        index++;
                    }
                    //执行合成
                    _this.SetExdata32(furnaceId, forgedId);
                    var needSeconds = tbForged.NeedTime*60;
                    var pets = GetPets(_this);
                    var param0 = GetBSParamByIndexForSmithy(_this.TbBuild.Type, tbBS, pets, 0);
                    var percent = 1f + param0*0.0001f;
                    if (pets.Count > 0)
                    {
                        foreach (var pet in pets)
                        {
                            var addpeed = GetPetRef(_this.TbBuild.Type, tbBS, pet, 1, tbBS.Param[1]);
                            percent += addpeed*0.0001f;
                        }
                    }
                    //needSeconds = (int) (needSeconds/percent);
                    _this.SetExdata64(furnaceId, DateTime.Now.AddSeconds(needSeconds).ToBinary());
                    _this.MarkDirty();
                }
                    break;
                case 1:
                    // 加速
                {
                    if (param.Count < 2)
                    {
                        return ErrorCodes.ParamError;
                    }
                    var furnaceId = param[1];
                    if (furnaceId > SmithyFurnaceMaxIndex)
                    {
                        return ErrorCodes.ParamError;
                    }
                    var forgedId = _this.GetExdata32(furnaceId);
                    var beginTimeBin = _this.GetExdata64(furnaceId);
                    if (forgedId == -1 || beginTimeBin == -1)
                    {
                        return ErrorCodes.ParamError;
                    }
                    // 检查铸造结束时间，如果已经结束，则返回错误码
                    var endTime = DateTime.FromBinary(_this.GetExdata64(furnaceId));
                    if (DateTime.Now >= endTime)
                    {
                        return ErrorCodes.Error_Build8CastingTimeOver;
                    }
                    // 根据还差多少时间来计算消费
                    var seconds = (int) (endTime - DateTime.Now).TotalSeconds;
                    var needZS = ((seconds + 299)/300)*Table.GetServerConfig(414).ToInt();
                    var oldZS = _this.mCharacter.mBag.GetRes(eResourcesType.DiamondRes);
                    if (oldZS < needZS)
                    {
                        return ErrorCodes.DiamondNotEnough;
                    }
                    _this.mCharacter.mBag.DelRes(eResourcesType.DiamondRes, needZS, eDeleteItemType.SpeedBuild);
                    // 把开始时间提早，这样就完成了
                    _this.SetExdata64(furnaceId, DateTime.Now.ToBinary());
                    _this.MarkDirty();
                }
                    break;
                case 2:
                    //收获
                {
                    if (param.Count < 2)
                    {
                        return ErrorCodes.Unknow;
                    }
                    var furnaceId = param[1];
                    if (furnaceId > SmithyFurnaceMaxIndex)
                    {
                        return ErrorCodes.ParamError;
                    }
                    var forgedId = _this.GetExdata32(furnaceId);
                    var tbForged = Table.Getforged(forgedId);
                    if (tbForged == null)
                    {
                        return ErrorCodes.Error_forgedID;
                    }

                    // 检查铸造结束时间
                    var endTime = DateTime.FromBinary(_this.GetExdata64(furnaceId));
                    if (DateTime.Now < endTime)
                    {
                        return ErrorCodes.Error_Build8CastingTimeNotOver;
                    }
                    if (_this.mCharacter.mBag.CheckAddItem(tbForged.ProductID, 1) != ErrorCodes.OK)
                    {
                        return ErrorCodes.Error_ItemNoInBag_All;
                    }
                    _this.mCharacter.mBag.AddItem(tbForged.ProductID, 1, eCreateItemType.BlacksmithShop);
                    //_this.mCharacter.mCity.CityAddExp(tbForged.NeedTime*tbBS.Param[4]/10000);
                    _this.SetExdata32(furnaceId, -1);
                    _this.SetExdata64(furnaceId, -1);
                    _this.MarkDirty();
                    GiveReward(_this, 406, 1, tbBS, 120, eCreateItemType.BlacksmithShop);
                }
                    break;
                case 3:
                    //进阶
                {
                    if (param.Count < 3)
                    {
                        return ErrorCodes.Unknow;
                    }
                    //装备材料判断
                    var item = _this.mCharacter.GetItemByBagByIndex(param[1], param[2]);
                    if (item == null || item.GetId() == -1)
                    {
                        return ErrorCodes.Error_ItemNotFind;
                    }
                    var mainEquip = item as ItemEquip2;
                    if (mainEquip == null)
                    {
                        return ErrorCodes.Error_ItemIsNoEquip;
                    }
                    var tbEquip = Table.GetEquip(item.GetId());
                    if (tbEquip == null)
                    {
                        return ErrorCodes.Error_EquipID;
                    }
                    if (tbEquip.EquipUpdateLogic == -1)
                    {
                        return ErrorCodes.Error_Build8EquipNotUpdata;
                    }
                    if (tbEquip.UpdateEquipID == -1)
                    {
                        return ErrorCodes.Error_Build8EquipNotUpdata;
                    }
                    var tbEquipUpdataLogic = Table.GetEquipUpdate(tbEquip.EquipUpdateLogic);
                    if (tbEquipUpdataLogic == null)
                    {
                        return ErrorCodes.Error_EquipUpdata;
                    }
                    var maxEnchance = mainEquip;
                    var maxAppend = mainEquip;
                    var otherEquip = tbEquipUpdataLogic.NeedEquipCount - 1;
                    if (otherEquip > 0)
                    {
                        if (param.Count < 3 + otherEquip*2)
                        {
                            return ErrorCodes.ParamError;
                        }

                        List<ItemBase> requiredItems = new List<ItemBase>();
                        requiredItems.Add(mainEquip);
                        for (var i = 0; i < otherEquip; i++)
                        {
                            var otherEquipBagId = param[i*2 + 3];
                            var otherEquipIndex = param[i*2 + 4];
                            var bag = _this.mCharacter.mBag.GetBag(otherEquipBagId);
                            var otherItem = bag.GetItemByIndex(otherEquipIndex);
                            requiredItems.Add(otherItem);
                            if (otherItem == null || otherItem.GetId() == -1)
                            {
                                return ErrorCodes.Error_BagIndexNoItem;
                            }

                            var tbOther = Table.GetEquip(otherItem.GetId());
                            if (tbOther == null)
                            {
                                return ErrorCodes.Error_EquipID;
                            }

                            //if (otherItem.GetId() != item.GetId())
                            if (tbOther.EquipUpdateLogic != tbEquip.EquipUpdateLogic)
                            {
                                return ErrorCodes.Error_Build8EquipNotSame;
                            }
                            if (maxEnchance.GetExdata(0) < otherItem.GetExdata(0))
                            {
                                maxEnchance = otherItem as ItemEquip2;
                                if (maxEnchance == null)
                                {
                                    return ErrorCodes.Error_ItemIsNoEquip;
                                }
                            }
                            if (maxAppend.GetExdata(1) < otherItem.GetExdata(1))
                            {
                                maxAppend = otherItem as ItemEquip2;
                                if (maxAppend == null)
                                {
                                    return ErrorCodes.Error_ItemIsNoEquip;
                                }
                            }
                        }

                        if (requiredItems.Distinct().Count() != requiredItems.Count)
                        {
                            return ErrorCodes.Error_ResNoEnough;
                        }
                    }


                    //附加材料判断
                    var index = 0;
                    foreach (var i in tbEquipUpdataLogic.NeedItemID)
                    {
                        if (i < 0)
                        {
                            break;
                        }
                        if (_this.mCharacter.mBag.GetItemCount(i) < tbEquipUpdataLogic.NeedItemCount[index])
                        {
                            return ErrorCodes.ItemNotEnough;
                        }
                        index++;
                    }
                    index = 0;
                    foreach (var i in tbEquipUpdataLogic.NeedResID)
                    {
                        if (i < 0)
                        {
                            break;
                        }
                        if (_this.mCharacter.mBag.GetRes((eResourcesType) i) < tbEquipUpdataLogic.NeedResCount[index])
                        {
                            return ErrorCodes.ItemNotEnough;
                        }
                        index++;
                    }
                    var tbNextEquip = Table.GetEquip(tbEquip.UpdateEquipID);
                    if (tbNextEquip == null)
                    {
                        return ErrorCodes.Error_Build8EquipNotUpdata;
                    }
                    //强化和追加，继承三个中最高的
                    if (mainEquip != maxEnchance)
                    {
                        mainEquip.SetExdata(0, maxEnchance.GetExdata(0));
                    }
                    if (mainEquip != maxAppend)
                    {
                        mainEquip.SetExdata(1, maxAppend.GetExdata(1));
                    }
                    if (mainEquip.GetBagId() == (int) eBagType.Equip)
                    {
                        //消耗其它装备材料
                        mainEquip.SetId(tbNextEquip.Id);
                        {//洗炼次数
                            int times = mainEquip.GetExdata(24) - 4;
                            times = times < 0 ? 0 : times;
                            times = times > 48 ? 48 : times;
                            mainEquip.SetExdata(24, times);
                        }
                        mainEquip.SetBinding();
                        mainEquip.MarkDbDirty();
                        //消耗其他装备材料
                        for (var i = 0; i < tbEquipUpdataLogic.NeedEquipCount; ++i)
                        {
                            var otherEquipBagId = param[i*2 + 1];
                            var otherEquipIndex = param[i*2 + 2];
                            var bag = _this.mCharacter.mBag.GetBag(otherEquipBagId);
                            var otherItem = bag.GetItemByIndex(otherEquipIndex);
                            if (otherItem != mainEquip)
                            {
                                var oldItemId = otherItem.GetId();
                                var oldItemCount = otherItem.GetCount();
                                bag.CleanItemByIndex(otherEquipIndex);
                                PlayerLog.DataLog(_this.mCharacter.mGuid, "id,{0},{1},{2}", oldItemId, oldItemCount,
                                    (int)eDeleteItemType.Casting3);

                                if (otherEquipBagId != (int) eBagType.Equip)
                                {
                                    _this.mCharacter.EquipChange(0, otherEquipBagId, otherEquipIndex, otherItem);
                                }
                            }
                        }
                    }
                    else
                    {
                        //主材料是穿在身上的装备，判断未来装备是否可穿
                        var resultErrorCode = _this.mCharacter.CheckEquipOn(tbNextEquip, mainEquip.GetBagId());
                        if (resultErrorCode != ErrorCodes.OK)
                        {
                            return resultErrorCode;
                        }


                        if (resultErrorCode == ErrorCodes.OK)
                        {
                            //直接穿
                            mainEquip.SetId(tbNextEquip.Id);

                            {//洗炼次数
                                int times = mainEquip.GetExdata(24) - 4;
                                times = times < 0 ? 0 : times;
                                times = times > 48 ? 48 : times;
                                mainEquip.SetExdata(24, times);
                            }
                            mainEquip.SetBinding();
                            mainEquip.MarkDbDirty();
                            //消耗其他装备材料
                            for (var i = 0; i < tbEquipUpdataLogic.NeedEquipCount; ++i)
                            {
                                var otherEquipBagId = param[i*2 + 1];
                                var otherEquipIndex = param[i*2 + 2];
                                var bag = _this.mCharacter.mBag.GetBag(otherEquipBagId);
                                var otherItem = bag.GetItemByIndex(otherEquipIndex);
                                if (otherItem != mainEquip)
                                {
                                    var oldItemId = otherItem.GetId();
                                    var oldItemCount = otherItem.GetCount();
                                    bag.CleanItemByIndex(otherEquipIndex);
                                    PlayerLog.DataLog(_this.mCharacter.mGuid, "id,{0},{1},{2}", oldItemId, oldItemCount,
                                        (int)eDeleteItemType.Casting3);
                                    if (otherEquipBagId != (int) eBagType.Equip)
                                    {
                                        _this.mCharacter.EquipChange(0, otherEquipBagId, otherEquipIndex, otherItem);
                                    }
                                }
                            }
                            _this.mCharacter.EquipChange(2, mainEquip.GetBagId(), mainEquip.GetIndex(), mainEquip);
                        }
                        else
                        {
                            //这种情况最复杂，进阶已经穿着的装备，但是进阶完已经穿不上了
                            //如果穿不上，检查背包里是否还有空位
                            var nItemInBag = 0;
                            for (var i = 0; i < tbEquipUpdataLogic.NeedEquipCount; i++)
                            {
                                if (param[i*2 + 1] == (int) eBagType.Equip)
                                {
                                    ++nItemInBag;
                                }
                            }
                            var equipBag = _this.mCharacter.GetBag((int) eBagType.Equip);
                            if (nItemInBag == 0) //说明没有一个是目前在装备包裹的，那么必须有空格
                            {
                                if (equipBag.GetFreeCount() <= 0)
                                {
                                    //穿不上，且背包里没有空格，不允许进阶，返回错误码
                                    return ErrorCodes.Error_ItemNoInBag_All;
                                }
                                //背包里有空格，允许进阶，背包里找个位置放下
                                mainEquip.SetId(tbNextEquip.Id);
                                {//洗炼次数
                                    int times = mainEquip.GetExdata(24) - 4;
                                    times = times < 0 ? 0 : times;
                                    times = times > 48 ? 48 : times;
                                    mainEquip.SetExdata(24, times);
                                }
                                mainEquip.SetBinding();
                                mainEquip.MarkDbDirty();
                                equipBag.ForceAddItemByDb(mainEquip.mDbData, _this.mCharacter, eCreateItemType.Casting3);
                            }
                            else
                            {
                                var minIndex = 9999;
                                for (var i = 0; i < tbEquipUpdataLogic.NeedEquipCount; ++i)
                                {
                                    var otherEquipBagId = param[i*2 + 1];
                                    var otherEquipIndex = param[i*2 + 2];
                                    if (otherEquipBagId == (int) eBagType.Equip && minIndex > otherEquipIndex)
                                    {
                                        minIndex = otherEquipIndex;
                                    }
                                }
                                var lastEquip = equipBag.GetItemByIndex(minIndex);
                                lastEquip.CopyFrom(mainEquip);
                                lastEquip.SetId(tbNextEquip.Id);
                                {//洗炼次数
                                    int times = lastEquip.GetExdata(24) - 4;
                                    times = times < 0 ? 0 : times;
                                    times = times > 48 ? 48 : times;
                                    lastEquip.SetExdata(24, times);
                                }
                                var e = lastEquip as ItemEquip2;
                                if (e != null)
                                {
                                    e.SetBinding();
                                }
                                lastEquip.MarkDbDirty();
                                //消耗其他装备材料
                                for (var i = 0; i < tbEquipUpdataLogic.NeedEquipCount; ++i)
                                {
                                    var otherEquipBagId = param[i*2 + 1];
                                    var otherEquipIndex = param[i*2 + 2];
                                    var bag = _this.mCharacter.mBag.GetBag(otherEquipBagId);
                                    var otherItem = bag.GetItemByIndex(otherEquipIndex);
                                    if (otherItem != lastEquip)
                                    {
                                        var oldItemId = otherItem.GetId();
                                        var oldItemCount = otherItem.GetCount();
                                        bag.CleanItemByIndex(otherEquipIndex);
                                        PlayerLog.DataLog(_this.mCharacter.mGuid, "id,{0},{1},{2}", oldItemId, oldItemCount,
                                            (int)eDeleteItemType.Casting3);
                                        if (otherEquipBagId != (int) eBagType.Equip)
                                        {
                                            _this.mCharacter.EquipChange(0, otherEquipBagId, otherEquipIndex, otherItem);
                                        }
                                    }
                                }
                            }
                            _this.mCharacter.EquipChange(0, mainEquip.GetBagId(), mainEquip.GetIndex(), mainEquip);
                        }
                    }
                    var oldLevel = mainEquip.GetBuffLevel(0);
                    var itemId = mainEquip.GetId();
                    var tbNewEquip = Table.GetEquip(itemId);
                    if (tbNewEquip != null && tbNewEquip.AddBuffSkillLevel > oldLevel)
                    {
                        mainEquip.SetBuffLevel(tbNewEquip.AddBuffSkillLevel);
                        mainEquip.MarkDirty();
                    }

                    _this.mCharacter.BroadCastGetEquip(itemId, 100002168);

                    //附加材料判断
                    index = 0;
                    foreach (var i in tbEquipUpdataLogic.NeedItemID)
                    {
                        if (i < 0)
                        {
                            break;
                        }
                        _this.mCharacter.mBag.DeleteItem(i, tbEquipUpdataLogic.NeedItemCount[index],
                            eDeleteItemType.Casting3);
                        index++;
                    }
                    index = 0;
                    foreach (var i in tbEquipUpdataLogic.NeedResID)
                    {
                        if (i < 0)
                        {
                            break;
                        }
                        _this.mCharacter.mBag.DelRes((eResourcesType) i, tbEquipUpdataLogic.NeedResCount[index],
                            eDeleteItemType.Casting3);
                        index++;
                    }
                    //_this.mCharacter.mCity.CityAddExp(tbEquipUpdataLogic.SuccessGetExp);
                    _this.mCharacter.AddExData((int) eExdataDefine.e337, 1);
                }
                    break;
            }
            return ErrorCodes.OK;
        }

        #endregion

        /// <summary>
        ///     额外奖励相关
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="exdataId"> 扩展计数ID </param>
        /// <param name="addValue"> 增加值 </param>
        /// <param name="TbBS"> 建筑服务表 </param>
        /// <param name="maildId"> 邮件ID </param>
        /// <param name="because"> 原因 </param>
        /// <param name="rewardIsEmpty"> 是否清空之前的奖励列表 </param>
        public void GiveReward(BuildingBase _this,
                               int exdataId,
                               int addValue,
                               BuildingServiceRecord TbBS,
                               int maildId,
                               eCreateItemType because,
                               bool rewardIsEmpty = true)
        {
            var oldValue = _this.mCharacter.GetExData(exdataId);
            var newValue = oldValue + addValue;
            var GiveCount = newValue/TbBS.RewardParam;
            _this.mCharacter.SetExData(exdataId, newValue%TbBS.RewardParam);
            if (GiveCount > 0)
            {
                if (rewardIsEmpty)
                {
                    _this.itemReward.Clear();
                }

                for (var j = 0; j < GiveCount; j++)
                {
                    if (TbBS.RewardRule == 0)
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            var dropId = TbBS.RewardID[i];
                            if (dropId == -1)
                            {
                                break;
                            }
                            var minCount = TbBS.RewardCountMin[i];
                            var maxCount = TbBS.RewardCountMax[i];
                            var count = MyRandom.Random(minCount, maxCount);
                            if (count < 1)
                            {
                                continue;
                            }
                            for (var k = 0; k < count; k++)
                            {
                                ShareDrop.DropSon(dropId, _this.itemReward);
                            }
                        }
                    }
                    else if (TbBS.RewardRule == 1)
                    {
                        _this.TempReward.Clear();
                        for (var i = 0; i < 3; i++)
                        {
                            var dropId = TbBS.RewardID[i];
                            if (dropId == -1)
                            {
                                break;
                            }
                            var minCount = TbBS.RewardCountMin[i];
                            var maxCount = TbBS.RewardCountMax[i];
                            var count = MyRandom.Random(minCount, maxCount);
                            if (count < 1)
                            {
                                continue;
                            }
                            _this.TempReward.modifyValue(dropId, count);
                        }
                        if (_this.TempReward.Count > 0)
                        {
                            var t = _this.TempReward.Random();
                            for (var k = 0; k < t.Value; k++)
                            {
                                ShareDrop.DropSon(t.Key, _this.itemReward);
                            }
                        }
                    }
                }

                //for (int j = 0; j < GiveCount; j++)
                //{
                //    if (TbBS.RewardRule == 0)
                //    {
                //        for (var i = 0; i < 3; i++)
                //        {
                //            var itemId = TbBS.RewardID[i];
                //            if (itemId == -1)
                //            {
                //                break;
                //            }
                //            var minCount = TbBS.RewardCountMin[i];
                //            var maxCount = TbBS.RewardCountMax[i];
                //            int count = MyRandom.Random(minCount, maxCount);
                //            if (count < 1)
                //            {
                //                continue;
                //            }
                //            itemReward.modifyValue(itemId, count);
                //        }
                //    }
                //    else
                //    {
                //        TempReward.Clear();
                //        for (var i = 0; i < 3; i++)
                //        {
                //            var itemId = TbBS.RewardID[i];
                //            if (itemId == -1)
                //            {
                //                break;
                //            }
                //            var minCount = TbBS.RewardCountMin[i] ;
                //            var maxCount = TbBS.RewardCountMax[i] ;
                //            int count = MyRandom.Random(minCount, maxCount);
                //            if (count < 1)
                //            {
                //                continue;
                //            }
                //            TempReward.modifyValue(itemId, count);
                //        }
                //        if (TempReward.Count > 0)
                //        {
                //            var t = TempReward.Random();
                //            itemReward.modifyValue(t.Key, t.Value);
                //        }
                //    }
                //}

                if (_this.itemReward.Count > 0)
                {
                    _this.mCharacter.mBag.AddItemOrMail(maildId, _this.itemReward, null, because);
                    _this.itemReward.Clear();
                }
                //if (rewardIsEmpty)
                //{
                //    itemReward.Clear();
                //}
                //if (TbBS.RewardRule == 0)
                //{
                //    for (var i = 0; i < 3; i++)
                //    {
                //        var itemId = TbBS.RewardID[i];
                //        if (itemId == -1)
                //        {
                //            break;
                //        }
                //        var minCount = TbBS.RewardCountMin[i] * GiveCount;
                //        var maxCount = TbBS.RewardCountMax[i] * GiveCount;
                //        int count = MyRandom.Random(minCount, maxCount);
                //        if (count < 1)
                //        {
                //            continue;
                //        }
                //        itemReward.modifyValue(itemId, count);
                //    }
                //}
                //else
                //{
                //    TempReward.Clear();
                //    for (var i = 0; i < 3; i++)
                //    {
                //        var itemId = TbBS.RewardID[i];
                //        if (itemId == -1)
                //        {
                //            break;
                //        }
                //        var minCount = TbBS.RewardCountMin[i] * GiveCount;
                //        var maxCount = TbBS.RewardCountMax[i] * GiveCount;
                //        int count = MyRandom.Random(minCount, maxCount);
                //        if (count < 1)
                //        {
                //            continue;
                //        }
                //        TempReward.modifyValue(itemId, count);
                //    }
                //    if (TempReward.Count > 0)
                //    {
                //        var t = TempReward.Random();
                //        itemReward.modifyValue(t.Key, t.Value);
                //    }
                //}
                //if (itemReward.Count > 0)
                //{
                //    _this.mCharacter.mBag.AddItemOrMail(maildId, itemReward, null, because);
                //    itemReward.Clear();
                //}
            }
            else
            {
                if (!rewardIsEmpty)
                {
                    if (_this.itemReward.Count > 0)
                    {
                        _this.mCharacter.mBag.AddItemOrMail(maildId, _this.itemReward, null, because);
                        _this.itemReward.Clear();
                    }
                }
            }
        }

        #region 孵化室

        public ErrorCodes Service5_Hatch(BuildingBase _this,
                                         BuildingServiceRecord tbBS,
                                         List<int> param,
                                         ref UseBuildServiceResult result)
        {
            if (param.Count < 1)
            {
                return ErrorCodes.Unknow;
            }
            var count = 0;
            switch (param[0])
            {
                case 0:
                    //孵化
                {
                    if (param.Count < 3)
                    {
                        return ErrorCodes.Unknow;
                    }
                    var index = param[1];
                    if (index < 0 || index >= _this.Exdata32.Count)
                    {
                        return ErrorCodes.Error_DataOverflow;
                    }
                    var oldValue = _this.GetExdata32(index);
                    if (oldValue != -1)
                    {
                        return ErrorCodes.Error_AlreadyHaveSeed;
                    }
                    var ItemID = param[2];

                    var tbItem = Table.GetItemBase(ItemID);
                    if (tbItem == null)
                    {
                        return ErrorCodes.Error_ItemID;
                    }

                    var pets = GetPets(_this);
                    var percent = CalculatePetAccTimePercent(_this.TbBuild, tbBS, pets);
                    switch (tbItem.Type)
                    {
                        case 26000: //孵化：蛋
                        case 26100: //孵化：蛋
                        {
                            if (_this.mCharacter.mBag.GetItemCount(ItemID) < 1)
                            {
                                return ErrorCodes.ItemNotEnough;
                            }
                            _this.mCharacter.mBag.DeleteItem(ItemID, 1, eDeleteItemType.Hatch0);
                            _this.SetExdata32(index, ItemID);
                            var needTime = (double) tbItem.Exdata[1]*percent;
                            var _thisTime = DateTime.Now.AddMinutes(needTime);
                            _this.SetExdata64(index, _thisTime.ToBinary());
                            //SetExdata64(index, DateTime.Now.AddMinutes(1.0).ToBinary()); //temp

                            //潜规则引导标记位
                            if (!_this.mCharacter.GetFlag(506))
                            {
                                _this.mCharacter.SetFlag(506);
                                //LogicServerControl.Timer.CreateTrigger(_thisTime, () =>
                                //{
                                //    mCharacter.SetFlag(507, true);
                                //});
                            }
                            _this.MarkDirty();
                        }
                            break;
                        case 70000: //孵化：碎片
                        {
                            //var pet = mCharacter.GetSamePet(tbItem.Exdata[2]);
                            //if (pet != null) return ErrorCodes.Error_PetIsHave;  //已经有这个碎片能孵化的宠物了
                            //if (mCharacter.mBag.GetItemCount(ItemID) < tbItem.Exdata[1]) return ErrorCodes.ItemNotEnough;//判断碎片是否足够
                            //mCharacter.mBag.DeleteItem(ItemID, tbItem.Exdata[1]); //删除道具
                            //SetExdata32(index, ItemID);
                            //SetExdata64(index, DateTime.Now.AddMinutes(tbItem.Exdata[1]).ToBinary());
                            //MarkDirty();

                            //换新的实现方式了
                            var Pet = _this.mCharacter.GetSamePet(tbItem.Exdata[2]);
                            if (Pet == null)
                            {
                                return ErrorCodes.Error_PetNotFind; //宠物已经找到了
                            }
                            if (Pet.GetState() != (int) PetStateType.Piece)
                            {
                                return ErrorCodes.Error_PetIsHave; //已经有这个碎片能孵化的宠物了
                            }
                            if (Pet.GetPiece() < tbItem.Exdata[1])
                            {
                                return ErrorCodes.ItemNotEnough; //判断碎片是否足够
                            }
                            Pet.AddPiece(-tbItem.Exdata[1]); //删除道具
                            //Pet.SetState(PetStateType.Hatch); 碎片就不设置成其他状态了，否则和进阶的区分不开了，如果有问题再改回来
                            Pet.MarkDirty();
                            _this.SetExdata32(index, ItemID);
                            var tbPet = Table.GetPet(Pet.GetId());
                            var needTime = (double) tbPet.NeedTime*percent;
                            _this.SetExdata64(index, DateTime.Now.AddMinutes(needTime).ToBinary());
                            _this.MarkDirty();
                        }
                            break;
                        case 60000: //孵化：宠物升星
                        {
                            var Pet = _this.mCharacter.GetPet(ItemID);
                            if (Pet == null)
                            {
                                return ErrorCodes.Error_PetNotFind;
                            }
                            if (Pet.GetState() != (int) PetStateType.Idle)
                            {
                                return ErrorCodes.Error_PetState;
                            }
                            var tbPet = Table.GetPet(ItemID);
                            if (tbPet == null)
                            {
                                return ErrorCodes.Error_PetID;
                            }
                            if (tbPet.NextId == -1)
                            {
                                return ErrorCodes.Error_PetStarMax; //已经没有下一级可以进阶了
                            }
                            //if (mCharacter.mBag.GetItemCount(tbPet.NeedItemId) < tbPet.NeedItemCount) return ErrorCodes.ItemNotEnough;//判断碎片是否足够
                            //mCharacter.mBag.DeleteItem(tbPet.NeedItemId, tbPet.NeedItemCount); //删除道具
                            if (Pet.GetPiece() < tbPet.NeedItemCount)
                            {
                                return ErrorCodes.ItemNotEnough; //判断碎片是否足够
                            }
                            Pet.AddPiece(-tbPet.NeedItemCount); //删除道具
                            _this.SetExdata32(index, ItemID);
                            var tbNextPet = Table.GetPet(tbPet.NextId);
                            var needTime = (double) tbNextPet.NeedTime*percent;
                            _this.SetExdata64(index, DateTime.Now.AddMinutes(needTime).ToBinary());
                            _this.MarkDirty();
                            Pet.SetState(PetStateType.Hatch);
                            var fp = Pet.GetFightPoint();
                            _this.mCharacter.SetExdataToMore(68, fp);
                            Pet.MarkDirty();
                        }
                            break;
                    }
                    Logger.Info("Service5_Hatch param0={0} itemID={1}", param[0], ItemID);
                    //SetExdata32(index, ItemID);
                    //SetExdata64(index, DateTime.Now.AddMinutes(tbPlant.MatureCycle).ToBinary());
                    //MarkDirty();
                }
                    break;
                case 1:
                    //查收
                {
                    if (param.Count < 2)
                    {
                        return ErrorCodes.Unknow;
                    }
                    var index = param[1];
                    if (index < 0 || index >= _this.Exdata32.Count)
                    {
                        return ErrorCodes.Error_DataOverflow;
                    }
                    var ItemID = _this.GetExdata32(index);
                    if (ItemID == -1)
                    {
                        return ErrorCodes.Error_ItemNotFind;
                    }
                    var overTime = GetIndexTime(_this, index);
                    if (overTime > DateTime.Now)
                    {
                        return ErrorCodes.Error_SeedTimeNotOver;
                    }
                    var tbItem = Table.GetItemBase(ItemID);
                    if (tbItem == null)
                    {
                        return ErrorCodes.Error_ItemID;
                    }
                    switch (tbItem.Type)
                    {
                        case 26000: //孵化：蛋
                        {
                            var itemList = new Dictionary<int, int>();
                            _this.mCharacter.DropMother(tbItem.Exdata[0], itemList);
                            foreach (var i in itemList)
                            {
                                _this.mCharacter.mBag.AddItem(i.Key, i.Value, eCreateItemType.Hatch);
                                count = i.Key;
                            }
                            _this.SetExdata32(index, -1);
                            _this.SetExdata64(index, 0);
                            if (_this.mCharacter != null)
                            {
                                _this.mCharacter.AddExData((int) eExdataDefine.e90, 1);
                            }
                            //var needTime = tbItem.Exdata[1];
                            //_this.mCharacter.mCity.CityAddExp(needTime*tbBS.Param[5]/10000);
                            _this.MarkDirty();
                        }
                            break;
                        case 26100: //孵化：固定蛋
                        {
                            count = tbItem.Exdata[0];
                            _this.mCharacter.mBag.AddItem(count, 1, eCreateItemType.Hatch);
                            _this.SetExdata32(index, -1);
                            _this.SetExdata64(index, 0);
                            if (_this.mCharacter != null)
                            {
                                _this.mCharacter.AddExData((int) eExdataDefine.e90, 1);
                            }
                            //var needTime = tbItem.Exdata[1];
                            //_this.mCharacter.mCity.CityAddExp(needTime*tbBS.Param[5]/10000);
                            _this.MarkDirty();
                        }
                            break;
                        case 70000: //孵化：碎片
                        {
                            var Pet = _this.mCharacter.GetSamePet(tbItem.Exdata[2]);
                            if (Pet == null)
                            {
                                return ErrorCodes.Error_PetNotFind;
                            }
                            Pet.SetState(PetStateType.Idle);
                            if (_this.mCharacter != null)
                            {
                                _this.mCharacter.AddExData((int) eExdataDefine.e90, 1);
                                _this.mCharacter.AddExData((int) eExdataDefine.e330, 1);
                                _this.mCharacter.AddExData((int) eExdataDefine.e331, 1);
                                var fp = Pet.GetFightPoint();
                                _this.mCharacter.SetExdataToMore(68, fp);
                            }
                            Pet.MarkDirty();
                            //mCharacter.mBag.AddItem(tbItem.Exdata[2], 1);
                            _this.SetExdata32(index, -1);
                            _this.SetExdata64(index, 0);
                            //mCharacter.AddExData((int)eAchievementExdata.e90, 1);
                            //var tbPet = Table.GetPet(Pet.GetId());
                            //var needTime = tbPet.NeedTime;
                            //_this.mCharacter.mCity.CityAddExp(needTime*tbBS.Param[5]/10000);
                            _this.MarkDirty();
                            count = Pet.GetId();
                        }
                            break;
                        case 60000: //孵化：宠物升星
                        {
                            var tbPet = Table.GetPet(ItemID);
                            var Pet = _this.mCharacter.GetPet(ItemID);
                            if (Pet == null || tbPet == null)
                            {
                                Logger.Error("Service5_Hatch not find pet param0={0} itemID={1}", param[0], ItemID);
                                return ErrorCodes.Unknow;
                            }
                            if (_this.mCharacter != null)
                            {
                                _this.mCharacter.AddExData((int) eExdataDefine.e269, 1);
                            }
                            Pet.SetId(tbPet.NextId);
                            //var tbNextPet = Table.GetPet(tbPet.NextId);
                            //var needTime = tbNextPet.NeedTime;
                            //_this.mCharacter.mCity.CityAddExp(needTime*tbBS.Param[5]/10000);
                            Pet.SetState(PetStateType.Idle);
                            Pet.MarkDirty();
                            _this.SetExdata32(index, -1);
                            _this.SetExdata64(index, 0);
                            _this.MarkDirty();
                            count = Pet.GetId();
                        }
                            break;
                    }
                    //潜规则引导标记位
//                         if (_this.mCharacter.GetFlag(507))
//                         {
//                             _this.mCharacter.SetFlag(508);
//                             _this.mCharacter.SetFlag(507, false);
//                         }
                    GiveReward(_this, 404, 1, tbBS, 118, eCreateItemType.Hatch);
                }
                    break;
                case 2:
                {
                    //停止
                    if (param.Count < 2)
                    {
                        return ErrorCodes.Unknow;
                    }
                    var index = param[1];
                    if (index < 0 || index >= _this.Exdata32.Count)
                    {
                        return ErrorCodes.Error_DataOverflow;
                    }
                    var ItemID = _this.GetExdata32(index);
                    if (ItemID == -1)
                    {
                        return ErrorCodes.Error_ItemNotFind;
                    }
                    var overTime = DateTime.FromBinary(_this.GetExdata64(index));
                    if (overTime < DateTime.Now)
                    {
                        return ErrorCodes.Error_HatchTimeOver;
                    }
                    var tbItem = Table.GetItemBase(ItemID);
                    if (tbItem == null)
                    {
                        return ErrorCodes.Error_ItemID;
                    }
                    switch (tbItem.Type)
                    {
                        case 26000: //孵化：蛋
                        case 26100: //孵化：蛋
                        {
                            //孵化室管理员,您在包裹已满时进行了取消孵化的操作，遗落了随从蛋
                            var resultCodes = _this.mCharacter.mBag.AddItemOrMail(1,
                                new Dictionary<int, int> {{ItemID, 1}}, null, eCreateItemType.Hatch);
                            //                                     ErrorCodes resultCodes = mCharacter.mBag.CheckAddItem(ItemID, 1);
                            //                                     if (resultCodes == ErrorCodes.OK)
                            //                                     {
                            //                                         mCharacter.mBag.AddItem(ItemID, 1, eCreateItemType.Hatch);
                            //                                     }
                            //                                     else if (resultCodes == ErrorCodes.Error_ItemNoInBag_All)
                            //                                     {
                            //                                         mCharacter.mMail.PushMail("取消的孵化", "您的包裹已满，请查收取消的蛋", new Dictionary<int, int>() { { ItemID, 1 } });
                            //                                     }
                            //                                     else
                            //                                     {
                            //                                         Logger.Warn("Service5_Hatch type=2 ItemID={0}", ItemID);
                            //                                     }
                            _this.SetExdata32(index, -1);
                            _this.SetExdata64(index, 0);
                            _this.MarkDirty();
                        }
                            break;
                        case 70000: //孵化：碎片
                        {
                            //换新的实现方式了
                            var pet = _this.mCharacter.GetSamePet(tbItem.Exdata[2]);
                            if (pet == null)
                            {
                                return ErrorCodes.Error_PetNotFind; //宠物没有找到了
                            }
                            pet.AddPiece(tbItem.Exdata[1]); //补道具
                            //if (pet.GetState() == (int)PetStateType.Hatch) 直接设置成碎片状态
                            {
                                pet.SetState(PetStateType.Piece);
                            }
                            pet.MarkDirty();
                            _this.SetExdata32(index, -1);
                            _this.SetExdata64(index, 0);
                            _this.MarkDirty();
                        }
                            break;
                        case 60000: //孵化：宠物升星
                        {
                            var tbPet = Table.GetPet(ItemID);
                            var pet = _this.mCharacter.GetSamePet(ItemID);
                            if (pet == null || tbPet == null)
                            {
                                Logger.Error("Service5_Hatch not find pet param0={0} itemID={1}", param[0], ItemID);
                                return ErrorCodes.Unknow;
                            }
                            pet.AddPiece(tbPet.NeedItemCount); //补道具
                            pet.SetState(PetStateType.Idle);
                            pet.MarkDirty();
                            _this.SetExdata32(index, -1);
                            _this.SetExdata64(index, 0);
                            _this.MarkDirty();
                        }
                            break;
                    }
                }
                    break;
                case 3:
                {
                    //加速
                    if (param.Count < 2)
                    {
                        return ErrorCodes.Unknow;
                    }
                    var index = param[1];
                    if (index < 0 || index >= _this.Exdata32.Count)
                    {
                        return ErrorCodes.Error_DataOverflow;
                    }
                    var ItemID = _this.GetExdata32(index);
                    if (ItemID == -1)
                    {
                        return ErrorCodes.Error_ItemNotFind;
                    }
                    var overTime = DateTime.FromBinary(_this.GetExdata64(index));
                    if (overTime < DateTime.Now)
                    {
                        return ErrorCodes.Error_HatchTimeOver;
                    }
                    var seconds = (int) overTime.GetDiffSeconds(DateTime.Now);
                    var needZS = ((seconds/300) + 1)*Table.GetServerConfig(240).ToInt();
                    var oldZS = _this.mCharacter.mBag.GetRes(eResourcesType.DiamondRes);
                    if (oldZS < needZS)
                    {
                        return ErrorCodes.DiamondNotEnough;
                    }
                    var tbItem = Table.GetItemBase(ItemID);
                    if (tbItem == null)
                    {
                        return ErrorCodes.Error_ItemID;
                    }
                    _this.mCharacter.mBag.DelRes(eResourcesType.DiamondRes, needZS, eDeleteItemType.HatchSpeed);
                    _this.SetExdata64(index, DateTime.Now.ToBinary());
                    _this.MarkDirty();
                }
                    break;
            }
            result.Data32.Add(count);
            return ErrorCodes.OK;
        }

        public Int64 GetExdata64(BuildingBase _this, int nIndex)
        {
            if (_this.Exdata64.Count <= nIndex)
            {
                return -1;
            }
            return _this.Exdata64[nIndex];
        }

        public DateTime GetIndexTime(BuildingBase _this, int index)
        {
            var value = GetExdata64(_this, index);
            if (value == -1)
            {
                return DateTime.Now;
            }
            return DateTime.FromBinary(value);
        }

        #endregion

        #region 宠物时间扩展数据方法

        public Int64 GetPetExdata64(BuildingBase _this, int nIndex)
        {
            if (_this.PetTime.Count <= nIndex)
            {
                return -1;
            }
            return _this.PetTime[nIndex];
        }

        public void SetPetExdata64(BuildingBase _this, int nIndex, Int64 nValue)
        {
            var nNowCount = _this.PetTime.Count;
            if (nIndex == nNowCount)
            {
                AddPetExdata64(_this, nValue);
                return;
            }
            if (nNowCount < nIndex)
            {
                Logger.Log(LogLevel.Warn,
                    string.Format("SetPetExdata64 AreaId={0},NowCount={1},SetIndex={2},Value={3}", _this.AreaId,
                        nNowCount, nIndex, nValue));
                //要设置的数据位数不足，用-1补足
                for (var i = nNowCount; i <= nIndex; ++i)
                {
                    AddPetExdata64(_this, -1);
                }
            }
            _this.PetTime[nIndex] = nValue;
        }

        //增加扩展数据
        public void AddPetExdata64(BuildingBase _this, Int64 nValue)
        {
            _this.PetTime.Add(nValue);
        }

        #endregion

        #region 宠物放置的时间

        //获得宠物索引
        public int GetPetIndex(BuildingBase _this, int petId)
        {
            var index = 0;
            foreach (var i in _this.PetList)
            {
                if (i == petId)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        //获得Pet的放置时间
        public DateTime GetPetIndexTime(BuildingBase _this, int index)
        {
            var value = GetPetExdata64(_this, index);
            if (value == -1)
            {
                return DateTime.Now;
            }
            return DateTime.FromBinary(value);
        }

        //设置Pet的放置时间
        public void SetPetIndexTime(BuildingBase _this, int nIndex, DateTime nValue)
        {
            SetPetExdata64(_this, nIndex, nValue.ToBinary());
        }

        //获得Pet的实际工作时间
        public DateTime GetPetWorkTime(BuildingBase _this, int index)
        {
            var tm = GetPetIndexTime(_this, index);
            if (_this.StateOverTime > tm)
            {
                return _this.StateOverTime;
            }
            return tm;
        }


        //给宠物经验
        public void GivePetExp(BuildingBase _this, DateTime lastTime)
        {
            if (_this.TbBuild.ServiceId == -1)
            {
            }

            //var table = Table.GetBuildingService(TbBuild.ServiceId);
            //if (null == table)
            //    return;

            //int addExp = (int)(DateTime.Now.GetDiffSeconds(lastTime) / 3600 * table.PetExp);
            //int index = 0;
            //foreach (int i in PetList)
            //{
            //    if (i == -1) continue;
            //    DateTime workTime = GetPetWorkTime(index);
            //    int _thisAddExp = addExp;
            //    if (workTime < lastTime)
            //    {
            //        workTime = lastTime;
            //        _thisAddExp = (int)(DateTime.Now.GetDiffSeconds(workTime) / 3600 * table.PetExp);
            //    }
            //    var pet = mCharacter.GetPet(i);
            //    if (pet == null)
            //    {
            //        return;
            //    }
            //    int oldLevel = pet.GetLevel();
            //    pet.AddExp(_thisAddExp);
            //    int newLevel = pet.GetLevel();
            //    if (newLevel > oldLevel)
            //    {
            //        mCharacter.AddExData((int)eExdataDefine.e331, newLevel - oldLevel);
            //    }
            //    index++;
            //}
            //if (TbBuild.Type == (int)BuildingType.ArenaTemple)
            //{
            //    bool isLevelUp = false;
            //    for (int i = 0; i != 5; ++i)
            //    {
            //        int sId = GetExdata32(i);
            //        if (sId == -1) continue;
            //        var tbStatue = Table.GetStatue(sId);
            //        if (tbStatue == null)
            //        {
            //            Logger.Error("buildAddExp type 6 not find Id={0}", GetExdata32(i));
            //            continue;
            //        }
            //        isLevelUp = GiveStatueExp(tbStatue, i, addExp);
            //    }
            //    if (isLevelUp)
            //    {
            //        mCharacter.BooksChange();
            //    }
            //    MarkDirty();
            //}
        }

        //给某个宠物经验
        public void GivePetExpByIndex(BuildingBase _this, int index, DateTime lastTime)
        {
            var workTime = GetPetWorkTime(_this, index);
            if (workTime < lastTime)
            {
                workTime = lastTime;
            }
            var pet = _this.mCharacter.GetPet(_this.PetList[index]);
            if (pet == null)
            {
                return;
            }
            var addExp = 0;
            // (int)(DateTime.Now.GetDiffSeconds(workTime) / 3600 * Table.GetBuildingService(TbBuild.ServiceId).PetExp);
            var oldLevel = pet.GetLevel();
            pet.PetAddExp(addExp);
            var newLevel = pet.GetLevel();
            if (newLevel > oldLevel)
            {
                _this.mCharacter.AddExData((int) eExdataDefine.e331, newLevel - oldLevel);
                var fp = pet.GetFightPoint();
                _this.mCharacter.SetExdataToMore(68, fp);
            }
        }

        #endregion

        #region 构造相关

        //重置建筑数据
        public void Reset(BuildingBase _this, int guid, int areaId, BuildingRecord tbBuild, BuildStateType type)
        {
            _this.mDbData.Guid = guid;
            _this.TypeId = tbBuild.Id;
            _this.TbBuild = tbBuild;
            if (_this.TbBuild != null)
            {
                if (_this.TbBuild.ServiceId != -1)
                {
                    _this.TbBs = Table.GetBuildingService(_this.TbBuild.ServiceId);
                }
                else
                {
                    _this.TbBs = null;
                }
                if (_this.TbBuild.OrderRefleshRule != -1)
                {
                    _this.TbOu = Table.GetOrderUpdate(_this.TbBuild.OrderRefleshRule);
                }
                else
                {
                    _this.TbOu = null;
                }
            }
            _this.State = BuildStateType.Idle;
            if (tbBuild.FlagId > 0)
            {
                _this.mCharacter.SetFlag(tbBuild.FlagId);
            }
            ResetExdata(_this);
            _this.State = type;
            if (type == BuildStateType.Building)
            {
                _this.mDbData.StateOverTime = DateTime.Now.AddMinutes(tbBuild.NeedMinutes).ToBinary();
                _this.StartTrigger(_this.StateOverTime);
            }
        }

        //更新建筑的扩展数据
        public void ResetExdata(BuildingBase _this)
        {
            _this.Exdata32.Clear();
            _this.Exdata64.Clear();
            switch (_this.Type)
            {
                //议事厅
                case BuildingType.BaseCamp:
                {
                }
                    break;
                //矿洞
                case BuildingType.Mine:
                {
                    _this.Exdata32.Add(0); //已经收回的宠物提供的资源点数
                }
                    break;
                //农场
                case BuildingType.Farm:
                {
                    //var tbBS = Table.GetBuildingService(TbBuild.ServiceId);
                    var role = _this.mCharacter.GetRole();
                    var initId = 90108;
                    if (role == 1)
                    {
                        initId = 90105;
                    }
                    else if (role == 2)
                    {
                        initId = 90100;
                    }
                    for (var i = 0; i != _this.TbBs.Param[0]; ++i) //农场的地开启个数（不会修正）
                    {
                        _this.AddExdata32(initId);
                        _this.AddExdata64(0);
                    }
                    _this.mCharacter.mCity.RefreshMission();
                }
                    break;
                //魔族洞穴
                case BuildingType.DemonCave:
                {
                }
                    break;
                //恶魔回廊
                case BuildingType.DemonCloister:
                {
                }
                    break;
                //孵化室
                case BuildingType.MercenaryCamp:
                {
                    //var tbBS = Table.GetBuildingService(TbBuild.ServiceId);
                    for (var i = 0; i != _this.TbBs.Param[1]; ++i) //孵化室的开启个数（不会修正）
                    {
                        _this.AddExdata32(-1);
                        _this.AddExdata64(0);
                    }
                }
                    break;
                //角斗圣殿
                case BuildingType.ArenaTemple:
                {
                    for (var i = 0; i != 5; ++i)
                    {
                        _this.PetList.Add(-1);
                        _this.PetTime.Add(0);
                        _this.AddExdata32(i*100);
                        _this.AddExdata64(0);
                    }
                }
                    break;
                //仓库
                case BuildingType.WarHall:
                {
                }
                    break;
                //铁匠屋
                case BuildingType.BlacksmithShop:
                {
                    // 确保 Exdata32 和 Exdata64 都有四个位置
                    _this.SetExdata32(SmithyFurnaceMaxIndex, -1);
                    _this.SetExdata64(SmithyFurnaceMaxIndex, -1);
                }
                    break;
                //交易所
                case BuildingType.Exchange:
                {
                    //var tbBS = Table.GetBuildingService(TbBuild.ServiceId);
                    _this.mCharacter.mExchange.ResetCount(_this.TbBs.Param[0]); //交易所的开启个数（不会修正）
                }
                    break;
                //合成屋
                case BuildingType.CompositeHouse:
                {
                }
                    break;
                //许愿池
                case BuildingType.WishingPool:
                {
                }
                    break;
                //勇士港
                case BuildingType.BraveHarbor:
                {
                    _this.AddExdata32(0);
                }
                    break;
                //伐木场
                case BuildingType.LogPlace:
                {
                    _this.Exdata32.Add(0); //已经收回的宠物提供的资源点数
                }
                    break;
                //破损的
                case BuildingType.Broken:
                {
                }
                    break;
                //杂物
                case BuildingType.Debris:
                {
                }
                    break;
                //空地
                case BuildingType.Space:
                {
                }
                    break;
            }
        }

        //更新建筑的数据
        public void UpdataExdata(BuildingBase _this)
        {
            _this.TbBuild = Table.GetBuilding(_this.mDbData.TypeId);
            if (_this.TbBuild == null)
            {
                return;
            }
            if (_this.TbBuild.ServiceId != -1)
            {
                _this.TbBs = Table.GetBuildingService(_this.TbBuild.ServiceId);
            }
            if (_this.TbBuild.OrderRefleshRule != -1)
            {
                _this.TbOu = Table.GetOrderUpdate(_this.TbBuild.OrderRefleshRule);
            }
            switch (_this.Type)
            {
                //议事厅
                case BuildingType.BaseCamp:
                {
                }
                    break;
                //矿洞
                case BuildingType.Mine:
                {
                }
                    break;
                //农场
                case BuildingType.Farm:
                {
                    if (_this.TbBs.Param[0] > _this.Exdata32.Count)
                    {
                        for (var i = _this.Exdata32.Count; i < _this.TbBs.Param[0]; ++i) //地的开启数量上限（不会修正）
                        {
                            _this.AddExdata32(-1);
                            _this.AddExdata64(0);
                        }
                    }
                    else if (_this.TbBs.Param[0] < _this.Exdata32.Count)
                    {
                        Logger.Error("BuildExdata Too Long!! type={0},ExdataCount ={1},Param={2}", _this.Type,
                            _this.Exdata32.Count, _this.TbBs.Param[0]);
                        var tempList = new List<int>();
                        for (var i = 0; i < _this.TbBs.Param[0]; ++i)
                        {
                            tempList.Add(_this.Exdata32[i]);
                        }
                        _this.Exdata32.Clear();
                        _this.Exdata32.AddRange(tempList);
                    }
                    _this.mCharacter.mBag.GetBag((int) eBagType.FarmDepot).SetNowCount(_this.TbBs.Param[2]);
                    //农场仓库格数（不会修正）
                }
                    break;
                //魔族洞穴
                case BuildingType.DemonCave:
                {
                }
                    break;
                //恶魔回廊
                case BuildingType.DemonCloister:
                {
                }
                    break;
                //孵化室
                case BuildingType.MercenaryCamp:
                {
                    if (_this.TbBs.Param[1] > _this.Exdata32.Count)
                    {
                        for (var i = _this.Exdata32.Count; i < _this.TbBs.Param[1]; ++i) //孵化室的神像开启个数（不会修正）
                        {
                            _this.AddExdata32(-1);
                            _this.AddExdata64(0);
                        }
                    }
                    else if (_this.TbBs.Param[1] < _this.Exdata32.Count)
                    {
                        Logger.Error("BuildExdata Too Long!! type={0},ExdataCount ={1},Param={2}", _this.Type,
                            _this.Exdata32.Count, _this.TbBs.Param[1]);
                        var tempList = new List<int>();
                        for (var i = 0; i < _this.TbBs.Param[1]; ++i)
                        {
                            tempList.Add(_this.Exdata32[i]);
                        }
                        _this.Exdata32.Clear();
                        _this.Exdata32.AddRange(tempList);
                    }
                }
                    break;
                //角斗圣殿
                case BuildingType.ArenaTemple:
                {
                    //if (TbBs.Param[0] > tbOldBS.Param[0])//2-6级 ：分别开启5个神像  //角斗圣殿的神像开启个数（不会修正）
                    //{
                    //    for (int index = tbOldBS.Param[0]; index < TbBs.Param[0]; ++index)//角斗圣殿的神像开启个数（不会修正）
                    //    {
                    //        int sId = GetExdata32(index);
                    //        if (sId == -1) continue;
                    //        var tbStatue = Table.GetStatue(sId);
                    //        AddStatueLevel(tbStatue, index);
                    //        mCharacter.AddExData((int)eExdataDefine.e336, 1);
                    //    }
                    //    mCharacter.mCity.SetAttrFlag();
                    //    mCharacter.BooksChange();
                    //}
                }
                    break;
                //仓库
                case BuildingType.WarHall:
                {
                }
                    break;
                //铁匠屋
                case BuildingType.BlacksmithShop:
                {
                }
                    break;
                //交易所
                case BuildingType.Exchange:
                {
                    _this.mCharacter.mExchange.ResetCount(_this.TbBs.Param[0]); //交易所的开启个数（不会修正）
                }
                    break;
                //合成屋
                case BuildingType.CompositeHouse:
                {
                }
                    break;
                //许愿池
                case BuildingType.WishingPool:
                {
                    var overtime = _this.mCharacter.lExdata64.GetTime(Exdata64TimeType.FreeWishingTime);
                    var willtime = DateTime.Now.AddMinutes(_this.TbBs.Param[0]);
                    if (overtime > willtime)
                    {
                        _this.mCharacter.lExdata64.SetTime(Exdata64TimeType.FreeWishingTime, willtime);
                    }
                }
                    break;
                //勇士港
                case BuildingType.BraveHarbor:
                {
					_this.mCharacter.mBag.GetBag((int)eBagType.MedalUsed).SetNowCount(10);
                    if (_this.Exdata32.Count == 0)
                    {
                        _this.Exdata32.Add(0);
                    }
                    else if (_this.Exdata32[0] > 4)
                    {
                        _this.Exdata32.Clear();
                        _this.Exdata32.Add(0);
                    }
                }
                    break;
                //伐木场
                case BuildingType.LogPlace:
                {
                }
                    break;
                //破损的
                case BuildingType.Broken:
                {
                }
                    break;
                //杂物
                case BuildingType.Debris:
                {
                }
                    break;
                //空地
                case BuildingType.Space:
                {
                }
                    break;
            }
        }

        public ErrorCodes Upgrade(BuildingBase _this)
        {
            var tbNextBuild = Table.GetBuilding(_this.TbBuild.NextId);
            if (tbNextBuild == null)
            {
                return ErrorCodes.Error_BuildID;
            }

            //如果是矿洞或者伐木场自动收取资源
            if (BuildingType.Mine == (BuildingType) _this.TbBuild.Type ||
                BuildingType.LogPlace == (BuildingType) _this.TbBuild.Type)
            {
                //var tbBS = Table.GetBuildingService(TbBuild.ServiceId);
                if (null != _this.TbBs)
                {
                    var ret = new UseBuildServiceResult();
                    Service1_Mine(_this, _this.TbBs, ref ret);
                }
            }

            _this.StateOverTime = DateTime.Now.AddMinutes(tbNextBuild.NeedMinutes);
            _this.StartTrigger(_this.StateOverTime);
            _this.State = BuildStateType.Upgrading;

            //潜规则引导标记位
            if (_this.TbBuild.NextId == 1)
            {
                if (_this.mCharacter.GetFlag(509))
                {
                    _this.mCharacter.SetFlag(509, false);
                }
            }


            _this.MarkDirty();
            return ErrorCodes.OK;
        }

        public ErrorCodes Speedup(BuildingBase _this)
        {
            var minutes = (int) (DateTime.FromBinary(_this.mDbData.StateOverTime) - DateTime.Now).TotalMinutes;
            //if (seconds <= 0)
            //{
            //    return ErrorCodes.Unknow;
            //}
            //int minutes = seconds/60;
            if (minutes > 5)
            {
                var AddskillUpId = Table.GetServerConfig(260).ToInt();
                var PerskillUpId = Table.GetServerConfig(261).ToInt();
                var DeMinuteskillUpId = Table.GetServerConfig(262).ToInt();
                var AddCount = Table.GetSkillUpgrading(AddskillUpId).GetSkillUpgradingValue(minutes);
                var perCount = Table.GetSkillUpgrading(PerskillUpId).GetSkillUpgradingValue(minutes);
                var DeMinutesCount = Table.GetSkillUpgrading(DeMinuteskillUpId).GetSkillUpgradingValue(minutes);
                var needZS = AddCount + (minutes - DeMinutesCount)*100/perCount;
                //var needZS = ((seconds/300) + 1)*Table.GetServerConfig(260).ToInt();


                var oldZS = _this.mCharacter.mBag.GetRes(eResourcesType.DiamondRes);
                if (oldZS < needZS)
                {
                    return ErrorCodes.DiamondNotEnough;
                }
                //mCharacter.mBag.SetRes(eResourcesType.DiamondRes, oldZS - needZS);
                _this.mCharacter.mBag.DelRes(eResourcesType.DiamondRes, needZS, eDeleteItemType.SpeedBuild);
            }
            if (_this.mOverTrigger != null)
            {
                LogicServerControl.Timer.DeleteTrigger(_this.mOverTrigger);
                _this.mOverTrigger = null;
            }
            _this.StateOverTime = DateTime.Now;
            TimeOver(_this);
            return ErrorCodes.OK;
        }

        public void UpgradeOver(BuildingBase _this)
        {
            var tbNextBuild = Table.GetBuilding(_this.TbBuild.NextId);
            if (tbNextBuild == null)
            {
                return;
            }
            var tbOldBS = _this.TbBs;
            _this.TypeId = tbNextBuild.Id;
            _this.TbBuild = tbNextBuild;
            _this.TbBs = Table.GetBuildingService(_this.TbBuild.ServiceId);
            _this.TbOu = _this.TbBuild.OrderRefleshRule != -1
                ? Table.GetOrderUpdate(_this.TbBuild.OrderRefleshRule)
                : null;
            _this.mCharacter.SetFlag(_this.TbBuild.FlagId);
            _this.State = BuildStateType.Idle;
            switch (_this.Type)
            {
                //议事厅
//                 case BuildingType.BaseCamp:
//                 {
//                     if (_this.TypeId == 1)
//                     {
//                         _this.mCharacter.SetFlag(510);
//                     }
//                 }
//                    break;
                //矿洞
                case BuildingType.Mine:
                {
                    //var tbBS = Table.GetBuildingService(TbBuild.ServiceId);
                    //for (int i = Exdata32.Count; i < tbBS.Param[0]; ++i)//矿洞开启设置上限（不会修正）
                    //{
                    //    AddExdata32(-1);
                    //    AddExdata64(0);
                    //}
                }
                    break;
                //农场
                case BuildingType.Farm:
                {
                    if (_this.TbBs.Param[0] > tbOldBS.Param[0])
                    {
                        for (var index = tbOldBS.Param[0]; index < _this.TbBs.Param[0]; ++index) //地的开启数量上限（不会修正）
                        {
                            _this.AddExdata32(-1);
                            _this.AddExdata64(0);
                        }
                    }
                    _this.mCharacter.mBag.GetBag((int) eBagType.FarmDepot).SetNowCount(_this.TbBs.Param[2]);
                    //农场仓库格数（不会修正）
                    _this.mCharacter.mCity.RefreshMission();
                }
                    break;
                //魔族洞穴
                case BuildingType.DemonCave:
                {
                }
                    break;
                //恶魔回廊
                case BuildingType.DemonCloister:
                {
                }
                    break;
                //孵化室
                case BuildingType.MercenaryCamp:
                {
                    if (_this.TbBs.Param[1] > tbOldBS.Param[1])
                    {
                        for (var index = tbOldBS.Param[1]; index < _this.TbBs.Param[1]; ++index) //孵化室的神像开启个数（不会修正）
                        {
                            _this.AddExdata32(-1);
                            _this.AddExdata64(0);
                        }
                    }
                }
                    break;
                //角斗圣殿
                case BuildingType.ArenaTemple:
                {
                    //var tbBS = Table.GetBuildingService(TbBuild.ServiceId);
                    if (_this.TbBs.Param[0] > tbOldBS.Param[0]) //2-6级 ：分别开启5个神像  //角斗圣殿的神像开启个数（不会修正）
                    {
                        for (var index = tbOldBS.Param[0]; index < _this.TbBs.Param[0]; ++index) //角斗圣殿的神像开启个数（不会修正）
                        {
                            var sId = _this.GetExdata32(index);
                            if (sId == -1)
                            {
                                continue;
                            }
                            var tbStatue = Table.GetStatue(sId);
                            AddStatueLevel(_this, tbStatue, index);
                            _this.mCharacter.AddExData((int) eExdataDefine.e336, 1);
                        }
                        _this.mCharacter.mCity.SetAttrFlag();
                        _this.mCharacter.BooksChange();
                    }
                }
                    break;
                //仓库
                case BuildingType.WarHall:
                {
                }
                    break;
                //铁匠屋
                case BuildingType.BlacksmithShop:
                {
                }
                    break;
                //交易所
                case BuildingType.Exchange:
                {
                    //var tbBS = Table.GetBuildingService(TbBuild.ServiceId);
                    _this.mCharacter.mExchange.ResetCount(_this.TbBs.Param[0]); //交易所的开启个数（不会修正）
                }
                    break;
                //合成屋
                case BuildingType.CompositeHouse:
                {
                }
                    break;
                //许愿池
                case BuildingType.WishingPool:
                {
                    var overtime = _this.mCharacter.lExdata64.GetTime(Exdata64TimeType.FreeWishingTime);
                    var willtime = DateTime.Now.AddMinutes(_this.TbBs.Param[0]);
                    if (overtime > willtime)
                    {
                        _this.mCharacter.lExdata64.SetTime(Exdata64TimeType.FreeWishingTime, willtime);
                    }
                }
                    break;
                //勇士港
                case BuildingType.BraveHarbor:
                {
                    //var tbBS = Table.GetBuildingService(TbBuild.ServiceId);
                    _this.mCharacter.mBag.GetBag((int) eBagType.MedalUsed).SetNowCount(_this.TbBs.Param[2]);
                    //勇士港的开启个数（不会修正）
                }
                    break;
                //伐木场
                case BuildingType.LogPlace:
                {
                    //var tbBS = Table.GetBuildingService(TbBuild.ServiceId);
                    //for (int i = Exdata32.Count; i < tbBS.Param[0]; ++i)
                    //{
                    //    AddExdata32(-1);
                    //    AddExdata64(0);
                    //}
                }
                    break;
                //破损的
                case BuildingType.Broken:
                {
                }
                    break;
                //杂物
                case BuildingType.Debris:
                {
                }
                    break;
                //空地
                case BuildingType.Space:
                {
                }
                    break;
            }
            _this.MarkDirty();
        }

        public void StartTrigger(BuildingBase _this, DateTime dateTime)
        {
            if (_this.mOverTrigger != null)
            {
                LogicServerControl.Timer.DeleteTrigger(_this.mOverTrigger);
            }
            _this.mOverTrigger = LogicServerControl.Timer.CreateTrigger(dateTime, () => TimeOver(_this));
        }

        public void TimeOver(BuildingBase _this)
        {
            _this.mOverTrigger = null;
            switch (_this.State)
            {
                case BuildStateType.Building:
                    _this.mCharacter.AddExData((int) eExdataDefine.e252, 1);
                    //_this.mCharacter.mCity.CityAddExp(_this.TbBuild.GetMainHouseExp);
                    _this.State = BuildStateType.Idle;
                    //潜规则引导标记位
//                     if (_this.TbBuild.Id == 50)
//                     {
//                         if (_this.mCharacter.GetFlag(504))
//                         {
//                             _this.mCharacter.SetFlag(505);
//                             _this.mCharacter.SetFlag(504, false);
//                         }
//                     }
                    _this.MarkDirty();
                    break;
                case BuildStateType.Upgrading:
                    _this.mCharacter.AddExData((int) eExdataDefine.e253, 1);
                    UpgradeOver(_this);
                    //_this.mCharacter.mCity.CityAddExp(_this.TbBuild.GetMainHouseExp);
                    break;
                case BuildStateType.Using:
                    break;
                case BuildStateType.Idle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnDestroy(BuildingBase _this)
        {
            if (null != _this.mOverTrigger)
            {
                LogicServerControl.Timer.DeleteTrigger(_this.mOverTrigger);
                _this.mOverTrigger = null;
            }
        }

        #endregion

        #region 宠物相关

        //指派宠物
        public ErrorCodes AssignPet(BuildingBase _this, int petId)
        {
            //todo 建筑最大放几个宠物
            var maxPet = _this.TbBuild.PetCount;
            if (_this.PetList.Count >= maxPet)
            {
                return ErrorCodes.Error_BuildPetMax;
            }
            var pet = _this.mCharacter.GetPet(petId);
            if (pet == null)
            {
                return ErrorCodes.Error_PetNotFind;
            }
            if (pet.GetState() != (int) PetStateType.Idle)
            {
                return ErrorCodes.Error_PetState;
            }
            //PetExdata64.Insert(PetList.Count, DateTime.Now.ToBinary());
            SetPetIndexTime(_this, _this.PetList.Count, DateTime.Now);
            _this.PetList.Add(petId);
            pet.SetState(PetStateType.Building);
            _this.MarkDirty();
            return ErrorCodes.OK;
        }

        public ErrorCodes AssignPet(BuildingBase _this, int index, int petId)
        {
            //todo 建筑最大放几个宠物
            var maxPet = _this.TbBuild.PetCount;
            if (index >= maxPet)
            {
                return ErrorCodes.Error_BuildPetMax;
            }
            if (_this.PetList.Count < index)
            {
                return ErrorCodes.Error_BuildPetMax;
            }

            var pet = _this.mCharacter.GetPet(petId);
            if (pet == null)
            {
                return ErrorCodes.Error_PetNotFind;
            }
            if (pet.GetState() != (int) PetStateType.Idle)
            {
                return ErrorCodes.Error_PetState;
            }
            //PetExdata64.Insert(PetList.Count, DateTime.Now.ToBinary());
            SetPetIndexTime(_this, index, DateTime.Now);
            //PetList.Add(petId);
            _this.PetList[index] = petId;
            pet.SetState(PetStateType.Building);
            _this.MarkDirty();
            return ErrorCodes.OK;
        }

        //收回宠物
        public ErrorCodes TakeBackPet(BuildingBase _this, int petId)
        {
            var pet = _this.mCharacter.GetPet(petId);
            if (pet == null)
            {
                return ErrorCodes.Error_PetNotFind;
            }

            var index = GetPetIndex(_this, petId);
            if (index == -1)
            {
                return ErrorCodes.Error_BuildNotFindPet;
            }
            //矿洞
            if (_this.Type == BuildingType.Mine || _this.Type == BuildingType.LogPlace)
            {
                //var tbBS = Table.GetBuildingService(TbBuild.ServiceId);
                //计算这个宠物在有效收益时间内，提高了多少量, 
                var willOtherReward = false;
                var CanGetMinCount = GetMineCount(_this, _this.TbBs, ref willOtherReward);
                if (CanGetMinCount >= _this.TbBs.Param[3]) //直接设置上限（不会修正）
                {
                    _this.SetExdata32(0, CanGetMinCount); //到达上限
                }
                else
                {
                    var oldMineCount = _this.GetExdata32(0);
                    var seconds = (int) (DateTime.Now - GetPetWorkTime(_this, index)).TotalSeconds;
                    oldMineCount += GetMinePet(_this.TbBuild.Type, _this.TbBs, pet, seconds);
                    _this.SetExdata32(0, oldMineCount);
                }
            }
            pet.SetState(PetStateType.Idle);
            //GivePetExpByIndex(index, DateTime.Now);
            //移除怪物
            _this.PetList.RemoveAt(index);
            //整理怪物的放置时间
            _this.PetTime.RemoveAt(index);
            _this.MarkDirty();
            return ErrorCodes.OK;
        }

        public ErrorCodes TakeBackPet(BuildingBase _this, int index, int petId)
        {
            var pet = _this.mCharacter.GetPet(petId);
            if (pet == null)
            {
                return ErrorCodes.Error_PetNotFind;
            }
            if (index == -1)
            {
                return ErrorCodes.Error_BuildNotFindPet;
            }
            //矿洞
            if (_this.Type == BuildingType.Mine || _this.Type == BuildingType.LogPlace)
            {
                //var tbBS = Table.GetBuildingService(TbBuild.ServiceId);
                //计算这个宠物在有效收益时间内，提高了多少量
                var willOtherReward = false;
                var CanGetMinCount = GetMineCount(_this, _this.TbBs, ref willOtherReward);
                if (CanGetMinCount >= _this.TbBs.Param[3]) //直接设置上限（不会修正）
                {
                    _this.SetExdata32(0, CanGetMinCount); //到达上限
                }
                else
                {
                    var oldMineCount = _this.GetExdata32(0);
                    var seconds = (int) (DateTime.Now - GetPetWorkTime(_this, index)).TotalSeconds;
                    oldMineCount += GetMinePet(_this.TbBuild.Type, _this.TbBs, pet, seconds);
                    _this.SetExdata32(0, oldMineCount);
                }
            }
            else if (_this.Type == BuildingType.ArenaTemple)
            {
            }
            pet.SetState(PetStateType.Idle);
            //GivePetExpByIndex(index, DateTime.Now);
            //移除怪物
            _this.PetList[index] = -1;
            //整理怪物的放置时间
            _this.PetTime[index] = 0;
            _this.MarkDirty();
            return ErrorCodes.OK;
        }

        public ErrorCodes AssignPetIndex(BuildingBase _this, int index, int petId)
        {
            var ret = ErrorCodes.OK;
            if (_this.PetList[index] != -1)
            {
                ret = TakeBackPet(_this, index, _this.PetList[index]);
            }
            if (petId != -1)
            {
                ret = AssignPet(_this, index, petId);
            }
            _this.MarkDirty();
            return ret;
        }

        public void OnPetChanged(BuildingBase _this, List<PetItem> oldPetList)
        {
            if (_this.Type == BuildingType.MercenaryCamp)
            {
                //var tableService = Table.GetBuildingService(TbBuild.ServiceId);
                var oldPercent = CalculatePetAccTimePercent(_this.TbBuild, _this.TbBs, oldPetList);
                var newPercent = CalculatePetAccTimePercent(_this.TbBuild, _this.TbBs, GetPets(_this));
                if (Math.Abs(oldPercent - newPercent) < 0.001f)
                {
                    return;
                }

                var processed = false;
                for (var i = 0; i < _this.Exdata32.Count && i < _this.Exdata64.Count; i++)
                {
                    if (-1 != _this.Exdata32[i])
                    {
                        var seconds = (DateTime.FromBinary(_this.Exdata64[i]) - DateTime.Now).TotalSeconds;
                        if (seconds <= 0)
                        {
                            continue;
                        }
                        var totalTime = seconds/oldPercent*newPercent;
                        _this.Exdata64[i] = DateTime.Now.AddSeconds(totalTime).ToBinary();

                        processed = true;
                    }
                }
                if (processed)
                {
                    _this.MarkDirty();
                }
            }
            else if (BuildingType.Mine == (BuildingType) _this.TbBuild.Type ||
                     BuildingType.LogPlace == (BuildingType) _this.TbBuild.Type)
            {
//如果是矿洞或者伐木场自动收取资源
                //var tbBS = Table.GetBuildingService(TbBuild.ServiceId);
                if (null != _this.TbBs)
                {
                    var ret = new UseBuildServiceResult();
                    Service1_Mine(_this, _this.TbBs, ref ret);
                }
            }
        }

        public List<PetItem> GetPets(BuildingBase _this)
        {
            //随从整理
            var pets = new List<PetItem>();
            foreach (var i in _this.PetList)
            {
                var pet = _this.mCharacter.GetPet(i);
                if (pet == null)
                {
                    Logger.Warn("GetMineCount pet not find!");
                    continue;
                }
                pets.Add(pet);
            }
            return pets;
        }

        public int GetPetRef(int buildType, BuildingServiceRecord tbBS, PetItem pet, int paramIndex, int oldValue)
        {
            var Bili = 10000;
            pet.ForeachSpecial(skill =>
            {
                if (buildType == skill.Param[0] && skill.EffectId == 0)
                {
                    if (paramIndex == skill.Param[1])
                    {
                        Bili += skill.Param[2];
                    }
                    if (paramIndex == skill.Param[3])
                    {
                        Bili += skill.Param[4];
                    }
                }
                return true;
            });
            var newValue = oldValue*Bili/10000;
            return newValue;
        }

        #endregion

        #region 服务相关

        //是否有某个服务
        public bool IsHaveService(BuildingBase _this, int serviceId)
        {
            if (_this.TbBuild.ServiceId != serviceId)
            {
                return false;
            }
            return true;
        }

        //使用服务
        public ErrorCodes UseService(BuildingBase _this,
                                     int serviceId,
                                     List<int> param,
                                     ref UseBuildServiceResult result)
        {
            if (!IsHaveService(_this, serviceId))
            {
                return ErrorCodes.Error_BuildNotService;
            }
            if (_this.State == BuildStateType.Building)
            {
                return ErrorCodes.Error_BuildStateError;
            }
            //var tbBS = Table.GetBuildingService(serviceId);
            if (_this.TbBs == null)
            {
                return ErrorCodes.Error_BuildServiceID;
            }
            switch (_this.TbBs.BuildingType)
            {
                case 1:
                case 13:
                {
                    return Service1_Mine(_this, _this.TbBs, ref result);
                }
                case 2:
                {
                    return Service2_Plant(_this, _this.TbBs, param, ref result);
                }
                case 3:
                {
                    return Service3_Astrology(_this, _this.TbBs, param, ref result);
                }
                case 5:
                {
                    return Service5_Hatch(_this, _this.TbBs, param, ref result);
                }
                case 6:
                {
                    return Service6_ArenaTemple(_this, _this.TbBs, param, ref result);
                }
                case 8:
                {
                    return Service8_Casting(_this, _this.TbBs, param, ref result);
                }
                case 11:
                {
                    return Service11_Wishing(_this, _this.TbBs, param, ref result);
                }
                case 12:
                {
                    return Service12_Sail(_this, _this.TbBs, param, ref result);
                }
            }

            return ErrorCodes.Error_BuildNotService;
        }

        #endregion

        #region 议事厅

        #endregion

        #region 矿洞

        //收取奖励
        public ErrorCodes Service1_Mine(BuildingBase _this, BuildingServiceRecord tbBS, ref UseBuildServiceResult result)
        {
            if (_this.State != BuildStateType.Idle)
            {
                return ErrorCodes.Error_BuildStateError;
            }
            var willOtherReward = false;
            var mineCount = GetMineCount(_this, tbBS, ref willOtherReward);
            if (willOtherReward)
            {
                //给予额外奖励
            }
            var ResId = tbBS.Param[0]; //建筑资源类型（不会修正）
            if (ResId == 9)
            {
                _this.mCharacter.AddExData((int) eExdataDefine.e258, mineCount);
                GiveReward(_this, 348, mineCount, tbBS, 113, eCreateItemType.Mine);
                _this.mCharacter.mBag.AddItem(ResId, mineCount, eCreateItemType.Mine); //收取资源
            }
            else if (ResId == 8)
            {
                _this.mCharacter.AddExData((int) eExdataDefine.e259, mineCount);
                GiveReward(_this, 349, mineCount, tbBS, 114, eCreateItemType.Wood);
                _this.mCharacter.mBag.AddItem(ResId, mineCount, eCreateItemType.Wood); //收取资源
            }
            //var expBili = tbBS.Param[4]; //建筑资源转化经验比例（不会修正）
            //if (expBili > 0)
            //{
            //    var exp = mineCount*expBili/10000;
            //    if (exp > 0)
            //    {
            //        _this.mCharacter.mCity.CityAddExp(exp);
            //    }
            //}
            _this.StateOverTime = DateTime.Now;
            _this.SetExdata32(0, 0);
            _this.MarkDirty();
            result.Data32.Add(mineCount);
            return ErrorCodes.OK;
        }

        //获得某个宠物能取多少奖励
        private int GetMinePet(int buildType, BuildingServiceRecord tbBS, PetItem pet, int seconds)
        {
            var Bili = 10000;
            pet.ForeachSpecial(skill =>
            {
                if (buildType == skill.Param[0] && skill.EffectId == 0)
                {
                    if (2 == skill.Param[1])
                    {
                        Bili += skill.Param[2];
                    }
                    if (2 == skill.Param[3])
                    {
                        Bili += skill.Param[4];
                    }
                }
                return true;
            });
            var nowCount = (int) (((double) seconds*tbBS.Param[2]*Bili)/10000/3600);
            return nowCount;
        }

        //获取目前能取多少奖励
        private int GetMineCount(BuildingBase _this, BuildingServiceRecord tbBS, ref bool OtherReward)
        {
            var oldTime = _this.StateOverTime;
            var nowTime = DateTime.Now;
            if (nowTime < oldTime)
            {
                return 0;
            }
            //额外奖励
            //if ((nowTime - oldTime).TotalHours >= tbBS.ValidClickDru)
            //{
            //    OtherReward = true;
            //}
            //随从整理
            var pets = GetPets(_this);
            //基础数量
            var param1 = GetBSParamByIndex(_this.TbBuild.Type, tbBS, pets, 1);
            if (oldTime > nowTime)
            {
                return 0;
            }
            var baseCount = (int) ((nowTime - oldTime).TotalSeconds*param1/3600);
            baseCount += _this.GetExdata32(0);
            //宠物数量
            var index = 0;
            foreach (var pet in pets)
            {
                var seconds = (int) (nowTime - GetPetWorkTime(_this, index)).TotalSeconds;
                //baseCount += tbBS.Param[2] * seconds / 3600;
                //baseCount += GetPetRef(TbBuild.Type, tbBS, pet, 2, tbBS.Param[2] * seconds / 3600);
                baseCount += GetMinePet(_this.TbBuild.Type, tbBS, pet, seconds);
                index++;
            }

            //int index = 0;
            //foreach (int i in PetList)
            //{
            //    var pet = mCharacter.GetPet(i);
            //    if (pet == null)
            //    {
            //        continue;
            //    }
            //    int seconds = (int)(nowTime - GetPetWorkTime(index)).TotalSeconds;
            //    baseCount += GetMinePet(tbBS, pet, seconds);
            //    index++;
            //}
            var param3 = GetBSParamByIndex(_this.TbBuild.Type, tbBS, pets, 3);
            if (baseCount >= param3)
            {
                baseCount = param3;
            }
            if (baseCount < 0)
            {
                baseCount = param3;
            }
            return baseCount;
        }

        #endregion

        #region 许愿池
        [Updateable("Building")]
        public static List<DropMotherRecord> Drops = new List<DropMotherRecord>
        {
            Table.GetDropMother(Table.GetServerConfig(250).ToInt()),
            Table.GetDropMother(Table.GetServerConfig(251).ToInt()),
            Table.GetDropMother(Table.GetServerConfig(252).ToInt()),
            Table.GetDropMother(Table.GetServerConfig(253).ToInt()),
            Table.GetDropMother(Table.GetServerConfig(254).ToInt()),
            Table.GetDropMother(Table.GetServerConfig(255).ToInt()),
            Table.GetDropMother(Table.GetServerConfig(256).ToInt()),
            Table.GetDropMother(Table.GetServerConfig(257).ToInt())
        };

        public ErrorCodes Service11_Wishing(BuildingBase _this,
                                            BuildingServiceRecord tbBS,
                                            List<int> param,
                                            ref UseBuildServiceResult resultValue)
        {
            if (param.Count < 2)
            {
                return ErrorCodes.Unknow;
            }
            var result = ErrorCodes.OK;
            var Character = _this.mCharacter;
            var drops = new List<DropMotherRecord>();
            var chatList = new List<int>();
            var value = param[1];
            var passCount = 0;
            var param1 = tbBS.Param[1]; //特长不会修改此参数:抽奖类型总数
            for (var i = 0; i != param1; ++i)
            {
                if (BitFlag.GetLow(value, i))
                {
                    passCount++;
                }
                else
                {
                    drops.Add(Drops[i]);
                }
            }
            if (drops.Count < 1)
            {
                return ErrorCodes.Unknow;
            }
            var motherDropId = 200;
            switch (param[0])
            {
                case 100:
                {
                    //许愿池单抽免费
                    var bag = _this.mCharacter.GetBag((int) eBagType.WishingPool);
                    if (bag.GetFreeCount() < 1)
                    {
                        return ErrorCodes.Error_ItemNoInBag_All;
                    }
                    var param0 = -1;
                    if (Character.lExdata64.GetTime(Exdata64TimeType.FreeWishingTime) < DateTime.Now)
                    {
                        //随从整理
                        var pets = GetPets(_this);
                        //基础数量
                        param0 = GetBSParamByIndex(_this.TbBuild.Type, tbBS, pets, 0);
                        Character.lExdata64.SetTime(Exdata64TimeType.FreeWishingTime, DateTime.Now.AddMinutes(param0));
                    }
                    else
                    {
                        return ErrorCodes.Error_TimeNotOver;
                    }

                    var exdataCount = Character.GetExData((int) eExdataDefine.e270);
                    //第一次抽奖
                    if (exdataCount > 0)
                    {
                        motherDropId = drops.Range().Id;
                    }
                    else
                    {
                        motherDropId = Table.GetServerConfig(212).ToInt();
                    }

                    var itemList = new Dictionary<int, int>();
                    Character.DropMother(motherDropId, itemList);
                    if (itemList.Count != 1)
                    {
                        Logger.Warn("WishingPool itemCount is {0}", itemList.Count);
                        itemList.Clear();
                        itemList[22000] = 1;
                    }
                    foreach (var i in itemList)
                    {
                        _this.mCharacter.mBag.AddItemToWishingPool(i.Key, i.Value, resultValue.Items);
                        chatList.Add(i.Key);
                    }
                    resultValue.Data64.Add(DateTime.Now.AddMinutes(param0).ToBinary());

                    Character.AddExData((int) eExdataDefine.e270, 1);
                    Character.AddExData((int) eExdataDefine.e479, 1);
                    //_this.mCharacter.mCity.CityAddExp(StaticParam.WishingExp);
                    //潜规则引导标记位
                    if (!_this.mCharacter.GetFlag(502))
                    {
                        _this.mCharacter.SetFlag(502);
                        _this.mCharacter.SetFlag(501, false);
                    }
                }
                    break;
                case 101:
                {
                    //许愿池单抽收费
                    var bag = _this.mCharacter.GetBag((int) eBagType.WishingPool);
                    if (bag.GetFreeCount() < 1)
                    {
                        return ErrorCodes.Error_ItemNoInBag_All;
                    }
                    var needResId = Table.GetServerConfig(213).ToInt();
                    var needResCount = Table.GetServerConfig(214).ToInt();
                    if (Character.mBag.GetItemCount(needResId) < needResCount)
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                    //消耗
                    Character.mBag.DeleteItem(needResId, needResCount, eDeleteItemType.Hatch0);

                    //第一次抽奖
                    var exdataCount = Character.GetExData((int) eExdataDefine.e251);
                    if (exdataCount > 0)
                    {
                        motherDropId = drops.Range().Id;
                    }
                    else
                    {
                        motherDropId = Table.GetServerConfig(212).ToInt();
                    }

                    var itemList = new Dictionary<int, int>();
                    Character.DropMother(motherDropId, itemList);
                    if (itemList.Count != 1)
                    {
                        Logger.Warn("WishingPool itemCount is {0}", itemList.Count);
                        itemList.Clear();
                        itemList[22000] = 1;
                    }
                    foreach (var i in itemList)
                    {
                        _this.mCharacter.mBag.AddItemToWishingPool(i.Key, i.Value, resultValue.Items);
                        chatList.Add(i.Key);
                    }
                    resultValue.Data64.Add(DateTime.Now.ToBinary());

                    Character.AddExData((int) eExdataDefine.e270, 1);
                    Character.AddExData((int) eExdataDefine.e479, 1);
                    //_this.mCharacter.mCity.CityAddExp(StaticParam.WishingExp);
                }
                    break;
                case 102:
                {
                    //许愿池10连
                    var bag = _this.mCharacter.GetBag((int) eBagType.WishingPool);
                    if (bag.GetFreeCount() < 10)
                    {
                        return ErrorCodes.Error_ItemNoInBag_All;
                    }
                    var needResId = Table.GetServerConfig(215).ToInt();
                    var needResCount = Table.GetServerConfig(216).ToInt();
                    if (Character.mBag.GetItemCount(needResId) < needResCount)
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                    //消耗
                    Character.mBag.DeleteItem(needResId, needResCount, eDeleteItemType.DrawWishingPool);
                    var exdataCount = Character.GetExData((int) eExdataDefine.e270);
                    var itemList = new Dictionary<int, int>();
                    for (var i = 0; i != 10; ++i)
                    {
                        if (exdataCount > 0)
                        {
                            motherDropId = drops.Range().Id;
                        }
                        else
                        {
                            motherDropId = Table.GetServerConfig(212).ToInt();
                        }
                        //motherDropId = Table.GetServerConfig(211).ToInt();
                        exdataCount++;
                        Character.DropMother(motherDropId, itemList);
                        if (itemList.Count != 1)
                        {
                            Logger.Warn("WishingPool itemCount is {0}", itemList.Count);
                            itemList.Clear();
                            itemList[22000] = 1;
                        }
                        foreach (var j in itemList)
                        {
                            _this.mCharacter.mBag.AddItemToWishingPool(j.Key, j.Value, resultValue.Items);
                            chatList.Add(j.Key);
                        }
                        itemList.Clear();
                    }

                    resultValue.Data64.Add(DateTime.Now.ToBinary());
                    Character.AddExData((int) eExdataDefine.e479, 10);
                    //_this.mCharacter.mCity.CityAddExp(StaticParam.WishingExp*10);
                }
                    break;
            }
            var idIndex = 0;
            var serverId = SceneExtension.GetServerLogicId(Character.serverId);
            foreach (var i in chatList)
            {
                if (GetIsQuality(_this, resultValue.Items[idIndex]))
                {
                    var strs = new List<string>
                    {
                        Character.GetName(),
                        Utils.AddItemId(i)
                    };
                    var exData = new List<int>(resultValue.Items[idIndex].Exdata);
                    var content = Utils.WrapDictionaryId(300417, strs, exData);
                    var chatAgent = LogicServer.Instance.ChatAgent;
                    chatAgent.BroadcastWorldMessage((uint) serverId, (int) eChatChannel.WishingGroup, 0, string.Empty,
                        new ChatMessageContent {Content = content});
                }

                idIndex++;
            }
            chatList.Clear();
            return result;
        }

        //紫色以上判断发送
        public bool GetIsQuality(BuildingBase _this, ItemBaseData item)
        {
            var tbBaseItem = Table.GetItemBase(item.ItemId);
            if (tbBaseItem == null)
            {
                return false;
            }
            if (tbBaseItem.Quality < 3) //test
            {
                return false;
            }
            return true;
        }

        #endregion

        #region 角斗圣殿
        [Updateable("Building")]
        public static int ArenaTempleCd = Table.GetServerConfig(410).ToInt();
        [Updateable("Building")]
        public static int ArenaTempleTotle = Table.GetServerConfig(411).ToInt();
        [Updateable("Building")]
        public static float ArenaSpendDiaUnit = float.Parse(Table.GetServerConfig(572).Value);
        [Updateable("Building")]
        public static int ArenaTempleCDFlagId = 487;

        public ErrorCodes Service6_ArenaTemple(BuildingBase _this,
                                               BuildingServiceRecord tbBS,
                                               List<int> param,
                                               ref UseBuildServiceResult result)
        {
            if (param.Count < 1)
            {
                return ErrorCodes.Unknow;
            }
            switch (param[0])
            {
                case 0:
                    //购买经验
                {
                    if (param.Count < 3)
                    {
                        return ErrorCodes.Unknow;
                    }
                    var StatueIndex = param[1];
                    if (StatueIndex < 0 || StatueIndex >= tbBS.Param[0])
                    {
                        return ErrorCodes.Error_DataOverflow;
                    }
                    var Index = param[2];
                    if (Index < 0 || Index > 2)
                    {
                        return ErrorCodes.Error_DataOverflow;
                    }
                    var pets = GetPets(_this);
                    var maxCount = GetBSParamByIndex(_this.TbBuild.Type, tbBS, pets, 5);
                    //vip增加次数
                    var vipLevel = _this.mCharacter.mBag.GetRes(eResourcesType.VipLevel);
                    var tbVip = Table.GetVIP(vipLevel);
                    maxCount += tbVip.StatueAddCount;
                    if (_this.mCharacter.GetExData(400) >= maxCount)
                    {
                        return ErrorCodes.Error_TempleCountNotEnough;
                    }
                    var cdTime = _this.mCharacter.lExdata64.GetTime(Exdata64TimeType.StatueCdTime);
                    if (cdTime < DateTime.Now)
                    {
                        cdTime = DateTime.Now;
                        _this.mCharacter.SetFlag(ArenaTempleCDFlagId, false);
                    }
                    else
                    {
                        if (_this.mCharacter.GetFlag(ArenaTempleCDFlagId))
                        {
                            return ErrorCodes.Error_TempleNoCD;
                        }
                    }
                    //if (Index == 1)
                    //{
                    //    if (mCharacter.GetExData(401) < 1)
                    //    {
                    //        return ErrorCodes.Error_TempleCountNotEnough;
                    //    }
                    //}
                    //else if (Index == 2)
                    //{
                    //    if (mCharacter.GetExData(402) < 1)
                    //    {
                    //        return ErrorCodes.Error_TempleCountNotEnough;
                    //    }
                    //}
                    var tbStatue = Table.GetStatue(_this.GetExdata32(StatueIndex));
                    var NeedItemIdSkill = Table.GetSkillUpgrading(tbBS.Param[2]).GetSkillUpgradingValue(tbStatue.Level);
                    var NeedItemCountSkill =
                        Table.GetSkillUpgrading(tbBS.Param[3]).GetSkillUpgradingValue(tbStatue.Level);
                    var NeedItemId = Table.GetSkillUpgrading(NeedItemIdSkill).GetSkillUpgradingValue(Index);
                    var NeedItemCount = Table.GetSkillUpgrading(NeedItemCountSkill).GetSkillUpgradingValue(Index);
                    if (_this.mCharacter.mBag.GetItemCount(NeedItemId) < NeedItemCount)
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                    _this.mCharacter.mBag.DeleteItem(NeedItemId, NeedItemCount, eDeleteItemType.StatueExp);
                    var GiveExpSkill = Table.GetSkillUpgrading(tbBS.Param[4]).GetSkillUpgradingValue(tbStatue.Level);
                    var GiveExp = Table.GetSkillUpgrading(GiveExpSkill).GetSkillUpgradingValue(Index);
                    GiveStatueExp(_this, tbStatue, StatueIndex, GiveExp);
                    //var ExpRef = GetBSParamByIndex(_this.TbBuild.Type, tbBS, pets, 6);
                    //_this.mCharacter.mCity.CityAddExp(GiveExp*ExpRef/10000);
                    _this.mCharacter.AddExData(400, 1);
                    if (Index == 1)
                    {
                        _this.mCharacter.AddExData(401, 1);
                    }
                    else if (Index == 2)
                    {
                        _this.mCharacter.AddExData(402, 1);
                    }
                    cdTime = cdTime.AddSeconds(ArenaTempleCd);
                    _this.mCharacter.lExdata64.SetTime(Exdata64TimeType.StatueCdTime, cdTime);
                    if (DateTime.Now.AddSeconds(ArenaTempleTotle) < cdTime)
                    {
                        _this.mCharacter.SetFlag(ArenaTempleCDFlagId);
                    }
                    GiveReward(_this, 405, 1, tbBS, 119, eCreateItemType.ArenaTemple);
                    _this.MarkDirty();
                }
                    break;
                case 1:
                    //购买冷却
                {
                    if (param.Count != 2)
                    {
                        return ErrorCodes.Error_DataOverflow;
                    }
                    var pets = GetPets(_this);
                    var maxCount = GetBSParamByIndex(_this.TbBuild.Type, tbBS, pets, 5);
                    var vipLevel = _this.mCharacter.mBag.GetRes(eResourcesType.VipLevel);
                    var tbVip = Table.GetVIP(vipLevel);
                    if (_this.mCharacter.GetExData((int) eExdataDefine.e400) >= maxCount + tbVip.StatueAddCount)
                    {
                        return ErrorCodes.Error_TempleCountNotEnough;
                    }
                    var cdTime = _this.mCharacter.lExdata64.GetTime(Exdata64TimeType.StatueCdTime);
                    if (cdTime <= DateTime.Now)
                    {
                        _this.mCharacter.lExdata64.SetTime(Exdata64TimeType.StatueCdTime, DateTime.Now);
                        _this.mCharacter.SetFlag(ArenaTempleCDFlagId, false);
                        return ErrorCodes.OK;
                    }
                    var tt = cdTime - DateTime.Now;
                    var needDia = (int) Math.Ceiling((tt.Minutes + 1)*ArenaSpendDiaUnit);
                    if (needDia > param[1])
                    {
                        return ErrorCodes.Unknow;
                    }
                    if (needDia > _this.mCharacter.mBag.GetItemCount((int) eResourcesType.DiamondRes))
                    {
                        return ErrorCodes.ItemNotEnough;
                    }

                    _this.mCharacter.mBag.DeleteItem((int) eResourcesType.DiamondRes, needDia,
                        eDeleteItemType.StatueBuyCooling);
                    _this.mCharacter.lExdata64.SetTime(Exdata64TimeType.StatueCdTime, DateTime.Now);
                    _this.mCharacter.SetFlag(ArenaTempleCDFlagId, false);
                    _this.MarkDirty();
                }
                    break;
                case 2:
                {
                    //计算属性
                }
                    break;
            }
            return ErrorCodes.OK;
        }

        //增加等级
        public void AddStatueLevel(BuildingBase _this, StatueRecord tbStatue, int index)
        {
            _this.SetExdata32(index, tbStatue.NextLevelID);
            _this.mCharacter.mCity.SetAttrFlag();
            _this.mCharacter.AddExData((int) eExdataDefine.e336, 1);
        }

        //增加经验
        public bool GiveStatueExp(BuildingBase _this, StatueRecord tbStatue, int index, int addExp)
        {
            var isLevelUp = false;
            var oldExp = _this.GetExdata64(index) + addExp;
            var needExp = tbStatue.LevelUpExp;
            while (oldExp >= needExp)
            {
                oldExp -= needExp;
                if (tbStatue.NextLevelID == -1)
                {
                    oldExp = tbStatue.LevelUpExp;
                    break;
                }
                AddStatueLevel(_this, tbStatue, index);
                tbStatue = Table.GetStatue(tbStatue.NextLevelID);
                needExp = tbStatue.LevelUpExp;
                isLevelUp = true;
            }

            if (isLevelUp)
            {
                _this.mCharacter.mCity.SetAttrFlag();
                _this.mCharacter.BooksChange();
            }
            _this.SetExdata64(index, oldExp);
            return isLevelUp;
        }

        #endregion

        #region 勇士港

        public int GetSpeed(BuildingBase _this, BuildingServiceRecord tbBS)
        {
            //随从整理
            var pets = GetPets(_this);
            var speed = GetBSParamByIndex(_this.TbBuild.Type, tbBS, pets, 0);
            var PetSpeed = tbBS.Param[1];
            foreach (var pet in pets)
            {
                var petRef = GetPetRef(_this.TbBuild.Type, tbBS, pet, 1, PetSpeed);
                speed += petRef;
            }
            return speed;
        }

        //是否可航行
        public void PointState(BuildingBase _this, int index, SailType sailType)
        {
            if (sailType == SailType.DisPlay)
            {
                _this.SetExdata32(index, _this.GetExdata32(index)%10);
            }
            else if (sailType == SailType.CanPlay)
            {
                _this.SetExdata32(index, _this.GetExdata32(index)%10 + 10);
            }
        }

        //开始或结束航行
        public void LineState(BuildingBase _this, int index, SailType sailType)
        {
            if (sailType == SailType.OverPlay)
            {
                _this.SetExdata32(index, _this.GetExdata32(index)/10*10);
            }
            else if (sailType == SailType.DoPlay)
            {
                _this.SetExdata32(index, 2);
            }
        }

        //是否正在航行
        public bool IsDoPlay(BuildingBase _this, int index)
        {
            return _this.GetExdata32(index)%10 == 2;
        }

        public ErrorCodes Service12_Sail(BuildingBase _this,
                                         BuildingServiceRecord tbBS,
                                         List<int> param,
                                         ref UseBuildServiceResult result)
        {//炼金
            if (param.Count < 1)
            {
                return ErrorCodes.ParamError;
            }
            var count = 0;
            switch (param[0])
            {
                case 0:
                    //出海
                {
                    if (_this.Exdata32.Count == 0)
                    {
                        return ErrorCodes.Error_DataOverflow;
                    }

                    var index = _this.Exdata32[0];
                    
                    var tbSailing = Table.GetSailing(index);
                    if (tbSailing == null)
                    {
                        return ErrorCodes.Error_SailingID;
                    }
                    if (_this.mCharacter.mBag.GetBag((int)eBagType.MedalTemp).GetFirstFreeIndex() == -1)
                    {//包满检测
                        return ErrorCodes.Error_ItemNoInBag_All;
                    }
                    var nowD = _this.mCharacter.mBag.GetRes(eResourcesType.Alchemy);
                    if (nowD < tbSailing.CostCount)
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                    _this.mCharacter.mBag.DelRes(eResourcesType.Alchemy, tbSailing.CostCount, eDeleteItemType.BraveHarbor);

                    var AllitemList = new Dictionary<int, int>();

                    var rnd = MyRandom.Random(10000);
                    if (rnd < tbSailing.SuccessProb)
                    {
                        var itemList = new Dictionary<int, int>();
                        _this.mCharacter.DropMother(tbSailing.SuccessDrop, itemList);
                        foreach (var i in itemList)
                        {
                            var item = Table.GetItemBase(i.Key);
                            if (item.Quality == 4)
                            {
                                var args = new List<string>
                            {
                            Utils.AddCharacter(_this.mCharacter.mGuid,_this.mCharacter.GetName()),
                            item.Name,   
                            };
                                var exExdata = new List<int>();
                                _this.mCharacter.SendSystemNoticeInfo(291010, args, exExdata);
                            }
                            
                            _this.mCharacter.mBag.AddItem(i.Key, i.Value, eCreateItemType.BraveHarbor);
                        }
                        if (tbSailing.SuccessID >= 0)
                        {
                            _this.SetExdata32(0,tbSailing.SuccessID);
                            count = tbSailing.SuccessID;
                        }
                        AllitemList.AddRange(itemList);
                    }
                    else
                    {
                        var itemList = new Dictionary<int, int>();
                        _this.mCharacter.DropMother(tbSailing.FailedDrop, itemList);
                        foreach (var i in itemList)
                        {
                            var item = Table.GetItemBase(i.Key);
                            if (item.Quality == 4)
                            {
                                var args = new List<string>
                            {
                            Utils.AddCharacter(_this.mCharacter.mGuid,_this.mCharacter.GetName()),
                            item.Name,   
                            };
                                var exExdata = new List<int>();
                                _this.mCharacter.SendSystemNoticeInfo(291010, args, exExdata);
                            }

                            _this.mCharacter.mBag.AddItem(i.Key, i.Value, eCreateItemType.BraveHarbor);
                        }
                        if (tbSailing.FailedID >= 0)
                        {
                            _this.SetExdata32(0, tbSailing.FailedID);
                            count = tbSailing.FailedID;
                        }
                        AllitemList.AddRange(itemList);
                    }

                    _this.mCharacter.AddExData((int) eExdataDefine.e416, 1);
                    _this.mCharacter.AddExData((int) eExdataDefine.e344, 1);
                    _this.MarkDirty();

                    try
                    {
                        string str = string.Empty;
                        foreach (var data in AllitemList)
                        {
                            str += data;
                            str += ",";
                        }
                        string v = string.Format("LianjingTimes#{0}|{1}|{2}|{3}|{4}|{5}",
                                             _this.mCharacter.serverId,
                                             _this.mCharacter.mGuid,
                                             _this.mCharacter.GetLevel(), //等级
                                             tbSailing.Id, // 类型
                                             DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), //在线数量
                                             str);
                        PlayerLog.Kafka(v);
                    }
                    catch (Exception)
                    {
                    }
                    
                }
                    break;
                case 4:
                    //直达
                    {
                        var index = param[1];

                        var tbSailing = Table.GetSailing(index);
                        if (tbSailing == null)
                        {
                            return ErrorCodes.Error_SailingID;
                        }
                        if (_this.mCharacter.mBag.GetBag((int)eBagType.MedalTemp).GetFirstFreeIndex() == -1)
                        {//包满检测
                            return ErrorCodes.Error_ItemNoInBag_All;
                        }
                        var nowD = _this.mCharacter.mBag.GetRes(eResourcesType.DiamondRes);
                        if (nowD < tbSailing.ItemCount)
                        {
                            return ErrorCodes.DiamondNotEnough;
                        }
                        _this.mCharacter.mBag.DelRes(eResourcesType.DiamondRes, tbSailing.ItemCount, eDeleteItemType.BraveHarbor);

                        var AllitemList = new Dictionary<int, int>();
                        var rnd = MyRandom.Random(10000);
                        if (rnd < tbSailing.SuccessProb)
                        {
                            var itemList = new Dictionary<int, int>();
                            _this.mCharacter.DropMother(tbSailing.SuccessDrop, itemList);
                            foreach (var i in itemList)
                            {
                                _this.mCharacter.mBag.AddItem(i.Key, i.Value, eCreateItemType.BraveHarbor);
                            }
                            AllitemList.AddRange(itemList);
                        }
                        else
                        {
                            var itemList = new Dictionary<int, int>();
                            _this.mCharacter.DropMother(tbSailing.FailedDrop, itemList);
                            foreach (var i in itemList)
                            {
                                _this.mCharacter.mBag.AddItem(i.Key, i.Value, eCreateItemType.BraveHarbor);
                            }
                            AllitemList.AddRange(itemList);
                        }
                        _this.mCharacter.AddExData((int)eExdataDefine.e416, 1);
                        _this.mCharacter.AddExData((int)eExdataDefine.e344, 1);
                        _this.mCharacter.AddExData((int)eExdataDefine.e345, 1);
                        _this.MarkDirty();

                        try
                        {
                            string str = string.Empty;
                            foreach (var data in AllitemList)
                            {
                                str += data;
                                str += ",";
                            }
                            string v = string.Format("LianjingTimes#{0}|{1}|{2}|{3}|{4}|{5}",
                                                 _this.mCharacter.serverId,
                                                 _this.mCharacter.mGuid,
                                                 _this.mCharacter.GetLevel(), //等级
                                                 tbSailing.Id, // 类型
                                                 DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), //在线数量
                                                 str);
                            PlayerLog.Kafka(v);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;
            }
            result.Data32.Add(count);
            return ErrorCodes.OK;
        }
        #endregion

        #region 计算相关

        public float CalculatePetAccTimePercent(BuildingRecord TbBuild,
                                                BuildingServiceRecord tbBS,
                                                List<PetItem> pets)
        {
            //随从参数修正
            var param0 = GetBSParamByIndex(TbBuild.Type, tbBS, pets, 0);
            var percent = 1 + param0*0.0001f;
            foreach (var pet in pets)
            {
                var addpeed = GetPetRef(TbBuild.Type, tbBS, pet, 3, tbBS.Param[3]);
                percent += addpeed*0.0001f;
            }
            percent = 1/percent;
            //int param3 = GetBSParamByIndex(TbBuild.Type, tbBS, pets, 3);
            return percent; //Math.Min(Math.Max(0, percent), 1);
        }

        //读取建筑服务参数
        public int GetBSParamByIndex(int buildType, BuildingServiceRecord tbBS, List<PetItem> petItems, int index)
        {
            var tbValue = tbBS.Param[index];
            var Bili = 10000;
            foreach (var pet in petItems)
            {
                pet.ForeachSpecial(skill =>
                {
                    if (buildType == skill.Param[0] && skill.EffectId == 0)
                    {
                        if (index == skill.Param[1])
                        {
                            Bili += skill.Param[2];
                        }
                        if (index == skill.Param[3])
                        {
                            Bili += skill.Param[4];
                        }
                    }
                    return true;
                });
            }
            return tbValue*Bili/10000;
        }

        //读取铁匠铺建筑服务参数
        public int GetBSParamByIndexForSmithy(int buildType,
                                              BuildingServiceRecord tbBS,
                                              List<PetItem> petItems,
                                              int index)
        {
            var value = tbBS.Param[index];
            foreach (var pet in petItems)
            {
                pet.ForeachSpecial(skill =>
                {
                    if (buildType == skill.Param[0] && skill.EffectId == 0)
                    {
                        if (index == skill.Param[1])
                        {
                            value += skill.Param[2];
                        }
                        if (index == skill.Param[3])
                        {
                            value += skill.Param[4];
                        }
                    }
                    return true;
                });
            }
            return value;
        }

        //读取植物产出附加值
        public int GetPlantHarvestAdd(PlantRecord tbPlant, List<PetItem> petItems)
        {
            var HarvestAdd = 0;
            foreach (var pet in petItems)
            {
                pet.ForeachSpecial(skill =>
                {
                    if (tbPlant.PlantType == skill.Param[0] && skill.EffectId == 2)
                    {
                        if (0 != skill.Param[2])
                        {
                            HarvestAdd += skill.Param[2];
                        }
                    }
                    return true;
                });
            }
            return HarvestAdd;
        }

        //读取植物产出需求时间修正
        public int GetPlantNeedTime(PlantRecord tbPlant, List<PetItem> petItems)
        {
            var TimeRef = 0;
            foreach (var pet in petItems)
            {
                pet.ForeachSpecial(skill =>
                {
                    if (tbPlant.PlantType == skill.Param[0] && skill.EffectId == 2)
                    {
                        if (0 != skill.Param[1])
                        {
                            TimeRef += skill.Param[1];
                        }
                    }
                    return true;
                });
            }
            return TimeRef;
        }

        #endregion
    }

    public class BuildingBase : NodeBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IBuildingBase mStaticImpl;

        static BuildingBase()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (BuildingBase), typeof (BuildingBaseDefaultImpl),
                o => { mStaticImpl = (IBuildingBase) o; });
        }
        public  Dictionary<int, int> TempReward = new Dictionary<int, int>();
        public  Dictionary<int, int> itemReward = new Dictionary<int, int>();

        public CharacterController mCharacter; //所在角色
        public DBBuild_One mDbData;
        public Trigger mOverTrigger;
        public BuildingServiceRecord TbBs;
        public BuildingRecord TbBuild;
        public OrderUpdateRecord TbOu;

        public static int GetBSParamByIndex(int buildType, BuildingServiceRecord tbBS, List<PetItem> petItems, int index)
        {
            return mStaticImpl.GetBSParamByIndex(buildType, tbBS, petItems, index);
        }

        public BuildingData GetBuildingData()
        {
            return mStaticImpl.GetBuildingData(this);
        }

        #region 额外奖励相关

        /// <summary>
        ///     额外奖励相关
        /// </summary>
        /// <param name="exdataId"> 扩展计数ID </param>
        /// <param name="addValue"> 增加值 </param>
        /// <param name="TbBS"> 建筑服务表 </param>
        /// <param name="maildId"> 邮件ID </param>
        /// <param name="because"> 原因 </param>
        /// <param name="rewardIsEmpty">  </param>
        public void GiveReward(int exdataId,
                               int addValue,
                               BuildingServiceRecord TbBS,
                               int maildId,
                               eCreateItemType because,
                               bool rewardIsEmpty = true)
        {
            mStaticImpl.GiveReward(this, exdataId, addValue, TbBS, maildId, because, rewardIsEmpty);
        }

        #endregion

        #region 矿洞

        //收取奖励
        public ErrorCodes Service1_Mine(BuildingServiceRecord tbBS, ref UseBuildServiceResult result)
        {
            return mStaticImpl.Service1_Mine(this, tbBS, ref result);
        }

        #endregion

        #region 农场

        public ErrorCodes Service2_Plant(BuildingServiceRecord tbBS, List<int> param, ref UseBuildServiceResult result)
        {
            return mStaticImpl.Service2_Plant(this, tbBS, param, ref result);
        }

        #endregion

        #region 占星台

        public ErrorCodes Service3_Astrology(BuildingServiceRecord tbBS,
                                             List<int> param,
                                             ref UseBuildServiceResult resultValue)
        {
            return mStaticImpl.Service3_Astrology(this, tbBS, param, ref resultValue);
        }

        #endregion

        #region 孵化室

        public ErrorCodes Service5_Hatch(BuildingServiceRecord tbBS, List<int> param, ref UseBuildServiceResult result)
        {
            return mStaticImpl.Service5_Hatch(this, tbBS, param, ref result);
        }

        #endregion

        #region 铁匠屋

        public ErrorCodes Service8_Casting(BuildingServiceRecord tbBS, List<int> param, ref UseBuildServiceResult result)
        {
            return mStaticImpl.Service8_Casting(this, tbBS, param, ref result);
        }

        #endregion

        //public IBuildingBase mBuildingBaseImpl;

        #region 快捷引用DB数据

        public int Guid
        {
            get { return mDbData.Guid; }
        }

        public int TypeId
        {
            get { return mDbData.TypeId; }
            set { mDbData.TypeId = value; }
        }

        public int AreaId
        {
            get { return mDbData.AreaId; }
            set { mDbData.AreaId = value; }
        }

        public BuildStateType State
        {
            get { return (BuildStateType) mDbData.State; }
            set { mDbData.State = (int) value; }
        }

        public BuildingType Type
        {
            get { return (BuildingType) TbBuild.Type; }
        }

        public List<int> Exdata32
        {
            get { return mDbData.Exdata; }
        }

        public List<Int64> Exdata64
        {
            get { return mDbData.Exdata64; }
        }

        public List<Int64> PetTime
        {
            get { return mDbData.PetTime; }
        }

        public List<int> PetList
        {
            get { return mDbData.PetList; }
        }

        public DateTime StateOverTime
        {
            get { return DateTime.FromBinary(mDbData.StateOverTime); }
            set { mDbData.StateOverTime = value.ToBinary(); }
        }

        #endregion

        #region 32位扩展数据方法

        public int GetExdata32(int nIndex)
        {
            if (Exdata32.Count <= nIndex)
            {
                return -1;
            }
            return Exdata32[nIndex];
        }

        public void SetExdata32(int nIndex, int nValue)
        {
            var nNowCount = Exdata32.Count;
            if (nNowCount <= nIndex)
            {
                Logger.Log(LogLevel.Warn,
                    string.Format("SetExdata32 AreaId={0},NowCount={1},SetIndex={2},Value={3}", AreaId, nNowCount,
                        nIndex, nValue));
                //要设置的数据位数不足，用-1补足
                for (var i = nNowCount; i <= nIndex; ++i)
                {
                    AddExdata32(-1);
                }
            }
            Exdata32[nIndex] = nValue;
        }

        //增加扩展数据
        public void AddExdata32(int nValue)
        {
            Exdata32.Add(nValue);
        }

        #endregion

        #region 64位扩展数据方法

        public Int64 GetExdata64(int nIndex)
        {
            if (Exdata64.Count <= nIndex)
            {
                return -1;
            }
            return Exdata64[nIndex];
        }

        public void SetExdata64(int nIndex, Int64 nValue)
        {
            var nNowCount = Exdata64.Count;
            if (nNowCount <= nIndex)
            {
                Logger.Log(LogLevel.Warn,
                    string.Format("SetExdata64 AreaId={0},NowCount={1},SetIndex={2},Value={3}", AreaId, nNowCount,
                        nIndex, nValue));
                //要设置的数据位数不足，用-1补足
                for (var i = nNowCount; i <= nIndex; ++i)
                {
                    AddExdata64(-1);
                }
            }
            Exdata64[nIndex] = nValue;
        }

        //增加扩展数据
        public void AddExdata64(Int64 nValue)
        {
            Exdata64.Add(nValue);
        }

        #endregion

        #region 宠物放置的时间

        //获得宠物索引
        public int GetPetIndex(int petId)
        {
            return mStaticImpl.GetPetIndex(this, petId);
        }

        //获得Pet的放置时间
        public DateTime GetPetIndexTime(int index)
        {
            return mStaticImpl.GetPetIndexTime(this, index);
        }

        //设置Pet的放置时间
        public void SetPetIndexTime(int nIndex, DateTime nValue)
        {
            mStaticImpl.SetPetIndexTime(this, nIndex, nValue);
        }

        //获得Pet的实际工作时间
        public DateTime GetPetWorkTime(int index)
        {
            return mStaticImpl.GetPetWorkTime(this, index);
        }


        //给宠物经验
        public void GivePetExp(DateTime lastTime)
        {
            mStaticImpl.GivePetExp(this, lastTime);
        }

        //给某个宠物经验
        public void GivePetExpByIndex(int index, DateTime lastTime)
        {
            mStaticImpl.GivePetExpByIndex(this, index, lastTime);
        }

        #endregion

        #region 构造相关

        //构造建筑
        public BuildingBase(CharacterController character, DBBuild_One dbdata)
        {
            mStaticImpl.Init(this, character, dbdata);
        }

        public BuildingBase(CharacterController character, int guid, BuildingRecord tbBuild)
        {
            mStaticImpl.Init(this, character, guid, tbBuild);
        }

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        //重置建筑数据
        public void Reset(int guid, int areaId, BuildingRecord tbBuild, BuildStateType type)
        {
            mStaticImpl.Reset(this, guid, areaId, tbBuild, type);
        }

        //更新建筑的扩展数据
        public void ResetExdata()
        {
            mStaticImpl.ResetExdata(this);
        }

        //更新建筑的数据
        public void UpdataExdata()
        {
            mStaticImpl.UpdataExdata(this);
        }

        public ErrorCodes Upgrade()
        {
            return mStaticImpl.Upgrade(this);
        }

        public ErrorCodes Speedup()
        {
            return mStaticImpl.Speedup(this);
        }

        public void UpgradeOver()
        {
            mStaticImpl.UpgradeOver(this);
        }

        public void StartTrigger(DateTime dateTime)
        {
            mStaticImpl.StartTrigger(this, dateTime);
        }

        public void TimeOver()
        {
            mStaticImpl.TimeOver(this);
        }

        public void OnDestroy()
        {
            mStaticImpl.OnDestroy(this);
        }

        #endregion

        #region 宠物相关

        //指派宠物
        public ErrorCodes AssignPet(int petId)
        {
            return mStaticImpl.AssignPet(this, petId);
        }

        public ErrorCodes AssignPet(int index, int petId)
        {
            return mStaticImpl.AssignPet(this, index, petId);
        }

        //收回宠物
        public ErrorCodes TakeBackPet(int petId)
        {
            return mStaticImpl.TakeBackPet(this, petId);
        }

        public ErrorCodes TakeBackPet(int index, int petId)
        {
            return mStaticImpl.TakeBackPet(this, index, petId);
        }

        public ErrorCodes AssignPetIndex(int index, int petId)
        {
            return mStaticImpl.AssignPetIndex(this, index, petId);
        }

        public void OnPetChanged(List<PetItem> oldPetList)
        {
            mStaticImpl.OnPetChanged(this, oldPetList);
        }

        public List<PetItem> GetPets()
        {
            return mStaticImpl.GetPets(this);
        }

        public static int GetPetRef(int buildType, BuildingServiceRecord tbBS, PetItem pet, int paramIndex, int oldValue)
        {
            return mStaticImpl.GetPetRef(buildType, tbBS, pet, paramIndex, oldValue);
        }

        #endregion

        #region 服务相关

        //是否有某个服务
        public bool IsHaveService(int serviceId)
        {
            return mStaticImpl.IsHaveService(this, serviceId);
        }

        //使用服务
        public ErrorCodes UseService(int serviceId, List<int> param, ref UseBuildServiceResult result)
        {
            return mStaticImpl.UseService(this, serviceId, param, ref result);
        }

        #endregion

        #region 议事厅

        #endregion

        #region 许愿池

        public ErrorCodes Service11_Wishing(BuildingServiceRecord tbBS,
                                            List<int> param,
                                            ref UseBuildServiceResult resultValue)
        {
            return mStaticImpl.Service11_Wishing(this, tbBS, param, ref resultValue);
        }

        //紫色以上判断发送
        public bool GetIsQuality(ItemBaseData item)
        {
            return mStaticImpl.GetIsQuality(this, item);
        }

        #endregion

        #region 角斗圣殿

        public ErrorCodes Service6_ArenaTemple(BuildingServiceRecord tbBS,
                                               List<int> param,
                                               ref UseBuildServiceResult result)
        {
            return mStaticImpl.Service6_ArenaTemple(this, tbBS, param, ref result);
        }

        //增加等级
        public void AddStatueLevel(StatueRecord tbStatue, int index)
        {
            mStaticImpl.AddStatueLevel(this, tbStatue, index);
        }

        //增加经验
        public bool GiveStatueExp(StatueRecord tbStatue, int index, int addExp)
        {
            return mStaticImpl.GiveStatueExp(this, tbStatue, index, addExp);
        }

        #endregion

        #region 勇士港

        public int GetSpeed(BuildingServiceRecord tbBS)
        {
            return mStaticImpl.GetSpeed(this, tbBS);
        }

        //是否可航行
        public void PointState(int index, SailType sailType)
        {
            mStaticImpl.PointState(this, index, sailType);
        }

        //开始或结束航行
        public void LineState(int index, SailType sailType)
        {
            mStaticImpl.LineState(this, index, sailType);
        }

        //是否正在航行
        public bool IsDoPlay(int index)
        {
            return mStaticImpl.IsDoPlay(this, index);
        }

        public ErrorCodes Service12_Sail(BuildingServiceRecord tbBS, List<int> param, ref UseBuildServiceResult result)
        {
            return mStaticImpl.Service12_Sail(this, tbBS, param, ref result);
        }

        #endregion
    }
}