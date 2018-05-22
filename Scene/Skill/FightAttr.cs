#region using

using System;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using EventSystem;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public interface IAttrBaseManager
    {
        int GetAttrRef(int nCharacterId, eAttributeType basetype, eAttributeType reftype);
        int GetAttrValue(int nCharacterId, int nLevel, eAttributeType type);
        List<int> GetBeRefAttrList(int nCharacterId, eAttributeType reftype);
        List<int> GetCanRefAttrList(int nCharacterId, eAttributeType basetype);
        void Init();
        void InitCharacter();
        void InitLevelData();
        void InitRefAttr();
    }

    public class AttrBaseManagerDefaultImpl : IAttrBaseManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //

        #region  初始化

        //初始化属性
        public void Init()
        {
            InitRefAttr();
            InitCharacter();
            InitLevelData();
        }

        //初始化等级系数
        public void InitLevelData()
        {
            Table.ForeachLevelData(record =>
            {
                if (record.Id > AttrBaseManager.LevelMax)
                {
                    return false;
                }
                AttrBaseManager.HitReduce[record.Id] = record.Hit;
                AttrBaseManager.DodgeReduce[record.Id] = record.Dodge;
                return true;
            });
        }

        public void InitRefAttr()
        {
            AttrBaseManager.AttrBeRefList = new List<int>[AttrBaseManager.CharacterCount, (int) eAttributeType.Count];
            AttrBaseManager.AttrCanRefList = new List<int>[AttrBaseManager.CharacterCount, (int) eAttributeType.Count];
            for (var c = 0; c != AttrBaseManager.CharacterCount; ++c) //角色
            {
                AttrBaseManager.AttrBeRefList[c, 0] = new List<int>();
                for (var i = 0; i != AttrBaseManager.Attr1Count; ++i) //影响别的属性 的属性
                {
                    AttrBaseManager.AttrCanRefList[c, i] = new List<int>();
                    var IsFind = false;

                    Table.ForeachAttrRef(record =>
                    {
                        if (record.CharacterId != c || i != record.AttrId)
                        {   
                            return true;
                        }
                        //AttrName[i] = record.Desc;
                        IsFind = true;  
                        for (var j = 1; j != (int) eAttributeType.Count; ++j) //被影响的属性
                        {
                            if (j > record.Attr.Length)
                            {
                                continue;
                            }
                            var AttrId = AttrBaseManager.RefIndextoAttrId[j - 1];
                            if (AttrBaseManager.AttrBeRefList[c, AttrId] == null)
                            {
                                AttrBaseManager.AttrBeRefList[c, AttrId] = new List<int>();
                            }
                            var nValue = record.Attr[j - 1];
                            AttrBaseManager.CharacterAttrRef[c, i, AttrId] = nValue;
                            if (nValue > 0)
                            {
                                AttrBaseManager.AttrCanRefList[c, i].Add(AttrId);
                                AttrBaseManager.AttrBeRefList[c, AttrId].Add(i);
                            }
                        }
                        return false;
                    });
                    if (!IsFind)
                    {
                        Logger.Warn("AttrBaseManager::InitRefAttr NoThisLine Character={0} AttrBaseId={1} ", c, i);
                    }
                }
            }
            //属性被其他影响的关系输出
            //Logger.Debug("-----------------职业属性的被影响关系-----------------");
            //for (int c = 0; c != CharacterCount; ++c) //角色
            //{
            //    Logger.Debug("职业ID={0} ", c);
            //    for (int i = 0; i != (int)eAttributeType.AttrCount - 2; ++i) //影响别的属性 的属性
            //    {
            //        string refattr = "";
            //        if (AttrBeRefList[c, i]==null) continue;
            //        foreach (var i1 in AttrBeRefList[c, i])
            //        {
            //            refattr = refattr + AttrName[i1] + ",";
            //        }
            //        if (refattr != "")
            //        {
            //            Logger.Debug("{0} : {1} ", AttrName[i], refattr);
            //        }
            //    }
            //}
            //属性影响的关系输出
            //Logger.Debug("-----------------职业属性的影响关系-----------------");
            //for (int c = 0; c != AttrBaseManager.CharacterCount; ++c) //角色
            //{
            //    Logger.Debug("职业ID={0} ", c);
            //    for (int i = 0; i != AttrBaseManager.Attr1Count; ++i) //影响别的属性 的属性
            //    {
            //        string refattr = "";
            //        foreach (var i1 in AttrBaseManager.AttrCanRefList[c, i])
            //        {
            //            refattr = refattr + AttrBaseManager.AttrName[i1] + ",";
            //        }
            //        if (refattr != "")
            //        {
            //            Logger.Debug("{0} : {1} ", AttrBaseManager.AttrName[i], refattr);
            //        }
            //    }
            //}
        }

        //初始化等级属性
        public void InitCharacter()
        {
            for (var i = 0; i != AttrBaseManager.CharacterCount; ++i)
            {
                var thisCharacter = Table.GetCharacterBase(i);
                if (thisCharacter == null)
                {
                    continue;
                }
                for (var k = 0; k != (int) eAttributeType.Count; ++k)
                {
                    var nBaseAttr = thisCharacter.Attr[k];
                    var nLevelUpAttr = GetAttrRef(i, eAttributeType.Level, (eAttributeType) k);
                    for (var j = 0; j != AttrBaseManager.CharacterLevelMax; ++j)
                    {
                        AttrBaseManager.CharacterAttr[i, j, k] = nBaseAttr + nLevelUpAttr*j/100;
                    }
                }
            }
        }

        #endregion

        #region  获取数据

        //获取属性
        public int GetAttrValue(int nCharacterId, int nLevel, eAttributeType type)
        {
            if (nCharacterId >= AttrBaseManager.CharacterCount || nCharacterId < 0)
            {
                //Logger.Error("AttrBaseManager::GetAttrValue Error CharacterId={0}", nCharacterId);
                var tbmonster = Table.GetCharacterBase(nCharacterId);
                if (tbmonster == null)
                {
                    Logger.Error("Character ID not find ! CharacterId={0}", nCharacterId);
                    return 0;
                }
                return tbmonster.Attr[(int) type];
            }
            nLevel = nLevel - 1;
            if (nLevel >= AttrBaseManager.CharacterLevelMax || nLevel < 0)
            {
                Logger.Error("AttrBaseManager::GetAttrValue Error Level={0}", nLevel); //玩家刚进来确实会有这种情况，默认是0级因为 
                return 0;
            }
            if (type >= eAttributeType.Count || type < 0)
            {
                Logger.Error("AttrBaseManager::GetAttrValue Error AttrId={0}", type);
                return 0;
            }
            return AttrBaseManager.CharacterAttr[nCharacterId, nLevel, (int) type];
        }

        //被哪些属性修正的列表
        public List<int> GetBeRefAttrList(int nCharacterId, eAttributeType reftype)
        {
            if (nCharacterId >= AttrBaseManager.CharacterCount || nCharacterId < 0)
            {
                //因为怪物也会调用这里，就不报错了 Logger.Error("AttrBaseManager::GetBeRefAttrList Error CharacterId={0}", nCharacterId);
                return new List<int>();
            }
            if (reftype >= eAttributeType.Count || reftype < 0)
            {
                Logger.Error("AttrBaseManager::GetBeRefAttrList Error BaseAttrId={0}", reftype);
                return new List<int>();
            }
            return AttrBaseManager.AttrBeRefList[nCharacterId, (int) reftype];
        }

        //会修改哪些属性
        public List<int> GetCanRefAttrList(int nCharacterId, eAttributeType basetype)
        {
            if (nCharacterId >= AttrBaseManager.CharacterCount || nCharacterId < 0)
            {
                Logger.Error("AttrBaseManager::GetCanRefAttrList Error CharacterId={0}", nCharacterId);
                return new List<int>();
            }
            if (basetype >= eAttributeType.Count || basetype < 0)
            {
                Logger.Error("AttrBaseManager::GetCanRefAttrList Error BaseAttrId={0}", basetype);
                return new List<int>();
            }
            return AttrBaseManager.AttrCanRefList[nCharacterId, (int) basetype];
        }

        //获得属性修正
        public int GetAttrRef(int nCharacterId, eAttributeType basetype, eAttributeType reftype)
        {
            if (nCharacterId >= AttrBaseManager.CharacterCount || nCharacterId < 0)
            {
                Logger.Error("AttrBaseManager::GetAttrRef Error CharacterId={0}", nCharacterId);
                return 0;
            }
            if (basetype >= eAttributeType.Count || basetype < 0)
            {
                Logger.Error("AttrBaseManager::GetAttrRef Error BaseAttrId={0}", basetype);
                return 0;
            }
            if (reftype >= eAttributeType.Count || reftype < 0)
            {
                Logger.Error("AttrBaseManager::GetAttrRef Error RefAttrId={0}", reftype);
                return 0;
            }
            return AttrBaseManager.CharacterAttrRef[nCharacterId, (int) basetype, (int) reftype];
        }

        #endregion
    }

    //组织静态数据
    public static class AttrBaseManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //
        private static IAttrBaseManager mImpl;

        static AttrBaseManager()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (AttrBaseManager),
                typeof (AttrBaseManagerDefaultImpl),
                o => { mImpl = (IAttrBaseManager) o; });
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
        }

        private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v.tableName == "LevelData")
            {
                InitLevelData();
            }
            if (v.tableName == "AttrRef" || v.tableName == "CharacterBase")
            {
                mImpl.InitRefAttr();
                mImpl.InitCharacter();
            }
        }

        #region  静态变量

        public static readonly int CharacterCount = 3; //职业数量
        public static readonly int Attr1Count = 5; //1级属性数量
        public static readonly int CharacterLevelMax = 500; //职业最大等级
        public static readonly int LevelMax = 500; //LevelData最大等级
        public static readonly int DodgeMax = 8000; //最大闪避率
        public static readonly int DamageCritical = 8000; //最大伤害暴击率
        public static readonly int HealthCritical = 10000; //最大治疗暴击率

        #endregion

        #region  数据结构

        public static readonly string[] AttrName = new string[(int) eAttributeType.Count]
        {
            "等级",
            "力量",
            "敏捷",
            "智力",
            "体力",
            "物攻最小值",
            "物攻最大值",
            "魔攻最小值",
            "魔攻最大值",
            "附加伤害",
            "物理防御",
            "魔法防御",
            "伤害抵挡",
            "生命上限",
            "魔法上限",
            "幸运一击率",
            "幸运一击伤害率",
            "卓越一击率",
            "卓越一击伤害率",
            "命中",
            "闪避",
            "伤害加成率",
            "伤害减少率",
            "伤害反弹率",
            "无视防御率",
            "移动速度",
            "击中回复",
            "火属性攻击",
            "火属性抗性",
            "冰属性攻击",
            "冰属性抗性",
            "毒属性攻击",
            "毒属性抗性"
        };

        public static readonly int[,,] CharacterAttr =
            new int[CharacterCount, CharacterLevelMax, (int) eAttributeType.Count]; //各角色各等级的各基础属性

        public static readonly int[,,] CharacterAttrRef =
            new int[CharacterCount, Attr1Count, (int) eAttributeType.Count]; //各职业各属性之间的影响

        public static List<int>[,] AttrBeRefList; //各职业各属性被哪些属性影响
        public static List<int>[,] AttrCanRefList; //各职业各属性影响哪些属性
        public static readonly double[] HitReduce = new double[LevelMax + 1];
        public static readonly double[] DodgeReduce = new double[LevelMax + 1];
        //public static readonly double[] Critical = new double[LevelMax + 1];
        //public static readonly double[] Toughness = new double[LevelMax + 1];
        //public static readonly double[] Dodge = new double[LevelMax + 1];
        //public static readonly double[] Hit = new double[LevelMax + 1];

        #endregion

        #region  初始化

        //初始化属性
        public static void Init()
        {
            mImpl.Init();
        }

        //初始化等级系数
        public static void InitLevelData()
        {
            mImpl.InitLevelData();
        }

        //初始化属性修正
        public static List<int> RefIndextoAttrId = new List<int>
        {
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            10,
            11,
            13,
            14,
            19,
            20
        };

        #endregion

        #region  获取数据

        //获取属性
        public static int GetAttrValue(int nCharacterId, int nLevel, eAttributeType type)
        {
            return mImpl.GetAttrValue(nCharacterId, nLevel, type);
        }

        //被哪些属性修正的列表
        public static List<int> GetBeRefAttrList(int nCharacterId, eAttributeType reftype)
        {
            return mImpl.GetBeRefAttrList(nCharacterId, reftype);
        }

        //会修改哪些属性
        public static List<int> GetCanRefAttrList(int nCharacterId, eAttributeType basetype)
        {
            return mImpl.GetCanRefAttrList(nCharacterId, basetype);
        }

        //获得属性修正
        public static int GetAttrRef(int nCharacterId, eAttributeType basetype, eAttributeType reftype)
        {
            return mImpl.GetAttrRef(nCharacterId, basetype, reftype);
        }

        #endregion
    }

    public interface IFightAttr
    {
        void BatchEquipAttr(FightAttr _this, ref int[] attr, ref int[] attrRef, Dictionary<int, ItemEquip2> equipList);
        void BookRefresh(FightAttr _this, Dictionary<int, int> bookAttrs, Dictionary<int, int> monsterAttrs);
        ErrorCodes CheckEquipOn(FightAttr _this, EquipRecord tbEquip);
        void CopyToAttr(FightAttr _this, List<int> list);
        void EquipRefresh(FightAttr _this);
        int GetAttackType(FightAttr _this);
        eHitType GetDamageHitType(FightAttr _this, ObjCharacter character);
        int GetDamageValue(FightAttr _this, ObjCharacter bear, int attrId, eDamageType damageType);
        int GetAttrDamageValue(FightAttr _this, ObjCharacter bear, eAttributeType type);
        int GetDataValue(FightAttr _this, eAttributeType type);
        int GetFightPoint(FightAttr _this);
        eHitType GetHealthHitType(FightAttr _this, ObjCharacter character);
        eHitType GetHitResult(FightAttr _this, ObjCharacter character, eSkillHitType skillHitType);
        eHitType GetHitTypeByRoundTable(FightAttr _this, List<int> RoundData);

        /// <summary>
        ///     获得抗性减伤
        /// </summary>
        /// <returns></returns>
        double GetMagReduction(FightAttr _this, int nAttackLevel);

        eHitType GetMustCritical(FightAttr _this, ObjCharacter character);
        eHitType GetMustHit(FightAttr _this, ObjCharacter character);

        /// <summary>
        ///     获得护甲减伤
        /// </summary>
        /// <returns></returns>
        double GetPhyReduction(FightAttr _this, int nAttackLevel);

        string GetStringData(FightAttr _this);
        void InitAttributesAll(FightAttr _this);
        void InitAttributesAllEx(FightAttr _this, Dictionary<int,int> dic);
        void InitFightAttr(FightAttr _this, ObjCharacter obj);
        void RankSendChanges(FightAttr _this);
        void RankSendChangesBytrigger(FightAttr _this);
        IEnumerator ScenetoRankCoroutine(Coroutine co, FightAttr _this, ObjCharacter character, int fp);
        void SetDataValue(FightAttr _this, eAttributeType type, int value);
        void SetFightPointFlag(FightAttr _this, bool b = true);
        void SetFlag(FightAttr _this, eAttributeType type);
        void SetFlagByAttrId(FightAttr _this, int attrId);
        void SetTitles(FightAttr _this, List<int> titles);
        void TalentRefresh(FightAttr _this, Dictionary<int, int> talentAttrs);
        void TitleRefresh(FightAttr _this, List<int> titles, int type);
    }

    public class FightAttrDefaultImpl : IFightAttr
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  数据结构

        public string GetStringData(FightAttr _this)
        {
            var str = "";
            for (var i = eAttributeType.Level; i < eAttributeType.HitRecovery; i++)
            {
                var v = GetDataValue(_this, i);
                str = string.Format("{0},{1}", str, v);
            }
            return str;
        }

        #endregion

        #region  初始化

        public int GetAttackType(FightAttr _this)
        {
            if (_this.mObj.TypeId == 1)
            {
                return 1;
            }
            return 0;
        }

        public void InitFightAttr(FightAttr _this, ObjCharacter obj)
        {
            _this.mObj = obj;
            _this.mDataId = obj.TypeId;
            _this.mData[0] = 1;
            //for (int i = 0; i < (int)eAttributeType.AttrCount; i++)
            //{
            //    mEquipDataRef[i] = 10000;
            //    mBookDataRef[i] = 10000;
            //    mTalentDataRef[i] = 10000;
            //}
        }

        //刷新天赋数据
        public void TalentRefresh(FightAttr _this, Dictionary<int, int> talentAttrs)
        {
            Array.Clear(_this.mTalentData, 0, (int) eAttributeType.AttrCount);
            Array.Clear(_this.mTalentDataRef, 0, (int) eAttributeType.AttrCount);
            ItemEquip2.AttrConvert(talentAttrs, _this.mTalentData, _this.mTalentDataRef, GetAttackType(_this));
            if (!_this.PlayerInit)
            {
                _this.mFlag.ReSetAllFlag(true);
                OnAllFlagChanged(_this);
                SetFightPointFlag(_this);
            }
        }

        //刷新图鉴数据
        public void BookRefresh(FightAttr _this, Dictionary<int, int> bookAttrs, Dictionary<int, int> monsterAttrs)
        {
            Array.Clear(_this.mTempData, 0, (int) eAttributeType.AttrCount);
            Array.Clear(_this.mTempDataRef, 0, (int) eAttributeType.AttrCount);
            _this.mMonsterAttr.Clear();
            _this.mMonsterAttr.AddRange(monsterAttrs);

            ItemEquip2.AttrConvert(bookAttrs, _this.mTempData, _this.mTempDataRef, GetAttackType(_this));
            var bBaseAttr = false;
            for (var i = 0; i < (int) eAttributeType.AttrCount; i++)
            {
                if (i >= (int) eAttributeType.Count && i < (int) eAttributeType.CountNext)
                {
                    continue;
                }
                var isCHange = false;
                if (_this.mTempData[i] != _this.mBookData[i])
                {
                    _this.mBookData[i] = _this.mTempData[i];
                    isCHange = true;
                }
                if (_this.mTempDataRef[i] != _this.mBookDataRef[i])
                {
                    _this.mBookDataRef[i] = _this.mTempDataRef[i];
                    isCHange = true;
                }
                if (isCHange)
                {
                    if (!_this.PlayerInit)
                    {
                        if (i >= 1 && i <= 5)
                        {
                            bBaseAttr = true;
                        }
                        SetFlag(_this, (eAttributeType) i);
                        SetFightPointFlag(_this);
                    }
                }
            }
            if (bBaseAttr)
            {
                EquipRefresh(_this);
            }
        }

        //刷新称号
        public void TitleRefresh(FightAttr _this, List<int> titles, int type)
        {
            if (type == 0)
            {
                foreach (var title in titles)
                {
                    SetTitle(_this, title);
                }
            }
            else if (type == 1)
            {
                _this.SetTitles(titles);
            }
        }

        private void RefreshTitleAttr(FightAttr _this)
        {
            var attrList = new Dictionary<int, int>();
            foreach (var title in _this.mTitles)
            {
                var tbNameTitle = Table.GetNameTitle(title);
                if (tbNameTitle == null)
                {
                    Logger.Error("In RefreshTitleAttr(), tbNameTitle == null!");
                    continue;
                }
                for (int i = 0, imax = tbNameTitle.PropId.Length; i < imax; i++)
                {
                    var id = tbNameTitle.PropId[i];
                    if (id < 0)
                    {
                        break;
                    }
                    var value = tbNameTitle.PropValue[i];
                    attrList.modifyValue(id, value);
                }
            }

            Array.Clear(_this.mTempData, 0, (int) eAttributeType.AttrCount);
            Array.Clear(_this.mTempDataRef, 0, (int) eAttributeType.AttrCount);
            ItemEquip2.AttrConvert(attrList, _this.mTempData, _this.mTempDataRef, GetAttackType(_this));
            for (var attrType = 0; attrType < (int) eAttributeType.AttrCount; attrType++)
            {
                if (attrType >= (int)eAttributeType.Count && attrType < (int)eAttributeType.CountNext)
                {
                    continue;
                }

                var isChange = false;
                if (_this.mTempData[attrType] != _this.mTitleData[attrType])
                {
                    _this.mTitleData[attrType] = _this.mTempData[attrType];
                    isChange = true;
                }
                if (_this.mTempDataRef[attrType] != _this.mTitleDataRef[attrType])
                {
                    _this.mTitleDataRef[attrType] = _this.mTempDataRef[attrType];
                    isChange = true;
                }
                if (isChange)
                {
                    if (!_this.PlayerInit)
                    {
                        SetFlag(_this, (eAttributeType) attrType);
                        SetFightPointFlag(_this);
                    }
                }
            }
        }

        //刷新装备数据
        public void EquipRefresh(FightAttr _this)
        {
            Array.Clear(_this.mTempData, 0, (int) eAttributeType.AttrCount);
            Array.Clear(_this.mTempDataRef, 0, (int) eAttributeType.AttrCount);
            BatchEquipAttr(_this, ref _this.mTempData, ref _this.mTempDataRef, _this.mObj.Equip);
            for (var i = 0; i < (int) eAttributeType.AttrCount; i++)
            {
                if (i >= (int)eAttributeType.Count && i < (int)eAttributeType.CountNext)
                {
                    continue;
                }

                var isCHange = false;
                if (_this.mTempData[i] != _this.mEquipData[i])
                {
                    _this.mEquipData[i] = _this.mTempData[i];
                    isCHange = true;
                }
                if (_this.mTempDataRef[i] != _this.mEquipDataRef[i])
                {
                    _this.mEquipDataRef[i] = _this.mTempDataRef[i];
                    isCHange = true;
                }
                if (isCHange)
                {
                    if (!_this.PlayerInit)
                    {
                        SetFlag(_this, (eAttributeType) i);
                        SetFightPointFlag(_this);
                    }
                }
            }
        }

        public ErrorCodes CheckEquipOn(FightAttr _this, EquipRecord tbEquip)
        {
            //属性需求
            for (var i = 0; i < tbEquip.NeedAttrId.Length; i++)
            {
                var needId = tbEquip.NeedAttrId[i];
                if (needId > 0)
                {
                    if (GetDataValue(_this, (eAttributeType) needId) < tbEquip.NeedAttrValue[i])
                    {
                        return ErrorCodes.Error_AttrNotEnough;
                    }
                }
            }
            return ErrorCodes.OK;
        }

        //批量整理装备属性
        public void BatchEquipAttr(FightAttr _this,
                                   ref int[] attr,
                                   ref int[] attrRef,
                                   Dictionary<int, ItemEquip2> equipList)
        {
            var tao = new Dictionary<int, int>(); //套装ID  件数
            var attrList = new Dictionary<int, int>();
            _this.EquipTieFightPoint = 0;
            foreach (var itemEquip in equipList)
            {
                if (itemEquip.Key/10 == (int) eBagType.Wing)
                {
                    var tbWing = Table.GetWingQuality(itemEquip.Value.GetId());
                    if (tbWing == null)
                    {
                        continue;
                    }
                    WingItem.GetAttrList(attrList, itemEquip.Value, tbWing, _this.mLevel, GetAttackType(_this));
                    continue;
                }
                var equip = itemEquip.Value;
                if (null == equip)
                {
                    continue;
                }
                if (equip.GetExdata(22) <= 0)
                {
                    continue;
                }
                if (equip.IsTrialEnd())
                {
                    continue;
                }
                var tbitem = Table.GetItemBase(equip.GetId());
                if (tbitem == null)
                {
                    Logger.Error("BatchEquipAttr itemId  Id={0} not find by Table", equip.GetId());
                    continue;
                }
                var equipid = tbitem.Exdata[0];
                var tbEquip = Table.GetEquip(equipid);
                if (tbEquip == null)
                {
                    Logger.Error("BatchEquipAttr itemId  Id={0} not find equip={1}", equip.GetId(), equipid);
                    continue;
                }

                if (CheckEquipOn(_this, tbEquip) != ErrorCodes.OK)
                {
                    continue;
                }
                if (tbEquip.TieId >= 0)
                {
                    int n;
                    if (tao.TryGetValue(tbEquip.TieId, out n))
                    {
                        tao[tbEquip.TieId] = n + 1;
                    }
                    else
                    {
                        tao[tbEquip.TieId] = 1;
                    }
                }
                var tblevel = Table.GetLevelData(itemEquip.Value.GetExdata(0));
                if (tblevel == null)
                {
                    Logger.Error("BatchEquipAttr itemId  Id={0}  Level={1} not find", equip.GetId(),
                        itemEquip.Value.GetExdata(0));
                    continue;
                }
                equip.GetAttrList(attrList, tbEquip, _this.mLevel, GetAttackType(_this));
            }
            var newBuff = new Dictionary<int, int>();
            //套装属性
            foreach (var equiptie in tao)
            {
                var tbTie = Table.GetEquipTie(equiptie.Key);
                if (tbTie == null)
                {
                    Logger.Error("BatchEquipAttr GetEquipTie  Id={0} not find", equiptie.Key);
                    continue;
                }
                for (var i = 0; i != tbTie.Attr1Id.Length; ++i)
                {
                    if (equiptie.Value >= tbTie.NeedCount[i])
                    {
                        var attrid = tbTie.Attr1Id[i];
                        var attrValue = tbTie.Attr1Value[i];
                        if (attrid > 0)
                        {
                            ItemEquip2.PushEquipAttr(attrList, attrid, attrValue, _this.mLevel, GetAttackType(_this));
                            //attr[attrid] += attrValue;
                        }
                        attrid = tbTie.Attr2Id[i];
                        attrValue = tbTie.Attr2Value[i];
                        if (attrid > 0)
                        {
                            ItemEquip2.PushEquipAttr(attrList, attrid, attrValue, _this.mLevel, GetAttackType(_this));
                            //attr[attrid] += attrValue;
                        }
                        var buffid = tbTie.BuffId[i];
                        if (buffid >= 0)
                        {
                            newBuff[buffid] = 1;
                        }
                        _this.EquipTieFightPoint += tbTie.FightPoint[i];
                    }
                }
            }
            ItemEquip2.AttrConvert(attrList, attr, attrRef, GetAttackType(_this), true);
            //清空当前附加的没有的Buff
            var removeList = new List<int>();
            foreach (var data in _this.mEquipBuff)
            {
                if (newBuff.ContainsKey(data.Key))
                {
                    newBuff.Remove(data.Key);
                }
                else
                {
                    removeList.Add(data.Key);
                }
            }
            foreach (var buffid in removeList)
            {
                var buffData = _this.mEquipBuff[buffid];
                MissBuff.DoEffect(_this.mObj.Scene, _this.mObj, buffData); //消失事件
                _this.mObj.DeleteBuff(buffData, eCleanBuffType.EquipTie);
                _this.mEquipBuff.Remove(buffid);
            }
            _this.mObj.BuffList.Do_Del_Buff();
            //重置套装Buff
            foreach (var data in newBuff)
            {
                var buffid = data.Key;
                BuffData buff;
                if (!_this.mEquipBuff.TryGetValue(buffid, out buff))
                {
//没有需要增加
                    var addbuff = _this.mObj.AddBuff(buffid, 1, _this.mObj);
                    GetBuff.DoEffect(_this.mObj.Scene, _this.mObj, addbuff, 0);
                    if (addbuff == null)
                    {
                        Logger.Error("BatchEquipAttr GetEquipTie buffid={1} AddBuff Error", buffid);
                        continue;
                    }
                    _this.mEquipBuff[buffid] = addbuff;
                }
            }
        }

        //初始化武将后开始计算属性
        public void InitAttributesAll(FightAttr _this)
        {
            _this.PlayerInit = false;
            _this.mFlag.ReSetAllFlag(true);
            OnAllFlagChanged(_this);
            _this.mFlag.CleanFlag((int) eAttributeType.HpNow);
            _this.mFlag.CleanFlag((int) eAttributeType.MpNow);
            //初始化值
            for (var i = eAttributeType.Level; i < eAttributeType.AttrCount; ++i)
            {
                GetDataValue(_this, i);
            }
            if (_this.mObj.GetObjType() != ObjType.PLAYER)
            {
                SetDataValue(_this, eAttributeType.HpNow, GetDataValue(_this, eAttributeType.HpMax));
                SetDataValue(_this, eAttributeType.MpNow, GetDataValue(_this, eAttributeType.MpMax));
            }
        }

        public void InitAttributesAllEx(FightAttr _this, Dictionary<int,int> dic)
        {
            _this.PlayerInit = false;
            _this.mFlag.ReSetAllFlag(true);
            OnAllFlagChanged(_this);
            _this.mFlag.CleanFlag((int)eAttributeType.HpNow);
            _this.mFlag.CleanFlag((int)eAttributeType.MpNow);
            //初始化值
            foreach (var v in dic)
            {
                if(v.Value>0)
                    SetDataValue(_this, (eAttributeType)v.Key, v.Value + GetDataValue(_this, (eAttributeType)v.Key));
            }
            if (_this.mObj.GetObjType() != ObjType.PLAYER)
            {
                SetDataValue(_this, eAttributeType.HpNow, GetDataValue(_this, eAttributeType.HpMax));
                SetDataValue(_this, eAttributeType.MpNow, GetDataValue(_this, eAttributeType.MpMax));
            }
        }
        #endregion

        #region  数据存取

        private void SetTitle(FightAttr _this, int titleId)
        {
            if (titleId < 0)
            {
                return;
            }
            var tbNameTitle = Table.GetNameTitle(titleId);
            if (tbNameTitle == null)
            {
                return;
            }
            var pos = tbNameTitle.Pos;
            if (pos < 0 || pos >= _this.mEquipedTitles.Length)
            {
                Logger.Error("In SetTitle(). pos = {0}", pos);
                return;
            }

            if (_this.mEquipedTitles[pos] != titleId)
            {
                _this.mEquipedTitles[pos] = titleId;
                _this.OnPropertyChanged((uint) (eSceneSyncId.SyncTitle0 + pos));
            }
        }

        public void SetTitles(FightAttr _this, List<int> titles)
        {
            _this.mTitles = titles;
            var equipedTitles = _this.mEquipedTitles;
            for (var i = 0; i < equipedTitles.Length; i++)
            {
                var title = equipedTitles[i];
                if (title != -1 && !titles.Contains(title))
                {
                    equipedTitles[i] = -1;
                    _this.OnPropertyChanged((uint) (eSceneSyncId.SyncTitle0 + i));
                }
            }
            RefreshTitleAttr(_this);
        }

        //获得属性
        public int GetDataValue(FightAttr _this, eAttributeType type)
        {
            if (type >= eAttributeType.Count && type < eAttributeType.CountNext)
            {
                return 0;
            }

            if (1 == _this.mFlag.GetFlag((int) type))
            {
                return UpdateDataValue(_this, type);
            }
            if (type < eAttributeType.Level || type >= eAttributeType.AttrCount)
            {
                return 0;
            }
            return _this.mData[(int) type];
        }

        //设置武将属性值
        public void SetDataValue(FightAttr _this, eAttributeType type, int value)
        {
            if (type >= eAttributeType.Count && type < eAttributeType.CountNext)
            {
                return;
            }

            _this.mFlag.CleanFlag((int) type);
            if (type == eAttributeType.HpNow)
            {
                value = Math.Min(value, GetDataValue(_this, eAttributeType.HpMax));
            }
            else if (type == eAttributeType.MpNow)
            {
                value = Math.Min(value, GetDataValue(_this, eAttributeType.MpMax));
            }
            else if (type == eAttributeType.HpMax)
            {
                if (GetDataValue(_this, eAttributeType.HpNow) > value)
                {
                    SetDataValue(_this, eAttributeType.HpNow, value);
                }
            }
            else if (type == eAttributeType.MpMax)
            {
                if (GetDataValue(_this, eAttributeType.MpNow) > value)
                {
                    SetDataValue(_this, eAttributeType.MpNow, value);
                }
            }
            if (type < eAttributeType.Level || type >= eAttributeType.AttrCount)
            {
                return;
            }
            _this.mData[(int) type] = value;
            UpdateOtherFlag(_this, type);
            switch (type)
            {
                case eAttributeType.Level:
                case eAttributeType.MoveSpeed:
                    _this.OnPropertyChanged((uint) type);
                    break;
                case eAttributeType.HpNow:
                    _this.mObj.MarkDbDirty();
                    _this.OnPropertyChanged((uint) type);
                    _this.mObj.UpdateDbAttribute(type);
                    break;
                case eAttributeType.MpNow:
                    _this.mObj.MarkDbDirty();
                    _this.OnPropertyChanged((uint) type);
                    _this.mObj.UpdateDbAttribute(type);
                    break;
            }
        }

        private void OnAllFlagChanged(FightAttr _this)
        {
            for (var i = 0; i < (int) eAttributeType.AttrCount; i++)
            {
                if (i >= (int)eAttributeType.Count && i < (int)eAttributeType.CountNext)
                    continue;
                OnFlagChanged(_this, (eAttributeType) i);
            }
        }

        private void OnFlagChanged(FightAttr _this, eAttributeType type)
        {
            _this.OnPropertyChanged((uint) type);
        }

        //设置属性脏标记
        public void SetFlag(FightAttr _this, eAttributeType type)
        {
            _this.mFlag.SetFlag((int) type);
            OnFlagChanged(_this, type);
            UpdateOtherFlag(_this, type);
        }

        //由属性ID修改标记位
        public void SetFlagByAttrId(FightAttr _this, int attrId)
        {
            switch (attrId)
            {
                case 105:
                {
                    SetFlag(_this, eAttributeType.MagPowerMin);
                    SetFlag(_this, eAttributeType.MagPowerMax);
                    SetFlag(_this, eAttributeType.PhyPowerMin);
                    SetFlag(_this, eAttributeType.PhyPowerMax);
                }
                    break;
                case 106:
                {
                    SetFlag(_this, eAttributeType.MagPowerMin);
                    SetFlag(_this, eAttributeType.MagPowerMax);
                    SetFlag(_this, eAttributeType.PhyPowerMin);
                    SetFlag(_this, eAttributeType.PhyPowerMax);
                }
                    break;
                case 110:
                {
                    SetFlag(_this, eAttributeType.PhyArmor);
                    SetFlag(_this, eAttributeType.MagArmor);
                }
                    break;
                case 111:
                {
                    SetFlag(_this, eAttributeType.PhyArmor);
                    SetFlag(_this, eAttributeType.MagArmor);
                }
                    break;
                case 113:
                {
                    SetFlag(_this, eAttributeType.HpMax);
                }
                    break;
                case 114:
                {
                    SetFlag(_this, eAttributeType.MpMax);
                }
                    break;
                case 119:
                {
                    SetFlag(_this, eAttributeType.Hit);
                }
                    break;
                case 120:
                {
                    SetFlag(_this, eAttributeType.Dodge);
                }
                    break;
                default:
                    SetFlag(_this, (eAttributeType) attrId);
                    break;
            }
        }

        //更新属性
        private int UpdateDataValue(FightAttr _this, eAttributeType type)
        {
            if (type >= eAttributeType.Count && type < eAttributeType.CountNext)
            {
                return 0;
            }

            if (type < eAttributeType.Level || type >= eAttributeType.AttrCount)
            {
                return 0;
            }
            double nValue = 0;
            _this.mFlag.CleanFlag((int) type); //清除脏标记
            //var hero = Table.hero[DataID];
            switch (type)
            {
                //case eAttributeType.Level:
                //    {
                //        nValue = 1;
                //    }
                //    break;
                //case eAttributeType.Strength:
                //case eAttributeType.Agility:
                //case eAttributeType.Intelligence:
                //case eAttributeType.Endurance:
                //    {
                //        double fBili = 1.0f;
                //        //基础属性
                //        nValue = AttrBaseManager.GetAttrValue(mDataId, GetDataValue(eAttributeType.Level), type);
                //        List<int> BeRefList = AttrBaseManager.GetBeRefAttrList(mDataId, type);
                //        foreach (var i in BeRefList)
                //        {
                //            if (i == 0) continue;
                //            nValue += GetDataValue((eAttributeType)i) * AttrBaseManager.GetAttrRef(mDataId, (eAttributeType)i, type);
                //        }
                //        //装备属性
                //        nValue += mEquipData[(int)type];
                //        //Buff属性
                //        nValue = nValue + mObj.BuffList.Calculate(mObj, type, ref fBili);
                //        //计算
                //        nValue = (int)(fBili * nValue);
                //    }
                //    break;
                //case eAttributeType.HpMax:
                //    {
                //        double fBili = 1.0f;
                //        //基础属性
                //        nValue = AttrBaseManager.GetAttrValue(mDataId, GetDataValue(eAttributeType.Level), type);
                //        List<int> BeRefList = AttrBaseManager.GetBeRefAttrList(mDataId, type);
                //        foreach (var i in BeRefList)
                //        {
                //            if (i == 0) continue;
                //            nValue += GetDataValue((eAttributeType)i) * AttrBaseManager.GetAttrRef(mDataId, (eAttributeType)i, type);
                //        }
                //        //装备影响
                //        nValue += mEquipData[(int)type];
                //        //Buff影响
                //        nValue = nValue + mObj.BuffList.Calculate(mObj, type, ref fBili);
                //        //计算
                //        nValue = (int)(fBili * nValue);
                //    }
                //    break;
                //case eAttributeType.MpMax:
                //    {
                //        double fBili = 1.0f;
                //        //基础属性
                //        nValue = AttrBaseManager.GetAttrValue(mDataId, GetDataValue(eAttributeType.Level), type);
                //        List<int> BeRefList = AttrBaseManager.GetBeRefAttrList(mDataId, type);
                //        foreach (var i in BeRefList)
                //        {
                //            if (i == 0) continue;
                //            nValue += GetDataValue((eAttributeType)i) * AttrBaseManager.GetAttrRef(mDataId, (eAttributeType)i, type);
                //        }
                //        //装备影响
                //        nValue += mEquipData[(int)type];
                //        //Buff影响
                //        nValue = nValue + mObj.BuffList.Calculate(mObj, type, ref fBili);
                //        //计算
                //        nValue = (int)(fBili * nValue);
                //    }
                //    break;
                case eAttributeType.Level:
                    nValue = _this.mData[(int) type];
                    break;
                case eAttributeType.HpMax:
                {
                    double fBili = 1.0f;
                    if (!_this.RobotPlayer)
                    {
                        //基础属性
                        nValue = AttrBaseManager.GetAttrValue(_this.mDataId, GetDataValue(_this, eAttributeType.Level),type);
                        var BeRefList = AttrBaseManager.GetBeRefAttrList(_this.mDataId, type);
                        if (BeRefList != null)
                        {
                            foreach (var i in BeRefList)
                            {
                                if (i == 0)
                                {
                                    continue;
                                }
                                // ReSharper disable once PossibleLossOfFraction
                                nValue += GetDataValue(_this, (eAttributeType) i)* AttrBaseManager.GetAttrRef(_this.mDataId, (eAttributeType) i, type)/100;
                            }
                        }
                        //装备属性
                        nValue += _this.mEquipData[(int) type];
                        nValue += _this.mTalentData[(int) type];
                    }
                    nValue += _this.mBookData[(int) type];
                    nValue += _this.mTitleData[(int) type];
                    //Buff属性
                    nValue = nValue + _this.mObj.BuffList.Calculate(_this.mObj, type, ref fBili);
                    //计算
                    nValue =
                        (int)
                            (fBili*nValue*
                             (10000 + _this.mEquipDataRef[(int) type] + _this.mBookDataRef[(int) type] +
                              _this.mTalentDataRef[(int) type] + _this.mTitleDataRef[(int) type])/10000);
                    //if (mObj.Scene != null && mObj.Scene.isNeedDamageModify)
                    //{
                    //    if (!(mObj is ObjNPC)) //不是怪物
                    //    {
                    //        break;
                    //    } 
                    //    var c = mObj as ObjRetinue;
                    //    if (c != null)
                    //    {
                    //        if (!(c.Owner is ObjPlayer))
                    //        {
                    //            break;
                    //        }
                    //    }
                    //    nValue = (int) (nValue/mObj.Scene.BeDamageModify);
                    //}
                }
                    break;
                case eAttributeType.Strength:
                case eAttributeType.Agility:
                case eAttributeType.Intelligence:
                case eAttributeType.Endurance:
                case eAttributeType.PhyPowerMin:
                case eAttributeType.PhyPowerMax:
                case eAttributeType.MagPowerMin:
                case eAttributeType.MagPowerMax:
                case eAttributeType.AddPower:
                case eAttributeType.PhyArmor:
                case eAttributeType.MagArmor:
                case eAttributeType.DamageResistance:
                case eAttributeType.MpMax:
                case eAttributeType.LuckyDamage:
                case eAttributeType.ExcellentDamage:
                case eAttributeType.Hit:
                case eAttributeType.Dodge:
                case eAttributeType.HitRecovery:
                case eAttributeType.FireAttack:
                case eAttributeType.IceAttack:
                case eAttributeType.PoisonAttack:
                case eAttributeType.FireResistance:
                case eAttributeType.IceResistance:
                case eAttributeType.PoisonResistance:
                {
                    double fBili = 1.0f;
                    if (!_this.RobotPlayer)
                    {
                        //基础属性
                        nValue = AttrBaseManager.GetAttrValue(_this.mDataId, GetDataValue(_this, eAttributeType.Level),
                            type);
                        var BeRefList = AttrBaseManager.GetBeRefAttrList(_this.mDataId, type);
                        if (BeRefList != null)
                        {
                            foreach (var i in BeRefList)
                            {
                                if (i == 0)
                                {
                                    continue;
                                }
                                // ReSharper disable once PossibleLossOfFraction
                                nValue += GetDataValue(_this, (eAttributeType) i)*
                                          AttrBaseManager.GetAttrRef(_this.mDataId, (eAttributeType) i, type)/100;
                            }
                        }
                        //装备属性
                        nValue += _this.mEquipData[(int) type];
                        nValue += _this.mTalentData[(int) type];
                    }
                    nValue += _this.mBookData[(int) type];
                    nValue += _this.mTitleData[(int) type];
                    //Buff属性
                    nValue = nValue + _this.mObj.BuffList.Calculate(_this.mObj, type, ref fBili);
                    //计算
                    nValue =
                        (int)
                            (fBili*nValue*
                             (10000 + _this.mEquipDataRef[(int) type] + _this.mBookDataRef[(int) type] +
                              _this.mTalentDataRef[(int) type] + _this.mTitleDataRef[(int) type])/10000);
                }
                    break;
                case eAttributeType.LuckyPro:
                case eAttributeType.ExcellentPro:
                case eAttributeType.DamageAddPro:
                case eAttributeType.DamageResPro:
                case eAttributeType.DamageReboundPro:
                case eAttributeType.IgnoreArmorPro:
                {
                    double fBili = 1.0f;
                    if (!_this.RobotPlayer)
                    {
                        //基础属性
                        nValue = AttrBaseManager.GetAttrValue(_this.mDataId, GetDataValue(_this, eAttributeType.Level),
                            type);
                        //List<int> BeRefList = AttrBaseManager.GetBeRefAttrList(mDataId, type);
                        //if (BeRefList != null)
                        //{
                        //    foreach (var i in BeRefList)
                        //    {
                        //        if (i == 0) continue;
                        //        nValue += GetDataValue((eAttributeType)i) * AttrBaseManager.GetAttrRef(mDataId, (eAttributeType)i, type);
                        //    }
                        //}
                        //装备属性
                        //nValue += _this.mEquipData[(int) type]*100;
                        nValue += _this.mEquipData[(int)type];
                        nValue += _this.mTalentData[(int) type];
                    }
                    nValue += _this.mBookData[(int) type];
                    nValue += _this.mTitleData[(int) type];
                    //Buff属性
                    nValue = nValue + _this.mObj.BuffList.Calculate(_this.mObj, type, ref fBili);
                    //计算
                    nValue =
                        (int)
                            (fBili*nValue*
                             (10000 + _this.mEquipDataRef[(int) type] + _this.mBookDataRef[(int) type] +
                              _this.mTalentDataRef[(int) type] + _this.mTitleDataRef[(int) type])/10000);
                }
                    break;
                case eAttributeType.MoveSpeed:
                {
                    double fBili = 1.0f;
                    if (!_this.RobotPlayer)
                    {
                        //基础属性
                        nValue =
                            AttrBaseManager.GetAttrValue(_this.mDataId, GetDataValue(_this, eAttributeType.Level), type)*
                            _this.MoveSpeedModify/10000;
                        nValue = nValue*_this.GMMoveSpeedModify/100;
                        //装备属性
                        nValue += _this.mEquipData[(int) type];
                        nValue += _this.mTalentData[(int) type];
                    }
                    nValue += _this.mBookData[(int) type];
                    nValue += _this.mTitleData[(int) type];
                    //Buff属性
                    nValue = nValue + _this.mObj.BuffList.Calculate(_this.mObj, type, ref fBili);
                    //计算
                    nValue =
                        (int)
                            (fBili*nValue*
                             (10000 + _this.mEquipDataRef[(int) type] + _this.mBookDataRef[(int) type] +
                              _this.mTalentDataRef[(int) type] + _this.mTitleDataRef[(int) type])/10000);
                }
                    break;
                case eAttributeType.HpNow:
                {
                    nValue = GetDataValue(_this, eAttributeType.HpMax);
                }
                    break;
                case eAttributeType.MpNow:
                {
                    nValue = GetDataValue(_this, eAttributeType.MpMax);
                }
                    break;
                case eAttributeType.AttrCount:
                {
                    Logger.Warn("UpdateDataValue eAttributeType={0}!Why???", type);
                }
                    break;
                default:
                    break;
            }

            if (nValue == _this.mData[(int) type])
            {
                return (int) nValue;
            }

            SetDataValue(_this, type, (int) nValue);
            return _this.mData[(int) type];
        }

        //设置其它属性脏标记
        private void UpdateOtherFlag(FightAttr _this, eAttributeType type)
        {
            if (type >= eAttributeType.Count && type < eAttributeType.CountNext)
            {
                return;
            }

            if (type == eAttributeType.Level)
            {
                SetFlag(_this, eAttributeType.HpMax);
                SetFlag(_this, eAttributeType.MpMax);
                SetFlag(_this, eAttributeType.PhyPowerMin);
                SetFlag(_this, eAttributeType.PhyPowerMax);
                SetFlag(_this, eAttributeType.MagPowerMin);
                SetFlag(_this, eAttributeType.MagPowerMax);
            }
            if (_this.mObj.GetObjType() != ObjType.PLAYER)
            {
                return;
            }
            if (type >= eAttributeType.Count)
            {
                return;
            }
            var CanRefList = AttrBaseManager.GetCanRefAttrList(_this.mDataId, type);
            if (CanRefList == null)
            {
                return;
            }
            foreach (var i in CanRefList)
            {
                SetFlag(_this, (eAttributeType) i);
            }
        }

        //把属性复制到这个列表
        public void CopyToAttr(FightAttr _this, List<int> list)
        {
            list.Clear();
            foreach (var i in _this.mData)
            {
                list.Add(i);
            }
        }

        #endregion

        #region 公式相关(命中，暴击，Miss，护甲抗性减伤)

        //获得命中结果
        public eHitType GetHitResult(FightAttr _this, ObjCharacter character, eSkillHitType skillHitType)
        {
            switch (skillHitType)
            {
                case eSkillHitType.Normal:
                    return GetDamageHitType(_this, character);
                case eSkillHitType.Health:
                    return GetHealthHitType(_this, character);
                case eSkillHitType.Hit:
                    return GetMustHit(_this, character);
                case eSkillHitType.Critical:
                    return GetMustCritical(_this, character);
                default:
                    throw new ArgumentOutOfRangeException("skillHitType");
            }
        }

        //强制暴击
        public eHitType GetMustCritical(FightAttr _this, ObjCharacter character)
        {
            var tbCasterLevel = Table.GetLevelData(GetDataValue(_this, eAttributeType.Level));
            var tbBearLevel = Table.GetLevelData(character.Attr.GetDataValue(eAttributeType.Level));
            //Dictionary<eHitType,int> RoundData =new Dictionary<eHitType, int>();
            var RoundData = new List<int>((int) eHitType.Count - 1) {0, 0, 0};
            var Hit = (int) (GetDataValue(_this, eAttributeType.Hit)/tbBearLevel.Hit);
            var Dodge = (int) (character.Attr.GetDataValue(eAttributeType.Dodge)/tbCasterLevel.Dodge);
            Dodge = Dodge - Hit;
            if (Dodge < 0)
            {
                Dodge = 0;
            }
            RoundData[(int) eHitType.Miss] = Dodge;
            var Excellent = GetDataValue(_this, eAttributeType.ExcellentPro);
            if (Excellent < 0)
            {
                Excellent = 0;
            }
            RoundData[(int) eHitType.Excellent] = Excellent;
            RoundData[(int) eHitType.Lucky] = 10000;
            return GetHitTypeByRoundTable(_this, RoundData);
            //var tbCasterLevel = Table.GetLevelData(GetDataValue(eAttributeType.Level));
            //var tbBearLevel = Table.GetLevelData(character.Attr.GetDataValue(eAttributeType.Level));
            ////防守方闪避率
            //int BaseDodge = character.Attr.GetDataValue(eAttributeType.DodgePro);
            //int Dodge = (int)(BaseDodge + character.Attr.GetDataValue(eAttributeType.DodgeLevel) / tbCasterLevel.Dodge);

            ////攻击方命中率
            //int BaseHit = GetDataValue(eAttributeType.HitPro);
            //int Hit = (int)(BaseHit + GetDataValue(eAttributeType.HitLevel) / tbBearLevel.Hit);
            ////真正闪避率
            //int RealDodge = Dodge - Hit;
            //if (RealDodge < 0)
            //{
            //    RealDodge = 0;
            //}
            //else if (RealDodge > AttrBaseManager.DodgeMax)
            //{
            //    RealDodge = AttrBaseManager.DodgeMax;
            //}
            //int nRnd = MyRandom.Random(10000);
            //if (nRnd < RealDodge)
            //{
            //    return eHitType.Miss;
            //}
            //return eHitType.Critical;
        }

        //强制命中
        public eHitType GetMustHit(FightAttr _this, ObjCharacter character)
        {
            //Dictionary<eHitType,int> RoundData =new Dictionary<eHitType, int>();
            var RoundData = new List<int>((int) eHitType.Count - 1) {0, 0, 0};
            RoundData[(int) eHitType.Miss] = 0;
            var Critical = GetDataValue(_this, eAttributeType.LuckyPro);
            if (Critical < 0)
            {
                Critical = 0;
            }
            RoundData[(int) eHitType.Lucky] = Critical;
            var Excellent = GetDataValue(_this, eAttributeType.ExcellentPro);
            if (Excellent < 0)
            {
                Excellent = 0;
            }
            RoundData[(int) eHitType.Excellent] = Excellent;
            return GetHitTypeByRoundTable(_this, RoundData);
            //var tbCasterLevel = Table.GetLevelData(GetDataValue(eAttributeType.Level));
            //var tbBearLevel = Table.GetLevelData(character.Attr.GetDataValue(eAttributeType.Level));
            ////防守方韧性率
            //int BaseToughness = character.Attr.GetDataValue(eAttributeType.ToughnessPro);
            //int Toughness = (int)(BaseToughness + character.Attr.GetDataValue(eAttributeType.ToughnessLevel) / tbCasterLevel.Toughness);

            ////攻击方暴击率
            //int BaseCritical = GetDataValue(eAttributeType.CriticalPro);
            //int Critical = (int)(BaseCritical + GetDataValue(eAttributeType.CriticalLevel) / tbBearLevel.Critical);

            ////真正暴击率
            //int RealCritical = Critical - Toughness;
            //if (RealCritical < 0)
            //{
            //    RealCritical = 0;
            //}
            //else if (RealCritical > AttrBaseManager.DamageCritical)
            //{
            //    RealCritical = AttrBaseManager.DamageCritical;
            //}
            //int nRnd = MyRandom.Random(10000);
            //if (nRnd < RealCritical)
            //{
            //    return eHitType.Critical;
            //}
            //return eHitType.Hit;
        }

        //治疗命中
        public eHitType GetHealthHitType(FightAttr _this, ObjCharacter character)
        {
            var RoundData = new List<int>((int) eHitType.Count - 1) {0, 0, 0};
            RoundData[(int) eHitType.Miss] = 0;
            var Critical = GetDataValue(_this, eAttributeType.LuckyPro);
            if (Critical < 0)
            {
                Critical = 0;
            }
            RoundData[(int) eHitType.Lucky] = Critical;
            var Excellent = GetDataValue(_this, eAttributeType.ExcellentPro);
            if (Excellent < 0)
            {
                Excellent = 0;
            }
            RoundData[(int) eHitType.Excellent] = Excellent;
            return GetHitTypeByRoundTable(_this, RoundData);
            //var tbBearLevel = Table.GetLevelData(GetDataValue(eAttributeType.Level));
            ////攻击方暴击率
            //int BaseCritical = GetDataValue(eAttributeType.CriticalPro);
            //int Critical = (int)(BaseCritical + GetDataValue(eAttributeType.CriticalLevel) / tbBearLevel.Critical);
            ////真正暴击率
            //int RealCritical = Critical;
            //if (RealCritical < 0)
            //{
            //    RealCritical = 0;
            //}
            //else if (RealCritical > AttrBaseManager.HealthCritical)
            //{
            //    RealCritical = AttrBaseManager.HealthCritical;
            //}
            //int nRnd = MyRandom.Random(10000);
            //if (nRnd < RealCritical)
            //{
            //    return eHitType.Critical;
            //}
            //return eHitType.Hit;
        }

        //伤害命中
        public eHitType GetDamageHitType(FightAttr _this, ObjCharacter character)
        {
            var RoundData = new List<int>((int) eHitType.Count - 1) {0, 0, 0};
            var tbCasterLevel = Table.GetLevelData(GetDataValue(_this, eAttributeType.Level));
            var tbBearLevel = Table.GetLevelData(character.Attr.GetDataValue(eAttributeType.Level));
            var Hit = (int) (GetDataValue(_this, eAttributeType.Hit)/tbBearLevel.Hit);
            var Dodge = (int) (character.Attr.GetDataValue(eAttributeType.Dodge)/tbCasterLevel.Dodge);
            Dodge = Dodge - Hit;
            if (Dodge < 0)
            {
                Dodge = 0;
            }
            RoundData[(int) eHitType.Miss] = Dodge;
            var Critical = GetDataValue(_this, eAttributeType.LuckyPro);
            if (Critical < 0)
            {
                Critical = 0;
            }
            RoundData[(int) eHitType.Lucky] = Critical;
            var Excellent = GetDataValue(_this, eAttributeType.ExcellentPro);
            if (Excellent < 0)
            {
                Excellent = 0;
            }
            RoundData[(int) eHitType.Excellent] = Excellent;
            return GetHitTypeByRoundTable(_this, RoundData);
            //var tbCasterLevel = Table.GetLevelData(GetDataValue(eAttributeType.Level));
            //var tbBearLevel = Table.GetLevelData(character.Attr.GetDataValue(eAttributeType.Level));
            ////防守方闪避率
            //int BaseDodge = character.Attr.GetDataValue(eAttributeType.DodgePro);
            //int Dodge = (int)(BaseDodge + character.Attr.GetDataValue(eAttributeType.DodgeLevel) / tbCasterLevel.Dodge);

            ////攻击方命中率
            //int BaseHit = GetDataValue(eAttributeType.HitPro);
            //int Hit = (int)(BaseHit + GetDataValue(eAttributeType.HitLevel) / tbBearLevel.Hit);
            ////真正闪避率
            //int RealDodge = Dodge - Hit;
            //if (RealDodge < 0)
            //{
            //    RealDodge = 0;
            //}
            //else if (RealDodge > AttrBaseManager.DodgeMax)
            //{
            //    RealDodge = AttrBaseManager.DodgeMax;
            //}
            ////防守方韧性率
            //int BaseToughness = character.Attr.GetDataValue(eAttributeType.ToughnessPro);
            //int Toughness = (int)(BaseToughness + character.Attr.GetDataValue(eAttributeType.ToughnessLevel) / tbCasterLevel.Toughness);

            ////攻击方暴击率
            //int BaseCritical = GetDataValue(eAttributeType.CriticalPro);
            //int Critical = (int)(BaseCritical + GetDataValue(eAttributeType.CriticalLevel) / tbBearLevel.Critical);

            ////真正暴击率
            //int RealCritical = Critical - Toughness;
            //if (RealCritical < 0)
            //{
            //    RealCritical = 0;
            //}
            //else if (RealCritical > AttrBaseManager.DamageCritical)
            //{
            //    RealCritical = AttrBaseManager.DamageCritical;
            //}
            //int nRnd = MyRandom.Random(10000);
            //if (nRnd < RealDodge)
            //{
            //    return eHitType.Miss;
            //}
            //else if (nRnd < RealDodge + RealCritical)
            //{
            //    return eHitType.Critical;
            //}
            //return eHitType.Hit;
        }

        //获得伤害
        public int GetDamageValue(FightAttr _this, ObjCharacter bear, int attrId, eDamageType damageType)
        {
            if (attrId >= (int)eAttributeType.Count && attrId < (int)eAttributeType.CountNext)
            {
                return 0;
            }

            if (attrId < 0 || attrId >= (int) eAttributeType.AttrCount)
            {
                return 0;
            }
            var nValue = GetDataValue(_this, (eAttributeType) attrId);
            var armor = 0;
            if (damageType == eDamageType.Physical)
            {
                //物理伤害类型：取物理防御
                armor = bear.GetAttribute(eAttributeType.PhyArmor);
            }
            else if (damageType == eDamageType.Magic)
            {
                //法术伤害类型：取物理防御
                armor = bear.GetAttribute(eAttributeType.MagArmor);
            }
            //忽视防御率
            if (armor > 0)
            {
                var resArmor = GetDataValue(_this, eAttributeType.IgnoreArmorPro);
                armor = (int) ((armor*(10000.0 - resArmor)/10000.0));
                if (armor < 0)
                {
                    armor = 0;
                }
            }

            if (attrId == (int) eAttributeType.PhyPowerMax)
            {
                var maxValue = nValue; //物攻最大值
                var minValue = GetDataValue(_this, eAttributeType.PhyPowerMin); //物攻最小值
                nValue = MyRandom.Random(minValue, maxValue) - armor;
            }
            else if (attrId == (int) eAttributeType.MagPowerMax)
            {
                var maxValue = nValue;
                var minValue = GetDataValue(_this, eAttributeType.MagPowerMin);
                nValue = MyRandom.Random(minValue, maxValue) - armor;
            }
            else
            {
//其他属性
                nValue = nValue - armor;
            }
            if (nValue < 0)
            {
                nValue = MyRandom.Random(1, 5);
            }
            return nValue;
        }

        // 属性伤害
        public int GetAttrDamageValue(FightAttr _this, ObjCharacter bear, eAttributeType type)
        {
            eAttributeType defType;
            switch (type)
            {
                case eAttributeType.FireAttack:
                    defType = eAttributeType.FireResistance;
                    break;
                case eAttributeType.IceAttack:
                    defType = eAttributeType.IceResistance;
                    break;
                case eAttributeType.PoisonAttack:
                    defType = eAttributeType.PoisonResistance;
                    break;
                default:
                    return 0;
            }

            var damage = GetDataValue(_this, type);
            var defence = bear.GetAttribute(defType);

            return Math.Max(0, damage - defence);
        }

        //获得护甲减伤
        /// <summary>
        ///     获得护甲减伤
        /// </summary>
        /// <returns></returns>
        public double GetPhyReduction(FightAttr _this, int nAttackLevel)
        {
            if (nAttackLevel < 0 || nAttackLevel > AttrBaseManager.LevelMax)
            {
                return 0;
            }
            double reduction = 0;
            var armor = GetDataValue(_this, eAttributeType.PhyArmor);
            //reduction = (double)armor / (armor + AttrBaseManager.PhyReduce[nAttackLevel]);
            return reduction;
        }

        //获得抗性减伤
        /// <summary>
        ///     获得抗性减伤
        /// </summary>
        /// <returns></returns>
        public double GetMagReduction(FightAttr _this, int nAttackLevel)
        {
            if (nAttackLevel < 0 || nAttackLevel > AttrBaseManager.LevelMax)
            {
                return 0;
            }
            double reduction = 0;
            var resistance = GetDataValue(_this, eAttributeType.MagArmor);
            //reduction = (double)resistance / (resistance + AttrBaseManager.MagReduce[nAttackLevel]);
            return reduction;
        }

        //根据圆桌理论获得结论
        //1、闪避率
        //2、卓越率
        //3、幸运率
        //4、一般命中率
        //Dictionary<eHitType,int>
        public eHitType GetHitTypeByRoundTable(FightAttr _this, List<int> RoundData)
        {
            var rnd = MyRandom.Random(0, 10000);
            var index = 0;
            foreach (var nPro in RoundData)
            {
                if (rnd < nPro)
                {
                    return (eHitType) index;
                }
                rnd -= nPro;
                index++;
            }
            return eHitType.Hit;
        }

        #endregion

        #region 战斗力相关

        public int GetFightPoint(FightAttr _this)
        {
            var ischange = _this.FightPointFlag;
            if (!ischange)
            {
                ischange = _this.mObj.Skill.GetFightPointFlag();
            }
            var attrF = GetAttrFightPoint(_this);
            var skillF = _this.mObj.Skill.GetFightPoint();
            var elfF = _this.mObj.GetElfSkillFightPoint();
            var fp = attrF + skillF + elfF;
            var tempObj = _this.mObj as ObjPlayer;
            if (tempObj != null && ischange && tempObj.Proxy != null)
            {
                tempObj.FriendsDirty = true;

                if (_this.RankTrigger == null)
                {

                    var now = DateTime.Now.AddMinutes(5);
                    var nextTime =
                        new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / 5 * 5, 0)
                            .AddSeconds(-(_this.mObj.ObjId % 10.0f));

                    if (nextTime < DateTime.Now)
                    {
                        nextTime = nextTime.AddMinutes(5);
                    }


                    _this.RankTrigger = SceneServerControl.Timer.CreateTrigger(nextTime,
                        () => { RankSendChangesBytrigger(_this); });
                }
                //CoroutineFactory.NewCoroutine(ScenetoRankCoroutine, mObj, fp).MoveNext();
            }
            return fp;
        }

        public void RankSendChanges(FightAttr _this)
        {
            if (_this.RankTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.RankTrigger);
                _this.RankTrigger = null;
                CoroutineFactory.NewCoroutine(ScenetoRankCoroutine, _this, _this.mObj,
                    GetAttrFightPoint(_this) + _this.mObj.Skill.GetFightPoint() + _this.mObj.GetElfSkillFightPoint()).MoveNext();
            }
        }

        public void RankSendChangesBytrigger(FightAttr _this)
        {
            _this.RankTrigger = null;
            CoroutineFactory.NewCoroutine(ScenetoRankCoroutine, _this, _this.mObj,
                GetAttrFightPoint(_this) + _this.mObj.Skill.GetFightPoint() + _this.mObj.GetElfSkillFightPoint()).MoveNext();
        }

        // 增加战斗力
        private void AddFightPoint(FightAttr _this, int value, string key)
        {
            if (0 >= value)
                return;

            _this.FightPoint += value;
        }

        // 计算战斗力
        private int GetAttrFightPoint(FightAttr _this)
        {
            if (!_this.FightPointFlag)
            {
                return _this.FightPoint;
            }
            _this.FightPointFlag = false;
            var level = GetDataValue(_this, eAttributeType.Level);
            var tbLevel = Table.GetLevelData(level);
            _this.FightPoint = 0;

            var RefBase = new Dictionary<eAttributeType, long>();
            RefBase[eAttributeType.PhyArmor] = 0;
            RefBase[eAttributeType.MagPowerMin] = 0;
            RefBase[eAttributeType.HpMax] = 0;
            RefBase[eAttributeType.MpMax] = 0;
            for (var type = eAttributeType.PhyPowerMin; type != eAttributeType.Count; ++type)
            {
                //基础固定属性
                var nValue = AttrBaseManager.GetAttrValue(_this.mDataId, level, type);
                var BeRefList = AttrBaseManager.GetBeRefAttrList(_this.mDataId, type);
                if (BeRefList != null)
                {
                    foreach (var i in BeRefList)
                    {
                        if (i == 0)
                        {
                            continue;
                        }
                        nValue += (int)(ActionNumBer(GetDataValue(_this, (eAttributeType)i), AttrBaseManager.GetAttrRef(_this.mDataId, (eAttributeType)i, type)) / 100);
                    }
                }
                nValue += _this.mBookData[(int) type]; //图鉴固定的
                nValue += _this.mTalentData[(int) type]; //天赋固定的
                nValue += _this.mTitleData[(int) type]; //称号固定的


                //战盟Buff的属性
                foreach (var data in _this.mObj.BuffList.mData)
                {
                    if (data.GetActive() == false)
                    {
                        continue;
                    }
                    var buffId = data.GetBuffId();
                    if (buffId < 500 || buffId > 503)
                    {
                        continue;
                    }
                    var index = -1;
                    foreach (var i in data.mBuff.effectid)
                    {
                        index++;
                        if (i != (int) eEffectType.RefAttr)
                        {
                            continue;
                        }
                        var param = data.mBuff.effectparam;
                        if (type != (eAttributeType) param[index, 0])
                        {
                            continue;
                        }
                        nValue += (int)ActionNumBer(param[index, 3], data.GetLayer());
                        
                    }
                }

                switch (type)
                {
                    case eAttributeType.LuckyPro:
                    {
                        //nValue += _this.mEquipData[(int) type] * 100; //装备固定的
                        nValue += _this.mEquipData[(int)type]; //装备固定的
                        AddFightPoint(_this, (int) (ActionNumBer(nValue, tbLevel.LuckyProFightPoint) / 10000), "LuckyPro");
                    }
                        break;
                    case eAttributeType.ExcellentPro:
                    {
                        //nValue += _this.mEquipData[(int) type] * 100; //装备固定的
                        nValue += _this.mEquipData[(int)type]; //装备固定的
                        AddFightPoint(_this, (int)(ActionNumBer(nValue, tbLevel.ExcellentProFightPoint) / 10000), "ExcellentPro");
                    }
                        break;
                    case eAttributeType.DamageAddPro:
                    {
                        //nValue += _this.mEquipData[(int) type] * 100; //装备固定的
                        nValue += _this.mEquipData[(int)type]; //装备固定的
                        AddFightPoint(_this, (int)(ActionNumBer(nValue, tbLevel.DamageAddProFightPoint) / 10000), "DamageAddPro");
                    }
                        break;
                    case eAttributeType.DamageResPro:
                    {
                        //nValue += _this.mEquipData[(int) type] * 100; //装备固定的
                        nValue += _this.mEquipData[(int)type]; //装备固定的
                        AddFightPoint(_this, (int)(ActionNumBer(nValue, tbLevel.DamageResProFightPoint) / 10000), "DamageResPro");
                    }
                        break;
                    case eAttributeType.DamageReboundPro:
                    {
                        //nValue += _this.mEquipData[(int) type] * 100; //装备固定的
                        nValue += _this.mEquipData[(int)type]; //装备固定的
                        AddFightPoint(_this, (int)(ActionNumBer(nValue, tbLevel.DamageReboundProFightPoint) / 10000), "DamageReboundPro");
                    }
                        break;
                    case eAttributeType.IgnoreArmorPro:
                    {
                        //nValue += _this.mEquipData[(int) type] * 100; //装备固定的
                        nValue += _this.mEquipData[(int)type]; //装备固定的
                        AddFightPoint(_this, (int)(ActionNumBer(nValue, tbLevel.IgnoreArmorProFightPoint) / 10000), "IgnoreArmorPro");
                    }
                        break;
                    default:
                    {
                        nValue += _this.mEquipData[(int) type]; //装备固定的
                        var tbState = Table.GetStats((int) type);
                        if (tbState == null)
                        {
                            continue;
                        }
                        if (_this.mDataId >= 0 && _this.mDataId < 3)
                        {
                            var baseValue = ActionNumBer(tbState.FightPoint[_this.mDataId], nValue);
                            AddFightPoint(_this, (int)(baseValue / 100), type.ToString());
                            if (type == eAttributeType.HpMax || type == eAttributeType.MagPowerMin ||
                                type == eAttributeType.PhyArmor
                                || type == eAttributeType.MpMax || type == eAttributeType.Hit ||
                                type == eAttributeType.Dodge)
                            {
                                RefBase[type] = baseValue;
                            }
                        }
                    }
                        break;
                }
            }

            var Ref = new Dictionary<eAttributeType, int>();
            //图鉴百分比
            //装备百分比
            //天赋百分比
            Ref[eAttributeType.MagPowerMin] = _this.mBookDataRef[(int) eAttributeType.MagPowerMin] +
                                              _this.mEquipDataRef[(int) eAttributeType.MagPowerMin] +
                                              _this.mTalentDataRef[(int) eAttributeType.MagPowerMin] +
                                              _this.mTitleDataRef[(int) eAttributeType.MagPowerMin];
            Ref[eAttributeType.PhyArmor] = _this.mBookDataRef[(int) eAttributeType.PhyArmor] +
                                           _this.mEquipDataRef[(int) eAttributeType.PhyArmor] +
                                           _this.mTalentDataRef[(int) eAttributeType.PhyArmor] +
                                           _this.mTitleDataRef[(int) eAttributeType.PhyArmor];
            Ref[eAttributeType.HpMax] = _this.mBookDataRef[(int) eAttributeType.HpMax] +
                                        _this.mEquipDataRef[(int) eAttributeType.HpMax] +
                                        _this.mTalentDataRef[(int) eAttributeType.HpMax] +
                                        _this.mTitleDataRef[(int) eAttributeType.HpMax];
            Ref[eAttributeType.MpMax] = _this.mBookDataRef[(int) eAttributeType.MpMax] +
                                        _this.mEquipDataRef[(int) eAttributeType.MpMax] +
                                        _this.mTalentDataRef[(int) eAttributeType.MpMax] +
                                        _this.mTitleDataRef[(int) eAttributeType.MpMax];
            Ref[eAttributeType.Hit] = _this.mBookDataRef[(int) eAttributeType.Hit] +
                                      _this.mEquipDataRef[(int) eAttributeType.Hit] +
                                      _this.mTalentDataRef[(int) eAttributeType.Hit] +
                                      _this.mTitleDataRef[(int) eAttributeType.Hit];
            Ref[eAttributeType.Dodge] = _this.mBookDataRef[(int) eAttributeType.Dodge] +
                                        _this.mEquipDataRef[(int) eAttributeType.Dodge] +
                                        _this.mTalentDataRef[(int) eAttributeType.Dodge] +
                                        _this.mTitleDataRef[(int) eAttributeType.Dodge];
            //百分比计算
            AddFightPoint(_this, (int)(ActionNumBer(Ref[eAttributeType.Hit], tbLevel.HitFightPoint) / 10000), "Hit");
            AddFightPoint(_this, (int)(ActionNumBer(Ref[eAttributeType.Dodge], tbLevel.DodgeFightPoint) / 10000), "Dodge");
            AddFightPoint(_this, RefFightPoint(RefBase[eAttributeType.MagPowerMin], Ref[eAttributeType.MagPowerMin], tbLevel.PowerFightPoint), "MagPowerMin");
            AddFightPoint(_this, RefFightPoint(RefBase[eAttributeType.PhyArmor], Ref[eAttributeType.PhyArmor], tbLevel.ArmorFightPoint), "PhyArmor");
            AddFightPoint(_this, RefFightPoint(RefBase[eAttributeType.HpMax], Ref[eAttributeType.HpMax], tbLevel.HpFightPoint), "HpMax");
            AddFightPoint(_this, RefFightPoint(RefBase[eAttributeType.MpMax], Ref[eAttributeType.MpMax], tbLevel.MpFightPoint), "MpMax");

            //套装
            AddFightPoint(_this, _this.EquipTieFightPoint, "EquipTieFightPoint");
            //技能
            //FightPoint += mObj.Skill.GetFightPoint();
            return _this.FightPoint;
        }

        public Dictionary<eAttributeType, long> GetBaseAttr(FightAttr _this)
        {
            var level = GetDataValue(_this, eAttributeType.Level);
            var refBase = new Dictionary<eAttributeType, long>();
            for (var type = eAttributeType.PhyPowerMin; type != eAttributeType.Count; ++type)
            {
                if (type == eAttributeType.HpMax || type == eAttributeType.MagPowerMin ||
                    type == eAttributeType.PhyArmor
                    || type == eAttributeType.MpMax || type == eAttributeType.Hit || type == eAttributeType.Dodge)
                { // 基础固定属性
                    var nValue = AttrBaseManager.GetAttrValue(_this.mDataId, level, type);
                    var BeRefList = AttrBaseManager.GetBeRefAttrList(_this.mDataId, type);
                    if (BeRefList != null)
                    {
                        foreach (var i in BeRefList)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            nValue +=
                                (int)
                                    (ActionNumBer(GetDataValue(_this, (eAttributeType) i),
                                        AttrBaseManager.GetAttrRef(_this.mDataId, (eAttributeType) i, type))/100);
                        }
                    }
                    nValue += _this.mBookData[(int) type]; //图鉴固定的
                    nValue += _this.mTalentData[(int) type]; //天赋固定的
                    nValue += _this.mTitleData[(int) type]; //称号固定的


                    //战盟Buff的属性
                    foreach (var data in _this.mObj.BuffList.mData)
                    {
                        if (data.GetActive() == false)
                        {
                            continue;
                        }
                        var buffId = data.GetBuffId();
                        if (buffId < 500 || buffId > 503)
                        {
                            continue;
                        }
                        var index = 0;
                        foreach (var i in data.mBuff.effectid)
                        {
                            if (i != (int) eEffectType.RefAttr)
                            {
                                continue;
                            }
                            var param = data.mBuff.effectparam;
                            if (type != (eAttributeType) param[index, 0])
                            {
                                continue;
                            }
                            nValue += (int) ActionNumBer(param[index, 3], data.GetLayer());
                            index++;
                        }
                    }

                    nValue += _this.mEquipData[(int) type]; //装备固定的
                    var tbState = Table.GetStats((int) type);
                    if (tbState == null)
                    {
                        continue;
                    }
                    if (_this.mDataId >= 0 && _this.mDataId < 3)
                    {
                        var baseValue = (long)tbState.FightPoint[_this.mDataId] * nValue;
                        refBase[type] = baseValue;
                    }
                }
            }
            return refBase;
        }


        private int RefFightPoint(long baseValue, int refValue, int level)
        {
            long v = baseValue * refValue / 10000L / 100L;
            return (int)v;
        }

        private long ActionNumBer(int a, int b)
        {
            var res = 0L;
            res = (long)(a) * (b);
            return res;
        }

        //设置战斗力标记
        public void SetFightPointFlag(FightAttr _this, bool b = true)
        {
            _this.FightPointFlag = b;
            _this.OnPropertyChanged((uint) eSceneSyncId.SyncFightValue);
        }

        //向排行榜同步数
        public IEnumerator ScenetoRankCoroutine(Coroutine co, FightAttr _this, ObjCharacter character, int fp)
        {
            var msg = SceneServer.Instance.RankAgent.CharacterChangeData(character.ObjId, 0, character.ServerId,
                character.ObjId, character.GetName(), fp);
            yield return msg.SendAndWaitUntilDone(co);

	        var msg1 = SceneServer.Instance.LogicAgent.SSSyncCharacterFightPoint(character.ObjId, fp);
			yield return msg1.SendAndWaitUntilDone(co);
        }

        #endregion
    }

    //玩家的属性
    public class FightAttr : ObjCharacter.INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IFightAttr mImpl;

        static FightAttr()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (FightAttr), typeof (FightAttrDefaultImpl),
                o => { mImpl = (IFightAttr) o; });
        }

        public virtual void OnPropertyChanged(uint id)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new ObjCharacter.PropertyChangedEventArgs(id));
            }
        }

        public event ObjCharacter.PropertyChangedEventHandler PropertyChanged;

        #region  数据结构

        public int mDataId; //Character数据ID
        //Obj
        public ObjCharacter mObj;
        //属性
        public int[] mData = new int[(int) eAttributeType.AttrCount];
        public int MoveSpeedModify = 10000;
        public int GMMoveSpeedModify = 100;
        public int Ladder = 0; //转生次数
        public int Vip { get; set; }
        public int Honor = 0; //军衔
        public int ExpRef = 10000; //经验修正比例
        public int ExpRef2 = 10000; //经验修正比例
        public float ExpAdd = 10000.0f;
        public int LifeCardFlag { get; set; }//终身卡

        public int _ExpRef
        {
            get { return ExpRef*ExpRef2/10000; }
        }

        public bool RobotPlayer = false; //是否玩家机器人
        public bool PlayerInit;
        //装备的称号
        public int[] mEquipedTitles = {-1, -1, -1, -1,-1};
        //所有算属性的称号
        public List<int> mTitles = new List<int>();

        public int mLevel
        {
            get { return mData[0]; }
        }

        //属性脏标记
        public BitFlag mFlag = new BitFlag((int) eAttributeType.AttrCount, -1);
        //装备数据
        public int[] mEquipData = new int[(int) eAttributeType.AttrCount];
        public int[] mEquipDataRef = new int[(int) eAttributeType.AttrCount];
        //图鉴数据
        public int[] mBookData = new int[(int) eAttributeType.AttrCount];
        public int[] mBookDataRef = new int[(int) eAttributeType.AttrCount];
        public Dictionary<int,int> mMonsterAttr = new Dictionary<int, int>();
        //天赋数据
        public int[] mTalentData = new int[(int) eAttributeType.AttrCount];
        public int[] mTalentDataRef = new int[(int) eAttributeType.AttrCount];
        //称号数据
        public int[] mTitleData = new int[(int) eAttributeType.AttrCount];
        public int[] mTitleDataRef = new int[(int) eAttributeType.AttrCount];
        //套装Buff
        public int EquipTieFightPoint;
        public Dictionary<int, BuffData> mEquipBuff = new Dictionary<int, BuffData>();
        //临时属性
        public int[] mTempData = new int[(int) eAttributeType.AttrCount];
        public int[] mTempDataRef = new int[(int) eAttributeType.AttrCount];
        //战斗力
        public int FightPoint;
        public bool FightPointFlag = true;

        #endregion

        #region  初始化

        public int GetAttackType()
        {
            return mImpl.GetAttackType(this);
        }

        public FightAttr(ObjCharacter obj)
        {
            mImpl.InitFightAttr(this, obj);
        }

        //刷新天赋数据
        public void TalentRefresh(Dictionary<int, int> talentAttrs)
        {
            mImpl.TalentRefresh(this, talentAttrs);
        }

        //刷新图鉴数据
        public void BookRefresh(Dictionary<int, int> bookAttrs, Dictionary<int, int> monsterAttrs)
        {
            mImpl.BookRefresh(this, bookAttrs,monsterAttrs);
        }

        //刷新称号
        public void TitleRefresh(List<int> titles, int type)
        {
            mImpl.TitleRefresh(this, titles, type);
        }

        //刷新装备数据
        public void EquipRefresh()
        {
            mImpl.EquipRefresh(this);
        }

        public ErrorCodes CheckEquipOn(EquipRecord tbEquip)
        {
            return mImpl.CheckEquipOn(this, tbEquip);
        }

        //批量整理装备属性
        public void BatchEquipAttr(ref int[] attr, ref int[] attrRef, Dictionary<int, ItemEquip2> equipList)
        {
            mImpl.BatchEquipAttr(this, ref attr, ref attrRef, equipList);
        }

        //初始化武将后开始计算属性
        public void InitAttributesAll()
        {
            mImpl.InitAttributesAll(this);
        }

        public void InitAttributessAllEx(Dictionary<int,int> dic)
        {
            mImpl.InitAttributesAllEx(this,dic);
        }

        #endregion

        #region  数据存取

        public string GetStringData()
        {
            return mImpl.GetStringData(this);
        }

        public void SetTitles(List<int> titles)
        {
            mImpl.SetTitles(this, titles);
        }

        //获得属性
        public int GetDataValue(eAttributeType type)
        {
            return mImpl.GetDataValue(this, type);
        }

        //设置武将属性值
        public void SetDataValue(eAttributeType type, int value)
        {
            mImpl.SetDataValue(this, type, value);
        }

        //设置属性脏标记
        public void SetFlag(eAttributeType type)
        {
            mImpl.SetFlag(this, type);
        }

        //由属性ID修改标记位
        public void SetFlagByAttrId(int attrId)
        {
            mImpl.SetFlagByAttrId(this, attrId);
        }

        //把属性复制到这个列表
        public void CopyToAttr(List<int> list)
        {
            mImpl.CopyToAttr(this, list);
        }

        #endregion

        #region 公式相关(命中，暴击，Miss，护甲抗性减伤)

        //获得命中结果
        public eHitType GetHitResult(ObjCharacter character, eSkillHitType skillHitType)
        {
            return mImpl.GetHitResult(this, character, skillHitType);
        }

        //强制暴击
        public eHitType GetMustCritical(ObjCharacter character)
        {
            return mImpl.GetMustCritical(this, character);
        }

        //强制命中
        public eHitType GetMustHit(ObjCharacter character)
        {
            return mImpl.GetMustHit(this, character);
        }

        //治疗命中
        public eHitType GetHealthHitType(ObjCharacter character)
        {
            return mImpl.GetHealthHitType(this, character);
        }

        //伤害命中
        public eHitType GetDamageHitType(ObjCharacter character)
        {
            return mImpl.GetDamageHitType(this, character);
        }

        //获得伤害
        public int GetDamageValue(ObjCharacter bear, int attrId, eDamageType damageType)
        {
            return mImpl.GetDamageValue(this, bear, attrId, damageType);
        }

        public int GetAttrDamageValue(ObjCharacter bear, eAttributeType type)
        {
            return mImpl.GetAttrDamageValue(this, bear, type);
        }

        //获得护甲减伤
        /// <summary>
        ///     获得护甲减伤
        /// </summary>
        /// <returns></returns>
        public double GetPhyReduction(int nAttackLevel)
        {
            return mImpl.GetPhyReduction(this, nAttackLevel);
        }

        //获得抗性减伤
        /// <summary>
        ///     获得抗性减伤
        /// </summary>
        /// <returns></returns>
        public double GetMagReduction(int nAttackLevel)
        {
            return mImpl.GetMagReduction(this, nAttackLevel);
        }

        //根据圆桌理论获得结论
        //1、闪避率
        //2、卓越率
        //3、幸运率
        //4、一般命中率
        //Dictionary<eHitType,int>
        public eHitType GetHitTypeByRoundTable(List<int> RoundData)
        {
            return mImpl.GetHitTypeByRoundTable(this, RoundData);
        }

        #endregion

        #region 战斗力相关

        public Trigger RankTrigger;

        public int GetFightPoint()
        {
            return mImpl.GetFightPoint(this);
        }

        public void RankSendChanges()
        {
            mImpl.RankSendChanges(this);
        }

        public void RankSendChangesBytrigger()
        {
            mImpl.RankSendChangesBytrigger(this);
        }

        //设置战斗力标记
        public void SetFightPointFlag(bool b = true)
        {
            mImpl.SetFightPointFlag(this, b);
        }

        //向排行榜同步数
        public IEnumerator ScenetoRankCoroutine(Coroutine co, ObjCharacter character, int fp)
        {
            return mImpl.ScenetoRankCoroutine(co, this, character, fp);
        }

        #endregion
    }
}