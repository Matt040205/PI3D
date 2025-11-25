using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
// using static Unity.VisualScripting.Member; // Removido pois geralmente causa conflito se não usado

[RequireComponent(typeof(PlayerHealthSystem))]
public class PlayerShooting : MonoBehaviour
{
    [Header("Configurações")]
    public CharacterBase characterData;
    public Transform firePoint;
    public GameObject projectileVisualPrefab;
    public GameObject impactEffectPrefab;

    [Header("Configurações de IK (Rigging)")] // NOVO
    [Tooltip("Arraste aqui o objeto vazio 'AimTarget' que o Multi-Aim Constraint está seguindo")]
    public Transform aimTarget;
    [Tooltip("Distância que o alvo de mira ficará do player")]
    public float aimTargetDistance = 20f;

    [Header("Configurações FMOD")]
    [Tooltip("Escreva 'Arma' ou 'Arco' para definir o som. (Deve corresponder exatamente)")]
    public string tipoDeSom = "Arma";

    [Header("FMOD - Sons da Arma")]
    [EventRef] public string eventoTiroUnicoArma = "event:/SFX/Atirar";
    [EventRef] public string eventoTiroContinuoArma = "event:/SFX/Atirar_segurando";
    [EventRef] public string eventoRecargaArma = "event:/SFX/Recarga Arma";

    [Header("FMOD - Sons do Arco")]
    [EventRef] public string eventoTiroUnicoArco = "event:/SFX/Arco";
    [EventRef] public string eventoTiroContinuoArco = "event:/SFX/Arco";

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

        // NOVO: Atualiza a posição do alvo de mira a cada frame
        UpdateAimTargetPosition();

        if (isReloading) return;

        HandleInput();
    }

    // NOVO: Lógica separada de Input para organização
    void HandleInput()
    {
        bool fireInput = characterData.fireMode == FireMode.FullAuto ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        if (fireInput && Time.time >= nextShotTime)
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

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < characterData.magazineSize)
        {
            StartReload();
        }
    }

    // NOVO: Essa função move o objeto invisível para onde a câmera está olhando
    void UpdateAimTargetPosition()
    {
        if (aimTarget == null || mainCamera == null) return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPosition;

        // Se o raio da câmera bater em algo, mira nesse ponto
        if (Physics.Raycast(ray, out hit, maxDistance, hitLayers))
        {
            targetPosition = hit.point;
        }
        else
        {
            // Se não bater em nada (olhando pro céu), mira num ponto distante na frente da câmera
            targetPosition = ray.origin + ray.direction * maxDistance;
        }

        // Suavização opcional (Lerp) para o movimento não ficar "duro" demais
        aimTarget.position = Vector3.Lerp(aimTarget.position, targetPosition, Time.deltaTime * 20f);
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

        PlayShootSound();

        // Ajuste visual simples do ponto de tiro (opcional se o Rigging estiver perfeito)
        if (modelPivot != null)
        {
            // Se o Rigging estiver funcionando, o modelPivot já deve estar rotacionando o corpo,
            // mas às vezes forçar a rotação do firePoint ajuda na precisão visual.
            firePoint.rotation = Quaternion.LookRotation(GetShotDirection());
        }

        Vector3 shotDirection = GetShotDirection();

        float finalDamage = CalculateDamage(out bool isCritical);

        SpawnProjectile(finalDamage, isCritical, shotDirection);

        nextShotTime = Time.time + (1f / characterData.attackSpeed);
        currentAmmo--;

        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }

    // Extrai a lógica de som para ficar mais limpo
    void PlayShootSound()
    {
        string eventToPlay = "";
        bool isFullAuto = characterData.fireMode == FireMode.FullAuto;

        if (tipoDeSom == "Arco")
        {
            eventToPlay = isFullAuto ? eventoTiroContinuoArco : eventoTiroUnicoArco;
        }
        else
        {
            eventToPlay = isFullAuto ? eventoTiroContinuoArma : eventoTiroUnicoArma;
        }

        if (!string.IsNullOrEmpty(eventToPlay))
            RuntimeManager.PlayOneShot(eventToPlay, transform.position);
    }

    float CalculateDamage(out bool isCritical)
    {
        float finalDamage = characterData.damage;
        isCritical = false;

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
        return finalDamage;
    }

    void SpawnProjectile(float damage, bool isCritical, Vector3 direction)
    {
        if (projectilePool != null)
        {
            GameObject visualProjectile = projectilePool.GetProjectile(
             firePoint.position,
             Quaternion.LookRotation(direction));

            if (visualProjectile != null)
            {
                ProjectileVisual visualScript = visualProjectile.GetComponent<ProjectileVisual>();
                if (visualScript != null)
                {
                    visualScript.Initialize(
                      damage,
                      isCritical,
                      characterData.armorPenetration,
                      playerHealth,
                      direction
                    );
                }
            }
        }
    }

    Vector3 GetShotDirection()
    {
        // Reutilizamos a lógica do UpdateAimTargetPosition indiretamente
        // Mas mantemos o cálculo exato do raio para precisão do projétil
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