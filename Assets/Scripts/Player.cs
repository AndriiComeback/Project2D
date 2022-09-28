using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDestructable
{
	[SerializeField] private int health;
	int IDestructable.Health { get { return health; } }

	public void Hit(int damage) {
		health -= damage;
		GameController.Instance.Hit(this);
		if (health <= 0) {
			gameObject.SetActive(false);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.layer == 9) {
			Hit(1);
		}
	}
}
