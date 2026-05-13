/*using UnityEngine;
using DTerrain;

public class Proyectil : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Buscamos el destructor en la escena
        var destructor = Object.FindFirstObjectByType<ClickAndDestroyOptimized>();

        if (destructor != null)
        {
            // Destruimos el mapa en la posición exacta del impacto
            destructor.EjecutarDestruccionDesdeGusano(transform.position);
        }

        // 2. Avisamos al Manager que la acción terminó
        if (TurnoManager.Instancia != null)
        {
            // Podrías poner un pequeño retraso aquí para ver la explosión
            TurnoManager.Instancia.FinalizarTurno();
        }

        // 3. Desaparece el proyectil
        Destroy(gameObject);
    }
}*/