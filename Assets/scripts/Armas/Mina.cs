using System.Collections;
using UnityEngine;

public class Mina : Arma
{
    [Header("Configuración de la Mina")]
    [SerializeField] private float radioProximidadDeteccion = 1.2f;
    [SerializeField] private float tiempoRetrasoExplosion = 1.5f;
    [SerializeField] private LayerMask capaEnemigos;

    [Header("Visuales de Alerta")]
    [SerializeField] private SpriteRenderer spriteMina;
    [SerializeField] private Color colorAlerta = Color.red;

    private Rigidbody2D rb;
    private bool estaArmada = false;
    private bool yaSeActivo = false;
    

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.parent = null;
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.MostrarNotificacion("Mina desplegada! Esperando que se active...");
        }
        StartCoroutine(RutinaArmarMina());
    }

    private IEnumerator RutinaArmarMina()
    {
        yield return new WaitForSeconds(0.5f);
        if (rb != null)
        {
            while (rb.linearVelocity.magnitude > 0.05f)
            {
                yield return new WaitForSeconds(0.1f);
            }

            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (TurnoManager.Instancia != null && TurnoManager.Instancia.soldadoActivoEnEsteTurno != null)
        {
            GameObject gusanitoActivo = TurnoManager.Instancia.soldadoActivoEnEsteTurno;
            var mov = gusanitoActivo.GetComponent<movimientoJugador>();
            if (mov != null) mov.atacando = false;
            camaraMovimiento scriptCam = Camera.main.GetComponent<camaraMovimiento>();
            if (scriptCam != null)
            {
                scriptCam.EnfocarObjetivo(gusanitoActivo.transform);

                var inputJugador = gusanitoActivo.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (inputJugador != null)
                {
                    scriptCam.ActualizarReferenciaInput(inputJugador);
                }
            }
        }
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.MostrarNotificacion("MINA PLANTADA Tienes 2 segundos para escapar!");
        }
        estaArmada = false;
        yield return new WaitForSeconds(2.0f);
        estaArmada = true;
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.MostrarNotificacion("🔒 Mina armada y activa en el área.");
            if (TurnoManager.Instancia.soldadoActivoEnEsteTurno != null)
            {
                var mov = TurnoManager.Instancia.soldadoActivoEnEsteTurno.GetComponent<movimientoJugador>();
                if (mov != null) mov.atacando = true;
            }
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        }
    }

    private void Update()
    {
        if (!estaArmada || yaSeActivo) return;
        Collider2D soldadoCerca = Physics2D.OverlapCircle(transform.position, radioProximidadDeteccion, capaEnemigos);
        if (soldadoCerca != null)
        {
            StartCoroutine(RutinaSecuenciaDetonacion(soldadoCerca.gameObject));
        }
    }

    private IEnumerator RutinaSecuenciaDetonacion(GameObject victima)
    {
        yaSeActivo = true;

        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.MostrarNotificacion("🚨 ¡PROXIMIDAD DETECTADA! La mina va a estallar...");
        }
        float tiempoTranscurrido = 0f;
        Color colorOriginal = spriteMina != null ? spriteMina.color : Color.white;

        while (tiempoTranscurrido < tiempoRetrasoExplosion)
        {
            if (spriteMina != null) spriteMina.color = colorAlerta;
            yield return new WaitForSeconds(0.15f);
            if (spriteMina != null) spriteMina.color = colorOriginal;
            yield return new WaitForSeconds(0.15f);
            tiempoTranscurrido += 0.3f;
        }

        EjecutarExplosionFinal(victima);
    }

    private void EjecutarExplosionFinal(GameObject victima)
    {
        Vector3 puntoImpacto = transform.position;
        var destructor = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();
        if (destructor != null)
        {
            destructor.CambiarTamaño(radioExplosion);
            destructor.EjecutarDestruccion(puntoImpacto);
        }
        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayExplosionAleatoria();
        }
        if (victima != null)
        {
            Vida vidaObjetivo = victima.GetComponent<Vida>();
            if (vidaObjetivo != null)
            {
                vidaObjetivo.RecibirDanio(danioMaximo);
            }

            Rigidbody2D rbVictima = victima.GetComponent<Rigidbody2D>();
            if (rbVictima != null)
            {
                Vector2 direccionEmpuje = (victima.transform.position - transform.position).normalized;
                direccionEmpuje.y += 0.3f;
                rbVictima.linearVelocity = Vector2.zero;
                rbVictima.AddForce(direccionEmpuje * fuerzaEmpuje, ForceMode2D.Impulse);
            }
        }
        if (spriteMina != null) spriteMina.enabled = false;    
        Destroy(gameObject, 0.1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (yaSeActivo) return;
        BalaProyectil bala = collision.GetComponent<BalaProyectil>();
        if (bala != null)
        {
            Debug.Log($"[Mina] Detonación remota provocada por: {collision.gameObject.name}");
            yaSeActivo = true;
            Collider2D personajeEncima = Physics2D.OverlapCircle(transform.position, radioProximidadDeteccion, capaEnemigos);
            GameObject objetivo = personajeEncima != null ? personajeEncima.gameObject : null;

            StopAllCoroutines();
            EjecutarExplosionFinal(objetivo);
        }
    }

    public override void Usar()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioProximidadDeteccion);
    }
}