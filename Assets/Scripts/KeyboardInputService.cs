using UnityEngine;

/// <summary>
/// Converts keyboard hold state to normalized power (0..1).
/// </summary>
public sealed class KeyboardInputService : MonoBehaviour
{
    [SerializeField] private KeyCode inputKey = KeyCode.Space;
    [SerializeField, Min(0.01f)] private float riseSpeed = 15f;
    [SerializeField, Min(0.01f)] private float fallSpeed = 10f;

    public KeyCode InputKey => inputKey;
    public float CurrentPower01 { get; private set; }

    private void Update()
    {
        var target = Input.GetKey(inputKey) ? 1f : 0f;
        var speed = target > CurrentPower01 ? riseSpeed : fallSpeed;
        CurrentPower01 = Mathf.MoveTowards(CurrentPower01, target, speed * Time.deltaTime);
    }
}
