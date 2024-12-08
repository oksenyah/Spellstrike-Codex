using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingText : MonoBehaviour
{
    TextMeshProUGUI text;
    Timer timer;
    [SerializeField] int intervalInMs = 250;
    [SerializeField] int maxEllipseCount = 3;
    int count = 0;
    string ellipsesString = "";

    void Awake() {
        text = GetComponent<TextMeshProUGUI>();
        timer = GetComponent<Timer>();
    }

    void Update() {
        int msSinceLastUpdate = timer.GetMilliSeconds();
        // Debug.Log("msSinceLastUpdate: " + msSinceLastUpdate);
        if (msSinceLastUpdate > intervalInMs) {
            if (count < maxEllipseCount) {
                // Debug.Log("Adding Ellipse");
                ellipsesString += ".";
                count++;
            } else {
                // Debug.Log("Resetting Ellipse");
                ellipsesString = "";
            }
            // Debug.Log("Resetting Timer");
            timer.ResetTimer();
        }
        
        text.text = string.Format("LOADING{0}", ellipsesString);
    }
}
