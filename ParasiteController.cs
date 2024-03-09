using System.Collections.Generic;
using Godot;

namespace Parasite;

public partial class ParasiteController : Node3D
{
	private PackedScene _segmentResource = GD.Load<PackedScene>("res://ParasiteSegment.tscn");

	private Tilemap _tilemap;

	private readonly List<ParasiteSegment> _segments = new();
	private Vector3[] _availableDirections;

	private Vector3 _previousHeadPosition;

	public List<ParasiteSegment> Segments => _segments;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CreateSegment(GlobalPosition);
		CreateSegment(GlobalPosition - Vector3.Back);
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
			_previousHeadPosition = _segments[0].GlobalPosition;

			Vector3 position = GlobalPosition + input;
			var data = _tilemap.GetTileData(position);

			if (data.Occupant is BloodCell || !data.IsOccupied)
			{
				GlobalPosition = position;
				
				// TODO: Two checks is dumb. Fix this.
				if (data.Occupant is BloodCell bloodCell)
				{
					CreateSegment(position);
					
					bloodCell.QueueFree();
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
		var previous = _segments[0];
		for (int i = 1; i < _segments.Count; i++)
		{
			var current = _segments[i];
			MoveSegment(current, previous.GlobalPosition - offset);

			previous = current;
		}
	}

	private Node3D CreateSegment(Vector3 position)
	{
		var segment = _segmentResource.Instantiate<ParasiteSegment>();
		AddChild(segment);
		
		MoveSegment(segment, position);
		_segments.Add(segment);

		return segment;
	}

	private void MoveSegment(Node3D segment, Vector3 position)
	{
		segment.GlobalPosition = position;
	}
}