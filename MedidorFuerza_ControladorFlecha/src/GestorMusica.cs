using Godot;
using System;

public partial class GestorMusica : Node
{
	// Variables para guardar las referencias a los reproductores
	[Export] public AudioStreamPlayer Reproductor1;
	[Export] public AudioStreamPlayer Reproductor2;

	public override void _Ready()
	{
		// 1. Verificamos que hemos asignado los nodos
		if (Reproductor1 != null && Reproductor2 != null)
		{
			// 2. Les damos al Play a la vez para que vayan sincronizados
			Reproductor1.Play();
			Reproductor2.Play();
			
			GD.Print("¡Música dual iniciada!");
		}
		else
		{
			GD.PrintErr("Falta asignar los reproductores en el GestorMusica");
		}
	}
}
