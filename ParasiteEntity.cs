using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Parasite;

public partial class ParasiteEntity : Node3D, IGameEntity
{
	private PackedScene _segmentResource = GD.Load<PackedScene>("res://ParasiteSegment.tscn");

	private GameManager _gameManager;
	private Color _color;
	private Roshambo.Option _currentRoshamboOption;
	private StandardMaterial3D _materialOverride;
	
	public Roshambo.Option CurrentRoshambo
	{
		get => _currentRoshamboOption;
		private set
		{
			_currentRoshamboOption = value;
			
			RoshamboChanged?.Invoke();
		}
	}

	public bool IsTurnActive { get; protected set; }

	public event TurnCompletedEventHandler TurnEnded;
	public event Action RoshamboChanged;
		
	public List<ParasiteSegment> Segments { get; } = new();
	
	public virtual EntityType EntityType => EntityType.Parasite;
	
	public void Initialize(GameManager gameManager, Color color, Vector3 position)
	{
		_gameManager = gameManager;
		_color = color;
		
		ParasiteSegment head = CreateSegment(position);
		CreateSegment(position - Vector3.Back);

		head.IsHead = true;
	}

	public Roshambo.Option RoleRoshambo()
	{
		Roshambo.Option roshambo = Roshambo.Role();
		CurrentRoshambo = roshambo;

		return roshambo;
	}
	
	public virtual void BeginTurn()
	{
		IsTurnActive = true;

		var directions = GetAvailableDirections();
		
		// TODO: Finish parasite movement
		// Get all occupied tiles who, if segments, do not belong to this.
		var occupiedTiles = _gameManager.Tilemap.Tiles
			.Where(x => x.Occupant != null
			            && !(x.Occupant is ParasiteSegment segment
			                 && segment.Parent == this)).ToList();

		var randomIndex = GD.Randi() % occupiedTiles.Count - 1;
		Tilemap.TileData bestTile = occupiedTiles[(int)randomIndex];
		float bestDistance = float.MaxValue;
		foreach (var head in Segments.Where(x => x.IsHead))
		{
			foreach (Tilemap.TileData tile in occupiedTiles)
			{
				var distance = GlobalPosition.DistanceTo(tile.GlobalPosition);
				if (distance < bestDistance)
				{
					// If closest tile has winning roshambo and the distance
					// would not result in a possibly different roshambo, then skip.
					if (tile.Occupant is IRoshamboUser user
					    && !Roshambo.Test(CurrentRoshambo, user.CurrentRoshambo)
					    && _gameManager.TurnsUntilReRoll > distance * 2)
					{
						continue;
					}
					
					bestDistance = distance;
					bestTile = tile;
				}
			}
		}
		
		// TODO: Get nearest parasite head
		// - Move away from head if head will reach you before you can consume.

		Vector3 bestDirection = directions[0];
		Vector3 direction = (bestTile.GlobalPosition - GlobalPosition).Normalized();
		var dot = 0f;
		foreach (Vector3 vec in directions)
		{
			var newDot = direction.Dot(vec);
			if (newDot > dot)
			{
				dot = newDot;
				bestDirection = vec;
			}
		}
		
		Move(bestDirection);
		
		EndTurn();
	}

	public void EndTurn(bool triggerGameEnd = false)
	{
		IsTurnActive = false;
		TurnEnded?.Invoke(this, triggerGameEnd);
	}
	
	public void HandleCutSegment(ParasiteSegment segment)
	{
		var index = Segments.IndexOf(segment);
		if (index != Segments.Count - 1)
		{
			// Set segment after this to new head if not tail
			Segments[index + 1].IsHead = true;
		}

		RoshamboChanged -= segment.OnRoshamboChanged;
		Segments.Remove(segment);
	}

	protected void Move(Vector3 direction)
	{
		foreach (ParasiteSegment head in Segments.Where(x => x.IsHead).ToList())
		{
			int index = Segments.IndexOf(head);
			Vector3 position = Segments[index].GlobalPosition + direction;
			if (_gameManager.Tilemap.IsTileEnterable(position, EntityType.BloodCell | EntityType.Parasite, out ITileOccupier entity, CurrentRoshambo))
			{
				if (entity is BloodCell bloodCell)
				{
					CreateSegment(position, true, index);
					
					bloodCell.Destroy(!bloodCell.IsWhiteBloodCell);
				}
				else if (entity is ParasiteSegment segment)
				{
					if (segment.Parent == this)
					{
						// Attempt to move into self
						return;
					}
					
					segment.Cut();
				}

				UpdateSegments(direction, index);
			}
		}
	}
	
	protected List<Vector3> GetAvailableDirections()
	{
		var availableDirection = new List<Vector3>();
		var heads = Segments.Where(x => x.IsHead);
		foreach (ParasiteSegment head in heads)
		{
			foreach (Vector3 direction in GameManager.PossibleDirections)
			{
				if (_gameManager.Tilemap.IsTileEnterable(head.GlobalPosition + direction,
					    EntityType.BloodCell | EntityType.Parasite,
					    out ITileOccupier affectedEntity,
					    CurrentRoshambo)
				    && (affectedEntity == null
				        || !affectedEntity.EntityType.HasFlag(EntityType.Player)))
				{
					availableDirection.Add(direction);
				}
			}
		}

		return availableDirection;
	}
	
	private void UpdateSegments(Vector3 offset)
	{
		Vector3 previousPosition = Segments[0].GlobalPosition;
		for (int i = 0; i < Segments.Count; i++)
		{
			ParasiteSegment current = Segments[i];
			Vector3 toSet = previousPosition + (current.IsHead ? offset : Vector3.Zero);
			previousPosition = current.GlobalPosition;
			
			bool setNull = i + 1 == Segments.Count || Segments[i + 1].IsHead;
			
			MoveSegment(current, toSet, !setNull);
			if (i + 1 != Segments.Count && Segments[i + 1].IsHead)
			{
				previousPosition = Segments[i + 1].GlobalPosition;
			}
		}
	}
	
	private void UpdateSegments(Vector3 offset, int startIndex)
	{
		Vector3 previousPosition = Segments[startIndex].GlobalPosition;
		for (int i = startIndex; i < Segments.Count; i++)
		{
			ParasiteSegment current = Segments[i];
			if (i != startIndex && current.IsHead)
			{
				break;
			}
				
			Vector3 toSet = previousPosition + (current.IsHead ? offset : Vector3.Zero);
			previousPosition = current.GlobalPosition;
			
			bool setNull = i + 1 == Segments.Count || Segments[i + 1].IsHead;
			
			MoveSegment(current, toSet, !setNull);
		}
	}

	protected virtual ParasiteSegment CreateSegment(
		Vector3 position,
		bool deferMove = false,
		int headIndex = 0)
	{
		var segment = _segmentResource.Instantiate<ParasiteSegment>();
		//StandardMaterial3D material = GetOrCreateMaterialOverride(segment, _color);

		//segment.SetSurfaceOverrideMaterial(0, material);
		// segment.MaterialOverride.Set("albedo_color", _color);
		if (segment.MaterialOverride is not StandardMaterial3D material)
		{
			throw new NullReferenceException("The segment did not have a material override");
		}
		
		material.AlbedoColor = _color;
		segment.MaterialOverride = material;
		
		AddChild(segment);
		RoshamboChanged += segment.OnRoshamboChanged;

		if (!deferMove)
		{
			MoveSegment(segment, position);
		}
		
		var index = headIndex + 1;
		while (index < Segments.Count && !Segments[index].IsHead)
		{
			index++;
		}

		try
		{
			Segments.Insert(index, segment);
		}
		catch (ArgumentOutOfRangeException )
		{
			// Index is at end of segments, so just add it.
			Segments.Add(segment);
		}

		return segment;
	}

	private void MoveSegment(ParasiteSegment segment, Vector3 position, bool ignoreNullSet = false)
	{
		if (!ignoreNullSet)
		{
			_gameManager.Tilemap.UpdateTileState(segment.GlobalPosition, null);
		}

		segment.GlobalPosition = position;
		_gameManager.Tilemap.UpdateTileState(position, segment);
	}
	
	private StandardMaterial3D GetOrCreateMaterialOverride(ParasiteSegment segment, Color color)
	{
		if (_materialOverride != null) return _materialOverride;
		
		// Material material = segment.Mesh.SurfaceGetMaterial(0);
		// _materialOverride = (StandardMaterial3D)material.Duplicate();

		_materialOverride = new StandardMaterial3D();

		_materialOverride.AlbedoColor = color;

		return _materialOverride;
	}
}