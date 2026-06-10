using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia;

    [Header("Canales de Audio")]
    [SerializeField] AudioSource canalMusica; 
    [SerializeField] AudioSource canalEfectos;
    [SerializeField] AudioSource canalDestrucciones;

    [Header("Control de Volumen")]
    [Range(0f, 1f)][SerializeField] private float volumenMusica = 0.4f;
    [Range(0f, 1f)][SerializeField] private float volumenEfectos = 0.8f;

    [Header("Lista de Músicas de Fondo")]
    [SerializeField] public List<AudioClip> listaMusicas;
    [Header("Lista de Explosiones Aleatorias")]
    [SerializeField] private List<AudioClip> sonidosExplosiones;
    [SerializeField] private List<AudioClip> efectos;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (listaMusicas != null && listaMusicas.Count > 0)
        {
            ReproducirMusicaPorIndice(0);
        }
    }

    void Update()
    {
        if (canalMusica != null) canalMusica.volume = volumenMusica;
        if (canalEfectos != null) canalEfectos.volume = volumenEfectos;
        if (canalDestrucciones != null) canalDestrucciones.volume = volumenEfectos;
    }

    public void PlaySFXDirecto(AudioClip clipParaReproducir)
    {
        if (canalEfectos == null || clipParaReproducir == null) return;
        canalEfectos.PlayOneShot(clipParaReproducir);
    }
    public void PlayDestrucciones(AudioClip clipFinalizacion) {
        if (canalDestrucciones == null || clipFinalizacion == null) return;
        canalDestrucciones.PlayOneShot(clipFinalizacion);
    }
    public void ReproducirMusicaPorIndice(int indice)
    {
        if (canalMusica == null) return;

        if (indice >= 0 && indice < listaMusicas.Count)
        {
            if (listaMusicas[indice] != null)
            {
                canalMusica.Stop();
                canalMusica.clip = listaMusicas[indice];
                canalMusica.loop = true;
                canalMusica.Play();
            }
        }
    }
    public void PlayExplosionAleatoria()
    {
        int indiceAleatorio = Random.Range(0, sonidosExplosiones.Count);
        AudioClip clipSeleccionado = sonidosExplosiones[indiceAleatorio];

        if (clipSeleccionado != null)
        {
            canalEfectos.PlayOneShot(clipSeleccionado);
        }
    }
}