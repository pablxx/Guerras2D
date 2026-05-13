using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurnoManager : MonoBehaviour
{
    public static TurnoManager Instancia;

    [SerializeField] camaraMovimiento miCamara;
    [SerializeField] int indiceTurnoActual = 0;
    [SerializeField] GameObject SoldadoPrefab;
    public List<GameObject> ListaSoldados;
    [SerializeField] int cantidadSoldados;
    [SerializeField] Transform LimiteIzq;
    [SerializeField] Transform LimiteDer;

    float min;
    float max;
    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }
   IEnumerator Start()
    {
        ListaSoldados = new List<GameObject>();

        min = LimiteIzq.transform.position.x;
        max = LimiteDer.transform.position.x;
        yield return new WaitForSeconds(0.2f);
        InsertarSoldados();
        BarajarLista();
        RandomizarPosicion();
        foreach (GameObject soldado in ListaSoldados)
        {
            soldado.SetActive(true);
        }
        ActivarSoldado(0);
    }

    void InsertarSoldados()
    {
        for (int i = 0; i < cantidadSoldados; i++)
        {
            GameObject nuevoSoldado = Instantiate(SoldadoPrefab, transform.position, transform.rotation);
            nuevoSoldado.SetActive(false);
            nuevoSoldado.name = "soldado_" + (i + 1);
            ListaSoldados.Add(nuevoSoldado);
        }
    }

    void BarajarLista()
    {
        for (int i = ListaSoldados.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            GameObject temporal = ListaSoldados[i];
            ListaSoldados[i] = ListaSoldados[j];
            ListaSoldados[j] = temporal;
        }
        Debug.Log("Orden de turnos barajado aleatoriamente.");
    }

    void RandomizarPosicion()
    {
        for (int i = 0; i < ListaSoldados.Count; i++)
        {
            float randomX = Random.Range(min, max);
            randomX = Mathf.Round(randomX * 10f) / 10f;
            Vector3 nuevaPos = new Vector3(randomX, ListaSoldados[i].transform.position.y, ListaSoldados[i].transform.position.z);
            ListaSoldados[i].transform.position = nuevaPos;
            Rigidbody2D rb = ListaSoldados[i].GetComponent<Rigidbody2D>();
            rb.gravityScale = 1f; 
        }
    }
    public void FinalizarTurno()
    {
        DesactivarSoldado(indiceTurnoActual);
        indiceTurnoActual++;
        if (indiceTurnoActual >= ListaSoldados.Count) {
            indiceTurnoActual = 0;
        }
        ActivarSoldado(indiceTurnoActual);
    }
    void ActivarSoldado(int indice)
    {
        GameObject soldadoActivo = ListaSoldados[indice];
        var input = soldadoActivo.GetComponent<PlayerInput>();
        if (input != null) input.enabled = true;
        var scriptMov = soldadoActivo.GetComponent<movimientoJugador>();
        if (scriptMov != null) scriptMov.enabled = true;
        if (miCamara != null && input != null)
        {
            miCamara.ActualizarReferenciaInput(input);
        }
    }

    void DesactivarSoldado(int indice)
    {
        GameObject soldadoInactivo = ListaSoldados[indice];
        var input = soldadoInactivo.GetComponent<PlayerInput>();
        if (input != null) input.enabled = false;
        var scriptMov = soldadoInactivo.GetComponent<movimientoJugador>();
        if (scriptMov != null) scriptMov.enabled = false;
    }


}