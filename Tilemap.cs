using System;
using System.Linq;
using Godot;

namespace Parasite;

public partial class Tilemap : Node3D
{
	[Export] public int MapSize { get; set; } = 20;
	
	private PackedScene _initialTile = GD.Load<PackedScene>("res://Tile.tscn");
	private PackedScene _parasiteResource = GD.Load<PackedScene>("res://Parasite.tscn");
	
	private TileData[] _tileData;

	private BloodCellSpawner _bloodCellSpawner;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_bloodCellSpawner = GetNode<BloodCellSpawner>("BloodCellSpawner");
		
		// TODO: Experiment with these numbers to make sure parts aren't spawned out of bounds
		var randX = GD.Randi() % MapSize - 1;
		var randZ = GD.Randi() % MapSize - 1;
		
		_tileData = new TileData[MapSize * MapSize];
		for (int x = 0; x < MapSize; x++)
		{
			for (int z = 0; z < MapSize; z++)
			{
				var tile = _initialTile.Instantiate<MeshInstance3D>();
				
				Transform3D transform = Transform;
				transform.Origin = new Vector3(x, 0f, z * -1);
				tile.Transform = transform;

				var tileData = new TileData(transform.Origin, tile);
				_tileData[x * MapSize + z] = tileData;
				
				AddChild(tile);
			}
		}

		// Handle player
		var player = _parasiteResource.Instantiate<ParasiteController>();
		player.SetTilemap(this);
		
		player.GlobalPosition = new Vector3(randX, 0f, randZ * -1);
		AddChild(player);
		
		var segments = player.Segments;
		UpdateTileState(segments[0].GlobalPosition, segments[0]);
		UpdateTileState(segments[1].GlobalPosition, segments[1]);
		
		// Handle red blood cell
		_bloodCellSpawner.SetTilemap(this);
		_bloodCellSpawner.SpawnRedBloodCell();
	}

	public void UpdateTileState(Vector3 globalPosition, ITileOccupier occupant)
	{
		TileData tileData = GetTileData(globalPosition);
		
		tileData.Occupant = occupant;
		tileData.TileMesh.GetChild<Node3D>(0).Visible = occupant != null;
	}

	public TileData GetTileData(Vector3 globalPosition)
	{
		try
		{
			var x = globalPosition.X;
			var z = globalPosition.Z;

			GD.Print("X: ", x, " Z: ", z);
			if (x < 0 || x >= MapSize || z > 0 || z <= -MapSize)
			{
				return null;
			}
			
			return _tileData[(int)(x * MapSize - z)];
		}
		catch (IndexOutOfRangeException)
		{
			return null;
		}
	}

	public Vector3 GetOpenTile()
	{
		var openCells = _tileData.Where(x => !x.IsOccupied).ToList();
		var index = GD.Randi() % openCells.Count;

		return openCells[(int)index].GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public class TileData
	{
		public TileData(Vector3 globalPosition, MeshInstance3D tileMesh)
		{
			GlobalPosition = globalPosition;
			TileMesh = tileMesh;
		}
		
		public Vector3 GlobalPosition { get; set; }

		public bool IsOccupied => Occupant != null;
		
		public ITileOccupier Occupant { get; set; }
		
		public MeshInstance3D TileMesh { get; }
	}
}