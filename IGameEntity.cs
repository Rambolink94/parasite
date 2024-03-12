using System;
using Godot;

namespace Parasite;

public interface IGameEntity
{
    public event TurnCompletedEventHandler TurnEnded;
    
    public bool IsTurnActive { get; }
    
    public Roshambo.Option CurrentRoshambo { get; }
    
    public EntityType EntityType { get; }

    public Roshambo.Option RoleRoshambo();
    public void BeginTurn();
    public void EndTurn();
}

public delegate void TurnCompletedEventHandler(IGameEntity entity);

[Flags]
public enum EntityType
{
    None = 0,
    Player = 1 << 0,
    WhiteBloodCell = 1 << 1,
    RedBloodCell = 1 << 2,
    BloodCell = WhiteBloodCell | RedBloodCell,
    Parasite = 1 << 3,
    All = ~(-1 << 4),
}