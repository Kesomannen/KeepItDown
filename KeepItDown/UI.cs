using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LethalSettings.UI;
using LethalSettings.UI.Components;
using UnityEngine;

namespace KeepItDown; 

[DefaultExecutionOrder(-10)]
public class UI : MonoBehaviour {
    const string Name = "Keep It Down!";
    const string Guid = PluginInfo.PLUGIN_GUID;
    const string Version = PluginInfo.PLUGIN_VERSION;
    const string Description = "Volume control for various sounds in the game.";
    
    readonly Dictionary<string, SliderComponent> _sliders = new();

    public void Awake() {
        var config = KeepItDownPlugin.Instance.Config;
        var sliders = config.Volumes.Values.Select(CreateSlider).OrderBy(slider => slider.Text);

        var resetButton = new ButtonComponent {
            Text = "Reset",
            OnClick = ResetSliders
        };

        var components = new List<MenuComponent> { resetButton, };
        components.AddRange(sliders);
        
        ModMenu.RegisterMod(new ModMenu.ModSettingsConfig {
            Name = Name,
            Id = Guid,
            Version = Version,
            Description = Description,
            MenuComponents = components.ToArray()
        }, true, true);
    }

    SliderComponent CreateSlider(VolumeConfig config) {
        var text = $"{NicifyName(config.Key)} Volume";
        if (config.Section != "Vanilla") {
            text += $" ({config.Section})";
        }
        
        var component = new SliderComponent {
            Text = text,
            MinValue = 0,
            MaxValue = 100,
            Value = config.RawValue,
            OnValueChanged = OnSliderValueChanged
        };
        
        config.OnChanged += OnConfigChangedHandler;

        _sliders[config.Key] = component;
        return component;
    }
    
    void OnSliderValueChanged(SliderComponent slider, float value) {
        var key = _sliders.First(kvp => kvp.Value == slider).Key;
        if (!KeepItDownPlugin.TryGetConfig(key, out var volumeConfig)) return;
        volumeConfig.RawValue = value;
    }

    void OnConfigChangedHandler(VolumeConfig config, float rawValue, float normalizedValue) {
        var slider = _sliders[config.Key];
        if (Mathf.Approximately(slider.Value, rawValue)) return;
        slider.Value = rawValue;
    }
    
    void ResetSliders(ButtonComponent instance) {
        foreach (var volumeConfig in KeepItDownPlugin.Instance.Config.Volumes.Values) {
            volumeConfig.NormalizedValue = 1f;
        }
    }
    
    readonly Regex _camelCaseRegex = new("([a-z])([A-Z])");

    string NicifyName(string name) {
        return _camelCaseRegex.Replace(name, "$1 $2");
    }
}