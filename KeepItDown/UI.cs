using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DunGen;
using LethalSettings.UI;
using LethalSettings.UI.Components;
using UnityEngine;

namespace KeepItDown; 

internal static class UI {
    const string Name = "Keep It Down!";
    const string Guid = PluginInfo.PLUGIN_GUID;
    const string Version = PluginInfo.PLUGIN_VERSION;
    const string Description = "Volume control for various sounds in the game.";

    static ModMenu.ModSettingsConfig _modMenuConfig;
    
    static readonly Dictionary<string, SliderComponent> _sliders = new();
    static readonly Dictionary<string, GameObject> _sliderGameObjects = new();

    public static void Initialize(KeepItDownConfig config) {
        var sliders = config.Volumes.Values.Select(CreateSlider).OrderBy(slider => slider.Text);

        var resetButton = new ButtonComponent {
            Text = "Reset",
            OnClick = ResetSliders
        };

        var searchBar = new InputComponent {
            Placeholder = "Search...",
            OnValueChanged = OnSearchValueChanged
        };

        var components = new List<MenuComponent> {
            resetButton,
            searchBar
        };
        components.AddRange(sliders);
        
        ModMenu.RegisterMod(_modMenuConfig = new ModMenu.ModSettingsConfig {
            Name = Name,
            Id = Guid,
            Version = Version,
            Description = Description,
            MenuComponents = components.ToArray()
        }, true, true);
    }

    public static void FindSliderGameObjects() {
        KeepItDownPlugin.Instance.Log.LogInfo($"bla bla");
        var sliderComponentObjectType = Type.GetType("LethalSettings.UI.Components.SliderComponentObject, LethalSettings");
        KeepItDownPlugin.Instance.Log.LogInfo($"Slider component object type: {sliderComponentObjectType}");
        var viewport = _modMenuConfig.GetPropertyValue<GameObject>("Viewport");
        KeepItDownPlugin.Instance.Log.LogInfo($"Viewport: {viewport}");

        var slierComponentObjects = viewport.GetComponentsInChildren(sliderComponentObjectType);
        KeepItDownPlugin.Instance.Log.LogInfo($"Slider component objects: {slierComponentObjects.Length}");
        foreach (var sliderObject in slierComponentObjects) {
            var sliderComponent = sliderObject.GetFieldValue<SliderComponent>("component");
            KeepItDownPlugin.Instance.Log.LogInfo($"Slider component: {sliderComponent}");
            var key = _sliders.First(kvp => kvp.Value == sliderComponent).Key;
            KeepItDownPlugin.Instance.Log.LogInfo($"Key: {key}");
            _sliderGameObjects[key] = sliderObject.gameObject;
        }
        
        KeepItDownPlugin.Instance.Log.LogInfo($"Found {_sliderGameObjects.Count} slider game objects.");
    }

    static SliderComponent CreateSlider(VolumeConfig config) {
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
    
    static void OnSliderValueChanged(SliderComponent slider, float value) {
        var key = _sliders.First(kvp => kvp.Value == slider).Key;
        if (!KeepItDownPlugin.TryGetConfig(key, out var volumeConfig)) return;
        volumeConfig.RawValue = value;
    }

    static void OnConfigChangedHandler(VolumeConfig config, float rawValue, float normalizedValue) {
        var slider = _sliders[config.Key];
        if (Mathf.Approximately(slider.Value, rawValue)) return;
        slider.Value = rawValue;
    }
    
    static void ResetSliders(ButtonComponent instance) {
        foreach (var volumeConfig in KeepItDownPlugin.Instance.Config.Volumes.Values) {
            volumeConfig.NormalizedValue = 1f;
        }
    }
    
    static void OnSearchValueChanged(InputComponent instance, string value) {
        var search = value.ToLower();
        foreach (var (key, gameObject) in _sliderGameObjects) {
            gameObject.SetActive(string.IsNullOrEmpty(search) || key.ToLower().Contains(search));
        }
    }

    static readonly Regex _camelCaseRegex = new("([a-z])([A-Z])");

    static string NicifyName(string name) {
        return _camelCaseRegex.Replace(name, "$1 $2");
    }
}