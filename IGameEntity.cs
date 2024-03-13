using System;
using Godot;

namespace Parasite;

public interface IGameEntity : IRoshamboUser
{
    public event TurnCompletedEventHandler TurnEnded;
    public event Action RoshamboChanged; 
    
    public bool IsTurnActive { get; }
    
    public EntityType EntityType { get; }

    public Roshambo.Option RoleRoshambo();
    public void BeginTurn();
    public void EndTurn(bool triggerGameEnd = false);
}

public delegate void TurnCompletedEventHandler(IGameEntity entity, bool triggerGameEnd = false);

[Flags]
public enum EntityType
{
    None = 0,
    WhiteBloodCell = 1 << 0,
    RedBloodCell = 1 << 1,
    BloodCell = WhiteBloodCell | RedBloodCell,
    Parasite = 1 << 2,
    All = ~(-1 << 3),
}