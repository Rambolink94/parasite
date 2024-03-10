using System;
using Godot;

namespace Parasite;

public interface IGameEntity
{
    public event TurnCompletedEventHandler TurnEnded;
    
    public EntityType EntityType { get; }

    public void BeginTurn();
    public void EndTurn();
}

public delegate void TurnCompletedEventHandler(IGameEntity entity);

public enum EntityType
{
    Player,
    BloodCell,
    Parasite,
}