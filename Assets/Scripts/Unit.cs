using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
	[Header ("General Data")]
	[SerializeField]
    private Vector2 spawnLocation;
	public SquareController targetSquare;
	public TileController currentTarget;
	internal List<Unit> enemiesInRange = new List<Unit>();
	
	public float moveSpeed = 10f;
	public int team;
	public Color savedColor;
	public bool showUI = false;
	public Animator animator;
	internal PathfindingManager pm;
	internal TeamManager tm;
	internal BoardController bc;
	private MovementHandler mh;

	[Header ("Logic Data")]
	public bool isMyTurn = true;
	public bool isSelected = false;
	public bool hasMoved = false;
	public bool hasAttacked = false;
	public bool mouseIsOver = false;
	
	[Header ("Stats")]
	public int range = 2;
	public int maxHealth = 10;
	public int currentHealth = 0;
	public int attack = 4;
	public int defense = 0;
	public int move = 2; //Including square at transform.position
	
	protected void Awake()
	{
		savedColor = GetComponent<SpriteRenderer>().color;
		transform.position = spawnLocation;
	}
	
	protected void Start()
	{	
		bc = GameObject.Find("Board").GetComponent<BoardController>();
		tm = GameObject.Find("Team Manager").GetComponent<TeamManager>();
		pm = GameObject.Find("Pathfinding Manager").GetComponent<PathfindingManager>();
		currentHealth = maxHealth;
		animator = gameObject.GetComponent<Animator>();
		mh = gameObject.AddComponent<MovementHandler>();
		mh.Begin(this, bc, pm);
	}

	protected void Update()
	{
		if(mouseIsOver && !isSelected)
		{
			GetComponent<SpriteRenderer>().color = Color.cyan;
		}
		else if(!isMyTurn && tm.currentTeamIndex == team)
		{
			GetComponent<SpriteRenderer>().color = new Color(savedColor.r/2, savedColor.g/2, savedColor.b/2, savedColor.a);  
		}
		else
		{
			GetComponent<SpriteRenderer>().color = savedColor;
		}

		if(Input.GetMouseButtonUp(0))
		{
			GetComponent<CircleCollider2D>().enabled = false;
			GetComponent<CircleCollider2D>().enabled = true;
			if(mouseIsOver && !isMyTurn)
			{
				OnClicked();
			}
		}
	}

	protected void EndTurn()
	{
		isMyTurn = false;
		targetSquare = null;
		isSelected = false;
	}

	void OnMouseEnter()
	{
		mouseIsOver = true;
	}

	void OnMouseExit()
	{
		mouseIsOver = false;
	}

	public void OnClicked()
	{
		if(tm.selectedUnitIndex >= 0)
		{
			Unit p = tm.teams[tm.currentTeamIndex].members[tm.selectedUnitIndex];
			if(p.hasMoved && p.isSelected && p.isMyTurn && p.enemiesInRange.Contains(this))
			{
				//TODO roll for hit and crit
				if(p.animator != null)
				{
					p.animator.SetTrigger("Attack");

					p.gameObject.GetComponent<SpriteRenderer>().flipX = (transform.position.x < p.gameObject.transform.position.x);
				}

				currentHealth -= (p.attack - defense);
				p.hasAttacked = true;
				if(currentHealth <= 0)
				{
					tm.RemoveUnit(this, team);
					FindCurrentSquare().isObstruction = false;
					Destroy(this.gameObject);	
				}
			}
		}
	}

	

	public TileController FindCurrentSquare()
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, 1f, LayerMask.GetMask("Board"));
		if(hit.transform.gameObject.tag == "Board")
		{
			return hit.transform.gameObject.GetComponent<TileController>();
		}
		else
			return null;
	}

	public void CheckForEnemiesInRange()
	{
		enemiesInRange.Clear();
		List<RaycastHit2D> hits = new List<RaycastHit2D>();
		if(Physics2D.CircleCast(transform.position, range, Vector2.up, new ContactFilter2D().NoFilter(), hits, 0f) > 0)
		{
			List<Unit> allInRange = new List<Unit>();
			foreach(RaycastHit2D hit in hits)
			{
				if (hit.transform.gameObject.tag == "Unit")
				{
					allInRange.Add(hit.transform.gameObject.GetComponent<Unit>());
				}   
			}

			foreach(Unit unit in allInRange)
			{
				if(SquareController.ManhattanDistance(transform, unit.transform) <= range && unit.team != team)
				{
					enemiesInRange.Add(unit);
				}
			}
		}
	}

	public void Move()
	{
		if(!mh.isCurrentlyMoving)
			mh.Move();
	}
}
