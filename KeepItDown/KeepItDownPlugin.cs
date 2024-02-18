using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalSettings.UI;
using LethalSettings.UI.Components;
using UnityEngine;

namespace KeepItDown; 

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.willis.lc.lethalsettings")]
public class KeepItDownPlugin : BaseUnityPlugin {
    public static KeepItDownPlugin Instance { get; private set; }
    
    internal new ManualLogSource Logger => base.Logger;
    public new KeepItDownConfig Config { get; private set; }
    
    void Awake() {
        Instance = this;
        
        Config = new KeepItDownConfig(base.Config);

        Harmony.CreateAndPatchAll(typeof(Patches));

        var settings = Config.Volumes.Select(kvp => new SliderComponent {
            Text = $"{kvp.Key} Volume",
            ShowValue = true,
            WholeNumbers = true,
            MinValue = 0,
            MaxValue = 100,
            Value = kvp.Value.RawValue,
            OnValueChanged = (_, value) => kvp.Value.RawValue = value
        }).Cast<MenuComponent>().ToArray();
        
        ModMenu.RegisterMod(new ModMenu.ModSettingsConfig {
            Name = PluginInfo.PLUGIN_NAME,
            Id = PluginInfo.PLUGIN_GUID,
            Version = PluginInfo.PLUGIN_VERSION,
            MenuComponents = settings
        }, true, true);
        
        Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is loaded!");
    }

    public static bool AddConfig(string key) {
        return Instance.Config.AddVolumeConfig(key);
    }
    
    public static bool Bind(string key, float baseVolume, Action<float> volumeSetter) {
        if (!Instance.Config.Volumes.TryGetValue(key, out var volumeConfig)) {
            Instance.Logger.LogWarning($"Trying to bind volume config for {key}, but it doesn't exist!");
            return false;
        }
        
        volumeConfig.OnChanged += normalizedVolume => volumeSetter(normalizedVolume * baseVolume);
        return true;
    }
    
    public static bool BindAudioSource(string configkey, AudioSource audioSource) {
        return Bind(configkey, audioSource.volume, newVolume => {
            audioSource.volume = newVolume;
        });
    }
}