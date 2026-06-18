using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dinamita : Arma
{
    [Header("Configuración Visual (Ajustes de Prefab)")]
    [SerializeField] private float elevacionExplosion = 0.5f;
    [SerializeField] private GameObject prefabEfectoExplosion;

    private GameObject soldadoDueno;
    private movimientoJugador scriptMovimientoDueno;
    private Vida vidaDueno;
    private bool haDetonado = false;
    private DTerrain.ClickAndDestroyOptimized destructorTerrain;

    void Start()
    {
        if (transform.parent != null)
        {
            soldadoDueno = transform.parent.gameObject;
            scriptMovimientoDueno = soldadoDueno.GetComponent<movimientoJugador>();
            vidaDueno = soldadoDueno.GetComponent<Vida>();
        }

        destructorTerrain = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();
        transform.SetParent(null);
        if (scriptMovimientoDueno != null)
        {
            scriptMovimientoDueno.atacando = false;
        }
        StartCoroutine(RutinaContadorDinamita());
    }

    public override void Usar()
    {
        base.Usar();
    }

    private IEnumerator RutinaContadorDinamita()
    {
        float tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < tiempoExplosion)
        {
            if (TurnoManager.Instancia != null)
            {
                TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
            }

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

 
        if (scriptMovimientoDueno != null)
        {
            scriptMovimientoDueno.atacando = true;
        }

        if (!haDetonado)
        {
            DetonarDinamita();
        }
    }

    private void DetonarDinamita()
    {
        haDetonado = true;
        Vector3 puntoImpacto = transform.position;

        // 1. Destrucción del terreno en DTerrain
        if (destructorTerrain != null)
        {
            destructorTerrain.CambiarTamaño(radioExplosion);
            destructorTerrain.EjecutarDestruccion(puntoImpacto);
        }

        // Visual del fogonazo
        if (prefabEfectoExplosion != null)
        {
            Instantiate(prefabEfectoExplosion, puntoImpacto, Quaternion.identity);
        }

        List<Vida> vidasProcesadas = new List<Vida>();
        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(puntoImpacto, radioDanio);

        foreach (Collider2D col in objetosDetectados)
        {
            Vida vidaObjetivo = col.GetComponent<Vida>();
            Rigidbody2D rbGusano = col.GetComponent<Rigidbody2D>();
            // --- BUSCAMOS EL SCRIPT DE ANIMACIÓN DEL ENEMIGO ---
            ControlAnimador animadorEnemigo = col.GetComponentInChildren<ControlAnimador>();
            // ---------------------------------------------------
            float distancia = Vector2.Distance(puntoImpacto, col.transform.position);
            float factorCercania = (radioDanio - distancia) / radioDanio;
            factorCercania = Mathf.Clamp01(factorCercania);

            if (vidaObjetivo != null && factorCercania > 0)
            {
                if (vidasProcesadas.Contains(vidaObjetivo)) continue;
                vidasProcesadas.Add(vidaObjetivo);

                float danioFinal = danioMaximo * factorCercania;
                if (danioFinal > 0)
                {
                    vidaObjetivo.RecibirDanio(danioFinal);
                    // --- TRIGER DE DOLOR ---
                    if (animadorEnemigo != null)
                    {
                        animadorEnemigo.EjecutarDanio();
                    }
                }
            }

            // 2. Empuje Seco (Fuerza bruta del ScriptableObject sin degradación por distancia)
            if (rbGusano != null && factorCercania > 0)
            {
                rbGusano.linearVelocity = Vector2.zero;

                Vector2 direccionEmpuje = (Vector2)col.transform.position - (Vector2)puntoImpacto;
                direccionEmpuje.Normalize();
                direccionEmpuje.y += elevacionExplosion;
                direccionEmpuje.Normalize();

                rbGusano.AddForce(direccionEmpuje * fuerzaEmpuje, ForceMode2D.Impulse);
                // --- TRIGER DE VUELO (EXPLOSION AIRE) ---
                if (animadorEnemigo != null && fuerzaEmpuje > 0.5f)
                {
                    animadorEnemigo.ModificarEstadoEmpuje(true);
                }
            }
        }

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayExplosionAleatoria();
        }

        // 3. Pasamos el turno limpiamente
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