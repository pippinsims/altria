using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class SquareController : Hoverable
{
    public TeamManager teams;
    private Color savedColor;
    public bool isInRange = false;
    public bool isGlowy;
    private BoardController board;

    void Awake() { isGlowy = Random.value < 0.1f; }

    void Start()
    {
        teams = GameObject.Find("Team Manager").GetComponent<TeamManager>();
        board = gameObject.transform.parent.gameObject.GetComponent<BoardController>();
        savedColor = GetComponent<SpriteRenderer>().color;
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0) && teams.SelectedUnit != null && mouseIsOver && teams.SelectedUnit is not AiUnitController && isInRange)
        {
            teams.SelectedUnit.targetSquare = this;
        }
        if(Input.GetMouseButtonDown(1) && mouseIsOver && isGlowy && teams.SelectedUnit != null && teams.SelectedUnit.HasMoved)
        {
            if(ManhattanDistance(teams.SelectedUnit.transform, transform) <= 1)
                print("yeet");
        }

        GetComponent<SpriteRenderer>().color = isGlowy ? Color.green
                                             : mouseIsOver ? Color.magenta
                                             : isInRange ? Color.gray
                                             : savedColor;
    }
}
