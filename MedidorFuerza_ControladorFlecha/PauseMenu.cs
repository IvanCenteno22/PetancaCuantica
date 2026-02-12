using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
	public override void _Ready()
	{
		// Escondemos el menú al iniciar el juego
		Hide();
		
		// Asegúrate de que estos nombres existan dentro de tu escena de pausa
		GetNode<Button>("CenterContainer/VBoxContainer/ResumeBtn").Pressed += OnResumePressed;
		GetNode<Button>("CenterContainer/VBoxContainer/QuitBtn").Pressed += OnQuitPressed;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel")) // "Esc" por defecto
		{
			TogglePause();
		}
	}

	private void TogglePause()
	{
		// Pausamos o despausamos el juego
		GetTree().Paused = !GetTree().Paused;
		
		// Mostramos u ocultamos este menú
		Visible = GetTree().Paused;

		// Liberamos el ratón si está pausado para poder clicar
		if (Visible)
			Input.MouseMode = Input.MouseModeEnum.Visible;
		else
			Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	private void OnResumePressed()
	{
		TogglePause();
	}

	private void OnQuitPressed()
	{
		
		// IMPORTANTE: Antes de cambiar de escena, debemos quitar la pausa
		// Si no, el menú principal aparecerá congelado.
		GetTree().Paused = false;
		
		// Cambiamos a la escena del menú principal
		GetTree().ChangeSceneToFile("res://main_menu.tscn");
	}
}
