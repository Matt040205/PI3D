using UnityEngine;

public abstract class TrapLogicBase : MonoBehaviour
{
    public TrapDataSO trapData;

    protected virtual void OnDestroy()
    {
        if (BuildManager.Instance != null && trapData != null)
        {
            BuildManager.Instance.DeregisterTrap(trapData);
        }
    }
}