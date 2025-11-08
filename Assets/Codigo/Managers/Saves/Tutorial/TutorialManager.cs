using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TutorialPopupUI))]
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Referências")]
    public TutorialPopupUI popupUI;
    public List<TutorialData> todosOsTutoriais;

    private Dictionary<string, TutorialData> databaseTutoriais = new Dictionary<string, TutorialData>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        popupUI = GetComponent<TutorialPopupUI>();

        foreach (TutorialData tutorial in todosOsTutoriais)
        {
            if (tutorial != null)
            {
                databaseTutoriais[tutorial.tutorialID] = tutorial;
            }
        }
    }

    public void TriggerTutorial(string tutorialID)
    {
        if (GameDataManager.Instance == null) return;
        if (GameDataManager.Instance.tutoriaisConcluidos.Contains(tutorialID))
        {
            return;
        }

        if (databaseTutoriais.ContainsKey(tutorialID))
        {
            TutorialData data = databaseTutoriais[tutorialID];
            if (popupUI != null)
            {
                popupUI.Show(data);
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