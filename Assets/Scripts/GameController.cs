using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum GameState { Play, Pause }
public class GameController : MonoBehaviour {
	static private GameController _instance;
	public static GameController Instance {
		get {
			if (_instance == null) {
				GameObject gameController =
				Instantiate(Resources.Load("Prefabs/GameController")) as GameObject;
				_instance = gameController.GetComponent<GameController>();
			}
			return _instance;
		}
	}
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

	[SerializeField] private List<InventoryItem> inventory;
	public List<InventoryItem> Inventory { get { return inventory; } set { inventory = value; } }
	[SerializeField] Audio audioManager;
	public Audio AudioManager { set { audioManager = value; } get { return audioManager; } }

	public delegate void InventoryUsedCallback(InventoryUIButton item);
	private void Awake() {
		if (_instance == null) {
			_instance = this;
		} else {
			if (_instance != this) {
				Destroy(gameObject);
			}
		}
		DontDestroyOnLoad(gameObject);
		State = GameState.Play;
		inventory = new List<InventoryItem>();
		Time.timeScale = 1.0f;
		InitializeAudioManager();
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
	public void AddNewInventoryItem(InventoryItem itemData) {
		InventoryUIButton newUiButton = HUD.Instance.AddNewInventoryItem(itemData);
		InventoryUsedCallback callback = new InventoryUsedCallback(InventoryItemUsed);
		newUiButton.Callback = callback;
		inventory.Add(itemData);
	}
	public void InventoryItemUsed(InventoryUIButton item) {
		switch (item.itemData.crystallType) {
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
		inventory.Remove(item.itemData);
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
	private void InitializeAudioManager() {
		audioManager.SourceSFX = gameObject.AddComponent<AudioSource>();
		audioManager.SourceMusic = gameObject.AddComponent<AudioSource>();
		audioManager.SourceRandomPitchSFX = gameObject.AddComponent<AudioSource>();
		gameObject.AddComponent<AudioListener>();
		Scene scene = SceneManager.GetActiveScene();
		if (!Equals(scene.name, "MainMenu")) {
			GameController.Instance.AudioManager.PlayMusic(false);
		}
	}
}