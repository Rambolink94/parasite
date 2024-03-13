using System;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace Parasite;

public partial class Tilemap : Node3D
{
	[Export] public int MapSize { get; set; } = 20;

	public TileData[] Tiles => _tileData;
	
	private PackedScene _initialTile = GD.Load<PackedScene>("res://Tile.tscn");
	
	private TileData[] _tileData;
	
	// Called when the node enters the scene tree for the first time.
	public void Generate()
	{
		_tileData = new TileData[MapSize * MapSize];
		for (int x = 0; x < MapSize; x++)
		{
			for (int z = 0; z < MapSize; z++)
			{
				var tile = _initialTile.Instantiate<MeshInstance3D>();
				var label = tile.GetNode<Label3D>("CoordLabel");
				
				Transform3D transform = Transform;
				transform.Origin = new Vector3(x, 0f, z * -1);
				label.Text = $"({x}, {z * -1})";
				
				tile.Transform = transform;

				var tileData = new TileData(transform.Origin, tile);
				_tileData[x * MapSize + z] = tileData;
				
				AddChild(tile);
			}
		}
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
		var openTiles = _tileData.Where(x => x.IsEnterable(EntityType.None, out _)).ToList();
		var index = GD.Randi() % openTiles.Count;	// TODO: If no open tiles, end game.

		TileData cell = openTiles[(int)index];
		Debug.Assert(cell.Occupant == null);
		
		return cell.GlobalPosition;
	}

	public bool IsTileEnterable(Vector3 globalPosition, EntityType allowedTypes, out ITileOccupier affectedEntity, Roshambo.Option option = Roshambo.Option.None)
	{
		affectedEntity = null;
		
		TileData tile = GetTileData(globalPosition);
		return tile?.IsEnterable(allowedTypes, out affectedEntity, option) ?? false;
	}

	public class TileData
	{
		public TileData(Vector3 globalPosition, MeshInstance3D tileMesh)
		{
			GlobalPosition = globalPosition;
			TileMesh = tileMesh;
		}
		
		public Vector3 GlobalPosition { get; }
		
		public ITileOccupier Occupant { get; set; }
		
		public MeshInstance3D TileMesh { get; }
		
		public bool IsEnterable(EntityType allowedTypes, out ITileOccupier affectedEntity, Roshambo.Option option = Roshambo.Option.None)
		{
			affectedEntity = null;
			if (Occupant == null || allowedTypes.HasFlag(Occupant.EntityType))
			{
				if (Occupant is IRoshamboUser entity
				    && !Roshambo.Test(option, entity.CurrentRoshambo))
				{
					return false;
				}
				
				affectedEntity = Occupant;
				return true;
			}

			return false;
		}
	}
}