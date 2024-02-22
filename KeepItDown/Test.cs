using System;
using ComponentBundler;
using UnityEngine;

namespace KeepItDown; 

public class Test : MonoBehaviour {
    void Awake() {
        var bundle = ComponentBundling.BundledComponents["targetComponentFullName"];
       for (var i = 0; i < bundle.Count; i++) { 
           gameObject.AddComponent(Type.GetType(bundle[i]));
        }
    }
}