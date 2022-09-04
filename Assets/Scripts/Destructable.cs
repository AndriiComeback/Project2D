using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    private int health;

    [SerializeField] private GameObject deathFx;

    private void Awake() {
        health = maxHealth;
	}

    private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.CompareTag("Damager")) {
            Damager damager = collision.gameObject.GetComponent<Damager>();
            if (damager != null) {
				TakeDamage(damager.Damage);
			}
		}
	}

    private void TakeDamage(int value) {
        health -= value;
        if (health <= 0) {
            health = 0;
            Destroy(gameObject);
		}
	}
    private void OnDestroy() {
        GameObject fx = Instantiate(deathFx, transform.position, transform.rotation);
        fx.transform.parent = null;
        Destroy(fx, .5f);
    }
}
