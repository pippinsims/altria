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
        GameObject[] gs = GameObject.FindGameObjectsWithTag("Unit");
        foreach(GameObject g in gs)
        {
            Unit u = g.GetComponent<Unit>();
            while(teams.Count < u.team+1) //add teams until u.team exists
            {
                teams.Add(new());
            }
            teams[u.team].members.Add(u);
        }
        //will be set at start of level
        teams[1].isAi = true;

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
            // print(newUnit.name+" selectunit-ed");
            newUnit.Select();
        }
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
