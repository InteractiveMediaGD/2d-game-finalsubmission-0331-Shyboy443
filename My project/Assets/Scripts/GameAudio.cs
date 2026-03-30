using UnityEngine;

/// <summary>
/// Centralized sound effect player that loads clips from Resources.
/// </summary>
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    AudioSource sfxSource;
    AudioSource uiSource;
    AudioSource musicSource;

    AudioClip playerShootClip;
    AudioClip enemyShootClip;
    AudioClip pickupClip;
    AudioClip damageClip;
    AudioClip gameOverClip;
    AudioClip uiConfirmClip;
    AudioClip pauseToggleClip;
    AudioClip menuMusicClip;
    AudioClip battleMusicClip;
    float sfxVolumeMultiplier = 1f;
    float musicVolumeMultiplier = 1f;

    public static GameAudio EnsureExists()
    {
        if (Instance != null)
            return Instance;

        GameAudio existing = FindAnyObjectByType<GameAudio>();
        if (existing != null)
            return existing;

        GameObject obj = new GameObject("GameAudio");
        return obj.AddComponent<GameAudio>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = 0.85f;

        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.playOnAwake = false;
        uiSource.loop = false;
        uiSource.spatialBlend = 0f;
        uiSource.volume = 0.9f;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 0.26f;

        LoadClips();
        ApplySavedSettings();
        EnsureMusicPlaying();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void LoadClips()
    {
        playerShootClip = Resources.Load<AudioClip>("Kenney/Audio/sfx_laser1");
        enemyShootClip = Resources.Load<AudioClip>("Kenney/Audio/sfx_laser2");
        pickupClip = Resources.Load<AudioClip>("Kenney/Audio/sfx_shieldUp");
        damageClip = Resources.Load<AudioClip>("Kenney/Audio/sfx_shieldDown");
        gameOverClip = Resources.Load<AudioClip>("Kenney/Audio/sfx_lose");
        uiConfirmClip = Resources.Load<AudioClip>("Kenney/Audio/sfx_twoTone");
        pauseToggleClip = Resources.Load<AudioClip>("Kenney/Audio/sfx_zap");
        menuMusicClip = Resources.Load<AudioClip>("Kenney/Audio/menu_theme");
        battleMusicClip = Resources.Load<AudioClip>("Kenney/Audio/battle_theme");

        if (menuMusicClip == null)
            menuMusicClip = CreateMenuAmbienceClip();
    }

    void Update()
    {
        EnsureMusicPlaying();
        UpdateMusicMix();
    }

    public void PlayPlayerShoot()
    {
        PlaySfx(playerShootClip, 0.45f);
    }

    public void PlayEnemyShoot()
    {
        PlaySfx(enemyShootClip, 0.4f);
    }

    public void PlayPickup()
    {
        PlaySfx(pickupClip, 0.7f);
    }

    public void PlayDamage()
    {
        PlaySfx(damageClip, 0.7f);
    }

    public void PlayGameOver()
    {
        PlaySfx(gameOverClip, 0.85f);
    }

    public void PlayUiConfirm()
    {
        PlayUi(uiConfirmClip, 0.75f);
    }

    public void PlayPauseToggle()
    {
        PlayUi(pauseToggleClip, 0.65f);
    }

    public void ApplySavedSettings()
    {
        musicVolumeMultiplier = SaveProfile.MusicVolume;
        sfxVolumeMultiplier = SaveProfile.SfxVolume;
    }

    public void SetMusicVolume(float value)
    {
        SaveProfile.SetMusicVolume(value);
        ApplySavedSettings();
    }

    public void SetSfxVolume(float value)
    {
        SaveProfile.SetSfxVolume(value);
        ApplySavedSettings();
    }

    void EnsureMusicPlaying()
    {
        if (musicSource == null)
            return;

        AudioClip targetClip = GetTargetMusicClip();
        if (targetClip == null)
            return;

        if (musicSource.clip != targetClip)
        {
            musicSource.Stop();
            musicSource.clip = targetClip;
            musicSource.time = 0f;
            musicSource.pitch = 1f;
        }

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    void UpdateMusicMix()
    {
        if (musicSource == null)
            return;

        bool usingMenuMusic = menuMusicClip != null && musicSource.clip == menuMusicClip;
        float targetVolume = usingMenuMusic ? 0.17f : 0.24f;
        float targetPitch = usingMenuMusic ? 0.98f : 1f;

        if (GameManager.Instance != null)
        {
            if (!GameManager.Instance.HasStarted)
            {
                targetVolume = 0.19f;
                targetPitch = 0.97f;
            }
            else if (GameManager.Instance.IsPaused)
            {
                targetVolume = 0.15f;
                targetPitch = 0.95f;
            }
            else if (GameManager.Instance.IsGameOver || GameManager.Instance.IsVictory)
            {
                targetVolume = usingMenuMusic ? 0.12f : 0.08f;
                targetPitch = usingMenuMusic ? 0.94f : 0.88f;
            }
            else if (GameManager.Instance.IsBossFightActive)
            {
                targetVolume = 0.34f;
                targetPitch = 1.08f;
            }
            else if (GameManager.Instance.IsGameplayActive)
            {
                targetVolume = 0.28f;
                targetPitch = GameManager.Instance.SelectedDifficulty switch
                {
                    GameManager.GameDifficulty.Easy => 0.98f,
                    GameManager.GameDifficulty.Hard => 1.04f,
                    _ => 1f,
                };
            }
            else
            {
                targetVolume = 0.18f;
                targetPitch = 0.96f;
            }
        }

        if (!usingMenuMusic && GameManager.Instance != null && GameManager.Instance.SelectedMode == GameManager.GameMode.TimeAttack)
            targetPitch += 0.03f;
        if (!usingMenuMusic && GameManager.Instance != null && GameManager.Instance.SelectedMode == GameManager.GameMode.Challenge)
            targetVolume += 0.03f;
        if (usingMenuMusic && GameManager.Instance != null && GameManager.Instance.SelectedMode == GameManager.GameMode.Challenge)
            targetPitch -= 0.02f;

        musicSource.volume = Mathf.MoveTowards(musicSource.volume, targetVolume * musicVolumeMultiplier, Time.unscaledDeltaTime * 0.45f);
        musicSource.pitch = Mathf.MoveTowards(musicSource.pitch, targetPitch, Time.unscaledDeltaTime * 0.35f);
    }

    AudioClip GetTargetMusicClip()
    {
        if (GameManager.Instance == null)
            return menuMusicClip != null ? menuMusicClip : battleMusicClip;

        if (!GameManager.Instance.HasStarted
            || GameManager.Instance.IsPaused
            || GameManager.Instance.IsGameOver
            || GameManager.Instance.IsVictory)
        {
            return menuMusicClip != null ? menuMusicClip : battleMusicClip;
        }

        return battleMusicClip != null ? battleMusicClip : menuMusicClip;
    }

    static AudioClip CreateMenuAmbienceClip()
    {
        const int sampleRate = 44100;
        const int durationSeconds = 18;
        int totalSamples = sampleRate * durationSeconds;
        float[] samples = new float[totalSamples];
        float[] chordRoots = { 43f, 46f, 50f, 53f };

        for (int i = 0; i < totalSamples; i++)
        {
            float t = i / (float)sampleRate;
            int section = Mathf.FloorToInt(t / 4.5f) % chordRoots.Length;
            float rootFrequency = MidiToFrequency(chordRoots[section]);
            float fifthFrequency = rootFrequency * 1.5f;
            float octaveFrequency = rootFrequency * 2f;

            float pad = Mathf.Sin(Mathf.PI * 2f * rootFrequency * t) * 0.46f;
            float fifth = Mathf.Sin(Mathf.PI * 2f * fifthFrequency * t + 0.42f) * 0.22f;
            float sub = Mathf.Sin(Mathf.PI * 2f * rootFrequency * 0.5f * t + 1.4f) * 0.24f;
            float shimmer = Mathf.Sin(Mathf.PI * 2f * (octaveFrequency + Mathf.Sin(t * 0.27f) * 4f) * t) * 0.06f;

            float pulse = 0.72f + Mathf.Max(0f, Mathf.Sin(Mathf.PI * 2f * 0.5f * t)) * 0.18f;
            float swell = 0.68f + Mathf.Sin(Mathf.PI * 2f * 0.09f * t) * 0.12f;
            float noise = (Mathf.PerlinNoise(t * 0.18f, 0.37f) - 0.5f) * 0.08f;

            samples[i] = Mathf.Clamp((pad + fifth + sub + shimmer + noise) * pulse * swell * 0.28f, -0.95f, 0.95f);
        }

        AudioClip clip = AudioClip.Create("MenuAmbienceRuntime", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    static float MidiToFrequency(float note)
    {
        return 440f * Mathf.Pow(2f, (note - 69f) / 12f);
    }

    void PlaySfx(AudioClip clip, float volume)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, volume * sfxVolumeMultiplier);
    }

    void PlayUi(AudioClip clip, float volume)
    {
        if (clip != null && uiSource != null)
            uiSource.PlayOneShot(clip, volume * sfxVolumeMultiplier);
    }
}
