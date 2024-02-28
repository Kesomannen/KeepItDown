using System.Collections.Generic;
using System.Linq;
using LethalSettings.UI;
using LethalSettings.UI.Components;
using UnityEngine;

namespace KeepItDown; 

public static class SharedUI {
    public const string Name = "Keep It Down!";
    public const string Guid = PluginInfo.PLUGIN_GUID;
    public const string Version = PluginInfo.PLUGIN_VERSION;
    public const string Description = "Volume control for various sounds in the game.";

    public static string GetDisplayName(VolumeConfig volumeConfig) {
        var text = $"{volumeConfig.Key} Volume";
        if (volumeConfig.Section != "Vanilla") {
            text += $" ({volumeConfig.Section})";
        }
        return text;
    }

    public static void ResetAllVolumes() {
        foreach (var volumeConfig in KeepItDownPlugin.Instance.Config.Volumes.Values) {
            volumeConfig.NormalizedValue = 1f;
        }
    }
}