using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AnimacionLogo : MonoBehaviour
{
    [Header("UI Image")]
    [SerializeField] private Image logoImage;

    [Header("Frames (Sprites de la hoja)")]
    [SerializeField] private Sprite[] frames;

    [Header("Animación")]
    [SerializeField] private float tiempoPorFrame = 0.1f;

    [Header("Escala final")]
    [SerializeField] private Vector3 escalaFinal = new Vector3(1.3f, 1.3f, 1f);
    [SerializeField] private float duracionEscala = 0.3f;

    private Vector3 escalaInicial;

    private void Start()
    {
        escalaInicial = logoImage.rectTransform.localScale;
        StartCoroutine(AnimarSprites());
    }

    private IEnumerator AnimarSprites()
    {
        for (int i = 0; i < frames.Length; i++)
        {
            logoImage.sprite = frames[i];
            yield return new WaitForSeconds(tiempoPorFrame);
        }

        logoImage.sprite = frames[frames.Length - 1];

        float t = 0f;

        while (t < duracionEscala)
        {
            t += Time.deltaTime;

            float p = t / duracionEscala;

            logoImage.rectTransform.localScale = Vector3.Lerp(
                escalaInicial,
                escalaFinal,
                p
            );

            yield return null;
        }

        logoImage.rectTransform.localScale = escalaFinal;

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene("Inicio");
    }
}