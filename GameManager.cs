using System;
using System.Collections.Generic;
using Godot;

namespace Parasite;

public partial class GameManager : Node3D
{
	[Export] public int WhiteBloodCellSpawnRate { get; set; } = 10;
	
	private PackedScene _parasiteResource = GD.Load<PackedScene>("res://Parasite.tscn");
	
	private Tilemap _tilemap;
	private BloodCellSpawner _bloodCellSpawner;
	private Label _turnLabel;
	private Label _turnCountLabel;
	private string _turnLabelFormatter;

	private IGameEntity _currentEntityTurn;
	private readonly Queue<IGameEntity> _gameEntities = new();
	private int _turnCount;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_tilemap = GetNode<Tilemap>("../TilemapRoot");
		_bloodCellSpawner = GetNode<BloodCellSpawner>("BloodCellSpawner");
		_turnLabel = GetNode<Label>("../TurnLabel");
		_turnCountLabel = GetNode<Label>("../TurnCountLabel");
		
		_turnLabelFormatter = _turnLabel.Text;
		
		_tilemap.Generate();
		
		// Handle player
		var player = _parasiteResource.Instantiate<ParasiteController>();
		player.SetTilemap(_tilemap);
		player.TurnEnded += OnTurnCompleted;

		_currentEntityTurn = player;
		BeginEntityTurn();
		
		// TODO: Experiment with these numbers to make sure parts aren't spawned out of bounds
		var randX = 1 + GD.Randi() % _tilemap.MapSize - 1;
		var randZ = 1 + GD.Randi() % _tilemap.MapSize - 1;
		
		player.GlobalPosition = new Vector3(randX, 0f, randZ * -1);
		AddChild(player);
		
		var segments = player.Segments;
		_tilemap.UpdateTileState(segments[0].GlobalPosition, segments[0]);
		_tilemap.UpdateTileState(segments[1].GlobalPosition, segments[1]);
		
		// Handle red blood cell
		_bloodCellSpawner.Initialize(_tilemap, player);
		_bloodCellSpawner.SpawnRedBloodCell();
		
		// Handle initial white blood cell
		BloodCell bloodCell = _bloodCellSpawner.SpawnWhiteBloodCell(OnTurnCompleted);
		
		_gameEntities.Enqueue(bloodCell);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnTurnCompleted(IGameEntity entity)
	{
		if (_currentEntityTurn != entity)
		{
			throw new InvalidOperationException("A turn was ended for an entity who turn it was not.");
		}

		if (entity.EntityType == EntityType.Player && _turnCount % WhiteBloodCellSpawnRate == 0)
		{
			BloodCell bloodCell = _bloodCellSpawner.SpawnWhiteBloodCell(OnTurnCompleted);
			
			_gameEntities.Enqueue(bloodCell);
		}

		_gameEntities.Enqueue(entity);	// Add original entity back onto queue to reprocess turn.
		
		bool validEntity = false;
		while (!validEntity && _gameEntities.Count > 0)
		{
			_currentEntityTurn = _gameEntities.Dequeue();
			if (!((Node)_currentEntityTurn).IsQueuedForDeletion())
			{
				validEntity = true;
			}
		}
		
		BeginEntityTurn();
	}

	private void BeginEntityTurn()
	{
		if (_currentEntityTurn.EntityType == EntityType.Player)
		{
			_turnCount++;
			_turnCountLabel.Text = _turnCount.ToString();
		}
		
		_turnLabel.Text = string.Format(_turnLabelFormatter, _currentEntityTurn.EntityType);
		_currentEntityTurn.BeginTurn();
	}
}