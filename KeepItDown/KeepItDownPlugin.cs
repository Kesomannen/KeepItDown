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

        var menuComponents = Config.Volumes.Select(kvp => {
                var config = kvp.Value;
                var component = new SliderComponent {
                    Text = $"{kvp.Key} Volume",
                    MinValue = 0,
                    MaxValue = 100,
                    Value = config.RawValue,
                    OnValueChanged = (_, value) => config.RawValue = value
                };

                config.OnChanged += (rawValue, _) => {
                    if (!Mathf.Approximately(component.Value, rawValue)) {
                        component.Value = rawValue;
                    }
                };

                return component;
            })
            .OrderBy(slider => slider.Text)
            .Cast<MenuComponent>()
            .Append(new ButtonComponent {
                Text = "Reset",
                OnClick = _ => {
                    foreach (var volumeConfig in Config.Volumes.Values) {
                        volumeConfig.NormalizedValue = 0.5f;
                    }
                },
            })
            .ToArray();
        
        ModMenu.RegisterMod(new ModMenu.ModSettingsConfig {
            Name = PluginInfo.PLUGIN_NAME,
            Id = PluginInfo.PLUGIN_GUID,
            Version = PluginInfo.PLUGIN_VERSION,
            MenuComponents = menuComponents
        }, true, true);
        
        Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is loaded!");
    }

    /// <summary>
    /// Adds a new volume config.
    /// </summary>
    /// <param name="key">
    /// The key of the config, used later for referencing. Must be unique.</param>
    /// <returns>Whether or not the config was successfully created.</returns>
    public static bool AddConfig(string key) {
        return Instance.Config.AddVolumeConfig(key);
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
        if (!Instance.Config.Volumes.TryGetValue(key, out var volumeConfig)) {
            Instance.Logger.LogWarning($"Trying to bind volume config for {key}, but it doesn't exist");
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
        if (!Instance.Config.Volumes.TryGetValue(key, out var volumeConfig)) {
            Instance.Logger.LogWarning($"Trying to remove volume config bindings for {key}, but it doesn't exist");
            return false;
        }
        
        volumeConfig.RemoveBindings(gameObject);
        return true;
    }
}