using System.Collections;
using UnityEngine;

public class Teletransporte : Arma
{
    [Header("Efectos Visuales")]
    [SerializeField] private float retrasoAparicion = 0.3f;

    [Header("Validación de Terreno")]
    [SerializeField] private LayerMask capaTerreno;
    [SerializeField] private float radioCuerpo = 0.5f;
    [SerializeField] private float rangoEscaneoHaciaArriba = 5f;
    [SerializeField] AudioClip audioFinalizar;

    private void Start()
    {
        StartCoroutine(RutinaTeletransporte());
    }

    private IEnumerator RutinaTeletransporte()
    {
        if (TurnoManager.Instancia != null && TurnoManager.Instancia.soldadoActivoEnEsteTurno != null)
        {
            GameObject gusanito = TurnoManager.Instancia.soldadoActivoEnEsteTurno;
            Rigidbody2D rbGusanito = gusanito.GetComponent<Rigidbody2D>();

            Vector3 posicionDestino = transform.position;
            Collider2D golpeTerreno = Physics2D.OverlapCircle(posicionDestino, radioCuerpo, capaTerreno);

            if (golpeTerreno != null)
            {
                TurnoManager.Instancia.MostrarNotificacion("Buscando superficie libre...");

                bool encontroEspacioLibre = false;
                for (float offsetY = 0.2f; offsetY <= rangoEscaneoHaciaArriba; offsetY += 0.2f)
                {
                    Vector3 posicionPrueba = posicionDestino + new Vector3(0f, offsetY, 0f);
                    Collider2D pruebaTerreno = Physics2D.OverlapCircle(posicionPrueba, radioCuerpo, capaTerreno);

                    if (pruebaTerreno == null)
                    {
                        posicionDestino = posicionPrueba;
                        encontroEspacioLibre = true;
                        break;
                    }
                }
                if (!encontroEspacioLibre)
                {
                    TurnoManager.Instancia.MostrarNotificacion("¡ERROR! Destino inválido bajo tierra.");
                    yield return new WaitForSeconds(1.5f);
                    TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
                    Destroy(gameObject);
                    yield break;
                }
            }
            if (TurnoManager.Instancia != null)
            {
                
                TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
                TurnoManager.Instancia.MostrarNotificacion("¡Teletransporte completado!");               
            }
            if (rbGusanito != null)
            {
                rbGusanito.linearVelocity = Vector2.zero;
                rbGusanito.bodyType = RigidbodyType2D.Kinematic;
            }
            yield return new WaitForSeconds(retrasoAparicion);
            gusanito.transform.position = posicionDestino;
            if (rbGusanito != null)
            {
                rbGusanito.bodyType = RigidbodyType2D.Dynamic;
            }
            yield return new WaitForSeconds(0.5f);
            TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        }
        AudioManager.Instancia.PlayDestrucciones(audioFinalizar);
        Destroy(gameObject, 0.1f);
    }

    public override void Usar()
    {
        base.Usar();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioCuerpo);
    }
}