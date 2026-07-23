using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Handles player run input, toggle behavior, state evaluation, and updates the PlayerStateManager.
/// </summary>

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerStateManager))]

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

    [SerializeField, Tooltip("If true, stopping movement or invalidating states will automatically reset the toggle run state when in Toggle mode.")]
    private bool cancelToggleOnStop = true;

    private PlayerController _playerController;
    private PlayerStateManager _playerStateManager;

    private bool _isRunPressed;
    private bool _isToggleActive;

    private void Awake()
    {
        TryGetComponent(out _playerController);
        TryGetComponent(out _playerStateManager);
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
            // Reset toggle state when sprinting is invalidated or movement stops
            if (inputMode == RunInputMode.Toggle && cancelToggleOnStop)
            {
                _isToggleActive = false;
            }

            if (_playerStateManager != null)
            {
                _playerStateManager.IsSprinting = false;
            }
            return;
        }

        bool wantsToRun = inputMode == RunInputMode.Hold ? _isRunPressed : _isToggleActive;

        if (_playerStateManager != null)
        {
            _playerStateManager.IsSprinting = wantsToRun;
        }
    }

    private bool CanRun()
    {
        if (_playerStateManager == null || !_playerStateManager.CanPlayerMove || !_playerStateManager.CanPlayerSprint)
        {
            return false;
        }

        // Character must be receiving movement input to run
        if (_playerController == null || _playerController.InputVector.sqrMagnitude <= 0.01f)
        {
            return false;
        }

        return true;
    }
}

#if UNITY_EDITOR
/// <summary>
/// Custom Inspector drawer for PlayerRun to conditionally disable settings based on the input mode.
/// </summary>

[CustomEditor(typeof(PlayerRun))]

public class PlayerRunEditor : Editor
{
    private SerializedProperty _inputModeProp;
    private SerializedProperty _cancelToggleOnStopProp;

    private void OnEnable()
    {
        _inputModeProp = serializedObject.FindProperty("inputMode");
        _cancelToggleOnStopProp = serializedObject.FindProperty("cancelToggleOnStop");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_inputModeProp);

        // Gray out 'cancelToggleOnStop' field when input mode is set to Hold
        bool isToggleMode = _inputModeProp.enumValueIndex == (int)PlayerRun.RunInputMode.Toggle;

        using (new EditorGUI.DisabledScope(!isToggleMode))
        {
            EditorGUILayout.PropertyField(_cancelToggleOnStopProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif