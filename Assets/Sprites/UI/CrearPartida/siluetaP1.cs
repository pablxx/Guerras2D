using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class siluetaP1 : MonoBehaviour
{
    [Header("Conectar al Prefab Animado")]
    [SerializeField] private SpriteRenderer rendererGorra;
    [SerializeField] private SpriteRenderer rendererRopa;

    [Header("Catálogo de Ropa (Ordenado)")]
    public Sprite[] opcionesGorras;
    public Sprite[] opcionesRopas;

    // ¡MANTENEMOS la misma función para no romper los botones de tu equipo!
    public void CargarImagen(int indiceRecibido)
    {
        // En lugar de cambiar el Canvas, cambiamos los sprites de tu Prefab animado

        if (rendererGorra != null && opcionesGorras != null && indiceRecibido < opcionesGorras.Length)
        {
            rendererGorra.sprite = opcionesGorras[indiceRecibido];
        }

        if (rendererRopa != null && opcionesRopas != null && indiceRecibido < opcionesRopas.Length)
        {
            rendererRopa.sprite = opcionesRopas[indiceRecibido];
        }
        // 2. MAGIA DEL SALUDO ALEATORIO
        if (rendererGorra != null)
        {
            // Buscamos el Animator en el objeto "Padre" (donde está el cuerpo entero)
            Animator anim = rendererGorra.GetComponentInParent<Animator>();

            if (anim != null)
            {
                // Random.Range con enteros en Unity incluye el primer número pero EXCLUYE el último.
                // Por eso ponemos (0, 3) para que nos dé como resultado: 0, 1 o 2.
                int saludoAleatorio = Random.Range(0, 3);

                // Le enviamos el número al Animator y jalamos el gatillo
                anim.SetInteger("TipoSaludo", saludoAleatorio);
                anim.SetTrigger("HacerSaludos");
            }
        }

        // ¡Opcional! Si tu personaje tiene un Animator en el padre, puedes hacerlo festejar aquí
        // Animator anim = rendererGorra.transform.parent.GetComponent<Animator>();
        // if (anim != null) anim.SetTrigger("CambioRopa");

    }

    //public List<Sprite> SiluetasPersonajes = new List<Sprite>();
    //private Image miComponenteImage;

    //void Awake()
    //{
    //    miComponenteImage = GetComponent<Image>();
    //}

    //public void CargarImagen(int indiceRecibido)
    //{
    //    if (miComponenteImage == null) miComponenteImage = GetComponent<Image>();
    //    if (miComponenteImage != null && SiluetasPersonajes.Count > indiceRecibido && SiluetasPersonajes[indiceRecibido] != null)
    //    {
    //        miComponenteImage.sprite = SiluetasPersonajes[indiceRecibido];
    //    }

    //}
}