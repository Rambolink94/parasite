using System.Collections.Generic;
using Godot;

namespace Parasite;

public partial class GameManager : Node3D
{
	private PackedScene _parasiteResource = GD.Load<PackedScene>("res://Parasite.tscn");
	
	private Tilemap _tilemap;
	private BloodCellSpawner _bloodCellSpawner;
	
	private readonly Queue<IGameEntity> _gameEntities = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_tilemap = GetNode<Tilemap>("../TilemapRoot");
		_bloodCellSpawner = GetNode<BloodCellSpawner>("BloodCellSpawner");
		
		_tilemap.Generate();
		
		// Handle player
		var player = _parasiteResource.Instantiate<ParasiteController>();
		player.SetTilemap(_tilemap);
		
		// TODO: Experiment with these numbers to make sure parts aren't spawned out of bounds
		var randX = GD.Randi() % _tilemap.MapSize - 1;
		var randZ = GD.Randi() % _tilemap.MapSize - 1;
		
		player.GlobalPosition = new Vector3(randX, 0f, randZ * -1);
		AddChild(player);
		
		AddGameEntity(player);
		
		var segments = player.Segments;
		_tilemap.UpdateTileState(segments[0].GlobalPosition, segments[0]);
		_tilemap.UpdateTileState(segments[1].GlobalPosition, segments[1]);
		
		// Handle red blood cell
		_bloodCellSpawner.SetTilemap(_tilemap);
		_bloodCellSpawner.SpawnRedBloodCell();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void AddGameEntity(IGameEntity entity)
	{
		_gameEntities.Enqueue(entity);
		
	}

	public void OnTurnCompleted(IGameEntity sender)
	{
		
	}
}