using TMPro;
using UnityEngine;

public enum TipoEquipo { 
    EquipoA, 
    EquipoB }
public class DatosJugador : MonoBehaviour
{
    
    public TipoEquipo equipoJugador;
    [SerializeField] private TextMeshProUGUI textoNombreUI;

    public void ConfigurarTextoCanvas(string textoNombre, Color colorEquipo)
    {
        if (textoNombreUI != null)
        {
            textoNombreUI.text = textoNombre;
            textoNombreUI.color = colorEquipo;
        }
        else
        {
            Debug.LogWarning($"[DatosJugador] No has asignado la referencia del texto de la UI en {gameObject.name}");
        }
    }

}