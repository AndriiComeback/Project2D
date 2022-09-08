using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AnimationController))]
public class Destructable : MonoBehaviour
{
    [SerializeField] private int maxHealth;
	[SerializeField] private GameObject deathFx;

	private AnimationController animationController;
	private int health;


	private void Awake() {
		animationController = GetComponent<AnimationController>();
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
			GameObject fx = Instantiate(deathFx, transform.position, transform.rotation);
			fx.transform.parent = null;
			Destroy(gameObject);
		}
	}
}
