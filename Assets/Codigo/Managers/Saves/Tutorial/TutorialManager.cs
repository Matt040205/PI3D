using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Referncias")]
    [Tooltip("Arraste o GameObject 'TutorialPopupPanel' (o que est desativado) aqui.")]
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

        foreach (TutorialData tutorial in todosOsTutoriais)
        {
            if (tutorial != null && !string.IsNullOrEmpty(tutorial.tutorialID))
            {
                if (!databaseTutoriais.ContainsKey(tutorial.tutorialID))
                {
                    databaseTutoriais.Add(tutorial.tutorialID, tutorial);
                }
                else
                {
                    // DEBUG: Avisa se tiver IDs duplicados
                    Debug.LogWarning($"TUTORIAL_MANAGER: ID Duplicado encontrado na lista 'todosOsTutoriais': {tutorial.tutorialID}. Apenas o primeiro será usado.");
                }
            }
        }
    }

    public void TriggerTutorial(string tutorialID)
    {
        Debug.Log($"TUTORIAL_MANAGER: Recebido gatilho para ID: [{tutorialID}]"); // DEBUG

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("TUTORIAL_MANAGER: GameDataManager.Instance é NULO.", this.gameObject);
            return;
        }

        if (popupPanelObject == null)
        {
            Debug.LogError("TUTORIAL_MANAGER: 'popupPanelObject' não foi anexado no Inspetor!", this.gameObject);
            return;
        }

        if (GameDataManager.Instance.tutoriaisConcluidos.Contains(tutorialID))
        {
            Debug.Log($"TUTORIAL_MANAGER: Tutorial [{tutorialID}] já foi concluído. Ignorando.", this.gameObject);
            return;
        }

        if (databaseTutoriais.ContainsKey(tutorialID))
        {
            TutorialData data = databaseTutoriais[tutorialID];

            // ----- DEBUG CHAVE -----
            if (data == null)
            {
                Debug.LogError($"TUTORIAL_MANAGER: Encontrei o ID [{tutorialID}], mas o Scriptable Object (TutorialData) associado é NULO!", this.gameObject);
                return;
            }

            Debug.Log($"TUTORIAL_MANAGER: Título que estou enviando: '{data.titulo}'");
            Debug.Log($"TUTORIAL_MANAGER: Descrição que estou enviando: '{data.descricao}'");
            // ----- FIM DO DEBUG -----

            popupPanelObject.SetActive(true);
            popupUIScript = popupPanelObject.GetComponent<TutorialPopupUI>();

            if (popupUIScript != null)
            {
                Debug.Log("TUTORIAL_MANAGER: Enviando dados para o popupUIScript.Show()");
                popupUIScript.Show(data);
            }
            else
            {
                Debug.LogError($"TUTORIAL_MANAGER: Ativei o 'popupPanelObject', mas não encontrei o script 'TutorialPopupUI' nele.", popupPanelObject);
            }

            ConcluirTutorial(tutorialID);
        }
        else
        {
            Debug.LogError($"TUTORIAL_MANAGER: O ID [{tutorialID}] não foi encontrado no 'databaseTutoriais'. Verifique se o Scriptable Object está na lista 'todosOsTutoriais' e se o ID está escrito CORRETAMENTE.", this.gameObject);
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