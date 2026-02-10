using Godot;
using System;

public partial class ControladorFlecha : Node2D
{
	[Export] public float VelocidadOscilacion = 3.0f;
	[Export] public float AnguloMaximoGrados = 60.0f;
	
	private float _tiempoAcumulado = 0.0f;
	public bool Activo = true; // El Medidor controlará esta variable

	public override void _Process(double delta)
	{
		if (Activo)
		{
			_tiempoAcumulado += (float)delta * VelocidadOscilacion;
			float ondaSeno = Mathf.Sin(_tiempoAcumulado);
			this.Rotation = ondaSeno * Mathf.DegToRad(AnguloMaximoGrados);
		}
	}

	public void Reiniciar()
	{
		Activo = true;
		_tiempoAcumulado = 0;
		Rotation = 0;
		Visible = true;
	}
}
