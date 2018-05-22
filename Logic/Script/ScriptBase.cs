#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DataTable;
using NLog;

#endregion

namespace Logic
{
    public class Script
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public Dictionary<int, string> CodeFile = new Dictionary<int, string>();
        public string funName = "";
        public object[] mArgs;
        public int NowLine = 1;
        public int nThisLine;
        public int ScriptId;
        public Dictionary<string, string> Type = new Dictionary<string, string>(); //变量类型
        public Dictionary<string, object> Variable = new Dictionary<string, object>(); //变量值
        //尝试网List<string>内增加数据
        public static void AddList(List<string> temp, string str)
        {
            if (str.Length != 0)
            {
                temp.Add(str);
            }
        }

        //解析(1=find function,2=do)
        public int AnalyzeDo(int nBegin, int nEnd)
        {
            var nState = 0;
            for (var i = nBegin; i <= nEnd; ++i)
            {
                nThisLine = i;
                var result = AnalyzeLine(nState, i);
                //switch (result)
                //{
                //    case 0:
                //        {

                //        }
                //        break;
                //    case 1:
                //        {//找到了
                //            nState = 2;
                //        }
                //        break;
                //    case -1:
                //        {
                //            return -1;
                //        }
                //        break;
                //}
            }

            return -1;
        }

        //查找函数开始点
        private int AnalyzeFindFunc(int nLine)
        {
            var tempStr = CodeFile[nLine];
            var nLenth = tempStr.Length;
            if (nLenth == 0)
            {
                return 0;
            }
            var Code = AnalyzeStr(tempStr);
            if (Code.Count > 2)
            {
                if (String.Compare("function", Code[0], StringComparison.OrdinalIgnoreCase) == 0 &&
                    String.Compare(funName, Code[1], StringComparison.OrdinalIgnoreCase) == 0)
                {
//匹配上了
                    var nIndex = GetStringIndex(Code, "(");
                    if (nIndex != -1)
                    {
                        for (var i = 0; i != mArgs.Length; ++i)
                        {
                            CreateVariable(Code[nIndex + i*3 + 2], Code[nIndex + i*3 + 1], mArgs[i]);
                        }
                        //if (Code.Count > nIndex + 2)
                        //{
                        //    CreateVariable(Code[nIndex + 2], Code[nIndex + 1], mArgs[0]);
                        //}
                    }
                    return 1;
                }
            }
            return 0;
        }

        //按一个行号{}查找结束点
        private int AnalyzeFindScope(ref int nLine, string ScopeBefore, string ScopeEnd)
        {
            var IsFind = false;
            var nScopeIndex = 0;
            var nNowLine = nLine;
            while (true)
            {
                string tempStr;
                if (!CodeFile.TryGetValue(nNowLine, out tempStr))
                {
                    return 0;
                }
                var nLenth = tempStr.Length;
                if (nLenth == 0)
                {
                    nNowLine++;
                    continue;
                }
                var Code = AnalyzeStr(tempStr);
                foreach (var s in Code)
                {
                    if (String.Compare(ScopeBefore, s, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (!IsFind)
                        {
                            IsFind = true;
                            nLine = nNowLine;
                        }
                        nScopeIndex++;
                    }
                    else if (String.Compare(ScopeEnd, s, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        nScopeIndex--;
                    }
                }
                if (IsFind && nScopeIndex == 0)
                {
                    if (Code.Count == 1)
                    {
                        return nNowLine - 1;
                    }
                    return nNowLine;
                }
                nNowLine++;
            }
        }

        public int AnalyzeLine(int nType, int nLine)
        {
            var tempStr = CodeFile[nLine];
            var nLenth = tempStr.Length;
            if (nLenth == 0)
            {
                return 0;
            }
            var Code = AnalyzeStr(tempStr);
            if (Code.Count < 1)
            {
                return 0;
            }
            switch (nType)
            {
                case 0:
                {
//开始状态
                    if (Variable.ContainsKey(Code[0]) && Code[1] == ".")
                    {
                        var nIndexB = GetStringIndex(Code, "(");
                        var nIndexE = GetStringIndex(Code, ")");
                        var nCount = 0;
                        for (var i = nIndexB + 1; i < nIndexE; i = i + 2)
                        {
                            nCount++;
                        }
                        var ssArgs = new object[nCount];
                        nCount = 0;
                        for (var i = nIndexB + 1; i < nIndexE; i = i + 2)
                        {
                            ssArgs[nCount] = Code[nIndexB + 1 + nCount*2];
                            nCount++;
                        }
                        object result = null;
                        if (nCount == 0)
                        {
                            result = DoClassFunction(Type[Code[0]], Code[2], Variable[Code[0]]);
                        }
                        else
                        {
                            result = DoClassFunction(Type[Code[0]], Code[2], Variable[Code[0]], ssArgs);
                        }
                        if (result != null)
                        {
                        }
                    }
                }
                    break;
                case 1:
                {
//
                }
                    break;
                case -1:
                {
                    return -1;
                }
            }

            return -1;
        }

        //解析一个字符串
        private static List<string> AnalyzeStr(string tempStr)
        {
            var Code = new List<string>();
            var nLenth = tempStr.Length;
            var NowStr = "";
            for (var j = 0; j != nLenth; ++j)
            {
                //当前的值
                if (NowStr == "(")
                {
                    AddList(Code, NowStr);
                    NowStr = "";
                }
                else if (NowStr == ")")
                {
                    AddList(Code, NowStr);
                    NowStr = "";
                }
                else if (NowStr == "[")
                {
                    AddList(Code, NowStr);
                    NowStr = "";
                }
                else if (NowStr == "]")
                {
                    AddList(Code, NowStr);
                    NowStr = "";
                }
                else if (NowStr == "{")
                {
                    AddList(Code, NowStr);
                    NowStr = "";
                }
                else if (NowStr == "}")
                {
                    AddList(Code, NowStr);
                    NowStr = "";
                }
                else if (NowStr == ".")
                {
                    AddList(Code, NowStr);
                    NowStr = "";
                }
                else if (NowStr == ",")
                {
                    AddList(Code, NowStr);
                    NowStr = "";
                }
                //下个值
                var tempChar = tempStr.Substring(j, 1);
                if (tempChar == "\t" || tempChar == " ")
                {
                    AddList(Code, NowStr);
                    NowStr = "";
                }
                else if (tempChar == "(")
                {
                    AddList(Code, NowStr);
                    NowStr = "(";
                }
                else if (tempChar == ")")
                {
                    AddList(Code, NowStr);
                    NowStr = ")";
                }
                else if (tempChar == "[")
                {
                    AddList(Code, NowStr);
                    NowStr = "[";
                }
                else if (tempChar == "]")
                {
                    AddList(Code, NowStr);
                    NowStr = "]";
                }
                else if (tempChar == "{")
                {
                    AddList(Code, NowStr);
                    NowStr = "{";
                }
                else if (tempChar == "}")
                {
                    AddList(Code, NowStr);
                    NowStr = "}";
                }
                else if (tempChar == ".")
                {
                    AddList(Code, NowStr);
                    NowStr = ".";
                }
                else if (tempChar == ",")
                {
                    AddList(Code, NowStr);
                    NowStr = ",";
                }
                else
                {
                    NowStr = NowStr + tempChar;
                }
            }
            AddList(Code, NowStr);
            return Code;
        }

        //创建变量
        private void CreateVariable(string Name, string typeName, object Value)
        {
            if (Variable.ContainsKey(Name) || Type.ContainsKey(Name))
            {
                Logger.Warn("CreateVariable is Haved! ScriptId={0},funName={1} VariableName={2} typeName={3}", ScriptId,
                    funName, Name, typeName);
            }
            Variable[Name] = Value;
            Type[Name] = GetTypeFull(typeName);
        }

        //执行一个类的某个方法
        public object DoClassFunction(string ClassName, string FuncName, object ClassExample, params object[] args)
        {
            var type = GetType(ClassName);
            if (type == null)
            {
                return null;
            }
            try
            {
                var method = GetMethod(type, FuncName);
                if (method == null)
                {
                    return null;
                }

                var paramlList = GetParamType(method.ToString());
                var paramMin = 0;
                for (var i = 0; i < paramlList.Count; i++)
                {
                    var ttttt = method.GetParameters()[i].Attributes;
                    if (ttttt == ParameterAttributes.HasDefault)
                    {
                    }
                    else
                    {
                        paramMin++;
                    }
                }
                var paramMax = paramlList.Count;
                var InputParamCount = args.Length;
                var NeedParam = Math.Min(InputParamCount, paramMax);
                if (InputParamCount < paramMin)
                {
                    Logger.Warn(
                        "DoClassFunction param not enough!  ScriptId={0} funName={1} Line={4} ClassName={2} funcName={3} ",
                        ScriptId, funName, ClassName, FuncName, nThisLine);
                    return null;
                }
                if (InputParamCount > paramMax)
                {
                    Logger.Warn(
                        "DoClassFunction param Overflow!  ScriptId={0} funName={1} Line={4} ClassName={2} funcName={3} ",
                        ScriptId, funName, ClassName, FuncName, nThisLine);
                    return null;
                }
                var ssArgs = new object[NeedParam];
                var nIndex = 0;
                foreach (var o in args)
                {
                    ssArgs[nIndex] = GetVariable(args[nIndex].ToString(), paramlList[nIndex]);
                    nIndex++;
                }

                Logger.Info("DoClassFunction !  ScriptId={0} funName={1} ClassName={2} funcName={3}", ScriptId, funName,
                    ClassName, FuncName);
                var returnType = method.ReturnType.Name;
                if (String.Compare("Void", returnType, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    method.Invoke((CharacterController) ClassExample, ssArgs);
                    return null;
                }
                var result = method.Invoke((CharacterController) ClassExample, ssArgs);
                return result;
            }
            catch (AmbiguousMatchException ex)
            {
                Logger.Warn(
                    "ambiguousMatchException GetMethod is faild!  ScriptId=" + ScriptId + " funName=" + funName +
                    " funcName=" + FuncName, ex);
                return null;
            }
            catch (TargetException ex)
            {
                Logger.Warn(
                    "targetException GetMethod is faild!  ScriptId=" + ScriptId + " funName=" + funName + " funcName=" +
                    FuncName, ex);
                return null;
            }
            catch (ArgumentException ex)
            {
                Logger.Warn(
                    "argumentException GetMethod is faild!  ScriptId=" + ScriptId + " funName=" + funName + " funcName=" +
                    FuncName, ex);
                return null;
            }
            catch (TargetParameterCountException ex)
            {
                Logger.Warn(
                    "targetParameterCountException GetMethod is faild!  ScriptId=" + ScriptId + " funName=" + funName +
                    " funcName=" + FuncName, ex);
                return null;
            }
            catch (MethodAccessException ex)
            {
                Logger.Warn(
                    "methodAccessException GetMethod is faild!  ScriptId=" + ScriptId + " funName=" + funName +
                    " funcName=" + FuncName, ex);
                return null;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn(
                    "invalidOperationException GetMethod is faild!  ScriptId=" + ScriptId + " funName=" + funName +
                    " funcName=" + FuncName, ex);
                return null;
            }
            catch (NotSupportedException ex)
            {
                Logger.Warn(
                    "notSupportedException GetMethod is faild!  ScriptId=" + ScriptId + " funName=" + funName +
                    " funcName=" + FuncName, ex);
                return null;
            }
            catch (TargetInvocationException ex)
            {
                Logger.Warn(
                    "targetInvocationException GetMethod is faild!  ScriptId=" + ScriptId + " funName=" + funName +
                    " funcName=" + FuncName, ex);
                return null;
            }
        }

        //执行
        public void DoScript(int nScriptId, string szfunName, params object[] args)
        {
            //DoClassFunction("Logic.CharacterController", "AddExData", args[0], new[] { args[1], args[2] });

            var tbScript = Table.GetScript(nScriptId);
            ScriptId = nScriptId;
            funName = szfunName;
            mArgs = args;
            TextReader tr = null;
            CodeFile.Clear();
            try
            {
                tr = new StreamReader(GetLoadPath(tbScript.Path), Encoding.Default);
                var LineIndex = 1;
                var TempLine = tr.ReadLine();
                while (TempLine != null)
                {
                    CodeFile[LineIndex] = TempLine.Trim();
                    TempLine = tr.ReadLine();
                    LineIndex++;
                }
                //查询开始行
                var BeginLine = 0;
                for (var l = 1; l != LineIndex - 1; ++l)
                {
                    var result = AnalyzeFindFunc(l);
                    if (result == 1) //
                    {
                        BeginLine = l;
                        break;
                    }
                }
                if (BeginLine <= 0)
                {
                    Logger.Warn("DoScript is not find function ScriptId={0},funName={1}", nScriptId, szfunName);
                }

                //查询结束行
                var EndLine = AnalyzeFindScope(ref BeginLine, "{", "}");

                if (EndLine <= BeginLine)
                {
                    Logger.Warn("DoScript is not find function ScriptId={0},funName={1}", nScriptId, szfunName);
                }
                AnalyzeDo(BeginLine + 1, EndLine);
            }
            catch (Exception ex)
            {
                //加入表格加载错误提示
                Logger.Error("Load {0} Error!!", tbScript.Path);
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

        public static string GetLoadPath(string localName)
        {
            return "../Scripts/" + localName;
        }

        //变量类型获得
        public MethodInfo GetMethod(Type type, string funcName)
        {
            if (type == null)
            {
                return null;
            }
            try
            {
                var method = type.GetMethod(funcName);
                return method;
            }
            catch (ArgumentNullException ex)
            {
                Logger.Warn(ex,
                    "argumentNullException GetMethod is faild!  ScriptId={0} funcName={1}", ScriptId, funcName);
            }
            return null;
        }

        //获得一个method的参数类型
        private List<string> GetParamType(string MethodStr)
        {
            var Code = AnalyzeStr(MethodStr);
            var nIndexB = GetStringIndex(Code, "(");
            var nIndexE = GetStringIndex(Code, ")");
            var paramlList = new List<string>();
            for (var i = nIndexB + 1; i < nIndexE; i = i + 2)
            {
                paramlList.Add(Code[i]);
            }
            return paramlList;
        }

        private static int GetStringIndex(List<string> Code, string CompareStr)
        {
            var nIndex = 0;
            foreach (var s in Code)
            {
                if (String.Compare(s, CompareStr, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return nIndex;
                }
                nIndex++;
            }
            return -1;
        }

        //变量类型获得
        public Type GetType(string typeName)
        {
            var type = Assembly.GetCallingAssembly().GetType(typeName);
            if (type == null)
            {
                type = Assembly.GetExecutingAssembly().GetType(typeName);
                if (type == null)
                {
                    Logger.Warn("GetType is faild! ScriptId={0} funName={1} typeName={2}", ScriptId, funName, typeName);
                    type = System.Type.GetType(typeName);
                }
            }
            return type;
        }

        //
        private string GetTypeFull(string typeName)
        {
            if (String.Compare("int", typeName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "System.Int32";
            }
            if (String.Compare("int32", typeName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "System.Int32";
            }
            if (String.Compare("CharacterController", typeName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Logic.CharacterController";
            }
            return "";
        }

        //获得变量的值
        private object GetVariable(string Name, string SysType)
        {
            var temp = GetVariable(Name);
            if (temp != null)
            {
                return temp;
            }
            if (String.Compare("int", SysType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return Table_Tamplet.Convert_Int(Name);
            }
            if (String.Compare("int32", SysType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                int tempInt;
                if (Int32.TryParse(Name, out tempInt))
                {
                    return tempInt;
                }
                Logger.Error("GetVariable Not Find! ScriptId={0} funName={1} Line={2} Variable={3}", ScriptId, funName,
                    nThisLine, Name);
                return 0;
            }
            if (String.Compare("float", SysType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return Table_Tamplet.Convert_Double(Name);
            }
            if (String.Compare("double", SysType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return Table_Tamplet.Convert_Double(Name);
            }
            return null;
        }

        //获得变量的值
        private object GetVariable(string Name)
        {
            if (Variable.ContainsKey(Name))
            {
                return Variable[Name];
            }
            //Logger.Warn("GetVariable is Faild! ScriptId={0},funName={1} VariableName={2}", ScriptId, funName, Name);
            return null;
        }

        //读
        public static void Read()
        {
        }
    }
}