using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class siluetaMapa : MonoBehaviour
{
    [Header("Banco de Imágenes de Mapas")]
    public List<Sprite> imagenesMapas = new List<Sprite>();

    [Header("Ajustes de Flotabilidad")]
    [SerializeField] private float velocidadFlotando = 2.5f;
    [SerializeField] private float AmplitudFlotando = 15f;

    private Image miComponenteImage;
    private RectTransform miRectTransform;
    private Vector3 posicionInicialLocal;

    void Awake()
    {
        miComponenteImage = GetComponent<Image>();
        miRectTransform = GetComponent<RectTransform>();

        if (miRectTransform != null)
        {
            posicionInicialLocal = miRectTransform.localPosition;
        }
    }

    void Update()
    {
        if (miRectTransform != null)
        {
            float nuevoY = posicionInicialLocal.y + Mathf.Sin(Time.time * velocidadFlotando) * AmplitudFlotando;
            miRectTransform.localPosition = new Vector3(posicionInicialLocal.x, nuevoY, posicionInicialLocal.z);
        }
    }

    public void CargarImagenMapa(int indiceRecibido)
    {
        if (miComponenteImage == null) miComponenteImage = GetComponent<Image>();

        if (miComponenteImage != null && imagenesMapas.Count > indiceRecibido && imagenesMapas[indiceRecibido] != null)
        {
            miComponenteImage.sprite = imagenesMapas[indiceRecibido];
        }
        else
        {
            Debug.LogWarning($"[siluetaMapa] El índice {indiceRecibido} no tiene una imagen asignada.");
        }
    }
}