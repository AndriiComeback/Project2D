using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportOnTriggerEnter : MonoBehaviour
{
    [SerializeField] Vector3 destination;
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            other.gameObject.transform.position = destination;
		}
    }
}
