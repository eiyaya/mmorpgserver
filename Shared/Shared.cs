#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DataContract;
using DataTable;
using LZ4;
using NLog;
using ProtoBuf;
using Scorpion;

#endregion

namespace Shared
{
    public static class Runtime
    {
        public static bool Mono
        {
            get { return Type.GetType("Mono.Runtime") != null; }
        }
    }

    public static class LevelExpExtension
    {
        public static void AddExp(this LevelExp exp, int value)
        {
            var newExp = exp.Exp + value;
            var newLevel = exp.Level;
            if (newLevel >= exp.GetMaxLevel())
            {
                return;
            }
            var needExp = exp.GetNeedExp(newLevel);
            while (newExp >= needExp && needExp > 0)
            {
                newExp -= needExp;
                newLevel++;
                if (newLevel == exp.GetMaxLevel())
                {
                    newExp = 0;
                    break;
                }
                needExp = exp.GetNeedExp(newLevel);
            }
            exp.Exp = newExp;
            if (newLevel > exp.Level)
            {
                exp.Level = newLevel;
            }
        }
    }

    public interface LevelExp
    {
        int Exp { get; set; }
        int Level { get; set; }
        int GetMaxLevel();
        int GetNeedExp(int lvl);
    }

    public static class Extension
    {
        public static ulong AddTimeDiffToNet(int Milliseconds)
        {
            return (ulong) DateTime.Now.AddMilliseconds(Milliseconds).ToBinary();
        }

        public static DBInt ToDbInt(this int i)
        {
            return new DBInt {Value = i};
        }

        public static DBString ToDbString(this string str)
        {
            return new DBString {Value = str};
        }

        public static DBUlong ToDBUlong(this ulong i)
        {
            return new DBUlong {Value = i};
        }

        public static DBUlong ToDBUlong(this long i)
        {
            return new DBUlong {Value = (ulong) i};
        }
    }

    public static class GlobalVariable
    {
        public static string[] ServerNames =
        {
            "Login",
            "Logic",
            "Scene",
            "Chat",
            "Rank",
            "Activity",
            "Team",
            "GameMaster"
        };

        public static int WaitToConnectTimespan = 2000; //time in second wait after listen.
    }

    /// <summary>
    ///     标记位集合
    /// </summary>
    public class BitFlag
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     初始化BitFlag
        /// </summary>
        /// <param name="maxCount">位数</param>
        public BitFlag(int maxCount)
        {
            mFlag = new List<int>();
            Init(maxCount);
        }

        /// <summary>
        ///     初始化BitFlag
        /// </summary>
        /// <param name="maxCount">位数</param>
        /// <param name="defaultValue">默认值，如果0则所有位都是0，如果是个数字，则每个int都是数字</param>
        public BitFlag(int maxCount, int defaultValue)
        {
            mFlag = new List<int>();
            Init(maxCount, defaultValue);
        }

        public BitFlag(int maxCount, List<int> itmes)
        {
            mCount = maxCount;
            mFlag = itmes;
            var intCount = maxCount/32;
            if (maxCount%32 > 0)
            {
                ++intCount;
            }
            var nowCount = itmes.Count;
            if (nowCount < intCount)
            {
                for (var i = nowCount + 1; i <= intCount; ++i)
                {
                    mFlag.Add(0);
                }
            }
        }

        private int mCount;
        private readonly List<int> mFlag;

        /// <summary>
        ///     清除第nIndex位的标记
        /// </summary>
        /// <param name="nIndex"></param>
        public void CleanFlag(int nIndex)
        {
            if (nIndex < 0 || nIndex >= mCount)
            {
                return;
            }
            mFlag[nIndex/32] &= ~(1 << (nIndex%32));
            //if (mCount <= 32)
            //{
            //    string tempstr = "ClnFlag ";
            //    for (int j = 0; j < 32; j++)
            //    {
            //        tempstr += GetFlag(j).ToString();
            //    }
            //    Logger.Info(tempstr);
            //}
        }

        /// <summary>
        ///     获得两个int的“与”操作的结果
        /// </summary>
        /// <param name="data1">参数1</param>
        /// <param name="data2">参数2</param>
        /// <returns></returns>
        public static int GetAnd(int data1, int data2)
        {
            return data1 & data2;
        }

        public List<int> GetData()
        {
            return mFlag;
        }

        /// <summary>
        ///     获取第nIndex位的标记
        /// </summary>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        public int GetFlag(int nIndex)
        {
            if (nIndex < 0 || nIndex >= mCount)
            {
                return -1;
            }

            return ((mFlag[nIndex/32] >> (nIndex%32)) & 1);
        }

        /// <summary>
        ///     data的低bit位是不是1
        /// </summary>
        /// <param name="data">要取数的数据</param>
        /// <param name="bit">低几位</param>
        /// <returns></returns>
        public static bool GetLow(int data, int bit)
        {
            return Convert.ToBoolean((data >> bit) & 1);
        }

        public static bool GetLow(ulong data, int bit)
        {
            return Convert.ToBoolean((data >> bit) & 1);
        }

        public static bool GetLow(double data, int bit)
        {
            return Convert.ToBoolean((Convert.ToInt64(data) >> bit) & 1);
        }

        public void Init(List<int> itmes)
        {
//用于从数据库数据初始化
            mFlag.Clear();
            mFlag.AddRange(itmes);
        }

        private void Init(int maxCount, int defaultValue = 0)
        {
            mCount = maxCount;
            var intCount = maxCount/32;
            if (maxCount%32 > 0)
            {
                ++intCount;
            }
            for (var i = 0; i != intCount; ++i)
            {
                mFlag.Add(defaultValue);
                //if (defaultValue == 0 && maxCount <= 32)
                //{
                //    string tempstr = "IniFlag ";
                //    for (int j = 0; j < 32; j++)
                //    {
                //        tempstr += GetFlag(j).ToString();
                //    }
                //    Logger.Info(tempstr);
                //}
            }
        }

        public static int IntSetFlag(int data, int nIndex, bool value = true)
        {
            if (nIndex < 0 || nIndex >= 32)
            {
                return data;
            }
            if (value)
            {
                data |= 1 << nIndex;
            }
            else
            {
                data &= ~(1 << nIndex);
            }
            return data;
        }

        public static ulong LongSetFlag(ulong data, int nIndex, bool value = true)
        {
            if (nIndex < 0 || nIndex >= 64)
            {
                return data;
            }
            if (value)
            {
                data |= 1ul << nIndex;
            }
            else
            {
                data &= ~(1ul << nIndex);
            }
            return data;
        }

        public bool IsDirty()
        {
            foreach (var i in mFlag)
            {
                if (i != 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     清除所有标记
        /// </summary>
        public void ReSetAllFlag(bool dirty = false)
        {
            for (var i = 0; i < mFlag.Count; i++)
            {
                if (dirty)
                {
                    mFlag[i] = -1;
                }
                else
                {
                    mFlag[i] = 0;
                }
            }
        }

        /// <summary>
        ///     设置第nIndex位的标记
        /// </summary>
        /// <param name="nIndex"></param>
        public void SetFlag(int nIndex)
        {
            if (nIndex < 0 || nIndex >= mCount)
            {
                return;
            }
            mFlag[nIndex/32] |= 1 << (nIndex%32);

            //if (mCount <= 32)
            //{
            //    string tempstr = "SetFlag ";
            //    for (int j = 0; j < 32; j++)
            //    {
            //        tempstr += GetFlag(j).ToString();
            //    }
            //    Logger.Info(tempstr);
            //}
        }
    }

    //二个浮点数据
    public class Vector2Float
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Vector2Float()
        {
        }

        public Vector2Float(float fValue1, float fValue2)
        {
            mData[0] = fValue1;
            mData[1] = fValue2;
        }

        private readonly float[] mData = new float[2];

        /// <summary>
        ///     加法
        /// </summary>
        /// <param name="v2F"></param>
        public void Add(Vector2Float v2F)
        {
            mData[0] += v2F.mData[0];
            mData[1] += v2F.mData[1];
        }

        /// <summary>
        ///     获得某一位
        /// </summary>
        /// <param name="nIdex"></param>
        /// <returns>具体值</returns>
        public float GetIndex(int nIdex)
        {
            if (nIdex < 0 || nIdex > 1)
            {
                Logger.Warn(string.Format("Vector2Float::GetIndex = {0}", nIdex));
                return 0;
            }
            return mData[nIdex];
        }

        /// <summary>
        ///     设置值
        /// </summary>
        /// <param name="fValue1">第一个值</param>
        /// <param name="fValue2">第二个值</param>
        public void SetValue(float fValue1, float fValue2)
        {
            mData[0] = fValue1;
            mData[1] = fValue2;
        }

        /// <summary>
        ///     设置值
        /// </summary>
        /// <param name="v2F">用另一个该变量给其赋值</param>
        public void SetValue(Vector2Float v2F)
        {
            mData[0] = v2F.mData[0];
            mData[1] = v2F.mData[1];
        }

        /// <summary>
        ///     减法
        /// </summary>
        /// <param name="v2F"></param>
        public void Sub(Vector2Float v2F)
        {
            mData[0] -= v2F.mData[0];
            mData[1] -= v2F.mData[1];
        }
    }

    //通用判断 100000+职业*100000+部位*1000+颜色*100+INT（装备等级/5））
    public static class CheckGeneral
    {
        /// <summary>
        ///     判断物品类型
        /// </summary>
        /// <param name="nId">物品ID</param>
        /// <param name="it"></param>
        /// <returns></returns>
        public static bool CheckItemType(int nId, eItemType it)
        {
            if (GetItemType(nId) == it)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///     获得物品类型
        /// </summary>
        /// <param name="nId"></param>
        /// <returns></returns>
        public static eItemType GetItemType(int nId)
        {
            if (nId < 0)
            {
                return eItemType.Error;
            }

            var tbItem = Table.GetItemBase(nId);
            if (tbItem == null)
            {
                return eItemType.Error;
            }

            var tbItemType = Table.GetItemType(tbItem.Type);
            if (null == tbItemType)
            {
                // 默认返回装备
                return eItemType.BaseItem;
            }

            return (eItemType) (tbItemType.LogicType - 1);
        }
    }

    //随机数
    public static class MyRandom
    {
        private static readonly Random r = new Random();

        public static int Random(int i)
        {
            return r.Next(i);
        }

        public static double Random()
        {
            return r.NextDouble();
        }

        // [_min, _max]
        public static int Random(int _min, int _max)
        {
            if (_min == _max)
            {
                return _min;
            }
            if (_min > _max)
            {
                return r.Next(_max, _min + 1);
            }
            return r.Next(_min, _max + 1);
        }

        public static float Random(float _min, float _max)
        {
            if (_min == _max)
            {
                return _min;
            }
            var diff = _max - _min;
            var temp = (float) Random();
            return _min + diff*temp;
        }
    }

    //时间处理
    public static class DataTimeExtension
    {
        static public DateTime EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);

        public static DateTime ExtensionModDay(this DateTime dt)
        {
            dt = dt.AddMilliseconds(-dt.Millisecond);
            dt = dt.AddSeconds(-dt.Second);
            dt = dt.AddHours(-dt.Hour);
            dt = dt.AddMinutes(-dt.Minute);
            return dt;
        }

        //获取两个时间的天数差异
        public static long GetDiffDays(this DateTime dt1, DateTime dt2)
        {
            var mindt = dt1 > dt2 ? dt2 : dt1;
            var maxdt = dt1 > dt2 ? dt1 : dt2;
            return (long) ((maxdt.ExtensionModDay().AddDays(1) - mindt).TotalDays);
        }

        //获取两个时间的秒数差异
        public static long GetDiffSeconds(this DateTime dt1, DateTime dt2)
        {
            var mindt = dt1 > dt2 ? dt2 : dt1;
            var maxdt = dt1 > dt2 ? dt1 : dt2;
            return (long) ((maxdt - mindt).TotalSeconds);
        }

        // 获取距离1970/1/1 0:0:0秒数
        public static int GetTimeStampSeconds(this DateTime dt)
        {
            return (int)(dt.Subtract(EpochStart).TotalSeconds);
        }

        //根据字符取出刷新类型和时间
        public static void TimeEvent(string Content, ref int Type, ref int Hour)
        {
            var index = Content.IndexOf("Hour", 0, StringComparison.Ordinal);
            if (index != -1)
            {
                var Day = Content.IndexOf("Day", 0, StringComparison.Ordinal);
                if (Day != -1)
                {
                    Type = 0;
                    var strHour = Content.Substring(4, Day - 4);
                    if (Int32.TryParse(strHour, out Hour))
                    {
                        return;
                    }
                    return;
                }
                var Week = Content.IndexOf("Week", 0, StringComparison.Ordinal);
                if (Week != -1)
                {
                    Type = 1;
                    var strHour = Content.Substring(4, Week - 4);
                    if (Int32.TryParse(strHour, out Hour))
                    {
                        return;
                    }
                    return;
                }
                var Month = Content.IndexOf("Month", 0, StringComparison.Ordinal);
                if (Month != -1)
                {
                    Type = 2;
                    var strHour = Content.Substring(4, Month - 4);
                    if (Int32.TryParse(strHour, out Hour))
                    {
                    }
                }
            }
        }
    }

    public static class ListExtension
    {
        /// <summary>
        ///     取交集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a_list"></param>
        /// <param name="b_list"></param>
        /// <returns></returns>
        public static List<T> And<T>(this List<T> a_list, List<T> b_list)
        {
            var temp = new List<T>();
            for (var i = 0; i < a_list.Count; i++)
            {
                if (b_list.Contains(a_list[i]))
                {
                    temp.Add(a_list[i]);
                }
            }
            return temp;
        }

        /// <summary>
        ///     取交集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="b_list"></param>
        /// <param name="a_list"></param>
        /// <returns></returns>
        public static List<T> And<T>(this List<T> b_list, T[] a_list)
        {
            var temp = new List<T>();
            for (var i = 0; i < a_list.Length; i++)
            {
                if (b_list.Contains(a_list[i]))
                {
                    temp.Add(a_list[i]);
                }
            }
            return temp;
        }

        /// <summary>
        ///     用List的数据集生成字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static String GetDataString<T>(this IEnumerable<T> list)
        {
            var sb = new StringBuilder();
            sb.Append("<");
            var index = 0;
            foreach (var iter in list)
            {
                if (index != 0)
                {
                    sb.Append(",");
                }
                sb.Append(iter);
                index++;
            }
            sb.Append(">");
            return sb.ToString();
        }

        public static String GetDataString2<T>(this IEnumerable<T> list)
        {
            var sb = new StringBuilder();
            var index = 0;
            foreach (var iter in list)
            {
                if (index != 0)
                {
                    sb.Append(",");
                }
                sb.Append(iter);
                index++;
            }
            return sb.ToString();
        }

        public static T GetIndexValue<T>(this List<T> list, int index)
        {
            if (list.Count < index)
            {
                return default(T);
            }
            return list[index];
        }

        /// <summary>
        ///     获得List某个值的数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int GetValueCount<T>(this List<T> list, T key)
        {
            var comparer = EqualityComparer<T>.Default;
            return list.Count(a => comparer.Equals(a, key));
        }

        /// <summary>
        ///     从[0,maxcount-1]中取随机几个[x0,x1...]
        /// </summary>
        /// <param name="maxcount"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static List<int> Random(int maxcount, int count)
        {
            var templist = new List<int>();
            if (count == 1)
            {
                templist.Add(MyRandom.Random(maxcount));
                return templist;
            }
            for (var i = 0; i != maxcount; i++)
            {
                templist.Add(i);
            }
            if (maxcount <= count)
            {
                return templist;
            }
            var result = new List<int>();
            for (var i = 0; i != count; i++)
            {
                var r = MyRandom.Random(templist.Count);
                result.Add(templist[r]);
                templist.RemoveAt(r);
            }
            return result;
        }

        /// <summary>
        ///     从列表中的某个索引后，随机取count个数量，不改变原始List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="l"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<T> RandRange<T>(this List<T> l, int startIndex, int count)
        {
            var tempObjList = new List<T>();
            var tempIndex = Random(l.Count - startIndex, count);
            foreach (var i in tempIndex)
            {
                tempObjList.Add(l[i + startIndex]);
            }
            return tempObjList;
        }

        /// <summary>
        ///     随机一个元素出来
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="l"></param>
        /// <returns></returns>
        /// <exception cref="Exception">List is empty.</exception>
        /// <exception cref="ArgumentNullException">The value of l cannot be null. </exception>
        public static T Range<T>(this List<T> l)
        {
            if (l == null)
            {
                throw new ArgumentNullException();
            }
            if (l.Count == 0)
            {
                throw new Exception("List is empty.");
            }

            return l[MyRandom.Random(l.Count)];
        }
    }

    public static class DictionayExtension
    {
        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2)
        {
            foreach (var keyValuePair in dict2)
            {
                dict1.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public static String GetDataString<T>(this Dictionary<int, T> list)
        {
            var strRet = "<";
            foreach (var iter in list)
            {
                strRet = strRet + iter.Key + "," + iter.Value + "> ";
            }
            return strRet;
        }

        public static int getListCount<T, T2>(this Dictionary<T, List<T2>> list, T key)
        {
            List<T2> oldValue;
            if (list.TryGetValue(key, out oldValue))
            {
                return oldValue.Count;
            }
            return 0;
        }

        public static int getValue<T>(this Dictionary<T, int> list, T key)
        {
            int oldValue;
            if (list.TryGetValue(key, out oldValue))
            {
                return oldValue;
            }
            return 0;
        }

        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            TValue value;
            if (dict.TryGetValue(key, out value))
            {
                return value;
            }
            return default(TValue);
        }

        //往一个字典中增加某项的值
        public static int modifyValue<T>(this Dictionary<T, int> list, T key, int modifyValue)
        {
            int oldValue;
            if (list.TryGetValue(key, out oldValue))
            {
                oldValue = oldValue + modifyValue;
            }
            else
            {
                oldValue = modifyValue;
            }
            list[key] = oldValue;
            return oldValue;
        }

        //往一个字典中增加某项的值
        public static uint modifyValue<T>(this Dictionary<T, uint> list, T key, uint modifyValue)
        {
            uint oldValue;
            if (list.TryGetValue(key, out oldValue))
            {
                oldValue = oldValue + modifyValue;
            }
            else
            {
                oldValue = modifyValue;
            }
            list[key] = oldValue;
            return oldValue;
        }

        /// <summary>
        ///     随机一个迭代器
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static KeyValuePair<TKey, TValue> Random<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException();
            }

            var index = MyRandom.Random(dict.Count);
            foreach (var value in dict)
            {
                if (index == 0)
                {
                    return value;
                }

                index--;
            }

            throw new Exception("Dictionay is empty.");
        }

        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict1, TKey key, TValue value)
        {
            try
            {
                dict1[key] = value;
                //dict1.Add(key, value);
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                dict1[key] = value;
                return false;
            }
        }
    }

    public static class EquipExtension
    {
        public static List<int> EquipModelBagId = new List<int> { 7, 11, 12, 14, 15, 16, 17, 18, 29, 30, 31 };
        public static List<int> Equips = new List<int> { 7, 8, 11, 12, 13, 14, 15, 16, 17, 18, 29, 30, 31 };
    }

    public static class SkillExtension
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int GetSkillUpgradingValue(this SkillUpgradingRecord tbUpgrade, int nLevel)
        {
            switch (tbUpgrade.Type)
            {
                case 0: //枚举
                {
                    if (nLevel >= tbUpgrade.Values.Count || nLevel < 0)
                    {
                        // 注释掉这行日志，输出比较多
                        //Logger.Warn("SkillUpgradingRecord=[{0}]  Level=[{1}]  is Out", tbUpgrade.Id, nLevel);
                        return tbUpgrade.Values[tbUpgrade.Values.Count - 1];
                    }
                    var result = tbUpgrade.Values[nLevel];
                    return result;
                }
                case 1: //等差
                {
                    var result = tbUpgrade.Param[0] + tbUpgrade.Param[1]*nLevel;
                    return result;
                }
                case 2: //等比
                {
                    if (nLevel < 1)
                    {
                        return 1;
                    }
                    var result = tbUpgrade.Param[0]*Pow(tbUpgrade.Param[1], nLevel - 1);
                    return result;
                }
                case 3: //等级阶跃
                {
                    var length = tbUpgrade.Values.Count;
                    if (length < 2)
                    {
                        return 0;
                    }
                    var lastIndex = 1;
                    for (var i = 0; i < tbUpgrade.Values.Count; i = i + 2)
                    {
                        var lvl = tbUpgrade.Values[i];
                        if (nLevel < lvl)
                        {
                            return tbUpgrade.Values[lastIndex];
                        }
                        lastIndex = i + 1;
                    }
                    return tbUpgrade.Values[length - 1];
                }
                case 4: //跨等级枚举
                {
                    var valueLevel = nLevel/tbUpgrade.Param[0];
                    if (valueLevel >= tbUpgrade.Values.Count || valueLevel < 0)
                    {
                        Logger.Warn("SkillUpgradingRecord=[{0}]  Level=[{1}] valueLevel=[{2}]  is Out", tbUpgrade.Id,
                            nLevel, valueLevel);
                        return tbUpgrade.Values[tbUpgrade.Values.Count - 1];
                    }
                    var result = tbUpgrade.Values[valueLevel];
                    return result;
                }
            }
            return -1;
        }

        public static int Pow(int baseValue, int count)
        {
            if (count < 1)
            {
                return 1;
            }
            var value = baseValue;
            for (var i = 1; i < count; i++)
            {
                value *= baseValue;
            }
            return value;
        }

        public static float Pow(float baseValue, int count)
        {
            if (count < 1)
            {
                return 1;
            }
            var value = baseValue;
            for (var i = 1; i < count; i++)
            {
                value *= baseValue;
            }
            return value;
        }

        //获取服务器参数的Int值
        public static int ToInt(this ServerConfigRecord tbConfig)
        {
            int temp;
            if (Int32.TryParse(tbConfig.Value, out temp))
            {
                return temp;
            }
            return -1;
        }
    }

    public static class SceneExtension
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //根据ServerId 获取 逻辑ServerId
        public static int GetServerLogicId(int serverId)
        {
            if (serverId == -1)
            {
                return -1;
            }
            var tbServer = Table.GetServerName(serverId);
            if (tbServer == null)
            {
                Logger.Warn("not find Server GetServerLogicId = {0}", serverId);
                return -1;
            }
            return tbServer.LogicID;
        }

        public static int GetWillScene(int dbSceneId,
                                       int dbFromSceneId,
                                       int hp,
                                       DateTime relifeTime,
                                       DateTime offlineTime,
                                       ref ulong sceneGuid,
                                       ref bool isNeedRelive)
        {
            var sceneId = -1;
            var guid = sceneGuid;
            if (hp < 1)
            {
//时间是死了的
                if (relifeTime < DateTime.Now)
                {
                    var tbScene = Table.GetScene(dbSceneId);
                    if (tbScene.ReliveType[1] == 1)
                    {
                        sceneId = tbScene.CityId;
                        if (dbSceneId != sceneId)
                        {
                            guid = 0;
                        }
                        isNeedRelive = true;
                    }
                }
            }
            if (sceneId == -1)
            {
                sceneId = dbSceneId;
            }

            if (!IsNormalScene(sceneId))
            {
                var tbScene = Table.GetScene(sceneId);
                guid = sceneGuid;

                if (tbScene.Type == 3) //pvp
                {
                    if ((DateTime.Now - offlineTime).TotalSeconds > 60)
                    {
                        guid = 0ul;
                    }
                }
                else if (tbScene.Type == 2) //fuben
                {
                    var tbFuben = Table.GetFuben(tbScene.FubenId);
                    if (tbFuben != null
                        && tbFuben.MainType == (int) eDungeonMainType.Fuben
                        && tbFuben.AssistType == 2) //多人副本
                    {
                        if ((DateTime.Now - offlineTime).TotalSeconds > 60)
                        {
                            guid = 0ul;
                        }
                    }
                }
                else if (tbScene.Type == 6) //BossHome
                {
                    if ((DateTime.Now - offlineTime).TotalSeconds > 60)
                    {
                        guid = 0ul;
                    }
                }
                sceneId = dbFromSceneId;
                if (sceneId == -1)
                {
                    sceneId = tbScene.CityId != -1 ? tbScene.CityId : 3;
                }
            }

            sceneGuid = guid;
            return sceneId;
        }

        public static bool IsNormalScene(int sceneId)
        {
            var record = Table.GetScene(sceneId);
            if (record == null)
            {
                Logger.Error("sceneId:{0} can not found.", sceneId);
                return false;
            }
            return record.Type != 2 && record.Type != 3 && record.Type != 6;
        }
    }

    public static class ProtocolExtension
    {
        private static readonly ThreadLocal<MemoryStream> mMemoryStream =
            new ThreadLocal<MemoryStream>(() => new MemoryStream());

        public static T Deserialize<T>(byte[] data)
        {
            using (var ms = new MemoryStream(data, false))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }

        public static byte[] Serialize<T>(T data)
        {
            var stream = mMemoryStream.Value;
            stream.SetLength(0);
            stream.Seek(0, SeekOrigin.Begin);
            Serializer.Serialize(stream, data);
            return stream.ToArray();
        }
    }

    public static class StaticData
    {
        public static ulong CharacterIdBegin = 1000000;

        public static bool IsRobot(ulong guid)
        {
            return guid < CharacterIdBegin;
        }
    }

    public static class StringExtension
    {
        public static bool CheckChatHornStr(string desc)
        {
            if (GetStringLength(desc) > Table.GetServerConfig(311).ToInt())
            {
                return false;
            }
            return true;
        }

        public static bool CheckChatStr(string desc)
        {
            if (GetStringLength(desc) > Table.GetServerConfig(310).ToInt())
            {
                return false;
            }
            return true;
        }

        private static byte[] decodeBuffer = new byte[1024*32];
        public static int GetStringLength(string desc)
        {
            if (string.IsNullOrEmpty(desc))
            {
                return 0;
            }
            if (desc.Length > 1000)
            {
                return 1000;
            }
            //去除聊私聊名字的长度，不能当成内容名字
            var nend1 = desc.IndexOf("/", 0, StringComparison.Ordinal);
            if (nend1 == 0)
            {
                var nend2 = desc.IndexOf(" ", 0, StringComparison.Ordinal);
                if (nend2 != -1)
                {
                    desc = desc.Substring(nend2 + 1, desc.Length - nend2 - 1);
                }
            }
            var nend = 0;
            var nbegin = 0;
            var token1 = "{!";
            var token2 = "!}";
            var result_str = "";
            var addCount = 0;
            while (nend != -1)
            {
                nend = desc.IndexOf(token1, nbegin, StringComparison.Ordinal);
                if (nend == -1)
                {
                    var temp1 = desc.Substring(nbegin, desc.Length - nbegin);
                    result_str = result_str + temp1;
                }
                else
                {
//拼接井号前的字符
                    var temp3 = desc.Substring(nbegin, nend - nbegin);
                    result_str = result_str + temp3;
                    nbegin = nend + token1.Length;
                    //查询结束符
                    var findend = true;
                    var midstr = "";
                    while (findend)
                    {
                        nend = desc.IndexOf(token2, nbegin, StringComparison.Ordinal);
                        if (nend == -1)
                        {
//没有找到
                            var temp1 = desc.Substring(nbegin, desc.Length - nbegin);
                            result_str = result_str + midstr + temp1;
                            result_str = Regex.Replace(result_str, @"[[][A-Fa-f0-9]{6}[]]", "");
                            result_str = result_str.Replace("[-]", "");
                            return result_str.Length + addCount;
                        }
                        var temp2 = desc.Substring(nbegin, nend - nbegin);
                        midstr = midstr + temp2;

                        var bytes = Convert.FromBase64String(midstr);
                        var l = LZ4Codec.Decode(bytes, 0, bytes.Length, decodeBuffer, 0, decodeBuffer.Length);

                        ChatInfoNodeData data = null;
                        using (var ms = new MemoryStream(decodeBuffer, 0, l, false))
                        {
                            data = Serializer.Deserialize<ChatInfoNodeData>(ms);
                        }

                        if (data == null)
                        {
                            addCount += midstr.Length;
                        }
                        else
                        {
                            switch ((eChatLinkType) data.Type)
                            {
                                case eChatLinkType.Face:
                                {
                                    addCount += 2;
                                }
                                    break;
                                case eChatLinkType.Postion:
                                {
                                    addCount += 4;
                                    var tbScene = Table.GetScene(data.ExData[0]);
                                    if (tbScene != null)
                                    {
                                        addCount += tbScene.Name.Length;
                                    }
                                    var x = data.ExData[1]/100;
                                    var y = data.ExData[2]/100;
                                    addCount += x.ToString().Length;
                                    addCount += y.ToString().Length;
                                }
                                    break;
                                case eChatLinkType.Equip:
                                {
                                    var tbItem = Table.GetItemBase(data.Id);
                                    if (tbItem != null)
                                    {
                                        addCount += 2;
                                        addCount += tbItem.Name.Length;
                                    }
                                }
                                    break;
                                case eChatLinkType.Team:
                                {
                                    addCount += data.ExData[0];
                                }
                                    break; 
//                                     case eChatLinkType.Dictionary:
//                                         {
//                                             var str = GameUtils.GetDictionaryText(data.Id);
//                                             addCount += str.Length;
//                                         }
//                                         break;
                            }
                        }
                        findend = false;
                        nbegin = nend + token2.Length;
                    }
                }
            }

            result_str = Regex.Replace(result_str, @"[[][A-Fa-f0-9]{6}[]]", "");
            result_str = result_str.Replace("[-]", "");

            return result_str.Length + addCount;
        }
    }

    public static class SpecialCode
    {
        public const string ChatBegin = "{!";
        public const string ChatEnd = "!}";
    }

    public class DamageUnitComparer : IComparer<DamageUnit>
    {
        public int Compare(DamageUnit x, DamageUnit y)
        {
            if (x.Damage > y.Damage)
            {
                return -1;
            }
            if (x.Damage < y.Damage)
            {
                return 1;
            }
            return 0;
        }
    }

    public class RankingUnitComparer : IComparer<RankingInfoOne>
    {
        public int Compare(RankingInfoOne x, RankingInfoOne y)
        {
            if (x.value > y.value)
            {
                return -1;
            }
            if (x.value < y.value)
            {
                return 1;
            }
            return 0;
        }
    }

    //工具函数
    public static class Utils
    {                            //<级别,  <id,个数>>
        private static Dictionary<int, Dictionary<int, int>> dicVipReward = new Dictionary<int, Dictionary<int, int>>();
  
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void initVipReward()
        {
            Table.ForeachVIP(tb =>
            {
                Dictionary<int, int> dic = new Dictionary<int, int>();
                var id = tb.Id;
                var reward = tb.PackageId.Split('|');
                foreach (var str in reward)
                {
                    var temp = str.Split('*');
                    if (temp.Length != 2)
                        continue;
                    dic.modifyValue(int.Parse(temp[0]), int.Parse(temp[1]));
                }
                dicVipReward.Add(id,dic);
                return true;
            });
        }

        public static void GetVipReward(int lv,ref Dictionary<int,int> dic )
        {
            if (dicVipReward.Count == 0)
            {
                initVipReward();
            }
            if (dicVipReward.ContainsKey(lv))
            {
                dic.AddRange(dicVipReward[lv]);
            }
        }
        public static string AddCharacter(ulong id, string name)
        {
            return "#Character:" + name + ":" + id;
        }

        public static string AddDate(long date, int formatDicId)
        {
            return "#Date:" + date + ":" + formatDicId;
        }

        public static string AddDictionaryId(int id)
        {
            return "#Dictionary:" + id;
        }

        public static string AddItemId(int itemId)
        {
            return "#ItemBase.Name:" + itemId;
        }

        public static string AddSceneId(int sceneId)
        {
            return "#Scene.Name:" + sceneId;
        }

        public static string AddSceneId(List<int> sceneIds)
        {
            if (sceneIds == null || sceneIds.Count == 0)
            {
                return "";
            }
            if (sceneIds.Count == 1)
            {
                return AddSceneId(sceneIds[0]);
            }
            var ret = AddSceneId(sceneIds[0]);
            for (var i = 1; i < sceneIds.Count; i++)
            {
                ret += ":" + sceneIds[i];
            }
            return ret;
        }

        public static bool GetDungeonOpenTime(FubenRecord tbFuben, out int hour, out int min)
        {
            hour = min = -1;
            if (tbFuben.OpenTime[0] == -1)
            {
                return false;
            }

            var lastMin = tbFuben.CanEnterTime;
            var now = DateTime.Now;
            foreach (var time in tbFuben.OpenTime)
            {
                var h = time/100;
                var m = time%100;
                var startTime = new DateTime(now.Year, now.Month, now.Day, h, m, 0, DateTimeKind.Local);
                if (startTime <= now && startTime.AddMinutes(lastMin) >= now)
                {
                    hour = h;
                    min = m + tbFuben.OpenLastMinutes;
                    return true;
                }
            }
            return false;
        }

        public static DateTime GetNextDungeonOpenTime(FubenRecord tbFuben)
        {
            var now = DateTime.Now;
            if (tbFuben.OpenTime[0] == -1)
            {
                return now;
            }

            foreach (var time in tbFuben.OpenTime)
            {
                var h = time/100;
                var m = time%100;
                var startTime = new DateTime(now.Year, now.Month, now.Day, h, m, 0, DateTimeKind.Local);
                if (startTime >= now)
                {
                    return startTime;
                }
            }
            {
                var time = tbFuben.OpenTime[0];
                var h = time/100;
                var m = time%100;
                return new DateTime(now.Year, now.Month, now.Day, h, m, 0, DateTimeKind.Local).AddDays(1);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="tbFuben">FubenRecord的引用</param>
        /// <param name="count">表里配的奖励数量</param>
        /// <param name="starLevel">用来计算星级奖励的level</param>
        /// <param name="level">用来计算动态奖励的level</param>
        /// <returns></returns>
        public static int GetRewardCount(FubenRecord tbFuben, int count, int starLevel = 0, int level = 0)
        {
            if (tbFuben.IsStarReward == 1)
            {
//星级奖励
                if (tbFuben.IsDyncReward == 1)
                {
//动态奖励
                    var tbUp1 = Table.GetSkillUpgrading(count);
                    if (tbUp1 == null)
                    {
                        Logger.Error("In GetRewardCount() tbUp1 == null! fuben id = {0}, ", tbFuben.Id);
                        return -1;
                    }
                    var suId = tbUp1.GetSkillUpgradingValue(starLevel);
                    var tbUp2 = Table.GetSkillUpgrading(suId);
                    if (tbUp2 == null)
                    {
                        Logger.Error("In GetRewardCount() tbUp2 == null! fuben id = {0}, ", tbFuben.Id);
                        return 0;
                    }
                    return tbUp2.GetSkillUpgradingValue(level);
                }
                else
                {
                    var tbUp1 = Table.GetSkillUpgrading(count);
                    if (tbUp1 == null)
                    {
                        Logger.Error("In GetRewardCount() tbUp1 == null! fuben id = {0}, ", tbFuben.Id);
                        return -1;
                    }
                    return tbUp1.GetSkillUpgradingValue(starLevel);
                }
            }
            if (tbFuben.IsDyncReward == 1)
            {
//动态奖励
                var tbUp1 = Table.GetSkillUpgrading(count);
                if (tbUp1 == null)
                {
                    Logger.Error("In GetRewardCount() tbUp1 == null! fuben id = {0}, ", tbFuben.Id);
                    return -1;
                }
                return tbUp1.GetSkillUpgradingValue(level);
            }
            return count;
        }

        public static TimeSpan GetServerAge(int serverId)
        {
            var tbServer = Table.GetServerName(serverId);
            var startTime = DateTime.Parse(tbServer.OpenTime);
            var now = DateTime.Now;
            return now - startTime;
        }

        public static string GetTableColorString(int id)
        {
            var tb = Table.GetColorBase(id);
            if (tb == null)
            {
                return "FFFFFF";
            }
            var ret = string.Format("{0:X2}{1:X2}{2:X2}", tb.Red, tb.Green, tb.Blue);
            return ret;
        }


        public static String GetPositionById(int id)
        {

            if (id == 226002 || id == 226003 || id == 226008 || id == 226009 || id == 226014 || id == 226015)
            {
                return Table.GetDictionary(300000099).Desc[0];
            }
            if (id == 226000 || id == 226001 || id == 226006 || id == 226007 || id == 226012 || id == 226013)
            {
                return Table.GetDictionary(300000100).Desc[0];
            }
            if (id == 226004 || id == 226005 || id == 226010 || id == 226011 || id == 226016 || id == 226017)
            {
                return Table.GetDictionary(300000102).Desc[0];
            }
            return "";
        }

        public static string WrapDictionaryId(int dictId, List<string> strs = null, List<int> exInt = null)
        {
            var data = new ChatInfoNodeData();
            data.Type = (int) eChatLinkType.Dictionary;
            data.Id = dictId;
            if (strs != null)
            {
                data.StrExData.AddRange(strs);
            }
            var str = "";
            if (exInt != null)
            {
                data.ExData.AddRange(exInt);
            }
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, data);
                var wrap = LZ4Codec.Encode(ms.GetBuffer(), 0, (int) ms.Length);
                str = Convert.ToBase64String(wrap);
            }
            str = SpecialCode.ChatBegin + str + SpecialCode.ChatEnd;
            return str;
        }
        public static string WrapPositionDictionaryId(int dictId, List<string> strs = null, List<int> exInt = null)
        {
            var data = new ChatInfoNodeData();
            data.Type = (int)eChatLinkType.Postion;
            data.Id = dictId;
            if (strs != null)
            {
                data.StrExData.AddRange(strs);
            }
            var str = "";
            if (exInt != null)
            {
                data.ExData.AddRange(exInt);
            }
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, data);
                var wrap = LZ4Codec.Encode(ms.GetBuffer(), 0, (int)ms.Length);
                str = Convert.ToBase64String(wrap);
            }
            str = SpecialCode.ChatBegin + str + SpecialCode.ChatEnd;
            str += (String.Format(Table.GetDictionary(dictId).Desc[0], strs.ToArray()));
            return str;
        }

        public static int GetEquipLevelExPos(int bagId,int bagIdx)
        {
            switch (bagId)
            {
                case (int)eBagType.Equip01:         //头盔
                    return 722;
                case (int)eBagType.Equip02:       //项链
                    return 727;
                case (int)eBagType.Equip05:          //胸甲
                    return 723;
                case (int)eBagType.Equip07:          //左戒指
                    if (bagIdx == 0)
                        return 728;
                    else
                        return 729;
                case (int)eBagType.Equip08:           //手套
                    return 726;
                case (int)eBagType.Equip09:            //裤子
                    return 724;
                case (int)eBagType.Equip10:           //靴子
                    return 725;
                case (int)eBagType.Equip11:     //主手
                    return 720;
                case (int)eBagType.Equip12:    //副手
                    return 721;
                case (int)eBagType.Mount:
                    return 740;
            }
            return -1;
        }
        public static int GetEquipAddtionalExPos(int bagId, int bagIdx)
        {
            switch (bagId)
            {
                case (int)eBagType.Equip01:         //头盔
                    return 732;
                case (int)eBagType.Equip02:       //项链
                    return 737;
                case (int)eBagType.Equip05:          //胸甲
                    return 733;
                case (int)eBagType.Equip07:          //左戒指
                    if (bagIdx == 0)
                        return 738;
                    else
                        return 739;
                case (int)eBagType.Equip08:           //手套
                    return 736;
                case (int)eBagType.Equip09:            //裤子
                    return 734;
                case (int)eBagType.Equip10:           //靴子
                    return 735;
                case (int)eBagType.Equip11:     //主手
                    return 730;
                case (int)eBagType.Equip12:    //副手
                    return 731;
            }
            return -1;
        }

        private static void AttrConvert(Dictionary<int, int> AttrList, int[] attr, int[] attrRef, int roleId)
        {
            foreach (var i in AttrList)
            {
                if (i.Key < (int)eAttributeType.AttrCount)
                {
                    attr[i.Key] = i.Value;
                }
                else
                {
                    switch (i.Key)
                    {
                        case 105:
                            {
                                if (roleId != 1)
                                {
                                    attr[(int)eAttributeType.PhyPowerMin] += i.Value;
                                    attr[(int)eAttributeType.PhyPowerMax] += i.Value;
                                }
                                else
                                {
                                    attr[(int)eAttributeType.MagPowerMin] += i.Value;
                                    attr[(int)eAttributeType.MagPowerMax] += i.Value;
                                }
                            }
                            break;
                        case 106:
                            {
                                attrRef[(int)eAttributeType.MagPowerMin] += i.Value * 100;
                                attrRef[(int)eAttributeType.MagPowerMax] += i.Value * 100;
                                attrRef[(int)eAttributeType.PhyPowerMin] += i.Value * 100;
                                attrRef[(int)eAttributeType.PhyPowerMax] += i.Value * 100;
                            }
                            break;
                        case 110:
                            {
                                attr[(int)eAttributeType.PhyArmor] += i.Value;
                                attr[(int)eAttributeType.MagArmor] += i.Value;
                            }
                            break;
                        case 111:
                            {
                                attrRef[(int)eAttributeType.PhyArmor] += i.Value * 100;
                                attrRef[(int)eAttributeType.MagArmor] += i.Value * 100;
                            }
                            break;
                        case 113:
                            {
                                attrRef[(int)eAttributeType.HpMax] += i.Value * 100;
                            }
                            break;
                        case 114:
                            {
                                attrRef[(int)eAttributeType.MpMax] += i.Value * 100;
                            }
                            break;
                        case 119:
                            {
                                attrRef[(int)eAttributeType.Hit] += i.Value * 100;
                            }
                            break;
                        case 120:
                            {
                                attrRef[(int)eAttributeType.Dodge] += i.Value * 100;
                            }
                            break;
                    }
                }
            }
        }
        private static int CalcFightPoint(int[] attr, int[] attrRef, int level)
        {
            //var level = GetLevel();
            var tbLevel = Table.GetLevelData(level);
            if (tbLevel == null)
            {
                return 0;
            }
            var FightPoint = 0L;
            for (var type = eAttributeType.PhyPowerMin; type != eAttributeType.HitRecovery; ++type)
            {
                //基础固定属性
                long nValue = attr[(int)type];
                switch ((int)type)
                {
                    case 15:
                        {
                            FightPoint += nValue * tbLevel.LuckyProFightPoint / 10000;
                        }
                        break;
                    case 17:
                        {
                            FightPoint += nValue * tbLevel.ExcellentProFightPoint / 10000;
                        }
                        break;
                    case 21:
                        {
                            FightPoint += nValue * tbLevel.DamageAddProFightPoint / 10000;
                        }
                        break;
                    case 22:
                        {
                            FightPoint += nValue * tbLevel.DamageResProFightPoint / 10000;
                        }
                        break;
                    case 23:
                        {
                            FightPoint += nValue * tbLevel.DamageReboundProFightPoint / 10000;
                        }
                        break;
                    case 24:
                        {
                            FightPoint += nValue * tbLevel.IgnoreArmorProFightPoint / 10000;
                        }
                        break;
                    default:
                        {
                            var tbState = Table.GetStats((int)type);
                            if (tbState == null)
                            {
                                continue;
                            }
                            FightPoint += tbState.PetFight * nValue / 100;
                        }
                        break;
                }
            }

            //百分比计算
            FightPoint += attrRef[(int)eAttributeType.MagPowerMin] * tbLevel.PowerFightPoint / 10000 / 100;
            FightPoint += attrRef[(int)eAttributeType.PhyArmor] * tbLevel.ArmorFightPoint / 10000 / 100;
            FightPoint += attrRef[(int)eAttributeType.HpMax] * tbLevel.HpFightPoint / 10000 / 100;
            FightPoint += attrRef[(int)eAttributeType.MpMax] * tbLevel.MpFightPoint / 10000 / 100;
            FightPoint += attrRef[(int)eAttributeType.Hit] * tbLevel.HitFightPoint / 10000 / 100;
            FightPoint += attrRef[(int)eAttributeType.Dodge] * tbLevel.DodgeFightPoint / 10000 / 100;
            return (int)FightPoint;
        }

        public static int CalcAttrFightPoint(Dictionary<int, int> fightAttr, int characterLevel, int roleId)
        {
            var talentData = new int[(int)eAttributeType.AttrCount];
            var talentDataRef = new int[(int)eAttributeType.AttrCount];
            AttrConvert(fightAttr, talentData, talentDataRef, roleId);
            var fightPoint = CalcFightPoint(talentData, talentDataRef, characterLevel);
            return fightPoint;
        }

        public static bool CheckIsWeekLoopOk(List<int> list)
        {
            if (list.Count <= 0) // 没填则认为OK
            {
                return true;
            }
            foreach (var value in list)
            {
                if (value == (int)DateTime.Now.DayOfWeek)
                {
                    return true;
                }
            }
            return false;
        }
    }



    public class ScorpionLogger : Scorpion.ILogger
    {
        NLog.Logger _logger;

        public ScorpionLogger(Logger logger)
        {
            _logger = logger;
        }

        public void Debug(string format, params object[] args)
        {
            _logger.Debug(format, args);
        }

        public void Error(string format, params object[] args)
        {
            _logger.Error(format, args);
        }

        public void ErrorException(string err, Exception ex)
        {
            _logger.Error(ex, err);
        }

        public void Fatal(string format, params object[] args)
        {
            _logger.Fatal(format, args);
        }

        public void Info(string format, params object[] args)
        {
            _logger.Info(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            _logger.Warn(format, args);
        }

        public void WarnException(string warn, Exception ex)
        {
            _logger.Warn(ex, warn);
        }

        public void FatalException(string fatal, Exception ex)
        {
            _logger.Fatal(ex, fatal);
        }
    }


}