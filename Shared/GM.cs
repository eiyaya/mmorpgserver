#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataTable;
using NLog;

#endregion

namespace Shared
{


	public static class GMCommandLevel
	{
		/// <summary>
		/// GMCommandLevel，对应的枚举type
		/// </summary>
		public enum GMCommandLevelType
		{
			NOALLOW = 0,    // 都不允许      
			GMALLOW = 1,    // 只有GM账号允许
			ALLOW = 2,      // 所有账号都允许
		}

		public static GMCommandLevelType GetCommandLevel()
		{
			var GmConfig = Table.GetServerConfig(4);
			if (null != GmConfig)
			{
				if (GmConfig.Value.Equals("2"))
				{
					return GMCommandLevelType.ALLOW;
				}
				else if (GmConfig.Value.Equals("1"))
				{
					return GMCommandLevelType.GMALLOW;
				}
			}
			return GMCommandLevelType.NOALLOW;
		}
	}


    public interface GmCommand
    {
        List<int> ArgsInt { get; set; }
        string Name { get; }
        int ExecuteAdd(ulong characterId);
        int ExecuteDel(ulong characterId);
        int ExecuteGet(ulong characterId);
        int ExecuteSet(ulong characterId);
        object GetResult();
        //增加
        bool ValidateAdd(params string[] args);
        //删除
        bool ValidateDel(params string[] args);
        //查询
        bool ValidateGet(params string[] args);
        //改变
        bool ValidateSet(params string[] args);
    }

    public class GmManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, GmSubCommand> mCommands = new Dictionary<string, GmSubCommand>();

        public object DoCommand(string command, ulong characterId)
        {
            if (!command.StartsWith("!!"))
            {
                return null;
            }

            command = command.Substring(2);
            var strs = command.Split(',');
            if (strs.Length < 1)
            {
                return null;
            }

            var target = strs[0].Split(':');
            if (target.Length < 1)
            {
                return null;
            }
            GmSubCommand c;
            if (mCommands.TryGetValue(target[0], out c))
            {
                try
                {
                    var param = strs.Skip(1).ToArray();
                    if (c.validate(param))
                    {
                        if (target.Length >= 2)
                        {
                            ulong Tempulong;
                            if (ulong.TryParse(target[1], out Tempulong))
                            {
                                characterId = Tempulong;
                            }
                        }
                        var result = c.Execute(characterId);
                        if (result == (int) ErrorCodes.OK)
                        {
                            if (c.GetResult == null)
                            {
                                return result;
                            }
                            return c.GetResult();
                        }
                        return result;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Execute GM command error.");
                }
            }
            return null;
        }

        public void RegisterCommand(GmCommand command)
        {
            mCommands.Add("Add" + command.Name,
                new GmSubCommand {Execute = command.ExecuteAdd, validate = command.ValidateAdd});
            mCommands.Add("Del" + command.Name,
                new GmSubCommand {Execute = command.ExecuteDel, validate = command.ValidateDel});
            mCommands.Add("Get" + command.Name,
                new GmSubCommand
                {
                    Execute = command.ExecuteGet,
                    validate = command.ValidateGet,
                    GetResult = command.GetResult
                });
            mCommands.Add("Set" + command.Name,
                new GmSubCommand {Execute = command.ExecuteSet, validate = command.ValidateSet});
        }

        private class GmSubCommand
        {
            public Executer Execute;
            public ResulteGetter GetResult;
            public Validater validate;

            public delegate bool Validater(params string[] args);

            public delegate int Executer(ulong characterId);

            public delegate object ResulteGetter();
        }
    }
}