using UnityEngine;

/// <summary>
/// ScriptableObject holding base player statistics.
/// </summary>

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Movement Settings")]
    [SerializeField, Tooltip("Base movement speed in units per second.")]
    private float moveSpeed = 5f;

    [SerializeField, Tooltip("Sprint movement speed in units per second.")]
    private float sprintSpeed = 8.5f;

    public float MoveSpeed => moveSpeed;
    public float SprintSpeed => sprintSpeed;
}