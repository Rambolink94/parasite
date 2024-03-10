using System.Collections.Generic;
using Godot;

namespace Parasite;

public partial class ParasiteController : Node3D, IGameEntity
{
	
	private PackedScene _segmentResource = GD.Load<PackedScene>("res://ParasiteSegment.tscn");

	private Tilemap _tilemap;
	private bool _turnActive;
	private Roshambo.Option _currentRoshamboOption;
	
	public EntityType EntityType => EntityType.Player | EntityType.Parasite;

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

	public List<ParasiteSegment> Segments { get; } = new();

	public event TurnCompletedEventHandler TurnEnded;
	
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
		if (!_turnActive) return;
		
		// TODO: Check all possible paths to make sure you can move.
		// If not, you lose!
		
		Vector3 input = Vector3.Zero;
		if (Input.IsActionJustPressed("parasite_left"))
			input = Vector3.Left;
		if (Input.IsActionJustPressed("parasite_forward"))
			input = Vector3.Forward;
		if (Input.IsActionJustPressed("parasite_right"))
			input = Vector3.Right;
		if (Input.IsActionJustPressed("parasite_back"))
			input = Vector3.Back;

		if (input.Length() > 0)
		{
			Vector3 position = Segments[0].GlobalPosition + input;
			Tilemap.TileData data = _tilemap.GetTileData(position);

			if (data != null && data.IsEnterable(EntityType.BloodCell, CurrentRoshambo))
			{
				if (data.Occupant is BloodCell bloodCell)
				{
					if (!Roshambo.Test(CurrentRoshambo, bloodCell.CurrentRoshambo))
					{
						return;
					}
					
					CreateSegment(position, true);
					
					bloodCell.Destroy(!bloodCell.IsWhiteBloodCell);
				}

				UpdateSegments(input);
				EndTurn();
			}
		}
	}

	public void SetTilemap(Tilemap tilemap)
	{
		_tilemap = tilemap;
	}

	private void UpdateSegments(Vector3 offset)
	{
		Vector3 previousPosition = Segments[0].GlobalPosition;
		for (int i = 1; i < Segments.Count; i++)
		{
			ParasiteSegment current = Segments[i];
			Vector3 toSet = previousPosition;
			previousPosition = current.GlobalPosition;
			
			MoveSegment(current, toSet);
		}
		
		MoveSegment(Segments[0], Segments[0].GlobalPosition + offset, true);
	}

	private ParasiteSegment CreateSegment(Vector3 position, bool deferMove = false)
	{
		var segment = _segmentResource.Instantiate<ParasiteSegment>();
		AddChild(segment);

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
	
	public void BeginTurn(Roshambo.Option option)
	{
		_turnActive = true;
		CurrentRoshambo = option;
	}

	public void EndTurn()
	{
		_turnActive = false;
		TurnEnded?.Invoke(this);
	}
}