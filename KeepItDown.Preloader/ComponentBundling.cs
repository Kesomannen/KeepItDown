using System;
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace ComponentBundler;

public static class ComponentBundling {
    static readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("ComponentBundlingPatcher");
    
    public static bool Bundle<T>(AssemblyDefinition targetAssembly, string targetComponentName) where T : MonoBehaviour {
        return Bundle(targetAssembly, typeof(T), targetComponentName);
    }

    public static bool Bundle(
        AssemblyDefinition targetAssembly,
        Type componentToAdd,
        string targetComponentFullName
    ) {
        if (!componentToAdd.IsSubclassOf(typeof(MonoBehaviour))) {
            _logger.LogError($"{componentToAdd.Name} must be a subclass of MonoBehaviour to be bundled!");
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

        var methodDefinition = new MethodDefinition(
            "Awake",
            MethodAttributes.Private,
            targetAssembly.MainModule.TypeSystem.Void
        );

        targetTypeDefinition.Methods.Add(methodDefinition);

        var gameObjectField = targetAssembly.MainModule.ImportReference(
            typeof(GameObject).GetProperty(nameof(Component.gameObject))!.GetMethod
        );

        var addComponentMethod = targetAssembly.MainModule.ImportReference(
            typeof(GameObject).GetMethod(nameof(GameObject.AddComponent), Array.Empty<Type>())!
                .MakeGenericMethod(componentToAdd)
        );

        var worker = methodDefinition.Body.GetILProcessor();

        worker.Emit(OpCodes.Ldarg_0); // this
        worker.Emit(OpCodes.Call, gameObjectField); // .gameObject
        worker.Emit(OpCodes.Callvirt, addComponentMethod); // .AddComponent<TToAdd>()
        worker.Emit(OpCodes.Pop);
        worker.Emit(OpCodes.Ret); // return

        _logger.LogInfo($"Bundled {componentToAdd.Name} with {targetComponentFullName}");
        return true;
    }
}