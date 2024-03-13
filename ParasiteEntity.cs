using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Parasite;

public partial class ParasiteEntity : Node3D, IGameEntity
{
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
		
	public List<ParasiteSegment> Segments { get; private set; } = new();
	
	public virtual EntityType EntityType => EntityType.Parasite;
	
	private PackedScene _segmentResource = GD.Load<PackedScene>("res://ParasiteSegment.tscn");

	private GameManager _gameManager;
	private Color _color;
	private Roshambo.Option _currentRoshamboOption;
	private StandardMaterial3D _materialOverride;
	
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
		
		// TODO: Revisit this. It doesn't work right.
		// Get all occupied tiles who, if segments, do not belong to this.
		var occupiedTiles = _gameManager.Tilemap.Tiles
			.Where(x => x.Occupant != null
			            && !(x.Occupant is ParasiteSegment segment
			                 && segment.Parent == this)).ToList();

		var randomIndex = GD.Randi() % occupiedTiles.Count;
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
		Vector3 bestDirection = Vector3.Zero;
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
		
		Move(bestDirection, directions);
		
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

	protected bool Move(Vector3 direction, List<Vector3> availableDirections)
	{
		bool moveSuccessful = false;
		foreach (ParasiteSegment head in Segments.Where(x => x.IsHead).ToList())
		{
			int index = Segments.IndexOf(head);
			Vector3 position = Segments[index].GlobalPosition + direction;
			if (availableDirections.Contains(direction))
			{
				Tilemap.TileData tile = _gameManager.Tilemap.GetTileData(position);
				if (tile == null) continue;
				
				ITileOccupier entity = tile.Occupant;
				if (entity is BloodCell bloodCell)
				{
					CreateSegment(position, true, index);
					
					bloodCell.Destroy(!bloodCell.IsWhiteBloodCell);
				}
				else if (entity is ParasiteSegment segment)
				{
					if (segment.Parent == this)
					{
						if (!IsSegmentValidTail(segment, head))
						{
							// Attempt to move into self at non-tail position.
							continue;
						}
						
						ReorganizeSegments(head, segment);
					}
					else
					{
						segment.Cut();
					}
				}

				UpdateSegments(direction, index);
				moveSuccessful = true;
			}
		}

		return moveSuccessful;
	}
	
	protected List<Vector3> GetAvailableDirections()
	{
		var availableDirection = new List<Vector3>();
		var heads = Segments.Where(x => x.IsHead);
		foreach (ParasiteSegment head in heads)
		{
			foreach (Vector3 direction in GameManager.PossibleDirections)
			{
				// TODO: Cleanup this an IsEnterable. 
				Tilemap.TileData tile = _gameManager.Tilemap.GetTileData(head.GlobalPosition + direction);
				if (tile == null) continue;
				if (tile.Occupant == null
				    || tile.Occupant is RedBloodCell
				    || (tile.Occupant is ParasiteSegment segment
				        && segment.Parent == this
				        && IsSegmentValidTail(segment))
				    || tile.Occupant is IRoshamboUser user
						&& Roshambo.Test(CurrentRoshambo, user.CurrentRoshambo))
				{
					availableDirection.Add(direction);
				}
			}
		}

		return availableDirection;
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

	private ParasiteSegment CreateSegment(
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

		var newMat = new StandardMaterial3D();
		segment.MaterialOverride = newMat;
		
		AddChild(segment);
		segment.Initialize(this);
		RoshamboChanged += segment.OnRoshamboChanged;

		if (!deferMove)
		{
			MoveSegment(segment, position, moveImmediately: true);
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

	private void MoveSegment(ParasiteSegment segment, Vector3 position, bool ignoreNullSet = false, bool moveImmediately = false)
	{
		if (!ignoreNullSet)
		{
			_gameManager.Tilemap.UpdateTileState(segment.GlobalPosition, null);
		}

		// TODO: Fix tweening
		if (true)
		{
			segment.GlobalPosition = position;
		}
		else
		{
			CreateTween().TweenProperty(segment, "global_position", position, 0.1f);
		}
		
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

	private bool IsSegmentValidTail(ParasiteSegment segment, ParasiteSegment head = null)
	{
		if (head != null)
		{
			var i = Segments.IndexOf(head) + 1;
			while (i < Segments.Count && !Segments[i].IsHead)
			{
				if (Segments[i] == segment)
				{
					// If here, the piece either isn't a tail, or is tail of the same parasite piece.
					return false;
				}

				i++;
			}
		}
		
		var index = Segments.IndexOf(segment);
		return index + 1 >= Segments.Count || Segments[index + 1].IsHead;
	}

	private void ReorganizeSegments(ParasiteSegment head, ParasiteSegment tail)
	{
		// TODO: This is shit. Clean it up if you have the time.
		var combinedSegment = new List<ParasiteSegment> { head };
		var headIndex = Segments.IndexOf(head);
		var index = headIndex + 1;
		while (index < Segments.Count && !Segments[index].IsHead)
		{
			combinedSegment.Add(Segments[index]);
			index++;
		}
		
		index = Segments.IndexOf(tail);
		while (index >= 0)
		{
			combinedSegment.Insert(0, Segments[index]);
			if (Segments[index].IsHead)
			{
				break;
			}
				
			index--;
			
		}
		
		head.IsHead = false;

		var diff = Segments.Except(combinedSegment);
		Segments = diff.Concat(combinedSegment).ToList();
	}
}