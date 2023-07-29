using Godot;
using System;

public partial class globals : Node
{
	private int _scoreValue = 0;
	public int Score
	{
		get
		{
			return _scoreValue;
		}
		set
		{
			_scoreValue = value;
		}
	}
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
