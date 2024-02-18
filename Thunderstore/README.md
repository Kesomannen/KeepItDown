# Keep It Down

Simple volume tuning mod for Lethal Company. The settings can be edited either through the ingame UI, or through BepInEx config.

![Mod Settings Window](https://github.com/Kesomannen/KeepItDown/assets/113015915/39229796-b2e2-4712-9d42-fd6c2d51f2dd)

## Installation

Go to [the thunderstore page](https://thunderstore.io/package/Kesomannen/KeepItDown/) to get the latest version. If you choose to do download manually, simply add `KeepItDown.dll` to your `/BepInEx/plugins` folder under the game directory. Note that `LethalSettings` is required as a dependency.

## For Developers

You can add your own configs for custom mods extremely easily.

1. Reference the `KeepItDown` assembly and add the plugin as a dependency.
```cs
[BepInDependency(KeepItDown.PluginInfo.PLUGIN_GUID)]
public class MyPlugin : BaseUnityPlugin {
  ...
}
```
3. For each config, call `KeepItDownPlugin.AddConfig` with a unique key.
```cs
public class MyPlugin : BaseUnityPlugin {
  void Awake() {
    KeepItDownPlugin.AddConfig("CowMoo");
  }
}
```
4. Finally, bind AudioSources with `KeepItDownPlugin.BindAudioSource`, to have their volumes synced with the config. Config values are relative (from 0-200% of the base volume), so you can tweak the AudioSource's volume freely.
```cs
public class CowEnemy : EnemyAI {
  [SerializeField] AudioSource _mooAudioSource;

  void Awake() {
    KeepItDownPlugin.BindAudioSource("CowMoo", _mooAudioSource);
  }
}
```
