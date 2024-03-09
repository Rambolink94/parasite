using Godot;

namespace Parasite;

public partial class Tilemap : Node3D
{
	[Export] public int MapSize { get; set; } = 20;
	
	private PackedScene _initialTile = GD.Load<PackedScene>("res://Tile.tscn");
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int x = 0; x < MapSize; x++)
		{
			for (int z = 0; z < MapSize; z++)
			{
				var node = _initialTile.Instantiate<Node3D>();

				var transform = Transform;
				transform.Origin = new Vector3(x, 0f, z);
				node.Transform = transform;
				
				AddChild(node);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}