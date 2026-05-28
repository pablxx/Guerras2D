using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

    public class SlotArmaUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
        [SerializeField] private Image imagenIcono;
        [SerializeField] private TextMeshProUGUI textoMunicion;
        private Button botonComponente;
        private DatosArmas armaAsignada;
        private PanelInventario panelPrincipal;
        private int cantidadActual;
        private bool esInfinitaActiva;

    private int cantidadLocal;
    private bool esInfinitaLocal;
    private void Awake()
        {
            botonComponente = GetComponent<Button>();
        }

        public void ConfigurarSlot(DatosArmas datosArma, int municion, bool infinita, PanelInventario panel)
        {
            armaAsignada = datosArma;
            panelPrincipal = panel;

        cantidadLocal = municion;
        esInfinitaLocal = infinita;

        if (botonComponente != null) botonComponente.interactable = true;
            if (imagenIcono != null && datosArma.icono != null)
            {
                imagenIcono.sprite = datosArma.icono;
                imagenIcono.enabled = true;
            }

            if (textoMunicion != null)
            {
                textoMunicion.text = infinita ? "infinita" : municion.ToString();
                textoMunicion.enabled = true;
            }
        }
        public void LimpiarSlot()
        {
            armaAsignada = null;

            if (botonComponente != null) botonComponente.interactable = false;

            if (imagenIcono != null) imagenIcono.enabled = false;
            if (textoMunicion != null) textoMunicion.text = "";
        }

        public void AlHacerClicEnSlot()
        {            
            panelPrincipal.SeleccionarArma(armaAsignada);
        if (TurnoManager.Instancia != null && TurnoManager.Instancia.soldadoActivoEnEsteTurno != null)
        {
            camaraMovimiento scriptCamara = Camera.main.GetComponent<camaraMovimiento>();
            if (scriptCamara != null)
            {
                scriptCamara.DispararEnfoqueTemporal(TurnoManager.Instancia.soldadoActivoEnEsteTurno.transform, 1f, 4.5f);
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (armaAsignada != null && panelPrincipal != null)
        {
            panelPrincipal.MostrarDetallesArma(armaAsignada.nombreArma, cantidadLocal, esInfinitaLocal);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (panelPrincipal != null)
        {
            panelPrincipal.LimpiarDetallesArma();
        }
    }

}