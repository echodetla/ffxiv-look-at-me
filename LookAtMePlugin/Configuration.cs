using Dalamud.Configuration;
using System;

namespace LookAtMePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsEnabled { get; set; } = true;

    public bool MonitorPublicChat { get; set; } = true;
    public bool MonitorParty { get; set; } = true;
    public bool MonitorFc { get; set; } = false;
    public bool MonitorCwl1 { get; set; } = false;
    public bool MonitorCwl2 { get; set; } = false;
    public bool MonitorCwl3 { get; set; } = false;
    public bool MonitorCwl4 { get; set; } = false;
    public bool MonitorCwl5 { get; set; } = false;
    public bool MonitorCwl6 { get; set; } = false;
    public bool MonitorCwl7 { get; set; } = false;
    public bool MonitorCwl8 { get; set; } = false;

    // The below exist just to make saving less cumbersome
    public void Save()
    {
        LookAtMePlugin.PluginInterface.SavePluginConfig(this);
    }
}
