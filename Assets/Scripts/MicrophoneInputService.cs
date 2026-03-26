using System;
using UnityEngine;

/// <summary>
/// Captures microphone loudness using Unity Microphone API
/// and exposes normalized power (0..1).
/// </summary>
public sealed class MicrophoneInputService : MonoBehaviour
{
    [Header("Device")]
    [SerializeField] private string preferredDeviceName = string.Empty;

    [Header("Capture")]
    [SerializeField, Min(8000)] private int targetSampleRate = 44100;
    [SerializeField, Min(1)] private int clipLengthSeconds = 1;
    [SerializeField, Min(0.1f)] private float startupTimeoutSeconds = 2f;

    [Header("Analysis")]
    [SerializeField, Min(64)] private int analysisWindowSize = 1024;
    [SerializeField, Min(0.01f)] private float loudnessRiseSpeed = 12f;
    [SerializeField, Min(0.01f)] private float loudnessFallSpeed = 8f;
    [SerializeField, Min(0.00001f)] private float silenceRms = 0.005f;
    [SerializeField, Min(0.00002f)] private float loudRms = 0.08f;

    public event Action CaptureStarted;
    public event Action CaptureStopped;
    public event Action<string> CaptureFailed;

    public bool IsCapturing { get; private set; }
    public bool IsInitialized { get; private set; }
    public string ActiveDeviceName { get; private set; } = string.Empty;
    public string StatusMessage { get; private set; } = "Idle";
    public float RawLoudness { get; private set; }
    public float SmoothedLoudness { get; private set; }
    public float NormalizedLoudness01 { get; private set; }

    private AudioClip microphoneClip;
    private float[] sampleBuffer;
    private float startupDeadlineTime;
    private bool waitingForStart;

    private void OnValidate()
    {
        if (loudRms <= silenceRms)
        {
            loudRms = silenceRms + 0.001f;
        }
    }

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

        if (!Microphone.IsRecording(ActiveDeviceName))
        {
            FailCapture("Microphone capture stopped unexpectedly.");
            return;
        }

        RawLoudness = ReadLatestWindowRms();
        SmoothedLoudness = InputSmoothing.SmoothExp(
            SmoothedLoudness,
            RawLoudness,
            loudnessRiseSpeed,
            loudnessFallSpeed,
            Time.deltaTime);

        NormalizedLoudness01 = Mathf.Clamp01(
            Mathf.InverseLerp(silenceRms, loudRms, SmoothedLoudness));
    }

    public void StartCapture()
    {
        if (IsCapturing || waitingForStart)
        {
            return;
        }

        ResetRuntimeState();

        string deviceName = SelectMicrophoneDevice();
        if (string.IsNullOrEmpty(deviceName))
        {
            FailCapture("No microphone device detected.");
            return;
        }

        ActiveDeviceName = deviceName;

        int sampleRate = ResolveSampleRate(deviceName, targetSampleRate);

        try
        {
            microphoneClip = Microphone.Start(deviceName, true, clipLengthSeconds, sampleRate);
        }
        catch (Exception exception)
        {
            FailCapture($"Failed to start microphone '{deviceName}': {exception.Message}");
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
        StatusMessage = "Starting microphone...";
    }

    public void StopCapture()
    {
        bool wasRunning = IsCapturing || waitingForStart;

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
            catch (Exception)
            {
            }
        }

        microphoneClip = null;
        sampleBuffer = null;
        ActiveDeviceName = string.Empty;
        RawLoudness = 0f;
        SmoothedLoudness = 0f;
        NormalizedLoudness01 = 0f;
        StatusMessage = "Stopped";

        if (wasRunning)
        {
            CaptureStopped?.Invoke();
        }
    }

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

    public float GetNormalizedLoudness01()
    {
        return NormalizedLoudness01;
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
        catch (Exception exception)
        {
            FailCapture($"Unable to query microphone position: {exception.Message}");
            return;
        }

        if (position > 0)
        {
            waitingForStart = false;
            IsCapturing = true;
            IsInitialized = true;
            StatusMessage = $"Mic: {ActiveDeviceName}";
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

        int writePosition = Microphone.GetPosition(ActiveDeviceName);
        if (writePosition < 0)
        {
            return 0f;
        }

        int clipSamples = microphoneClip.samples;
        int windowSize = sampleBuffer.Length;
        int readPosition = writePosition - windowSize;

        if (readPosition < 0)
        {
            readPosition += clipSamples;
        }

        if (!microphoneClip.GetData(sampleBuffer, readPosition))
        {
            return 0f;
        }

        double sumSquares = 0d;

        for (int i = 0; i < windowSize; i++)
        {
            float sample = sampleBuffer[i];
            sumSquares += sample * sample;
        }

        return Mathf.Sqrt((float)(sumSquares / windowSize));
    }

    private string SelectMicrophoneDevice()
    {
        string[] devices = Microphone.devices;

        if (devices == null || devices.Length == 0)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(preferredDeviceName))
        {
            for (int i = 0; i < devices.Length; i++)
            {
                if (string.Equals(devices[i], preferredDeviceName, StringComparison.OrdinalIgnoreCase))
                {
                    return devices[i];
                }
            }
        }

        return devices[0];
    }

    private int ResolveSampleRate(string deviceName, int requestedRate)
    {
        try
        {
            Microphone.GetDeviceCaps(deviceName, out int minFreq, out int maxFreq);

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
        catch (Exception)
        {
            return requestedRate;
        }
    }

    private void FailCapture(string reason)
    {
        bool wasRunning = IsCapturing || waitingForStart;

        StopCapture();
        StatusMessage = reason;

        if (wasRunning)
        {
            CaptureFailed?.Invoke(reason);
            return;
        }

        CaptureFailed?.Invoke(reason);
    }

    private void ResetRuntimeState()
    {
        IsCapturing = false;
        IsInitialized = false;
        RawLoudness = 0f;
        SmoothedLoudness = 0f;
        NormalizedLoudness01 = 0f;
        ActiveDeviceName = string.Empty;
        StatusMessage = "Idle";
    }
}