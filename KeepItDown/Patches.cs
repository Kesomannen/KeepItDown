using HarmonyLib;

namespace KeepItDown; 

internal static class Patches {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NoisemakerProp), nameof(NoisemakerProp.Start))]
    static void StartPatch(NoisemakerProp __instance) {
        var gameObject = __instance.gameObject;
        var name = gameObject.name.Replace("(Clone)", "").Replace("Item", "");
        KeepItDownPlugin.Bind(name, gameObject, __instance.maxLoudness, v => __instance.maxLoudness = v);
        KeepItDownPlugin.Bind(name, gameObject, __instance.minLoudness, v => __instance.minLoudness = v);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BoomboxItem), nameof(BoomboxItem.Start))]
    static void ItemActivatePatch(BoomboxItem __instance) {
        KeepItDownPlugin.BindAudioSource("Boombox", __instance.boomboxAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WalkieTalkie), nameof(WalkieTalkie.Start))]
    static void StartPatch(WalkieTalkie __instance) {
        KeepItDownPlugin.BindAudioSource("Walkie-talkie", __instance.thisAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FlashlightItem), nameof(FlashlightItem.Start))]
    static void StartPatch(FlashlightItem __instance) {
        KeepItDownPlugin.BindAudioSource("Flashlight", __instance.flashlightAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Start))]
    static void StartPatch(GrabbableObject __instance) {
        if (__instance is RemoteProp remoteProp) {
            KeepItDownPlugin.BindAudioSource("Remote", remoteProp.remoteAudio);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HUDManager), "OnEnable")]
    static void OnEnablePatch(HUDManager __instance) {
        KeepItDownPlugin.BindAudioSource("Scan", __instance.UIAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HUDManager), "OnDisable")]
    static void OnDisablePatch(HUDManager __instance) {
        KeepItDownPlugin.RemoveBindings("Scan", __instance.UIAudio.gameObject);
    }
}