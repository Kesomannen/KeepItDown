using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

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
            "Walkie-talkie"
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
    
    bool _isDisposed;
    
    public float NormalizedValue {
        get => RawValue / 100f * 2;
        set => RawValue = value * 100f / 2;
    }
    
    public float RawValue {
        get => _configEntry.Value;
        set => _configEntry.Value = value;
    }

    public event ChangedEventHandler OnChanged; 
    
    public delegate void ChangedEventHandler(float normalizedVolume);
    
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
        OnChanged?.Invoke(NormalizedValue);
    }
}