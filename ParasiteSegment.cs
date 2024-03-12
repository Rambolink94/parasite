using Godot;

namespace Parasite;

public partial class ParasiteSegment : Node3D, ITileOccupier, IRoshamboUser
{
	private PlayerParasite _playerParasite;
	private RoshamboController _roshamboController;
	private bool _isHead;

	public RoshamboController RoshamboController =>
		_roshamboController ??= GetNode<RoshamboController>("Roshambo");
	
	public Roshambo.Option CurrentRoshambo { get; set; }

	public EntityType EntityType => EntityType.Player | EntityType.Parasite;

	public bool IsHead
	{
		get => _isHead;
		set
		{
			if (value)
			{
				Scale *= Vector3.One * 1.3f;
			}
			
			_isHead = value;
		}
	}

	public void SetController(PlayerParasite controller)
	{
		_playerParasite = controller;
	}

	public void SetRoshambo(Roshambo.Option option)
	{
		CurrentRoshambo = option;
		RoshamboController.SetRoshambo(option);
	}

	public void Cut()
	{
		_playerParasite.HandleCutSegment(this);
		
		QueueFree();
	}
}