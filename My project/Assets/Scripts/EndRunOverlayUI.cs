using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Full-screen end-of-run overlay for victory and defeat states.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class EndRunOverlayUI : MonoBehaviour
{
    static Font cachedFont;

    GameObject panel;
    Image backdropImage;
    Image playerShipImage;
    Image enemyShipImage;
    Image titlePlateImage;
    Text titleText;
    Text subtitleText;
    Text modeText;
    Text summaryText;
    Text achievementText;
    Text bestText;
    Text inputHintText;
    Button restartButton;
    Button menuButton;
    bool showing;

    void Awake()
    {
        EnsureBuilt();
    }

    void Update()
    {
        if (!showing || panel == null || !panel.activeSelf)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.rKey.wasPressedThisFrame)
        {
            GameAudio.Instance?.PlayUiConfirm();
            GameManager.Instance?.RestartGame();
        }
        else if (keyboard.mKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
        {
            GameAudio.Instance?.PlayPauseToggle();
            GameManager.Instance?.OpenStartMenu();
        }
    }

    public void Show(string title, Color titleColor, string body, bool isVictory)
    {
        EnsureBuilt();
        RefreshContent(title, titleColor, body, isVictory);
        showing = true;
        panel.SetActive(true);
    }

    public void Hide()
    {
        EnsureBuilt();
        showing = false;
        panel.SetActive(false);
    }

    void EnsureBuilt()
    {
        if (panel != null)
            return;

        panel = new GameObject("EndRunOverlayPanel");
        panel.transform.SetParent(transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image rootImage = panel.AddComponent<Image>();
        rootImage.sprite = SpriteHelper.Square;
        rootImage.color = new Color(0f, 0f, 0f, 0.001f);

        backdropImage = CreateStretchImage(panel.transform, "Backdrop", KenneyAssets.BackgroundBlack, new Color(1f, 1f, 1f, 0.92f));
        CreateStretchImage(panel.transform, "BackdropShade", SpriteHelper.Square, new Color(0.02f, 0.03f, 0.08f, 0.76f));
        CreateCenteredImage(panel.transform, "LeftGlow", SpriteHelper.Circle, new Vector2(960f, 960f), new Vector2(-620f, 120f), new Color(0.22f, 0.48f, 0.95f, 0.12f));
        CreateCenteredImage(panel.transform, "RightGlow", SpriteHelper.Circle, new Vector2(840f, 840f), new Vector2(640f, -50f), new Color(0.95f, 0.46f, 0.16f, 0.1f));

        GameObject card = CreatePanel(panel.transform, "EndRunCard", new Vector2(1000f, 620f), Vector2.zero,
            new Color(0.05f, 0.08f, 0.15f, 0.95f), new Color(0.38f, 0.62f, 0.98f, 0.72f), new Vector2(0f, -8f));

        playerShipImage = CreateCenteredImage(card.transform, "PlayerShipDeco", KenneyAssets.PlayerBlue, new Vector2(190f, 190f), new Vector2(-370f, 184f), Color.white);
        enemyShipImage = CreateCenteredImage(card.transform, "EnemyShipDeco", KenneyAssets.BossCore, new Vector2(190f, 190f), new Vector2(370f, 184f), Color.white);

        GameObject titlePlate = CreatePanel(card.transform, "TitlePlate", new Vector2(760f, 136f), new Vector2(0f, 188f),
            new Color(0.08f, 0.13f, 0.24f, 0.96f), new Color(0.56f, 0.76f, 1f, 0.7f), new Vector2(0f, -5f));
        titlePlateImage = titlePlate.GetComponent<Image>();
        titleText = CreateText(titlePlate.transform, "TitleText", "MISSION COMPLETE", 54, FontStyle.Bold,
            new Vector2(700f, 68f), new Vector2(0f, 22f), TextAnchor.MiddleCenter, Color.white);
        subtitleText = CreateText(titlePlate.transform, "SubtitleText", "Boss destroyed. Sector secured.", 23, FontStyle.Italic,
            new Vector2(700f, 34f), new Vector2(0f, -30f), TextAnchor.MiddleCenter, new Color(0.84f, 0.91f, 1f));

        GameObject modePanel = CreatePanel(card.transform, "ModePanel", new Vector2(820f, 74f), new Vector2(0f, 88f),
            new Color(0.07f, 0.1f, 0.17f, 0.96f), new Color(0.98f, 0.74f, 0.24f, 0.4f), new Vector2(0f, -4f));
        modeText = CreateText(modePanel.transform, "ModeText", string.Empty, 22, FontStyle.Bold,
            new Vector2(760f, 24f), new Vector2(0f, 14f), TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.48f));
        summaryText = CreateText(modePanel.transform, "SummaryText", string.Empty, 18, FontStyle.Normal,
            new Vector2(760f, 32f), new Vector2(0f, -18f), TextAnchor.MiddleCenter, new Color(0.88f, 0.93f, 1f));

        GameObject statsPanel = CreatePanel(card.transform, "StatsPanel", new Vector2(820f, 172f), new Vector2(0f, -46f),
            new Color(0.07f, 0.1f, 0.17f, 0.96f), new Color(0.34f, 0.58f, 0.96f, 0.38f), new Vector2(0f, -4f));
        achievementText = CreateText(statsPanel.transform, "AchievementText", string.Empty, 24, FontStyle.Bold,
            new Vector2(740f, 36f), new Vector2(0f, 42f), TextAnchor.MiddleCenter, Color.white);
        bestText = CreateText(statsPanel.transform, "BestText", string.Empty, 19, FontStyle.Normal,
            new Vector2(740f, 48f), new Vector2(0f, -6f), TextAnchor.MiddleCenter, new Color(0.84f, 0.91f, 1f));
        inputHintText = CreateText(statsPanel.transform, "InputHintText", "ENTER / R TO RESTART   |   M / ESC FOR MAIN MENU",
            17, FontStyle.Normal, new Vector2(760f, 24f), new Vector2(0f, -56f), TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.74f));

        restartButton = CreateButton(card.transform, "RestartButton", "RESTART RUN", new Vector2(290f, 64f), new Vector2(-160f, -230f),
            new Color(0.16f, 0.66f, 0.34f), new Color(0.22f, 0.76f, 0.42f));
        menuButton = CreateButton(card.transform, "MenuButton", "MAIN MENU", new Vector2(290f, 64f), new Vector2(160f, -230f),
            new Color(0.15f, 0.39f, 0.82f), new Color(0.22f, 0.48f, 0.94f));

        restartButton.onClick.AddListener(() =>
        {
            GameAudio.Instance?.PlayUiConfirm();
            GameManager.Instance?.RestartGame();
        });

        menuButton.onClick.AddListener(() =>
        {
            GameAudio.Instance?.PlayPauseToggle();
            GameManager.Instance?.OpenStartMenu();
        });

        panel.SetActive(false);
        showing = false;
    }

    void RefreshContent(string title, Color titleColor, string body, bool isVictory)
    {
        GameManager gameManager = GameManager.Instance;

        if (backdropImage != null)
        {
            Sprite backdrop = gameManager != null
                ? GetBackdrop(gameManager, isVictory)
                : (isVictory ? KenneyAssets.BackgroundBlue : KenneyAssets.BackgroundBlack);
            backdropImage.sprite = backdrop;
        }

        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = titleColor;
        }

        if (subtitleText != null)
            subtitleText.text = body;

        if (titlePlateImage != null)
        {
            Color plate = isVictory ? new Color(0.13f, 0.18f, 0.3f, 0.96f) : new Color(0.24f, 0.08f, 0.12f, 0.96f);
            titlePlateImage.color = plate;
        }

        if (playerShipImage != null)
        {
            playerShipImage.sprite = KenneyAssets.GetPlayerShipForUnlockCount(SaveProfile.ShipSkinUnlockCount);
            playerShipImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 14f);
        }

        if (enemyShipImage != null)
        {
            enemyShipImage.sprite = GetRightSideSprite(gameManager, isVictory);
            enemyShipImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -14f);
            enemyShipImage.rectTransform.localScale = new Vector3(-1.05f, 1.05f, 1f);
        }

        if (modeText != null)
        {
            if (gameManager != null)
                modeText.text = gameManager.SelectedModeLabel.ToUpperInvariant() + "  //  " + gameManager.SelectedDifficulty.ToString().ToUpperInvariant() + "  //  " + gameManager.CurrentStageName;
            else
                modeText.text = "RUN COMPLETE";
        }

        if (summaryText != null && gameManager != null)
            summaryText.text = "SCORE: " + gameManager.Score + "   |   SCRAP: " + gameManager.ScrapThisRun + "   |   BOSS KILLS: " + gameManager.BossKillsThisRun;

        if (achievementText != null)
            achievementText.text = SaveProfile.GetAchievementSummary().ToUpperInvariant();

        if (bestText != null)
            bestText.text = BuildBestText(gameManager);

        if (inputHintText != null)
            inputHintText.text = isVictory
                ? "SECTOR CLEARED  |  ENTER / R TO FLY AGAIN  |  M / ESC FOR MAIN MENU"
                : "RUN LOST  |  ENTER / R TO RESTART  |  M / ESC FOR MAIN MENU";
    }

    static Sprite GetBackdrop(GameManager gameManager, bool isVictory)
    {
        if (gameManager == null)
            return isVictory ? KenneyAssets.BackgroundBlue : KenneyAssets.BackgroundBlack;

        if (isVictory)
        {
            return gameManager.SelectedMode switch
            {
                GameManager.GameMode.Challenge => KenneyAssets.BackgroundPurple,
                GameManager.GameMode.TimeAttack => KenneyAssets.BackgroundBlue,
                _ => KenneyAssets.BackgroundPurple,
            };
        }

        return gameManager.SelectedMode == GameManager.GameMode.Campaign
            ? KenneyAssets.BackgroundBlack
            : KenneyAssets.GetBackgroundForRun(gameManager.SelectedMode, gameManager.CurrentStage);
    }

    static Sprite GetRightSideSprite(GameManager gameManager, bool isVictory)
    {
        if (isVictory)
            return gameManager != null && gameManager.BossKillsThisRun > 0 ? KenneyAssets.BossCore : KenneyAssets.BossWing;

        if (gameManager == null)
            return KenneyAssets.GetEnemySprite(Enemy.EnemyType.Shielded);

        Enemy.EnemyType type = gameManager.SelectedMode switch
        {
            GameManager.GameMode.Challenge => Enemy.EnemyType.Sniper,
            GameManager.GameMode.TimeAttack => Enemy.EnemyType.Kamikaze,
            _ => gameManager.SelectedDifficulty == GameManager.GameDifficulty.Hard ? Enemy.EnemyType.Shielded : Enemy.EnemyType.Turret,
        };

        return KenneyAssets.GetEnemySprite(type);
    }

    static string BuildBestText(GameManager gameManager)
    {
        if (gameManager == null)
            return "Best score data unavailable.";

        int bestScore = SaveProfile.GetBestScore(gameManager.SelectedMode);
        bool matchedBest = gameManager.Score >= bestScore && gameManager.Score > 0;
        string status = matchedBest ? "NEW PERSONAL BEST" : "BEST " + gameManager.SelectedModeLabel.ToUpperInvariant() + ": " + bestScore;
        return status + "\nTOTAL SCRAP BANK: " + SaveProfile.TotalScrap + "   |   BEST BOSS CLEARS: " + SaveProfile.BestBossKills;
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, Color fillColor, Color outlineColor, Vector2 shadowDistance)
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
        image.sprite = SpriteHelper.Square;
        image.color = fillColor;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.38f);
        shadow.effectDistance = shadowDistance;
        return obj;
    }

    static Image CreateStretchImage(Transform parent, string name, Sprite sprite, Color color)
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
        image.preserveAspect = false;
        return image;
    }

    static Image CreateCenteredImage(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 anchoredPosition, Color color)
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
        return image;
    }

    static Text CreateText(Transform parent, string name, string value, int fontSize, FontStyle fontStyle,
        Vector2 size, Vector2 anchoredPosition, TextAnchor alignment, Color color)
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
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.font = GetFont(fontSize);

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.42f);
        outline.effectDistance = new Vector2(1f, -1f);
        return text;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 size, Vector2 anchoredPosition, Color normalColor, Color highlightedColor)
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
        image.sprite = SpriteHelper.Square;
        image.color = normalColor;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.12f);
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.34f);
        shadow.effectDistance = new Vector2(0f, -4f);

        Button button = obj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = highlightedColor;
        colors.selectedColor = highlightedColor;
        colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.2f);
        button.colors = colors;

        CreateText(obj.transform, "Label", label, 24, FontStyle.Bold, size, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
        return button;
    }

    static Font GetFont(int fontSize)
    {
        if (cachedFont != null)
            return cachedFont;

        cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (cachedFont == null)
            cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (cachedFont == null)
            cachedFont = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        return cachedFont;
    }
}
