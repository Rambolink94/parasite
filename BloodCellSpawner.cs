using System;
using System.Collections.Generic;
using Godot;

namespace Parasite;

public partial class BloodCellSpawner : Node3D
{
	private PackedScene _redBloodCellResource = GD.Load<PackedScene>("res://RedBloodCell.tscn");
	private PackedScene _whiteBloodCellResource = GD.Load<PackedScene>("res://WhiteBloodCell.tscn");

	private Dictionary<BloodCell, TurnCompletedEventHandler> _handlers = new();
	
	public ParasiteController Player { get; private set; }
	
	public Tilemap Tilemap { get; private set; }

	public void Initialize(Tilemap tilemap, ParasiteController player)
	{
		Tilemap = tilemap;
		Player = player;
	}
	
	public void SpawnRedBloodCell()
	{
		try
		{
			var bloodCell = _redBloodCellResource.Instantiate<BloodCell>();
			SetupBloodCell(bloodCell, false);
		}
		catch (Exception e)
		{
			GD.Print(e);
		}
	}

	public BloodCell SpawnWhiteBloodCell(TurnCompletedEventHandler handler)
	{
		var bloodCell = _whiteBloodCellResource.Instantiate<BloodCell>();
		_handlers.Add(bloodCell, handler);
		bloodCell.TurnEnded += handler;
		
		SetupBloodCell(bloodCell, true);

		return bloodCell;
	}

	public void Destroy(BloodCell bloodCell, bool triggerRespawn = false)
	{
		if (bloodCell.IsWhiteBloodCell)
		{
			if (!_handlers.TryGetValue(bloodCell, out TurnCompletedEventHandler handler))
			{
				throw new InvalidOperationException($"Failed to unsubscribe event handler for '{bloodCell.Name}'");
			}
			
			_handlers.Remove(bloodCell);
			bloodCell.TurnEnded -= handler;	
		}
		
		Tilemap.UpdateTileState(bloodCell.GlobalPosition, null);
		bloodCell.QueueFree();

		if (triggerRespawn)
		{
			SpawnRedBloodCell();
		}
	}

	private void SetupBloodCell(BloodCell bloodCell, bool isWhiteBloodCell)
	{
		Vector3 position = Tilemap.GetOpenTile();
		
		bloodCell.Initialize(this, isWhiteBloodCell);
		AddChild(bloodCell);

		bloodCell.GlobalPosition = position;
		Tilemap.UpdateTileState(position, bloodCell);
	}
}