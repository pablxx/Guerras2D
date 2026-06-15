using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerInmortal : MonoBehaviour
{
    public string escenaObjetivo;
    public float tiempoCargaPersonalizado = 1.5f;
    [Header("Referencias de la Escena")]
    [SerializeField] private InicioManager vistaUI;

    [Header("Ajustes de Escala Visual")]
    [SerializeField] private float factorEscalaSeleccionado = 1.15f;
    [SerializeField] private float velocidadEscalado = 12f;

    [Header("Ajustes de Palpitación")]
    [SerializeField] private float velocidadPalpitacionBoton = 4f;
    [SerializeField] private float intensidadPalpitacionBoton = 0.04f;

    [Header("Ajustes de Color")]
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorSeleccionado = Color.green;

    [Header("Listas de Equipos")]
    public List<string> nombresEquipoA = new List<string>();
    public List<string> nombresEquipoB = new List<string>();

    [Header("Datos Guardados de la Partida")]
    public int contadorSoldados;
    public string dificultadSeleccionada;
    public string faccionJugador1;
    public string faccionJugador2;
    public string mapaSeleccionado;

    private int indiceActual = 0;
    private bool juegoIniciado = false;
    private Coroutine[] corrutinasEscala;
    private Vector3[] escalasOriginales;

    private CrearManager vistaCrearPartida;
    private bool enPantallaConfiguracion = false;

    private MapaManager vistaMapa;
    private bool enPantallaMapas = false;

    private void Awake()
    {
        PlayerInmortal[] managersActivos = Object.FindObjectsByType<PlayerInmortal>(FindObjectsSortMode.None);

        foreach (PlayerInmortal manager in managersActivos)
        {
            if (manager != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        DontDestroyOnLoad(gameObject);
    }

    public void ConfigurarNuevaEscenaPartida(CrearManager managerPartida)
    {
        vistaCrearPartida = managerPartida;
        enPantallaConfiguracion = true;
        enPantallaMapas = false;
        indiceActual = 0;

        int cantidadBotones = vistaCrearPartida.ListaBotonesPartida.Count;
        corrutinasEscala = new Coroutine[cantidadBotones];
        escalasOriginales = new Vector3[cantidadBotones];

        for (int i = 0; i < cantidadBotones; i++)
        {
            if (vistaCrearPartida.ListaBotonesPartida[i] != null)
            {
                escalasOriginales[i] = vistaCrearPartida.ListaBotonesPartida[i].GetComponent<RectTransform>().localScale;
            }
            else
            {
                escalasOriginales[i] = Vector3.one;
            }
        }

        ActualizarSeleccionFisica();
    }

    public void ActualizarReferenciaVista(InicioManager nuevaVista)
    {
        vistaUI = nuevaVista;
        if (vistaUI != null)
        {
            int cantidadBotones = vistaUI.MisBotones.Length;
            corrutinasEscala = new Coroutine[cantidadBotones];
            escalasOriginales = new Vector3[cantidadBotones];

            for (int i = 0; i < cantidadBotones; i++)
            {
                if (vistaUI.MisBotones[i].rectBoton != null)
                {
                    escalasOriginales[i] = vistaUI.MisBotones[i].rectBoton.localScale;
                }
                else
                {
                    escalasOriginales[i] = Vector3.one;
                }
            }
        }
    }

    public void AlmacenarDatosPartida(int contador, string dificultad, string faccionJ1, string faccionJ2)
    {
        contadorSoldados = contador;
        dificultadSeleccionada = dificultad;
        faccionJugador1 = faccionJ1;
        faccionJugador2 = faccionJ2;

        Debug.Log($"[PlayerInmortal] Datos guardados: C:{contador} | D:{dificultad} | J1:{faccionJ1} | J2:{faccionJ2}");
    }

    public void OnConfirmar(InputValue value)
    {
        if (!value.isPressed) return;

        Debug.Log($"[PlayerInmortal] OnConfirmar detectado. Juego Iniciado = {juegoIniciado}");

        if (enPantallaConfiguracion || enPantallaMapas)
        {
            return;
        }

        if (!juegoIniciado)
        {
            juegoIniciado = true;
            if (AudioManager.Instancia != null)
            {
                AudioManager.Instancia.PlaySFXPorIndice(0);
                AudioManager.Instancia.ReproducirMusicaPorIndice(Random.Range(0, 2));
            }
            if (vistaUI != null) vistaUI.LevantarMenuPrincipal();
        }
        else
        {
            if (vistaUI == null)
            {
                Debug.LogError("[PlayerInmortal] Error: No hay referencia a vistaUI asignada.");
                return;
            }

            var listaDeBotones = vistaUI.MisBotones;
            if (listaDeBotones != null && listaDeBotones.Length > 0)
            {
                Debug.Log($"[PlayerInmortal] Botón detectado. ÍNDICE ACTUAL = {indiceActual}");

                if (indiceActual == 0)
                {
                    Debug.Log("[PlayerInmortal] ¡ORDEN CONFIRMADA! Llamando a CargarEscenaCrearPartida().");
                    if (AudioManager.Instancia != null)
                    {
                        AudioManager.Instancia.PlayUIPorIndice(7);
                    }
                    vistaUI.CargarEscenaCrearPartida();
                }
            }
            else
            {
                Debug.LogError("[PlayerInmortal] Error: El array de botones en InicioManager está vacío o es nulo.");
            }
        }
    }

    public void OnArriba(InputValue value)
    {
        if (!value.isPressed || !enabled) return;
        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayUIPorIndice(1);
        }
        ProcesarCambioIndice(-1);
    }

    public void OnAbajo(InputValue value)
    {
        if (!value.isPressed || !enabled) return;
        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayUIPorIndice(1);
        }
        ProcesarCambioIndice(1);
    }

    public void OnDerecha(InputValue value)
    {
        if (!value.isPressed || !enabled) return;
        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayUIPorIndice(1);
        }
        ProcesarCambioIndice(1);
    }

    public void OnIzquierda(InputValue value)
    {
        if (!value.isPressed || !enabled) return;
        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlayUIPorIndice(1);
        }
        ProcesarCambioIndice(-1);
    }

    private void ProcesarCambioIndice(int cambio)
    {
        int totalBotones = 0;
        if (enPantallaConfiguracion) totalBotones = vistaCrearPartida.ListaBotonesPartida.Count;
        else if (enPantallaMapas) totalBotones = vistaMapa.ListaBotonesMapa.Count;
        else totalBotones = (vistaUI != null ? vistaUI.MisBotones.Length : 0);

        if (totalBotones == 0) return;

        indiceActual += cambio;

        if (indiceActual < 0) indiceActual = totalBotones - 1;
        if (indiceActual >= totalBotones) indiceActual = 0;

        ActualizarSeleccionFisica();
    }

    public void IniciarNavegacion()
    {
        indiceActual = 0;
        if (vistaUI != null && (corrutinasEscala == null || escalasOriginales == null || escalasOriginales.Length == 0))
        {
            ActualizarReferenciaVista(vistaUI);
        }

        ActualizarSeleccionFisica();
    }

    private void ActualizarSeleccionFisica()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (enPantallaConfiguracion)
        {
            if (vistaCrearPartida == null) return;
            var botones = vistaCrearPartida.ListaBotonesPartida;

            for (int i = 0; i < botones.Count; i++)
            {
                if (botones[i] == null) continue;
                RectTransform rect = botones[i].GetComponent<RectTransform>();
                if (rect == null) continue;

                bool esElSeleccionado = (i == indiceActual);

                Graphic grafico = botones[i].GetComponent<Graphic>();
                if (grafico != null)
                {
                    grafico.color = esElSeleccionado ? colorSeleccionado : colorNormal;
                }

                Vector3 escalaObjetivo = esElSeleccionado
                    ? escalasOriginales[i] * factorEscalaSeleccionado
                    : escalasOriginales[i];

                if (corrutinasEscala != null && i < corrutinasEscala.Length && corrutinasEscala[i] != null)
                {
                    StopCoroutine(corrutinasEscala[i]);
                }

                if (corrutinasEscala != null && i < corrutinasEscala.Length)
                {
                    corrutinasEscala[i] = StartCoroutine(RutinaEscalarYPalpitarBoton(rect, escalaObjetivo, esElSeleccionado, escalasOriginales[i]));
                }
            }

            if (EventSystem.current != null && botones.Count > indiceActual && botones[indiceActual] != null)
            {
                botones[indiceActual].Select();
            }
        }
        else if (enPantallaMapas)
        {
            if (vistaMapa == null) return;
            var botones = vistaMapa.ListaBotonesMapa;

            for (int i = 0; i < botones.Count; i++)
            {
                if (botones[i] == null) continue;
                RectTransform rect = botones[i].GetComponent<RectTransform>();
                if (rect == null) continue;

                bool esElSeleccionado = (i == indiceActual);

                Graphic grafico = botones[i].GetComponent<Graphic>();
                if (grafico != null)
                {
                    grafico.color = esElSeleccionado ? colorSeleccionado : colorNormal;
                }

                Vector3 escalaObjetivo = esElSeleccionado
                    ? escalasOriginales[i] * factorEscalaSeleccionado
                    : escalasOriginales[i];

                if (corrutinasEscala != null && i < corrutinasEscala.Length && corrutinasEscala[i] != null)
                {
                    StopCoroutine(corrutinasEscala[i]);
                }

                if (corrutinasEscala != null && i < corrutinasEscala.Length)
                {
                    corrutinasEscala[i] = StartCoroutine(RutinaEscalarYPalpitarBoton(rect, escalaObjetivo, esElSeleccionado, escalasOriginales[i]));
                }
            }

            if (EventSystem.current != null && botones.Count > indiceActual && botones[indiceActual] != null)
            {
                botones[indiceActual].Select();
            }
        }
        else
        {
            if (vistaUI == null) return;

            ActualizarTextoDescripcion();

            var botones = vistaUI.MisBotones;

            for (int i = 0; i < botones.Length; i++)
            {
                if (botones[i].rectBoton == null) continue;

                bool esElSeleccionado = (i == indiceActual);

                if (botones[i].botonReal != null)
                {
                    Graphic grafico = botones[i].botonReal.GetComponent<Graphic>();
                    if (grafico != null)
                    {
                        grafico.color = esElSeleccionado ? colorSeleccionado : colorNormal;
                    }
                }

                Vector3 escalaObjetivo = esElSeleccionado
                    ? escalasOriginales[i] * factorEscalaSeleccionado
                    : escalasOriginales[i];

                if (corrutinasEscala != null && i < corrutinasEscala.Length && corrutinasEscala[i] != null)
                {
                    StopCoroutine(corrutinasEscala[i]);
                }

                if (corrutinasEscala != null && i < corrutinasEscala.Length)
                {
                    corrutinasEscala[i] = StartCoroutine(RutinaEscalarYPalpitarBoton(botones[i].rectBoton, escalaObjetivo, esElSeleccionado, escalasOriginales[i]));
                }
            }

            if (EventSystem.current != null && botones.Length > indiceActual && botones[indiceActual].botonReal != null)
            {
                botones[indiceActual].botonReal.Select();
            }
        }
    }

    private void ActualizarTextoDescripcion()
    {
        if (vistaUI.TextoDinamicoIntro == null) return;

        switch (indiceActual)
        {
            case 0: vistaUI.TextoDinamicoIntro.text = "Juega con un rival por turnos"; break;
            case 1: vistaUI.TextoDinamicoIntro.text = "Configura opciones del juego "; break;
            case 2: vistaUI.TextoDinamicoIntro.text = "¿Ya te vas?"; break;
            default: vistaUI.TextoDinamicoIntro.text = ""; break;
        }
    }

    private IEnumerator RutinaEscalarYPalpitarBoton(RectTransform target, Vector3 destino, bool esSeleccionado, Vector3 escalaBaseOriginal)
    {
        while (target != null && Vector3.Distance(target.localScale, destino) > 0.005f)
        {
            target.localScale = Vector3.Lerp(target.localScale, destino, Time.deltaTime * velocidadEscalado);
            yield return null;
        }
        if (target == null) yield break;
        target.localScale = destino;
        float tiempoInicio = Time.time;
        while (esSeleccionado)
        {
            if (target == null) yield break;
            float oscilacion = Mathf.PingPong((Time.time - tiempoInicio) * velocidadPalpitacionBoton, intensidadPalpitacionBoton);
            float factorTotal = factorEscalaSeleccionado + oscilacion;
            target.localScale = escalaBaseOriginal * factorTotal;
            yield return null;
        }
    }

    public void DesactivarPantallaConfiguracion()
    {
        enPantallaConfiguracion = false;
        juegoIniciado = false;
        indiceActual = 0;
    }

    public void ConfigurarNuevaEscenaMapas(MapaManager managerMapas)
    {
        vistaMapa = managerMapas;
        enPantallaConfiguracion = false;
        enPantallaMapas = true;
        indiceActual = 0;
        int cantidadBotones = vistaMapa.ListaBotonesMapa.Count;
        corrutinasEscala = new Coroutine[cantidadBotones];
        escalasOriginales = new Vector3[cantidadBotones];

        for (int i = 0; i < cantidadBotones; i++)
        {
            if (vistaMapa.ListaBotonesMapa[i] != null)
            {
                escalasOriginales[i] = vistaMapa.ListaBotonesMapa[i].GetComponent<RectTransform>().localScale;
            }
            else
            {
                escalasOriginales[i] = Vector3.one;
            }
        }

        ActualizarSeleccionFisica();
    }
    public void CambiarEscenaConCarga(string nombreDestino, float duracionTransicion)
    {
        escenaObjetivo = nombreDestino;
        tiempoCargaPersonalizado = duracionTransicion;
        SceneManager.LoadScene("Carga");
    }
}