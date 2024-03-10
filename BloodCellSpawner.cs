using Godot;

namespace Parasite;

public partial class BloodCellSpawner : Node3D
{
	private PackedScene _bloodCellResource = GD.Load<PackedScene>("res://BloodCell.tscn");
	private Tilemap _tilemap;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetTilemap(Tilemap tilemap)
	{
		_tilemap = tilemap;
	}
	
	public void SpawnRedBloodCell()
	{
		Vector3 position = _tilemap.GetOpenTile();

		var bloodCell = _bloodCellResource.Instantiate<BloodCell>();
		bloodCell.SetSpawner(this);
		AddChild(bloodCell);

		bloodCell.GlobalPosition = position;
		_tilemap.UpdateTileState(position, bloodCell);
	}

	public void Destroy(BloodCell bloodCell)
	{
		_tilemap.UpdateTileState(bloodCell.GlobalPosition, null);
		bloodCell.QueueFree();
		
		SpawnRedBloodCell();
	}
}