using Godot;

namespace Parasite;

public partial class BloodCellSpawner : Node3D
{
	private PackedScene _bloodCellResource = GD.Load<PackedScene>("res://BloodCell.tscn");
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SpawnRedBloodCell(Tilemap tilemap)
	{
		Vector3 position = tilemap.GetOpenTile();

		var bloodCell = _bloodCellResource.Instantiate<BloodCell>();
		AddChild(bloodCell);

		bloodCell.GlobalPosition = position;
		tilemap.UpdateTileState(position, bloodCell);
	}
}