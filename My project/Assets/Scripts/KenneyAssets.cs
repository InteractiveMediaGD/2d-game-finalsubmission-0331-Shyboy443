using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Loads sprites from the Kenney Space Shooter Redux pack plus selected Raven UI icons.
/// Uses Resources at runtime and AssetDatabase fallback in the editor.
/// </summary>
public static class KenneyAssets
{
    const string AssetRoot = "Assets/kenney_space-shooter-redux/";
    const string ResourcesRoot = "Kenney/";
    const string RavenAssetRoot = "Assets/Free - Raven Fantasy Icons/Free - Raven Fantasy Icons/Separated Files/64x64/";
    const string RavenResourcesRoot = "Raven/Icons/";

    static Sprite _player;
    static Sprite _playerGreen;
    static Sprite _playerBlue;
    static Sprite _playerRed;
    static Sprite _playerDamage1;
    static Sprite _playerDamage2;
    static Sprite _playerDamage3;
    static Sprite _laser;
    static Sprite _enemyLaser;
    static Sprite _beamProjectile;
    static Sprite _chargeProjectile;
    static Sprite _pierceProjectile;
    static Sprite _missileProjectile;
    static Sprite _snipedProjectile;
    static Sprite _healthPack;
    static Sprite _healthBadge;
    static Sprite _shieldBadge;
    static Sprite _rapidFireBadge;
    static Sprite _spreadShotBadge;
    static Sprite _beamBadge;
    static Sprite _missileBadge;
    static Sprite _chargeBadge;
    static Sprite _pierceBadge;
    static Sprite _scrapBadge;
    static Sprite _healthIcon;
    static Sprite _shieldIcon;
    static Sprite _rapidFireIcon;
    static Sprite _spreadShotIcon;
    static Sprite _bossCore;
    static Sprite _bossWing;
    static Sprite _bossCoreAlt;
    static Sprite _bossWingAlt;
    static Sprite _backgroundDark;
    static Sprite _backgroundBlue;
    static Sprite _backgroundPurple;
    static Sprite _backgroundBlack;
    static Sprite _menuBackground;
    static Sprite _enemyStandard;
    static Sprite _enemyTurret;
    static Sprite _enemyShielded;
    static Sprite _enemySniper;
    static Sprite _enemyKamikaze;
    static Sprite[] _obstacleMeteors;

    public static Sprite Player => _player ??= Load("PNG/playerShip1_orange.png", SpriteHelper.Square);
    public static Sprite PlayerGreen => _playerGreen ??= Load("PNG/playerShip1_green.png", Player);
    public static Sprite PlayerBlue => _playerBlue ??= Load("PNG/playerShip1_blue.png", Player);
    public static Sprite PlayerRed => _playerRed ??= Load("PNG/playerShip1_red.png", Player);
    public static Sprite PlayerDamage1 => _playerDamage1 ??= Load("PNG/Damage/playerShip1_damage1.png", Player);
    public static Sprite PlayerDamage2 => _playerDamage2 ??= Load("PNG/Damage/playerShip1_damage2.png", Player);
    public static Sprite PlayerDamage3 => _playerDamage3 ??= Load("PNG/Damage/playerShip1_damage3.png", Player);
    public static Sprite Laser => _laser ??= Load("PNG/Lasers/laserBlue07.png", SpriteHelper.Circle);
    public static Sprite EnemyLaser => _enemyLaser ??= Load("PNG/Lasers/laserRed07.png", SpriteHelper.Circle);
    public static Sprite BeamProjectile => _beamProjectile ??= Load("PNG/Lasers/laserGreen13.png", Laser);
    public static Sprite ChargeProjectile => _chargeProjectile ??= Load("PNG/Lasers/laserBlue15.png", Laser);
    public static Sprite PierceProjectile => _pierceProjectile ??= Load("PNG/Lasers/laserGreen10.png", Laser);
    public static Sprite MissileProjectile => _missileProjectile ??= Load("PNG/Lasers/laserRed10.png", EnemyLaser);
    public static Sprite SnipedProjectile => _snipedProjectile ??= Load("PNG/Lasers/laserRed14.png", EnemyLaser);
    public static Sprite HealthPack => _healthPack ??= Load("PNG/Power-ups/pill_green.png", SpriteHelper.Circle);
    public static Sprite HealthBadge => _healthBadge ??= Load("PNG/Power-ups/powerupGreen.png", SpriteHelper.Circle);
    public static Sprite ShieldBadge => _shieldBadge ??= Load("PNG/Power-ups/powerupBlue.png", SpriteHelper.Circle);
    public static Sprite RapidFireBadge => _rapidFireBadge ??= Load("PNG/Power-ups/powerupYellow.png", SpriteHelper.Circle);
    public static Sprite SpreadShotBadge => _spreadShotBadge ??= Load("PNG/Power-ups/powerupRed.png", SpriteHelper.Circle);
    public static Sprite BeamBadge => _beamBadge ??= Load("PNG/Power-ups/powerupBlue_star.png", SpriteHelper.Circle);
    public static Sprite MissileBadge => _missileBadge ??= Load("PNG/Power-ups/powerupRed_bolt.png", SpriteHelper.Circle);
    public static Sprite ChargeBadge => _chargeBadge ??= Load("PNG/Power-ups/powerupBlue_shield.png", SpriteHelper.Circle);
    public static Sprite PierceBadge => _pierceBadge ??= Load("PNG/Power-ups/powerupGreen_star.png", SpriteHelper.Circle);
    public static Sprite ScrapBadge => _scrapBadge ??= Load("PNG/Power-ups/things_gold.png", SpriteHelper.Circle);
    public static Sprite HealthIcon => _healthIcon ??= LoadRaven("health_heart.png", HealthBadge);
    public static Sprite ShieldIcon => _shieldIcon ??= LoadRaven("shield_guard.png", Load("PNG/Power-ups/shield_gold.png", SpriteHelper.Circle));
    public static Sprite RapidFireIcon => _rapidFireIcon ??= LoadRaven("rapid_fire_burst.png", Load("PNG/Power-ups/bolt_gold.png", SpriteHelper.Circle));
    public static Sprite SpreadShotIcon => _spreadShotIcon ??= LoadRaven("spread_shot_flare.png", Load("PNG/Power-ups/star_gold.png", SpriteHelper.Circle));
    public static Sprite BossCore => _bossCore ??= Load("PNG/Enemies/enemyBlack5.png", SpriteHelper.Diamond);
    public static Sprite BossWing => _bossWing ??= Load("PNG/Enemies/enemyRed5.png", SpriteHelper.Diamond);
    public static Sprite BossCoreAlt => _bossCoreAlt ??= Load("PNG/Enemies/enemyBlue5.png", BossCore);
    public static Sprite BossWingAlt => _bossWingAlt ??= Load("PNG/Enemies/enemyGreen5.png", BossWing);
    public static Sprite Background => _backgroundDark ??= Load("Backgrounds/darkPurple.png", null);
    public static Sprite BackgroundBlue => _backgroundBlue ??= Load("Backgrounds/blue.png", Background);
    public static Sprite BackgroundPurple => _backgroundPurple ??= Load("Backgrounds/purple.png", Background);
    public static Sprite BackgroundBlack => _backgroundBlack ??= Load("Backgrounds/black.png", Background);
    public static Sprite MenuBackground => _menuBackground ??= Load("Menu/menuBackground.png", BackgroundPurple);
    static Sprite[] ObstacleMeteors => _obstacleMeteors ??= new[]
    {
        Load("PNG/Meteors/meteorBrown_big1.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorBrown_big2.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorBrown_big3.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorBrown_big4.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorBrown_med1.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorBrown_med3.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorBrown_small1.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorBrown_small2.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorBrown_tiny1.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorBrown_tiny2.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorGrey_big1.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorGrey_big2.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorGrey_big3.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorGrey_big4.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorGrey_med1.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorGrey_med2.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorGrey_small1.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorGrey_small2.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorGrey_tiny1.png", SpriteHelper.Circle),
        Load("PNG/Meteors/meteorGrey_tiny2.png", SpriteHelper.Circle),
    };

    public static int ObstacleMeteorCount => ObstacleMeteors.Length;

    public static Sprite GetEnemySprite(Enemy.EnemyType type)
    {
        return type switch
        {
            Enemy.EnemyType.Kamikaze => _enemyKamikaze ??= Load("PNG/ufoRed.png", SpriteHelper.Circle),
            Enemy.EnemyType.Turret => _enemyTurret ??= Load("PNG/Enemies/enemyBlue4.png", SpriteHelper.Diamond),
            Enemy.EnemyType.Shielded => _enemyShielded ??= Load("PNG/Enemies/enemyGreen4.png", SpriteHelper.Diamond),
            Enemy.EnemyType.Sniper => _enemySniper ??= Load("PNG/Enemies/enemyBlack3.png", SpriteHelper.Diamond),
            _ => _enemyStandard ??= Load("PNG/Enemies/enemyRed2.png", SpriteHelper.Diamond),
        };
    }

    public static Sprite GetObstacleMeteorSprite(int index)
    {
        Sprite[] meteors = ObstacleMeteors;
        if (meteors == null || meteors.Length == 0)
            return SpriteHelper.Circle;

        return meteors[Mathf.Abs(index) % meteors.Length];
    }

    public static Sprite GetPlayerShipForUnlockCount(int unlockCount)
    {
        return Mathf.Clamp(unlockCount, 1, 4) switch
        {
            4 => PlayerRed,
            3 => PlayerBlue,
            2 => PlayerGreen,
            _ => Player,
        };
    }

    public static Sprite GetPlayerDamageSprite(int level)
    {
        return level switch
        {
            3 => PlayerDamage3,
            2 => PlayerDamage2,
            1 => PlayerDamage1,
            _ => Player,
        };
    }

    public static Sprite GetBackgroundForRun(GameManager.GameMode mode, GameManager.RunStage stage)
    {
        if (mode == GameManager.GameMode.TimeAttack)
            return BackgroundBlue;
        if (mode == GameManager.GameMode.Challenge)
            return BackgroundBlack;

        return stage switch
        {
            GameManager.RunStage.Level2 => BackgroundPurple,
            GameManager.RunStage.Level3 => BackgroundBlue,
            GameManager.RunStage.Boss => BackgroundBlack,
            _ => Background,
        };
    }

    static Sprite Load(string relativePath, Sprite fallback)
    {
        Sprite sprite = Resources.Load<Sprite>(ToResourcesPath(relativePath, ResourcesRoot));

#if UNITY_EDITOR
        if (sprite == null)
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetRoot + relativePath);
#endif

        return sprite != null ? sprite : fallback;
    }

    static Sprite LoadRaven(string relativePath, Sprite fallback)
    {
        Sprite sprite = Resources.Load<Sprite>(ToResourcesPath(relativePath, RavenResourcesRoot));

#if UNITY_EDITOR
        if (sprite == null)
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RavenAssetRoot + relativePath);
#endif

        return sprite != null ? sprite : fallback;
    }

    static string ToResourcesPath(string relativePath, string root)
    {
        string path = relativePath.Replace('\\', '/');
        int extensionIndex = path.LastIndexOf('.');
        if (extensionIndex >= 0)
            path = path.Substring(0, extensionIndex);

        return root + path;
    }
}
