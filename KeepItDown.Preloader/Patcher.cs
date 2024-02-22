using System.Collections.Generic;
using KeepItDown;
using Mono.Cecil;

namespace ComponentBundler;

public static class Patcher {
    public static IEnumerable<string> TargetDLLs {
        get {
            yield return "Assembly-CSharp.dll";
        }
    }
    
    public static void Patch(AssemblyDefinition assembly) {
        ComponentBundling.Bundle<MyBehaviour>(assembly,"GrabbableObject");
    }
}