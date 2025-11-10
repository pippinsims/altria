using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    public Unit myUnit;
    public Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        myUnit = transform.parent.parent.gameObject.GetComponent<Unit>();
        slider.maxValue = myUnit.maxHealth;
    }

    void Update()
    {
        slider.value = myUnit.currentHealth;
    }
}
