using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MisilAereo : MonoBehaviour
{
    [SerializeField] private float velocidadCaida = 15f;
    [SerializeField] private GameObject prefabParticulas; 

    private Rigidbody2D rb;
    private SecuenciadorAereo miSecuenciador;
    private bool yaExplotó = false;
    private bool soyElUltimo = false;

    private void Start()
    {
        AudioManager.Instancia.PlaySFXPorIndice(9);
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = new Vector2(0f, -velocidadCaida);
        }
    }

    public void ConfigurarMisil(SecuenciadorAereo spawnerPadre, bool esElUltimoMisil)
    {
        miSecuenciador = spawnerPadre;
        soyElUltimo = esElUltimoMisil;
    }

    private void Update()
    {
        if (!yaExplotó && rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.linearVelocity = new Vector2(0f, -velocidadCaida);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (yaExplotó || miSecuenciador == null) return;
        yaExplotó = true;

        Vector3 puntoImpacto = collision.contacts[0].point;
        transform.position = puntoImpacto;

        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        // Inyección quirúrgica: Instanciar los efectos visuales de la explosión
        if (prefabParticulas != null)
        {
            Instantiate(prefabParticulas, puntoImpacto, Quaternion.identity);
        }

        int radioExplosionPadre = miSecuenciador.ObtenerRadioExplosion();
        float radioDanioPadre = miSecuenciador.ObtenerRadioDanio();
        int danioMaximoPadre = miSecuenciador.ObtenerDanioMaximo();
        float fuerzaEmpujePadre = miSecuenciador.ObtenerFuerzaEmpuje();
        var destructor = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();
        if (destructor != null)
        {
            destructor.CambiarTamaño(radioExplosionPadre);
            destructor.EjecutarDestruccion(puntoImpacto);
        }
        List<Vida> vidasProcesadas = new List<Vida>();
        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(puntoImpacto, radioDanioPadre);

        foreach (Collider2D col in objetosDetectados)
        {
            Vida vidaObjetivo = col.GetComponent<Vida>();
            Rigidbody2D rbGusano = col.GetComponent<Rigidbody2D>();
            ControlAnimador animadorEnemigo = col.GetComponentInChildren<ControlAnimador>();

            float distancia = Vector2.Distance(puntoImpacto, col.transform.position);
            float factorCercania = (radioDanioPadre - distancia) / radioDanioPadre;
            factorCercania = Mathf.Clamp01(factorCercania);

            if (vidaObjetivo != null && factorCercania > 0)
            {
                if (vidasProcesadas.Contains(vidaObjetivo)) continue;
                vidasProcesadas.Add(vidaObjetivo);

                float danioFinal = danioMaximoPadre * factorCercania;
                if (danioFinal > 0)
                {
                    vidaObjetivo.RecibirDanio(danioFinal);

                    if (animadorEnemigo != null)
                    {
                        animadorEnemigo.EjecutarDanio();
                    }
                }
            }

            if (rbGusano != null && factorCercania > 0)
            {
                Vector2 direccionEmpuje = (Vector2)col.transform.position - (Vector2)puntoImpacto;
                direccionEmpuje.Normalize();
                float fuerzaFinal = fuerzaEmpujePadre * factorCercania;
                rbGusano.AddForce(direccionEmpuje * fuerzaFinal, ForceMode2D.Impulse);
            }
        }
        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
        if (soyElUltimo && TurnoManager.Instancia != null)
        {
            Debug.Log("[Ataque Aéreo] Último misil detonado. Limpiando secuenciador y cambiando turno.");
            Destroy(miSecuenciador.gameObject);
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        }
        AudioManager.Instancia.PlayExplosionAleatoria();
        Destroy(gameObject, 0.1f);
    }
}