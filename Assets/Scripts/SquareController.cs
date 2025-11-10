using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareController : MonoBehaviour
{
    private bool mouseIsOver = false;
    public TeamManager tm;
    private Color savedColor;
    public bool isInRange = false;
    private BoardController bc;

    void Start()
    {
        tm = GameObject.Find("Team Manager").GetComponent<TeamManager>();
        bc = gameObject.transform.parent.gameObject.GetComponent<BoardController>();
        savedColor = GetComponent<SpriteRenderer>().color;
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0) && bc.currentUnit != null && mouseIsOver && !(bc.currentUnit is AiUnitController) && isInRange)
        {
            bc.currentUnit.targetSquare = this;
        }

        if(mouseIsOver)
        {
            GetComponent<SpriteRenderer>().color = Color.magenta;
        }
        else if(isInRange)
        {
            GetComponent<SpriteRenderer>().color = Color.gray;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = savedColor;
        }
    }

    void OnMouseEnter()
    {
        mouseIsOver = true;
    }

    void OnMouseExit()
    {
        mouseIsOver = false;
    }

    public static float ManhattanDistance(Transform t1, Transform t2)
    {
         return Mathf.Abs(t1.position.x - t2.position.x) + Mathf.Abs(t1.position.y - t2.position.y);
    }
}
