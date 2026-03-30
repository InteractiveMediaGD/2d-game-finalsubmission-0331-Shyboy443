using UnityEngine;

/// <summary>
/// Swaps Kenney backgrounds based on the active mode and sector.
/// </summary>
public class BackgroundThemeController : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Sprite lastSprite;

    public static void EnsureExists()
    {
        GameObject background = GameObject.Find("SpaceBackground");
        if (background == null)
            return;

        if (background.GetComponent<BackgroundThemeController>() == null)
            background.AddComponent<BackgroundThemeController>();
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            return;

        Sprite target = KenneyAssets.GetBackgroundForRun(
            GameManager.Instance != null ? GameManager.Instance.SelectedMode : GameManager.GameMode.Campaign,
            GameManager.Instance != null ? GameManager.Instance.CurrentStage : GameManager.RunStage.Level1);

        if (target == null || target == lastSprite)
            return;

        lastSprite = target;
        spriteRenderer.sprite = target;
        FitToCamera(target);
    }

    void FitToCamera(Sprite sprite)
    {
        Camera cam = Camera.main;
        if (cam == null || sprite == null)
            return;

        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;
        if (spriteWidth <= 0f || spriteHeight <= 0f)
            return;

        transform.localScale = new Vector3(width / spriteWidth + 1f, height / spriteHeight + 1f, 1f);
    }
}
