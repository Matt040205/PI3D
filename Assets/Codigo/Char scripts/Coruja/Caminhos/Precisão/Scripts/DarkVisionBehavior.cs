using UnityEngine;

public class DarkVisionBehavior : TowerBehavior
{
    public float darkVisionBonus = 0.5f;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        // Lógica para aumentar a visão em área escura.
        // A torre deve ter uma propriedade de visibilidade que pode ser modificada.
        // Exemplo: towerController.AddDarkVision(darkVisionBonus);
        Debug.Log("DarkVisionBehavior ativado. Visão em área escura aumentada em " + (darkVisionBonus * 100) + "% .");
    }

    private void OnDestroy()
    {
        // Lógica para remover o bônus de visão.
        // Exemplo: towerController.RemoveDarkVision(darkVisionBonus);
    }
}