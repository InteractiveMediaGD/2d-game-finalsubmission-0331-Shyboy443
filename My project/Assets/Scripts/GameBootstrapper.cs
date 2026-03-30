using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Automatically sets up the entire game scene at runtime.
/// Uses [RuntimeInitializeOnLoadMethod] so it runs when you press Play
/// even on an empty scene - no manual setup required.
/// </summary>
public static class GameBootstrapper
{
    const string ControlsHintText = "WASD/Arrows: Move  |  Mouse: Aim  |  Hold Left Click: Fire  |  1-5: Switch Weapons  |  ESC: Pause";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSetup()
    {
        ConfigureDisplay();
        SetupCamera();
        EnsureBackground();

        if (Object.FindAnyObjectByType<GameManager>() != null)
        {
            RefreshExistingScene();
            Debug.Log("[IT22361004] Existing scene found. Reusing gameplay objects and filling in missing visuals.");
            return;
        }

        Debug.Log("[IT22361004] Auto-setting up game scene...");

        CreatePlayer();
        CreateGameManager();
        CreateObstacleSpawner();
        CreateUI();

        Debug.Log("[IT22361004] Game ready! WASD/Arrows to move, Mouse to aim, Hold Left Click to fire.");
    }

    static void ConfigureDisplay()
    {
#if !UNITY_EDITOR
        Resolution desktop = Screen.currentResolution;
        FullScreenMode targetMode = FullScreenMode.FullScreenWindow;

        if (desktop.width > 0 && desktop.height > 0)
            Screen.SetResolution(desktop.width, desktop.height, targetMode, desktop.refreshRateRatio);
        else
            Screen.fullScreenMode = targetMode;

        Screen.fullScreen = true;
#endif
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
        cam.backgroundColor = new Color(0.04f, 0.04f, 0.08f);

        if (!cam.GetComponent<ScreenShake>())
            cam.gameObject.AddComponent<ScreenShake>();

        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
            Object.Destroy(l.gameObject);
    }

    static void EnsureBackground()
    {
        if (GameObject.Find("SpaceBackground") == null)
        {
            // Kenney space background (darkPurple.png) stretched to fill camera
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

                if (bgImage.GetComponent<BackgroundThemeController>() == null)
                    bgImage.AddComponent<BackgroundThemeController>();
            }
        }

        if (GameObject.Find("StarBackground") == null)
        {
            // Sparse star parallax layer on top of background
            GameObject bgObj = new GameObject("StarBackground");
            BackgroundStars stars = bgObj.AddComponent<BackgroundStars>();
            stars.Initialize(60);
        }
    }

    static void RefreshExistingScene()
    {
        GameObject controls = GameObject.Find("ControlsHint");
        if (controls == null) return;

        Transform controlsParent = controls.transform.parent;
        if (controlsParent != null && controlsParent.name != "ControlsHintPanel")
        {
            Transform existingPanel = controlsParent.Find("ControlsHintPanel");
            GameObject panelObj = existingPanel != null ? existingPanel.gameObject : new GameObject("ControlsHintPanel");
            panelObj.transform.SetParent(controlsParent, false);

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            if (panelRect == null)
                panelRect = panelObj.AddComponent<RectTransform>();

            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.sizeDelta = new Vector2(980f, 40f);
            panelRect.anchoredPosition = new Vector2(0f, 14f);

            Image panelImage = panelObj.GetComponent<Image>();
            if (panelImage == null)
                panelImage = panelObj.AddComponent<Image>();

            GameUiStyle.ApplyPanelStyle(panelImage, panelRect.sizeDelta, new Color(0.05f, 0.08f, 0.14f, 0.76f), new Color(0.46f, 0.7f, 1f, 0.18f), new Vector2(0f, -3f), false, 0f);
            controls.transform.SetParent(panelObj.transform, false);
        }

        RectTransform rect = controls.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(920f, 24f);
            rect.anchoredPosition = Vector2.zero;
        }

        Text legacyText = controls.GetComponent<Text>();
        if (legacyText != null)
        {
            legacyText.text = ControlsHintText;
            GameUiStyle.StyleText(legacyText, 15, new Color(1f, 1f, 1f, 0.84f), TextAnchor.MiddleCenter, FontStyle.Normal, false, false);
        }

        TMP_Text tmpText = controls.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = ControlsHintText;
            tmpText.fontSize = 15;
            tmpText.color = new Color(1f, 1f, 1f, 0.84f);
        }
    }

    static void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(-6f, 0, 0);
        // No rotation on parent — movement uses world Y axis

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.55f, 0.35f);   // fits ship body

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerPowerUps>();
        player.AddComponent<PlayerShooting>();

        // Trail from player origin
        TrailRenderer trail = player.AddComponent<TrailRenderer>();
        trail.time = 0.3f;
        trail.startWidth = 0.25f;
        trail.endWidth = 0f;
        trail.startColor = new Color(0.5f, 0.8f, 1f, 0.6f);
        trail.endColor   = new Color(0.3f, 0.6f, 1f, 0f);
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.sortingOrder = 4;
        trail.numCapVertices = 3;
        trail.minVertexDistance = 0.05f;

        // Ship visual child — rotated so sprite faces right
        GameObject shipVisual = new GameObject("ShipVisual");
        shipVisual.transform.SetParent(player.transform, false);
        shipVisual.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        shipVisual.transform.localEulerAngles = new Vector3(0f, 0f, -90f); // face right

        SpriteRenderer sr = shipVisual.AddComponent<SpriteRenderer>();
        sr.sprite = KenneyAssets.Player;
        sr.color = Color.white;
        sr.sortingOrder = 5;

        // Engine glow behind the ship
        GameObject glow = new GameObject("EngineGlow");
        glow.transform.SetParent(player.transform, false);
        glow.transform.localPosition = new Vector3(-0.3f, 0, 0);
        glow.transform.localScale = Vector3.one * 0.8f;
        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = SpriteHelper.Circle;
        glowSr.color = new Color(0.4f, 0.8f, 1f, 0.25f);
        glowSr.sortingOrder = 4;
    }

    static void CreateGameManager()
    {
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
    }

    static void CreateObstacleSpawner()
    {
        GameObject spawner = new GameObject("ObstacleSpawner");
        spawner.AddComponent<ObstacleSpawner>();
    }

    static void CreateUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // --- Health Bar ---
        GameObject healthBG = CreateImage(canvasObj.transform, "HealthBarBG",
            new Vector2(300, 40), new Color(0.1f, 0.1f, 0.15f, 0.95f));
        RectTransform hbgRect = healthBG.GetComponent<RectTransform>();
        hbgRect.anchorMin = new Vector2(0, 1);
        hbgRect.anchorMax = new Vector2(0, 1);
        hbgRect.pivot = new Vector2(0, 1);
        hbgRect.anchoredPosition = new Vector2(20, -15);

        // Border effect
        Outline border = healthBG.AddComponent<Outline>();
        border.effectColor = new Color(0.4f, 0.4f, 0.5f, 0.7f);
        border.effectDistance = new Vector2(2, 2);

        // HP icon/label
        GameObject hpLabel = CreateText(healthBG.transform, "HPLabel", "HP", 20,
            TextAnchor.MiddleLeft, new Vector2(40, 35));
        RectTransform hpRect = hpLabel.GetComponent<RectTransform>();
        hpRect.anchorMin = new Vector2(0, 0.5f);
        hpRect.anchorMax = new Vector2(0, 0.5f);
        hpRect.pivot = new Vector2(0, 0.5f);
        hpRect.anchoredPosition = new Vector2(8, 0);
        hpLabel.GetComponent<Text>().fontStyle = FontStyle.Bold;

        // Fill bar - uses sizeDelta width to shrink (pivot on left so it shrinks from right)
        GameObject healthFill = CreateImage(healthBG.transform, "HealthBarFill",
            new Vector2(210, 28), new Color(0.2f, 0.9f, 0.2f));
        RectTransform fillRect = healthFill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0.5f);
        fillRect.anchorMax = new Vector2(0, 0.5f);
        fillRect.pivot = new Vector2(0, 0.5f); // LEFT pivot so bar shrinks from right
        fillRect.anchoredPosition = new Vector2(48, 0);

        Image fillImage = healthFill.GetComponent<Image>();

        // Health number text (e.g., "5 / 5")
        GameObject healthNum = CreateText(healthBG.transform, "HealthNumber", "5 / 5", 16,
            TextAnchor.MiddleRight, new Vector2(60, 28));
        RectTransform numRect = healthNum.GetComponent<RectTransform>();
        numRect.anchorMin = new Vector2(1, 0.5f);
        numRect.anchorMax = new Vector2(1, 0.5f);
        numRect.pivot = new Vector2(1, 0.5f);
        numRect.anchoredPosition = new Vector2(-8, 0);

        // Wire HealthBarUI component
        HealthBarUI healthBarUI = healthBG.AddComponent<HealthBarUI>();
        healthBarUI.fillImage = fillImage;
        healthBarUI.healthNumberText = healthNum.GetComponent<Text>();

        // --- Score Text ---
        GameObject scoreObj = CreateText(canvasObj.transform, "ScoreText", "Score: 0", 38,
            TextAnchor.UpperRight, new Vector2(300, 55));
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(1, 1);
        scoreRect.anchorMax = new Vector2(1, 1);
        scoreRect.pivot = new Vector2(1, 1);
        scoreRect.anchoredPosition = new Vector2(-20, -15);
        scoreObj.GetComponent<Text>().fontStyle = FontStyle.Bold;

        // Score shadow for readability
        Outline scoreShadow = scoreObj.AddComponent<Outline>();
        scoreShadow.effectColor = new Color(0, 0, 0, 0.6f);
        scoreShadow.effectDistance = new Vector2(2, -2);

        // --- Speed Text ---
        GameObject speedObj = CreateText(canvasObj.transform, "SpeedText", "Speed: 4.0", 22,
            TextAnchor.UpperRight, new Vector2(300, 40));
        RectTransform speedRect = speedObj.GetComponent<RectTransform>();
        speedRect.anchorMin = new Vector2(1, 1);
        speedRect.anchorMax = new Vector2(1, 1);
        speedRect.pivot = new Vector2(1, 1);
        speedRect.anchoredPosition = new Vector2(-20, -65);
        speedObj.GetComponent<Text>().color = new Color(0.7f, 0.8f, 1f);

        // --- Damage Flash ---
        GameObject flashObj = CreateImage(canvasObj.transform, "DamageFlash",
            Vector2.zero, Color.clear);
        RectTransform flashRect = flashObj.GetComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.sizeDelta = Vector2.zero;
        flashObj.GetComponent<Image>().raycastTarget = false;
        DamageFlash df = flashObj.AddComponent<DamageFlash>();
        df.flashImage = flashObj.GetComponent<Image>();

        // --- Game Over Panel ---
        GameObject goPanel = CreateImage(canvasObj.transform, "GameOverPanel",
            new Vector2(550, 400), new Color(0.03f, 0.03f, 0.08f, 0.94f));
        RectTransform goPanelRect = goPanel.GetComponent<RectTransform>();
        goPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        goPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        goPanelRect.pivot = new Vector2(0.5f, 0.5f);
        goPanelRect.anchoredPosition = Vector2.zero;

        Outline panelBorder = goPanel.AddComponent<Outline>();
        panelBorder.effectColor = new Color(0.8f, 0.2f, 0.2f, 0.7f);
        panelBorder.effectDistance = new Vector2(3, 3);

        // Game Over title
        GameObject goTitle = CreateText(goPanel.transform, "GameOverText", "GAME OVER", 64,
            TextAnchor.MiddleCenter, new Vector2(480, 90));
        goTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        goTitle.GetComponent<Text>().color = new Color(1f, 0.25f, 0.25f);
        goTitle.GetComponent<Text>().fontStyle = FontStyle.Bold;
        goTitle.AddComponent<Outline>().effectColor = new Color(0.5f, 0, 0, 0.5f);

        // Final score
        GameObject finalScore = CreateText(goPanel.transform, "FinalScoreText", "Final Score: 0", 38,
            TextAnchor.MiddleCenter, new Vector2(400, 55));
        finalScore.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20);

        // Restart button
        GameObject btnObj = new GameObject("RestartButton");
        btnObj.transform.SetParent(goPanel.transform, false);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(240, 60);
        btnRect.anchoredPosition = new Vector2(0, -80);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.sprite = SpriteHelper.Square;
        btnImage.color = new Color(0.15f, 0.5f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.5f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.6f, 1f);
        colors.pressedColor = new Color(0.1f, 0.35f, 0.8f);
        colors.selectedColor = new Color(0.15f, 0.5f, 1f);
        btn.colors = colors;

        Outline btnBorder = btnObj.AddComponent<Outline>();
        btnBorder.effectColor = new Color(0.4f, 0.7f, 1f, 0.5f);

        GameObject btnText = CreateText(btnObj.transform, "BtnText", "RESTART", 32,
            TextAnchor.MiddleCenter, new Vector2(240, 60));
        btnText.GetComponent<Text>().fontStyle = FontStyle.Bold;

        goPanel.SetActive(false);

        // --- Controls hint ---
        GameObject controlsPanel = CreateImage(canvasObj.transform, "ControlsHintPanel",
            new Vector2(980, 40), new Color(0.05f, 0.08f, 0.14f, 0.76f));
        RectTransform controlsPanelRect = controlsPanel.GetComponent<RectTransform>();
        controlsPanelRect.anchorMin = new Vector2(0.5f, 0);
        controlsPanelRect.anchorMax = new Vector2(0.5f, 0);
        controlsPanelRect.pivot = new Vector2(0.5f, 0);
        controlsPanelRect.anchoredPosition = new Vector2(0, 14);
        GameUiStyle.ApplyPanelStyle(controlsPanel.GetComponent<Image>(), controlsPanelRect.sizeDelta,
            new Color(0.05f, 0.08f, 0.14f, 0.76f), new Color(0.46f, 0.7f, 1f, 0.18f), new Vector2(0f, -3f), false, 0f);

        GameObject controls = CreateText(controlsPanel.transform, "ControlsHint",
            ControlsHintText, 18,
            TextAnchor.MiddleCenter, new Vector2(920, 24));
        RectTransform ctrlRect = controls.GetComponent<RectTransform>();
        ctrlRect.anchorMin = new Vector2(0.5f, 0.5f);
        ctrlRect.anchorMax = new Vector2(0.5f, 0.5f);
        ctrlRect.pivot = new Vector2(0.5f, 0.5f);
        ctrlRect.anchoredPosition = Vector2.zero;
        GameUiStyle.StyleText(controls.GetComponent<Text>(), 15, new Color(1f, 1f, 1f, 0.84f), TextAnchor.MiddleCenter, FontStyle.Normal, false, false);

        // --- Event System ---
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // --- Wire GameManager references ---
        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.scoreText = null;
            gm.speedText = null;
            gm.gameOverPanel = goPanel;
            gm.gameOverScoreText = null;
            gm.restartButton = btn;
            canvasObj.AddComponent<UITextBridge>();
        }
    }

    static GameObject CreateImage(Transform parent, string name, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        Image img = obj.AddComponent<Image>();
        img.color = color;
        return obj;
    }

    static GameObject CreateText(Transform parent, string name, string text, int fontSize,
        TextAnchor alignment, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        Text t = obj.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.color = Color.white;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        t.font = font;

        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return obj;
    }
}
