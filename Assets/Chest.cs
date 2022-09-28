using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrystallType { Random, Red, Green, Blue }
public class Chest : MonoBehaviour
{
	[SerializeField] private InventoryItem itemData;
	private void OnTriggerEnter2D(Collider2D collider) {
		CharacterController player = collider.gameObject.GetComponent<CharacterController>();
		if (player != null) {
			if (itemData.crystallType == CrystallType.Random) {
				itemData.crystallType = (CrystallType)Random.Range(1,
				4);
			}
			if (itemData.quantity == 0) {
				itemData.quantity = Random.Range(1, 6);
			}
			GameController.Instance.AddNewInventoryItem(itemData);
			Destroy(gameObject);
		}
	}

}
