using System;
using System.Collections;
using System.Linq;
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
            "Scan",
            "Spraycan",
            "Dentures",
            "RobotToy",
            "Hairdryer",
            "Jetpack",
            "RadarBoosterPing",
            "ShipAlarm",
            "ShipAlarmCord",
            "ItemCharger",
            "Shovel",
            "RubberDucky",
            "Landmine",
            "Jester",
            "Thunder",
            "WhoopieCushion",
            "ExtensionLadder",
            "Turret"
        }, "Vanilla");

        Harmony.CreateAndPatchAll(typeof(Patches), PluginInfo.PLUGIN_GUID);

        var ui = new GameObject("KeepItDownUI").AddComponent<UI>();
        DontDestroyOnLoad(ui.gameObject);
        
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
    /// with a volume config. If you want to bind an AudioSource's volume,
    /// use <see cref="BindAudioSource"/> instead.
    /// </summary>
    /// <param name="key">The key of the config.</param>
    /// <param name="gameObject">
    /// The "owner" GameObject. When this is destroyed, the binding is removed.
    /// </param>
    /// <param name="baseVolume">The default volume, will be scaled by the config value.</param>
    /// <param name="volumeSetter">An action to set the raw volume.</param>
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
    /// Binds the volume of one or more AudioSources to a volume config.
    /// </summary>
    /// <param name="key">The key of the config.</param>
    /// <param name="audioSources">The AudioSources to bind to.</param>
    /// <returns>Whether or not the binding was successfully created.</returns>
    public static bool BindAudioSources(string key, params AudioSource[] audioSources) {
        return audioSources.All(audioSource => BindAudioSource(key, audioSource));
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