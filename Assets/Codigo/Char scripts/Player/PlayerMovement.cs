using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;
using FMODUnity;
using FMOD.Studio;
using static Unity.VisualScripting.Member;
using static UnityEngine.Rendering.VolumeComponent;

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

    [Header("FMOD")]
    [EventRef]
    public string eventoPassos = "event:/SFX/Passos";
    private EventInstance passosSoundInstance;
    private bool isPlayingFootsteps = false;

    [HideInInspector]
    public bool isDashing = false;

    private bool isAiming = false;
    private Transform aimTarget;

    private CharacterController controller;
    private Vector3 velocity;
    public bool isGrounded;
    private float currentSpeed;
    private float rotationVelocity;

    private Animator animator;

    private Vector3 direction;
    private float targetAngle;

    public bool canDoubleJump = false;
    private bool hasDoubleJumped = false;
    public bool isFloating = false;
    public float floatDuration = 0f;
    public float jumpHeightModifier = 1f;

    private bool jaMoveuTutorial = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = modelPivot.GetComponentInChildren<Animator>();

        if (!string.IsNullOrEmpty(eventoPassos))
        {
            passosSoundInstance = RuntimeManager.CreateInstance(eventoPassos);
            RuntimeManager.AttachInstanceToGameObject(passosSoundInstance, transform);
        }
    }

    private void Start()
    {
        if (cameraController == null && Camera.main != null)
        {
            cameraController = Camera.main.transform;
        }

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

        if (TutorialManager.Instance != null && GameDataManager.Instance != null)
        {
            if (GameDataManager.Instance.tutoriaisConcluidos.Contains("PLAYER_MOVEMENT"))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                TutorialManager.Instance.TriggerTutorial("PLAYER_MOVEMENT");
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;

        if (PauseControl.isPaused || BuildManager.isBuildingMode || isDashing)
        {
            if (animator != null && !isDashing) animator.SetFloat("MovementSpeed", 0f);
            StopFootstepSound();
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
            StopFootstepSound();
        }
        else
        {
            HandleMovement();
            HandleJump();
            ApplyGravity();
        }

        if (animator != null) animator.SetBool("isGrounded", isGrounded);
    }

    private void OnDestroy()
    {
        if (passosSoundInstance.isValid())
        {
            passosSoundInstance.release();
        }
    }

    private void LateUpdate()
    {
        if (PauseControl.isPaused || BuildManager.isBuildingMode || isFloating || isDashing)
        {
            return;
        }

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

        if (isAiming || direction.sqrMagnitude > 0.01f)
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
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        direction = new Vector3(horizontal, 0f, vertical);

        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        if (direction.sqrMagnitude > 0.01f)
        {
            if (!jaMoveuTutorial && GameDataManager.Instance != null && GameDataManager.Instance.tutoriaisConcluidos.Contains("PLAYER_MOVEMENT"))
            {
                jaMoveuTutorial = true;
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.TriggerTutorial("EXPLAIN_BUILD_MODE");
                }
            }

            Vector3 moveDir;

            if (isAiming)
            {
                float lookAngle = cameraController.eulerAngles.y;
                 moveDir = Quaternion.Euler(0f, lookAngle, 0f) * direction;

                if (animator != null)
                {
                    animator.SetFloat("AimMoveX", horizontal, 0.1f, Time.deltaTime);
                    animator.SetFloat("AimMoveY", vertical, 0.1f, Time.deltaTime);
                }
            }
            else
            {
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraController.eulerAngles.y;
                moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                if (animator != null)
                {
                    float animSpeed = (Input.GetKey(KeyCode.LeftShift) ? 1.0f : 0.5f) * direction.magnitude;
                    animator.SetFloat("MovementSpeed", animSpeed, 0.1f, Time.deltaTime);
                }
            }
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);

            if (isGrounded)
            {
                PlayFootstepSound();
            }
            else
            {
                StopFootstepSound();
             }
        }
        else
        {
            controller.Move(Vector3.zero);
            if (animator != null)
            {
                animator.SetFloat("MovementSpeed", 0f, 0.1f, Time.deltaTime);
                animator.SetFloat("AimMoveX", 0f, 0.1f, Time.deltaTime);
                animator.SetFloat("AimMoveY", 0f, 0.1f, Time.deltaTime);
            }
            StopFootstepSound();
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
                StopFootstepSound();
            }

        else if (canDoubleJump && !hasDoubleJumped)
                    {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity) * jumpHeightModifier;
                hasDoubleJumped = true;
                if (animator != null) animator.SetTrigger("Jump");
                StopFootstepSound();
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

    private void PlayFootstepSound()
    {
        if (!isPlayingFootsteps && passosSoundInstance.isValid())
        {
            passosSoundInstance.start();
            isPlayingFootsteps = true;
        }
    }

    private void StopFootstepSound()
    {
        if (isPlayingFootsteps && passosSoundInstance.isValid())
        {
            passosSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            isPlayingFootsteps = false;
        }
    }

    public Transform GetModelPivot()
    {
        return modelPivot;
    }
}