using UnityEngine;

public sealed class RocketInputModeController : MonoBehaviour
{
    public enum InputMode
    {
        Voice = 0,
        Keyboard = 1
    }

    [SerializeField] private InputMode startMode = InputMode.Voice;
    [SerializeField] private MicrophoneInputService microphoneInputService;
    [SerializeField] private KeyboardInputService keyboardInputService;

    public InputMode CurrentMode { get; private set; }
    public float CurrentPower01 { get; private set; }

    private void Awake()
    {
        CurrentMode = startMode;
    }

    private void Update()
    {
        CurrentPower01 = GetPowerByMode(CurrentMode);
    }

    public void SetMode(InputMode mode)
    {
        CurrentMode = mode;
    }

    public void SwitchToVoice() => SetMode(InputMode.Voice);

    public void SwitchToKeyboard() => SetMode(InputMode.Keyboard);

    public void ToggleMode()
    {
        SetMode(CurrentMode == InputMode.Voice ? InputMode.Keyboard : InputMode.Voice);
    }

    private float GetPowerByMode(InputMode mode)
    {
        if (mode == InputMode.Keyboard)
        {
            return keyboardInputService == null ? 0f : keyboardInputService.CurrentPower01;
        }

        return microphoneInputService == null ? 0f : microphoneInputService.NormalizedLoudness01;
    }
}
