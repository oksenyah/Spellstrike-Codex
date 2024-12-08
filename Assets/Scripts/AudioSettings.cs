using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;

    void Start() {
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SfxVolume", 0f);
        SetMasterVolume();
        SetMusicVolume();
        SetSfxVolume();
    }

    public void SetVolume(String groupName, float value) {
        float adjustedVolume = Mathf.Log10(value) * 20;
        if (value == 0) {
            adjustedVolume = -80;
        }
        audioMixer.SetFloat(groupName, adjustedVolume);
    }

    public void SetMasterVolume() {
        SetVolume("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
    }

    public void SetMusicVolume() {
        SetVolume("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
    }

    public void SetSfxVolume() {
        SetVolume("SfxVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolumeSlider.value);
    }
    
}
