using YamlDotNet.Serialization;

namespace Celeste.Mod.CrowdControl;

public class CrowdControlSettings : EverestModuleSettings
{
    [SettingIgnore]
    [YamlMember(Alias = "Enabled")]
    protected bool _Enabled { get; set; } = false;
    public bool Enabled
    {
        get => _Enabled;
        set
        {
            if (_Enabled == value) { return; }

            if (value) { CrowdControlHelper.Add(); }
            else { CrowdControlHelper.Remove(); }

            _Enabled = value;
        }
    }
}