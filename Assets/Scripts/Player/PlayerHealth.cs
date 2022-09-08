using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerShootingController))]
public class PlayerHealth : MonoBehaviour
{
	[SerializeField] private int maxHealth;
	[SerializeField] private GameObject deathFx;
	[SerializeField] private List<GameObject> healthBars;
	private PlayerMovementController playerMovementController;
	private PlayerShootingController playerShotingController;
	private int health;
	private int currentHealthImage;

	private void Start() {
		playerMovementController = GetComponent<PlayerMovementController>();
		playerShotingController = GetComponent<PlayerShootingController>();
		health = maxHealth;
		currentHealthImage = maxHealth - 1;
		if (healthBars.Count < maxHealth) {
			Debug.LogException(new System.Exception("Health bars not enough!"));
		}
		foreach (GameObject bar in healthBars) {
			bar.SetActive(false);
		}
		for (int i = 0; i < maxHealth; i++) {
			healthBars[i].SetActive(true);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		string tag = collision.gameObject.tag;
		if (string.Equals(tag, "Enemy") || string.Equals(tag, "Damager")) {
			Damager damager = collision.gameObject.GetComponent<Damager>();
			if (damager != null) {
				TakeDamage(damager.Damage);
			}

		}
	}
	private void TakeDamage(int value) {
		health -= value;
		healthBars[currentHealthImage--].SetActive(false);
		if (health <= 0) {
			health = 0;
			GameObject fx = Instantiate(deathFx, transform.position, transform.rotation);
			fx.transform.parent = null;
			Destroy(gameObject);
		}
		playerMovementController.Hurt();
		playerShotingController.LockShoot();
	}
}
