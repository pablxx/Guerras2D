using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class InicioManager : MonoBehaviour
{
    [System.Serializable]
    public struct ConfigBoton
    {
        public string nombre;
        public Button botonReal;
        public RectTransform rectBoton;
        public RectTransform puntoInicio;
        public RectTransform puntoDestino;
    }

    [Header("UI - Componente Unificado")]
    [SerializeField] private TextMeshProUGUI textoDinamicoIntro;
    [SerializeField] private float velocidadPalpitacion = 2f;

    [Header("UI - Fase Menú Principal")]
    [SerializeField] private GameObject panelMenuPrincipal;
    [SerializeField] private ConfigBoton[] misBotones;
    [SerializeField] private float duracionMovimiento = 0.6f;
    [SerializeField] private float delayEntreBotones = 0.18f;
    [SerializeField] private AnimationCurve curvaSuave = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Controlador del Jugador")]
    [SerializeField] private PlayerInmortal scriptJugador;

    private bool yaConfirmoIntro = false;
    private Coroutine corrutinaPalpitacion;

    public ConfigBoton[] MisBotones => misBotones;
    public TextMeshProUGUI TextoDinamicoIntro => textoDinamicoIntro; 

    private void Start()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);

        if (scriptJugador == null)
        {
            scriptJugador = Object.FindFirstObjectByType<PlayerInmortal>();
        }

        if (scriptJugador != null)
        {
            scriptJugador.ActualizarReferenciaVista(this);
            scriptJugador.enabled = false;
        }

        if (textoDinamicoIntro != null)
        {
            corrutinaPalpitacion = StartCoroutine(RutinaPalpitarTexto());
        }
    }

    public void LevantarMenuPrincipal()
    {
        if (yaConfirmoIntro) return;
        yaConfirmoIntro = true;
        if (corrutinaPalpitacion != null) StopCoroutine(corrutinaPalpitacion);
        if (textoDinamicoIntro != null)
        {
            textoDinamicoIntro.color = new Color(textoDinamicoIntro.color.r, textoDinamicoIntro.color.g, textoDinamicoIntro.color.b, 1f);
            textoDinamicoIntro.text = "";
        }

        if (panelMenuPrincipal != null)
        {
            panelMenuPrincipal.SetActive(true);

            foreach (var boton in misBotones)
            {
                if (boton.rectBoton != null && boton.puntoInicio != null)
                {
                    boton.rectBoton.anchoredPosition = boton.puntoInicio.anchoredPosition;
                }
            }

            StartCoroutine(RutinaSecuenciaEntradaBotones());
        }
    }

    private IEnumerator RutinaSecuenciaEntradaBotones()
    {
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < misBotones.Length; i++)
        {
            if (misBotones[i].rectBoton != null && misBotones[i].puntoInicio != null && misBotones[i].puntoDestino != null)
            {
                StartCoroutine(MoverBotonHaciaPunto(misBotones[i]));
                yield return new WaitForSeconds(delayEntreBotones);
            }
        }

        if (scriptJugador != null)
        {
            scriptJugador.enabled = true;
            scriptJugador.IniciarNavegacion();
        }
    }

    private IEnumerator MoverBotonHaciaPunto(ConfigBoton datos)
    {
        float tiempo = 0f;
        Vector2 posInicial = datos.puntoInicio.anchoredPosition;
        Vector2 posFinal = datos.puntoDestino.anchoredPosition;

        while (tiempo < duracionMovimiento)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracionMovimiento;
            float factorCurva = curvaSuave.Evaluate(progreso);

            datos.rectBoton.anchoredPosition = Vector2.Lerp(posInicial, posFinal, factorCurva);
            yield return null;
        }

        datos.rectBoton.anchoredPosition = posFinal;
    }

    private IEnumerator RutinaPalpitarTexto()
    {
        Color colorOriginal = textoDinamicoIntro.color;
        while (!yaConfirmoIntro)
        {
            float alfa = Mathf.PingPong(Time.time * velocidadPalpitacion, 1f);
            textoDinamicoIntro.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alfa);
            yield return null;
        }
    }

    public void CargarEscenaCrearPartida()
    {
        PlayerInmortal jugador = Object.FindFirstObjectByType<PlayerInmortal>();
        jugador.CambiarEscenaConCarga("CrearPartida", 0.2f);
    }
}