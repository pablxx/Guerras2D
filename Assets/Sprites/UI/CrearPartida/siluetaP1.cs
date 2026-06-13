using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class siluetaP1 : MonoBehaviour
{
    public List<Sprite> SiluetasPersonajes = new List<Sprite>();
    private Image miComponenteImage;

    void Awake()
    {
        miComponenteImage = GetComponent<Image>();
    }

    public void CargarImagen(int indiceRecibido)
    {
        if (miComponenteImage == null) miComponenteImage = GetComponent<Image>();
        if (miComponenteImage != null && SiluetasPersonajes.Count > indiceRecibido && SiluetasPersonajes[indiceRecibido] != null)
        {
            miComponenteImage.sprite = SiluetasPersonajes[indiceRecibido];
        }
        
    }
}