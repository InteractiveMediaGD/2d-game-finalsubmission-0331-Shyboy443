using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows active power-ups as readable status chips beneath the health card.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class PowerUpStatusUI : MonoBehaviour
{
    struct PowerUpSlot
    {
        public PlayerPowerUps.PowerUpType type;
        public GameObject root;
        public Image panelImage;
        public Image badgeImage;
        public Image iconImage;
        public Text nameText;
        public Text timerText;
    }

    GameObject panel;
    PowerUpSlot[] slots;
    PlayerPowerUps playerPowerUps;

    void Awake()
    {
        EnsurePanel();
    }

    void Update()
    {
        EnsurePanel();

        if (GameManager.Instance == null)
            return;

        if (playerPowerUps == null)
            playerPowerUps = FindAnyObjectByType<PlayerPowerUps>();

        bool anyVisible = false;
        for (int i = 0; i < slots.Length; i++)
        {
            bool visible = playerPowerUps != null && playerPowerUps.IsActive(slots[i].type) && GameManager.Instance.IsGameplayActive;
            slots[i].root.SetActive(visible);

            if (!visible)
                continue;

            anyVisible = true;
            slots[i].timerText.text = playerPowerUps.GetTimeRemaining(slots[i].type).ToString("F1") + "s";
        }

        panel.SetActive(anyVisible);
    }

    void EnsurePanel()
    {
        if (panel != null && slots != null && slots.Length == 3)
            return;

        panel = FindDeep(transform, "PowerUpStatusPanel")?.gameObject;
        if (panel != null)
            Destroy(panel);

        panel = new GameObject("PowerUpStatusPanel");
        panel.transform.SetParent(transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.sizeDelta = new Vector2(390f, 56f);
        panelRect.anchoredPosition = new Vector2(22f, -110f);

        slots = new[]
        {
            CreateSlot(panel.transform, "ShieldSlot", PlayerPowerUps.PowerUpType.Shield, 0f),
            CreateSlot(panel.transform, "RapidFireSlot", PlayerPowerUps.PowerUpType.RapidFire, 132f),
            CreateSlot(panel.transform, "SpreadShotSlot", PlayerPowerUps.PowerUpType.SpreadShot, 264f),
        };

        panel.SetActive(false);
    }

    PowerUpSlot CreateSlot(Transform parent, string name, PlayerPowerUps.PowerUpType type, float x)
    {
        GameObject slot = new GameObject(name);
        slot.transform.SetParent(parent, false);

        RectTransform slotRect = slot.AddComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0f, 1f);
        slotRect.anchorMax = new Vector2(0f, 1f);
        slotRect.pivot = new Vector2(0f, 1f);
        slotRect.sizeDelta = new Vector2(124f, 52f);
        slotRect.anchoredPosition = new Vector2(x, 0f);

        Image panelImage = slot.AddComponent<Image>();
        GameUiStyle.ApplyPanelStyle(panelImage, slotRect.sizeDelta, new Color(0.06f, 0.1f, 0.18f, 0.88f), new Color(0.44f, 0.68f, 1f, 0.24f), new Vector2(0f, -3f), false, 0f);

        Image badge = CreateImage(slot.transform, "Badge", GetBadgeSprite(type), new Vector2(32f, 32f), new Vector2(18f, -26f));
        Image icon = CreateImage(slot.transform, "Icon", GetIconSprite(type), new Vector2(20f, 20f), new Vector2(18f, -26f));
        Text nameText = CreateText(slot.transform, "Name", GetDisplayName(type), 13, FontStyle.Bold, GameUiStyle.TextMuted,
            TextAnchor.MiddleLeft, new Vector2(74f, 16f), new Vector2(44f, -16f));
        Text timerText = CreateText(slot.transform, "Timer", string.Empty, 18, FontStyle.Bold, GameUiStyle.TextPrimary,
            TextAnchor.MiddleLeft, new Vector2(74f, 22f), new Vector2(44f, -35f));

        return new PowerUpSlot
        {
            type = type,
            root = slot,
            panelImage = panelImage,
            badgeImage = badge,
            iconImage = icon,
            nameText = nameText,
            timerText = timerText,
        };
    }

    Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = obj.AddComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    Text CreateText(Transform parent, string name, string value, int fontSize, FontStyle fontStyle, Color color,
        TextAnchor alignment, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Text text = obj.AddComponent<Text>();
        text.text = value;
        GameUiStyle.StyleText(text, fontSize, color, alignment, fontStyle, fontStyle == FontStyle.Bold && fontSize >= 16);
        return text;
    }

    static string GetDisplayName(PlayerPowerUps.PowerUpType type)
    {
        return type switch
        {
            PlayerPowerUps.PowerUpType.RapidFire => "Rapid Fire",
            PlayerPowerUps.PowerUpType.SpreadShot => "Spread Shot",
            _ => "Shield",
        };
    }

    static Sprite GetBadgeSprite(PlayerPowerUps.PowerUpType type)
    {
        return type switch
        {
            PlayerPowerUps.PowerUpType.Shield => KenneyAssets.ShieldBadge,
            PlayerPowerUps.PowerUpType.RapidFire => KenneyAssets.RapidFireBadge,
            PlayerPowerUps.PowerUpType.SpreadShot => KenneyAssets.SpreadShotBadge,
            _ => SpriteHelper.Circle,
        };
    }

    static Sprite GetIconSprite(PlayerPowerUps.PowerUpType type)
    {
        return type switch
        {
            PlayerPowerUps.PowerUpType.Shield => KenneyAssets.ShieldIcon,
            PlayerPowerUps.PowerUpType.RapidFire => KenneyAssets.RapidFireIcon,
            PlayerPowerUps.PowerUpType.SpreadShot => KenneyAssets.SpreadShotIcon,
            _ => SpriteHelper.Circle,
        };
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
