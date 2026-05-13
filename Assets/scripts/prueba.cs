using UnityEngine;
using DTerrain;

public class BombaPrueba : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        var destructor = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();

        if (destructor != null)
        {
            Vector3 puntoImpacto = collision.contacts[0].point;

            // Cambiamos a un tamaño grande (ej: 60)
            destructor.CambiarTamaño(50);

            // Ejecutamos la explosión
            destructor.EjecutarDestruccionDesdeGusano(puntoImpacto);

            // Opcional: Regresamos el tamaño a lo normal (ej: 16) 
            // para que el pincel del mouse no se quede gigante
            destructor.CambiarTamaño(16);
        }

        Destroy(gameObject);
    }
}