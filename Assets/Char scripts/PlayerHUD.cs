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
        if (playerHealth == null || playerShooting == null) return;

        UpdateHealthDisplay();
        UpdateAmmoDisplay();
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
                OnHealthChanged(); // Atualiza imediatamente
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
        // Atualização suave da barra de vida
        healthBarFill.fillAmount = Mathf.Lerp(
            healthBarFill.fillAmount,
            targetHealthPercent,
            healthLerpSpeed * Time.deltaTime
        );

        // Atualiza cores baseado na vida
        float healthPercent = healthBarFill.fillAmount;
        Color healthColor = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
        healthBarFill.color = healthColor;

        // Atualiza texto no formato "100/100"
        healthText.text = $"{Mathf.CeilToInt(playerHealth.currentHealth)}/{playerHealth.characterData.maxHealth}";
        healthText.color = healthColor;

        // Efeito de regeneração
        if (regenEffect != null)
        {
            regenEffect.SetActive(isRegenerating);
        }
    }

    void UpdateAmmoDisplay()
    {
        // Formato "30/40"
        ammoText.text = $"{playerShooting.currentAmmo}/{playerShooting.characterData.magazineSize}";

        // Muda cor quando munição está baixa
        bool isAmmoLow = playerShooting.currentAmmo <= playerShooting.characterData.magazineSize * 0.2f;
        ammoText.color = isAmmoLow ? ammoLowColor : ammoNormalColor;

        // Mostra efeito de recarga
        if (reloadEffect != null)
        {
            reloadEffect.SetActive(playerShooting.isReloading);
        }

        // Atualiza barra de recarga
        if (reloadSlider != null)
        {
            reloadSlider.gameObject.SetActive(playerShooting.isReloading);

            if (playerShooting.isReloading)
            {
                float reloadProgress = 1f - (playerShooting.GetRemainingReloadTime() /
                                            playerShooting.characterData.reloadSpeed);
                reloadSlider.value = reloadProgress;

                // Gradiente de cor na recarga
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