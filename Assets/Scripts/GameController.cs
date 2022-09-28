using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum GameState { Play, Pause }
public class GameController : MonoBehaviour {
	static private GameController instance;
	public static GameController Instance { get { return instance; } }
	public GameState state;
	public GameState State {
		get {
			return state;
		}
		set {
			if (value == GameState.Play) {
				Time.timeScale = 1.0f;
			} else {
				Time.timeScale = 0.0f;
			}
			state = value;
		}
	}
	private int score;
	private int Score {
		get {
			return score;
		}
		set {
			if (value != score) {
				score = value;
				HUD.Instance.SetScore(score.ToString());
			}
		}
	}
	private List<InventoryItem> inventory;

	public delegate void InventoryUsedCallback(InventoryItem item);
	private void Awake() {
		instance = this;
		state = GameState.Play;
		inventory = new List<InventoryItem>();
		Time.timeScale = 1.0f;
	}
	private void OnEnable() {
		//HUD.Instance.HealthBar.maxValue = 5;
		//HUD.Instance.HealthBar.value = 5;
		//HUD.Instance.SetScore(Score.ToString());
	}
	public void Hit(IDestructable victim) {
		if (victim.GetType() == typeof(Enemy)) {
			if (victim.Health > 0) {
				Score += 10;
			} else {
				Score += 50;
			}
		}
		if (victim.GetType() == typeof(CharacterController)) {
			HUD.Instance.HealthBar.value = victim.Health;
		}
	}
	public void AddNewInventoryItem(CrystallType type, int amount) {
		InventoryItem newItem = HUD.Instance.AddNewInventoryItem(type, amount);
		InventoryUsedCallback callback = new InventoryUsedCallback(InventoryItemUsed);
		newItem.Callback = callback;
		inventory.Add(newItem);
	}
	public void InventoryItemUsed(InventoryItem item) {
		switch (item.CrystallType) {
			case CrystallType.Blue:
			break;
			case CrystallType.Red:
			break;
			case CrystallType.Green:
			break;
			default:
			Debug.LogError("Wrong crystall type!");
			break;
		}
		inventory.Remove(item);
		Destroy(item.gameObject);
	}
	public void LoadNextLevel() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1,
		LoadSceneMode.Single);
	}
	public void RestartLevel() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
	}
	public void LoadMainMenu() {
		SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
	}
	public void PrincessFound() {
		HUD.Instance.ShowLevelWonWindow();
	}
}