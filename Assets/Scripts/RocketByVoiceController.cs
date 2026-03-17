using UnityEngine;

/// <summary>
/// Legacy controller that applies upward force from microphone normalized power.
/// Prefer RocketFlightController + RocketInputModeController for new setup.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public sealed class RocketByVoiceController : MonoBehaviour
{
    [SerializeField] private MicrophoneInputService microphoneInputService;
    [SerializeField, Min(0f)] private float activationThreshold = 0.02f;
    [SerializeField, Min(0f)] private float maxUpwardForce = 15f;

    private Rigidbody2D cachedRigidbody;

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (cachedRigidbody == null || microphoneInputService == null || !microphoneInputService.IsCapturing)
        {
            return;
        }

        var power01 = microphoneInputService.GetNormalizedLoudness01();
        if (power01 <= activationThreshold)
        {
            return;
        }

        cachedRigidbody.AddForce(Vector2.up * (power01 * maxUpwardForce), ForceMode2D.Force);
    }
}
