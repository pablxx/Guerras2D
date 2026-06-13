using System.Collections.Generic;
using UnityEngine;

public class SoldadoVisual : MonoBehaviour
{
    [SerializeField] private List<Sprite> listaSpritesFacciones = new List<Sprite>();

    private SpriteRenderer miSpriteRenderer;

    private void Awake()
    {
        miSpriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void CambiarSpritePorCadena(string nombreFaccion)
    {
        if (miSpriteRenderer == null) miSpriteRenderer = GetComponent<SpriteRenderer>();
        string faccionLimpia = nombreFaccion.Trim();
        int indiceElegido = -1;
        if (faccionLimpia == "Policias")
        {
            indiceElegido = 0;
        }
        else if (faccionLimpia == "Mineros")
        {
            indiceElegido = 1;
        }
        else if (faccionLimpia == "Campesinitos")
        {
            indiceElegido = 2;
        }
        else if (faccionLimpia == "Cholitas")
        {
            indiceElegido = 3;
        }
        if (indiceElegido != -1 && listaSpritesFacciones.Count > indiceElegido && listaSpritesFacciones[indiceElegido] != null)
        {
            miSpriteRenderer.sprite = listaSpritesFacciones[indiceElegido];
        }
        else
        {
            Debug.LogWarning($"[SoldadoVisual] No se encontró un sprite para la facción: '{nombreFaccion}'");
        }
    }
}