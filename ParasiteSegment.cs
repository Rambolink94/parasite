using Godot;

namespace Parasite;

public partial class ParasiteSegment : Node3D, ITileOccupier
{
	private RoshamboController _roshamboController;
	private bool _isHead;

	public RoshamboController RoshamboController
	{
		get
		{
			if (_roshamboController == null)
			{
				_roshamboController = GetNode<RoshamboController>("Roshambo");
			}

			return _roshamboController;
		}
	}
	
	public Roshambo.Option CurrentRoshambo { get; set; }

	public EntityType EntityType => EntityType.Player | EntityType.Parasite;

	public bool IsHead
	{
		get => _isHead;
		set
		{
			if (value)
			{
				Transform = Transform.Scaled(new Vector3(1.2f, 1.2f, 1.2f));
			}
			
			_isHead = value;
		}
	}

	public void SetRoshambo(Roshambo.Option option)
	{
		CurrentRoshambo = option;
		RoshamboController.SetRoshambo(option);
	}
}