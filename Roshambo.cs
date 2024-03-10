using System;
using System.Linq;
using Godot;

namespace Parasite;

public static class Roshambo
{
	public static Option Role()
	{
		var options = Enum.GetValues<Option>().ToList();
		options.Remove(Option.None);
		var index = GD.Randi() % options.Count;

		return options[(int)index];
	}

	/// <summary>
	///		Tests whether <paramref name="option1"/> beats <paramref name="option2"/>.
	/// </summary>
	/// <param name="option1"> The option to check success with. </param>
	/// <param name="option2"> The option to test against. </param>
	/// <returns> A value indicating whether <paramref name="option1"/> succeeded. </returns>
	public static bool Test(Option option1, Option option2)
	{
		if (option2 == Option.None)
		{
			return true;
		}
		
		return option1 switch
		{
			Option.Rock => option2 == Option.Scissors,
			Option.Paper => option2 == Option.Rock,
			Option.Scissors => option2 == Option.Paper,
			_ => false,
		};
	}

	public enum Option
	{
		None,
		Rock,
		Paper,
		Scissors
	}
}