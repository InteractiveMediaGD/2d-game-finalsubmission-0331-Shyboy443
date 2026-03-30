using System;
using UnityEngine;

/// <summary>
/// Persistent meta progression, high scores, achievements, and settings.
/// Stored in PlayerPrefs as JSON so it works in both the editor and builds.
/// </summary>
public static class SaveProfile
{
    [Flags]
    public enum AchievementId
    {
        None = 0,
        FirstBlood = 1 << 0,
        WeaponCollector = 1 << 1,
        BossBreaker = 1 << 2,
        EndlessPilot = 1 << 3,
        TimeAttackAce = 1 << 4,
        ChallengeVictor = 1 << 5,
        SectorSurvivor = 1 << 6,
    }

    [Serializable]
    class SaveProfileData
    {
        public float musicVolume = 0.75f;
        public float sfxVolume = 0.95f;
        public int totalScrap;
        public int bestCampaignScore;
        public int bestEndlessScore;
        public int bestTimeAttackScore;
        public int bestChallengeScore;
        public int bestBossKills;
        public int shipSkinUnlockCount = 1;
        public int permanentWeaponFlags;
        public int achievementFlags;
        public bool tutorialSeen;
    }

    const string SaveKey = "starfall_frontier_save_v2";
    static SaveProfileData data;

    static SaveProfileData Data
    {
        get
        {
            if (data == null)
                Load();

            return data;
        }
    }

    public static float MusicVolume => Mathf.Clamp01(Data.musicVolume);
    public static float SfxVolume => Mathf.Clamp01(Data.sfxVolume);
    public static int TotalScrap => Mathf.Max(0, Data.totalScrap);
    public static bool TutorialSeen => Data.tutorialSeen;
    public static int ShipSkinUnlockCount => Mathf.Clamp(Data.shipSkinUnlockCount, 1, 4);
    public static int PermanentWeaponFlags => Mathf.Max(0, Data.permanentWeaponFlags);
    public static int BestBossKills => Mathf.Max(0, Data.bestBossKills);

    public static void SetMusicVolume(float value)
    {
        Data.musicVolume = Mathf.Clamp01(value);
        Save();
    }

    public static void SetSfxVolume(float value)
    {
        Data.sfxVolume = Mathf.Clamp01(value);
        Save();
    }

    public static void MarkTutorialSeen()
    {
        if (Data.tutorialSeen)
            return;

        Data.tutorialSeen = true;
        Save();
    }

    public static void AddScrap(int amount)
    {
        if (amount <= 0)
            return;

        Data.totalScrap += amount;
        UnlockShipSkinsFromScrap();
        Save();
    }

    public static void AddPermanentWeaponFlag(int mask)
    {
        if (mask == 0)
            return;

        int oldFlags = Data.permanentWeaponFlags;
        Data.permanentWeaponFlags |= mask;
        if (oldFlags != Data.permanentWeaponFlags)
            Save();
    }

    public static bool HasPermanentWeaponFlag(int mask)
    {
        return (Data.permanentWeaponFlags & mask) != 0;
    }

    public static int GetBestScore(GameManager.GameMode mode)
    {
        return mode switch
        {
            GameManager.GameMode.Endless => Data.bestEndlessScore,
            GameManager.GameMode.TimeAttack => Data.bestTimeAttackScore,
            GameManager.GameMode.Challenge => Data.bestChallengeScore,
            _ => Data.bestCampaignScore,
        };
    }

    public static void RecordRun(GameManager.GameMode mode, int score, int scrapEarned, int bossKills)
    {
        if (score > GetBestScore(mode))
        {
            switch (mode)
            {
                case GameManager.GameMode.Endless:
                    Data.bestEndlessScore = score;
                    break;
                case GameManager.GameMode.TimeAttack:
                    Data.bestTimeAttackScore = score;
                    break;
                case GameManager.GameMode.Challenge:
                    Data.bestChallengeScore = score;
                    break;
                default:
                    Data.bestCampaignScore = score;
                    break;
            }
        }

        if (bossKills > 0)
            Data.bestBossKills = Mathf.Max(Data.bestBossKills, bossKills);

        if (scrapEarned > 0)
            AddScrap(scrapEarned);
        else
            Save();
    }

    public static bool UnlockAchievement(AchievementId achievement)
    {
        int mask = (int)achievement;
        if ((Data.achievementFlags & mask) != 0)
            return false;

        Data.achievementFlags |= mask;
        Save();
        return true;
    }

    public static bool IsAchievementUnlocked(AchievementId achievement)
    {
        return (Data.achievementFlags & (int)achievement) != 0;
    }

    public static string GetAchievementLabel(AchievementId achievement)
    {
        return achievement switch
        {
            AchievementId.FirstBlood => "First Kill",
            AchievementId.WeaponCollector => "Weapon Collector",
            AchievementId.BossBreaker => "Boss Breaker",
            AchievementId.EndlessPilot => "Endless Pilot",
            AchievementId.TimeAttackAce => "Time Attack Ace",
            AchievementId.ChallengeVictor => "Challenge Victor",
            AchievementId.SectorSurvivor => "Sector Survivor",
            _ => "Achievement",
        };
    }

    public static string GetAchievementSummary()
    {
        AchievementId[] achievements =
        {
            AchievementId.FirstBlood,
            AchievementId.WeaponCollector,
            AchievementId.BossBreaker,
            AchievementId.EndlessPilot,
            AchievementId.TimeAttackAce,
            AchievementId.ChallengeVictor,
            AchievementId.SectorSurvivor,
        };

        int unlocked = 0;
        for (int i = 0; i < achievements.Length; i++)
            if (IsAchievementUnlocked(achievements[i]))
                unlocked++;

        return unlocked + " / " + achievements.Length + " achievements";
    }

    public static void Load()
    {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);
        data = string.IsNullOrEmpty(json)
            ? new SaveProfileData()
            : JsonUtility.FromJson<SaveProfileData>(json);

        if (data == null)
            data = new SaveProfileData();

        UnlockShipSkinsFromScrap();
    }

    public static void Save()
    {
        if (data == null)
            data = new SaveProfileData();

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    static void UnlockShipSkinsFromScrap()
    {
        int unlocks = 1;
        if (Data.totalScrap >= 120) unlocks = 2;
        if (Data.totalScrap >= 280) unlocks = 3;
        if (Data.totalScrap >= 520) unlocks = 4;
        Data.shipSkinUnlockCount = unlocks;
    }
}
