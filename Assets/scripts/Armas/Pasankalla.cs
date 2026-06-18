using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pasankalla : Arma
{
    [Header("Configuración del Proyectil Autónomo")]
    [SerializeField] private float velocidadBala = 25f;
    [SerializeField] private GameObject prefabParticulas;
    [SerializeField] private float velocidadRotacionVisual = 720f; // Grados por segundo al girar en el aire

    [Header("Retroceso del Jugador")]
    [SerializeField] private float fuerzaRetrocesoJugador = 40f;

    bool Explotar = true;
    bool mostrarRadioImpacto = false;
    private bool volando = false;
    private float sentidoGiro = -1f;

    private void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;

            Vector3 trajectoryDirection = transform.right;

            if (TurnoManager.Instancia != null && TurnoManager.Instancia.soldadoActivoEnEsteTurno != null)
            {
                GameObject soldadoActivo = TurnoManager.Instancia.soldadoActivoEnEsteTurno;

                if (soldadoActivo.transform.localScale.x < 0)
                {
                    trajectoryDirection = -trajectoryDirection;
                    sentidoGiro = 1f; // Invierte el giro visual si dispara a la izquierda
                }
                else
                {
                    sentidoGiro = -1f;
                }

                Rigidbody2D rbSoldado = soldadoActivo.GetComponent<Rigidbody2D>();
                if (rbSoldado != null)
                {
                    rbSoldado.linearVelocity = Vector2.zero;
                    Vector2 strictKnockbackDirection = -(Vector2)trajectoryDirection.normalized;
                    rbSoldado.AddForce(strictKnockbackDirection * fuerzaRetrocesoJugador, ForceMode2D.Impulse);
                }
            }

            rb.linearVelocity = trajectoryDirection.normalized * velocidadBala;
            volando = true; // Activamos el switch para que empiece a rotar en el Update
        }
    }

    private void Update()
    {
        // Inyección quirúrgica: Si el proyectil está viajando, rota su transform en el eje Z
        if (volando)
        {
            transform.Rotate(0f, 0f, velocidadRotacionVisual * sentidoGiro * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Explotar == true)
        {
            volando = false; // Apagamos el giro visual al impactar
            mostrarRadioImpacto = true;
            Explotar = false;

            if (TurnoManager.Instancia != null)
            {
                TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
            }

            Vector3 puntoImpacto = collision.contacts[0].point;
            transform.position = puntoImpacto;

            // Aseguramos que la rotación se congele limpiamente al estallar
            transform.rotation = Quaternion.identity;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }

            Collider2D miColisionador = GetComponent<Collider2D>();
            if (miColisionador != null) miColisionador.enabled = false;

            if (prefabParticulas != null)
            {
                Instantiate(prefabParticulas, puntoImpacto, Quaternion.identity);
            }

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
                    if (animadorEnemigo != null && fuerzaFinal > 0.5f)
                    {
                        animadorEnemigo.ModificarEstadoEmpuje(true);
                    }
                }
            }

            if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
            if (AudioManager.Instancia != null) AudioManager.Instancia.PlayExplosionAleatoria();

            if (TurnoManager.Instancia != null)
            {
                TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(gameObject));
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

    public override void Usar()
    {
        base.Usar();
        Explotar = true;
    }
}