using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[System.Serializable]
public class CharacterSaveData
{
    public string characterName;
    public List<string> unlockedSkills = new List<string>();
    public int pointsSpent;
    public int pointsAvailable;

    // Status
    public float maxHealth;
    public float damage;
    public float moveSpeed;
    public float attackSpeed;
    public float armor;
    public float critChance;
    public float critDamage;
    public float armorPenetration;
}

[System.Serializable]
public class FullSaveData
{
    public List<string> tutorials = new List<string>();
    public List<CharacterSaveData> characters = new List<CharacterSaveData>();
}

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    [Header("Banco de Dados (IMPORTANTE: Arraste os Assets Originais aqui)")]
    public List<CharacterBase> bibliotecaOriginalPersonagens;

    [Header("Estado Atual")]
    public CharacterBase[] equipeSelecionada = new CharacterBase[8];
    public CharacterBase personagemParaRastros;

    [Header("Progresso dos Tutoriais")]
    public List<string> tutoriaisConcluidos = new List<string>();

    // Cache dos dados carregados
    private Dictionary<string, CharacterSaveData> loadedCharacterData = new Dictionary<string, CharacterSaveData>();
    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LimparSelecao()
    {
        Debug.Log("GAMEDATA_MANAGER: Limpando seleção e destruindo instâncias.");
        for (int i = 0; i < equipeSelecionada.Length; i++)
        {
            if (equipeSelecionada[i] != null)
            {
                Debug.Log($"GAMEDATA_MANAGER: Destruindo {equipeSelecionada[i].name}");
                Destroy(equipeSelecionada[i]);
            }
            equipeSelecionada[i] = null;
        }
    }

    // --- SISTEMA DE SAVE ---

    public void SaveGame()
    {
        FullSaveData data = new FullSaveData();

        // 1. Salva Tutoriais
        data.tutorials = new List<string>(tutoriaisConcluidos);

        // 2. Mescla dados existentes com dados atuais da equipe
        foreach (var kvp in loadedCharacterData)
        {
            data.characters.Add(kvp.Value);
        }

        foreach (CharacterBase charInstance in equipeSelecionada)
        {
            if (charInstance != null)
            {
                string cleanName = charInstance.name.Replace("(Clone)", "");

                // Remove dados antigos desse personagem para atualizar
                data.characters.RemoveAll(x => x.characterName == cleanName);

                CharacterSaveData charData = new CharacterSaveData();
                charData.characterName = cleanName;
                charData.unlockedSkills = new List<string>(charInstance.habilidadesDesbloqueadas);
                charData.pointsSpent = charInstance.pontosRastrosGastos;
                charData.pointsAvailable = charInstance.pontosRastrosDisponiveis;

                // Salva os status
                charData.maxHealth = charInstance.maxHealth;
                charData.damage = charInstance.damage;
                charData.moveSpeed = charInstance.moveSpeed;
                charData.attackSpeed = charInstance.attackSpeed;
                charData.armor = charInstance.armor;
                charData.critChance = charInstance.critChance;
                charData.critDamage = charInstance.critDamage;
                charData.armorPenetration = charInstance.armorPenetration;

                data.characters.Add(charData);

                // Atualiza cache local
                if (loadedCharacterData.ContainsKey(cleanName))
                    loadedCharacterData[cleanName] = charData;
                else
                    loadedCharacterData.Add(cleanName, charData);
            }
        }

        // Se estiver editando um personagem nos Rastros que não está na equipe, salva ele também
        if (personagemParaRastros != null)
        {
            string cleanName = personagemParaRastros.name.Replace("(Clone)", "");
            data.characters.RemoveAll(x => x.characterName == cleanName);

            CharacterSaveData charData = new CharacterSaveData();
            charData.characterName = cleanName;
            charData.unlockedSkills = new List<string>(personagemParaRastros.habilidadesDesbloqueadas);
            charData.pointsSpent = personagemParaRastros.pontosRastrosGastos;
            charData.pointsAvailable = personagemParaRastros.pontosRastrosDisponiveis;

            charData.maxHealth = personagemParaRastros.maxHealth;
            charData.damage = personagemParaRastros.damage;
            charData.moveSpeed = personagemParaRastros.moveSpeed;
            charData.attackSpeed = personagemParaRastros.attackSpeed;
            charData.armor = personagemParaRastros.armor;
            charData.critChance = personagemParaRastros.critChance;
            charData.critDamage = personagemParaRastros.critDamage;
            charData.armorPenetration = personagemParaRastros.armorPenetration;

            data.characters.Add(charData);

            if (loadedCharacterData.ContainsKey(cleanName)) loadedCharacterData[cleanName] = charData;
            else loadedCharacterData.Add(cleanName, charData);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("JOGO SALVO em: " + saveFilePath);
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                FullSaveData data = JsonUtility.FromJson<FullSaveData>(json);

                tutoriaisConcluidos = data.tutorials;

                loadedCharacterData.Clear();
                foreach (var charData in data.characters)
                {
                    if (!loadedCharacterData.ContainsKey(charData.characterName))
                    {
                        loadedCharacterData.Add(charData.characterName, charData);
                    }
                }
                Debug.Log("JOGO CARREGADO.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Erro ao carregar save: " + e.Message);
            }
        }
    }

    public void AplicarDadosCarregados(CharacterBase instanciaPersonagem)
    {
        string cleanName = instanciaPersonagem.name.Replace("(Clone)", "");

        if (loadedCharacterData.ContainsKey(cleanName))
        {
            CharacterSaveData data = loadedCharacterData[cleanName];

            instanciaPersonagem.pontosRastrosGastos = data.pointsSpent;
            instanciaPersonagem.pontosRastrosDisponiveis = data.pointsAvailable;
            instanciaPersonagem.habilidadesDesbloqueadas = new List<string>(data.unlockedSkills);

            // Restaura Status
            instanciaPersonagem.maxHealth = data.maxHealth;
            instanciaPersonagem.damage = data.damage;
            instanciaPersonagem.moveSpeed = data.moveSpeed;
            instanciaPersonagem.attackSpeed = data.attackSpeed;
            instanciaPersonagem.armor = data.armor;
            instanciaPersonagem.critChance = data.critChance;
            instanciaPersonagem.critDamage = data.critDamage;
            instanciaPersonagem.armorPenetration = data.armorPenetration;
        }
    }

    // Para Debug: Apagar Save
    [ContextMenu("Apagar Save")]
    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Save apagado!");
            tutoriaisConcluidos.Clear();
            loadedCharacterData.Clear();
        }
    }
}