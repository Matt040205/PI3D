// Ability.cs
using UnityEngine;

// Este é o nosso "molde". Não vamos usá-lo diretamente.
public abstract class Ability : ScriptableObject
{
    [Header("Informações da Habilidade")]
    public string abilityName = "Nova Habilidade";
    public Sprite icon;
    public float cooldown = 1f;

    // Este é o "botão" que o personagem vai apertar.
    // 'abstract' significa que o molde não diz o que o botão faz,
    // apenas que ele existe. Cada receita específica vai ter que definir isso.
    //
    // Ele retorna 'true' se a habilidade deve entrar em cooldown.
    // Retorna 'false' se o cooldown deve ser ignorado (nosso caso de reset).
    public abstract bool Activate(GameObject quemUsou);
}