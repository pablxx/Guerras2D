using UnityEngine;

public class ControladorPies : MonoBehaviour
{
    [Header("Configuración del Chasis (Oruga)")]
    [SerializeField] private LayerMask capaTerreno;       
    [SerializeField] private float anchoOruga = 0.5f;       
    [SerializeField] private float longitudSensores = 1.2f;
    [SerializeField] private float alturaEjePadre = 0.4f; 
    [SerializeField] private float velocidadRotacion = 15f; 

    [Header("Tolerancia de Obstáculos (Escalada)")]
    [SerializeField] private float offsetEscaneoY = 0.4f;


    private Rigidbody2D rbPadre;
    private Transform transformPadre;

    void Start()
    {
        transformPadre = transform.parent;
        if (transformPadre != null)
        {
            rbPadre = transformPadre.GetComponent<Rigidbody2D>();
            rbPadre.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void FixedUpdate()
    {
        if (transformPadre == null || rbPadre == null) return;
        Vector2 posicionCentroMundo = transform.position;
        Vector2 origenIzquierdo = posicionCentroMundo + (Vector2.left * (anchoOruga / 2f)) + (Vector2.up * offsetEscaneoY);
        Vector2 origenDerecho = posicionCentroMundo + (Vector2.right * (anchoOruga / 2f)) + (Vector2.up * offsetEscaneoY);
        RaycastHit2D hitIzquierdo = Physics2D.Raycast(origenIzquierdo, Vector2.down, longitudSensores, capaTerreno);
        RaycastHit2D hitDerecho = Physics2D.Raycast(origenDerecho, Vector2.down, longitudSensores, capaTerreno);
        Debug.DrawRay(origenIzquierdo, Vector2.down * longitudSensores, hitIzquierdo.collider != null ? Color.green : Color.red);
        Debug.DrawRay(origenDerecho, Vector2.down * longitudSensores, hitDerecho.collider != null ? Color.green : Color.red);
        if (hitIzquierdo.collider != null && hitDerecho.collider != null)
        {
            Vector2 vectorSuelo = hitDerecho.point - hitIzquierdo.point;
            float anguloMundo = Mathf.Atan2(vectorSuelo.y, vectorSuelo.x) * Mathf.Rad2Deg;
            float signoEscalaX = Mathf.Sign(transformPadre.localScale.x);
            anguloMundo *= signoEscalaX;
            anguloMundo = Mathf.Clamp(anguloMundo, -55f, 55f);

            Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, anguloMundo);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, rotacionObjetivo, Time.fixedDeltaTime * velocidadRotacion);
            if (rbPadre.linearVelocity.y > 0.2f)
            {
                return;
            }
            float puntoMedioSueloY = (hitIzquierdo.point.y + hitDerecho.point.y) / 2f;
            float destinoYPadre = puntoMedioSueloY + alturaEjePadre;
            Vector3 posicionCorregidaPadre = transformPadre.position;
            posicionCorregidaPadre.y = Mathf.Lerp(posicionCorregidaPadre.y, destinoYPadre, Time.fixedDeltaTime * 20f);
            transformPadre.position = posicionCorregidaPadre;
            if (rbPadre.linearVelocity.y < 0.05f)
            {
                rbPadre.linearVelocity = new Vector2(rbPadre.linearVelocity.x, 0f);
            }
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.fixedDeltaTime * velocidadRotacion);
        }
    }
}