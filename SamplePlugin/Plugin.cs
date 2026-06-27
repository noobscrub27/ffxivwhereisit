using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using WhereIsItPlugin.Windows;
using Lumina.Excel.Sheets;
using System;

namespace WhereIsItPlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IChatGui Chat { get; private set; } = null!;

    private const string MenuCommandName = "/whereisit";
    private const string LocateCommandName = "/wit_locate";
    private const string SavePositionCommandName = "/wit_save";
    private const string ComparePositionCommandName = "/wit_compare";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("WhereIsItPlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(MenuCommandName, new CommandInfo(MenuCommand)
        {
            HelpMessage = "Opens WhereIsIt menu."
        });
        CommandManager.AddHandler(LocateCommandName, new CommandInfo(LocateCommand)
        {
            HelpMessage = "Prints the target's location."
        });
        CommandManager.AddHandler(SavePositionCommandName, new CommandInfo(SavePositionCommand)
        {
            HelpMessage = "Saves your target's location."
        });
        CommandManager.AddHandler(ComparePositionCommandName, new CommandInfo(ComparePositionCommand)
        {
            HelpMessage = "Compares your target's location with your saved location."
        });

        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [WhereIsItPlugin] ===A cool log message from Sample Plugin===
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(MenuCommandName);
        CommandManager.RemoveHandler(LocateCommandName);
        CommandManager.RemoveHandler(MenuCommandName);
        CommandManager.RemoveHandler(MenuCommandName);
    }
    public string GetSavedPositionText()
    {
        if (Configuration.savedZoneName is null | Configuration.savedTargetName is null)
        {
            return "Current saved location: none";
        }
        else
        {
            if (string.IsNullOrEmpty(Configuration.savedZoneName) | string.IsNullOrWhiteSpace(Configuration.savedZoneName))
            {
                return $"Current saved location: {Configuration.savedPosition} in (unknown zone)";
            }
            else
            {
                return $"Current saved location: {Configuration.savedPosition} in {Configuration.savedZoneName}";
            }
        }
    }
    private void MenuCommand(string command, string args)
    {
        ToggleMainUi();
    }
    private void LocateCommand(string command, string args)
    {
        DisplayTargetLocation();
    }
    private void SavePositionCommand(string command, string args)
    {
        SaveTargetLocation();
    }
    private void ComparePositionCommand(string command, string args)
    {
        CompareDistance();
    }
    public void DisplayTargetLocation()
    {
        var currentTarget = TargetManager.Target;
        if (currentTarget is null)
        {
            SendEchoChat($"No target to locate.");
        }
        else
        {
            SendEchoChat($"{currentTarget.Name}'s location: {currentTarget.Position}");
        }
    }
    public void SaveTargetLocation()
    {
        var currentTarget = TargetManager.Target;
        if (currentTarget is null)
        {
            SendEchoChat($"No target to save location of.");
        }
        else
        {
            var zoneName = GetZone();
            if (zoneName is null)
            {
                SendEchoChat($"The target's zone could not be identified, so its location was not saved.");
            }
            else
            {
                Configuration.savedZoneName = zoneName;
                Configuration.savedTargetName = currentTarget.Name.ToString();
                Configuration.savedPosition = currentTarget.Position;
                Configuration.Save();
                SendEchoChat($"Saved {currentTarget.Name}'s location: {currentTarget.Position}");
            }
        }
    }
    public void ClearSavedLocation()
    {
        Configuration.savedZoneName = null;
        Configuration.savedTargetName = null;
        Configuration.savedPosition = new System.Numerics.Vector3();
        Configuration.Save();
        SendEchoChat($"Cleared saved location.");
    }
    public void CompareDistance()
    {
        var currentTarget = TargetManager.Target;
        if (currentTarget is null)
        {
            SendEchoChat($"No target to compare.");
        }
        else if (Configuration.savedZoneName is null | Configuration.savedTargetName is null)
        {
            SendEchoChat($"The target's location could not be compared because there is no saved target.");
        }
        else
        {
            var zoneName = GetZone();
            if (zoneName is null)
            {
                SendEchoChat($"The target's zone could not be identified, so its location cannot be compared.");
            }
            else if (zoneName != Configuration.savedZoneName)
            {
                SendEchoChat($"The target's zone is different from the saved location's zone, so their locations cannot be compared.");
            }
            else
            {
                var currentPosition = currentTarget.Position;
                var distanceBase = 
                    Math.Pow((Configuration.savedPosition.X - currentPosition.X), 2) +
                    Math.Pow((Configuration.savedPosition.Z - currentPosition.Z), 2);
                var distance2d = Math.Sqrt(distanceBase);
                var distance3d = Math.Sqrt(distanceBase + Math.Pow((Configuration.savedPosition.Y - currentPosition.Y), 2));
                SendEchoChat($"Saved location -> {currentTarget.Name}: {distance2d} (2D), {distance3d} (3D)");
            }
        }
    }
    public string? GetZone()
    {
        var territoryId = Plugin.ClientState.TerritoryType;
        if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
        {
            return territoryRow.PlaceName.Value.Name.ToString();
        }
        return null;
    }

    public void SendEchoChat(string message)
    {
        Chat.Print(message, "WIT");
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}
