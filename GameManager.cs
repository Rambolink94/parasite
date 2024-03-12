using System;
using System.Collections.Generic;
using Godot;

namespace Parasite;

public partial class GameManager : Node3D
{
	[Export] public int WhiteBloodCellSpawnRate { get; set; } = 10;
	[Export] public int ReRollRate { get; set; } = 10;
	[Export] public int StartingParasiteLength { get; set; } = 2;
	[Export] public Vector2 PlayerSpawnOverride { get; set; } = Vector2.Zero;
	[Export] public Vector2 PlayerSpawnForwardOverride { get; set; } = Vector2.Zero;
	[Export] public Vector2 WhiteBloodCellSpawnOverride { get; set; } = Vector2.Zero;
	
	public Tilemap Tilemap { get; private set; }
	public PlayerParasite Player { get; private set; }
	
	public static readonly Vector3[] PossibleDirections = { Vector3.Left, Vector3.Forward, Vector3.Right, Vector3.Back };

	private ParasiteSpawner _parasiteSpawner;
	private BloodCellSpawner _bloodCellSpawner;
	private Label _turnLabel;
	private Label _turnCountLabel;
	private Label _turnsUntilReRollLabel;
	private string _turnLabelFormatter;

	private IGameEntity _currentEntityTurn;
	private readonly Queue<IGameEntity> _gameEntities = new();
	private int _turnResolutions;
	private int _turnsUntilReRoll;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Tilemap = GetNode<Tilemap>("../TilemapRoot");
		_turnLabel = GetNode<Label>("../TurnLabel");
		_turnCountLabel = GetNode<Label>("../TurnCountLabel");
		_turnsUntilReRollLabel = GetNode<Label>("../ReRollBox/TurnsUntilReRollLabel");

		_parasiteSpawner = new ParasiteSpawner(this);
		_bloodCellSpawner = new BloodCellSpawner(this);
		
		_turnLabelFormatter = _turnLabel.Text;
		
		Tilemap.Generate();

		var environment = new EnvironmentEntity();
		var player = _parasiteSpawner.Spawn<PlayerParasite>();
		var whiteBloodCell = _bloodCellSpawner.Spawn<WhiteBloodCell>();
		_bloodCellSpawner.Spawn<RedBloodCell>();

		Player = player;
		
		Enqueue(environment);
		Enqueue(player);
		Enqueue(whiteBloodCell);
		
		BeginEntityTurn();
	}

	private void Enqueue(IGameEntity entity)
	{
		entity.TurnEnded += OnTurnCompleted;
			
		_gameEntities.Enqueue(entity);
	}

	private void OnTurnCompleted(IGameEntity entity, bool triggerGameEnd = false)
	{
		if (triggerGameEnd)
		{
			entity.TurnEnded -= OnTurnCompleted;
			
			while (_gameEntities.Count > 0)
			{
				IGameEntity entityToCleanup = _gameEntities.Dequeue();
				entityToCleanup.TurnEnded -= OnTurnCompleted;
			}
			
			// TODO: Display game ended screen
			return;
		}
		
		if (_currentEntityTurn != entity)
		{
			throw new InvalidOperationException("A turn was ended for an entity who turn it was not.");
		}

		if (entity is PlayerParasite && _turnResolutions % WhiteBloodCellSpawnRate == 0)
		{
			var bloodCell = _bloodCellSpawner.Spawn<WhiteBloodCell>();
			Enqueue(bloodCell);
		}

		_gameEntities.Enqueue(entity);	// Add original entity back onto queue to reprocess turn.
		
		var validEntity = false;
		while (!validEntity && _gameEntities.Count > 0)
		{
			_currentEntityTurn = _gameEntities.Dequeue();
			if (_currentEntityTurn is EnvironmentEntity || !((Node)_currentEntityTurn).IsQueuedForDeletion())
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
		_currentEntityTurn ??= _gameEntities.Dequeue();
		
		if (_currentEntityTurn is EnvironmentEntity)
		{
			_turnResolutions++;
			_turnCountLabel.Text = _turnResolutions.ToString();

			_turnsUntilReRoll--;
			
			if (_turnsUntilReRoll <= 0)
			{
				foreach (IGameEntity entity in _gameEntities)
				{
					entity.RoleRoshambo();
				}

				_turnsUntilReRoll = ReRollRate;
			}

			_turnsUntilReRollLabel.Text = _turnsUntilReRoll.ToString();
		}
		
		_turnLabel.Text = string.Format(_turnLabelFormatter, _currentEntityTurn.EntityType);
		_currentEntityTurn.BeginTurn();
	}
}