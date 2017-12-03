using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Audio_Script : MonoBehaviour {

    public AudioSource volumeAudio;
    public Slider volumeSlider;


    void Update()
    {

        volumeAudio.volume = volumeSlider.value;

    }


}
