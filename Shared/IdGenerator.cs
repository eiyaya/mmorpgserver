#region using

using System;
using System.Collections;
using System.Collections.Generic;
using Database;
using DataContract;
using Scorpion;

#endregion

namespace Shared
{
    public static class IdGenerator
    {
        private static readonly int AddMax = 999;
        private static readonly int AddMin = 1;
        private static DataManager DB;
        private static bool DbDirty;
        private static readonly Random R = new Random((int) DateTime.Now.ToBinary());
        private static TimeManager Timer;

        public static string Confuse(ulong number)
        {
            var result = string.Empty;
            Confuse(number, ref result);
            return result;
        }

        private static void Confuse(ulong number, ref string result)
        {
            var remainder = (int) (number%ArrayLen);
            result = Array[remainder] + result;
            var a = number/ArrayLen;
            if (a >= ArrayLen)
            {
                Confuse(a, ref result);
            }
            else if (a > 0)
            {
                result = Array[(int) a] + result;
            }
        }

        private static ulong CyclicShift(ulong num, int bit)
        {
            var a = num << bit;
            var b = num >> (64 - bit);
            return a | b;
        }

        public static IEnumerator Init(Coroutine co, DataManager db, TimeManager timer)
        {
            DB = db;
            Timer = timer;
            var co1 = CoroutineFactory.NewSubroutine(LoadCoroutine, co);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            Timer.CreateTrigger(DateTime.Now.AddMinutes(2), Save, 60000);
        }

        private static IEnumerator LoadCoroutine(Coroutine co)
        {
            var data = DB.Get<DBUlong>(co, DataCategory.GiftCode, "seed");
            yield return data;
            if (data.Status != DataStatus.Ok)
            {
                yield break;
            }
            if (data.Data != null)
            {
                Seed = data.Data.Value;
            }
        }

        private static ulong Mix(ulong count)
        {
            var a = count & OddMask;
            var b = count & EvenMask;
            a = CyclicShift(a, 6);
            return a | b;
        }

        public static string Next(int len = -1)
        {
            Seed += (ulong) R.Next(AddMin, AddMax);
            DbDirty = true;
            var ret = Confuse(Mix(Seed));
            while (ret.Length < len)
            {
                ret = Zero + ret;
            }
            return ret.Substring(0, len);
        }

        public static ulong RevertConfuse(string code)
        {
            var ret = 0ul;
            foreach (var c in code)
            {
                var a = Array.IndexOf(c.ToString());
                if (a >= 0)
                {
                    ret = ArrayLen*ret + (ulong) a;
                }
            }
            return ret;
        }

        private static void Save()
        {
            if (!DbDirty)
            {
                return;
            }
            CoroutineFactory.NewCoroutine(SaveCoroutine).MoveNext();
        }

        public static IEnumerator SaveCoroutine(Coroutine co)
        {
            var data = DB.Set(co, DataCategory.GiftCode, "seed", new DBUlong
            {
                Value = Seed
            });
            yield return data;
        }

        private static readonly ulong Start = 100000000ul;

        private static readonly List<string> Array = new List<string>
        {
            "Q",
            "T",
            "R",
            "H",
            "6",
            "J",
            "X",
            "C",
            "Z",
            "7",
            "9",
            "E",
            "L",
            "D",
            "Y",
            "U",
            "W",
            "K",
            "5",
            "P",
            "S",
            "V",
            "F",
            "A",
            "N",
            "B",
            "8",
            "2",
            "3",
            "G",
            "4",
            "M"
        };

        private static readonly ulong ArrayLen = (ulong) Array.Count;
        private static readonly string Zero = Array[0];
        public static ulong Seed = Start;
        private static readonly ulong OddMask = 0x5555555555555555ul;
        private static readonly ulong EvenMask = ~OddMask;
    }
}