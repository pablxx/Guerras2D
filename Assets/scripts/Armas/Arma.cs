using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Arma : MonoBehaviour
{
    protected string nombre;
    protected int danioMaximo;
    protected int radioExplosion;
    protected float radioDanio;
    protected float fuerzaEmpuje;
    protected bool usaTemporizador;
    protected float tiempoExplosion;
    protected int rafagas;
    protected AudioClip audioInstancia;

    public void Inicializar(DatosArmas Datos) {
        nombre = Datos.nombreArma;
        danioMaximo = Datos.danioMaximo;
        radioExplosion = Datos.radioExplosion;
        radioDanio = Datos.radioDanio;
        fuerzaEmpuje = Datos.fuerzaEmpuje;
        usaTemporizador = Datos.temporizador;
        tiempoExplosion = Datos.tiempoExplosion;
        rafagas = Datos.rafagas;
        audioInstancia = Datos.audioInstancia;
        Usar();
    }
    public virtual void Usar() {

        AudioManager.Instancia.PlaySFXDirecto(audioInstancia);
    }

    
}
