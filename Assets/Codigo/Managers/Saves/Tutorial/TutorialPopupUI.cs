using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialPopupUI : MonoBehaviour
{
    public GameObject painelRoot;
    public TextMeshProUGUI tituloText;
    public TextMeshProUGUI descricaoText;
    public Button botaoFechar;

    private string debug_expectedTitle = "";
    private string debug_expectedDesc = "";

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

        Debug.Log($"POPUP_UI INFO: Recebi dados para o ID: [{data.tutorialID}]");

        if (tituloText == null)
        {
            Debug.LogError("POPUP_UI ERRO: A referência 'tituloText' (TextMeshPro) está NULA.", this.gameObject);
        }
        else
        {
            debug_expectedTitle = data.titulo;
            tituloText.text = data.titulo;
            Debug.Log($"POPUP_UI INFO: Título que recebi: '{data.titulo}'. Texto agora no componente é: '{tituloText.text}'");
        }

        if (descricaoText == null)
        {
            Debug.LogError("POPUP_UI ERRO: A referência 'descricaoText' (TextMeshPro) está NULA.", this.gameObject);
        }
        else
        {
            debug_expectedDesc = data.descricao;
            descricaoText.text = data.descricao;
            Debug.Log($"POPUP_UI INFO: Descrição que recebi: '{data.descricao}'. Texto agora no componente é: '{descricaoText.text}'");
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

        debug_expectedTitle = "";
        debug_expectedDesc = "";
        Time.timeScale = 1f;

        // --- MUDANÇA CORRIGIDA AQUI ---
        // 1. Verifica se estamos na cena de Jogo (se o BuildManager existe)
        if (BuildManager.Instance != null)
        {
            // 2. Se sim, estamos no modo construção?
            if (BuildManager.isBuildingMode)
            {
                // Modo Construção: Mouse livre
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // Modo Shooter: Mouse travado
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            // 3. Se o BuildManager NÃO existe, estamos num Menu (Seleção de Personagem).
            // Modo Menu: Mouse livre
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Update()
    {
        if (painelRoot != null && painelRoot.activeSelf)
        {
            if (tituloText != null && tituloText.text != debug_expectedTitle)
            {
                Debug.LogWarning($"INTERFERÊNCIA DETECTADA (Título)! Algo mudou o texto para: '{tituloText.text}'", this.gameObject);
            }

            if (descricaoText != null && descricaoText.text != debug_expectedDesc)
            {
                Debug.LogWarning($"INTERFERÊNCIA DETECTADA (Descrição)! Algo mudou o texto para: '{descricaoText.text}'", this.gameObject);
            }
        }
    }
}