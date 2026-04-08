using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class RocketUiAutoBuilder
{
    private const string HudRootName = "RocketHUD";

    [MenuItem("Tools/SoundRacket2D/Build HUD")]
    public static void BuildHud()
    {
        Canvas canvas = FindOrCreateCanvas();
        FindOrCreateEventSystem();

        Transform oldRoot = canvas.transform.Find(HudRootName);
        if (oldRoot != null)
        {
            Object.DestroyImmediate(oldRoot.gameObject);
        }

        GameObject root = CreateUiObject(HudRootName, canvas.transform);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        StretchToFullScreen(rootRect);

        RocketHudPresenterTMP presenter = root.AddComponent<RocketHudPresenterTMP>();

        TextMeshProUGUI modeText = CreateText(
            root.transform,
            "ModeText",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(32f, -28f),
            new Vector2(360f, 50f),
            30f,
            TextAlignmentOptions.Left);

        TextMeshProUGUI heightText = CreateText(
            root.transform,
            "HeightText",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -28f),
            new Vector2(260f, 70f),
            56f,
            TextAlignmentOptions.Center);

        TextMeshProUGUI stateText = CreateText(
            root.transform,
            "StateText",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-32f, -24f),
            new Vector2(260f, 40f),
            28f,
            TextAlignmentOptions.Right);

        TextMeshProUGUI statusText = CreateText(
            root.transform,
            "StatusText",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-32f, -62f),
            new Vector2(500f, 36f),
            22f,
            TextAlignmentOptions.Right);

        TextMeshProUGUI hintText = CreateText(
            root.transform,
            "HintText",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 22f),
            new Vector2(900f, 40f),
            24f,
            TextAlignmentOptions.Center);

        GameObject barsRoot = CreateUiObject("PowerBarsRoot", root.transform);
        RectTransform barsRootRect = barsRoot.GetComponent<RectTransform>();
        barsRootRect.anchorMin = new Vector2(0f, 0f);
        barsRootRect.anchorMax = new Vector2(0f, 0f);
        barsRootRect.pivot = new Vector2(0f, 0f);
        barsRootRect.anchoredPosition = new Vector2(32f, 32f);
        barsRootRect.sizeDelta = new Vector2(320f, 160f);

        Image[] bars = CreateBars(barsRoot.transform);

        RocketInputModeController inputModeController = Object.FindObjectOfType<RocketInputModeController>();
        RocketFlightController rocketFlightController = Object.FindObjectOfType<RocketFlightController>();
        MicrophoneInputService microphoneInputService = Object.FindObjectOfType<MicrophoneInputService>();
        AttemptSessionController attemptSessionController = Object.FindObjectOfType<AttemptSessionController>();

        presenter.Setup(
            inputModeController,
            rocketFlightController,
            microphoneInputService,
            attemptSessionController,
            modeText,
            heightText,
            stateText,
            statusText,
            hintText,
            bars);

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Selection.activeGameObject = root;
    }

    private static Canvas FindOrCreateCanvas()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            return canvas;
        }

        GameObject canvasObject = new GameObject(
            "Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private static void FindOrCreateEventSystem()
    {
        EventSystem existing = Object.FindObjectOfType<EventSystem>();
        if (existing != null)
        {
            return;
        }

        new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule));
    }

    private static GameObject CreateUiObject(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void StretchToFullScreen(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.enableWordWrapping = false;
        text.text = objectName;

        return text;
    }

    private static Image[] CreateBars(Transform parent)
    {
        const int barsCount = 10;
        const float barWidth = 18f;
        const float spacing = 8f;
        const float minHeight = 28f;
        const float heightStep = 10f;

        Image[] result = new Image[barsCount];
        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        for (int i = 0; i < barsCount; i++)
        {
            GameObject barObject = new GameObject(
                "Bar_" + (i + 1),
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            barObject.transform.SetParent(parent, false);

            RectTransform rectTransform = barObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 0f);
            rectTransform.pivot = new Vector2(0f, 0f);
            rectTransform.anchoredPosition = new Vector2(i * (barWidth + spacing), 0f);
            rectTransform.sizeDelta = new Vector2(barWidth, minHeight + i * heightStep);

            Image image = barObject.GetComponent<Image>();
            image.sprite = uiSprite;
            image.type = Image.Type.Simple;
            image.color = new Color(1f, 0.72f, 0.18f, 0.08f);

            result[i] = image;
        }

        return result;
    }
}