using UnityEngine;
using UnityEngine.InputSystem;

public class camaraMovimiento : MonoBehaviour
{
    [SerializeField] PlayerInput playerInputJugador;
    [SerializeField] float velocidad;
    [SerializeField] float velocidadZoom;
    [SerializeField] float Seguimiento;

    InputAction mover;
    InputAction zoom;

    Vector2 movimiento;
    Camera cam;
    Transform objetivoTransform;
    bool centrar = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        ConfigurarAcciones();
    }

    public void ActualizarReferenciaInput(PlayerInput nuevoInput)
    {
        playerInputJugador = nuevoInput;
        if (nuevoInput != null)
        {
            objetivoTransform = nuevoInput.transform;
            centrar = true;
        }
        ConfigurarAcciones();
    }

    void ConfigurarAcciones()
    {
        if (playerInputJugador != null)
        {
            mover = playerInputJugador.actions.FindAction("MoverCamara");
            zoom = playerInputJugador.actions.FindAction("Zoom");
        }
    }

    void Update()
    {
        if (playerInputJugador == null || mover == null) return;

        movimiento = mover.ReadValue<Vector2>();
        transform.Translate(new Vector3(movimiento.x, movimiento.y, 0) * velocidad * Time.deltaTime, Space.World);
        if (centrar && objetivoTransform != null)
        {
            Vector3 destino = new Vector3(objetivoTransform.position.x, objetivoTransform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, destino, Seguimiento * Time.deltaTime);
            if (Vector3.Distance(transform.position, destino) < 0.1f)
            {
                centrar = false;
            }
        }
        if (movimiento.magnitude > 0.1f)
        {
            centrar = false;
        }
       
        float valorZoom = (zoom != null) ? zoom.ReadValue<float>() : 0;
        if (cam != null && cam.orthographic && valorZoom != 0)
        {
            cam.orthographicSize -= valorZoom * velocidadZoom * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 2f, 20f);
        }
    }
}