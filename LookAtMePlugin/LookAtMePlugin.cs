using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using LookAtMePlugin.Windows;

namespace LookAtMePlugin;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class LookAtMePlugin : IDalamudPlugin
{
    // Plugin services
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

    // Constants
    private const string SettingsCommandName = "/lookatme";
    private const string ToggleCommandName = "/togglelookatme";

    // Properties
    public Configuration Configuration { get; init; }
    public readonly WindowSystem WindowSystem = new("LookAtMePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private ChatWatcher Watcher { get; init; }
    public IPlayerCharacter? CachedLocalPlayer { get; set; }
    public bool InPvPZone { get; private set; }

    // Constructor
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

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        Watcher = new ChatWatcher(this, Chat, TargetManager, ObjectTable, Log);
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        ClientState.Login += OnClientStateOnLogin;
        ClientState.EnterPvP += ClientStateOnEnterPvP;
        ClientState.LeavePvP += ClientStateOnLeavePvP;
        Framework.Update += OnFrameworkOnUpdate;
    }

    // Event Handlers
    private void OnConfigCommand(string command, string args)
    {
        ToggleConfigUi();
    }

    private void OnToggleCommand(string command, string arguments)
    {
        Configuration.IsEnabled = !Configuration.IsEnabled;
        Configuration.Save();
    }

    private void OnClientStateOnLogin()
    {
        var player = Framework.RunOnFrameworkThread(() => ClientState.LocalPlayer).Result;
        if (player != null)
        {
            CachedLocalPlayer = player;
            Framework.Update -= OnFrameworkOnUpdate;
        }
        else
        {
            Framework.Update += OnFrameworkOnUpdate;
        }
    }

    private void ClientStateOnEnterPvP()
    {
        InPvPZone = true;
    }

    private void ClientStateOnLeavePvP()
    {
        InPvPZone = false;
    }

    private void OnFrameworkOnUpdate(IFramework framework)
    {
        try
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
        } catch (Exception ex)
        {
            Log.Error($"Error in OnFrameworkOnUpdate: {ex}");
        }
    }

    // Public Methods
    public void ToggleConfigUi() => ConfigWindow.Toggle();

    // Dispose
    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        WindowSystem.RemoveAllWindows();

        ClientState.Login -= OnClientStateOnLogin;
        ClientState.EnterPvP -= ClientStateOnEnterPvP;
        ClientState.LeavePvP -= ClientStateOnLeavePvP;
        Framework.Update -= OnFrameworkOnUpdate;

        ConfigWindow.Dispose();
        Watcher.Dispose();
        CommandManager.RemoveHandler(SettingsCommandName);
    }
}
