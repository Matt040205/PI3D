// GameDataManager.cs
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    // Array para os 8 slots: [0] = Comandante, [1-7] = Torres
    public CharacterBase[] equipeSelecionada = new CharacterBase[8];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Limpa a seleção ao iniciar uma nova escolha
    public void LimparSelecao()
    {
        for (int i = 0; i < equipeSelecionada.Length; i++)
        {
            equipeSelecionada[i] = null;
        }
    }
}