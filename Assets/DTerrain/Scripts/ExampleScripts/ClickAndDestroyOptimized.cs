using UnityEngine;
using UnityEngine.InputSystem;
using DTerrain;

namespace DTerrain
{
    public class ClickAndDestroyOptimized : ClickAndDestroy
    {
        // Usamos 'new' para ocultar el Update original del asset
        new void Update()
        {
            if (Mouse.current == null) return;

            // Detecta el clic izquierdo presionado
            /*if (Mouse.current.rightButton.isPressed)
            {
                BorrarConClicIzquierdo();
            }*/
        }

        private void BorrarConClicIzquierdo()
        {
            if (primaryLayer == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
            Vector3 p = worldPos - primaryLayer.transform.position;

            PaintingParameters pp = new PaintingParameters()
            {
                Color = Color.clear,
                Position = new Vector2Int((int)(p.x * primaryLayer.PPU) - circleSize, (int)(p.y * primaryLayer.PPU) - circleSize),
                Shape = destroyCircle,
                PaintingMode = PaintingMode.REPLACE_COLOR,
                DestructionMode = DestructionMode.DESTROY
            };

            primaryLayer.Paint(pp);

            if (secondaryLayer != null)
            {
                pp.DestructionMode = DestructionMode.NONE;
                secondaryLayer.Paint(pp);
            }
        }
        public void EjecutarDestruccion(Vector3 posicionMundo)
        {
            if (primaryLayer == null || destroyCircle == null) return;

            Vector3 p = posicionMundo - primaryLayer.transform.position;
            Vector2Int pixelPos = new Vector2Int((int)(p.x * primaryLayer.PPU) - circleSize, (int)(p.y * primaryLayer.PPU) - circleSize);

            PaintingParameters pp = new PaintingParameters()
            {
                Color = Color.clear,
                Position = pixelPos,
                Shape = destroyCircle,
                PaintingMode = PaintingMode.REPLACE_COLOR,
                DestructionMode = DestructionMode.DESTROY
            };

            primaryLayer.Paint(pp);

            if (secondaryLayer != null)
            {
                pp.DestructionMode = DestructionMode.NONE;
                secondaryLayer.Paint(pp);
            }
        }

        public void CambiarTamaño(int nuevoTamaño)
        {
            circleSize = nuevoTamaño;
            // Ahora usamos el nombre exacto que vimos en el script padre:
            destroyCircle = Shape.GenerateShapeCircle(circleSize);
        }

    }
}