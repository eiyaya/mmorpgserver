#region using

using System;
using DataContract;
using DataTable;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene.Character
{
    public interface IAutoPlayer
    {
        /// <summary>
        ///     输出成一个objdata用于客户端创建
        /// </summary>
        /// <returns></returns>
        ObjData DumpObjData(AutoPlayer _this, ReasonType reason);

        /// <summary>
        ///     获得等级
        /// </summary>
        /// <returns></returns>
        int GetLevel(AutoPlayer _this);

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="npcId"></param>
        /// <param name="logic"></param>
        /// <param name="scene"></param>
        void Init(AutoPlayer _this, ulong npcId, LogicSimpleData logic, SceneSimpleData scene);

        void InitAI(AutoPlayer _this);
        bool InitAttr(AutoPlayer _this, int level);
        bool InitBuff(AutoPlayer _this, int level);
        void InitByRobot(AutoPlayer _this, ulong npcId, int RobotId);
        bool InitEquip(AutoPlayer _this, int level);
        bool InitSkill(AutoPlayer _this, int level);

        /// <summary>
        ///     初始化表格数据，基类的Init会调用，逼不得已不要手动调
        /// </summary>
        int InitTableData(AutoPlayer _this, int level);

        /// <summary>
        ///     死亡时
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="characterId"></param>
        /// <param name="delayView"></param>
        /// <param name="damage"></param>
        void OnDie(AutoPlayer _this, ulong characterId, int delayView, int damage = 0);

        void OnEnterScene(AutoPlayer _this);
    }

    public class AutoPlayerDefaultImpl : IAutoPlayer
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     获得等级
        /// </summary>
        /// <returns></returns>
        public int GetLevel(AutoPlayer _this)
        {
            return _this.Attr.GetDataValue(eAttributeType.Level);
        }

        /// <summary>
        ///     死亡时
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="characterId"></param>
        /// <param name="delayView"></param>
        /// <param name="damage"></param>
        public void OnDie(AutoPlayer _this, ulong characterId, int delayView, int damage = 0)
        {
            var replyMsg = new BuffResultMsg();
            replyMsg.buff.Add(new BuffResult
            {
                TargetObjId = _this.ObjId,
                Type = BuffType.HT_DIE,
                ViewTime = Extension.AddTimeDiffToNet(delayView)
            });
            _this.BroadcastBuffList(replyMsg);
            _this.Skill.StopCurrentSkill();
            _this.RemoveMeFromOtherEnemyList();
            _this.ClearEnemy();

            //Drop.MonsterKill(this, characterId);
            try
            {
                _this.Scene.OnNpcDie(_this, characterId);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            _this.EnterState(BehaviorState.Die);
            //mTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(TableNpc.CorpseTime), Disapeare);

            try
            {
                _this.Script.OnDie(_this, characterId, delayView, damage);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /// <summary>
        ///     输出成一个objdata用于客户端创建
        /// </summary>
        /// <returns></returns>
        public ObjData DumpObjData(AutoPlayer _this, ReasonType reason)
        {
            var data = ObjCharacter.GetImpl().DumpObjData(_this, reason);
            data.EquipsModel.AddRange(_this.mLogicData.EquipsModel);
            //data.PkModel = (int)ePkModel.AutoPlayer;
            //GetEquipsModel(data.EquipsModel);
            data.Owner = new Uint64Array();
            data.Owner.Items.Add(_this.mLogicData.Id);
            data.Reborn = _this.mLogicData.Ladder;
            return data;
        }

        public void OnEnterScene(AutoPlayer _this)
        {
            ObjNPC.GetImpl().OnEnterScene(_this);
            if (null != _this.Scene)
            {
                var tag = _this.Scene.GetObstacleValue(_this.GetPosition().X, _this.GetPosition().Y);
                if (tag == SceneObstacle.ObstacleValue.Runable)
                {
//跑
                    _this.NormalAttr.AreaState = eAreaState.Wild;
                }
                else if (tag == SceneObstacle.ObstacleValue.Walkable)
                {
//走
                    _this.NormalAttr.AreaState = eAreaState.City;
                }
            }
        }

        #region 初始化

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="npcId"></param>
        /// <param name="logic"></param>
        /// <param name="scene"></param>
        public void Init(AutoPlayer _this, ulong npcId, LogicSimpleData logic, SceneSimpleData scene)
        {
            _this.mLogicData = logic;
            _this.mSceneData = scene;
            _this.mObjId = npcId;
            _this.mTypeId = logic.TypeId;
            _this.mDirection = new Vector2(1, 0);
            _this.mName = _this.mSceneData.Name;
            _this.BuffList = new BuffList();
            _this.BuffList.InitByBase(_this);
            _this.Attr = new FightAttr(_this);
            _this.Skill = new SkillManager(_this);
            _this.SetLevel(_this.mLogicData.Level);
            _this.InitTableData(_this.mLogicData.Level);
            _this.InitEquip(_this.mLogicData.Level);
            _this.InitSkill(_this.mLogicData.Level);
            _this.InitBuff(_this.mLogicData.Level);
            _this.Attr.RobotPlayer = true;
            _this.InitAttr(_this.mLogicData.Level);
            _this.mCamp = 2;
            _this.TableCamp = Table.GetCamp(_this.mCamp);
            _this.InitAI(_this.mLogicData.Level);
        }

        public void InitAI(AutoPlayer _this)
        {
            _this.Script = NPCScriptRegister.CreateScriptInstance(220000);
            _this.m_AITimer = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(_this.mAiTickSeconds),
                () => { _this.Tick_AI(); }, (int) (_this.mAiTickSeconds*1000));
        }

        public void InitByRobot(AutoPlayer _this, ulong npcId, int RobotId)
        {
            _this.mObjId = npcId;
            _this.mLogicData = new LogicSimpleData();
            _this.mLogicData.Id = (ulong) RobotId;
            //mSceneData = scene;
            var tbRobot = Table.GetJJCRoot(RobotId);
            _this.mTypeId = tbRobot.Career;
            //mTypeId = logic.TypeId;
            _this.mDirection = new Vector2(1, 0);
            _this.mName = tbRobot.Name;
            _this.BuffList = new BuffList();
            _this.BuffList.InitByBase(_this);
            _this.Attr = new FightAttr(_this);
            _this.Skill = new SkillManager(_this);
            _this.SetLevel(tbRobot.Level);
            var lastLadder = 0;
            Table.ForeachTransmigration(record =>
            {
                if (record.TransLevel <= tbRobot.Level)
                {
                    if (record.PropPoint < lastLadder)
                    {
                        return false;
                    }
                    lastLadder = record.PropPoint;
                }
                else
                {
                    return false;
                }
                return true;
            });
            _this.Attr.Ladder = lastLadder;
            _this.mLogicData.Ladder = lastLadder;
            _this.InitTableData(tbRobot.Level);
            //InitEquip();
            if (tbRobot.EquipHand != -1)
            {
                var bagId = 17;
                var dbitem = new ItemBaseData();
                var item = new ItemEquip2(tbRobot.EquipHand, dbitem);
                item.SetExdata(0, tbRobot.EquipLevel);
                _this.Equip.Add(bagId*10, item);
                _this.mLogicData.EquipsModel.Add(bagId, tbRobot.EquipHand*100 + tbRobot.EquipLevel);
            }

            if (tbRobot.EquipHead != -1)
            {
                var bagId = 7;
                var dbitem = new ItemBaseData();
                var item = new ItemEquip2(tbRobot.EquipHead, dbitem);
                item.SetExdata(0, tbRobot.EquipLevel);
                _this.Equip.Add(bagId*10, item);
                _this.mLogicData.EquipsModel.Add(bagId, tbRobot.EquipHead*100 + tbRobot.EquipLevel);
            }
            if (tbRobot.EquipChest != -1)
            {
                var bagId = 11;
                var dbitem = new ItemBaseData();
                var item = new ItemEquip2(tbRobot.EquipChest, dbitem);
                item.SetExdata(0, tbRobot.EquipLevel);
                _this.Equip.Add(bagId*10, item);
                _this.mLogicData.EquipsModel.Add(bagId, tbRobot.EquipChest*100 + tbRobot.EquipLevel);
            }

            if (tbRobot.EquipGlove != -1)
            {
                var bagId = 14;
                var dbitem = new ItemBaseData();
                var item = new ItemEquip2(tbRobot.EquipGlove, dbitem);
                item.SetExdata(0, tbRobot.EquipLevel);
                _this.Equip.Add(bagId*10, item);
                _this.mLogicData.EquipsModel.Add(bagId, tbRobot.EquipGlove*100 + tbRobot.EquipLevel);
            }

            if (tbRobot.EquipTrouser != -1)
            {
                var bagId = 15;
                var dbitem = new ItemBaseData();
                var item = new ItemEquip2(tbRobot.EquipTrouser, dbitem);
                item.SetExdata(0, tbRobot.EquipLevel);
                _this.Equip.Add(bagId*10, item);
                _this.mLogicData.EquipsModel.Add(bagId, tbRobot.EquipTrouser*100 + tbRobot.EquipLevel);
            }

            if (tbRobot.EquipShoes != -1)
            {
                var bagId = 16;
                var dbitem = new ItemBaseData();
                var item = new ItemEquip2(tbRobot.EquipShoes, dbitem);
                item.SetExdata(0, tbRobot.EquipLevel);
                _this.Equip.Add(bagId*10, item);
                _this.mLogicData.EquipsModel.Add(bagId, tbRobot.EquipShoes*100 + tbRobot.EquipLevel);
            }

            if (tbRobot.WingID != -1)
            {
                var bagId = 12;
                var dbitem = new ItemBaseData();
                var item = new WingItem(tbRobot.WingID, dbitem);
                var ib = new ItemEquip2();
                ib.SetId(dbitem.ItemId);
                ib.SetCount(dbitem.Count);
                ib.CopyFrom(dbitem.Exdata);
                _this.Equip.Add(bagId*10, ib);
                _this.mLogicData.EquipsModel.Add(bagId, tbRobot.WingID*100 + tbRobot.EquipLevel);
            }
            _this.Attr.EquipRefresh();
            //InitSkill();
            _this.NormalSkillId = _this.TableCharacter.InitSkill[0];
            _this.Skill.AddSkill(_this.TableCharacter.InitSkill[0], 1, eAddskillType.InitByRobot);
            foreach (var skill in tbRobot.Skill)
            {
                _this.Skill.AddSkill(skill, 1, eAddskillType.InitByRobot2);
            }
            //InitBuff();
            //InitAttr();
            _this.Attr.mBookData[1] = tbRobot.Power;
            _this.Attr.mBookData[2] = tbRobot.Agility;
            _this.Attr.mBookData[3] = tbRobot.Intelligence;
            _this.Attr.mBookData[4] = tbRobot.physical;
            _this.Attr.mBookData[(int) eAttributeType.PhyPowerMin] = tbRobot.AttackMin;
            _this.Attr.mBookData[(int) eAttributeType.PhyPowerMax] = tbRobot.AttackMax;
            _this.Attr.mBookData[(int) eAttributeType.MagPowerMin] = tbRobot.AttackMin;
            _this.Attr.mBookData[(int) eAttributeType.MagPowerMax] = tbRobot.AttackMax;
            _this.Attr.mBookData[(int) eAttributeType.PhyArmor] = tbRobot.PhysicsDefense;
            _this.Attr.mBookData[(int) eAttributeType.MagArmor] = tbRobot.MagicDefense;
            _this.Attr.mBookData[(int) eAttributeType.MpMax] = tbRobot.MagicLimit;
            _this.Attr.mBookData[(int) eAttributeType.HpMax] = tbRobot.LifeLimit;
            _this.Attr.InitAttributesAll();
            //阵营
            _this.mCamp = 2;
            _this.TableCamp = Table.GetCamp(_this.mCamp);
            _this.InitAI(tbRobot.Level);
        }

        /// <summary>
        ///     初始化表格数据，基类的Init会调用，逼不得已不要手动调
        /// </summary>
        public int InitTableData(AutoPlayer _this, int level)
        {
            _this.TableCharacter = Table.GetCharacterBase(_this.TypeId);
            return level;
        }

        public bool InitEquip(AutoPlayer _this, int level)
        {
            return true;
        }

        public bool InitSkill(AutoPlayer _this, int level)
        {
            _this.NormalSkillId = _this.TableCharacter.InitSkill[0];
            _this.Skill.AddSkill(_this.TableCharacter.InitSkill[0], 1, eAddskillType.AutoPlayer);
            foreach (var skill in _this.mLogicData.Skills)
            {
                _this.Skill.AddSkill(skill.Key, skill.Value, eAddskillType.AutoPlayer2);
            }
            return true;
        }

        public bool InitBuff(AutoPlayer _this, int level)
        {
            return true;
        }

        public bool InitAttr(AutoPlayer _this, int level)
        {
            var index = 0;
            foreach (var i in _this.mSceneData.AttrList)
            {
                if (index == 0)
                {
                    index++;
                    continue;
                }
                _this.Attr.mBookData[index] = i;
                index++;
            }
            _this.Attr.InitAttributesAll();
            return true;
        }

        #endregion
    }

    public class AutoPlayer : ObjNPC
    {
        private static IAutoPlayer mImpl;
        public const float MIN_AI_TICK_SECOND_AUTOPLAYER = 0.2f;
        public static NpcBaseRecord mTableNpc = Table.GetNpcBase(14);

        static AutoPlayer()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (AutoPlayer), typeof (AutoPlayerDefaultImpl),
                o => { mImpl = (IAutoPlayer) o; });
        }

        //构造函数
        public AutoPlayer()
        {
            mAiTickSeconds = MIN_AI_TICK_SECOND_AUTOPLAYER;
        }

        public LogicSimpleData mLogicData;
        public SceneSimpleData mSceneData;

        public override NpcBaseRecord TableNpc
        {
            get
            {
                //Logger.Warn("");
                return mTableNpc;
            }
            set { }
        }

        /// <summary>
        ///     输出成一个objdata用于客户端创建
        /// </summary>
        /// <returns></returns>
        public override ObjData DumpObjData(ReasonType reason)
        {
            return mImpl.DumpObjData(this, reason);
        }

        /// <summary>
        ///     获得等级
        /// </summary>
        /// <returns></returns>
        public override int GetLevel()
        {
            return mImpl.GetLevel(this);
        }

        //Obj类型
        public override ObjType GetObjType()
        {
            return ObjType.AUTOPLAYER;
        }

        /// <summary>
        ///     死亡时
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="delayView"></param>
        /// <param name="damage"></param>
        public override void OnDie(ulong characterId, int delayView, int damage = 0)
        {
            mImpl.OnDie(this, characterId, delayView, damage);
        }

        public override void OnEnterScene()
        {
            mImpl.OnEnterScene(this);
        }

        #region 初始化

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="logic"></param>
        /// <param name="scene"></param>
        public void Init(ulong npcId, LogicSimpleData logic, SceneSimpleData scene)
        {
            mImpl.Init(this, npcId, logic, scene);
        }

        public override void InitAI(int level)
        {
            mImpl.InitAI(this);
        }

        public void InitByRobot(ulong npcId, int RobotId)
        {
            mImpl.InitByRobot(this, npcId, RobotId);
        }

        /// <summary>
        ///     初始化表格数据，基类的Init会调用，逼不得已不要手动调
        /// </summary>
        public override int InitTableData(int level)
        {
            return mImpl.InitTableData(this, level);
        }

        public override bool InitEquip(int level)
        {
            return mImpl.InitEquip(this, level);
        }

        public override bool InitSkill(int level)
        {
            return mImpl.InitSkill(this, level);
        }

        public override bool InitBuff(int level)
        {
            return mImpl.InitBuff(this, level);
        }

        public override bool InitAttr(int level)
        {
            return mImpl.InitAttr(this, level);
        }

        #endregion
    }
}