using UnityEngine;

public abstract class TrapLogicBase : MonoBehaviour
{
    public TrapDataSO trapData;
    public float sellRefundPercentage = 0.6f;
    private bool isBeingSoldOrDestroyed = false;

    public virtual void SellTrap()
    {
        if (isBeingSoldOrDestroyed) return;
        isBeingSoldOrDestroyed = true;

        if (trapData == null)
        {
            Debug.LogError("TrapLogicBase: trapData é nulo. Não é possível vender. Destruindo mesmo assim.");
            Destroy(gameObject);
            return;
        }

        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.DeregisterTrap(trapData);
        }

        if (CurrencyManager.Instance != null)
        {
            int geoditeRefund = Mathf.FloorToInt(trapData.geoditeCost * sellRefundPercentage);
            int etherRefund = Mathf.FloorToInt(trapData.darkEtherCost * sellRefundPercentage);

            if (geoditeRefund > 0)
            {
                CurrencyManager.Instance.AddCurrency(geoditeRefund, CurrencyType.Geodites);
            }
            if (etherRefund > 0)
            {
                CurrencyManager.Instance.AddCurrency(etherRefund, CurrencyType.DarkEther);
            }
        }

        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (isBeingSoldOrDestroyed) return;
        isBeingSoldOrDestroyed = true;

        if (BuildManager.Instance != null && trapData != null)
        {
            BuildManager.Instance.DeregisterTrap(trapData);
        }
    }
}