using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroll : MonoBehaviour
{
	RectTransform r;
    float time = 0f;
    private void Start() {
        r = GetComponent<RectTransform>();
    }
    private void Update() {
        time += Time.deltaTime;
        r.position = new Vector3(r.position.x, time * 25 + 425, r.position.z);
    }
}
