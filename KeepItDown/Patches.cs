using System;
using System.Collections;
using HarmonyLib;
using LethalSettings.UI;
using Object = UnityEngine.Object;

namespace KeepItDown; 

internal static class Patches {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NoisemakerProp), nameof(NoisemakerProp.Start))]
    static void NoiseMakerProp_Start_Postfix(NoisemakerProp __instance) {
        var gameObject = __instance.gameObject;
        var name = gameObject.name.Replace("(Clone)", "").Replace("Item", "");
        KeepItDownPlugin.Bind(name, gameObject, __instance.maxLoudness, v => __instance.maxLoudness = v);
        KeepItDownPlugin.Bind(name, gameObject, __instance.minLoudness, v => __instance.minLoudness = v);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BoomboxItem), nameof(BoomboxItem.Start))]
    static void BoomboxItem_Start_Postfix(BoomboxItem __instance) {
        KeepItDownPlugin.BindAudioSource("Boombox", __instance.boomboxAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WalkieTalkie), nameof(WalkieTalkie.Start))]
    static void WalkieTalkie_Start_Postfix(WalkieTalkie __instance) {
        KeepItDownPlugin.BindAudioSource("Walkie-talkie", __instance.thisAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FlashlightItem), nameof(FlashlightItem.Start))]
    static void FlashlightItem_Start_Postfix(FlashlightItem __instance) {
        KeepItDownPlugin.BindAudioSource("Flashlight", __instance.flashlightAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Start))]
    static void GrabbableObject_Start_Postfix(GrabbableObject __instance) {
        if (__instance is RemoteProp remoteProp) {
            KeepItDownPlugin.BindAudioSource("Remote", remoteProp.remoteAudio);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HUDManager), "OnEnable")]
    static void HUDManager_OnEnable_Postfix(HUDManager __instance) {
        KeepItDownPlugin.BindAudioSource("Scan", __instance.UIAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HUDManager), "OnDisable")]
    static void HUDManager_OnDisable_Postfix(HUDManager __instance) {
        KeepItDownPlugin.RemoveBindings("Scan", __instance.UIAudio.gameObject);
    }
}