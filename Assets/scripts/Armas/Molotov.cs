using System.Collections;
using UnityEngine;

public class SecuenciadorMolotov : Arma
{
    [Header("Configuración del Bombardeo de Fuego")]
    [SerializeField] private GameObject prefabFragmentoFuego;
    [SerializeField] private float tiempoEntreMisiles = 0.25f;
    [SerializeField] private float distanciaDesplazamientoX = 2.0f;
    [SerializeField] private float alturaCieloY = 15f;

    private void Start()
    {
        base.Usar();
        transform.position = new Vector3(transform.position.x, transform.position.y + alturaCieloY, 0f);
        StartCoroutine(RutinaBombardeoOrdenado());
    }

    private IEnumerator RutinaBombardeoOrdenado()
    {
        for (int i = 1; i <= rafagas; i++)
        {
            GameObject fragmentoClonado = Instantiate(prefabFragmentoFuego, transform.position, Quaternion.identity);
            FragmentoFuego scriptFragmento = fragmentoClonado.GetComponent<FragmentoFuego>();

            if (scriptFragmento != null)
            {
                bool esElUltimo = (i == rafagas);
                scriptFragmento.InicializarFragmento(this, esElUltimo, tiempoExplosion);

                if (esElUltimo)
                {
                    camaraMovimiento scriptCam = Camera.main.GetComponent<camaraMovimiento>();
                    if (scriptCam != null)
                    {
                        Debug.Log("[Lluvia Fuego] Enfocando la cámara en el fragmento final para el desenlace.");
                        scriptCam.EnfocarObjetivo(fragmentoClonado.transform);
                    }
                }
            }

            if (i < rafagas)
            {
                yield return new WaitForSeconds(tiempoEntreMisiles);
                transform.position += new Vector3(distanciaDesplazamientoX, 0f, 0f);
            }
        }
    }

    public int ObtenerRadioExplosion() => radioExplosion;
    public float ObtenerRadioDanio() => radioDanio;
    public int ObtenerDanioMaximo() => danioMaximo;
    public float ObtenerFuerzaEmpuje() => fuerzaEmpuje;

    public override void Usar()
    {

    }
}