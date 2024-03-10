using System.Linq;
using Godot;

namespace Parasite;

public partial class BloodCell : Node3D, ITileOccupier, IGameEntity
{
	[Export] public float WanderChance { get; set; } = 0.5f;
	[Export] public int MaxWanderTurns { get; set; } = 5;
	[Export] public int MinWanderTurns { get; set; } = 1;
	
	private BloodCellSpawner _spawner;
	private bool _turnActive;
	private readonly Vector3[] _possibleDirections = { Vector3.Left, Vector3.Forward, Vector3.Right, Vector3.Back };

	private int _wanderTurn;
	private int _requiredWanderTurns;
	private bool _isWandering;
	
	public event TurnCompletedEventHandler TurnEnded;

	public EntityType EntityType => EntityType.BloodCell;
	
	public bool IsWhiteBloodCell { get; private set; }
	
	public void Initialize(BloodCellSpawner spawner, bool isWhiteBloodCell)
	{
		_spawner = spawner;
		IsWhiteBloodCell = isWhiteBloodCell;
	}
	
	public void Destroy(bool triggerRespawn = false)
	{
		_spawner.Destroy(this, triggerRespawn);
	}
	
	public void BeginTurn()
	{
		_turnActive = true;

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
			for (var i = (int)(GD.Randi() % (availableDirections.Count - 1)); i >= 0;)
			{
				Vector3 vec = availableDirections[i];
				Tilemap.TileData data = _spawner.Tilemap.GetTileData(GlobalPosition + vec);
				if (data == null || data.IsOccupied)
				{
					availableDirections.RemoveAt(i);
					i = (int)(GD.Randi() % (availableDirections.Count - 1));
					continue;
				}
				
				UpdateCell(vec);
				EndTurn();

				return;
			}
		}

		_isWandering = false;

		var minDistance = float.MaxValue;
		ParasiteSegment minSegment = _spawner.Player.Segments[0];
		foreach (ParasiteSegment segment in _spawner.Player.Segments)
		{
			var distance = GlobalPosition.DistanceTo(segment.GlobalPosition);
			if (distance < minDistance)
			{
				minDistance = distance;
				minSegment = segment;
			}
		}

		Vector3 direction = (minSegment.GlobalPosition - GlobalPosition).Normalized();
		Vector3 bestDirection = Vector3.Zero;
		
		var dot = 0f;
		foreach (Vector3 vec in _possibleDirections)
		{
			Tilemap.TileData data = _spawner.Tilemap.GetTileData(GlobalPosition + vec);
			if (data == null || data.IsOccupied)
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
		
		UpdateCell(bestDirection);
		EndTurn();
	}

	public void EndTurn()
	{
		_turnActive = false;
		TurnEnded?.Invoke(this);
	}

	private void UpdateCell(Vector3 direction)
	{
		_spawner.Tilemap.UpdateTileState(GlobalPosition, null);
		Transform = Transform.Translated(direction);
		_spawner.Tilemap.UpdateTileState(GlobalPosition, this);
	}
}