using System.Collections;
using HarmonyLib;
using LethalSettings.UI;

namespace KeepItDown; 

internal static class MenuPatches {
    readonly struct EnumeratorPatch : IEnumerable {
        readonly IEnumerator _enumerator;
        
        public IEnumerator GetEnumerator() {
            UI.Init();
            yield return _enumerator;
        }

        public EnumeratorPatch(IEnumerator enumerator) {
            _enumerator = enumerator;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ModMenu), "Start")]
    static void MenuManager_Start_Postfix(ref IEnumerator __result) {
        __result = new EnumeratorPatch(__result).GetEnumerator();
    }
}