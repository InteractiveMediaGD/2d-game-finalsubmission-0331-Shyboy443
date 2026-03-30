using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Central runtime controller for combat, progression, modes, upgrades, UI, and meta progression.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameDifficulty
    {
        Easy,
        Medium,
        Hard,
    }

    public enum GameMode
    {
        Campaign,
        Endless,
        TimeAttack,
        Challenge,
    }

    public enum RunStage
    {
        Level1,
        Level2,
        Level3,
        Boss,
        Victory,
    }

    public class UpgradeChoice
    {
        public RunUpgradeType type;
        public string title;
        public string description;
        public Sprite icon;
    }

    const float StageBannerDuration = 3f;
    const float AchievementBannerDuration = 2.6f;
    const int BaseLevel2ScoreTarget = 12;
    const int BaseLevel3ScoreTarget = 28;
    const int BaseBossScoreTarget = 50;

    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float initialScrollSpeed = 4f;
    public float speedIncreaseRate = 0.05f;
    public float maxScrollSpeed = 12f;
    public int maxHealth = 5;
    public GameDifficulty defaultDifficulty = GameDifficulty.Medium;
    public GameMode defaultMode = GameMode.Campaign;
    public float timeAttackDuration = 120f;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI speedText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;
    public Button restartButton;

    [Header("Wall Colors (changes with health)")]
    public Color wallColorHealthy = new Color(0.3f, 0.3f, 0.4f);
    public Color wallColorDanger = new Color(0.6f, 0.15f, 0.15f);

    int score;
    int scrapThisRun;
    int enemiesDestroyedThisRun;
    int bossKillsThisRun;
    int upgradesTakenThisRun;
    int nextLevel2Target;
    int nextLevel3Target;
    int nextBossTarget;
    float scrollSpeed;
    float stageBannerTimer;
    float achievementBannerTimer;
    float modeTimerRemaining;
    bool gameOver;
    bool victory;
    bool paused;
    bool hasStarted;
    bool upgradeSelectionActive;
    string stageBannerText;
    string achievementBannerText;
    string objectiveText;
    PlayerController playerRef;
    PlayerShooting playerShooting;
    GameMenuUI menuUI;
    EndRunOverlayUI endRunOverlayUI;
    GameDifficulty selectedDifficulty;
    GameMode selectedMode;
    RunStage currentStage;
    BossEnemy activeBoss;
    BossEnemy.BossType pendingBossType;
    UpgradeChoice[] currentUpgradeChoices = new UpgradeChoice[0];

    public float ScrollSpeed => scrollSpeed;
    public bool IsGameOver => gameOver;
    public bool IsVictory => victory;
    public bool IsPaused => paused;
    public bool HasStarted => hasStarted;
    public bool IsUpgradeSelectionOpen => upgradeSelectionActive;
    public bool IsGameplayActive => hasStarted && !paused && !gameOver && !victory && !upgradeSelectionActive;
    public bool IsBossFightActive => currentStage == RunStage.Boss && activeBoss != null && !gameOver && !victory;
    public bool AllowsWorldSpawns => IsGameplayActive && currentStage != RunStage.Boss;
    public int Score => score;
    public int ScrapThisRun => scrapThisRun;
    public int EnemiesDestroyedThisRun => enemiesDestroyedThisRun;
    public int BossKillsThisRun => bossKillsThisRun;
    public int UpgradesTakenThisRun => upgradesTakenThisRun;
    public float ModeTimerRemaining => modeTimerRemaining;
    public bool HasModeTimer => selectedMode == GameMode.TimeAttack;
    public bool HasObjective => !string.IsNullOrEmpty(objectiveText);
    public string ObjectiveText => objectiveText;
    public bool HasAchievementBanner => achievementBannerTimer > 0f && !string.IsNullOrEmpty(achievementBannerText);
    public string AchievementBannerText => achievementBannerText;
    public float AchievementBannerAlpha => Mathf.Clamp01(achievementBannerTimer / AchievementBannerDuration);
    public Color WallColor { get; private set; }
    public GameDifficulty SelectedDifficulty => selectedDifficulty;
    public GameMode SelectedMode => selectedMode;
    public string SelectedModeLabel => selectedMode switch
    {
        GameMode.Campaign => "Campaign",
        GameMode.Endless => "Endless",
        GameMode.TimeAttack => "Time Attack",
        GameMode.Challenge => "Challenge",
        _ => "Run",
    };
    public RunStage CurrentStage => currentStage;
    public string CurrentStageLabel => currentStage == RunStage.Boss ? "BOSS  |  " + BossName : GetStageLabel(currentStage);
    public string CurrentStageName => GetStageName(currentStage);
    public int CurrentLevelNumber => currentStage switch
    {
        RunStage.Level1 => 1,
        RunStage.Level2 => 2,
        RunStage.Level3 => 3,
        RunStage.Boss => 4,
        _ => 0,
    };
    public bool EnemiesCanShoot => selectedMode == GameMode.Challenge
        || selectedMode == GameMode.Endless
        || selectedMode == GameMode.TimeAttack
        || selectedDifficulty != GameDifficulty.Easy;
    public bool EnemiesCanStrafe => selectedMode == GameMode.Challenge || selectedMode == GameMode.Endless || (selectedDifficulty == GameDifficulty.Hard && currentStage != RunStage.Level1);
    public bool HasBossHealth => activeBoss != null && currentStage == RunStage.Boss;
    public float BossHealthRatio => activeBoss != null ? activeBoss.HealthRatio : 0f;
    public string BossHealthText => activeBoss != null ? activeBoss.CurrentHealth + " / " + activeBoss.MaxHealth : string.Empty;
    public string BossName => activeBoss != null ? activeBoss.BossDisplayName : BossEnemy.GetDisplayName(pendingBossType);
    public bool HasStageBanner => stageBannerTimer > 0f && !string.IsNullOrEmpty(stageBannerText);
    public string StageBannerText => stageBannerText;
    public float StageBannerAlpha => Mathf.Clamp01(stageBannerTimer / StageBannerDuration);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SaveProfile.Load();
        Time.timeScale = 1f;
        WallColor = wallColorHealthy;
        selectedDifficulty = defaultDifficulty;
        selectedMode = defaultMode;
        currentStage = RunStage.Level1;
        ResetRunTargets();
    }

    void Start()
    {
        scrollSpeed = GetInitialScrollSpeed();
        ResetRunFlags();
        UpdateScoreUI();
        UpdateSpeedUI();
        ResetEndPanelPresentation();

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
        }

        EnsureRuntimeSystems();
        OpenStartMenu(playSound: false);
    }

    void Update()
    {
        HandleGlobalInput();
        UpdateBannerTimers();

        if (!IsGameplayActive)
            return;

        UpdateTimedMode();

        scrollSpeed += GetSpeedIncreaseRate() * Time.deltaTime;
        scrollSpeed = Mathf.Min(scrollSpeed, GetMaxScrollSpeed());
        UpdateSpeedUI();

        if (playerRef == null)
            playerRef = FindAnyObjectByType<PlayerController>();

        if (playerShooting == null)
            playerShooting = FindAnyObjectByType<PlayerShooting>();

        if (playerRef != null)
        {
            float ratio = playerRef.MaxHealth > 0 ? (float)playerRef.CurrentHealth / playerRef.MaxHealth : 1f;
            WallColor = Color.Lerp(wallColorDanger, wallColorHealthy, ratio);
        }

        RefreshObjectiveText();
    }

    void HandleGlobalInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            return;

        if (gameOver || victory || !hasStarted || upgradeSelectionActive)
            return;

        if (paused)
            ResumeGame();
        else
            PauseGame();
    }

    void UpdateBannerTimers()
    {
        if (stageBannerTimer > 0f)
            stageBannerTimer = Mathf.Max(0f, stageBannerTimer - Time.unscaledDeltaTime);

        if (achievementBannerTimer > 0f)
            achievementBannerTimer = Mathf.Max(0f, achievementBannerTimer - Time.unscaledDeltaTime);
    }

    void UpdateTimedMode()
    {
        if (selectedMode != GameMode.TimeAttack)
            return;

        modeTimerRemaining = Mathf.Max(0f, modeTimerRemaining - Time.deltaTime);
        if (modeTimerRemaining <= 0f)
            TriggerGameOver("TIME UP", new Color(0.96f, 0.86f, 0.28f), "Score: " + score + "  |  Scrap: " + scrapThisRun);
    }

    void EnsureRuntimeSystems()
    {
        GameAudio.EnsureExists();

        Canvas hudCanvas = FindAnyObjectByType<Canvas>();
        if (hudCanvas != null)
        {
            if (hudCanvas.GetComponent<PowerUpStatusUI>() == null)
                hudCanvas.gameObject.AddComponent<PowerUpStatusUI>();

            if (hudCanvas.GetComponent<RunStatusUI>() == null)
                hudCanvas.gameObject.AddComponent<RunStatusUI>();

            if (hudCanvas.GetComponent<MetaStatusUI>() == null)
                hudCanvas.gameObject.AddComponent<MetaStatusUI>();

            if (hudCanvas.GetComponent<UpgradeSelectionUI>() == null)
                hudCanvas.gameObject.AddComponent<UpgradeSelectionUI>();

            if (hudCanvas.GetComponent<TutorialOverlayUI>() == null)
                hudCanvas.gameObject.AddComponent<TutorialOverlayUI>();

            if (hudCanvas.GetComponent<EndRunOverlayUI>() == null)
                hudCanvas.gameObject.AddComponent<EndRunOverlayUI>();
        }

        menuUI = FindAnyObjectByType<GameMenuUI>();
        if (menuUI == null && hudCanvas != null)
            menuUI = hudCanvas.gameObject.AddComponent<GameMenuUI>();

        if (menuUI != null)
            menuUI.Bind(this);

        endRunOverlayUI = hudCanvas != null
            ? hudCanvas.GetComponent<EndRunOverlayUI>()
            : FindAnyObjectByType<EndRunOverlayUI>();

        BackgroundThemeController.EnsureExists();
    }

    public void SetDifficulty(GameDifficulty difficulty)
    {
        selectedDifficulty = difficulty;
        scrollSpeed = GetInitialScrollSpeed();
        UpdateSpeedUI();
        RefreshObjectiveText();
    }

    public void SetMode(GameMode mode)
    {
        selectedMode = mode;
        UpdateSpeedUI();
        RefreshObjectiveText();
    }

    public void StartGame()
    {
        EnsureRuntimeSystems();
        ResetRunState();

        hasStarted = true;
        paused = false;
        gameOver = false;
        victory = false;
        upgradeSelectionActive = false;
        Time.timeScale = 1f;
        WallColor = wallColorHealthy;
        scrollSpeed = GetInitialScrollSpeed();
        modeTimerRemaining = selectedMode == GameMode.TimeAttack ? timeAttackDuration : 0f;
        playerRef = FindAnyObjectByType<PlayerController>();
        playerShooting = FindAnyObjectByType<PlayerShooting>();

        if (playerRef != null)
            playerRef.ResetForNewRun(selectedMode);
        if (playerShooting != null)
            playerShooting.ResetForNewRun();

        if (gameOverPanel) gameOverPanel.SetActive(false);
        menuUI?.HideMenus();
        FindAnyObjectByType<UpgradeSelectionUI>()?.Hide();
        FindAnyObjectByType<TutorialOverlayUI>()?.Hide();

        UpdateScoreUI();
        UpdateSpeedUI();
        RefreshObjectiveText();
        ShowStageBanner("LEVEL 1\n" + GetStageName(RunStage.Level1));
        GameAudio.Instance?.PlayUiConfirm();
    }

    public void StartGame(GameDifficulty difficulty)
    {
        SetDifficulty(difficulty);
        StartGame();
    }

    public void StartGame(GameMode mode, GameDifficulty difficulty)
    {
        SetMode(mode);
        SetDifficulty(difficulty);
        StartGame();
    }

    public void PauseGame()
    {
        if (!hasStarted || paused || gameOver || victory || upgradeSelectionActive)
            return;

        paused = true;
        Time.timeScale = 0f;
        menuUI?.ShowPauseMenu();
        GameAudio.Instance?.PlayPauseToggle();
    }

    public void ResumeGame()
    {
        if (!paused || gameOver || victory || upgradeSelectionActive)
            return;

        paused = false;
        Time.timeScale = 1f;
        menuUI?.HideMenus();
        GameAudio.Instance?.PlayUiConfirm();
    }

    public void OpenStartMenu(bool playSound = true)
    {
        EnsureRuntimeSystems();
        ResetRunState();

        hasStarted = false;
        paused = false;
        gameOver = false;
        victory = false;
        upgradeSelectionActive = false;
        Time.timeScale = 0f;
        WallColor = wallColorHealthy;
        UpdateScoreUI();
        UpdateSpeedUI();
        RefreshObjectiveText();
        ResetEndPanelPresentation();

        if (gameOverPanel) gameOverPanel.SetActive(false);
        menuUI?.ShowStartMenu();
        FindAnyObjectByType<UpgradeSelectionUI>()?.Hide();
        FindAnyObjectByType<TutorialOverlayUI>()?.MaybeShowFirstTime();

        if (playSound)
            GameAudio.Instance?.PlayPauseToggle();
    }

    public void AddScore(int amount)
    {
        if (!hasStarted || gameOver || victory)
            return;

        score += amount;
        UpdateScoreUI();
        EvaluateStageProgression();
        RefreshObjectiveText();
    }

    public void AddScrap(int amount)
    {
        if (!hasStarted || gameOver || victory || amount <= 0)
            return;

        scrapThisRun += amount;
        RefreshObjectiveText();
    }

    public void RegisterEnemyDestroyed(int scrapReward)
    {
        enemiesDestroyedThisRun++;
        AddScrap(scrapReward);

        if (enemiesDestroyedThisRun >= 1)
            UnlockAchievement(SaveProfile.AchievementId.FirstBlood);

        RefreshObjectiveText();
    }

    public void RegisterBoss(BossEnemy boss)
    {
        activeBoss = boss;
        pendingBossType = boss != null ? boss.bossType : pendingBossType;
    }

    public void UnregisterBoss(BossEnemy boss)
    {
        if (activeBoss == boss)
            activeBoss = null;
    }

    public void NotifyBossDefeated(int rewardScore)
    {
        bossKillsThisRun++;
        UnlockAchievement(SaveProfile.AchievementId.BossBreaker);
        SaveProfile.AddPermanentWeaponFlag(1 << (int)WeaponType.Missile);

        if (rewardScore > 0)
            AddScore(rewardScore);

        activeBoss = null;

        if (selectedMode == GameMode.Campaign || selectedMode == GameMode.Challenge)
        {
            TriggerVictory("MISSION COMPLETE", new Color(0.95f, 0.9f, 0.35f),
                "Boss Destroyed  |  Score: " + score + "  |  Scrap: " + scrapThisRun);
            return;
        }

        ShowAchievementBanner(BossEnemy.GetDisplayName(pendingBossType) + " DESTROYED");
        currentStage = RunStage.Level1;
        pendingBossType = GetBossTypeForCurrentRun();
        ClearTransientGameplayObjects(includeBoss: false);
        ResetRunTargets(relativeToCurrentScore: true);
        ShowStageBanner("NEXT LOOP\n" + GetStageName(RunStage.Level1));
        OfferUpgradeChoices();
    }

    void EvaluateStageProgression()
    {
        if (currentStage == RunStage.Boss || currentStage == RunStage.Victory)
            return;

        if (currentStage == RunStage.Level1 && score >= nextLevel2Target)
        {
            AdvanceToStage(RunStage.Level2);
            return;
        }

        if (currentStage == RunStage.Level2 && score >= nextLevel3Target)
        {
            AdvanceToStage(RunStage.Level3);
            return;
        }

        if (currentStage == RunStage.Level3 && score >= nextBossTarget)
            StartBossFight();
    }

    void AdvanceToStage(RunStage newStage)
    {
        currentStage = newStage;
        scrollSpeed = Mathf.Max(scrollSpeed, GetStageEntrySpeed());
        UpdateSpeedUI();
        RefreshObjectiveText();
        ShowStageBanner("LEVEL " + CurrentLevelNumber + "\n" + GetStageName(newStage));
        GameAudio.Instance?.PlayUiConfirm();

        if (newStage == RunStage.Level2 || newStage == RunStage.Level3)
            OfferUpgradeChoices();
    }

    void StartBossFight()
    {
        currentStage = RunStage.Boss;
        pendingBossType = GetBossTypeForCurrentRun();
        ClearTransientGameplayObjects(includeBoss: false);
        SpawnBoss();
        scrollSpeed = Mathf.Max(scrollSpeed, GetStageEntrySpeed());
        UpdateSpeedUI();
        RefreshObjectiveText();
        ShowStageBanner("BOSS FIGHT\n" + BossEnemy.GetDisplayName(pendingBossType));
        GameAudio.Instance?.PlayUiConfirm();
    }

    void SpawnBoss()
    {
        if (activeBoss != null)
            return;

        GameObject bossObject = new GameObject("BossEnemy");
        bossObject.tag = "Enemy";
        bossObject.transform.position = new Vector3(10.8f, 0f, 0f);
        BossEnemy boss = bossObject.AddComponent<BossEnemy>();
        boss.bossType = pendingBossType;
        boss.bossTier = Mathf.Max(1, bossKillsThisRun + 1);
    }

    BossEnemy.BossType GetBossTypeForCurrentRun()
    {
        if (selectedMode == GameMode.Campaign)
            return selectedDifficulty == GameDifficulty.Hard ? BossEnemy.BossType.AegisSwarm : BossEnemy.BossType.DreadCarrier;

        if (selectedMode == GameMode.TimeAttack)
            return BossEnemy.BossType.AegisSwarm;

        return bossKillsThisRun % 2 == 0 ? BossEnemy.BossType.DreadCarrier : BossEnemy.BossType.AegisSwarm;
    }

    void OfferUpgradeChoices()
    {
        if (!hasStarted || gameOver || victory)
            return;

        playerRef ??= FindAnyObjectByType<PlayerController>();
        playerShooting ??= FindAnyObjectByType<PlayerShooting>();

        currentUpgradeChoices = BuildUpgradeChoices();
        if (currentUpgradeChoices == null || currentUpgradeChoices.Length == 0)
            return;

        upgradeSelectionActive = true;
        Time.timeScale = 0f;
        FindAnyObjectByType<UpgradeSelectionUI>()?.ShowChoices();
    }

    UpgradeChoice[] BuildUpgradeChoices()
    {
        List<UpgradeChoice> pool = new List<UpgradeChoice>
        {
            CreateUpgradeChoice(RunUpgradeType.Repair, "Field Repair", "Restore 2 HP and steady the hull.", KenneyAssets.HealthIcon),
            CreateUpgradeChoice(RunUpgradeType.MaxHealth, "Hull Plating", "Increase max HP by 1.", KenneyAssets.ShieldIcon),
            CreateUpgradeChoice(RunUpgradeType.EngineBoost, "Engine Boost", "Move 15% faster.", KenneyAssets.BeamBadge),
            CreateUpgradeChoice(RunUpgradeType.DamageBoost, "Damage Core", "Player shots hit harder.", KenneyAssets.PierceBadge),
            CreateUpgradeChoice(RunUpgradeType.CooldownBoost, "Overclock", "Fire more rapidly.", KenneyAssets.RapidFireIcon),
            CreateUpgradeChoice(RunUpgradeType.ScrapBurst, "Scrap Cache", "Gain bonus scrap for permanent unlocks.", KenneyAssets.ScrapBadge),
        };

        if (playerShooting != null)
        {
            if (!playerShooting.IsWeaponUnlocked(WeaponType.Missile))
                pool.Add(CreateUpgradeChoice(RunUpgradeType.MissileUnlock, "Missile Rack", "Unlock heavy explosive missiles.", KenneyAssets.MissileBadge));
            if (!playerShooting.IsWeaponUnlocked(WeaponType.Beam))
                pool.Add(CreateUpgradeChoice(RunUpgradeType.BeamUnlock, "Beam Array", "Unlock a fast piercing beam rifle.", KenneyAssets.BeamBadge));
            if (!playerShooting.IsWeaponUnlocked(WeaponType.Charge))
                pool.Add(CreateUpgradeChoice(RunUpgradeType.ChargeUnlock, "Charge Core", "Unlock charged heavy blasts.", KenneyAssets.ChargeBadge));
            if (!playerShooting.IsWeaponUnlocked(WeaponType.Piercer))
                pool.Add(CreateUpgradeChoice(RunUpgradeType.PiercerUnlock, "Piercer Rounds", "Unlock shots that pass through targets.", KenneyAssets.PierceBadge));
        }

        List<UpgradeChoice> selected = new List<UpgradeChoice>();
        int guard = 0;
        while (selected.Count < 3 && pool.Count > 0 && guard < 20)
        {
            int index = Random.Range(0, pool.Count);
            selected.Add(pool[index]);
            pool.RemoveAt(index);
            guard++;
        }

        return selected.ToArray();
    }

    UpgradeChoice CreateUpgradeChoice(RunUpgradeType type, string title, string description, Sprite icon)
    {
        return new UpgradeChoice
        {
            type = type,
            title = title,
            description = description,
            icon = icon,
        };
    }

    public int GetUpgradeChoiceCount()
    {
        return currentUpgradeChoices != null ? currentUpgradeChoices.Length : 0;
    }

    public UpgradeChoice GetUpgradeChoice(int index)
    {
        if (currentUpgradeChoices == null || index < 0 || index >= currentUpgradeChoices.Length)
            return null;

        return currentUpgradeChoices[index];
    }

    public void ApplyUpgradeChoice(int index)
    {
        UpgradeChoice choice = GetUpgradeChoice(index);
        if (choice == null)
            return;

        playerRef ??= FindAnyObjectByType<PlayerController>();
        playerShooting ??= FindAnyObjectByType<PlayerShooting>();

        switch (choice.type)
        {
            case RunUpgradeType.Repair:
                playerRef?.Heal(2);
                break;
            case RunUpgradeType.MaxHealth:
                playerRef?.IncreaseMaxHealth(1);
                break;
            case RunUpgradeType.EngineBoost:
                playerRef?.ApplyEngineBoost(0.15f);
                break;
            case RunUpgradeType.DamageBoost:
                playerShooting?.ApplyDamageBoost(1);
                break;
            case RunUpgradeType.CooldownBoost:
                playerShooting?.ApplyCooldownBoost(0.12f);
                break;
            case RunUpgradeType.MissileUnlock:
                playerShooting?.UnlockWeapon(WeaponType.Missile, true);
                break;
            case RunUpgradeType.BeamUnlock:
                playerShooting?.UnlockWeapon(WeaponType.Beam, true);
                break;
            case RunUpgradeType.ChargeUnlock:
                playerShooting?.UnlockWeapon(WeaponType.Charge, true);
                break;
            case RunUpgradeType.PiercerUnlock:
                playerShooting?.UnlockWeapon(WeaponType.Piercer, true);
                break;
            case RunUpgradeType.ScrapBurst:
                AddScrap(40);
                break;
        }

        upgradesTakenThisRun++;
        ShowAchievementBanner(choice.title);
        currentUpgradeChoices = new UpgradeChoice[0];
        upgradeSelectionActive = false;
        Time.timeScale = paused ? 0f : 1f;
        FindAnyObjectByType<UpgradeSelectionUI>()?.Hide();
        RefreshObjectiveText();
        GameAudio.Instance?.PlayPickup();
    }

    public void DismissTutorial()
    {
        SaveProfile.MarkTutorialSeen();
        FindAnyObjectByType<TutorialOverlayUI>()?.Hide();
    }

    public void ShowAchievementBanner(string text)
    {
        achievementBannerText = text;
        achievementBannerTimer = AchievementBannerDuration;
    }

    public bool UnlockAchievement(SaveProfile.AchievementId achievement)
    {
        bool unlocked = SaveProfile.UnlockAchievement(achievement);
        if (unlocked)
            ShowAchievementBanner("ACHIEVEMENT: " + SaveProfile.GetAchievementLabel(achievement));

        return unlocked;
    }

    void RefreshObjectiveText()
    {
        objectiveText = selectedMode switch
        {
            GameMode.Endless => IsBossFightActive
                ? "Survive the boss assault and push for the next loop."
                : "Endless Loop  |  Next Boss at Score " + nextBossTarget,
            GameMode.TimeAttack => IsBossFightActive
                ? "Boss active. Burn it down before time runs out."
                : "Score hard before the timer expires.",
            GameMode.Challenge => currentStage switch
            {
                RunStage.Level1 => "Challenge Run  |  Reach Score " + nextLevel2Target,
                RunStage.Level2 => "Hold the line to Score " + nextLevel3Target,
                RunStage.Level3 => "Prepare for the boss at Score " + nextBossTarget,
                RunStage.Boss => "Destroy " + BossName,
                _ => string.Empty,
            },
            _ => currentStage switch
            {
                RunStage.Level1 => "Reach Score " + nextLevel2Target + " to enter Sector 2.",
                RunStage.Level2 => "Reach Score " + nextLevel3Target + " to enter Sector 3.",
                RunStage.Level3 => "Reach Score " + nextBossTarget + " to trigger the boss fight.",
                RunStage.Boss => "Target the weak point and destroy " + BossName + ".",
                _ => string.Empty,
            },
        };
    }

    void ClearTransientGameplayObjects(bool includeBoss)
    {
        HashSet<GameObject> toDestroy = new HashSet<GameObject>();

        foreach (ScrollingObject scrolling in FindObjectsByType<ScrollingObject>(FindObjectsSortMode.None))
            if (scrolling != null) toDestroy.Add(scrolling.gameObject);

        foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            if (enemy != null) toDestroy.Add(enemy.gameObject);

        foreach (HealthPack pack in FindObjectsByType<HealthPack>(FindObjectsSortMode.None))
            if (pack != null) toDestroy.Add(pack.gameObject);

        foreach (PowerUpPickup pickup in FindObjectsByType<PowerUpPickup>(FindObjectsSortMode.None))
            if (pickup != null) toDestroy.Add(pickup.gameObject);

        foreach (Projectile projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
            if (projectile != null) toDestroy.Add(projectile.gameObject);

        foreach (ObstacleWall wall in FindObjectsByType<ObstacleWall>(FindObjectsSortMode.None))
            if (wall != null && wall.transform.parent != null) toDestroy.Add(wall.transform.parent.gameObject);

        if (includeBoss)
        {
            foreach (BossEnemy boss in FindObjectsByType<BossEnemy>(FindObjectsSortMode.None))
                if (boss != null) toDestroy.Add(boss.gameObject);
        }

        foreach (GameObject obj in toDestroy)
            if (obj != null) Destroy(obj);
    }

    void ResetRunState()
    {
        ClearTransientGameplayObjects(includeBoss: true);
        ResetRunFlags();
        ResetRunTargets();
        ResetEndPanelPresentation();
    }

    void ResetRunFlags()
    {
        score = 0;
        scrapThisRun = 0;
        enemiesDestroyedThisRun = 0;
        bossKillsThisRun = 0;
        upgradesTakenThisRun = 0;
        scrollSpeed = GetInitialScrollSpeed();
        modeTimerRemaining = selectedMode == GameMode.TimeAttack ? timeAttackDuration : 0f;
        gameOver = false;
        victory = false;
        paused = false;
        hasStarted = false;
        upgradeSelectionActive = false;
        stageBannerTimer = 0f;
        achievementBannerTimer = 0f;
        stageBannerText = string.Empty;
        achievementBannerText = string.Empty;
        objectiveText = string.Empty;
        currentStage = RunStage.Level1;
        activeBoss = null;
        pendingBossType = GetBossTypeForCurrentRun();
        currentUpgradeChoices = new UpgradeChoice[0];
    }

    void ResetRunTargets(bool relativeToCurrentScore = false)
    {
        int baseScore = relativeToCurrentScore ? score : 0;
        nextLevel2Target = baseScore + BaseLevel2ScoreTarget;
        nextLevel3Target = baseScore + BaseLevel3ScoreTarget;
        nextBossTarget = baseScore + BaseBossScoreTarget;
    }

    void ShowStageBanner(string message)
    {
        stageBannerText = message;
        stageBannerTimer = StageBannerDuration;
    }

    void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = "Score: " + score;
    }

    void UpdateSpeedUI()
    {
        if (speedText)
            speedText.text = "Speed: " + scrollSpeed.ToString("F1") + "  |  " + selectedDifficulty + "  |  " + SelectedModeLabel;
    }

    public void TriggerGameOver()
    {
        TriggerGameOver("GAME OVER", new Color(1f, 0.25f, 0.25f), "Final Score: " + score + "  |  Scrap: " + scrapThisRun);
    }

    public void TriggerGameOver(string title, Color titleColor, string body)
    {
        if (gameOver || victory)
            return;

        gameOver = true;
        paused = false;
        upgradeSelectionActive = false;
        Time.timeScale = 0f;

        RecordRunResults();
        menuUI?.HideMenus(keepHudVisible: true);
        FindAnyObjectByType<UpgradeSelectionUI>()?.Hide();
        SetEndPanelPresentation(title, titleColor, body);
        GameAudio.Instance?.PlayGameOver();
    }

    public void TriggerVictory()
    {
        TriggerVictory("MISSION COMPLETE", new Color(0.95f, 0.9f, 0.35f), "Boss Destroyed  |  Score: " + score + "  |  Scrap: " + scrapThisRun);
    }

    public void TriggerVictory(string title, Color titleColor, string body)
    {
        if (victory || gameOver)
            return;

        victory = true;
        currentStage = RunStage.Victory;
        paused = false;
        upgradeSelectionActive = false;
        Time.timeScale = 0f;

        RecordRunResults();
        menuUI?.HideMenus(keepHudVisible: true);
        FindAnyObjectByType<UpgradeSelectionUI>()?.Hide();
        SetEndPanelPresentation(title, titleColor, body);
        GameAudio.Instance?.PlayUiConfirm();
    }

    void RecordRunResults()
    {
        SaveProfile.RecordRun(selectedMode, score, scrapThisRun, bossKillsThisRun);

        if (selectedMode == GameMode.Endless && score >= 80)
            UnlockAchievement(SaveProfile.AchievementId.EndlessPilot);
        if (selectedMode == GameMode.TimeAttack && score >= 35)
            UnlockAchievement(SaveProfile.AchievementId.TimeAttackAce);
        if (selectedMode == GameMode.Challenge && (victory || bossKillsThisRun > 0))
            UnlockAchievement(SaveProfile.AchievementId.ChallengeVictor);
        if (bossKillsThisRun >= 2)
            UnlockAchievement(SaveProfile.AchievementId.SectorSurvivor);
        if (playerShooting != null && playerShooting.UnlockedWeaponCount >= 5)
            UnlockAchievement(SaveProfile.AchievementId.WeaponCollector);
    }

    void SetEndPanelPresentation(string title, Color titleColor, string body)
    {
        endRunOverlayUI ??= FindAnyObjectByType<EndRunOverlayUI>();
        if (endRunOverlayUI != null)
        {
            endRunOverlayUI.Show(title, titleColor, body, victory);
            ApplyLegacyEndPanelPresentation(title, titleColor, body);
            if (gameOverPanel) gameOverPanel.SetActive(false);
            return;
        }

        ApplyLegacyEndPanelPresentation(title, titleColor, body);
    }

    void ApplyLegacyEndPanelPresentation(string title, Color titleColor, string body)
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);

        Text legacyTitle = FindNamedComponent<Text>(gameOverPanel != null ? gameOverPanel.transform : null, "GameOverText");
        if (legacyTitle != null)
        {
            legacyTitle.text = title;
            legacyTitle.color = titleColor;
        }

        TMP_Text tmpTitle = FindNamedComponent<TMP_Text>(gameOverPanel != null ? gameOverPanel.transform : null, "GameOverText");
        if (tmpTitle != null)
        {
            tmpTitle.text = title;
            tmpTitle.color = titleColor;
        }

        if (gameOverScoreText) gameOverScoreText.text = body;

        string fullBody = body + "\n" + SaveProfile.GetAchievementSummary();
        Text legacyBody = FindNamedComponent<Text>(gameOverPanel != null ? gameOverPanel.transform : null, "FinalScoreText");
        if (legacyBody != null)
            legacyBody.text = fullBody;

        TMP_Text tmpBody = FindNamedComponent<TMP_Text>(gameOverPanel != null ? gameOverPanel.transform : null, "FinalScoreText");
        if (tmpBody != null)
            tmpBody.text = fullBody;
    }

    void ResetEndPanelPresentation()
    {
        endRunOverlayUI ??= FindAnyObjectByType<EndRunOverlayUI>();
        endRunOverlayUI?.Hide();
        ApplyLegacyEndPanelPresentation("GAME OVER", new Color(1f, 0.25f, 0.25f), "Final Score: " + score);
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Instance = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public float GetInitialScrollSpeed()
    {
        float difficultyValue = selectedDifficulty switch
        {
            GameDifficulty.Easy => initialScrollSpeed * 0.85f,
            GameDifficulty.Hard => initialScrollSpeed * 1.15f,
            _ => initialScrollSpeed,
        };

        return difficultyValue * GetModeBaseSpeedMultiplier();
    }

    public float GetSpeedIncreaseRate()
    {
        float difficultyValue = selectedDifficulty switch
        {
            GameDifficulty.Easy => speedIncreaseRate * 0.85f,
            GameDifficulty.Hard => speedIncreaseRate * 1.35f,
            _ => speedIncreaseRate,
        };

        return difficultyValue * GetStageSpeedGrowthMultiplier() * GetModeGrowthMultiplier();
    }

    public float GetMaxScrollSpeed()
    {
        float difficultyValue = selectedDifficulty switch
        {
            GameDifficulty.Easy => maxScrollSpeed * 0.9f,
            GameDifficulty.Hard => maxScrollSpeed * 1.15f,
            _ => maxScrollSpeed,
        };

        return difficultyValue * GetStageSpeedCapMultiplier() * GetModeCapMultiplier();
    }

    public float GetSpawnInterval(float baseInterval)
    {
        float difficultyValue = selectedDifficulty switch
        {
            GameDifficulty.Easy => baseInterval * 1.18f,
            GameDifficulty.Hard => baseInterval * 0.82f,
            _ => baseInterval,
        };

        return difficultyValue * GetStageSpawnIntervalMultiplier() * GetModeSpawnIntervalMultiplier();
    }

    public float GetGapSize(float baseGapSize)
    {
        float difficultyValue = selectedDifficulty switch
        {
            GameDifficulty.Easy => baseGapSize * 1.16f,
            GameDifficulty.Hard => baseGapSize * 0.82f,
            _ => baseGapSize,
        };

        return difficultyValue * GetStageGapMultiplier() * GetModeGapMultiplier();
    }

    public float GetHealthPackChance(float baseChance)
    {
        float difficultyValue = selectedDifficulty switch
        {
            GameDifficulty.Easy => Mathf.Clamp01(baseChance + 0.2f),
            GameDifficulty.Hard => Mathf.Clamp01(baseChance - 0.1f),
            _ => baseChance,
        };

        return Mathf.Clamp01(difficultyValue + GetStageHealthPackBonus() + GetModeHealthPackBonus());
    }

    public float GetEnemyChance(float baseChance)
    {
        float difficultyValue = selectedDifficulty switch
        {
            GameDifficulty.Easy => Mathf.Clamp01(baseChance - 0.2f),
            GameDifficulty.Hard => Mathf.Clamp01(baseChance + 0.2f),
            _ => baseChance,
        };

        return Mathf.Clamp01(difficultyValue + GetStageEnemyChanceBonus() + GetModeEnemyChanceBonus());
    }

    public float GetPowerUpChance(float baseChance)
    {
        if (currentStage == RunStage.Boss)
            return 0f;

        float difficultyValue = selectedDifficulty switch
        {
            GameDifficulty.Easy => Mathf.Clamp01(baseChance + 0.12f),
            GameDifficulty.Hard => Mathf.Clamp01(baseChance - 0.05f),
            _ => baseChance,
        };

        return Mathf.Clamp01(difficultyValue + GetStagePowerUpBonus() + GetModePowerUpBonus());
    }

    public int GetMaxEnemiesPerObstacle(int baseMax)
    {
        int difficultyValue = selectedDifficulty switch
        {
            GameDifficulty.Easy => Mathf.Max(1, baseMax - 1),
            GameDifficulty.Hard => baseMax + 1,
            _ => baseMax,
        };

        return Mathf.Max(1, difficultyValue + GetStageEnemyCountBonus() + GetModeEnemyCountBonus());
    }

    public float GetEnemyFireIntervalMultiplier()
    {
        float difficultyValue = selectedDifficulty switch
        {
            GameDifficulty.Hard => 0.72f,
            _ => 1f,
        };

        return difficultyValue * GetStageEnemyFireMultiplier() * GetModeEnemyFireMultiplier();
    }

    public float GetEnemyProjectileSpeedMultiplier()
    {
        float difficultyValue = selectedDifficulty switch
        {
            GameDifficulty.Hard => 1.12f,
            GameDifficulty.Easy => 0.9f,
            _ => 1f,
        };

        return difficultyValue * GetStageEnemyProjectileMultiplier() * GetModeEnemyProjectileMultiplier();
    }

    public float GetEnemyStrafeAmplitude()
    {
        return selectedMode switch
        {
            GameMode.Challenge => 0.65f,
            GameMode.Endless => currentStage == RunStage.Level1 ? 0.22f : 0.55f,
            _ => selectedDifficulty == GameDifficulty.Hard && currentStage != RunStage.Level1 ? 0.5f : 0f,
        };
    }

    public float GetEnemyStrafeSpeed()
    {
        return selectedMode switch
        {
            GameMode.Challenge => 3.2f,
            GameMode.Endless => currentStage == RunStage.Level1 ? 1.6f : 2.8f,
            _ => selectedDifficulty == GameDifficulty.Hard && currentStage != RunStage.Level1 ? 2.6f : 0f,
        };
    }

    float GetModeBaseSpeedMultiplier()
    {
        return selectedMode switch
        {
            GameMode.Endless => 1.06f + bossKillsThisRun * 0.03f,
            GameMode.TimeAttack => 1.08f,
            GameMode.Challenge => 1.1f,
            _ => 1f,
        };
    }

    float GetModeGrowthMultiplier()
    {
        return selectedMode switch
        {
            GameMode.Endless => 1.12f + bossKillsThisRun * 0.03f,
            GameMode.TimeAttack => 1.12f,
            GameMode.Challenge => 1.16f,
            _ => 1f,
        };
    }

    float GetModeCapMultiplier()
    {
        return selectedMode switch
        {
            GameMode.Endless => 1.08f + bossKillsThisRun * 0.02f,
            GameMode.TimeAttack => 1.05f,
            GameMode.Challenge => 1.08f,
            _ => 1f,
        };
    }

    float GetModeSpawnIntervalMultiplier()
    {
        return selectedMode switch
        {
            GameMode.Endless => 0.92f,
            GameMode.TimeAttack => 0.88f,
            GameMode.Challenge => 0.84f,
            _ => 1f,
        };
    }

    float GetModeGapMultiplier()
    {
        return selectedMode switch
        {
            GameMode.Endless => 0.96f,
            GameMode.TimeAttack => 0.9f,
            GameMode.Challenge => 0.86f,
            _ => 1f,
        };
    }

    float GetModeHealthPackBonus()
    {
        return selectedMode switch
        {
            GameMode.TimeAttack => -0.04f,
            GameMode.Challenge => -0.08f,
            _ => 0f,
        };
    }

    float GetModeEnemyChanceBonus()
    {
        return selectedMode switch
        {
            GameMode.Endless => 0.08f,
            GameMode.TimeAttack => 0.12f,
            GameMode.Challenge => 0.16f,
            _ => 0f,
        };
    }

    float GetModePowerUpBonus()
    {
        return selectedMode switch
        {
            GameMode.TimeAttack => 0.04f,
            GameMode.Challenge => -0.02f,
            _ => 0f,
        };
    }

    int GetModeEnemyCountBonus()
    {
        return selectedMode switch
        {
            GameMode.Endless => 1,
            GameMode.TimeAttack => 1,
            GameMode.Challenge => 2,
            _ => 0,
        };
    }

    float GetModeEnemyFireMultiplier()
    {
        return selectedMode switch
        {
            GameMode.Endless => 0.92f,
            GameMode.TimeAttack => 0.86f,
            GameMode.Challenge => 0.8f,
            _ => 1f,
        };
    }

    float GetModeEnemyProjectileMultiplier()
    {
        return selectedMode switch
        {
            GameMode.Endless => 1.06f,
            GameMode.TimeAttack => 1.08f,
            GameMode.Challenge => 1.14f,
            _ => 1f,
        };
    }

    float GetStageEntrySpeed()
    {
        return currentStage switch
        {
            RunStage.Level2 => GetInitialScrollSpeed() * 1.12f,
            RunStage.Level3 => GetInitialScrollSpeed() * 1.24f,
            RunStage.Boss => GetInitialScrollSpeed() * 1.1f,
            _ => GetInitialScrollSpeed(),
        };
    }

    float GetStageSpawnIntervalMultiplier()
    {
        return currentStage switch
        {
            RunStage.Level1 => 1.05f,
            RunStage.Level2 => 0.92f,
            RunStage.Level3 => 0.78f,
            _ => 1f,
        };
    }

    float GetStageGapMultiplier()
    {
        return currentStage switch
        {
            RunStage.Level1 => 1.08f,
            RunStage.Level2 => 0.94f,
            RunStage.Level3 => 0.82f,
            _ => 1f,
        };
    }

    float GetStageHealthPackBonus()
    {
        return currentStage switch
        {
            RunStage.Level1 => 0.08f,
            RunStage.Level3 => -0.04f,
            _ => 0f,
        };
    }

    float GetStageEnemyChanceBonus()
    {
        return currentStage switch
        {
            RunStage.Level1 => -0.08f,
            RunStage.Level2 => 0.05f,
            RunStage.Level3 => 0.16f,
            _ => 0f,
        };
    }

    float GetStagePowerUpBonus()
    {
        return currentStage switch
        {
            RunStage.Level1 => 0.04f,
            RunStage.Level2 => 0.02f,
            RunStage.Level3 => -0.03f,
            _ => 0f,
        };
    }

    int GetStageEnemyCountBonus()
    {
        return currentStage switch
        {
            RunStage.Level2 => 1,
            RunStage.Level3 => 2,
            _ => 0,
        };
    }

    float GetStageEnemyFireMultiplier()
    {
        return currentStage switch
        {
            RunStage.Level2 => 0.92f,
            RunStage.Level3 => 0.82f,
            _ => 1f,
        };
    }

    float GetStageEnemyProjectileMultiplier()
    {
        return currentStage switch
        {
            RunStage.Level2 => 1.06f,
            RunStage.Level3 => 1.14f,
            _ => 1f,
        };
    }

    float GetStageSpeedGrowthMultiplier()
    {
        return currentStage switch
        {
            RunStage.Level2 => 1.08f,
            RunStage.Level3 => 1.16f,
            _ => 1f,
        };
    }

    float GetStageSpeedCapMultiplier()
    {
        return currentStage switch
        {
            RunStage.Level2 => 1.05f,
            RunStage.Level3 => 1.12f,
            _ => 1f,
        };
    }

    static string GetStageName(RunStage stage)
    {
        return stage switch
        {
            RunStage.Level1 => "BREACH LINE",
            RunStage.Level2 => "CROSSFIRE CORRIDOR",
            RunStage.Level3 => "WARZONE RIFT",
            RunStage.Boss => "BOSS ENCOUNTER",
            RunStage.Victory => "MISSION COMPLETE",
            _ => "RUN",
        };
    }

    static string GetStageLabel(RunStage stage)
    {
        return stage switch
        {
            RunStage.Level1 => "LEVEL 1  |  BREACH LINE",
            RunStage.Level2 => "LEVEL 2  |  CROSSFIRE CORRIDOR",
            RunStage.Level3 => "LEVEL 3  |  WARZONE RIFT",
            RunStage.Boss => "BOSS  |  ENCOUNTER",
            RunStage.Victory => "MISSION COMPLETE",
            _ => "RUN",
        };
    }

    static T FindNamedComponent<T>(Transform parent, string name) where T : Component
    {
        if (parent == null)
            return null;

        if (parent.name == name)
            return parent.GetComponent<T>();

        for (int i = 0; i < parent.childCount; i++)
        {
            T child = FindNamedComponent<T>(parent.GetChild(i), name);
            if (child != null)
                return child;
        }

        return null;
    }
}
