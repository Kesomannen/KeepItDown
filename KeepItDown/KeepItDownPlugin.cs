using System;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalSettings.UI;
using UnityEngine;

namespace KeepItDown; 

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.willis.lc.lethalsettings")]
public class KeepItDownPlugin : BaseUnityPlugin {
    public static KeepItDownPlugin Instance { get; private set; }
    
    internal ManualLogSource Log => Logger;
    public new KeepItDownConfig Config { get; private set; }
    
    void Awake() {
        Instance = this;
        
        Config = new KeepItDownConfig(base.Config);
        Config.AddVolumeConfigs(new[] {
            "Airhorn",
            "Boombox",
            "CashRegister",
            "Remote",
            "Flashlight",
            "Walkie-talkie",
            "Scan"
        }, "Vanilla");
        
        UI.Initialize(Config);
        Harmony.CreateAndPatchAll(typeof(Patches));
        
        Log.LogInfo($"{PluginInfo.PLUGIN_GUID} is loaded!");
    }

    /// <inheritdoc cref="KeepItDownConfig.AddVolumeConfig"/>
    public static bool AddConfig(string key, string section, ConfigFile cfg = null) {
        return Instance.Config.AddVolumeConfig(key, section, cfg);
    }
    
    public static bool TryGetConfig(string key, out VolumeConfig config) {
        return Instance.Config.Volumes.TryGetValue(key, out config);
    }
    
    /// <summary>
    /// Binds to a volume config. Use this when you want to sync a property
    /// to a volume config. If you want to bind an AudioSource's volume,
    /// use <see cref="BindAudioSource"/> instead.
    /// </summary>
    /// <param name="key">The key of the config.</param>
    /// <param name="gameObject">
    /// The "owner" GameObject. When this is destroyed, the binding is removed.
    /// </param>
    /// <param name="baseVolume">The default volume, will be scaled by the config value.</param>
    /// <param name="volumeSetter">An action to set the volume property (not normalized).</param>
    /// <returns>Whether or not the binding was successfully created.</returns>
    public static bool Bind(string key, GameObject gameObject, float baseVolume, Action<float> volumeSetter) {
        if (!TryGetConfig(key, out var volumeConfig)) {
            Instance.Log.LogWarning($"Trying to bind volume config for {key}, but it doesn't exist");
            return false;
        }
        
        volumeConfig.AddBinding(new VolumeConfig.Binding(gameObject, baseVolume, volumeSetter));
        return true;
    }
    
    /// <summary>
    /// Binds the volume of an AudioSource to a volume config.
    /// </summary>
    /// <param name="key">The key of the config.</param>
    /// <param name="audioSource">The AudioSource to bind to.</param>
    /// <returns>Whether or not the binding was successfully created.</returns>
    public static bool BindAudioSource(string key, AudioSource audioSource) {
        return Bind(key, audioSource.gameObject, audioSource.volume, v => audioSource.volume = v);
    }
    
    /// <summary>
    /// Removes all bindings for a specific GameObject.
    /// </summary>
    /// <param name="key">The key of the config.</param>
    /// <param name="gameObject">The target GameObject.</param>
    /// <returns>Whether or not the bindings were successfully removed.</returns>
    public static bool RemoveBindings(string key, GameObject gameObject) {
        if (!TryGetConfig(key, out var volumeConfig)) {
            Instance.Log.LogWarning($"Trying to remove volume config bindings for {key}, but it doesn't exist");
            return false;
        }
        
        volumeConfig.RemoveBindings(gameObject);
        return true;
    }
}