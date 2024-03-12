using System;
using System.Collections.Generic;
using Godot;

namespace Parasite;

public partial class BloodCellSpawner : Node3D, IEntitySpawner<BloodCell>
{
	private PackedScene _redBloodCellResource = GD.Load<PackedScene>("res://RedBloodCell.tscn");
	private PackedScene _whiteBloodCellResource = GD.Load<PackedScene>("res://WhiteBloodCell.tscn");

	public T Spawn<T>(GameManager gameManager, Vector2 positionOverride = default)
		where T : BloodCell
	{
		PackedScene resource = typeof(T) == typeof(WhiteBloodCell)
			? _whiteBloodCellResource
			: _redBloodCellResource;
		var bloodCell = resource.Instantiate<T>();
		
		Vector3 position = positionOverride.Length() > 0
			? new Vector3(positionOverride.X, 0f, positionOverride.Y)
			: gameManager.Tilemap.GetOpenTile();
		
		bloodCell.Initialize(gameManager, this);
		AddChild(bloodCell);

		bloodCell.GlobalPosition = position;
		gameManager.Tilemap.UpdateTileState(position, bloodCell);

		return bloodCell;
	}

	public void Destroy<T>(T bloodCell, GameManager gameManager, bool triggerRespawn = false)
		where T : BloodCell
	{
		gameManager.Tilemap.UpdateTileState(bloodCell.GlobalPosition, null);
		bloodCell.QueueFree();

		if (triggerRespawn)
		{
			Spawn<T>(gameManager);
		}
	}
}