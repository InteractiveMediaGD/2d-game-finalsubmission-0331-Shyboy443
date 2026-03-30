using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared runtime UI styling helpers for readable fonts, rounded panels, and subtle sci-fi accents.
/// </summary>
public static class GameUiStyle
{
    static Font headlineFont;
    static Font bodyFont;

    public static readonly Color PanelFill = new Color(0.04f, 0.07f, 0.14f, 0.86f);
    public static readonly Color SubPanelFill = new Color(0.06f, 0.09f, 0.16f, 0.9f);
    public static readonly Color OutlineBlue = new Color(0.48f, 0.72f, 1f, 0.32f);
    public static readonly Color OutlineGold = new Color(0.98f, 0.78f, 0.28f, 0.24f);
    public static readonly Color AccentBlue = new Color(0.74f, 0.86f, 1f, 0.08f);
    public static readonly Color TextPrimary = new Color(0.96f, 0.98f, 1f, 0.98f);
    public static readonly Color TextMuted = new Color(0.8f, 0.88f, 0.98f, 0.92f);
    public static readonly Color TextWarm = new Color(1f, 0.88f, 0.48f, 0.98f);

    public static Font GetHeadlineFont(int size)
    {
        if (headlineFont == null)
            headlineFont = LoadFont(new[] { "Bahnschrift SemiBold", "Segoe UI Semibold", "Trebuchet MS Bold", "Verdana Bold", "Arial Bold" }, size);

        return headlineFont;
    }

    public static Font GetBodyFont(int size)
    {
        if (bodyFont == null)
            bodyFont = LoadFont(new[] { "Segoe UI", "Trebuchet MS", "Verdana", "Arial" }, size);

        return bodyFont;
    }

    public static void StyleText(Text text, int fontSize, Color color, TextAnchor alignment, FontStyle fontStyle, bool headline = false, bool addOutline = true)
    {
        if (text == null)
            return;

        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = fontStyle;
        text.lineSpacing = 1f;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.font = headline ? GetHeadlineFont(fontSize) : GetBodyFont(fontSize);
        text.raycastTarget = false;

        Outline outline = text.GetComponent<Outline>();
        if (addOutline)
        {
            if (outline == null)
                outline = text.gameObject.AddComponent<Outline>();

            outline.effectColor = new Color(0f, 0f, 0f, 0.48f);
            outline.effectDistance = new Vector2(1f, -1f);
        }
        else if (outline != null)
        {
            Object.Destroy(outline);
        }
    }

    public static void ApplyPanelStyle(Image image, Vector2 size, Color fillColor, Color outlineColor, Vector2 shadowDistance,
        bool addAccent = false, float accentAlpha = 0.08f)
    {
        if (image == null)
            return;

        image.sprite = SpriteHelper.RoundedRect;
        image.type = Image.Type.Sliced;
        image.color = fillColor;
        image.raycastTarget = false;

        AddOutline(image, outlineColor, new Vector2(1.5f, -1.5f));
        AddShadow(image, new Color(0f, 0f, 0f, 0.32f), shadowDistance);
        SetTopAccent(image.transform, size, addAccent, accentAlpha);
    }

    public static void ApplyBarStyle(Image image, Color fillColor)
    {
        if (image == null)
            return;

        image.sprite = SpriteHelper.RoundedRect;
        image.type = Image.Type.Sliced;
        image.color = fillColor;
        image.raycastTarget = false;
    }

    public static void AddOutline(Graphic graphic, Color color, Vector2 distance)
    {
        if (graphic == null)
            return;

        Outline outline = graphic.GetComponent<Outline>();
        if (outline == null)
            outline = graphic.gameObject.AddComponent<Outline>();

        outline.effectColor = color;
        outline.effectDistance = distance;
    }

    public static void AddShadow(Graphic graphic, Color color, Vector2 distance)
    {
        if (graphic == null)
            return;

        Shadow shadow = graphic.GetComponent<Shadow>();
        if (shadow == null)
            shadow = graphic.gameObject.AddComponent<Shadow>();

        shadow.effectColor = color;
        shadow.effectDistance = distance;
    }

    static void SetTopAccent(Transform parent, Vector2 parentSize, bool enabled, float alpha)
    {
        if (parent == null)
            return;

        Transform existing = parent.Find("TopAccent");
        if (!enabled || alpha <= 0f)
        {
            if (existing != null)
                Object.Destroy(existing.gameObject);
            return;
        }

        GameObject accentObj = existing != null ? existing.gameObject : new GameObject("TopAccent");
        accentObj.transform.SetParent(parent, false);

        RectTransform rect = accentObj.GetComponent<RectTransform>();
        if (rect == null)
            rect = accentObj.AddComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(Mathf.Max(0f, parentSize.x - 28f), Mathf.Clamp(parentSize.y * 0.11f, 8f, 18f));
        rect.anchoredPosition = new Vector2(0f, parentSize.y * 0.38f);

        Image accent = accentObj.GetComponent<Image>();
        if (accent == null)
            accent = accentObj.AddComponent<Image>();

        accent.sprite = SpriteHelper.RoundedRect;
        accent.type = Image.Type.Sliced;
        accent.color = new Color(AccentBlue.r, AccentBlue.g, AccentBlue.b, alpha);
        accent.raycastTarget = false;
    }

    static Font LoadFont(string[] osFontNames, int size)
    {
        Font font = null;

        if (osFontNames != null && osFontNames.Length > 0)
            font = Font.CreateDynamicFontFromOSFont(osFontNames, size);

        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Font.CreateDynamicFontFromOSFont("Arial", size);

        return font;
    }
}
