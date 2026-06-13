using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Taladro : Arma
{
    [Header("Configuración del Avance (Perforación Vertical)")]
    [SerializeField] private float velocidadEmbestida = 10f;
    [SerializeField] private float frecuenciaDestruccion = 0.04f;
    [SerializeField] private float offsetInicialAbajo = 0.5f;
    [SerializeField] private GameObject prefabEfectoPolvo;

    private GameObject soldadoDueno;
    private Rigidbody2D rbDueno;
    private movimientoJugador scriptMovimientoDueno;
    private bool ejecucionTerminada = false;
    private DTerrain.ClickAndDestroyOptimized destructorTerrain;
    private List<Vida> enemigosImpactadosEnEsteViaje = new List<Vida>();

    void Start()
    {
        if (transform.parent != null)
        {
            soldadoDueno = transform.parent.gameObject;
            rbDueno = soldadoDueno.GetComponent<Rigidbody2D>();
            scriptMovimientoDueno = soldadoDueno.GetComponent<movimientoJugador>();
        }

        destructorTerrain = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();

        transform.SetParent(null);

        if (soldadoDueno != null)
        {
            transform.position = soldadoDueno.transform.position + (Vector3.down * offsetInicialAbajo);
        }

        StartCoroutine(RutinaTaladroExcavador());
    }

    public override void Usar()
    {
        base.Usar();
    }

    private IEnumerator RutinaTaladroExcavador()
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
        while (tiempoTranscurrido < tiempoExplosion && !ejecucionTerminada)
        {
            if (TurnoManager.Instancia != null)
            {
                TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
            }

            if (rbDueno != null && soldadoDueno != null)
            {
                rbDueno.linearVelocity = new Vector2(0f, -velocidadEmbestida);
                transform.position = soldadoDueno.transform.position + (Vector3.down * offsetInicialAbajo);

                temporizadorDestruccion += Time.deltaTime;
                if (temporizadorDestruccion >= frecuenciaDestruccion)
                {
                    MorderTerreno(transform.position, radioExplosion);

                    if (prefabEfectoPolvo != null)
                    {
                        Instantiate(prefabEfectoPolvo, transform.position, Quaternion.identity);
                    }

                    temporizadorDestruccion = 0f;
                }

                DetectarYTaclearEnemigos();
            }

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        FinalizarExcavacion();
    }

    private void MorderTerreno(Vector3 punto, int radio)
    {
        if (destructorTerrain != null)
        {
            destructorTerrain.CambiarTamaño(radio);
            destructorTerrain.EjecutarDestruccion(punto);
        }
    }

    private void DetectarYTaclearEnemigos()
    {
        Vector3 puntoImpacto = transform.position;
        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(puntoImpacto, radioDanio);

        foreach (Collider2D col in objetosDetectados)
        {
            if (col.gameObject == soldadoDueno) continue;

            Vida vidaObjetivo = col.GetComponent<Vida>();
            Rigidbody2D rbGusano = col.GetComponent<Rigidbody2D>();

            if (vidaObjetivo != null && !enemigosImpactadosEnEsteViaje.Contains(vidaObjetivo))
            {
                enemigosImpactadosEnEsteViaje.Add(vidaObjetivo);
                vidaObjetivo.RecibirDanio(danioMaximo);
            }

            if (rbGusano != null && col.gameObject != soldadoDueno)
            {
                rbGusano.linearVelocity = Vector2.zero;
                Vector2 direccionEmpujeFinal = (col.transform.position - transform.position).normalized;
                rbGusano.AddForce(direccionEmpujeFinal * fuerzaEmpuje, ForceMode2D.Impulse);
            }
        }
    }

    private void FinalizarExcavacion()
    {
        ejecucionTerminada = true;

        if (rbDueno != null)
        {
            rbDueno.linearVelocity = Vector2.zero;
        }

        MorderTerreno(transform.position, radioExplosion);

        if (scriptMovimientoDueno != null)
        {
            scriptMovimientoDueno.atacando = false;
        }

        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioExplosion / 10f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDanio);
    }
}