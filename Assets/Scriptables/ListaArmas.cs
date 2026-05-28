using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ListaArmas", menuName = "Scriptable Objects/ListaArmas")]
public class ListaArmas : ScriptableObject
{
    [System.Serializable]
    public class RanuraInventario
    {
        [SerializeField] public DatosArmas datosBaseArma;
        [SerializeField] public int cantidadMunicion;
        [SerializeField] public bool esInfinita;
    }
    [SerializeField] private List<RanuraInventario> armasInv = new List<RanuraInventario>();
    public List<RanuraInventario> ArmasInv => armasInv;
}
