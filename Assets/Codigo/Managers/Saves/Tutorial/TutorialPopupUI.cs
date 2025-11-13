using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialPopupUI : MonoBehaviour
{
    public GameObject painelRoot;
    public TextMeshProUGUI tituloText;
    public TextMeshProUGUI descricaoText;
    public Button botaoFechar;

    void Awake()
    {
        if (botaoFechar != null)
        {
            botaoFechar.onClick.AddListener(Fechar);
        }
        else
        {
            Debug.LogError("POPUP_UI ERRO: O 'botaoFechar' não foi anexado no Inspetor!", this.gameObject);
        }
    }

    public void Show(TutorialData data)
    {
        if (data == null)
        {
            Debug.LogError("POPUP_UI ERRO: Show() foi chamado, mas o 'TutorialData' (data) veio NULO.", this.gameObject);
            Time.timeScale = 0f;
            return;
        }

        if (tituloText == null)
        {
            Debug.LogError("POPUP_UI ERRO: A referência 'tituloText' (TextMeshPro) está NULA.", this.gameObject);
        }
        else
        {
            tituloText.text = data.titulo;
        }

        if (descricaoText == null)
        {
            Debug.LogError("POPUP_UI ERRO: A referência 'descricaoText' (TextMeshPro) está NULA.", this.gameObject);
        }
        else
        {
            descricaoText.text = data.descricao;
        }

        if (painelRoot != null)
            painelRoot.SetActive(true);
        else
            this.gameObject.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Fechar()
    {
        if (painelRoot != null)
            painelRoot.SetActive(false);
        else
            this.gameObject.SetActive(false);

        Time.timeScale = 1f;

        if (BuildManager.Instance != null)
        {
            if (BuildManager.isBuildingMode)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}