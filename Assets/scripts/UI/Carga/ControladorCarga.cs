using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ControladorCarga : MonoBehaviour
{
    [Header("UI de Carga")]
    [SerializeField] private Image barraProgreso;
    [SerializeField] private TextMeshProUGUI textoPorcentaje;

    [Header("Configuracion de Tiempo")]
    [SerializeField] private float tiempoMinimoCarga = 1.5f;

    private void Start()
    {
        PlayerInmortal jugador = Object.FindFirstObjectByType<PlayerInmortal>();

        if (jugador != null && !string.IsNullOrEmpty(jugador.escenaObjetivo))
        {
    
            float tiempoAjustado = jugador.tiempoCargaPersonalizado > 0f ? jugador.tiempoCargaPersonalizado : 1.5f;

            StartCoroutine(RutinaCargaAsincrona(jugador.escenaObjetivo, tiempoAjustado));
        }
        else
        {
            SceneManager.LoadScene("Inicio");
        }
    }

    private IEnumerator RutinaCargaAsincrona(string nombreEscena, float tiempoMinimoCarga)
    {
        AsyncOperation operacion = SceneManager.LoadSceneAsync(nombreEscena);
        operacion.allowSceneActivation = false;

        float tiempoTranscurrido = 0f;

        while (!operacion.isDone)
        {
            tiempoTranscurrido += Time.deltaTime;

            float progresoTiempo = Mathf.Clamp01(tiempoTranscurrido / tiempoMinimoCarga);
            float progresoUnity = Mathf.Clamp01(operacion.progress / 0.9f);

            float progresoVisual = Mathf.Min(progresoTiempo, progresoUnity);

            if (barraProgreso != null)
            {
                barraProgreso.fillAmount = progresoVisual;
            }

            if (textoPorcentaje != null)
            {
                textoPorcentaje.text = $"Cargando... {(progresoVisual * 100f):F0}%";
            }

            if (operacion.progress >= 0.9f && tiempoTranscurrido >= tiempoMinimoCarga)
            {
                operacion.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}