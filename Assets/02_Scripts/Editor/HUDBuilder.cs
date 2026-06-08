#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class HUDBuilder
{
    // === Asset paths ===
    private const string CardSmall    = "Assets/04_Sprites/UI/cards/Card_small.png";
    private const string CardMedium   = "Assets/04_Sprites/UI/cards/Card_medium.png";
    private const string CardWide     = "Assets/04_Sprites/UI/cards/Card_wide.png";
    private const string FrameSprite  = "Assets/04_Sprites/UI/Buttons/Pixelated Frame 204.png";
    private const string HoneyIcon    = "Assets/04_Sprites/UI/Icons/currentsy/honey_icon.png";
    private const string SpringCard   = "Assets/04_Sprites/UI/cards/SpringUI.png";
    private const string FontRegular  = "Assets/Fonts/PixelifySans-Regular SDF.asset";
    private const string FontBold     = "Assets/Fonts/PixelifySans-Bold SDF.asset";
    private const string FontFallback = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    private const string IconQueen    = "Assets/04_Sprites/UI/cards/QueenUI1.png";
    private const string IconNurse    = "Assets/04_Sprites/UI/Icons/Nurse_bee_icon.png";
    private const string IconBuilder  = "Assets/04_Sprites/UI/Icons/Builder_bee_icon.png";
    private const string IconHouse    = "Assets/04_Sprites/UI/Icons/Worker_bee_icon.png";
    private const string IconForager  = "Assets/04_Sprites/UI/Icons/Forager_bee_icon.png";
    private const string IconDrone    = "Assets/04_Sprites/UI/cards/Drone.png";

    // === Colors ===
    private static readonly Color DarkBrown   = new Color(0.26f, 0.16f, 0.12f);
    private static readonly Color HiveOrange  = new Color(1f, 0.71f, 0.24f);
    private static readonly Color White       = Color.white;

    // === Loaded once per build ===
    private static TMP_FontAsset fontReg;
    private static TMP_FontAsset fontBold;
    private static Sprite spCardSmall, spCardMedium, spCardWide, spFrame, spHoney, spSpringCard;

    [MenuItem("Tools/Plan Bee/Build HUD")]
    public static void Build()
    {
        var canvasGO = GameObject.Find("MainCanvass");
        if (canvasGO == null)
        {
            EditorUtility.DisplayDialog("HUD Builder",
                "MainCanvass not found in scene. Open K-MainScene first.", "OK");
            return;
        }

        // Load fonts + sprites once
        fontReg  = LoadFontAsset(FontRegular);
        fontBold = LoadFontAsset(FontBold) ?? fontReg;
        spCardSmall   = AssetDatabase.LoadAssetAtPath<Sprite>(CardSmall);
        spCardMedium  = AssetDatabase.LoadAssetAtPath<Sprite>(CardMedium);
        spCardWide    = AssetDatabase.LoadAssetAtPath<Sprite>(CardWide);
        spFrame       = AssetDatabase.LoadAssetAtPath<Sprite>(FrameSprite);
        spHoney       = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyIcon);
        spSpringCard  = AssetDatabase.LoadAssetAtPath<Sprite>(SpringCard);

        // Replace existing
        var existing = GameObject.Find("HUD");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("HUD Builder",
                "HUD already exists. Replace it?", "Replace", "Cancel")) return;
            Object.DestroyImmediate(existing);
        }

        // HUD root
        var hud = MakeChild(canvasGO, "HUD");
        var hudRt = hud.AddComponent<RectTransform>();
        Stretch(hudRt);

        // Top bar (full-width strip at top, 80px tall)
        var topBar = MakeChild(hud, "TopBar");
        var topRt = topBar.AddComponent<RectTransform>();
        topRt.anchorMin = new Vector2(0, 1);
        topRt.anchorMax = new Vector2(1, 1);
        topRt.pivot     = new Vector2(0.5f, 1);
        topRt.sizeDelta = new Vector2(0, 80);
        topRt.anchoredPosition = new Vector2(0, 0);

        BuildResourcesPanel(topBar);
        BuildSeasonPanel(topBar);
        BuildControlsPanel(topBar);

        // Bottom bar
        var bottomBar = MakeChild(hud, "BottomBar");
        var botRt = bottomBar.AddComponent<RectTransform>();
        botRt.anchorMin = new Vector2(0, 0);
        botRt.anchorMax = new Vector2(1, 0);
        botRt.pivot     = new Vector2(0.5f, 0);
        botRt.sizeDelta = new Vector2(0, 160);
        botRt.anchoredPosition = new Vector2(0, 0);

        var hud_controller = canvasGO.GetComponent<HUDController>();
        if (hud_controller == null) hud_controller = canvasGO.AddComponent<HUDController>();

        BuildColonyPanel(bottomBar, hud_controller);
        BuildEggQueuePlaceholder(bottomBar);
        BuildBuildPlaceholder(bottomBar);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = hud;
        EditorUtility.DisplayDialog("HUD Builder",
            "HUD built. Save the scene (Cmd+S), then:\n" +
            "1. Press Play to verify\n" +
            "2. Manually reparent your existing EggQueue and BuildPanel under HUD/BottomBar/EggQueueContainer and BuildContainer\n" +
            "3. Disable the old scattered HUD elements", "Got it");
    }

    // === Top: Resources panel (honey only) ===
    private static void BuildResourcesPanel(GameObject parent)
    {
        var panel = MakeChild(parent, "ResourcesPanel");
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot     = new Vector2(0, 0.5f);
        rt.sizeDelta = new Vector2(220, 60);
        rt.anchoredPosition = new Vector2(20, 0);
        AddPanelImage(panel, spCardSmall);

        var icon = MakeImageChild(panel, "HoneyIcon", spHoney, White);
        var iconRt = icon.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0, 0.5f);
        iconRt.anchorMax = new Vector2(0, 0.5f);
        iconRt.pivot     = new Vector2(0, 0.5f);
        iconRt.sizeDelta = new Vector2(40, 40);
        iconRt.anchoredPosition = new Vector2(20, 0);

        var text = MakeText(panel, "HoneyText", "250", fontBold, 36, DarkBrown, TextAlignmentOptions.Left);
        var txtRt = text.GetComponent<RectTransform>();
        txtRt.anchorMin = new Vector2(0, 0);
        txtRt.anchorMax = new Vector2(1, 1);
        txtRt.offsetMin = new Vector2(75, 0);
        txtRt.offsetMax = new Vector2(-10, 0);

        // Wire to CurrencyManager
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.honeyText = text;
        else
        {
            var cm = Object.FindObjectOfType<CurrencyManager>();
            if (cm != null) cm.honeyText = text;
        }
    }

    // === Top: Season panel ===
    private static void BuildSeasonPanel(GameObject parent)
    {
        var panel = MakeChild(parent, "SeasonPanel");
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(360, 70);
        rt.anchoredPosition = new Vector2(0, 0);
        AddPanelImage(panel, spCardWide ?? spCardMedium);

        var season = MakeText(panel, "SeasonText", "SPRING", fontBold, 26, HiveOrange, TextAlignmentOptions.Center);
        var seasonRt = season.GetComponent<RectTransform>();
        seasonRt.anchorMin = new Vector2(0, 0.55f);
        seasonRt.anchorMax = new Vector2(1, 1f);
        seasonRt.offsetMin = Vector2.zero;
        seasonRt.offsetMax = Vector2.zero;

        var day = MakeText(panel, "DayText", "Day 1", fontBold, 22, DarkBrown, TextAlignmentOptions.Center);
        var dayRt = day.GetComponent<RectTransform>();
        dayRt.anchorMin = new Vector2(0, 0.1f);
        dayRt.anchorMax = new Vector2(0.5f, 0.55f);
        dayRt.offsetMin = Vector2.zero;
        dayRt.offsetMax = Vector2.zero;

        var time = MakeText(panel, "TimeText", "9 AM", fontReg, 22, DarkBrown, TextAlignmentOptions.Center);
        var timeRt = time.GetComponent<RectTransform>();
        timeRt.anchorMin = new Vector2(0.5f, 0.1f);
        timeRt.anchorMax = new Vector2(1, 0.55f);
        timeRt.offsetMin = Vector2.zero;
        timeRt.offsetMax = Vector2.zero;

        // Wire to managers
        var sm = Object.FindObjectOfType<SeasonManager>();
        if (sm != null) sm.seasonText = season;
        var dcm = Object.FindObjectOfType<DayCycleManager>();
        if (dcm != null) { dcm.dayText = day; dcm.timeText = time; }
    }

    // === Top: Controls panel ===
    private static void BuildControlsPanel(GameObject parent)
    {
        var panel = MakeChild(parent, "ControlsPanel");
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot     = new Vector2(1, 0.5f);
        rt.sizeDelta = new Vector2(220, 60);
        rt.anchoredPosition = new Vector2(-20, 0);
        AddPanelImage(panel, spCardSmall);

        var pauseBtn = MakeIconButton(panel, "PauseButton", "II", 0);
        var speedBtn = MakeIconButton(panel, "SpeedButton", "1x", 70);
        var menuBtn  = MakeIconButton(panel, "MenuButton", "...", 140);

        // Wire Pause → PauseMenuController.Pause()
        var pauseController = Object.FindObjectOfType<PauseMenuController>();
        if (pauseController != null)
        {
            var pauseAction = (UnityAction)System.Delegate.CreateDelegate(
                typeof(UnityAction), pauseController,
                typeof(PauseMenuController).GetMethod("Pause"));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                pauseBtn.GetComponentInChildren<Button>().onClick, pauseAction);
        }

        // Wire Speed → HUDController.CycleSpeed()
        var canvasGO = parent.transform.parent.parent.gameObject;
        var hudController = canvasGO.GetComponent<HUDController>();
        if (hudController == null) hudController = canvasGO.AddComponent<HUDController>();
        hudController.speedLabel = speedBtn.GetComponentInChildren<TMP_Text>();
        var speedAction = (UnityAction)System.Delegate.CreateDelegate(
            typeof(UnityAction), hudController,
            typeof(HUDController).GetMethod("CycleSpeed"));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            speedBtn.GetComponent<Button>().onClick, speedAction);
    }

    // === Bottom: Colony panel ===
    private static void BuildColonyPanel(GameObject parent, HUDController hudController)
    {
        var panel = MakeChild(parent, "ColonyPanel");
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot     = new Vector2(0, 0.5f);
        rt.sizeDelta = new Vector2(220, 150);
        rt.anchoredPosition = new Vector2(20, 0);
        AddPanelImage(panel, spCardMedium);

        var title = MakeText(panel, "Title", "COLONY", fontBold, 20, DarkBrown, TextAlignmentOptions.Center);
        var titleRt = title.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot     = new Vector2(0.5f, 1);
        titleRt.sizeDelta = new Vector2(0, 26);
        titleRt.anchoredPosition = new Vector2(0, -8);

        // Stat rows
        string[] labels = { "Queen", "Nurse", "Builder", "House", "Forager", "Drone" };
        TMP_Text[] outRefs = new TMP_Text[6];
        for (int i = 0; i < labels.Length; i++)
        {
            float y = -40 - i * 16;
            var row = MakeText(panel, labels[i] + "Count",
                $"{labels[i],-8}  0", fontReg, 14, DarkBrown, TextAlignmentOptions.Left);
            var rrt = row.GetComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0, 1);
            rrt.anchorMax = new Vector2(1, 1);
            rrt.pivot     = new Vector2(0.5f, 1);
            rrt.sizeDelta = new Vector2(-20, 16);
            rrt.anchoredPosition = new Vector2(0, y);
            outRefs[i] = row;
        }
        hudController.queenCountText   = outRefs[0];
        hudController.nurseCountText   = outRefs[1];
        hudController.builderCountText = outRefs[2];
        hudController.houseCountText   = outRefs[3];
        hudController.foragerCountText = outRefs[4];
        hudController.droneCountText   = outRefs[5];

        var total = MakeText(panel, "TotalCount", "TOTAL: 0", fontBold, 14, HiveOrange, TextAlignmentOptions.Center);
        var totalRt = total.GetComponent<RectTransform>();
        totalRt.anchorMin = new Vector2(0, 0);
        totalRt.anchorMax = new Vector2(1, 0);
        totalRt.pivot     = new Vector2(0.5f, 0);
        totalRt.sizeDelta = new Vector2(0, 22);
        totalRt.anchoredPosition = new Vector2(0, 8);
        hudController.totalCountText = total;
    }

    private static void BuildEggQueuePlaceholder(GameObject parent)
    {
        var panel = MakeChild(parent, "EggQueueContainer");
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(480, 150);
        rt.anchoredPosition = new Vector2(0, 0);
        AddPanelImage(panel, spCardWide ?? spCardMedium);

        var label = MakeText(panel, "Placeholder", "EGG QUEUE\n(reparent existing egg UI here)",
            fontReg, 16, DarkBrown, TextAlignmentOptions.Center);
        var lrt = label.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
    }

    private static void BuildBuildPlaceholder(GameObject parent)
    {
        var panel = MakeChild(parent, "BuildContainer");
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot     = new Vector2(1, 0.5f);
        rt.sizeDelta = new Vector2(220, 150);
        rt.anchoredPosition = new Vector2(-20, 0);
        AddPanelImage(panel, spCardMedium);

        var label = MakeText(panel, "Placeholder", "BUILD\n(reparent BuildPanel here)",
            fontReg, 16, DarkBrown, TextAlignmentOptions.Center);
        var lrt = label.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static GameObject MakeChild(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Image AddPanelImage(GameObject go, Sprite sprite)
    {
        var img = go.AddComponent<Image>();
        if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; }
        else img.color = new Color(0.95f, 0.85f, 0.55f);
        return img;
    }

    private static GameObject MakeImageChild(GameObject parent, string name, Sprite sprite, Color tint)
    {
        var go = MakeChild(parent, name);
        var img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        img.color = tint;
        img.preserveAspect = true;
        return go;
    }

    private static TextMeshProUGUI MakeText(GameObject parent, string name, string content,
                                            TMP_FontAsset font, int size, Color color,
                                            TextAlignmentOptions align)
    {
        var go = MakeChild(parent, name);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = content;
        t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        return t;
    }

    private static GameObject MakeIconButton(GameObject parent, string name, string label, float xOffset)
    {
        var go = MakeChild(parent, name);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot     = new Vector2(0, 0.5f);
        rt.sizeDelta = new Vector2(60, 50);
        rt.anchoredPosition = new Vector2(20 + xOffset, 0);
        var img = go.AddComponent<Image>();
        if (spFrame != null) { img.sprite = spFrame; img.type = Image.Type.Sliced; }
        else img.color = new Color(0.85f, 0.7f, 0.4f);
        var btn = go.AddComponent<Button>();
        var labelText = MakeText(go, "Label", label, fontBold, 22, DarkBrown, TextAlignmentOptions.Center);
        var labelRt = labelText.GetComponent<RectTransform>();
        Stretch(labelRt);
        return go;
    }

    private static TMP_FontAsset LoadFontAsset(string path)
    {
        var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (f == null) f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontFallback);
        return f;
    }
}
#endif
