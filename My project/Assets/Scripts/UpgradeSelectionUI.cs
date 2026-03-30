using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Clean reward chamber overlay for choosing one upgrade between sectors.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class UpgradeSelectionUI : MonoBehaviour
{
    GameObject panel;
    Image backdropImage;
    Text sectorText;
    Text summaryText;
    Text inputHintText;
    Button[] optionButtons;
    Image[] optionCardImages;
    Image[] optionBadgeImages;
    Image[] optionIcons;
    Text[] optionTypeTexts;
    Text[] optionTitles;
    Text[] optionDescriptions;
    Text[] optionHintTexts;

    void Awake()
    {
        EnsureBuilt();
    }

    void Update()
    {
        if (panel == null || !panel.activeSelf || GameManager.Instance == null || !GameManager.Instance.IsUpgradeSelectionOpen)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (WasChoicePressed(keyboard, 0))
            GameManager.Instance.ApplyUpgradeChoice(0);
        else if (WasChoicePressed(keyboard, 1))
            GameManager.Instance.ApplyUpgradeChoice(1);
        else if (WasChoicePressed(keyboard, 2))
            GameManager.Instance.ApplyUpgradeChoice(2);
    }

    public void ShowChoices()
    {
        EnsureBuilt();
        RefreshContent();
        panel.SetActive(true);
    }

    public void Hide()
    {
        EnsureBuilt();
        panel.SetActive(false);
    }

    void EnsureBuilt()
    {
        if (panel != null && optionButtons != null && optionButtons.Length == 3)
            return;

        Transform existing = FindDeep(transform, "UpgradeSelectionPanel");
        if (existing != null)
            Destroy(existing.gameObject);

        BuildInterface();
    }

    void BuildInterface()
    {
        panel = new GameObject("UpgradeSelectionPanel");
        panel.transform.SetParent(transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image rootImage = panel.AddComponent<Image>();
        rootImage.sprite = SpriteHelper.Square;
        rootImage.color = new Color(0f, 0f, 0f, 0.001f);

        backdropImage = CreateStretchImage(panel.transform, "Backdrop", KenneyAssets.BackgroundPurple, new Color(1f, 1f, 1f, 0.94f));
        CreateStretchImage(panel.transform, "BackdropShade", SpriteHelper.Square, new Color(0.02f, 0.03f, 0.08f, 0.82f));

        GameObject card = CreatePanel(panel.transform, "RewardChamberCard", new Vector2(1220f, 628f), Vector2.zero,
            GameUiStyle.PanelFill, GameUiStyle.OutlineBlue, new Vector2(0f, -8f), true, 0.08f);

        GameObject headerPlate = CreatePanel(card.transform, "HeaderPlate", new Vector2(920f, 104f), new Vector2(0f, 222f),
            new Color(0.08f, 0.13f, 0.23f, 0.94f), new Color(0.62f, 0.8f, 1f, 0.36f), new Vector2(0f, -4f), true, 0.06f);
        CreateText(headerPlate.transform, "UpgradeTitle", "SECTOR REWARD CHAMBER", 42, FontStyle.Bold, GameUiStyle.TextPrimary,
            TextAnchor.MiddleCenter, new Vector2(840f, 40f), new Vector2(0f, 12f), true);
        CreateText(headerPlate.transform, "UpgradeSubtitle", "Choose one upgrade before the next wave begins.", 20, FontStyle.Italic, GameUiStyle.TextMuted,
            TextAnchor.MiddleCenter, new Vector2(840f, 24f), new Vector2(0f, -22f), false);

        GameObject summaryPanel = CreatePanel(card.transform, "SectorPanel", new Vector2(860f, 70f), new Vector2(0f, 138f),
            new Color(0.07f, 0.1f, 0.17f, 0.92f), GameUiStyle.OutlineGold, new Vector2(0f, -4f), false, 0f);
        sectorText = CreateText(summaryPanel.transform, "SectorText", string.Empty, 22, FontStyle.Bold, GameUiStyle.TextWarm,
            TextAnchor.MiddleCenter, new Vector2(780f, 24f), new Vector2(0f, 12f), false);
        summaryText = CreateText(summaryPanel.transform, "SummaryText", string.Empty, 17, FontStyle.Normal, GameUiStyle.TextMuted,
            TextAnchor.MiddleCenter, new Vector2(800f, 26f), new Vector2(0f, -14f), false);

        optionButtons = new Button[3];
        optionCardImages = new Image[3];
        optionBadgeImages = new Image[3];
        optionIcons = new Image[3];
        optionTypeTexts = new Text[3];
        optionTitles = new Text[3];
        optionDescriptions = new Text[3];
        optionHintTexts = new Text[3];

        for (int i = 0; i < 3; i++)
        {
            float x = -352f + i * 352f;
            GameObject option = CreatePanel(card.transform, "Option" + i, new Vector2(316f, 300f), new Vector2(x, -48f),
                new Color(0.07f, 0.1f, 0.18f, 0.96f), GameUiStyle.OutlineBlue, new Vector2(0f, -6f), false, 0f);

            optionCardImages[i] = option.GetComponent<Image>();
            Button button = option.AddComponent<Button>();
            optionCardImages[i].raycastTarget = true;
            button.transition = Selectable.Transition.ColorTint;
            int choiceIndex = i;
            button.onClick.AddListener(() => GameManager.Instance?.ApplyUpgradeChoice(choiceIndex));
            optionButtons[i] = button;

            optionBadgeImages[i] = CreateImage(option.transform, "Badge", SpriteHelper.Circle, new Vector2(76f, 76f), new Vector2(0f, 88f), Color.white);
            optionIcons[i] = CreateImage(option.transform, "Icon", SpriteHelper.Circle, new Vector2(40f, 40f), new Vector2(0f, 88f), Color.white);
            optionTypeTexts[i] = CreateText(option.transform, "Type", "SYSTEM", 15, FontStyle.Bold, GameUiStyle.TextMuted,
                TextAnchor.MiddleCenter, new Vector2(210f, 18f), new Vector2(0f, 36f), false);
            optionTitles[i] = CreateText(option.transform, "Title", "Upgrade", 30, FontStyle.Bold, GameUiStyle.TextPrimary,
                TextAnchor.MiddleCenter, new Vector2(244f, 34f), new Vector2(0f, 0f), true);
            optionDescriptions[i] = CreateText(option.transform, "Description", "Description", 18, FontStyle.Normal, GameUiStyle.TextMuted,
                TextAnchor.UpperCenter, new Vector2(244f, 76f), new Vector2(0f, -50f), false);
            optionHintTexts[i] = CreateText(option.transform, "Hint", "PRESS 1", 14, FontStyle.Bold, new Color(1f, 1f, 1f, 0.74f),
                TextAnchor.MiddleCenter, new Vector2(200f, 18f), new Vector2(0f, -122f), false);
        }

        inputHintText = CreateText(card.transform, "InputHint", "CLICK A CARD OR PRESS 1 / 2 / 3 TO LOCK IN YOUR REWARD",
            17, FontStyle.Normal, new Color(1f, 1f, 1f, 0.76f), TextAnchor.MiddleCenter, new Vector2(900f, 24f), new Vector2(0f, -262f), false);

        panel.SetActive(false);
    }

    void RefreshContent()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || optionButtons == null)
            return;

        if (backdropImage != null)
            backdropImage.sprite = KenneyAssets.GetBackgroundForRun(gameManager.SelectedMode, gameManager.CurrentStage);

        if (sectorText != null)
            sectorText.text = gameManager.CurrentStageLabel + " // " + gameManager.SelectedModeLabel;

        if (summaryText != null)
        {
            string objective = gameManager.HasObjective ? gameManager.ObjectiveText : "Choose one upgrade and continue the run.";
            summaryText.text = "Score " + gameManager.Score + "   |   Scrap " + gameManager.ScrapThisRun + "   |   " + objective;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            GameManager.UpgradeChoice choice = gameManager.GetUpgradeChoice(i);
            bool visible = choice != null;
            optionButtons[i].gameObject.SetActive(visible);

            if (!visible)
                continue;

            Color accent = GetUpgradeAccent(choice.type);
            Color buttonBase = Color.Lerp(new Color(0.07f, 0.1f, 0.18f, 0.98f), accent, 0.16f);
            Color buttonHighlight = Color.Lerp(buttonBase, accent, 0.18f);

            optionCardImages[i].color = buttonBase;
            GameUiStyle.AddOutline(optionCardImages[i], new Color(accent.r, accent.g, accent.b, 0.46f), new Vector2(1.5f, -1.5f));

            ColorBlock colors = optionButtons[i].colors;
            colors.normalColor = buttonBase;
            colors.highlightedColor = buttonHighlight;
            colors.selectedColor = buttonHighlight;
            colors.pressedColor = Color.Lerp(buttonBase, Color.black, 0.18f);
            colors.fadeDuration = 0.08f;
            optionButtons[i].colors = colors;

            optionBadgeImages[i].sprite = GetUpgradeBadge(choice.type);
            optionBadgeImages[i].color = Color.white;
            optionIcons[i].sprite = choice.icon != null ? choice.icon : GetUpgradeBadge(choice.type);
            optionIcons[i].color = Color.white;
            optionTypeTexts[i].text = GetUpgradeCategory(choice.type);
            optionTypeTexts[i].color = Color.Lerp(accent, Color.white, 0.28f);
            optionTitles[i].text = choice.title;
            optionDescriptions[i].text = choice.description;
            optionHintTexts[i].text = "SELECT  |  PRESS " + (i + 1);
        }
    }

    static bool WasChoicePressed(Keyboard keyboard, int index)
    {
        return index switch
        {
            0 => keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame,
            1 => keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame,
            2 => keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame,
            _ => false,
        };
    }

    static string GetUpgradeCategory(RunUpgradeType type)
    {
        return type switch
        {
            RunUpgradeType.Repair => "RECOVERY",
            RunUpgradeType.MaxHealth => "DEFENSE",
            RunUpgradeType.EngineBoost => "MOBILITY",
            RunUpgradeType.DamageBoost => "OFFENSE",
            RunUpgradeType.CooldownBoost => "WEAPON TUNE",
            RunUpgradeType.ScrapBurst => "RESOURCE",
            _ => "WEAPON UNLOCK",
        };
    }

    static Color GetUpgradeAccent(RunUpgradeType type)
    {
        return type switch
        {
            RunUpgradeType.Repair => new Color(0.24f, 0.82f, 0.4f),
            RunUpgradeType.MaxHealth => new Color(0.18f, 0.72f, 0.78f),
            RunUpgradeType.EngineBoost => new Color(0.32f, 0.7f, 1f),
            RunUpgradeType.DamageBoost => new Color(1f, 0.52f, 0.18f),
            RunUpgradeType.CooldownBoost => new Color(0.96f, 0.8f, 0.24f),
            RunUpgradeType.MissileUnlock => new Color(0.94f, 0.3f, 0.28f),
            RunUpgradeType.BeamUnlock => new Color(0.24f, 0.85f, 0.9f),
            RunUpgradeType.ChargeUnlock => new Color(0.5f, 0.58f, 1f),
            RunUpgradeType.PiercerUnlock => new Color(0.3f, 0.96f, 0.58f),
            RunUpgradeType.ScrapBurst => new Color(0.96f, 0.72f, 0.28f),
            _ => new Color(0.38f, 0.62f, 1f),
        };
    }

    static Sprite GetUpgradeBadge(RunUpgradeType type)
    {
        return type switch
        {
            RunUpgradeType.Repair => KenneyAssets.HealthIcon,
            RunUpgradeType.MaxHealth => KenneyAssets.ShieldIcon,
            RunUpgradeType.CooldownBoost => KenneyAssets.RapidFireIcon,
            RunUpgradeType.EngineBoost => KenneyAssets.SpreadShotBadge,
            RunUpgradeType.BeamUnlock => KenneyAssets.BeamBadge,
            RunUpgradeType.MissileUnlock => KenneyAssets.MissileBadge,
            RunUpgradeType.ChargeUnlock => KenneyAssets.ChargeBadge,
            RunUpgradeType.PiercerUnlock => KenneyAssets.PierceBadge,
            RunUpgradeType.ScrapBurst => KenneyAssets.ScrapBadge,
            _ => KenneyAssets.RapidFireBadge,
        };
    }

    GameObject CreatePanel(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, Color fillColor, Color outlineColor, Vector2 shadowDistance,
        bool accent, float accentAlpha)
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
        GameUiStyle.ApplyPanelStyle(image, size, fillColor, outlineColor, shadowDistance, accent, accentAlpha);
        return obj;
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
        image.preserveAspect = false;
        image.raycastTarget = false;
        return image;
    }

    Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 anchoredPosition, Color color)
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

    Text CreateText(Transform parent, string name, string value, int fontSize, FontStyle fontStyle, Color color,
        TextAnchor alignment, Vector2 size, Vector2 anchoredPosition, bool headline)
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
        GameUiStyle.StyleText(text, fontSize, color, alignment, fontStyle, headline);
        return text;
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
}
