using UnityEngine;
using FMODUnity;

public class LoseSound : MonoBehaviour
{
    // Define os caminhos dos eventos
    private string[] defeatEvents = new string[]
    {
        "event:/MUSiC/Defeat_1",
        "event:/MUSiC/Defeat_2"
    };

    void Start()
    {
        // Escolhe um índice aleatório (0 ou 1)
        int index = Random.Range(0, defeatEvents.Length);
        string eventoEscolhido = defeatEvents[index];

        // Toca o som escolhido
        RuntimeManager.PlayOneShot(eventoEscolhido);
    }
}