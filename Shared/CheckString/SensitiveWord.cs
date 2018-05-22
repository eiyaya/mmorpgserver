#region using

using System;
using DataTable;

#endregion

namespace Shared
{
    public static class SensitiveWord
    {
        public static bool CheckString(string str)
        {
            var isSensitive = true;
            Table.ForeachSensitiveWord(record =>
            {
                if (str.IndexOf(record.Name, 0, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    isSensitive = false;
                    return false;
                }
                //if (String.Compare(str, record.Name, StringComparison.OrdinalIgnoreCase) == 0)
                //{
                //    isSensitive = false;
                //    return false;
                //}
                return true;
            });
            return isSensitive;
        }
    }
}