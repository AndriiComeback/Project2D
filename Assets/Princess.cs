using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Princess : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collider) {
		CharacterController player = collider.gameObject.GetComponent<CharacterController>();
		if (player != null) {
			GameController.Instance.PrincessFound();
		}
	}

}
