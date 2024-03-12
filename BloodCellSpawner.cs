using Godot;

namespace Parasite;

public class BloodCellSpawner : IEntitySpawner<BloodCell>
{
	private readonly PackedScene _redBloodCellResource = GD.Load<PackedScene>("res://RedBloodCell.tscn");
	private readonly PackedScene _whiteBloodCellResource = GD.Load<PackedScene>("res://WhiteBloodCell.tscn");
	private readonly GameManager _gameManager;
	
	public BloodCellSpawner(GameManager gameManager)
	{
		_gameManager = gameManager;
	}
	
	public T Spawn<T>(Vector2 positionOverride = default)
		where T : BloodCell
	{
		PackedScene resource = typeof(T) == typeof(WhiteBloodCell)
			? _whiteBloodCellResource
			: _redBloodCellResource;
		var bloodCell = resource.Instantiate<T>();
		
		Vector3 position = positionOverride.Length() > 0
			? new Vector3(positionOverride.X, 0f, positionOverride.Y)
			: _gameManager.Tilemap.GetOpenTile();
		
		bloodCell.Initialize(_gameManager, this);
		_gameManager.AddChild(bloodCell);

		bloodCell.GlobalPosition = position;
		_gameManager.Tilemap.UpdateTileState(position, bloodCell);

		return bloodCell;
	}

	public void Destroy<T>(T bloodCell, bool triggerRespawn = false)
		where T : BloodCell
	{
		// Respawn before updating tile so that same tile isn't selected
		if (triggerRespawn)
		{
			Spawn<T>();
		}
		
		_gameManager.Tilemap.UpdateTileState(bloodCell.GlobalPosition, null);
		bloodCell.QueueFree();
	}
}