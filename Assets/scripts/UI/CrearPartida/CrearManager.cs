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
            Debug.Log("[CrearManager] Jugador Inmortal enlazado con éxito. Configurando 10 botones...");
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

    public void AccionBotonGO()
    {
        PlayerInmortal jugadorInmortal = Object.FindFirstObjectByType<PlayerInmortal>();

        if (jugadorInmortal != null)
        {
            int contador = valorPrimerTexto;
            string dificultad = opcionesDificultad[indiceDificultadActual];
            string faccionJ1 = opcionesPersonajes[indicePersonajeJ1];
            string faccionJ2 = opcionesPersonajes[indicePersonajeJ2];
            jugadorInmortal.AlmacenarDatosPartida(contador, dificultad, faccionJ1, faccionJ2);
            jugadorInmortal.DesactivarPantallaConfiguracion();
            jugadorInmortal.CambiarEscenaConCarga("Mapas", 0.3f);
        }
    }
}