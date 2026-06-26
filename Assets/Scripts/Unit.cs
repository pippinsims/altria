using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Utils;

public class Unit : Hoverable
{
	[Header ("General Data")]
    public Vector2 spawnLocation;
	public SquareController targetSquare;
	public TileController currentTarget;
	public UnitPageController page;
	public List<Unit> enemiesInRange = new List<Unit>();
	public List<SquareController> interactiblesInRange = new();
	
	public float moveSpeed = 10f;
	public int team;
	public Color savedColor;
	public Animator animator;
	public PathfindingManager pm;
	public TeamManager teams;
	public BoardController bc;
	private MovementHandler mh;

	[Header ("Logic Data")]
	public bool HasInteracted { get; protected set; } = false;
	public bool HasAttacked { get; protected set; } = false;
	public bool IsSelected { get; protected set; } = false;
	public bool HasMoved { get; protected set; } = false;
	public bool IsMyTurn { get; protected set; } = true;
	
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
		page = GameObject.Find("Unit Page").GetComponent<UnitPageController>();
		bc = GameObject.Find("Board").GetComponent<BoardController>();
		teams = GameObject.Find("Team Manager").GetComponent<TeamManager>();
		pm = GameObject.Find("Pathfinding Manager").GetComponent<PathfindingManager>();
		currentHealth = maxHealth;
		animator = gameObject.GetComponent<Animator>();
		mh = gameObject.AddComponent<MovementHandler>();
		mh.Begin(this, bc, pm);
	}

	protected void Update()
	{ 
		GetComponent<SpriteRenderer>().color = mouseIsOver && !IsSelected ? Color.cyan
										     : !IsMyTurn && teams.CurrentTeam.Contains(this) ? new Color(savedColor.r/2, savedColor.g/2, savedColor.b/2, savedColor.a) 
											 : savedColor;

		if(Input.GetMouseButtonUp(0))
		{
			GetComponent<CircleCollider2D>().enabled = false;
			GetComponent<CircleCollider2D>().enabled = true;
			if(mouseIsOver && !IsMyTurn) OnClicked();
		}
		
		if(Input.GetMouseButtonUp(1) && mouseIsOver)
		{
			page.currentUnit = this;
		}
	}

	public void BeginTurn()
	{
		IsMyTurn = true;
		HasMoved = false;
		HasInteracted = false;
		HasAttacked = false;
	}

	public void Select()
	{
		// print(name+" selected");
		IsSelected = true;
	}

	/// <summary>
	/// sets logic data and informs team manager that a unit's turn is over
	/// </summary>
	public virtual void EndTurn()
	{
		targetSquare = null;
		IsMyTurn = false;
		IsSelected = false;
		teams.UpdateCurrentTeam();
	}
	public virtual void FinishActions()
	{
		print(name+"1");
		HasInteracted = true;
		HasAttacked = true;
	}

	public void OnClicked()
	{
		if(teams.SelectedUnit is Unit sel && sel.IsMyTurn && sel.HasMoved && sel.enemiesInRange.Contains(this) && !sel.HasAttacked)
		{
			if(sel.animator != null)
			{
				sel.animator.SetTrigger("Attack");

				sel.gameObject.GetComponent<SpriteRenderer>().flipX = transform.position.x < sel.gameObject.transform.position.x;
			}
			sel.HasAttacked = true;
			
			//TODO: MAKE p.CalculateHit IN THE UI, AS WELL AS AVOID
			sel.Attack(this);
			if(ManhattanDistance(transform, sel.transform) <= range && currentHealth > 0)
				Attack(sel);
			var (adv, dis, dif) = AdvantageResults(sel);
			adv.AttackMutipleTimes(dis, dif >= 5 ? dif/5 : 0);
		}
	}

	private (Unit adv, Unit disAdv, int advMag) AdvantageResults(Unit other)
    {
		int dif = speed - other.speed;
		Unit a = this, d = other;
		if(dif < 0) (a, d) = (d, a);

        return (a, d, Mathf.Abs(dif));
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
        if((currentHealth -= dmg - defense) <= 0)
			Die();
		return currentHealth <= 0;
    }

	private void Die()
    {
		if(IsSelected) EndTurn();
        teams.RemoveUnit(this, team);
		GetCurrentSquare().isObstruction = false;
		Destroy(gameObject);
    }

	private void Attack(Unit target)
    {
        if(WithinPercent(CalculateHit() - (target.dexterity + target.speed)))
			target.ReceiveDamage(CalculateDamage());
    }

	public void AttackMutipleTimes(Unit target, int num)
    {
        for(int i = 0; i < num; i++) if(target.currentHealth > 0) Attack(target); else break;
    }

	public TileController GetCurrentSquare()
	{
		return Physics2D.Raycast(transform.position, Vector2.zero, 1f, LayerMask.GetMask("Board"))
			.transform.gameObject.GetComponent<TileController>();
	}

	private void CheckForEnemiesInRange()
	{
		enemiesInRange.Clear();
		List<RaycastHit2D> hits = new();
		if(Physics2D.CircleCast(transform.position, range, Vector2.up, ContactFilter2D.noFilter, hits, 0f) > 0)
		{
			List<Unit> allInRange = new();
			foreach(RaycastHit2D hit in hits)
			{
				if (hit.transform.gameObject.tag == "Unit")
				{
					allInRange.Add(hit.transform.gameObject.GetComponent<Unit>());
				}   
			}

			foreach(Unit unit in allInRange)
			{
				if(ManhattanDistance(transform, unit.transform) <= range && unit.team != team)
				{
					enemiesInRange.Add(unit);
				}
			}
		}
	}

	private void SetInteractiblesInRange() 
	{ 
		interactiblesInRange.Clear();
		List<RaycastHit2D> hits = new();
		if(Physics2D.CircleCast(transform.position, range, Vector2.up, ContactFilter2D.noFilter, hits, 0f) > 0)
		{
			foreach(RaycastHit2D hit in hits.Where(h => h.transform.gameObject.tag == "Board"))
			{
				var s = hit.transform.gameObject.GetComponent<SquareController>();
				if(s.isGlowy) interactiblesInRange.Add(s);
			}
		}
	}

	public void Move()
	{
		if(!mh.isCurrentlyMoving)
			mh.Move();
	}

	public void OnMovementEnded()
	{
		CheckForEnemiesInRange();
		SetInteractiblesInRange();
		if (this is PlayerUnitController p)
		{
			p.ResetSquaresInRange();
			if(!HasAttacked) p.NotifySquaresInArea(range);
			else if(!HasInteracted) p.NotifySquaresInArea(1);
		}
		HasMoved = true;
	}
}
