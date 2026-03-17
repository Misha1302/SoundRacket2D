using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RocketHudPresenterTMP : MonoBehaviour
{
    [SerializeField] private RocketInputModeController inputModeController;
    [SerializeField] private AttemptResultTracker attemptResultTracker;
    [SerializeField] private MicrophoneInputService microphoneInputService;

    [Header("TMP Labels")]
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private TMP_Text currentPowerText;
    [SerializeField] private TMP_Text peakPowerText;
    [SerializeField] private TMP_Text heightText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text statusText;

    [Header("Power UI")]
    [SerializeField] private Slider powerSlider;

    private void OnEnable()
    {
        if (microphoneInputService != null)
        {
            microphoneInputService.CaptureFailed += OnCaptureFailed;
        }
    }

    private void OnDisable()
    {
        if (microphoneInputService != null)
        {
            microphoneInputService.CaptureFailed -= OnCaptureFailed;
        }
    }

    private void Update()
    {
        if (inputModeController != null)
        {
            SetText(modeText, $"Mode: {inputModeController.CurrentMode}");
        }

        if (attemptResultTracker != null)
        {
            SetText(currentPowerText, $"Power: {attemptResultTracker.CurrentPower01:0.00}");
            SetText(peakPowerText, $"Peak: {attemptResultTracker.PeakPower01:0.00}");
            SetText(heightText, $"Height: {attemptResultTracker.CurrentHeight:0.00} / {attemptResultTracker.MaxHeight:0.00} m");
            SetText(resultText, $"Result: {attemptResultTracker.FinalResult:0}");

            if (powerSlider != null)
            {
                powerSlider.normalizedValue = attemptResultTracker.CurrentPower01;
            }
        }

        if (microphoneInputService != null)
        {
            SetText(statusText, microphoneInputService.StatusMessage);
        }
    }

    private void OnCaptureFailed(string reason)
    {
        SetText(statusText, reason);
    }

    private static void SetText(TMP_Text label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }
}
