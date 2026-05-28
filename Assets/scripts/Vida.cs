using UnityEngine;
using TMPro;

public class Vida : MonoBehaviour
{
    [SerializeField] private float vidaMaxima = 100f;
    [SerializeField] private TextMeshProUGUI textoVidaUI;

    public float vidaActual;

    void Start()
    {
        vidaActual = vidaMaxima;
        ActualizarTextoVida();
    }

    public void RecibirDanio(float cantidad)
    {
        vidaActual = Mathf.Max(vidaActual - cantidad, 0f);
        Debug.Log($"{gameObject.name} recibió {cantidad:F1} de daño. Vida restante: {vidaActual:F1}");
        ActualizarTextoVida();
        if (vidaActual <= 0f)
        {
            Morir();
        }
    }

    private void ActualizarTextoVida()
    {
        if (textoVidaUI != null)
        {
            textoVidaUI.text = Mathf.CeilToInt(vidaActual).ToString();
        }
    }

    private void Morir()
    {
        Debug.Log($"[Vida] {gameObject.name} llegó a 0 de vida. Notificando al TurnoManager.");
        TurnoManager.Instancia.RegistrarMuerteJugador(gameObject);
    }
}