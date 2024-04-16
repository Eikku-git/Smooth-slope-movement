using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {
    public static Camera main { get; private set; }
    public static Quaternion localRotation { private get; set; }
    public static Quaternion lookRotation { private get; set; }
    public static Vector3 localPosition { private get; set; }

    private void Awake() {
        main = Camera.main;
    }

    private void Update() {
        StartCoroutine(UpdateRotation());
    }

    public IEnumerator UpdateRotation() {
        yield return new WaitForEndOfFrame();
        main.transform.SetLocalPositionAndRotation(localPosition, lookRotation * localRotation);
    }
}
