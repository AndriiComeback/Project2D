using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameController;

public class HUD : MonoBehaviour {
	public TMPro.TMP_Text scoreLabel;
	public Slider HealthBar;
	public static HUD Instance { private set; get; }
	[SerializeField] GameObject inventoryWindow;
	[SerializeField] InventoryUIButton inventoryItemPrefab;
	[SerializeField] Transform inventoryContainer;
	[SerializeField] GameObject LevelWonWindow;
	private void Awake() {
		Instance = this;
		LoadInventory();
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
	public InventoryUIButton AddNewInventoryItem(InventoryItem itemData) {
		InventoryUIButton newItem = Instantiate(inventoryItemPrefab) as InventoryUIButton;
		newItem.transform.SetParent(inventoryContainer);
		newItem.ItemData = itemData;
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
	public void LoadInventory() {
		InventoryUsedCallback callback = new
		InventoryUsedCallback(GameController.Instance.InventoryItemUsed);
		for (int i = 0; i < GameController.Instance.Inventory.Count; i++) {
			InventoryUIButton newItem =
		   AddNewInventoryItem(GameController.Instance.Inventory[i]);

			newItem.Callback = callback;
		}
	}
	public void SetSoundVolume(Slider slider) {
		GameController.Instance.AudioManager.SfxVolume = slider.value;
	}
	public void SetMusicVolume(Slider slider) {
		GameController.Instance.AudioManager.MusicVolume = slider.value;
	}
}

