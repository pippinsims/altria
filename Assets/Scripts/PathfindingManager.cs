using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager: MonoBehaviour
{
	/// <summary>
    /// Creates a path stack of Tiles from <paramref name="startTile"/> to the tile at <paramref name="end"/> using the FindPath() and StackPath() functions.<br/>
    /// If <paramref name="range"/> >= 0 the path will be capped at a max length of <paramref name="range"/> using the CapPath() function.<br/>
    /// </summary>
    /// <param name="startTile"></param>
    /// <param name="end"></param>
    /// <param name="range"></param>
    /// <returns></returns>
	public Stack<TileController> CreatePathStack(TileController startTile, Transform end, int range)
	{
		if (startTile == null || end == null)
			Debug.Log("cannot create path with a null start or a null end");
		else
		{
			//print("CreatePath from: " + startTile.transform.position + " to: " + end.position);
			TileController endTile = FindPath(startTile, end, true);
			if (endTile != null)
			{
				if (range >= 0)
					endTile = CapPath(endTile, range);
				return StackPath(endTile);
			}
			else
				return null;
		}
		return null;
	}

	private Stack<TileController> StackPath(TileController endTile)
    {
		Stack<TileController> pathStack = new Stack<TileController>();

		while (endTile.parentTile != null)
		{
			pathStack.Push(endTile);
			endTile = endTile.parentTile;
		}
		pathStack.Push(endTile);

		return pathStack;
	}

	public TileController FindPath(TileController startTile, Transform end, bool accountForBlockedEndpoint)
	{
		List<TileController> open = new List<TileController>() { startTile };
		List<TileController> closed = new List<TileController>();

		startTile.transform.parent.GetComponent<BoardController>().SetAllSquaresForPath();

		startTile.parentTile = null;
		startTile.G = 0;
		startTile.H = SquareController.ManhattanDistance(startTile.transform, end);
		startTile.F = startTile.G + startTile.H;

		while(open.Count > 0)
        {
			TileController currentTile = smallestF(open);

			if(currentTile.transform.position == end.position)
            {
				//reached end
				return currentTile;
            }

			open.Remove(currentTile);
			closed.Add(currentTile);

			foreach(TileController nbr in GetNeighbors(currentTile))
            {
				if (closed.Contains(nbr)) continue;
				if (nbr.isObstruction)
				{
					if (nbr.transform.position == end.position)
					{
						if (accountForBlockedEndpoint)
						{
							//Debug.Log("Endpoint was blocked! Returning one before end.");
							return currentTile;
						}
						else
						{
							//Debug.Log("Endpoint was blocked.");
							return null;
						}
					}
					else
					{
						closed.Add(nbr);
						continue;
					}
				}

				float tempG = currentTile.G + SquareController.ManhattanDistance(currentTile.transform, nbr.transform);
				if(tempG < nbr.G)
                {
					nbr.parentTile = currentTile;
					nbr.G = tempG;
					nbr.H = SquareController.ManhattanDistance(nbr.transform, end);
					nbr.F = nbr.G + nbr.H;

					if (!open.Contains(nbr))
					{
						open.Add(nbr);
					}
				}
			}
		}

		//Debug.Log("No path to target.");
		return null;
	}

	/// <summary>
    /// Limits the path of <paramref name="endTile"/> to a size of <paramref name="range"/> number of nodes.
    /// </summary>
    /// <param name="endTile"></param>
    /// <param name="range"></param>
    /// <returns></returns>
	private TileController CapPath(TileController endTile, int range)
	{
		TileController newEndTile = endTile;
		if (endTile != null)
		{
			int length = FindPathLength(newEndTile);
			newEndTile = endTile;

			if (range >= 0)
			{
				for (int i = 0; i < length - range; i++)
				{
					newEndTile = newEndTile.parentTile;
				}
			}
		}
		return newEndTile;
	}

	public int FindPathLength(TileController endTile)
    {
		int length = 1;
		while (endTile.parentTile != null)
		{
			endTile = endTile.parentTile;
			length++;
		}
		return length;
    }

	public int PathfindingDistance(TileController startTile, Transform end)
	{
		return CreatePathStack(startTile, end, -1).Count;
	}

	private List<TileController> GetNeighbors(TileController tile)
	{
		List<TileController> neighbors = new List<TileController>();

		for(int i = 0; i < 4; i++)
		{
			TileController newNeighbor;
			switch(i)
			{
				case 0:
					newNeighbor = tile.FindNeighbor(Vector2.left);
					break;

				case 1:
					newNeighbor = tile.FindNeighbor(Vector2.up);
					break;

				case 2:
					newNeighbor = tile.FindNeighbor(Vector2.right);
					break;

				case 3:
					newNeighbor = tile.FindNeighbor(Vector2.down);
					break;

				default:
					newNeighbor = null;
					break;
			}
			if(newNeighbor != null)
			{
				neighbors.Add(newNeighbor);
			}
		}

		return neighbors;
	}

	private TileController smallestF(List<TileController> open)
	{
		TileController smallest = open[0];

		foreach(TileController tile in open)
		{
			if(tile.F < smallest.F || (tile.F == smallest.F && tile.H < smallest.H))
				smallest = tile;
		}

		return smallest;
	}
}
