using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Parasite;

public partial class ParasiteEntity : Node3D, IGameEntity
{
	private PackedScene _segmentResource = GD.Load<PackedScene>("res://ParasiteSegment.tscn");

	private Tilemap _tilemap;
	private Roshambo.Option _currentRoshamboOption;
	
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
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ParasiteSegment head = CreateSegment(GlobalPosition);
		CreateSegment(GlobalPosition - Vector3.Back);

		head.IsHead = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void Initialize(Tilemap tilemap)
	{
		_tilemap = tilemap;
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
		// Get nearest red blood cell
		// Get nearest white blood cell
		// Get nearest parasite body segment
		// - Equal priority, though the winning roshambo will be chosen
		// Get nearest parasite head
		// - Move away from head if head will reach you before you can consume.
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
		Vector3 position = Segments[0].GlobalPosition + direction;
		if (_tilemap.IsTileEnterable(position, EntityType.BloodCell | EntityType.Parasite, out ITileOccupier entity, CurrentRoshambo))
		{
			if (entity is BloodCell bloodCell)
			{
				CreateSegment(position, true, CurrentRoshambo);
					
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

			UpdateSegments(direction);
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
				if (_tilemap.IsTileEnterable(head.GlobalPosition + direction,
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
	
	protected void UpdateSegments(Vector3 offset)
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

	protected virtual ParasiteSegment CreateSegment(Vector3 position, bool deferMove = false, Roshambo.Option option = Roshambo.Option.None)
	{
		var segment = _segmentResource.Instantiate<ParasiteSegment>();
		AddChild(segment);
		RoshamboChanged += segment.OnRoshamboChanged;

		if (!deferMove)
		{
			MoveSegment(segment, position);
		}
		
		Segments.Add(segment);

		return segment;
	}

	private void MoveSegment(ParasiteSegment segment, Vector3 position, bool ignoreNullSet = false)
	{
		if (!ignoreNullSet)
		{
			_tilemap.UpdateTileState(segment.GlobalPosition, null);
		}

		segment.GlobalPosition = position;
		_tilemap.UpdateTileState(position, segment);
	}
}