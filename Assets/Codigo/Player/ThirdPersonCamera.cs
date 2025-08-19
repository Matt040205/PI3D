using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public enum CameraState { ThirdPerson, FirstPerson }
    public CameraState currentState = CameraState.ThirdPerson;

    // Valores padrão para terceira pessoa
    private Vector3 thirdPersonOffset = new Vector3(0.8f, 2.0f, -4f);
    public float defaultDistance = 4f;
    public float minDistance = 0.5f;
    public float maxDistance = 10f;
    public float zoomSensitivity = 1f;

    // Posição para primeira pessoa (relativa à cabeça do personagem)
    public Vector3 firstPersonOffset = new Vector3(0f, 1.6f, 0.1f);

    // Valores de rotação
    public float sensitivity = 2f;
    public float minY = -20f;
    public float maxY = 80f;

    // Referências
    public Transform target;
    private float currentX = 0f;
    private float currentY = 0f;
    private float currentDistance;

    void Start()
    {
        currentDistance = defaultDistance;
        if (target != null)
        {
            Vector3 direction = transform.position - target.position;
            Quaternion initialRotation = Quaternion.LookRotation(direction);
            currentX = initialRotation.eulerAngles.y;
            currentY = initialRotation.eulerAngles.x;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Controle de zoom com o scroll do mouse
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= zoomInput * zoomSensitivity;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        // Transição para primeira pessoa
        if (currentDistance <= minDistance + 0.1f)
        {
            currentState = CameraState.FirstPerson;
        }
        else
        {
            currentState = CameraState.ThirdPerson;
        }

        // Rotação com mouse
        currentX += Input.GetAxis("Mouse X") * sensitivity;
        currentY -= Input.GetAxis("Mouse Y") * sensitivity;
        currentY = Mathf.Clamp(currentY, minY, maxY);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        if (currentState == CameraState.ThirdPerson)
        {
            Vector3 finalOffset = rotation * thirdPersonOffset.normalized * currentDistance;
            transform.position = target.position + finalOffset;

            transform.LookAt(target.position + Vector3.up * 0.2f);
        }
        else if (currentState == CameraState.FirstPerson)
        {
            transform.position = target.position + rotation * firstPersonOffset;
            transform.rotation = rotation;
        }
    }
}