using System;
using UnityEngine;

/// <summary>
/// Captures microphone audio and exposes loudness metrics for gameplay systems.
/// Uses Unity's built-in Microphone API only.
/// </summary>
public sealed class MicrophoneInputService : MonoBehaviour
{
    [Header("Device")]
    [SerializeField] private string preferredDeviceName = string.Empty;

    [Header("Capture")]
    [SerializeField, Min(8000)] private int targetSampleRate = 44100;
    [SerializeField, Min(1)] private int clipLengthSeconds = 1;
    [SerializeField, Min(0.01f)] private float startupTimeoutSeconds = 2f;

    [Header("Analysis")]
    [SerializeField, Min(16)] private int analysisWindowSize = 1024;
    [SerializeField, Min(0.01f)] private float loudnessRiseSpeed = 12f;
    [SerializeField, Min(0.01f)] private float loudnessFallSpeed = 8f;

    public event Action CaptureStarted;
    public event Action CaptureStopped;
    public event Action<string> CaptureFailed;

    public bool IsCapturing { get; private set; }
    public bool IsInitialized { get; private set; }
    public string ActiveDeviceName { get; private set; } = string.Empty;
    public float RawLoudness { get; private set; }
    public float SmoothedLoudness { get; private set; }

    private const float SilenceFloor = 1e-7f;

    private AudioClip microphoneClip;
    private float[] sampleBuffer;
    private float startupDeadlineTime;
    private bool waitingForStart;

    private void OnEnable()
    {
        StartCapture();
    }

    private void OnDisable()
    {
        StopCapture();
    }

    private void Update()
    {
        if (waitingForStart)
        {
            PollForCaptureStart();
            return;
        }

        if (!IsCapturing || microphoneClip == null)
        {
            return;
        }

        RawLoudness = ReadLatestWindowRms();
        SmoothedLoudness = SmoothLoudness(SmoothedLoudness, RawLoudness, Time.deltaTime);
    }

    /// <summary>
    /// Starts microphone capture. Safe to call repeatedly.
    /// </summary>
    public void StartCapture()
    {
        if (IsCapturing || waitingForStart)
        {
            return;
        }

        ResetRuntimeState();

        var deviceName = SelectMicrophoneDevice();
        if (string.IsNullOrEmpty(deviceName))
        {
            FailCapture("No microphone device detected.");
            return;
        }

        ActiveDeviceName = deviceName;
        var sampleRate = ResolveSampleRate(deviceName, targetSampleRate);

        try
        {
            microphoneClip = Microphone.Start(deviceName, true, clipLengthSeconds, sampleRate);
        }
        catch (Exception ex)
        {
            FailCapture($"Failed to start microphone '{deviceName}': {ex.Message}");
            return;
        }

        if (microphoneClip == null)
        {
            FailCapture($"Microphone.Start returned null for device '{deviceName}'.");
            return;
        }

        sampleBuffer = new float[Mathf.Min(analysisWindowSize, microphoneClip.samples)];
        waitingForStart = true;
        startupDeadlineTime = Time.realtimeSinceStartup + startupTimeoutSeconds;
    }

    /// <summary>
    /// Stops microphone capture and clears analysis state.
    /// </summary>
    public void StopCapture()
    {
        var wasRunning = IsCapturing || waitingForStart;

        waitingForStart = false;
        IsCapturing = false;
        IsInitialized = false;

        if (!string.IsNullOrEmpty(ActiveDeviceName))
        {
            try
            {
                if (Microphone.IsRecording(ActiveDeviceName))
                {
                    Microphone.End(ActiveDeviceName);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error while stopping microphone '{ActiveDeviceName}': {ex.Message}");
            }
        }

        microphoneClip = null;
        sampleBuffer = null;
        ActiveDeviceName = string.Empty;
        RawLoudness = 0f;
        SmoothedLoudness = 0f;

        if (wasRunning)
        {
            CaptureStopped?.Invoke();
        }
    }

    /// <summary>
    /// Restarts capture by stopping and starting again.
    /// </summary>
    public void RestartCapture()
    {
        StopCapture();
        StartCapture();
    }

    public float GetSoundLoudness()
    {
        return SmoothedLoudness;
    }

    public float GetSoundLoudnessRaw()
    {
        return RawLoudness;
    }

    public float GetSoundLoudnessDecibels(float minDecibels = -80f)
    {
        var linear = Mathf.Max(RawLoudness, SilenceFloor);
        var decibels = 20f * Mathf.Log10(linear);
        return Mathf.Max(decibels, minDecibels);
    }

    private void PollForCaptureStart()
    {
        if (string.IsNullOrEmpty(ActiveDeviceName))
        {
            FailCapture("Capture device became unavailable before stream startup.");
            return;
        }

        int position;
        try
        {
            position = Microphone.GetPosition(ActiveDeviceName);
        }
        catch (Exception ex)
        {
            FailCapture($"Unable to query microphone position: {ex.Message}");
            return;
        }

        if (position > 0)
        {
            waitingForStart = false;
            IsCapturing = true;
            IsInitialized = true;
            CaptureStarted?.Invoke();
            return;
        }

        if (Time.realtimeSinceStartup > startupDeadlineTime)
        {
            FailCapture($"Microphone stream for '{ActiveDeviceName}' did not start within {startupTimeoutSeconds:0.##}s.");
        }
    }

    private float ReadLatestWindowRms()
    {
        if (microphoneClip == null || sampleBuffer == null || sampleBuffer.Length == 0)
        {
            return 0f;
        }

        var writePosition = Microphone.GetPosition(ActiveDeviceName);
        if (writePosition <= 0)
        {
            return 0f;
        }

        var clipSamples = microphoneClip.samples;
        var windowSize = sampleBuffer.Length;
        var readPosition = writePosition - windowSize;

        if (readPosition < 0)
        {
            readPosition += clipSamples;
        }

        if (!microphoneClip.GetData(sampleBuffer, readPosition))
        {
            return 0f;
        }

        double sumSquares = 0d;
        for (var i = 0; i < windowSize; i++)
        {
            var sample = sampleBuffer[i];
            sumSquares += sample * sample;
        }

        return Mathf.Sqrt((float)(sumSquares / windowSize));
    }

    private float SmoothLoudness(float current, float target, float deltaTime)
    {
        var speed = target > current ? loudnessRiseSpeed : loudnessFallSpeed;
        var t = 1f - Mathf.Exp(-speed * deltaTime);
        return Mathf.Lerp(current, target, t);
    }

    private string SelectMicrophoneDevice()
    {
        var devices = Microphone.devices;
        if (devices == null || devices.Length == 0)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(preferredDeviceName))
        {
            for (var i = 0; i < devices.Length; i++)
            {
                if (string.Equals(devices[i], preferredDeviceName, StringComparison.OrdinalIgnoreCase))
                {
                    return devices[i];
                }
            }

            Debug.LogWarning($"Preferred microphone '{preferredDeviceName}' not found. Falling back to '{devices[0]}'.");
        }

        return devices[0];
    }

    private int ResolveSampleRate(string deviceName, int requestedRate)
    {
        try
        {
            Microphone.GetDeviceCaps(deviceName, out var minFreq, out var maxFreq);

            if (minFreq == 0 && maxFreq == 0)
            {
                return requestedRate;
            }

            if (maxFreq > 0)
            {
                return Mathf.Clamp(requestedRate, minFreq, maxFreq);
            }

            return Mathf.Max(requestedRate, minFreq);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Could not read device caps for '{deviceName}'. Using requested sample rate. Error: {ex.Message}");
            return requestedRate;
        }
    }

    private void FailCapture(string reason)
    {
        Debug.LogError($"[MicrophoneInputService] {reason}");
        StopCapture();
        CaptureFailed?.Invoke(reason);
    }

    private void ResetRuntimeState()
    {
        IsCapturing = false;
        IsInitialized = false;
        RawLoudness = 0f;
        SmoothedLoudness = 0f;
        ActiveDeviceName = string.Empty;
    }

    /*
     * macOS note:
     * In Unity Player Settings, set "Microphone Usage Description".
     * Unity maps this to NSMicrophoneUsageDescription in the app Info.plist.
     */
}
