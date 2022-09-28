using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrystallType { Random, Red, Green, Blue }
public class Chest : MonoBehaviour
{
	[SerializeField] private CrystallType content;
	[SerializeField] private int amount;
	private void OnTriggerEnter2D(Collider2D collider) {
		CharacterController player = collider.gameObject.GetComponent<CharacterController>();
		if (player != null) {
			if (content == CrystallType.Random) {
				content = (CrystallType)Random.Range(1,
				4);
			}
			if (amount == 0) {
				amount = Random.Range(1, 6);
			}
			GameController.Instance.AddNewInventoryItem(content, amount);
			Destroy(gameObject);
		}
	}

}
