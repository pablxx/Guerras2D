using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Botella : Arma
{
    [Header("Ejes y Animación Visual")]
    [SerializeField] private float velocidadGiro = 400f;
    [SerializeField] private float anguloInicio = 180f;
    [SerializeField] private float anguloFin = 90f;

    [Header("Configuración del Overlap (Efecto Palanca)")]
    [SerializeField] private float longitudBate = 1.5f;
    [SerializeField] private float radioDeteccionGolpe = 1f;
    [SerializeField] private Vector2 offsetPosicionOverlap = Vector2.zero;
    [SerializeField] private GameObject prefabEfectoGolpe;

    [Header("Físicas")]
    [SerializeField] private float fuerzaEmpujeMelee = 15f;
    [SerializeField] private float elevacionGolpe = 0.4f;

    private int clicsRealizados = 0;
    private bool golpeEnProgreso = false;
    private List<Vida> vidasProcesadasEnEsteGolpe = new List<Vida>();
    private movimientoJugador scriptMovimientoDueno;

    void Start()
    {
        transform.localRotation = Quaternion.Euler(0, 0, anguloInicio);

        if (transform.parent != null)
        {
            scriptMovimientoDueno = transform.parent.GetComponent<movimientoJugador>();
        }

        if (scriptMovimientoDueno != null)
        {
            scriptMovimientoDueno.atacando = false;
        }

        StartCoroutine(RutinaControlAtaques());
    }

    public override void Usar()
    {
        base.Usar();
    }

    private IEnumerator RutinaControlAtaques()
    {
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
        }

        yield return new WaitForSeconds(0.15f);

        while (clicsRealizados < 3)
        {
            if (TurnoManager.Instancia != null)
            {
                TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
            }

            bool clicDetectado = false;
            while (!clicDetectado)
            {
                if (!golpeEnProgreso && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    clicDetectado = true;
                }
                yield return null;
            }

            if (scriptMovimientoDueno != null)
            {
                scriptMovimientoDueno.atacando = true;
            }

            golpeEnProgreso = true;
            clicsRealizados++;
            vidasProcesadasEnEsteGolpe.Clear();
            DetectarImpactosDuranteGolpe();

            float anguloActual = anguloInicio;
            while (Mathf.Abs(anguloActual - anguloFin) > 0.1f)
            {
                anguloActual = Mathf.MoveTowardsAngle(anguloActual, anguloFin, velocidadGiro * Time.deltaTime);
                transform.localRotation = Quaternion.Euler(0, 0, anguloActual);
                yield return null;
            }
            transform.localRotation = Quaternion.Euler(0, 0, anguloFin);
            while (Mathf.Abs(anguloActual - anguloInicio) > 0.1f)
            {
                anguloActual = Mathf.MoveTowardsAngle(anguloActual, anguloInicio, velocidadGiro * Time.deltaTime);
                transform.localRotation = Quaternion.Euler(0, 0, anguloActual);
                yield return null;
            }
            transform.localRotation = Quaternion.Euler(0, 0, anguloInicio);

            if (scriptMovimientoDueno != null && clicsRealizados < 3)
            {
                scriptMovimientoDueno.atacando = false;
            }

            yield return new WaitForSeconds(0.2f);
            golpeEnProgreso = false;
        }

        if (clicsRealizados == 3)
        {
            if (scriptMovimientoDueno != null)
            {
                scriptMovimientoDueno.atacando = false;
            }

            if (TurnoManager.Instancia != null)
            {
                TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
            }
        }

        Destroy(gameObject);
    }

    private void DetectarImpactosDuranteGolpe()
    {
        Vector3 posicionPuntaBate = transform.position
                                    + (transform.right * (longitudBate + offsetPosicionOverlap.x))
                                    + (transform.up * offsetPosicionOverlap.y);

        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(posicionPuntaBate, radioDeteccionGolpe);

        GameObject soldadoActivo = null;
        if (TurnoManager.Instancia != null)
        {
            soldadoActivo = TurnoManager.Instancia.soldadoActivoEnEsteTurno;
        }

        foreach (Collider2D col in objetosDetectados)
        {
            if (soldadoActivo != null && col.gameObject == soldadoActivo) continue;

            Vida vidaObjetivo = col.GetComponent<Vida>();
            Rigidbody2D rbEnemigo = col.GetComponent<Rigidbody2D>();

            if (vidaObjetivo != null && !vidasProcesadasEnEsteGolpe.Contains(vidaObjetivo))
            {
                vidasProcesadasEnEsteGolpe.Add(vidaObjetivo);

                float distanciaALaPunta = Vector2.Distance(posicionPuntaBate, col.transform.position);

                float factorLejanadura = (radioDeteccionGolpe - distanciaALaPunta) / radioDeteccionGolpe;
                factorLejanadura = Mathf.Clamp01(factorLejanadura);

                float danioFinal = danioMaximo * factorLejanadura;
                if (danioFinal > 0)
                {
                    vidaObjetivo.RecibirDanio(danioFinal);
                }

                if (rbEnemigo != null)
                {
                    rbEnemigo.linearVelocity = Vector2.zero;

                    float direccionHorizontal = (col.transform.position.x - transform.position.x) >= 0 ? 1f : -1f;
                    Vector2 vectorEmpuje = new Vector2(direccionHorizontal, elevacionGolpe).normalized;

                    rbEnemigo.AddForce(vectorEmpuje * (fuerzaEmpujeMelee * factorLejanadura), ForceMode2D.Impulse);
                }

                if (prefabEfectoGolpe != null)
                {
                    Instantiate(prefabEfectoGolpe, col.transform.position, Quaternion.identity);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 posicionPuntaBate = transform.position
                                    + (transform.right * (longitudBate + offsetPosicionOverlap.x))
                                    + (transform.up * offsetPosicionOverlap.y);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(posicionPuntaBate, radioDeteccionGolpe);
    }
}