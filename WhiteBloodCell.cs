using System.Linq;
using Godot;

namespace Parasite;

public partial class WhiteBloodCell : BloodCell, IGameEntity
{
	[Export] public float WanderChance { get; set; } = 0.5f;
	[Export] public int MaxWanderTurns { get; set; } = 5;
	[Export] public int MinWanderTurns { get; set; } = 1;
	public bool IsTurnActive { get; private set; }
	
	public RoshamboController RoshamboController
	{
		get
		{
			if (_roshamboController == null)
			{
				_roshamboController = GetNode<RoshamboController>("Roshambo");
			}

			return _roshamboController;
		}
	}
	public Roshambo.Option CurrentRoshambo { get; set; }
	public override EntityType EntityType => EntityType.WhiteBloodCell;

	public event TurnCompletedEventHandler TurnEnded;

	private RoshamboController _roshamboController;
	private readonly Vector3[] _possibleDirections = { Vector3.Left, Vector3.Forward, Vector3.Right, Vector3.Back };
	
	private int _wanderTurn;
	private int _requiredWanderTurns;
	private bool _isWandering;
	
	public Roshambo.Option RoleRoshambo()
	{
		Roshambo.Option roshambo = Roshambo.Role();
		CurrentRoshambo = roshambo;

		return roshambo;
	}
	
	public void BeginTurn()
	{
		IsTurnActive = true;

		Vector3 direction = DetermineDirection();
		
		UpdateTile(direction);
		EndTurn();
	}

	public void EndTurn()
	{
		IsTurnActive = false;
		TurnEnded?.Invoke(this);
	}
	
	private Vector3 DetermineDirection()
	{
		// TODO: Clean this up...it's messy.
		ITileOccupier affectedEntity = null;
		Vector3 bestDirection = Vector3.Zero;
		var chance = GD.Randf();
		if (!_isWandering && chance <= WanderChance)
		{
			_wanderTurn = 0;
			_requiredWanderTurns = GD.RandRange(MinWanderTurns, MaxWanderTurns);
			_isWandering = true;
		}
		
		if (_isWandering && _wanderTurn < _requiredWanderTurns)
		{
			_wanderTurn++;
			
			var availableDirections = _possibleDirections.ToList();
			for (var i = (int)(GD.Randi() % (availableDirections.Count - 1)); i >= 0 && availableDirections.Count > 0;)
			{
				Vector3 vec = availableDirections[i];

				if (IsDirectionValid(vec, out affectedEntity))
				{
					bestDirection = vec;
				}
				
				availableDirections.RemoveAt(i);
				i = 0;
				if (availableDirections.Count > 1)
				{
					i = (int)(GD.Randi() % (availableDirections.Count - 1));
				}
			}
		}
		else
		{
			_isWandering = false;

			var minDistance = float.MaxValue;
			ParasiteSegment minSegment = GameManager.Player.Segments[0];
			foreach (ParasiteSegment segment in GameManager.Player.Segments)
			{
				var distance = GlobalPosition.DistanceTo(segment.GlobalPosition);
				if (distance < minDistance)
				{
					minDistance = distance;
					minSegment = segment;
				}
			}

			Vector3 direction = (minSegment.GlobalPosition - GlobalPosition).Normalized();

			var dot = 0f;
			foreach (Vector3 vec in _possibleDirections)
			{
				if (!IsDirectionValid(vec, out affectedEntity))
				{
					continue;
				}

				var newDot = direction.Dot(vec);
				if (newDot > dot)
				{
					dot = newDot;
					bestDirection = vec;
				}
			}
		}

		if (affectedEntity is ParasiteSegment parasiteSegment)
		{
			parasiteSegment.Cut();
		}
		
		return bestDirection;
	}

	private bool IsDirectionValid(Vector3 direction, out ITileOccupier entity)
	{
		entity = null;
		Tilemap.TileData data = GameManager.Tilemap.GetTileData(GlobalPosition + direction);
		return data == null
		       || !data.IsEnterable(
			       EntityType.Player | EntityType.Parasite,
			       out entity,
			       CurrentRoshambo);
	}
	
	private void UpdateTile(Vector3 direction)
	{
		GameManager.Tilemap.UpdateTileState(GlobalPosition, null);
		Transform = Transform.Translated(direction);
		GameManager.Tilemap.UpdateTileState(GlobalPosition, this);
	}
}