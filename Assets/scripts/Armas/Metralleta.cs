using System.Collections;
using UnityEngine;

public class Metralleta : Arma
{
    [Header("Configuración de la Ráfaga")]
    [SerializeField] private GameObject prefabBalaProyectil;
    [SerializeField] private float tiempoEntreBalas = 0.08f;

    private Vector3 posicionOrigen;
    private Quaternion rotacionOrigen;
    private Vector3 direccionDisparo;

    private void Start()
    {
        if (TurnoManager.Instancia != null && TurnoManager.Instancia.soldadoActivoEnEsteTurno != null)
        {
            float escalaX = TurnoManager.Instancia.soldadoActivoEnEsteTurno.transform.localScale.x;
            if (escalaX < 0)
            {
                transform.Rotate(0f, 180f, 0f);
            }
        }
        direccionDisparo = transform.right.normalized;
        transform.parent = null;
        posicionOrigen = transform.position;
        rotacionOrigen = transform.rotation;
        Usar();
        StartCoroutine(RutinaDisparoInstantaneo());
    }

    private IEnumerator RutinaDisparoInstantaneo()
    {
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.DetenerTemporizadorPorAtaque();
        }
        for (int i = 1; i <= rafagas; i++)
        {
            Debug.Log($"[Metralleta] Inyectando datos y soltando bala {i} de {rafagas}.");
            GameObject balaClonada = Instantiate(prefabBalaProyectil, posicionOrigen, rotacionOrigen);
            AudioManager.Instancia.PlaySFXDirecto(audioInstancia);
            BalaProyectil scriptBala = balaClonada.GetComponent<BalaProyectil>();
            if (scriptBala != null)
            {
                scriptBala.ConfigurarDatosBala(danioMaximo, radioExplosion, radioDanio, fuerzaEmpuje, direccionDisparo);
                if (i == rafagas)
                {
                    scriptBala.AsignarPermisoDeTurno(true);
                }
                else
                {
                    scriptBala.AsignarPermisoDeTurno(false);
                }
            }
            if (i < rafagas)
            {
                yield return new WaitForSeconds(tiempoEntreBalas);
            }
        }
        Destroy(gameObject, 0.1f);
    }

    public override void Usar()
    {
        base.Usar();
    }
}