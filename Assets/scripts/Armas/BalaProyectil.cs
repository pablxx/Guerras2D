using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalaProyectil : MonoBehaviour
{
    [Header("Físicas del Proyectil")]
    [SerializeField] private float velocidadBala = 25f;
    [SerializeField] private float tiempoMaximoVida = 2f;
    [Header("Efecto del disparo")]
    [SerializeField] private GameObject particulaDisparo;
    [Header("Impacto con terreno")]
    [SerializeField] private GameObject particulaTierra;

    private float danio;
    private int radioExplo;
    private float radioDng;
    private float empuje;
    private Vector3 direccionVuelo;

    private bool volando = false;
    private bool yaImpacto = false;
    private bool tengoPermisoDeTurno = false;
    private List<Vida> vidasProcesadasEnEsteImpacto = new List<Vida>();

    public void ConfigurarDatosBala(float d, int re, float rd, float fe, Vector3 dir)
    {
        danio = d;
        radioExplo = re;
        radioDng = rd;
        empuje = fe;
        direccionVuelo = dir;

        volando = true;

        if (particulaDisparo != null)
        {
            float angulo = Mathf.Atan2(
                direccionVuelo.y,
                direccionVuelo.x) * Mathf.Rad2Deg;

            Quaternion rotacionEfecto = Quaternion.Euler(0, 0, angulo);

            GameObject efecto = Instantiate(
                particulaDisparo,
                transform.position,
                rotacionEfecto);

            Destroy(efecto, 1f);
        }

        Destroy(gameObject, tiempoMaximoVida);
    }

    public void AsignarPermisoDeTurno(bool permiso)
    {
        tengoPermisoDeTurno = permiso;
    }

    private void Update()
    {
        if (!volando || yaImpacto) return;
        transform.Translate(direccionVuelo * velocidadBala * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!volando || yaImpacto || collision.isTrigger) return;
        yaImpacto = true;
        volando = false;

        Vector3 puntoImpacto = transform.position;

        Vida vidaObjetivo = collision.GetComponent<Vida>();
        if (vidaObjetivo == null)
        {
            if (particulaTierra != null)
            {
                GameObject efectoTierra = Instantiate(
                    particulaTierra,
                    puntoImpacto,
                    Quaternion.identity);

                Destroy(efectoTierra, 2f);
            }
        }
        if (vidaObjetivo != null && !vidasProcesadasEnEsteImpacto.Contains(vidaObjetivo))
        {
            vidasProcesadasEnEsteImpacto.Add(vidaObjetivo);
            vidaObjetivo.RecibirDanio(danio);
        }

        var destructor = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();
        if (destructor != null)
        {
            destructor.CambiarTamaño(radioExplo);
            destructor.EjecutarDestruccion(puntoImpacto);
        }
        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (tengoPermisoDeTurno && TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        }
    }
}