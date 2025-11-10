using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitController : Unit
{
	public List<SquareController> squaresInRange = new List<SquareController>();
	new void Update()
	{
		base.Update();
		if(isSelected)
		{
			GetComponent<SpriteRenderer>().color = Color.yellow;
		}

		if (Input.GetMouseButtonUp(0))
		{
			if (mouseIsOver && isMyTurn && !isSelected)
			{
				if (tm.selectedUnitIndex >= 0)
				{
					Unit oldSelectedUnit = tm.teams[team].members[tm.selectedUnitIndex];

					if (oldSelectedUnit != null)
					{
						tm.UpdateSelectedUnit(this);
						if (oldSelectedUnit.isMyTurn)
						{
							if (oldSelectedUnit.hasMoved)
								oldSelectedUnit.hasAttacked = true;
							oldSelectedUnit.isSelected = false;
							if(oldSelectedUnit is PlayerUnitController)
								oldSelectedUnit.gameObject.GetComponent<PlayerUnitController>().ResetSquaresInRange();
						}
						NotifyAccessibleSquares(move);
					}
					else
					{
						Debug.Log("ERROR!! selectedUnitIndex was >= 0 but selectedUnit was null!");
					}
				}
				else
				{
					tm.UpdateSelectedUnit(this);
					NotifyAccessibleSquares(move);
				}
			}
		}

		if(!hasMoved && targetSquare != null)
		{
			Move();
		}

		if (Input.GetMouseButtonUp(1))
		{
			if(!showUI)
			{
				if(mouseIsOver)
				{
					showUI = true;
				}
			}
			else
				showUI = false;

			if(hasMoved)
				hasAttacked = true;
		}

		if(hasMoved && enemiesInRange.Count == 0 && !hasAttacked)
		{
			hasAttacked = true;
		}

		if(hasAttacked && hasMoved && isMyTurn)
		{
			EndTurn();
		}
	}

	new void EndTurn()
	{
		base.EndTurn();
		ResetSquaresInRange();
		tm.MaybeUpdateCurrentTeam();
	}

	/// <summary>
	/// Resets the isInRange boolean in all the squares in squaresInRange to false, and then empties squaresInRange.
	/// </summary>
	public void ResetSquaresInRange()
	{
		foreach (SquareController square in squaresInRange)
		{
			square.isInRange = false;
		}
		squaresInRange.Clear();
	}

	//Squares in an area (used for attack range)
	public void NotifySquaresInArea(int radius)
	{
		List<RaycastHit2D> hits = new List<RaycastHit2D>();
		if(Physics2D.CircleCast(transform.position, radius, Vector2.up, new ContactFilter2D().NoFilter(), hits, 0f) > 0)
		{
			List<SquareController> circleCastSquares = new List<SquareController>();
			foreach(RaycastHit2D hit in hits)
			{
				if (hit.transform.gameObject.tag == "Board")
				{
					circleCastSquares.Add(hit.transform.gameObject.GetComponent<SquareController>());
				}
			}

			foreach(SquareController square in circleCastSquares)
			{
				if(SquareController.ManhattanDistance(transform, square.transform) <= radius)
				{
					squaresInRange.Add(square);
				}
			}

			foreach(SquareController square in squaresInRange)
			{
				square.isInRange = true;
			}
		}
	}

	//Squares accessible by move
	void NotifyAccessibleSquares(int radius)
	{
		List<RaycastHit2D> hits = new List<RaycastHit2D>();
		if(Physics2D.CircleCast(transform.position, radius, Vector2.up, new ContactFilter2D().NoFilter(), hits, 0f) > 0)
		{
			List<SquareController> circleCastSquares = new List<SquareController>();
			foreach(RaycastHit2D hit in hits)
			{
				if (hit.transform.gameObject.tag == "Board")
				{
					circleCastSquares.Add(hit.transform.gameObject.GetComponent<SquareController>());
				}
			}

			TileController pathEndTile;
			foreach(SquareController square in circleCastSquares)
			{
				if(SquareController.ManhattanDistance(transform, square.transform) <= radius)
				{
					pathEndTile = pm.FindPath(FindCurrentSquare(), square.transform, false);
					if (pathEndTile != null && pm.FindPathLength(pathEndTile) <= move) 
					{
						squaresInRange.Add(square);
					}
				}
			}

			foreach(SquareController square in squaresInRange)
			{
				square.isInRange = true;
			}
		}
	}
}
