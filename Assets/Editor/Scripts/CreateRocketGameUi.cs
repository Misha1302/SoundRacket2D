using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CreateRocketGameUi
{
    private const string RootName = "[Generated Rocket HUD]";
    private const string GeneratedFolderPath = "Assets/Generated";
    private const string GeneratedUiFolderPath = "Assets/Generated/UI";
    private const string GeneratedFontFolderPath = "Assets/Generated/UI/Fonts";

    private const string NormalFontPath = "Assets/Sprites/шрифт топ DS-Digital/DS-DIGI.TTF";
    private const string BoldFontPath = "Assets/Sprites/шрифт топ DS-Digital/DS-DIGIB.TTF";

    private const string VolumeSpritePath = "Assets/Sprites/громкость.png";
    private const string RocketSpritePath = "Assets/Sprites/Ракета.png";

    private static bool EnsureTmpEssentials()
    {
        if (TMP_Settings.GetSettings() != null && TMP_Settings.defaultFontAsset != null)
        {
            return true;
        }

        Debug.Log("TMP Essential Resources are missing. Importing them now...");
        TMP_PackageUtilities.ImportProjectResourcesMenu();
        AssetDatabase.Refresh();

        if (TMP_Settings.GetSettings() == null || TMP_Settings.defaultFontAsset == null)
        {
            Debug.LogWarning("TMP resources were imported. Run 'Create Beautiful UI' one more time.");
            return false;
        }

        return true;
    }

    [MenuItem("Tools/Sound Rocket/Create Beautiful UI")]
    public static void CreateBeautifulUi()
    {        
        if (!EnsureTmpEssentials())
        {
            return;
        }

        EnsureFolders();

        Canvas canvas = GetOrCreateCanvas();
        EnsureEventSystem();

        GameObject root = GetOrCreateRoot(canvas.transform);
        ClearChildren(root.transform);

        TMP_FontAsset normalFont = GetOrCreateTmpFontAsset(NormalFontPath, "DS-DIGI SDF.asset");
        TMP_FontAsset boldFont = GetOrCreateTmpFontAsset(BoldFontPath, "DS-DIGIB SDF.asset");

        Sprite volumeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(VolumeSpritePath);
        Sprite rocketSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RocketSpritePath);

        CreateBackdrop(root.transform);
        CreateDecor(root.transform, rocketSprite);

        RectTransform topBar = CreatePanel(
            "TopBar",
            root.transform,
            new Color(0.04f, 0.08f, 0.14f, 0.84f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -28f),
            new Vector2(-80f, 110f));

        CreateText(
            "Title",
            topBar,
            boldFont,
            "SOUND ROCKET",
            42f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            new Color(0.83f, 0.97f, 1f, 1f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(40f, 12f),
            new Vector2(700f, 60f));

        CreateText(
            "Subtitle",
            topBar,
            normalFont,
            "Shout into the mic or hold the key to launch",
            21f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            new Color(0.67f, 0.87f, 0.94f, 0.95f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(42f, -28f),
            new Vector2(840f, 36f));

        RectTransform leftPanel = CreatePanel(
            "LeftInfoPanel",
            root.transform,
            new Color(0.05f, 0.10f, 0.16f, 0.78f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(42f, 0f),
            new Vector2(430f, 390f));

        TMP_Text stateText = CreateLabeledValue(
            leftPanel,
            normalFont,
            boldFont,
            "STATE",
            "Готово",
            34f,
            new Vector2(28f, -42f));

        TMP_Text modeText = CreateLabeledValue(
            leftPanel,
            normalFont,
            boldFont,
            "MODE",
            "Режим: микрофон",
            28f,
            new Vector2(28f, -132f));

        TMP_Text hintText = CreateText(
            "HintText",
            leftPanel,
            normalFont,
            "Кричи в микрофон, чтобы запустить ракету",
            24f,
            FontStyles.Normal,
            TextAlignmentOptions.TopLeft,
            new Color(0.88f, 0.95f, 1f, 0.96f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
            new Vector2(28f, -220f),
            new Vector2(-56f, 90f));

        Button voiceButton = CreateButton(
            "VoiceButton",
            leftPanel,
            normalFont,
            "МИКРОФОН",
            new Color(0.10f, 0.44f, 0.63f, 0.96f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(28f, 92f),
            new Vector2(170f, 58f));

        Button keyboardButton = CreateButton(
            "KeyboardButton",
            leftPanel,
            normalFont,
            "КНОПКА",
            new Color(0.15f, 0.31f, 0.54f, 0.96f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(212f, 92f),
            new Vector2(170f, 58f));

        Button restartButton = CreateButton(
            "RestartButton",
            leftPanel,
            normalFont,
            "ПОВТОР",
            new Color(0.19f, 0.57f, 0.76f, 0.96f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(28f, 24f),
            new Vector2(354f, 54f));

        RectTransform rightPanel = CreatePanel(
            "RightPowerPanel",
            root.transform,
            new Color(0.05f, 0.10f, 0.16f, 0.78f),
            new Vector2(1f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(-42f, 0f),
            new Vector2(240f, 520f));

        CreateText(
            "PowerLabel",
            rightPanel,
            normalFont,
            "POWER",
            30f,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            new Color(0.86f, 0.95f, 1f, 1f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -32f),
            new Vector2(-30f, 42f));

        if (volumeSprite != null)
        {
            Image icon = CreateImage(
                "VolumeIcon",
                rightPanel,
                volumeSprite,
                new Color(1f, 1f, 1f, 0.95f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -88f),
                new Vector2(60f, 60f));

            icon.preserveAspect = true;
        }

        RectTransform meterFrame = CreatePanel(
            "PowerMeterFrame",
            rightPanel,
            new Color(0.02f, 0.04f, 0.08f, 0.95f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 42f),
            new Vector2(92f, 320f));

        Outline frameOutline = meterFrame.gameObject.AddComponent<Outline>();
        frameOutline.effectColor = new Color(0.26f, 0.84f, 1f, 0.55f);
        frameOutline.effectDistance = new Vector2(2f, 2f);

        RectTransform meterBackground = CreateImage(
            "MeterBackground",
            meterFrame,
            null,
            new Color(0.08f, 0.15f, 0.22f, 1f),
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(-18f, -18f)).rectTransform;

        Image powerFillImage = CreateImage(
            "PowerFill",
            meterBackground,
            null,
            new Color(0.16f, 0.90f, 1f, 1f),
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 0f),
            Vector2.zero);

        powerFillImage.type = Image.Type.Filled;
        powerFillImage.fillMethod = Image.FillMethod.Vertical;
        powerFillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        powerFillImage.fillAmount = 0.35f;

        Shadow fillShadow = powerFillImage.gameObject.AddComponent<Shadow>();
        fillShadow.effectColor = new Color(0.18f, 0.86f, 1f, 0.45f);
        fillShadow.effectDistance = new Vector2(0f, 0f);

        CreateText(
            "PowerBottomLabel",
            rightPanel,
            normalFont,
            "LOUDER = HIGHER",
            20f,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            new Color(0.84f, 0.95f, 1f, 0.85f),
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 16f),
            new Vector2(-22f, 28f));

        RectTransform centerCard = CreatePanel(
            "CenterResultCard",
            root.transform,
            new Color(0.05f, 0.11f, 0.18f, 0.82f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 34f),
            new Vector2(620f, 180f));

        CreateText(
            "ResultCaption",
            centerCard,
            normalFont,
            "RESULT",
            26f,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            new Color(0.71f, 0.89f, 0.98f, 1f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -28f),
            new Vector2(-24f, 34f));

        TMP_Text resultText = CreateText(
            "ResultText",
            centerCard,
            boldFont,
            "Результат: —",
            44f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            new Color(0.91f, 0.99f, 1f, 1f),
            new Vector2(0f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f),
            new Vector2(-40f, 60f));

        TMP_Text heightText = CreateText(
            "HeightText",
            centerCard,
            boldFont,
            "Высота: 0.0 м",
            30f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            new Color(0.38f, 0.91f, 1f, 1f),
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 22f),
            new Vector2(-40f, 34f));

        AttemptSessionController attemptController = Object.FindObjectOfType<AttemptSessionController>();
        RocketInputModeController inputModeController = Object.FindObjectOfType<RocketInputModeController>();

        TryAssignAttemptSessionUi(
            attemptController,
            powerFillImage,
            stateText,
            modeText,
            heightText,
            resultText,
            hintText);

        TryBindButtons(voiceButton, keyboardButton, restartButton, inputModeController, attemptController);

        Selection.activeObject = root;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log("Rocket UI created successfully.");
    }

    private static void EnsureFolders()
    {
        CreateFolderIfMissing("Assets", "Generated");
        CreateFolderIfMissing(GeneratedFolderPath, "UI");
        CreateFolderIfMissing(GeneratedUiFolderPath, "Fonts");
    }

    private static void CreateFolderIfMissing(string parent, string folderName)
    {
        string fullPath = parent + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }

    private static Canvas GetOrCreateCanvas()
    {
        Canvas existingCanvas = Object.FindObjectOfType<Canvas>();

        if (existingCanvas != null)
        {
            CanvasScaler existingScaler = existingCanvas.GetComponent<CanvasScaler>();
            if (existingScaler == null)
            {
                existingScaler = existingCanvas.gameObject.AddComponent<CanvasScaler>();
            }

            ConfigureCanvasScaler(existingScaler);
            return existingCanvas;
        }

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;
        ConfigureCanvasScaler(canvasObject.GetComponent<CanvasScaler>());

        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");
        return canvas;
    }

    private static void ConfigureCanvasScaler(CanvasScaler scaler)
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
    }

    private static GameObject GetOrCreateRoot(Transform canvasTransform)
    {
        Transform existing = canvasTransform.Find(RootName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject root = new GameObject(RootName, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(root, "Create Rocket UI Root");
        root.transform.SetParent(canvasTransform, false);

        RectTransform rectTransform = root.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        return root;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; --i)
        {
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static TMP_FontAsset GetOrCreateTmpFontAsset(string sourceFontPath, string outputAssetName)
    {
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(sourceFontPath);
        if (sourceFont == null)
        {
            Debug.LogWarning("Source font not found: " + sourceFontPath);
            return TMP_Settings.defaultFontAsset;
        }

        string outputPath = Path.Combine(GeneratedFontFolderPath, outputAssetName).Replace("\\", "/");
        TMP_FontAsset existingAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outputPath);
        if (existingAsset != null)
        {
            return existingAsset;
        }

        try
        {
            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            AssetDatabase.CreateAsset(fontAsset, outputPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outputPath);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("Failed to create TMP font asset from: " + sourceFontPath + "\n" + exception);
            return TMP_Settings.defaultFontAsset;
        }
    }

    private static void CreateBackdrop(Transform parent)
    {
        CreateImage(
            "Backdrop",
            parent,
            null,
            new Color(0.01f, 0.03f, 0.06f, 0.74f),
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero);
    }

    private static void CreateDecor(Transform parent, Sprite rocketSprite)
    {
        if (rocketSprite == null)
        {
            return;
        }

        Image rocketDecor = CreateImage(
            "RocketDecor",
            parent,
            rocketSprite,
            new Color(1f, 1f, 1f, 0.07f),
            new Vector2(1f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(-260f, -40f),
            new Vector2(420f, 420f));

        rocketDecor.preserveAspect = true;
    }

    private static RectTransform CreatePanel(
        string name,
        Transform parent,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        Image image = CreateImage(name, parent, null, color, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta);
        Shadow shadow = image.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.30f);
        shadow.effectDistance = new Vector2(0f, -10f);
        return image.rectTransform;
    }

    private static Image CreateImage(
        string name,
        Transform parent,
        Sprite sprite,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(gameObject, "Create " + name);
        gameObject.transform.SetParent(parent, false);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        Image image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;

        return image;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        TMP_FontAsset font,
        string text,
        float fontSize,
        FontStyles fontStyle,
        TextAlignmentOptions alignment,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(gameObject, "Create " + name);
        gameObject.transform.SetParent(parent, false);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        TextMeshProUGUI tmp = gameObject.GetComponent<TextMeshProUGUI>();
        tmp.font = font != null ? font : TMP_Settings.defaultFontAsset;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = fontStyle;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;

        return tmp;
    }

    private static TMP_Text CreateLabeledValue(
        Transform parent,
        TMP_FontAsset labelFont,
        TMP_FontAsset valueFont,
        string label,
        string value,
        float valueFontSize,
        Vector2 labelPosition)
    {
        CreateText(
            label + "_Label",
            parent,
            labelFont,
            label,
            20f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            new Color(0.62f, 0.82f, 0.92f, 0.92f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
            labelPosition,
            new Vector2(-56f, 24f));

        return CreateText(
            label + "_Value",
            parent,
            valueFont,
            value,
            valueFontSize,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            new Color(0.93f, 0.99f, 1f, 1f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
            labelPosition + new Vector2(0f, -36f),
            new Vector2(-56f, 42f));
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        TMP_FontAsset font,
        string text,
        Color baseColor,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(gameObject, "Create " + name);
        gameObject.transform.SetParent(parent, false);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        Image image = gameObject.GetComponent<Image>();
        image.color = baseColor;

        Shadow shadow = gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.26f);
        shadow.effectDistance = new Vector2(0f, -4f);

        Button button = gameObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = baseColor * 1.08f;
        colors.pressedColor = baseColor * 0.90f;
        colors.selectedColor = baseColor * 1.04f;
        colors.disabledColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.45f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        TMP_Text label = CreateText(
            "Label",
            gameObject.transform,
            font,
            text,
            23f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            Color.white,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero);

        label.enableWordWrapping = false;

        return button;
    }

    private static void TryAssignAttemptSessionUi(
        AttemptSessionController attemptController,
        Image powerFillImage,
        TMP_Text stateText,
        TMP_Text modeText,
        TMP_Text heightText,
        TMP_Text resultText,
        TMP_Text hintText)
    {
        if (attemptController == null)
        {
            Debug.LogWarning("AttemptSessionController not found in scene. UI was created, but references were not assigned.");
            return;
        }

        SerializedObject serializedObject = new SerializedObject(attemptController);

        SetObjectReference(serializedObject, "powerFillImage", powerFillImage);
        SetObjectReference(serializedObject, "stateText", stateText);
        SetObjectReference(serializedObject, "modeText", modeText);
        SetObjectReference(serializedObject, "heightText", heightText);
        SetObjectReference(serializedObject, "resultText", resultText);
        SetObjectReference(serializedObject, "hintText", hintText);

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(attemptController);
    }

    private static void SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void ClearPersistentListeners(UnityEngine.Events.UnityEvent unityEvent)
    {
        for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; --i)
        {
            UnityEventTools.RemovePersistentListener(unityEvent, i);
        }
    }

    private static void TryBindButtons(
        Button voiceButton,
        Button keyboardButton,
        Button restartButton,
        RocketInputModeController inputModeController,
        AttemptSessionController attemptController)
    {
        if (inputModeController != null)
        {
            ClearPersistentListeners(voiceButton.onClick);
            ClearPersistentListeners(keyboardButton.onClick);

            UnityEventTools.AddPersistentListener(voiceButton.onClick, inputModeController.SwitchToVoice);
            UnityEventTools.AddPersistentListener(keyboardButton.onClick, inputModeController.SwitchToKeyboard);

            EditorUtility.SetDirty(voiceButton);
            EditorUtility.SetDirty(keyboardButton);
        }
        else
        {
            Debug.LogWarning("RocketInputModeController not found. Mode buttons were created without bindings.");
        }

        if (attemptController != null)
        {
            ClearPersistentListeners(restartButton.onClick);

            UnityEventTools.AddPersistentListener(restartButton.onClick, attemptController.RestartNow);
            EditorUtility.SetDirty(restartButton);
        }
        else
        {
            Debug.LogWarning("AttemptSessionController not found. Restart button was created without binding.");
        }
    }
}