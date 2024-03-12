using Godot;

namespace Parasite;

public abstract partial class BloodCell : Node3D, ITileOccupier
{
	protected GameManager GameManager;

	private BloodCellSpawner _spawner;
	
	public abstract EntityType EntityType { get; }

	public bool IsWhiteBloodCell => GetType() == typeof(WhiteBloodCell);
	
	public void Initialize(GameManager gameManager, BloodCellSpawner spawner)
	{
		GameManager = gameManager;
		_spawner = spawner;
	}
	
	public void Destroy(bool triggerRespawn = false)
	{
		_spawner.Destroy(this, triggerRespawn);
	}
}