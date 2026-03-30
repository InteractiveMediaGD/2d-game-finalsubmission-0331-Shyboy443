# VIVA PREPARATION GUIDE - SE4031 SPACE SHOOTER PROJECT

---

## 📋 **TABLE OF CONTENTS**

1. [PROJECT OVERVIEW](#project-overview)
2. [ARCHITECTURE OVERVIEW](#architecture-overview)
3. [DETAILED VIVA Q&A](#detailed-viva-qa)
   - [GameManager Script](#1-gamemanager-script)
   - [PlayerController Script](#2-playercontroller-script)
   - [PlayerShooting Script](#3-playershooting-script)
   - [Enemy Script](#4-enemy-script)
   - [Projectile Script](#5-projectile-script)
   - [GameBootstrapper](#6-gamebotstrapper--scene-initialization)
   - [UI Systems](#7-ui-systems)
   - [Audio System](#8-audio-system)
   - [Connecting to Tutorials](#9-connecting-to-tutorials)
   - [Design Patterns](#10-design-patterns-used)
4. [VIVA TIPS](#viva-tips)
5. [POTENTIAL VIVA QUESTIONS](#potential-viva-questions)

**Use `Ctrl + F` to search or `Ctrl + Shift + O` to see full outline**

---

## PROJECT OVERVIEW

Your project is a **2D Top-Down Space Shooter** with advanced mechanics that extends the tutorial concepts:

```
Tutorial Foundation → Your Advanced Project
- Basic Movement (PlayerMovement.cs) → PlayerController.cs (complex with bounds, health, aiming)
- Simple Colliders → Multiple enemy types with different behaviors
- Singleton Pattern (GameManager) → Full game state management (modes, upgrades, progression)
- Simple UI → Complete UI system with menus, overlays, upgrades, meta-progression
```

---

## ARCHITECTURE OVERVIEW

### **Core Systems**

```
GameBootstrapper (Entry Point)
    ↓
GameManager (Central Controller - Singleton)
    ├─ PlayerController (Player movement, health, aiming)
    ├─ PlayerShooting (Weapon switching, projectile creation)
    ├─ PlayerPowerUps (Temporary upgrades)
    ├─ Enemy & BossEnemy (AI and behavior)
    ├─ ObstacleSpawner (Environment hazards)
    ├─ UI Systems (Menus, HUD, Upgrades)
    └─ GameAudio (Sound effects and music)
```

---

## DETAILED VIVA Q&A

### **1. GAMEMANAGER SCRIPT**

**Q1: What is the singleton pattern and why is it used in GameManager?**

**A:**
- **Singleton**: Ensures only one instance of GameManager exists globally
- `public static GameManager Instance { get; private set; }`
- Allows any script to access game state: `GameManager.Instance.Score`
- **Benefits**:
  - Central access to game data (score, difficulty, game mode)
  - No scene reloading required for data persistence
  - Easy management of game flow (paused, gameplay active, game over)
- **Tutorial Connection**: Extended from the basic singleton in the GameManager tutorial

**Q2: What are the different game modes and how do they affect gameplay?**

**A:**
```csharp
public enum GameMode
{
    Campaign,    // Story progression through levels (1, 2, 3, Boss)
    Endless,     // Infinite waves until defeat
    TimeAttack,  // Complete objective in 2 minutes
    Challenge,   // Special modifiers
}
```
- **Campaign**: Structured stages with increasing difficulty
- **Endless**: Constant enemy spawning, no end condition
- **TimeAttack**: Racing against timer, `HasModeTimer` property
- **Challenge**: Different rule set (e.g., `EnemiesCanShoot` property varies)

**Q3: Explain the difficulty system and stage progression.**

**A:**
```csharp
public enum GameDifficulty { Easy, Medium, Hard }
public enum RunStage { Level1, Level2, Level3, Boss, Victory }
```

- **Easy Mode**: Enemies don't shoot (`EnemiesCanShoot = false`)
- **Medium/Hard**: Enemies shoot, harder patterns
- **Stages**:
  - Level 1: Basic introduction
  - Level 2: More enemies (score target: 12)
  - Level 3: Advanced enemies (score target: 28)
  - Boss: Special boss encounter (score target: 50)
  - Victory: Win state

**Q4: How does the upgrade system work?**

**A:**
- Stored as `UpgradeChoice[]` - array of available choices
- Called when player reaches score thresholds (`nextLevel2Target`, `nextLevel3Target`)
- Properties tracked: `upgradesTakenThisRun`, `scrapThisRun`
- Types: Weapon unlocks, damage boost, health increase, weapon cooldown reduction
- **Pattern**: Observer pattern - upgrades trigger UI selection, then apply changes

**Q5: What does "IsGameplayActive" mean and why is it important?**

**A:**
```csharp
public bool IsGameplayActive => 
    hasStarted && !paused && !gameOver && !victory && !upgradeSelectionActive;
```

- Checks if game is actually playing (not paused, not in menus)
- Used in `Update()` methods: `if (!GameManager.Instance.IsGameplayActive) return;`
- Prevents updates to player, enemies, projectiles when game isn't active
- **Tutorial Connection**: Similar to the `Time.deltaTime` concept but at a higher level

---

### **2. PLAYERCONTROLLER SCRIPT**

**Q1: How does player movement work and why is it grid-bounded?**

**A:**
```csharp
void HandleMovement()
{
    // Read keyboard input
    Vector2 input = Vector2.zero;
    if (kb.wKey.isPressed) input.y += 1f;
    if (kb.aKey.isPressed) input.x -= 1f;
    // ... etc
    
    // Apply movement with speed
    transform.position += (Vector3)(input * (moveSpeed * Time.deltaTime));
    
    // Clamp to screen bounds
    pos.x = Mathf.Clamp(pos.x, horizontalMin, horizontalMax);
    pos.y = Mathf.Clamp(pos.y, -verticalBound, verticalBound);
}
```

- **Direct position change**: Uses `transform.position` (not Rigidbody forces like tutorials)
- **Why**: 2D game needs direct control, Rigidbody forces would feel floaty
- **Bounds**: Prevents player leaving screen
  - `horizontalMin = -7.2f, horizontalMax = 1.5f`
  - `verticalBound = 4.5f` (both positive and negative Y)
- **Input normalization**: `if (input.sqrMagnitude > 1f) input.Normalize()` prevents faster diagonal movement

**Q2: Explain the aiming system and how it works with the mouse.**

**A:**
```csharp
void HandleAim()
{
    if (TryGetMouseWorldPosition(out Vector3 mouseWorld))
    {
        Vector2 mouseAim = mouseWorld - AimOrigin;
        if (mouseAim.sqrMagnitude > 0.001f)
            aimDirection = mouseAim.normalized;
    }

    float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
    // Rotate ship visual to face direction
    Quaternion target = Quaternion.Euler(0f, 0f, angle);
}
```

- **Mouse to World Conversion**: Screen mouse position → world coordinates
- **Direction Calculation**: Get normalized vector from player to mouse
- **Ship Rotation**: Uses `Atan2()` to calculate angle, convert radians to degrees
- **-90f offset**: Adjust for sprite orientation
- **Public property**: `public Vector2 AimDirection => aimDirection` lets other scripts use this

**Q3: How does the health and invincibility system work?**

**A:**
- `currentHealth` starts at `maxHealth` (default 5)
- When taking damage:
  1. `TakeDamage()` is called
  2. `invincible = true`, starts `invincibleTimer`
  3. Player flashes visually (DamageFlash component)
  4. Updates health bar UI
- `HandleInvincibility()` in Update:
  ```csharp
  if (invincible) {
      invincibleTimer -= Time.deltaTime;
      if (invincibleTimer <= 0f) invincible = false;
  }
  ```
- **Purpose**: Prevents instant death from multiple hits

**Q4: What does GetComponentsInChildren do and why use it here?**

**A:**
```csharp
visualRenderers = GetComponentsInChildren<SpriteRenderer>(true);
```

- Gets ALL SpriteRenderer components in this object and children
- The `true` parameter includes inactive objects
- **Tutorial Connection**: Similar to GetComponent but searches children instead
- **Usage**: When flashing on damage, all sprites flash simultaneously
- **Alternative**: Could manually reference each sprite, but this is cleaner

**Q5: Why have properties like `public int CurrentHealth => currentHealth`?**

**A:**
- Provides **read-only access** to private data
- Other scripts view health safely without modifying it directly
- UI can display `playerController.CurrentHealth`
- Enemies can check `if (player.CurrentHealth <= 0) {...}`
- Prevents bugs from external code accidentally modifying health

---

### **3. PLAYERSHOOTING SCRIPT**

**Q1: Explain the weapon system and how swapping works.**

**A:**
```csharp
public enum WeaponType { Pulse, Missile, Beam, Charge, Piercer }
WeaponType currentWeapon = WeaponType.Pulse;

void HandleWeaponSwitchInput()
{
    if (kb.eKey.wasPressedThisFrame) CycleWeapon();
}

void CycleWeapon()
{
    do {
        int nextIndex = ((int)currentWeapon + 1) % 5;
        currentWeapon = (WeaponType)nextIndex;
    } while (!IsWeaponUnlocked(currentWeapon));
}
```

- **System**: 5 different weapons, each with unique behavior
- **Unlocking**: Players unlock weapons through meta-progression
- **Cycling**: Press E to cycle through unlocked weapons
- **Default**: Always starts with Pulse (basic weapon)

**Q2: Explain the charge weapon mechanic.**

**A:**
```csharp
void HandleChargeWeapon(Mouse mouse)
{
    if (mouse.leftButton.isPressed && !wasHoldingFire) {
        chargeHoldTimer += Time.deltaTime;
        // Display charging indicator
    }
    
    if (!mouse.leftButton.isPressed && wasHoldingFire) {
        float chargeLevel = Mathf.Clamp01(chargeHoldTimer / maxChargeTime);
        FireChargeShot(chargeLevel);
        chargeHoldTimer = 0f;
    }
}
```

- **Hold to Charge**: Hold mouse button to build up energy
- **Release to Fire**: Power scales with hold time
- **Visual Feedback**: UI shows charge bar
- **Scaling**: Higher charge = more projectiles or higher damage

**Q3: What is cooldown and why boost it in the shooting system?**

**A:**
```csharp
float cooldownTimer;
float cooldownBoostMultiplier = 1f;

if (mouse.leftButton.isPressed && cooldownTimer <= 0f) {
    FireCurrentWeapon(1f);
    cooldownTimer = pulseCooldown / cooldownBoostMultiplier;
}
```

- **Cooldown**: Prevents spam shooting - wait time between shots
- **Boost**: Power-ups can increase firing rate
  - `cooldownBoostMultiplier = 1.5f` → fires 50% faster
- **Formula**: `cooldownTimer = pulseCooldown / multiplier`
  - Higher multiplier = shorter cooldown = faster firing

**Q4: How does the muzzle distance work when firing?**

**A:**
```csharp
public float muzzleDistance = 0.75f;
Vector3 firePosition = playerController.AimOrigin + 
                       (Vector3)playerController.AimDirection * muzzleDistance;
```

- **Purpose**: Projectile spawns slightly ahead of player ship
- **Prevents**: Projectile spawning inside player, causing instant collision
- **Visual**: Makes it look like bullet comes from gun barrel, not center

---

### **4. ENEMY SCRIPT**

**Q1: Explain the different enemy types and their behaviors.**

**A:**
```csharp
public enum EnemyType { Standard, Kamikaze, Turret, Shielded, Sniper }
```

| Type | Behavior | Health | Damage | Score |
|------|----------|--------|--------|-------|
| **Standard** | Baseline, moves in pattern | 1 | 1 | 2 |
| **Kamikaze** | Dives straight at player | 1 | 2 | 3 |
| **Turret** | Stationary, rapid fire | 2 | 1 | 4 |
| **Shielded** | Tough, high health | 3 | 1 | 5 |
| **Sniper** | Accurate single shots | 1 | 2 | 4 |

- **Archetype Pattern**: `ApplyArchetypeSettings()` sets stats per type
- **Progression**: Campaign introduces harder types in later levels

**Q2: How does enemy AI track and aim at the player?**

**A:**
```csharp
void AimAtPlayer()
{
    if (player == null) return;
    Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
    float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg - 90f;
    enemyVisual.rotation = Quaternion.Lerp(
        enemyVisual.rotation, 
        Quaternion.Euler(0, 0, angle), 
        Time.deltaTime * aimTurnSpeed
    );
}
```

- **Direction Vector**: Calculate direction to player
- **Angle Calculation**: Same as player aiming (Atan2)
- **Smooth Rotation**: Uses `Lerp()` instead of immediate snap → smoother visuals
- **Tutorial Connection**: Similar to PlayerController aiming!

**Q3: Explain the firing logic and range system.**

**A:**
```csharp
float distance = Vector3.Distance(transform.position, player.transform.position);
if (distance <= fireRange) {
    fireTimer -= Time.deltaTime;
    if (fireTimer <= 0f) {
        FireProjectile();
        fireTimer = Random.Range(fireIntervalMin, fireIntervalMax);
    }
}
```

- **Range Check**: Only fire if player within `fireRange`
- **Interval**: Random delay between `fireIntervalMin` and `fireIntervalMax`
- **Timer**: Counts down each frame, fires when reaches 0
- **Pattern**: Kamikaze has `fireRange = 0` (never shoots)

**Q4: What is the movement pattern and why use sine waves?**

**A:**
```csharp
void HandleMovement()
{
    // Scroll down with game speed
    transform.position += Vector3.down * GameManager.Instance.ScrollSpeed * Time.deltaTime;
    
    // Hover side to side
    float hoverAmount = Mathf.Sin((Time.time + hoverSeed) * hoverSpeed) * hoverRange;
    transform.localPosition = baseLocalPosition + Vector3.right * hoverAmount;
}
```

- **Scrolling**: Moves down with background (2D scroller pattern)
- **Sine Wave Hovering**: Smooth side-to-side motion
  - `Mathf.Sin()` produces smooth oscillation
  - `hoverSeed`: Random offset so enemies don't move in sync
  - `hoverSpeed` and `hoverRange`: Control frequency and amplitude
- **Result**: Natural, organic enemy movement

---

### **5. PROJECTILE SCRIPT**

**Q1: How do projectiles have a lifetime and when do they despawn?**

**A:**
```csharp
float projectileLifetime = 2.5f;
float lifeTimer;

void Update() {
    lifeTimer -= Time.deltaTime;
    if (lifeTimer <= 0f) {
        Destroy(gameObject);
    }
}
```

- **Purpose**: Prevents projectiles accumulating in memory forever
- **Approach**: Simple timer - create projectile, count down, destroy when timer reaches 0
- **Collision Fallback**: If projectile hits something before timer expires, destroy on collision
- **Tutorial Connection**: Similar to destroying collectables in Roll-a-Ball tutorial!

**Q2: Explain collision detection with enemies and obstacles.**

**A:**
```csharp
void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Enemy")) {
        collision.GetComponent<Enemy>().TakeDamage(damage);
        Destroy(gameObject);
    }
    else if (collision.CompareTag("Obstacle")) {
        Destroy(gameObject);
    }
}
```

- **Is Trigger Enabled**: Collider is ghost collider (like Roll-a-Ball tutorial!)
- **Tag System**: Check what was hit using tags
- **Action**: Deal damage to enemy OR destroy on obstacle
- **Inheritance**: Player and enemy projectiles inherit this behavior

---

### **6. GAMEBOTSTRAPPER & SCENE INITIALIZATION**

**Q1: What is GameBootstrapper and why is it needed?**

**A:**
- **Purpose**: Initializes game on scene load
- **Typical code**:
  ```csharp
  void Awake() {
      // Set up GameManager singleton
      // Spawn initial level
      // Set up UI
      // Load difficulty settings
  }
  ```
- **Why separate class**: Keeps initialization logic separate from gameplay logic
- **Pattern**: Common in larger games to organize startup sequence

**Q2: What happens on game start vs run reset?**

**A:**
- **Game Start**: Initialize everything (UI, GameManager, player position)
- **Run Reset**: Clear temporary data but keep meta-progression
  ```csharp
  score = 0;
  currentHealth = maxHealth;
  enemies.Clear();
  // BUT: Keep permanent unlocks, difficulty settings
  ```

---

### **7. UI SYSTEMS**

**Q1: How do different UI systems communicate with GameManager?**

**A:**
- **Pattern**: GameManager is the data source
- **UI Updates**: Subscribed to GameManager changes
  ```csharp
  // In HealthBarUI
  void Update() {
      healthBar.value = playerRef.CurrentHealth / (float)playerRef.MaxHealth;
  }
  ```
- **Data Flow**: GameManager → Player/Enemy → UI reads from Player/Enemy
- **Decoupling**: UI doesn't directly modify gameplay, only displays state

**Q2: Explain the upgrade selection UI system.**

**A:**
- **When triggered**: Player reaches score milestone
- **UI Panel**: Shows 3 upgrade choices
- **Selection**: Player clicks button to select one
- **Implementation**: GameManager pauses gameplay while UI is open
  - `IsUpgradeSelectionOpen = true` makes `IsGameplayActive = false`
  - All Update() checks skip if gameplay not active
- **Result**: Smooth pause and upgrade selection

---

### **8. AUDIO SYSTEM**

**Q1: How does the audio system manage sound effects and music?**

**A:**
- **Singleton Pattern**: `GameAudio.Instance`
- **Methods**: PlaySFX(), PlayMusic(), StopMusic()
- **Integration**: Called from PlayerShooting, Enemy, Explosion effects
- **Music Management**: Different tracks per level/mode
- **Tutorial Connection**: Not covered in tutorials but follows same singleton pattern

---

### **9. CONNECTING TO TUTORIALS**

| Tutorial Concept | How It's Used in Project |
|------------------|-------------------------|
| **Movement** | PlayerController extends tutorial movement with bounds and aiming |
| **Rigidbody Physics** | Not used (direct position changes appropriate for 2D game) |
| **Colliders & Triggers** | Projectiles use triggers (OnTriggerEnter2D) just like tutorial |
| **Tags** | Tag system for identifying enemy types, obstacles, projectiles |
| **Singleton Pattern** | GameManager follows exact tutorial pattern but with game states |
| **Input Handling** | New Input System instead of old Input.GetKey() |
| **Destroy/Instantiate** | Used for projectiles and enemies (prefab-based) |
| **Update Loop** | Organized with IsGameplayActive checks |
| **Component References** | GetComponent, GetComponentsInChildren patterns |

---

### **10. DESIGN PATTERNS USED**

| Pattern | Where | Purpose |
|---------|-------|---------|
| **Singleton** | GameManager, GameAudio | Global state management |
| **Observer** | UI updates on score change | Decoupled systems |
| **Factory** | Enemy spawner creates different types | Flexible object creation |
| **State Machine** | Game modes, game stages | Manage different game states |
| **Pool** | Projectile recycling | Performance optimization |

---

## VIVA TIPS

### **Strong Answers**
✅ Explain WHY, not just WHAT
✅ Show how tutorial concepts evolved to your project
✅ Connect to actual code snippets
✅ Explain design decision reasoning
✅ Show understanding of performance considerations

### **Example Strong Answer**
**Q: Why do enemies move with sine waves instead of straight lines?**

**A:** "The enemies use Mathf.Sin() for hovering movement because:
1. Sine waves create smooth, natural oscillation
2. We seed it with `hoverSeed` so each enemy has different timing, preventing all enemies from moving in sync
3. It's more engaging visually than linear back-and-forth
4. Computationally cheap - just one trigonometric function per frame
5. The amplitude controls intensity while frequency controls speed
This follows game design best practices for AI movement patterns."

### **Avoid**
❌ Just reading code line-by-line
❌ Saying "I don't know" without attempting to reason through it
❌ Forgetting to relate things to tutorials
❌ Not explaining performance or design choices

---

## POTENTIAL VIVA QUESTIONS

1. **Architecture**: How is your game structured? What's the purpose of each major system?
2. **GameManager**: Explain the singleton pattern and why it's better than FindObjectOfType()
3. **Gameplay Flow**: How do game modes differ? What state variables track this?
4. **Enemy AI**: How do enemies detect and shoot the player?
5. **Collision**: How do projectiles detect hits? What's the Is Trigger pattern?
6. **Tutorial Reflection**: How did you extend the Roll-a-Ball concepts to this 2D shooter?
7. **Optimization**: How do you prevent memory leaks? (Answer: projectile lifetime system)
8. **Input Handling**: Why use New Input System instead of old Input class?
9. **Performance**: What's the performance impact of your spawning system?
10. **Difficulty Scaling**: How does difficulty increase across campaign stages?

---

**GOOD LUCK WITH YOUR VIVA!** 🚀

Remember: The panel wants to see you understand your code deeply, not memorize it. Be ready to explain decisions and trade-offs.
