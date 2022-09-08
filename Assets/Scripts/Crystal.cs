using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crystal : MonoBehaviour
{
    [SerializeField] private int value = 1;
    private GameController gameController;

	private void Awake() {
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null) {
            gameController = gameControllerObject.GetComponent<GameController>();
            if (gameController == null) {
                Debug.LogException(new System.Exception("Crystal cannot find GameController"));
            }
        }
	}

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            gameController.AddCrystalCount(value);
            Destroy(gameObject);
		}
    }
}
