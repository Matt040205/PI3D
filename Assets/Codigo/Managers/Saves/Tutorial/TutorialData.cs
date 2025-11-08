using UnityEngine;

[CreateAssetMenu(fileName = "NovoTutorial", menuName = "ExoBeasts/Tutorial")]
public class TutorialData : ScriptableObject
{
    [Tooltip("ID único para este tutorial. Ex: 'BUILD_TOWER' ou 'FIRST_KILL'")]
    public string tutorialID;
    public string titulo;
    [TextArea(3, 10)]
    public string descricao;
}