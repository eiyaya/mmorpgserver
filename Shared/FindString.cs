#region using

using System.Collections.Generic;

#endregion

namespace Shared
{
    public class FindString<T>
    {
        public Dictionary<string, Dictionary<T, bool>> dic = new Dictionary<string, Dictionary<T, bool>>();
        private readonly List<string> list = new List<string>();

        public Dictionary<T, bool> Find(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            Dictionary<T, bool> outvalue;
            if (dic.TryGetValue(s, out outvalue))
            {
                return outvalue;
            }
            return null;
        }

        //type1 :abc= a,ab,abc,b,bc,c
        //type2 :abc=a,ab,abc
        private List<string> GetStringList(string s)
        {
            list.Clear();
            //for (int i = 0; i < s.Length; i++)
            //{
            //    for (int j = 0; j < i; j++)
            //    {
            //        list.Add(s.Substring(j,i-j));
            //    }
            //}
            for (int i = 1, max = s.Length; i <= max; i++)
            {
                list.Add(s.Substring(0, i));
            }
            return list;
        }

        public void Push(string s, T t)
        {
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            var lists = GetStringList(s);
            foreach (var ss in lists)
            {
                var ll = Find(ss);
                if (ll == null)
                {
                    dic[ss] = new Dictionary<T, bool> {{t, true}};
                }
                else
                {
                    ll[t] = true;
                }
            }
        }

        public void Remove(string s, T t)
        {
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            var lists = GetStringList(s);
            foreach (var ss in lists)
            {
                var ll = Find(ss);
                if (ll != null)
                {
                    ll.Remove(t);
                }
            }
        }
    }
}