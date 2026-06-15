using UnityEngine;
using UnityEngine.UI;

public class MoverFondo : MonoBehaviour
{
    [SerializeField] private RawImage img;
    [SerializeField] private float velocidadMaxima = 0.2f;

    private float velocidadX;
    private float velocidadY;
    private float cronometro;

    void Start()
    {
        AsignarDireccionAleatoria();
    }

    void Update()
    {
        cronometro += Time.deltaTime;

        if (cronometro >= 0.7f)
        {
            AsignarDireccionAleatoria();
            cronometro = 0f;
        }

        if (img != null)
        {
            img.uvRect = new Rect(img.uvRect.position + new Vector2(velocidadX, velocidadY) * Time.deltaTime, img.uvRect.size);
        }
    }

    private void AsignarDireccionAleatoria()
    {
        velocidadX = Random.Range(-velocidadMaxima, velocidadMaxima);
        velocidadY = Random.Range(-velocidadMaxima, velocidadMaxima);
    }
}