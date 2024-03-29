using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Parasite;

public partial class PlayerParasite : ParasiteEntity
{
	private List<Vector3> _availableDirections = new();
	private bool _processedDirections;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!IsTurnActive) return;

		if (!_processedDirections)
		{
			_availableDirections = GetAvailableDirections();
			_processedDirections = true;
		}
		
		if (_availableDirections.Count == 0)
		{
			// Cannot move, so end game.
			EndTurn(triggerGameEnd: true);
		}
		
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
			if (!Move(input, _availableDirections))
			{
				// Could not move toward input direction.
				return;
			}

			_processedDirections = false;
			EndTurn();
		}
	}

	public override void BeginTurn()
	{
		IsTurnActive = true;
	}
}