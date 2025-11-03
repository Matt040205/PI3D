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

    // --- ADIÇÃO 1: Referência para o Animator ---
    private Animator animator;

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

            // --- ADIÇÃO 2: Pegar o Animator que está no modelo ---
            if (modelPivot != null)
            {
                // Assumindo que o Animator está no "samurai", que é filho do "modelPivot"
                animator = modelPivot.GetComponentInChildren<Animator>();
            }
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
        Debug.Log("<color=cyan>Habilidade Voo Gracioso ativada:</color> O próximo tiro tem o bônus. hasNextShotBonus = " + hasNextShotBonus);
    }

    void Shoot()
    {
        // --- ADIÇÃO 3: Disparar o Trigger "Shoot" ---
        if (animator != null)
        {
            // Isso vai disparar a animação na sua "Shooting Layer"
            animator.SetTrigger("Shoot");
        }

        // Você pode deletar isso se o seu firePoint já estiver posicionado
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

        // --- LÓGICA DO MÉTODO B ---

        // 1. Você precisa saber o tempo original da sua animação
        // (Infelizmente, você tem que "escrever" ele aqui)
        float originalAnimationLength = 3.0f; // Ex: sua animação tem 3 segundos

        // 2. Calcule o multiplicador
        // Se seu script quer 1.5s, o multiplicador é 3.0 / 1.5 = 2 (tocar 2x mais rápido)
        float multiplier = originalAnimationLength / characterData.reloadSpeed;

        // 3. Envie para o Animator
        if (animator != null)
        {
            animator.SetFloat("ReloadSpeedMultiplier", multiplier);
            animator.SetTrigger("Reload");
        }
        // ----------------------------

        isReloading = true;
        reloadStartTime = Time.time;
        Invoke("FinishReload", characterData.reloadSpeed); // O Invoke ainda é necessário aqui
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