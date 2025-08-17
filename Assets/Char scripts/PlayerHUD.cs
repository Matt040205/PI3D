using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Refer�ncias")]
    public Image healthBar;
    public TMP_Text ammoText;

    [Header("Configura��es")]
    public float healthUpdateSpeed = 10f;

    private PlayerHealthSystem playerHealth;
    private PlayerShooting playerShooting;
    private float targetHealthPercent = 1f;

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        UpdateHealthBar();
        UpdateAmmoText();
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
                // Inscreve para receber atualiza��es de sa�de
                playerHealth.OnHealthChanged += HandleHealthChanged;
                HandleHealthChanged(); // Atualiza imediatamente
            }
        }
        else
        {
            Debug.LogWarning("Player n�o encontrado! Tentando novamente...");
            Invoke("FindPlayer", 1f);
        }
    }

    void HandleHealthChanged()
    {
        if (playerHealth != null)
        {
            targetHealthPercent = playerHealth.currentHealth / playerHealth.characterData.maxHealth;
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar == null) return;

        // Atualiza��o suave da barra de vida
        healthBar.fillAmount = Mathf.MoveTowards(
            healthBar.fillAmount,
            targetHealthPercent,
            healthUpdateSpeed * Time.deltaTime
        );
    }

    void UpdateAmmoText()
    {
        if (ammoText == null || playerShooting == null) return;

        ammoText.text = $"{playerShooting.currentAmmo}";
    }

    void OnDestroy()
    {
        // Limpa a inscri��o para evitar erros
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
        }
    }
}