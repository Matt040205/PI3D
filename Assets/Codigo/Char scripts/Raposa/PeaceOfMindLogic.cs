using UnityEngine;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

public class PeaceOfMindLogic : MonoBehaviour
{
    private PlayerHealthSystem healthSystem;
    private EventInstance curaSoundInstance;

    [Header("FMOD")]
    [EventRef]
    public string eventoCura = "event:/SFX/Cura";

    public void StartEffect(float totalHeal, float duration)
    {
        healthSystem = GetComponent<PlayerHealthSystem>();
        if (healthSystem != null)
        {
            if (!string.IsNullOrEmpty(eventoCura))
            {
                curaSoundInstance = RuntimeManager.CreateInstance(eventoCura);
                RuntimeManager.AttachInstanceToGameObject(curaSoundInstance, transform);
                curaSoundInstance.start();
            }
            StartCoroutine(HealCoroutine(totalHeal, duration));
        }
        else
        {
            Destroy(this);
        }
    }

    private IEnumerator HealCoroutine(float totalHeal, float duration)
    {
        float healPerSecond = totalHeal / duration;
        float timeLeft = duration;

        while (timeLeft > 0)
        {
            healthSystem.Heal(healPerSecond * Time.deltaTime);
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        Destroy(this);
    }

    private void OnDestroy()
    {
        if (curaSoundInstance.isValid())
        {
            curaSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            curaSoundInstance.release();
        }
    }
}