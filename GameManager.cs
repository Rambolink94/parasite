using System;
using System.Collections.Generic;
using Godot;

namespace Parasite;

public partial class GameManager : Node3D
{
	[Export] public int WhiteBloodCellSpawnRate { get; set; } = 10;
	[Export] public Vector2 PlayerSpawnOverride { get; set; } = Vector2.Zero;
	[Export] public Vector2 PlayerSpawnForwardOverride { get; set; } = Vector2.Zero;
	[Export] public Vector2 WhiteBloodCellSpawnOverride { get; set; } = Vector2.Zero;
	
	public Tilemap Tilemap { get; private set; }
	public PlayerParasite Player { get; private set; }

	private ParasiteSpawner _parasiteSpawner;
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
		Tilemap = GetNode<Tilemap>("../TilemapRoot");
		_parasiteSpawner = GetNode<ParasiteSpawner>("ParasiteSpawner");
		_bloodCellSpawner = GetNode<BloodCellSpawner>("BloodCellSpawner");
		_turnLabel = GetNode<Label>("../TurnLabel");
		_turnCountLabel = GetNode<Label>("../TurnCountLabel");
		
		_turnLabelFormatter = _turnLabel.Text;
		
		Tilemap.Generate();
		
		// Handle player
		var player = _parasiteSpawner.Spawn<PlayerParasite>(this);
		var whiteBloodCell = _bloodCellSpawner.Spawn<WhiteBloodCell>(this);
		_bloodCellSpawner.Spawn<RedBloodCell>(this);

		Player = player;
		
		Enqueue(player);
		Enqueue(whiteBloodCell);
		
		BeginEntityTurn();
	}

	private void Enqueue(IGameEntity entity)
	{
		entity.TurnEnded += OnTurnCompleted;
			
		_gameEntities.Enqueue(entity);
	}

	private void OnTurnCompleted(IGameEntity entity)
	{
		if (_currentEntityTurn != entity)
		{
			throw new InvalidOperationException("A turn was ended for an entity who turn it was not.");
		}

		if (entity is PlayerParasite && _turnCount % WhiteBloodCellSpawnRate == 0)
		{
			var bloodCell = _bloodCellSpawner.Spawn<WhiteBloodCell>(this);
			Enqueue(bloodCell);
		}

		_gameEntities.Enqueue(entity);	// Add original entity back onto queue to reprocess turn.
		
		var validEntity = false;
		while (!validEntity && _gameEntities.Count > 0)
		{
			_currentEntityTurn = _gameEntities.Dequeue();
			if (!((Node)_currentEntityTurn).IsQueuedForDeletion())
			{
				validEntity = true;
			}
			else
			{
				_currentEntityTurn.TurnEnded -= OnTurnCompleted;
			}
		}
		
		BeginEntityTurn();
	}

	private void BeginEntityTurn()
	{
		if (_currentEntityTurn == null)
		{
			_currentEntityTurn = _gameEntities.Dequeue();
		}
		
		if (_currentEntityTurn.EntityType.HasFlag(EntityType.Player))
		{
			_turnCount++;
			_turnCountLabel.Text = _turnCount.ToString();
		}
		
		_turnLabel.Text = string.Format(_turnLabelFormatter, _currentEntityTurn.EntityType);
		_currentEntityTurn.BeginTurn(Roshambo.Role());
	}
}