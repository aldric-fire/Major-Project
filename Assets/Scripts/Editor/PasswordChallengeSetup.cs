using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Editor utility that auto-creates the entire Password Challenge setup:
/// - ChallengeData ScriptableObject asset
/// - OptionButton prefab
/// - HUD Canvas with interact prompt
/// - PasswordUI panel with all child elements wired up
/// - GameManager object with ChallengeManager
/// - PasswordStation test cube in the scene
///
/// Run via: Tools > Cybersecurity > Setup Password Challenge
/// </summary>
public static class PasswordChallengeSetup
{
    [MenuItem("Tools/Cybersecurity/Setup Password Challenge")]
    public static void Setup()
    {
        // ── 1. Create folders ──
        EnsureFolder("Assets/ChallengeData");
        EnsureFolder("Assets/Prefabs/UI");

        // ── 2. Create ChallengeData ScriptableObject ──
        ChallengeData data = CreatePasswordChallengeData();

        // ── 3. Create OptionButton prefab ──
        GameObject optionButtonPrefab = CreateOptionButtonPrefab();

        // ── 4. Find or create GameManager ──
        GameObject gameManagerObj = SetupGameManager(data);

        // ── 5. Create HUD Canvas ──
        Canvas hudCanvas;
        HUDController hudController;
        CreateHUDCanvas(out hudCanvas, out hudController);

        // ── 6. Create PasswordUI panel inside the canvas ──
        CreatePasswordUIPanel(hudCanvas, optionButtonPrefab);

        // ── 7. Create PasswordStation test cube ──
        CreatePasswordStation(data);

        // ── 8. Done ──
        EditorUtility.DisplayDialog(
            "Password Challenge Setup Complete",
            "Created:\n" +
            "• ChallengeData asset (Assets/ChallengeData/Password_01)\n" +
            "• OptionButton prefab (Assets/Prefabs/UI/OptionButton)\n" +
            "• GameManager object (with ChallengeManager)\n" +
            "• HUDCanvas (with interact prompt)\n" +
            "• PasswordUIPanel (inside HUDCanvas)\n" +
            "• PasswordStation test cube\n\n" +
            "IMPORTANT: Add the InteractionController component to your Player prefab if not already added.\n\n" +
            "Press Play and walk up to the green cube to test!",
            "OK"
        );

        Debug.Log("[PasswordChallengeSetup] Setup complete!");
    }

    // ═══════════════════════════════════════════════════════
    //  STEP 1: ChallengeData ScriptableObject
    // ═══════════════════════════════════════════════════════

    private static ChallengeData CreatePasswordChallengeData()
    {
        string path = "Assets/ChallengeData/Password_01.asset";

        // Check if it already exists
        ChallengeData existing = AssetDatabase.LoadAssetAtPath<ChallengeData>(path);
        if (existing != null)
        {
            Debug.Log("[Setup] ChallengeData already exists, reusing.");
            return existing;
        }

        ChallengeData data = ScriptableObject.CreateInstance<ChallengeData>();
        data.challengeId = "password_01";
        data.title = "Password Security Check";
        data.description =
            "You found a sticky note on a colleague's monitor with the password \"Company123!\" written on it.\n\n" +
            "You also need to update your own password. Choose the strongest option below.";
        data.challengeType = ChallengeType.Password;
        data.difficultyTier = 1;

        data.options = new List<ChallengeOption>
        {
            new ChallengeOption
            {
                text = "Company123!",
                isCorrect = false,
                feedbackNarrative = "This is the same weak password from the sticky note — easily guessed using company info."
            },
            new ChallengeOption
            {
                text = "password",
                isCorrect = false,
                feedbackNarrative = "This is the #1 most common password in the world. It would be cracked in under 1 second."
            },
            new ChallengeOption
            {
                text = "T!g3r$Blu3-M0unt@1n",
                isCorrect = true,
                feedbackNarrative = "Great choice! This password is long, uses mixed case, numbers, and special symbols with no dictionary words."
            },
            new ChallengeOption
            {
                text = "John2024",
                isCorrect = false,
                feedbackNarrative = "Passwords containing names and years are very common and easily guessed by attackers."
            }
        };

        data.consequenceNarrative =
            "BREACH: An attacker used weak credentials to access the payroll database. Employee records have been compromised.";
        data.consequenceType = ConsequenceType.DataBreach;
        data.debriefText =
            "Weak passwords are one of the easiest entry points for attackers. " +
            "Never reuse passwords, avoid dictionary words, and never write passwords on sticky notes. " +
            "Use a password manager instead.";
        data.realWorldStat = "81% of data breaches are caused by weak or stolen passwords. (Verizon DBIR)";

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        Debug.Log("[Setup] Created ChallengeData at " + path);
        return data;
    }

    // ═══════════════════════════════════════════════════════
    //  STEP 2: OptionButton Prefab
    // ═══════════════════════════════════════════════════════

    private static GameObject CreateOptionButtonPrefab()
    {
        string path = "Assets/Prefabs/UI/OptionButton.prefab";

        // Check if it already exists
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            Debug.Log("[Setup] OptionButton prefab already exists, reusing.");
            return existing;
        }

        // Build it in-scene temporarily
        GameObject btnObj = new GameObject("OptionButton", typeof(RectTransform));
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(480f, 55f);

        // Image background
        Image bg = btnObj.AddComponent<Image>();
        bg.color = new Color(0.9f, 0.9f, 0.95f, 1f);

        // Button component
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = bg;

        // Hover/press colors
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.9f, 0.9f, 0.95f, 1f);
        cb.highlightedColor = new Color(0.7f, 0.8f, 1f, 1f);
        cb.pressedColor = new Color(0.5f, 0.6f, 0.9f, 1f);
        cb.selectedColor = new Color(0.7f, 0.8f, 1f, 1f);
        btn.colors = cb;

        // Text child
        GameObject textObj = new GameObject("Text (TMP)", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(10f, 5f);
        textRt.offsetMax = new Vector2(-10f, -5f);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Option";
        tmp.fontSize = 20f;
        tmp.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.textWrappingMode = TextWrappingModes.Normal;

        // Save as prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(btnObj, path);
        Object.DestroyImmediate(btnObj);

        Debug.Log("[Setup] Created OptionButton prefab at " + path);
        return prefab;
    }

    // ═══════════════════════════════════════════════════════
    //  STEP 3: GameManager
    // ═══════════════════════════════════════════════════════

    private static GameObject SetupGameManager(ChallengeData data)
    {
        // Find existing or create new
        ChallengeManager existingCM = Object.FindFirstObjectByType<ChallengeManager>();
        GameObject gmObj;

        if (existingCM != null)
        {
            gmObj = existingCM.gameObject;
            Debug.Log("[Setup] Found existing GameManager object.");
        }
        else
        {
            gmObj = new GameObject("GameManager");
            gmObj.AddComponent<ChallengeManager>();
            Debug.Log("[Setup] Created GameManager object.");
        }

        // Add ConsequenceManager if missing
        if (gmObj.GetComponent<ConsequenceManager>() == null)
            gmObj.AddComponent<ConsequenceManager>();

        // Add GameManager script if missing
        if (gmObj.GetComponent<GameManager>() == null)
            gmObj.AddComponent<GameManager>();

        // Register the challenge data
        ChallengeManager cm = gmObj.GetComponent<ChallengeManager>();
        if (!cm.allChallenges.Contains(data))
        {
            cm.allChallenges.Add(data);
        }

        EditorUtility.SetDirty(cm);
        return gmObj;
    }

    // ═══════════════════════════════════════════════════════
    //  STEP 4: HUD Canvas
    // ═══════════════════════════════════════════════════════

    private static void CreateHUDCanvas(out Canvas canvas, out HUDController hud)
    {
        // Check if HUDCanvas already exists
        HUDController existingHud = Object.FindFirstObjectByType<HUDController>();
        if (existingHud != null)
        {
            canvas = existingHud.GetComponent<Canvas>();
            hud = existingHud;
            Debug.Log("[Setup] Found existing HUDCanvas.");
            return;
        }

        // Create Canvas
        GameObject canvasObj = new GameObject("HUDCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Add HUDController
        hud = canvasObj.AddComponent<HUDController>();

        // ── Interact Prompt Panel ──
        GameObject promptPanel = CreatePanel(canvasObj.transform, "InteractPromptPanel",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), // anchor bottom center
            new Vector2(0f, 80f), new Vector2(400f, 60f),
            new Color(0f, 0f, 0f, 0.75f));

        TextMeshProUGUI promptText = CreateTMPText(promptPanel.transform, "InteractPromptText",
            stretch: true, fontSize: 24f, align: TextAlignmentOptions.Center, color: Color.white,
            defaultText: "Press E to Interact");

        hud.interactPromptPanel = promptPanel;
        hud.interactPromptText = promptText;

        // Prompt starts disabled (code handles showing/hiding)
        promptPanel.SetActive(false);

        Debug.Log("[Setup] Created HUDCanvas with InteractPromptPanel.");
    }

    // ═══════════════════════════════════════════════════════
    //  STEP 5: PasswordUI Panel
    // ═══════════════════════════════════════════════════════

    private static void CreatePasswordUIPanel(Canvas parentCanvas, GameObject optionButtonPrefab)
    {
        // Check if it already exists
        PasswordUI existingUI = Object.FindFirstObjectByType<PasswordUI>(FindObjectsInactive.Include);
        if (existingUI != null)
        {
            Debug.Log("[Setup] PasswordUIPanel already exists, skipping.");
            return;
        }

        // ── Root panel (full screen dark overlay) ──
        GameObject panel = new GameObject("PasswordUIPanel", typeof(RectTransform));
        panel.transform.SetParent(parentCanvas.transform, false);

        RectTransform panelRt = panel.GetComponent<RectTransform>();
        StretchFull(panelRt);

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

        // Add PasswordUI script
        PasswordUI pwUI = panel.AddComponent<PasswordUI>();

        // ── Title ──
        GameObject titleObj = CreateTMPTextObject(panel.transform, "TitleText",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -60f), new Vector2(700f, 60f),
            "Password Security Check", 36f, TextAlignmentOptions.Center, Color.white,
            bold: true);

        // ── Description ──
        GameObject descObj = CreateTMPTextObject(panel.transform, "DescriptionText",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -160f), new Vector2(750f, 120f),
            "Scenario description here...", 20f, TextAlignmentOptions.Center, Color.white);

        // ── Options Container ──
        GameObject containerObj = new GameObject("OptionsContainer", typeof(RectTransform));
        containerObj.transform.SetParent(panel.transform, false);

        RectTransform containerRt = containerObj.GetComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, 0.5f);
        containerRt.anchorMax = new Vector2(0.5f, 0.5f);
        containerRt.pivot = new Vector2(0.5f, 0.5f);
        containerRt.anchoredPosition = new Vector2(0f, -20f);
        containerRt.sizeDelta = new Vector2(500f, 300f);

        VerticalLayoutGroup vlg = containerObj.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = containerObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ── Submit Button ──
        GameObject submitObj = new GameObject("SubmitButton", typeof(RectTransform));
        submitObj.transform.SetParent(panel.transform, false);

        RectTransform submitRt = submitObj.GetComponent<RectTransform>();
        submitRt.anchorMin = new Vector2(0.5f, 0f);
        submitRt.anchorMax = new Vector2(0.5f, 0f);
        submitRt.pivot = new Vector2(0.5f, 0.5f);
        submitRt.anchoredPosition = new Vector2(0f, 120f);
        submitRt.sizeDelta = new Vector2(220f, 55f);

        Image submitBg = submitObj.AddComponent<Image>();
        submitBg.color = new Color(0.2f, 0.6f, 0.9f, 1f);

        Button submitBtn = submitObj.AddComponent<Button>();
        submitBtn.targetGraphic = submitBg;

        ColorBlock submitColors = submitBtn.colors;
        submitColors.normalColor = new Color(0.2f, 0.6f, 0.9f, 1f);
        submitColors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        submitColors.pressedColor = new Color(0.15f, 0.4f, 0.7f, 1f);
        submitColors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        submitBtn.colors = submitColors;

        TextMeshProUGUI submitText = CreateTMPText(submitObj.transform, "Text (TMP)",
            stretch: true, fontSize: 24f, align: TextAlignmentOptions.Center,
            color: Color.white, defaultText: "Submit");

        // ── Feedback Text ──
        GameObject feedbackObj = CreateTMPTextObject(panel.transform, "FeedbackText",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 200f), new Vector2(750f, 80f),
            "", 20f, TextAlignmentOptions.Center, Color.white);

        // ── Wire up all references ──
        pwUI.titleText = titleObj.GetComponent<TextMeshProUGUI>();
        pwUI.descriptionText = descObj.GetComponent<TextMeshProUGUI>();
        pwUI.feedbackText = feedbackObj.GetComponent<TextMeshProUGUI>();
        pwUI.optionsContainer = containerRt;
        pwUI.optionButtonPrefab = optionButtonPrefab;
        pwUI.submitButton = submitBtn;

        // Panel starts DISABLED (code activates it when challenge opens)
        panel.SetActive(false);

        EditorUtility.SetDirty(pwUI);
        Debug.Log("[Setup] Created PasswordUIPanel with all child elements.");
    }

    // ═══════════════════════════════════════════════════════
    //  STEP 6: Password Station (test cube)
    // ═══════════════════════════════════════════════════════

    private static void CreatePasswordStation(ChallengeData data)
    {
        // Check if one already exists
        PasswordStation existing = Object.FindFirstObjectByType<PasswordStation>();
        if (existing != null)
        {
            Debug.Log("[Setup] PasswordStation already exists, skipping.");
            return;
        }

        GameObject station = GameObject.CreatePrimitive(PrimitiveType.Cube);
        station.name = "PasswordStation";
        station.transform.position = new Vector3(3f, 1f, 3f); // Adjust as needed
        station.transform.localScale = new Vector3(1f, 0.7f, 0.1f);

        // Give it a visible color
        Renderer rend = station.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.2f, 0.8f, 0.4f); // green so it's easy to spot
            rend.material = mat;
        }

        // Add the station script
        PasswordStation ps = station.AddComponent<PasswordStation>();
        ps.challengeData = data;

        EditorUtility.SetDirty(ps);
        Debug.Log("[Setup] Created PasswordStation cube at (3, 1, 3).");
    }

    // ═══════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static GameObject CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = color;

        return obj;
    }

    private static TextMeshProUGUI CreateTMPText(Transform parent, string name,
        bool stretch, float fontSize, TextAlignmentOptions align, Color color, string defaultText)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        if (stretch)
        {
            StretchFull(rt);
            rt.offsetMin = new Vector2(10f, 5f);
            rt.offsetMax = new Vector2(-10f, -5f);
        }

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.textWrappingMode = TextWrappingModes.Normal;

        return tmp;
    }

    private static GameObject CreateTMPTextObject(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size,
        string text, float fontSize, TextAlignmentOptions align, Color color, bool bold = false)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.textWrappingMode = TextWrappingModes.Normal;

        if (bold)
            tmp.fontStyle = FontStyles.Bold;

        return obj;
    }
}
