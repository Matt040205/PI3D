using UnityEngine;
using FMODUnity;

public class WinSound : MonoBehaviour
{
    // Define os caminhos dos eventos
    private string[] victoryEvents = new string[]
    {
        "event:/MUSiC/Victory_1",
        "event:/MUSiC/Victory_2"
    };

    void Start()
    {
        // Escolhe um índice aleatório (0 ou 1)
        int index = Random.Range(0, victoryEvents.Length);
        string eventoEscolhido = victoryEvents[index];

        // Toca o som escolhido
        RuntimeManager.PlayOneShot(eventoEscolhido);
    }
}