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
        bool unitIsSelected = (tm.selectedUnitIndex >= 0);
        
            gameObject.GetComponent<Image>().enabled = false;
            healthText.enabled = false;
            attackText.enabled = false;
            defenseText.enabled = false;
            rangeText.enabled = false;
            moveText.enabled = false;
        if(unitIsSelected)
        {
            currentUnit = tm.teams[tm.currentTeamIndex].members[tm.selectedUnitIndex];
            if(currentUnit.showUI)
            {
                gameObject.GetComponent<Image>().enabled = unitIsSelected;
                healthText.enabled = unitIsSelected;
                attackText.enabled = unitIsSelected;
                defenseText.enabled = unitIsSelected;
                rangeText.enabled = unitIsSelected;
                moveText.enabled = unitIsSelected;
                healthText.text = ("HP:" + currentUnit.currentHealth + "/" + currentUnit.maxHealth);
                attackText.text = ("A:" + currentUnit.attack);
                defenseText.text = ("D:" + currentUnit.defense);
                rangeText.text = ("R:" + currentUnit.range);
                moveText.text = ("M:" + (currentUnit.move - 1));
            }
        }
        
        
    }
}
