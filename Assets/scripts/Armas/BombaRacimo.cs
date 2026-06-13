using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GranadaRacimo : Arma
{
    [Header("Configuración del Racimo (Abanico)")]
    [SerializeField] private GameObject prefabFragmento;
    [SerializeField] private int cantidadFragmentos = 5;

    [SerializeField] private float anguloAperturaTotal = 60f;
    [SerializeField] private float anguloCentral = 90f;

    [SerializeField] private float fuerzaMinima = 8f;
    [SerializeField] private float fuerzaMaxima = 14f;

    bool Explotar = true;
    bool mostrarRadioImpacto = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Explotar == true)
        {
            mostrarRadioImpacto = true;
            Explotar = false;

            if (TurnoManager.Instancia != null)
            {
                TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
            }

            Vector3 puntoImpacto = collision.contacts[0].point;
            transform.position = puntoImpacto;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }

            Collider2D miColisionador = GetComponent<Collider2D>();
            if (miColisionador != null) miColisionador.enabled = false;

            var destructor = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();
            if (destructor != null)
            {
                int tamanoDestruccion = radioExplosion;
                destructor.CambiarTamaño(tamanoDestruccion);
                destructor.EjecutarDestruccion(puntoImpacto);
            }

            List<Vida> vidasProcesadasEnEsteImpacto = new List<Vida>();
            Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(puntoImpacto, radioDanio);

            foreach (Collider2D col in objetosDetectados)
            {
                Vida vidaObjetivo = col.GetComponent<Vida>();
                Rigidbody2D rbGusano = col.GetComponent<Rigidbody2D>();

                float distancia = Vector2.Distance(puntoImpacto, col.transform.position);
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
                    Vector2 direccionEmpuje = col.transform.position - puntoImpacto;
                    direccionEmpuje.Normalize();
                    float fuerzaFinal = fuerzaEmpuje * factorCercania;
                    rbGusano.AddForce(direccionEmpuje * fuerzaFinal, ForceMode2D.Impulse);
                }
            }

            if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
            if (AudioManager.Instancia != null) AudioManager.Instancia.PlayExplosionAleatoria();

            SoltarRacimo();

            Destroy(gameObject, 0.1f);
        }
    }

    void SoltarRacimo()
    {
        if (prefabFragmento == null || cantidadFragmentos <= 0) return;

        float anguloInicio = anguloCentral - (anguloAperturaTotal / 2f);
        float pasoAngulo = (cantidadFragmentos > 1) ? anguloAperturaTotal / (cantidadFragmentos - 1) : 0f;

        for (int i = 0; i < cantidadFragmentos; i++)
        {
            float anguloBase = anguloInicio + (pasoAngulo * i);
            float variacionAleatoria = Random.Range(-2f, 2f);
            float anguloFinal = anguloBase + variacionAleatoria;

            float radianes = anguloFinal * Mathf.Deg2Rad;
            Vector2 direccionSalida = new Vector2(Mathf.Cos(radianes), Mathf.Sin(radianes));

            Vector3 posicionNacimiento = transform.position + (Vector3.up * 0.3f);
            GameObject fragmentoGO = Instantiate(prefabFragmento, posicionNacimiento, Quaternion.identity);

            FragmentoRacimo scriptFragmento = fragmentoGO.GetComponent<FragmentoRacimo>();
            if (scriptFragmento != null)
            {
                bool esElUltimo = (i == cantidadFragmentos - 1);
                scriptFragmento.ConfigurarFragmento(esElUltimo);
            }

            Rigidbody2D rbFrag = fragmentoGO.GetComponent<Rigidbody2D>();
            if (rbFrag != null)
            {
                float fuerza = Random.Range(fuerzaMinima, fuerzaMaxima);
                rbFrag.AddForce(direccionSalida * fuerza, ForceMode2D.Impulse);
                rbFrag.AddTorque(Random.Range(-300f, 300f));
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDanio);
    }

    private void OnDrawGizmos()
    {
        if (mostrarRadioImpacto)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawSphere(transform.position, radioDanio);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radioDanio);
        }
    }

    public override void Usar()
    {
        base.Usar();
        Explotar = true;
    }
}