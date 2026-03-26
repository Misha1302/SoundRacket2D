using UnityEngine;

/// <summary>
/// Converts keyboard hold state to normalized power (0..1)
/// using the same smoothing model as microphone input.
/// </summary>
public sealed class KeyboardInputService : MonoBehaviour
{
    [SerializeField] private KeyCode inputKey = KeyCode.Space;

    [Header("Smoothing")]
    [SerializeField, Min(0.01f)] private float riseSpeed = 12f;
    [SerializeField, Min(0.01f)] private float fallSpeed = 8f;

    public KeyCode InputKey => inputKey;

    public float CurrentPower01 { get; private set; }

    private void Update()
    {
        float target = Input.GetKey(inputKey) ? 1f : 0f;
        CurrentPower01 = InputSmoothing.SmoothExp(
            CurrentPower01,
            target,
            riseSpeed,
            fallSpeed,
            Time.deltaTime);
    }
}