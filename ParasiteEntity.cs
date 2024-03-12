using System.Collections.Generic;
using Godot;

namespace Parasite;

public partial class ParasiteEntity : Node3D, IGameEntity
{
	private PackedScene _segmentResource = GD.Load<PackedScene>("res://ParasiteSegment.tscn");

	protected Tilemap Tilemap;
	private Roshambo.Option _currentRoshamboOption;
	
	public Roshambo.Option CurrentRoshambo
	{
		get => _currentRoshamboOption;
		set
		{
			_currentRoshamboOption = value;
			
			foreach (ParasiteSegment segment in Segments)
			{
				segment.SetRoshambo(value);
			}
		}
	}

	public bool IsTurnActive { get; private set; }

	public event TurnCompletedEventHandler TurnEnded;
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
		Tilemap = tilemap;
	}

	public Roshambo.Option RoleRoshambo()
	{
		Roshambo.Option roshambo = Roshambo.Role();
		CurrentRoshambo = roshambo;

		return roshambo;
	}
	
	public void BeginTurn()
	{
		IsTurnActive = true;
	}

	public void EndTurn()
	{
		IsTurnActive = false;
		TurnEnded?.Invoke(this);
	}
	
	public void HandleCutSegment(ParasiteSegment segment)
	{
		var index = Segments.IndexOf(segment);
		if (index != Segments.Count - 1)
		{
			// Set segment after this to new head if not tail
			Segments[index + 1].IsHead = true;
		}
		
		Segments.Remove(segment);
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
		segment.SetRoshambo(option);

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
			Tilemap.UpdateTileState(segment.GlobalPosition, null);
		}

		segment.GlobalPosition = position;
		Tilemap.UpdateTileState(position, segment);
	}
}