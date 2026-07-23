using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central hub for player state, cached references, input handling via New Input System references,
/// and physics execution using CharacterController.
/// </summary>

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField, Tooltip("Direct reference to the Move action from an Input Actions asset.")]
    private InputActionReference moveAction;

    [SerializeField, Tooltip("Direct reference to the Run action from an Input Actions asset.")]
    private InputActionReference runAction;

    [Header("Gravity & Grounding")]
    [SerializeField, Tooltip("Force of gravity applied to the player.")]
    private float gravity = -19.62f;

    [SerializeField, Tooltip("Constant downward force applied while grounded to keep character snapped to terrain.")]
    private float groundedForce = -2f;

    // Public cached properties for modular movement components
    public CharacterController CharacterController { get; private set; }
    public Vector2 InputVector { get; private set; }
    public InputActionReference RunAction => runAction;
    public Vector3 Velocity => _finalVelocity;
    public bool IsGrounded { get; private set; }

    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;
    private Vector3 _finalVelocity;

    private void Awake()
    {
        CharacterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }

        if (runAction != null)
        {
            runAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }

        if (runAction != null)
        {
            runAction.action.Disable();
        }
    }

    private void Update()
    {
        ReadInput();
        UpdateGroundedState();
        ApplyGravity();

        // Aggregate velocities from modules and physics
        _finalVelocity = _horizontalVelocity + (Vector3.up * _verticalVelocity);

        // Execute unified movement once per frame
        CharacterController.Move(_finalVelocity * Time.deltaTime);
    }

    private void ReadInput()
    {
        // Zero-allocation input sampling via action reference
        if (moveAction != null)
        {
            InputVector = moveAction.action.ReadValue<Vector2>();
        }
        else
        {
            InputVector = Vector2.zero;
        }
    }

    private void UpdateGroundedState()
    {
        IsGrounded = CharacterController.isGrounded;

        // Reset accumulated downward gravity when touching ground
        if (IsGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = groundedForce;
        }
    }

    private void ApplyGravity()
    {
        if (!IsGrounded)
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
    }

    public void SetHorizontalVelocity(Vector3 velocity)
    {
        _horizontalVelocity = velocity;
    }

    public void SetVerticalVelocity(float velocity)
    {
        _verticalVelocity = velocity;
    }
}