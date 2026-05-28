using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.InputSystem; 

public class OvejaVoladora : Arma
{
    public enum EstadoOveja
    {
        Saltando,
        Volando
    }

    [Header("Configuración de Estados y Velocidad")]
    [SerializeField] private EstadoOveja estadoActual = EstadoOveja.Saltando;
    [SerializeField] private float velocidadVuelo = 5f;
    [SerializeField] private float velocidadGiro = 100f;
    [SerializeField] private float velocidadSalto = 3f;

    private Rigidbody2D rb;
    private PlayerInput miPlayerInput;
    private bool yaExplotó = false;
    private float direccionMirada = 1f;
    private bool listaParaCaminar = false;

    private float valorGiroEntrada = 0f;



    private void Awake()
    {
        miPlayerInput = GetComponent<PlayerInput>();
        if (miPlayerInput != null)
        {
            miPlayerInput.enabled = false;
        }
    }

    public void InicializarHerenciaDeInput(PlayerInput inputGusanito, float direccionLanzamiento)
    {
        direccionMirada = direccionLanzamiento;
        if (miPlayerInput != null && inputGusanito != null)
        {
            miPlayerInput.actions = inputGusanito.actions;
            miPlayerInput.SwitchCurrentControlScheme(inputGusanito.currentControlScheme, inputGusanito.devices.ToArray());
        }
        if (direccionMirada < 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        if (miPlayerInput != null && Object.FindFirstObjectByType<camaraMovimiento>() != null)
        {
            Object.FindFirstObjectByType<camaraMovimiento>().ActualizarReferenciaInput(miPlayerInput);
            miPlayerInput.enabled = true; 
        }
    }
    private IEnumerator Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        yield return new WaitForFixedUpdate();

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        listaParaCaminar = true;
    }

    public override void Usar()
    {

    }

    public void OnAtacar(InputValue value)
    {
        if (yaExplotó || !listaParaCaminar) return;

        if (value.isPressed)
        {
            if (estadoActual == EstadoOveja.Saltando)
            {
                ActivarModoVuelo();
            }
            else if (estadoActual == EstadoOveja.Volando)
            {
                StopAllCoroutines();
                CrearDanio(transform.position);
            }
        }
    }
    public void OnMover(InputValue value)
    {
        if (yaExplotó || estadoActual != EstadoOveja.Volando) return;
        Vector2 inputVector = value.Get<Vector2>();
        valorGiroEntrada = inputVector.y * direccionMirada;
    }

    void Update()
    {
        if (!listaParaCaminar || yaExplotó || estadoActual != EstadoOveja.Volando) return;
        transform.Rotate(0, 0, valorGiroEntrada * velocidadGiro * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (!listaParaCaminar || yaExplotó || rb == null) return;

        if (estadoActual == EstadoOveja.Volando)
        {
            rb.linearVelocity = transform.right * velocidadVuelo;
        }
        else if (estadoActual == EstadoOveja.Saltando)
        {
            AvanzarSaltando();
        }
    }

    void AvanzarSaltando()
    {
        rb.linearVelocity = new Vector2(direccionMirada * velocidadSalto, rb.linearVelocity.y);
    }

    void ActivarModoVuelo()
    {
        Debug.Log("[Oveja] ¡Despegue completado! Modo Planeador Activo.");
        estadoActual = EstadoOveja.Volando;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.None;
        }
    }
    public void CrearDanio(Vector3 puntoDeImpacto)
    {
        if (yaExplotó == true) return;
        yaExplotó = true;
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
        }
        if (miPlayerInput != null) miPlayerInput.enabled = false;

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

            float distancia = Vector2.Distance(puntoDeImpacto, col.transform.position);
            float factorCercania = (radioDanio - distancia) / radioDanio;
            factorCercania = Mathf.Clamp01(factorCercania);

            if (vidaObjetivo != null && factorCercania > 0)
            {
                if (vidasProcesadasEnEsteImpacto.Contains(vidaObjetivo)) continue;
                vidasProcesadasEnEsteImpacto.Add(vidaObjetivo);

                float danioFinal = danioMaximo * factorCercania;
                if (danioFinal > 0) vidaObjetivo.RecibirDanio(danioFinal);
            }
            if (rbGusano != null && factorCercania > 0)
            {
                Vector2 direccionEmpuje = col.transform.position - puntoDeImpacto;
                direccionEmpuje.Normalize();
                float fuerzaFinal = fuerzaEmpuje * factorCercania;
                rbGusano.AddForce(direccionEmpuje * fuerzaFinal, ForceMode2D.Impulse);
            }
        }
        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        }
        Destroy(gameObject, 0.1f);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (estadoActual == EstadoOveja.Volando && !yaExplotó)
        {
            Vector3 puntoImpacto = collision.contacts[0].point;
            CrearDanio(puntoImpacto);
        }
    }

    private void OnDestroy()
    {
        yaExplotó = true;
    }
}