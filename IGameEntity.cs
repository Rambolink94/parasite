using System;
using Godot;

namespace Parasite;

public interface IGameEntity
{
    public event TurnCompletedEventHandler TurnEnded;
    
    public Roshambo.Option CurrentRoshambo { get; set; }
    
    public EntityType EntityType { get; }

    public void BeginTurn(Roshambo.Option option);
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