using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarIndicator : MonoBehaviour
{
    Image healthBarImage;

    [SerializeField] Wizard wizard;

    void Awake()
    {
        healthBarImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        healthBarImage.fillAmount = wizard.CurrentHealth() / wizard.MaxHealth();
    }
}
