using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kamikaze : Arma
{
    [Header("Configuración del Avance (Ajustes de Prefab)")]
    [SerializeField] private float velocidadEmbestida = 15f;
    [SerializeField] private float duracionMaximaAvance = 1.2f;
    [SerializeField] private float frecuenciaDestruccion = 0.05f;
    [SerializeField] private float elevacionExplosion = 0.5f;
    [SerializeField] private GameObject prefabEfectoExplosion;

    [Header("Ajustes de Empuje en Carrera")]
    [SerializeField] private float fuerzaEmpujeEnCarrera = 8f;
    [SerializeField] private float offsetRadioCarrera = 0.5f; // Offset para ajustar el alcance del Overlap en carrera

    private GameObject soldadoDueno;
    private Rigidbody2D rbDueno;
    private movimientoJugador scriptMovimientoDueno;
    private Vida vidaDueno;
    private float direccionX = 1f;
    private bool haDetonado = false;
    private DTerrain.ClickAndDestroyOptimized destructorTerrain;

    void Start()
    {
        if (transform.parent != null)
        {
            soldadoDueno = transform.parent.gameObject;
            rbDueno = soldadoDueno.GetComponent<Rigidbody2D>();
            scriptMovimientoDueno = soldadoDueno.GetComponent<movimientoJugador>();
            vidaDueno = soldadoDueno.GetComponent<Vida>();

            float diferenciaX = transform.position.x - transform.parent.position.x;
            direccionX = diferenciaX >= 0f ? 1f : -1f;
        }
        else
        {
            direccionX = 1f;
        }

        destructorTerrain = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();

        transform.SetParent(null);

        StartCoroutine(RutinaKamikazeExcavador());
    }

    public override void Usar()
    {
        base.Usar();
    }

    private IEnumerator RutinaKamikazeExcavador()
    {
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
        }

        if (scriptMovimientoDueno != null)
        {
            scriptMovimientoDueno.atacando = true;
            scriptMovimientoDueno.enabled = false;
        }

        float tiempoTranscurrido = 0f;
        float temporizadorDestruccion = 0f;

        while (tiempoTranscurrido < duracionMaximaAvance && !haDetonado)
        {
            if (TurnoManager.Instancia != null)
            {
                TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
            }

            if (rbDueno != null && soldadoDueno != null)
            {
                rbDueno.linearVelocity = new Vector2(direccionX * velocidadEmbestida, 0f);
                transform.position = soldadoDueno.transform.position;

                temporizadorDestruccion += Time.deltaTime;
                if (temporizadorDestruccion >= frecuenciaDestruccion)
                {
                    MorderTerrenoEnAvance(transform.position, radioExplosion);
                    EmpujarEnemigosEnElCamino(transform.position);
                    temporizadorDestruccion = 0f;
                }
            }

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        if (rbDueno != null)
        {
            rbDueno.linearVelocity = Vector2.zero;
        }

        if (!haDetonado)
        {
            ExplosionFinalKamikaze();
        }
    }

    private void MorderTerrenoEnAvance(Vector3 punto, int radio)
    {
        if (destructorTerrain != null)
        {
            destructorTerrain.CambiarTamaño(radio);
            destructorTerrain.EjecutarDestruccion(punto);
        }
    }

    private void EmpujarEnemigosEnElCamino(Vector3 punto)
    {
        // Se calcula el radio base y se le inyecta quirúrgicamente el offset de alcance
        float radioDeteccion = (radioExplosion / 10f) + offsetRadioCarrera;
        Collider2D[] detectados = Physics2D.OverlapCircleAll(punto, radioDeteccion);

        foreach (Collider2D col in detectados)
        {
            if (col.gameObject == soldadoDueno) continue;

            Rigidbody2D rbEnemigo = col.GetComponent<Rigidbody2D>();
            if (rbEnemigo != null)
            {
                rbEnemigo.linearVelocity = Vector2.zero;
                Vector2 dirEmpuje = new Vector2(direccionX, 0.7f).normalized;
                rbEnemigo.AddForce(dirEmpuje * fuerzaEmpujeEnCarrera, ForceMode2D.Impulse);

                movimientoJugador movEnemigo = col.GetComponent<movimientoJugador>();
                if (movEnemigo != null)
                {
                    movEnemigo.enabled = true;
                }
            }
        }
    }

    private void ExplosionFinalKamikaze()
    {
        haDetonado = true;
        Vector3 puntoImpacto = transform.position;

        int radioFinalMapa = Mathf.RoundToInt(radioExplosion * 1.5f);
        MorderTerrenoEnAvance(puntoImpacto, radioFinalMapa);

        if (prefabEfectoExplosion != null)
        {
            Instantiate(prefabEfectoExplosion, puntoImpacto, Quaternion.identity);
        }

        List<Vida> vidasProcesadasEnEsteImpacto = new List<Vida>();
        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(puntoImpacto, radioDanio);

        foreach (Collider2D col in objetosDetectados)
        {
            Vida vidaObjetivo = col.GetComponent<Vida>();
            Rigidbody2D rbGusano = col.GetComponent<Rigidbody2D>();

            float distancia = Vector2.Distance(puntoImpacto, col.transform.position);
            float factorCercania = (radioDanio - distancia) / radioDanio;
            factorCercania = Mathf.Clamp01(factorCercania);

            if (vidaObjetivo != null && factorCercania > 0)
            {
                if (col.gameObject == soldadoDueno || vidasProcesadasEnEsteImpacto.Contains(vidaObjetivo)) continue;

                vidasProcesadasEnEsteImpacto.Add(vidaObjetivo);

                float danioFinal = danioMaximo * factorCercania;
                if (danioFinal > 0)
                {
                    vidaObjetivo.RecibirDanio(danioFinal);
                }
            }
            if (rbGusano != null && col.gameObject != soldadoDueno && factorCercania > 0)
            {
                rbGusano.linearVelocity = Vector2.zero;
                Vector2 direccionEmpujeFinal = new Vector2(direccionX, elevacionExplosion).normalized;
                rbGusano.AddForce(direccionEmpujeFinal * fuerzaEmpuje, ForceMode2D.Impulse);

                movimientoJugador movEnemigo = col.GetComponent<movimientoJugador>();
                if (movEnemigo != null)
                {
                    movEnemigo.enabled = true;
                }
            }
        }

        if (vidaDueno != null)
        {
            vidaDueno.RecibirDanio(9999f);
        }
        else if (soldadoDueno != null)
        {
            Destroy(soldadoDueno);
        }

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayExplosionAleatoria();
        }

        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        }

        Destroy(gameObject, 0.05f);
    }

    private void OnDrawGizmos()
    {
       
        float radioDeteccionVisual = (radioExplosion / 10f) + offsetRadioCarrera;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioDeteccionVisual);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDanio);
    }
}