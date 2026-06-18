using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistola : Arma
{
    [SerializeField] float velocidadBala = 15f;
    [Header("Efecto del disparo")]
    [SerializeField] GameObject particulaDisparo;
    [Header("Impacto con terreno")]
    [SerializeField] GameObject particulaTierra;
    [Header("Duración máxima del proyectil")]
    [SerializeField] float tiempoMaximoVida = 2f;
    [Header("Tiempo de espera entre cada disparo")]
    [SerializeField] float tiempoEntreBalas = 0.15f;


    private Vector3 posicionOrigenMemorizada;
    private Quaternion rotacionOrigenMemorizada;
    private Vector3 direccionDisparoMemorizada;
    private bool volando = false;
    private bool cicloBalaCompletado = false;
    private bool configuracionInicialLista = false;
    private Coroutine rutinaTiempoVida;
    private SpriteRenderer miSpriteRenderer;
    private List<Vida> vidasProcesadasEnEsteTiro = new List<Vida>();

    private void Awake()
    {
        miSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void Usar()
    {
        base.Usar();
    }

    public void ConfigurarDisparoInicial(Vector3 posicion, Quaternion rotacion, Vector3 direccion)
    {
        posicionOrigenMemorizada = posicion;
        rotacionOrigenMemorizada = rotacion;
        direccionDisparoMemorizada = direccion.normalized;

        // Inyección quirúrgica: Volteamos el eje Y del propio padre según hacia dónde mire el gusanito activo
        if (TurnoManager.Instancia != null && TurnoManager.Instancia.soldadoActivoEnEsteTurno != null)
        {
            Vector3 escalaActual = transform.localScale;

            if (TurnoManager.Instancia.soldadoActivoEnEsteTurno.transform.localScale.x < 0)
            {
                // Espejo en Y para corregir el visual cuando apunta a la izquierda
                transform.localScale = new Vector3(Mathf.Abs(escalaActual.x), -Mathf.Abs(escalaActual.y), escalaActual.z);
            }
            else
            {
                // Escala normal cuando apunta a la derecha
                transform.localScale = new Vector3(Mathf.Abs(escalaActual.x), Mathf.Abs(escalaActual.y), escalaActual.z);
            }
        }

        configuracionInicialLista = true;
    }

    private void Start()
    {
        transform.parent = null;
        StartCoroutine(MecanicaRafagaLimpia());
    }

    private IEnumerator MecanicaRafagaLimpia()
    {
        while (!configuracionInicialLista) yield return null;
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
        }
        for (int i = 0; i < rafagas; i++)
        {
            Debug.Log($"[Bala] Disparo ráfaga {i + 1} de {rafagas}");
            transform.position = posicionOrigenMemorizada;
            transform.rotation = rotacionOrigenMemorizada;
            if (particulaDisparo != null)
            {
                float angulo = Mathf.Atan2(
    direccionDisparoMemorizada.y,
    direccionDisparoMemorizada.x) * Mathf.Rad2Deg;

                Quaternion rotacionEfecto = Quaternion.Euler(0, 0, angulo);

                GameObject efecto = Instantiate(
                    particulaDisparo,
                    posicionOrigenMemorizada,
                    rotacionEfecto);

                Destroy(efecto, 1f);
            }
            if (miSpriteRenderer != null)
            {
                miSpriteRenderer.enabled = true;
            }
            vidasProcesadasEnEsteTiro.Clear();
            Usar();
            cicloBalaCompletado = false;
            volando = true;
            rutinaTiempoVida = StartCoroutine(ControlTiempoVida(tiempoMaximoVida));
            while (!cicloBalaCompletado)
            {
                yield return null;
            }
            if (i < rafagas - 1)
            {
                yield return new WaitForSeconds(tiempoEntreBalas);
            }
        }
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        }
        Destroy(gameObject);
    }

    void Update()
    {
        if (!volando) return;
        transform.Translate(direccionDisparoMemorizada * velocidadBala * Time.deltaTime, Space.World);
    }

    private IEnumerator ControlTiempoVida(float segundos)
    {
        yield return new WaitForSeconds(segundos);

        if (volando)
        {
            Debug.Log("[Bala] Tiempo de vuelo agotado. Ocultando y retornando al origen.");
            TerminarTiroActual();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!volando || collision.isTrigger) return;
        if (rutinaTiempoVida != null)
        {
            StopCoroutine(rutinaTiempoVida);
        }
        Vector3 puntoImpacto = transform.position;
        if (collision.GetComponent<Vida>() == null)
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
        Vida vidaObjetivo = collision.GetComponent<Vida>();
        if (vidaObjetivo != null)
        {
            if (!vidasProcesadasEnEsteTiro.Contains(vidaObjetivo))
            {
                vidasProcesadasEnEsteTiro.Add(vidaObjetivo);
                vidaObjetivo.RecibirDanio(danioMaximo);
                ControlAnimador animadorEnemigo = collision.GetComponentInChildren<ControlAnimador>();
                if (animadorEnemigo != null)
                {
                    animadorEnemigo.EjecutarDanio();
                }
            }
        }
        var destructor = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();
        if (destructor != null)
        {
            destructor.CambiarTamaño(radioExplosion);
            destructor.EjecutarDestruccion(puntoImpacto);
        }
        TerminarTiroActual();
    }

    private void TerminarTiroActual()
    {
        if (miSpriteRenderer != null)
        {
            miSpriteRenderer.enabled = false;
        }
        volando = false;
        cicloBalaCompletado = true;
    }
}