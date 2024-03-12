using System;

namespace Parasite;

public class EnvironmentEntity : IGameEntity
{
	public event TurnCompletedEventHandler TurnEnded;
	public event Action RoshamboChanged;
	public bool IsTurnActive { get; private set; }
	public Roshambo.Option CurrentRoshambo { get; set; }
	public EntityType EntityType => EntityType.None;
	
	public Roshambo.Option RoleRoshambo()
	{
		Roshambo.Option roshambo = Roshambo.Role();
		CurrentRoshambo = roshambo;

		return roshambo;
	}
	
	public void BeginTurn()
	{
		IsTurnActive = true;
		EndTurn();
	}

	public void EndTurn(bool triggerGameEnd = false)
	{
		IsTurnActive = false;
		TurnEnded?.Invoke(this, triggerGameEnd);
	}
}