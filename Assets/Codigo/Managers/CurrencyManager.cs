using UnityEngine;
using TMPro;

public enum CurrencyType
{
    Geodites,
    DarkEther
}

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Referências da UI")]
    public TextMeshProUGUI geoditesTextBuild;
    public TextMeshProUGUI geoditesText;
    public TextMeshProUGUI darkEtherText;

    [Header("Valores Iniciais")]
    [SerializeField] private int initialGeodites = 500;
    [SerializeField] private int initialDarkEther = 0;

    public int CurrentGeodites { get; private set; }
    public int CurrentDarkEther { get; private set; }

    private bool jaGanhouRecursoTutorial = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CurrentGeodites = initialGeodites;
        CurrentDarkEther = initialDarkEther;
        UpdateUI();
    }

    public void AddCurrency(int amount, CurrencyType type)
    {
        if (amount <= 0)
        {
            UpdateUI();
            return;
        }

        switch (type)
        {
            case CurrencyType.Geodites:
                CurrentGeodites += amount;
                break;
            case CurrencyType.DarkEther:
                CurrentDarkEther += amount;
                break;
        }

        if (!jaGanhouRecursoTutorial && GameDataManager.Instance != null && GameDataManager.Instance.tutoriaisConcluidos.Contains("USE_SKILLS"))
        {
            jaGanhouRecursoTutorial = true;
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.TriggerTutorial("EXPLAIN_UPGRADE");
            }
        }

        UpdateUI();
    }

    public bool HasEnoughCurrency(int amount, CurrencyType type)
    {
        switch (type)
        {
            case CurrencyType.Geodites:
                return CurrentGeodites >= amount;
            case CurrencyType.DarkEther:
                return CurrentDarkEther >= amount;
            default:
                return false;
        }
    }

    public void SpendCurrency(int amount, CurrencyType type)
    {
        if (HasEnoughCurrency(amount, type))
        {
            switch (type)
            {
                case CurrencyType.Geodites:
                    CurrentGeodites -= amount;
                    break;
                case CurrencyType.DarkEther:
                    CurrentDarkEther -= amount;
                    break;
            }
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (geoditesText != null)
        {
            geoditesText.text = $"Geoditas: {CurrentGeodites}";
        }

        if (geoditesTextBuild != null)
        {
            geoditesTextBuild.text = $" {CurrentGeodites}";
        }

        if (darkEtherText != null)
        {
            darkEtherText.text = $"Éter Negro: {CurrentDarkEther}";
        }
    }
}