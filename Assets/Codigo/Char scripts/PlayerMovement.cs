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
    private bool isGrounded;
    private float currentSpeed;
    private float rotationVelocity;

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
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraController.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(modelPivot.eulerAngles.y, targetAngle, ref rotationVelocity, 0.1f);

            modelPivot.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
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