using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Audio
{
	#region Private_Variables
	//Посилання на джерело звуку для відтворення звуків
	[SerializeField] private AudioSource sourceSFX;
	//Посилання на джерело звуку для відтворення музики
	[SerializeField] private AudioSource sourceMusic;
	//Посилання на джерело звуку для відтворення звуків
	//з випадковою частотою
	[SerializeField] private AudioSource sourceRandomPitchSFX;
	//Гучність музики
	private float musicVolume = 1f;
	//Гучність звуків
	private float sfxVolume = 1f;
	//Масив звуків
	[SerializeField] private AudioClip[] sounds;
	//Звук за замовчуванням, на випадок, якщо в масиві відсутній необхідний
	[SerializeField] private AudioClip defaultClip;
	//Музика для головного меню
	[SerializeField] private AudioClip menuMusic;
	//Музика для гри на рівнях
	[SerializeField] private AudioClip gameMusic;
	#endregion
	public AudioSource SourceSFX { set { sourceSFX = value; } get { return sourceSFX; } }
	public AudioSource SourceRandomPitchSFX { set { sourceRandomPitchSFX = value; } get { return sourceRandomPitchSFX; } }
	public AudioSource SourceMusic { set { sourceMusic = value; } get { return sourceMusic; } }
	public float MusicVolume {
		get {
			return musicVolume;
		}
		set {
			musicVolume = value;
			SourceMusic.volume = musicVolume;
		}
	}
	public float SfxVolume {
		get {
			return sfxVolume;
		}
		set {
			sfxVolume = value;
			SourceSFX.volume = sfxVolume;
			SourceRandomPitchSFX.volume = sfxVolume;
		}
	}

	private AudioClip GetSound(string clipName) {
		for (int i = 0; i < sounds.Length; i++) {
			if (sounds[i].name == clipName) {
				return sounds[i];
			}
		}
		Debug.LogError("Can not find clip " + clipName);
		return defaultClip;
	}
	public void PlaySound(string clipName) {
		SourceSFX.PlayOneShot(GetSound(clipName), SfxVolume);
	}
	public void PlaySoundRandomPitch(string clipName) {
		SourceRandomPitchSFX.pitch = Random.Range(0.7f, 1.3f);
		SourceRandomPitchSFX.PlayOneShot(GetSound(clipName), SfxVolume);
	}
	public void PlayMusic(bool menu) {
		if (menu) {
			SourceMusic.clip = menuMusic;
		} else {
			SourceMusic.clip = gameMusic;
		}
		SourceMusic.volume = MusicVolume;
		SourceMusic.loop = true;
		SourceMusic.Play();
	}

}
