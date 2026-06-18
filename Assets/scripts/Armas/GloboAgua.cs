using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GloboAgua : Arma
{
    [Header("Ajustes de Rotación Visual")]
    [SerializeField] private float velocidadRotacionAire = 600f;

    [Header("Ajustes de Efectos Visuales")]
    [SerializeField] private GameObject prefabParticulasExplosion;

    [Header("Ajuste Especial de Empuje Hidráulico")]
    [SerializeField] private float multiplicadorFuerzaAgua = 2.5f;

    private bool yaExplotó = false;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (yaExplotó == true) return;

        Vector3 puntoImpacto = collision.contacts[0].point;
        CrearDanio(puntoImpacto);
    }

    public override void Usar()
    {
        base.Usar();
        if (rb != null)
        {
            float direccionGiro = -1f;

            if (TurnoManager.Instancia != null && TurnoManager.Instancia.soldadoActivoEnEsteTurno != null)
            {
                if (TurnoManager.Instancia.soldadoActivoEnEsteTurno.transform.localScale.x < 0)
                {
                    direccionGiro = 1f;
                }
                else
                {
                    direccionGiro = -1f;
                }
            }

            rb.angularVelocity = velocidadRotacionAire * direccionGiro;
        }
    }

    public void CrearDanio(Vector3 puntoDeImpacto)
    {
        if (yaExplotó == true) return;
        yaExplotó = true;

        if (prefabParticulasExplosion != null)
        {
            Instantiate(prefabParticulasExplosion, puntoDeImpacto, Quaternion.identity);
        }

        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
        }
        var destructor = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();
        if (destructor != null)
        {
            destructor.CambiarTamaño(radioExplosion);
            destructor.EjecutarDestruccion(puntoDeImpacto);
        }

        List<Vida> vidasProcesadasEnEsteImpacto = new List<Vida>();
        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(puntoDeImpacto, radioDanio);
        foreach (Collider2D col in objetosDetectados)
        {
            Vida vidaObjetivo = col.GetComponent<Vida>();
            Rigidbody2D rbGusano = col.GetComponent<Rigidbody2D>();
            // ---  ANIMADOR DEL GUSANO AFECTADO ---
            ControlAnimador animadorEnemigo = col.GetComponentInChildren<ControlAnimador>();
            // ------------------------------------------------
            float distancia = Vector2.Distance(puntoDeImpacto, col.transform.position);
            float factorCercania = (radioDanio - distancia) / radioDanio;
            factorCercania = Mathf.Clamp01(factorCercania);

            if (vidaObjetivo != null && factorCercania > 0)
            {
                if (vidasProcesadasEnEsteImpacto.Contains(vidaObjetivo)) continue;
                vidasProcesadasEnEsteImpacto.Add(vidaObjetivo);
                float danioFinal = danioMaximo * factorCercania;
                if (danioFinal > 0)
                {
                    vidaObjetivo.RecibirDanio(danioFinal);
                    // --- AQUÍ LLAMAMOS A LA ANIMACIÓN DE DOLOR ---
                    if (animadorEnemigo != null)
                    {
                        animadorEnemigo.EjecutarDanio();
                    }
                    // ---------------------------------------------
                }
            }
            if (rbGusano != null && factorCercania > 0)
            {
                Vector2 direccionEmpuje = (Vector2)col.transform.position - (Vector2)puntoDeImpacto;
                direccionEmpuje.Normalize();

                float fuerzaFinal = fuerzaEmpuje * factorCercania * multiplicadorFuerzaAgua;
                rbGusano.AddForce(direccionEmpuje * fuerzaFinal, ForceMode2D.Impulse);
            }
        }
        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        Rigidbody2D rbGranada = GetComponent<Rigidbody2D>();
        if (rbGranada != null)
        {
            rbGranada.angularVelocity = 0f;
            rbGranada.bodyType = RigidbodyType2D.Static;
        }
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        }
        AudioManager.Instancia.PlaySFXPorIndice(Random.Range(2, 4));
        Destroy(gameObject, 0.1f);
    }

    private void OnDestroy()
    {
        yaExplotó = true;
    }
}