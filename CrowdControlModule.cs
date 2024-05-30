using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.CrowdControl;

public class CrowdControlModule : EverestModule
{
    public static CrowdControlModule Instance = null!;

    public override Type SettingsType => typeof(CrowdControlSettings);
    public static CrowdControlSettings Settings => (CrowdControlSettings) Instance._Settings;

    public CrowdControlModule() => Instance = this;

    static CrowdControlModule() => AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

    private static Assembly? ResolveAssembly(object sender, ResolveEventArgs args)
    {
        try
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            string name = args.Name.Split(',').First();
            string path = Path.Combine(Path.GetDirectoryName(thisAssembly.Location)!, $"CrowdControl.{name}.dll");
            return File.Exists(path) ? Assembly.LoadFile(path) : null;
        }
        catch { return null; }
    }

    public override void Load() { }

    public override void Unload() { }

    public override void Initialize()
    {
        if (Settings.Enabled)
        {
            CrowdControlHelper.Add();
        }
    }
}