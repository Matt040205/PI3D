using UnityEngine;

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

    private CharacterController controller;
    private Vector3 velocity;
    public bool isGrounded;
    private float currentSpeed;
    private float rotationVelocity;

    // --- MUDANÇA 1: Variáveis para compartilhar dados entre Update e LateUpdate ---
    private Vector3 direction;
    private float targetAngle;
    // -------------------------------------------------------------------------

    // Variáveis da passiva do comandante
    public bool canDoubleJump = false;
    private bool hasDoubleJumped = false;

    // Variáveis para flutuação no ar (Voo Gracioso)
    public bool isFloating = false;
    public float floatDuration = 0f;
    public float jumpHeightModifier = 1f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraController == null && Camera.main != null)
        {
            cameraController = Camera.main.transform;
        }
    }

    private void Update()
    {
        // Se o jogo está pausado ou no modo de construção, a lógica de movimento é ignorada
        if (PauseControl.isPaused || BuildManager.isBuildingMode)
        {
            return;
        }

        // Lógica de flutuação
        if (isFloating)
        {
            // ... (código de flutuação) ...
        }
        else
        {
            HandleMovement();
            HandleJump();
            ApplyGravity();
        }
    }

    // --- MUDANÇA 2: Adicionamos a função LateUpdate() ---
    private void LateUpdate()
    {
        // Não rotacionar se pausado, flutuando, etc.
        if (PauseControl.isPaused || BuildManager.isBuildingMode || isFloating)
        {
            return;
        }

        // A lógica de rotação foi MOVIDA para cá
        if (direction.magnitude >= 0.1f)
        {
            // Isso roda DEPOIS do Animator, forçando a rotação correta
            float angle = Mathf.SmoothDampAngle(modelPivot.eulerAngles.y, targetAngle, ref rotationVelocity, 0.1f);
            modelPivot.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
    // -----------------------------------------------------------------

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // --- MUDANÇA 3: Usando a variável de classe (removido "Vector3") ---
        direction = new Vector3(horizontal, 0f, vertical).normalized;

        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        if (direction.magnitude >= 0.1f)
        {
            // --- MUDANÇA 4: Usando a variável de classe (removido "float") ---
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraController.eulerAngles.y;

            // --- MUDANÇA 5: O CÓDIGO DE ROTAÇÃO FOI REMOVIDO DAQUI ---
            // float angle = ... (REMOVIDO)
            // modelPivot.rotation = ... (REMOVIDO)

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }
        else
        {
            controller.Move(Vector3.zero);
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
            }
            else if (canDoubleJump && !hasDoubleJumped)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity) * jumpHeightModifier;
                hasDoubleJumped = true;
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