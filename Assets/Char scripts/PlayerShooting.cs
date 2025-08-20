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

    void Start()
    {
        currentAmmo = characterData.magazineSize;
        cameraController = FindObjectOfType<CameraController>();
        mainCamera = Camera.main;

        // Busca a referência do modelPivot
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            modelPivot = playerMovement.GetModelPivot();
        }

        // Busca os pools
        projectilePool = ProjectilePool.Instance;

        // Cria pools se não existirem
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
        if (isReloading) return;

        // Controle de disparo baseado no modo de fogo
        if (characterData.fireMode == FireMode.FullAuto)
        {
            // Disparo automático: atira enquanto o botão estiver pressionado
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
        else // FireMode.SemiAuto
        {
            // Disparo semi-automático: atira uma vez por clique
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

        // Recarga manual com tecla R
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < characterData.magazineSize)
        {
            StartReload();
        }
    }

    void Shoot()
    {
        // Atualiza a posição do firePoint baseado no modelo
        if (modelPivot != null)
        {
            firePoint.position = modelPivot.position + modelPivot.forward * 0.5f;
            firePoint.rotation = modelPivot.rotation;
        }

        // 1. Determinar direção do disparo
        Vector3 shotDirection = GetShotDirection();

        // 2. Calcular trajetória com Raycast
        RaycastHit hit;
        bool hasHit = Physics.Raycast(
            firePoint.position,
            shotDirection,
            out hit,
            maxDistance,
            hitLayers
        );

        // 3. Processar acerto
        Vector3 hitPosition = hasHit ? hit.point : firePoint.position + shotDirection * maxDistance;

        if (hasHit)
        {
            // Aplicar dano ao inimigo
            EnemyHealthSystem enemyHealth = hit.collider.GetComponent<EnemyHealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(characterData.damage);
                Debug.Log("Acertou inimigo: " + hit.collider.name + " com " + characterData.damage + " de dano");
            }
            else
            {
                Debug.Log("Acertou algo que não é inimigo: " + hit.collider.name);
            }
        }
        else
        {
            Debug.Log("Tiro disparado mas não acertou nada");
        }

        // 4. Criar projétil visual usando pool
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

        // 5. Atualizar estado
        nextShotTime = Time.time + (1f / characterData.attackSpeed);
        currentAmmo--;
    }

    Vector3 GetShotDirection()
    {
        if (cameraController != null && cameraController.isAiming)
        {
            // Se estiver mirando, usa a direção central da câmera
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

        return transform.forward; // Fallback
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

    // Método adicionado para a UI
    public float GetRemainingReloadTime()
    {
        if (!isReloading) return 0;
        return characterData.reloadSpeed - (Time.time - reloadStartTime);
    }
}