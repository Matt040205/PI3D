using UnityEngine;
using UnityEngine.UI;

public class SlotEquipeUI : MonoBehaviour
{
    public GameObject plusSignObject;
    public Image characterImage;

    public void SetPersonagem(CharacterBase personagem)
    {
        if (personagem != null)
        {
            characterImage.sprite = personagem.characterIcon;
            characterImage.gameObject.SetActive(true);
            plusSignObject.SetActive(false);
        }
        else
        {
            LimparSlot();
        }
    }

    public void LimparSlot()
    {
        characterImage.sprite = null;
        characterImage.gameObject.SetActive(false);
        plusSignObject.SetActive(true);
    }
}