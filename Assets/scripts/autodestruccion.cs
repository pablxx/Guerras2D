using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class autodestruccion : MonoBehaviour
{
    [Header("Configuración de Explosión Final")]
    [SerializeField] int radioExplosion = 4;
    [SerializeField] float radioDanio = 5f;
    [SerializeField] float danioMaximo = 45f;
    [SerializeField] float fuerzaEmpuje = 10f;
    [SerializeField] float tiempoAntesDeEstallar;

    public System.Action OnDetonacionCompletada;

    public void IniciarCuentaRegresiva()
    {
        StartCoroutine(SecuenciaMuerte());
    }
    private IEnumerator SecuenciaMuerte()
    {
        Debug.Log($"[Autodestrucción] {gameObject.name} va a estallar en {tiempoAntesDeEstallar}s...");
        yield return new WaitForSeconds(tiempoAntesDeEstallar);
        Vector3 puntoImpacto = transform.position;
        var destructor = Object.FindFirstObjectByType<DTerrain.ClickAndDestroyOptimized>();
        if (destructor != null)
        {
            destructor.CambiarTamaño(radioExplosion);
            destructor.EjecutarDestruccion(puntoImpacto);
        }
        List<Vida> vidasProcesadasEnEstaExplosion = new List<Vida>();
        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(puntoImpacto, radioDanio);
        foreach (Collider2D col in objetosDetectados)
        {
            if (col.gameObject == gameObject) continue;
            Vida vidaObjetivo = col.GetComponent<Vida>();
            Rigidbody2D rbGusano = col.GetComponent<Rigidbody2D>();
            float distancia = Vector2.Distance(puntoImpacto, col.transform.position);
            float factorCercania = Mathf.Clamp01((radioDanio - distancia) / radioDanio);
            if (vidaObjetivo != null && factorCercania > 0)
            {             
                if (vidasProcesadasEnEstaExplosion.Contains(vidaObjetivo)) continue;
                vidasProcesadasEnEstaExplosion.Add(vidaObjetivo);
                vidaObjetivo.RecibirDanio(danioMaximo * factorCercania);
            }
            if (rbGusano != null && factorCercania > 0)
            {
                Vector2 direccionEmpuje = (col.transform.position - puntoImpacto).normalized;
                rbGusano.AddForce(direccionEmpuje * (fuerzaEmpuje * factorCercania), ForceMode2D.Impulse);
            }
        }
        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
        var miColisionador = GetComponent<Collider2D>();
        if (miColisionador != null) miColisionador.enabled = false;
        var miRigidbody = GetComponent<Rigidbody2D>();
        if (miRigidbody != null) miRigidbody.bodyType = RigidbodyType2D.Static;
        if (transform.childCount > 0) transform.GetChild(0).gameObject.SetActive(false);
        OnDetonacionCompletada?.Invoke();
        Destroy(gameObject, 0.1f);
    }
}