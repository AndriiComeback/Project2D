using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    private int health;

    private void Awake() {
        health = maxHealth;
	}
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.otherCollider.CompareTag("Damager")) {
            Damager damager = collision.otherCollider.GetComponent<Damager>();
            if (damager != null) {
                TakeDamage(damager.Damage);
            }
		}
    }

    private void TakeDamage(int value) {
        health -= value;
        if (health <= 0) {
            health = 0;
            Debug.Log("DIED!");
            // to do - Die
		}
	}
}
