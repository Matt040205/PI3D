using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Refer�ncias de Vida")]
    public Image healthBarFill;
    public TMP_Text healthText;
    public Image healthIcon;
    public GameObject regenEffect;

    [Header("Refer�ncias de Muni��o")]
    public TMP_Text ammoText;
    public Image ammoIcon;
    public GameObject reloadEffect;
    public Slider reloadSlider;

    // NOVO: Header e refer�ncias para a UI de Moedas
    [Header("Refer�ncias de Moeda")]
    public TMP_Text geoditesText;
    public TMP_Text darkEtherText;

    [Header("Configura��es Visuais")]
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
        // A l�gica do jogador continua a mesma
        if (playerHealth != null && playerShooting != null)
        {
            UpdateHealthDisplay();
            UpdateAmmoDisplay();
        }

        // NOVO: Chamada para atualizar a UI de moedas a cada frame
        UpdateCurrencyDisplay();
    }

    // NOVO: Fun��o inteira para atualizar a exibi��o das moedas
    void UpdateCurrencyDisplay()
    {
        // Verifica se a inst�ncia do CurrencyManager existe para evitar erros
        if (CurrencyManager.Instance != null)
        {
            // Atualiza o texto das Geoditas
            if (geoditesText != null)
            {
                // Acessa o valor diretamente da inst�ncia singleton do CurrencyManager
                geoditesText.text = $"{CurrencyManager.Instance.CurrentGeodites}";
            }

            // Atualiza o texto do �ter Negro
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