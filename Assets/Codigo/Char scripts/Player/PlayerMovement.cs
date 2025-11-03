using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    public float jumpForce = 4f;
    public float gravity = -9.81f;
    public float rotationSpeed = 15f;

    [Header("References")]
    public Transform cameraController;
    public Transform modelPivot;

    [Header("Aiming Settings (Preencha no Prefab)")]
    public Rig aimRig;
    public MultiAimConstraint aimConstraint;
    public LayerMask aimLayerMask;

    private bool isAiming = false;
    private Transform aimTarget;

    private CharacterController controller;
    private Vector3 velocity;
    public bool isGrounded;
    private float currentSpeed;
    private float rotationVelocity;

    private Animator animator;

    private Vector3 direction; // MUDANÇA: 'direction' agora é baseado no input local
    private float targetAngle;

    // Suas variáveis originais
    public bool canDoubleJump = false;
    private bool hasDoubleJumped = false;
    public bool isFloating = false;
    public float floatDuration = 0f;
    public float jumpHeightModifier = 1f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = modelPivot.GetComponentInChildren<Animator>();

        if (cameraController == null && Camera.main != null)
        {
            cameraController = Camera.main.transform;
        }

        // Configuração Automática da Mira (Prova de Instanciação)
        GameObject aimTargetObj = new GameObject(name + " AimTarget");
        aimTarget = aimTargetObj.transform;

        if (aimConstraint != null)
        {
            var sourceObjects = aimConstraint.data.sourceObjects;
            sourceObjects.Clear();
            sourceObjects.Add(new WeightedTransform(aimTarget, 1f));
            aimConstraint.data.sourceObjects = sourceObjects;
        }

        if (aimRig != null) aimRig.weight = 0f;
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;

        if (PauseControl.isPaused || BuildManager.isBuildingMode)
        {
            if (animator != null) animator.SetFloat("MovementSpeed", 0f);
            return;
        }

        HandleAiming();

        if (isFloating)
        {
            velocity.y = 0;
            floatDuration -= Time.deltaTime;
            if (floatDuration <= 0)
            {
                isFloating = false;
            }
        }
        else
        {
            HandleMovement();
            HandleJump();
            ApplyGravity();
        }

        if (animator != null) animator.SetBool("isGrounded", isGrounded);
    }

    private void LateUpdate()
    {
        if (PauseControl.isPaused || BuildManager.isBuildingMode || isFloating)
        {
            return;
        }

        // Mover o AimTarget com o Raycast
        if (aimTarget != null)
        {
            Ray ray = new Ray(cameraController.position, cameraController.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimLayerMask))
            {
                aimTarget.position = hit.point;
            }
            else
            {
                aimTarget.position = ray.GetPoint(100f);
            }
        }

        // Forçar a rotação se estivermos MIRANDO ou MOVENDO
        // MUDANÇA: 'direction.magnitude' foi substituído por 'direction.sqrMagnitude' (um pouco mais otimizado)
        if (isAiming || direction.sqrMagnitude > 0.01f) // 'direction' agora é (horizontal, 0, vertical)
        {
            if (isAiming)
            {
                targetAngle = cameraController.eulerAngles.y;
            }

            float angle = Mathf.SmoothDampAngle(modelPivot.eulerAngles.y, targetAngle, ref rotationVelocity, 0.1f);
            modelPivot.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    private void HandleAiming()
    {
        if (animator == null) return;

        bool aimingInput = Input.GetButton("Fire2");

        if (aimingInput != isAiming)
        {
            isAiming = aimingInput;
            animator.SetBool("isAiming", isAiming);
            StartCoroutine(FadeRigWeight(isAiming ? 1f : 0f));
        }
    }

    private IEnumerator FadeRigWeight(float targetWeight)
    {
        if (aimRig == null) yield break;
        float time = 0f;
        float startWeight = aimRig.weight;
        float duration = 0.2f;
        while (time < duration)
        {
            aimRig.weight = Mathf.Lerp(startWeight, targetWeight, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        aimRig.weight = targetWeight;
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");   // W/S

        // MUDANÇA: 'direction' agora armazena o input local (horizontal, 0, vertical)
        direction = new Vector3(horizontal, 0f, vertical);

        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Usamos .sqrMagnitude porque é mais rápido que .magnitude
        if (direction.sqrMagnitude > 0.01f) // 0.01f em vez de 0.1f, pois é ao quadrado
        {
            Vector3 moveDir; // O vetor de direção final

            if (isAiming)
            {
                // --- LÓGICA DE STRAFE (MIRA) ---
                // O personagem já está travado para a frente (pelo LateUpdate)
                // Precisamos mover relativo à câmera

                // Pega o ângulo de "olhar" (que é o ângulo da câmera)
                float lookAngle = cameraController.eulerAngles.y;

                // Gira o nosso vetor de input (direction) pelo ângulo da câmera
                // (horizontal, 0, vertical) -> (movimento de strafe real)
                moveDir = Quaternion.Euler(0f, lookAngle, 0f) * direction;

                // Envia os inputs locais para o Blend Tree 2D de mira
                if (animator != null)
                {
                    animator.SetFloat("AimMoveX", horizontal, 0.1f, Time.deltaTime);
                    animator.SetFloat("AimMoveY", vertical, 0.1f, Time.deltaTime);
                }
            }
            else
            {
                // --- LÓGICA DE LOCOMOÇÃO (NORMAL) ---
                // O personagem gira na direção do movimento
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraController.eulerAngles.y;
                moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                // Envia a velocidade para o Blend Tree de locomoção
                if (animator != null)
                {
                    // Usa a magnitude do input para o blend (normalizado para walk/run)
                    float animSpeed = (Input.GetKey(KeyCode.LeftShift) ? 1.0f : 0.5f) * direction.magnitude;
                    animator.SetFloat("MovementSpeed", animSpeed, 0.1f, Time.deltaTime);
                }
            }

            // Move o personagem (normalizado para evitar movimento diagonal mais rápido)
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }
        else
        {
            // --- PARADO ---
            controller.Move(Vector3.zero);
            if (animator != null)
            {
                // Zera os dois animators
                animator.SetFloat("MovementSpeed", 0f, 0.1f, Time.deltaTime);
                animator.SetFloat("AimMoveX", 0f, 0.1f, Time.deltaTime);
                animator.SetFloat("AimMoveY", 0f, 0.1f, Time.deltaTime);
            }
        }
    }

    private void HandleJump()
    {
        if (isGrounded)
        {
            hasDoubleJumped = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity) * jumpHeightModifier;
                isGrounded = false;
                if (animator != null) animator.SetTrigger("Jump");
            }
            else if (canDoubleJump && !hasDoubleJumped)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity) * jumpHeightModifier;
                hasDoubleJumped = true;
                if (animator != null) animator.SetTrigger("Jump");
            }
        }
    }

    private void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public Transform GetModelPivot()
    {
        return modelPivot;
    }
}