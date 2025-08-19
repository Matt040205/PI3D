using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    // Mantém referência ao CharacterBase original
    public CharacterBase characterData;

    [Header("Referências")]
    public Transform firePoint;
    public GameObject projectileVisualPrefab;
    public GameObject impactEffectPrefab;

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
    private CharacterStatsBridge statsBridge;

    void Start()
    {
        statsBridge = GetComponent<CharacterStatsBridge>();

        if (statsBridge != null && statsBridge.currentStats != null)
        {
            currentAmmo = statsBridge.currentStats.magazineSize;
        }
        else if (characterData != null)
        {
            currentAmmo = characterData.magazineSize;
        }

        cameraController = FindObjectOfType<CameraController>();

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            modelPivot = playerMovement.GetModelPivot();
        }

        projectilePool = ProjectilePool.Instance;
        if (projectilePool == null)
        {
            GameObject poolObj = new GameObject("ProjectilePool");
            projectilePool = poolObj.AddComponent<ProjectilePool>();
            projectilePool.projectilePrefab = projectileVisualPrefab;
            ProjectilePool.Instance = projectilePool;
            projectilePool.InitializePool();
        }
    }

    void Update()
    {
        if (isReloading) return;

        if (GetFireMode() == FireMode.FullAuto)
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

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < GetMagazineSize())
        {
            StartReload();
        }
    }

    void Shoot()
    {
        if (modelPivot != null)
        {
            firePoint.position = modelPivot.position + modelPivot.forward * 0.5f;
            firePoint.rotation = modelPivot.rotation;
        }

        Vector3 shotDirection = GetShotDirection();
        RaycastHit hit;
        bool hasHit = Physics.Raycast(firePoint.position, shotDirection, out hit, maxDistance, hitLayers);

        Vector3 hitPosition = hasHit ? hit.point : firePoint.position + shotDirection * maxDistance;

        if (hasHit)
        {
            EnemyHealthSystem health = hit.collider.GetComponent<EnemyHealthSystem>();
            if (health != null)
            {
                health.TakeDamage(GetDamage());
            }
        }

        if (projectilePool != null && projectileVisualPrefab != null)
        {
            GameObject visualProjectile = projectilePool.GetProjectile(
                firePoint.position,
                Quaternion.LookRotation(shotDirection)
            );

            ProjectileVisual visualScript = visualProjectile.GetComponent<ProjectileVisual>();
            if (visualScript != null)
            {
                visualScript.Initialize(hitPosition);
            }
        }

        nextShotTime = Time.time + (1f / GetAttackSpeed());
        currentAmmo--;
    }

    Vector3 GetShotDirection()
    {
        if (cameraController != null && cameraController.isAiming)
        {
            return cameraController.GetAimDirection();
        }
        else if (modelPivot != null)
        {
            return modelPivot.forward;
        }

        return transform.forward;
    }

    void StartReload()
    {
        isReloading = true;
        reloadStartTime = Time.time;
        Invoke("FinishReload", GetReloadSpeed());
    }

    void FinishReload()
    {
        currentAmmo = GetMagazineSize();
        isReloading = false;
    }

    public float GetRemainingReloadTime()
    {
        if (!isReloading) return 0;
        return GetReloadSpeed() - (Time.time - reloadStartTime);
    }

    // Métodos para acessar valores com suporte a upgrades
    private float GetDamage()
    {
        if (statsBridge != null && statsBridge.currentStats != null)
            return statsBridge.currentStats.damage;
        return characterData.damage;
    }

    private float GetAttackSpeed()
    {
        if (statsBridge != null && statsBridge.currentStats != null)
            return statsBridge.currentStats.attackSpeed;
        return characterData.attackSpeed;
    }

    private float GetReloadSpeed()
    {
        if (statsBridge != null && statsBridge.currentStats != null)
            return statsBridge.currentStats.reloadSpeed;
        return characterData.reloadSpeed;
    }

    private int GetMagazineSize()
    {
        if (statsBridge != null && statsBridge.currentStats != null)
            return statsBridge.currentStats.magazineSize;
        return characterData.magazineSize;
    }

    private FireMode GetFireMode()
    {
        // FireMode não é afetado por upgrades, sempre usa o base
        return characterData.fireMode;
    }

    // Chamado quando os stats são atualizados
    public void OnStatsUpdated(CharacterStats stats)
    {
        // Ajusta a munição atual se o tamanho do pente diminuiu
        if (currentAmmo > stats.magazineSize)
            currentAmmo = stats.magazineSize;

        Debug.Log($"Sistema de tiro atualizado. Dano: {stats.damage}, Velocidade: {stats.attackSpeed}");
    }
}