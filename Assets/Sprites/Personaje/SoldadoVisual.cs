using System.Collections.Generic;
using UnityEngine;

public class SoldadoVisual : MonoBehaviour
{
    [Header("Referencias del Cuerpo")]
    [SerializeField] private List<SpriteRenderer> piezasDelCuerpo = new List<SpriteRenderer>();

    [Header("Colecciones de Sprites")]
    [SerializeField] private List<Sprite> spritesPolicias = new List<Sprite>();
    [SerializeField] private List<Sprite> spritesMineros = new List<Sprite>();
    [SerializeField] private List<Sprite> spritesCampesinitos = new List<Sprite>();
    [SerializeField] private List<Sprite> spritesCholitas = new List<Sprite>();

    public void CambiarSpritePorCadena(string nombreFaccion)
    {
        string faccionLimpia = nombreFaccion.Trim();
        List<Sprite> listaSpritesElegida = null;
        if (faccionLimpia == "Policias")
        {
            listaSpritesElegida = spritesPolicias;
        }
        else if (faccionLimpia == "Mineros")
        {
            listaSpritesElegida = spritesMineros;
        }
        else if (faccionLimpia == "Campesinitos")
        {
            listaSpritesElegida = spritesCampesinitos;
        }
        else if (faccionLimpia == "Cholitas")
        {
            listaSpritesElegida = spritesCholitas;
        }

        if (listaSpritesElegida != null && listaSpritesElegida.Count > 0)
        {
            for (int i = 0; i < piezasDelCuerpo.Count; i++)
            {
                if (piezasDelCuerpo[i] != null && i < listaSpritesElegida.Count && listaSpritesElegida[i] != null)
                {
                    piezasDelCuerpo[i].sprite = listaSpritesElegida[i];
                }
                else
                {
                    Debug.LogWarning($"[SoldadoVisual] Omisión o discrepancia en el elemento índice {i} para la facción '{faccionLimpia}'.");
                }
            }
            Debug.Log($"[SoldadoVisual] ¡Éxito! Aspecto completo cambiado a la facción: '{faccionLimpia}'");
        }
        else
        {
            Debug.LogError($"[SoldadoVisual] Error crítico: No se encontraron sprites asignados o no existe la facción: '{nombreFaccion}'");
        }
    }
}