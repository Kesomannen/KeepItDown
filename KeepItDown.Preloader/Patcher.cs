using System.Collections.Generic;
using Mono.Cecil;

namespace ComponentBundler;

public static class Patcher {
    public static IEnumerable<string> TargetDLLs {
        get {
            yield return "UnityEngine.CoreModule.dll";
            yield return "Assembly-CSharp.dll";
        }
    }
    
    static AssemblyDefinition _unityAssembly;
    
    public static void Patch(AssemblyDefinition assembly) {
        if (assembly.Name.Name == "UnityEngine.CoreModule") {
            _unityAssembly = assembly;
            return;
        }

        ComponentBundling.Bundle(
            assembly, 
            _unityAssembly,
            "GrabbableObject",
            "KeepItDown.MyBehaviour, KeepItDown"
        );
    }
}