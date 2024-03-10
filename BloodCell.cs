using Godot;

namespace Parasite;

public partial class BloodCell : Node3D, ITileOccupier
{
	private BloodCellSpawner _spawner;

	public void SetSpawner(BloodCellSpawner spawner)
	{
		_spawner = spawner;
	}
	
	public void Destroy()
	{
		_spawner.Destroy(this);
	}
}