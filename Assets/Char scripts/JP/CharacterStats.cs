using UnityEngine;
using System.Collections.Generic;

// Classe que representa os stats em runtime — construída a partir do CharacterBase
[System.Serializable]
public class CharacterStats
{
    // campos copiados do CharacterBase (runtime)
    public float maxHealth;
    public float damage;
    public float moveSpeed;
    public float reloadSpeed;
    public float attackSpeed;
    public int magazineSize;

    // runtime bookkeeping
    public List<AbilitySO> unlockedAbilities = new List<AbilitySO>();
    public Transform runtimeOwner; // referência para spawn de VFX, efeitos etc.

    public CharacterStats(CharacterBase baseData, Transform owner = null)
    {
        if (baseData == null) return;
        maxHealth = baseData.maxHealth;
        damage = baseData.damage;
        moveSpeed = baseData.moveSpeed;
        reloadSpeed = baseData.reloadSpeed;
        attackSpeed = baseData.attackSpeed;
        magazineSize = baseData.magazineSize;
        runtimeOwner = owner;
    }

    public void ApplyGlobalCooldownReduction(float seconds)
    {
        // exemplo simples: reduz reloadSpeed / cooldowns de habilidades
        // aqui você pode propagar para sistemas de habilidade específicos
        reloadSpeed = Mathf.Max(0.01f, reloadSpeed - seconds);
    }

    public void UnlockAbility(AbilitySO ability)
    {
        if (ability == null) return;
        if (!unlockedAbilities.Contains(ability))
            unlockedAbilities.Add(ability);
    }
}