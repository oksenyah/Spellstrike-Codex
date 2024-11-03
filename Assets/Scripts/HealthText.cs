using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthText : MonoBehaviour
{

    TextMeshProUGUI text;

    [SerializeField] Wizard wizard;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = wizard.CurrentHealth().ToString() + "/" + wizard.MaxHealth().ToString();
    }
}
