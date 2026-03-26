using UnityEngine;

public sealed class AttemptResultTracker : MonoBehaviour
{
    [SerializeField] private RocketInputModeController inputModeController;
    [SerializeField] private RocketFlightController rocketFlightController;
    [SerializeField, Min(0f)] private float startThreshold = 0.05f;

    public bool AttemptActive { get; private set; }
    public float CurrentPower01 { get; private set; }
    public float PeakPower01 { get; private set; }
    public float CurrentHeight { get; private set; }
    public float MaxHeight { get; private set; }
    public float FinalResult { get; private set; }

    private void OnEnable()
    {
        ResetAttempt();
    }

    private void Update()
    {
        CurrentPower01 = inputModeController == null
            ? 0f
            : Mathf.Clamp01(inputModeController.CurrentPower01);

        CurrentHeight = rocketFlightController == null
            ? 0f
            : Mathf.Max(0f, rocketFlightController.HeightFromStart);

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

        // Теперь результат = просто лучший достигнутый результат попытки.
        // Ничего автоматически не завершается.
        FinalResult = MaxHeight;
    }

    public void ResetAttempt()
    {
        AttemptActive = false;
        CurrentPower01 = 0f;
        PeakPower01 = 0f;
        CurrentHeight = 0f;
        MaxHeight = 0f;
        FinalResult = 0f;
    }

    public void ForceStartAttempt()
    {
        StartAttempt();
    }

    private void StartAttempt()
    {
        AttemptActive = true;
        PeakPower01 = CurrentPower01;
        MaxHeight = CurrentHeight;
        FinalResult = MaxHeight;
    }
}