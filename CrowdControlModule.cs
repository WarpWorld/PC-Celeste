using System;

namespace Celeste.Mod.CrowdControl
{
    public class CrowdControlModule : EverestModule
    {
        public static CrowdControlModule Instance;

        public override Type SettingsType => typeof(CrowdControlSettings);
        public static CrowdControlSettings Settings => (CrowdControlSettings) Instance._Settings;

        public CrowdControlModule() => Instance = this;

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
}
