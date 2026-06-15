using UnityEngine;
using System.Collections.Generic;

public class EscaneadorMapa : MonoBehaviour
{
    [Header("Configuración de Escaneo")]
    [SerializeField] private LayerMask capaTerreno;
    [SerializeField] private float rangoBusqueda = 100f;
    [SerializeField] private float offsetOrigenRayoY = 30f;

    private struct DatosGizmoPrueba
    {
        public Vector3 origen;
        public Vector3 destino;
        public Color colorRayo;
    }

    private List<DatosGizmoPrueba> rayosParaDibujar = new List<DatosGizmoPrueba>();

    public Vector3 ObtenerPosicionSobreSuelo(Vector3 posicionOriginal, float offsetSobreElSuelo = 2f)
    {
        float puntoMasAltoY = posicionOriginal.y + offsetOrigenRayoY;
        Vector2 origenRayo = new Vector2(posicionOriginal.x, puntoMasAltoY);
        RaycastHit2D hit = Physics2D.Raycast(origenRayo, Vector2.down, rangoBusqueda, capaTerreno);

        DatosGizmoPrueba datosG = new DatosGizmoPrueba();
        datosG.origen = origenRayo;

        if (hit.collider != null)
        {
            datosG.destino = hit.point;
            datosG.colorRayo = Color.cyan;
            rayosParaDibujar.Add(datosG);
            return new Vector3(hit.point.x, hit.point.y + offsetSobreElSuelo, 0f);
        }
        else
        {
            datosG.destino = (Vector3)origenRayo + (Vector3.down * rangoBusqueda);
            datosG.colorRayo = Color.red;
            rayosParaDibujar.Add(datosG);
            return posicionOriginal;
        }
    }

    private void OnDrawGizmos()
    {
        if (rayosParaDibujar == null || rayosParaDibujar.Count == 0) return;

        foreach (var rayo in rayosParaDibujar)
        {
            Gizmos.color = rayo.colorRayo;
            Gizmos.DrawLine(rayo.origen, rayo.destino);

            if (rayo.colorRayo == Color.cyan)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(rayo.destino, 0.25f);
            }
        }
    }

    private void OnDisable()
    {
        if (rayosParaDibujar != null) rayosParaDibujar.Clear();
    }
}