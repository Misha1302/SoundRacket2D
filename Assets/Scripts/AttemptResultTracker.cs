using UnityEngine;

public sealed class AttemptResultTracker : MonoBehaviour
{
    [SerializeField] private RocketInputModeController inputModeController;
    [SerializeField] private RocketFlightController rocketFlightController;
    [SerializeField, Min(0f)] private float startThreshold = 0.05f;
    [SerializeField, Min(0f)] private float endThreshold = 0.02f;
    [SerializeField, Min(0f)] private float endDelaySeconds = 0.35f;

    public bool AttemptActive { get; private set; }
    public float CurrentPower01 { get; private set; }
    public float PeakPower01 { get; private set; }
    public float ActiveInputDuration { get; private set; }
    public float CurrentHeight { get; private set; }
    public float MaxHeight { get; private set; }
    public float FinalResult { get; private set; }

    private float lowPowerTime;

    private void Update()
    {
        CurrentPower01 = inputModeController == null ? 0f : inputModeController.CurrentPower01;
        CurrentHeight = rocketFlightController == null ? 0f : rocketFlightController.HeightFromStart;

        if (!AttemptActive && CurrentPower01 >= startThreshold)
        {
            StartAttempt();
        }

        if (!AttemptActive)
        {
            return;
        }

        PeakPower01 = Mathf.Max(PeakPower01, CurrentPower01);
        MaxHeight = Mathf.Max(MaxHeight, CurrentHeight);

        if (CurrentPower01 > endThreshold)
        {
            ActiveInputDuration += Time.deltaTime;
            lowPowerTime = 0f;
        }
        else
        {
            lowPowerTime += Time.deltaTime;
            if (lowPowerTime >= endDelaySeconds)
            {
                EndAttempt();
            }
        }
    }

    public void ResetAttempt()
    {
        AttemptActive = false;
        CurrentPower01 = 0f;
        PeakPower01 = 0f;
        ActiveInputDuration = 0f;
        CurrentHeight = 0f;
        MaxHeight = 0f;
        FinalResult = 0f;
        lowPowerTime = 0f;
    }

    private void StartAttempt()
    {
        AttemptActive = true;
        PeakPower01 = CurrentPower01;
        ActiveInputDuration = 0f;
        MaxHeight = Mathf.Max(0f, CurrentHeight);
        FinalResult = 0f;
        lowPowerTime = 0f;
    }

    private void EndAttempt()
    {
        AttemptActive = false;
        FinalResult = Mathf.Round((MaxHeight * 100f + PeakPower01 * 100f + ActiveInputDuration * 10f) * 0.1f);
    }
}
