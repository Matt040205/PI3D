using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TutorialReviewUI : MonoBehaviour
{
    [Header("Controlo")]
    public KeyCode reviewKey = KeyCode.T;
    public GameObject panelRoot;

    [Header("Referências da UI")]
    [Tooltip("O objeto que contém o Vertical Layout Group para a lista")]
    public Transform listaContainer;
    [Tooltip("O Prefab do botão que será instanciado na lista")]
    public GameObject tutorialButtonPrefab;

    [Header("Painel de Detalhes")]
    public TextMeshProUGUI nomeTutorialText;
    public TextMeshProUGUI descricaoTutorialText;
    public Button botaoFechar;

    private bool isPanelOpen = false;

    void Start()
    {
        if (botaoFechar != null)
        {
            botaoFechar.onClick.AddListener(ClosePanel);
        }
        panelRoot.SetActive(false);

        LimparDetalhes();
    }

    void Update()
    {
        if (Input.GetKeyDown(reviewKey))
        {
            TogglePanel();
        }
    }

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        panelRoot.SetActive(isPanelOpen);

        if (isPanelOpen)
        {
            Time.timeScale = 0f;
            PopularLista();
            LimparDetalhes();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void ClosePanel()
    {
        isPanelOpen = false;
        panelRoot.SetActive(isPanelOpen);
        Time.timeScale = 1f;
    }

    void PopularLista()
    {
        foreach (Transform child in listaContainer)
        {
            Destroy(child.gameObject);
        }

        if (TutorialManager.Instance == null || GameDataManager.Instance == null)
        {
            Debug.LogError("TutorialReviewUI: TutorialManager ou GameDataManager não encontrados!");
            return;
        }

        List<TutorialData> todosOsTutoriais = TutorialManager.Instance.todosOsTutoriais;
        List<string> concluidos = GameDataManager.Instance.tutoriaisConcluidos;

        foreach (TutorialData tutorial in todosOsTutoriais)
        {
            if (concluidos.Contains(tutorial.tutorialID))
                {
                GameObject botaoGO = Instantiate(tutorialButtonPrefab, listaContainer);

                TextMeshProUGUI botaoText = botaoGO.GetComponentInChildren<TextMeshProUGUI>();
                if (botaoText != null)
                {
                    botaoText.text = tutorial.titulo;
                }

                Button botaoComponent = botaoGO.GetComponent<Button>();
                if (botaoComponent != null)
                {
                    TutorialData dataParaEsteBotao = tutorial;
                    botaoComponent.onClick.AddListener(() => {
                        MostrarDetalhes(dataParaEsteBotao);
                    });
                }
            }
        }
    }

    void MostrarDetalhes(TutorialData data)
    {
        if (data == null) return;
        nomeTutorialText.text = data.titulo;
        descricaoTutorialText.text = data.descricao;
    }

    void LimparDetalhes()
    {
        nomeTutorialText.text = "Selecione um Tutorial";
        descricaoTutorialText.text = "Selecione um item da lista para ler a descrição.";
    }
}