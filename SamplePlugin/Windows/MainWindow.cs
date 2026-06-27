using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace WhereIsItPlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("Where Is It##noobscrubWITMainWindowId", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text(plugin.GetSavedPositionText());
        ImGui.Spacing();
        if (ImGui.Button("Get target location (/wit_locate)"))
        {
            plugin.DisplayTargetLocation();
        }
        if (ImGui.Button("Save target location (/wit_save)"))
        {
            plugin.SaveTargetLocation();
        }
        if (ImGui.Button("Compare locations (/wit_compare)"))
        {
            plugin.CompareDistance();
        }
        ImGui.Spacing();
        if (ImGui.Button("Clear saved location"))
        {
            plugin.ClearSavedLocation();
        }
    }
}
