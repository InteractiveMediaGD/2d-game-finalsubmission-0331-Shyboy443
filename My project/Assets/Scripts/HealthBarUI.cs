using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Modern health card that keeps the fill shrinking from right to left while restyling legacy scene UI at runtime.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;
    public Text healthNumberText;

    Gradient healthGradient;
    RectTransform fillRect;
    RectTransform trackRect;
    Image trackImage;
    Image iconImage;
    Text titleText;
    float fullWidth;
    bool visualsPrepared;

    void Awake()
    {
        healthGradient = new Gradient();
        healthGradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.92f, 0.22f, 0.2f), 0f),
                new GradientColorKey(new Color(1f, 0.62f, 0.16f), 0.38f),
                new GradientColorKey(new Color(1f, 0.88f, 0.22f), 0.62f),
                new GradientColorKey(new Color(0.24f, 0.92f, 0.46f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });

        AutoBindReferences();
        PrepareVisuals();
    }

    void Start()
    {
        PrepareVisuals();
    }

    public void UpdateHealth(int current, int max)
    {
        PrepareVisuals();

        float ratio = max > 0 ? (float)current / max : 0f;
        ratio = Mathf.Clamp01(ratio);

        if (fillRect != null)
        {
            Vector2 size = fillRect.sizeDelta;
            size.x = fullWidth * ratio;
            fillRect.sizeDelta = size;
        }

        if (fillImage != null)
            fillImage.color = healthGradient.Evaluate(ratio);

        if (healthNumberText != null)
            healthNumberText.text = current + " / " + max;
    }

    void PrepareVisuals()
    {
        if (visualsPrepared)
            return;

        AutoBindReferences();
        DestroyLegacyChild("HPLabel");

        RectTransform rootRect = transform as RectTransform;
        Image rootImage = GetComponent<Image>();
        if (rootRect == null || rootImage == null)
            return;

        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.sizeDelta = new Vector2(368f, 82f);
        rootRect.anchoredPosition = new Vector2(22f, -18f);
        GameUiStyle.ApplyPanelStyle(rootImage, rootRect.sizeDelta, GameUiStyle.PanelFill, GameUiStyle.OutlineBlue, new Vector2(0f, -4f), true, 0.08f);

        EnsureImage("HealthIconBadge", new Vector2(34f, 34f), new Vector2(22f, -22f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), KenneyAssets.HealthBadge, Color.white);
        iconImage = EnsureImage("HealthIcon", new Vector2(20f, 20f), new Vector2(22f, -22f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), KenneyAssets.HealthIcon, Color.white);
        titleText = EnsureText("HealthTitle", "HULL INTEGRITY", 18, FontStyle.Bold, GameUiStyle.TextPrimary,
            TextAnchor.MiddleLeft, new Vector2(132f, 22f), new Vector2(60f, -18f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), true);
        healthNumberText = EnsureText("HealthNumber", healthNumberText != null ? healthNumberText.text : "5 / 5", 17, FontStyle.Bold, GameUiStyle.TextWarm,
            TextAnchor.MiddleRight, new Vector2(80f, 22f), new Vector2(-18f, -18f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), false);

        trackImage = EnsureImage("HealthBarTrack", new Vector2(324f, 18f), new Vector2(22f, -54f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
            SpriteHelper.RoundedRect, new Color(0.12f, 0.16f, 0.24f, 0.96f));
        GameUiStyle.ApplyBarStyle(trackImage, new Color(0.1f, 0.13f, 0.2f, 0.98f));
        GameUiStyle.AddOutline(trackImage, new Color(1f, 1f, 1f, 0.08f), new Vector2(1f, -1f));

        if (fillImage == null)
            fillImage = EnsureImage("HealthBarFill", Vector2.zero, Vector2.zero, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), SpriteHelper.RoundedRect, Color.green);

        fillImage.transform.SetParent(trackImage.transform, false);
        fillRect = fillImage.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0.5f);
        fillRect.anchorMax = new Vector2(0f, 0.5f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(trackImage.rectTransform.sizeDelta.x, 18f);
        fillImage.sprite = SpriteHelper.RoundedRect;
        fillImage.type = Image.Type.Sliced;

        trackRect = trackImage.rectTransform;
        fullWidth = trackRect.sizeDelta.x;
        visualsPrepared = true;
    }

    void AutoBindReferences()
    {
        if (fillImage == null)
        {
            Transform fill = transform.Find("HealthBarFill");
            if (fill != null)
                fillImage = fill.GetComponent<Image>();
        }

        if (healthNumberText == null)
        {
            Transform number = transform.Find("HealthNumber");
            if (number != null)
                healthNumberText = number.GetComponent<Text>();
        }
    }

    Image EnsureImage(string name, Vector2 size, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Sprite sprite, Color color)
    {
        Transform existing = transform.Find(name);
        GameObject obj = existing != null ? existing.gameObject : new GameObject(name);
        obj.transform.SetParent(transform, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null)
            rect = obj.AddComponent<RectTransform>();

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = obj.GetComponent<Image>();
        if (image == null)
            image = obj.AddComponent<Image>();

        image.sprite = sprite;
        image.color = color;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    Text EnsureText(string name, string value, int size, FontStyle fontStyle, Color color, TextAnchor alignment,
        Vector2 rectSize, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, bool headline)
    {
        Transform existing = transform.Find(name);
        GameObject obj = existing != null ? existing.gameObject : new GameObject(name);
        obj.transform.SetParent(transform, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null)
            rect = obj.AddComponent<RectTransform>();

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = rectSize;
        rect.anchoredPosition = anchoredPosition;

        Text text = obj.GetComponent<Text>();
        if (text == null)
            text = obj.AddComponent<Text>();

        text.text = value;
        GameUiStyle.StyleText(text, size, color, alignment, fontStyle, headline);
        return text;
    }

    void DestroyLegacyChild(string name)
    {
        Transform child = transform.Find(name);
        if (child != null)
            Destroy(child.gameObject);
    }
}
