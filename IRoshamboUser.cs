namespace Parasite;

public interface IRoshamboUser
{
    public Roshambo.Option CurrentRoshambo { get; set; }
    public RoshamboController RoshamboController { get; }
    
    public void SetRoshambo(Roshambo.Option option);
}