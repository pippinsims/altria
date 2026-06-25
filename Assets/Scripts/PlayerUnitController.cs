using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Utils;

public class PlayerUnitController : Unit
{
	public List<SquareController> squaresInRange = new();
	new void Update()
	{
		base.Update();
		if(IsSelected)
		{
			GetComponent<SpriteRenderer>().color = Color.yellow;
		}
		else if(Input.GetMouseButtonUp(0) && mouseIsOver && IsMyTurn)
		{
			if (teams.SelectedUnit != null)
			{
				var old = teams.SelectedUnit;
				if(old.IsMyTurn && old.HasMoved)
				{
					old.FinishActions();
					old.EndTurn();
				}
				if(old is PlayerUnitController oldp)
					oldp.Unselect();
			}
            // print(name+" selfselect");
			teams.SelectUnit(this);
			NotifyAccessibleSquares(move);
		}

		if(!HasMoved)
		{
			if(targetSquare != null)
			{
				Move();
			}
		}
		else
		{
			if(enemiesInRange.Count == 0 && !HasAttacked)
			{
				print(name+":hasattacked");
				HasAttacked = true;
			}

			if(interactiblesInRange.Count == 0 && !HasInteracted)
			{
				print(name+":hasinteracted");
				HasInteracted = true;
			}

			if(Keyboard.current.escapeKey.isPressed && (!HasAttacked || !HasInteracted /*Actions.Any(a=>a.isCompleted)*/) && page.currentUnit == null)
			{
				FinishActions();
			}

			if(HasInteracted && HasAttacked && IsMyTurn)
			{
				EndTurn();
			}
		}
	}

	public void Unselect()
	{
		// print(name+" unselected");
		ResetSquaresInRange();
		IsSelected = false;
	}

	public override void EndTurn()
	{
		ResetSquaresInRange();
		base.EndTurn();
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
		if(Physics2D.CircleCast(transform.position, radius, Vector2.up, ContactFilter2D.noFilter, hits, 0f) > 0)
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
				if(ManhattanDistance(transform, square.transform) <= radius)
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

	/// <summary>
	/// Sets the <c>isInRange</c> field to <c>true</c> for all accessible squares
	/// within the specified <paramref name="radius"/>.
	/// </summary>
	/// <param name="radius">
	/// The maximum distance from the origin square to evaluate accessibility.
	/// </param>
	void NotifyAccessibleSquares(int radius)
	{
		List<RaycastHit2D> hits = new List<RaycastHit2D>();
		if(Physics2D.CircleCast(transform.position, radius, Vector2.up, ContactFilter2D.noFilter, hits, 0f) > 0)
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
				if(ManhattanDistance(transform, square.transform) <= radius)
				{
					pathEndTile = pm.FindPath(GetCurrentSquare(), square.transform, false);
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
