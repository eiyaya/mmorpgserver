#region using

using System.Collections.Generic;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IDrop
    {
        void DropItem(CharacterController character, Dictionary<int, int> Droplist, int nId, int nCount);
        void DropMother(CharacterController character, int nId, Dictionary<int, int> Droplist);
        void DropSon(CharacterController character, int nId, Dictionary<int, int> Droplist);
    }

    public class DropDefaultImpl : IDrop
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //掉落母表
        public void DropMother(CharacterController character, int nId, Dictionary<int, int> Droplist)
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
                    character.DropSon(nId, Droplist);
                    return;
                }
            }
            //计数掉落
            if (tbdropmo.ExdataId >= 0 && tbdropmo.ExdataCount > 0 && tbdropmo.ExdataDropId >= 0)
            {
                character.AddExData(tbdropmo.ExdataId, 1);
                if (character.GetExData(tbdropmo.ExdataId) >= tbdropmo.ExdataCount)
                {
                    //累积掉落
                    Logger.Info("DropMother Exdata={0}", nId);
                    character.SetExData(tbdropmo.ExdataId, 0);
                    character.DropMother(tbdropmo.ExdataDropId, Droplist);
                    return;
                }
            }
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
                            character.DropSon(tbdropmo.DropSon[i], Droplist);
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
                            character.DropSon(tbdropmo.DropSon[i], Droplist);
                        }
                    }
                    Grouplist[GroupId] = pro - tbdropmo.Pro[i];
                }
            }
        }

        //掉落子表
        public void DropSon(CharacterController character, int nId, Dictionary<int, int> Droplist)
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
                                character.DropItem(Droplist, itemid, itemCount);
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
                                character.DropItem(Droplist, itemid, itemCount);
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
        public void DropItem(CharacterController character, Dictionary<int, int> Droplist, int nId, int nCount)
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

        static Drop()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (Drop), typeof (DropDefaultImpl),
                o => { mImpl = (IDrop) o; });
        }

        //掉落物品
        public static void DropItem(this CharacterController character,
                                    Dictionary<int, int> Droplist,
                                    int nId,
                                    int nCount)
        {
            mImpl.DropItem(character, Droplist, nId, nCount);
        }

        //掉落母表
        public static void DropMother(this CharacterController character, int nId, Dictionary<int, int> Droplist)
        {
            mImpl.DropMother(character, nId, Droplist);
        }

        //掉落子表
        public static void DropSon(this CharacterController character, int nId, Dictionary<int, int> Droplist)
        {
            mImpl.DropSon(character, nId, Droplist);
        }
    }
}