#region using

using System.Collections;
using System.Collections.Generic;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public interface IDrop
    {
        void DropItem(Dictionary<int, int> Droplist, int nId, int nCount);
        void DropMother(int nId, Dictionary<int, int> Droplist);
        void DropSon(int nId, Dictionary<int, int> Droplist);
        void MonsterKill(ObjNPC npc, ulong killer);
        IEnumerator MonsterKillMessageToLogic(Coroutine coroutine, ulong id, int monsterId, int exp, int sceneId);
        void TryDelAttackEquipDurable(ObjPlayer player);
    }

    public class DropDefaultImpl : IDrop
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //通知Logic有杀怪事件，并获得经验
        public IEnumerator MonsterKillMessageToLogic(Coroutine coroutine, ulong id, int monsterId, int exp, int sceneId)
        {
            var result = SceneServer.Instance.LogicAgent.LogicKillMonster(id, monsterId, exp, sceneId);
            yield return result.SendAndWaitUntilDone(coroutine);
        }

        public void TryDelAttackEquipDurable(ObjPlayer player)
        {
            if (MyRandom.Random(10000) >= Drop.tbAckEquip)
            {
                return;
            }
            var defEquips = new Dictionary<int, ItemEquip2>();
            foreach (var itemEquip2 in player.Equip)
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
                if (Table.GetEquip(equip.GetId()).DurableType != 1)
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
            var durable = rrr.Value.GetExdata(22) - 1;
            rrr.Value.SetDurable(durable);
            durableList.Add(rrr.Key, -1);
            if (durableList.Count > 0)
            {
                player.EquipDurableDown(durableList, durable <= 0);
            }
        }

        //触发掉落
        public void MonsterKill(ObjNPC npc, ulong killer)
        {
            var tbNpc = Table.GetNpcBase(npc.TypeId);
            if (tbNpc == null)
            {
                Logger.Error("MonsterKill Type={0}", npc.TypeId);
                return;
            }
            var scene = npc.Scene;
            var GiveExp = new Dictionary<ObjCharacter, int>();
            var playerList = npc.GetExpList(GiveExp);
            if (playerList == null)
            {
                return;
            }
            if (playerList.Count < 1)
            {
                return;
            }
            //分经验
            foreach (var i in GiveExp)
            {
                var player = i.Key as ObjPlayer;
                if (player != null)
                {
                    var giveExp = 0;
                    if (tbNpc.IsDynamicExp == 1)
                    {
                        var refExp = i.Key.Attr._ExpRef;
                        if (tbNpc.ExpMultiple > 2)
                        {
                            refExp = (tbNpc.ExpMultiple + i.Key.Attr._ExpRef - 10000);
                        }
                        giveExp =
                            (int)
                                (tbNpc.DynamicExpRatio/10000.0f*Table.GetLevelData(player.GetLevel()).DynamicExp*refExp/
                                 10000);
                    }
                    else
                    {
                        if (tbNpc.ExpMultiple < 2)
                        {
                            giveExp = i.Value*i.Key.Attr._ExpRef/10000;
                        }
                        else
                        {
                            giveExp = (int) (1.0*i.Value*(tbNpc.ExpMultiple + i.Key.Attr._ExpRef - 10000)/10000);
                        }
                    }

                    var addCount = AutoActivityManager.GetActivity(1020);
                    if (addCount > 1)
                    {
                        if (tbNpc.Id >= 210000 && tbNpc.Id <= 213104)
                        {
                            giveExp = giveExp*addCount;
                        }
                    }
                    giveExp = (int)(giveExp*player.GetAdditionExp());
                    float fExp = player.Attr.ExpAdd / 10000.0f;
                    giveExp = (int)(giveExp * fExp);                           
                    if(player.Scene != null)
                    {
                        player.Scene.OnPlayerPickUp(player.ObjId, (int)eResourcesType.ExpRes, giveExp);
                    }

                    CoroutineFactory.NewCoroutine(MonsterKillMessageToLogic, i.Key.ObjId, npc.TypeId, giveExp,
                        scene.TypeId).MoveNext();
                    //攻击装备耐久相关
                    player.TryDelAttackEquipDurable();
                }
            }

            var dropId = tbNpc.DropId;
            if (scene.SpecialDrop != -1 && tbNpc.Spare != -1)
            {
                var tbSU = Table.GetSkillUpgrading(tbNpc.Spare);
                dropId = tbSU.GetSkillUpgradingValue(scene.SpecialDrop);
            }
            if (dropId == -1)
            {
                return;
            }
            var Droplist = new Dictionary<int, int>();
            //特殊掉落
            {
                List<int> list;
                if (Drop.SpecialDropForNewCharacter.TryGetValue(tbNpc.Id, out list))
                {
                    var id = list[0];
                    var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(killer);
                    if (character != null)
                    {
                        var ex = character.mDbData.SpecialDrop;
                        if (!BitFlag.GetLow(ex, id))
                        {
                            Droplist.Clear();
                            var dropid = list[character.TypeId + 1];
                            DropMother(dropid, Droplist);
                            if (Droplist.Count > 0)
                            {
                                ex = BitFlag.LongSetFlag(ex, id);
                                character.mDbData.SpecialDrop = ex;
                                character.MarkDbDirty();
                            }
                            foreach (var i in Droplist)
                            {
                                scene.CreateDropItem(tbNpc.BelongType, new List<ulong> { character.ObjId }, 0, i.Key, i.Value,
                                    npc.GetPosition());
                            }
                            Droplist.Clear();
                        }
                    }
                }
            }

            //节日掉落
            {
                var dropIds = tbNpc.SpecialDrops;
                var yunyingIds = tbNpc.YunYingIds;
                for (int i = 0; i < yunyingIds.Count; i++)
                {
                    var tbOperationActivity = Table.GetOperationActivity(yunyingIds[i]);
                    if (null == tbOperationActivity)
                        continue;
                    if (!(System.DateTime.Now >= System.DateTime.Parse(tbOperationActivity.openTime) &&
                          System.DateTime.Now <= System.DateTime.Parse(tbOperationActivity.closeTime)))
                        continue;
                    if (i >= dropIds.Count)
                        continue;
                    Droplist.Clear();
                    DropMother(dropIds[i], Droplist);
                    var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(killer);
                    if (character != null)
                    {
                        foreach (var dropitem in Droplist)
                        {
                            scene.CreateDropItem(tbNpc.BelongType,
                                                 new List<ulong> { character.ObjId },
                                                 0,
                                                 dropitem.Key,
                                                 dropitem.Value,
                                                 npc.GetPosition());
                            //Logger.Warn("holidaydrop: [{0},{1}]", dropitem.Key, dropitem.Value);
                        }
                        Droplist.Clear();
                    }
                }
            }

            //分掉落物品
            switch (tbNpc.BelongType)
            {
                case 0: //队内自由拾取
                {
                    DropMother(dropId, Droplist);
                    var ownerList = new List<ulong>();
                    foreach (var character in playerList)
                    {
                        if (character != null && tbNpc.LimitFlag > 0)
                        {
                            if (character.GetObjType() == ObjType.PLAYER)
                            {
                                var p = character as ObjPlayer;
                                if (p != null)
                                {
                                    Dict_int_int_Data data = new Dict_int_int_Data();
                                    data.Data.Add(tbNpc.LimitFlag, 1);
                                    p.SendExDataChange(data);
                                }                                
                            }

                        }
                        ownerList.Add(character.ObjId);
                    }
                    foreach (var i in Droplist)
                    {
                        scene.CreateDropItem(tbNpc.BelongType, ownerList, playerList[0].GetTeamId(), i.Key, i.Value,
                            npc.GetPosition());
                    }
                }
                    break;
                case 1: //队内伤害拾取
                {
                    DropMother(dropId, Droplist);
                    var ownerList = new List<ulong>();
                    if (playerList[0].GetTeamId() == 0)
                    {
                        ownerList.Add(playerList[0].ObjId);
                        var p = playerList[0] as ObjPlayer;
                        if (p != null)
                        {
                            Dict_int_int_Data data = new Dict_int_int_Data();
                            data.Data.Add(tbNpc.LimitFlag, 1);
                            p.SendExDataChange(data);
                        }
                    }
                    else
                    {
                        var maxHatre = 0;
                        ObjCharacter maxCharacter = null;
                        foreach (var character in playerList)
                        {
                            var nh = npc.GetNowHatre(character);
                            if (nh > maxHatre)
                            {
                                maxHatre = nh;
                                maxCharacter = character;
                            }
                        }
                        //ObjCharacter maxCharacter = npc.GetMaxHatreByTeam(playerList[0].GetTeamId());
                        if (maxCharacter == null)
                        {
                            ownerList.Add(playerList[0].ObjId);
                            maxCharacter = playerList[0];
                        }
                        else
                        {
                            ownerList.Add(maxCharacter.ObjId);
                        }
                        if(maxCharacter.GetObjType() == ObjType.PLAYER)
                        {
                            var p = maxCharacter as ObjPlayer;
                            if (p != null)
                            {
                                Dict_int_int_Data data = new Dict_int_int_Data();
                                data.Data.Add(tbNpc.LimitFlag, 1);
                                p.SendExDataChange(data);
                            }                            
                        }

                    }
                    foreach (var i in Droplist)
                    {
                        scene.CreateDropItem(tbNpc.BelongType, ownerList, playerList[0].GetTeamId(), i.Key, i.Value,
                            npc.GetPosition());
                    }
                }
                    break;
                case 2: //队内分别拾取
                {
                    foreach (var character in playerList)
                    {
                        if (npc.Scene == null || character.Scene == null)
                            continue;
                        if (npc.Scene.Guid != character.Scene.Guid)
                            continue;
                        Droplist.Clear();
                        DropMother(dropId, Droplist);
                        foreach (var i in Droplist)
                        {
                            scene.CreateDropItem(tbNpc.BelongType, new List<ulong> {character.ObjId}, 0, i.Key, i.Value,
                                npc.GetPosition());
                        }

                        if (character.GetObjType() == ObjType.PLAYER)
                        {
                            var p = character as ObjPlayer;
                            if (p != null)
                            {
                                Dict_int_int_Data data = new Dict_int_int_Data();
                                data.Data.Add(tbNpc.LimitFlag, 1);
                                p.SendExDataChange(data);
                            }
                        }
                    }
                }
                    break;
                case 3: //所有人分别拾取
                {
                    var Hatres = npc.GetAllHatre();
                    foreach (var hatre in Hatres)
                    {
                        if (!(hatre.Key is ObjPlayer))
                        {
                            continue;
                        }
                        if (npc.Scene == null || hatre.Key.Scene == null)
                            continue;
                        if (npc.Scene.Guid != hatre.Key.Scene.Guid)
                            continue;

                        Droplist.Clear();
                        DropMother(dropId, Droplist);
                        foreach (var i in Droplist)
                        {
                            scene.CreateDropItem(tbNpc.BelongType, new List<ulong> {hatre.Key.ObjId}, 0, i.Key, i.Value,
                                npc.GetPosition());
                        }

                        if (hatre.Key.GetObjType() == ObjType.PLAYER)
                        {
                            var p = hatre.Key as ObjPlayer;
                            if (p != null)
                            {
                                Dict_int_int_Data data = new Dict_int_int_Data();
                                data.Data.Add(tbNpc.LimitFlag, 1);
                                p.SendExDataChange(data);
                            }
                        }
                    }
                }
                    break;
                case 4: //所有人自由拾取
                {
                    Droplist.Clear();
                    DropMother(dropId, Droplist);
                    var addCount = AutoActivityManager.GetActivity(1010);
                    if (addCount > 1)
                    {
                        if (npc.TableNpc.Id >= 65000 && npc.TableNpc.Id < 65005)
                        {
                            for (var i = 1; i < addCount; i++)
                            {
                                DropMother(dropId, Droplist);
                            }
                        }
                    }

                    foreach (var i in Droplist)
                    {
                        scene.CreateDropItem(tbNpc.BelongType, new List<ulong>(), 0, i.Key, i.Value, npc.GetPosition());
                    }
                }
                    break;
            }
        }

        //掉落母表
        public void DropMother(int nId, Dictionary<int, int> Droplist)
        {
            var tbdropmo = Table.GetDropMother(nId);
            if (tbdropmo == null)
            {
                return;
            }
            //惊喜掉落
            if (tbdropmo.SurprisePro > 0 && tbdropmo.SurpriseDrop >= 0)
            {
                if (MyRandom.Random(10000) < tbdropmo.SurprisePro)
                {
                    //走惊喜掉落
                    Logger.Info("DropMother Surprise={0}", nId);
                    DropSon(nId, Droplist);
                    return;
                }
            }
            //计数掉落
            //if (tbdropmo.ExdataId >= 0 && tbdropmo.ExdataCount > 0 && tbdropmo.ExdataDropId >= 0)
            //{
            //    character.AddExData(tbdropmo.ExdataId, 1);
            //    if (character.GetExData(tbdropmo.ExdataId) >= tbdropmo.ExdataCount)
            //    {
            //        //累积掉落
            //        Logger.Info("DropMother Exdata={0}", nId);
            //        character.SetExData(tbdropmo.ExdataId, 0);
            //        character.DropMother(tbdropmo.ExdataDropId, Droplist);
            //        return;
            //    }
            //}
            //正常掉落
            var Grouplist = new Dictionary<int, int>();
            for (var i = 0; i != tbdropmo.DropSon.Length; ++i)
            {
                var GroupId = tbdropmo.Group[i];
                if (GroupId == -1)
                {
//单独组
                    if (tbdropmo.Pro[i] > 0 && tbdropmo.DropSon[i] >= 0)
                    {
                        if (tbdropmo.DropMin[i] < 1 || tbdropmo.DropMax[i] < tbdropmo.DropMin[i])
                        {
                            Logger.Error("DropMother DropSon={0},Index={1},Min={2},Max={3}", nId, i, tbdropmo.DropMin[i],
                                tbdropmo.DropMax[i]);
                            continue;
                        }
                        var nCount = MyRandom.Random(tbdropmo.DropMin[i], tbdropmo.DropMax[i]);
                        Logger.Info("DropMother DropSon={0},DropSon={1},Count={2}", nId, tbdropmo.DropSon[i], nCount);
                        for (var j = 0; j != nCount; ++j)
                        {
                            DropSon(tbdropmo.DropSon[i], Droplist);
                        }
                    }
                }
                else
                {
                    int pro;
                    if (!Grouplist.TryGetValue(GroupId, out pro))
                    {
                        pro = MyRandom.Random(10000);
                        Grouplist[GroupId] = pro;
                    }
                    if (pro <= 0)
                    {
                        continue;
                    }
                    if (pro <= tbdropmo.Pro[i])
                    {
//随机中了
                        var nCount = tbdropmo.DropMin[i];
                        if (nCount < 0 || nCount > tbdropmo.DropMax[i])
                        {
                            Logger.Error("DropMother CountError DropId={0},Group={1},Index={2},DropSon={3}", nId,
                                GroupId, i, tbdropmo.DropSon[i]);
                            return;
                        }
                        if (tbdropmo.DropMin[i] < tbdropmo.DropMax[i])
                        {
                            nCount = MyRandom.Random(tbdropmo.DropMin[i], tbdropmo.DropMax[i]);
                        }
                        Logger.Info("DropMother DropId={0},Group={1},Index={2},DropSon={3},Count={4}", nId, GroupId, i,
                            tbdropmo.DropSon[i], nCount);
                        for (var j = 0; j != nCount; ++j)
                        {
                            DropSon(tbdropmo.DropSon[i], Droplist);
                        }
                    }
                    Grouplist[GroupId] = pro - tbdropmo.Pro[i];
                }
            }
        }

        //掉落子表
        public void DropSon(int nId, Dictionary<int, int> Droplist)
        {
            var tbdropson = Table.GetDropSon(nId);
            if (tbdropson == null)
            {
                return;
            }
            var nType = tbdropson.DropType;
            switch (nType)
            {
                case 0: //固定掉落
                {
                    for (var i = 0; i != tbdropson.Item.Length; ++i)
                    {
                        var itemid = tbdropson.Item[i];
                        var itemCount = tbdropson.Count[i];
                        var itempro = tbdropson.Pro[i];
                        if (itemCount < tbdropson.MaxCount[i])
                        {
                            itemCount = MyRandom.Random(itemCount, tbdropson.MaxCount[i]);
                        }
                        if (itemid >= 0 && itemCount > 0 && itempro > 0)
                        {
                            if (MyRandom.Random(10000) < itempro)
                            {
                                DropItem(Droplist, itemid, itemCount);
                            }
                        }
                    }
                }
                    break;
                case 1: //随机掉落
                {
                    if (MyRandom.Random(10000) > tbdropson.TotlePro)
                    {
                        return;
                    }
                    var totlePro = MyRandom.Random(10000);
                    for (var i = 0; i != tbdropson.Item.Length; ++i)
                    {
                        var itemid = tbdropson.Item[i];
                        var itemCount = tbdropson.Count[i];
                        var itempro = tbdropson.Pro[i];
                        if (itemCount < tbdropson.MaxCount[i])
                        {
                            itemCount = MyRandom.Random(itemCount, tbdropson.MaxCount[i]);
                        }
                        if (itemid >= 0 && itemCount > 0 && itempro > 0)
                        {
                            if (totlePro < itempro)
                            {
                                DropItem(Droplist, itemid, itemCount);
                                return;
                            }
                            totlePro -= itempro;
                        }
                    }
                }
                    break;
            }
        }

        //掉落物品
        public void DropItem(Dictionary<int, int> Droplist, int nId, int nCount)
        {
            //character.mBag.AddItem(nId, nCount, character);
            if (Droplist.ContainsKey(nId))
            {
                Droplist[nId] += nCount;
            }
            else
            {
                Droplist[nId] = nCount;
            }
        }
    }

    public static class Drop
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IDrop mImpl;
        //攻击装备耐久相关
        public static readonly int tbAckEquip = Table.GetServerConfig(361).ToInt();
        public static readonly Dictionary<int, List<int>> SpecialDropForNewCharacter = new Dictionary<int, List<int>>();

        static Drop()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (Drop), typeof (DropDefaultImpl),
                o => { mImpl = (IDrop) o; });            
        }

        //掉落物品
        public static void DropItem(Dictionary<int, int> Droplist, int nId, int nCount)
        {
            mImpl.DropItem(Droplist, nId, nCount);
        }

        //掉落母表
        public static void DropMother(int nId, Dictionary<int, int> Droplist)
        {
            mImpl.DropMother(nId, Droplist);
        }

        //掉落子表
        public static void DropSon(int nId, Dictionary<int, int> Droplist)
        {
            mImpl.DropSon(nId, Droplist);
        }

        //触发掉落
        public static void MonsterKill(ObjNPC npc, ulong killer)
        {
            mImpl.MonsterKill(npc, killer);
        }

        //通知Logic有杀怪事件，并获得经验
        public static IEnumerator MonsterKillMessageToLogic(Coroutine coroutine,
                                                            ulong id,
                                                            int monsterId,
                                                            int exp,
                                                            int sceneId)
        {
            return mImpl.MonsterKillMessageToLogic(coroutine, id, monsterId, exp, sceneId);
        }

        public static void TryDelAttackEquipDurable(this ObjPlayer player)
        {
            mImpl.TryDelAttackEquipDurable(player);
        }
    }
}