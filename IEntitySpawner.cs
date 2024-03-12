using Godot;

namespace Parasite;

public interface IEntitySpawner<in TEntity>
{
	public T Spawn<T>(Vector2 positionOverride = default)
		where T : TEntity;
	
	public void Destroy<T>(T entity, bool triggerRespawn = false)
		where T : TEntity;
}