using System.Collections;
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

    [Header("Gameplay")]
    [SerializeField, Min(0f)] private float startThreshold = 0.08f;
    [SerializeField] private bool requireReleaseBetweenAttempts = true;
    [SerializeField] private bool freezePhysicsOutsideAttempt = true;

    [Header("Reload Transition")]
    [SerializeField] private KeyCode reloadSceneKey = KeyCode.Alpha2;
    [SerializeField, Min(0.01f)] private float reloadFadeDuration = 0.45f;
    [SerializeField, Min(0f)] private float reloadBlackPause = 0.05f;

    [Header("Events")]
    [SerializeField] private UnityEvent onAttemptStarted;

    private AttemptState _state;
    private bool _armedForNextAttempt;
    private bool _isReloadingScene;
    private CanvasGroup _reloadFadeOverlay;

    public bool IsAttemptRunning => _state == AttemptState.Running;
    public KeyCode ReloadSceneKey => reloadSceneKey;

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
        GoToWaitingState();
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

        switch (_state)
        {
            case AttemptState.WaitingForInput:
                TryStartAttempt(power01);
                break;

            case AttemptState.Running:
                break;
        }
    }

    public void RestartNow()
    {
        if (_isReloadingScene)
        {
            return;
        }

        StartCoroutine(ReloadSceneWithFadeCoroutine());
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
        _armedForNextAttempt = false;

        if (rocketFlightController != null)
        {
            rocketFlightController.ResetToStart();
            rocketFlightController.enabled = true;
        }

        SetPhysicsActive(true);
        onAttemptStarted?.Invoke();
    }

    private void GoToWaitingState()
    {
        _state = AttemptState.WaitingForInput;
        _armedForNextAttempt = !requireReleaseBetweenAttempts;

        if (rocketFlightController != null)
        {
            rocketFlightController.enabled = false;
            rocketFlightController.ResetToStart();
        }

        SetPhysicsActive(false);
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