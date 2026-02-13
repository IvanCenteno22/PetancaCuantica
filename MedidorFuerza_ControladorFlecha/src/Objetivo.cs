using Godot;
using System;

public partial class Objetivo : Sprite2D
{
	[Export] public bool SeMueve = false; 
	[Export] public float Velocidad = 5.0f; // Ajustado para que 5 sea una velocidad normal
	[Export] public float Amplitud = 300.0f; 

	private Vector2 _posicionInicial;
	private float _tiempo = 0.0f;
	private bool _detenido = false;

	public override void _Ready()
	{
		// Guardamos la posición donde lo pusiste en el editor
		_posicionInicial = Position; 
		GD.Print("Objetivo listo. ¿Se mueve?: " + SeMueve);
	}

	public override void _Process(double delta)
	{
		if (SeMueve && !_detenido)
		{
			_tiempo += (float)delta;
			
			// Calculamos el desfase
			float offset = Mathf.Sin(_tiempo * Velocidad) * Amplitud;
			
			// Aplicamos a la posición local
			Position = new Vector2(_posicionInicial.X + offset, _posicionInicial.Y);
		}
	}

	public void DetenerMovimiento()
	{
		_detenido = true;
		GD.Print("Diana detenida en: " + Position.X);
	}

	public void ReiniciarMovimiento()
	{
		_detenido = false;
	}
}
