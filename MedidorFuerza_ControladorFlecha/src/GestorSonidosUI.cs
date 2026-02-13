using Godot;
using System;

public partial class GestorSonidosUI : Node
{
	// Esto permite acceder al script desde cualquier lado escribiendo "GestorSonidosUI.Instance"
	public static GestorSonidosUI Instance;

	[Export] public AudioStreamPlayer AudioBoton;

	public override void _Ready()
	{
		// Guardamos la referencia a nosotros mismos
		Instance = this;
	}

	public void ReproducirSonidoBoton()
	{
		if (AudioBoton != null)
		{
			// Opcional: Variar un poco el tono para que no sea robótico
			AudioBoton.PitchScale = (float)GD.RandRange(0.95, 1.05);
			AudioBoton.Play();
		}
	}
}
