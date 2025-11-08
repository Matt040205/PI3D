using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Referências")]
    [Tooltip("Arraste o GameObject 'TutorialPopupPanel' (o que está desativado) aqui.")]
    public GameObject popupPanelObject;
    public List<TutorialData> todosOsTutoriais;

    private Dictionary<string, TutorialData> databaseTutoriais = new Dictionary<string, TutorialData>();
    private TutorialPopupUI popupUIScript;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Não faz DontDestroyOnLoad se o painel for da cena

        foreach (TutorialData tutorial in todosOsTutoriais)
        {
            if (tutorial != null && !databaseTutoriais.ContainsKey(tutorial.tutorialID))
            {
                databaseTutoriais.Add(tutorial.tutorialID, tutorial);
            }
        }
    }

    public void TriggerTutorial(string tutorialID)
    {
        if (GameDataManager.Instance == null || popupPanelObject == null) return;

        if (GameDataManager.Instance.tutoriaisConcluidos.Contains(tutorialID))
        {
            return;
        }

        if (databaseTutoriais.ContainsKey(tutorialID))
        {
            TutorialData data = databaseTutoriais[tutorialID];

            // 1. Ativa o GameObject do painel
            popupPanelObject.SetActive(true);

            // 2. Pega o script (que agora está ativo)
            popupUIScript = popupPanelObject.GetComponent<TutorialPopupUI>();

            if (popupUIScript != null)
            {
                // 3. Mostra o pop-up
                popupUIScript.Show(data);
            }

            ConcluirTutorial(tutorialID);
        }
    }

    private void ConcluirTutorial(string tutorialID)
    {
        if (GameDataManager.Instance != null && !GameDataManager.Instance.tutoriaisConcluidos.Contains(tutorialID))
        {
            GameDataManager.Instance.tutoriaisConcluidos.Add(tutorialID);
        }
    }
}