using System.Collections.Generic;
using System.Linq;
using LethalSettings.UI;
using LethalSettings.UI.Components;
using UnityEngine;

namespace KeepItDown; 

public static class LethalSettingsUI {
    static SliderComponent[] _sliders;
    static bool _isInitialized;
    
    static readonly Dictionary<SliderComponent, string> _sliderToConfigKey = new();

    internal static void Init() {
        if (_isInitialized) return;
        _isInitialized = true;
        
        var config = KeepItDownPlugin.Instance.Config;
        _sliders = config.Volumes.Select(kvp => {
            kvp.Value.OnChanged += OnConfigChangedHandler;
            
            return new SliderComponent {
                MinValue = 0,
                MaxValue = 100,
                OnValueChanged = OnSliderValueChanged
            };
        }).ToArray();
        RefreshOrder();

        var resetButton = new ButtonComponent {
            Text = "Reset",
            OnClick = OnResetClicked
        };

        var searchBar = new InputComponent {
            Placeholder = "Search...",
            OnValueChanged = (_, text) => RefreshOrder(text)
        };

        var components = new List<MenuComponent> {
            resetButton,
            searchBar
        };
        components.AddRange(_sliders);

        var settings = new ModMenu.ModSettingsConfig {
            Name = SharedUI.Name,
            Id = SharedUI.Guid,
            Version = SharedUI.Version,
            Description = SharedUI.Description,
            MenuComponents = components.ToArray()
        };
        
        ModMenu.RegisterMod(settings, true, true);
    }
    
    static void RefreshOrder(string searchTerm = null) {
        var config = KeepItDownPlugin.Instance.Config;
        
        IEnumerable<string> orderedKeys;
        if (searchTerm == null) {
            orderedKeys = config.Volumes.Keys.OrderBy(k => k);
        } else {
            var lowerSearchTerm = searchTerm.ToLower();
            orderedKeys = config.Volumes.Keys
                .OrderBy(k => k.ToLower().Contains(lowerSearchTerm) ? 0 : 1);
        }
        
        _sliderToConfigKey.Clear();
        var i = 0;
        foreach (var key in orderedKeys) {
            var slider = _sliders[i++];
            var volumeConfig = config.Volumes[key];
            
            slider.Text = SharedUI.GetDisplayName(volumeConfig);
            slider.Value = volumeConfig.RawValue;
            
            _sliderToConfigKey[slider] = key;
        }
    }
    
    static void OnSliderValueChanged(SliderComponent slider, float value) {
        if (!_sliderToConfigKey.TryGetValue(slider, out var key)) return;
        if (!KeepItDownPlugin.Instance.TryGetConfig(key, out var config)) return;
        
        config.RawValue = value;
    }

    static void OnConfigChangedHandler(VolumeConfig config, float rawValue, float normalizedValue) {
        var slider = _sliders.FirstOrDefault(s => _sliderToConfigKey[s] == config.Key);
        if (slider == null) return;
        if (Mathf.Approximately(slider.Value, rawValue)) return;
        slider.Value = rawValue;
    }
    
    static void OnResetClicked(ButtonComponent instance) {
        SharedUI.ResetAllVolumes();
    }
}