using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AncianaSuicida : Arma
{
    [SerializeField] private float velocidadCaminata = 3f;

    [Header("Ajustes de Navegación Inteligente")]
    [SerializeField] private float distanciaDeteccionPared = 0.4f;
    [SerializeField] private LayerMask capaTerreno;
    [SerializeField] private Vector2 offsetRayoFrontal = new Vector2(0f, 0.2f); 

    // Variables Internas
    private float direccionX = 1f;
    private Rigidbody2D rb;
    private bool yaExplotó = false;
    private bool listaParaCaminar = false;
    private IEnumerator Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.gravityScale = 1f;
        yield return new WaitForFixedUpdate();
        if (rb != null)
        {
            if (rb.linearVelocity.x < -0.01f)
            {
                direccionX = -1f;
            }
            else if (rb.linearVelocity.x > 0.01f)
            {
                direccionX = 1f;
            }
            else
            {
                direccionX = Mathf.Sign(transform.right.x);
            }
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        AjustarSprite();
        if (capaTerreno.value == 0)
        {
            capaTerreno = LayerMask.GetMask("Default"); 
        }
        listaParaCaminar = true;
    }

    public override void Usar()
    {
        if (usaTemporizador)
        {
            StartCoroutine(TemporizadorExplosion());
        }
    }
    private void FixedUpdate()
    {
        if (!listaParaCaminar || yaExplotó || rb == null) return;
        VerificarObstaculoFrontal();
        rb.linearVelocity = new Vector2(direccionX * velocidadCaminata, rb.linearVelocity.y);
    }

    private void VerificarObstaculoFrontal()
    {
        Vector3 origenRayo = transform.position + new Vector3(offsetRayoFrontal.x * direccionX, offsetRayoFrontal.y, 0f);
        Vector2 direccionRayo = new Vector2(direccionX, 0f);
        RaycastHit2D hit = Physics2D.Raycast(origenRayo, direccionRayo, distanciaDeteccionPared, capaTerreno);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            if (hit.collider.GetComponent<Vida>() == null)
            {
                Debug.Log($"[Anciana Inteligente] Obstáculo detectado en {hit.collider.name}. Girando bando.");
                direccionX *= -1f;
                AjustarSprite();
            }
        }
    }

    private void AjustarSprite()
    {
        Vector3 escala = transform.localScale;
        escala.x = Mathf.Abs(escala.x) * direccionX;
        transform.localScale = escala;
    }

    private IEnumerator TemporizadorExplosion()
    {
        yield return new WaitForSeconds(tiempoExplosion);
        CrearDanio(transform.position);
    }

    private void CrearDanio(Vector3 puntoDeImpacto)
    {
        if (yaExplotó == true) return;
        yaExplotó = true;

        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
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
            // --- BUSCAMOS EL SCRIPT DE ANIMACIÓN DEL ENEMIGO ---
            ControlAnimador animadorEnemigo = col.GetComponentInChildren<ControlAnimador>();
            //---------------------
            float distancia = Vector2.Distance(puntoDeImpacto, col.transform.position);
            float factorCercania = (radioDanio - distancia) / radioDanio;
            factorCercania = Mathf.Clamp01(factorCercania);

            if (vidaObjetivo != null && factorCercania > 0)
            {
                if (vidasProcesadasEnEsteImpacto.Contains(vidaObjetivo)) continue;
                vidasProcesadasEnEsteImpacto.Add(vidaObjetivo);

                float danioFinal = danioMaximo * factorCercania;
                //if (danioFinal > 0) vidaObjetivo.RecibirDanio(danioFinal);
                if (danioFinal > 0)
                {
                    vidaObjetivo.RecibirDanio(danioFinal);

                    // --- TRIGER DE DOLOR (GOLPEADO) ---
                    if (animadorEnemigo != null)
                    {
                        animadorEnemigo.EjecutarDanio();
                    }
                }
            }

            if (rbGusano != null && factorCercania > 0)
            {
                Vector2 direccionEmpuje = col.transform.position - puntoDeImpacto;
                direccionEmpuje.Normalize();
                float fuerzaFinal = fuerzaEmpuje * factorCercania;
                rbGusano.AddForce(direccionEmpuje * fuerzaFinal, ForceMode2D.Impulse);
                // --- TRIGER DE VUELO (EXPLOSION AIRE) ---
                if (animadorEnemigo != null && fuerzaFinal > 0.5f)
                {
                    animadorEnemigo.ModificarEstadoEmpuje(true);
                }
            }
        }

        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        }
        AudioManager.Instancia.PlayExplosionAleatoria();
        Destroy(gameObject, 0.1f);
    }
    private void OnDrawGizmos()
    {
        if (yaExplotó) return;
        Vector3 origenRayo = transform.position + new Vector3(offsetRayoFrontal.x * direccionX, offsetRayoFrontal.y, 0f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(origenRayo, new Vector2(direccionX, 0f) * distanciaDeteccionPared);
    }
}