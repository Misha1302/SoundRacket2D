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

    [Header("Power Visual")]
    [SerializeField] private Image powerFillImage;
    [SerializeField] private Image[] powerBars;
    [SerializeField] private Color activeBarColor = new Color(1f, 0.72f, 0.18f, 1f);
    [SerializeField] private Color inactiveBarColor = new Color(1f, 0.72f, 0.18f, 0.08f);
    [SerializeField, Min(0.01f)] private float powerSmoothSpeed = 12f;

    [Header("Result Visual")]
    [SerializeField] private bool padResultToFourDigits = true;
    [SerializeField] private bool resultUsesMetersRounded = true;

    private float _displayedPower01;

    private void Update()
    {
        float rawPower01 = attemptResultTracker != null
            ? Mathf.Clamp01(attemptResultTracker.CurrentPower01)
            : inputModeController == null
                ? 0f
                : Mathf.Clamp01(inputModeController.CurrentPower01);

        _displayedPower01 = SmoothExp(_displayedPower01, rawPower01, powerSmoothSpeed, Time.deltaTime);

        UpdatePowerVisuals(_displayedPower01);
        UpdateTexts(rawPower01);
    }

    private void UpdatePowerVisuals(float power01)
    {
        if (powerFillImage != null)
        {
            powerFillImage.fillAmount = power01;
        }

        if (powerBars == null || powerBars.Length == 0)
        {
            return;
        }

        int activeCount = power01 <= 0.001f
            ? 0
            : Mathf.Clamp(Mathf.CeilToInt(power01 * powerBars.Length), 0, powerBars.Length);

        for (int i = 0; i < powerBars.Length; i++)
        {
            Image bar = powerBars[i];
            if (bar == null)
            {
                continue;
            }

            bool active = i < activeCount;
            bar.enabled = true;
            bar.color = active ? activeBarColor : inactiveBarColor;
        }
    }

    private void UpdateTexts(float rawPower01)
    {
        if (modeText != null && inputModeController != null)
        {
            modeText.text = inputModeController.CurrentMode == RocketInputModeController.InputMode.Voice
                ? "МИКРОФОН"
                : "КНОПКА";
        }

        if (currentPowerText != null)
        {
            currentPowerText.text = $"{Mathf.RoundToInt(rawPower01 * 100f)}%";
        }

        if (peakPowerText != null)
        {
            float peakPower01 = attemptResultTracker == null ? 0f : attemptResultTracker.PeakPower01;
            peakPowerText.text = $"{Mathf.RoundToInt(peakPower01 * 100f)}%";
        }

        if (heightText != null)
        {
            float maxHeight = attemptResultTracker == null ? 0f : attemptResultTracker.MaxHeight;
            heightText.text = $"{maxHeight:0.0} м";
        }

        if (resultText != null)
        {
            float result = attemptResultTracker == null ? 0f : attemptResultTracker.FinalResult;

            int displayValue = resultUsesMetersRounded
                ? Mathf.RoundToInt(result)
                : Mathf.RoundToInt(result * 10f);

            displayValue = Mathf.Clamp(displayValue, 0, 9999);

            resultText.text = padResultToFourDigits
                ? displayValue.ToString("0000")
                : displayValue.ToString();
        }

        if (statusText != null)
        {
            statusText.text = BuildStatusText();
        }
    }

    private string BuildStatusText()
    {
        if (inputModeController == null)
        {
            return string.Empty;
        }

        if (inputModeController.CurrentMode == RocketInputModeController.InputMode.Voice)
        {
            if (microphoneInputService == null)
            {
                return "Микрофон не назначен";
            }

            return microphoneInputService.StatusMessage;
        }

        return "Space = тяга | 8 = микрофон | 9 = кнопка | 2 = выход";
    }

    private static float SmoothExp(float current, float target, float speed, float deltaTime)
    {
        float t = 1f - Mathf.Exp(-speed * deltaTime);
        return Mathf.Lerp(current, target, t);
    }
}