using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CrearManager : MonoBehaviour
{
    [Header("Orden de Navegación")]
    [SerializeField] private List<Button> listaBotonesPartida = new List<Button>();

    [Header("Textos Modificables de la Partida")]
    [SerializeField] private List<TextMeshProUGUI> listaTextosPartida = new List<TextMeshProUGUI>();

    [Header("Scripts de Siluetas de los Jugadores")]
    [SerializeField] private siluetaP1 componenteSiluetaJ1;
    [SerializeField] private siluetaP1 componenteSiluetaJ2;

    [Header("Paneles de la Interfaz (Ventanas)")]
    [SerializeField] private GameObject panelPrincipal;
    [SerializeField] private GameObject panelNombresEquipos;

    [SerializeField] private Transform contenedorUI_EquipoA;
    [SerializeField] private Transform contenedorUI_EquipoB;
    [SerializeField] private TextMeshProUGUI textoAdvertenciaNombres;

    public List<Button> ListaBotonesPartida => listaBotonesPartida;
    private int valorPrimerTexto = 1;
    private string[] opcionesDificultad = { "Yesca", "Normal", "Jaila" };
    private int indiceDificultadActual = 1;
    private string[] opcionesPersonajes = { "Policias", "Mineros ", "Campesinitos", "Cholitas" };
    private int indicePersonajeJ1 = 0;
    private int indicePersonajeJ2 = 1;

    private void Start()
    {
        ActualizarPantallaVisual();

        PlayerInmortal jugador = Object.FindFirstObjectByType<PlayerInmortal>();
        if (jugador != null)
        {
            Debug.Log("[CrearManager] Jugador Inmortal enlazado con éxito. Configurando botones...");
            jugador.ConfigurarNuevaEscenaPartida(this);
        }
        else
        {
            Debug.LogError("[CrearManager] No se encontró al PlayerInmortal en esta escena.");
        }
    }

    private void ActualizarPantallaVisual()
    {
        if (listaTextosPartida.Count > 0 && listaTextosPartida[0] != null)
        {
            listaTextosPartida[0].text = valorPrimerTexto.ToString();
        }

        if (listaTextosPartida.Count > 1 && listaTextosPartida[1] != null)
        {
            listaTextosPartida[1].text = opcionesDificultad[indiceDificultadActual];
        }

        if (listaTextosPartida.Count > 2 && listaTextosPartida[2] != null)
        {
            listaTextosPartida[2].text = opcionesPersonajes[indicePersonajeJ1];
        }

        if (listaTextosPartida.Count > 3 && listaTextosPartida[3] != null)
        {
            listaTextosPartida[3].text = opcionesPersonajes[indicePersonajeJ2];
        }

        if (componenteSiluetaJ1 != null)
        {
            componenteSiluetaJ1.CargarImagen(indicePersonajeJ1);
        }

        if (componenteSiluetaJ2 != null)
        {
            componenteSiluetaJ2.CargarImagen(indicePersonajeJ2);
        }
    }

    public void AccionRegresarInicio()
    {
        PlayerInmortal jugador = Object.FindFirstObjectByType<PlayerInmortal>();
        if (jugador != null) jugador.DesactivarPantallaConfiguracion();
        SceneManager.LoadScene("Inicio");
    }

    public void AccionRestarPrimerTexto()
    {
        if (valorPrimerTexto > 1)
        {
            valorPrimerTexto--;
            ActualizarPantallaVisual();
        }
    }

    public void AccionSumarPrimerTexto()
    {
        if (valorPrimerTexto < 5)
        {
            valorPrimerTexto++;
            ActualizarPantallaVisual();
        }
    }

    public void AccionAnteriorSegundoTexto()
    {
        indiceDificultadActual--;
        if (indiceDificultadActual < 0) indiceDificultadActual = opcionesDificultad.Length - 1;
        ActualizarPantallaVisual();
    }

    public void AccionSiguienteSegundoTexto()
    {
        indiceDificultadActual++;
        if (indiceDificultadActual >= opcionesDificultad.Length) indiceDificultadActual = 0;
        ActualizarPantallaVisual();
    }

    public void AccionAnteriorPerfilJ1()
    {
        indicePersonajeJ1--;
        if (indicePersonajeJ1 < 0) indicePersonajeJ1 = opcionesPersonajes.Length - 1;
        ActualizarPantallaVisual();
    }

    public void AccionSiguientePerfilJ1()
    {
        indicePersonajeJ1++;
        if (indicePersonajeJ1 >= opcionesPersonajes.Length) indicePersonajeJ1 = 0;
        ActualizarPantallaVisual();
    }

    public void AccionAnteriorPerfilJ2()
    {
        indicePersonajeJ2--;
        if (indicePersonajeJ2 < 0) indicePersonajeJ2 = opcionesPersonajes.Length - 1;
        ActualizarPantallaVisual();
        Debug.Log($"[CrearManager] Jugador 2 retrocedió a: {opcionesPersonajes[indicePersonajeJ2]}");
    }

    public void AccionSiguientePerfilJ2()
    {
        indicePersonajeJ2++;
        if (indicePersonajeJ2 >= opcionesPersonajes.Length) indicePersonajeJ2 = 0;
        ActualizarPantallaVisual();
        Debug.Log($"[CrearManager] Jugador 2 avanzó a: {opcionesPersonajes[indicePersonajeJ2]}");
    }

    public void AccionBotonNombresAbrirVentana()
    {
        if (panelPrincipal != null && panelNombresEquipos != null)
        {
            panelPrincipal.SetActive(false);
            panelNombresEquipos.SetActive(true);
            Debug.Log("[CrearManager] Ventana auxiliar de nombres abierta.");
            PlayerInmortal jugador = Object.FindFirstObjectByType<PlayerInmortal>();
            if (jugador != null && jugador.TryGetComponent(out UnityEngine.InputSystem.PlayerInput pi))
            {
                pi.enabled = false;
                Debug.Log("[CrearManager] PlayerInput desactivado temporalmente para permitir la escritura.");
            }
        }
    }

    public void VolverAlMenuPrincipal()
    {
        if (panelPrincipal != null && panelNombresEquipos != null)
        {
            PlayerInmortal jugadorInmortal = Object.FindFirstObjectByType<PlayerInmortal>();

            if (jugadorInmortal != null)
            {
                jugadorInmortal.nombresEquipoA.Clear();
                jugadorInmortal.nombresEquipoB.Clear();
                if (contenedorUI_EquipoA != null)
                {
                    TMP_InputField[] inputsA = contenedorUI_EquipoA.GetComponentsInChildren<TMP_InputField>();
                    for (int i = 0; i < inputsA.Length; i++)
                    {
                        if (i == 0)
                        {
                            string nombreEquipo = string.IsNullOrEmpty(inputsA[i].text) ? "Equipo A" : inputsA[i].text;
                            jugadorInmortal.nombresEquipoA.Add(nombreEquipo);
                        }
                        else
                        {
                            string nombreSoldado = string.IsNullOrEmpty(inputsA[i].text) ? $"Soldado A {i}" : inputsA[i].text;
                            jugadorInmortal.nombresEquipoA.Add(nombreSoldado);
                        }
                    }
                }
                if (contenedorUI_EquipoB != null)
                {
                    TMP_InputField[] inputsB = contenedorUI_EquipoB.GetComponentsInChildren<TMP_InputField>();
                    for (int i = 0; i < inputsB.Length; i++)
                    {
                        if (i == 0)
                        {

                            string nombreEquipo = string.IsNullOrEmpty(inputsB[i].text) ? "Equipo B" : inputsB[i].text;
                            jugadorInmortal.nombresEquipoB.Add(nombreEquipo);
                        }
                        else
                        {

                            string nombreSoldado = string.IsNullOrEmpty(inputsB[i].text) ? $"Soldado B {i}" : inputsB[i].text;
                            jugadorInmortal.nombresEquipoB.Add(nombreSoldado);
                        }
                    }
                }
            }

            panelNombresEquipos.SetActive(false);
            panelPrincipal.SetActive(true);
            Debug.Log("[CrearManager] Regresando al panel principal.");
            if (jugadorInmortal != null && jugadorInmortal.TryGetComponent(out UnityEngine.InputSystem.PlayerInput pi))
            {
                pi.enabled = true;
                Debug.Log("[CrearManager] PlayerInput reactivado con éxito.");
            }
        }
    }

    public void AccionBotonGO()
    {
        PlayerInmortal jugadorInmortal = Object.FindFirstObjectByType<PlayerInmortal>();

        if (jugadorInmortal != null)
        {
            if (jugadorInmortal.nombresEquipoA.Count == 0 || jugadorInmortal.nombresEquipoB.Count == 0)
            {
                if (textoAdvertenciaNombres != null)
                {
                    StopAllCoroutines();
                    StartCoroutine(MostrarYDesvanecerAdvertencia());
                }
                return;
            }

            int contador = valorPrimerTexto;
            string dificultad = opcionesDificultad[indiceDificultadActual];
            string faccionJ1 = opcionesPersonajes[indicePersonajeJ1];
            string faccionJ2 = opcionesPersonajes[indicePersonajeJ2];
            jugadorInmortal.AlmacenarDatosPartida(contador, dificultad, faccionJ1, faccionJ2);
            jugadorInmortal.DesactivarPantallaConfiguracion();
            jugadorInmortal.CambiarEscenaConCarga("Mapas", 0.3f);
        }
    }

    private IEnumerator MostrarYDesvanecerAdvertencia()
    {
        textoAdvertenciaNombres.text = "¡Atención! Debes configurar los nombres de los equipos antes de iniciar.";
        textoAdvertenciaNombres.gameObject.SetActive(true);

        Color colorTexto = textoAdvertenciaNombres.color;
        colorTexto.a = 1f;
        textoAdvertenciaNombres.color = colorTexto;

        yield return new WaitForSeconds(2.5f);

        float tiempoDesvanecer = 1f;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoDesvanecer)
        {
            tiempoTranscurrido += Time.deltaTime;
            colorTexto.a = Mathf.Lerp(1f, 0f, tiempoTranscurrido / tiempoDesvanecer);
            textoAdvertenciaNombres.color = colorTexto;
            yield return null;
        }

        textoAdvertenciaNombres.gameObject.SetActive(false);
    }
}