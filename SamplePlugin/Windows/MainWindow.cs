using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Text;
using Lumina.Text.ReadOnly;
using System;
using System.Numerics;

namespace MogsketoolPlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private readonly ReadOnlySeString emptyLogText;

    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("Mogsketool##noobscrubMTMainWindowId", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        SeStringBuilder emptyLogTextBuilder = new SeStringBuilder();
        emptyLogTextBuilder
            .AppendSetItalic(true)
            .Append("The log is empty.");
        emptyLogText = emptyLogTextBuilder.ToReadOnlySeString();
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text(plugin.GetSavedPositionText());
        ImGui.Spacing();
        if (ImGui.Button("Get target location (/mt_locate)"))
        {
            plugin.DisplayTargetLocation();
        }
        if (ImGui.Button("Save target location (/mt_savepos)"))
        {
            plugin.SaveTargetLocation();
        }
        if (ImGui.Button("Compare locations (/mt_comparepos)"))
        {
            plugin.CompareDistance();
        }
        ImGui.Spacing();
        if (ImGui.Button("Clear saved location"))
        {
            plugin.ClearSavedLocation();
        }
        ImGui.Spacing();
        using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
        {
            // Check if this child is drawing
            if (child.Success)
            {
                if (plugin.Configuration.KupoHistory.Count > 0)
                {
                    for (int i = 0; i < plugin.Configuration.KupoHistory.Count; i++)
                    {
                        ImGuiHelpers.SeStringWrapped(plugin.Configuration.KupoHistory[i].getSeString());
                    }
                }
                else
                {
                    ImGuiHelpers.SeStringWrapped(emptyLogText);
                }
            }
        }
    }
}
