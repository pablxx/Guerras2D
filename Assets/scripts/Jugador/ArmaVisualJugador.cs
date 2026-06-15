using UnityEngine;

public class ArmaVisualJugador : MonoBehaviour
{
    private SpriteRenderer spriteRendererArma;
    [SerializeField] Vector3 scalaSugerida;

    void Awake()
    {
        spriteRendererArma = GetComponent<SpriteRenderer>();
    }
    public void RenderizarArma(Sprite spriteIcono)
    {
        if (spriteRendererArma == null) return;

        spriteRendererArma.sprite = spriteIcono;
        transform.localScale = scalaSugerida;
    }

    public void LimpiarArma()
    {
        if (spriteRendererArma != null) spriteRendererArma.sprite = null;
    }
}