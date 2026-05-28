using System.Collections;
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

    private Vector3 posicionAntesDelClic;
    private float zoomAntesDelClic;
    private float zoomObjetivoCercano;
    private bool haciendoZoomDeEnfoque = false; 
    private bool regresandoAPosicionOriginal = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        ConfigurarAcciones();
    }

    public void EnfocarObjetivo(Transform nuevoObjetivo)
    {
        objetivoTransform = nuevoObjetivo;
        centrar = true;
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
            haciendoZoomDeEnfoque = false;
            regresandoAPosicionOriginal = false;
        }

        float valorZoom = (zoom != null) ? zoom.ReadValue<float>() : 0;
        if (cam != null && cam.orthographic && valorZoom != 0 && !regresandoAPosicionOriginal && !haciendoZoomDeEnfoque)
        {
            cam.orthographicSize -= valorZoom * velocidadZoom * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 2f, 20f);
        }

        if (cam != null)
        {
            if (haciendoZoomDeEnfoque)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoomObjetivoCercano, Seguimiento * Time.deltaTime);
                if (Mathf.Abs(cam.orthographicSize - zoomObjetivoCercano) < 0.05f)
                {
                    cam.orthographicSize = zoomObjetivoCercano;
                    haciendoZoomDeEnfoque = false;
                }
            }
            if (regresandoAPosicionOriginal)
            {
                transform.position = Vector3.Lerp(transform.position, posicionAntesDelClic, Seguimiento * Time.deltaTime);
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoomAntesDelClic, Seguimiento * Time.deltaTime);

                if (Vector3.Distance(transform.position, posicionAntesDelClic) < 0.05f && Mathf.Abs(cam.orthographicSize - zoomAntesDelClic) < 0.05f)
                {
                    transform.position = posicionAntesDelClic;
                    cam.orthographicSize = zoomAntesDelClic;
                    regresandoAPosicionOriginal = false;
                }
            }
        }
    }
    public void DispararEnfoqueTemporal(Transform gusanitoActivo, float tiempoEspera, float nivelZoomCercano)
    {
        if (gusanitoActivo == null || cam == null) return;
        regresandoAPosicionOriginal = false;
        posicionAntesDelClic = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        zoomAntesDelClic = cam.orthographicSize;
        zoomObjetivoCercano = nivelZoomCercano;
        haciendoZoomDeEnfoque = true;
        EnfocarObjetivo(gusanitoActivo);
        StartCoroutine(TemporizadorEnfoque(tiempoEspera));
    }

    private IEnumerator TemporizadorEnfoque(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        centrar = false;
        haciendoZoomDeEnfoque = false;
        regresandoAPosicionOriginal = true;
        PanelInventario.Instancia.PermisoAtacar = true;
    }
}