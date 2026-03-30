@echo off
title IT22361004 - 2D Game Setup
color 0B

echo ============================================================
echo   IT22361004 - 2D Interactive Game (SE4031 Assignment 02)
echo ============================================================
echo.
echo  This script helps you set up the game project in Unity.
echo.
echo  STEP 1: Open Unity Hub
echo  STEP 2: Add the project folder "IT22361004" from this directory
echo  STEP 3: Open the project (Unity 6000.3.10f1 required)
echo  STEP 4: Wait for Unity to compile all scripts
echo  STEP 5: If prompted, import TMP Essentials (TextMeshPro)
echo  STEP 6: In Unity menu bar, click: IT22361004 ^> Setup Game Scene
echo  STEP 7: Unity may restart to apply input settings - if so,
echo          run the setup menu again after restart
echo  STEP 8: Press the PLAY button to start the game!
echo.
echo ============================================================
echo   CONTROLS
echo ============================================================
echo.
echo   W / Up Arrow    - Move player UP
echo   S / Down Arrow  - Move player DOWN
echo   Left Click      - Shoot projectile toward mouse
echo.
echo ============================================================
echo   GAME FEATURES (All 7 Core Requirements)
echo ============================================================
echo.
echo   1. HEALTH SYSTEM   - Health bar with gradient colors (green
echo                         to red). Decreases on obstacle/enemy
echo                         hit. Resets on restart.
echo.
echo   2. SCORE SYSTEM     - Score displayed on UI. Increments when
echo                         passing through obstacle gaps. Resets
echo                         on restart.
echo.
echo   3. HEALTH PACKS     - Green circles in obstacle gaps. Heal
echo                         player on contact. Destroyed when
echo                         collected or scrolled off-screen.
echo.
echo   4. PROJECTILE ATTACK- Left click shoots yellow projectile
echo                         toward mouse. Destroyed on contact
echo                         with obstacles/enemies or after timeout.
echo.
echo   5. ENEMIES          - Red diamond shapes. Damage player on
echo                         contact. Destroyed by projectiles for
echo                         bonus score (+2). Visually distinct
echo                         from health packs.
echo.
echo   6. SPEED INCREASE   - Game speed gradually increases over
echo                         time, capped at max speed. Displayed
echo                         on UI.
echo.
echo   7. CREATIVE FEATURE - Gradient health bar that smoothly
echo                         transitions colors. Screen shake on
echo                         damage. Wall colors change based on
echo                         player health. Red flash overlay on hit.
echo.
echo ============================================================
echo   PROJECT STRUCTURE
echo ============================================================
echo.
echo   My project/Assets/Scripts/
echo     - GameManager.cs       (game state, score, speed)
echo     - PlayerController.cs  (movement, health, damage)
echo     - PlayerShooting.cs    (projectile spawning)
echo     - ObstacleSpawner.cs   (obstacle generation)
echo     - ObstacleWall.cs      (wall collision)
echo     - ScrollingObject.cs   (left-scrolling movement)
echo     - ScoreTrigger.cs      (gap score detection)
echo     - HealthPack.cs        (health pickup)
echo     - Enemy.cs             (enemy behavior)
echo     - Projectile.cs        (projectile behavior)
echo     - HealthBarUI.cs       (gradient health bar)
echo     - ScreenShake.cs       (camera shake effect)
echo     - DamageFlash.cs       (red flash overlay)
echo     - SpriteHelper.cs      (runtime sprite generation)
echo     - Editor/GameSetupEditor.cs (one-click scene setup)
echo.
echo ============================================================
echo.

REM Try to find Unity Hub and open it
if exist "C:\Program Files\Unity Hub\Unity Hub.exe" (
    echo Opening Unity Hub...
    start "" "C:\Program Files\Unity Hub\Unity Hub.exe"
) else (
    echo Unity Hub not found at default location.
    echo Please open Unity Hub manually and add this project.
)

echo.
pause
