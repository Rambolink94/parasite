using Godot;

namespace Parasite;

public class ParasiteSpawner : IEntitySpawner<ParasiteEntity>
{
    private readonly PackedScene _parasiteResource = GD.Load<PackedScene>("res://Parasite.tscn");
    private readonly GameManager _gameManager;
    
    public ParasiteSpawner(GameManager gameManager)
    {
        _gameManager = gameManager;
    }
    
    public T Spawn<T>(Vector2 positionOverride = default)
        where T : ParasiteEntity
    {
        Tilemap tilemap = _gameManager.Tilemap;
        var parasite = _parasiteResource.Instantiate<T>();

        var r = GD.Randi() % 255;
        var g = GD.Randi() % 255;
        var b = GD.Randi() % 255;

        var color = new Color(r, g, b, 255f);
        
        var x = positionOverride.X;
        var z = positionOverride.Y;
		
        if (positionOverride.Length() <= 0)
        {
            // No spawn override.
            x = GD.RandRange(2, tilemap.MapSize - 2);
            z = GD.RandRange(2, tilemap.MapSize - 2);
        }
        
        _gameManager.AddChild(parasite);
        parasite.Initialize(_gameManager, color, new Vector3(x, 0f, z * -1));

        return parasite;
    }

    public void Destroy<T>(T entity, bool triggerRespawn = false)
        where T : ParasiteEntity
    {
        throw new System.NotImplementedException();
    }
}