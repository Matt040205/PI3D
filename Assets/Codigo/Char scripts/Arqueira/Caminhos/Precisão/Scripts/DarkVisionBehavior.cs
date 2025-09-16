using UnityEngine;

public class DarkVisionBehavior : TowerBehavior
{
    public float darkVisionBonus = 0.5f;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        // L�gica para aumentar a vis�o em �rea escura.
        // A torre deve ter uma propriedade de visibilidade que pode ser modificada.
        // Exemplo: towerController.AddDarkVision(darkVisionBonus);
        Debug.Log("DarkVisionBehavior ativado. Vis�o em �rea escura aumentada em " + (darkVisionBonus * 100) + "% .");
    }

    private void OnDestroy()
    {
        // L�gica para remover o b�nus de vis�o.
        // Exemplo: towerController.RemoveDarkVision(darkVisionBonus);
    }
}