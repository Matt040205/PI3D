using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public CinemachineVirtualCamera normalCamera;
    public CinemachineVirtualCamera aimCamera;
    public float shoulderOffset = 0.5f;
    public float aimTransitionSpeed = 5f;

    private bool isAiming;
    private Vector3 currentShoulderOffset;
    private CinemachineTransposer aimTransposer;

    private void Start()
    {
        // Configuração inicial das câmeras
        normalCamera.Priority = 10;
        aimCamera.Priority = 0;
        currentShoulderOffset = Vector3.zero;

        // Cache do transposer da câmera de mira
        aimTransposer = aimCamera.GetCinemachineComponent<CinemachineTransposer>();
    }

    private void Update()
    {
        HandleAimInput();
        UpdateShoulderOffset();
    }

    private void HandleAimInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            normalCamera.Priority = 0;
            aimCamera.Priority = 10;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
            normalCamera.Priority = 10;
            aimCamera.Priority = 0;
        }
    }

    private void UpdateShoulderOffset()
    {
        Vector3 targetOffset = isAiming ? new Vector3(shoulderOffset, 0, 0) : Vector3.zero;
        currentShoulderOffset = Vector3.Lerp(currentShoulderOffset, targetOffset, aimTransitionSpeed * Time.deltaTime);

        // Aplica o offset no transposer
        if (aimTransposer != null)
        {
            aimTransposer.m_FollowOffset.x = currentShoulderOffset.x;
        }
    }
}