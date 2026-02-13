using Godot;
using System;

public partial class ControladorFlecha : Node2D
{
	[ExportGroup("Configuración Manual")]
	[Export] public float VelocidadOscilacion = 3.0f;
	[Export] public float AnguloMaximoGrados = 60.0f;
	
	// ¡AQUÍ ESTÁ LA CLAVE! 
	// Si la flecha sale torcida, cambia este número en el Inspector (prueba 0, 90, -90).
	[Export] public float AnguloInicial = 0.0f; 

	private float _tiempoAcumulado = 0.0f;
	public bool Activo = true;

	public override void _Process(double delta)
	{
		if (Activo)
		{
			_tiempoAcumulado += (float)delta * VelocidadOscilacion;
			
			// Convertimos tu ángulo manual a radianes
			float baseRadianes = Mathf.DegToRad(AnguloInicial);
			
			// Calculamos el vaivén
			float oscilacion = Mathf.Sin(_tiempoAcumulado) * Mathf.DegToRad(AnguloMaximoGrados);
			
			// Sumamos: Tu corrección manual + el movimiento
			this.Rotation = baseRadianes + oscilacion;
		}
	}

	public void Reiniciar()
	{
		Activo = true;
		_tiempoAcumulado = 0;
		// Al reiniciar, la ponemos en el ángulo que tú hayas configurado
		Rotation = Mathf.DegToRad(AnguloInicial);
		Visible = true;
	}
}
