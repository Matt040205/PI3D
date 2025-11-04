using UnityEngine;

public class Broca : MonoBehaviour
{
    public int geodidasPorCiclo = 5;
    public float tempoPorCiclo = 10f;

    void Start()
    {
        StartCoroutine(GerarGeodidas());
    }

    private System.Collections.IEnumerator GerarGeodidas()
    {
        while (true)
        {
            yield return new WaitForSeconds(tempoPorCiclo);

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCurrency(geodidasPorCiclo, CurrencyType.Geodites);
            }
        }
    }
}