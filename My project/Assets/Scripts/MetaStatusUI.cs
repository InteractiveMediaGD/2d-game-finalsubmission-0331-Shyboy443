using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Compact run information panels for mode, timer, weapon, scrap, objectives, and achievement callouts.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class MetaStatusUI : MonoBehaviour
{
    GameObject infoPanel;
    GameObject objectivePanel;
    Text modeText;
    Text timerText;
    Text weaponText;
    Text scrapText;
    Text objectiveText;
    Text achievementText;

    void Awake()
    {
        EnsureBuilt();
    }

    void Update()
    {
        EnsureBuilt();

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
            return;

        PlayerShooting shooting = FindAnyObjectByType<PlayerShooting>();
        bool hudVisible = gameManager.HasStarted && !gameManager.IsGameOver && !gameManager.IsVictory;

        if (infoPanel != null)
            infoPanel.SetActive(hudVisible);
        if (objectivePanel != null)
            objectivePanel.SetActive(hudVisible && gameManager.HasObjective);
        if (achievementText != null)
            achievementText.gameObject.SetActive(gameManager.HasAchievementBanner);

        if (!hudVisible)
            return;

        modeText.text = gameManager.SelectedModeLabel + " // " + gameManager.SelectedDifficulty;
        timerText.text = gameManager.HasModeTimer
            ? "Time Left  " + gameManager.ModeTimerRemaining.ToString("F1") + "s"
            : "Speed  " + gameManager.ScrollSpeed.ToString("F1");
        weaponText.text = "Weapon  " + (shooting != null ? shooting.CurrentWeaponLabel : "Pulse");
        scrapText.text = "Scrap  " + gameManager.ScrapThisRun;
        objectiveText.text = gameManager.ObjectiveText;

        if (gameManager.HasAchievementBanner)
        {
            Color color = achievementText.color;
            color.a = gameManager.AchievementBannerAlpha;
            achievementText.color = color;
            achievementText.text = gameManager.AchievementBannerText;
        }
    }

    void EnsureBuilt()
    {
        if (modeText != null && timerText != null && weaponText != null && scrapText != null && objectiveText != null && achievementText != null)
            return;

        DestroyIfExists("MetaInfoPanel");
        DestroyIfExists("ObjectivePanel");
        DestroyIfExists("MetaModeText");
        DestroyIfExists("MetaTimerText");
        DestroyIfExists("MetaWeaponText");
        DestroyIfExists("MetaScrapText");
        DestroyIfExists("MetaObjectiveText");
        DestroyIfExists("AchievementToastText");

        infoPanel = CreatePanel("MetaInfoPanel", new Vector2(292f, 114f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-22f, -124f), GameUiStyle.SubPanelFill, GameUiStyle.OutlineBlue, true);
        modeText = CreateText(infoPanel.transform, "MetaModeText", string.Empty, 17, FontStyle.Bold, GameUiStyle.TextPrimary,
            TextAnchor.UpperRight, new Vector2(236f, 20f), new Vector2(-18f, -14f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), false);
        timerText = CreateText(infoPanel.transform, "MetaTimerText", string.Empty, 16, FontStyle.Bold, GameUiStyle.TextWarm,
            TextAnchor.UpperRight, new Vector2(236f, 20f), new Vector2(-18f, -40f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), false);
        weaponText = CreateText(infoPanel.transform, "MetaWeaponText", string.Empty, 15, FontStyle.Normal, GameUiStyle.TextMuted,
            TextAnchor.UpperRight, new Vector2(236f, 18f), new Vector2(-18f, -64f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), false);
        scrapText = CreateText(infoPanel.transform, "MetaScrapText", string.Empty, 15, FontStyle.Bold, new Color(1f, 0.86f, 0.4f, 0.98f),
            TextAnchor.UpperRight, new Vector2(236f, 18f), new Vector2(-18f, -86f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), false);

        objectivePanel = CreatePanel("ObjectivePanel", new Vector2(560f, 42f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 74f), new Color(0.06f, 0.1f, 0.18f, 0.9f), GameUiStyle.OutlineGold, false);
        objectiveText = CreateText(objectivePanel.transform, "MetaObjectiveText", string.Empty, 17, FontStyle.Italic, GameUiStyle.TextPrimary,
            TextAnchor.MiddleCenter, new Vector2(520f, 24f), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), false);

        achievementText = CreateText(transform, "AchievementToastText", string.Empty, 24, FontStyle.Bold, GameUiStyle.TextWarm,
            TextAnchor.MiddleCenter, new Vector2(740f, 34f), new Vector2(0f, -76f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), true);
        achievementText.gameObject.SetActive(false);
    }

    GameObject CreatePanel(string name, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition,
        Color fillColor, Color outlineColor, bool accent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMax.x, anchorMax.y);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = obj.AddComponent<Image>();
        GameUiStyle.ApplyPanelStyle(image, size, fillColor, outlineColor, new Vector2(0f, -4f), accent, accent ? 0.08f : 0f);
        return obj;
    }

    Text CreateText(Transform parent, string name, string value, int fontSize, FontStyle fontStyle, Color color,
        TextAnchor alignment, Vector2 size, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, bool headline)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
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

    void DestroyIfExists(string name)
    {
        Transform existing = FindDeep(transform, name);
        if (existing != null)
            Destroy(existing.gameObject);
    }
}
