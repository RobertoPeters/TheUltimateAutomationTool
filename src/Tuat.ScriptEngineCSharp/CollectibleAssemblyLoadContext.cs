using System.Reflection;
using System.Runtime.Loader;

namespace Tuat.ScriptEngineCSharp;

public class CollectibleAssemblyLoadContext : AssemblyLoadContext
{
    public CollectibleAssemblyLoadContext() : base(isCollectible: true)
    {
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        return base.Load(assemblyName);
    }
}
