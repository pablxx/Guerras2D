using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentoFuego : MonoBehaviour
{
    [Header("Configuración de Movimiento Manual")]
    [SerializeField] private float velocidadCaidaAire = 15f;
    [SerializeField] private float velocidadExcavacionSuelo = 0.1f;
    [SerializeField] private float frecuenciaDestruccion = 0.05f;
    [SerializeField] private float elevacionImpacto = 0.3f;
    [SerializeField] private GameObject prefabEfectoPolvo;
    [SerializeField] private LayerMask capaTerreno;

    private SecuenciadorMolotov miSecuenciador;
    private bool yaExplotó = false;
    private bool soyElUltimo = false;
    private float tiempoVidaMaximo;
    private bool tocandoSuelo = false;

    private Vector3 direccionActual;

    private int danioCalculado;
    private int radioExplosionPixeles;
    private float radioDanioGusanos;
    private float fuerzaEmpujeBruta;

    private DTerrain.ClickAndDestroyOptimized destructorTerrain;

    public void InicializarFragmento(SecuenciadorMolotov spawnerPadre, bool esElUltimoMisil, float tiempoMecha)
    {
        miSecuenciador = spawnerPadre;
        soyElUltimo = esElUltimoMisil;
        tiempoVidaMaximo = tiempoMecha;

        if (miSecuenciador != null)
        {
            danioCalculado = Mathf.RoundToInt(miSecuenciador.ObtenerDanioMaximo() * 0.35f);
            radioExplosionPixeles = Mathf.Max(6, Mathf.RoundToInt(miSecuenciador.ObtenerRadioExplosion() * 0.4f));
            radioDanioGusanos = miSecuenciador.ObtenerRadioDanio() * 0.6f;
            fuerzaEmpujeBruta = miSecuenciador.ObtenerFuerzaEmpuje() * 0.6f;
        }

        destructorTerrain = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();

        direccionActual = Vector3.down;

        StartCoroutine(RutinaCicloDeVidaFuego());
    }

    private IEnumerator RutinaCicloDeVidaFuego()
    {
        while (!tocandoSuelo)
        {
            if (TurnoManager.Instancia != null)
            {
                TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
            }

            transform.position += direccionActual * (velocidadCaidaAire * Time.deltaTime);

            Collider2D golpeTerreno = Physics2D.OverlapCircle(transform.position, 0.25f, capaTerreno);
            if (golpeTerreno != null)
            {
                tocandoSuelo = true;
            }

            yield return null;
        }
        yaExplotó = true;

        Vector2 desvioAleatorio = Random.insideUnitCircle.normalized;
        direccionActual = new Vector3(desvioAleatorio.x, -1f, 0f).normalized;

        float tiempoTranscurrido = 0f;
        float temporizadorDestruccion = 0f;
        float temporizadorDanioSeco = 0f;

        while (tiempoTranscurrido < tiempoVidaMaximo)
        {
            if (TurnoManager.Instancia != null)
            {
                TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
            }

            transform.position += direccionActual * (velocidadExcavacionSuelo * Time.deltaTime);
            temporizadorDestruccion += Time.deltaTime;
            if (temporizadorDestruccion >= frecuenciaDestruccion)
            {
                if (destructorTerrain != null)
                {
                    destructorTerrain.CambiarTamaño(radioExplosionPixeles);
                    destructorTerrain.EjecutarDestruccion(transform.position);
                }

                if (prefabEfectoPolvo != null)
                {
                    Instantiate(prefabEfectoPolvo, transform.position, Quaternion.identity);
                }

                temporizadorDestruccion = 0f;
            }
            temporizadorDanioSeco += Time.deltaTime;
            if (temporizadorDanioSeco >= 1f)
            {
                AplicarPulsoDanioSeco(transform.position);
                temporizadorDanioSeco = 0f;
            }
            AplicarEmpujeContinuo(transform.position);

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }
        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;

        if (soyElUltimo && TurnoManager.Instancia != null && miSecuenciador != null)
        {
            Debug.Log("[Lluvia Fuego] Último mini-taladro caótico terminado. Cambiando turno.");
            Destroy(miSecuenciador.gameObject);
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        }

        Destroy(gameObject, 0.1f);
    }

    private void AplicarPulsoDanioSeco(Vector3 centro)
    {
        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(centro, radioDanioGusanos);

        foreach (Collider2D col in objetosDetectados)
        {
            Vida vidaObjetivo = col.GetComponent<Vida>();

            if (vidaObjetivo != null)
            {               
                float danioFinal = miSecuenciador != null ? miSecuenciador.ObtenerDanioMaximo() : danioCalculado;

                if (danioFinal > 0)
                {
                    vidaObjetivo.RecibirDanio(danioFinal);
                }
            }
        }
    }

    private void AplicarEmpujeContinuo(Vector3 centro)
    {
        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(centro, radioDanioGusanos);

        foreach (Collider2D col in objetosDetectados)
        {
            Rigidbody2D rbGusano = col.GetComponent<Rigidbody2D>();

            if (rbGusano != null)
            {
                float distancia = Vector2.Distance(centro, col.transform.position);
                float factorCercania = (radioDanioGusanos - distancia) / radioDanioGusanos;
                factorCercania = Mathf.Clamp01(factorCercania);

                if (factorCercania > 0)
                {
                    rbGusano.linearVelocity = Vector2.zero;
                    Vector2 direccionEmpuje = (Vector2)col.transform.position - (Vector2)centro;
                    direccionEmpuje.Normalize();
                    direccionEmpuje.y += elevacionImpacto;
                    direccionEmpuje.Normalize();

                    float fuerzaFinal = fuerzaEmpujeBruta * factorCercania;
                    rbGusano.AddForce(direccionEmpuje * fuerzaFinal, ForceMode2D.Impulse);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioExplosionPixeles / 10f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDanioGusanos);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, direccionActual * 1.5f);
    }
}