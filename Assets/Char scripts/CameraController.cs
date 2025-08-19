using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public CinemachineCamera normalCamera;
    public CinemachineCamera aimCamera;
    public float shoulderOffset = 1.16f;
    public float aimTransitionSpeed = 15f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float verticalAngleLimit = 80f;

    public bool isAiming { get; private set; }

    private CinemachineThirdPersonFollow normalFollow;
    private CinemachineThirdPersonFollow aimFollow;
    private float cameraRotationX;
    private float cameraRotationY;

    private void Start()
    {
        // Obter componentes de follow
        normalFollow = normalCamera.GetComponent<CinemachineThirdPersonFollow>();
        aimFollow = aimCamera.GetComponent<CinemachineThirdPersonFollow>();

        // Configuração inicial
        normalCamera.Priority.Value = 10;
        aimCamera.Priority.Value = 5;

        // Travar cursor no centro da tela
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleCameraRotation();
        HandleAimToggle();
        UpdateCameraOffsets();
    }

    public Vector3 GetAimDirection()
    {
        return isAiming ? aimCamera.transform.forward : normalCamera.transform.forward;
    }

    private void HandleCameraRotation()
    {
        // Obter input do mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * -1;

        // Atualizar rotações
        cameraRotationX += mouseX;
        cameraRotationY += mouseY;

        // Limitar ângulo vertical
        cameraRotationY = Mathf.Clamp(cameraRotationY, -verticalAngleLimit, verticalAngleLimit);

        // Aplicar rotação ao transform da câmera
        transform.rotation = Quaternion.Euler(cameraRotationY, cameraRotationX, 0);
    }

    private void HandleAimToggle()
    {
        // Alternar entre câmeras com botão direito do mouse
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            normalCamera.Priority.Value = 5;
            aimCamera.Priority.Value = 10;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
            normalCamera.Priority.Value = 10;
            aimCamera.Priority.Value = 5;
        }
    }

    private void UpdateCameraOffsets()
    {
        if (normalFollow == null || aimFollow == null) return;

        // Suavizar transição do ombro
        Vector3 targetAimOffset = aimFollow.ShoulderOffset;
        targetAimOffset.x = isAiming ? shoulderOffset : 0;

        aimFollow.ShoulderOffset = Vector3.Lerp(
            aimFollow.ShoulderOffset,
            targetAimOffset,
            aimTransitionSpeed * Time.deltaTime
        );

        // Suavizar altura da câmera normal
        Vector3 targetNormalOffset = normalFollow.ShoulderOffset;
        targetNormalOffset.y = isAiming ? 1.2f : 1.8f;

        normalFollow.ShoulderOffset = Vector3.Lerp(
            normalFollow.ShoulderOffset,
            targetNormalOffset,
            aimTransitionSpeed * Time.deltaTime
        );
    }
}