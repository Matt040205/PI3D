using UnityEngine;

public class PiercingBehavior : TowerBehavior
{
    public int enemiesToPierce = 1;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        // L�gica para modificar o proj�til da torre para que ele perfure inimigos.
        // Isso pode ser tratado no script do proj�til, que teria que ser instanciado
        // por esta classe.
        Debug.Log("PiercingBehavior ativado. Flechas perfuram " + enemiesToPierce + " inimigo(s).");
    }

    private void OnDestroy()
    {
        // Se houver l�gica de limpeza, deve ir aqui.
    }
}