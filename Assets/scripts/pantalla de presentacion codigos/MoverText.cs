using System.Collections;
using TMPro;
using UnityEngine;

public class ListaTextosAnimados : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private RectTransform[] textos;

    [Header("Movimiento")]
    [SerializeField] private float desplazamientoX = 600f;

    [Header("Tiempo")]
    [SerializeField] private float duracion = 1f;
    [SerializeField] private float retrasoEntreTextos = 0.2f;

    private void Start()
    {
        for (int i = 0; i < textos.Length; i++)
        {
            StartCoroutine(MoverTexto(textos[i], i * retrasoEntreTextos));
        }
    }

    private IEnumerator MoverTexto(RectTransform texto, float retraso)
    {
        Vector2 posicionInicial = texto.anchoredPosition;

        yield return new WaitForSeconds(retraso);

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracion;

            float offsetX = Mathf.Lerp(0, desplazamientoX, t);

            texto.anchoredPosition = new Vector2(
                posicionInicial.x + offsetX,
                posicionInicial.y
            );

            yield return null;
        }

        texto.anchoredPosition = new Vector2(
            posicionInicial.x + desplazamientoX,
            posicionInicial.y
        );
    }
}
