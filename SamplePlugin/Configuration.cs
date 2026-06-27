using Dalamud.Configuration;
using System;

namespace WhereIsItPlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string? savedZoneName { get; set; }
    public System.Numerics.Vector3 savedPosition { get; set; }
    public string? savedTargetName { get; set; }

    // The below exists just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
