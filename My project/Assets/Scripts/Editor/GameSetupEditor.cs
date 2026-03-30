using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor utility that sets up the entire game scene with one click.
/// Menu: IT22361004 > Setup Game Scene
/// </summary>
public class GameSetupEditor
{
    const string ControlsHintText = "WASD/Arrows: Move  |  Mouse: Aim  |  Hold Left Click: Fire  |  ESC: Pause";

    [MenuItem("IT22361004/Setup Game Scene")]
    public static void SetupScene()
    {
        if (!EditorUtility.DisplayDialog(
            "IT22361004 - Game Setup",
            "This will set up the complete game scene.\n\n" +
            "- Camera (orthographic 2D)\n" +
            "- Player with health & shooting\n" +
            "- Obstacle spawner\n" +
            "- UI (health bar, score, game over)\n\n" +
            "Continue?", "Setup", "Cancel"))
            return;

        // Ensure required tags exist
        AddTag("Player");
        AddTag("Obstacle");
        AddTag("HealthPack");
        AddTag("Enemy");
        AddTag("Projectile");
        AddTag("ScoreTrigger");

        // Ensure input handling supports old Input Manager
        SetInputHandling();

        // Clean up any previous setup
        CleanScene();

        // Build the scene
        SetupCamera();
        CreateBackground();
        CreatePlayer();
        GameObject gmObj = CreateGameManager();
        CreateObstacleSpawner();
        Canvas canvas = CreateUI();
        WireReferences(gmObj, canvas);

        // Mark scene as modified
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("[IT22361004] Game scene setup complete! Press Play to start the game.");
        EditorUtility.DisplayDialog("Setup Complete",
            "Game scene is ready!\n\n" +
            "Controls:\n" +
            "- WASD or Arrow Keys: Move player\n" +
            "- Mouse: Aim\n" +
            "- Hold Left Click: Shoot\n\n" +
            "Press Play to start!", "OK");
    }

    [MenuItem("IT22361004/About")]
    public static void About()
    {
        EditorUtility.DisplayDialog("IT22361004",
            "SE4031 - Games Development\n" +
            "Assignment 02 - 2D Interactive Game\n\n" +
            "Student: IT22361004\n\n" +
            "Features:\n" +
            "- Health system with gradient bar\n" +
            "- Score system\n" +
            "- Health packs\n" +
            "- Projectile attacks\n" +
            "- Enemies\n" +
            "- Speed increase over time\n" +
            "- Creative: Screen shake + gradient health bar + dynamic wall colors",
            "OK");
    }

    static void SetInputHandling()
    {
        // Set to "Both" (old + new input system) so Input.GetAxisRaw works
        var playerSettings = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var inputProp = playerSettings.FindProperty("activeInputHandler");
        if (inputProp != null && inputProp.intValue != 2)
        {
            inputProp.intValue = 2; // 0=Old, 1=New, 2=Both
            playerSettings.ApplyModifiedProperties();
            Debug.Log("[IT22361004] Set input handling to 'Both' for old Input Manager compatibility.");
        }
    }

    static void CleanScene()
    {
        // Remove existing game objects from previous setup
        DestroyIfExists<GameManager>();
        DestroyIfExists<ObstacleSpawner>();
        DestroyByName("Player");
        DestroyByName("GameCanvas");
        DestroyByName("EventSystem");
        DestroyByName("SpaceBackground");
        DestroyByName("StarBackground");

        // Remove directional lights (not needed for 2D)
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var light in lights)
            Object.DestroyImmediate(light.gameObject);
    }

    static void DestroyIfExists<T>() where T : MonoBehaviour
    {
        var obj = Object.FindAnyObjectByType<T>();
        if (obj) Object.DestroyImmediate(obj.gameObject);
    }

    static void DestroyByName(string name)
    {
        var obj = GameObject.Find(name);
        if (obj) Object.DestroyImmediate(obj);
    }

    static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }

        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.06f, 0.1f);

        // Add screen shake component
        if (!cam.GetComponent<ScreenShake>())
            cam.gameObject.AddComponent<ScreenShake>();
    }

    static void CreateBackground()
    {
        Sprite bgSprite = KenneyAssets.Background;
        if (bgSprite != null)
        {
            GameObject bgImage = new GameObject("SpaceBackground");
            bgImage.transform.position = new Vector3(0, 0, 1f);
            SpriteRenderer bgSr = bgImage.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -20;

            Camera cam = Camera.main;
            if (cam != null)
            {
                float h = cam.orthographicSize * 2f;
                float w = h * cam.aspect;
                float sprW = bgSprite.bounds.size.x;
                float sprH = bgSprite.bounds.size.y;
                if (sprW > 0 && sprH > 0)
                    bgImage.transform.localScale = new Vector3(w / sprW + 1f, h / sprH + 1f, 1f);
                else
                    bgImage.transform.localScale = new Vector3(4f, 3f, 1f);
            }
        }

        GameObject bgObj = new GameObject("StarBackground");
        BackgroundStars stars = bgObj.AddComponent<BackgroundStars>();
        stars.Initialize(60);
    }

    static void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(-6f, 0, 0);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.55f, 0.35f);

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerShooting>();

        TrailRenderer trail = player.AddComponent<TrailRenderer>();
        trail.time = 0.3f;
        trail.startWidth = 0.25f;
        trail.endWidth = 0f;
        trail.startColor = new Color(0.5f, 0.8f, 1f, 0.6f);
        trail.endColor = new Color(0.3f, 0.6f, 1f, 0f);
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.sortingOrder = 4;
        trail.numCapVertices = 3;
        trail.minVertexDistance = 0.05f;

        GameObject shipVisual = new GameObject("ShipVisual");
        shipVisual.transform.SetParent(player.transform, false);
        shipVisual.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        shipVisual.transform.localEulerAngles = new Vector3(0f, 0f, -90f);

        SpriteRenderer shipRenderer = shipVisual.AddComponent<SpriteRenderer>();
        shipRenderer.sprite = KenneyAssets.Player;
        shipRenderer.color = shipRenderer.sprite == SpriteHelper.Square
            ? new Color(0.3f, 0.7f, 1f)
            : Color.white;
        shipRenderer.sortingOrder = 5;

        GameObject glow = new GameObject("EngineGlow");
        glow.transform.SetParent(player.transform, false);
        glow.transform.localPosition = new Vector3(-0.3f, 0f, 0f);
        glow.transform.localScale = Vector3.one * 0.8f;

        SpriteRenderer glowRenderer = glow.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = SpriteHelper.Circle;
        glowRenderer.color = new Color(0.4f, 0.8f, 1f, 0.25f);
        glowRenderer.sortingOrder = 4;
    }

    static GameObject CreateGameManager()
    {
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
        return gm;
    }

    static void CreateObstacleSpawner()
    {
        GameObject spawner = new GameObject("ObstacleSpawner");
        spawner.AddComponent<ObstacleSpawner>();
    }

    static Canvas CreateUI()
    {
        // === Canvas ===
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // === Health Bar ===
        // Background
        GameObject healthBG = CreateUIImage(canvasObj.transform, "HealthBarBG",
            new Vector2(260, 35), new Vector2(160, -35),
            new Color(0.15f, 0.15f, 0.2f, 0.9f));
        RectTransform healthBGRect = healthBG.GetComponent<RectTransform>();
        healthBGRect.anchorMin = new Vector2(0, 1);
        healthBGRect.anchorMax = new Vector2(0, 1);
        healthBGRect.pivot = new Vector2(0.5f, 0.5f);

        // Border
        Outline border = healthBG.AddComponent<Outline>();
        border.effectColor = new Color(0.4f, 0.4f, 0.5f, 1f);
        border.effectDistance = new Vector2(2, 2);

        // Fill bar
        GameObject healthFill = CreateUIImage(healthBG.transform, "HealthBarFill",
            new Vector2(245, 25), new Vector2(3, 0),
            new Color(0.2f, 0.9f, 0.2f));
        Image fillImage = healthFill.GetComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;

        // HealthBarUI component
        HealthBarUI healthBarUI = healthBG.AddComponent<HealthBarUI>();
        healthBarUI.fillImage = fillImage;

        // HP Label
        GameObject hpLabel = CreateTextElement(healthBG.transform, "HPLabel", "HP",
            new Vector2(-118, 0), 18, TextAlignmentOptions.MidlineLeft,
            new Vector2(40, 30));

        // === Score Text ===
        GameObject scoreObj = CreateTextElement(canvasObj.transform, "ScoreText", "Score: 0",
            new Vector2(-30, -25), 36, TextAlignmentOptions.TopRight,
            new Vector2(300, 50));
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(1, 1);
        scoreRect.anchorMax = new Vector2(1, 1);
        scoreRect.pivot = new Vector2(1, 1);

        // === Speed Text ===
        GameObject speedObj = CreateTextElement(canvasObj.transform, "SpeedText", "Speed: 4.0",
            new Vector2(-30, -70), 24, TextAlignmentOptions.TopRight,
            new Vector2(300, 40));
        RectTransform speedRect = speedObj.GetComponent<RectTransform>();
        speedRect.anchorMin = new Vector2(1, 1);
        speedRect.anchorMax = new Vector2(1, 1);
        speedRect.pivot = new Vector2(1, 1);

        // === Damage Flash Overlay ===
        GameObject flashObj = CreateUIImage(canvasObj.transform, "DamageFlash",
            Vector2.zero, Vector2.zero, Color.clear);
        RectTransform flashRect = flashObj.GetComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.sizeDelta = Vector2.zero;
        flashObj.GetComponent<Image>().raycastTarget = false;
        DamageFlash damageFlash = flashObj.AddComponent<DamageFlash>();
        damageFlash.flashImage = flashObj.GetComponent<Image>();

        // === Game Over Panel ===
        GameObject gameOverPanel = CreateUIImage(canvasObj.transform, "GameOverPanel",
            new Vector2(520, 380), Vector2.zero,
            new Color(0.05f, 0.05f, 0.1f, 0.92f));
        RectTransform goPanelRect = gameOverPanel.GetComponent<RectTransform>();
        goPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        goPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        goPanelRect.pivot = new Vector2(0.5f, 0.5f);

        // Panel border
        Outline panelBorder = gameOverPanel.AddComponent<Outline>();
        panelBorder.effectColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);
        panelBorder.effectDistance = new Vector2(3, 3);

        // Game Over title
        CreateTextElement(gameOverPanel.transform, "GameOverText", "GAME OVER",
            new Vector2(0, 90), 64, TextAlignmentOptions.Center,
            new Vector2(450, 90));

        // Final score
        CreateTextElement(gameOverPanel.transform, "FinalScoreText", "Final Score: 0",
            new Vector2(0, 15), 36, TextAlignmentOptions.Center,
            new Vector2(400, 50));

        // Restart Button
        GameObject btnObj = new GameObject("RestartButton");
        btnObj.transform.SetParent(gameOverPanel.transform, false);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(220, 55);
        btnRect.anchoredPosition = new Vector2(0, -75);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.55f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.55f, 1f);
        colors.highlightedColor = new Color(0.35f, 0.65f, 1f);
        colors.pressedColor = new Color(0.15f, 0.4f, 0.8f);
        colors.selectedColor = new Color(0.2f, 0.55f, 1f);
        btn.colors = colors;

        CreateTextElement(btnObj.transform, "BtnText", "RESTART",
            Vector2.zero, 30, TextAlignmentOptions.Center,
            new Vector2(220, 55));

        // Hide game over panel by default
        gameOverPanel.SetActive(false);

        // === Controls Hint ===
        GameObject controlsObj = CreateTextElement(canvasObj.transform, "ControlsHint",
            ControlsHintText,
            new Vector2(0, 25), 18, TextAlignmentOptions.Bottom,
            new Vector2(920, 35));
        RectTransform controlsRect = controlsObj.GetComponent<RectTransform>();
        controlsRect.anchorMin = new Vector2(0.5f, 0);
        controlsRect.anchorMax = new Vector2(0.5f, 0);
        controlsRect.pivot = new Vector2(0.5f, 0);
        controlsObj.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.4f);

        // === Event System ===
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvas;
    }

    static void WireReferences(GameObject gmObj, Canvas canvas)
    {
        GameManager gm = gmObj.GetComponent<GameManager>();
        if (gm == null) return;

        Transform ct = canvas.transform;
        gm.scoreText = FindDeep(ct, "ScoreText")?.GetComponent<TextMeshProUGUI>();
        gm.speedText = FindDeep(ct, "SpeedText")?.GetComponent<TextMeshProUGUI>();
        gm.gameOverPanel = FindDeep(ct, "GameOverPanel")?.gameObject;
        gm.gameOverScoreText = FindDeep(ct, "FinalScoreText")?.GetComponent<TextMeshProUGUI>();
        gm.restartButton = FindDeep(ct, "RestartButton")?.GetComponent<Button>();

        EditorUtility.SetDirty(gm);
    }

    // === Helper Methods ===

    static GameObject CreateUIImage(Transform parent, string name,
        Vector2 size, Vector2 position, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        Image img = obj.AddComponent<Image>();
        img.color = color;
        return obj;
    }

    static GameObject CreateTextElement(Transform parent, string name, string text,
        Vector2 position, float fontSize, TextAlignmentOptions alignment, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.enableAutoSizing = false;

        return obj;
    }

    static Transform FindDeep(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform found = FindDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }

    static void AddTag(string tag)
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
