#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DataContract;
using DataTable;
using MathNet.Numerics.LinearAlgebra.Complex;
using Scorpion;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public class JewellWars : DungeonScene
    {
        #region 刷新表格

        class JPlayer
        {
            public ulong objId { get; set; }
            public string name { get; set; }
            public int job { get; set; }

            public int exp { get; set; }
            public int lv { get; set; }
            public Dictionary<int, int> buf = new Dictionary<int, int>();
            public List<int> randBuffList = new List<int>();
            public int point { get; set; }
            public int fight { get; set; }
            public int kill { get; set; }
            public int die { get; set; }

            public ulong enemyId { get; set; }
            public JPlayer()
            {
                lv = 1;
            }
        }
        private List<JPlayer> _playerList = new List<JPlayer>();
        private Vector2 _Center = new Vector2(0f, 0f);
        private float _Radius = 100f;//球的直径
        private float correctRadius = 3;//矫正偏差值
        private List<Vector2> BufPosList = new List<Vector2>();    //Buff随机点
        private List<Vector2> BornPosList = new List<Vector2>();   //出生随机点
        private List<Vector2> BossPosList = new List<Vector2>();   //Boss刷新点 
        static JewellWars()
        {

        }

        #endregion

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region 数据
        //随机产生buff的计时器
        private Trigger _triggerBuff = null;
        //随机产生buff的计时器
        private Trigger _triggerChicken = null;
        //缩圈计时器
        private Trigger _triggerRadius = null;
        //缩圈伤害计时器
        private Trigger _triggerHurt = null;
        //广播排行榜计时器
        private Trigger _triggerBroadCast = null;

        private Trigger _triggerBroadSSRank = null;

        private DateTime startTime = DateTime.Now;
        private int randomNum = 4;
        private List<CheckenRecord> checkenTableList = new List<CheckenRecord>();

        //生成的buff列表 
        private List<ObjDropItem> dropBuffList = new List<ObjDropItem>();
        //生成的宝石列表
        private List<ObjDropItem> dropChickenList = new List<ObjDropItem>();

        private int MaxBuff = 20;
        private int Maxchicken = 50;
        private int bigCheckenDamage = 0;
        private int CircleReduceTime = 30;//毒圈缩小间隔时间
        private float circleMin = 20;//毒圈最小值
        private int RefreshChickenTime = 60;//刷新道具时间间隔
        private int RefreshBuffTime = 60;//刷新buff时间间隔
        private float circleReduce = 2;//毒圈每次缩小的值
        private HashSet<int> hurtRate = new HashSet<int>();
        private bool bosIsDie = true;
        private ObjNPC npcBoss;
        private ulong kingId = 0;

        private int Rate = 5;//掉血百分比

        private int DieRate = 5;//死亡掉落积分百分比

        private string[] BuffGroup = new string[] { "29001", "29002", "29003", "29004", "29005", "29006", "29007" };

        private string[] BigRefrshTime = new string[] { "60", "180",  "300" };//刷新时间
        private int leftTime = 30;//倒计时
        private int refreshIndex = 0;//刷新下标
        //private readonly int _fubenId = 30000;
        #endregion

        #region 重写父类方法
        public override void OnCreate()
        {
            base.OnCreate();
            InitPos();
            InitConfig();
      
            checkenTableList.Add(Table.GetChecken(2));
            checkenTableList.Add(Table.GetChecken(1));

            var configRecord = Table.GetServerConfig(1217);
            if (configRecord != null)
                randomNum = int.Parse(configRecord.Value);

            var tbFuben = Table.GetFuben(TableSceneData.FubenId);
            //MaxBuff | Maxchicken | 

            //tbFuben.lParam1;

            tbFuben.OpenLastMinutes = 0;
            var firstMobTime = tbFuben.OpenLastMinutes * 60;
            var startTime = DateTime.Now.AddSeconds(firstMobTime);
            StartTimer(eDungeonTimerType.WaitStart, startTime, OnStart);
            tbFuben.TimeLimitMinutes = 5;
            StartTimer(eDungeonTimerType.WaitEnd, startTime.AddSeconds(tbFuben.TimeLimitMinutes * 60), OnGameOver);
        }

        private void OnStart()
        {
            DateTime overTime = DateTime.Now.AddMinutes(1);
            _triggerBuff = CreateTimer(DateTime.Now, CreateBuff, RefreshBuffTime * 1000);
            _triggerChicken = CreateTimer(DateTime.Now, CreateChicken, RefreshChickenTime * 1000);
            _triggerHurt = CreateTimer(DateTime.Now, CheckRange, 3000);
            _triggerRadius = CreateTimer(DateTime.Now, UpdateRadius, CircleReduceTime*1000);
            _triggerBroadCast = CreateTimer(DateTime.Now, BroadCastRank, 1000);
            _triggerBroadSSRank = CreateTimer(DateTime.Now, StartSyncBatteryGuid, 10000);
            //UpdateRadius();
            leftTime = int.Parse(BigRefrshTime[refreshIndex]);
            CreateNpcLogic();
            base.TimeOverStart();
        }

        private void InitConfig()
        {
            var temp = Table.GetServerConfig(1500);
            if (temp != null)
                MaxBuff = int.Parse(temp.Value);
            temp = Table.GetServerConfig(1501);
            if (temp != null)
                Maxchicken = int.Parse(temp.Value);
            temp = Table.GetServerConfig(1502);
            if (temp != null)
                circleReduce = int.Parse(temp.Value);
            temp = Table.GetServerConfig(1503);
            if (temp != null)
                CircleReduceTime = int.Parse(temp.Value);
            temp = Table.GetServerConfig(1504);
            if (temp != null)
                _Radius = int.Parse(temp.Value);
            temp = Table.GetServerConfig(1505);
            if (temp != null)
                circleMin = int.Parse(temp.Value);
        
            temp = Table.GetServerConfig(1506);
            if (temp != null)
                RefreshChickenTime = int.Parse(temp.Value);
            temp = Table.GetServerConfig(1507);
            if (temp != null)
                RefreshBuffTime = int.Parse(temp.Value);
            
            temp = Table.GetServerConfig(1508);
            if (temp != null)
                Rate = int.Parse(temp.Value);
            
            temp = Table.GetServerConfig(1509);
            if (temp != null)
                BigRefrshTime = temp.Value.Split('|');
            temp = Table.GetServerConfig(1510);
            if (temp != null)
                BuffGroup = temp.Value.Split('|');

            temp = Table.GetServerConfig(1511);
            if (temp != null)
                DieRate = int.Parse(temp.Value);
            
        }

        private void InitPos()
        {
            BufPosList.Clear();
            {//150002 
                var tb = Table.GetSkillUpgrading(150002);
                if (tb != null)
                {
                    for (int i = 0; i < tb.Values.Count; i++)
                    {
                        var tbPos = Table.GetRandomCoordinate(tb.Values[i]);
                        if (tbPos != null)
                        {
                            BufPosList.Add(new Vector2(tbPos.PosX, tbPos.PosY));
                        }
                    }
                }
            }
            {//150000
                List<Vector2> temp = new List<Vector2>();

                var tb = Table.GetSkillUpgrading(150000);
                if (tb != null)
                {
                    for (int i = 0; i < tb.Values.Count; i++)
                    {
                        var tbPos = Table.GetRandomCoordinate(tb.Values[i]);
                        if (tbPos != null)
                        {
                            temp.Add(new Vector2(tbPos.PosX, tbPos.PosY));
                        }
                    }
                }
                var idx = MyRandom.Random(0, temp.Count - 1);
                if (idx < temp.Count)
                {
                    _Center = temp[idx];
                }

            }
            BornPosList.Clear();
            {//150001
                var tb = Table.GetSkillUpgrading(150001);
                if (tb != null)
                {
                    for (int i = 0; i < tb.Values.Count; i++)
                    {
                        var tbPos = Table.GetRandomCoordinate(tb.Values[i]);
                        if (tbPos != null)
                        {
                            BornPosList.Add(new Vector2(tbPos.PosX, tbPos.PosY));
                        }
                    }
                }
            }
            BossPosList.Clear();
            {//150003
                var tb = Table.GetSkillUpgrading(150003);
                if (tb != null)
                {
                    for (int i = 0; i < tb.Values.Count; i++)
                    {
                        var tbPos = Table.GetRandomCoordinate(tb.Values[i]);
                        if (tbPos != null)
                        {
                            BossPosList.Add(new Vector2(tbPos.PosX, tbPos.PosY));
                        }
                    }
                }
            }
        }
        public override void ExitDungeon(ObjPlayer player)
        {
            if (player == null) return;

            player.ExitDungeon();
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
            if (player == null) return;
            var killer = FindCharacter(characterId);
            if (killer == null)
            {
                return;
            }

            int idkill = _playerList.FindIndex(r => r.objId == characterId);
            _playerList[idkill].kill += 1;
            if (_playerList[idkill].enemyId == player.ObjId)//杀死了自己的仇人
            {

                var content1 = _playerList[idkill].name + "手刃了仇人" + player.mName;
                SceneServer.Instance.ChatAgent.BroadcastWorldMessage((uint)this.ServerId,
                    (int)eChatChannel.Scene, 0,
                    string.Empty, new ChatMessageContent { Content = content1 });
            }

            int idx = _playerList.FindIndex(r => r.objId == player.ObjId);
            _playerList[idx].die += 1;
            _playerList[idx].point = _playerList[idx].point * DieRate/100;
            //加入敌人列表
            _playerList[idx].enemyId = characterId;

            //死亡掉落一半积分
            DropPoint(_playerList[idx].point, player);

            //通知场景玩家被杀死了
            var killerObj = killer.GetRewardOwner();
            if (killerObj.GetObjType() != ObjType.PLAYER)
            {
                return;
            }
            SceneServer.Instance.ServerControl.BroadcastSceneChat(EnumAllPlayerId(), killer.GetName() + "|" + player.GetName(), 272000);

            base.OnPlayerDie(player, characterId);
        }

        void DropPoint(int point, ObjPlayer player)
        {
            for (int i = 0; i < checkenTableList.Count; i++)
            {
                var itemChecken = checkenTableList[i];
                if (point >= itemChecken.ExData1)
                {
                    var count = (int)Math.Floor((float)point / (float)itemChecken.ExData1);
                    point = point % itemChecken.ExData1;
                    if (point > 0 && i + 1 == checkenTableList.Count)
                        count += 1;
                    Vector2 v2 = new Vector2(player.GetPosition().X + MyRandom.Random(-randomNum, randomNum), player.GetPosition().Y + MyRandom.Random(-randomNum, randomNum));
                    CreateDropItem(4, new List<ulong>(), 0, itemChecken.ItemID[2], count, v2);
                }
                else
                {
                    if (i + 1 == checkenTableList.Count) //last
                    {
                        Vector2 v2 = new Vector2(player.GetPosition().X + MyRandom.Random(-randomNum, randomNum), player.GetPosition().Y + MyRandom.Random(-randomNum, randomNum));
                        CreateDropItem(4, new List<ulong>(), 0, itemChecken.ItemID[2], 1, player.GetPosition());
                    }
                }
            }
        }

        public override void OnNpcDamage(ObjNPC npc, int damage, ObjBase enemy)
        {
            base.OnNpcDamage(npc, damage, enemy);
            if (npc.GetObjType() == ObjType.NPC)
            {
                DamageAddChicken(npc);
            }
        }

        private void DamageAddChicken(ObjNPC npc)
        {
            var leftHp = npc.Attr.GetDataValue(eAttributeType.HpNow);
            var sumHp = npc.Attr.GetDataValue(eAttributeType.HpMax);
            var curIndex = (int)Math.Floor((float)(sumHp - leftHp) * 10 / sumHp);
            bool isNewHurt = hurtRate.Add(curIndex);
            if (isNewHurt && curIndex - bigCheckenDamage > 0)
            {
                int D_value = curIndex - bigCheckenDamage;
                //checken
                bigCheckenDamage = curIndex;
                if (curIndex == 10) //buff
                {
                    var v2 = new Vector2(npc.GetPosition().X + MyRandom.Random(-randomNum, randomNum),
                        npc.GetPosition().Y + MyRandom.Random(-randomNum, randomNum));
                    int itemId = int.Parse(BuffGroup[MyRandom.Random(0, BuffGroup.Length)]);
                    DropItem(itemId, v2);
                }
                for (int i = 0; i < D_value; i++)
                {
                    var vt = new Vector2(npc.GetPosition().X + MyRandom.Random(-randomNum, randomNum), npc.GetPosition().Y + MyRandom.Random(-randomNum, randomNum));
                    CreateDropItem(4, new List<ulong>(), 0, 29001, 1, vt);
                }
            }
            if (leftHp <= 0)
            {
                bigCheckenDamage = 0;
                hurtRate.Clear();
            }
        }
        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            base.OnNpcDie(npc, characterId);
            if (npc == null) return;
            bosIsDie = true;
            var killer = FindCharacter(characterId);
            if (killer == null)
            {
                return;
            }

        }
        public override void OnPlayerEnter(ObjPlayer player)
        {
            player.PkModel = (int)ePkModel.GoodEvil;
            base.OnPlayerEnter(player);

            if (State >= eDungeonState.Start)
            {
                //    mDropPlayers.Remove(player.ObjId);
            }
        }

        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            if (player == null) return;
            base.OnPlayerEnterOver(player);
            int idx = _playerList.FindIndex(r => r.objId == player.ObjId);
            if (idx >= 0)
                _playerList.RemoveAt(idx);

            JPlayer p = new JPlayer();
            p.objId = player.ObjId;
            p.name = player.GetName();
            p.lv = 1;
            _playerList.Add(p);
            var tb = Table.GetCheckenLv(p.lv);
            if (tb != null)
            {//buff
                player.AddBuff(tb.BaseBuff, 1, player);
            }
            SendRadius(player);
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            if (player == null) return;

            base.OnPlayerLeave(player);
            int idx = _playerList.FindIndex(r => r.objId == player.ObjId);
            if (idx >= 0)
            {
                _playerList.RemoveAt(idx);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void CleanUp()
        {
            if (_triggerBuff != null)
            {
                DeleteTimer(_triggerBuff);
                _triggerBuff = null;
            }
            if (_triggerRadius != null)
            {
                DeleteTimer(_triggerRadius);
                _triggerRadius = null;
            }
            if (_triggerHurt != null)
            {
                DeleteTimer(_triggerHurt);
                _triggerHurt = null;
            }
            if (_triggerBroadCast != null)
            {
                DeleteTimer(_triggerBroadCast);
                _triggerBroadCast = null;
            }
            if (_triggerBroadSSRank != null)
            {
                DeleteTimer(_triggerBroadSSRank);
                _triggerBroadSSRank = null;
            }

            checkenTableList.Clear();
            dropBuffList.Clear();
            dropChickenList.Clear();
            hurtRate.Clear();
            kingId = 0;
            bosIsDie = true;
        }

        public override void OnPlayerExDataChanged(ObjPlayer obj, int idx, int val)
        {
            if (obj == null) return;
            base.OnPlayerExDataChanged(obj, idx, val);
        }

        #endregion

        #region 内部逻辑

        public void CreateChicken()
        {
            //销毁null
            for (int i = 0; i < dropChickenList.Count; i++)
            {
                if (dropChickenList[i] == null)
                    dropChickenList.RemoveAt(i);
            }
            for (int i = 0; i < Maxchicken - dropChickenList.Count; i++)
            {
                float _x = 0;
                float _y = 0;
                do
                {
                    _x = MyRandom.Random(1, mSceneHeight);
                    _y = MyRandom.Random(1, mSceneWidth);
                }
                while (GetObstacleValue(_x, _y) == SceneObstacle.ObstacleValue.Walkable);
                var item = DropItem(29000, new Vector2(_x, _y));
                if (item != null)
                    dropChickenList.Add(item);
            }
        }
        public void CreateBuff()
        {
            List<Vector2> tempList = new List<Vector2>();
            //销毁null
            for (int i = 0; i < dropBuffList.Count; i++)
            {
                if (dropBuffList[i] == null)
                    dropBuffList.RemoveAt(i);
            }
            //未随机点取到
            foreach (var tempV in BufPosList)
            {
                int idx = dropBuffList.FindIndex(dropItem => dropItem.GetPosition().X == tempV.X && dropItem.GetPosition().Y == tempV.Y);
                if (idx == -1)
                    tempList.Add(tempV);
            }
            //从没有buff的点 随机剩下的buff生成点
            for (int i = 0; i < MaxBuff - dropBuffList.Count; i++)
            {
                var tempV = tempList[MyRandom.Random(0, tempList.Count)];
                tempList.Remove(tempV);
                int itemId = int.Parse(BuffGroup[MyRandom.Random(0, BuffGroup.Length)]);
                var item = DropItem(itemId, tempV);
                if (item != null)
                    dropBuffList.Add(item);
            }

        }
        public void CheckRange()
        {
            var safe_r = ((_Radius + correctRadius) / 2) * ((_Radius + correctRadius) / 2);
            foreach (var player in EnumAllPlayer())
            {
                var dis = (player.GetPosition() - _Center).LengthSquared();
                if (dis > safe_r)
                {
                    player.AddBuff(100001, 1, player);
                    player.Proxy.CK_NotifyHurt(0);
                }
                //_Radius
                //_Center
            }
        }

        public void UpdateRadius()
        {
            _Radius -= circleReduce;
            MsgCheckenSceneInfo info = new MsgCheckenSceneInfo();
            var Pos = new Vector2Int32
            {
                x = Utility.MultiplyPrecision(_Center.X),
                y = Utility.MultiplyPrecision(_Center.Y)
            };
            info.CenterPos = Pos;
            info.Radius = _Radius;
            info.Timer = 10;
            foreach (var player in EnumAllPlayer())
            {
                if (player.mActive == true)
                    player.Proxy.CK_NotifyCheckenSceneInfo(info);
            }

            if (_Radius <= circleMin)
            {
                DeleteTimer(_triggerRadius);
            }
        }

        public void SendRadius(ObjPlayer player)
        {
            MsgCheckenSceneInfo info = new MsgCheckenSceneInfo();
            var Pos = new Vector2Int32
            {
                x = Utility.MultiplyPrecision(_Center.X),
                y = Utility.MultiplyPrecision(_Center.Y)
            };
            info.CenterPos = Pos;
            info.Radius = _Radius + correctRadius;
            info.Timer = (startTime - DateTime.Now).Seconds;
            player.Proxy.CK_NotifyCheckenSceneInfo(info);
        }

        private ObjDropItem DropItem(int id, Vector2 pos)
        {
            return CreateDropItem(4, new List<ulong>(), 0, id, 1, pos);
        }
        public override void OnPlayerPickItem(ObjPlayer player, ObjDropItem item)
        {
            if (player.mIsDead)
                return;
            if (item.TableDrop.Type != 300)
                return;
            var tbChecken = Table.GetChecken(item.TableDrop.Exdata[0]);
            if (tbChecken == null)
                return;
            var p = _playerList.Find(r => r.objId == player.ObjId);
            if (p == null)
                return;

            //播放特效
            player.Proxy.NotifyPlayEffect(700);

            //添加经验
            OnPlayerGetExp(player, p, tbChecken.ExData1);
            int idx = _playerList.FindIndex(r => r.objId == player.ObjId);

            //添加吃鸡的积分
            if (_playerList[idx] != null)
                _playerList[idx].point += tbChecken.ExData2;

            //添加Buff
            player.AddBuff(tbChecken.BuffId, 1, player);
            //添加道具
            Dict_int_int_Data data = new Dict_int_int_Data();
            for (int i = 0; i < tbChecken.ItemID.Length && i < tbChecken.Num.Length; i++)
            {
                if (tbChecken.ItemID[i] > 0 && tbChecken.Num[i] > 0)
                {
                    data.Data.Add(tbChecken.ItemID[i], tbChecken.Num[i]);
                }
            }
            for (int i = 0; i < dropBuffList.Count; i++)
            {
                if (item.ObjId == dropBuffList[i].ObjId)
                {
                    dropBuffList.RemoveAt(i);
                    break;
                }
            }
            for (int i = 0; i < dropChickenList.Count; i++)
            {
                if (item.ObjId == dropChickenList[i].ObjId)
                {
                    dropChickenList.RemoveAt(i);
                    break;
                }
            }

            CoroutineFactory.NewCoroutine(GiveItemList, player, data).MoveNext();
        }

        public int OnPlayerSkillUp(ObjPlayer player, int BuffId)
        {
            var p = _playerList.Find(r => r.objId == player.ObjId);
            if (p == null)
                return (int)ErrorCodes.Error_CharacterId_Not_Exist;
            if (p.point <= 0)
                return (int)ErrorCodes.Error_Checken_Skill_Point;
            if (p.randBuffList.FindIndex(r => r == BuffId) < 0)
                return (int)ErrorCodes.Error_Checken_Skill_Id;
            p.point--;
            p.buf.modifyValue(BuffId, 1);

            return 0;
        }

        private void OnPlayerGetExp(ObjPlayer player, JPlayer jp, int exp)
        {
            var tb = Table.GetCheckenLv(jp.lv);
            if (tb == null)
                return;
            jp.exp += exp;
            Dict_int_int_Data data = new Dict_int_int_Data();
            if (tb.NeedExp <= jp.exp)
            {
                var tbnew = Table.GetCheckenLv(jp.lv + 1);
                if (tbnew == null)//达到最大等级
                {
                    jp.exp = tb.NeedExp;
                }
                else
                {
                    //删除旧的buff
                    var tbold = Table.GetCheckenLv(jp.lv);
                    if (tbold != null)
                    {//buff
                        player.DeleteBuff(tbold.BaseBuff, eCleanBuffType.Clear);
                    }

                    jp.exp -= tb.NeedExp;
                    jp.lv++;

                    player.AddBuff(tbnew.BaseBuff, 1, player);

                    var ids = RandBuffList(tb.BuffGroup, 3);
                    foreach (var id in ids)
                    {
                        int lv = 0;
                        jp.buf.TryGetValue(id, out lv);
                        data.Data.Add(id, lv + 1);
                    }
                }
            }
            player.Proxy.CK_NotifyClientLevelup(player.mObjId, jp.lv, jp.exp, data);
        }

        public List<int> RandBuffList(int buffGroupId, int num)
        {
            List<int> ret = new List<int>();
            var tbBuffGroup = Table.GetBuffGroup(buffGroupId);
            if (tbBuffGroup == null)
                return ret;
            if (tbBuffGroup.BuffID.Count != tbBuffGroup.QuanZhong.Count || tbBuffGroup.BuffID.Count < num)
            {
                Logger.Error("RandBuffList  {0} addbuffgroup size not eaqual!", buffGroupId);
                return ret;
            }
            var buffList = new List<int>();
            var propList = new List<int>();
            var maxProp = 0;
            for (int i = 0; i < tbBuffGroup.BuffID.Count; i++)
            {
                maxProp += tbBuffGroup.QuanZhong[i];
                buffList.Add(tbBuffGroup.BuffID[i]);
                propList.Add(tbBuffGroup.QuanZhong[i]);
            }
            for (int _i = 0; _i < num; _i++)
            {
                var rand = MyRandom.Random(0, maxProp - 1);
                var prop = 0;
                for (int i = 0; i < buffList.Count && i < propList.Count; i++)
                {
                    prop += propList[i];
                    if (prop >= rand)
                    {
                        ret.Add(buffList[i]);
                        buffList.RemoveAt(i);
                        propList.RemoveAt(i);
                        break;
                    }
                }
            }
            return ret;
        }

        private IEnumerator GiveItemList(Coroutine coroutine, ObjPlayer character, Dict_int_int_Data item)
        {
            var result = SceneServer.Instance.LogicAgent.GiveItemList(character.ObjId, item, (int)eCreateItemType.CheckenPick);
            yield return result.SendAndWaitUntilDone(coroutine);
        }

        private void BroadCastRank()
        {
            if (leftTime > 0)
            {
                leftTime--;
            }
            if (leftTime <= 0)
            {
                refreshIndex++;
                if (refreshIndex > BigRefrshTime.Length - 1)
                    refreshIndex = BigRefrshTime.Length - 1;
                leftTime = int.Parse(BigRefrshTime[refreshIndex]);
                CreateNpcLogic();
            }
            if (_playerList.Count == 0)
                return;
            _playerList.Sort((a, b) => { return a.point - b.point; });
            if (_playerList.Count > 0 && kingId != _playerList[0].objId)//成为鸡王
            {
                kingId = _playerList[0].objId;
                SceneServer.Instance.ServerControl.BroadcastSceneChat(EnumAllPlayerId(), _playerList[0].name, 272000);
            }
            MsgCheckenRankList msg = new MsgCheckenRankList();
            for (int i = _playerList.Count - 1; i >= 0; i--)
            {
                MsgCheckenRankInfo info = new MsgCheckenRankInfo();
                info.Guid = _playerList[i].objId;
                info.Name = _playerList[i].name;
                info.Score = _playerList[i].point;
                info.Kill = _playerList[i].kill;
                info.Die = _playerList[i].die;
                info.EnemyGuid = _playerList[i].enemyId;

                var selfChar = FindPlayer(info.Guid);
                if (selfChar != null)
                {
                    info.PosX = selfChar.GetPosition().X;
                    info.PosZ = selfChar.GetPosition().Y;
                }
                msg.RankList.Add(info);

            }
            msg.bosDie = bosIsDie;
            msg.PosX = npcBoss.mPosition.X;
            msg.PosZ = npcBoss.mPosition.Y;
            msg.LeftTime = leftTime;
            foreach (var player in EnumAllPlayer())
            {
                player.Proxy.CK_NotifyRankList(msg);
            }
        }

        private void StartSyncBatteryGuid()
        {
            CoroutineFactory.NewCoroutine(SendSyncBatteryGuidMsg).MoveNext();
        }

        private IEnumerator SendSyncBatteryGuidMsg(Coroutine co)
        {
            ChickenRankData data = new ChickenRankData();
            for (int i = 0; i < _playerList.Count; i++)
            {
                ChickenData dataItem = new ChickenData();
                var player = FindPlayer(_playerList[i].objId);
                dataItem.Score = _playerList[i].point;
                dataItem.FightValue = player.Attr.GetFightPoint();
                dataItem.Guid = _playerList[i].objId;
                dataItem.Level = player.GetLevel();
                dataItem.Name = _playerList[i].name;
                dataItem.Profession = player.TypeId;
                dataItem.Rank = i + 1;
                data.RankList.Add(dataItem);
            }
            var notifyMsg = SceneServer.Instance.ActivityAgent.SSSyncChickenScore(0, data);
            yield return notifyMsg.SendAndWaitUntilDone(co);

        }

        private void CreateNpcLogic()
        {
            if (bosIsDie == true)
            {
                var nextTime = DateTime.Now.AddSeconds(30);
                npcBoss = CreateNpc(null, 59500, _Center, Vector2.UnitX);
                bosIsDie = false;
            }
        }

        private void OnGameOver()
        {
            CleanUp();
            BroadCastRank();
            _playerList.Sort((a, b) => { return a.point - b.point; });
            foreach (var player in EnumAllPlayer())
            {
                var result = new FubenResult();
                result.CompleteType = 0;
                int idx = _playerList.FindIndex(r => r.objId == player.ObjId);
                result.Args.Add(idx + 1);
                Complete(player.ObjId, result);
            }
            base.TimeOverEnd();
        }
        #endregion
    }
}
