using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granada : Arma
{
    private bool yaExplotó = false;

    public override void Usar()
    {
        StartCoroutine(Temporizador1());
    }

    public void CrearDanio(Vector3 puntoDeImpacto)
    {
        if (yaExplotó == true) return;
        yaExplotó = true;
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
        }
        var destructor = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();
        if (destructor != null)
        {
            destructor.CambiarTamaño(radioExplosion);
            destructor.EjecutarDestruccion(puntoDeImpacto);
        }

        List<Vida> vidasProcesadasEnEsteImpacto = new List<Vida>();
        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(puntoDeImpacto, radioDanio);
        foreach (Collider2D col in objetosDetectados)
        {
            Vida vidaObjetivo = col.GetComponent<Vida>();
            Rigidbody2D rbGusano = col.GetComponent<Rigidbody2D>();

            float distancia = Vector2.Distance(puntoDeImpacto, col.transform.position);
            float factorCercania = (radioDanio - distancia) / radioDanio;
            factorCercania = Mathf.Clamp01(factorCercania);

            if (vidaObjetivo != null && factorCercania > 0)
            {
                if (vidasProcesadasEnEsteImpacto.Contains(vidaObjetivo)) continue;
                vidasProcesadasEnEsteImpacto.Add(vidaObjetivo);
                float danioFinal = danioMaximo * factorCercania;
                if (danioFinal > 0)
                {
                    vidaObjetivo.RecibirDanio(danioFinal);
                }
            }
            if (rbGusano != null && factorCercania > 0)
            {
                Vector2 direccionEmpuje = (Vector2)col.transform.position - (Vector2)puntoDeImpacto;
                direccionEmpuje.Normalize();
                float fuerzaFinal = fuerzaEmpuje * factorCercania;
                rbGusano.AddForce(direccionEmpuje * fuerzaFinal, ForceMode2D.Impulse);
            }
        }
        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        Rigidbody2D rbGranada = GetComponent<Rigidbody2D>();
        if (rbGranada != null) rbGranada.bodyType = RigidbodyType2D.Static;
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        }
        AudioManager.Instancia.PlayExplosionAleatoria();
        Destroy(gameObject, 0.1f);
    }

    private IEnumerator Temporizador1()
    {
        yield return new WaitForSeconds(tiempoExplosion);
        CrearDanio(transform.position);
    }
    private void OnDestroy()
    {
        yaExplotó = true;
    }
}