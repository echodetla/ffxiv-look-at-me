using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using LookAtMePlugin.Windows;
using Lumina.Excel.Sheets.Experimental;

namespace LookAtMePlugin;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class LookAtMePlugin : IDalamudPlugin
{
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;

    [PluginService]
    internal static IObjectTable ObjectTable { get; private set; } = null!;
    
    [PluginService]
    internal static ITargetManager TargetManager { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;

    [PluginService]
    internal static IChatGui Chat { get; private set; } = null!;

    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;

    private const string SettingsCommandName = "/lookatme";
    private const string ToggleCommandName = "/togglelookatme";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("LookAtMePlugin");
    private ConfigWindow ConfigWindow { get; init; }

    private ChatWatcher Watcher { get; init; }
    public IPlayerCharacter? CachedLocalPlayer { get; set; }
    
    public bool InPvPZone { get; private set; }

    public LookAtMePlugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(SettingsCommandName, new CommandInfo(OnConfigCommand)
        {
            HelpMessage = "Opens the settings window"
        });
        
        CommandManager.AddHandler(ToggleCommandName, new CommandInfo(OnToggleCommand)
        {
            HelpMessage = "Toggles the functionality of look at me"
        });


        // Tell the UI system that we want our windows to be drawn throught he window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        Watcher = new ChatWatcher(this, Chat, TargetManager, ObjectTable, Log);

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        ClientState.Login += OnClientStateOnLogin;
        ClientState.Logout += OnClientStateOnLogout;
        ClientState.EnterPvP += ClientStateOnEnterPvP;
        ClientState.LeavePvP += ClientStateOnLeavePvP;
        Framework.Update += OnFrameworkOnUpdate;
    }

    private void OnToggleCommand(string command, string arguments)
    {
        Configuration.IsEnabled = !Configuration.IsEnabled;
        Configuration.Save();
    }

    private void ClientStateOnLeavePvP()
    {
        InPvPZone = false;
    }

    private void ClientStateOnEnterPvP()
    {
        InPvPZone = true;
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;

        WindowSystem.RemoveAllWindows();

        ClientState.Login -= OnClientStateOnLogin;
        ClientState.Logout -= OnClientStateOnLogout;
        ClientState.EnterPvP -= ClientStateOnEnterPvP;
        ClientState.LeavePvP -= ClientStateOnLeavePvP;
        Framework.Update -= OnFrameworkOnUpdate;
        
        ConfigWindow.Dispose();
        Watcher.Dispose();

        CommandManager.RemoveHandler(SettingsCommandName);
    }

    private void OnFrameworkOnUpdate(IFramework framework)
    {
        if (CachedLocalPlayer == null)
        {
            var player = framework.RunOnFrameworkThread(() => ClientState.LocalPlayer).Result;
            if (player != null)
            {
                CachedLocalPlayer = player;
                Framework.Update -= OnFrameworkOnUpdate;
            }
        }
    }

    private void OnClientStateOnLogout(int type, int code)
    {
        CachedLocalPlayer = null;
        Framework.Update += OnFrameworkOnUpdate;
    }

    private void OnClientStateOnLogin()
    {
        var player = Framework.RunOnFrameworkThread(() => ClientState.LocalPlayer).Result;
        if (player != null)
        {
            CachedLocalPlayer = player;
            Framework.Update -= OnFrameworkOnUpdate;
        }
    }
    
    private void OnConfigCommand(string command, string args)
    {
        ToggleConfigUi();
    }
    public void ToggleConfigUi() => ConfigWindow.Toggle();
}
