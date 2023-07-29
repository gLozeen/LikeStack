using Godot;
using System;

public partial class game_start : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_start_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://main_frame.tscn");
	}


	private void _on_quit_button_pressed()
	{
		GetTree().Quit();
	}

}




