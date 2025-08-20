using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    // NOVO: Header e referências para a UI de Moedas
    [Header("Referências de Moeda")]
    public TMP_Text geoditesText;
    public TMP_Text darkEtherText;

    [Header("Configurações Visuais")]
    public float healthLerpSpeed = 5f;
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public Color ammoNormalColor = Color.white;
    public Color ammoLowColor = Color.yellow;
    public Color reloadColor = new Color(1, 0.5f, 0); // Laranja

    private PlayerHealthSystem playerHealth;
    private PlayerShooting playerShooting;
    private float targetHealthPercent = 1f;
    private bool isRegenerating;

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        // A lógica do jogador continua a mesma
        if (playerHealth != null && playerShooting != null)
        {
            UpdateHealthDisplay();
            UpdateAmmoDisplay();
        }

        // NOVO: Chamada para atualizar a UI de moedas a cada frame
        UpdateCurrencyDisplay();
    }

    // NOVO: Função inteira para atualizar a exibição das moedas
    void UpdateCurrencyDisplay()
    {
        // Verifica se a instância do CurrencyManager existe para evitar erros
        if (CurrencyManager.Instance != null)
        {
            // Atualiza o texto das Geoditas
            if (geoditesText != null)
            {
                // Acessa o valor diretamente da instância singleton do CurrencyManager
                geoditesText.text = $"{CurrencyManager.Instance.CurrentGeodites}";
            }

            // Atualiza o texto do Éter Negro
            if (darkEtherText != null)
            {
                darkEtherText.text = $"{CurrencyManager.Instance.CurrentDarkEther}";
            }
        }
    }

    void FindPlayer()
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
            Invoke("FindPlayer", 0.5f);
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
        healthBarFill.fillAmount = Mathf.Lerp(
            healthBarFill.fillAmount,
            targetHealthPercent,
            healthLerpSpeed * Time.deltaTime
        );

        float healthPercent = healthBarFill.fillAmount;
        Color healthColor = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
        healthBarFill.color = healthColor;

        healthText.text = $"{Mathf.CeilToInt(playerHealth.currentHealth)}/{playerHealth.characterData.maxHealth}";
        healthText.color = healthColor;

        if (regenEffect != null)
        {
            regenEffect.SetActive(isRegenerating);
        }
    }

    void UpdateAmmoDisplay()
    {
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

                reloadSlider.fillRect.GetComponent<Image>().color = Color.Lerp(
                    reloadColor,
                    Color.green,
                    reloadProgress
                );
            }
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }
    }
}