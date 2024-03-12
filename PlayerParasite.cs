using System.Collections.Generic;
using Godot;

namespace Parasite;

public partial class PlayerParasite : ParasiteEntity
{
	
	public override EntityType EntityType => base.EntityType | EntityType.Player;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!IsTurnActive) return;
		
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
			Tilemap.TileData data = Tilemap.GetTileData(position);

			if (data != null &&
			    data.IsEnterable(EntityType.BloodCell, out ITileOccupier entity, CurrentRoshambo))
			{
				if (entity is BloodCell bloodCell)
				{
					CreateSegment(position, true);
					
					bloodCell.Destroy(!bloodCell.IsWhiteBloodCell);
				}

				UpdateSegments(input);
				EndTurn();
			}
		}
	}

	protected override ParasiteSegment CreateSegment(Vector3 position, bool deferMove = false)
	{
		ParasiteSegment segment = base.CreateSegment(position, deferMove);
		segment.SetController(this);

		return segment;
	}
}