using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player run input, state evaluation, and updates the PlayerStateManager.
/// </summary>

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerStateManager))]
[RequireComponent(typeof(PlayerWalk))]
public class PlayerRun : MonoBehaviour
{
    public enum RunInputMode
    {
        Hold,
        Toggle
    }

    [Header("Run Settings")]
    [SerializeField, Tooltip("Input mode: Hold down key/button to run or Toggle on/off.")]
    private RunInputMode inputMode = RunInputMode.Hold;

    private PlayerController _playerController;
    private PlayerStateManager _playerStateManager;

    private bool _isRunPressed;
    private bool _isToggleActive;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerStateManager = GetComponent<PlayerStateManager>();
    }

    private void OnEnable()
    {
        if (_playerController != null && _playerController.RunAction != null)
        {
            _playerController.RunAction.action.performed += OnRunPerformed;
            _playerController.RunAction.action.canceled += OnRunCanceled;
        }
    }

    private void OnDisable()
    {
        if (_playerController != null && _playerController.RunAction != null)
        {
            _playerController.RunAction.action.performed -= OnRunPerformed;
            _playerController.RunAction.action.canceled -= OnRunCanceled;
        }

        _isRunPressed = false;
        _isToggleActive = false;

        if (_playerStateManager != null)
        {
            _playerStateManager.IsSprinting = false;
        }
    }

    private void Update()
    {
        EvaluateRunState();
    }

    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        _isRunPressed = true;

        if (inputMode == RunInputMode.Toggle)
        {
            _isToggleActive = !_isToggleActive;
        }
    }

    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        _isRunPressed = false;
    }

    private void EvaluateRunState()
    {
        if (!CanRun())
        {
            _isToggleActive = false;
            _playerStateManager.IsSprinting = false;
            return;
        }

        bool wantsToRun = inputMode == RunInputMode.Hold ? _isRunPressed : _isToggleActive;
        _playerStateManager.IsSprinting = wantsToRun;
    }

    private bool CanRun()
    {
        if (!_playerStateManager.CanPlayerMove || !_playerStateManager.CanPlayerSprint)
        {
            return false;
        }

        // Character must be receiving movement input to run
        return _playerController.InputVector.sqrMagnitude > 0.01f;
    }
}