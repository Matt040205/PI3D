using UnityEngine;

public class GridPlacement : MonoBehaviour
{
    [Header("Configuração de Grade")]
    public float gridSize = 1f;

    [Header("Custo")]
    public int cost = 100;

    // Obtém a metade da altura do objeto (do Renderer ou Collider)
    public float GetObjectHalfHeight()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            return rend.bounds.extents.y;
        }

        // Se não tiver Renderer (ex: objeto sem renderização, apenas com collider)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            return col.bounds.extents.y;
        }

        return 0f; // Retorna 0 se não encontrar nada
    }

    public void SnapToGrid()
    {
        Vector3 currentPosition = transform.position;

        float alignedX = Mathf.Round(currentPosition.x / gridSize) * gridSize;
        // Não ajustamos o Y aqui, será feito no BuildManager
        float alignedZ = Mathf.Round(currentPosition.z / gridSize) * gridSize;

        transform.position = new Vector3(alignedX, currentPosition.y, alignedZ);
    }
    public float GetObjectHeight()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            return rend.bounds.size.y;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            return col.bounds.size.y;
        }

        return 0f;
    }
}