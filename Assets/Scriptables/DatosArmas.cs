using UnityEngine;

[CreateAssetMenu(fileName = "DatosArmas", menuName = "Datos Armas/Añadir Nueva Arma")]
public class DatosArmas : ScriptableObject
{
    public enum TipoArma {
        Proyectil,
        Arrojable,
        CaC,
        Aereo,
        Invocable,
        ArmaFuego
    }
    [SerializeField] TipoArma Tipo;
    [SerializeField] string NombreArma;
    [SerializeField] Sprite Icono;
    [SerializeField][TextArea(2, 4)] string Descripcion;
    [SerializeField] int RadioExplosion;
    [SerializeField] float RadioDanio;
    [SerializeField] int DanioMaximo;
    [SerializeField] float FuerzaEmpuje;
    [SerializeField] GameObject PrefabArma;
    [SerializeField] bool Temporizador;
    [SerializeField] float TiempoExplosion;
    [Header("Solo Armas de Fuego")]
    [SerializeField] int Rafagas;
    [SerializeField] AudioClip AudioInstancia;


    public string nombreArma => NombreArma;
    public Sprite icono => Icono;
    public string DescripcionArma => Descripcion;
    public TipoArma tipo => Tipo;
    public int radioExplosion => RadioExplosion;
    public float radioDanio => RadioDanio;
    public int danioMaximo => DanioMaximo;
    public float fuerzaEmpuje => FuerzaEmpuje;
    public GameObject prefabArma => PrefabArma;
    public float tiempoExplosion => TiempoExplosion;
    public bool temporizador => Temporizador;
    public int rafagas => Rafagas;
    public AudioClip audioInstancia => AudioInstancia;
}
