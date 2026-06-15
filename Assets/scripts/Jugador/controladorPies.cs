using UnityEngine;

public class ControladorPies : MonoBehaviour
{
    [Header("Configuración de los Sensores")]
    [SerializeField] private LayerMask capaTerreno;
    [SerializeField] private float anchoPie = 0.5f;
    [SerializeField] private float longitudSensores = 1f;
    [SerializeField] private float velocidadAjuste = 100f;
    [SerializeField] private float velocidadRetornoAire = 5f;

    private Transform transformPadre;
    private float anguloActual = 0f;

    public bool tocandoSuelo = false;

    void Start()
    {
        transformPadre = transform.parent;
        transform.localRotation = Quaternion.identity;
    }

    void FixedUpdate()
    {
        Vector2 posicionCentro = transform.position;
        Vector2 origenIzquierdo = posicionCentro + (Vector2.left * (anchoPie / 2f));
        Vector2 origenDerecho = posicionCentro + (Vector2.right * (anchoPie / 2f));

        Vector2 direccionMundo = Vector2.down;

        RaycastHit2D hitIzquierdo = Physics2D.Raycast(origenIzquierdo, direccionMundo, longitudSensores, capaTerreno);
        RaycastHit2D hitDerecho = Physics2D.Raycast(origenDerecho, direccionMundo, longitudSensores, capaTerreno);

        Debug.DrawRay(origenIzquierdo, direccionMundo * longitudSensores, hitIzquierdo.collider != null ? Color.green : Color.red);
        Debug.DrawRay(origenDerecho, direccionMundo * longitudSensores, hitDerecho.collider != null ? Color.green : Color.red);

        tocandoSuelo = (hitIzquierdo.collider != null || hitDerecho.collider != null);

        float signoEscalaX = (transformPadre != null) ? Mathf.Sign(transformPadre.localScale.x) : 1f;

        if ((hitIzquierdo.collider == null && hitDerecho.collider == null) || (hitIzquierdo.collider != null && hitDerecho.collider != null))
        {
            anguloActual = Mathf.Lerp(anguloActual, 0f, Time.fixedDeltaTime * velocidadRetornoAire);
        }
        else if (hitIzquierdo.collider != null && hitDerecho.collider == null)
        {
            anguloActual -= velocidadAjuste * Time.fixedDeltaTime * signoEscalaX;
        }
        else if (hitIzquierdo.collider == null && hitDerecho.collider != null)
        {
            anguloActual += velocidadAjuste * Time.fixedDeltaTime * signoEscalaX;
        }

        anguloActual = Mathf.Clamp(anguloActual, -55f, 55f);
        transform.localRotation = Quaternion.Euler(0f, 0f, anguloActual);
    }
}