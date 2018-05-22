using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using EventSystem;
using NLog;

namespace DataTable
{
    public class ReloadTableEvent : EventBase
    {
        public static string EVENT_TYPE = "ReloadTableEvent";
        public string tableName { get; set; }
        public ReloadTableEvent(string name)
            : base(EVENT_TYPE)
        {
            tableName = name;
        }
    }
    public static class Table_Tamplet
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();      //
        public static int Convert_Int(string _str)
        {
            int temp;
            if (Int32.TryParse(_str, out temp))
            {
                return temp;
            }
            //Logger.Error("Convert_Int Error!  {0}", _str);
            throw new Exception(string.Format("Convert_Int Error!  {0}", _str));
        }

        public static float Convert_Float(string _str)
        {
            float temp;
            if (Single.TryParse(_str, out temp))
            {
                return temp;
            }
            //Logger.Error("Convert_Float Error!  {0}", _str);
            throw new Exception(string.Format("Convert_Float Error!  {0}", _str));
        }
        
        public static double Convert_Double(string _str)
        {
            double temp;
            if (Double.TryParse(_str, out temp))
            {
                return temp;
            }
            //Logger.Error("Convert_Double Error!  {0}", _str);
            throw new Exception(string.Format("Convert_Double Error!  {0}", _str));
            //return 0;
        }
        public static string Convert_String(string _str)
        {
            return Convert.ToString(_str);
        }
        public static void Convert_Value(List<int> _col_name, string str)
        {
			_col_name.Clear();

			if (string.IsNullOrEmpty(str))
            {
                return;
            }
            
            string[] temp = str.Split('|');
            foreach (var s in temp)
            {
                int temp_int = Convert.ToInt32(s);
                _col_name.Add(temp_int);
            }
        }
        public static void Convert_Value(List<List<int>> _col_name, string str)
        {
			_col_name.Clear();

            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            string[] temp1 = str.Split(';');
            Int16 i = 0;
            foreach (string ss in temp1)
            {
                _col_name.Add(new List<int>());
                string[] temp2 = ss.Split(',');
                foreach (string s in temp2)
                {
                    _col_name[i].Add(Convert.ToInt32(s));
                }
                ++i;
            }
        }
    }
    public interface IRecord
    {
        void __Init__(string[] strs);
    }
    public static class TableInit<T> where T : IRecord, new()
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //加载表格
        public static void Table_Init(string _path, Dictionary<int, T> _table_name)
        {
            _table_name.Clear();
            System.IO.TextReader tr = null;
            try
            {
                tr = new System.IO.StreamReader(_path, Encoding.UTF8);
                Int32 state = 1;
                string str = tr.ReadLine();
                while (str != null)
                {
                    string[] strs = str.Split('\t');
                    string first = strs[0];
                    if (state == 1 && first == "INT")
                    {
                        state = 2;
                    }
                    else if (first.Substring(0, 1) == "#" || first == "" || first == " ") //跳过此行加载
                    {

                    }
                    else if (state == 2)
                    {
                        state = 3;
                    }
                    else if (state == 3)
                    {
                        var t = new T();
                        t.__Init__(strs);
                        _table_name[Convert.ToInt32(first)] = t;
                    }
                    str = tr.ReadLine();
                }
            }
            catch (Exception ex)
            {
                //加入表格加载错误提示
                Logger.Error("Load " + _path + " Error!!");
                throw ex;
            }
            finally
            {
                if (tr != null)
                {
                    tr.Close();
                }
            }
        }


        public static void Table_Reload(string _path, Dictionary<int, T> _table_name)
        {
            //_table_name.Clear();
            System.IO.TextReader tr = null;
            try
            {
                tr = new System.IO.StreamReader(_path, Encoding.UTF8);
                Int32 state = 1;
                string str = tr.ReadLine();
                while (str != null)
                {
                    string[] strs = str.Split('\t');
                    string first = strs[0];
                    if (state == 1 && first == "INT")
                    {
                        state = 2;
                    }
                    else if (first.Substring(0, 1) == "#" || first == "" || first == " ") //跳过此行加载
                    {

                    }
                    else if (state == 2)
                    {
                        state = 3;
                    }
                    else if (state == 3)
                    {
                        T t;
                        int id = Convert.ToInt32(first);
                        if (!_table_name.TryGetValue(id, out t))
                        {
                            t = new T();
                            _table_name[Convert.ToInt32(first)] = t;
                        }
                        t.__Init__(strs);
                    }
                    str = tr.ReadLine();
                }
            }
            catch (Exception ex)
            {
                //加入表格加载错误提示
                Logger.Error("Load " + _path + " Error!!");
                throw ex;
            }
            finally
            {
                if (tr != null)
                {
                    tr.Close();
                }
            }
        }

        public static T Table_Copy(T _TB_Value)
        {
            return _TB_Value;
        }
    }

    public static class TalbeHelper
    {
        public static string Path = "../Tables/";
        public static string GetLoadPath(string localName)
        {
            return Path + localName + ".txt";
        }
    }
}
