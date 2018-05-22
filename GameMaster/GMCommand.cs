#region using

using System;
using System.Collections.Generic;
using DataTable;
using EventSystem;
using NLog;

#endregion

namespace GameMaster
{
    public static class GMCommandManager
    {
        public static Dictionary<string, eGmCommandType> Gms = new Dictionary<string, eGmCommandType>();
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        private static bool CheckCommandLogic(string[] strs)
        {
            if (strs.Length < 1)
            {
                return false;
            }

            if (String.Compare(strs[0], "!!ReloadTable", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (strs.Length < 2)
                {
                    return false;
                }
                return true;
            }
            if (String.Compare(strs[0], "!!UpdateServer", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            if (String.Compare(strs[0], "!!PetMissionDone", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            if (String.Compare(strs[0], "!!PetMissionRefresh", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            if (String.Compare(strs[0], "!!PushMail", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (strs.Length < 2)
                {
                    return false;
                }

                if (strs.Length == 4)
                {
                    int TempInt;
                    if (!Int32.TryParse(strs[3], out TempInt))
                    {
                        return false;
                    }
                }
                else if (strs.Length == 5)
                {
                    int TempInt;
                    if (!Int32.TryParse(strs[3], out TempInt))
                    {
                        return false;
                    }
                    int TempInt2;
                    if (!Int32.TryParse(strs[4], out TempInt2))
                    {
                        return false;
                    }
                }
                else if (strs.Length == 6)
                {
                    ulong Guid;
                    if (!ulong.TryParse(strs[5], out Guid))
                    {
                        return false;
                    }

                    int TempInt;
                    if (!Int32.TryParse(strs[3], out TempInt))
                    {
                        return false;
                    }

                    int TempInt2;
                    if (!Int32.TryParse(strs[4], out TempInt2))
                    {
                        return false;
                    }
                }
                return true;
            }
            var nIndex = 0;
            foreach (var s in strs)
            {
                if (nIndex != 0)
                {
                    int TempInt;
                    if (!Int32.TryParse(s, out TempInt))
                    {
                        return false;
                    }
                }
                nIndex++;
            }
            return true;
        }

        public static string CheckCommands(List<string> commands)
        {
            var ret = string.Empty;
            foreach (var command in commands)
            {
                var strs = command.Split(',');
                eGmCommandType type;
                if (Gms.TryGetValue(strs[0], out type))
                {
                    switch (type)
                    {
                        case eGmCommandType.GMLogic:
                            if (!CheckCommandLogic(strs))
                            {
                                ret += command + '\n';
                            }
                            break;
                        case eGmCommandType.GMScene:
                            if (!CheckCommandScene(strs))
                            {
                                ret += command + '\n';
                            }
                            break;
                    }
                }
                else
                {
                    ret += command + '\n';
                }
            }
            return ret;
        }

        private static bool CheckCommandScene(string[] strs)
        {
            if (strs.Length < 1)
            {
                return false;
            }
            if (String.Compare(strs[0], "!!ReloadTable", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (strs.Length < 2)
                {
                    return false;
                }
                return true;
            }

            var nIndex = 0;
            var IntData = new List<long>();
            foreach (var s in strs)
            {
                if (nIndex != 0)
                {
                    long TempInt;
                    if (!long.TryParse(s, out TempInt))
                    {
                        return false;
                    }
                    IntData.Add(TempInt);
                }
                nIndex++;
            }

            if (String.Compare(strs[0], "!!Goto", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count != 3)
                {
                    return false;
                }
            }
            if (String.Compare(strs[0], "!!EnterScene", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count != 1)
                {
                    return false;
                }
            }
            else if (String.Compare(strs[0], "!!LookScene", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count != 0)
                {
                    return false;
                }
            }
            else if (String.Compare(strs[0], "!!SpeedSet", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count != 1)
                {
                    return false;
                }
            }
            else if (String.Compare(strs[0], "!!HpSet", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count != 1)
                {
                    return false;
                }
            }
            else if (String.Compare(strs[0], "!!AddBuff", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count != 2)
                {
                    return false;
                }
            }
            else if (String.Compare(strs[0], "!!DelBuff", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count != 1)
                {
                    return false;
                }
            }
            else if (String.Compare(strs[0], "!!MpSet", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count != 1)
                {
                    return false;
                }
            }
            else if (String.Compare(strs[0], "!!CreateNpc", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count < 1 || IntData.Count > 3)
                {
                    return false;
                }
            }
            else if (String.Compare(strs[0], "!!DumpScene", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count != 0 && IntData.Count != 1)
                {
                    return false;
                }
            }
            else if (String.Compare(strs[0], "!!ShowObjInfo", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count < 1)
                {
                    return false;
                }
            }
            else if (String.Compare(strs[0], "!!ResetObj", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count < 1)
                {
                    return false;
                }
            }
            return true;
        }

        public static void Init()
        {
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
            ResetGMCommand();
        }

        private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v.tableName == "GMCommand")
            {
                ResetGMCommand();
            }
        }

        private static void ResetGMCommand()
        {
            Gms.Clear();
            Table.ForeachGMCommand(r =>
            {
                Gms.Add("!!" + r.Command, (eGmCommandType) r.Type);
                return true;
            });
        }

        public static List<string> SplitCommands(IEnumerable<string> commands, eGmCommandType type)
        {
            var c1 = new List<string>();
            foreach (var command in commands)
            {
                var cs = command.Split(',');
                eGmCommandType t;
                if (!Gms.TryGetValue(cs[0], out t))
                {
                    continue;
                }
                if (t == type)
                {
                    c1.Add(command);
                }
            }
            return c1;
        }
    }
}