using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// First-time tutorial card shown on top of the start menu.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class TutorialOverlayUI : MonoBehaviour
{
    GameObject panel;

    void Awake()
    {
        EnsureBuilt();
    }

    public void MaybeShowFirstTime()
    {
        EnsureBuilt();
        panel.SetActive(!SaveProfile.TutorialSeen);
    }

    public void Hide()
    {
        EnsureBuilt();
        panel.SetActive(false);
    }

    void EnsureBuilt()
    {
        if (panel != null)
            return;

        Transform existing = FindDeep(transform, "TutorialOverlayPanel");
        if (existing != null)
        {
            panel = existing.gameObject;
            return;
        }

        panel = new GameObject("TutorialOverlayPanel");
        panel.transform.SetParent(transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760f, 280f);
        panelRect.anchoredPosition = new Vector2(0f, -240f);

        Image background = panel.AddComponent<Image>();
        background.sprite = SpriteHelper.Square;
        background.color = new Color(0.05f, 0.07f, 0.12f, 0.95f);

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.36f, 0.55f, 0.86f, 0.45f);
        outline.effectDistance = new Vector2(3f, 3f);

        CreateText(panel.transform, "TutorialTitle", "FIRST FLIGHT", 28, FontStyle.Bold,
            new Vector2(440f, 40f), new Vector2(0f, 86f), TextAnchor.MiddleCenter, new Color(0.96f, 0.98f, 1f));
        CreateText(panel.transform, "TutorialBody",
            "1-5 switch unlocked weapons\nCollect scrap and upgrades between sectors\nDestroy bosses to unlock stronger permanent loadouts",
            22, FontStyle.Normal, new Vector2(600f, 96f), new Vector2(0f, 16f), TextAnchor.MiddleCenter,
            new Color(0.84f, 0.9f, 1f, 0.94f));

        Button dismiss = CreateButton(panel.transform, "TutorialContinue", "CONTINUE", new Vector2(220f, 52f),
            new Vector2(0f, -82f), new Color(0.18f, 0.68f, 0.42f), new Color(0.24f, 0.8f, 0.5f));
        dismiss.onClick.AddListener(() => GameManager.Instance?.DismissTutorial());

        panel.SetActive(false);
    }

    Text CreateText(Transform parent, string name, string value, int fontSize, FontStyle fontStyle,
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

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.font = font;
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
        image.sprite = SpriteHelper.Square;
        image.color = normalColor;

        Button button = obj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = highlightedColor;
        colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.2f);
        button.colors = colors;

        CreateText(obj.transform, "Label", label, 24, FontStyle.Bold, size, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
        return button;
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
