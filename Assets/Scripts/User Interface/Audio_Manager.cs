using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Audio_Manager : MonoBehaviour {

	public AudioSource sfxSource;
	public AudioSource musicSource;
	public Slider musicSlider;
	public Slider sfxSlider;
	public static Audio_Manager instance = null;
	public float lowPitchRange = .95f;
	public float highPitchRange = 1.05f;
	

	void Awake () 
	{

		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);

	}

	void Update()
	{

		musicSource.volume = musicSlider.value;

		sfxSource.volume = sfxSlider.value;

	}

	public void PlaySingle(AudioClip Clip)
	{

		sfxSource.clip = Clip;
		sfxSource.Play ();

	}

	public void RandomSFX(params AudioClip[] clips)
	{

		int randomIndex = Random.Range (0, clips.Length);

		float randomPitch = Random.Range (lowPitchRange, highPitchRange);

		sfxSource.pitch = randomPitch;

		sfxSource.clip = clips [randomIndex];

		sfxSource.Play ();

	}

}
