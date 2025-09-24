using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Referências de Vida do Jogador")]
    public Image healthBarFill;
    public TMP_Text healthText;
    public Image healthIcon;
    public GameObject regenEffect;

    [Header("Referências de Vida do Objetivo")]
    public Image objectiveHealthBarFill;
    public TMP_Text objectiveHealthText;
    public Image BuildModeObjectiveHealthBarFill;
    public TMP_Text BuildModeObjectiveHealthText;

    [Header("Referências de Munição")]
    public TMP_Text ammoText;
    public Image ammoIcon;
    public GameObject reloadEffect;
    public Slider reloadSlider;

    [Header("Referências de Moeda")]
    public TMP_Text geoditesText;
    public TMP_Text darkEtherText;

    [Header("Configurações Visuais")]
    public float healthLerpSpeed = 5f;
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public Color ammoNormalColor = Color.white;
    public Color ammoLowColor = Color.yellow;
    public Color reloadColor = new Color(1, 0.5f, 0);

    // Referências aos sistemas do Jogador
    private PlayerHealthSystem playerHealth;
    private PlayerShooting playerShooting;
    private float targetHealthPercent = 1f;
    private bool isRegenerating;

    // Referências para o sistema do Objetivo
    private ObjectiveHealthSystem objectiveHealth;
    private float targetObjectiveHealthPercent = 1f;


    void Start()
    {
        FindGameSystems();
    }

    void Update()
    {
        if (playerHealth != null)
        {
            UpdateHealthDisplay();
        }
        if (playerShooting != null)
        {
            UpdateAmmoDisplay();
        }
        UpdateObjectiveHealthDisplay();
        UpdateCurrencyDisplay();
    }

    void UpdateCurrencyDisplay()
    {
        if (CurrencyManager.Instance != null)
        {
            if (geoditesText != null)
            {
                geoditesText.text = $"{CurrencyManager.Instance.CurrentGeodites}";
            }
            if (darkEtherText != null)
            {
                darkEtherText.text = $"{CurrencyManager.Instance.CurrentDarkEther}";
            }
        }
    }

    void FindGameSystems()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealthSystem>();
            playerShooting = player.GetComponent<PlayerShooting>();

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += OnHealthChanged;
                OnHealthChanged();
            }
        }
        else
        {
            Invoke("FindGameSystems", 0.5f);
            return;
        }

        GameObject objective = GameObject.FindGameObjectWithTag("Objective");
        if (objective != null)
        {
            objectiveHealth = objective.GetComponent<ObjectiveHealthSystem>();
            if (objectiveHealth != null)
            {
                objectiveHealth.OnHealthChanged += OnObjectiveHealthChanged;
                OnObjectiveHealthChanged();
            }
        }
    }

    void OnHealthChanged()
    {
        if (playerHealth == null) return;
        targetHealthPercent = playerHealth.currentHealth / playerHealth.characterData.maxHealth;
        isRegenerating = playerHealth.isRegenerating;
    }

    void UpdateHealthDisplay()
    {
        healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetHealthPercent, healthLerpSpeed * Time.deltaTime);
        healthText.text = $"{Mathf.CeilToInt(playerHealth.currentHealth)}/{playerHealth.characterData.maxHealth}";
        if (regenEffect != null)
        {
            regenEffect.SetActive(isRegenerating);
        }
    }

    void OnObjectiveHealthChanged()
    {
        if (objectiveHealth == null) return;
        targetObjectiveHealthPercent = objectiveHealth.currentHealth / objectiveHealth.maxHealth;
    }

    void UpdateObjectiveHealthDisplay()
    {
        // Ambos os elementos de UI são ativados para que fiquem visíveis o tempo todo.
        if (objectiveHealthBarFill != null) objectiveHealthBarFill.gameObject.SetActive(true);
        if (objectiveHealthText != null) objectiveHealthText.gameObject.SetActive(true);
        if (BuildModeObjectiveHealthBarFill != null) BuildModeObjectiveHealthBarFill.gameObject.SetActive(true);
        if (BuildModeObjectiveHealthText != null) BuildModeObjectiveHealthText.gameObject.SetActive(true);

        // Atualiza a barra de vida normal
        if (objectiveHealthBarFill != null)
        {
            objectiveHealthBarFill.fillAmount = Mathf.Lerp(objectiveHealthBarFill.fillAmount, targetObjectiveHealthPercent, healthLerpSpeed * Time.deltaTime);
        }
        if (objectiveHealthText != null)
        {
            objectiveHealthText.text = $"{Mathf.CeilToInt(objectiveHealth.currentHealth)}/{objectiveHealth.maxHealth}";
        }

        // Atualiza a barra de vida do modo de construção
        if (BuildModeObjectiveHealthBarFill != null)
        {
            BuildModeObjectiveHealthBarFill.fillAmount = Mathf.Lerp(BuildModeObjectiveHealthBarFill.fillAmount, targetObjectiveHealthPercent, healthLerpSpeed * Time.deltaTime);
        }
        if (BuildModeObjectiveHealthText != null)
        {
            BuildModeObjectiveHealthText.text = $"{Mathf.CeilToInt(objectiveHealth.currentHealth)}/{objectiveHealth.maxHealth}";
        }
    }

    void UpdateAmmoDisplay()
    {
        if (playerShooting == null) return;

        ammoText.text = $"{playerShooting.currentAmmo}/{playerShooting.characterData.magazineSize}";

        bool isAmmoLow = playerShooting.currentAmmo <= playerShooting.characterData.magazineSize * 0.2f;
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
                float reloadProgress = 1f - (playerShooting.GetRemainingReloadTime() /
                                            playerShooting.characterData.reloadSpeed);
                reloadSlider.value = reloadProgress;

                Image sliderFillImage = reloadSlider.fillRect.GetComponent<Image>();
                if (sliderFillImage != null)
                {
                    sliderFillImage.color = Color.Lerp(
                        reloadColor,
                        Color.green,
                        reloadProgress
                    );
                }
            }
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }
        if (objectiveHealth != null)
        {
            objectiveHealth.OnHealthChanged -= OnObjectiveHealthChanged;
        }
    }
}