using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KamikazeEmbestidaDTerrain : Arma
{
    [Header("Configuración del Avance (Ajustes de Prefab)")]
    [SerializeField] private float velocidadEmbestida = 15f;
    [SerializeField] private float duracionMaximaAvance = 1.2f;
    [SerializeField] private float frecuenciaDestruccion = 0.05f;
    [SerializeField] private float elevacionExplosion = 0.5f;
    [SerializeField] private GameObject prefabEfectoExplosion;

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
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioExplosion / 10f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDanio);
    }
}