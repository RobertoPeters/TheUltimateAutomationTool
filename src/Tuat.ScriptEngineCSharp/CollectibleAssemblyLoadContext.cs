using System.Reflection;
using System.Runtime.Loader;

namespace Tuat.ScriptEngineCSharp;

public class CollectibleAssemblyLoadContext : AssemblyLoadContext
{
    public CollectibleAssemblyLoadContext() : base(isCollectible: true)
    {
    }

    //protected override Assembly? Load(AssemblyName name)
    //{
    //    if (name.Name == "Tuat.ScriptEngineCSharp")
    //    {
    //        return LoadFromAssemblyPath(typeof(SystemMethods).Assembly.Location);
    //    }
    //    else if (System.IO.File.Exists(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, $"{name.Name}.dll")))
    //    {
    //        return LoadFromAssemblyPath(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, $"{name.Name}.dll"));
    //    }

    //    return null;
    //}
}
