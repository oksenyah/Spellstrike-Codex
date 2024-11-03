using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBarIndicator : MonoBehaviour
{
    Image manaBarImage;

    [SerializeField] Wizard wizard;

    void Awake()
    {
        manaBarImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        manaBarImage.fillAmount = wizard.CurrentMana() / wizard.MaxMana();
    }
}
