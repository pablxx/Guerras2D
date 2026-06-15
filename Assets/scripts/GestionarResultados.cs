using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GestionarResultados : MonoBehaviour
{
    [Header("UI de Resultados - Contenedor")]
    [SerializeField] private GameObject panelResultadosUI;

    [Header("UI de Resultados - Textos")]
    [SerializeField] private TextMeshProUGUI textoGanador;
    [SerializeField] private TextMeshProUGUI textoNombreEquipoA;
    [SerializeField] private TextMeshProUGUI textoNombreEquipoB;
    [SerializeField] private TextMeshProUGUI textoBajasEquipoA;
    [SerializeField] private TextMeshProUGUI textoBajasEquipoB;

    [Header("Contador de Bajas de la Partida")]
    public int bajasEquipoA = 0;
    public int bajasEquipoB = 0;

    public void ContabilizarBaja(GameObject soldadoCaido)
    {
        if (soldadoCaido == null) return;

        DatosJugador datos = soldadoCaido.GetComponent<DatosJugador>();
        if (datos != null)
        {
            if (datos.equipoJugador == TipoEquipo.EquipoA)
            {
                bajasEquipoA++;
                Debug.Log($"[GestionarResultados] Baja registrada para el Equipo A. Total actual: {bajasEquipoA}");
            }
            else if (datos.equipoJugador == TipoEquipo.EquipoB)
            {
                bajasEquipoB++;
                Debug.Log($"[GestionarResultados] Baja registrada para el Equipo B. Total actual: {bajasEquipoB}");
            }
        }
    }

    public void MostrarResultados()
    {
        AudioManager.Instancia.PlaySFXPorIndice(1);
        string nombreA = "EQUIPO A";
        string nombreB = "EQUIPO B";

        if (TurnoManager.Instancia != null)
        {
            nombreA = TurnoManager.Instancia.NombreEquipoA;
            nombreB = TurnoManager.Instancia.NombreEquipoB;
        }

        if (textoGanador != null)
        {
            if (TurnoManager.Instancia != null && TurnoManager.Instancia.ListaSoldadosA.Count == 0 && TurnoManager.Instancia.ListaSoldadosB.Count == 0)
            {
                textoGanador.text = "¡EMPATE ABSOLUTO!";
            }
            else if (TurnoManager.Instancia != null && TurnoManager.Instancia.ListaSoldadosA.Count == 0)
            {
                textoGanador.text = $"¡VICTORIA DE {nombreB.ToUpper()}!";
            }
            else if (TurnoManager.Instancia != null && TurnoManager.Instancia.ListaSoldadosB.Count == 0)
            {
                textoGanador.text = $"¡VICTORIA DE {nombreA.ToUpper()}!";
            }
            else
            {
                textoGanador.text = "PARTIDA FINALIZADA";
            }
        }

        if (textoNombreEquipoA != null) textoNombreEquipoA.text = nombreA;
        if (textoNombreEquipoB != null) textoNombreEquipoB.text = nombreB;

        if (textoBajasEquipoA != null) textoBajasEquipoA.text = bajasEquipoA.ToString();
        if (textoBajasEquipoB != null) textoBajasEquipoB.text = bajasEquipoB.ToString();

        if (panelResultadosUI != null)
        {
            panelResultadosUI.SetActive(true);
            Debug.Log("[GestionarResultados] Panel de UI de Resultados inyectado y mostrado con éxito.");
        }
        else
        {
            Debug.LogWarning("[GestionarResultados] El panel visual no se pudo encender because falta su referencia.");
        }
    }

    public void RegresarAlMenuCrearPartida()
    {
        AudioManager.Instancia.CalmarTodosLosEfectos();
        PlayerInmortal jugadorInmortal = Object.FindFirstObjectByType<PlayerInmortal>(FindObjectsInactive.Include);
        if (jugadorInmortal != null)
        {
            jugadorInmortal.gameObject.SetActive(true);
            jugadorInmortal.GetComponent<PlayerInmortal>().enabled = true;

            if (jugadorInmortal.TryGetComponent(out PlayerInput pi))
            {
                pi.enabled = true;
            }

            jugadorInmortal.nombresEquipoA.Clear();
            jugadorInmortal.nombresEquipoB.Clear();

            jugadorInmortal.CambiarEscenaConCarga("CrearPartida", 0.3f);
            AudioManager.Instancia.ReproducirMusicaPorIndice(1);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("CrearPartida");
        }
    }
}