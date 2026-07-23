using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Compact, low-profile on-screen debug panel for real-time player telemetry.
/// Hardcoded to toggle via the backquote/tilde (`) key.
/// </summary>

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerStateManager))]
[RequireComponent(typeof(PlayerData))]
[RequireComponent(typeof(PlayerWalk))]
public class PlayerMovementDebug : MonoBehaviour
{
    private const float PanelMargin = 12f;
    private const float PanelWidth = 230f;
    private const float PanelHeight = 200f;
    private const int FontSize = 10;

    private PlayerController _playerController;
    private PlayerStateManager _playerStateManager;
    private PlayerData _playerData;
    private PlayerWalk _playerWalk;

    private InputAction _toggleAction;
    private bool _isVisible = true;

    private GUIStyle _panelStyle;
    private GUIStyle _labelStyle;
    private Texture2D _backgroundTexture;
    private StringBuilder _stringBuilder;
    private Rect _panelRect;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerStateManager = GetComponent<PlayerStateManager>();
        _playerData = GetComponent<PlayerData>();
        _playerWalk = GetComponent<PlayerWalk>();

        _stringBuilder = new StringBuilder(512);
        _panelRect = new Rect(PanelMargin, PanelMargin, PanelWidth, PanelHeight);

        // Hardcoded backquote binding using New Input System
        _toggleAction = new InputAction("ToggleDebug", InputActionType.Button, "<Keyboard>/backquote");
    }

    private void OnEnable()
    {
        if (_toggleAction != null)
        {
            _toggleAction.Enable();
            _toggleAction.performed += OnTogglePerformed;
        }
    }

    private void OnDisable()
    {
        if (_toggleAction != null)
        {
            _toggleAction.performed -= OnTogglePerformed;
            _toggleAction.Disable();
        }
    }

    private void OnDestroy()
    {
        if (_backgroundTexture != null)
        {
            Destroy(_backgroundTexture);
        }

        if (_toggleAction != null)
        {
            _toggleAction.Dispose();
        }
    }

    private void OnTogglePerformed(InputAction.CallbackContext context)
    {
        _isVisible = !_isVisible;
    }

    private void OnGUI()
    {
        if (!_isVisible) return;

        InitializeStylesIfNeeded();

        // Draw dark translucent background panel
        GUI.Box(_panelRect, GUIContent.none, _panelStyle);

        _stringBuilder.Clear();

        float fps = 1f / Time.unscaledDeltaTime;
        Vector3 velocity = _playerController.Velocity;
        float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;

        // Construct telemetry string with compact formatting
        _stringBuilder.Append("<color=#61AFEF><b>[ MOVEMENT DEBUG ]</b></color> <color=#5C6370>").AppendFormat("{0:F0} FPS", fps).Append("</color>\n");
        _stringBuilder.Append("<color=#3E4451>----------------------------------------</color>\n");

        _stringBuilder.Append("Can Move: ").Append(_playerStateManager.CanPlayerMove ? "<color=#98C379>TRUE</color>" : "<color=#E06C75>FALSE</color>").Append(" | ");
        _stringBuilder.Append("Grounded: ").Append(_playerController.IsGrounded ? "<color=#98C379>TRUE</color>" : "<color=#E06C75>FALSE</color>").Append("\n");
        _stringBuilder.Append("Is Walking: ").Append(_playerStateManager.IsWalking ? "<color=#98C379>TRUE</color>" : "<color=#5C6370>FALSE</color>").Append("\n\n");

        _stringBuilder.Append("<color=#D19A66><b>INPUT & SPEED</b></color>\n");
        _stringBuilder.Append("Input: ").AppendFormat("({0:F2}, {1:F2})", _playerController.InputVector.x, _playerController.InputVector.y).Append("\n\n");

        _stringBuilder.Append("<color=#E5C07B><b>VELOCITY</b></color>\n");
        _stringBuilder.Append("Horizontal: ").AppendFormat("{0:F2} m/s", horizontalSpeed).Append("\n");
        _stringBuilder.Append("Vertical: ").AppendFormat("{0:F2} m/s", velocity.y);

        Rect contentRect = new Rect(_panelRect.x + 8f, _panelRect.y + 6f, _panelRect.width - 16f, _panelRect.height - 12f);
        GUI.Label(contentRect, _stringBuilder.ToString(), _labelStyle);
    }

    private void InitializeStylesIfNeeded()
    {
        if (_panelStyle != null) return;

        // Create sleek 1x1 dark slate background
        _backgroundTexture = new Texture2D(1, 1);
        _backgroundTexture.SetPixel(0, 0, new Color(0.08f, 0.09f, 0.11f, 0.88f));
        _backgroundTexture.Apply();

        _panelStyle = new GUIStyle
        {
            normal = { background = _backgroundTexture }
        };

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = FontSize,
            richText = true,
            alignment = TextAnchor.UpperLeft
        };
        _labelStyle.normal.textColor = new Color(0.82f, 0.85f, 0.88f, 1f);
    }
}