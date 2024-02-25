using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace KeepItDown; 

public class KeepItDownConfig {
    readonly Dictionary<string, VolumeConfig> _volumes = new();
    readonly ConfigFile _mainCfg;
    
    public IReadOnlyDictionary<string, VolumeConfig> Volumes => _volumes;

    internal KeepItDownConfig(ConfigFile mainCfg) {
        _mainCfg = mainCfg;
    }

    static VolumeConfig CreateVolumeConfig(ConfigFile cfg, string key, string section) {
        return new VolumeConfig(key, cfg.Bind(
            section,
            $"{key}Volume",
            50f,
            $"Volume of {key}"
        ));
    }
    
    /// <summary>
    /// Adds a new volume config to the config file.
    /// </summary>
    /// <param name="key">A unique key of the config.</param>
    /// <param name="section">The section of the config file. Usually your mod's name.</param>
    /// <param name="cfg">The config file to use. Defaults to KeepItDown's config file.</param>
    /// <returns>Whether or not the config was successfully added.</returns>
    public bool AddVolumeConfig(string key, string section, ConfigFile cfg = null) {
        if (_volumes.ContainsKey(key)) {
            KeepItDownPlugin.Instance.Log.LogWarning($"Volume config for {key} already exists!");
            return false;
        }
        
        _volumes[key] = CreateVolumeConfig(cfg ?? _mainCfg, key, section);
        return true;
    }
    
    /// <summary>
    /// Adds multiple volume configs to the config file.
    /// </summary>
    /// <param name="keys">Unique keys of the configs.</param>
    /// <param name="section">The section of the config file. Usually your mod's name.</param>
    /// <param name="cfg">The config file to use. Defaults to KeepItDown's config file.</param>
    /// <returns>Whether or not all configs were successfully added.</returns>
    public bool AddVolumeConfigs(IEnumerable<string> keys, string section, ConfigFile cfg = null) {
        return keys.Select(key => AddVolumeConfig(key, section, cfg)).All(x => x);
    }
}

/// <summary>
/// A volume configuration entry. It is essentially a wrapper around a
/// <see cref="ConfigEntry{T}"/> with some extra functionality.
/// </summary>
public class VolumeConfig : IDisposable {
    readonly ConfigEntry<float> _configEntry;
    readonly List<Binding> _bindings = new();

    bool _isDisposed;
    
    /// <summary>
    /// The normalized volume, between 0 and 2, where 1 is normal volume.
    /// </summary>
    public float NormalizedValue {
        get => RawValue / 100f * 2;
        set => RawValue = value * 100f / 2;
    }
    
    /// <summary>
    /// The raw volume value as entered in the config file,
    /// between 0 and 100, where 50 is normal volume.
    /// </summary>
    public float RawValue {
        get => _configEntry.Value;
        set => _configEntry.Value = value;
    }
    
    public string Key { get; }
    public string Section => _configEntry.Definition.Section;

    /// <summary>
    /// Invoked when the volume value changes.
    /// </summary>
    public event ChangedEventHandler OnChanged;
    
    /// <summary>
    /// Delegate for the <see cref="OnChanged"/> event.
    /// </summary>
    public delegate void ChangedEventHandler(VolumeConfig config, float rawValue, float normalizedValue); 
    
    internal VolumeConfig(string key, ConfigEntry<float> configEntry) {
        Key = key;
        _configEntry = configEntry;

        _configEntry.SettingChanged += SettingChangedEventHandler;
    }

    ~VolumeConfig() {
        Dispose();
    }
    
    public void Dispose() {
        if (_isDisposed) return;
        _isDisposed = true;
        
        _configEntry.SettingChanged -= SettingChangedEventHandler;
    }
    
    void SettingChangedEventHandler(object sender, EventArgs e) {
        for (var i = 0; i < _bindings.Count; i++) {
            var binding = _bindings[i];
            if (binding.GameObject == null) {
                _bindings.RemoveAt(i--);
                continue;
            }
            
            ActivateBinding(in binding);
        }
        
        OnChanged?.Invoke(this, RawValue, NormalizedValue);
    }

    /// <summary>
    /// Adds a new binding to the volume config and activates it
    /// with the current volume value.
    /// </summary>
    /// <param name="binding">The binding to add.</param>
    public void AddBinding(Binding binding) {
        _bindings.Add(binding);
        ActivateBinding(in binding);
    }
    
    /// <summary>
    /// Removes all bindings for a GameObject.
    /// </summary>
    /// <param name="gameObject">The target GameObject.</param>
    public void RemoveBindings(GameObject gameObject) {
        for (var i = 0; i < _bindings.Count; i++) {
            if (_bindings[i].GameObject == gameObject) {
                _bindings.RemoveAt(i--);
            }
        }
    }
    
    void ActivateBinding(in Binding binding) {
        binding.Setter(NormalizedValue * binding.BaseVolume);
    }

    /// <summary>
    /// Represents a binding between a GameObject and a volume config.
    /// You can create bindings with <see cref="VolumeConfig.AddBinding"/> or
    /// <see cref="KeepItDownPlugin.Bind"/>.
    /// </summary>
    public struct Binding {
        /// <summary>
        /// The "owner" GameObject. When this is destroyed, the binding is removed.
        /// </summary>
        public GameObject GameObject { get; }
        
        /// <summary>
        /// The default volume, will be scaled by the config value.
        /// </summary>
        public float BaseVolume { get; }
        
        /// <summary>
        /// An action to set the raw volume.
        /// </summary>
        public Action<float> Setter { get; }

        public Binding(GameObject gameObject, float baseVolume, Action<float> setter) {
            GameObject = gameObject;
            BaseVolume = baseVolume;
            Setter = setter;
        }
    }
}