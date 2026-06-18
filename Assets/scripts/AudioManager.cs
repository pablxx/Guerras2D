using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia;

    public enum EstadoJuego { UI, Jugando }
    [Header("Estado Actual del Sistema")]
    public EstadoJuego estadoActual = EstadoJuego.UI;

    [Header("Canales de Audio")]
    [SerializeField] AudioSource canalMusica;
    [SerializeField] AudioSource canalEfectos;
    [SerializeField] AudioSource canalDestrucciones;
    [SerializeField] AudioSource canalVoces;
    [SerializeField] AudioSource canalExplosiones;

    [Header("Control de Volumen")]
    [Range(0f, 1f)][SerializeField] private float volumenMusica = 0.4f;
    [Range(0f, 1f)][SerializeField] private float volumenEfectos = 0.8f;
    [Range(0f, 1f)][SerializeField] private float volumenVoces = 0.8f;
    [Range(0f, 1f)][SerializeField] private float volumenExplosiones = 0.8f;

    [Header("Lista de Músicas de Fondo")]
    [SerializeField] public List<AudioClip> listaMusicas;
    [Header("Lista de Explosiones Aleatorias")]
    [SerializeField] private List<AudioClip> sonidosExplosiones;
    [Header("Lista de Efectos de Juego")]
    [SerializeField] private List<AudioClip> efectos;
    [Header("Lista de Efectos UI")]
    [SerializeField] private List<AudioClip> efectosUI;

    [Header("Sistema de Voces del Soldado")]
    [SerializeField] private List<AudioClip> vocesConfirmacionTurno;
    [SerializeField] private List<AudioClip> vocesDanio;
    [SerializeField] private List<AudioClip> vocesFinTurno;
    [SerializeField] private List<AudioClip> vocesFinalTiempo;
    [SerializeField] private List<AudioClip> vocesSalto;
    [SerializeField] private List<AudioClip> vocesMuerte;

    private float tiempoSiguienteVozDanio = 0f;

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

    }

    void Update()
    {
        if (canalMusica != null) canalMusica.volume = volumenMusica;
        if (canalEfectos != null) canalEfectos.volume = volumenEfectos;
        if (canalDestrucciones != null) canalDestrucciones.volume = volumenEfectos;
        if (canalVoces != null) canalVoces.volume = volumenVoces;
        if (canalExplosiones != null) canalExplosiones.volume = volumenExplosiones;
    }

    public void PlaySonidoSalto(int indice)
    {
        if (canalVoces == null || vocesSalto == null || vocesSalto.Count == 0) return;

        AudioClip clipSeleccionado = null;

        if (indice >= 0 && indice < vocesSalto.Count)
        {
            clipSeleccionado = vocesSalto[indice];
        }
        else
        {
            int indiceAleatorio = Random.Range(0, vocesSalto.Count);
            clipSeleccionado = vocesSalto[indiceAleatorio];
        }

        if (clipSeleccionado != null)
        {
            canalVoces.PlayOneShot(clipSeleccionado);
        }
    }

    public void CambiarEstado(EstadoJuego nuevoEstado)
    {
        estadoActual = nuevoEstado;
        Debug.Log($"[AudioManager] Estado cambiado a: {nuevoEstado}");

        if (nuevoEstado == EstadoJuego.Jugando)
        {
            ReproducirMusicaPorIndice(1);
        }
        else if (nuevoEstado == EstadoJuego.UI)
        {
            ReproducirMusicaPorIndice(0);
        }
    }

    public void PlaySFXPorIndice(int indice)
    {
        if (canalEfectos == null || efectos == null) return;

        if (indice >= 0 && indice < efectos.Count)
        {
            if (efectos[indice] != null)
            {
                canalEfectos.PlayOneShot(efectos[indice]);
            }
        }
    }

    public void PlayUIPorIndice(int indice)
    {
        if (canalEfectos == null || efectosUI == null) return;

        if (indice >= 0 && indice < efectosUI.Count)
        {
            if (efectosUI[indice] != null)
            {
                canalEfectos.PlayOneShot(efectosUI[indice]);
            }
        }
    }

    public void PlaySFXDirecto(AudioClip clipParaReproducir)
    {
        if (canalEfectos == null || clipParaReproducir == null) return;
        canalEfectos.PlayOneShot(clipParaReproducir);
    }

    public void PlayDestrucciones(AudioClip clipFinalizacion)
    {
        if (canalDestrucciones == null || clipFinalizacion == null) return;
        canalDestrucciones.PlayOneShot(clipFinalizacion);
    }

    public void ReproducirVozConfirmacionAleatoria()
    {
        if (canalVoces == null || vocesConfirmacionTurno == null || vocesConfirmacionTurno.Count == 0) return;

        int indiceAleatorio = Random.Range(0, vocesConfirmacionTurno.Count);
        AudioClip clipSeleccionado = vocesConfirmacionTurno[indiceAleatorio];

        if (clipSeleccionado != null)
        {
            canalVoces.PlayOneShot(clipSeleccionado);
        }
    }

    public void ReproducirVozDanioAleatoria()
    {
        if (Time.time < tiempoSiguienteVozDanio) return;

        if (canalVoces == null || vocesDanio == null || vocesDanio.Count == 0) return;

        int indiceAleatorio = Random.Range(0, vocesDanio.Count);
        AudioClip clipSeleccionado = vocesDanio[indiceAleatorio];

        if (clipSeleccionado != null)
        {
            canalVoces.PlayOneShot(clipSeleccionado);
            tiempoSiguienteVozDanio = Time.time + 1.0f;
        }
    }

    public void ReproducirVozFinTurnoAleatoria()
    {
        if (canalVoces == null || vocesFinTurno == null || vocesFinTurno.Count == 0) return;

        int indiceAleatorio = Random.Range(0, vocesFinTurno.Count);
        AudioClip clipSeleccionado = vocesFinTurno[indiceAleatorio];

        if (clipSeleccionado != null)
        {
            canalVoces.PlayOneShot(clipSeleccionado);
        }
    }

    public void ReproducirVozFinalTiempoAleatoria()
    {
        if (canalVoces == null || vocesFinalTiempo == null || vocesFinalTiempo.Count == 0) return;

        int indiceAleatorio = Random.Range(0, vocesFinalTiempo.Count);
        AudioClip clipSeleccionado = vocesFinalTiempo[indiceAleatorio];

        if (clipSeleccionado != null)
        {
            canalVoces.PlayOneShot(clipSeleccionado);
        }
    }

    public void ReproducirVozMuerteAleatoria()
    {
        if (canalVoces == null || vocesMuerte == null || vocesMuerte.Count == 0) return;

        int indiceAleatorio = Random.Range(0, vocesMuerte.Count);
        AudioClip clipSeleccionado = vocesMuerte[indiceAleatorio];

        if (clipSeleccionado != null)
        {
            canalVoces.PlayOneShot(clipSeleccionado);
        }
    }

    public void ReproducirMusicaPorIndice(int indice)
    {
        if (canalMusica == null) return;

        if (indice >= 0 && indice < listaMusicas.Count)
        {
            if (listaMusicas[indice] != null)
            {
                if (canalMusica.clip == listaMusicas[indice] && canalMusica.isPlaying)
                {
                    return;
                }

                canalMusica.Stop();
                canalMusica.clip = listaMusicas[indice];
                canalMusica.loop = true;
                canalMusica.Play();
            }
        }
    }

    // AudioManager.Instancia.PlayExplosionAleatoria()
    public void PlayExplosionAleatoria()
    {
        if (canalExplosiones == null || sonidosExplosiones == null || sonidosExplosiones.Count == 0) return;

        int indiceAleatorio = Random.Range(0, sonidosExplosiones.Count);
        AudioClip clipSeleccionado = sonidosExplosiones[indiceAleatorio];

        if (clipSeleccionado != null)
        {
            canalExplosiones.PlayOneShot(clipSeleccionado);
        }
    }

    public void CalmarTodosLosEfectos()
    {
        if (canalEfectos != null) canalEfectos.Stop();
        if (canalDestrucciones != null) canalDestrucciones.Stop();
        if (canalMusica != null) canalMusica.Stop();
        if (canalVoces != null) canalVoces.Stop();
        if (canalExplosiones != null) canalExplosiones.Stop();
    }

    public void ReproducirMusicaJuegoAleatoria()
    {
        if (canalMusica == null || listaMusicas == null || listaMusicas.Count <= 1) return;

        int indiceAleatorio = Random.Range(2, listaMusicas.Count);

        if (listaMusicas[indiceAleatorio] != null)
        {
            canalMusica.Stop();
            canalMusica.clip = listaMusicas[indiceAleatorio];
            canalMusica.loop = true;
            canalMusica.Play();
        }
    }
}