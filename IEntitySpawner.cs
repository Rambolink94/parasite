using Godot;

namespace Parasite;

public interface IEntitySpawner<in TEntity>
{
	public T Spawn<T>(GameManager gameManager, Vector2 positionOverride = default)
		where T : TEntity;
	
	public void Destroy<T>(T entity, GameManager gameManager, bool triggerRespawn = false)
		where T : TEntity;
}