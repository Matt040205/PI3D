using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
using static Unity.VisualScripting.Member;

[RequireComponent(typeof(PlayerHealthSystem))]
public class PlayerShooting : MonoBehaviour
{
    [Header("Configurações")]
    public CharacterBase characterData;
    public Transform firePoint;
    public GameObject projectileVisualPrefab;
    public GameObject impactEffectPrefab;

    [Header("Configurações FMOD")]
    [Tooltip("Escreva 'Arma' ou 'Arco' para definir o som. (Deve corresponder exatamente)")]
    public string tipoDeSom = "Arma";

    [Header("FMOD - Sons da Arma")]
    [EventRef]
    public string eventoTiroUnicoArma = "event:/SFX/Atirar";
    [EventRef]
    public string eventoTiroContinuoArma = "event:/SFX/Atirar_segurando";
    [EventRef]
    public string eventoRecargaArma = "event:/SFX/Recarga Arma";

    [Header("FMOD - Sons do Arco")]
    [EventRef]
    public string eventoTiroUnicoArco = "event:/SFX/Arco";
    [EventRef]
    public string eventoTiroContinuoArco = "event:/SFX/Arco";

    [Header("Raycast Settings")]
    public float maxDistance = 100f;
    public LayerMask hitLayers;

    [Header("Estado")]
    public int currentAmmo;
    public bool isReloading;
    public bool isFiring;
    public float reloadStartTime;

    private float nextShotTime;
    private CameraController cameraController;
    private Transform modelPivot;
    private ProjectilePool projectilePool;
    private Camera mainCamera;
    private Animator animator;
    private PlayerHealthSystem playerHealth;

    private bool hasNextShotBonus = false;
    private float nextShotDamageBonus = 1f;
    private float nextShotAreaBonus = 1f;

    void Start()
    {
        currentAmmo = characterData.magazineSize;
        cameraController = FindObjectOfType<CameraController>();
        mainCamera = Camera.main;
        playerHealth = GetComponent<PlayerHealthSystem>();

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            modelPivot = playerMovement.GetModelPivot();

            if (modelPivot != null)
            {
                animator = modelPivot.GetComponentInChildren<Animator>();
            }
        }

        projectilePool = ProjectilePool.Instance;

        if (projectilePool != null)
        {
            if (projectileVisualPrefab != null)
            {
                projectilePool.projectilePrefab = this.projectileVisualPrefab;
                projectilePool.InitializePool();
            }
            else
            {
                Debug.LogError($"PlayerShooting no {gameObject.name} não tem um 'projectileVisualPrefab' definido!");
            }
        }
        else
        {
            Debug.LogError("ProjectilePool.Instance não foi encontrado na cena!");
        }
    }

    void Update()
    {
        if (PauseControl.isPaused || BuildManager.isBuildingMode)
        {
            return;
        }

        if (isReloading) return;

        if (characterData.fireMode == FireMode.FullAuto)
        {
            if (Input.GetButton("Fire1") && Time.time >= nextShotTime)
            {
                if (currentAmmo > 0)
                {
                    Shoot();
                }
                else
                {
                    StartReload();
                }
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1") && Time.time >= nextShotTime)
            {
                if (currentAmmo > 0)
                {
                    Shoot();
                }
                else
                {
                    StartReload();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < characterData.magazineSize)
        {
            StartReload();
        }
    }

    public void SetNextShotBonus(float damageBonus, float areaBonus)
    {
        hasNextShotBonus = true;
        nextShotDamageBonus = damageBonus;
        nextShotAreaBonus = areaBonus;
    }

    void Shoot()
    {
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        if (tipoDeSom == "Arco")
        {
            if (characterData.fireMode == FireMode.FullAuto)
            {
                if (!string.IsNullOrEmpty(eventoTiroContinuoArco))
                    RuntimeManager.PlayOneShot(eventoTiroContinuoArco, transform.position);
            }
            else
            {
                if (!string.IsNullOrEmpty(eventoTiroUnicoArco))
                    RuntimeManager.PlayOneShot(eventoTiroUnicoArco, transform.position);
            }
        }
        else
        {
            if (characterData.fireMode == FireMode.FullAuto)
            {
                if (!string.IsNullOrEmpty(eventoTiroContinuoArma))
                    RuntimeManager.PlayOneShot(eventoTiroContinuoArma, transform.position);
            }
            else
            {
                if (!string.IsNullOrEmpty(eventoTiroUnicoArma))
                    RuntimeManager.PlayOneShot(eventoTiroUnicoArma, transform.position);
            }
        }

        if (modelPivot != null)
        {
            firePoint.position = modelPivot.position + modelPivot.forward * 0.5f;
            firePoint.rotation = modelPivot.rotation;
        }

        Vector3 shotDirection = GetShotDirection();

        float finalDamage = characterData.damage;
        bool isCritical = false;

        if (Random.value <= characterData.critChance)
        {
            finalDamage *= characterData.critDamage;
            isCritical = true;
        }

        if (hasNextShotBonus)
        {
            finalDamage *= nextShotDamageBonus;
            hasNextShotBonus = false;
            nextShotDamageBonus = 1f;
            nextShotAreaBonus = 1f;
        }

        if (projectilePool != null)
        {
            GameObject visualProjectile = projectilePool.GetProjectile(
             firePoint.position,
             Quaternion.LookRotation(shotDirection));

            if (visualProjectile != null)
            {
                ProjectileVisual visualScript = visualProjectile.GetComponent<ProjectileVisual>();
                if (visualScript != null)
                {
                    visualScript.Initialize(
                     finalDamage,
                     isCritical,
                     characterData.armorPenetration,
                     playerHealth,
                     shotDirection
                    );
                }
            }
        }

        nextShotTime = Time.time + (1f / characterData.attackSpeed);
        currentAmmo--;

        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }

    Vector3 GetShotDirection()
    {
        if (cameraController != null && cameraController.isAiming)
        {
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance, hitLayers))
            {
                return (hit.point - firePoint.position).normalized;
            }
            else
            {
                return ray.direction;
            }
        }

       else if (modelPivot != null)
        {
            return modelPivot.forward;
        }

        return transform.forward;
    }

    void StartReload()
    {
        if (isReloading) return;

        float originalAnimationLength = 3.0f;
        float multiplier = originalAnimationLength / characterData.reloadSpeed;

        if (animator != null)
        {
            animator.SetFloat("ReloadSpeedMultiplier", multiplier);
            animator.SetTrigger("Reload");
        }

        if (tipoDeSom == "Arma" && !string.IsNullOrEmpty(eventoRecargaArma))
        {
            RuntimeManager.PlayOneShot(eventoRecargaArma, transform.position);
        }

        isReloading = true;
        reloadStartTime = Time.time;
        Invoke("FinishReload", characterData.reloadSpeed);
          }

    void FinishReload()
    {
        currentAmmo = characterData.magazineSize;
        isReloading = false;
    }

    public float GetRemainingReloadTime()
    {
        if (!isReloading) return 0;
        return characterData.reloadSpeed - (Time.time - reloadStartTime);
    }
}