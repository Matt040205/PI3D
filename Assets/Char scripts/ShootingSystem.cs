using System.ComponentModel.Design;
using UnityEngine;

/*public class ShootingSystem : MonoBehaviour
{
    [Header("Referências")]
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Estado")]
    public bool isReloading;
    public float reloadProgress;

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && !isReloading)
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        if (commander.currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        commander.currentAmmo--;

        // Atualiza UI
        AmmoUI.UpdateAmmo(commander.currentAmmo, characterData.magazineSize);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        reloadProgress = 0;

        while (reloadProgress < 1)
        {
            reloadProgress += Time.deltaTime / characterData.reloadSpeed;
            // Atualiza UI de recarga
            yield return null;
        }

        commander.currentAmmo = characterData.magazineSize;
        isReloading = false;
    }
}*/