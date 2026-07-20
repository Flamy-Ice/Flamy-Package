using UnityEngine;

/// <summary>
/// Handles camera-relative horizontal movement, speed interpolation, and character rotation.
/// </summary>

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerData))]
public class PlayerWalk : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField, Tooltip("Sharpness of movement acceleration and deceleration.")]
    private float acceleration = 12f;

    [Header("Rotation")]
    [SerializeField, Tooltip("Speed at which the character rotates towards movement direction.")]
    private float rotationSpeed = 15f;

    [Header("References")]
    [SerializeField, Tooltip("Camera transform used to calculate directional movement. Defaults to Main Camera if unassigned.")]
    private Transform cameraTransform;

    private PlayerController _playerController;
    private PlayerData _playerData;
    private Vector3 _currentHorizontalVelocity;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerData = GetComponent<PlayerData>();
    }

    private void Start()
    {
        EnsureCameraReference();
    }

    private void Update()
    {
        HandleHorizontalMovement();
    }

    private void HandleHorizontalMovement()
    {
        Vector2 rawInput = _playerController.InputVector;

        // Clamp input magnitude to 1.0 to fix the diagonal speed bug (W+D moving 41% faster) 
        // while maintaining smooth analog stick sensitivity for controllers.
        Vector2 clampedInput = Vector2.ClampMagnitude(rawInput, 1f);
        float inputMagnitude = clampedInput.magnitude;

        // Fetch move speed dynamically from PlayerData
        float moveSpeed = _playerData.MoveSpeed;

        // Determine target movement speed scaled by input strength
        float targetSpeed = inputMagnitude < 0.01f ? 0f : moveSpeed * inputMagnitude;

        // Calculate 3D target direction based on camera view
        Vector3 targetDirection = CalculateCameraRelativeDirection(clampedInput);

        // Normalize direction to ensure unit length regardless of camera alignment
        if (targetDirection.sqrMagnitude > 0.01f)
        {
            targetDirection.Normalize();
        }
        else
        {
            targetDirection = Vector3.zero;
        }

        // Calculate velocity target
        Vector3 targetVelocity = targetDirection * targetSpeed;

        // Framerate-independent exponential interpolation factor
        float lerpFactor = 1f - Mathf.Exp(-acceleration * Time.deltaTime);
        _currentHorizontalVelocity = Vector3.Lerp(_currentHorizontalVelocity, targetVelocity, lerpFactor);

        // Pass calculated movement vector back to the main controller
        _playerController.SetHorizontalVelocity(_currentHorizontalVelocity);

        // Smoothly rotate the character model towards target direction
        if (targetDirection.sqrMagnitude > 0.01f)
        {
            RotateTowards(targetDirection);
        }
    }

    private Vector3 CalculateCameraRelativeDirection(Vector2 input)
    {
        // Lazy initialization fallback in case the camera was instantiated late
        if (cameraTransform == null)
        {
            EnsureCameraReference();

            // World-space fallback if no main camera is found in the scene
            if (cameraTransform == null)
            {
                return new Vector3(input.x, 0f, input.y);
            }
        }

        // Project camera vectors onto XZ plane
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        // Fallback for pitch alignment edge-case (camera pointing straight down/up)
        if (forward.sqrMagnitude < 0.001f)
        {
            // Use camera's UP vector as forward baseline when looking top-down
            forward = cameraTransform.up;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }
        }

        if (right.sqrMagnitude < 0.001f)
        {
            right = Vector3.right;
        }

        forward.Normalize();
        right.Normalize();

        return (forward * input.y) + (right * input.x);
    }

    private void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Framerate-independent spherical interpolation factor
        float slerpFactor = 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, slerpFactor);
    }

    private void EnsureCameraReference()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
}