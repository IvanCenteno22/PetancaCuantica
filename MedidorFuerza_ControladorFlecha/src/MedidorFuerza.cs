using Godot;
using System;

public partial class MedidorFuerza : Node2D
{
	[Export] public ControladorFlecha ScriptFlecha; 
	[Export] public TextureProgressBar BarraVisual;
	[Export] public RigidBody2D PelotaA;
	[Export] public RigidBody2D PelotaB;

	[Export] public Line2D LineaA;
	[Export] public Line2D LineaB;

	// --- SOLO EL SPRITE ---
	[Export] public Sprite2D PuntoColapso; 
	[Export] public float VelocidadOscilacion = 4.0f;

	[Export] public float VelocidadCarga = 150.0f;
	[Export] public float MultiplicadorFuerza = 20.0f;

	private Vector2 _posicionSpawn = new Vector2(0, 0); 

	private enum Estado { Apuntando, CargandoFuerza, Lanzado, EsperandoColapso }
	private Estado _estadoActual = Estado.Apuntando;
	private bool _fuerzaSubiendo = true;
	private float _tiempoOscilacion = 0.0f;

	public override void _Ready()
	{
		BarraVisual.Visible = false;
		PelotaA.CanSleep = true;
		PelotaB.CanSleep = true;
		
		PuntoColapso.Visible = false;

		foreach (Line2D linea in new[] { LineaA, LineaB })
		{
			linea.Visible = false;
			linea.TopLevel = true;
			linea.GlobalPosition = _posicionSpawn;
			linea.ZIndex = 5;
			// Seguridad para las líneas de proyección
			if (linea.Points.Length < 2) { linea.AddPoint(Vector2.Zero); linea.AddPoint(Vector2.Zero); }
		}
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_accept"))
		{
			if (_estadoActual != Estado.Lanzado) AvanzarEstado();
		}

		if (_estadoActual == Estado.CargandoFuerza)
		{
			OscilarBarra((float)delta);
			ActualizarDibujoProyeccion();
		}
		else if (_estadoActual == Estado.EsperandoColapso)
		{
			ProcesarMovimientoPunto((float)delta);
		}
	}

	private void ProcesarMovimientoPunto(float delta)
	{
		_tiempoOscilacion += delta * VelocidadOscilacion;
		
		// Valor entre 0 y 1 para el movimiento de vaivén
		float t = (Mathf.Sin(_tiempoOscilacion) + 1.0f) / 2.0f;

		// Movemos el punto entre Pelota A y Pelota B usando LERP
		// No necesitamos la Line2D para calcular la posición
		PuntoColapso.GlobalPosition = PelotaA.GlobalPosition.Lerp(PelotaB.GlobalPosition, t);
	}

	private void ActualizarDibujoProyeccion()
	{
		float anguloBase = ScriptFlecha.GlobalRotation - (Mathf.Pi / 2.0f);
		float fuerzaActual = (float)BarraVisual.Value;
		float dispersionRadianes = fuerzaActual * 0.008f; 
		float largoLinea = fuerzaActual * 2.5f;

		Vector2 dirA = Vector2.Right.Rotated(anguloBase - dispersionRadianes + Mathf.Pi);
		Vector2 dirB = Vector2.Right.Rotated(anguloBase + dispersionRadianes + Mathf.Pi);

		if (LineaA.Points.Length < 2) LineaA.AddPoint(Vector2.Zero);
		if (LineaB.Points.Length < 2) LineaB.AddPoint(Vector2.Zero);

		LineaA.SetPointPosition(1, dirA * largoLinea);
		LineaB.SetPointPosition(1, dirB * largoLinea);
	}

	private void OscilarBarra(float delta)
	{
		float paso = VelocidadCarga * delta;
		if (_fuerzaSubiendo) {
			BarraVisual.Value += paso;
			if (BarraVisual.Value >= BarraVisual.MaxValue) _fuerzaSubiendo = false;
		} else {
			BarraVisual.Value -= paso;
			if (BarraVisual.Value <= 0) _fuerzaSubiendo = true;
		}
	}

	private void AvanzarEstado()
	{
		if (_estadoActual == Estado.Apuntando)
		{
			ScriptFlecha.Activo = false;
			_estadoActual = Estado.CargandoFuerza;
			BarraVisual.Visible = true;
			LineaA.Visible = true; LineaB.Visible = true;
		}
		else if (_estadoActual == Estado.CargandoFuerza)
		{
			_estadoActual = Estado.Lanzado;
			LineaA.Visible = false; LineaB.Visible = false;
			EjecutarLanzamientoCuantico();
		}
		else if (_estadoActual == Estado.EsperandoColapso)
		{
			EjecutarColapsoEnPunto();
		}
	}

	private void EjecutarLanzamientoCuantico()
	{
		float anguloBase = ScriptFlecha.Rotation - (Mathf.Pi / 2.0f);
		float fuerza = (float)BarraVisual.Value * MultiplicadorFuerza;
		float dispersion = (float)BarraVisual.Value * 0.008f; 

		PelotaA.SleepingStateChanged += AlPararseLasPelotas;
		PelotaB.SleepingStateChanged += AlPararseLasPelotas;

		PelotaA.ApplyImpulse(Vector2.Right.Rotated(anguloBase - dispersion) * fuerza);
		PelotaB.ApplyImpulse(Vector2.Right.Rotated(anguloBase + dispersion) * (fuerza * 0.95f));

		BarraVisual.Visible = false;
		ScriptFlecha.Visible = false;
	}

	private void AlPararseLasPelotas()
	{
		if (PelotaA.Sleeping && PelotaB.Sleeping)
		{
			PelotaA.SleepingStateChanged -= AlPararseLasPelotas;
			PelotaB.SleepingStateChanged -= AlPararseLasPelotas;
			
			_estadoActual = Estado.EsperandoColapso;
			PuntoColapso.Visible = true;
			_tiempoOscilacion = 0;
		}
	}

	private void EjecutarColapsoEnPunto()
	{
		_estadoActual = Estado.Lanzado; 

		Vector2 posicionFinal = PuntoColapso.GlobalPosition;

		PelotaA.GlobalPosition = posicionFinal;
		PelotaB.Visible = false;
		PelotaB.ProcessMode = ProcessModeEnum.Disabled;

		PuntoColapso.Visible = false;

		GetTree().CreateTimer(3.0f).Timeout += ReiniciarJuego;
	}

	public void ReiniciarJuego()
	{
		_estadoActual = Estado.Apuntando;
		ScriptFlecha.Reiniciar();
		ScriptFlecha.Visible = true;
		BarraVisual.Value = 0;
		BarraVisual.Visible = false;
		PuntoColapso.Visible = false;

		ResetearPelotaFisica(PelotaA);
		ResetearPelotaFisica(PelotaB);
	}

	private void ResetearPelotaFisica(RigidBody2D pelota)
	{
		pelota.ProcessMode = ProcessModeEnum.Inherit;
		pelota.Visible = true;
		pelota.Sleeping = false;
		pelota.LinearVelocity = Vector2.Zero;
		pelota.AngularVelocity = 0;

		var state = PhysicsServer2D.BodyGetDirectState(pelota.GetRid());
		if (state != null)
		{
			Transform2D t = pelota.GlobalTransform;
			t.Origin = _posicionSpawn;
			state.Transform = t;
			state.LinearVelocity = Vector2.Zero;
			state.AngularVelocity = 0;
		}
		pelota.SetDeferred(RigidBody2D.PropertyName.GlobalPosition, _posicionSpawn);
	}
}
