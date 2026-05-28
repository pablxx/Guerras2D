using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TurnoManager : MonoBehaviour
{
    public static TurnoManager Instancia;

    [SerializeField] camaraMovimiento miCamara;
    [SerializeField] private GameObject prefabEquipoA;
    [SerializeField] private GameObject prefabEquipoB;

    [Header("Configuración de Partida")]
    [SerializeField] int cantidadSoldadosPorEquipo;
    [SerializeField] private Color colorEquipoA;
    [SerializeField] private Color colorEquipoB;
    [SerializeField] Transform LimiteIzq;
    [SerializeField] Transform LimiteDer;

    [Header("Espacio para Notificaciones UI")]
    [SerializeField] private TextMeshProUGUI textoNotificacionesUI;
    [SerializeField] TextMeshProUGUI Temporizador;

    [Header("Configuración del Temporizador")]
    [SerializeField] private float tiempoPorTurno = 45f;
    private float tiempoRestante;
    private bool temporizadorActivo = false;

    public List<GameObject> ListaSoldadosA;
    public List<GameObject> ListaSoldadosB;
    public GameObject soldadoActivoEnEsteTurno;
    public TipoEquipo equipoQueLeToca;
    float min;
    float max;
    public List<GameObject> listaMuertosPendientes = new List<GameObject>();
    public bool procesandoFaseMuertos = false;
    private Coroutine corrutinaNotificacionActiva;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }

    void Start()
    {
        ListaSoldadosA = new List<GameObject>();
        ListaSoldadosB = new List<GameObject>();
        min = LimiteIzq.transform.position.x;
        max = LimiteDer.transform.position.x;

        if (textoNotificacionesUI != null) textoNotificacionesUI.text = "";

        InitializePartidaPorEquipos();
        foreach (GameObject s in ListaSoldadosA) s.SetActive(true);
        foreach (GameObject s in ListaSoldadosB) s.SetActive(true);

        equipoQueLeToca = TipoEquipo.EquipoA;
        ActivarSiguienteEnCola();
    }
    void Update()
    {
        if (!temporizadorActivo || procesandoFaseMuertos) return;
        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            if (textoNotificacionesUI != null && corrutinaNotificacionActiva == null)
            {
                Temporizador.text = Mathf.CeilToInt(tiempoRestante).ToString() + "s";
            }
        }
        else
        {
            tiempoRestante = 0;
            temporizadorActivo = false;
            if (textoNotificacionesUI != null && corrutinaNotificacionActiva == null)
                textoNotificacionesUI.text = "¡TIEMPO AGOTADO!";
            Debug.LogWarning("[TurnoManager] El jugador agotó su tiempo de turno.");
            FinalizarTurno(null);
        }
    }

    private bool VerificarFinDePartida()
    {
        if (ListaSoldadosA.Count == 0)
        {
            Debug.LogWarning("yeiiii por fin se acabo el juego.....quiero dormir :v");

            if (textoNotificacionesUI != null)
            {
                if (corrutinaNotificacionActiva != null) StopCoroutine(corrutinaNotificacionActiva);
                textoNotificacionesUI.text = "¡VICTORIA! GANÓ EL EQUIPO B";
            }

            temporizadorActivo = false;
            return true;
        }
        if (ListaSoldadosB.Count == 0)
        {
            Debug.LogWarning("yeiiii por fin se acabo el juego.....quiero dormir :v");

            if (textoNotificacionesUI != null)
            {
                if (corrutinaNotificacionActiva != null) StopCoroutine(corrutinaNotificacionActiva);
                textoNotificacionesUI.text = "¡VICTORIA! GANÓ EL EQUIPO A";
            }
            temporizadorActivo = false;
            return true;
        }

        return false;
    }
    public void FinalizarTurno(GameObject ArmaUtilizada)
    {
        temporizadorActivo = false;
        PanelInventario.Instancia.PermisoAtacar = false;

        if (soldadoActivoEnEsteTurno != null)
        {
            DesactivarSoldadoEspecifico(soldadoActivoEnEsteTurno);

            if (ListaSoldadosA.Contains(soldadoActivoEnEsteTurno) || ListaSoldadosB.Contains(soldadoActivoEnEsteTurno))
            {
                RotarSoldadoAlFinal(soldadoActivoEnEsteTurno);
            }
        }

        if (listaMuertosPendientes.Count > 0)
        {
            Debug.Log($"[TurnoManager] ¡Alto ahí! Hay {listaMuertosPendientes.Count} muertos acumulados. Deteniendo cambio de bando e iniciando secuencia de cámara.");
            StartCoroutine(ProcesarCadenaDeMuertes(ArmaUtilizada));
            return;
        }
        else
        {
            Debug.Log("[TurnoManager] Turno limpio sin bajas en cola. Continuando de forma normal.");
            if (ArmaUtilizada != null)
            {
                Destroy(ArmaUtilizada);
            }
        }
        if (VerificarFinDePartida())
        {
            return;
        }
        equipoQueLeToca = (equipoQueLeToca == TipoEquipo.EquipoA) ? TipoEquipo.EquipoB : TipoEquipo.EquipoA;
        PanelInventario.Instancia.DibujarInventarioUI();
        ActivarSiguienteEnCola();
    }

    private IEnumerator ProcesarCadenaDeMuertes(GameObject armaParaLimpiar)
    {
        procesandoFaseMuertos = true;

        for (int i = 0; i < listaMuertosPendientes.Count; i++)
        {
            GameObject soldadoCaido = listaMuertosPendientes[i];
            if (soldadoCaido == null) continue;
            Debug.Log($"[Fase Muertos] Enfocando a {soldadoCaido.name} por 3 segundos.");
            MostrarNotificacion($"{soldadoCaido.name} cayó en combate y va a estallar."); // Adaptado al nuevo nombre
            DesactivarSoldadoEspecifico(soldadoCaido);
             var inputGusanoMuerto = soldadoCaido.GetComponent<PlayerInput>();
            if (miCamara != null && inputGusanoMuerto != null)
            {
                miCamara.ActualizarReferenciaInput(inputGusanoMuerto);
            }
            yield return new WaitForSeconds(3f);
            autodestruccion scriptExplosion = soldadoCaido.GetComponent<autodestruccion>();
            if (scriptExplosion != null)
            {
                bool detonacionTerminada = false;
                scriptExplosion.OnDetonacionCompletada = () => {
                    detonacionTerminada = true;
                };
                Debug.Log($"[Fase Muertos] Iniciando cuenta regresiva de {soldadoCaido.name}.");
                scriptExplosion.IniciarCuentaRegresiva();
                yield return new WaitUntil(() => detonacionTerminada);
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                Destroy(soldadoCaido);
                yield return new WaitForSeconds(0.5f);
            }
        }
        Debug.Log("[Fase Muertos] Todos los caídos de este turno han detonado. Reanudando flujo de juego.");
        listaMuertosPendientes.Clear();
        procesandoFaseMuertos = false;

        if (armaParaLimpiar != null)
        {
            Destroy(armaParaLimpiar);
        }
        if (VerificarFinDePartida())
        {
            yield break;
        }
        equipoQueLeToca = (equipoQueLeToca == TipoEquipo.EquipoA) ? TipoEquipo.EquipoB : TipoEquipo.EquipoA;
        PanelInventario.Instancia.DibujarInventarioUI();
        ActivarSiguienteEnCola();
    }

    public void RegistrarMuerteJugador(GameObject gusanoTarget)
    {
        if (gusanoTarget == null) return;
        if (ListaSoldadosA.Contains(gusanoTarget)) ListaSoldadosA.Remove(gusanoTarget);
        if (ListaSoldadosB.Contains(gusanoTarget)) ListaSoldadosB.Remove(gusanoTarget);
        Vida vidaGusano = gusanoTarget.GetComponent<Vida>();
        if (listaMuertosPendientes.Contains(gusanoTarget) && !gusanoTarget.activeInHierarchy)
        {
            listaMuertosPendientes.Remove(gusanoTarget);
            MostrarNotificacion($"{gusanoTarget.name} voló fuera del mapa."); // Adaptado al nuevo nombre
            Destroy(gusanoTarget);
            return;
        }
        if (vidaGusano == null || vidaGusano.vidaActual > 0 || !gusanoTarget.activeInHierarchy)
        {
            MostrarNotificacion($"{gusanoTarget.name} cayó al vacío."); // Adaptado al nuevo nombre
            if (listaMuertosPendientes.Contains(gusanoTarget)) listaMuertosPendientes.Remove(gusanoTarget);
            Destroy(gusanoTarget);
            return;
        }
        if (!listaMuertosPendientes.Contains(gusanoTarget))
        {
            listaMuertosPendientes.Add(gusanoTarget);
            Debug.Log($"[TurnoManager] {gusanoTarget.name} anotado para autodestrucción cinematográfica en tierra. Total: {listaMuertosPendientes.Count}");
        }
    }

    public void DetenerTemporizadorPorAtaque()
    {
        temporizadorActivo = false;
    }

    void ActivarSiguienteEnCola()
    {
        if (ListaSoldadosA.Count == 0 || ListaSoldadosB.Count == 0) return;
        soldadoActivoEnEsteTurno = (equipoQueLeToca == TipoEquipo.EquipoA) ? ListaSoldadosA[0] : ListaSoldadosB[0];
        if (soldadoActivoEnEsteTurno != null)
        {
            Debug.Log($"[TurnoManager] Turno de: {soldadoActivoEnEsteTurno.name} al frente de la cola ({equipoQueLeToca})");
            EnfocarSoldadoEspecifico(soldadoActivoEnEsteTurno);
            var mov = soldadoActivoEnEsteTurno.GetComponent<movimientoJugador>();
            if (mov != null)
            {
                mov.atacando = false;
            }
            tiempoRestante = tiempoPorTurno;
            temporizadorActivo = true;
        }
    }
    public void MostrarNotificacion(string mensaje)
    {
        Debug.LogWarning($"[NOTIFICACIÓN]: {mensaje}");
        if (textoNotificacionesUI != null)
        {
            if (corrutinaNotificacionActiva != null) StopCoroutine(corrutinaNotificacionActiva);
            corrutinaNotificacionActiva = StartCoroutine(SecuenciaLetreroUI(mensaje));
        }
    }

    private IEnumerator SecuenciaLetreroUI(string mensaje)
    {
        textoNotificacionesUI.text = mensaje;
        yield return new WaitForSeconds(3.5f);
        textoNotificacionesUI.text = "";
        corrutinaNotificacionActiva = null;
    }
    void InitializePartidaPorEquipos()
    {
        for (int i = 0; i < cantidadSoldadosPorEquipo; i++)
        {
            GameObject nuevoSoldado = Instantiate(prefabEquipoA, transform.position, transform.rotation);
            ConfigurarComponentesBase(nuevoSoldado);
            DatosJugador datos = nuevoSoldado.GetComponent<DatosJugador>();
            datos.equipoJugador = TipoEquipo.EquipoA;
            string nombreParaCanvas = "Soldado " + (i + 1);
            datos.ConfigurarTextoCanvas(nombreParaCanvas, colorEquipoA);
            ListaSoldadosA.Add(nuevoSoldado);
        }

        for (int i = 0; i < cantidadSoldadosPorEquipo; i++)
        {
            GameObject nuevoSoldado = Instantiate(prefabEquipoB, transform.position, transform.rotation);
            ConfigurarComponentesBase(nuevoSoldado);
            DatosJugador datos = nuevoSoldado.GetComponent<DatosJugador>();
            datos.equipoJugador = TipoEquipo.EquipoB;
            string nombreParaCanvas = "Soldado " + (i + 1);
            datos.ConfigurarTextoCanvas(nombreParaCanvas, colorEquipoB);
            ListaSoldadosB.Add(nuevoSoldado);
        }

        BarajarLista(ListaSoldadosA);
        BarajarLista(ListaSoldadosB);
        RandomizarPosicionLista(ListaSoldadosA);
        RandomizarPosicionLista(ListaSoldadosB);
    }

    void RotarSoldadoAlFinal(GameObject soldado)
    {
        if (soldado == null) return;
        DatosJugador datos = soldado.GetComponent<DatosJugador>();
        if (datos == null) return;

        if (datos.equipoJugador == TipoEquipo.EquipoA)
        {
            if (ListaSoldadosA.Count > 1 && ListaSoldadosA[0] == soldado)
            {
                ListaSoldadosA.RemoveAt(0);
                ListaSoldadosA.Add(soldado);
            }
        }
        else
        {
            if (ListaSoldadosB.Count > 1 && ListaSoldadosB[0] == soldado)
            {
                ListaSoldadosB.RemoveAt(0);
                ListaSoldadosB.Add(soldado);
            }
        }
    }

    void ConfigurarComponentesBase(GameObject soldado)
    {
        soldado.transform.GetChild(0).gameObject.SetActive(false);
        var scriptMov = soldado.GetComponent<movimientoJugador>();
        if (scriptMov != null && scriptMov.barraFuerzaUI != null)
        {
            scriptMov.barraFuerzaUI.gameObject.SetActive(false);
        }
        soldado.SetActive(false);
    }

    void BarajarLista(List<GameObject> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            GameObject temporal = lista[i];
            lista[i] = lista[j];
            lista[j] = temporal;
        }
    }

    void RandomizarPosicionLista(List<GameObject> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            float randomX = UnityEngine.Random.Range(min, max);
            randomX = Mathf.Round(randomX * 10f) / 10f;
            Vector3 nuevaPos = new Vector3(randomX, lista[i].transform.position.y, lista[i].transform.position.z);
            lista[i].transform.position = nuevaPos;

            Rigidbody2D rb = lista[i].GetComponent<Rigidbody2D>();
            if (rb != null) rb.gravityScale = 1f;
        }
    }

    void EnfocarSoldadoEspecifico(GameObject soldado)
    {
        soldado.transform.GetChild(0).gameObject.SetActive(true);
        var input = soldado.GetComponent<PlayerInput>();
        if (input != null) input.enabled = true;
        var scriptMov = soldado.GetComponent<movimientoJugador>();
        if (scriptMov != null) scriptMov.enabled = true;
        miCamara.ActualizarReferenciaInput(input);
    }

    void DesactivarSoldadoEspecifico(GameObject soldado)
    {
        if (soldado == null) return;
        soldado.transform.GetChild(0).gameObject.SetActive(false);
        var input = soldado.GetComponent<PlayerInput>();
        if (input != null) input.enabled = false;
        var scriptMov = soldado.GetComponent<movimientoJugador>();
        if (scriptMov != null) scriptMov.enabled = false;
    }

    public IEnumerator TemporizadorCambioTurno(GameObject ArmaUtilizada)
    {
        yield return new WaitForSeconds(0.8f);
        while (!TodoElMundoEstaQuieto())
        {
            yield return new WaitForSeconds(0.2f); 
        }
        yield return new WaitForSeconds(0.5f);
        FinalizarTurno(ArmaUtilizada);
    }
    private bool TodoElMundoEstaQuieto()
    {
        foreach (GameObject soldado in ListaSoldadosA)
        {
            if (soldado == null) continue;
            Rigidbody2D rb = soldado.GetComponent<Rigidbody2D>();
            if (rb != null && (rb.linearVelocity.magnitude > 0.05f || Mathf.Abs(rb.angularVelocity) > 0.05f))
            {
                return false;
            }
        }
        foreach (GameObject soldado in ListaSoldadosB)
        {
            if (soldado == null) continue;
            Rigidbody2D rb = soldado.GetComponent<Rigidbody2D>();

            if (rb != null && (rb.linearVelocity.magnitude > 0.05f || Mathf.Abs(rb.angularVelocity) > 0.05f))
            {
                return false; 
            }
        }
        return true;
    }
}