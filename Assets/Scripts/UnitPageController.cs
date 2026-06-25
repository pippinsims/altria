using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitPageController : MonoBehaviour
{
    public Text healthText;
    public Text attackText;
    public Text defenseText;
    public Text rangeText;
    public Text moveText;
    private TeamManager tm;
    public Unit currentUnit;

    void Start()
    {
        healthText = transform.Find("Health Text").GetComponent<Text>();
        attackText = transform.Find("Attack Text").GetComponent<Text>();
        defenseText = transform.Find("Defense Text").GetComponent<Text>();
        rangeText = transform.Find("Range Text").GetComponent<Text>();
        moveText = transform.Find("Move Text").GetComponent<Text>();
        tm = GameObject.Find("Team Manager").GetComponent<TeamManager>();
    }

    void Update()
    {
        if(
            gameObject.GetComponent<Image>().enabled = 
            defenseText.enabled = 
            healthText.enabled = 
            attackText.enabled = 
            rangeText.enabled = 
            moveText.enabled = 
            currentUnit != null
        )
        {
            defenseText.text = "D:"  + currentUnit.defense;
            healthText.text  = "HP:" + currentUnit.currentHealth + "/" + currentUnit.maxHealth;
            attackText.text  = "A:"  + currentUnit.strength;
            rangeText.text   = "R:"  + currentUnit.range;
            moveText.text    = "M:"  + (currentUnit.move - 1);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) //TODO: add right click on nobody check
        {
            currentUnit = null;
        }
    }
}