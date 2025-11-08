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
        botaoFechar.onClick.AddListener(Fechar);
        // Não desative o painel aqui, ele já começa desativado na hierarquia
    }

    public void Show(TutorialData data)
    {
        tituloText.text = data.titulo;
        descricaoText.text = data.descricao;

        // O TutorialManager já ativou o painel,
        // mas garantimos que o painelRoot (a referência a si mesmo) está ativo.
        if (painelRoot != null)
            painelRoot.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Fechar()
    {
        if (painelRoot != null)
            painelRoot.SetActive(false);

        Time.timeScale = 1f;
    }
}