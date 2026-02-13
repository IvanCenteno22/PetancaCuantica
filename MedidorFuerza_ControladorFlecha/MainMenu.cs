using Godot;
using System;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		// Conexión de señales (esto lo tenías bien)
		GetNode<Button>("CenterContainer/VBoxContainer/start").Pressed += OnstartPressed;
		GetNode<Button>("CenterContainer/VBoxContainer/quit").Pressed += OnquitPressed;
	}

	private void OnstartPressed()
	{
		// 1. Reproducir sonido (usamos ?. por si acaso el Gestor no está cargado, que no crashee)
		if (GestorSonidosUI.Instance != null)
			GestorSonidosUI.Instance.ReproducirSonidoBoton();

		// 2. Cambiar escena
		GetTree().ChangeSceneToFile("res://FirstGame.tscn");
	}

	private void OnquitPressed()
	{
		// 1. Reproducir sonido
		if (GestorSonidosUI.Instance != null)
			GestorSonidosUI.Instance.ReproducirSonidoBoton();

		// 2. Salir
		GetTree().Quit();
	}
}
