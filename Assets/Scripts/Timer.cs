using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    float timer = 0.0f;

    void FixedUpdate() {
        timer += Time.fixedDeltaTime;
    }

    public int GetMinutes() {
        return Mathf.FloorToInt(timer / 60);
    }

    public int GetSeconds() {
        return Mathf.FloorToInt(timer % 60);
    }

    public int GetMilliSeconds() {
        return Mathf.FloorToInt(timer * 1000);
    }

    public void ResetTimer() {
        timer = 0f;
    }
}
