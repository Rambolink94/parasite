using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Parasite;

public partial class WhiteBloodCell : BloodCell, IGameEntity
{
	[Export] public float WanderChance { get; set; } = 0.5f;
	[Export] public int MaxWanderTurns { get; set; } = 5;
	[Export] public int MinWanderTurns { get; set; } = 1;
	
	public bool IsTurnActive { get; private set; }
	public Roshambo.Option CurrentRoshambo { get; private set; }
	public override EntityType EntityType => EntityType.WhiteBloodCell;

	public event TurnCompletedEventHandler TurnEnded;
	public event Action RoshamboChanged;

	private RoshamboController _roshamboController;
	
	private int _wanderTurn;
	private int _requiredWanderTurns;
	private bool _isWandering;
	
	public Roshambo.Option RoleRoshambo()
	{
		Roshambo.Option roshambo = Roshambo.Role();
		CurrentRoshambo = roshambo;
		
		_roshamboController ??= GetNode<RoshamboController>("Roshambo");
		_roshamboController.SetRoshambo(roshambo);

		return roshambo;
	}
	
	public void BeginTurn()
	{
		IsTurnActive = true;

		Vector3 direction = DetermineDirection();
		
		UpdateTile(direction);
		EndTurn();
	}

	public void EndTurn(bool triggerGameEnd = false)
	{
		IsTurnActive = false;
		TurnEnded?.Invoke(this, triggerGameEnd);
	}
	
	private Vector3 DetermineDirection()
	{
		var directionInfo = new Dictionary<Vector3, ITileOccupier>();
		
		// TODO: Clean this up...it's messy.
		Vector3 bestDirection = Vector3.Zero;
		var chance = GD.Randf();
		if (!_isWandering && chance <= WanderChance)
		{
			SetWandering();
		}
		
		if (_isWandering && _wanderTurn < _requiredWanderTurns)
		{
			_wanderTurn++;
			
			var availableDirections = GameManager.PossibleDirections.ToList();
			for (var i = (int)(GD.Randi() % (availableDirections.Count - 1)); i >= 0 && availableDirections.Count > 0;)
			{
				Vector3 vec = availableDirections[i];

				if (IsDirectionValid(vec, out ITileOccupier affectedEntity))
				{
					directionInfo.Add(vec, affectedEntity);
					bestDirection = vec;
					break;
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

			var allSegments = GameManager.Entities
				.Where(x => x is ParasiteEntity)
				.SelectMany(x => ((ParasiteEntity)x).Segments)
				.ToList();

			var minDistance = float.MaxValue;
			ParasiteSegment minSegment = allSegments[0];
			foreach (ParasiteSegment segment in allSegments)
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
			foreach (Vector3 vec in GameManager.PossibleDirections)
			{
				if (!IsDirectionValid(vec, out ITileOccupier affectedEntity))
				{
					continue;
				}

				directionInfo.Add(vec, affectedEntity);
				var newDot = direction.Dot(vec);
				if (newDot > dot)
				{
					dot = newDot;
					bestDirection = vec;
				}
			}
		}
		
		if (directionInfo.TryGetValue(bestDirection, out ITileOccupier entity)
		    && entity is ParasiteSegment parasiteSegment)
		{
			parasiteSegment.Cut();
			SetWandering();	// Make cell wonder after doing damage.
		}
		
		return bestDirection;
	}

	private bool IsDirectionValid(Vector3 direction, out ITileOccupier entity)
	{
		entity = null;
		Tilemap.TileData data = GameManager.Tilemap.GetTileData(GlobalPosition + direction);
		return data != null
		       && data.IsEnterable(
			       EntityType.Parasite,
			       out entity,
			       CurrentRoshambo);
	}

	private void SetWandering()
	{
		_wanderTurn = 0;
		_requiredWanderTurns = GD.RandRange(MinWanderTurns, MaxWanderTurns);
		_isWandering = true;
	}
	
	private void UpdateTile(Vector3 direction)
	{
		GameManager.Tilemap.UpdateTileState(GlobalPosition, null);
		Transform = Transform.Translated(direction);
		GameManager.Tilemap.UpdateTileState(GlobalPosition, this);
	}
}