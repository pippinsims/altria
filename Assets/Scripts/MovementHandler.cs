using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementHandler : MonoBehaviour
{
	private Unit myUnit;
    private Vector3 direction;
	internal bool isCurrentlyMoving = false;
	internal PathfindingManager pm;
	internal BoardController bc;
	internal bool stepping = false;

	public void Begin(Unit unit, BoardController bController, PathfindingManager pManager)
	{
		myUnit = unit;
		bc = bController;
		pm = pManager;
	}

	public void Move()
	{
		isCurrentlyMoving = true;

		StartCoroutine(WalkToTile(pm.CreatePathStack(myUnit.FindCurrentSquare(), myUnit.targetSquare.transform, myUnit.move)));
	}

	IEnumerator WalkToTile(Stack<TileController> movePath)
	{
		while (movePath.Count > 0)
		{
			myUnit.currentTarget = movePath.Pop();
			while (myUnit.currentTarget != null)
			{
				direction = myUnit.currentTarget.gameObject.transform.position - transform.position;
				Step();
				yield return new WaitForSeconds(0.001f);
			}
		}

		bc.UpdateAllUnitSquares();
		myUnit.CheckForEnemiesInRange();
		if (!(myUnit is AiUnitController))
		{
			GetComponent<PlayerUnitController>().ResetSquaresInRange();
			GetComponent<PlayerUnitController>().NotifySquaresInArea(myUnit.range);
		}
		isCurrentlyMoving = false;
		myUnit.hasMoved = true;
	}

	void Step()
	{
		if((Mathf.Abs(transform.position.x - myUnit.currentTarget.gameObject.transform.position.x)  < 0.1f 
		&& Mathf.Abs(transform.position.y - myUnit.currentTarget.gameObject.transform.position.y) < 0.1f))
		{
			transform.position = myUnit.currentTarget.gameObject.transform.position;
			myUnit.currentTarget = null;
		}
		else
		{
			transform.position += direction/direction.magnitude * myUnit.moveSpeed * 0.01f;
		}
	}
}
