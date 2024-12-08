using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PercentageSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI percentageText;

    void Awake() {
        slider = GetComponent<Slider>();
        SetPercentage();
    }

    public void SetPercentage() {
        percentageText.text = (slider.value * 100).ToString("F2") + "%";
    }
}
