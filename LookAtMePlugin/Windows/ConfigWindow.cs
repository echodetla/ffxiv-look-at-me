using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace LookAtMePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(LookAtMePlugin lookAtMePlugin) : base("Look at me Configuration###LookAtConfig")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(200, 300);
        SizeCondition = ImGuiCond.Always;

        configuration = lookAtMePlugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var isEnabled = configuration.IsEnabled;
        if (ImGui.Checkbox("Enable Look at me", ref isEnabled))
        {
            configuration.IsEnabled = isEnabled;
            configuration.Save();
        }
        
        var monitorPublicChat = configuration.MonitorPublicChat;
        if (ImGui.Checkbox("Monitor the public chats", ref monitorPublicChat))
        {
            configuration.MonitorPublicChat = monitorPublicChat;
            configuration.Save();
        }
    
        var monitorParty = configuration.MonitorParty;
        if (ImGui.Checkbox("Monitor the party", ref monitorParty))
        {
            configuration.MonitorParty = monitorParty;
            configuration.Save();
        }
    
        var monitorFc = configuration.MonitorFc;
        if (ImGui.Checkbox("Monitor FC", ref monitorFc))
        {
            configuration.MonitorFc = monitorFc;
            configuration.Save();
        }
    
        var monitorCwl1 = configuration.MonitorCwl1;
        if (ImGui.Checkbox("Monitor CWL 1", ref monitorCwl1))
        {
            configuration.MonitorCwl1 = monitorCwl1;
            configuration.Save();
        }
    
        var monitorCwl2 = configuration.MonitorCwl2;
        if (ImGui.Checkbox("Monitor CWL 2", ref monitorCwl2))
        {
            configuration.MonitorCwl2 = monitorCwl2;
            configuration.Save();
        }
    
        var monitorCwl3 = configuration.MonitorCwl3;
        if (ImGui.Checkbox("Monitor CWL 3", ref monitorCwl3))
        {
            configuration.MonitorCwl3 = monitorCwl3;
            configuration.Save();
        }
    
        var monitorCwl4 = configuration.MonitorCwl4;
        if (ImGui.Checkbox("Monitor CWL 4", ref monitorCwl4))
        {
            configuration.MonitorCwl4 = monitorCwl4;
            configuration.Save();
        }
    
        var monitorCwl5 = configuration.MonitorCwl5;
        if (ImGui.Checkbox("Monitor CWL 5", ref monitorCwl5))
        {
            configuration.MonitorCwl5 = monitorCwl5;
            configuration.Save();
        }
    
        var monitorCwl6 = configuration.MonitorCwl6;
        if (ImGui.Checkbox("Monitor CWL 6", ref monitorCwl6))
        {
            configuration.MonitorCwl6 = monitorCwl6;
            configuration.Save();
        }
    
        var monitorCwl7 = configuration.MonitorCwl7;
        if (ImGui.Checkbox("Monitor CWL 7", ref monitorCwl7))
        {
            configuration.MonitorCwl7 = monitorCwl7;
            configuration.Save();
        }
    
        var monitorCwl8 = configuration.MonitorCwl8;
        if (ImGui.Checkbox("Monitor CWL 8", ref monitorCwl8))
        {
            configuration.MonitorCwl8 = monitorCwl8;
            configuration.Save();
        }
    }
}
