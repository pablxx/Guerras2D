using System.Collections;
using UnityEngine;

public class ControlAnimador : MonoBehaviour
{
    [Header("Referencias Directas (Arrastrar aquí)")]
    [SerializeField] private ControladorPies sensoresPies;

    [Header("Sistema de Aburrimiento (Worms)")]
    [SerializeField] private float tiempoMinimoAburrimiento = 4f;
    [SerializeField] private float tiempoMaximoAburrimiento = 8f;

    private Animator animator;
    private Rigidbody2D rbPadre;
    private movimientoJugador scriptMovimiento;

    // Variables internas para el temporizador
    private float temporizadorAburrimiento = 0f;
    private float limiteActualAburrimiento = 5f;

    // --- NUEVO: SEGURO PARA ARMAS ESPECIALES ---
    public bool bloqueoPorArmaEspecial = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (transform.parent != null)
        {
            rbPadre = transform.parent.GetComponent<Rigidbody2D>();
            scriptMovimiento = transform.parent.GetComponent<movimientoJugador>();
        }
        else
        {
            scriptMovimiento = GetComponent<movimientoJugador>();
        }

        if (sensoresPies == null)
        {
            Debug.LogWarning("[ControlAnimador] ¡Falta arrastrar el ControladorPies al Inspector, Harold!");
        }

        limiteActualAburrimiento = Random.Range(tiempoMinimoAburrimiento, tiempoMaximoAburrimiento);
    }

    void Update()
    {
        if (animator != null)
        {
            // =============================================================
            // 1. Control básico del suelo y física
            // =============================================================
            if (sensoresPies != null)
            {
                bool estaEnAire = !sensoresPies.tocandoSuelo;
                animator.SetBool("EnAire", estaEnAire);

                if (sensoresPies.tocandoSuelo)
                {
                    animator.SetBool("FueEmpujado", false);
                    animator.SetBool("Saltando", false);
                }
                else
                {
                    if (rbPadre != null && rbPadre.linearVelocity.y < -0.1f)
                    {
                        animator.SetBool("FueEmpujado", false);
                    }
                }
            }

            // =============================================================
            // 2. SISTEMA DE ANIMACIONES RANDOM (INACTIVIDAD)
            // =============================================================
            bool estaEnAireActual = !sensoresPies.tocandoSuelo;
            bool estaMoviendose = rbPadre != null && Mathf.Abs(rbPadre.linearVelocity.x) > 0.1f;
            bool estaAtacando = scriptMovimiento != null && (scriptMovimiento.animacionAtacando || scriptMovimiento.animacionCargando);
            bool estaOcupado = animator.GetBool("Saltando") || animator.GetBool("EstaQuemando") || bloqueoPorArmaEspecial;
            bool tieneArma = scriptMovimiento != null && scriptMovimiento.animacionArmaEquipada;

            if (!estaEnAireActual && !estaMoviendose && !estaAtacando && !estaOcupado && !tieneArma)
            {
                temporizadorAburrimiento += Time.deltaTime;

                if (temporizadorAburrimiento >= limiteActualAburrimiento)
                {
                    int animacionElegida = Random.Range(1, 7);
                    animator.SetInteger("IndiceQuieto", animacionElegida);
                    animator.SetTrigger("HacerQuietoRandom");

                    temporizadorAburrimiento = 0f;
                    limiteActualAburrimiento = Random.Range(tiempoMinimoAburrimiento, tiempoMaximoAburrimiento);
                }
            }
            else
            {
                temporizadorAburrimiento = 0f;
                if (animator.GetInteger("IndiceQuieto") != 0)
                {
                    InterrumpirAburrimiento();
                }
            }

            // =============================================================
            // 3. Control de armas y ESTADO FINAL DEL TURNO
            // =============================================================
            if (scriptMovimiento != null)
            {
                if (scriptMovimiento.enabled)
                {
                    // Si el turno está activo y el jugador tiene control:
                    animator.SetBool("ArmaEquipada", scriptMovimiento.animacionArmaEquipada);
                    animator.SetBool("CargandoObj", scriptMovimiento.animacionCargando);
                    animator.SetBool("Lanzado", scriptMovimiento.animacionAtacando);

                    int tipoActual = scriptMovimiento.animacionTipoArma;
                    if (tipoActual != -1)
                    {
                        animator.SetInteger("TipoArma", tipoActual);
                        animator.SetInteger("IDAnimacion", scriptMovimiento.animacionID);
                    }
                }
                else
                {
                    // --- ¡EL PARCHE MÁGICO CONTRA LA CAMINATA INFINITA! ---
                    // Si TurnoManager apagó el control, forzamos al gusanito a quedarse quieto
                    animator.SetFloat("VelocidadX", 0f);
                    animator.SetBool("ArmaEquipada", false);
                    animator.SetBool("CargandoObj", false);
                    animator.SetBool("Lanzado", false);
                    // ------------------------------------------------------
                }
            }
        }
    }

    public void InterrumpirAburrimiento()
    {
        if (animator != null)
        {
            animator.SetInteger("IndiceQuieto", 0);
            animator.SetTrigger("InterrumpirAburrimiento");
        }
    }

    public void EstablecerBloqueoArmaEspecial(bool estado)
    {
        bloqueoPorArmaEspecial = estado;
        if (estado)
        {
            InterrumpirAburrimiento();
        }
    }

    public void ActualizarCaminata(float velocidadX)
    {
        // Solo actualiza la caminata si el script de movimiento está encendido
        if (animator != null && scriptMovimiento != null && scriptMovimiento.enabled)
        {
            animator.SetFloat("VelocidadX", Mathf.Abs(velocidadX));
        }
    }

    public void EjecutarImpulsoSalto()
    {
        if (animator != null)
        {
            StartCoroutine(RutinaDisparoSalto());
        }
    }

    private IEnumerator RutinaDisparoSalto()
    {
        InterrumpirAburrimiento();
        animator.SetBool("FueEmpujado", false);
        animator.SetBool("Saltando", true);
        yield return null;
        animator.SetBool("Saltando", false);
    }

    public void EjecutarMuerte()
    {
        if (animator != null)
        {
            animator.SetBool("Morir", true);
        }
    }

    public void EjecutarFestejo()
    {
        if (animator != null)
        {
            animator.SetBool("Festejando", true);
        }
    }

    public void EjecutarDanio()
    {
        if (animator != null)
        {
            InterrumpirAburrimiento();
            animator.SetTrigger("Golpeado");
        }
    }

    public void ModificarEstadoEmpuje(bool estado)
    {
        if (animator != null)
        {
            animator.SetBool("FueEmpujado", estado);
        }
    }

    public void ModificarEstadoQuemaduras(bool estado)
    {
        if (animator != null)
        {
            InterrumpirAburrimiento();
            animator.SetBool("EstaQuemando", estado);
        }
    }
}