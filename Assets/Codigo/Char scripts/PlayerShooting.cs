using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Configurações")]
    public CharacterBase characterData;
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
    private Camera mainCamera;

    // Variáveis para o bônus de habilidade
    private bool hasNextShotBonus = false;
    private float nextShotDamageBonus = 1f;
    private float nextShotAreaBonus = 1f;

    void Start()
    {
        currentAmmo = characterData.magazineSize;
        cameraController = FindObjectOfType<CameraController>();
        mainCamera = Camera.main;

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            modelPivot = playerMovement.GetModelPivot();
        }

        projectilePool = ProjectilePool.Instance;

        if (projectilePool == null && projectileVisualPrefab != null)
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
        if (PauseControl.isPaused)
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
        Debug.Log("<color=cyan>Habilidade Voo Gracioso ativada:</color> O próximo tiro tem o bônus. hasNextShotBonus = " + hasNextShotBonus);
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
        bool hasHit = Physics.Raycast(
            firePoint.position,
            shotDirection,
            out hit,
            maxDistance,
            hitLayers
        );

        Vector3 hitPosition = hasHit ? hit.point : firePoint.position + shotDirection * maxDistance;

        if (hasHit)
        {
            EnemyHealthSystem enemyHealth = hit.collider.GetComponent<EnemyHealthSystem>();
            if (enemyHealth != null)
            {
                float finalDamage = characterData.damage;

                if (hasNextShotBonus)
                {
                    finalDamage *= nextShotDamageBonus;
                    Debug.Log("<color=yellow>BÔNUS APLICADO:</color> Dano do tiro foi aumentado de " + characterData.damage + " para " + finalDamage + ".");

                    // Reseta os bônus após o tiro
                    hasNextShotBonus = false;
                    nextShotDamageBonus = 1f;
                    nextShotAreaBonus = 1f;
                }

                enemyHealth.TakeDamage(finalDamage);
                Debug.Log("Acertou inimigo: " + hit.collider.name + " com " + finalDamage + " de dano");
            }
            else
            {
                Debug.Log("Acertou algo que não é inimigo: " + hit.collider.name);
            }
        }
        else
        {
            //  Debug.Log("Tiro disparado mas não acertou nada");
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

        nextShotTime = Time.time + (1f / characterData.attackSpeed);
        currentAmmo--;
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