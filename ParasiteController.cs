using System.Collections.Generic;
using Godot;

namespace Parasite;

public partial class ParasiteController : Node3D
{
	private PackedScene _segmentResource = GD.Load<PackedScene>("res://ParasiteSegment.tscn");

	private readonly List<Node3D> _segments = new();
	private Vector3[] _availableDirections;

	private Node3D _leftBar;
	private Node3D _forwardBar;
	private Node3D _rightBar;

	private Vector3 _previousHeadPosition;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Node3D head = CreateSegment(GlobalPosition);
		Node3D tail = CreateSegment(GlobalPosition - Vector3.Back);

		_leftBar = GetNode<Node3D>("Left");
		_forwardBar = GetNode<Node3D>("Forward");
		_rightBar = GetNode<Node3D>("Right");

		_availableDirections = GetAvailableDirections();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector3 input = Vector3.Zero;
		if (Input.IsActionJustPressed("parasite_left"))
			input = _availableDirections[0];
		if (Input.IsActionJustPressed("parasite_forward"))
			input = _availableDirections[1];
		if (Input.IsActionJustPressed("parasite_right"))
			input = _availableDirections[2];

		if (input.Length() > 0)
		{
			_previousHeadPosition = _segments[0].GlobalPosition;
			Transform = Transform.Translated(input);
			UpdateSegments(input);
		}
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

		_availableDirections = GetAvailableDirections();
	}

	private Vector3[] GetAvailableDirections()
	{
		Vector3 head = _segments[0].Transform.Origin;
		Vector3 next = _segments[1].Transform.Origin;
		
		Vector3 forward = head - next;
		forward = forward.Normalized();
		
		Vector3 right = forward.Cross(Vector3.Up);
		Vector3 left = -right;

		_leftBar.GlobalPosition = Transform.Origin + left;
		_forwardBar.GlobalPosition = Transform.Origin + forward;
		_rightBar.GlobalPosition = Transform.Origin + right;

		return new[] { left, forward, right };
	}

	private Node3D CreateSegment(Vector3 position)
	{
		var segment = _segmentResource.Instantiate<Node3D>();
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