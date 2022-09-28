using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {
	public TMPro.TMP_Text scoreLabel;
	public Slider HealthBar;
	public static HUD Instance { private set; get; }
	[SerializeField] GameObject inventoryWindow;
	[SerializeField] InventoryItem inventoryItemPrefab;
	[SerializeField] Transform inventoryContainer;
	[SerializeField] GameObject LevelWonWindow;
	private void Awake() {
		Instance = this;
	}
	public void SetScore(string scoreValue) {
		scoreLabel.text = scoreValue;
	}
	public void ShowWindow(GameObject window) {
		window.GetComponent<Animator>().SetBool("Open", true);
		GameController.Instance.State = GameState.Pause;

	}
	public void HideWindow(GameObject window) {
		window.GetComponent<Animator>().SetBool("Open", false);
		GameController.Instance.State = GameState.Play;
	}
	public InventoryItem AddNewInventoryItem(CrystallType crystallType, int amount) {
		InventoryItem newItem = Instantiate(inventoryItemPrefab) as InventoryItem;
		newItem.transform.SetParent(inventoryContainer);
		newItem.Quantity = amount;
		newItem.CrystallType = crystallType;
		return newItem;
	}
	public void ButtonNext() {
		GameController.Instance.LoadNextLevel();
	}
	public void ButtonRestart() {
		GameController.Instance.RestartLevel();
	}
	public void ButtonMainMenu() {
		GameController.Instance.LoadMainMenu();
	}
	public void ShowLevelWonWindow() {
		ShowWindow(LevelWonWindow);
	}
}

