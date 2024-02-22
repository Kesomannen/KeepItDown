using UnityEngine;

namespace KeepItDown;

public class MyBehaviour : MonoBehaviour {
    void Awake() {
        KeepItDownPlugin.Instance.Log.LogInfo("Hello from MyBehaviour!");
    }
}