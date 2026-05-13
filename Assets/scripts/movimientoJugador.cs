using UnityEngine;
using UnityEngine.InputSystem;

public class movimientoJugador : MonoBehaviour
{
    [SerializeField] float velocidad;
    [SerializeField] float fuerzaSalto;
    [SerializeField] float alturaEscalon = 0.2f;

    PlayerInput input;
    InputAction mover;
    InputAction salto;
    InputAction atacar;
    Rigidbody2D rb2d;
    Vector2 movimiento;
    CapsuleCollider2D col2d;

    void Start()
    {
        input = GetComponent<PlayerInput>();
        rb2d = GetComponent<Rigidbody2D>();
        col2d = GetComponent<CapsuleCollider2D>();

        mover = input.actions.FindAction("MoverPersonaje");
        salto = input.actions.FindAction("Jump");
        atacar = input.actions.FindAction("Atacar");
    }

    void Update()
    {
        movimiento = mover.ReadValue<Vector2>();

        if (salto.WasPressedThisFrame())
        {
            saltar();
        }
        if (atacar.WasPressedThisFrame()) {
            Atacar();
        }
    }

    private void FixedUpdate()
    {
        rb2d.linearVelocity = new Vector2(movimiento.x * velocidad, rb2d.linearVelocity.y);

        if (Mathf.Abs(movimiento.x) > 0.01f)
        {
            SubirEscalon();
        }
    }

    void SubirEscalon()
    {
        float direccionX = Mathf.Sign(movimiento.x);
        Vector2 origenPies = new Vector2(
            transform.position.x + (direccionX * col2d.bounds.extents.x),
            transform.position.y - col2d.bounds.extents.y + 0.05f
        );
        RaycastHit2D hitBajo = Physics2D.Raycast(origenPies, Vector2.right * direccionX, 0.1f);
        if (hitBajo.collider != null)
        {
            RaycastHit2D hitAlto = Physics2D.Raycast(origenPies + Vector2.up * alturaEscalon, Vector2.right * direccionX, 0.15f);
            if (hitAlto.collider == null)
            {
                transform.position += new Vector3(0, 0.05f, 0);
            }
        }
    }

    void saltar()
    {
        rb2d.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
    }

    void Atacar()
    {
        Debug.Log(gameObject.name + " esta atacando");
        if (TurnoManager.Instancia != null)
        {
            TurnoManager.Instancia.FinalizarTurno();
        }
    }

    private void OnDrawGizmos()
    {
        if (!enabled || col2d == null ) return;

        float direccion = (movimiento.x != 0) ? Mathf.Sign(movimiento.x) : 1;
        Vector2 origenPies = new Vector2(
            transform.position.x + (direccion * col2d.bounds.extents.x),
            transform.position.y - col2d.bounds.extents.y + 0.05f
        );
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origenPies, Vector2.right * direccion * 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(origenPies + Vector2.up * alturaEscalon, Vector2.right * direccion * (0.5f + 0.05f));
    }
}