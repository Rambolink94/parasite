using System.Collections.Generic;
using Godot;

namespace Parasite;

public partial class ParasiteController : Node3D, IGameEntity
{
	private PackedScene _segmentResource = GD.Load<PackedScene>("res://ParasiteSegment.tscn");

	private Tilemap _tilemap;

	private readonly List<ParasiteSegment> _segments = new();
	private Vector3[] _availableDirections;

	public List<ParasiteSegment> Segments => _segments;
	
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
			Vector3 position = _segments[0].GlobalPosition + input;
			Tilemap.TileData data = _tilemap.GetTileData(position);

			if (data != null && (data.Occupant is BloodCell || !data.IsOccupied))
			{
				// TODO: Two checks is dumb. Fix this.
				if (data.Occupant is BloodCell bloodCell)
				{
					CreateSegment(position, true);
					
					bloodCell.Destroy();
				}

				UpdateSegments(input);
			}
		}
	}

	public void SetTilemap(Tilemap tilemap)
	{
		_tilemap = tilemap;
	}

	private void UpdateSegments(Vector3 offset)
	{
		Vector3 previousPosition = _segments[0].GlobalPosition;
		for (int i = 1; i < _segments.Count; i++)
		{
			ParasiteSegment current = _segments[i];
			Vector3 toSet = previousPosition;
			previousPosition = current.GlobalPosition;
			
			MoveSegment(current, toSet);
		}
		
		MoveSegment(_segments[0], _segments[0].GlobalPosition + offset, true);
	}

	private ParasiteSegment CreateSegment(Vector3 position, bool deferMove = false)
	{
		var segment = _segmentResource.Instantiate<ParasiteSegment>();
		AddChild(segment);

		if (!deferMove)
		{
			MoveSegment(segment, position);
		}
		
		_segments.Add(segment);

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