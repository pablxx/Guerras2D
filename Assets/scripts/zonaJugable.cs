using UnityEngine;

public class zonaJugable : MonoBehaviour
{
    [Header("Puntos de Referencia del Perímetro")]
    [SerializeField] private Transform puntoSuperiorIzquierdo;
    [SerializeField] private float limiteMuerteY = -15f;
    [SerializeField] private Transform puntoInferiorDerecho;

    private float minX, maxX, minY, maxY;

    void Start()
    {
        CalcularLimitesPerimetro();
    }

    void Update()
    {
        if (TurnoManager.Instancia == null) return;
        for (int i = TurnoManager.Instancia.ListaSoldadosA.Count - 1; i >= 0; i--)
        {
            GameObject soldado = TurnoManager.Instancia.ListaSoldadosA[i];
            if (soldado != null && soldado.activeInHierarchy)
            {
                if (EstaFueraDelPerimetro(soldado.transform.position))
                {
                    ProcesarCaidaSoldado(soldado);
                }
            }
        }
        for (int i = TurnoManager.Instancia.ListaSoldadosB.Count - 1; i >= 0; i--)
        {
            GameObject soldado = TurnoManager.Instancia.ListaSoldadosB[i];
            if (soldado != null && soldado.activeInHierarchy)
            {
                if (EstaFueraDelPerimetro(soldado.transform.position))
                {
                    ProcesarCaidaSoldado(soldado);
                }
            }
        }
        Arma armaEnEscena = Object.FindFirstObjectByType<Arma>();
        if (armaEnEscena != null)
        {
            if (EstaFueraDelPerimetro(armaEnEscena.transform.position))
            {
                ProcesarCaidaArma(armaEnEscena.gameObject);
            }
        }
    }

    void CalcularLimitesPerimetro()
    {
        if (puntoSuperiorIzquierdo == null || puntoInferiorDerecho == null)
        {
            Debug.LogError("[zonaJugable] ¡Faltan asignar los Transforms de referencia en el inspector!");
            return;
        }
        minX = Mathf.Min(puntoSuperiorIzquierdo.position.x, puntoInferiorDerecho.position.x);
        maxX = Mathf.Max(puntoSuperiorIzquierdo.position.x, puntoInferiorDerecho.position.x);
        minY = Mathf.Min(puntoSuperiorIzquierdo.position.y, puntoInferiorDerecho.position.y);
        maxY = Mathf.Max(puntoSuperiorIzquierdo.position.y, puntoInferiorDerecho.position.y);
    }
    bool EstaFueraDelPerimetro(Vector3 posicion)
    {
        return posicion.x < minX || posicion.x > maxX || posicion.y < minY || posicion.y > maxY;
    }

    void ProcesarCaidaSoldado(GameObject soldado)
    {
        if (soldado == null) return;
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.RegistrarMuerteJugador(soldado);
        }
    }

    void ProcesarCaidaArma(GameObject arma)
    {
        Debug.Log($"[zonaJugable] El proyectil {arma.name} abandonó el mapa de juego.");

        TurnoManager.Instancia.StartCoroutine(TurnoManager.Instancia.TemporizadorCambioTurno(null));
        Destroy(arma);
    }

    private void OnDrawGizmos()
    {
        if (puntoSuperiorIzquierdo != null && puntoInferiorDerecho != null)
        {
            float sMinX = Mathf.Min(puntoSuperiorIzquierdo.position.x, puntoInferiorDerecho.position.x);
            float sMaxX = Mathf.Max(puntoSuperiorIzquierdo.position.x, puntoInferiorDerecho.position.x);
            float sMinY = Mathf.Min(puntoSuperiorIzquierdo.position.y, puntoInferiorDerecho.position.y);
            float sMaxY = Mathf.Max(puntoSuperiorIzquierdo.position.y, puntoInferiorDerecho.position.y);

            Vector3 esquinaSuperiorDerecha = new Vector3(sMaxX, sMaxY, 0f);
            Vector3 esquinaInferiorIzquierda = new Vector3(sMinX, sMinY, 0f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(puntoSuperiorIzquierdo.position, esquinaSuperiorDerecha);
            Gizmos.DrawLine(esquinaSuperiorDerecha, puntoInferiorDerecho.position);
            Gizmos.DrawLine(puntoInferiorDerecho.position, esquinaInferiorIzquierda);
            Gizmos.DrawLine(esquinaInferiorIzquierda, puntoSuperiorIzquierdo.position);
        }
    }
}