# Keep It Down

Volume tuning mod for Lethal Company. Supports a wide range of Vanilla sounds, and can easily be extended by mod makers.

Install LethalSettings or LethalConfig to edit the volumes in-game (where changes are applied immediately). LethalConfig is more widely supported by other mods, while LethalSettings offers more features.

![Mod Settings Window](https://github.com/Kesomannen/KeepItDown/assets/113015915/39229796-b2e2-4712-9d42-fd6c2d51f2dd)

## Installation

Go to [the thunderstore page](https://thunderstore.io/c/lethal-company/p/Kesomannen/KeepItDown/) to get the latest version. If you choose to do download manually, simply add `KeepItDown.dll` to your `/BepInEx/plugins` folder under the game directory.

## For Developers

You can add your own configs for custom mods very easily.

1. Reference the `KeepItDown` assembly and add the plugin as a depency. If you prefer to have it as an optional dependency, check the [wiki](https://github.com/Kesomannen/KeepItDown/wiki) for an example.
```cs
using KeepItDown;

[BepInDependency(KeepItDownInfo.Guid)]
public class MyPlugin : BaseUnityPlugin {
    ...
}
```
2. Call `KeepItDownPlugin.AddVolumeConfigs` with unique keys for your volume configs, as well as your mod name.
```cs
public class MyPlugin : BaseUnityPlugin {
    void Awake() {
        KeepItDownPlugin.Instance.AddVolumeConfigs(
            new [] {
                "CowMoo",
                "SheepBaa"
            },
            "MyPlugin"
        );
    }
}
```
3. Finally, bind AudioSources with `KeepItDownPlugin.BindAudioSource`, to have their volumes synced with the config. Config values are relative and applied on top of the AudioSource's original volume, so you can tune the `volume` property however you like.
```cs
public class CowEnemy : EnemyAI {
    [SerializeField] AudioSource _mooAudioSource;

    void Awake() {
        KeepItDownPlugin.Instance.BindAudioSource("CowMoo", _mooAudioSource);
    }
}
```

For more information, see the [wiki](https://github.com/Kesomannen/KeepItDown/wiki) on Github.