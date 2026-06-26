using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    private int currentTeamIndex = -1;
    private int selectedUnitIndex = -1;
    private BoardController board;
    private SpriteRenderer sprite;
    
    public List<Team> teams = new();
    public List<Unit> CurrentTeam => currentTeamIndex > -1 ? teams[currentTeamIndex].members : null;
    public Unit SelectedUnit => selectedUnitIndex > -1 && CurrentTeam != null ? CurrentTeam[selectedUnitIndex] : null;

    void Start()
    {
        board = GameObject.Find("Board").GetComponent<BoardController>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public void SetTeams()
    {
        GameObject[] gs = GameObject.FindGameObjectsWithTag("Unit");
        foreach(GameObject g in gs)
        {
            Unit u = g.GetComponent<Unit>();
            while(teams.Count < u.team+1) //add teams until u.team exists
            {
                teams.Add(new Team(true));
            }
            teams[u.team].members.Add(u);
        }
        //will be set at start of level
        teams[0].isAi = false;

        sprite = GetComponent<SpriteRenderer>();
        currentTeamIndex = 0;
        UpdateCurrentTeam();
        board.UpdateAllUnitSquares();
    }

    public void SelectNextAiUnit()
    {
        if(teams[currentTeamIndex].isAi && selectedUnitIndex < CurrentTeam.Count - 1)
        {
            // print("selectnextai selectunit-ing");
            SelectUnit(CurrentTeam[selectedUnitIndex + 1]);
        }
    }

    public bool UpdateCurrentTeam()
    {
        if(!CurrentTeam.Any(u=>u.IsMyTurn))
        {   
            if(++currentTeamIndex >= teams.Count) currentTeamIndex = 0;
            selectedUnitIndex = -1;
            
            foreach(Unit u in CurrentTeam)
                u.BeginTurn();

            if(currentTeamIndex >= 0)
                sprite.color = GetTeamColor(currentTeamIndex);

            if(teams[currentTeamIndex].isAi)
            {
                // print("ai selectunit-ing");
                SelectUnit(CurrentTeam[0]);
            }
            return true;
        }
        return false;
    }

    public void SelectUnit(Unit newUnit)
    {
        if(newUnit.team == currentTeamIndex)
        {
            selectedUnitIndex = CurrentTeam.IndexOf(newUnit);
            newUnit.Select();
        }
    }

    private bool CheckTeamIsDone()
    {
        foreach(Unit unit in teams[currentTeamIndex].members)
        {
            if(unit.IsMyTurn)
            {
                return false;
            }
        }
        return true;
    }


    public void RemoveUnit(Unit u, int teamIndex)
    {
        teams[teamIndex].members.Remove(u);
        if(currentTeamIndex == teamIndex)
        {
            if(teams[teamIndex].isAi)
            {
                foreach (var mem in teams[teamIndex].members)
                {
                    if(mem.IsMyTurn)
                    {
                        selectedUnitIndex = teams[teamIndex].members.IndexOf(mem);
                        break;
                    }
                }
            }else
                selectedUnitIndex = -1;
        }
        
        if(teams[teamIndex].members.Count <= 0)
        {
            teams.Remove(teams[teamIndex]);
            ResetTeamIndexes();
            if(currentTeamIndex > teamIndex)
            {
                currentTeamIndex--;
            }
        }
    }

    void ResetTeamIndexes()
    {
        foreach(Team t in teams)
        {
            foreach(Unit u in t.members)
            {
                u.team = teams.IndexOf(t);
            }
        }
    }

    Color GetTeamColor(int index)
    {
        return teams[index].members[0].savedColor;
    }
}
