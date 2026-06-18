using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Silla : Arma
{
    [Header("Ajustes de la Silla")]
    [SerializeField] private float velocidadRotacionAire = 500f;

    bool Explotar = true;
    bool mostrarRadioImpacto = false;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void Usar()
    {
        base.Usar();

        if (rb != null)
        {
            float direccionGiro = 1f;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Explotar == true)
        {
           
            mostrarRadioImpacto = true;
            Explotar = false;

            if (TurnoManager.Instancia != null)
            {
                TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
            }

            Vector3 puntoImpacto = collision.contacts[0].point;
            transform.position = puntoImpacto;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Static;
            }

            Collider2D miColisionador = GetComponent<Collider2D>();
            if (miColisionador != null) miColisionador.enabled = false;

            var destructor = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();
            if (destructor != null)
            {
                int tamanoDestruccion = radioExplosion;
                destructor.CambiarTamaño(tamanoDestruccion);
                destructor.EjecutarDestruccion(puntoImpacto);
            }

            List<Vida> vidasProcesadasEnEsteImpacto = new List<Vida>();

            Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(puntoImpacto, radioDanio);
            foreach (Collider2D col in objetosDetectados)
            {
                Vida vidaObjetivo = col.GetComponent<Vida>();
                Rigidbody2D rbGusano = col.GetComponent<Rigidbody2D>();
                // --- BUSCAMOS EL ANIMADOR DEL GUSANO ---
                ControlAnimador animadorEnemigo = col.GetComponentInChildren<ControlAnimador>();
                float distancia = Vector2.Distance(puntoImpacto, col.transform.position);
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
                        // --- TRIGER DE DOLOR ---
                        if (animadorEnemigo != null)
                        {
                            animadorEnemigo.EjecutarDanio();
                        }
                    }
                }

                if (rbGusano != null && factorCercania > 0)
                {
                    Vector2 direccionEmpuje = col.transform.position - puntoImpacto;
                    direccionEmpuje.Normalize();
                    float fuerzaFinal = fuerzaEmpuje * factorCercania;
                    rbGusano.AddForce(direccionEmpuje * fuerzaFinal, ForceMode2D.Impulse);
                    // --- TRIGER DE VUELO (EXPLOSIÓN AIRE) ---
                    if (animadorEnemigo != null && fuerzaFinal > 0.5f)
                    {
                        animadorEnemigo.ModificarEstadoEmpuje(true);
                    }
                }
            }

            if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;

            if (TurnoManager.Instancia != null)
            {
                TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(gameObject));
            }

            if (AudioManager.Instancia != null)
            {
                AudioManager.Instancia.PlaySFXPorIndice(5);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDanio);
    }

    private void OnDrawGizmos()
    {
        if (mostrarRadioImpacto)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawSphere(transform.position, radioDanio);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radioDanio);
        }
    }
}