using Godot;

namespace Parasite;

public partial class ParasiteSegment : Node3D, ITileOccupier
{
	private bool _isHead;

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
}