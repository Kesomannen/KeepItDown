# Keep It Down

Volume tuning mod for Lethal Company. Supports a wide range of Vanilla sounds, and can easily be extended by mod makers.

### To get the ingame UI, please install [LethalConfig](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/) or [LethalSettings](https://thunderstore.io/c/lethal-company/p/willis81808/LethalSettings/)!
 _(or both if you're feeling spicy)_ LethalConfig is more widely supported and has a nicer interface, but LethalSettings has more features.

<br/>

_LethalConfig Menu_

![LethalConfig Settings Window](https://github.com/Kesomannen/KeepItDown/assets/113015915/7b6c2ee7-2800-4e43-a7ce-9676cf6f8e89)

_LethalSettings Menu_

![LethalSettings Settings Window](https://github.com/Kesomannen/KeepItDown/assets/113015915/82942dd0-14a6-41f3-96d6-c31efbb0391e)

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