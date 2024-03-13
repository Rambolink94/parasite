using Godot;

namespace Parasite;

public class ParasiteSpawner : IEntitySpawner<ParasiteEntity>
{
    private readonly PackedScene _parasiteResource = GD.Load<PackedScene>("res://Parasite.tscn");
    private readonly PackedScene _enemyParasiteResource = GD.Load<PackedScene>("res://EnemyParasite.tscn");
    private readonly GameManager _gameManager;
    
    public ParasiteSpawner(GameManager gameManager)
    {
        _gameManager = gameManager;
    }
    
    public T Spawn<T>(Vector2 positionOverride = default)
        where T : ParasiteEntity
    {
        Tilemap tilemap = _gameManager.Tilemap;
        PackedScene resource = typeof(T) == typeof(PlayerParasite) ? _parasiteResource : _enemyParasiteResource;
        var parasite = resource.Instantiate<T>();

        var r = GD.Randi() % 255;
        var g = GD.Randi() % 255;
        var b = GD.Randi() % 255;

        var color = new Color(r, g, b, 255f);
        
        var x = positionOverride.X;
        var z = positionOverride.Y;
		
        // TODO: This shit doesn't work very well.
        if (positionOverride.Length() <= 0)
        {
            // No spawn override.
            x = GD.RandRange(0, tilemap.MapSize - 1);
            z = GD.RandRange(2, tilemap.MapSize - 2);
        }
        
        parasite.Initialize(_gameManager, color, new Vector3(x, 0f, z * -1));
        _gameManager.AddChild(parasite);

        return parasite;
    }

    public void Destroy<T>(T entity, bool triggerRespawn = false)
        where T : ParasiteEntity
    {
        throw new System.NotImplementedException();
    }
}