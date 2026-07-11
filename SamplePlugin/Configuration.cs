using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace MogsketoolPlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public string? savedZoneName { get; set; }
    public System.Numerics.Vector3 savedPosition { get; set; }
    public string? savedTargetName { get; set; }

    public int HistorySize = 256;
    public List<KupoMessage> KupoHistory = new List<KupoMessage>();
    // The below exists just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
