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
    public interface IMountManager
    {
        void RefreshAttr(MountManager _this);
        bool Ride(MountManager _this, int id);
        ErrorCodes MountUp(MountManager _this);
        ErrorCodes SkillUp(MountManager _this,int skillId);
        ErrorCodes MountFeed(MountManager _this, int itemId);
        bool AddGift(MountManager _this, int id);
        ErrorCodes AddSkin(MountManager _this, int id);
        void RefreshSkinState(MountManager _this);
        bool RemoveSkin(MountManager _this, int id);
        MountData GetMountData(MountManager _this);
        void DeleteMount(MountManager _this, int id);
        int GetFightPoint(MountManager _this, int characterLevel, int roleId);
    }

    public class MountManagerDefaultImpl : IMountManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void RefreshAttr(MountManager _this)
        {
            _this.Mountattrs.Clear();

            foreach (var obj in _this.mDbData.Special)
            {//特殊坐骑属性
                var tb = Table.GetMount(obj.Key);
                if (tb != null)
                {
                    for (int i = 0; i < tb.Attr.Length && i < tb.Value.Length; i++)
                    {
                        if (tb.Attr[i] > 0 && tb.Value[i] > 0)
                            _this.Mountattrs.modifyValue(tb.Attr[i], tb.Value[i]);
                    }
                }
            }

            var data = _this.mDbData;
            if (data.Id <= 0)
                return;
            var tbMount = Table.GetMount(data.Id);
            if (tbMount == null)
                return;
            for (int i = 0; i < tbMount.Attr.Length && i < tbMount.Value.Length; i++)
            {
                if (tbMount.Attr[i] > 0 && tbMount.Value[i] > 0)
                    _this.Mountattrs.modifyValue(tbMount.Attr[i], tbMount.Value[i]);
            }
            foreach (var i in _this.mDbData.Attrs)
            {
                _this.Mountattrs.modifyValue(i.Key, i.Value);
            }
        }
        public bool Ride(MountManager _this, int id)
        {
            RefreshSkinState(_this);
            var tb = Table.GetMount(id);
            if (tb != null)
            {
                if (tb.Special <= 0 && tb.Level + tb.Step*100 > _this.mDbData.Level + _this.mDbData.Step*100)
                {
                    return false;
                }
                if (tb.Special > 0 && false == _this.mDbData.Special.ContainsKey(id))
                    return false;
                _this.mCharacter.Mount(id);
                _this.mDbData.Ride = id;
                _this.MarkDbDirty();
                return true;
            }
            return false;
        }

        public ErrorCodes MountUp(MountManager _this)
        {
            var tb = Table.GetMount(_this.mDbData.Id);
            if(tb == null)
                return ErrorCodes.Unknow;
            var tbNext = Table.GetMount(tb.NextId);
            if (tbNext == null)
                return ErrorCodes.Unknow;
            if (tbNext.IsOpen == 0)
                return ErrorCodes.Error_Mount_MAX_LEVEL;
            _this.mDbData.Exp += tb.GetExp;
            _this.mCharacter.SetFlag(2683);

            if (_this.mDbData.Exp >= tb.NeedExp)
            {
                _this.mDbData.Id = tb.NextId;
                _this.mDbData.Exp -= tb.NeedExp;
                _this.mDbData.Step = tbNext.Step;
                _this.mDbData.Level = tbNext.Level;
                if (tbNext.SkillId > 0)
                {//开启技能
                    _this.mDbData.Skills.Add(tbNext.SkillId, 0);
                }
                RefreshAttr(_this);
                _this.mCharacter.BooksChange();
                _this.mCharacter.SetRankFlag(RankType.Mount);

                if (_this.mDbData.Ride <= 0)
                    _this.mDbData.Ride = _this.mDbData.Id;
                else if(tbNext.Level == 1)
                {
                    var tbRide = Table.GetMount(_this.mDbData.Ride);
                    if (tbRide != null && tbRide.Special <=0 && tbNext.Step>tbRide.Step)
                    {//当前骑乘的是非特殊坐骑
                        _this.mDbData.Ride = _this.mDbData.Id;
                    }
                }
                if (tb.Step < tbNext.Step)
                {

                    var item = Table.GetItemBase(tbNext.ItemId);
                    var args = new List<string>
                    {
                    Utils.AddCharacter(_this.mCharacter.mGuid,_this.mCharacter.GetName()),
                    string.Format("[{0}]{1}[-]",Utils.GetTableColorString(item.Quality),item.Name),  
                    };
                    var exExdata = new List<int>();
                    _this.mCharacter.SendSystemNoticeInfo(291009, args, exExdata);

                    _this.mCharacter.AddExData((int)eExdataDefine.e740,1);
                    var diamond = _this.mCharacter.GetExData((int) eExdataDefine.e78_TotalRechargeDiamond);
                    var klog = string.Format("ride_advance#{0}|{1}|{2}|{3}|{4}|{5}",
                          _this.mCharacter.mGuid,
                          _this.mCharacter.GetLevel(),
                          _this.mCharacter.serverId,
                          tb.Step,
                          diamond,
                          DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    PlayerLog.Kafka(klog, 2); 
                }
            }
           
            _this.MarkDbDirty();
            return ErrorCodes.OK;
        }

        public ErrorCodes SkillUp(MountManager _this, int skillId)
        {
            _this.mDbData.Skills.modifyValue(skillId, 1);
            _this.MarkDbDirty();
            CharacterController.mImpl.AddBuff(_this.mCharacter, skillId, _this.mDbData.Skills[skillId]);
            return ErrorCodes.OK;
        }

        public void DeleteMount(MountManager _this, int id)
        {
            var tb = Table.GetMount(id);
            if (tb == null)
                return ;
            if (tb.Special > 0)
            {
                if (_this.mDbData.Special.ContainsKey(id) == false)
                {
                    return;
                }
                _this.mDbData.Special.Remove(id);
                RefreshAttr(_this);
                _this.mCharacter.BooksChange();
                _this.mCharacter.SetRankFlag(RankType.Mount);
                _this.MarkDbDirty();
            }
            if (_this.mDbData.Special.Count > 0)
            {
                var specialList = _this.mDbData.Special.Keys.ToList();
                specialList.Sort();
                _this.mDbData.Ride = specialList[specialList.Count - 1];//选择ID最大的皮肤坐骑
            }
            else
            {
                _this.mDbData.Ride = _this.mDbData.Id;//若没有坐骑皮肤，则选择当前玩家拥有的常规坐骑
            }
            _this.mCharacter.Mount(_this.mDbData.Ride);
        }
        public ErrorCodes MountFeed(MountManager _this, int itemId)
        {
            //if (_this.mCharacter.mBag.GetItemCount(itemId) <= 0)
            //{
                                
            //    return ErrorCodes.Unknow;
            //}
            var tbItem = Table.GetMountFeed(itemId);
            if (tbItem == null)
            {
                return ErrorCodes.Unknow;
            }
            var tbMount = Table.GetMount(tbItem.UseLimit);
            if(tbMount == null)
                return ErrorCodes.Unknow;
            if (tbMount.Step*100 + tbMount.Level > _this.mDbData.Step*100 + _this.mDbData.Level)
                return ErrorCodes.Unknow;
            int cur = 0;
            _this.mDbData.Feeds.TryGetValue(itemId, out cur);
            if(cur >= tbItem.MaxCount)
                return ErrorCodes.Error_MountSkill_Limit;

            _this.mDbData.Feeds.modifyValue(itemId, 1);
            for (int i = 0; i < tbItem.Attr.Length && i < tbItem.Value.Length; i++)
            {
                if (tbItem.Attr[i] > 0 && tbItem.Value[i] > 0)
                {
                    _this.mDbData.Attrs.modifyValue(tbItem.Attr[i], tbItem.Value[i]);
                }
            }
            RefreshAttr(_this);
            _this.mCharacter.BooksChange();
            _this.mCharacter.SetRankFlag(RankType.Mount);
            _this.MarkDbDirty();
            //_this.mCharacter.mBag.DeleteItem(itemId, 1, eDeleteItemType.ActivateBook);
            return ErrorCodes.OK;
        }

        public bool AddGift(MountManager _this, int id)
        {
            var tb = Table.GetMount(id);
            if (tb == null)
                return false;

            if (tb.Special > 0)
            {
                AddSkin(_this, id);
            }
            else
            {
                if (tb.Step <= _this.mDbData.Step)
                    return false;

                var record = Table.GetMount(_this.mDbData.Id);
                if (record == null || record.IsOpen == 0)
                    return false;
                while ((record = Table.GetMount(record.NextId)) != null)
                {
                    if (record.IsOpen == 0)
                        break;
                    if (record.SkillId > 0)
                    {//开启技能
                        _this.mDbData.Skills.Add(record.SkillId, 0);
                    }
                    if (record.Id == id)
                        break;
                };
                _this.mDbData.Step = tb.Step;
                _this.mDbData.Id = id;
                _this.mDbData.Exp = 0;
                _this.mDbData.Level = tb.Level;
                _this.mCharacter.Mount(id);
                _this.mDbData.Ride = id;
                RefreshAttr(_this);
                _this.mCharacter.BooksChange();
                _this.mCharacter.SetRankFlag(RankType.Mount);
                _this.MarkDbDirty();
            }
            return true;
        }

        public ErrorCodes AddSkin(MountManager _this, int id)
        {
            RefreshSkinState(_this);
            var tb = Table.GetMount(id);
            if (tb == null)
                return ErrorCodes.Unknow;

            var alreadyHave = false;

            if (tb.Special > 0)
            {
                if (_this.mDbData.Special.ContainsKey(id))
                {
                    alreadyHave = true;
                    if (tb.IsPermanent == 1)//重复永久坐骑 发礼包
                    {
                        var tbItem = Table.GetItemBase(tb.ItemId);
                        if (tbItem != null)
                        {
                            var value = tbItem.Exdata[1];
                            if (!_this.mCharacter.CheckBagCanIn(value))
                            {
                                return ErrorCodes.Error_ItemNoInBag_All;
                            }
                            var error = _this.mCharacter.Gift(eActivationRewardType.TableGift, tbItem.Exdata[0]);
                            if (error != ErrorCodes.OK)
                            {
                                return error;
                            }
                        }
                    }
                    else
                    {
                        _this.mDbData.Special[id] += (int)tb.ValidityData;//应策划需求，限时坐骑时间叠加     
                    }
                }
                else
                {
                    var invalidTime = -1;
                    if (tb.IsPermanent == 0) //限时坐骑
                    {
                        invalidTime = DataTimeExtension.GetTimeStampSeconds(DateTime.Now.AddSeconds(tb.ValidityData));
                    }
                    _this.mDbData.Special.Add(id, invalidTime);
                    _this.mCharacter.Mount(id);
                    _this.mDbData.Ride = id;
                    RefreshAttr(_this);
                    _this.mCharacter.BooksChange();
                    _this.mCharacter.SetRankFlag(RankType.Mount);
                }

                var item = Table.GetItemBase(tb.ItemId);
                if (null != item && id != 99 && !alreadyHave)//过滤掉战马试用的公告
                {
                    var args = new List<string>
                    {
                        Utils.AddCharacter(_this.mCharacter.mGuid,_this.mCharacter.GetName()),
                        string.Format("[{0}]{1}[-]", Utils.GetTableColorString(item.Quality), item.Name),
                    };
                    var exExdata = new List<int>();
                    _this.mCharacter.SendSystemNoticeInfo(291011, args, exExdata); //恭喜玩家{0}拥有了上古稀有坐骑{1}，从此帅出新高度！
                }

                _this.MarkDbDirty();
            }
            return ErrorCodes.OK;
        }

        public bool RemoveSkin(MountManager _this, int id)
        {
            var tb = Table.GetMount(id);
            if (tb == null)
                return false;

            if (tb.Special > 0)
            {
                if (_this.mDbData.Special.ContainsKey(id))
                {
                    _this.mDbData.Special.Remove(id);

                    if (_this.mDbData.Ride == id)//当前骑乘的是限时坐骑
                    {
                        if (_this.mDbData.Special.Count > 0)
                        {
                            var specialList = _this.mDbData.Special.Keys.ToList();
                            specialList.Sort();
                            _this.mDbData.Ride = specialList[specialList.Count - 1];//选择ID最大的皮肤坐骑
                        }
                        else
                        {
                            _this.mDbData.Ride = _this.mDbData.Id;//若没有坐骑皮肤，则选择当前玩家拥有的阶数最大的坐骑形象
                        }
                        _this.mCharacter.Mount(_this.mDbData.Ride);
                    }
                    else
                    {
                        //当前骑乘的不是限时坐骑，策划表示不需要操作，仅扣除即可   
                    }

                    RefreshAttr(_this);
                    _this.mCharacter.BooksChange();
                    _this.mCharacter.SetRankFlag(RankType.Mount);
                    _this.MarkDbDirty();
                }
            }
            else
            {
                return false;
            }

            //限时皮肤到期邮件提示
            {
                var tbMail = Table.GetMail(502);
                var title = tbMail.Title;
                var tbItemBase = Table.GetItemBase(tb.ItemId);
                if (null != tbItemBase)
                {
                    var content = string.Format(tbMail.Text, tbItemBase.Name);
                    _this.mCharacter.mMail.PushMail(title, content, new Dictionary<int, int>(), tbMail.Sender);
                }
            }

            return true;
        }

        public void RefreshSkinState(MountManager _this)
        {
            List<int> removeList = new List<int>();
            foreach (var special in _this.mDbData.Special)
            {
                if (special.Key == 99)//过滤掉战马试用
                    continue;
                if (special.Value == -1)//过滤掉永久坐骑
                    continue;
                var tb = Table.GetMount(special.Key);
                if (tb != null)
                {
                    if (tb.IsPermanent == 1)
                    {
                        continue;
                    }
                }
                var nowTime = DataTimeExtension.GetTimeStampSeconds(DateTime.Now);
                var invalidTime = special.Value;
                if (nowTime >= invalidTime)
                {
                    removeList.Add(special.Key);
                }
            }
            foreach (var item in removeList)
            {
                RemoveSkin(_this, item);
            }
        }

        public MountData GetMountData(MountManager _this)
        {
            RefreshSkinState(_this);
            MountData data = new MountData();
            data.Attrs.AddRange(_this.mDbData.Attrs);
            data.Id = _this.mDbData.Id;
            data.Exp = _this.mDbData.Exp;
            data.Level = _this.mDbData.Level;
            data.Step = _this.mDbData.Step;
            data.Ride = _this.mDbData.Ride;
            data.Skills.AddRange(_this.mDbData.Skills);
            data.Feeds.AddRange(_this.mDbData.Feeds);
            data.Special.AddRange(_this.mDbData.Special);
            return data;
        }

        public int GetFightPoint(MountManager _this, int characterLevel, int roleId)
        {
            if (_this.mDbData.Id <= 0 || _this.mDbData.Step <= 0)
            {
                return -1;
            }
            var value = Utils.CalcAttrFightPoint(_this.Mountattrs, characterLevel, roleId);

            return value;
        }
    }

    public class MountManager : NodeBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IMountManager mStaticImpl;
        public CharacterController mCharacter; //角色
        public DBMountData mDbData;
        public Dictionary<int, int> Mountattrs = new Dictionary<int, int>(); //坐骑所提供的属性列表
        static MountManager()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof(MountManager), typeof(MountManagerDefaultImpl),
    o => { mStaticImpl = (IMountManager)o; });
        }
        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public DBMountData InitByBase(CharacterController character)
        {
            mDbData = new DBMountData();
            mDbData.Level = 0;
            mDbData.Step = 0;
            mDbData.Id = 0;

            mCharacter = character;
            MarkDirty();
            return mDbData;
        }

        //用数据库数据
        public void InitByDB(CharacterController character, DBMountData TalentData)
        {

            if (TalentData == null)
            {
                character.mDbData.MountData = InitByBase(character);
            }
            else
            {
                mCharacter = character;
                mDbData = TalentData ;
                RefreshAttr();
            }

        }
        public void RefreshAttr()
        {
            mStaticImpl.RefreshAttr(this);
        }

        public bool Ride(int id)
        {
            return mStaticImpl.Ride(this, id);
        }

        public ErrorCodes MountUp()
        {
            return mStaticImpl.MountUp(this);
        }

        public ErrorCodes SkillUp(int skillId)
        {
            return mStaticImpl.SkillUp(this,skillId);
        }
        public ErrorCodes MountFeed(int itemId)
        {
            return mStaticImpl.MountFeed(this, itemId);
        }
        public bool AddGift(int id)
        {
            return mStaticImpl.AddGift(this,id);
        }
        public ErrorCodes AddSkin(int id)
        {
            return mStaticImpl.AddSkin(this, id);
        }

        public void RefreshSkinState()
        {
            mStaticImpl.RefreshSkinState(this);
        }

        public bool RemoveSkin(int id)
        {
            return mStaticImpl.RemoveSkin(this, id);
        }

        public MountData GetMountData()
        {
            return mStaticImpl.GetMountData(this);
        }

        public void DeleteMount(int id)
        {
            mStaticImpl.DeleteMount(this, id);
        }

        public int GetFightPoint(int characterLevel, int roleId)
        {
            return mStaticImpl.GetFightPoint(this, characterLevel, roleId);
        }
    }
}