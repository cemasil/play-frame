# Development to Store — Complete Workflow

End-to-end guide: from starting a new game to publishing on iOS App Store and Google Play.

---

## Table of Contents

1. [Phase 1: Project Setup](#phase-1-project-setup)
2. [Phase 2: Game Design & Prototyping](#phase-2-game-design--prototyping)
3. [Phase 3: Core Development](#phase-3-core-development)
4. [Phase 4: Art & Audio Integration](#phase-4-art--audio-integration)
5. [Phase 5: Localization](#phase-5-localization)
6. [Phase 6: Monetization](#phase-6-monetization)
7. [Phase 7: Analytics & Tracking](#phase-7-analytics--tracking)
8. [Phase 8: Testing & QA](#phase-8-testing--qa)
9. [Phase 9: Performance Optimization](#phase-9-performance-optimization)
10. [Phase 10: Build & Release](#phase-10-build--release)
11. [Phase 11: Store Submission](#phase-11-store-submission)
12. [Phase 12: Post-Launch](#phase-12-post-launch)
13. [Quick Reference Checklists](#quick-reference-checklists)

---

## Phase 1: Project Setup

### 1.1 Clone PlayFrame

```bash
git clone [playframe-repo] MyNewGame
cd MyNewGame
```

Open in Unity (6000.x LTS recommended).

### 1.2 Configure Identity

**Tools → PlayFrame → Project Setup Wizard**

| Field | Example |
|-------|---------|
| Product Name | Puzzle Match |
| Company Name | MyStudio |
| Bundle ID Suffix | puzzlematch |
| Platforms | iOS ✓, Android ✓ |

Click **Apply Settings** — this configures PlayerSettings, platform targets, IL2CPP, and bundle identifiers.

### 1.3 Create Build Configs

**Tools → PlayFrame → Build → Create Build Configs**

Creates `BuildConfig_Dev.asset` and `BuildConfig_Prod.asset` in `Assets/GameSettings/`.

### 1.4 Create Core Scenes

**Assets → Create → PlayFrame → Scene →**
1. **Bootstrap Scene** — all manager singletons (set as scene index 0)
2. **MainMenu Scene** — title screen
3. **Loading Scene** — transition screen
4. **Game Scene** — your game scene(s)

### 1.5 Source Control

```bash
git init
git add .
git commit -m "Initial PlayFrame setup for [GameName]"
```

Ensure `.gitignore` excludes: `Library/`, `Temp/`, `Logs/`, `Builds/`, `*.csproj`, `*.sln`.

### 1.6 Branching Strategy

| Branch | Purpose |
|--------|---------|
| `main` | Production-ready code |
| `develop` | Integration branch, daily work |
| `feature/[name]` | Individual features |
| `release/[version]` | Release candidates |
| `hotfix/[issue]` | Emergency fixes |

---

## Phase 2: Game Design & Prototyping

### 2.1 Game Design Document (GDD)

Create `docs/GDD.md` covering:

- **Core loop**: What does the player do repeatedly?
- **Win/lose conditions**: When does a level end?
- **Progression**: How do levels get harder?
- **Scoring**: What earns points?
- **Interaction mode**: Tap? Swap? Chain? (See SYSTEMS_DOCUMENTATION.md)
- **Target audience**: Age group, casual/hardcore
- **Monetization plan**: Ads, IAP, premium

### 2.2 Rapid Prototype

1. Create a `BaseGame` subclass for your game
2. Configure a `GridConfig` with basic settings
3. Use colored sprites or Unity's default sprite for piece placeholders
4. Implement core interaction in event handlers
5. Test the core loop — is it fun with placeholder art?

### 2.3 Evaluate

Before investing in art/audio, validate:

- [ ] Core mechanic is fun
- [ ] Level difficulty curve is clear
- [ ] Session length is appropriate (2-5 min for mobile)
- [ ] No critical design flaws

---

## Phase 3: Core Development

### 3.1 Game Logic

```csharp
public class MyGame : BaseGame
{
    [SerializeField] private GridManager gridManager;

    protected override async void OnGameStart()
    {
        // Validate required components
        if (gridManager.GetComponent<GridInputHandler>() == null)
        {
            Debug.LogError("GridInputHandler missing on GridManager!");
            return;
        }

        gridManager.OnConfigurePiece += ConfigurePiece;
        gridManager.OnPiecesSwapped += OnSwap;  // or whichever mode
        gridManager.InitializeGrid();
        await gridManager.FillGridAsync();
    }
}
```

### 3.2 Level System

If your game has levels:

- Create level data as ScriptableObjects or JSON
- Use `GridConfig.boardLayout` for deterministic piece placement
- Track progress via `SaveManager`
- Load next level by reading next config

### 3.3 UI Panels

Use existing panels or create new ones:

- **GameObject → PlayFrame → Panel → [Type]** for built-in panels
- `SettingsPanel`: always include
- `PausePanel`: always include for game scenes
- `LevelCompletePanel` / `LevelFailedPanel`: if game has levels
- `TutorialPanel`: for first-time user experience

### 3.4 Save System Integration

```csharp
// Save high score
SaveManager.Instance.UpdateGameHighScore("PuzzleMatch", score);

// Save level progress
SaveManager.Instance.SetGameData("PuzzleMatch", "level", currentLevel.ToString());

// Load
int level = int.Parse(SaveManager.Instance.GetGameData("PuzzleMatch", "level") ?? "1");
```

---

## Phase 4: Art & Audio Integration

### 4.1 Art Pipeline

See [ASSET_MANAGEMENT_GUIDE.md](ASSET_MANAGEMENT_GUIDE.md) for detailed import settings.

1. Organize: `Assets/Art/Sprites/[GameName]/`
2. Import sprites with correct settings (ASTC, no mipmaps, no read/write)
3. Create Sprite Atlases for batching
4. Assign sprites to `GridConfig.pieceSprites`
5. Use Board Layout Designer for deterministic placement

### 4.2 Audio Pipeline

1. Music → `Assets/Audio/Music/` (OGG, streaming)
2. SFX → `Assets/Audio/SFX/` (WAV for short, OGG for long)
3. Create `SFXCollection` and `MusicCollection` ScriptableObjects
4. Configure `GridAudioConfig` for grid events (place, match, swap, chain)
5. Wire `AudioManager` in Bootstrap scene

### 4.3 VFX

- Keep effects simple for mobile
- Pool particle systems for match/destroy effects
- Test on lowest target device

---

## Phase 5: Localization

### 5.1 Setup

1. Add all UI text keys to `LocalizationKeys.cs`
2. Create `LocalizedStringTable` assets for each language
3. Replace all hardcoded UI text with `LocalizedText` components
4. Test each language by switching in Settings

### 5.2 Recommended Initial Languages

| Priority | Languages | Market Coverage |
|----------|-----------|----------------|
| P0 (launch) | English | ~40% global |
| P1 (week 1) | Turkish, Spanish, Portuguese | +15% |
| P2 (month 1) | German, French, Japanese, Korean | +20% |
| P3 (later) | Chinese (Simplified), Russian, Arabic | +15% |

### 5.3 Translation Tips

- Keep strings short — mobile screens are small
- Don't concatenate strings — use format args: `Get("SCORE_FORMAT", score)`
- Account for text expansion: German/French can be 30% longer than English
- RTL support: Arabic/Hebrew require special handling (consider later)
- Test with longest translation to catch overflow

---

## Phase 6: Monetization

### 6.1 Strategy Options

| Model | Pros | Cons |
|-------|------|------|
| Free + Ads | Large audience, simple | Revenue depends on volume |
| Free + IAP | Higher revenue per user | Requires balancing |
| Free + Ads + IAP | Maximum revenue | Complex, balance crucial |
| Premium (paid) | No ads/IAP needed | Small audience on mobile |

### 6.2 Ad Integration

1. Install mediation SDK (AdMob, ironSource, AppLovin)
2. Create ad unit IDs for each format:
   - Banner (bottom of screen)
   - Interstitial (between levels)
   - Rewarded (optional rewards)
3. Implement GDPR consent (EU requirement)
4. Implement ATT consent (iOS) — `IOSTrackingPermission`
5. Test with sandbox ads before going live

### 6.3 IAP Integration

1. Install Unity IAP or platform-native SDK
2. Configure products in store consoles
3. Use `IAPPanel` for store UI
4. Implement receipt validation (server-side recommended)
5. Test with sandbox accounts

### 6.4 IAP Product Ideas

| Product | Type | Price Range |
|---------|------|-------------|
| Remove Ads | Non-consumable | $2.99-4.99 |
| Coin Pack (Small) | Consumable | $0.99 |
| Coin Pack (Large) | Consumable | $4.99 |
| Premium Bundle | Non-consumable | $9.99 |
| Extra Lives | Consumable | $0.99-1.99 |

---

## Phase 7: Analytics & Tracking

### 7.1 Key Events to Track

| Event | When | Data |
|-------|------|------|
| `level_start` | Level begins | level_number, difficulty |
| `level_complete` | Level won | level_number, score, time, moves |
| `level_fail` | Level lost | level_number, reason, time |
| `level_retry` | Player retries | level_number, retry_count |
| `game_pause` | Player pauses | current_state |
| `tutorial_complete` | Tutorial finished | step_count |
| `iap_initiated` | Purchase started | product_id |
| `iap_completed` | Purchase done | product_id, revenue |
| `ad_impression` | Ad viewed | ad_type, placement |

### 7.2 PlayFrame Integration

`BaseGame` has built-in analytics methods:

```csharp
TrackLevelStart(level, difficulty, metadata);
TrackLevelComplete(level, score, time, moves, stars, metadata);
TrackLevelFail(level, reason, score, time, metadata);
TrackLevelRetry(level);
```

### 7.3 Analytics Provider Setup

Configure in `AnalyticsSettings` ScriptableObject:
- Console Logger (development)
- Local Storage (offline)
- Unity Analytics
- Firebase Analytics (recommended for production)

---

## Phase 8: Testing & QA

### 8.1 Test Matrix

| Dimension | Coverage |
|-----------|----------|
| Devices | At least 3 iOS + 3 Android (old, mid, new) |
| OS Versions | iOS 15, 16, 17+ / Android 10, 12, 14+ |
| Screen Sizes | Small phone, large phone, tablet |
| Network | WiFi, 4G, airplane mode, slow network |
| Languages | All supported languages |
| Orientations | Portrait (primary), check no crash on rotation |

### 8.2 Test Types

| Type | What | When |
|------|------|------|
| Unit Tests | Core logic (match finding, scoring) | During development |
| Integration Tests | System interactions (grid + audio + save) | After feature complete |
| Playtest | Full gameplay flow | Every milestone |
| Regression | Ensure old features still work | Before each build |
| Performance | FPS, memory, battery | Before release |
| Localization | All strings display correctly | Before release |
| Store Compliance | Guidelines met | Before submission |

### 8.3 Device Testing

**iOS:** TestFlight internal testing (up to 100 testers, instant access)
**Android:** Play Console internal testing track

### 8.4 Common Issues Checklist

- [ ] App doesn't crash on launch
- [ ] Back button works (Android)
- [ ] App resumes correctly after backgrounding
- [ ] Audio stops when app is backgrounded
- [ ] Audio resumes when app is foregrounded
- [ ] Save data persists across app restart
- [ ] No memory leaks (play 20+ levels continuously)
- [ ] No UI overlap on any aspect ratio
- [ ] Text doesn't overflow in any language
- [ ] Permissions handled gracefully when denied

---

## Phase 9: Performance Optimization

### 9.1 Target Metrics

| Metric | Target | Acceptable |
|--------|--------|------------|
| FPS | 60 | 30 (minimum) |
| RAM | <200MB | <300MB |
| CPU per frame | <16ms | <33ms |
| Battery drain | <15%/hour | <25%/hour |
| Startup time | <3s | <5s |
| Install size | <100MB | <150MB |

### 9.2 Optimization Order

1. **Profile first** — don't guess. Use Unity Profiler.
2. **GPU**: Reduce overdraw, batch sprites (atlases), minimize shader complexity
3. **CPU**: Cache GetComponent results, avoid LINQ in hot paths, use object pooling
4. **Memory**: Compress textures, stream audio, unload unused assets
5. **GC**: Avoid allocations in Update/FixedUpdate, use struct over class for small data
6. **Build size**: See [ASSET_MANAGEMENT_GUIDE.md](ASSET_MANAGEMENT_GUIDE.md)

### 9.3 Unity Profiler Usage

```
Window → Analysis → Profiler
```

1. Connect to device via USB (iOS) or WiFi (Android)
2. Enable Deep Profiling only when needed (heavy overhead)
3. Look for: GC.Alloc spikes, high draw calls, slow scripts
4. Target: 0 GC allocations per frame during gameplay

---

## Phase 10: Build & Release

### 10.1 Version Planning

```
Development:  1.0.0-dev (internal builds)
Alpha:        0.x.x (feature incomplete)
Beta:         1.0.0-beta (feature complete, testing)
RC:           1.0.0-rc (release candidate)
Release:      1.0.0 (store submission)
Hotfix:       1.0.1 (bug fixes)
Update:       1.1.0 (new content)
```

### 10.2 Build Process

1. **Bump version:** `Tools → PlayFrame → Build → Increment [Patch/Minor/Major]`
2. **Dev build for testing:** `Build [Platform] Dev`
3. **Test thoroughly** on real devices
4. **Production build:** `Build [Platform] Prod`
5. **Verify**: Install production build on device, test critical paths

### 10.3 Release Checklist

- [ ] All features working in production build
- [ ] No debug UI / test buttons visible
- [ ] No placeholder art or text
- [ ] Version string correct
- [ ] Bundle ID correct (no `.dev` suffix)
- [ ] Code signing valid
- [ ] Privacy policy page live
- [ ] Store listing complete (screenshots, description, etc.)

---

## Phase 11: Store Submission

See [STORE_PREPARATION_GUIDE.md](STORE_PREPARATION_GUIDE.md) for detailed store-specific requirements.

### 11.1 iOS Submission Flow

```
Build iOS Prod → Open in Xcode → Archive → Upload to App Store Connect
→ Fill metadata & screenshots → Submit for Review → Wait (24-48h typical)
→ Approved → Release
```

### 11.2 Android Submission Flow

```
Build Android Prod → Upload AAB to Play Console
→ Fill listing & data safety → Internal/Closed testing → Open testing (optional)
→ Production release → Review (hours to days) → Published
```

### 11.3 Simultaneous Release Strategy

| Step | iOS | Android |
|------|-----|---------|
| 1. Internal test | TestFlight | Internal track |
| 2. Beta test | TestFlight (external) | Closed/Open testing |
| 3. Submit | App Store Review | Production release |
| 4. Schedule | Schedule release date | Managed/Timed release |
| 5. Launch | Same day release | Same day release |

Set both stores to "Manually release" (not "Automatically after approval") so you can time them.

---

## Phase 12: Post-Launch

### 12.1 First Week

- [ ] Monitor crash rates (target: <0.1%)
- [ ] Monitor ANR rates (Android, target: <0.5%)
- [ ] Respond to user reviews
- [ ] Track retention metrics (Day 1, Day 7)
- [ ] Track monetization metrics (ARPU, conversion rate)

### 12.2 Ongoing

| Cadence | Activity |
|---------|----------|
| Daily | Check crash reports, critical reviews |
| Weekly | Review analytics dashboard, plan fixes |
| Bi-weekly | Content update or bug fix release |
| Monthly | Feature update, ASO optimization |
| Quarterly | Major update, new content/features |

### 12.3 Key Metrics to Watch

| Metric | Good | Needs Work |
|--------|------|------------|
| Day 1 retention | >40% | <25% |
| Day 7 retention | >20% | <10% |
| Day 30 retention | >10% | <5% |
| Average session | >5 min | <2 min |
| Crash rate | <0.1% | >0.5% |
| Rating | >4.0 | <3.5 |
| ARPU (ads) | >$0.02/day | <$0.005/day |

---

## Quick Reference Checklists

### New Game Kickoff

```
1. Clone PlayFrame → Open in Unity
2. Tools → PlayFrame → Project Setup Wizard
3. Tools → PlayFrame → Build → Create Build Configs
4. Assets → Create → PlayFrame → Scene → Bootstrap (once)
5. Assets → Create → PlayFrame → Scene → Game Scene
6. Create BaseGame subclass
7. Create GridConfig, assign piece sprites
8. Implement interaction handlers
9. Test core loop
```

### Pre-Release

```
1. All features tested on real devices
2. Performance profiled and optimized
3. Localization complete and verified
4. Analytics events verified
5. Version bumped (Patch/Minor/Major)
6. Production build created
7. Store listing complete
8. Privacy policy live
9. Screenshots uploaded
10. Submitted to stores
```

### Post-Update

```
1. Bump version: Tools → PlayFrame → Build → Increment Patch
2. Build production: Build [Platform] Prod
3. Test critical paths on device
4. Upload to stores
5. Add "What's New" text
6. Submit update
7. Monitor crashes for 24h after rollout
```
