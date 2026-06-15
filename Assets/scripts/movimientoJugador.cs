using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class movimientoJugador : MonoBehaviour
{
    [SerializeField] float velocidad;
    [SerializeField] float fuerzaSalto;
    [SerializeField] float alturaEscalon = 0.2f;
    [SerializeField] GameObject Granada;
    [SerializeField] Transform anclaPunto;
    [SerializeField] Transform puntoDisparo;
    [SerializeField] float fuerzaMinima = 3f;
    [SerializeField] float fuerzaMaxima = 25f;
    [SerializeField] float velocidadCarga = 15f;
    [SerializeField] public Slider barraFuerzaUI;
    [SerializeField] GameObject prefabMiraMouse;

    [Header("Configuración de Saltos Especiales")]
    [SerializeField] private float multiplicadorSaltoAlto = 1.9f;
    [SerializeField] private float potenciaBackflipX = 3f;
    [SerializeField] private float potenciaBackflipY = 1.5f;
    [SerializeField] private float tiempoDobleToque = 0.2f;

    PlayerInput input;
    InputAction mover;
    InputAction salto;
    InputAction atacar;
    InputAction inventario;
    InputAction ajustarAngulo;
    InputAction opciones;
    Rigidbody2D rb2d;
    Vector2 movimiento;
    float anguloActual = 0f;
    float velocidadRotacion = 50f;
    int sentido = 1; // 1 = Derecha, 0 = Izquierda
    float fuerzaActual = 0f;
    bool cargandoDisparo = false;
    int direccionCarga = 1; // 1 = Subiendo, -1 = Bajando
    public bool atacando = false;

    CapsuleCollider2D col2d;
    private GameObject instanciaMiraMouse;
    private ControlAnimador MiAnimador;

    private float ultimoToqueEspacio = 0f;
    private bool puedeSaltar = true;
    private bool saltoAltoUsado = false;
    private bool saltoNormalReciente = false;
    private bool backflipUsado = false;
    private bool haciendoBackflip = false;

    void Start()
    {
        input = GetComponent<PlayerInput>();
        rb2d = GetComponent<Rigidbody2D>();
        col2d = GetComponent<CapsuleCollider2D>();
        MiAnimador = GetComponent<ControlAnimador>();

        mover = input.actions.FindAction("MoverPersonaje");
        salto = input.actions.FindAction("Jump");
        atacar = input.actions.FindAction("Atacar");
        inventario = input.actions.FindAction("Inventario");
        ajustarAngulo = input.actions.FindAction("AjustarAngulo");
        opciones = input.actions.FindAction("Opciones");

        AlternarMira(true);
        barraFuerzaUI.gameObject.SetActive(false);
        atacar.canceled += AlSoltarBotonDisparo;
    }

    bool EstaEnSuelo()
    {
        return Mathf.Abs(rb2d.linearVelocity.y) < 0.05f;
    }

    void Update()
    {
        if (atacando)
        {
            movimiento = Vector2.zero;
            DestruirMiraMouse();
            return;
        }

        movimiento = mover.ReadValue<Vector2>();
        MiAnimador.ActualizarCaminata(rb2d.linearVelocity.x);

        if (EstaEnSuelo() && !haciendoBackflip)
        {
            puedeSaltar = true;
            saltoAltoUsado = false;
            backflipUsado = false;
        }

        if (salto.WasPressedThisFrame())
        {
            saltar();
        }

        if (inventario.WasPressedThisFrame())
        {
            PanelInventario.Instancia.PermisoAtacar = false;
            if (PanelInventario.Instancia.InventarioActivo == false)
            {
                PanelInventario.Instancia.InventarioActivo = true;
            }
            else if (PanelInventario.Instancia.InventarioActivo == true)
            {
                PanelInventario.Instancia.InventarioActivo = false;
            }
        }
        if (opciones.WasPressedThisFrame())
        {

            {

            }
        }

        Apuntar();
        ManejarInputsPorTipoDeArma();
        ActualizarPosicionYEstadoMiraMouse();
    }

    void ActualizarPosicionYEstadoMiraMouse()
    {
        if (PanelInventario.Instancia == null || PanelInventario.Instancia.InventarioActivo || !PanelInventario.Instancia.PermisoAtacar || atacando)
        {
            DestruirMiraMouse();
            return;
        }
        DatosArmas datos = PanelInventario.Instancia.ArmaEquipadaActiva;

        if (MiAnimador != null)
        {

        }

        if (instanciaMiraMouse != null)
        {
            Vector3 posicionMouse = Mouse.current.position.ReadValue();
            Vector3 posicionMundo = Camera.main.ScreenToWorldPoint(posicionMouse);
            posicionMundo.z = 0f;
            instanciaMiraMouse.transform.position = posicionMundo;

            if (datos == null) return;
        }
        if (datos == null || datos.prefabArma == null)
        {
            return;
        }
        if (datos.tipo == DatosArmas.TipoArma.Aereo || datos.tipo == DatosArmas.TipoArma.Invocable)
        {
            AlternarMira(false);

            if (instanciaMiraMouse == null && prefabMiraMouse != null)
            {
                Vector3 posicionMouse = Mouse.current.position.ReadValue();
                Vector3 posicionMundo = Camera.main.ScreenToWorldPoint(posicionMouse);
                posicionMundo.z = 0f;

                instanciaMiraMouse = Instantiate(prefabMiraMouse, posicionMundo, Quaternion.identity);
            }
        }
        else
        {
            AlternarMira(true);
            DestruirMiraMouse();
        }
    }
    void DestruirMiraMouse()
    {
        if (instanciaMiraMouse != null)
        {
            Destroy(instanciaMiraMouse);
            instanciaMiraMouse = null;
        }
    }
    private void FixedUpdate()
    {
        if (atacando)
        {
            return;
        }

        if (!haciendoBackflip)
        {
            rb2d.linearVelocity = new Vector2(movimiento.x * velocidad, rb2d.linearVelocity.y);
        }

        if (Mathf.Abs(movimiento.x) > 0.01f)
        {
            SubirEscalon();
            VerificarVolteo(movimiento.x);
        }
    }
    void Apuntar()
    {
        if (atacando) return;
        Vector2 inputVector = ajustarAngulo.ReadValue<Vector2>();
        float valorInput = inputVector.y;
        anguloActual += valorInput * velocidadRotacion * Time.deltaTime;
        anguloActual = Mathf.Clamp(anguloActual, -45f, 90f);
        anclaPunto.localRotation = Quaternion.Euler(0, 0, anguloActual);
    }

    void ManejarInputsPorTipoDeArma()
    {
        if (PanelInventario.Instancia == null || PanelInventario.Instancia.PermisoAtacar == false || atacando) return;

        DatosArmas datos = PanelInventario.Instancia.ArmaEquipadaActiva;
        if (datos == null || datos.prefabArma == null) return;

        switch (datos.tipo)
        {
            case DatosArmas.TipoArma.Proyectil:
            case DatosArmas.TipoArma.Arrojable:
                ManejarCargaDisparo();
                break;

            case DatosArmas.TipoArma.ArmaFuego:
                if (atacar.WasPressedThisFrame())
                {
                    EjecutarAtaqueInstantaneo(false);
                }
                break;

            case DatosArmas.TipoArma.CaC:
                if (atacar.WasPressedThisFrame())
                {
                    EjecutarAtaqueInstantaneo(true);
                }
                break;

            case DatosArmas.TipoArma.Aereo:
            case DatosArmas.TipoArma.Invocable:
                if (atacar.WasPressedThisFrame())
                {
                    EjecutarAtaqueClicMapa();
                }
                break;
        }
    }

    void ManejarCargaDisparo()
    {
        if (atacar.IsPressed())
        {
            if (!cargandoDisparo)
            {
                cargandoDisparo = true;
                fuerzaActual = fuerzaMinima;
                direccionCarga = 1;

                if (barraFuerzaUI != null)
                {
                    barraFuerzaUI.gameObject.SetActive(true);
                }
            }
            fuerzaActual += velocidadCarga * direccionCarga * Time.deltaTime;
            if (fuerzaActual >= fuerzaMaxima)
            {
                fuerzaActual = fuerzaMaxima;
                direccionCarga = -1;
            }
            else if (fuerzaActual <= fuerzaMinima)
            {
                fuerzaActual = fuerzaMinima;
                direccionCarga = 1;
            }
            if (barraFuerzaUI != null)
            {
                barraFuerzaUI.value = fuerzaActual;
            }
        }
    }

    private void AlSoltarBotonDisparo(InputAction.CallbackContext context)
    {
        if (cargandoDisparo)
        {
            PanelInventario.Instancia.PermisoAtacar = false;
            cargandoDisparo = false;
            if (barraFuerzaUI != null)
            {
                barraFuerzaUI.gameObject.SetActive(false);
            }
            ArmaVisualJugador visualArma = gameObject.GetComponentInChildren<ArmaVisualJugador>();
            visualArma.LimpiarArma();
            Atacar(fuerzaActual);
        }
    }

    void EjecutarAtaqueInstantaneo(bool esMelee)
    {
        PanelInventario.Instancia.PermisoAtacar = false;
        atacando = true;

        DatosArmas armaSeleccionada = PanelInventario.Instancia.ArmaEquipadaActiva;
        PanelInventario.Instancia.DescontarArma(armaSeleccionada);
        ArmaVisualJugador visualArma = gameObject.GetComponentInChildren<ArmaVisualJugador>();
        visualArma.LimpiarArma();
        GameObject nuevaArma = Instantiate(armaSeleccionada.prefabArma, puntoDisparo.position, puntoDisparo.rotation);

        if (esMelee) nuevaArma.transform.SetParent(transform);

        Arma scriptArma = nuevaArma.GetComponent<Arma>();
        if (scriptArma != null)
        {
            scriptArma.Inicializar(armaSeleccionada);

            Pistola scriptBala = nuevaArma.GetComponent<Pistola>();
            if (scriptBala != null)
            {
                Vector3 dir = puntoDisparo.right.normalized;
                if (transform.localScale.x < 0) dir = -dir;

                scriptBala.ConfigurarDisparoInicial(puntoDisparo.position, puntoDisparo.rotation, dir);
            }

            scriptArma.Usar();
        }

        AlternarMira(false);
        camaraMovimiento scriptCam = Camera.main.GetComponent<camaraMovimiento>();
        if (scriptCam != null) scriptCam.EnfocarObjetivo(nuevaArma.transform);
    }

    void EjecutarAtaqueClicMapa()
    {
        Vector3 posicionMouse = Mouse.current.position.ReadValue();
        Vector3 puntoMundo = Camera.main.ScreenToWorldPoint(posicionMouse);
        puntoMundo.z = 0f;

        PanelInventario.Instancia.PermisoAtacar = false;
        atacando = true;
        ArmaVisualJugador visualArma = gameObject.GetComponentInChildren<ArmaVisualJugador>();
        visualArma.LimpiarArma();
        DestruirMiraMouse();
        DatosArmas armaSeleccionada = PanelInventario.Instancia.ArmaEquipadaActiva;
        if (armaSeleccionada != null)
        {
            PanelInventario.Instancia.DescontarArma(armaSeleccionada);
            GameObject nuevaArma = Instantiate(armaSeleccionada.prefabArma, puntoMundo, Quaternion.identity);
            Arma scriptArma = nuevaArma.GetComponent<Arma>();
            if (scriptArma != null)
            {
                scriptArma.Inicializar(armaSeleccionada);
            }
            camaraMovimiento scriptCam = Camera.main.GetComponent<camaraMovimiento>();
            if (scriptCam != null) scriptCam.EnfocarObjetivo(nuevaArma.transform);
        }
    }

    void Atacar(float fuerzaLanzamiento)
    {
        ArmaVisualJugador visualArma =  gameObject.GetComponentInChildren<ArmaVisualJugador>();
        visualArma.LimpiarArma();
        atacando = true;
        Debug.Log(gameObject.name + " está atacando con fuerza de: " + fuerzaLanzamiento);

        if (TurnoManager.Instancia != null)
        {
            DatosArmas armaSeleccionada = PanelInventario.Instancia.ArmaEquipadaActiva;
            PanelInventario.Instancia.DescontarArma(armaSeleccionada);
            GameObject nuevaArma = Instantiate(armaSeleccionada.prefabArma, puntoDisparo.position, puntoDisparo.rotation);
            Arma scriptArma = nuevaArma.GetComponent<Arma>();
            if (scriptArma != null)
            {
                scriptArma.Inicializar(armaSeleccionada);
                scriptArma.Usar();
            }
            Pistola scriptBala = nuevaArma.GetComponent<Pistola>();
            if (scriptBala != null)
            {
                Vector3 dir = puntoDisparo.right.normalized;
                if (transform.localScale.x < 0) dir = -dir;
                scriptBala.ConfigurarDisparoInicial(puntoDisparo.position, puntoDisparo.rotation, dir);
            }
            // Proyectiles con física física física
            Rigidbody2D rbArma = nuevaArma.GetComponent<Rigidbody2D>();
            if (rbArma != null)
            {
                Vector2 direccion = puntoDisparo.right;
                if (sentido == 0)
                {
                    direccion = -direccion;
                }
                rbArma.AddForce(direccion * fuerzaLanzamiento, ForceMode2D.Impulse);
            }
            AlternarMira(false);
            camaraMovimiento scriptCam = Camera.main.GetComponent<camaraMovimiento>();
            if (scriptCam != null) scriptCam.EnfocarObjetivo(nuevaArma.transform);
        }
    }

    public void AlternarMira(bool activar)
    {
        if (anclaPunto != null)
        {
            anclaPunto.gameObject.SetActive(activar);
        }
    }
    void VerificarVolteo(float direccionX)
    {
        if (direccionX < -0.1f && sentido == 1)
        {
            sentido = 0;
            InvertirEscalaPersonaje();
        }
        else if (direccionX > 0.1f && sentido == 0)
        {
            sentido = 1;
            InvertirEscalaPersonaje();
        }
    }

    private void InvertirEscalaPersonaje()
    {
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
        RectTransform canvasHijo = GetComponentInChildren<RectTransform>();
        Vector3 escalaCanvas = canvasHijo.localScale;
        escalaCanvas.x = Mathf.Abs(escalaCanvas.x) * Mathf.Sign(transform.localScale.x);
        canvasHijo.localScale = escalaCanvas;
    }
    void SubirEscalon()
    {
        float direccionX = Mathf.Sign(movimiento.x);
        Vector2 origenPies = new Vector2(
            transform.position.x + (direccionX * col2d.bounds.extents.x),
            transform.position.y - col2d.bounds.extents.y + 0.05f
        );
        RaycastHit2D hitBajo = Physics2D.Raycast(origenPies, Vector2.right * direccionX, 0.1f);
        if (hitBajo.collider != null)
        {
            RaycastHit2D hitAlto = Physics2D.Raycast(origenPies + Vector2.up * alturaEscalon, Vector2.right * direccionX, 0.15f);
            if (hitAlto.collider == null)
            {
                transform.position += new Vector3(0, 0.05f, 0);
            }
        }
    }

    void saltar()
    {
        if (atacando) return;

        //  CONDICIÓN BACKFLIP
        if (puedeSaltar && Keyboard.current.leftShiftKey.isPressed && !backflipUsado)
        {
            AudioManager.Instancia.PlaySonidoSalto(0);
            float direccionAtras = (transform.localScale.x > 0) ? -1f : 1f;
            haciendoBackflip = true;

            rb2d.linearVelocity = new Vector2(direccionAtras * potenciaBackflipX, fuerzaSalto * potenciaBackflipY);
            StartCoroutine(HacerBackflip());

            if (MiAnimador != null) MiAnimador.EjecutarImpulsoSalto();

            backflipUsado = true;
            puedeSaltar = false;
            return;
        }

        //  CONDICIÓN SALTO ALTO (COMBO)
        if (!saltoAltoUsado && saltoNormalReciente)
        {
            AudioManager.Instancia.PlaySonidoSalto(1);
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, fuerzaSalto * multiplicadorSaltoAlto);

            if (MiAnimador != null) MiAnimador.EjecutarImpulsoSalto();

            saltoAltoUsado = true;
            saltoNormalReciente = false;
            return;
        }

        //  CONDICIÓN SALTO NORMAL
        if (puedeSaltar)
        {
            AudioManager.Instancia.PlaySonidoSalto(2);
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, fuerzaSalto);

            if (MiAnimador != null) MiAnimador.EjecutarImpulsoSalto();

            puedeSaltar = false;
            ultimoToqueEspacio = Time.time;
            saltoNormalReciente = true;

            StartCoroutine(DesactivarSaltoNormalReciente());
        }
    }

    IEnumerator HacerBackflip()
    {
        float velocidadGiro = 420f;
        float rotacionAcumulada = 0f;

        while (rotacionAcumulada < 360f)
        {
            float giroFrame = velocidadGiro * Time.deltaTime;
            if (rotacionAcumulada + giroFrame > 360f)
            {
                giroFrame = 360f - rotacionAcumulada;
            }

            rotacionAcumulada += giroFrame;
            float direccionRotacion = (transform.localScale.x > 0) ? 360f : -360f;

            float angulo = Mathf.Lerp(0f, direccionRotacion, rotacionAcumulada / 360f);
            transform.rotation = Quaternion.Euler(0f, 0f, angulo);

            yield return null;
        }

        transform.rotation = Quaternion.identity;
        yield return new WaitUntil(() => EstaEnSuelo());

        rb2d.linearVelocity = new Vector2(0f, rb2d.linearVelocity.y);
        haciendoBackflip = false;
    }

    IEnumerator DesactivarSaltoNormalReciente()
    {
        yield return new WaitForSeconds(tiempoDobleToque);
        saltoNormalReciente = false;
    }

    private void OnDrawGizmos()
    {
        if (!enabled || col2d == null) return;

        float direccion = (movimiento.x != 0) ? Mathf.Sign(movimiento.x) : 1;
        Vector2 origenPies = new Vector2(
            transform.position.x + (direccion * col2d.bounds.extents.x),
            transform.position.y - col2d.bounds.extents.y + 0.05f
        );
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origenPies, Vector2.right * direccion * 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(origenPies + Vector2.up * alturaEscalon, Vector2.right * direccion * (0.5f + 0.05f));
    }
    private void OnDestroy()
    {
        if (atacar != null)
        {
            atacar.canceled -= AlSoltarBotonDisparo;
        }
        DestruirMiraMouse();
    }
}