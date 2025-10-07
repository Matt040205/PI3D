// Ability.cs
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    [Header("Informações da Habilidade")]
    public string abilityName = "Nova Habilidade";
    [TextArea(3, 5)] // Isso só deixa a caixa de texto maior na Unity, ajuda a escrever.
    public string description = "Escreva a descrição aqui."; // << LINHA ADICIONADA
    public Sprite icon;
    public float cooldown = 1f;

    public abstract bool Activate(GameObject quemUsou);
}