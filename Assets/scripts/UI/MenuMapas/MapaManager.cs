using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MapaManager : MonoBehaviour
{
    [Header("Orden de Navegación (4 Botones)")]
    [SerializeField] private List<Button> listaBotonesMapa = new List<Button>();

    [Header("Texto Modificable del Mapa")]
    [SerializeField] private TextMeshProUGUI textoNombreMapa;

    [Header("Script de Silueta de Mapa")]
    [Tooltip("Arrastra aquí el objeto Image del mapa que tiene el script siluetaMapa")]
    [SerializeField] private siluetaMapa componenteSiluetaMapa;

    public List<Button> ListaBotonesMapa => listaBotonesMapa;

    private string[] opcionesMapas = { "Gobernacion", "Parque Cretacico", "Villa Tunari" };
    private int indiceMapaActual = 0;

    private void Start()
    {
        ActualizarPantallaVisual();

        PlayerInmortal jugador = Object.FindFirstObjectByType<PlayerInmortal>();
        if (jugador != null)
        {
            Debug.Log("[MapaManager] Jugador Inmortal enlazado con éxito en Mapas.");
            jugador.ConfigurarNuevaEscenaMapas(this);
        }
        else
        {
            Debug.LogError("[MapaManager] No se encontró al PlayerInmortal en la escena de Mapas.");
        }
    }

    private void ActualizarPantallaVisual()
    {
        if (textoNombreMapa != null && opcionesMapas.Length > indiceMapaActual)
        {
            textoNombreMapa.text = opcionesMapas[indiceMapaActual];
        }
        if (componenteSiluetaMapa != null)
        {
            componenteSiluetaMapa.CargarImagenMapa(indiceMapaActual);
        }
    }

    public void AccionRegresarConfiguracion()
    {
        AudioManager.Instancia.PlayUIPorIndice(0);
        SceneManager.LoadScene("CrearPartida");
    }

    public void AccionAnteriorMapa()
    {
        AudioManager.Instancia.PlayUIPorIndice(3);
        indiceMapaActual--;
        if (indiceMapaActual < 0)
        {
            indiceMapaActual = opcionesMapas.Length - 1;
        }
        ActualizarPantallaVisual();
        Debug.Log($"[MapaManager] Mapa cambiado a: {opcionesMapas[indiceMapaActual]}");
    }

    public void AccionSiguienteMapa()
    {
        AudioManager.Instancia.PlayUIPorIndice(3);
        indiceMapaActual++;
        if (indiceMapaActual >= opcionesMapas.Length)
        {
            indiceMapaActual = 0;
        }
        ActualizarPantallaVisual();
        Debug.Log($"[MapaManager] Mapa cambiado a: {opcionesMapas[indiceMapaActual]}");
    }

    public void AccionIniciarPartida()
    {
        AudioManager.Instancia.PlayUIPorIndice(7);
        string mapaFinal = opcionesMapas[indiceMapaActual];
        Debug.Log($"[MapaManager] Preparando guardado del escenario: {mapaFinal}");
        PlayerInmortal jugador = Object.FindFirstObjectByType<PlayerInmortal>();
        if (jugador != null)
        {
            jugador.mapaSeleccionado = mapaFinal;
            Debug.Log($"[MapaManager] ¡Mapa '{jugador.mapaSeleccionado}' guardado con éxito en PlayerInmortal!");
        }
        else
        {
            Debug.LogWarning("[MapaManager] No se pudo guardar el mapa porque PlayerInmortal no fue encontrado.");
        }
        Debug.Log($" Lanzando partida oficial en: {mapaFinal}");
        jugador.CambiarEscenaConCarga("EscenaGusanos", 3f);
        AudioManager.Instancia.CalmarTodosLosEfectos();
    }


}