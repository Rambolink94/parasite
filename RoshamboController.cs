using System;
using Godot;

namespace Parasite;

public partial class RoshamboController : Node3D
{
	private Node3D _rock;
	private Node3D _paper;
	private Node3D _scissors;

	private Node3D _previous;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_rock = GetNode<Node3D>("Rock");
		_paper = GetNode<Node3D>("Paper");
		_scissors = GetNode<Node3D>("Scissors");

		_rock.Visible = false;
		_paper.Visible = false;
		_scissors.Visible = false;
	}

	public void SetRoshambo(Roshambo.Option option)
	{
		Node3D roshambo = option switch
		{
			Roshambo.Option.Rock => _rock,
			Roshambo.Option.Paper => _paper,
			Roshambo.Option.Scissors => _scissors,
			_ => null,
		};

		if (_previous != null) _previous.Visible = false;
		if (roshambo != null) roshambo.Visible = true;
		
		_previous = roshambo;
	}
}