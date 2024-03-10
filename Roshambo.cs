using System;
using Godot;

namespace Parasite;

public partial class Roshambo : Node
{
	public Option Role()
	{
		var options = Enum.GetValues<Option>();
		var index = GD.Randi() % options.Length;

		return options[index];
	}

	public enum Option
	{
		Rock,
		Paper,
		Scissors
	}
}