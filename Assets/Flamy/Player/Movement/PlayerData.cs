using UnityEngine;

/// <summary>
/// Central container for player runtime data and ScriptableObject stats configuration.
/// </summary>

public class PlayerData : MonoBehaviour
{
    [Header("Stats Asset")]
    [SerializeField, Tooltip("Reference to the PlayerStats ScriptableObject asset.")]
    private PlayerStats reference;

    // Returns current base movement speed or fallback default if asset is unassigned.
    public float MoveSpeed => reference != null ? reference.MoveSpeed : 5f;

    // Returns current run speed or fallback default if asset is unassigned.
    public float SprintSpeed => reference != null ? reference.SprintSpeed : 8.5f;

    public PlayerStats Stats => reference;
}