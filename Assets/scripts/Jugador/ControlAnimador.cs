using System.Collections;
using UnityEngine;

public class ControlAnimador : MonoBehaviour
{
    [Header("Referencias Directas (Arrastrar aquí)")]
    [SerializeField] private ControladorPies sensoresPies;

    private Animator animator;
    private Rigidbody2D rbPadre;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (transform.parent != null)
        {
            rbPadre = transform.parent.GetComponent<Rigidbody2D>();
        }

        if (sensoresPies == null)
        {
            Debug.LogWarning("[ControlAnimador] ¡Falta arrastrar el ControladorPies al Inspector, Harold!");
        }
    }

    void Update()
    {
        if (animator != null && sensoresPies != null)
        {
            animator.SetBool("EnAire", !sensoresPies.tocandoSuelo);
        }
    }

    public void ActualizarCaminata(float velocidadX)
    {
        if (animator != null)
        {
            animator.SetFloat("VelocidadX", Mathf.Abs(velocidadX));
        }
    }

    public void EjecutarImpulsoSalto()
    {
        if (animator != null)
        {
            StartCoroutine(RutinaDisparoSalto());
        }
    }

    private IEnumerator RutinaDisparoSalto()
    {
        animator.SetBool("Saltando", true);
        yield return null;
        animator.SetBool("Saltando", false);
    }

    public void EjecutarMuerte()
    {
        if (animator != null)
        {
            animator.SetBool("Morir", true);
        }
    }

    public void EjecutarFestejo()
    {
        if (animator != null)
        {
            animator.SetBool("Festejando", true);
        }
    }
}