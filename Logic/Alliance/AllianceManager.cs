#region using

using System.Collections.Generic;
using DataContract;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IAllianceManager
    {
        ErrorCodes CheckDonationAllianceItem(AllianceManager _this, int type);
        ErrorCodes CheckOperation(AllianceManager _this, int type);
        ErrorCodes CheckOver(AllianceManager _this, int type, int SendValue, int ResultValue);
        void CleanApplyList(AllianceManager _this);
        ErrorCodes CreateNewAlliance(AllianceManager _this, string allianceName);
        CharacterAllianceData InitByBase(AllianceManager _this, CharacterController character);
        void InitByDB(AllianceManager _this, CharacterController character, CharacterAllianceData allianceData);
        void SetAllianceId(AllianceManager _this, int value);
        void SuccessDonationAllianceItem(AllianceManager _this, int type, int level);
    }

    public class AllianceManagerDefaultImpl : IAllianceManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void SetAllianceId(AllianceManager _this, int value)
        {
            _this.mDbData.AllianceId = value;
            _this.mCharacter.SetExData(282, value);
        }

        #region 初始化

        public CharacterAllianceData InitByBase(AllianceManager _this, CharacterController character)
        {
            _this.mDbData = new CharacterAllianceData();
            _this.mCharacter = character;
            _this.MarkDirty();
            return _this.mDbData;
        }

        public void InitByDB(AllianceManager _this, CharacterController character, CharacterAllianceData allianceData)
        {
            _this.mCharacter = character;
            _this.mDbData = allianceData;
            if (_this.mDbData.State != (int) AllianceState.Have)
            {
                _this.mDbData.State = (int) AllianceState.None;
            }
        }

        #endregion

        #region 操作方法

        //清空申请列表
        public void CleanApplyList(AllianceManager _this)
        {
            _this.mCharacter.SetExData(286, 0);
            _this.mCharacter.SetExData(287, 0);
            _this.mCharacter.SetExData(288, 0);
        }

        private void PushApplyList(AllianceManager _this, int aId)
        {
            var e286 = _this.mCharacter.GetExData(286);
            if (e286 == 0)
            {
                _this.mCharacter.SetExData(286, aId);
                return;
            }
            if (e286 == aId)
            {
                return;
            }
            var e287 = _this.mCharacter.GetExData(287);
            if (e287 == 0)
            {
                _this.mCharacter.SetExData(287, aId);
                return;
            }
            if (e287 == aId)
            {
                return;
            }
            if (_this.mCharacter.GetExData(288) == 0)
            {
                _this.mCharacter.SetExData(288, aId);
            }
        }

        //请求创建
        public ErrorCodes CreateNewAlliance(AllianceManager _this, string allianceName)
        {
            if (_this.State == AllianceState.Have)
            {
                return ErrorCodes.Error_CharacterHaveAlliance;
            }
            if (_this.State != AllianceState.None)
            {
                return ErrorCodes.Error_AllianceState;
            }
            if (_this.mCharacter.GetLevel() < AllianceManager.CreateNewAllianceNeedLevel)
            {
                return ErrorCodes.Error_LevelNoEnough;
            }
            if (_this.mCharacter.mBag.GetRes(eResourcesType.GoldRes) < AllianceManager.CreateNewAllianceNeedMoney)
            {
                return ErrorCodes.MoneyNotEnough;
            }
            return ErrorCodes.OK;
        }

        //请求加入
        public ErrorCodes CheckOperation(AllianceManager _this, int type)
        {
            //type：0=申请加入（value=战盟ID）  1=取消申请（value=战盟ID）  2=退出战盟   3=同意邀请（value=战盟ID）  4=拒绝邀请（value=战盟ID）
            switch (type)
            {
                case 0:
                {
                    if (_this.mCharacter.GetLevel() < AllianceManager.EnjoinAllianceNeedLevel)
                    {
                        return ErrorCodes.Error_LevelNoEnough;
                    }
                    if (_this.State != AllianceState.None)
                    {
                        return ErrorCodes.Error_AllianceState;
                    }
                    if (_this.mCharacter.GetExData(286) != 0 && _this.mCharacter.GetExData(287) != 0 &&
                        _this.mCharacter.GetExData(288) != 0)
                    {
                        return ErrorCodes.Error_AllianceApplyIsFull;
                    }
                }
                    break;
                case 1:
                {
                }
                    break;
                case 2:
                {
                    if (_this.State != AllianceState.Have)
                    {
                        return ErrorCodes.Error_CharacterNoAlliance;
                    }
                }
                    break;
                case 3:
                {
                }
                    break;
                case 4:
                {
                }
                    break;
            }
            return ErrorCodes.OK;
        }

        //请求
        public ErrorCodes CheckOver(AllianceManager _this, int type, int SendValue, int ResultValue)
        {
            //type：-1申请加入，并且自动加入成功, 0=申请加入（value=战盟ID）  1=取消申请（value=战盟ID）  2=退出战盟   3=同意邀请（value=战盟ID）  4=拒绝邀请（value=战盟ID）
            switch (type)
            {
                case -1:
                {
                    CleanApplyList(_this);
                    _this.State = AllianceState.Have;
                    _this.AllianceId = SendValue;
                    _this.Ladder = 0;
                    _this.mCharacter.SetFlag(2801);
                }
                    break;
                case 0:
                {
                    PushApplyList(_this, SendValue);
                }
                    break;
                case 1:
                {
                    if (_this.mCharacter.GetExData(286) == SendValue)
                    {
                        _this.mCharacter.SetExData(286, 0);
                    }
                    if (_this.mCharacter.GetExData(287) == SendValue)
                    {
                        _this.mCharacter.SetExData(287, 0);
                    }
                    if (_this.mCharacter.GetExData(288) == SendValue)
                    {
                        _this.mCharacter.SetExData(288, 0);
                    }
                }
                    break;
                case 2:
                {
                    _this.State = AllianceState.None;
                    _this.AllianceId = 0;
                    _this.Ladder = 0;
                }
                    break;
                case 3:
                {
                    _this.State = AllianceState.Have;
                    _this.AllianceId = SendValue;
                    CleanApplyList(_this);
                    _this.Ladder = 0;
                    _this.mCharacter.SetFlag(2801);
                }
                    break;
                case 4:
                {
                }
                    break;
            }
            return ErrorCodes.OK;
        }

        //捐献判断条件
        public ErrorCodes CheckDonationAllianceItem(AllianceManager _this, int type)
        {
            if (_this.State != AllianceState.Have)
            {
                return ErrorCodes.Error_AllianceState;
            }
            switch (type)
            {
                case 0:
                case 1:
                case 2:
                {
                    var todayCount = _this.mCharacter.GetExData((int) eExdataDefine.e285);
                    if (todayCount >= 10)
                    {
                        return ErrorCodes.Error_AllianceDonationCount;
                    }
                }
                    break;
                default:
                {
                    var todayCount = _this.mCharacter.GetExData((int) eExdataDefine.e284);
                    if (todayCount >= AllianceManager.GongjiMax)
                    {
                        return ErrorCodes.Error_AllianceDonationCount;
                    }
                }
                    break;
            }
            return ErrorCodes.OK;
        }

        //捐献成功
        public void SuccessDonationAllianceItem(AllianceManager _this, int type, int level)
        {
            switch (type)
            {
                case 0:
                {
                    var tbGuild = Table.GetGuild(level);
                    if (tbGuild == null)
                    {
                        return;
                    }
                    //if (mCharacter.mBag.DelRes(eResourcesType.GoldRes, tbGuild.LessNeedCount) != ErrorCodes.OK)
                    //{
                    //    return;
                    //}
                    _this.mCharacter.mBag.AddRes(eResourcesType.Contribution, tbGuild.LessUnionDonation,
                        eCreateItemType.SuccessDonation);
                    _this.mCharacter.AddExData((int) eExdataDefine.e38, 1);
                    _this.mCharacter.AddExData((int) eExdataDefine.e329, 1);
                    _this.mCharacter.AddExData((int) eExdataDefine.e285, 1);
                }
                    break;
                case 1:
                {
                    var tbGuild = Table.GetGuild(level);
                    if (tbGuild == null)
                    {
                        return;
                    }
                    //if (mCharacter.mBag.mCharacter.mBag.DelRes(eResourcesType.GoldRes, tbGuild.MoreNeedCount) != ErrorCodes.OK)
                    //{
                    //    return;
                    //}
                    _this.mCharacter.mBag.AddRes(eResourcesType.Contribution, tbGuild.MoreUnionDonation,
                        eCreateItemType.SuccessDonation);
                    _this.mCharacter.AddExData((int) eExdataDefine.e38, 1);
                    _this.mCharacter.AddExData((int) eExdataDefine.e329, 1);
                    _this.mCharacter.AddExData((int) eExdataDefine.e285, 1);
                }
                    break;
                case 2:
                {
                    var tbGuild = Table.GetGuild(level);
                    if (tbGuild == null)
                    {
                        return;
                    }
                    //if (mCharacter.mBag.DelRes(eResourcesType.DiamondRes, tbGuild.DiaNeedCount)!=ErrorCodes.OK)
                    //{
                    //    return;
                    //}
                    _this.mCharacter.mBag.AddRes(eResourcesType.Contribution, tbGuild.DiaUnionDonation,
                        eCreateItemType.SuccessDonation);
                    _this.mCharacter.AddExData((int) eExdataDefine.e38, 1);
                    _this.mCharacter.AddExData((int) eExdataDefine.e329, 1);
                    _this.mCharacter.AddExData((int) eExdataDefine.e285, 1);
                }
                    break;
                default:
                {
                    var tbGuildMiss = Table.GetGuildMission(type);
                    if (tbGuildMiss == null)
                    {
                        Logger.Warn("DonationAllianceItem type={0}", type);
                        return;
                    }
                    _this.mCharacter.mBag.AddRes(eResourcesType.Contribution, tbGuildMiss.GetGongJi,
                        eCreateItemType.SuccessDonation);
                    //mCharacter.AddExData((int)eExdataDefine.e417, tbGuildMiss.GetGongJi);  重复
                    _this.mCharacter.AddExData((int) eExdataDefine.e284, tbGuildMiss.GetGongJi);
                }
                    break;
            }
        }

        #endregion
    }

    public enum AllianceState
    {
        None = 0, //初始
        NetCreate = 1, //请求创建
        //NetJoin = 2,        //请求加入
        Have = 99 //已有战盟
    }

    public class AllianceManager : NodeBase
    {
        public static int CreateNewAllianceNeedLevel = Table.GetServerConfig(242).ToInt();
        public static int CreateNewAllianceNeedMoney = Table.GetServerConfig(241).ToInt();
        public static int EnjoinAllianceNeedLevel = Table.GetServerConfig(243).ToInt();
        public static int GongjiMax = Table.GetServerConfig(280).ToInt();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //
        private static IAllianceManager mImpl;

        static AllianceManager()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (AllianceManager),
                typeof (AllianceManagerDefaultImpl),
                o => { mImpl = (IAllianceManager) o; });
        }

        public CharacterController mCharacter; //所在角色
        public CharacterAllianceData mDbData;

        public int AllianceId
        {
            get { return mDbData.AllianceId; }
            set { mImpl.SetAllianceId(this, value); }
        }

        #region 继承方法

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        #endregion

        public int Ladder
        {
            get { return mDbData.Ladder; }
            set { mDbData.Ladder = value; }
        }

        public AllianceState State
        {
            get { return (AllianceState) mDbData.State; }
            set { mDbData.State = (int) value; }
        }

        #region 初始化

        public CharacterAllianceData InitByBase(CharacterController character)
        {
            return mImpl.InitByBase(this, character);
        }

        public void InitByDB(CharacterController character, CharacterAllianceData allianceData)
        {
            mImpl.InitByDB(this, character, allianceData);
        }

        #endregion

        #region 操作方法

        //清空申请列表
        public void CleanApplyList()
        {
            mImpl.CleanApplyList(this);
        }

        //请求创建
        public ErrorCodes CreateNewAlliance(string allianceName)
        {
            return mImpl.CreateNewAlliance(this, allianceName);
        }

        //请求加入
        public ErrorCodes CheckOperation(int type)
        {
            return mImpl.CheckOperation(this, type);
        }

        //请求
        public ErrorCodes CheckOver(int type, int sendValue, int resultValue)
        {
            return mImpl.CheckOver(this, type, sendValue, resultValue);
        }

        //捐献判断条件
        public ErrorCodes CheckDonationAllianceItem(int type)
        {
            return mImpl.CheckDonationAllianceItem(this, type);
        }

        //捐献成功
        public void SuccessDonationAllianceItem(int type, int level)
        {
            mImpl.SuccessDonationAllianceItem(this, type, level);
        }

        #endregion
    }
}