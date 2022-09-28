using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	private void Start() {
		Time.timeScale = 1.0f;
		GameController.Instance.AudioManager.PlayMusic(true);
	}
	public void Play() {
		SceneManager.LoadScene("Level01", LoadSceneMode.Single);
	}
	public void Options() {
		
	}
	public void Exit() {
		Application.Quit();
	}
}
