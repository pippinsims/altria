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
	public int move = 2; //Including square at transform.position

	[Header ("Combat Stats")]
	public int strength = 4; //Affects Physical weapon damage
	public int defense = 0;
	public int speed = 5; //Affects multi-attack, and affects hit/avoid
	public int dexterity = 3; //Primary hit/avoid modifier
	public int luck = 0; //Crit
	public int temporaryWeaponHit = 80; //TODO: Make weapons

	
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
		GetComponent<SpriteRenderer>().color = mouseIsOver && !isSelected ? Color.cyan
										     : !isMyTurn && tm.currentTeamIndex == team ? new Color(savedColor.r/2, savedColor.g/2, savedColor.b/2, savedColor.a) 
											 : savedColor;

		if(Input.GetMouseButtonUp(0))
		{
			GetComponent<CircleCollider2D>().enabled = false;
			GetComponent<CircleCollider2D>().enabled = true;
			if(mouseIsOver && !isMyTurn) OnClicked();
		}
	}

	protected virtual void EndTurn()
	{
		print("nend");
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
				if(p.animator != null)
				{
					p.animator.SetTrigger("Attack");

					p.gameObject.GetComponent<SpriteRenderer>().flipX = (transform.position.x < p.gameObject.transform.position.x);
				}
				p.hasAttacked = true;
				
				//TODO: MAKE p.CalculateHit IN THE UI, AS WELL AS AVOID
				p.Attack(this);
				if(SquareController.ManhattanDistance(transform, p.transform) <= range)
					Attack(p);
				(Unit a, Unit d, int m) = GetAdvantageResultsAgainst(p);
				a.AttackMutipleTimes(d, m >= 5 ? m/5 : 0);
			}
		}
	}

	private (Unit adv, Unit disAdv, int advMag) GetAdvantageResultsAgainst(Unit other)
    {
		int dif = speed - other.speed;
		Unit a = this;
		Unit d = other;
		if(dif < 0) (a, d) = (d, a);

        return (a, d, Mathf.Abs(dif));
    }

	private bool WithinPercent(int chance)
    {
        return Random.Range(0,101) <= chance;
    }
	public int CalculateDamage()
    {
		return strength * (WithinPercent(luck / 2) ? 2 : 1);
    }

	public int CalculateHit()
    {
		//TODO: MAKE THIS IN THE UI, AS WELL AS AVOID
        return temporaryWeaponHit + dexterity * 2 + speed;
    }

	public bool ReceiveDamage(int dmg)
    {
		currentHealth -= dmg - defense;
        if(currentHealth <= 0)
		{
			Die();
		}
		return currentHealth <= 0;
    }

	private void Die()
    {
		if(tm.teams[team].members[tm.selectedUnitIndex] == this)
			EndTurn();
        tm.RemoveUnit(this, team);
		FindCurrentSquare().isObstruction = false;
		Destroy(this.gameObject);
		print("died");	
    }

	private void Attack(Unit target)
    {
        if(WithinPercent(CalculateHit() - (target.dexterity + target.speed)))
			target.ReceiveDamage(CalculateDamage());
    }

	public void AttackMutipleTimes(Unit target, int num)
    {
        for(int i = 0; i < num; i++) Attack(target);
    }

	public TileController FindCurrentSquare()
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, 1f, LayerMask.GetMask("Board"));
		if(hit.transform.gameObject.tag == "Board")
			return hit.transform.gameObject.GetComponent<TileController>();
		else
			return null;
	}

	public void CheckForEnemiesInRange()
	{
		enemiesInRange.Clear();
		List<RaycastHit2D> hits = new List<RaycastHit2D>();
		if(Physics2D.CircleCast(transform.position, range, Vector2.up, ContactFilter2D.noFilter, hits, 0f) > 0)
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
