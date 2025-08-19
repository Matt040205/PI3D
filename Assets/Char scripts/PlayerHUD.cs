using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerHUD : MonoBehaviour
{
    [Header("Referências de Vida")]
    public Image healthBarFill;
    public TMP_Text healthText;
    public Image healthIcon;
    public GameObject regenEffect;

    [Header("Referências de Munição")]
    public TMP_Text ammoText;
    public Image ammoIcon;
    public GameObject reloadEffect;
    public Slider reloadSlider;

    [Header("Referências de Habilidades")]
    public Image[] abilityIcons;
    public Image[] abilityCooldownOverlays;
    public TMP_Text[] abilityKeyTexts;
    public GameObject abilityUnlockedEffect;

    [Header("Configurações Visuais")]
    public float healthLerpSpeed = 5f;
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public Color ammoNormalColor = Color.white;
    public Color ammoLowColor = Color.yellow;
    public Color reloadColor = new Color(1, 0.5f, 0);
    public Color abilityReadyColor = Color.white;
    public Color abilityCooldownColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

    private PlayerHealthSystem playerHealth;
    private PlayerShooting playerShooting;
    private AbilityManager abilityManager;
    private CharacterStatsBridge statsBridge;
    private float targetHealthPercent = 1f;
    private bool isRegenerating;
    private List<AbilitySO> lastKnownAbilities = new List<AbilitySO>();

    void Start()
    {
        FindPlayer();
        SetupAbilityUI();
    }

    void Update()
    {
        if (playerHealth == null || playerShooting == null)
            FindPlayer();

        UpdateHealthDisplay();
        UpdateAmmoDisplay();
        UpdateAbilitiesDisplay();
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealthSystem>();
            playerShooting = player.GetComponent<PlayerShooting>();
            abilityManager = player.GetComponent<AbilityManager>();
            statsBridge = player.GetComponent<CharacterStatsBridge>();

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += OnHealthChanged;
                OnHealthChanged();
            }

            if (statsBridge != null)
            {
                // CORREÇÃO: Usar OnStatsChanged em vez de OnStatsUpdated
                statsBridge.OnStatsChanged += OnStatsUpdated;
            }

            if (abilityManager != null)
            {
                abilityManager.OnAbilityUnlocked += OnAbilityUnlocked;
                abilityManager.OnAbilityUsed += OnAbilityUsed;
            }
        }
        else
        {
            Invoke("FindPlayer", 0.5f);
        }
    }

    void SetupAbilityUI()
    {
        for (int i = 0; i < abilityKeyTexts.Length; i++)
        {
            abilityKeyTexts[i].text = (i + 1).ToString();
        }
    }

    void OnHealthChanged()
    {
        if (playerHealth == null) return;

        targetHealthPercent = playerHealth.currentHealth / GetMaxHealth();
        isRegenerating = playerHealth.isRegenerating;
    }

    void OnStatsUpdated()
    {
        OnHealthChanged();
    }

    void OnAbilityUnlocked(AbilitySO ability)
    {
        if (abilityUnlockedEffect != null)
        {
            Instantiate(abilityUnlockedEffect, transform.position, Quaternion.identity);
        }

        Debug.Log($"Habilidade desbloqueada na UI: {ability.abilityName}");
    }

    void OnAbilityUsed(AbilitySO ability)
    {
        // Feedback visual quando habilidade é usada
    }

    void UpdateHealthDisplay()
    {
        healthBarFill.fillAmount = Mathf.Lerp(
            healthBarFill.fillAmount,
            targetHealthPercent,
            healthLerpSpeed * Time.deltaTime
        );

        float healthPercent = healthBarFill.fillAmount;
        Color healthColor = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
        healthBarFill.color = healthColor;

        healthText.text = $"{Mathf.CeilToInt(playerHealth.currentHealth)}/{GetMaxHealth()}";
        healthText.color = healthColor;

        if (regenEffect != null)
        {
            regenEffect.SetActive(isRegenerating);
        }
    }

    void UpdateAmmoDisplay()
    {
        ammoText.text = $"{playerShooting.currentAmmo}/{GetMagazineSize()}";

        bool isAmmoLow = playerShooting.currentAmmo <= GetMagazineSize() * 0.2f;
        ammoText.color = isAmmoLow ? ammoLowColor : ammoNormalColor;

        if (reloadEffect != null)
        {
            reloadEffect.SetActive(playerShooting.isReloading);
        }

        if (reloadSlider != null)
        {
            reloadSlider.gameObject.SetActive(playerShooting.isReloading);

            if (playerShooting.isReloading)
            {
                float reloadProgress = 1f - (playerShooting.GetRemainingReloadTime() / GetReloadSpeed());
                reloadSlider.value = reloadProgress;

                reloadSlider.fillRect.GetComponent<Image>().color = Color.Lerp(
                    reloadColor,
                    Color.green,
                    reloadProgress
                );
            }
        }
    }

    void UpdateAbilitiesDisplay()
    {
        if (abilityManager == null) return;

        for (int i = 0; i < abilityIcons.Length; i++)
        {
            if (i < abilityManager.unlockedAbilities.Count)
            {
                AbilitySO ability = abilityManager.unlockedAbilities[i];

                abilityIcons[i].sprite = ability.icon;
                abilityIcons[i].color = abilityReadyColor;

                float cooldown = abilityManager.GetCooldown(ability);
                if (cooldown > 0)
                {
                    abilityCooldownOverlays[i].fillAmount = cooldown / ability.cooldown;
                    abilityCooldownOverlays[i].color = abilityCooldownColor;
                }
                else
                {
                    abilityCooldownOverlays[i].fillAmount = 0;
                }

                abilityKeyTexts[i].gameObject.SetActive(true);
            }
            else
            {
                abilityIcons[i].color = abilityCooldownColor;
                abilityCooldownOverlays[i].fillAmount = 1f;
                abilityKeyTexts[i].gameObject.SetActive(false);
            }
        }
    }

    private float GetMaxHealth()
    {
        if (statsBridge != null && statsBridge.currentStats != null)
            return statsBridge.currentStats.maxHealth;
        return playerHealth.characterData.maxHealth;
    }

    private int GetMagazineSize()
    {
        if (statsBridge != null && statsBridge.currentStats != null)
            return statsBridge.currentStats.magazineSize;
        return playerShooting.characterData.magazineSize;
    }

    private float GetReloadSpeed()
    {
        if (statsBridge != null && statsBridge.currentStats != null)
            return statsBridge.currentStats.reloadSpeed;
        return playerShooting.characterData.reloadSpeed;
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }

        if (statsBridge != null)
        {
            // CORREÇÃO: Usar OnStatsChanged em vez de OnStatsUpdated
            statsBridge.OnStatsChanged -= OnStatsUpdated;
        }

        if (abilityManager != null)
        {
            abilityManager.OnAbilityUnlocked -= OnAbilityUnlocked;
            abilityManager.OnAbilityUsed -= OnAbilityUsed;
        }
    }
}