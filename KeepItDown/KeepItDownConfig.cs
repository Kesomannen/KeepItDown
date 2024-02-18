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
    
    public bool AddVolumeConfig(string key) {
        if (_volumes.ContainsKey(key)) {
            KeepItDownPlugin.Instance.Logger.LogWarning($"Volume config for {key} already exists!");
            return false;
        }
        
        _volumes[key] = CreateVolumeConfig(_cfg, key);
        return true;
    }
    
    public bool AddVolumeConfigs(IEnumerable<string> keys) {
        return keys.Select(AddVolumeConfig).All(x => x);
    }
}

public class VolumeConfig : IDisposable {
    readonly ConfigEntry<float> _configEntry;
    readonly List<Binding> _bindings = new();
    
    bool _isDisposed;
    
    public float NormalizedValue {
        get => RawValue / 100f * 2;
        set => RawValue = value * 100f / 2;
    }
    
    public float RawValue {
        get => _configEntry.Value;
        set => _configEntry.Value = value;
    }
    
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