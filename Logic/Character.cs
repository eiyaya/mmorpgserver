using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using DataTable;
using NLog;

namespace Logic
{
    public class Character
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();      //
        private ulong mGuid;                                    //玩家的Guid
        public PlayerBag mBag = new PlayerBag();                //玩家包裹
        public MissionManager mTask = new MissionManager();     //玩家任务
        public BitFlag mFlag = new BitFlag(2048);               //玩家标记位
        public int[] mExdata = new int[1024];                      //玩家扩展数据
        //基础方法

        public ulong Guid
        {
            get { return mGuid; }
            set { mGuid = value; }
        }
        //public ulong GetGuid() { return mGuid; }
        //public void SetGuid(ulong Guid) { mGuid = Guid; }


        //使用装备
        /// <summary>
        /// 使用装备
        /// </summary>
        /// <param name="nBagIndex">装备包裹的索引</param>
        /// <returns></returns>
        public ErrorCodes UseEquip(int nBagIndex)
        {
            //索引是否有效
            ItemBase item=mBag.mBags[0].GetItemByIndex(nBagIndex);
            if(item==null) return ErrorCodes.Error_BagIndexNoItem;
            //道具是否装备
            if(!CheckGeneral.CheckItemType(item.GetId(),eItemType.Equip))  return ErrorCodes.Error_ItemIsNoEquip;
            //获得装备点
            var tb_item=Table.ItemBase[item.GetId()];
            int nEquipPoint=tb_item.Type - 9993;
            //穿戴条件检查

            //执行
            return mBag.MoveItem(0, nBagIndex, nEquipPoint, 0, 1);
        }

        //判断条件
        public static bool CheckCondition(int nConditionId)
        {
            return true;
        }
    }
}
