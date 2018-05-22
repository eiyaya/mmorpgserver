#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using NLog;
using Enumerable = System.Linq.Enumerable;

#endregion

namespace Shared
{
    public interface IUpdatable
    {
        void SetImpl(object impl);
    }

    public class Updateable : Attribute
    {
        public Updateable(string whoConfirmed)
        {
            
        }
    }

    public class UpdateManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, string> mFileMap = new Dictionary<string, string>();
        private readonly Dictionary<string, ImplInfo> mImpls = new Dictionary<string, ImplInfo>();
        private readonly List<IUpdatable> mInstances = new List<IUpdatable>();
        private string mServerName;
        private readonly List<StaticImplInfo> mStaticInstances = new List<StaticImplInfo>();
        private static object mLock = new object();
        private readonly HashSet<Type> mHotfixed = new HashSet<Type>();

#if DEBUG
        private static HashSet<Type> s_CheckedTypes = new HashSet<Type>();
#endif

        public Type GetNewType(string t)
        {
            lock (mLock)
            {
                ImplInfo impl;
                if (mImpls.TryGetValue(t, out impl))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(impl.PluginAssemblyName))
                        {
                            var ass = Assembly.LoadFile(Path.GetFullPath(mFileMap[impl.PluginAssemblyName]));
                            Logger.Info("Load type from new Plugin {0}, type {1}", mFileMap[impl.PluginAssemblyName],
                                impl.ImplFullTypeName);
                            return ass.GetType(impl.ImplFullTypeName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex,
                            "load assebly error plugin path: {0} default path: {1}.",
                            Path.GetFullPath(Path.Combine("../Plugin/", mFileMap[impl.PluginAssemblyName])),
                            impl.DefaultAssemblyName);
                    }
                }

                Logger.Info("Load type from default assembly type {0}", t);
                return Assembly.GetCallingAssembly().GetType(t);
            }
        }

        private Type GetNewType(ImplInfo impl)
        {
            lock (mLock)
            {
                Assembly ass = null;
                try
                {
                    if (!string.IsNullOrEmpty(impl.PluginAssemblyName))
                    {
                        var file = Path.GetFullPath(mFileMap[impl.PluginAssemblyName]);
                        ass = Assembly.LoadFile(file);
                        Logger.Info("Load type from new Plugin {0}, type {1}", file, impl.ImplFullTypeName);
                    }
                    else
                    {
                        ass = Assembly.LoadFile(impl.DefaultAssemblyName);
                        Logger.Info("Can not load type from Plugin, type {0}", impl.ImplFullTypeName);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex,
                        "load assebly error plugin path: {0} default path: {1}",
                        Path.GetFullPath(Path.Combine("../Plugin/", mFileMap[impl.PluginAssemblyName])),
                        impl.DefaultAssemblyName);
                }

                if (ass != null)
                {
                    Logger.Info("Load ass, [{0}]", ass.ToString());
                    return ass.GetType(impl.ImplFullTypeName);
                }

                Logger.Error("Can not load ass, {0}",
                    Path.GetFullPath(mFileMap[impl.PluginAssemblyName]));

                return null;
            }
        }

        public void Init(string serverName)
        {
            lock (mLock)
            {
                try
                {
                    if (!Directory.Exists("../Plugin/"))
                    {
                        try
                        {
                            Directory.CreateDirectory("../Plugin/");
                        }
                        catch
                        {
                            // 如果删除不掉，或者找不到文件夹，那就算了，可能其他进程或者线程已经给删了
                        }
                    }

                    if (!File.Exists("../Plugin/Plugin.config"))
                    {
                        return;
                    }

                    mServerName = serverName;
                    var tempFile = Path.GetTempFileName();
                    File.Copy("../Plugin/Plugin.config", tempFile, true);
                    var lines = File.ReadAllLines(tempFile);
                    File.Delete(tempFile);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("//"))
                        {
                            continue;
                        }

                        var strs = line.Trim().Split(' ', '\t', ',');

                        if (strs.Length != 3)
                        {
                            continue;
                        }

                        ImplInfo info;
                        if (mImpls.TryGetValue(strs[0], out info))
                        {
                            info.PluginAssemblyName = strs[1].Trim();
                            info.ImplFullTypeName = strs[2].Trim();
                        }
                        else
                        {
                            mImpls[strs[0]] = new ImplInfo
                            {
                                PluginAssemblyName = strs[1].Trim(),
                                ImplFullTypeName = strs[2].Trim()
                            };
                        }
                    }

                    var files = Directory.GetFiles("../Plugin/", "*.dll", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        var fileName = Path.GetTempFileName();
                        File.Copy(file, fileName, true);
                        mFileMap[Path.GetFileName(file)] = fileName;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Init update manager error {0}", serverName);
                    throw;
                }
            }
        }

        public void InitImpl<T>(T t, Type defaultImpl) where T : IUpdatable
        {
            lock (mLock)
            {
                var ttype = t.GetType();
                ImplInfo impl;
                if (!mImpls.TryGetValue(ttype.FullName, out impl))
                {
                    impl = new ImplInfo
                    {
                        ImplFullTypeName = defaultImpl.FullName,
                        DefaultAssemblyName = defaultImpl.Assembly.Location
                    };
                    mImpls.Add(ttype.FullName, impl);
                    Logger.Info("Instance or type " + t.GetType() + " can update.");
                }

                var type = GetNewType(impl);

#if DEBUG
                if (!s_CheckedTypes.Contains(type))
                {
                    TypeIsStateless(type);
                    s_CheckedTypes.Add(type);
                }
#endif

                try
                {
                    if (!mHotfixed.Contains(type))
                    {
                        var v = Activator.CreateInstance(type);
                        CallHotfixStart(type, v);
                        t.SetImpl(v);
                        mHotfixed.Add(type);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "CreateInstance for type {0} failed.", type.FullName);
                }

                mInstances.Add(t);
            }
        }

        private bool TypeIsStateless(Type type)
        {
            lock (mLock)
            {
                var fields = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var info in fields)
                {
                    if (info.MemberType == MemberTypes.Field)
                    {
                        // maked
                        if (Enumerable.Any(info.CustomAttributes,
                            attribute => attribute.AttributeType == typeof(Updateable)))
                            continue;

                        var field = type.GetField(info.Name,
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (field.FieldType == typeof(Logger))
                        {
                            continue;
                        }

                        System.Diagnostics.Trace.Assert(false, string.Format("{0} has state: {1}", type, info.Name));
                    }
                }

                var statics = type.GetMembers(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var info in statics)
                {
                    // genrated
                    if (Enumerable.Any(info.CustomAttributes,
                        attribute => attribute.AttributeType ==
                                     typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)))
                        continue;

                    // maked
                    if (Enumerable.Any(info.CustomAttributes,
                        attribute => attribute.AttributeType == typeof(Updateable)))
                        continue;

                    if (info.MemberType == MemberTypes.Field)
                    {
                        var field = type.GetField(info.Name,
                            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                        if (field.FieldType == typeof(Logger))
                        {
                            continue;
                        }

                        System.Diagnostics.Trace.Assert(false,
                            string.Format("{0} has static state: {1}", type, info.Name));
                    }
                }

                return true;
            }
        }

        public void InitStaticImpl(Type t, Type defaultImpl, Action<object> act)
        {
            lock (mLock)
            {
                ImplInfo impl;
                if (!mImpls.TryGetValue(t.FullName, out impl))
                {
                    impl = new ImplInfo
                    {
                        ImplFullTypeName = defaultImpl.FullName,
                        DefaultAssemblyName = defaultImpl.Assembly.Location
                    };
                    mImpls.Add(t.FullName, impl);
                    Logger.Info("Static instance or type " + t.GetType() + " can update.");
                }

                var type = GetNewType(impl);

#if DEBUG
                if (!s_CheckedTypes.Contains(type))
                {
                    TypeIsStateless(type);
                    s_CheckedTypes.Add(type);
                }
#endif
                try
                {
                    if (!mHotfixed.Contains(type))
                    {
                        var v = Activator.CreateInstance(type);
                        CallHotfixStart(type, v);
                        act(v);
                        mHotfixed.Add(type);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "CreateInstance static for type {0} failed.", type.FullName);
                }

                mStaticInstances.Add(new StaticImplInfo
                {
                    WrapperType = t,
                    AssignOperator = act
                });
            }
        }

        private static void CallHotfixStart(Type type, object v)
        {
            var method = type.GetMethod("OnHotfixStart",
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method != null)
            {
                method.Invoke(v, new object[] { });
            }
        }

        public void Stop()
        {
            lock (mLock)
            {
                mImpls.Clear();
                mInstances.Clear();
                mStaticInstances.Clear();
            }
        }

        public void Update()
        {
            lock (mLock)
            {
                Logger.Info("Start update server {0} - pid:{1}", mServerName, Thread.CurrentThread.ManagedThreadId);

                Init(mServerName);

                Logger.Info("Start update server {0} - pid:{1}", mServerName, Thread.CurrentThread.ManagedThreadId);

                foreach (var inst in mStaticInstances)
                {
                    var impl = mImpls[inst.WrapperType.FullName];
                    var type = GetNewType(impl);
                    if (type == null)
                    {
                        Logger.Error("Load static type error. {0} {1} {2}", inst.WrapperType.FullName,
                            impl.PluginAssemblyName, impl.ImplFullTypeName);
                        continue;
                    }

                    Logger.Info("Inst {0} has updated to {1} on {2} - pid:{3}", inst.WrapperType.GetType(), type,
                        mServerName, Thread.CurrentThread.ManagedThreadId);

                    var v = Activator.CreateInstance(type);

                    Logger.Info("Create instance of type {0} {1}", inst.WrapperType.GetType(), v);

                    if (!mHotfixed.Contains(type))
                    {
                        CallHotfixStart(type, v);
                        mHotfixed.Add(type);
                    }

                    inst.AssignOperator(v);
                }

                foreach (var inst in mInstances)
                {
                    var impl = mImpls[inst.GetType().FullName];
                    var type = GetNewType(impl);
                    if (type == null)
                    {
                        Logger.Error("Load inst type error. {0} {1} {2}", inst.GetType().FullName,
                            impl.PluginAssemblyName,
                            impl.ImplFullTypeName);
                        continue;
                    }

                    Logger.Info("Inst {0} has updated to {1} on {2} - pid:{3}", inst.GetType(), type, mServerName,
                        Thread.CurrentThread.ManagedThreadId);

                    var v = Activator.CreateInstance(type);

                    Logger.Info("Create instance of type {0} {1}", inst.GetType(), v);

                    if (!mHotfixed.Contains(type))
                    {
                        CallHotfixStart(type, v);
                        mHotfixed.Add(type);
                    }

                    inst.SetImpl(v);
                }
            }
        }

        private class ImplInfo
        {
            public string DefaultAssemblyName;
            public string ImplFullTypeName;
            public string PluginAssemblyName;
        }

        private class StaticImplInfo
        {
            public Action<object> AssignOperator;
            public Type WrapperType;
        }
    }
}