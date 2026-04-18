# Asset & Package Management Guide

Best practices for managing images, audio, VFX, and packages in PlayFrame projects.

---

## Table of Contents

1. [Folder Structure](#1-folder-structure)
2. [Image Assets](#2-image-assets)
3. [Audio Assets](#3-audio-assets)
4. [VFX & Particles](#4-vfx--particles)
5. [Sprite Atlases](#5-sprite-atlases)
6. [Addressables vs Resources](#6-addressables-vs-resources)
7. [Package Management](#7-package-management)
8. [Build Size Optimization](#8-build-size-optimization)
9. [Import Settings Checklist](#9-import-settings-checklist)

---

## 1. Folder Structure

```
Assets/
├── Art/
│   ├── Sprites/          ← Game sprites (pieces, icons, backgrounds)
│   │   ├── Common/       ← Shared across games
│   │   └── [GameName]/   ← Per-game sprites
│   ├── UI/               ← UI-specific graphics (buttons, frames, panels)
│   └── Atlases/          ← Sprite Atlas assets (.spriteatlas)
├── Audio/
│   ├── Music/            ← Background music (*.ogg recommended)
│   ├── SFX/              ← Sound effects (*.wav for short, *.ogg for long)
│   └── Collections/      ← SFXCollection / MusicCollection ScriptableObjects
├── VFX/
│   ├── Particles/        ← Particle systems
│   ├── Shaders/          ← Custom shaders
│   └── Materials/        ← VFX materials
├── Prefabs/
│   ├── UI/               ← Panel prefabs
│   └── Game/             ← Game-specific prefabs (pieces, effects)
├── GameSettings/         ← ScriptableObject configs (GridConfig, BuildConfig, etc.)
├── Resources/            ← ONLY for assets that MUST be loaded by name at runtime
├── Scenes/
└── Scripts/
```

**Rules:**
- Never put large assets in `Resources/` — they are always included in builds
- Keep `Resources/` for small essential configs only
- One folder per game under `Art/Sprites/[GameName]/`

---

## 2. Image Assets

### Import Settings

| Setting | Sprites | UI | Backgrounds |
|---------|---------|-----|-------------|
| Texture Type | Sprite | Sprite | Sprite |
| Max Size | 512-1024 | 256-512 | 1024-2048 |
| Compression | ASTC 6x6 (mobile) | ASTC 6x6 | ASTC 8x8 |
| Generate Mip Maps | OFF | OFF | OFF |
| Read/Write | OFF | OFF | OFF |
| Filter Mode | Bilinear | Bilinear | Bilinear |
| Sprite Mode | Single / Multiple | Single / 9-Slice | Single |

### Size Guidelines

| Asset Type | Recommended Max Size | Notes |
|-----------|---------------------|-------|
| Game piece | 128×128 – 256×256 | Pack into atlas |
| Icon | 64×64 – 128×128 | Pack into atlas |
| UI button | 128×64 – 256×128 | 9-slice when possible |
| Background | 1080×1920 | Full screen, compress aggressively |
| Particle texture | 64×64 – 128×128 | Alpha-heavy, use ASTC |

### Best Practices

- **Use 9-slice** for buttons, panels, frames → smaller textures that scale
- **Use Sprite Atlases** — single draw call for multiple sprites
- **Power of 2 textures** are not strictly required for UI sprites but help compression
- **Remove unused sprites** — check with `Tools → PlayFrame → Build → Unused Asset Report` (or use Build Report Analyzer package)
- **Don't import PSD/AI files** — export PNG/SVG only
- **Use single-channel textures** for masks / patterns (R8 format)

---

## 3. Audio Assets

### Format Guidelines

| Type | Format | Sample Rate | Channels | Compression |
|------|--------|------------|----------|-------------|
| Music | .ogg (Vorbis) | 44100 Hz | Stereo | Quality 70% |
| Short SFX (<2s) | .wav | 22050–44100 Hz | Mono | Decompress on Load |
| Long SFX (>2s) | .ogg | 22050 Hz | Mono | Compressed in Memory |
| UI sounds | .wav | 22050 Hz | Mono | Decompress on Load |

### Import Settings

| Setting | Music | Short SFX | Long SFX |
|---------|-------|-----------|----------|
| Force To Mono | No | Yes | Yes |
| Load Type | Streaming | Decompress on Load | Compressed in Memory |
| Compression | Vorbis (70%) | ADPCM | Vorbis (50%) |
| Quality | 70 | - | 50 |
| Sample Rate | Preserve | Optimize | Optimize |

### Best Practices

- **Force SFX to Mono** — halves memory, unnoticeable on mobile speakers
- **Stream music** — don't load 5MB tracks into memory
- **Short SFX → Decompress on Load** — no CPU cost during playback
- **Avoid WAV for music** — 10× larger than OGG, no quality benefit on mobile
- **Keep loops clean** — ensure loop points are at zero-crossings
- **Normalize audio** to -3dB peak to prevent clipping when mixing

---

## 4. VFX & Particles

### Guidelines

- **Use UI Particle System** (use `ParticleSystemRenderer` with `Canvas` sorting) for in-game UI effects
- **Keep particle counts low**: mobile target ≤ 50-100 active particles per system
- **Shared materials**: reuse materials across similar particle effects
- **Simple shaders**: avoid transparent/alpha-blended overdraw — use additive blending
- **Pool particle systems**: don't Instantiate/Destroy constantly

### Memory Tips

- Particle textures: 64×64 to 128×128, no larger
- Use `Stop Action: Disable` instead of `Destroy` for pooling
- Pre-warm only if absolutely needed (costs performance on spawn)

---

## 5. Sprite Atlases

### Creating Atlases

1. **Right-click in Project → Create → 2D → Sprite Atlas**
2. Name it: `Atlas_[Category]` (e.g., `Atlas_GamePieces`, `Atlas_UI`)
3. Drag folders or individual sprites into the Objects list
4. Set Max Texture Size: 2048 (mobile) or 4096 (tablets)

### Recommended Atlas Groups

| Atlas | Contents | Max Size |
|-------|----------|----------|
| `Atlas_UI` | Buttons, icons, panel frames | 2048 |
| `Atlas_GamePieces` | Grid pieces per game | 1024 |
| `Atlas_Common` | Shared UI elements | 2048 |
| Per-game atlas | Game-specific assets | 1024-2048 |

### Best Practices

- **One atlas per category**, not one giant atlas
- **Don't mix always-loaded and rarely-loaded** sprites in the same atlas
- **Include → Enable Tight Packing** for better atlas utilization
- **Padding**: 2px minimum to prevent edge bleeding
- Check atlas packing in **Window → 2D → Sprite Atlas Preview**

---

## 6. Addressables vs Resources

### Resources Folder

**Use only for:** Settings that must load before anything else (e.g., `LogSettings`, boot configs).

**Problems with Resources:**
- ALL assets in `Resources/` are included in every build, even if unused
- Cannot be unloaded until the scene changes
- No async loading control
- Grows build size silently

### Addressables (Recommended for Large Projects)

Install via Package Manager: `com.unity.addressables`

**Use for:**
- Per-game assets that load only when needed
- Downloadable content (DLC)
- Assets referenced by name/key instead of direct reference
- Large asset sets (100+ sprites, audio files)

**For PlayFrame:** Direct references in Inspector are sufficient for most mobile games. Switch to Addressables when:
- Build size exceeds 150MB
- You need downloadable content
- You have 5+ mini-games with unique asset sets

---

## 7. Package Management

### Installed Packages

Check `Packages/manifest.json` for current packages. Remove unused ones.

### Audit Checklist

| Check | Action |
|-------|--------|
| Unused packages | Remove from manifest.json |
| Package updates | Update non-breaking changes only |
| Preview packages | Avoid in production builds |
| Package dependencies | Check transitive deps for bloat |
| Native plugins | Check per-platform inclusion (iOS/Android only) |

### Common Packages & Sizes

| Package | Size Impact | Needed? |
|---------|-------------|---------|
| TextMeshPro | ~5MB | YES — required |
| UniTask | ~200KB | YES — async/await |
| Unity UI | ~2MB | YES — UI system |
| Cinemachine | ~3MB | Only if using camera animation |
| Post Processing | ~1MB | Only if using visual effects |
| Analytics | ~1MB | If using Unity analytics |
| Ads (mediation) | 10-50MB | Only for ad-supported games |

### Removing a Package

1. Remove from `Packages/manifest.json`
2. Delete any references in code (will show compile errors)
3. Delete cached data: `Library/PackageCache/[package-name]`
4. Reimport: `Assets → Reimport All`

---

## 8. Build Size Optimization

### Quick Wins

| Action | Typical Savings |
|--------|----------------|
| Remove unused scenes from Build Settings | 1-10MB |
| Compress textures (ASTC) | 30-60% texture size |
| Force SFX to mono | 50% audio memory |
| Stream music instead of loading | 5-20MB RAM |
| Remove unused packages | 1-10MB |
| Strip unused Engine code | 5-15MB |
| Enable IL2CPP code stripping | 2-5MB |

### Unity Settings

```
Edit → Project Settings → Player:
├── Strip Engine Code: YES
├── Managed Stripping Level: Medium (Prod) / Minimal (Dev)
├── Scripting Backend: IL2CPP
└── Target Architecture: ARM64 only (drop ARMv7)
```

### Build Report

After building, check **Console → Build Report** or use the **Build Report Inspector** package for detailed size analysis by category.

### Size Budget (Mobile Games)

| Category | Target | Limit |
|----------|--------|-------|
| Total install size | <100MB | <150MB |
| Textures | <30MB | <50MB |
| Audio | <15MB | <25MB |
| Code (IL2CPP) | <15MB | <25MB |
| Scenes | <5MB | <10MB |
| Other (plugins, etc.) | <10MB | <20MB |

---

## 9. Import Settings Checklist

Run through this for every new asset added to the project:

### Textures / Sprites
- [ ] Texture Type = Sprite (2D and UI)
- [ ] Generate Mip Maps = OFF
- [ ] Read/Write Enabled = OFF
- [ ] Max Size appropriate (128-1024 for sprites, 2048 max for backgrounds)
- [ ] Compression = ASTC (iOS/Android)
- [ ] Added to appropriate Sprite Atlas
- [ ] No duplicate / unused sprites

### Audio
- [ ] SFX: Force To Mono = YES
- [ ] SFX (<2s): Load Type = Decompress On Load, Compression = ADPCM
- [ ] SFX (>2s): Load Type = Compressed In Memory, Compression = Vorbis
- [ ] Music: Load Type = Streaming, Compression = Vorbis (70%)
- [ ] Sample Rate = Optimize or 22050
- [ ] No WAV files for music (use OGG/Vorbis)

### Prefabs
- [ ] No unused components
- [ ] No disabled but included scripts
- [ ] Image raycastTarget = OFF where not needed (performance)
