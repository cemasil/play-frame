# Store Preparation Guide

Complete checklist for publishing PlayFrame games to iOS App Store and Google Play Store.

---

## Table of Contents

1. [Pre-Submission Checklist](#1-pre-submission-checklist)
2. [iOS App Store](#2-ios-app-store)
3. [Google Play Store](#3-google-play-store)
4. [Common Requirements](#4-common-requirements)
5. [App Store Optimization (ASO)](#5-app-store-optimization-aso)
6. [Post-Launch](#6-post-launch)

---

## 1. Pre-Submission Checklist

Complete these before building for any store:

### Unity Project

- [ ] Version bumped: `Tools → PlayFrame → Build → Increment [Patch/Minor/Major]`
- [ ] Build number incremented (automatic on build)
- [ ] Correct Bundle Identifier set (`Tools → PlayFrame → Project Setup Wizard`)
- [ ] No development build flags in production config
- [ ] IL2CPP scripting backend (not Mono)
- [ ] ARM64 architecture (drop ARMv7)
- [ ] Code stripping enabled (Medium or High)
- [ ] All Debug.Log calls wrapped in `#if UNITY_EDITOR` or using PlayFrame Logger with proper log levels
- [ ] All test scenes removed from Build Settings
- [ ] Build Report checked — no unexpected large assets

### Game Quality

- [ ] All features tested on real devices (not just editor)
- [ ] Performance tested: stable 60 FPS or 30 FPS minimum
- [ ] Memory usage: under 300MB on target devices
- [ ] No crashes on cold start
- [ ] No ANR (Application Not Responding) — no blocking main thread
- [ ] Audio works correctly (volume, mute persists)
- [ ] Save data persists across app restart
- [ ] Localization tested for all supported languages
- [ ] UI fits all aspect ratios (18:9, 19.5:9, 20:9, iPad 4:3)
- [ ] Safe area works (notch, home indicator, camera cutout)

---

## 2. iOS App Store

### Developer Account Requirements

| Requirement | Details |
|------------|---------|
| Apple Developer Account | $99/year — [developer.apple.com](https://developer.apple.com) |
| Certificates | iOS Distribution Certificate in Keychain |
| Provisioning Profile | App Store Distribution profile |
| Xcode | Latest stable version on Mac |
| macOS | Required for iOS builds |

### Build Steps

1. **Build Xcode project:** `Tools → PlayFrame → Build → Build iOS Prod`
2. **Open Xcode project:** Open `Builds/iOS_Prod/[name].xcodeproj`
3. **In Xcode:**
   - Select your Team in Signing & Capabilities
   - Verify Bundle Identifier matches App Store Connect
   - Set Deployment Target (iOS 15.0+)
   - Product → Archive
4. **Distribute:** Window → Organizer → Distribute App → App Store Connect

### App Store Connect Setup

| Section | Requirements |
|---------|-------------|
| **App Information** | Primary language, category (Games → Puzzle), subtitle |
| **Pricing** | Free or paid, in-app purchases listed |
| **Privacy** | Privacy policy URL (required), data collection declarations |
| **Age Rating** | Complete questionnaire (gambling, violence, etc.) |
| **Screenshots** | See [Required Screenshots](#required-screenshots-ios) |
| **App Icon** | 1024×1024 PNG (no alpha, no rounded corners) |
| **Description** | Max 4000 chars, first 3 lines are crucial |
| **Keywords** | Max 100 chars, comma-separated |
| **Support URL** | Required — website or support page |
| **Build** | Upload via Xcode or Transporter |

### Required Screenshots (iOS)

| Device | Size | Required |
|--------|------|----------|
| iPhone 6.7" (15 Pro Max) | 1290×2796 | YES |
| iPhone 6.5" (11 Pro Max) | 1242×2688 | YES |
| iPhone 5.5" (8 Plus) | 1242×2208 | Optional (if supporting) |
| iPad Pro 12.9" (6th gen) | 2048×2732 | YES (if iPad supported) |
| iPad Pro 12.9" (2nd gen) | 2048×2732 | Optional |

**Tips:**
- Up to 10 screenshots per device class
- First 3 screenshots are shown in search results
- Include gameplay, UI, and feature highlights
- Localize screenshots for each supported language

### iOS-Specific Checklist

- [ ] Info.plist privacy descriptions set (automatic via `IOSPostProcessBuild`)
- [ ] ATT consent dialog implemented (`IOSTrackingPermission` in Bootstrap)
- [ ] No private API usage
- [ ] App does not crash when permissions are denied
- [ ] In-app purchases use StoreKit and are configured in App Store Connect
- [ ] Launch screen (storyboard) configured — not just a static image
- [ ] App thins properly — no unnecessary architectures
- [ ] Push notifications entitlement only if actually using push
- [ ] No "beta", "test", "demo" text in UI or metadata

### Common iOS Rejection Reasons

| Reason | Prevention |
|--------|------------|
| Crashes / bugs | Test on real devices, multiple iOS versions |
| Guideline 4.3 (spam) | Ensure unique value, not a reskin |
| Broken links | Verify all URLs (privacy, support) |
| Incomplete metadata | Fill all required fields, screenshots |
| Missing privacy declarations | Complete data collection questionnaire |
| Login required | Provide demo credentials if login needed |
| In-app purchase issues | Test sandbox purchases |

---

## 3. Google Play Store

### Developer Account Requirements

| Requirement | Details |
|------------|---------|
| Google Play Developer Account | $25 one-time — [play.google.com/console](https://play.google.com/console) |
| Signing Key | Google Play App Signing (let Google manage) |
| AAB format | Android App Bundle required since 2021 |

### Build Steps

1. **Build AAB:** `Tools → PlayFrame → Build → Build Android Prod`
2. Output: `Builds/Android_Prod/[name]_[version]_b[num].aab`
3. Upload to Google Play Console

### Google Play Console Setup

| Section | Requirements |
|---------|-------------|
| **App details** | Title, short description (80 chars), full description (4000 chars) |
| **Graphics** | See [Required Graphics](#required-graphics-android) |
| **Category** | Games → Puzzle |
| **Content rating** | Complete IARC questionnaire |
| **Target audience** | Age group, teacher-approved (if applicable) |
| **Privacy policy** | URL required |
| **Data safety** | Declare all data collected, shared, and stored |
| **Ads** | Declare if app contains ads |

### Required Graphics (Android)

| Asset | Size | Required |
|-------|------|----------|
| App icon (hi-res) | 512×512 PNG (32-bit, alpha ok) | YES |
| Feature graphic | 1024×500 PNG/JPG | YES |
| Phone screenshots | 16:9 or 9:16, min 320px, max 3840px | YES (2-8 per listing) |
| 7" tablet screenshots | Same as phone | YES (if targeting tablets) |
| 10" tablet screenshots | Same as phone | YES (if targeting tablets) |
| Promo video | YouTube URL | Optional but recommended |

### Android-Specific Checklist

- [ ] Keystore created and backed up securely (or use Google Play App Signing)
- [ ] AAB format enabled (`EditorUserBuildSettings.buildAppBundle = true`)
- [ ] minSdkVersion ≥ 28 (Android 9+) — Google Play requirement
- [ ] Target SDK set to Auto or latest stable
- [ ] Permissions declared only if needed (no unnecessary permissions)
- [ ] 64-bit support (ARM64) — required since 2019
- [ ] ProGuard/R8 minification for release builds
- [ ] App not exceeding 150MB (APK) or 200MB (AAB base + config APKs)
- [ ] No cleartext HTTP traffic (use HTTPS only)
- [ ] Back button works correctly (doesn't crash or freeze)
- [ ] App handles orientation changes / split-screen gracefully

### Common Google Play Rejection Reasons

| Reason | Prevention |
|--------|------------|
| Data safety declaration mismatch | Accurately declare all SDKs' data collection |
| Ads not declared | Check "Contains ads" if using any ad SDK |
| Content rating missing / wrong | Complete the questionnaire honestly |
| Target audience issues | Don't target children unless COPPA compliant |
| Broken functionality | Test on multiple Android versions (9-14) |
| Policy violation (deceptive) | No misleading screenshots or descriptions |

### Testing Tracks

| Track | Purpose |
|-------|---------|
| Internal testing | Team testing, instant access, up to 100 testers |
| Closed testing | Limited beta, requires approval |
| Open testing | Public beta, anyone can join |
| Production | Public release |

**Flow:** Internal → Closed → Open → Production

Always test in Internal first. Google may review Closed/Open tracks.

---

## 4. Common Requirements

### Privacy Policy

You **must** have a privacy policy. Minimum content:

- What data you collect (analytics, device info, ad identifiers)
- How you use the data
- Third-party services (analytics, ads, crash reporting)
- User rights (data deletion, opt-out)
- Contact information

Host on a website or use a privacy policy generator that produces a URL.

### In-App Purchases

If using IAP:

| Platform | Setup |
|----------|-------|
| iOS | Configure products in App Store Connect → In-App Purchases |
| Android | Configure products in Play Console → Monetization → Products |
| Unity | Use Unity IAP package or platform-native APIs |

Test with sandbox accounts (iOS) / license test accounts (Android) before submission.

### Ads

If using ads:

- [ ] Declare ads in store listings
- [ ] Implement ATT consent (iOS) — `IOSTrackingPermission`
- [ ] Implement GDPR consent (EU users) — use UMP SDK or equivalent
- [ ] Interstitials: don't show on first open or immediately after purchase
- [ ] Rewarded ads: make rewards clear before watching
- [ ] No ads in children-targeted apps without COPPA compliance

---

## 5. App Store Optimization (ASO)

### Title & Subtitle

| Element | iOS | Android |
|---------|-----|---------|
| Title | Max 30 chars | Max 30 chars |
| Subtitle | Max 30 chars | N/A |
| Short description | N/A | Max 80 chars |

Use primary keyword in title. Secondary keywords in subtitle/short description.

### Keywords (iOS)

- Max 100 characters, comma-separated
- Don't repeat words from title/subtitle
- Use singular forms (not "puzzles", just "puzzle")
- Include competitor names only if relevant and not trademarked
- Test with ASO tools (AppFollow, Sensor Tower)

### Screenshots Best Practices

1. **First 2-3 screenshots** should show core gameplay
2. Add text overlays explaining features
3. Use device frames (optional but professional)
4. Show progression: early game → mid game → exciting moment
5. Localize captions for each market
6. Use video preview if possible (iOS: 15-30 seconds)

---

## 6. Post-Launch

### Monitoring

| What | Tool |
|------|------|
| Crashes | Firebase Crashlytics / Xcode Organizer |
| ANR (Android) | Play Console Vitals |
| Reviews | Store consoles, AppFollow |
| Analytics | PlayFrame AnalyticsManager / Firebase |
| Performance | Unity Performance Reporting |

### Update Cycle

| Update Type | Frequency | Version Bump |
|-------------|-----------|-------------|
| Hotfix (crash/bug) | ASAP | Patch (1.0.x) |
| Content update | 2-4 weeks | Minor (1.x.0) |
| Major feature | 1-3 months | Major (x.0.0) |

### Review Response

- Respond to negative reviews constructively
- Fix reported bugs promptly
- Thank users for positive feedback
- Never share personal data in public responses
