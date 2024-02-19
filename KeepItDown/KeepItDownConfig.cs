using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace KeepItDown; 

public class KeepItDownConfig {
    readonly Dictionary<string, VolumeConfig> _volumes;
    readonly ConfigFile _cfg;
    
    public IReadOnlyDictionary<string, VolumeConfig> Volumes => _volumes;

    internal KeepItDownConfig(ConfigFile cfg) {
        _cfg = cfg;
        _volumes = new [] {
            "Airhorn",
            "Boombox",
            "CashRegister",
            "Remote",
            "Flashlight",
            "Walkie-talkie",
            "HUD"
        }.ToDictionary(name => name, name => CreateVolumeConfig(cfg, name));
    }

    static VolumeConfig CreateVolumeConfig(ConfigFile cfg, string key) {
        return new VolumeConfig(cfg.Bind(
            "Volume",
            $"{key}Volume",
            50f,
            $"Volume of the {key.ToLower()} sound (0-100). Defaults to 50."
        ));
    }
    
    /// <summary>
    /// Adds a new volume config to the config file.
    /// </summary>
    /// <param name="key">A unique key of the config.</param>
    /// <returns>Whether or not the config was successfully added.</returns>
    public bool AddVolumeConfig(string key) {
        if (_volumes.ContainsKey(key)) {
            KeepItDownPlugin.Instance.Logger.LogWarning($"Volume config for {key} already exists!");
            return false;
        }
        
        _volumes[key] = CreateVolumeConfig(_cfg, key);
        return true;
    }
    
    /// <summary>
    /// Adds multiple volume configs to the config file.
    /// </summary>
    /// <param name="keys">Unique keys of the configs.</param>
    /// <returns>Whether or not all configs were successfully added.</returns>
    public bool AddVolumeConfigs(IEnumerable<string> keys) {
        return keys.Select(AddVolumeConfig).All(x => x);
    }
}

/// <summary>
/// A volume configuration entry. It is at its simplest a wrapper
/// around a <see cref="ConfigEntry{T}"/> with some additional
/// functionality.
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
    
    /// <summary>
    /// Invoked when the volume value changes.
    /// </summary>
    public event ChangedEventHandler OnChanged;
    
    /// <summary>
    /// A delegate for the <see cref="OnChanged"/> event.
    /// </summary>
    public delegate void ChangedEventHandler(float rawValue, float normalizedValue); 
    
    internal VolumeConfig(ConfigEntry<float> configEntry) {
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
        
        OnChanged?.Invoke(RawValue, NormalizedValue);
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

    public struct Binding {
        public GameObject GameObject { get; }
        public float BaseVolume { get; }
        public Action<float> Setter { get; }

        public Binding(GameObject gameObject, float baseVolume, Action<float> setter) {
            GameObject = gameObject;
            BaseVolume = baseVolume;
            Setter = setter;
        }
    }
}