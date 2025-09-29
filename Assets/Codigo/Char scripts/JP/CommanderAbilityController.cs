using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// AGORA ESTE SCRIPT É GENÉRICO. Ele funcionará para QUALQUER personagem
// sem precisar de modificações para novas habilidades.
public class CommanderAbilityController : MonoBehaviour
{
    [Header("Dados do Personagem")]
    public CharacterBase characterData;

    // --- ADICIONADO: Lógica de Carregamento da Ultimate ---
    [Header("Ultimate Charge")]
    [Tooltip("A taxa de carregamento passivo da Ultimate por segundo (ex: 0.01f = 1% por segundo).")]
    public float ultimateChargeRatePerSecond = 0.01f;

    [Tooltip("O carregamento atual da Ultimate (0.0f = 0% a 1.0f = 100%).")]
    private float currentUltimateCharge = 0f;

    // Propriedade pública para acessar o carregamento atual (PARA A HUD)
    public float CurrentUltimateCharge { get { return currentUltimateCharge; } }

    // Propriedade para verificar se a Ultimate está pronta (PARA A HUD)
    public bool IsUltimateReady { get { return currentUltimateCharge >= 1f; } }
    // --------------------------------------------------------

    private Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();

    void Start()
    {
        if (characterData.ability1 != null) abilityCooldowns[characterData.ability1] = 0f;
        if (characterData.ability2 != null) abilityCooldowns[characterData.ability2] = 0f;

        // A ultimate agora começa em 0% de carga.
        if (characterData.ultimate != null)
        {
            // Inicializa cooldowns das habilidades normais
            if (!abilityCooldowns.ContainsKey(characterData.ultimate))
            {
                abilityCooldowns[characterData.ultimate] = 0f;
            }
            // Garante que a carga comece em 0
            currentUltimateCharge = 0f;
        }
    }

    void Update()
    {
        HandleAbilityInputs();

        // --- ADICIONADO: Carregamento Passivo da Ultimate ---
        if (characterData.ultimate != null && currentUltimateCharge < 1f)
        {
            currentUltimateCharge += ultimateChargeRatePerSecond * Time.deltaTime;
            currentUltimateCharge = Mathf.Clamp01(currentUltimateCharge);
        }
        // --------------------------------------------------------
    }

    private void HandleAbilityInputs()
    {
        if (Input.GetKeyDown(KeyCode.Q) && characterData.ability1 != null)
        {
            TryActivateAbility(characterData.ability1);
        }

        if (Input.GetKeyDown(KeyCode.E) && characterData.ability2 != null)
        {
            TryActivateAbility(characterData.ability2);
        }

        if (Input.GetKeyDown(KeyCode.X) && characterData.ultimate != null)
        {
            TryActivateAbility(characterData.ultimate);
        }
    }

    private void TryActivateAbility(Ability ability)
    {
        // --- MODIFICADO: Lógica de Ativação da Ultimate ---
        if (ability == characterData.ultimate)
        {
            if (IsUltimateReady)
            {
                bool shouldGoOnCooldown = ability.Activate(this.gameObject);
                if (shouldGoOnCooldown)
                {
                    // Reinicia a carga após o uso bem-sucedido
                    currentUltimateCharge = 0f;
                    Debug.Log("Ultimate ativada com sucesso! Carga reiniciada.");
                }
            }
            else
            {
                Debug.Log($"Ultimate {ability.name} carregando: {CurrentUltimateCharge:P2}");
            }
            return; // Sai da função após lidar com a Ultimate
        }
        // --------------------------------------------------------

        // Lógica de Cooldown para Habilidades Normais (Q e E)
        if (abilityCooldowns.ContainsKey(ability) && Time.time >= abilityCooldowns[ability])
        {
            bool shouldGoOnCooldown = ability.Activate(this.gameObject);

            if (shouldGoOnCooldown)
            {
                abilityCooldowns[ability] = Time.time + ability.cooldown;
            }
        }
        else
        {
            Debug.Log("Habilidade " + ability.name + " em recarga!");
        }
    }

    // --- ADICIONADO: Método público para carregar a Ultimate baseado em eventos (dano, kill, etc.) ---
    public void AddUltimateCharge(float amount)
    {
        if (characterData.ultimate == null) return;
        currentUltimateCharge += amount;
        currentUltimateCharge = Mathf.Clamp01(currentUltimateCharge); // Garante que a carga não ultrapasse 1.0f (100%)
        // Opcional: Logar o progresso
        // Debug.Log($"Ultimate carregada em: {CurrentUltimateCharge:P2}");
    }
    // ------------------------------------------------------------------------------------------

    public void ReduceAllAbilityCooldowns(float percent)
    {
        List<Ability> abilitiesOnCooldown = new List<Ability>(abilityCooldowns.Keys);

        foreach (var ability in abilitiesOnCooldown)
        {
            // Pula a Ultimate no sistema de cooldown de tempo
            if (ability == characterData.ultimate) continue;

            float remainingTime = abilityCooldowns[ability] - Time.time;
            if (remainingTime > 0)
            {
                abilityCooldowns[ability] -= remainingTime * percent;
            }
        }
        Debug.Log("Cooldowns reduzidos em " + (percent * 100) + "%! A Ultimate usa um sistema de carga separado.");
    }
}