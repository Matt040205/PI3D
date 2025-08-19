using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SequentialFade : MonoBehaviour
{
    public List<GameObject> elements;
    public float delayBetweenElements = 0.5f;
    public float fadeDuration = 1.0f;
    public bool hideOnStart = true; // Nova variável

    void Start()
    {
        if (hideOnStart)
        {
            // Inicializa todos os elementos com alpha 0
            foreach (GameObject element in elements)
            {
                SetAlpha(element, 0f);
            }
        }

        StartCoroutine(FadeInSequence());
    }

    void SetAlpha(GameObject obj, float alpha)
    {
        // Para imagens
        Image img = obj.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }

        // Para textos tradicionais
        Text text = obj.GetComponentInChildren<Text>();
        if (text != null)
        {
            Color c = text.color;
            c.a = alpha;
            text.color = c;
        }

        // Para TextMeshPro
        TMP_Text tmpText = obj.GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
        {
            Color c = tmpText.color;
            c.a = alpha;
            tmpText.color = c;
        }
    }

    IEnumerator FadeInSequence()
    {
        foreach (GameObject element in elements)
        {
            StartCoroutine(FadeElement(element));
            yield return new WaitForSeconds(delayBetweenElements);
        }
    }

    IEnumerator FadeElement(GameObject element)
    {
        float elapsed = 0f;
        List<Graphic> graphics = new List<Graphic>();

        // Coleta todos os componentes gráficos
        graphics.AddRange(element.GetComponentsInChildren<Image>());
        graphics.AddRange(element.GetComponentsInChildren<Text>());
        graphics.AddRange(element.GetComponentsInChildren<TMP_Text>());

        // Fade in
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);

            foreach (Graphic graphic in graphics)
            {
                if (graphic != null)
                {
                    Color c = graphic.color;
                    c.a = alpha;
                    graphic.color = c;
                }
            }
            yield return null;
        }

        // Garante alpha final = 1
        foreach (Graphic graphic in graphics)
        {
            if (graphic != null)
            {
                Color c = graphic.color;
                c.a = 1f;
                graphic.color = c;
            }
        }
    }
}