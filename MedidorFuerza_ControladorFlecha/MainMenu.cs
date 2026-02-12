using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("CenterContainer/VBoxContainer/start").Pressed += OnstartPressed;
		GetNode<Button>("CenterContainer/VBoxContainer/quit").Pressed += OnquitPressed;
	}

	private void OnstartPressed()
	{
		GetTree().ChangeSceneToFile("res://FirstGame.tscn");
	}

	private void OnquitPressed()
	{
		GetTree().Quit();
	}
}
