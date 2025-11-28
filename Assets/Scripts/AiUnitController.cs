using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiUnitController : Unit
{
	private Unit targetUnit;
	new void Update()
	{
		base.Update();
		if (isSelected)
		{
			if(targetSquare == null)
			{
				FindTargetSquare();
			}

			if(!hasMoved && targetSquare != null)
			{
				Move();
			}

			if(hasMoved)
			{
				//if units are in range, choose unit in range to attack,
				if(enemiesInRange.Count > 0)
				{
					if(enemiesInRange.Contains(targetUnit))
					{
						targetUnit.OnClicked();
					}
					else
					{
						FindWeakestUnitInRange().OnClicked();
					}
				}
				else
				{
					hasAttacked = true;
				}
			}
		}

		if(hasAttacked && hasMoved && isMyTurn)
		{
			EndTurn();
		}
	}

	/// <summary>
    /// resets target unit, informs team manager that a unit's turn is over
    /// </summary>
	protected override void EndTurn()
	{
		print("enend");
		if(moveSpeed == 1f)
		{
			print("Turn ended");
		}
		base.EndTurn();
		GetComponent<AiUnitController>().SetTargetUnit(null);
		tm.MaybeSelectNextAiUnit();
		tm.MaybeUpdateCurrentTeam();
	}

	/// <summary>
	/// Finds a specified TileController on a path, given a <paramref name="path"/> Stack and an integer <paramref name="distance"/>. <br/>
	/// This distance is the amount of nodes, inclusively, from the start to the desired node. <br/>
	/// For example, Stack Count 2, distance 2, returns node with index 1, the 2nd node in the Stack.
	/// </summary>
	/// <param name="path"></param>
	/// <param name="distance"></param>
	/// <returns>The TileController on the Stack that corresponds to the specified distance number</returns>
	TileController PointOnPathFromStart(Stack<TileController> path, int distance)
	{
		if (path == null)
			return null;
		else
		{
			Stack<TileController> currentPath = new Stack<TileController>(new Stack<TileController>(path));

			if(distance - 1 > currentPath.Count - 1)
			{
				//if they want to go further than the path,
				//meaning they cant reach the target because the path is too short,
				return null;
			}

			for (int i = 0; i < distance - 1; i++)
				currentPath.Pop();
			return currentPath.Peek();
		}
	}

	/// <summary>
	/// Finds the furthest square from which this unit will still be able to attack it's target. <br/>
	/// First a CircleCast from the position of the enemy to get the squares that this would be able to attack from,<br/>
	/// then refined to be within the pathfinding distance from this unit. <br/>
	/// Then runs pm.CreatePath() to the furthest square and returns the result.
	/// </summary>
	/// <returns>A path created to the furthest square still in range to enemy</returns>
	TileController FindEscapeTile()
	{
		Debug.Log("Escaping");
		List<RaycastHit2D> hits = new List<RaycastHit2D>();
		SquareController furthest = null;
		SquareController tempSquare;
		if (Physics2D.CircleCast(targetUnit.transform.position, range, Vector2.up, ContactFilter2D.noFilter, hits, 0f) > 0)
		{
			foreach (RaycastHit2D hit in hits)
			{
				if (hit.transform.gameObject.tag == "Board")
				{
					tempSquare = hit.transform.gameObject.GetComponent<SquareController>();
					float newDist = SquareController.ManhattanDistance(targetUnit.transform, tempSquare.transform);
					//check manhattan distance from target to new square
					if (newDist <= range)
					{   
						//check pathfinding distance
						if (pm.PathfindingDistance(FindCurrentSquare(), tempSquare.transform) <= move)
						{
							//if new is further than my position
							if (furthest == null && newDist > SquareController.ManhattanDistance(targetUnit.transform, FindCurrentSquare().transform))
							{
								furthest = tempSquare;
							}
							if (furthest != null)
							{
								//if new is further than the previous furthest
								if (newDist > SquareController.ManhattanDistance(targetUnit.transform, furthest.transform))
								{
									furthest = tempSquare;
								}
								//or if new is the same but it's closer walking distance
								if (newDist == SquareController.ManhattanDistance(targetUnit.transform, furthest.transform)
								&& pm.PathfindingDistance(FindCurrentSquare(), tempSquare.transform) < pm.PathfindingDistance(FindCurrentSquare(), furthest.transform))
								{
									furthest = tempSquare;
								}
							}
						}
					}
				}
			}
		}
		if (furthest != null)
		{
			print("furthest: " + furthest.transform.position);
			return furthest.gameObject.GetComponent<TileController>();
		}
		else
			return null;
	}

	/// <summary>
	/// Sets targetSquare. First sets targetUnit with <see cref="FindWeakestUnitGlobal"/>. <br/>
	/// If targetUnit exists and FindTargetTile() gives us a TileController, set that as the targetSquare. <br/>
	/// Otherwise targetSquare is set to be at current position.
	/// </summary>
	void FindTargetSquare()
	{
		targetUnit = FindWeakestUnitGlobal();

		if(targetUnit != null)
		{
			TileController targetTile = FindTargetTile(pm.CreatePathStack(FindCurrentSquare(), targetUnit.transform, -1));

			if(targetTile != null)
			{
				targetSquare = targetTile.gameObject.GetComponent<SquareController>();
			}
			else
			{
				print("ERROR!! couldn't find target tile!");
				targetSquare = FindCurrentSquare().GetComponent<SquareController>();
			}
		}
		else
		{
			targetSquare = FindCurrentSquare().GetComponent<SquareController>();
		}
	}

	//For debugging
	void TracePath(Stack<TileController> path)
    {
		Stack<TileController> copy = new Stack<TileController>(new Stack<TileController>(path));
		while (copy.Count > 0)
			Debug.Log(copy.Pop().transform.position);
	}

	/// <summary>
	/// Finds and returns the TileController where this unit has just enough range to attack the targetUnit. <br/>
	/// Uses <see cref="FindEscapeTile"/> if it needs to move further away.
	/// </summary>
	/// <param name="path">The path from the unit to the targetUnit.</param>
	/// <returns>The Tile for the unit to target based on where targetUnit is.</returns>
	TileController FindTargetTile(Stack<TileController> path)
	{
		//dist to target is the number of spaces this unit must cross in order to have the same position as the target unit
		int distToTargetUnit = path.Count;

		if (distToTargetUnit >= range + move - 1)
		{
			print("target further than or at range + move");
			return PointOnPathFromStart(path, move);
		}
		else if(distToTargetUnit > range)
		{
			print("target further than range");
			return PointOnPathFromStart(path, distToTargetUnit + 1 - range); //+ 1 because PointOnPath includes the original position
		}
		else
		{
			if (distToTargetUnit == range)
			{
				print("target at range");
				return FindCurrentSquare();
			}
			else
			{
				print("target closer than range");

				TileController escapeTile = FindEscapeTile();
				if (escapeTile != null)
					return escapeTile;
				else
				{
					print("no escape path!");
					return FindCurrentSquare();
				}
			}
		}
		//If targetTile and targetUnit are less than range apart, make them exactly range apart.
		//This will only happen if move is more than we want.
		//Also works for melee units because their range is 1.
	}

	Unit FindWeakestUnitInRange()
	{
		Unit chsn = null;

		//chsn = lowest health after attack out of the people in range
		foreach(Unit unit in enemiesInRange)
		{
			if(chsn == null || IsWeaker(chsn, unit))
			{
				chsn = unit;
			}
		}
		//print(chsn.transform.position);
		return chsn;
	}

	//the data variable move includes the current position, so all uses of move when using RelevantDistance() must be move - 1,
    //because RelevantDistance() doesn't include the current tile position
	Unit FindWeakestUnitGlobal()
	{
		Unit chsn = null;
		int relDist;
		if (moveSpeed == 1f)
			print("move: " + move + ", range: " + range);
		foreach (Team t in tm.teams)
		{
			if (tm.teams.IndexOf(t) != team)
			{
				//chsn = unit with lowest health after attack or in range if possible
				foreach (Unit unit in t.members)
				{
					relDist = RelevantDistance(unit);
					if (moveSpeed == 1f)
						print(relDist);

					//if there is a path to current
					if ((relDist >= 0))																								
					{
						//if current is first on list
						if (chsn == null)                                                                                          
						{
							if (moveSpeed == 1f)
								print("first unit in list");
							chsn = unit;                                                                                            
						}
						//else if saved is in range
						else if (RelevantDistance(chsn) <= move - 1 + range)                                                            
						{
							//and current is also in range and current is weaker than saved
							if ((relDist <= move - 1 + range) && IsWeaker(chsn, unit))
							{
								if (moveSpeed == 1f)
									print("new unit is in range and weaker than saved");
								chsn = unit;                                                                                        
							}
						}
						//if saved is outside of range
						else
						{
							//if current is in range
							if (relDist <= move - 1 + range)																		
							{
								if (moveSpeed == 1f)
									print("new unit is in range and saved is outside");
								chsn = unit;                                                                                        
							}
							//if current is also outside of range and is weaker than saved
							else if (IsWeaker(chsn, unit))   
							{
								if (moveSpeed == 1f)
									print("both are outside of range, but new unit is weaker");
								chsn = unit;
							}
						}
					}
				}
			}
		}
		
		if(moveSpeed == 1f && chsn != null)
			print("chosen unit: " + chsn.gameObject.name + ", " + RelevantDistance(chsn) + " spaces away.");
		return chsn;
	}

	private bool IsWeaker(Unit chosen, Unit compareTo)
    {
        return chosen.currentHealth - (strength - chosen.defense) > compareTo.currentHealth - (strength - compareTo.defense); //TODO: not a great formula, fix.
    }

	/// <summary>
    /// Returns the number of spaces this unit must cross in order to have the same position as <paramref name="unitToAttack"/>.
    /// </summary>
    /// <param name="unitToAttack"></param>
    /// <returns></returns>
	int RelevantDistance(Unit unitToAttack)
	{
		Stack<TileController> movePath = pm.CreatePathStack(FindCurrentSquare(), unitToAttack.transform, move);
		if (movePath == null) return -1;
		else
		{
			TileController moveEndPoint = GetEndOfStack(movePath);
			return movePath.Count - 1 + (int)SquareController.ManhattanDistance(moveEndPoint.transform, unitToAttack.transform);
		}
	}

	private TileController GetEndOfStack(Stack<TileController> stack)
	{
		Stack<TileController> copy = new Stack<TileController>(new Stack<TileController>(stack));
		TileController t = null;
		while (copy.Count > 0)
			t = copy.Pop();
		return t;
	}

	public void SetTargetUnit(Unit unit)
	{
		targetUnit = unit;
	}
}
