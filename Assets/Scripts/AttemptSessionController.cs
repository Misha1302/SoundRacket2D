using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class AttemptSessionController : MonoBehaviour
{
    private enum AttemptState
    {
        WaitingForInput,
        Running
    }

    [Header("References")]
    [SerializeField] private RocketFlightController rocketFlightController;
    [SerializeField] private RocketInputModeController inputModeController;
    [SerializeField] private Rigidbody2D rocketRigidbody;

    [Header("HUD")]
    [SerializeField] private Image powerFillImage;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private TMP_Text heightText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text hintText;

    [Header("Gameplay")]
    [SerializeField, Min(0f)] private float startThreshold = 0.08f;
    [SerializeField, Min(0f)] private float hudSmoothness = 10f;
    [SerializeField] private bool requireReleaseBetweenAttempts = true;
    [SerializeField] private bool freezePhysicsOutsideAttempt = true;

    [Header("Reload Transition")]
    [SerializeField] private KeyCode reloadSceneKey = KeyCode.Alpha2;
    [SerializeField, Min(0.01f)] private float reloadFadeDuration = 0.45f;
    [SerializeField, Min(0f)] private float reloadBlackPause = 0.05f;

    [Header("Events")]
    [SerializeField] private UnityEvent onAttemptStarted;

    private AttemptState _state;
    private float _attemptTime;
    private float _peakHeight;
    private float _peakPower01;
    private float _smoothedHudPower;
    private bool _armedForNextAttempt;
    private bool _isReloadingScene;
    private CanvasGroup _reloadFadeOverlay;

    public bool IsAttemptRunning => _state == AttemptState.Running;
    public float PeakHeightCurrentAttempt => _peakHeight;
    public float PeakPowerCurrentAttempt01 => _peakPower01;

    private void Reset()
    {
        rocketFlightController = FindObjectOfType<RocketFlightController>();
        inputModeController = FindObjectOfType<RocketInputModeController>();

        if (rocketFlightController != null)
        {
            rocketRigidbody = rocketFlightController.GetComponent<Rigidbody2D>();
        }
    }

    private void Awake()
    {
        if (rocketRigidbody == null && rocketFlightController != null)
        {
            rocketRigidbody = rocketFlightController.GetComponent<Rigidbody2D>();
        }
    }

    private void Start()
    {
        GoToWaitingState(clearResultText: true);
    }

    private void Update()
    {
        if (!_isReloadingScene && Input.GetKeyDown(reloadSceneKey))
        {
            StartCoroutine(ReloadSceneWithFadeCoroutine());
            return;
        }

        float power01 = inputModeController == null
            ? 0f
            : Mathf.Clamp01(inputModeController.CurrentPower01);

        if (power01 <= 0.001f)
        {
            _armedForNextAttempt = true;
        }

        UpdateHud(power01);

        switch (_state)
        {
            case AttemptState.WaitingForInput:
                TryStartAttempt(power01);
                break;

            case AttemptState.Running:
                UpdateRunningAttempt(power01);
                break;
        }
    }

    public void RestartNow()
    {
        GoToWaitingState(clearResultText: false);
    }

    private void TryStartAttempt(float power01)
    {
        if (power01 < startThreshold)
        {
            return;
        }

        if (requireReleaseBetweenAttempts && !_armedForNextAttempt)
        {
            return;
        }

        StartAttempt();
    }

    private void StartAttempt()
    {
        _state = AttemptState.Running;
        _attemptTime = 0f;
        _peakHeight = 0f;
        _peakPower01 = 0f;
        _armedForNextAttempt = false;

        if (rocketFlightController != null)
        {
            rocketFlightController.ResetToStart();
            rocketFlightController.enabled = true;
        }

        SetPhysicsActive(true);

        if (resultText != null)
        {
            resultText.text = "Рекорд попытки: 0.0 м";
        }

        onAttemptStarted?.Invoke();
    }

    private void UpdateRunningAttempt(float power01)
    {
        _attemptTime += Time.deltaTime;
        _peakPower01 = Mathf.Max(_peakPower01, power01);

        if (rocketFlightController != null)
        {
            _peakHeight = Mathf.Max(_peakHeight, Mathf.Max(0f, rocketFlightController.HeightFromStart));
        }

        if (resultText != null)
        {
            resultText.text = $"Рекорд попытки: {_peakHeight:0.0} м";
        }
    }

    private void GoToWaitingState(bool clearResultText)
    {
        _state = AttemptState.WaitingForInput;
        _attemptTime = 0f;
        _peakHeight = 0f;
        _peakPower01 = 0f;
        _smoothedHudPower = 0f;
        _armedForNextAttempt = !requireReleaseBetweenAttempts;

        if (rocketFlightController != null)
        {
            rocketFlightController.enabled = false;
            rocketFlightController.ResetToStart();
        }

        SetPhysicsActive(false);

        if (clearResultText && resultText != null)
        {
            resultText.text = "Рекорд попытки: 0.0 м";
        }
    }

    private void SetPhysicsActive(bool active)
    {
        if (rocketRigidbody == null)
        {
            return;
        }

        rocketRigidbody.velocity = Vector2.zero;
        rocketRigidbody.angularVelocity = 0f;

        if (freezePhysicsOutsideAttempt)
        {
            rocketRigidbody.simulated = active;
        }
    }

    private void UpdateHud(float rawPower01)
    {
        _smoothedHudPower = Mathf.Lerp(
            _smoothedHudPower,
            rawPower01,
            1f - Mathf.Exp(-hudSmoothness * Time.unscaledDeltaTime));

        if (powerFillImage != null)
        {
            powerFillImage.fillAmount = _smoothedHudPower;
        }

        if (modeText != null && inputModeController != null)
        {
            modeText.text = inputModeController.CurrentMode == RocketInputModeController.InputMode.Voice
                ? "Режим: микрофон"
                : "Режим: кнопка";
        }

        if (heightText != null)
        {
            float height = rocketFlightController == null
                ? 0f
                : Mathf.Max(0f, rocketFlightController.HeightFromStart);

            heightText.text = $"Высота: {height:0.0} м";
        }

        if (stateText != null)
        {
            stateText.text = _state switch
            {
                AttemptState.WaitingForInput => "Готово",
                AttemptState.Running => "Игра идет",
                _ => string.Empty
            };
        }

        if (hintText != null)
        {
            hintText.text = BuildHintText();
        }
    }

    private string BuildHintText()
    {
        if (_state == AttemptState.Running)
        {
            return $"Нажми {GetReadableKeyName(reloadSceneKey)} для завершения игры";
        }

        if (inputModeController == null)
        {
            return "Назначь ссылки в инспекторе";
        }

        return inputModeController.CurrentMode == RocketInputModeController.InputMode.Voice
            ? "Кричи в микрофон, чтобы запустить ракету"
            : "Удерживай кнопку, чтобы запустить ракету";
    }

    private static string GetReadableKeyName(KeyCode keyCode)
    {
        return keyCode switch
        {
            KeyCode.Alpha0 => "0",
            KeyCode.Alpha1 => "1",
            KeyCode.Alpha2 => "2",
            KeyCode.Alpha3 => "3",
            KeyCode.Alpha4 => "4",
            KeyCode.Alpha5 => "5",
            KeyCode.Alpha6 => "6",
            KeyCode.Alpha7 => "7",
            KeyCode.Alpha8 => "8",
            KeyCode.Alpha9 => "9",
            KeyCode.Keypad0 => "Num 0",
            KeyCode.Keypad1 => "Num 1",
            KeyCode.Keypad2 => "Num 2",
            KeyCode.Keypad3 => "Num 3",
            KeyCode.Keypad4 => "Num 4",
            KeyCode.Keypad5 => "Num 5",
            KeyCode.Keypad6 => "Num 6",
            KeyCode.Keypad7 => "Num 7",
            KeyCode.Keypad8 => "Num 8",
            KeyCode.Keypad9 => "Num 9",
            _ => keyCode.ToString()
        };
    }

    private IEnumerator ReloadSceneWithFadeCoroutine()
    {
        _isReloadingScene = true;

        if (rocketFlightController != null)
        {
            rocketFlightController.enabled = false;
        }

        SetPhysicsActive(false);

        CanvasGroup overlay = GetOrCreateReloadFadeOverlay();
        overlay.blocksRaycasts = true;
        overlay.interactable = false;
        overlay.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < reloadFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            overlay.alpha = Mathf.Clamp01(elapsed / reloadFadeDuration);
            yield return null;
        }

        overlay.alpha = 1f;

        if (reloadBlackPause > 0f)
        {
            yield return new WaitForSecondsRealtime(reloadBlackPause);
        }

        Scene activeScene = SceneManager.GetActiveScene();
        string sceneToLoad = !string.IsNullOrEmpty(activeScene.path)
            ? activeScene.path
            : activeScene.name;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);

        while (!loadOperation.isDone)
        {
            yield return null;
        }
    }

    private CanvasGroup GetOrCreateReloadFadeOverlay()
    {
        if (_reloadFadeOverlay != null)
        {
            return _reloadFadeOverlay;
        }

        GameObject overlayObject = new GameObject(
            "[Reload Fade Overlay]",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup),
            typeof(Image));

        Canvas canvas = overlayObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler scaler = overlayObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform rectTransform = overlayObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = overlayObject.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;

        _reloadFadeOverlay = overlayObject.GetComponent<CanvasGroup>();
        _reloadFadeOverlay.alpha = 0f;
        _reloadFadeOverlay.blocksRaycasts = false;
        _reloadFadeOverlay.interactable = false;

        return _reloadFadeOverlay;
    }
}