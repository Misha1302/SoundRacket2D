using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RocketHudPresenterTMP : MonoBehaviour
{
    [SerializeField] private RocketInputModeController inputModeController;
    [SerializeField] private RocketFlightController rocketFlightController;
    [SerializeField] private MicrophoneInputService microphoneInputService;
    [SerializeField] private AttemptSessionController attemptSessionController;

    [Header("TMP Labels")]
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private TMP_Text heightText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text hintText;

    [Header("Power Visual")]
    [SerializeField] private Image[] powerBars;
    [SerializeField] private Color activeBarColor = new Color(1f, 0.72f, 0.18f, 1f);
    [SerializeField] private Color inactiveBarColor = new Color(1f, 0.72f, 0.18f, 0.08f);
    [SerializeField, Min(0.01f)] private float powerSmoothSpeed = 12f;

    [Header("Height Text")]
    [SerializeField] private bool digitsOnly = true;
    [SerializeField] private float heightScale = 10;
    [SerializeField] private string digitsFormat = "000.0";

    private float _displayedPower01;

    public void Setup(
        RocketInputModeController newInputModeController,
        RocketFlightController newRocketFlightController,
        MicrophoneInputService newMicrophoneInputService,
        AttemptSessionController newAttemptSessionController,
        TMP_Text newModeText,
        TMP_Text newHeightText,
        TMP_Text newStateText,
        TMP_Text newStatusText,
        TMP_Text newHintText,
        Image[] newPowerBars)
    {
        inputModeController = newInputModeController;
        rocketFlightController = newRocketFlightController;
        microphoneInputService = newMicrophoneInputService;
        attemptSessionController = newAttemptSessionController;

        modeText = newModeText;
        heightText = newHeightText;
        stateText = newStateText;
        statusText = newStatusText;
        hintText = newHintText;
        powerBars = newPowerBars;
    }

    private void Update()
    {
        float rawPower01 = rocketFlightController == null
            ? 0f
            : rocketFlightController.CurrentSpeed01;

        _displayedPower01 = SmoothExp(_displayedPower01, rawPower01, powerSmoothSpeed, Time.deltaTime);

        UpdatePowerVisuals(_displayedPower01);
        UpdateTexts();
    }

    private void UpdatePowerVisuals(float power01)
    {
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

            bar.enabled = true;
            bar.color = i < activeCount ? new Color(activeBarColor.r, activeBarColor.g, activeBarColor.b, 1f / 10f * (i + 1)) : inactiveBarColor;
        }
    }

    private void UpdateTexts()
    {
        if (modeText != null && inputModeController != null)
        {
            modeText.text = inputModeController.CurrentMode == RocketInputModeController.InputMode.Voice
                ? "МИКРОФОН"
                : "КНОПКА";
        }

        if (heightText != null)
        {
            float height = rocketFlightController == null
                ? 0f
                : Mathf.Max(0f, rocketFlightController.HeightFromStart);
            height *= heightScale;

            heightText.text = digitsOnly
                ? height.ToString(digitsFormat, CultureInfo.InvariantCulture)
                : "Высота: " + height.ToString("0.0", CultureInfo.InvariantCulture);
        }

        if (stateText != null)
        {
            bool isRunning = attemptSessionController != null && attemptSessionController.IsAttemptRunning;
            stateText.text = isRunning ? "ИГРА ИДЕТ" : "ГОТОВО";
        }

        if (statusText != null)
        {
            statusText.text = BuildStatusText();
        }

        if (hintText != null)
        {
            hintText.text = BuildHintText();
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
            return microphoneInputService == null
                ? "Микрофон не назначен"
                : microphoneInputService.StatusMessage;
        }

        return "Space = тяга";
    }

    private string BuildHintText()
    {
        bool isRunning = attemptSessionController != null && attemptSessionController.IsAttemptRunning;

        if (isRunning)
        {
            return "Нажми 2 для перезапуска";
        }

        if (inputModeController == null)
        {
            return string.Empty;
        }

        return inputModeController.CurrentMode == RocketInputModeController.InputMode.Voice
            ? "Кричи в микрофон, чтобы взлететь"
            : "Удерживай пробел, чтобы взлететь";
    }

    private static float SmoothExp(float current, float target, float speed, float deltaTime)
    {
        float t = 1f - Mathf.Exp(-speed * deltaTime);
        return Mathf.Lerp(current, target, t);
    }
}