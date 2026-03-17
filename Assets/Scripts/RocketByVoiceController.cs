using UnityEngine;

/// <summary>
/// Applies upward force to a Rigidbody2D based on microphone loudness.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public sealed class RocketByVoiceController : MonoBehaviour
{
    [SerializeField] private MicrophoneInputService microphoneInputService;
    [SerializeField, Min(0f)] private float activationThreshold = 0.02f;
    [SerializeField, Min(0f)] private float maxLoudnessForFullForce = 0.25f;
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

        var loudness = microphoneInputService.GetSoundLoudness();
        print(loudness);
        if (loudness <= activationThreshold)
        {
            return;
        }

        var normalized = Mathf.InverseLerp(activationThreshold, maxLoudnessForFullForce, loudness);
        var force = normalized * maxUpwardForce;
        cachedRigidbody.AddForce(Vector2.up * force, ForceMode2D.Force);
    }
}
