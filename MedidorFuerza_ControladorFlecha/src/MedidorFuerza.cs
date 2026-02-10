using Godot;
using System;

public partial class MedidorFuerza : Node2D
{
	[ExportGroup("Referencias Básicas")]
	[Export] public ControladorFlecha ScriptFlecha; 
	[Export] public TextureProgressBar BarraVisual;
	[Export] public RigidBody2D PelotaA;
	[Export] public RigidBody2D PelotaB;
	[Export] public Line2D LineaA;
	[Export] public Line2D LineaB;

	[ExportGroup("Sistema de Colapso")]
	[Export] public Sprite2D PuntoColapso; 
	[Export] public float VelocidadOscilacion = 15.0f;

	[ExportGroup("Sistema de Duelo")]
	[Export] public Sprite2D Objetivo;    // Sprite diana en el mapa
	[Export] public Sprite2D MarcaAzul;  // Sprite fantasma azul
	[Export] public Sprite2D MarcaRojo;  // Sprite fantasma rojo
	[Export] public Label LabelInfo;     // Label para textos

	[ExportGroup("Ajustes Físicos")]
	[Export] public float VelocidadCarga = 150.0f;
	[Export] public float MultiplicadorFuerza = 20.0f;

	private Vector2 _posicionSpawn = new Vector2(0, 0); 
	private enum Estado { Apuntando, CargandoFuerza, Lanzado, EsperandoColapso, FinDuelo }
	private Estado _estadoActual = Estado.Apuntando;
	
	private int _jugadorActual = 1; // 1: Azul, 2: Rojo
	private Vector2 _posFinalAzul;
	private Vector2 _posFinalRojo;
	private bool _fuerzaSubiendo = true;
	private float _tiempoOscilacion = 0.0f;

	public override void _Ready()
	{
		BarraVisual.Visible = false;
		PelotaA.CanSleep = true;
		PelotaB.CanSleep = true;
		PuntoColapso.Visible = false;
		MarcaAzul.Visible = false;
		MarcaRojo.Visible = false;

		ActualizarTextoTurno();

		foreach (Line2D linea in new[] { LineaA, LineaB })
		{
			linea.Visible = false;
			linea.TopLevel = true;
			linea.GlobalPosition = _posicionSpawn;
			linea.ZIndex = 5;
			if (linea.Points.Length < 2) { linea.AddPoint(Vector2.Zero); linea.AddPoint(Vector2.Zero); }
		}
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_accept") && _estadoActual != Estado.FinDuelo)
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
		float t = (Mathf.Sin(_tiempoOscilacion) + 1.0f) / 2.0f;
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
		Vector2 posicionFinal = PuntoColapso.GlobalPosition;
		PuntoColapso.Visible = false;

		if (_jugadorActual == 1)
		{
			_posFinalAzul = posicionFinal;
			MarcaAzul.GlobalPosition = _posFinalAzul;
			MarcaAzul.Visible = true;
			_jugadorActual = 2;
			GetTree().CreateTimer(2.0f).Timeout += ReiniciarParaSiguienteTurno;
		}
		else
		{
			_posFinalRojo = posicionFinal;
			MarcaRojo.GlobalPosition = _posFinalRojo;
			MarcaRojo.Visible = true;
			DeterminarGanador();
		}
	}

	private void DeterminarGanador()
	{
		_estadoActual = Estado.FinDuelo;
		float distAzul = _posFinalAzul.DistanceTo(Objetivo.GlobalPosition);
		float distRojo = _posFinalRojo.DistanceTo(Objetivo.GlobalPosition);

		string resultado = "";
		if (distAzul < distRojo) resultado = "¡GANADOR: AZUL!";
		else if (distRojo < distAzul) resultado = "¡GANADOR: ROJO!";
		else resultado = "¡EMPATE!";

		if (LabelInfo != null) LabelInfo.Text = resultado;

		// Reiniciar el duelo completo tras 5 segundos
		GetTree().CreateTimer(5.0f).Timeout += () => {
			_jugadorActual = 1;
			MarcaAzul.Visible = false;
			MarcaRojo.Visible = false;
			ReiniciarParaSiguienteTurno();
		};
	}

	private void ReiniciarParaSiguienteTurno()
	{
		_estadoActual = Estado.Apuntando;
		ActualizarTextoTurno();
		ScriptFlecha.Reiniciar();
		ScriptFlecha.Visible = true;
		BarraVisual.Value = BarraVisual.MinValue;
		ResetearPelotaFisica(PelotaA);
		ResetearPelotaFisica(PelotaB);
	}

	private void ActualizarTextoTurno()
	{
		if (LabelInfo != null)
			LabelInfo.Text = _jugadorActual == 1 ? "TURNO: AZUL" : "TURNO: ROJO";
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
