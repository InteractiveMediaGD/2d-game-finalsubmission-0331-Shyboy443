using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds and controls the start, pause, and options menu overlays on the main canvas.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class GameMenuUI : MonoBehaviour
{
    const string StartPanelName = "StartMenuPanel";
    const string PausePanelName = "PauseMenuPanel";
    const string OptionsPanelName = "OptionsMenuPanel";

    static Font cachedDisplayFont;
    static Font cachedBodyFont;

    GameObject startMenuPanel;
    GameObject pauseMenuPanel;
    GameObject optionsPanel;
    Button startRunButton;
    Button resumeButton;
    Button restartFromPauseButton;
    Button openOptionsFromStartButton;
    Button openOptionsFromPauseButton;
    Button closeOptionsButton;
    Button[] difficultyButtons;
    Button[] modeButtons;
    Text selectedModeText;
    Text modeDescriptionText;
    Text bestScoreText;
    Text pauseSummaryText;
    Slider musicSlider;
    Slider sfxSlider;
    GameObject[] hudObjects;
    GameManager gameManager;
    Image startBackdropImage;
    Image pauseBackdropImage;
    Image optionsBackdropImage;
    Image startHeroShipImage;
    Image startEnemyShipImage;
    GameManager.GameDifficulty selectedDifficulty = GameManager.GameDifficulty.Medium;
    GameManager.GameMode selectedMode = GameManager.GameMode.Campaign;

    void Awake()
    {
        EnsureBuilt();
    }

    public void Bind(GameManager manager)
    {
        if (manager == null)
            return;

        gameManager = manager;
        selectedDifficulty = manager.SelectedDifficulty;
        selectedMode = manager.SelectedMode;
        EnsureBuilt();
        WireButtons();
        RefreshLabels();
    }

    public void ShowStartMenu()
    {
        EnsureBuilt();
        SetHudVisible(false);
        RefreshLabels();
        startMenuPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    public void ShowPauseMenu()
    {
        EnsureBuilt();
        SetHudVisible(false);
        RefreshLabels();
        startMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void HideMenus(bool keepHudVisible = true)
    {
        EnsureBuilt();
        startMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        SetHudVisible(keepHudVisible);
    }

    void EnsureBuilt()
    {
        CacheHudObjects();
        if (startMenuPanel == null)
            startMenuPanel = BuildStartMenu();
        if (pauseMenuPanel == null)
            pauseMenuPanel = BuildPauseMenu();
        if (optionsPanel == null)
            optionsPanel = BuildOptionsMenu();

        WireButtons();
        RefreshLabels();
    }

    void CacheHudObjects()
    {
        hudObjects = new[]
        {
            FindDeep(transform, "HealthBarBG")?.gameObject,
            FindDeep(transform, "PowerUpStatusPanel")?.gameObject,
            FindDeep(transform, "ScoreText")?.gameObject,
            FindDeep(transform, "SpeedText")?.gameObject,
            FindDeep(transform, "ControlsHint")?.gameObject,
            FindDeep(transform, "MetaModeText")?.gameObject,
            FindDeep(transform, "MetaTimerText")?.gameObject,
            FindDeep(transform, "MetaWeaponText")?.gameObject,
            FindDeep(transform, "MetaScrapText")?.gameObject,
            FindDeep(transform, "MetaObjectiveText")?.gameObject,
        };
    }

    void WireButtons()
    {
        if (startRunButton != null)
        {
            startRunButton.onClick.RemoveAllListeners();
            startRunButton.onClick.AddListener(() => gameManager?.StartGame(selectedMode, selectedDifficulty));
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(() => gameManager?.ResumeGame());
        }

        if (restartFromPauseButton != null)
        {
            restartFromPauseButton.onClick.RemoveAllListeners();
            restartFromPauseButton.onClick.AddListener(() => gameManager?.RestartGame());
        }

        if (openOptionsFromStartButton != null)
        {
            openOptionsFromStartButton.onClick.RemoveAllListeners();
            openOptionsFromStartButton.onClick.AddListener(() => ShowOptions(startMenuPanel));
        }

        if (openOptionsFromPauseButton != null)
        {
            openOptionsFromPauseButton.onClick.RemoveAllListeners();
            openOptionsFromPauseButton.onClick.AddListener(() => ShowOptions(pauseMenuPanel));
        }

        if (closeOptionsButton != null)
        {
            closeOptionsButton.onClick.RemoveAllListeners();
            closeOptionsButton.onClick.AddListener(() =>
            {
                GameAudio.Instance?.PlayUiConfirm();
                optionsPanel.SetActive(false);
                if (gameManager != null && gameManager.IsPaused)
                    pauseMenuPanel.SetActive(true);
                else
                    startMenuPanel.SetActive(true);
            });
        }

        WireSelectionButtons();
        WireSliders();
    }

    void WireSelectionButtons()
    {
        if (difficultyButtons != null)
        {
            for (int i = 0; i < difficultyButtons.Length; i++)
            {
                int index = i;
                difficultyButtons[i].onClick.RemoveAllListeners();
                difficultyButtons[i].onClick.AddListener(() =>
                {
                    selectedDifficulty = (GameManager.GameDifficulty)index;
                    GameAudio.Instance?.PlayUiConfirm();
                    RefreshLabels();
                });
            }
        }

        if (modeButtons != null)
        {
            for (int i = 0; i < modeButtons.Length; i++)
            {
                int index = i;
                modeButtons[i].onClick.RemoveAllListeners();
                modeButtons[i].onClick.AddListener(() =>
                {
                    selectedMode = (GameManager.GameMode)index;
                    GameAudio.Instance?.PlayUiConfirm();
                    RefreshLabels();
                });
            }
        }
    }

    void WireSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.SetValueWithoutNotify(SaveProfile.MusicVolume);
            musicSlider.onValueChanged.AddListener(value => GameAudio.Instance?.SetMusicVolume(value));
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.SetValueWithoutNotify(SaveProfile.SfxVolume);
            sfxSlider.onValueChanged.AddListener(value => GameAudio.Instance?.SetSfxVolume(value));
        }
    }

    void RefreshLabels()
    {
        if (selectedModeText != null)
            selectedModeText.text = GetModeDisplayName(selectedMode) + " // " + selectedDifficulty;

        if (modeDescriptionText != null)
            modeDescriptionText.text = GetModeDescription(selectedMode);

        if (bestScoreText != null)
        {
            int unlockedWeapons = 1 + CountSetBits(SaveProfile.PermanentWeaponFlags);
            int bestScore = SaveProfile.GetBestScore(selectedMode);
            bestScoreText.text =
                "Best " + GetModeDisplayName(selectedMode) + ": " + bestScore
                + "\nScrap Bank: " + SaveProfile.TotalScrap
                + "\nWeapons Online: " + unlockedWeapons + " / 5"
                + "\nBoss Clears: " + SaveProfile.BestBossKills
                + "\n" + SaveProfile.GetAchievementSummary();
        }

        if (pauseSummaryText != null)
            pauseSummaryText.text = BuildPauseSummary();

        UpdateButtonHighlighting(modeButtons, (int)selectedMode, GetModeAccentColor(selectedMode));
        UpdateButtonHighlighting(difficultyButtons, (int)selectedDifficulty, GetDifficultyAccentColor(selectedDifficulty));
        RefreshBackdropArt();
    }

    void UpdateButtonHighlighting(Button[] buttons, int selectedIndex, Color selectedColor)
    {
        if (buttons == null)
            return;

        Color normalColor = new Color(0.08f, 0.12f, 0.2f, 0.76f);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null)
                continue;

            Image image = buttons[i].GetComponent<Image>();
            if (image == null)
                continue;

            Color tint = i == selectedIndex ? selectedColor : normalColor;
            image.color = tint;

            Outline outline = buttons[i].GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = i == selectedIndex
                    ? new Color(selectedColor.r, selectedColor.g, selectedColor.b, 0.72f)
                    : new Color(1f, 1f, 1f, 0.14f);
            }

            ColorBlock colors = buttons[i].colors;
            colors.normalColor = tint;
            colors.highlightedColor = Color.Lerp(tint, Color.white, 0.15f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = Color.Lerp(tint, Color.black, 0.18f);
            colors.fadeDuration = 0.08f;
            buttons[i].colors = colors;
        }
    }

    void RefreshBackdropArt()
    {
        Sprite startSprite = GetMenuBackdrop();
        if (startBackdropImage != null)
        {
            ApplyBackdropSprite(startBackdropImage, startSprite, Color.white);
        }

        if (pauseBackdropImage != null)
        {
            ApplyBackdropSprite(pauseBackdropImage, GetPauseBackdrop(), new Color(1f, 1f, 1f, 0.86f));
        }

        if (optionsBackdropImage != null)
        {
            ApplyBackdropSprite(optionsBackdropImage,
                gameManager != null && gameManager.IsPaused ? GetPauseBackdrop() : startSprite,
                new Color(1f, 1f, 1f, 0.88f));
        }

        if (startHeroShipImage != null)
            startHeroShipImage.gameObject.SetActive(false);

        if (startEnemyShipImage != null)
            startEnemyShipImage.gameObject.SetActive(false);
    }

    GameObject BuildStartMenu()
    {
        GameObject overlay = CreateOverlayPanel(StartPanelName);
        startBackdropImage = CreateStretchImage(overlay.transform, "Backdrop", GetMenuBackdrop(), Color.white);
        CreateStretchImage(overlay.transform, "BackdropShade", SpriteHelper.Square, new Color(0.01f, 0.02f, 0.05f, 0.76f));
        CreatePanel(overlay.transform, "BackdropFocusPanel", new Vector2(1260f, 800f), new Vector2(0f, 0f),
            new Color(0.02f, 0.04f, 0.09f, 0.34f), Color.clear, Vector2.zero, false, 0f);

        GameObject card = CreatePanel(overlay.transform, "StartMenuCard", new Vector2(1080f, 724f), Vector2.zero,
            new Color(0.03f, 0.06f, 0.11f, 0.88f), new Color(0.58f, 0.78f, 1f, 0.44f), new Vector2(0f, -12f), false, 0f);

        startHeroShipImage = CreateCenteredImage(card.transform, "PlayerHero", KenneyAssets.PlayerBlue, new Vector2(148f, 148f), new Vector2(-448f, 198f), Color.white);
        startEnemyShipImage = CreateCenteredImage(card.transform, "EnemyHero", KenneyAssets.GetEnemySprite(Enemy.EnemyType.Shielded), new Vector2(148f, 148f), new Vector2(448f, 198f), Color.white);

        GameObject headerPlate = CreatePanel(card.transform, "HeaderPlate", new Vector2(880f, 122f), new Vector2(0f, 242f),
            new Color(0.07f, 0.12f, 0.22f, 0.88f), new Color(0.6f, 0.78f, 1f, 0.38f), new Vector2(0f, -6f), true, 0.04f);
        CreateText(headerPlate.transform, "TitleText", "STARFALL FRONTIER", 58, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(820f, 62f), new Vector2(0f, 4f), Color.white);
        CreateText(headerPlate.transform, "SubtitleText", "Choose your sector, set the threat level, and launch into the breach.",
            24, FontStyle.Italic, TextAnchor.MiddleCenter, new Vector2(820f, 34f), new Vector2(0f, -34f), new Color(0.9f, 0.95f, 1f));

        GameObject modePanel = CreatePanel(card.transform, "ModePanel", new Vector2(470f, 220f), new Vector2(-250f, 54f),
            new Color(0.07f, 0.1f, 0.18f, 0.84f), new Color(0.38f, 0.66f, 1f, 0.28f), new Vector2(0f, -6f), false, 0f);
        CreateText(modePanel.transform, "ModesLabel", "MISSION TYPE", 24, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(280f, 34f), new Vector2(0f, 78f), new Color(0.95f, 0.97f, 1f));

        modeButtons = new[]
        {
            CreateButton(modePanel.transform, "ModeCampaign", "CAMPAIGN", new Vector2(190f, 54f), new Vector2(-102f, 22f), new Color(0.12f, 0.16f, 0.24f), new Color(0.18f, 0.24f, 0.32f)),
            CreateButton(modePanel.transform, "ModeEndless", "ENDLESS", new Vector2(190f, 54f), new Vector2(102f, 22f), new Color(0.12f, 0.16f, 0.24f), new Color(0.18f, 0.24f, 0.32f)),
            CreateButton(modePanel.transform, "ModeTime", "TIME ATTACK", new Vector2(190f, 54f), new Vector2(-102f, -48f), new Color(0.12f, 0.16f, 0.24f), new Color(0.18f, 0.24f, 0.32f)),
            CreateButton(modePanel.transform, "ModeChallenge", "CHALLENGE", new Vector2(190f, 54f), new Vector2(102f, -48f), new Color(0.12f, 0.16f, 0.24f), new Color(0.18f, 0.24f, 0.32f)),
        };

        GameObject difficultyPanel = CreatePanel(card.transform, "DifficultyPanel", new Vector2(360f, 236f), new Vector2(285f, 54f),
            new Color(0.07f, 0.1f, 0.18f, 0.84f), new Color(0.95f, 0.72f, 0.26f, 0.24f), new Vector2(0f, -6f), false, 0f);
        CreateText(difficultyPanel.transform, "DifficultyLabel", "THREAT LEVEL", 24, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(260f, 34f), new Vector2(0f, 86f), new Color(0.95f, 0.97f, 1f));

        difficultyButtons = new[]
        {
            CreateButton(difficultyPanel.transform, "DiffEasy", "EASY", new Vector2(230f, 52f), new Vector2(0f, 42f), new Color(0.12f, 0.16f, 0.24f), new Color(0.18f, 0.24f, 0.32f)),
            CreateButton(difficultyPanel.transform, "DiffMedium", "MEDIUM", new Vector2(230f, 52f), new Vector2(0f, -22f), new Color(0.12f, 0.16f, 0.24f), new Color(0.18f, 0.24f, 0.32f)),
            CreateButton(difficultyPanel.transform, "DiffHard", "HARD", new Vector2(230f, 52f), new Vector2(0f, -86f), new Color(0.12f, 0.16f, 0.24f), new Color(0.18f, 0.24f, 0.32f)),
        };

        GameObject briefingPanel = CreatePanel(card.transform, "BriefingPanel", new Vector2(420f, 156f), new Vector2(-230f, -146f),
            new Color(0.06f, 0.09f, 0.16f, 0.86f), new Color(0.4f, 0.64f, 1f, 0.24f), new Vector2(0f, -6f), false, 0f);
        CreateText(briefingPanel.transform, "BriefingLabel", "SECTOR BRIEF", 20, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(260f, 28f), new Vector2(0f, 54f), new Color(0.95f, 0.97f, 1f));
        selectedModeText = CreateText(briefingPanel.transform, "SelectedModeText", string.Empty, 22, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Vector2(360f, 28f), new Vector2(0f, 18f), new Color(1f, 0.86f, 0.45f));
        modeDescriptionText = CreateText(briefingPanel.transform, "ModeDescriptionText", string.Empty, 19, FontStyle.Normal,
            TextAnchor.MiddleCenter, new Vector2(360f, 56f), new Vector2(0f, -34f), new Color(0.92f, 0.95f, 1f));

        GameObject profilePanel = CreatePanel(card.transform, "ProfilePanel", new Vector2(420f, 156f), new Vector2(230f, -146f),
            new Color(0.06f, 0.09f, 0.16f, 0.86f), new Color(0.95f, 0.72f, 0.28f, 0.22f), new Vector2(0f, -6f), false, 0f);
        CreateText(profilePanel.transform, "ProfileLabel", "PILOT PROFILE", 20, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(260f, 28f), new Vector2(0f, 54f), new Color(0.95f, 0.97f, 1f));
        bestScoreText = CreateText(profilePanel.transform, "BestScoreText", string.Empty, 16, FontStyle.Normal,
            TextAnchor.UpperCenter, new Vector2(360f, 96f), new Vector2(0f, -14f), new Color(0.84f, 0.91f, 1f));

        startRunButton = CreateButton(card.transform, "StartRunButton", "START RUN", new Vector2(270f, 64f),
            new Vector2(-150f, -300f), new Color(0.16f, 0.66f, 0.34f), new Color(0.22f, 0.76f, 0.42f));
        openOptionsFromStartButton = CreateButton(card.transform, "OptionsButton", "OPTIONS", new Vector2(270f, 64f),
            new Vector2(150f, -300f), new Color(0.15f, 0.39f, 0.82f), new Color(0.22f, 0.48f, 0.94f));

        CreateText(card.transform, "ControlsText", "MOVE: WASD / ARROWS   |   FIRE: HOLD LEFT CLICK   |   SWITCH: 1-5   |   PAUSE: ESC",
            18, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(880f, 28f), new Vector2(0f, -338f), new Color(1f, 1f, 1f, 0.84f));
        return overlay;
    }

    GameObject BuildPauseMenu()
    {
        GameObject overlay = CreateOverlayPanel(PausePanelName);
        pauseBackdropImage = CreateStretchImage(overlay.transform, "Backdrop", GetPauseBackdrop(), Color.white);
        CreateStretchImage(overlay.transform, "BackdropShade", SpriteHelper.Square, new Color(0.02f, 0.03f, 0.08f, 0.8f));
        CreateCenteredImage(overlay.transform, "PauseGlow", SpriteHelper.Circle, new Vector2(880f, 880f), new Vector2(0f, -80f), new Color(0.3f, 0.5f, 0.95f, 0.08f));

        GameObject card = CreatePanel(overlay.transform, "PauseMenuCard", new Vector2(720f, 520f), Vector2.zero,
            new Color(0.04f, 0.07f, 0.14f, 0.9f), new Color(0.5f, 0.72f, 1f, 0.36f), new Vector2(0f, -10f), true, 0.04f);

        CreateText(card.transform, "PauseTitle", "SECTOR PAUSED", 54, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(520f, 64f), new Vector2(0f, 176f), Color.white);
        CreateText(card.transform, "PauseBody", "Refit your setup, check the run status, and jump back in when ready.",
            21, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(560f, 46f), new Vector2(0f, 126f), new Color(0.84f, 0.91f, 1f));

        GameObject summaryPanel = CreatePanel(card.transform, "PauseSummaryPanel", new Vector2(540f, 132f), new Vector2(0f, 32f),
            new Color(0.07f, 0.1f, 0.18f, 0.86f), new Color(0.95f, 0.72f, 0.26f, 0.22f), new Vector2(0f, -6f), false, 0f);
        pauseSummaryText = CreateText(summaryPanel.transform, "PauseSummaryText", string.Empty, 21, FontStyle.Normal,
            TextAnchor.MiddleCenter, new Vector2(470f, 96f), new Vector2(0f, 0f), new Color(0.92f, 0.96f, 1f));

        resumeButton = CreateButton(card.transform, "ResumeButton", "RESUME RUN", new Vector2(270f, 58f),
            new Vector2(0f, -86f), new Color(0.16f, 0.66f, 0.34f), new Color(0.22f, 0.76f, 0.42f));
        openOptionsFromPauseButton = CreateButton(card.transform, "OptionsPauseButton", "AUDIO / OPTIONS", new Vector2(270f, 58f),
            new Vector2(0f, -158f), new Color(0.15f, 0.39f, 0.82f), new Color(0.22f, 0.48f, 0.94f));
        restartFromPauseButton = CreateButton(card.transform, "RestartPauseButton", "RESTART RUN", new Vector2(270f, 58f),
            new Vector2(0f, -230f), new Color(0.82f, 0.25f, 0.2f), new Color(0.92f, 0.32f, 0.24f));

        overlay.SetActive(false);
        return overlay;
    }

    GameObject BuildOptionsMenu()
    {
        GameObject overlay = CreateOverlayPanel(OptionsPanelName);
        optionsBackdropImage = CreateStretchImage(overlay.transform, "Backdrop", GetMenuBackdrop(), Color.white);
        CreateStretchImage(overlay.transform, "BackdropShade", SpriteHelper.Square, new Color(0.01f, 0.02f, 0.05f, 0.82f));
        CreateCenteredImage(overlay.transform, "OptionsGlow", SpriteHelper.Circle, new Vector2(820f, 820f), new Vector2(-260f, 0f), new Color(0.28f, 0.48f, 0.95f, 0.1f));

        GameObject card = CreatePanel(overlay.transform, "OptionsCard", new Vector2(700f, 430f), Vector2.zero,
            new Color(0.04f, 0.07f, 0.14f, 0.9f), new Color(0.5f, 0.72f, 1f, 0.34f), new Vector2(0f, -10f), true, 0.04f);

        CreateText(card.transform, "OptionsTitle", "AUDIO OPTIONS", 46, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(460f, 48f), new Vector2(0f, 146f), Color.white);
        CreateText(card.transform, "OptionsBody", "Menu ambience and battle music are mixed separately from gameplay SFX.",
            19, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(520f, 34f), new Vector2(0f, 102f), new Color(0.84f, 0.91f, 1f));

        CreateText(card.transform, "MusicLabel", "MUSIC", 24, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(170f, 34f), new Vector2(-210f, 24f), new Color(0.95f, 0.97f, 1f));
        CreateText(card.transform, "SfxLabel", "SFX", 24, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(170f, 34f), new Vector2(-210f, -48f), new Color(0.95f, 0.97f, 1f));

        musicSlider = CreateSlider(card.transform, "MusicSlider", new Vector2(320f, 24f), new Vector2(58f, 24f));
        sfxSlider = CreateSlider(card.transform, "SfxSlider", new Vector2(320f, 24f), new Vector2(58f, -48f));

        closeOptionsButton = CreateButton(card.transform, "CloseOptionsButton", "CLOSE", new Vector2(240f, 56f),
            new Vector2(0f, -136f), new Color(0.15f, 0.39f, 0.82f), new Color(0.22f, 0.48f, 0.94f));

        overlay.SetActive(false);
        return overlay;
    }

    void ShowOptions(GameObject fallbackPanel)
    {
        GameAudio.Instance?.PlayUiConfirm();
        if (fallbackPanel != null)
            fallbackPanel.SetActive(false);

        RefreshLabels();
        optionsPanel.SetActive(true);
        WireSliders();
    }

    GameObject CreateOverlayPanel(string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.sprite = SpriteHelper.Square;
        image.color = new Color(0f, 0f, 0f, 0.001f);
        return panel;
    }

    GameObject CreatePanel(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, Color fillColor, Color borderColor, Vector2 shadowDistance,
        bool addGlassHighlight = true, float highlightAlpha = 0.1f)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = panel.AddComponent<Image>();
        StyleRoundedImage(image, fillColor);
        AddOutline(image, borderColor, new Vector2(1.6f, -1.6f));
        AddShadow(image, new Color(0f, 0f, 0f, 0.42f), shadowDistance);

        if (addGlassHighlight)
            AddGlassHighlight(panel.transform, size, highlightAlpha);

        return panel;
    }

    Image CreateStretchImage(Transform parent, string name, Sprite sprite, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = obj.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.preserveAspect = true;
        image.raycastTarget = false;

        AspectRatioFitter fitter = obj.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = GetAspectRatio(sprite);
        return image;
    }

    Image CreateCenteredImage(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 anchoredPosition, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = obj.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    Text CreateText(Transform parent, string name, string value, int fontSize, FontStyle fontStyle,
        TextAnchor alignment, Vector2 size, Vector2 anchoredPosition, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Text text = obj.AddComponent<Text>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.lineSpacing = 1f;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.font = GetMenuFont(fontStyle, fontSize);
        text.raycastTarget = false;
        AddShadow(text, new Color(0f, 0f, 0f, 0.48f), new Vector2(1.8f, -1.8f));
        return text;
    }

    Button CreateButton(Transform parent, string name, string label, Vector2 size, Vector2 anchoredPosition,
        Color normalColor, Color highlightedColor)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = obj.AddComponent<Image>();
        StyleRoundedImage(image, normalColor);
        AddOutline(image, new Color(1f, 1f, 1f, 0.14f), new Vector2(1.4f, -1.4f));
        AddShadow(image, new Color(0f, 0f, 0f, 0.36f), new Vector2(0f, -4f));
        AddGlassHighlight(obj.transform, size, 0.18f);

        Button button = obj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = highlightedColor;
        colors.selectedColor = highlightedColor;
        colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.18f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        CreateText(obj.transform, "Label", label, 24, FontStyle.Bold, TextAnchor.MiddleCenter, size, Vector2.zero, Color.white);
        return button;
    }

    Slider CreateSlider(Transform parent, string name, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        GameObject background = new GameObject("Background");
        background.transform.SetParent(obj.transform, false);
        RectTransform backgroundRect = background.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        Image backgroundImage = background.AddComponent<Image>();
        StyleRoundedImage(backgroundImage, new Color(0.08f, 0.12f, 0.2f, 0.7f));
        AddOutline(backgroundImage, new Color(1f, 1f, 1f, 0.12f), new Vector2(1.2f, -1.2f));
        AddGlassHighlight(background.transform, size, 0.12f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(obj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0f);
        fillAreaRect.anchorMax = new Vector2(1f, 1f);
        fillAreaRect.offsetMin = new Vector2(5f, 5f);
        fillAreaRect.offsetMax = new Vector2(-24f, -5f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.sprite = SpriteHelper.RoundedRect;
        fillImage.type = Image.Type.Sliced;
        fillImage.color = new Color(0.28f, 0.7f, 1f);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(obj.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(24f, 30f);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.sprite = SpriteHelper.RoundedRect;
        handleImage.type = Image.Type.Sliced;
        handleImage.color = new Color(0.96f, 0.98f, 1f);
        AddOutline(handleImage, new Color(1f, 1f, 1f, 0.18f), new Vector2(1f, -1f));
        AddShadow(handleImage, new Color(0f, 0f, 0f, 0.24f), new Vector2(0f, -2f));

        Slider slider = obj.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.75f;
        return slider;
    }

    void SetHudVisible(bool visible)
    {
        if (hudObjects == null)
            return;

        for (int i = 0; i < hudObjects.Length; i++)
        {
            if (hudObjects[i] != null)
                hudObjects[i].SetActive(visible);
        }
    }

    static void AddOutline(Graphic graphic, Color color, Vector2 distance)
    {
        if (graphic == null)
            return;

        Outline outline = graphic.GetComponent<Outline>();
        if (outline == null)
            outline = graphic.gameObject.AddComponent<Outline>();

        outline.effectColor = color;
        outline.effectDistance = distance;
    }

    static void AddShadow(Graphic graphic, Color color, Vector2 distance)
    {
        if (graphic == null)
            return;

        Shadow shadow = graphic.GetComponent<Shadow>();
        if (shadow == null)
            shadow = graphic.gameObject.AddComponent<Shadow>();

        shadow.effectColor = color;
        shadow.effectDistance = distance;
    }

    static void StyleRoundedImage(Image image, Color color)
    {
        if (image == null)
            return;

        image.sprite = SpriteHelper.RoundedRect;
        image.type = Image.Type.Sliced;
        image.color = color;
    }

    static void AddGlassHighlight(Transform parent, Vector2 parentSize, float alpha)
    {
        if (parent == null || alpha <= 0f)
            return;

        GameObject obj = new GameObject("GlassHighlight");
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(Mathf.Max(0f, parentSize.x - 24f), Mathf.Clamp(parentSize.y * 0.14f, 8f, 20f));
        rect.anchoredPosition = new Vector2(0f, parentSize.y * 0.38f);

        Image image = obj.AddComponent<Image>();
        image.sprite = SpriteHelper.RoundedRect;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.74f, 0.86f, 1f, alpha);
        image.raycastTarget = false;
    }

    static Font GetMenuFont(FontStyle fontStyle, int fontSize)
    {
        bool useDisplayFace = fontSize >= 42;

        if (useDisplayFace)
        {
            if (cachedDisplayFont == null)
                cachedDisplayFont = LoadFont(new[] { "Bahnschrift SemiBold", "Segoe UI Semibold", "Trebuchet MS Bold", "Verdana Bold", "Arial Bold" }, fontSize);
            return cachedDisplayFont;
        }

        if (cachedBodyFont == null)
            cachedBodyFont = LoadFont(new[] { "Segoe UI", "Trebuchet MS", "Verdana", "Arial" }, fontSize);
        return cachedBodyFont;
    }

    static Font LoadFont(string[] osFontNames, int fontSize)
    {
        Font font = null;

        if (osFontNames != null && osFontNames.Length > 0)
            font = Font.CreateDynamicFontFromOSFont(osFontNames, fontSize);

        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        return font;
    }

    static Transform FindDeep(Transform parent, string name)
    {
        if (parent == null)
            return null;

        if (parent.name == name)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeep(parent.GetChild(i), name);
            if (found != null)
                return found;
        }

        return null;
    }

    static string GetModeDisplayName(GameManager.GameMode mode)
    {
        return mode switch
        {
            GameManager.GameMode.TimeAttack => "Time Attack",
            _ => mode.ToString(),
        };
    }

    static string GetModeDescription(GameManager.GameMode mode)
    {
        return mode switch
        {
            GameManager.GameMode.Campaign => "Three sectors, upgrade breaks, and one boss to finish the mission.",
            GameManager.GameMode.Endless => "Loop the battlefield forever and chase the highest possible score.",
            GameManager.GameMode.TimeAttack => "Two minutes of all-out pressure. Score hard and score fast.",
            GameManager.GameMode.Challenge => "The harshest sector mix with tighter gaps and nastier enemy behavior.",
            _ => "Launch into the frontier and push your run as far as it will go.",
        };
    }

    string BuildPauseSummary()
    {
        if (gameManager == null || !gameManager.HasStarted)
            return "Awaiting launch...";

        string summary =
            gameManager.SelectedModeLabel + " // " + gameManager.SelectedDifficulty
            + "\n" + gameManager.CurrentStageLabel
            + "\nScore: " + gameManager.Score + "   |   Scrap: " + gameManager.ScrapThisRun;

        if (gameManager.HasObjective)
            summary += "\n" + gameManager.ObjectiveText;

        return summary;
    }

    Sprite GetPauseBackdrop()
    {
        if (gameManager == null)
            return GetBackdropForMode(selectedMode);

        return KenneyAssets.GetBackgroundForRun(gameManager.SelectedMode, gameManager.CurrentStage);
    }

    static Sprite GetBackdropForMode(GameManager.GameMode mode)
    {
        return mode switch
        {
            GameManager.GameMode.Endless => KenneyAssets.BackgroundPurple,
            GameManager.GameMode.TimeAttack => KenneyAssets.BackgroundBlue,
            GameManager.GameMode.Challenge => KenneyAssets.BackgroundBlack,
            _ => KenneyAssets.Background,
        };
    }

    static Sprite GetMenuBackdrop()
    {
        return KenneyAssets.MenuBackground != null ? KenneyAssets.MenuBackground : KenneyAssets.BackgroundPurple;
    }

    Sprite GetDecorativeEnemySprite()
    {
        GameManager.GameDifficulty difficulty = gameManager != null && gameManager.HasStarted
            ? gameManager.SelectedDifficulty
            : selectedDifficulty;
        GameManager.GameMode mode = gameManager != null && gameManager.HasStarted
            ? gameManager.SelectedMode
            : selectedMode;

        Enemy.EnemyType type = mode switch
        {
            GameManager.GameMode.Challenge => Enemy.EnemyType.Sniper,
            GameManager.GameMode.TimeAttack => Enemy.EnemyType.Kamikaze,
            _ => difficulty == GameManager.GameDifficulty.Hard ? Enemy.EnemyType.Shielded : Enemy.EnemyType.Turret,
        };

        return KenneyAssets.GetEnemySprite(type);
    }

    static Color GetModeAccentColor(GameManager.GameMode mode)
    {
        return mode switch
        {
            GameManager.GameMode.Campaign => new Color(0.18f, 0.45f, 0.88f),
            GameManager.GameMode.Endless => new Color(0.18f, 0.72f, 0.78f),
            GameManager.GameMode.TimeAttack => new Color(0.92f, 0.58f, 0.16f),
            GameManager.GameMode.Challenge => new Color(0.82f, 0.24f, 0.2f),
            _ => new Color(0.24f, 0.58f, 0.94f),
        };
    }

    static Color GetDifficultyAccentColor(GameManager.GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameManager.GameDifficulty.Easy => new Color(0.18f, 0.7f, 0.36f),
            GameManager.GameDifficulty.Hard => new Color(0.86f, 0.26f, 0.22f),
            _ => new Color(0.22f, 0.55f, 0.94f),
        };
    }

    static int CountSetBits(int value)
    {
        int count = 0;
        while (value != 0)
        {
            count += value & 1;
            value >>= 1;
        }

        return count;
    }

    static void ApplyBackdropSprite(Image image, Sprite sprite, Color color)
    {
        if (image == null)
            return;

        image.sprite = sprite;
        image.color = color;
        image.preserveAspect = true;

        AspectRatioFitter fitter = image.GetComponent<AspectRatioFitter>();
        if (fitter != null)
            fitter.aspectRatio = GetAspectRatio(sprite);
    }

    static float GetAspectRatio(Sprite sprite)
    {
        if (sprite == null || sprite.rect.height <= 0f)
            return 16f / 9f;

        return sprite.rect.width / sprite.rect.height;
    }
}
