using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Top-center run HUD for stage labels, transition banners, and boss health.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class RunStatusUI : MonoBehaviour
{
    GameObject stagePanel;
    Text stageLabel;
    Text bannerLabel;
    GameObject bossPanel;
    Image bossFill;
    Text bossNameText;
    Text bossHealthText;

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

        bool showStage = gameManager.IsGameplayActive;
        stagePanel.SetActive(showStage);
        if (showStage)
            stageLabel.text = gameManager.CurrentStageLabel;

        bool showBanner = gameManager.HasStageBanner && gameManager.IsGameplayActive;
        bannerLabel.gameObject.SetActive(showBanner);
        if (showBanner)
        {
            Color color = bannerLabel.color;
            color.a = Mathf.Lerp(0f, 1f, gameManager.StageBannerAlpha);
            bannerLabel.color = color;
            bannerLabel.text = gameManager.StageBannerText;
        }

        bool showBoss = gameManager.IsGameplayActive && gameManager.IsBossFightActive && gameManager.HasBossHealth;
        bossPanel.SetActive(showBoss);
        if (showBoss)
        {
            bossFill.fillAmount = gameManager.BossHealthRatio;
            bossNameText.text = gameManager.BossName;
            bossHealthText.text = gameManager.BossHealthText;
        }
    }

    void EnsureBuilt()
    {
        if (stageLabel != null && bannerLabel != null && bossPanel != null && bossFill != null)
            return;

        DestroyIfExists("StagePanel");
        DestroyIfExists("StageLabel");
        DestroyIfExists("StageBanner");
        DestroyIfExists("BossHealthPanel");

        stagePanel = CreatePanel("StagePanel", new Vector2(360f, 36f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -14f), new Color(0.06f, 0.1f, 0.17f, 0.88f), GameUiStyle.OutlineBlue, false);
        stageLabel = CreateText(stagePanel.transform, "StageLabel", string.Empty, 18, FontStyle.Bold, GameUiStyle.TextPrimary,
            TextAnchor.MiddleCenter, new Vector2(320f, 22f), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), false);

        bannerLabel = CreateText(transform, "StageBanner", string.Empty, 30, FontStyle.Bold, GameUiStyle.TextWarm,
            TextAnchor.MiddleCenter, new Vector2(760f, 56f), new Vector2(0f, -94f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), true);
        bannerLabel.gameObject.SetActive(false);

        bossPanel = CreatePanel("BossHealthPanel", new Vector2(540f, 64f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -54f), new Color(0.11f, 0.07f, 0.1f, 0.92f), new Color(0.96f, 0.42f, 0.3f, 0.28f), true);
        bossNameText = CreateText(bossPanel.transform, "BossName", string.Empty, 16, FontStyle.Bold, new Color(1f, 0.86f, 0.72f, 1f),
            TextAnchor.UpperLeft, new Vector2(180f, 18f), new Vector2(18f, -10f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), false);
        bossHealthText = CreateText(bossPanel.transform, "BossHealthText", string.Empty, 15, FontStyle.Bold, GameUiStyle.TextPrimary,
            TextAnchor.UpperRight, new Vector2(140f, 18f), new Vector2(-18f, -10f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), false);

        Image fillBackground = CreateImage(bossPanel.transform, "BossFillBackground", SpriteHelper.RoundedRect, new Color(0.18f, 0.12f, 0.15f, 1f),
            new Vector2(0f, 10f), new Vector2(500f, 18f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        GameUiStyle.ApplyBarStyle(fillBackground, new Color(0.18f, 0.12f, 0.15f, 1f));

        bossFill = CreateImage(fillBackground.transform, "BossFill", SpriteHelper.RoundedRect, new Color(0.98f, 0.32f, 0.22f, 1f),
            Vector2.zero, new Vector2(500f, 18f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
        bossFill.type = Image.Type.Filled;
        bossFill.fillMethod = Image.FillMethod.Horizontal;
        bossFill.fillOrigin = 0;
        bossFill.fillAmount = 1f;

        bossPanel.SetActive(false);
    }

    GameObject CreatePanel(string name, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition,
        Color fillColor, Color outlineColor, bool accent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = obj.AddComponent<Image>();
        GameUiStyle.ApplyPanelStyle(image, size, fillColor, outlineColor, new Vector2(0f, -4f), accent, accent ? 0.08f : 0f);
        return obj;
    }

    Text CreateText(Transform parent, string name, string value, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment,
        Vector2 size, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, bool headline)
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

    Image CreateImage(Transform parent, string name, Sprite sprite, Color color, Vector2 anchoredPosition, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = obj.AddComponent<Image>();

        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = false;
        return image;
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
