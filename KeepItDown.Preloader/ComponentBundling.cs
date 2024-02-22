using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace ComponentBundler;

public static class ComponentBundling {
    public static readonly ManualLogSource _logger = Logger.CreateLogSource("ComponentBundlingPatcher");
    
    public static Dictionary<string, List<string>> BundledComponents = new();
    
    public static bool Bundle(
        AssemblyDefinition targetAssembly,
        AssemblyDefinition unityAssembly,
        string targetComponentFullName,
        string toAddAssemblyQualifiedName
    ) {
        if (BundledComponents.TryGetValue(targetComponentFullName, out var bundle)) {
            bundle.Add(toAddAssemblyQualifiedName);
            _logger.LogInfo($"Bundled {toAddAssemblyQualifiedName} with {targetComponentFullName}");
            return true;
        }
        
        if (targetAssembly == null) {
            _logger.LogError("Target assembly is null!");
            return false;
        }
        
        if (unityAssembly == null) {
            _logger.LogError("Unity assembly is null!");
            return false;
        }

        var targetTypeDefinition = targetAssembly.MainModule.GetType(targetComponentFullName);
        if (targetTypeDefinition == null) {
            var message = $"Type {targetComponentFullName} not found in assembly {targetAssembly.Name.Name}.";

            if (!targetComponentFullName.Contains('.')) {
                message += " Did you forget to include the namespace?";
            }
            
            _logger.LogError(message);
            return false;
        }

        var methodDefinition = targetTypeDefinition.Methods.FirstOrDefault(m => m.Name == "Awake");
        
        if (methodDefinition == null) {
            methodDefinition = new MethodDefinition(
                "Awake",
                MethodAttributes.Private,
                targetAssembly.MainModule.TypeSystem.Void
            );

            targetTypeDefinition.Methods.Add(methodDefinition);
        }

        var gameObjectType = unityAssembly.MainModule.GetType("UnityEngine.GameObject")!;
        
        var il = methodDefinition.Body.GetILProcessor();
        
        var stringListReference = targetAssembly.MainModule.ImportReference(typeof(List<string>));
        var bundleVariable = new VariableDefinition(stringListReference);
        methodDefinition.Body.Variables.Add(bundleVariable);
        
        var iVariable = new VariableDefinition(targetAssembly.MainModule.TypeSystem.Int32);
        methodDefinition.Body.Variables.Add(iVariable);
        
        // bundle = ComponentBundling.BundledComponents[<targetComponentFullName>];
        il.Emit(OpCodes.Ldsfld, targetAssembly.MainModule.ImportReference(
            typeof(ComponentBundling).GetField(nameof(BundledComponents))
        ));  // ComponentBundling.BundledComponents
        il.Emit(OpCodes.Ldstr, targetComponentFullName); // <targetComponentFullName>
        il.Emit(OpCodes.Callvirt, targetAssembly.MainModule.ImportReference(
            typeof(Dictionary<string, List<string>>).GetMethod("get_Item")
        )); // []
        il.Emit(OpCodes.Stloc_0); // bundle =
        
        // Initialize i
        il.Emit(OpCodes.Ldc_I4_0); // 0
        il.Emit(OpCodes.Stloc_1); // i 
        
        var loopStart = il.Create(OpCodes.Nop);
        il.Append(loopStart);
        
        // gameObject.AddComponent(Type.GetType(bundle[i]));
        il.Emit(OpCodes.Ldarg_0); // this
        il.Emit(OpCodes.Call, targetAssembly.MainModule.ImportReference(
            gameObjectType.Properties.First(p => p.Name == "gameObject").GetMethod
        )); // .gameObject
        il.Emit(OpCodes.Ldloc_0); // bundle
        il.Emit(OpCodes.Ldloc_1); // i
        il.Emit(OpCodes.Callvirt, targetAssembly.MainModule.ImportReference(
            typeof(List<string>).GetMethod("get_Item")
        )); // bundle[i]
        il.Emit(OpCodes.Call, targetAssembly.MainModule.ImportReference(
            typeof(Type).GetMethod(nameof(Type.GetType), new[] { typeof(string) })
        )); // Type.GetType
        il.Emit(OpCodes.Callvirt, targetAssembly.MainModule.ImportReference(
            gameObjectType.Methods.First(m => m.Name == "AddComponent" && !m.HasGenericParameters)
        )); // .AddComponent
        il.Emit(OpCodes.Pop); // pop result
        
        // i++
        il.Emit(OpCodes.Ldloc_1); // i
        il.Emit(OpCodes.Ldc_I4_1); // 1
        il.Emit(OpCodes.Add); // i + 1
        il.Emit(OpCodes.Stloc_1); // pop i
        
        // Loop condition
        il.Emit(OpCodes.Ldloc_1); // i
        il.Emit(OpCodes.Ldloc_0); // bundle
        il.Emit(OpCodes.Callvirt, targetAssembly.MainModule.ImportReference(
            typeof(List<string>).GetProperty(nameof(List<string>.Count))!.GetMethod
        )); // bundle.Count
        il.Emit(OpCodes.Blt, loopStart); // i < bundle.Count
        
        il.Emit(OpCodes.Ret); // return
        
        _logger.LogInfo($"Patched {targetComponentFullName} with bundling logic");
        
        BundledComponents.Add(targetComponentFullName, new List<string> { toAddAssemblyQualifiedName });
        _logger.LogInfo($"Bundled {toAddAssemblyQualifiedName} with {targetComponentFullName}");
        return true;
    }
}