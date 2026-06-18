using System.Collections;
using UnityEngine;

public class SecuenciadorAereo : Arma
{
    [Header("Configuración del Bombardeo")]
    [SerializeField] private GameObject prefabMisilAereo;
    [SerializeField] private float tiempoEntreMisiles = 0.25f;
    [SerializeField] private float distanciaDesplazamientoX = 2.0f;
    [SerializeField] private float alturaCieloY = 15f;

    private void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + alturaCieloY, 0f);
        StartCoroutine(RutinaBombardeoOrdenado());
    }

    private IEnumerator RutinaBombardeoOrdenado()
    {
        // Inyección quirúrgica: Delay de 1 segundo antes de soltar el primer misil
        yield return new WaitForSeconds(1.0f);

        for (int i = 1; i <= rafagas; i++)
        {
            GameObject misilClonado = Instantiate(prefabMisilAereo, transform.position, Quaternion.identity);
            MisilAereo scriptMisil = misilClonado.GetComponent<MisilAereo>();
            if (scriptMisil != null)
            {
                bool esElUltimo = (i == rafagas);
                scriptMisil.ConfigurarMisil(this, esElUltimo);
                if (esElUltimo)
                {
                    camaraMovimiento scriptCam = Camera.main.GetComponent<camaraMovimiento>();
                    if (scriptCam != null)
                    {
                        Debug.Log("[Ataque Aéreo] Enfocando la cámara en el misil final para el desenlace.");
                        scriptCam.EnfocarObjetivo(misilClonado.transform);
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
        base.Usar();
    }
}