using Godot;

namespace Parasite;

public partial class ParasiteSegment : MeshInstance3D, ITileOccupier, IRoshamboUser
{
	private RoshamboController _roshamboController;
	private bool _isHead;

	public Roshambo.Option CurrentRoshambo => Parent.CurrentRoshambo;
	
	public ParasiteEntity Parent { get; private set; }

	public EntityType EntityType => Parent.EntityType;
	
	private RoshamboController RoshamboController =>
		_roshamboController ??= GetNode<RoshamboController>("Roshambo");

	public bool IsHead
	{
		get => _isHead;
		set
		{
			if (value)
			{
				Scale *= 1.3f;
			}
			else
			{
				Scale /= 1.3f;
			}
			
			_isHead = value;
		}
	}

	public void Initialize(ParasiteEntity parent)
	{
		Parent = parent;
		OnRoshamboChanged();
	}

	public void OnRoshamboChanged()
	{
		RoshamboController.SetRoshambo(CurrentRoshambo);
	}

	public void Cut()
	{
		Parent.HandleCutSegment(this);
		
		QueueFree();
	}
}