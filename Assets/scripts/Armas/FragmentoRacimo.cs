using System.Collections.Generic;
using UnityEngine;

public class FragmentoRacimo : MonoBehaviour
{
    [Header("Configuración de Daño del Fragmento")]
    [SerializeField] private int radioExplosionMapa = 15;
    [SerializeField] private float radioDanioSoldados = 2f;
    [SerializeField] private float danioMaximo = 15f;
    [SerializeField] private float fuerzaEmpuje = 8f;

    private bool esElUltimoFragmento = false;
    private bool yaExploto = false;

    public void ConfigurarFragmento(bool esUltimo)
    {
        esElUltimoFragmento = esUltimo;
        if (esElUltimoFragmento) gameObject.name = "Fragmento_Final_Controlador";
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (yaExploto) return;
        if (collision.gameObject.layer == gameObject.layer) return;

        yaExploto = true;

        Vector3 puntoImpacto = collision.contacts[0].point;
        var destructor = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();
        if (destructor != null)
        {
            destructor.CambiarTamaño(radioExplosionMapa);
            destructor.EjecutarDestruccion(puntoImpacto);
        }
        List<Vida> vidasProcesadas = new List<Vida>();
        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(puntoImpacto, radioDanioSoldados);

        foreach (Collider2D col in objetosDetectados)
        {
            Vida vidaObjetivo = col.GetComponent<Vida>();
            Rigidbody2D rbGusano = col.GetComponent<Rigidbody2D>();
            // --- BUSCAMOS EL SCRIPT DE ANIMACIÓN DEL ENEMIGO ---
            ControlAnimador animadorEnemigo = col.GetComponentInChildren<ControlAnimador>();
            // ---------------------------------------------------
            float distancia = Vector2.Distance(puntoImpacto, col.transform.position);
            float factorCercania = (radioDanioSoldados - distancia) / radioDanioSoldados;
            factorCercania = Mathf.Clamp01(factorCercania);

            if (vidaObjetivo != null && factorCercania > 0)
            {
                if (vidasProcesadas.Contains(vidaObjetivo)) continue;
                vidasProcesadas.Add(vidaObjetivo);

                float danioFinal = danioMaximo * factorCercania;
                // if (danioFinal > 0) vidaObjetivo.RecibirDanio(danioFinal);
                // --- TRIGER DE DOLOR ---
                if (danioFinal > 0)
                {
                    vidaObjetivo.RecibirDanio(danioFinal);

                    // --- TRIGER DE DOLOR ---
                    if (animadorEnemigo != null)
                    {
                        animadorEnemigo.EjecutarDanio();
                    }
                }
            }

            if (rbGusano != null && factorCercania > 0)
            {
                Vector2 direccionEmpuje = col.transform.position - puntoImpacto;
                direccionEmpuje.Normalize();
                rbGusano.AddForce(direccionEmpuje * (fuerzaEmpuje * factorCercania), ForceMode2D.Impulse);
                //--animacion de empuje---
                float fuerzaFinal = fuerzaEmpuje * factorCercania;
                if (animadorEnemigo != null && fuerzaFinal > 0.5f)
                {
                    animadorEnemigo.ModificarEstadoEmpuje(true);
                }
                //-----------------
            }
        }
        if (AudioManager.Instancia != null) AudioManager.Instancia.PlayExplosionAleatoria();
        if (esElUltimoFragmento && TurnoManager.Instancia != null)
        {
            DesactivarObjetoVisualmente();
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(gameObject));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void DesactivarObjetoVisualmente()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDanioSoldados);
    }
}