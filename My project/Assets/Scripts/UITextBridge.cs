using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Restyles and updates the legacy score/speed texts into a compact top-right HUD card.
/// </summary>
public class UITextBridge : MonoBehaviour
{
    Text scoreText;
    Text speedText;
    Text finalScoreText;
    Text headerText;
    GameObject scorePanel;
    int lastScore = -1;
    float lastSpeed = -1f;
    string lastDifficulty = string.Empty;
    string lastMode = string.Empty;

    void Start()
    {
        scoreText = FindTextByName("ScoreText");
        speedText = FindTextByName("SpeedText");
        finalScoreText = FindTextByName("FinalScoreText");
        EnsureHudLayout();
    }

    void Update()
    {
        if (GameManager.Instance == null)
            return;

        EnsureHudLayout();

        if (scoreText != null && GameManager.Instance.Score != lastScore)
        {
            lastScore = GameManager.Instance.Score;
            scoreText.text = "Score: " + lastScore;
        }

        float spd = GameManager.Instance.ScrollSpeed;
        string difficulty = GameManager.Instance.SelectedDifficulty.ToString();
        string mode = GameManager.Instance.SelectedModeLabel;
        if (speedText != null && (Mathf.Abs(spd - lastSpeed) > 0.05f || difficulty != lastDifficulty || mode != lastMode))
        {
            lastSpeed = spd;
            lastDifficulty = difficulty;
            lastMode = mode;
            speedText.text = "Speed " + spd.ToString("F1") + "  |  " + difficulty;
        }

        if (GameManager.Instance.IsGameOver && finalScoreText != null)
            finalScoreText.text = "Final Score: " + GameManager.Instance.Score;
    }

    void EnsureHudLayout()
    {
        if (scoreText == null || speedText == null)
            return;

        if (scorePanel == null)
        {
            Transform existing = transform.Find("ScoreHudPanel");
            scorePanel = existing != null ? existing.gameObject : new GameObject("ScoreHudPanel");
            scorePanel.transform.SetParent(transform, false);

            RectTransform panelRect = scorePanel.GetComponent<RectTransform>();
            if (panelRect == null)
                panelRect = scorePanel.AddComponent<RectTransform>();

            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.sizeDelta = new Vector2(292f, 102f);
            panelRect.anchoredPosition = new Vector2(-22f, -18f);

            Image panelImage = scorePanel.GetComponent<Image>();
            if (panelImage == null)
                panelImage = scorePanel.AddComponent<Image>();

            GameUiStyle.ApplyPanelStyle(panelImage, panelRect.sizeDelta, GameUiStyle.PanelFill, GameUiStyle.OutlineBlue, new Vector2(0f, -4f), true, 0.08f);

            GameObject headerObj = new GameObject("HeaderText");
            headerObj.transform.SetParent(scorePanel.transform, false);
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(1f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(1f, 1f);
            headerRect.sizeDelta = new Vector2(240f, 18f);
            headerRect.anchoredPosition = new Vector2(-18f, -12f);
            headerText = headerObj.AddComponent<Text>();
            headerText.text = "RUN SCORE";
            GameUiStyle.StyleText(headerText, 13, GameUiStyle.TextMuted, TextAnchor.UpperRight, FontStyle.Bold, false);
        }

        StyleScoreText(scoreText, new Vector2(250f, 42f), new Vector2(-18f, -28f), 36, GameUiStyle.TextPrimary, true);
        StyleScoreText(speedText, new Vector2(250f, 24f), new Vector2(-18f, -66f), 17, GameUiStyle.TextMuted, false);
    }

    void StyleScoreText(Text text, Vector2 size, Vector2 anchoredPosition, int fontSize, Color color, bool headline)
    {
        if (text == null || scorePanel == null)
            return;

        text.transform.SetParent(scorePanel.transform, false);
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        GameUiStyle.StyleText(text, fontSize, color, TextAnchor.UpperRight, headline ? FontStyle.Bold : FontStyle.Normal, headline);
    }

    Text FindTextByName(string name)
    {
        Text[] allTexts = GetComponentsInChildren<Text>(true);
        for (int i = 0; i < allTexts.Length; i++)
        {
            if (allTexts[i].gameObject.name == name)
                return allTexts[i];
        }

        return null;
    }
}
