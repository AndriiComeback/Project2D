using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnTime : MonoBehaviour
{
    [SerializeField] private float time = .5f;

    private void Start() {
        Destroy(gameObject, time);
    }
}
