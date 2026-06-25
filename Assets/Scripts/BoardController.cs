using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardController : MonoBehaviour
{
    public int length=17;
    public int height=13;
    public GameObject squarePrefab;
    private TeamManager tm;

    void Awake()
    {
        tm = GameObject.Find("Team Manager").GetComponent<TeamManager>();
        transform.position = new Vector2(-length / 2, -height / 2);
        Physics2D.queriesHitTriggers = true;
        for (float x = 0; x < length; x += 1)
        {
            for (float y = 0; y < height; y += 1)
            {
                Instantiate(squarePrefab, new Vector2(transform.position.x + x, transform.position.y + y), Quaternion.identity,transform);
            }
        }
    }

    void Start()
    {
        foreach(Transform c in transform)
            c.gameObject.GetComponent<TileController>().isObstruction = c.GetComponent<SquareController>().isGlowy;
    }

    public void UpdateAllUnitSquares()
    {
        ClearAllUnitSquares();
        SetAllUnitSquaresAsObstructions();
    }

    public void ClearAllUnitSquares()
    {
        foreach(Transform child in transform)
        {
            var c = child.gameObject.GetComponent<TileController>();
            if(c.occupant != null) c.isObstruction = false;
        }
    }

    public void SetAllUnitSquaresAsObstructions()
    {
        List<TileController> unitSquares = new List<TileController>();
        foreach(Team t in tm.teams)
        {
            foreach (Unit unit in t.members)
            {
                unit.GetCurrentSquare().occupant = unit;
                unit.GetCurrentSquare().isObstruction = true;//TO BE ERASED
            }
        }
    }

	public void SetAllSquaresForPath()
	{
		foreach(Transform child in gameObject.transform)
        {
            child.gameObject.GetComponent<TileController>().G = Mathf.Infinity;
            child.gameObject.GetComponent<TileController>().parentTile = null;
        }
	}

    public SquareController SquareAt(Transform t)
    {
        foreach(Transform child in transform)
        {
            if (child.position == t.position)
                return child.gameObject.GetComponent<SquareController>();
        }
        return null;
    }
}
