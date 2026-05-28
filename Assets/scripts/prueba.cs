using UnityEngine;


public class BombaPrueba : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        var destructor = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();

        if (destructor != null)
        {
            Vector3 puntoImpacto = collision.contacts[0].point;
            destructor.CambiarTamaño(50);
            destructor.EjecutarDestruccion(puntoImpacto);
            destructor.CambiarTamaño(16);
        }

        Destroy(gameObject);
    }
}