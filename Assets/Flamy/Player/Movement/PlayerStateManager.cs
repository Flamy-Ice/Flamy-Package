using UnityEngine;

/// <summary>
/// Central manager for tracking and toggling player movement states and permissions.
/// </summary>

[RequireComponent(typeof(PlayerController))]
public class PlayerStateManager : MonoBehaviour
{
    [Header("Core")]
    [SerializeField, Tooltip("Determines whether the player is allowed to move (e.g., set to false during cutscenes).")]
    private bool canPlayerMove = true;

    [Header("Move")]
    [SerializeField, Tooltip("Indicates whether the character is currently walking/moving horizontally.")]
    private bool isWalking;

    // Public properties for clean external reading and state toggling
    public bool CanPlayerMove
    {
        get => canPlayerMove;
        set => canPlayerMove = value;
    }

    public bool IsWalking => isWalking;

    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        UpdateWalkingState();
    }

    private void UpdateWalkingState()
    {
        if (!canPlayerMove)
        {
            isWalking = false;
            return;
        }

        // Calculate horizontal velocity on the XZ plane
        Vector3 horizontalVelocity = _playerController.Velocity;
        horizontalVelocity.y = 0f;

        bool hasInput = _playerController.InputVector.sqrMagnitude > 0.01f;
        bool isMoving = horizontalVelocity.sqrMagnitude > 0.01f;

        isWalking = hasInput && isMoving;
    }
}