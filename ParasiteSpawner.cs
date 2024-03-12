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
        
        parasite.Initialize(_gameManager.Tilemap);
        
        var x = positionOverride.X;
        var z = positionOverride.Y;
		
        if (positionOverride.Length() <= 0)
        {
            // No spawn override.
            x = GD.RandRange(2, tilemap.MapSize - 2);
            z = GD.RandRange(2, tilemap.MapSize - 2);
        }

        // TODO: Move segments, not player
        parasite.GlobalPosition = new Vector3(x, 0f, z * -1);
        _gameManager.AddChild(parasite);
		
        var segments = parasite.Segments;
        tilemap.UpdateTileState(segments[0].GlobalPosition, segments[0]);
        tilemap.UpdateTileState(segments[1].GlobalPosition, segments[1]);

        return parasite;
    }

    public void Destroy<T>(T entity, bool triggerRespawn = false)
        where T : ParasiteEntity
    {
        throw new System.NotImplementedException();
    }
}