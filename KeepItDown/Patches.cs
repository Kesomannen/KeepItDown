using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace KeepItDown; 

internal static class Patches {
    static string GetFormattedName(Object gameObject) {
        return gameObject.name.Replace("(Clone)", "").Replace("Item", "");
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NoisemakerProp), nameof(NoisemakerProp.Start))]
    static void NoiseMakerProp_Start_Postfix(NoisemakerProp __instance) {
        var gameObject = __instance.gameObject;
        var name = GetFormattedName(gameObject);
        KeepItDownPlugin.Bind(name, gameObject, __instance.maxLoudness, v => __instance.maxLoudness = v);
        KeepItDownPlugin.Bind(name, gameObject, __instance.minLoudness, v => __instance.minLoudness = v);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AnimatedItem), nameof(AnimatedItem.Start))]
    static void AnimatedItem_Start_Postfix(AnimatedItem __instance) {
        var name = GetFormattedName(__instance.gameObject);
        KeepItDownPlugin.BindAudioSource(name, __instance.itemAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RadarBoosterItem), nameof(RadarBoosterItem.Start))]
    static void RadarBoosterItem_Start_Postfix(RadarBoosterItem __instance) {
        KeepItDownPlugin.BindAudioSource("RadarBoosterPing", __instance.pingAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShipAlarmCord), "Start")]
    static void ShipAlarmCord_Start_Postfix(ShipAlarmCord __instance) {
        KeepItDownPlugin.BindAudioSources("ShipAlarm", __instance.hornClose, __instance.hornFar);
        KeepItDownPlugin.BindAudioSource("ShipAlarmCord", __instance.cordAudio);
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
    [HarmonyPatch(typeof(SprayPaintItem), nameof(SprayPaintItem.Start))]
    static void SprayPaintItem_Start_Postfix(SprayPaintItem __instance) {
        KeepItDownPlugin.BindAudioSource("Spraycan", __instance.sprayAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Landmine), "Start")]
    static void Landmine_Start_Postfix(Landmine __instance) {
        KeepItDownPlugin.BindAudioSource("Landmine", __instance.mineAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Turret), "Start")]
    static void Turret_Start_Postfix(Turret __instance) {
        KeepItDownPlugin.BindAudioSources("Turret", __instance.mainAudio, __instance.berserkAudio, __instance.farAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(JesterAI), nameof(JesterAI.Start))]
    static void JesterAI_Start_Postfix(JesterAI __instance) {
        KeepItDownPlugin.BindAudioSource("Jester", __instance.farAudio);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StormyWeather), "OnEnable")]
    static void StormyWeather_OnEnable_Postfix(StormyWeather __instance) {
        KeepItDownPlugin.BindAudioSources(
            "Thunder",
            __instance.randomStrikeAudio,
            __instance.randomStrikeAudioB,
            __instance.targetedStrikeAudio
        );  
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StormyWeather), "OnDisable")]
    static void StormyWeather_OnDisable_Postfix(StormyWeather __instance) {
        KeepItDownPlugin.RemoveBindings("Thunder", __instance.gameObject);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Start))]
    static void GrabbableObject_Start_Postfix(GrabbableObject __instance) {
        switch (__instance) {
            case RemoteProp remoteProp:
                KeepItDownPlugin.BindAudioSource("Remote", remoteProp.remoteAudio);
                break;
            case JetpackItem jetpackItem:
                KeepItDownPlugin.BindAudioSource("Jetpack", jetpackItem.jetpackAudio);
                break;
            case Shovel shovel:
                KeepItDownPlugin.BindAudioSource("Shovel", shovel.shovelAudio);
                break;
            case WhoopieCushionItem whoopieCushionItem:
                KeepItDownPlugin.BindAudioSource("WhoopieCushion", whoopieCushionItem.whoopieCushionAudio);
                break;
            case ExtensionLadderItem extensionLadderItem:
                KeepItDownPlugin.BindAudioSource("ExtensionLadder", extensionLadderItem.ladderAudio);
                break;
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.OnNetworkSpawn))]
    static void NetworkBehaviour_OnNetworkSpawn_Postfix(NetworkBehaviour __instance) {
        switch (__instance) {
            case ItemCharger itemCharger:
                KeepItDownPlugin.BindAudioSource("ItemCharger", itemCharger.zapAudio);
                break;
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