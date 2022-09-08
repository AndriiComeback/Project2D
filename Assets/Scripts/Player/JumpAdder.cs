using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpAdder : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == 6) { // AddsJump
            GameObject.FindWithTag("Player").GetComponent<PlayerMovementController>().SetJump();
		}
    }
}
