using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
	public override void _Ready()
	{
		Hide();
		
		// Asegúrate de que las rutas a los nodos sean correctas en tu escena
		var resumeBtn = GetNode<Button>("CenterContainer/VBoxContainer/ResumeBtn");
		var quitBtn = GetNode<Button>("CenterContainer/VBoxContainer/QuitBtn");

		// Conectamos las señales
		resumeBtn.Pressed += OnResumePressed;
		quitBtn.Pressed += OnQuitPressed;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel")) // Esc
		{
			// Opcional: Si quieres que suene también al dar a ESC, descomenta la siguiente línea:
			// if (GestorSonidosUI.Instance != null) GestorSonidosUI.Instance.ReproducirSonidoBoton();
			
			TogglePause();
		}
	}

	private void TogglePause()
	{
		GetTree().Paused = !GetTree().Paused;
		Visible = GetTree().Paused;

		if (Visible)
			Input.MouseMode = Input.MouseModeEnum.Visible;
		else
			Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	private void OnResumePressed()
	{
		// 1. Sonido
		if (GestorSonidosUI.Instance != null)
			GestorSonidosUI.Instance.ReproducirSonidoBoton();

		// 2. Lógica
		TogglePause();
	}

	private void OnQuitPressed()
	{
		// 1. Sonido
		if (GestorSonidosUI.Instance != null)
			GestorSonidosUI.Instance.ReproducirSonidoBoton();

		// 2. Lógica crítica antes de cambiar escena
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://main_menu.tscn");
	}
}
