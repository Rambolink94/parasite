using Godot;

namespace Parasite;

public partial class BloodCell : Node3D, ITileOccupier, IGameEntity
{
	private BloodCellSpawner _spawner;
	private bool _turnActive;
	
	public event TurnCompletedEventHandler TurnEnded;

	public EntityType EntityType => EntityType.BloodCell;
	
	public bool IsWhiteBloodCell { get; private set; }
	
	public void Initialize(BloodCellSpawner spawner, bool isWhiteBloodCell)
	{
		_spawner = spawner;
		IsWhiteBloodCell = isWhiteBloodCell;
	}
	
	public void Destroy(bool triggerRespawn = false)
	{
		_spawner.Destroy(this, triggerRespawn);
	}
	
	public void BeginTurn()
	{
		_turnActive = true;

		var minDistance = float.MaxValue;
		ParasiteSegment minSegment = _spawner.Player.Segments[0];
		foreach (ParasiteSegment segment in _spawner.Player.Segments)
		{
			var distance = GlobalPosition.DistanceTo(segment.GlobalPosition);
			if (distance < minDistance)
			{
				minDistance = distance;
				minSegment = segment;
			}
		}

		Vector3 direction = (minSegment.GlobalPosition - GlobalPosition).Normalized();
		Vector3 bestDirection = Vector3.Zero;
		
		var dot = 0f;
		foreach (Vector3 vec in new[] { Vector3.Left, Vector3.Forward, Vector3.Right, Vector3.Back })
		{
			Tilemap.TileData data = _spawner.Tilemap.GetTileData(GlobalPosition + vec);
			if (data == null || data.IsOccupied)
			{
				continue;
			}
				
			var newDot = direction.Dot(vec);
			if (newDot > dot)
			{
				dot = newDot;
				bestDirection = vec;
			}
		}

		// TODO: These updates could probably be cleaner.
		_spawner.Tilemap.UpdateTileState(GlobalPosition, null);
		Transform = Transform.Translated(bestDirection);
		_spawner.Tilemap.UpdateTileState(GlobalPosition, this);
		
		EndTurn();
	}

	public void EndTurn()
	{
		_turnActive = false;
		TurnEnded?.Invoke(this);
	}
}