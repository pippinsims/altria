using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public List<Team> teams = new List<Team>();
    public int currentTeamIndex = -1;
    public int selectedUnitIndex = -1; //-1 if there is no selected unit
    public BoardController bc;
    public SpriteRenderer sr;

    void Start()
    {
        bc = GameObject.Find("Board").GetComponent<BoardController>();
        GameObject[] gs = GameObject.FindGameObjectsWithTag("Unit");
        foreach(GameObject g in gs)
        {
            Unit u = g.GetComponent<Unit>();
            while(teams.Count < u.team+1)
            {
                teams.Add(new Team(false));
            }
            teams[u.team].members.Add(u);
        }
        //will be set at start of level
        teams[1].isAi = true;
        //teams[0].isAi = true;

        sr = GetComponent<SpriteRenderer>();
        currentTeamIndex = 0;
        BeginTeamTurn(teams[currentTeamIndex]);
        bc.UpdateAllUnitSquares();
    }

    private void BeginTeamTurn(Team teamToBegin)
    {
        foreach(Unit unit in teamToBegin.members)
        {
            unit.isMyTurn = true;
        }

        if(currentTeamIndex >= 0)
        {
            sr.color = GetTeamColor(currentTeamIndex);
        }

        if(teamToBegin.isAi)
        {
            RunAiTurn();
        }
    }

    void RunAiTurn()
    {
        Team currentTeam = teams[currentTeamIndex];
        Unit currentUnit = currentTeam.members[0];
        UpdateSelectedUnit(currentUnit);
    }

    public void MaybeSelectNextAiUnit()
    {
        if(selectedUnitIndex != teams[currentTeamIndex].members.Count - 1)
        {
            UpdateSelectedUnit(teams[currentTeamIndex].members[selectedUnitIndex + 1]);
        }
    }

    private void EndTeamTurn(Team teamToEnd)
    {
        foreach(Unit unit in teamToEnd.members)
        {
            unit.hasAttacked = false;
            unit.hasMoved = false;
        }
    }

    public void MaybeUpdateCurrentTeam()
    {
        if(CheckTeamIsDone())
        {   
            EndTeamTurn(teams[currentTeamIndex]);
            if(currentTeamIndex < teams.Count - 1)
            {
                currentTeamIndex++;
            }else
            {
                currentTeamIndex = 0;
            }
            selectedUnitIndex = -1;
            
            BeginTeamTurn(teams[currentTeamIndex]); 
        }
    }

    public void UpdateSelectedUnit(Unit newUnit)
    {
        if(newUnit.team == currentTeamIndex)
        {
            selectedUnitIndex = teams[currentTeamIndex].members.IndexOf(newUnit);
            bc.currentUnit = teams[currentTeamIndex].members[selectedUnitIndex];
            newUnit.isSelected = true;
        }
    }

    private bool CheckTeamIsDone()
    {
        foreach(Unit unit in teams[currentTeamIndex].members)
        {
            if(unit.isMyTurn)
            {
                return false;
            }
        }
        return true;
    }


    public void RemoveUnit(Unit u, int teamIndex)
    {
        teams[teamIndex].members.Remove(u);
        if(teams[teamIndex].members.Count <= 0)
        {
            teams.Remove(teams[teamIndex]);
            ResetTeamIndexes(teamIndex);
            if(currentTeamIndex > teamIndex)
            {
                currentTeamIndex--;
            }
        }
    }

    void ResetTeamIndexes(int deletedTeamIndex)
    {
        foreach(Team t in teams)
        {
            foreach(Unit unit in t.members)
            {
                unit.team = teams.IndexOf(t);
            }
        }
    }

    Color GetTeamColor(int index)
    {
        return teams[index].members[0].savedColor;
    }
}
