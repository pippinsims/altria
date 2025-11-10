using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController: MonoBehaviour
{
	public float F;
	public float G;
	public float H;
	public bool isObstruction;
	public Unit occupant;
	public TileController parentTile;
	private BoxCollider2D col;

	void Start()
	{
		col = gameObject.GetComponent<BoxCollider2D>();
	}

	public TileController FindNeighbor(Vector2 direction)
	{
		col.enabled = false;
		RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x + (direction.x/2), transform.position.y + direction.y/2), direction, 1f, LayerMask.GetMask("Board"));
		col.enabled = true;
		if(hit.transform != null)
		{
			if(hit.transform.gameObject.tag == "Board")
			{
				return hit.transform.gameObject.GetComponent<TileController>();
			}
			else
				return null;
		}
		else
			return null;
	}
}
