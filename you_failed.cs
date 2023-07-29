using Godot;
using System;

public partial class you_failed : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var lbl = GetNode<Label>("/root/Scene/VBoxContainer/Score");
		var global = GetNode<globals>("/root/Globals");
		lbl.Text = (global.Score).ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void _on_repeat_pressed()
	{
			GetTree().ChangeSceneToFile("res://main_frame.tscn");
	}
	private void _on_quit_pressed()
	{
			GetTree().Quit();
	}

}



