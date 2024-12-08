using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ScreenSettings : MonoBehaviour
{
    [SerializeField] TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;

    void Start() {
        resolutions = Screen.resolutions;
        int currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Length - 1);
        for (int i = 0; i < resolutions.Length; i++) {
            string resolutionString = resolutions[i].width.ToString() + "x" + resolutions[i].height.ToString();
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(resolutionString));
        }
        currentResolutionIndex = Math.Min(currentResolutionIndex, resolutions.Length - 1);
        resolutionDropdown.value = currentResolutionIndex;
        SetResolution();
    }

    public void SetResolution() {
        int resolutionIndex = resolutionDropdown.value;
        Screen.SetResolution(resolutions[resolutionIndex].width, resolutions[resolutionIndex].width, true);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
    }
    
}
