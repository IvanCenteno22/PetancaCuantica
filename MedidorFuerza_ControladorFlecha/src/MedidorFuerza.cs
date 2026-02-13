using Godot;
using System;

public partial class MedidorFuerza : Node2D
{
	[ExportGroup("Referencias Básicas")]
	[Export] public ControladorFlecha ScriptFlecha; 
	[Export] public TextureProgressBar BarraVisual;
	[Export] public RigidBody2D PelotaA;
	[Export] public RigidBody2D PelotaB;
	
	[Export] public AnimatedSprite2D AnimPelotaA; 
	[Export] public AnimatedSprite2D AnimPelotaB; 
	
	[Export] public Line2D LineaA;
	[Export] public Line2D LineaB;
	[Export] public string RutaSiguienteNivel = ""; 

	// --- AUDIOS ---
	[Export] public AudioStreamPlayer AudioFlecha; 
	[Export] public AudioStreamPlayer AudioBarra; 
	[Export] public AudioStreamPlayer AudioDisparo; 
	[Export] public AudioStreamPlayer AudioColapso; 

	[ExportGroup("Sistema de Colapso")]
	// CAMBIO AQUÍ: Ahora es AnimatedSprite2D para que se mueva igual que las bolas
	[Export] public AnimatedSprite2D PuntoColapso; 
	[Export] public float VelocidadOscilacion = 15.0f;

	[ExportGroup("Sistema de Duelo")]
	[Export] public Sprite2D Objetivo;      
	[Export] public Label LabelInfo;
	[Export] public Sprite2D MarcaAzul;    
	[Export] public Sprite2D MarcaRojo;    
	[Export] public Sprite2D MetaAzul;     
	[Export] public Sprite2D MetaRojo;     

	private Vector2 _posMetaAzul;
	private Vector2 _posMetaRojo;
	private Vector2 _posFinalAzul;
	private Vector2 _posFinalRojo;

	[ExportGroup("Ajustes Físicos")]
	[Export] public float VelocidadCarga = 150.0f;
	[Export] public float MultiplicadorFuerza = 20.0f;

	private Vector2 _posicionSpawn = new Vector2(0, 0); 
	private enum Estado { Apuntando, CargandoFuerza, Lanzado, EsperandoColapso, ProcesandoColapso, FinDuelo }
	private Estado _estadoActual = Estado.Apuntando;
	
	private int _jugadorActual = 1; 
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
		MetaAzul.Visible = false;
		MetaRojo.Visible = false;

		ActualizarTextoTurno();

		foreach (Line2D linea in new[] { LineaA, LineaB })
		{
			linea.Visible = false;
			linea.TopLevel = true; 
			linea.ZIndex = 5;
			linea.ClearPoints(); 
			linea.AddPoint(Vector2.Zero); 
			linea.AddPoint(Vector2.Zero); 
		}

		// Frenamos animaciones al inicio
		if (AnimPelotaA != null) { AnimPelotaA.Stop(); AnimPelotaA.Frame = 0; }
		if (AnimPelotaB != null) { AnimPelotaB.Stop(); AnimPelotaB.Frame = 0; }
		if (PuntoColapso != null) PuntoColapso.Stop(); // Frenamos al fantasma también

		// Audios
		if (AudioFlecha != null) AudioFlecha.Play();
		if (AudioBarra != null) AudioBarra.Stop();
		if (AudioDisparo != null) AudioDisparo.Stop();
		if (AudioColapso != null) AudioColapso.Stop();
	}

	public override void _Process(double delta)
	{
		if (ScriptFlecha != null)
		{
			if (LineaA != null) LineaA.GlobalPosition = ScriptFlecha.GlobalPosition;
			if (LineaB != null) LineaB.GlobalPosition = ScriptFlecha.GlobalPosition;
		}

		if (Input.IsActionJustPressed("ui_accept") && _estadoActual != Estado.FinDuelo && _estadoActual != Estado.ProcesandoColapso)
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

		// Efecto Pitch Audio
		if (_estadoActual == Estado.Lanzado && AudioDisparo != null && AudioDisparo.Playing)
		{
			float velocidad = PelotaA.LinearVelocity.Length();
			float nuevoPitch = Math.Clamp(velocidad * 0.002f, 0.5f, 1.5f);
			AudioDisparo.PitchScale = Mathf.Lerp(AudioDisparo.PitchScale, nuevoPitch, (float)delta * 5.0f);
		}

		// Rotación Animaciones Bolas
		if (_estadoActual == Estado.Lanzado)
		{
			if (AnimPelotaA != null && PelotaA.LinearVelocity.Length() > 5.0f)
			{
				AnimPelotaA.GlobalRotation = PelotaA.LinearVelocity.Angle();
				AnimPelotaA.SpeedScale = PelotaA.LinearVelocity.Length() * 0.005f; 
			}
			if (AnimPelotaB != null && PelotaB.LinearVelocity.Length() > 5.0f)
			{
				AnimPelotaB.GlobalRotation = PelotaB.LinearVelocity.Angle();
				AnimPelotaB.SpeedScale = PelotaB.LinearVelocity.Length() * 0.005f;
			}
		}
	}

	private void AvanzarEstado()
	{
		if (_estadoActual == Estado.Apuntando)
		{
			if (AudioFlecha != null) AudioFlecha.Stop();
			if (AudioBarra != null) AudioBarra.Play();

			ScriptFlecha.Activo = false;
			_estadoActual = Estado.CargandoFuerza;
			BarraVisual.Visible = true;
			LineaA.Visible = true; LineaB.Visible = true;
		}
		else if (_estadoActual == Estado.CargandoFuerza)
		{
			if (AudioBarra != null) AudioBarra.Stop();

			_estadoActual = Estado.Lanzado;
			LineaA.Visible = false; LineaB.Visible = false;
			
			if (Objetivo is Objetivo scriptObjetivo) 
			{
				scriptObjetivo.DetenerMovimiento();
				if (_jugadorActual == 1) { _posMetaAzul = Objetivo.GlobalPosition; MetaAzul.GlobalPosition = _posMetaAzul; MetaAzul.Visible = true; }
				else { _posMetaRojo = Objetivo.GlobalPosition; MetaRojo.GlobalPosition = _posMetaRojo; MetaRojo.Visible = true; }
			}

			EjecutarLanzamientoCuantico();
		}
		else if (_estadoActual == Estado.EsperandoColapso)
		{
			EjecutarColapsoEnPunto();
		}
	}

	private void ActualizarDibujoProyeccion()
	{
		float fuerzaActual = (float)BarraVisual.Value;
		float dispersionRadianes = fuerzaActual * 0.008f; 
		float largoLinea = fuerzaActual * 2.5f;

		Vector2 baseDireccion = Vector2.Up.Rotated(ScriptFlecha.GlobalRotation);
		Vector2 dirA = baseDireccion.Rotated(-dispersionRadianes);
		Vector2 dirB = baseDireccion.Rotated(dispersionRadianes);

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

		if (AudioBarra != null && AudioBarra.Playing)
		{
			float porcentaje = (float)BarraVisual.Value / (float)BarraVisual.MaxValue;
			AudioBarra.PitchScale = 0.8f + (porcentaje * 0.7f);
		}
	}

	private void EjecutarLanzamientoCuantico()
	{
		float fuerza = (float)BarraVisual.Value * MultiplicadorFuerza;
		float dispersion = (float)BarraVisual.Value * 0.008f; 

		PelotaA.SleepingStateChanged += AlPararseLasPelotas;
		PelotaB.SleepingStateChanged += AlPararseLasPelotas;

		if (AudioDisparo != null) { AudioDisparo.PitchScale = 1.0f; AudioDisparo.Play(); }
		if (AnimPelotaA != null) AnimPelotaA.Play();
		if (AnimPelotaB != null) AnimPelotaB.Play();

		Vector2 baseDireccion = Vector2.Up.Rotated(ScriptFlecha.Rotation);
		PelotaA.ApplyImpulse(baseDireccion.Rotated(-dispersion) * fuerza);
		PelotaB.ApplyImpulse(baseDireccion.Rotated(dispersion) * (fuerza * 0.95f));

		BarraVisual.Visible = false;
		ScriptFlecha.Visible = false;
	}

	private void AlPararseLasPelotas()
	{
		if (PelotaA.Sleeping && PelotaB.Sleeping)
		{
			if (AudioDisparo != null) AudioDisparo.Stop();
			if (AudioColapso != null) AudioColapso.Play();
			
			// Paramos las bolas normales
			if (AnimPelotaA != null) AnimPelotaA.Stop();
			if (AnimPelotaB != null) AnimPelotaB.Stop();

			PelotaA.SleepingStateChanged -= AlPararseLasPelotas;
			PelotaB.SleepingStateChanged -= AlPararseLasPelotas;
			
			_estadoActual = Estado.EsperandoColapso;
			
			// ACTIVAMOS EL FANTASMA Y SU ANIMACIÓN
			PuntoColapso.Visible = true;
			PuntoColapso.Play(); // <--- IMPORTANTE: Que ruede la bola fantasma
			
			_tiempoOscilacion = 0;
		}
	}

	private void EjecutarColapsoEnPunto()
	{
		if (AudioColapso != null) AudioColapso.Stop();

		_estadoActual = Estado.ProcesandoColapso;
		Vector2 posicionFinal = PuntoColapso.GlobalPosition;
		PuntoColapso.Visible = false;

		if (_jugadorActual == 1)
		{
			_posFinalAzul = posicionFinal; MarcaAzul.GlobalPosition = _posFinalAzul; MarcaAzul.Visible = true; PelotaA.Visible = false; PelotaB.Visible = false;
			_jugadorActual = 2;
			GetTree().CreateTimer(2.0f).Timeout += ReiniciarParaSiguienteTurno;
		}
		else
		{
			_posFinalRojo = posicionFinal; MarcaRojo.GlobalPosition = _posFinalRojo; MarcaRojo.Visible = true; PelotaA.Visible = false; PelotaB.Visible = false;
			DeterminarGanador();
		}
	}

	private void DeterminarGanador()
	{
		_estadoActual = Estado.FinDuelo;
		float distAzul = _posFinalAzul.DistanceTo(_posMetaAzul);
		float distRojo = _posFinalRojo.DistanceTo(_posMetaRojo);
		string resultado = "";
		if (distAzul < distRojo) resultado = "¡GANADOR: AZUL!";
		else if (distRojo < distAzul) resultado = "¡GANADOR: ROJO!";
		else resultado = "¡EMPATE!";

		if (LabelInfo != null) LabelInfo.Text = resultado;

		GetTree().CreateTimer(3.0f).Timeout += () => {
			if (!string.IsNullOrEmpty(RutaSiguienteNivel)) GetTree().ChangeSceneToFile(RutaSiguienteNivel);
			else {
				_jugadorActual = 1; MarcaAzul.Visible = false; MarcaRojo.Visible = false; MetaAzul.Visible = false; MetaRojo.Visible = false;
				ReiniciarParaSiguienteTurno();
			}
		};
	}

	private void ProcesarMovimientoPunto(float delta)
	{
		_tiempoOscilacion += delta * VelocidadOscilacion;
		float t = (Mathf.Sin(_tiempoOscilacion) + 1.0f) / 2.0f;
		PuntoColapso.GlobalPosition = PelotaA.GlobalPosition.Lerp(PelotaB.GlobalPosition, t);
	}

	private void ReiniciarParaSiguienteTurno()
	{
		_estadoActual = Estado.Apuntando;
		ActualizarTextoTurno();
		ScriptFlecha.Reiniciar();
		ScriptFlecha.Visible = true;
		BarraVisual.Value = BarraVisual.MinValue;
		if (Objetivo is Objetivo scriptObjetivo) scriptObjetivo.ReiniciarMovimiento();
		ResetearPelotaFisica(PelotaA);
		ResetearPelotaFisica(PelotaB);

		// REINICIAR TODO
		if (AudioFlecha != null) AudioFlecha.Play();
		if (AudioBarra != null) AudioBarra.Stop();
		if (AudioDisparo != null) AudioDisparo.Stop();
		if (AudioColapso != null) AudioColapso.Stop();

		if (AnimPelotaA != null) { AnimPelotaA.Stop(); AnimPelotaA.Frame = 0; }
		if (AnimPelotaB != null) { AnimPelotaB.Stop(); AnimPelotaB.Frame = 0; }
		if (PuntoColapso != null) PuntoColapso.Stop();
	}

	private void ActualizarTextoTurno()
	{
		if (LabelInfo != null) LabelInfo.Text = _jugadorActual == 1 ? "TURNO: AZUL" : "TURNO: ROJO";
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
