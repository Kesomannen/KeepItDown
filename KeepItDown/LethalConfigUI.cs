using System.Linq;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;

namespace KeepItDown; 

public static class LethalConfigUI {
    internal static void Init() {
        LethalConfigManager.SkipAutoGen();
        
        LethalConfigManager.SetModDescription(SharedUI.Description);
        
        var volumeConfigs = KeepItDownPlugin.Instance.Config.Volumes;
        
        /*
        LethalConfigManager.AddConfigItem(new GenericButtonConfigItem(
            "General",
            "Reset All Volumes",
            "Resets all volumes to their default values.",
            "Reset",
            SharedUI.ResetAllVolumes
        ));
        */
        
        foreach (var (_, volumeConfig) in volumeConfigs.OrderBy(kvp => kvp.Key)) {
            var slider = new FloatSliderConfigItem(volumeConfig.ConfigEntry, new FloatStepSliderOptions {
                Name = SharedUI.GetDisplayName(volumeConfig),
                RequiresRestart = false,
                Min = 0f,
                Max = 100f
            });
            LethalConfigManager.AddConfigItem(slider);
        }
    }
}