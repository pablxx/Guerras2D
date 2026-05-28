using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PanelInventario : MonoBehaviour
{
    public static PanelInventario Instancia;
    [SerializeField] private ListaArmas inventarioOriginal;
    [SerializeField] private GameObject objetoPanel;
    [SerializeField] private Transform contenedorSlots;
    [SerializeField] private DatosArmas armaEquipadaActiva;
    [SerializeField] RectTransform rectTransformPanel;
    [SerializeField] RectTransform PuntoA;
    [SerializeField] RectTransform PuntoB;
    [SerializeField] TextMeshProUGUI textoDetalleNombre;
    [SerializeField] TextMeshProUGUI textoDetalleIntentos;

    public DatosArmas ArmaEquipadaActiva => armaEquipadaActiva;

    private ListaArmas inventarioEquipoA;
    private ListaArmas inventarioEquipoB;

    float velocidadPanel = 5f;
    public bool PermisoAtacar;
    public bool InventarioActivo;

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        inventarioEquipoA = ScriptableObject.Instantiate(inventarioOriginal);
        inventarioEquipoB = ScriptableObject.Instantiate(inventarioOriginal);

        InventarioActivo = false;
        PermisoAtacar = false;
        DibujarInventarioUI();
    }

    void Update()
    {
        if (InventarioActivo == true)
        {
            MoverPanel();
        }
        else if (InventarioActivo == false)
        {
            AlejarPanel();
        }
    }

    void MoverPanel()
    {
        if (Vector3.Distance(rectTransformPanel.position, PuntoA.position) >= 0.2f)
        {
            rectTransformPanel.transform.position = Vector3.Lerp(rectTransformPanel.position, PuntoA.position, velocidadPanel * Time.deltaTime);
        }
    }

    void AlejarPanel()
    {
        if (Vector3.Distance(rectTransformPanel.position, PuntoB.position) >= 0.2f)
        {
            rectTransformPanel.position = Vector3.Lerp(rectTransformPanel.position, PuntoB.position, velocidadPanel * Time.deltaTime);
        }
    }

    public void DibujarInventarioUI()
    {
        if (contenedorSlots == null)
        {
            return;
        }

        ListaArmas inventarioActual = inventarioEquipoA;
        if (TurnoManager.Instancia != null)
        {
            if (TurnoManager.Instancia.equipoQueLeToca == TipoEquipo.EquipoB)
            {
                inventarioActual = inventarioEquipoB;
            }
        }

        if (inventarioActual == null)
        {
            return;
        }

        SlotArmaUI[] slotsDisponibles = contenedorSlots.GetComponentsInChildren<SlotArmaUI>(true);
        for (int i = 0; i < slotsDisponibles.Length; i++)
        {
            if (i < inventarioActual.ArmasInv.Count)
            {
                ListaArmas.RanuraInventario ranura = inventarioActual.ArmasInv[i];

                if (ranura != null && ranura.datosBaseArma != null)
                {
                    if (ranura.esInfinita == false && ranura.cantidadMunicion <= 0)
                    {
                        slotsDisponibles[i].LimpiarSlot();
                    }
                    else
                    {
                        slotsDisponibles[i].ConfigurarSlot(
                            ranura.datosBaseArma,
                            ranura.cantidadMunicion,
                            ranura.esInfinita,
                            this
                        );
                    }
                }
                else
                {
                    slotsDisponibles[i].LimpiarSlot();
                }
            }
            else
            {
                slotsDisponibles[i].LimpiarSlot();
            }
        }
    }

    public void DescontarArma(DatosArmas armaDisparada)
    {
        ListaArmas inventarioActual = inventarioEquipoA;
        if (TurnoManager.Instancia != null)
        {
            if (TurnoManager.Instancia.equipoQueLeToca == TipoEquipo.EquipoB)
            {
                inventarioActual = inventarioEquipoB;
            }
        }

        if (inventarioActual == null)
        {
            return;
        }

        foreach (var ranura in inventarioActual.ArmasInv)
        {
            if (ranura.datosBaseArma == armaDisparada)
            {
                if (ranura.esInfinita == true)
                {
                    return;
                }

                if (ranura.cantidadMunicion > 0)
                {
                    ranura.cantidadMunicion--;
                    Debug.Log($"Munición reducida para el {TurnoManager.Instancia.equipoQueLeToca}. Quedan: {ranura.cantidadMunicion}");
                    if (ranura.cantidadMunicion <= 0)
                    {
                        armaEquipadaActiva = null;
                        PermisoAtacar = false;
                        LimpiarDetallesArma();
                    }
                }

                DibujarInventarioUI();
                break;
            }
        }
    }

    public void SeleccionarArma(DatosArmas armaSeleccionada)
    {
        armaEquipadaActiva = armaSeleccionada;
        if (armaEquipadaActiva != null)
        {
            Debug.Log("Has equipado: " + armaEquipadaActiva.nombreArma);
        }

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        InventarioActivo = false;
    }

    public void MostrarDetallesArma(string nombre, int municion, bool infinita)
    {
        if (textoDetalleNombre != null)
        {
            textoDetalleNombre.text = nombre;
        }

        if (textoDetalleIntentos != null)
        {
            if (infinita == true)
            {
                textoDetalleIntentos.text = "Infinito";
            }
            else
            {
                textoDetalleIntentos.text = "x" + municion.ToString();
            }
        }
    }

    public void LimpiarDetallesArma()
    {
        if (textoDetalleNombre != null)
        {
            textoDetalleNombre.text = "";
        }
        if (textoDetalleIntentos != null)
        {
            textoDetalleIntentos.text = "";
        }
    }  
}