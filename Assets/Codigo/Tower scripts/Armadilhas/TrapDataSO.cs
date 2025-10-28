// TrapDataSO.cs
using UnityEngine;

// Enum para definir onde a armadilha pode ser colocada
public enum TrapPlacementType
{
    OnPath,  // Apenas no caminho dos inimigos
    OffPath  // Apenas fora do caminho e fora de locais de torre
}

[CreateAssetMenu(fileName = "Nova Armadilha", menuName = "ExoBeasts/Armadilha")]
public class TrapDataSO : ScriptableObject
{
    [Header("Informações da Armadilha")]
    public string trapName = "Nova Armadilha";
    [TextArea(3, 5)]
    public string description = "Descrição da armadilha.";
    public Sprite icon;
    public GameObject prefab; // O prefab da armadilha a ser instanciado

    [Header("Posicionamento")]
    public TrapPlacementType placementType = TrapPlacementType.OffPath;

    [Header("Custos")]
    public int geoditeCost = 10;
    public int darkEtherCost = 0; // Se armadilhas usarem Ether

    // Adicione outras propriedades se necessário (dano, efeito, etc.)
}