using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerText : MonoBehaviour
{

    [Header("Inputs")]
    [SerializeField] Timer timer;
    TextMeshProUGUI text;

    void Awake() {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        text.text = string.Format("{0:00}:{1:00}", timer.GetMinutes(), timer.GetSeconds());
    }
}
