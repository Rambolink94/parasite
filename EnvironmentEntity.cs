namespace Parasite;

public class EnvironmentEntity : IGameEntity
{
	public event TurnCompletedEventHandler TurnEnded;
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

	public void EndTurn()
	{
		IsTurnActive = false;
		TurnEnded?.Invoke(this);
	}
}